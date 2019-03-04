#region Copyright
// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2016 FortuNet, Inc
#endregion

//US4436: Close a bank from the POS
//US4698: POS: Denomination receipt
//US4767: POS > Close Bank: Add onscreen number key pad
//DE13214: POS > Close Bank: Total due is blank after midnight
//DE13314: POS > Close Bank: Unhandled exception error when closing when nothing was sold
//US5380: POS > Close Bank: Set the denomination order

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using GTI.Modules.POS.Business;
using GTI.Modules.POS.Properties;
using GTI.Modules.Shared;
using GTI.Modules.Shared.Data;
using System.ComponentModel;
using GTI.Modules.POS.Data;
using System.Windows.Forms;

// ReSharper disable once CheckNamespace
namespace GTI.Modules.POS.UI
{
    internal partial class CloseDropBankForm : POSForm
    {
        public enum BankCloseDisplayOptions
        {
            AllowBoth = 0,      // default
            DoNotAllowClose,    // they're just viewing, don't allow them to actually close
            ForceClose,         // They reconciled paper, they have to close their bank
        }

        #region Local Fields

        private readonly bool m_isDrop;
        private readonly string m_staffName;
        private readonly Bank m_bank;
        private readonly int m_sessionNumber; // 0 means all sessions
        private StaffSummaryControl m_totalDropSummaryControl;
        private StaffSummaryControl m_totalDueSummaryControl;
        private StaffSummaryControl m_totalPaperSalesSummaryControl; // US5024
        private StaffSummaryControl m_totalPaperUsageSummaryControl; // US4978
        private string m_currencyCode;
        private DenomControl m_selectedDenomControl;
        private BankCloseDisplayOptions m_displayOptions;

        #endregion

        #region Constructor
        public CloseDropBankForm(string staffName, int sessionNumber, PointOfSale parent, bool isDrop = false, BankCloseDisplayOptions displayOptions = BankCloseDisplayOptions.AllowBoth)
        {
            InitializeComponent();

            m_isDrop = isDrop;
            m_parent = parent;
            m_staffName = staffName;
            m_selectedDenomControl = null;
            m_displayOptions = displayOptions;
            m_sessionNumber = sessionNumber;
            
            BtnDropBank.Visible = false;

            //init title text
            TitleLabel.Text = string.Format("Close Bank for {0}", staffName);
            m_bank = parent.Bank;

            //Moved all the server messages to the "load" event handler. Need the window handle to use the waitform pop-up
            
            if (m_displayOptions == BankCloseDisplayOptions.DoNotAllowClose)
            {
                BtnSaveAndCloseBank.Enabled = BtnDropBank.Enabled = false;
                errIcon.Visible = closePaperLabel.Visible = true;
            }
            else if (m_displayOptions == BankCloseDisplayOptions.ForceClose)
            {
                BtnCancel.Enabled = false;
                errIcon.Visible = closePaperLabel.Visible = false;
            }
        }
        #endregion

        #region Methods

        #region KeyPad Events

        private void PressThisKey(object sender, EventArgs e)
        {
            SendKeyPress(((Button)sender).Text);
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            if (m_selectedDenomControl == null)
                return;

            m_selectedDenomControl.CountTextBox.Text = "0";
            m_selectedDenomControl.SetCountTextboxFocus();
        }

        private void BackButton_Click(object sender, EventArgs e)
        {
            SendKeyPress("\x08");
        }
        #endregion

        /// <summary>
        /// Actions that occur when this form finishes loading (the window is ready)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseDropBankForm_Load(object sender, EventArgs e)
        {
            //get info from server

            m_parent.RunWorker(Resources.WaitFormGetBankCurrencies, GetCurrencies, null, GetCurrenciesComplete);
            m_parent.ShowWaitForm(this); // window has to be initialized first when sending "this" as a parent. Halts here until background worker is complete

            m_parent.RunWorker(Resources.WaitFormGetStaffSales, GetStaffSales, null, GetStaffSalesComplete);
            m_parent.ShowWaitForm(this); // Halts here until background worker is complete
            
            m_parent.RunWorker(Resources.WaitFormGetPaperUsage, GetPaperUsage, null, GetPaperUsageComplete);
            m_parent.ShowWaitForm(this); // Halts here until background worker is complete

            //calculate over short
            CalculateOverShort();
            DenomListFlowPanel.Focus();
        }

        /// <summary>
        /// Raises event went the denom total changed
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void DenomTotalChangedEvent(object sender, EventArgs eventArgs)
        {
            var denom = sender as DenomControl;

            //check for null
            if (denom == null)
            {
                return;
            }

            //get sum of all controls
            decimal total = DenomListFlowPanel.Controls.OfType<DenomControl>().Sum(denomControl => denomControl.Total);

            //update UI controls
            TotalAmountLabel.Text = string.Format("{0:C}", total);
            m_totalDropSummaryControl.Value = total;

            CalculateOverShort();
        }

        /// <summary>
        /// Denoms key press enter event. Select the next denom
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void DenomEnterPressEvent(object sender, EventArgs eventArgs)
        {
            //current denom
            var denom = sender as DenomControl;

            //check for null
            if (denom == null)
            {
                return;
            }

            //flag to select next denom control
            var selectNextDenomControl = false;

            //iterate through all the denoms, and find the current denom
            foreach (var denomControl in DenomListFlowPanel.Controls.OfType<DenomControl>())
            {
                //if flag is set then set focus
                if (selectNextDenomControl)
                {
                    denomControl.SetCountTextboxFocus();
                    break;
                }

                //if current denom set flag to give focus to the next denom control
                if (denomControl.Denom.Id == denom.Denom.Id)
                {
                    selectNextDenomControl = true;
                }
            }
        }

        //US4767
        /// <summary>
        /// Denom control on focus event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void DenomControlOnFocusEvent(object sender, EventArgs eventArgs)
        {
            //update current selected denom control
            var control = sender as DenomControl;

            //unfocus previous denom control
            if (m_selectedDenomControl != null && m_selectedDenomControl != control)
            {
                m_selectedDenomControl.UpdateSelectedBackground(false);
            }

            m_selectedDenomControl = control;
        }

        /// <summary>
        /// Handles the Click event of the DenomListFlowPanel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void DenomListFlowPanel_Click(object sender, EventArgs e)
        {
            DenomListFlowPanel.Focus();
        }

        /// <summary>
        /// Handles the Click event of the CloseButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void CloseButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Handles the Click event of the SaveButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void SaveButton_Click(object sender, EventArgs e)
        {
            //iterate through currencies in the current bank
            foreach (var currency in m_bank.Currencies)
            {
                //clear and re-add
                currency.ClearDenominations();

                //add denoms with the appropriate count
                foreach (var control in DenomListFlowPanel.Controls)
                {
                    var denomControl = control as DenomControl;

                    if (denomControl == null)
                    {
                        continue;
                    }
                    if (denomControl.CurrencyName == currency.Name)
                    {
                        currency.AddDenomination(denomControl.Denom);
                    }
                }

                //update currency total
                currency.Total = m_totalDropSummaryControl.Value;
            }

            //US4698: POS: Denomination receipt
            if (!m_parent.Settings.PrintPosBankDenomReceipt)
            {
                return;
            }

            var denomList = new List<Denomination>();

            foreach (var control in DenomListFlowPanel.Controls)
            {
                var denom = control as DenomControl;

                if (denom == null || denom.Denom == null) // || denom.Denom.Count == 0) // JAN - not sure if they want this. Travis told me to leave in denoms that have no count
                {
                    continue;
                }

                denomList.Add(denom.Denom);
            }

            //check for presale session
            //if the selected session is a presale, then want to find the active sale session
            var session = m_parent.CurrentSession;
            if (m_parent.CurrentSession.IsPreSale && m_parent.ActiveSalesSession != null)
            {
                session = m_parent.ActiveSalesSession;
            }

            var receipt = new BankDenominationsReceipt
            {
                Denominations = denomList,
                StaffName = m_staffName,
                GamingDate = m_parent.GamingDate,
                SoldFromMachineId = m_parent.MachineId,
                Session = session.SessionNumber,
                CurrencyCode = m_currencyCode,
                OperatorHeaderLine1 = m_parent.Settings.ReceiptHeaderLine1,
                OperatorHeaderLine2 = m_parent.Settings.ReceiptHeaderLine2,
                OperatorHeaderLine3 = m_parent.Settings.ReceiptHeaderLine3,
                TotalDue = m_totalDueSummaryControl.Value, //DE13214
                TotalPaperSalesDue = m_totalPaperSalesSummaryControl.Value, //US5024
                TotalPaperUsageDue = m_totalPaperUsageSummaryControl == null ? 0 : m_totalPaperUsageSummaryControl.Value, //US4978, DE13314: possible null reference error. Have to check null
                BankCloseSignatureLineCount = m_parent.Settings.BankCloseSignatureLineCount, //DE13632
            };

            try
            {   
                receipt.Print(new Printer(m_parent.Settings.ReceiptPrinterName), (short)m_parent.Settings.NumberOfBankCloseReceipts);
            }
            catch (Exception ex)
            {
                POSMessageForm.Show(this, m_parent, string.Format(Resources.CloseBankSucceedFailedToPrintReceipt, ex.Message));
            }

            //End US4698: POS: Denomination receipt
        }

        /// <summary>
        /// Handles the Click event of the BtnDropBank control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void BtnDropBank_Click(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Calculates the over short.
        /// </summary>
        private void CalculateOverShort()
        {
            var totalDue = m_totalDueSummaryControl.Value;
            var totalDrop = m_totalDropSummaryControl.Value;
            var overShort = totalDrop - totalDue;

            //update the UI labels for over/short
            if (overShort >= 0)
            {
                OverShortValueLabel.ForeColor = Color.Lime;
                OverShortLabel.ForeColor = Color.Lime;
            }
            else
            {
                OverShortValueLabel.ForeColor = Color.Red;
                OverShortLabel.ForeColor = Color.Red;
            }

            OverShortValueLabel.Text = string.Format("{0:C}", overShort);
        }
        //US4767
        /// <summary>
        /// Updates the count textbox text
        /// </summary>
        /// <param name="text">The text.</param>
        private void SendKeyPress(string number)
        {
            if (m_selectedDenomControl == null)
                return;

            m_selectedDenomControl.SetCountTextboxFocus();

            SendKeys.Send(number);
        }

        #region Server Messages

        /// <summary>
        /// Load this machine's paper usage from the server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GetPaperUsage(object sender, DoWorkEventArgs e)
        {
            //load machine paper usage
            GetMachinePaperUsageMessage message;

            try
            {
                message = GetMachinePaperUsageMessage.GetMachineAuditNumbers(false);
            }
            catch (ServerException ex)
            {
                throw new POSException(string.Format(CultureInfo.CurrentCulture, Resources.GetPaperUsageFailed, ServerExceptionTranslator.FormatExceptionMessage(ex)), ex);
            }

            if (message != null)
            {
                e.Result = message.PaperUsageItems;
            }
        }

        /// <summary>
        /// Actions that occur when the "GetMachinePaperUsageMessage" message is complete
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GetPaperUsageComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                decimal totalUsage = 0;
                List<PaperUsageItem> paperUsageItems = (List<PaperUsageItem>)e.Result;

                if (paperUsageItems != null && paperUsageItems.Count > 0)
                {
                    foreach (var paperUsageItem in paperUsageItems)
                    {
                        totalUsage += paperUsageItem.Price * paperUsageItem.Quantity;
                    }
                }
                else
                {
                    BtnSaveAndCloseBank.Enabled = BtnDropBank.Enabled = true;
                    errIcon.Visible = closePaperLabel.Visible = false;
                }

                //DE13314
                //since this may not be initialized, we need to check for null to before getting the value
                if (totalUsage > 0)
                {
                    m_totalPaperUsageSummaryControl = new StaffSummaryControl("Total Usage", totalUsage);

                    AdditionSummaryListFlowPanel.Controls.Add(m_totalPaperUsageSummaryControl);
                }
            }
            else
            {
                POSMessageForm.Show(this, m_parent, e.Error.Message);
                BtnSaveAndCloseBank.Enabled = BtnDropBank.Enabled = true;
            }

            m_parent.CloseWaitForm();
        }

        /// <summary>
        /// Loads the available currencies from the server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GetCurrencies(object sender, DoWorkEventArgs e)
        {
            //load machine paper usage
            var getCurrencyMessage = new GetCurrencyDefinitionListMessage(string.Empty, false); // DE13299 only get active currencies
            try
            {
                getCurrencyMessage.Send();
            }
            catch (ServerException ex)
            {
                throw new POSException(string.Format(CultureInfo.CurrentCulture, Resources.GetCurrenciesFailed, ServerExceptionTranslator.FormatExceptionMessage(ex)), ex);
            }
            
            var getDenomCountMessage = new GetBankDenomsMessage(m_parent.Bank.Id);
            try
            {
                getDenomCountMessage.Send();
            }
            catch (ServerException ex)
            {
                throw new POSException(string.Format(CultureInfo.CurrentCulture, Resources.GetBankDenomCountsFailed, ServerExceptionTranslator.FormatExceptionMessage(ex)), ex);
            }
            e.Result = new Tuple<List<Currency>, Dictionary<int, int>>(getCurrencyMessage.Currencies.ToList(), getDenomCountMessage.DenomIdToCountDictionary);
        }

        /// <summary>
        /// Actions that occur when the "GetCurrencyDefinitionListMessage" message is complete
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GetCurrenciesComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                decimal total = 0;
                var dataTuple = e.Result as Tuple<List<Currency>, Dictionary<int, int>> ??
                                new Tuple<List<Currency>, Dictionary<int, int>>(new List<Currency>(), new Dictionary<int, int>());

                var currencies = dataTuple.Item1;
                var denomIdToCountDictionary = dataTuple.Item2;

                //init all denom controls
                foreach (var currency in currencies)
                {
                    //US5380: POS > Close Bank: Set the denomination order
                    IOrderedEnumerable<Denomination> sortedDenomList = currency.Denominations.OrderBy(x => x.Order).ThenBy(x => -x.Value).ThenBy(y => y.Name); // US5025 put the largest denoms first. Sort per currency grouping
                    
                    foreach (var denomination in sortedDenomList)
                    {
                        if (!denomination.IsActive)
                        {
                            continue;
                        }

                        //check to see if we have a count for the denom
                        if (denomIdToCountDictionary.ContainsKey(denomination.Id))
                        {
                            //set the denom count
                            denomination.Count = denomIdToCountDictionary[denomination.Id];
                        }

                        //create control
                        var denomControl = new DenomControl(denomination, currency.Name);

                        //add to UI
                        DenomListFlowPanel.Controls.Add(denomControl);

                        //listen for events
                        denomControl.TotalChangedEvent += DenomTotalChangedEvent;
                        denomControl.EnterPressEvent += DenomEnterPressEvent;
                        denomControl.FocusEvent += DenomControlOnFocusEvent;
                        denomControl.Click += DenomListFlowPanel_Click;

                        //update total
                        total += denomControl.Total;
                    }

                    m_currencyCode = currency.ISOCode;

                    //update total
                    TotalAmountLabel.Text = string.Format("{0:C}", total);
                }
            }
            else
            {
                POSMessageForm.Show(this, m_parent, e.Error.Message);

                TotalAmountLabel.Text = string.Format("{0:C}", 0);
            }

            m_parent.CloseWaitForm();
        }

        /// <summary>
        /// Loads the available currencies from the server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GetStaffSales(object sender, DoWorkEventArgs e)
        {
            //DE13214
            GetStaffTotalDueMessage message;

            try
            {
                message = GetStaffTotalDueMessage.GetTotalDue(m_parent.GamingDate, m_sessionNumber);
            }
            catch (ServerException ex)
            {
                throw new POSException(string.Format(CultureInfo.CurrentCulture, Resources.GetStaffSalesFailed, ServerExceptionTranslator.FormatExceptionMessage(ex)), ex);
            }

            if (message != null)
            {
                e.Result = new Tuple<decimal,decimal>(message.TotalDue, message.TotalPaperDue);
            }
        }

        /// <summary>
        /// Actions that occur when the "GetCurrencyDefinitionListMessage" message is complete
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GetStaffSalesComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                Tuple<decimal, decimal> results = (Tuple<decimal, decimal>)e.Result;
                decimal totalDue = results.Item1;
                decimal totalPaper = results.Item2; // US4978
                decimal totalDrop = DenomListFlowPanel.Controls.OfType<DenomControl>().Sum(denomControl => denomControl.Total);

                //Get issued amount. DE13199
                totalDue += m_bank.Currencies.Sum(i => i.Total);
                m_totalDueSummaryControl = new StaffSummaryControl("Total Due", totalDue);
                m_totalDropSummaryControl = new StaffSummaryControl("Total Drop", totalDrop);
                m_totalPaperSalesSummaryControl = new StaffSummaryControl("Total Paper", totalPaper);

                StaffSummaryListFlowPanel.Controls.Add(m_totalDueSummaryControl);
                StaffSummaryListFlowPanel.Controls.Add(m_totalDropSummaryControl);
                if (totalPaper != 0)
                    AdditionSummaryListFlowPanel.Controls.Add(m_totalPaperSalesSummaryControl);
            }
            else
            {
                decimal totalDrop = DenomListFlowPanel.Controls.OfType<DenomControl>().Sum(denomControl => denomControl.Total);
                m_totalDueSummaryControl = new StaffSummaryControl("Total Due", 0m);
                m_totalDropSummaryControl = new StaffSummaryControl("Total Drop", totalDrop);

                StaffSummaryListFlowPanel.Controls.Add(m_totalDueSummaryControl);
                StaffSummaryListFlowPanel.Controls.Add(m_totalDropSummaryControl);

                POSMessageForm.Show(this, m_parent, e.Error.Message);
            }

            m_parent.CloseWaitForm();
        }
        #endregion

        #endregion

        #region Public Properties

        public BankCloseDisplayOptions DisplayOptions
        {
            get { return m_displayOptions; }
            set
            {
                m_displayOptions = value;

                if (m_displayOptions == BankCloseDisplayOptions.DoNotAllowClose)
                {
                    BtnSaveAndCloseBank.Enabled = BtnDropBank.Enabled = false;
                    errIcon.Visible = closePaperLabel.Visible = true;
                }
                else if (m_displayOptions == BankCloseDisplayOptions.ForceClose)
                {
                    BtnCancel.Enabled = false;
                    errIcon.Visible = closePaperLabel.Visible = false;
                }
                else
                {
                    BtnSaveAndCloseBank.Enabled = BtnDropBank.Enabled = true;
                }
            }
        }

        #endregion
    }
}
