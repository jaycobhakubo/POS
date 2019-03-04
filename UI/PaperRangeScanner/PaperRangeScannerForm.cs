using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GTI.Controls;
using System.Threading;
using System.Text.RegularExpressions;
using GTI.Modules.POS.Properties;
using GTI.Modules.POS.Business;
using GTI.Modules.Shared;
using System.Globalization;

namespace GTI.Modules.POS.UI.PaperRangeScanner
{
    internal partial class PaperRangeScannerForm : POSForm
    {
        #region Constants

        public const int SERIAL_LENGTH = 8;
        public const int AUDIT_STR_LENGTH = 5;

        #endregion

        #region Private Members

        /// <summary>
        /// Used to stop entry after things are starting to be processed
        /// </summary>
        private bool _stopEntry = false;
        /// <summary>
        /// The last field to recieve focus (used for scanning and keyboard entry)
        /// </summary>
        protected object _lastFocus;
        /// <summary>
        /// The background worker that sends barcodes to the POS
        /// 
        /// JAN: I moved it off the background worker so I could get the status of the WaitForm, but it should be on a background worker so the user can cancel.
        /// </summary>
        private BackgroundWorker _sendBarcodesWorker;
        #endregion

        #region Public Properties

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
        public string AuditInfoMessage1
        {
            get
            {
                return infoMessageAudit1.Text;
            }
            set
            {
                infoMessageAudit1.Text = value;
            }
        }

        /// <summary>
        /// Get or sets the prompt for the Audit number message
        /// </summary>
        public string AuditInfoMessage2
        {
            get
            {
                return infoMessageAudit2.Text;
            }
            set
            {
                infoMessageAudit2.Text = value;
            }
        }
        #endregion

        #region Constructor

        public PaperRangeScannerForm(PointOfSale parent, DisplayMode displayMode)
            : base(parent, displayMode)
        {
            InitializeComponent();
            ApplyDisplayMode();

            m_virtualKeyboard.SetLayoutByCulture(CultureInfo.CurrentUICulture);
            if (CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "es")
                m_virtualKeyboard.ShiftImageIcon = Resources.ArrowUp;

            _sendBarcodesWorker = new BackgroundWorker();
            _sendBarcodesWorker.WorkerSupportsCancellation = true;

            //_sendBarcodesWorker.DoWork += new DoWorkEventHandler(sendBarcodesWorker_DoWork);
            //_sendBarcodesWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(sendBarcodesWorker_RunWorkerCompleted);
        }

        /// <summary>
        /// Actions that occur when this control finishes loading
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PaperRangeScannerForm_Load(object sender, EventArgs e)
        {
            _lastFocus = SerialNumberTxtBx;
        }
        #endregion

        #region Private Functions
        /// <summary>
        /// Sets the settings of this form based on the current display mode.
        /// </summary>
        protected override void ApplyDisplayMode()
        {
            Size s = this.Size;

            base.ApplyDisplayMode();

            // This is a special dialog, so override the default size.
            this.Size = s;
        }

        /// <summary>
        /// Checks to see if the last async. operation returned an exception and 
        /// show a message box if necessary.
        /// </summary>
        /// <returns>true if there was an exception; otherwise false.</returns>
        private bool CheckForError()
        {
            if (m_parent.LastAsyncException != null)
            {
                if (m_parent.LastAsyncException is ServerCommException)
                    m_parent.ServerCommFailed();
                else if (!(m_parent.LastAsyncException is POSUserCancelException))
                    POSMessageForm.Show(this, m_parent, m_parent.LastAsyncException.Message);

                return true;
            }
            else
                return false;
        }
        
        /// <summary>
        /// Clears the info message for the item that currently has focus
        /// </summary>
        private void ClearMessage()
        {
            if (SerialNumberTxtBx.Equals((TextBox)_lastFocus))
            {
                SerialInfoMessage = "";
            }
            else if (StartingAuditTxtBx.Equals((TextBox)_lastFocus))
            {
                AuditInfoMessage1 = "";
            }
            else if (EndingAuditTxtBx.Equals((TextBox)_lastFocus))
            {
                AuditInfoMessage2 = "";
            }
        }

        /// <summary>
        /// We got a barcode scan, lets try to decode it
        /// </summary>
        private void DecodeBarcodeScan(TextBox input)
        {
            string maybeBarcode = input.Text; // maybe it's a barcode

            //handle possible barcode scan
            if (maybeBarcode.Length == 13) //could be a paper barcode
            {
                string serial = maybeBarcode.Substring(0, SERIAL_LENGTH);
                string pack = maybeBarcode.Substring(SERIAL_LENGTH, AUDIT_STR_LENGTH);

                if (Regex.IsMatch(serial, @"^[0-9]{8}") && Regex.IsMatch(pack, @"^[0-9A-Za-z]{5}"))
                {
                    try
                    {
                        EndingAuditTxtBx.Text = BaseConverter.BaseConvertToDec(pack, 32).ToString();
                        SerialNumberTxtBx.Text = serial;
                    }
                    catch { } // unable to convert. Leave it alone
                }
            }
        }

        /// <summary>
        /// Does the work for the sendBarcodes background worker
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns>whether or not the barcodes were sent successfully</returns>
        private bool sendBarcodesWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            // Unbox the arguments.
            string serial;
            int startingAudit, endingAudit, curAudit;
            BackgroundWorker worker = (BackgroundWorker)sender;
            object[] args = (object[])e.Argument;
            serial =        (string)args[0];
            startingAudit = (int)args[1];
            endingAudit =   (int)args[2];
            curAudit = startingAudit;

            List<Tuple<int, int>> paperSkips = new List<Tuple<int, int>>();
            List<int> badAudits = new List<int>();
            //if (_dbAccessor.IsDBConnectionValid())
            //{
            //    paperSkips = _dbAccessor.GetPaperSkipsInRange(serial, startingAudit, endingAudit);
            //    badAudits.AddRange(_dbAccessor.SoldPaperInRange(serial, startingAudit, endingAudit));
            //    badAudits.AddRange(_dbAccessor.DamagedPaperInRange(serial, startingAudit, endingAudit));
            //    badAudits.Sort();
            //}

            int barcodeCount = endingAudit - startingAudit; // the number of barcodes to send to the POS

            for (int i = 0; i < barcodeCount; i++)
            {
                if (worker.CancellationPending)
                    break;

                while (paperSkips.Count > 0 && paperSkips.First().Item2 < curAudit)
                    paperSkips.RemoveAt(0); // we're beyond that range, remove it from the list
                while (badAudits.Count > 0 && badAudits.First() < curAudit)
                    badAudits.RemoveAt(0);  // we're beyond that audit, remove it from the list

                if ((paperSkips.Count == 0 
                    || curAudit < paperSkips.First().Item1 || curAudit > paperSkips.First().Item2)
                    && !badAudits.Contains(curAudit) ) // if it's not within the skip range and not a bad audit
                {
                    string base32Audit = BaseConverter.BaseConvertFromDec((ulong)curAudit, 32); // Have to send the serial as 32

                    string barcode = String.Format("{0}{1}",
                        serial.ToString().PadLeft(SERIAL_LENGTH, '0'),
                        base32Audit.PadLeft(AUDIT_STR_LENGTH, '0'));
                    
                    if (!SendBarcodeToPOS(barcode))
                        return false;
                }

                curAudit++;

            }

            return true;
        }

        /// <summary>
        /// Sends the sent in barcode to the POS window
        /// </summary>
        /// <param name="barcode"></param>
        /// <returns>Whether or not an item was found for the sent in barcode</returns>
        private bool SendBarcodeToPOS(string barcode)
        {
            bool foundItem = false;
            try
            {
                m_parent.StartGetProduct(barcode);
                m_parent.ShowWaitForm(this, true);

                if (!CheckForError())
                {
                    if (m_parent.GetProductDataButton != null)
                    {
                        m_parent.GetProductDataButton.Click(this, m_parent.GetProductDataButtonValues);
                        foundItem = true;
                    }
                }
            }
            catch (Exception ex)
            {
                m_parent.Log("Failed to find the product for barcode: " + ex.Message, LoggerLevel.Warning);
                POSMessageForm.Show(this, m_parent, string.Format(CultureInfo.CurrentCulture, Resources.GetProductDataByBarcodeFailed, ex.Message));
            }
            return foundItem;
        }

        /// <summary>
        /// Actions that occur when the background worker is done running
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sendBarcodesWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Visible = true;
        }
        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles when a text box is clicked / touched to ensure proper text entry and barcode scanning
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClickTextBox(object sender, EventArgs e)
        {
            if (SerialNumberTxtBx.Equals((TextBox)sender))
            {
                SerialNumberTxtBx.Focus();
                _lastFocus = SerialNumberTxtBx;
            }
            else if (StartingAuditTxtBx.Equals((TextBox)sender))
            {
                StartingAuditTxtBx.Focus();
                _lastFocus = StartingAuditTxtBx;
            }
            else if (EndingAuditTxtBx.Equals((TextBox)sender))
            {
                EndingAuditTxtBx.Focus();
                _lastFocus = EndingAuditTxtBx;
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
            if (_stopEntry)
            {
                e.Handled = true;
                return;
            }

            ClearMessage();

            if (e.KeyCode == Keys.Enter && sender is TextBox) // we're assuming this means they scanned something
            {
                DecodeBarcodeScan((TextBox)sender);
                if (SerialNumberTxtBx.Equals((TextBox)sender))
                {
                    StartingAuditTxtBx.Focus();
                    _lastFocus = StartingAuditTxtBx;
                }
                else if (StartingAuditTxtBx.Equals((TextBox)sender))
                {
                    EndingAuditTxtBx.Focus();
                    _lastFocus = EndingAuditTxtBx;
                }
                else if (EndingAuditTxtBx.Equals((TextBox)sender))
                {
                    OkBtn_Click(this, new EventArgs());
                }
            }
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
            if (_stopEntry)
            {
                e.Handled = true;
                return;
            }

            if (!char.IsDigit(e.KeyChar) && e.KeyChar != (char)Keys.Back)
                e.Handled = true;
        }

        /// <summary>
        /// Actions that occur when the cancel button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelBtn_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Handles when a key on the virtual keyboard is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">A KeyboardEventArgs object that contains the 
        /// event data.</param>
        private void KeyboardKeyPressed(object sender, KeyboardEventArgs e)
        {
            if (_stopEntry)
                return;

            ClearMessage();
            
            if (_lastFocus is Control && (_lastFocus != m_virtualKeyboard))
            {
                // Trap the tab keys and handle them internally
                if (e.KeyPressed == "{TAB}" || e.KeyPressed == "+{TAB}")
                {
                    if (SerialNumberTxtBx.Equals((TextBox)_lastFocus))
                    {
                        StartingAuditTxtBx.Focus();
                        _lastFocus = StartingAuditTxtBx;
                    }
                    else if (StartingAuditTxtBx.Equals((TextBox)_lastFocus))
                    {
                        EndingAuditTxtBx.Focus();
                        _lastFocus = EndingAuditTxtBx;
                    }
                }
                else
                {
                    // This was not a tab key so pass it on 
                    //  to the key handler
                    ((Control)_lastFocus).Focus();
                    SendKeys.Send(e.KeyPressed);
                }
            }
        }

        /// <summary>
        /// Actions that occur when the OK button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OkBtn_Click(object sender, EventArgs e)
        {
            bool success = true;
            string serial = SerialNumberTxtBx.Text;
            int startingAudit = 0, endingAudit = 0;

            if (String.IsNullOrWhiteSpace(serial))
            {
                SerialInfoMessage = Resources.InputRequired;
                SerialNumberTxtBx.Focus();
                _lastFocus = SerialNumberTxtBx;
                success = false;
            }
            if (String.IsNullOrWhiteSpace(StartingAuditTxtBx.Text) || !Int32.TryParse(StartingAuditTxtBx.Text, out startingAudit))
            {
                AuditInfoMessage1 = Resources.InvalidAuditNumber;
                if (success) // if this is the first failure, then set the focus
                {
                    StartingAuditTxtBx.Focus();
                    _lastFocus = StartingAuditTxtBx;
                    success = false;
                }
            }
            if (String.IsNullOrWhiteSpace(EndingAuditTxtBx.Text) || !Int32.TryParse(EndingAuditTxtBx.Text, out endingAudit))
            {
                AuditInfoMessage2 = Resources.InvalidAuditNumber;
                if (success) // if this is the first failure, then set the focus
                {
                    EndingAuditTxtBx.Focus();
                    _lastFocus = EndingAuditTxtBx;
                    success = false;
                }
            }
            if (success && startingAudit >= endingAudit) // DE13783 only check valid input here if all other input is valid
            {
                AuditInfoMessage2 = Resources.InvalidStartEndAudit;
                if (success) // if this is the first failure, then set the focus
                {
                    EndingAuditTxtBx.Focus();
                    _lastFocus = EndingAuditTxtBx;
                    success = false;
                }
            }

            Application.DoEvents(); 
            
            if (success && !_sendBarcodesWorker.IsBusy)
            {
                try
                {
                    OkBtn.Enabled = false;
                    Cursor = Cursors.WaitCursor;

                    object[] args = new object[3];
                    args[0] = serial;
                    args[1] = startingAudit;
                    args[2] = endingAudit;

                    if (sendBarcodesWorker_DoWork(_sendBarcodesWorker, new DoWorkEventArgs(args)))
                        CancelBtn_Click(this, null);
                    //_sendBarcodesWorker.RunWorkerAsync(args);
                }
                catch(Exception)
                {
                }
                finally
                {
                    Cursor = Cursors.Default;
                    OkBtn.Enabled = true;
                }
            }            
        }

        /// <summary>
        /// Actions that occur when the clear button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void imageButton1_Click(object sender, EventArgs e)
        {
            ClickTextBox(SerialNumberTxtBx, new EventArgs());
            SerialNumberTxtBx.Text = StartingAuditTxtBx.Text = EndingAuditTxtBx.Text = "";
        }
        #endregion

    }
}
