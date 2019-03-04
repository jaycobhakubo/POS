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
    /// Represents a simple form with a keypad.
    /// </summary>
    internal partial class KeypadForm : POSForm
    {
        #region Member Variables
        protected bool m_allowMagCards;
        protected object m_value;
        protected KeypadResult m_result;
        protected bool m_detectedSwipe; // PDTS 1064
        private DateTime m_idleSince = DateTime.Now;
        protected bool m_EnterPressesOption1 = false;
        #endregion

        #region Constructors
        // TTP 50433
        /// <summary>
        /// Initalizes a new instance of the KeypadForm class.
        /// Required method for Designer support.
        /// </summary>
        protected KeypadForm() 
            : base()
        {
            InitializeComponent(); // Rally TA7465
        }

        /// <summary>
        /// Initializes a new instance of the KeypadForm class.
        /// </summary>
        /// <param name="parent">The PointOfSale to which this form 
        /// belongs.</param>
        /// <param name="displayMode">The display mode used to show this 
        /// form.</param>
        /// <param name="allowMagCards">If true the form will allow a mag card 
        /// number for a value in place of clicking on keypad; otherwise 
        /// false.</param>
        /// <exception cref="System.ArgumentNullException">parent or 
        /// displayMode is a null reference.</exception>
        public KeypadForm(PointOfSale parent, DisplayMode displayMode, bool allowMagCards)
            : base(parent, displayMode)
        {
            InitializeComponent();
            ApplyDisplayMode();

            m_allowMagCards = allowMagCards;

            // PDTS 1064
            // Start listening for swipes.
            m_parent.MagCardReader.CardSwiped += new MagneticCardSwipedHandler(CardSwiped);

            if (m_parent.WeAreAPOSKiosk)
            {
                m_keypad.ValueChanged += m_keypad_Click;
                m_keypad.UseClickSound = m_parent.Settings.UseKeyClickSoundsOnKiosk;
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
            Size = new Size(281, 437);
        }

        /// <summary>
        /// Determines if the option buttons are shown or the big button.  Also, 
        /// determines which option buttons are shown.
        /// </summary>
        /// <param name="showOptions">If true the option buttons are shown; 
        /// otherwise the big button.</param>
        /// <param name="showOption1">If true the first option button is 
        /// shown; otherwise false.</param>
        /// <param name="showOption2">If true the second option button is 
        /// shown; otherwise false.</param>
        /// <param name="showOption3">If true the third option button is 
        /// shown; otherwise false.</param>
        /// <param name="showOption4">If true the fourth option button is 
        /// shown; otherwise false.</param>
        public void ShowOptionButtons(bool showOptions, bool showOption1, bool showOption2, bool showOption3, bool showOption4)
        {
            m_keypad.ShowOptionButtons = showOptions;
            m_keypad.Option1Visible = showOption1;
            m_keypad.Option2Visible = showOption2;
            m_keypad.Option3Visible = showOption3;
            m_keypad.Option4Visible = showOption4;
        }

        // TTP 50114
        /// <summary>
        /// Resets the value of the keypad.
        /// </summary>
        public void Clear()
        {
            NotIdle();
            m_keypad.Clear();
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
            m_value = (object)m_keypad.Value;
            m_result = KeypadResult.BigButton;
            Close();
        }

        /// <summary>
        /// Handles the keypad's option 1 button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the 
        /// event data.</param>
        protected virtual void Option1Click(object sender, EventArgs e)
        {
            NotIdle();
            m_value = (object)m_keypad.Value;
            m_result = KeypadResult.Option1;
            Close();
        }

        /// <summary>
        /// Handles the keypad's option 2 button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the 
        /// event data.</param>
        protected virtual void Option2Click(object sender, EventArgs e)
        {
            NotIdle();
            m_value = (object)m_keypad.Value;
            m_result = KeypadResult.Option2;
            Close();
        }

        /// <summary>
        /// Handles the keypad's option 3 button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the 
        /// event data.</param>
        protected virtual void Option3Click(object sender, EventArgs e)
        {
            NotIdle();
            m_value = (object)m_keypad.Value;
            m_result = KeypadResult.Option3;
            Close();
        }

        /// <summary>
        /// Handles the keypad's option 4 button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the 
        /// event data.</param>
        protected virtual void Option4Click(object sender, EventArgs e)
        {
            NotIdle();
            m_value = (object)m_keypad.Value;
            m_result = KeypadResult.Option4;
            Close();
        }

        /// <summary>
        /// Processes a dialog box key.
        /// </summary>
        /// <param name="keyData">One of the Keys values that 
        /// represents the key to process.</param>
        /// <returns>true if the keystroke was processed and consumed by the 
        /// control; otherwise, false to allow further processing.</returns>
        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (m_parent.MagCardReader.ReadingCards)
                return false;

            NotIdle();

            // PDTS 1064
            if ((keyData & Keys.Enter) == Keys.Enter)
            {
                // TTP 50066
                if (m_detectedSwipe)
                {
                    Close();
                    return true;
                }
                else if (!m_detectedSwipe && m_keypad.BigButtonEnabled && !m_keypad.ShowOptionButtons)
                {
                    BigButtonClick(this, new EventArgs());
                    return true;
                }
                else if (!m_detectedSwipe && m_keypad.Option1Enabled && m_keypad.Option1Visible)
                {
                    Option1Click(this, new EventArgs());
                    return true;
                }
                else if (!m_detectedSwipe && m_keypad.Option2Enabled && m_keypad.Option2Visible)
                {
                    Option2Click(this, new EventArgs());
                    return true;
                }
                else if (!m_detectedSwipe && m_keypad.Option3Enabled && m_keypad.Option3Visible)
                {
                    Option3Click(this, new EventArgs());
                    return true;
                }
                else if (!m_detectedSwipe && m_keypad.Option4Enabled && m_keypad.Option4Visible)
                {
                    Option4Click(this, new EventArgs());
                    return true;
                }
                else
                {
                    return base.ProcessDialogKey(keyData);
                }
            }
            else
            {
                return base.ProcessDialogKey(keyData);
            }
        }

        /// <summary>
        /// Handles the form's KeyPress event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An KeyPressEventArgs object that contains the 
        /// event data.</param>
        private void OnKeyPress(object sender, KeyPressEventArgs e)
        {
            NotIdle();

            // PDTS 1064
            if (m_allowMagCards && m_parent.MagCardReader.ProcessCharacter(e.KeyChar))
                e.Handled = true; // Don't send to the active control.

            // TTP 50066
            // If the mag card didn't handle it, then can the keypad?
            if(!e.Handled && char.IsDigit(e.KeyChar))
            {
                m_keypad.ClickButton((Keypad.KeypadButton)char.GetNumericValue(e.KeyChar));
                e.Handled = true;
            }

            if (!e.Handled && e.KeyChar == '\r' && m_EnterPressesOption1)
                m_keypad.ClickButton(Keypad.KeypadButton.ButtonOp1);
        }

        // PDTS 1064
        /// <summary>
        /// Handles the MagneticCardReader's CardSwiped event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An MagneticCardSwipeArgs object that contains the
        /// event data.</param>
        void CardSwiped(object sender, MagneticCardSwipeArgs e)
        {
            if(ContainsFocus)
            {
                // If we've read a mag. card, close the form.
                m_detectedSwipe = true;
                m_result = KeypadResult.MagneticCard;
                m_value = (object)e.CardData;
                Close();
            }
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
            // Don't listen to the CardSwiped event anymore since we 
            // are closing.
            m_parent.MagCardReader.CardSwiped -= CardSwiped;
            m_detectedSwipe = false;

            if (m_parent.WeAreAPOSKiosk)
            {
                m_kioskIdleTimer.Enabled = false;
                m_parent.SellingForm.NotIdle(); //start the selling form triggers
            }
        }

        private void KeypadForm_Shown(object sender, EventArgs e)
        {
            if (m_parent != null && m_parent.WeAreAPOSKiosk)
            {
                m_timeoutProgress.Maximum = (KioskShortIdleLimitInSeconds / 3) * 2000;
                m_parent.SellingForm.NotIdle(true); //stop the selling form triggers
                NotIdle();
                m_kioskIdleTimer.Enabled = true;
            }
        }

        private void m_keypad_Click(object sender, EventArgs e)
        {
            NotIdle();
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
                    m_keypad.Clear();

                    if (m_keypad.BigButtonEnabled)
                        m_result = KeypadResult.BigButton;
                    else if (m_keypad.Option1Enabled)
                        m_result = KeypadResult.Option1;
                    else if (m_keypad.Option2Enabled)
                        m_result = KeypadResult.Option2;
                    else if (m_keypad.Option3Enabled)
                        m_result = KeypadResult.Option3;
                    else if (m_keypad.Option4Enabled)
                        m_result = KeypadResult.Option4;

                    m_detectedSwipe = false;
                    m_value = m_keypad.Value;

                    m_parent.SellingForm.ForceKioskTimeout(1);
                    Close(); // Close the form because the source probably wasn't the keyboard.
                }
            }
        }

        #endregion

        #region Member Properties
        /// <summary>
        /// Gets or sets the message displayed on the form.
        /// </summary>
        public string Message
        {
            get
            {
                return m_messageLabel.Text;
            }
            set
            {
                m_messageLabel.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets how numbers are displayed on the keypad.
        /// </summary>
        public Keypad.NumberMode NumberDisplayMode
        {
            get
            {
                return m_keypad.NumberDisplayMode;
            }
            set
            {
                m_keypad.NumberDisplayMode = value;
            }
        }

        /// <summary>
        /// Gets or sets the keypad's currency symbol.
        /// </summary>
        public string CurrencySymbol
        {
            get
            {
                return m_keypad.CurrencySymbol;
            }
            set
            {
                m_keypad.CurrencySymbol = value;
            }
        }

        /// <summary>
        /// Gets or sets the normal image for the first option button.
        /// </summary>
        public Image Option1ImageNormal
        {
            get
            {
                return m_keypad.Option1ImageNormal;
            }
            set
            {
                m_keypad.Option1ImageNormal = value;
            }
        }

        /// <summary>
        /// Gets or sets the normal image for the second option button.
        /// </summary>
        public Image Option2ImageNormal
        {
            get
            {
                return m_keypad.Option2ImageNormal;
            }
            set
            {
                m_keypad.Option2ImageNormal = value;
            }
        }

        /// <summary>
        /// Gets or sets the normal image for the third option button.
        /// </summary>
        public Image Option3ImageNormal
        {
            get
            {
                return m_keypad.Option3ImageNormal;
            }
            set
            {
                m_keypad.Option3ImageNormal = value;
            }
        }

        /// <summary>
        /// Gets or sets the normal image for the fourth option button.
        /// </summary>
        public Image Option4ImageNormal
        {
            get
            {
                return m_keypad.Option4ImageNormal;
            }
            set
            {
                m_keypad.Option4ImageNormal = value;
            }
        }

        /// <summary>
        /// Gets or sets the normal image for the big button.
        /// </summary>
        public Image BigButtonImageNormal
        {
            get
            {
                return m_keypad.BigButtonImageNormal;
            }
            set
            {
                m_keypad.BigButtonImageNormal = value;
            }
        }

        /// <summary>
        /// Gets or sets the pressed image for the first option button.
        /// </summary>
        public Image Option1ImagePressed
        {
            get
            {
                return m_keypad.Option1ImagePressed;
            }
            set
            {
                m_keypad.Option1ImagePressed = value;
            }
        }

        /// <summary>
        /// Gets or sets the pressed image for the second option button.
        /// </summary>
        public Image Option2ImagePressed
        {
            get
            {
                return m_keypad.Option2ImagePressed;
            }
            set
            {
                m_keypad.Option2ImagePressed = value;
            }
        }

        /// <summary>
        /// Gets or sets the pressed image for the third option button.
        /// </summary>
        public Image Option3ImagePressed
        {
            get
            {
                return m_keypad.Option3ImagePressed;
            }
            set
            {
                m_keypad.Option3ImagePressed = value;
            }
        }

        /// <summary>
        /// Gets or sets the pressed image for the fourth option button.
        /// </summary>
        public Image Option4ImagePressed
        {
            get
            {
                return m_keypad.Option4ImagePressed;
            }
            set
            {
                m_keypad.Option4ImagePressed = value;
            }
        }

        /// <summary>
        /// Gets or sets the pressed image for the big button.
        /// </summary>
        public Image BigButtonImagePressed
        {
            get
            {
                return m_keypad.BigButtonImagePressed;
            }
            set
            {
                m_keypad.BigButtonImagePressed = value;
            }
        }

        /// <summary>
        /// Gets or sets the disabled image for the first option button.
        /// </summary>
        public Image Option1ImageDisabled
        {
            get
            {
                return m_keypad.Option1ImageDisabled;
            }
            set
            {
                m_keypad.Option1ImageDisabled = value;
            }
        }

        /// <summary>
        /// Gets or sets the disabled image for the second option button.
        /// </summary>
        public Image Option2ImageDisabled
        {
            get
            {
                return m_keypad.Option2ImageDisabled;
            }
            set
            {
                m_keypad.Option2ImageDisabled = value;
            }
        }

        /// <summary>
        /// Gets or sets the disabled image for the third option button.
        /// </summary>
        public Image Option3ImageDisabled
        {
            get
            {
                return m_keypad.Option3ImageDisabled;
            }
            set
            {
                m_keypad.Option3ImageDisabled = value;
            }
        }

        /// <summary>
        /// Gets or sets the disabled image for the fourth option button.
        /// </summary>
        public Image Option4ImageDisabled
        {
            get
            {
                return m_keypad.Option4ImageDisabled;
            }
            set
            {
                m_keypad.Option4ImageDisabled = value;
            }
        }

        /// <summary>
        /// Gets or sets the disabled image for the big button.
        /// </summary>
        public Image BigButtonImageDisabled
        {
            get
            {
                return m_keypad.BigButtonImageDisabled;
            }
            set
            {
                m_keypad.BigButtonImageDisabled = value;
            }
        }

        /// <summary>
        /// Gets or sets whether to stretch the normal, pressed, and disabled
        /// images to the size of the first option button.
        /// </summary>
        public bool Option1Stretch
        {
            get
            {
                return m_keypad.Option1Stretch;
            }
            set
            {
                m_keypad.Option1Stretch = value;
            }
        }

        /// <summary>
        /// Gets or sets whether to stretch the normal, pressed, and disabled
        /// images to the size of the second option button.
        /// </summary>
        public bool Option2Stretch
        {
            get
            {
                return m_keypad.Option2Stretch;
            }
            set
            {
                m_keypad.Option2Stretch = value;
            }
        }

        /// <summary>
        /// Gets or sets whether to stretch the normal, pressed, and disabled
        /// images to the size of the third option button.
        /// </summary>
        public bool Option3Stretch
        {
            get
            {
                return m_keypad.Option3Stretch;
            }
            set
            {
                m_keypad.Option3Stretch = value;
            }
        }

        /// <summary>
        /// Gets or sets whether to stretch the normal, pressed, and disabled
        /// images to the size of the fourth option button.
        /// </summary>
        public bool Option4Stretch
        {
            get
            {
                return m_keypad.Option4Stretch;
            }
            set
            {
                m_keypad.Option4Stretch = value;
            }
        }

        /// <summary>
        /// Gets or sets whether to stretch the normal, pressed, and disabled
        /// images to the size of the big button.
        /// </summary>
        public bool BigButtonStretch
        {
            get
            {
                return m_keypad.BigButtonStretch;
            }
            set
            {
                m_keypad.BigButtonStretch = value;
            }
        }

        /// <summary>
        /// Gets or sets the text on the keypad's big button.
        /// </summary>
        public string BigButtonText
        {
            get
            {
                return m_keypad.BigButtonText;
            }
            set
            {
                m_keypad.BigButtonText = value;
            }
        }

        /// <summary>
        /// Gets or sets the text on the keypad's first option button.
        /// </summary>
        public string Option1Text
        {
            get
            {
                return m_keypad.Option1Text;
            }
            set
            {
                m_keypad.Option1Text = value;
            }
        }

        public void SetNewMessageFont(Font font)
        {
            m_messageLabel.Font = font;
        }

        /// <summary>
        /// Gets or sets the text on the keypad's second option button.
        /// </summary>
        public string Option2Text
        {
            get
            {
                return m_keypad.Option2Text;
            }
            set
            {
                m_keypad.Option2Text = value;
            }
        }

        public Keypad GetKeypad()
        {
            return m_keypad;
        }

        /// <summary>
        /// Gets or sets the text on the keypad's third option button.
        /// </summary>
        public string Option3Text
        {
            get
            {
                return m_keypad.Option3Text;
            }
            set
            {
                m_keypad.Option3Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the text on the keypad's fourth option button.
        /// </summary>
        public string Option4Text
        {
            get
            {
                return m_keypad.Option4Text;
            }
            set
            {
                m_keypad.Option4Text = value;
            }
        }

        /// <summary>
        /// Gets the value of the keypad.
        /// </summary>
        public object Value
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

        /// <summary>
        /// Gets which button was click in order to close the keypad.
        /// </summary>
        public KeypadResult Result
        {
            get
            {
                return m_result;
            }
        }

        public bool EnterPressesOption1
        {
            get
            {
                return m_EnterPressesOption1;
            }

            set
            {
                m_EnterPressesOption1 = value;
            }
        }
        #endregion
    }

    /// <summary>
    /// Specifies identifiers to indicate the return value of a KeypadForm.
    /// </summary>
    internal enum KeypadResult
    {
        BigButton,
        Option1,
        Option2,
        Option3,
        Option4,
        MagneticCard
    }
}