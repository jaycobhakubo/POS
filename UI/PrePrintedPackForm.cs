#region Copyright
// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2009 GameTech
// International, Inc.
#endregion

// Rally US510

using System;
using System.Globalization;
using System.Windows.Forms;
using GTI.Controls;
using GTI.Modules.Shared;
using GTI.Modules.POS.Business;
using GTI.Modules.POS.Properties;

namespace GTI.Modules.POS.UI
{
    /// <summary>
    /// Represents a form used to collect a pre-printed pack information bar
    /// code.
    /// </summary>
    internal partial class PrePrintedPackForm : POSForm
    {
        #region Member Variables
        protected object m_lastFocus;
        protected PrePrintedPackInfo m_info;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the PrePrintedPackForm class.
        /// </summary>
        /// <param name="parent">The PointOfSale to which this form 
        /// belongs.</param>
        /// <param name="displayMode">The display mode used to show this 
        /// form.</param>
        /// <exception cref="System.ArgumentNullException">parent or 
        /// displayMode is a null reference.</exception>
        public PrePrintedPackForm(PointOfSale parent)
            : base(parent)
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
            if(string.IsNullOrEmpty(m_textBox.Text) || m_textBox.Text.Trim().Length == 0)
                POSMessageForm.Show(this, m_parent, Resources.InputRequired);
            else
            {
                // Attempt to parse the input string.
                try
                {
                    m_info = PrePrintedPackInfo.Parse(m_textBox.Text.Trim());

                    m_info.Barcode = m_textBox.Text.Trim();

                    DialogResult = DialogResult.OK;
                    Close();
                }
                catch(Exception ex)
                {
                    m_textBox.Text = null;
                    m_info = null;
                    m_parent.Log("Failed to decode the pre-printed pack info: " + ex.Message, LoggerLevel.Warning);
                    POSMessageForm.Show(this, m_parent, Resources.PrePrintedDecodeFailed);
                }
            }
        }
        #endregion

        #region Member Properties
        /// <summary>
        /// Gets the pre-printed pack information entered on the form.
        /// </summary>
        public PrePrintedPackInfo PrePrintedPackInfo
        {
            get
            {
                return m_info;
            }
        }
        #endregion
    }
}
