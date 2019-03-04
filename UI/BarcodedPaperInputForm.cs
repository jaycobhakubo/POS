#region Copyright
// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2008 GameTech
// International, Inc.
#endregion

// US2826 Adding support for barcoded paper

using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using GTI.Controls;
using GTI.Modules.Shared;
using GTI.Modules.POS.Business;
using GTI.Modules.POS.Properties;
using System.Text.RegularExpressions;
using System.Drawing.Drawing2D;
using System.Text;

namespace GTI.Modules.POS.UI
{
    /// <summary>
    /// Represents a form used to collect a single line of input.
    /// </summary>
    internal partial class BarcodedPaperInputForm : POSForm
    {
        #region Member Variables
        protected object m_lastFocus;
        protected bool m_requireInput;
        private bool m_stopEntry = false;
        private DateTime m_idleSince = DateTime.Now;
        private StringBuilder m_kioskBarcode = new StringBuilder();
        private string m_titleText;
        private bool m_isBarcode = true;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the BarcodedPaperInputForm class.
        /// </summary>
        /// <param name="parent">The PointOfSale to which this form 
        /// belongs.</param>
        /// <param name="displayMode">The display mode used to show this 
        /// form.</param>
        /// <param name="requireInput">Whether any input is required to press 
        /// the ok button.</param>
        /// <exception cref="System.ArgumentNullException">parent or 
        /// displayMode is a null reference.</exception>
        public BarcodedPaperInputForm(PointOfSale parent, DisplayMode displayMode, bool requireInput, string titleText)
            : base(parent, displayMode)
        {
            InitializeComponent();
            ApplyDisplayMode();

            m_titleText = titleText;

            if (m_parent.WeAreNotAPOSKiosk)
            {
                m_btnEnterManually.Visible = true;
                m_btnCancel.Location = new Point(548, 534);
                m_btnCancel.ImageNormal = Resources.BlueButtonUp;
                m_btnCancel.ImagePressed = Resources.BlueButtonDown;
            }

            m_serialNumberLabel.Visible = false;
            m_serialNumber.Visible = false;
            infoMessageSerial.Visible = false;
            m_auditNumberLabel.Visible = false;
            m_auditNumber.Visible = false;
            infoMessageAudit.Visible = false;
            m_virtualKeyboard.Visible = false;
            m_okButton.Visible = false;
            m_cancelButton.Visible = false;

            BackgroundImage = null;
            DrawAsGradient = true;
            m_btnCancel.Visible = true;
            m_btnCancel.UseClickSound = m_parent.Settings.UseKeyClickSoundsOnKiosk;
            m_lblInstructions.Size = new Size(m_lblInstructions.Size.Width, 512);

            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.BarcodedPaperInputForm_KeyPress);

            m_requireInput = requireInput;

            // Set the last focused control to the text box.
            m_lastFocus = m_serialNumber;

            m_serialNumberLabel.Text = Resources.PaperInfoSerialNumberOrBarcodeScan;
            m_auditNumberLabel.Text = Resources.PaperInfoAuditNumber;

            // Which keyboard do we need to use?
            // PDTS 964
            m_virtualKeyboard.SetLayoutByCulture(CultureInfo.CurrentUICulture);

            if(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "es")
                m_virtualKeyboard.ShiftImageIcon = Resources.ArrowUp;

            m_lblInstructions.Text = string.Format(Resources.PleaseScanPaper, titleText);

            string packName = titleText.ToUpper();

            if (packName.Contains("BLUE"))
            {
                m_lblInstructions.ForeColor = Color.Blue;
                m_lblInstructions.ShowEdge = false;
            }
            else if (packName.Contains("RED"))
            {
                m_lblInstructions.ForeColor = Color.Maroon;
                m_lblInstructions.ShowEdge = false;
            }
            else if (packName.Contains("GREEN"))
            {
                m_lblInstructions.ForeColor = Color.LimeGreen;
                m_lblInstructions.ShowEdge = true;
            }
            else if (packName.Contains("ORANGE"))
            {
                m_lblInstructions.ForeColor = Color.Orange;
                m_lblInstructions.ShowEdge = true;
            }
            else if (packName.Contains("TAN"))
            {
                m_lblInstructions.ForeColor = Color.Tan;
                m_lblInstructions.ShowEdge = true;
            }
            else if (packName.Contains("PURPLE"))
            {
                m_lblInstructions.ForeColor = Color.Purple;
                m_lblInstructions.ShowEdge = false;
            }
            else if (packName.Contains("RAINBOW"))
            {
                m_lblInstructions.ForeColor = Color.Violet;
                m_lblInstructions.ShowEdge = true;
            }
            else if (packName.Contains("GOLD"))
            {
                m_lblInstructions.ForeColor = Color.Goldenrod;
                m_lblInstructions.ShowEdge = true;
            }
            else
            {
                m_lblInstructions.ForeColor = Color.Black;
                m_lblInstructions.ShowEdge = false;
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
            Size = new Size(800, 611); //m_displayMode.LargeDialogSize;
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
           
            if (m_stopEntry)
                return;

            if (m_parent.WeAreAPOSKiosk || m_btnEnterManually.Visible)
            {
                m_kioskBarcode.Append(e.KeyPressed);
                return;
            }

            ClearMessage();

            if(m_lastFocus is Control && (m_lastFocus != m_virtualKeyboard))
            {
                // Trap the tab keys and handle them internally
                if (e.KeyPressed == "{TAB}" || e.KeyPressed == "+{TAB}")
                {
                    if (m_serialNumber.Equals((TextBox)m_lastFocus))
                    {
                        m_auditNumber.Focus();
                        m_lastFocus = m_auditNumber;
                    }
                    else
                    {
                        m_serialNumber.Focus();
                        m_lastFocus = m_serialNumber;
                    }
                }
                else
                {
                    // This was not a tab key so pass it on 
                    //  to the key handler
                    ((Control)m_lastFocus).Focus();
                    SendKeys.Send(e.KeyPressed);
                }
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

            if (m_stopEntry)
            {
                e.Handled = true;
                return;
            }

            ClearMessage();
            
            if(e.KeyCode == Keys.Enter)
                OkClick(this, new EventArgs());
        }

        /// <summary>
        /// Handles a key press for the Audit Number control to ensure
        /// that only numbers are allowed to be entered
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">A KeyPressEventArgs object that contains
        /// the event data</param>
        private void AuditKeyPress(object sender, KeyPressEventArgs e)
        {
            NotIdle();

            if (m_stopEntry)
            {
                e.Handled = true;
                return;
            }

            if (!char.IsDigit(e.KeyChar) && e.KeyChar != (char)Keys.Back)
                e.Handled = true;
        }

        /// <summary>
        /// Handles when a text box is clicked / touched to ensure
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClickTextBox(object sender, EventArgs e)
        {
            NotIdle();

            if (m_serialNumber.Equals((TextBox)sender))
            {
                m_serialNumber.Focus();
                m_lastFocus = m_serialNumber;
            }
            else if (m_auditNumber.Equals((TextBox)sender))
            {
                m_auditNumber.Focus();
                m_lastFocus = m_auditNumber;
            }
        }

        /// <summary>
        /// Handles the ok button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the 
        /// event data.</param>
        private void OkClick(object sender, EventArgs e)
        {
            m_stopEntry = true;
            m_isBarcode = m_parent.WeAreAPOSKiosk || m_btnEnterManually.Visible;

            bool done = true;

            if (m_isBarcode)
            {
                m_serialNumber.Text = m_kioskBarcode.ToString();
                m_kioskBarcode.Clear();
            }

            string serialNumber = m_serialNumber.Text;
            string auditNumber = m_auditNumber.Text;

            //handle possible barcode scan
            if (m_isBarcode) //barcode was scanned
                m_auditNumber.Text = "-1";
            
            if (string.IsNullOrEmpty(m_serialNumber.Text) && m_serialNumber.Text.Trim().Length == 0)
            {
                SerialInfoMessage = Resources.InputRequired;
                m_serialNumber.Focus();
                m_lastFocus = m_serialNumber;
                done = false;
            }
            
            if (string.IsNullOrEmpty(m_auditNumber.Text) && m_auditNumber.Text.Trim().Length == 0)
            {
                AuditInfoMessage = Resources.InputRequired;

                if (done)
                {
                    m_auditNumber.Focus();
                    m_lastFocus = m_auditNumber;
                    done = false;
                }
            }
            else if (!m_btnEnterManually.Visible && !Regex.IsMatch(m_auditNumber.Text, @"^\d+$"))
            {
                // Make sure that this a number
                AuditInfoMessage = Resources.InvalidAuditNumber;

                if (done)
                {
                    m_auditNumber.Focus();
                    m_auditNumber.SelectAll();
                    m_lastFocus = m_auditNumber;
                    done = false;
                }
            }

            Application.DoEvents();

            if (done)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                m_stopEntry = false;
            }
        }

        /// <summary>
        /// Clears the info message for the item that currently has focus
        /// </summary>
        private void ClearMessage()
        {
            if (m_serialNumber.Equals((TextBox)m_lastFocus))
            {
                SerialInfoMessage = "";
            }
            else if (m_auditNumber.Equals((TextBox)m_lastFocus))
            {
                AuditInfoMessage = "";
            }
        }

        private void m_cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void m_kioskIdleTimer_Tick(object sender, EventArgs e)
        {
            if (m_parent.GuardianHasUsSuspended) //don't do anything
            {
                NotIdle();
                return;
            }

            TimeSpan idleFor = DateTime.Now - m_idleSince;

            if (idleFor > TimeSpan.FromMilliseconds(m_parent.SellingForm.KioskShortIdleLimitInSeconds / 3 * 2000))
            {
                if (!m_timeoutProgress.Visible)
                    m_timeoutProgress.Visible = true;

                m_timeoutProgress.Increment(m_kioskIdleTimer.Interval);

                if (m_timeoutProgress.Value >= m_timeoutProgress.Maximum)
                {
                    DialogResult = DialogResult.Cancel;
                    m_parent.SellingForm.ForceKioskTimeout(1);
                    Close();
                }
            }
        }

        private void BarcodedPaperInputForm_Shown(object sender, EventArgs e)
        {
            if (m_parent.WeAreAPOSKiosk)
            {
                m_parent.SellingForm.NotIdle(true); //stop the selling form triggers
                NotIdle();
                m_timeoutProgress.Maximum = (m_parent.SellingForm.KioskShortIdleLimitInSeconds / 3) * 1000;
                m_kioskIdleTimer.Enabled = true;
            }
        }

        private void BarcodedPaperInputForm_FormClosed(object sender, FormClosedEventArgs e)
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

        private void BarcodedPaperInputForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            m_kioskBarcode.Append(e.KeyChar);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (m_parent.WeAreAPOSKiosk || m_btnEnterManually.Visible)
            {
                if (keyData == Keys.Return || keyData == Keys.Enter)
                {
                    OkClick(null, new EventArgs());
                    return true;
                }
                else
                {
                    return base.ProcessCmdKey(ref msg, keyData);
                }
            }
            else
            {
                return base.ProcessCmdKey(ref msg, keyData);
            }
        }
        private void UserActivityDetected(object sender, EventArgs e)
        {
            NotIdle();
        }

        private void m_btnEnterManually_Click(object sender, EventArgs e)
        {
            m_serialNumberLabel.Visible = true;
            m_serialNumber.Visible = true;
            infoMessageSerial.Visible = true;
            m_auditNumberLabel.Visible = true;
            m_auditNumber.Visible = true;
            infoMessageAudit.Visible = true;
            m_virtualKeyboard.Visible = true;
            m_okButton.Visible = true;
            m_cancelButton.Visible = true;
            m_btnScanPack.Visible = true;

            BackgroundImage = Resources.PaperInputBack;
            DrawAsGradient = false;
            m_btnCancel.Visible = false;
            m_btnEnterManually.Visible = false;
            m_lblInstructions.Text = m_titleText;
            m_lblInstructions.Size = new Size(m_lblInstructions.Size.Width, 53);

            this.KeyPress -= new System.Windows.Forms.KeyPressEventHandler(this.BarcodedPaperInputForm_KeyPress);
            m_lastFocus = m_serialNumber;
            m_kioskBarcode.Clear();
            m_serialNumber.Focus();
        }

        private void m_btnScanPack_Click(object sender, EventArgs e)
        {
            m_btnEnterManually.Visible = true;
            m_btnScanPack.Visible = false;
            m_btnCancel.Location = new Point(548, 534);
            m_serialNumberLabel.Visible = false;
            m_serialNumber.Visible = false;
            infoMessageSerial.Visible = false;
            m_auditNumberLabel.Visible = false;
            m_auditNumber.Visible = false;
            infoMessageAudit.Visible = false;
            m_virtualKeyboard.Visible = false;
            m_okButton.Visible = false;
            m_cancelButton.Visible = false;
            BackgroundImage = null;
            DrawAsGradient = true;
            m_btnCancel.Visible = true;
            m_btnCancel.UseClickSound = m_parent.Settings.UseKeyClickSoundsOnKiosk;
            m_lblInstructions.Size = new Size(m_lblInstructions.Size.Width, 512);
            m_lastFocus = m_serialNumber;
            m_lblInstructions.Text = string.Format(Resources.PleaseScanPaper, m_titleText);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.BarcodedPaperInputForm_KeyPress);
            this.Focus();
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
        public string SerialInfoMessage
        {
            get
            {
                return infoMessageSerial.Text;
            }
            set
            {
                infoMessageSerial.Text = value;
            }
        }

        /// <summary>
        /// Get or sets the prompt for the Audit number message
        /// </summary>
        public string AuditInfoMessage
        {
            get
            {
                return infoMessageAudit.Text;
            }
            set
            {
                infoMessageAudit.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the audit number typed in by the user.
        /// </summary>
        public string AuditNumber
        {
            get
            {
                return m_auditNumber.Text;
            }
            set
            {
                m_auditNumber.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the audit number typed in by the user.
        /// </summary>
        public string SerialNumber
        {
            get
            {
                //DE12740: Add leading zeros
                return m_isBarcode ? m_serialNumber.Text : m_serialNumber.Text; //.PadLeft(8, '0');
            }

            set
            {
                m_serialNumber.Text = value;
            }
        }

        #endregion
    }
}