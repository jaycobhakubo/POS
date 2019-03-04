#region Copyright
// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2008 GameTech
// International, Inc.
#endregion

using System;
using System.Drawing;
using System.Windows.Forms;
using GTI.Controls;
using GTI.Modules.Shared;
using GTI.Modules.POS.Data;
using GTI.Modules.POS.Business;
using GTI.Modules.POS.Properties;
using GTI.GTIDevices.ExternalDevices;

namespace GTI.Modules.POS.UI
{
    /// <summary>
    /// The form that allows the user to specify how the customer will 
    /// choose their Crystal Ball Bingo numbers.
    /// </summary>
    internal partial class CrystalBallPromptForm : POSForm
    {
        #region Member Variables
        protected ProductType m_selectionType = ProductType.CrystalBallQuickPick;
        private DateTime m_idleSince = DateTime.Now;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the CrystalBallPromptForm class.
        /// </summary>
        /// <param name="parent">The PointOfSale to which this form 
        /// belongs.</param>
        /// <param name="displayMode">The display mode used to show this 
        /// form.</param>
        /// <exception cref="System.ArgumentNullException">parent or 
        /// displayMode is a null reference.</exception>
        public CrystalBallPromptForm(PointOfSale parent, bool useFavorites, bool secondPrompt)
            : base(parent)
        {
            InitializeComponent();
            ApplyDisplayMode();

            if (m_parent.WeAreAPOSKiosk) //make it a little more friendly for the kiosk
            {
                if (!secondPrompt)
                    m_selectionLabel.Text = Resources.CBBHowWouldYouLikeToEnterYourNumbers;
                else
                    m_selectionLabel.Text = Resources.CBBHowWouldYouLikeToEnterYourNumbers2;

                m_selectionLabel.Font = m_quickPickButton.Font;

                m_quickPickButton.ImageNormal = Resources.PurpleButtonUp;
                m_quickPickButton.ImagePressed = Resources.PurpleButtonDown;
                m_quickPickButton.Font = new Font(m_quickPickButton.Font.FontFamily, 16, FontStyle.Bold);
                m_quickPickButton.UseClickSound = m_parent.Settings.UseKeyClickSoundsOnKiosk;

                m_selectionLabel.Font = m_quickPickButton.Font;

                m_scanButton.ImageNormal = Resources.OrangeButtonUp;
                m_scanButton.ImagePressed = Resources.OrangeButtonDown;
                m_scanButton.Font = m_quickPickButton.Font;
                m_scanButton.UseClickSound = m_parent.Settings.UseKeyClickSoundsOnKiosk;

                m_favoritesButton.ImageNormal = Resources.GreenButtonUp;
                m_favoritesButton.ImagePressed = Resources.GreenButtonDown;
                m_favoritesButton.Font = m_quickPickButton.Font;
                m_favoritesButton.UseClickSound = m_parent.Settings.UseKeyClickSoundsOnKiosk;

                m_handPickButton.Font = m_quickPickButton.Font;
                m_handPickButton.UseClickSound = m_parent.Settings.UseKeyClickSoundsOnKiosk;

                m_cancelButton.ImageNormal = Resources.RedButtonUp;
                m_cancelButton.ImagePressed = Resources.RedButtonDown;
                m_cancelButton.Font = m_quickPickButton.Font;
                m_cancelButton.UseClickSound = m_parent.Settings.UseKeyClickSoundsOnKiosk;
            }
            else
            {
                Size = new Size(Size.Width, m_quickPickButton.Location.Y + m_quickPickButton.Size.Height + 10);
                m_cancelButton.Visible = false;
            }

            // FIX: DE4052
            m_quickPickButton.Visible = m_parent.Settings.CBBQuickPickEnabled;
            m_scanButton.Visible = m_parent.Settings.CbbScannerType != SupportedOMRDevices.NO_DEVICE;
            m_favoritesButton.Visible = useFavorites;
            
            //space out the visible buttons
            int visibleButtons = 4;

            if (!m_parent.Settings.CBBQuickPickEnabled)
                visibleButtons--;

            if (m_parent.Settings.CbbScannerType == SupportedOMRDevices.NO_DEVICE)
                visibleButtons--;

            if (!useFavorites)
                visibleButtons--;

            int gap = (this.Width - visibleButtons * m_handPickButton.Width) / (visibleButtons + 1);
            int nextX = gap;

            if (m_parent.Settings.CBBQuickPickEnabled)
            {
                m_quickPickButton.Location = new Point(nextX, m_quickPickButton.Location.Y);
                nextX += m_quickPickButton.Width + gap;
            }

            if (m_parent.Settings.CbbScannerType != SupportedOMRDevices.NO_DEVICE)
            {
                m_scanButton.Location = new Point(nextX, m_scanButton.Location.Y);
                nextX += m_scanButton.Width + gap;
            }

            if (useFavorites)
            {
                m_favoritesButton.Location = new Point(nextX, m_favoritesButton.Location.Y);
                nextX += m_favoritesButton.Width + gap;
            }

            m_handPickButton.Location = new Point(nextX, m_handPickButton.Location.Y);
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
            Size = new Size(580, 280);
        }

        /// <summary>
        /// Handles the Quick Pick button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the 
        /// event data.</param>
        private void QuickPickButtonClick(object sender, EventArgs e)
        {
            NotIdle();

            m_selectionType = ProductType.CrystalBallQuickPick;
            DialogResult = DialogResult.No;
            Close();
        }

        /// <summary>
        /// Handles the Scanned button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the 
        /// event data.</param>
        private void ScanButtonClick(object sender, EventArgs e)
        {
            NotIdle();

            m_selectionType = ProductType.CrystalBallScan;
            DialogResult = DialogResult.No;
            Close();
        }

        /// <summary>
        /// Handles the Hand Pick button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the 
        /// event data.</param>
        private void HandPickButtonClick(object sender, EventArgs e)
        {
            NotIdle();

            m_selectionType = ProductType.CrystalBallHandPick;
            DialogResult = DialogResult.No;
            Close();
        }

        private void m_favoritesButton_Click(object sender, EventArgs e)
        {
            NotIdle();

            m_selectionType = ProductType.CrystalBallHandPick;
            DialogResult = DialogResult.Yes;
            Close();
        }

        private void imageButton1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        #endregion

        #region Member Properties
        /// <summary>
        /// Gets the selection type specified by the user.
        /// </summary>
        public ProductType SelectionType
        {
            get
            {
                return m_selectionType;
            }
        }
        #endregion

        private void m_kioskIdleTimer_Tick(object sender, EventArgs e)
        {
            if (m_parent.GuardianHasUsSuspended)
            {
                NotIdle();
                return;
            }

            TimeSpan idleFor = DateTime.Now - m_idleSince;

            if (idleFor > TimeSpan.FromMilliseconds(m_parent.SellingForm.KioskShortIdleLimitInSeconds / 3 * 2000))
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

        private void CrystalBallPromptForm_Shown(object sender, EventArgs e)
        {
            if (m_parent.WeAreAPOSKiosk)
            {
                m_parent.SellingForm.NotIdle(true); //stop the selling form triggers
                NotIdle();
                m_timeoutProgress.Maximum = (m_parent.SellingForm.KioskShortIdleLimitInSeconds / 3) * 1000;
                m_kioskIdleTimer.Enabled = true;
            }
        }

        private void CrystalBallPromptForm_FormClosed(object sender, FormClosedEventArgs e)
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
    }
}