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
using GTI.Modules.POS.Business;
using GTI.Modules.POS.Properties;

namespace GTI.Modules.POS.UI
{
    /// <summary>
    /// Represents a simple form with a keypad and options
    /// for adding or removing items on a kiosk order.
    /// </summary>
    internal partial class KioskKeypadForm : POSForm
    {
        #region Member Variables
        protected int m_value;
        private DateTime m_idleSince = DateTime.Now;
        #endregion

        #region Constructors
        // TTP 50433
        /// <summary>
        /// Initalizes a new instance of the KioskKeypadForm class.
        /// Required method for Designer support.
        /// </summary>
        protected KioskKeypadForm() 
            : base()
        {
            InitializeComponent(); // Rally TA7465
        }

        /// <summary>
        /// Initializes a new instance of the KioskKeypadForm class.
        /// </summary>
        /// <param name="parent">The PointOfSale to which this form 
        /// belongs.</param>
        /// <param name="displayMode">The display mode used to show this 
        /// form.</param>
        public KioskKeypadForm(PointOfSale parent, DisplayMode displayMode, int currentQuantity)
            : base(parent, displayMode)
        {
            InitializeComponent();
            ApplyDisplayMode();

            Clear();

            m_keypad.ValueChanged += UserActivityDetected; //for timeout handling

            m_btnRemove.Enabled = currentQuantity != 0;

            m_btnClose.UseClickSound = m_parent.Settings.UseKeyClickSoundsOnKiosk;
            m_btnRemove.UseClickSound = m_parent.Settings.UseKeyClickSoundsOnKiosk;
            m_keypad.UseClickSound = m_parent.Settings.UseKeyClickSoundsOnKiosk;
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
            Size = new Size(600, 370);
        }

        // TTP 50114
        /// <summary>
        /// Resets the value of the keypad.
        /// </summary>
        public void Clear()
        {
            NotIdle();
            m_keypad.Clear();
            m_value = 0;
        }

        // TTP 50433
        /// <summary>
        /// Handles the keypad's big button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the 
        /// event data.</param>
        protected virtual void BigButtonClick(object sender, EventArgs e)
        {
            NotIdle();
            m_value = (int)m_keypad.Value;

            if (m_value == 0)
            {
                m_kioskIdleTimer.Stop();
                POSMessageForm.Show(this, m_parent, Resources.PleaseEnterTheNumberOfItemsToAddToTheOrder);
                m_kioskIdleTimer.Start();
                NotIdle();
                return;
            }

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }


        // PDTS 1064
        /// <summary>
        /// Handles the FormClosing event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An FormClosingEventArgs object that contains the 
        /// event data.</param>
        private void FormClose(object sender, FormClosingEventArgs e)
        {
            if (m_parent.WeAreAPOSKiosk)
            {
                m_kioskIdleTimer.Enabled = false;
                m_parent.SellingForm.NotIdle(); //start the selling form triggers
            }
        }

        private void KeypadForm_Shown(object sender, EventArgs e)
        {
            if (m_parent.WeAreAPOSKiosk)
            {
                m_timeoutProgress.Maximum = (KioskShortIdleLimitInSeconds / 3) * 2000;
                m_parent.SellingForm.NotIdle(true); //stop the selling form triggers
                NotIdle();
                m_kioskIdleTimer.Enabled = true;
            }
        }

        private void UserActivityDetected(object sender, EventArgs e)
        {
            NotIdle();
        }

        private void NotIdle()
        {
            m_idleSince = DateTime.Now;
            m_timeoutProgress.Hide();
            m_timeoutProgress.Value = 0;
            m_btnClose.Pulse = false;
        }

        private void m_kioskIdleTimer_Tick(object sender, EventArgs e)
        {
            if (m_parent.GuardianHasUsSuspended)
            {
                NotIdle();
                return;
            }

            TimeSpan idleFor = DateTime.Now - m_idleSince;

            if (idleFor > TimeSpan.FromMilliseconds(KioskShortIdleLimitInSeconds / 3 * 1000))
            {
                if (!m_timeoutProgress.Visible)
                {
                    m_timeoutProgress.Visible = true;
                    m_btnClose.Pulse = m_parent.Settings.KioskTimeoutPulseDefaultButton;
                }

                m_timeoutProgress.Increment(m_kioskIdleTimer.Interval);

                if (m_timeoutProgress.Value >= m_timeoutProgress.Maximum)
                {
                    m_keypad.Clear();
                    this.DialogResult = System.Windows.Forms.DialogResult.Abort;
                    m_value = 0;

                    m_parent.SellingForm.ForceKioskTimeout(1);
                    Close();
                }
            }
        }

        private void m_btnRemove_Click(object sender, EventArgs e)
        {
            m_value = 0;
            this.DialogResult = System.Windows.Forms.DialogResult.No;
            Close();
        }

        private void m_btnClose_Click(object sender, EventArgs e)
        {
            m_value = 0;
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            Close();
        }
        
        #endregion

        #region Member Properties

        /// <summary>
        /// Gets the value of the keypad.
        /// </summary>
        public int QuantityToAdd
        {
            get
            {
                return m_value;
            }
        }

        /// <summary>
        /// Sets the initial value of the keypad.
        /// </summary>
        public decimal InitialValue
        {
            set
            {
                m_keypad.InitialValue = value;
            }
        }

        #endregion
    }
}