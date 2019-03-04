#region Copyright
// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2011 GameTech
// International, Inc.
#endregion

// US1955

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GTI.Modules.POS.Business;
using GTI.Modules.Shared;
using System.Globalization;
using GTI.Modules.POS.Properties;
using GTI.Controls;

namespace GTI.Modules.POS.UI
{
    /// <summary>
    /// Represents a form used to collect a password.
    /// </summary>
    internal partial class PasswordForm : POSForm
    {
        #region Member Variables
        protected object m_lastFocus;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the PasswordForm class.
        /// </summary>
        /// <param name="parent">The PointOfSale to which this form 
        /// belongs.</param>
        /// <param name="displayMode">The display mode used to show this 
        /// form.</param>
        /// <exception cref="System.ArgumentNullException">parent or 
        /// displayMode is a null reference.</exception>
        public PasswordForm(PointOfSale parent, DisplayMode displayMode)
            : base(parent, displayMode)
        {
            InitializeComponent();
            ApplyDisplayMode();

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
            Size = new Size(800, 455);
        }

        /// <summary>
        /// Handles when a key on the virtual keyboard is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">A KeyboardEventArgs object that contains the 
        /// event data.</param>
        private void KeyboardKeyPressed(object sender, KeyboardEventArgs e)
        {
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
            if(e.KeyCode == Keys.Enter)
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
            if(string.IsNullOrWhiteSpace(m_textBox.Text))
                POSMessageForm.Show(this, m_parent, Resources.InputRequired);
            else
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }
        #endregion

        #region Member Methods
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

        public bool EnterPressesOK
        {
            get
            {
                return AcceptButton == m_okButton;
            }

            set
            {
                if (value)
                    AcceptButton = m_okButton;
                else
                    AcceptButton = null;
            }
        }
        #endregion
    }
}
