#region Copyright
// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2008-2009 GameTech
// International, Inc.
#endregion

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Globalization;
using GTI.Controls;
using GTI.Modules.Shared;
using GTI.Modules.POS.Data;
using GTI.Modules.POS.Business;
using GTI.Modules.POS.Properties;

namespace GTI.Modules.POS.UI
{
    /// <summary>
    /// The form that allows the track the progress of scanning Crystal Ball 
    /// cards.
    /// </summary>
    internal partial class CrystalBallScanForm : POSForm
    {
        #region Member Variables
        protected CrystalBallManager m_manager;
        private DateTime m_idleSince = DateTime.Now;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the CrystalBallScanForm class.
        /// </summary>
        /// <param name="parent">The PointOfSale to which this form 
        /// belongs.</param>
        /// <param name="displayMode">The display mode used to show this 
        /// form.</param>
        /// <param name="helper">The instance of the CrystalBallManager 
        /// using this form.</param>
        /// <param name="numbersRequired">The amount of numbers that are 
        /// required to be chosen.</param>
        /// <exception cref="System.ArgumentNullException">parent, 
        /// displayMode, or manager is a null reference.</exception>
        public CrystalBallScanForm(PointOfSale parent, CrystalBallManager manager, int numbersRequired)
            : base(parent)
        {
            if(manager == null)
                throw new ArgumentNullException("manager");

            InitializeComponent();
            ApplyDisplayMode();

            // FIX: DE4052
            m_quickFinishButton.Visible = m_parent.Settings.CBBQuickPickEnabled;

            m_manager = manager;
            m_numbersReqLabel.Text = Resources.CBBNumbersRequired + numbersRequired.ToString(CultureInfo.CurrentCulture);

            if (m_parent.WeAreAPOSKiosk) //make it look better for kiosk users
            {
                Point cancel = m_cancelButton.Location;
                Point jam = m_clearJamButton.Location;
                Point quick = m_quickFinishButton.Location;

                m_quickFinishButton.Location = cancel;
                m_quickFinishButton.ImageNormal = Resources.PurpleButtonUp;
                m_quickFinishButton.ImagePressed = Resources.PurpleButtonDown;
                m_quickFinishButton.Font = new Font(m_quickFinishButton.Font.FontFamily, 16, FontStyle.Bold);
                m_quickFinishButton.UseClickSound = m_parent.Settings.UseKeyClickSoundsOnKiosk;

                m_clearJamButton.Location = quick;
                m_clearJamButton.ImageNormal = Resources.OrangeButtonUp;
                m_clearJamButton.ImagePressed = Resources.OrangeButtonDown;
                m_clearJamButton.Font = m_quickFinishButton.Font;
                m_clearJamButton.UseClickSound = m_parent.Settings.UseKeyClickSoundsOnKiosk;

                m_cancelButton.Location = jam;
                m_cancelButton.ImageNormal = Resources.RedButtonUp;
                m_cancelButton.ImagePressed = Resources.RedButtonDown;
                m_cancelButton.Font = m_quickFinishButton.Font;
                m_cancelButton.UseClickSound = m_parent.Settings.UseKeyClickSoundsOnKiosk;
            }
        }
        #endregion

        #region Member Methods
        /// <summary>
        /// Sets the settings of this form based on the current display mode.
        /// </summary>
        protected override void ApplyDisplayMode()
        {
            base.ApplyDisplayMode();

            // This is a special dialog, so override the default size.
            Size = new Size(420, 492); // Rally US505
        }

        /// <summary>
        /// Updates the form with the progress of the cards scanned.
        /// </summary>
        /// <param name="cardsScanned">The number of cards already 
        /// scanned.</param>
        /// <param name="totalCards">The total number of cards to be 
        /// scanned.</param>
        public void SetScanProgress(int cardsScanned, int totalCards)
        {
            m_cardsScannedLabel.Text = string.Format(CultureInfo.CurrentCulture, Resources.CBBCardsScanned, cardsScanned, totalCards);
        }

        /// <summary>
        /// Prompts the user to see if they want to keep duplicate cards.
        /// </summary>
        /// <returns>true if the do want to keep duplicates; otherwise 
        /// false.</returns>
        public bool PromptForDuplicates()
        {
            NotIdle();
            m_kioskIdleTimer.Stop();

            bool answer = m_parent.ShowMessage(this, m_displayMode, Resources.CBBDuplicateCards, POSMessageFormTypes.YesNo_DefNO) == DialogResult.Yes;

            m_kioskIdleTimer.Start();
            NotIdle();
            
            return answer;
        }

        /// <summary>
        /// Tells the user one of the cards was invalid so the whole card is lost
        /// </summary>
        public void CBBQuickPickError()
        {
            NotIdle();
            m_kioskIdleTimer.Stop();
            m_parent.ShowMessage(this, m_displayMode, Resources.CBBCardScanError);
            m_kioskIdleTimer.Start();
            NotIdle();
        }

        /// <summary>
        /// Handles the clear jam button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the 
        /// event data.</param>
        private void ClearJamClick(object sender, EventArgs e)
        {
            NotIdle();
            m_manager.ClearJam();
        }

        // FIX: DE2478
        /// <summary>
        /// Handles the finish with quick pick(s) button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the 
        /// event data.</param>
        private void QuickFinishClick(object sender, EventArgs e)
        {
            NotIdle();
            m_manager.FinishWithQuickPicks();
            Finished();
        }
        // END: DE2478

        /// <summary>
        /// Closes the dialog and sets the DialogResult to OK.
        /// </summary>
        public void Finished()
        {
            NotIdle();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void m_kioskIdleTimer_Tick(object sender, EventArgs e)
        {
            if (m_parent.GuardianHasUsSuspended)
            {
                NotIdle();
                return;
            }

            TimeSpan idleFor = DateTime.Now - m_idleSince;

            if (idleFor > TimeSpan.FromMilliseconds(m_parent.SellingForm.KioskIdleLimitInSeconds / 3 * 2000))
            {
                if (!m_timeoutProgress.Visible)
                {
                    m_timeoutProgress.Visible = true;
                    m_cancelButton.Pulse = m_parent.Settings.KioskTimeoutPulseDefaultButton;
                }

                m_timeoutProgress.Increment(m_kioskIdleTimer.Interval);

                if (m_timeoutProgress.Value >= m_timeoutProgress.Maximum)
                {
                    DialogResult = DialogResult.Abort;
                    m_parent.SellingForm.ForceKioskTimeout(2);
                    Close();
                }
            }
        }

        private void NotIdle()
        {
            m_idleSince = DateTime.Now;
            m_timeoutProgress.Hide();
            m_timeoutProgress.Value = 0;
            m_cancelButton.Pulse = false;
        }

        private void CrystalBallScanForm_Shown(object sender, EventArgs e)
        {
            if (m_parent.WeAreAPOSKiosk)
            {
                m_parent.SellingForm.NotIdle(true); //stop the selling form triggers
                NotIdle();
                m_timeoutProgress.Maximum = (m_parent.SellingForm.KioskIdleLimitInSeconds / 3) * 1000;
                m_kioskIdleTimer.Enabled = true;
            }
        }

        private void CrystalBallScanForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (m_parent.WeAreAPOSKiosk)
            {
                m_kioskIdleTimer.Enabled = false;
                m_parent.SellingForm.NotIdle(); //start the selling form triggers
            }
        }

        private void UserActivityDetected(object sender, EventArgs e)
        {
            NotIdle();
        }

        #endregion
    }

    /// <summary>
    /// A delegate that allows cross-thread calls to SetScanProgess on the 
    /// CrystalBallScanForm class.
    /// </summary>
    /// <param name="cardsScanned">The number of cards already scanned.</param>
    /// <param name="totalCards">The total number of cards to be 
    /// scanned.</param>
    internal delegate void ScanProgressDelegate(int cardsScanned, int totalCards);

    /// <summary>
    /// A delegate that allows cross-thread calls to PromptForDuplicates on the 
    /// CrystalBallScanForm class.
    /// </summary>
    internal delegate bool PromptDelegate();

    /// <summary>
    /// A delegate that allows cross-thread calls to CBBQuickPickError on the 
    /// CrystalBallScanForm class
    /// </summary>
    internal delegate void CBBFailureDelegate(); //RALLY DE2961
}