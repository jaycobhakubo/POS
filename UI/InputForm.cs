#region Copyright
// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2008 GameTech
// International, Inc.
#endregion

// TTP 50066

using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using GTI.Controls;
using GTI.Modules.Shared;
using GTI.Modules.POS.Business;
using GTI.Modules.POS.Properties;

namespace GTI.Modules.POS.UI
{
    /// <summary>
    /// Represents a form used to collect a single line of input.
    /// </summary>
    internal partial class InputForm : POSForm
    {
        #region Member Variables
        protected object m_lastFocus;
        protected bool m_requireInput;
        private DateTime m_idleSince = DateTime.Now;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the InputForm class.
        /// </summary>
        /// <param name="parent">The PointOfSale to which this form 
        /// belongs.</param>
        /// <param name="displayMode">The display mode used to show this 
        /// form.</param>
        /// <param name="requireInput">Whether any input is required to press 
        /// the ok button.</param>
        /// <exception cref="System.ArgumentNullException">parent or 
        /// displayMode is a null reference.</exception>
        public InputForm(PointOfSale parent, DisplayMode displayMode, bool requireInput)
            : base(parent, displayMode)
        {
            InitializeComponent();
            ApplyDisplayMode();

            m_requireInput = requireInput;

            // Set the last focused control to the text box.
            m_lastFocus = m_textBox;

            // Which keyboard do we need to use?
            // PDTS 964
            m_virtualKeyboard.SetLayoutByCulture(CultureInfo.CurrentUICulture);

            if(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "es")
                m_virtualKeyboard.ShiftImageIcon = Resources.ArrowUp;
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
            Size = m_displayMode.LargeDialogSize;
        }

        /// <summary>
        /// Handles when a key on the virtual keyboard is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">A KeyboardEventArgs object that contains the 
        /// event data.</param>
        private void KeyboardKeyPressed(object sender, KeyboardEventArgs e)
        {
            NotIdle();

            if(m_lastFocus is Control && (m_lastFocus != m_virtualKeyboard))
            {
                ((Control)m_lastFocus).Focus();
                SendKeys.Send(e.KeyPressed);
            }
        }

        /// <summary>
        /// Handles when a key is pressed while the input box has focus.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">A KeyboardEventArgs object that contains the 
        /// event data.</param>
        private void InputKeyDown(object sender, KeyEventArgs e)
        {
            NotIdle();

            if (e.KeyCode == Keys.Enter)
                OkClick(this, new EventArgs());
        }

        /// <summary>
        /// Handles the ok button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the 
        /// event data.</param>
        private void OkClick(object sender, EventArgs e)
        {
            if (m_requireInput && !string.IsNullOrEmpty(m_textBox.Text) && m_textBox.Text.Trim().Length == 0)
            {
                m_idleSince = DateTime.Now + TimeSpan.FromDays(1);

                POSMessageForm.Show(this, m_parent, Resources.InputRequired);

                NotIdle();
            }
            else
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void m_cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }


        private void InputForm_Shown(object sender, EventArgs e)
        {
            if (m_parent.WeAreAPOSKiosk)
            {
                m_parent.SellingForm.NotIdle(true); //stop the selling form triggers
                NotIdle();
                m_timeoutProgress.Maximum = (KioskShortIdleLimitInSeconds / 3) * 2000;
                m_kioskIdleTimer.Enabled = true;
            }
        }

        private void InputForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (m_parent.WeAreAPOSKiosk)
            {
                m_kioskIdleTimer.Enabled = false;
                m_parent.SellingForm.NotIdle(); //start the selling form triggers
            }
        }

        private void NotIdle()
        {
            m_idleSince = DateTime.Now;
            m_timeoutProgress.Hide();
            m_timeoutProgress.Value = 0;
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
                    m_timeoutProgress.Visible = true;

                m_timeoutProgress.Increment(m_kioskIdleTimer.Interval);

                if (m_timeoutProgress.Value >= m_timeoutProgress.Maximum)
                {
                    m_parent.SellingForm.ForceKioskTimeout(1);
                    m_cancelButton_Click(sender, e);
                }
            }
        }
        
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets whether input is required in order to press the 
        /// ok button.
        /// </summary>
        public bool RequireInput
        {
            get
            {
                return m_requireInput;
            }
            set
            {
                m_requireInput = value;
            }
        }

        /// <summary>
        /// Gets or sets the prompt to display on the form.
        /// </summary>
        public string Message
        {
            get
            {
                return m_promptLabel.Text;
            }
            set
            {
                m_promptLabel.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the input typed in by the user.
        /// </summary>
        public string Input
        {
            get
            {
                return m_textBox.Text;
            }
            set
            {
                m_textBox.Text = value;
            }
        }
        #endregion
    }
}