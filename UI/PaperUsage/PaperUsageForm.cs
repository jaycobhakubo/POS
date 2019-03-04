#region Copyright
// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2016 FortuNet, Inc.
#endregion

//US4955: POS > Paper Usage: Damaged
//US5200 POS: Do not display bank close UI when in money center mode.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Timers;
using System.Windows.Forms;
using GTI.Modules.POS.Business;
using GTI.Modules.POS.Data;
using GTI.Modules.POS.UI.PaperUsage;
using GTI.Modules.Shared;
using System.ComponentModel;
using GTI.Modules.POS.Properties;

// ReSharper disable once CheckNamespace
namespace GTI.Modules.POS.UI
{
    internal partial class PaperUsageForm : POSForm
    {
        #region Local Fields

        private readonly BankOpenType m_bankOpenType;
        private readonly System.Timers.Timer m_statusLabelTimer;
        private TextBox m_selectedTextBox;
        private readonly IWin32Window m_owner;
        private readonly string m_version;
        private PaperUsageItemControl m_selectedPaperUsageControl;
        private readonly List<PaperUsageItem> m_listOfItemsToBeRemoved;
        private bool m_itemModified;
        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="PaperUsageForm" /> class.
        /// </summary>
        /// <param name="owner">Window owner</param>
        /// <param name="parent">The parent</param>
        /// <param name="version">The version.</param>
        /// <param name="bankOpenType">Flag to determine opened on startup</param>
        public PaperUsageForm(IWin32Window owner, PointOfSale parent, string version, BankOpenType bankOpenType):base(parent, parent.Settings.DisplayMode)
        {
            InitializeComponent();

            if (parent.Settings.DisplayMode is CompactDisplayMode)
            {
                this.SuspendLayout();
                this.BtnRemove.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
                this.BtnRemove.Location = new System.Drawing.Point(12, 561);
                this.BtnRemove.Size = new System.Drawing.Size(120, 35);
                this.BtnConfirmAndPrint.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
                this.BtnConfirmAndPrint.Location = new System.Drawing.Point(536, 561);
                this.BtnConfirmAndPrint.Size = new System.Drawing.Size(120, 35);
                this.BtnPrintUnscannedPacks.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
                this.BtnPrintUnscannedPacks.Location = new System.Drawing.Point(590, 311);
                this.BtnPrintUnscannedPacks.Size = new System.Drawing.Size(183, 30);
                this.BtnPrint.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
                this.BtnPrint.Location = new System.Drawing.Point(274, 561);
                this.BtnPrint.Size = new System.Drawing.Size(120, 35);
                this.m_backButton.Font = new System.Drawing.Font("Tahoma", 24F, System.Drawing.FontStyle.Bold);
                this.m_backButton.Location = new System.Drawing.Point(721, 485);
                this.m_backButton.Size = new System.Drawing.Size(60, 40);
                this.m_decimalButton.Font = new System.Drawing.Font("Tahoma", 24F, System.Drawing.FontStyle.Bold);
                this.m_decimalButton.Location = new System.Drawing.Point(580, 485);
                this.m_decimalButton.Size = new System.Drawing.Size(60, 40);
                this.m_button2.Font = new System.Drawing.Font("Tahoma", 24F, System.Drawing.FontStyle.Bold);
                this.m_button2.Location = new System.Drawing.Point(651, 441);
                this.m_button2.Size = new System.Drawing.Size(60, 40);
                this.m_button3.Font = new System.Drawing.Font("Tahoma", 24F, System.Drawing.FontStyle.Bold);
                this.m_button3.Location = new System.Drawing.Point(721, 441);
                this.m_button3.Size = new System.Drawing.Size(60, 40);
                this.m_button6.Font = new System.Drawing.Font("Tahoma", 24F, System.Drawing.FontStyle.Bold);
                this.m_button6.Location = new System.Drawing.Point(721, 397);
                this.m_button6.Size = new System.Drawing.Size(60, 40);
                this.m_button5.Font = new System.Drawing.Font("Tahoma", 24F, System.Drawing.FontStyle.Bold);
                this.m_button5.Location = new System.Drawing.Point(651, 397);
                this.m_button5.Size = new System.Drawing.Size(60, 40);
                this.m_button4.Font = new System.Drawing.Font("Tahoma", 24F, System.Drawing.FontStyle.Bold);
                this.m_button4.Location = new System.Drawing.Point(580, 397);
                this.m_button4.Size = new System.Drawing.Size(60, 40);
                this.m_button7.Font = new System.Drawing.Font("Tahoma", 24F, System.Drawing.FontStyle.Bold);
                this.m_button7.Location = new System.Drawing.Point(580, 353);
                this.m_button7.Size = new System.Drawing.Size(60, 40);
                this.m_button8.Font = new System.Drawing.Font("Tahoma", 24F, System.Drawing.FontStyle.Bold);
                this.m_button8.Location = new System.Drawing.Point(651, 353);
                this.m_button8.Size = new System.Drawing.Size(60, 40);
                this.m_button9.Font = new System.Drawing.Font("Tahoma", 24F, System.Drawing.FontStyle.Bold);
                this.m_button9.Location = new System.Drawing.Point(721, 353);
                this.m_button9.Size = new System.Drawing.Size(60, 40);
                this.m_button0.Font = new System.Drawing.Font("Tahoma", 24F, System.Drawing.FontStyle.Bold);
                this.m_button0.Location = new System.Drawing.Point(651, 485);
                this.m_button0.Size = new System.Drawing.Size(60, 40);
                this.m_button1.Font = new System.Drawing.Font("Tahoma", 24F, System.Drawing.FontStyle.Bold);
                this.m_button1.Location = new System.Drawing.Point(580, 441);
                this.m_button1.Size = new System.Drawing.Size(60, 40);
                this.TotalLabel.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                this.TotalLabel.Location = new System.Drawing.Point(417, 507);
                this.TotalLabel.Size = new System.Drawing.Size(137, 18);
                this.BtnClosePaper.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
                this.BtnClosePaper.Location = new System.Drawing.Point(143, 561);
                this.BtnClosePaper.Size = new System.Drawing.Size(120, 35);
                this.StaffLabel.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                this.StaffLabel.Location = new System.Drawing.Point(84, 8);
                this.StaffLabel.Size = new System.Drawing.Size(79, 16);
                this.SessionLabel.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                this.SessionLabel.Location = new System.Drawing.Point(84, 26);
                this.SessionLabel.Size = new System.Drawing.Size(16, 16);
                this.StatusLabel.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                this.StatusLabel.Location = new System.Drawing.Point(12, 507);
                this.StatusLabel.Size = new System.Drawing.Size(101, 18);
                this.label8.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                this.label8.Location = new System.Drawing.Point(465, 46);
                this.label8.Size = new System.Drawing.Size(31, 16);
                this.label15.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                this.label15.Location = new System.Drawing.Point(353, 506);
                this.label15.Size = new System.Drawing.Size(58, 19);
                this.UnscannedPacksListBox.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                this.UnscannedPacksListBox.Location = new System.Drawing.Point(575, 71);
                this.UnscannedPacksListBox.Size = new System.Drawing.Size(212, 234);
                this.PaperUsageFlowPanel.Location = new System.Drawing.Point(12, 72);
                this.PaperUsageFlowPanel.Size = new System.Drawing.Size(548, 431);
                this.label14.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                this.label14.Location = new System.Drawing.Point(577, 46);
                this.label14.Size = new System.Drawing.Size(137, 18);
                this.label17.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                this.label17.Location = new System.Drawing.Point(19, 26);
                this.label17.Size = new System.Drawing.Size(66, 16);
                this.label16.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                this.label16.Location = new System.Drawing.Point(19, 8);
                this.label16.Size = new System.Drawing.Size(49, 16);
                this.label11.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                this.label11.Location = new System.Drawing.Point(456, 46);
                this.label11.Size = new System.Drawing.Size(40, 16);
                this.label7.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                this.label7.Location = new System.Drawing.Point(397, 46);
                this.label7.Size = new System.Drawing.Size(36, 16);
                this.label6.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                this.label6.Location = new System.Drawing.Point(335, 46);
                this.label6.Size = new System.Drawing.Size(31, 16);
                this.label10.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                this.label10.Location = new System.Drawing.Point(510, 46);
                this.label10.Size = new System.Drawing.Size(44, 16);
                this.label5.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                this.label5.Location = new System.Drawing.Point(241, 46);
                this.label5.Size = new System.Drawing.Size(31, 16);
                this.label4.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                this.label4.Location = new System.Drawing.Point(298, 46);
                this.label4.Size = new System.Drawing.Size(31, 16);
                this.label3.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                this.label3.Location = new System.Drawing.Point(196, 46);
                this.label3.Size = new System.Drawing.Size(42, 16);
                this.label2.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                this.label2.Location = new System.Drawing.Point(17, 46);
                this.label2.Size = new System.Drawing.Size(43, 16);
                this.label1.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                this.label1.Location = new System.Drawing.Point(108, 46);
                this.label1.Size = new System.Drawing.Size(44, 16);
                this.BtnSave.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
                this.BtnSave.Location = new System.Drawing.Point(405, 561);
                this.BtnSave.Size = new System.Drawing.Size(120, 35);
                this.BtnExit.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
                this.BtnExit.Location = new System.Drawing.Point(667, 561);
                this.BtnExit.Size = new System.Drawing.Size(120, 35);
                this.TitleLabel.Font = new System.Drawing.Font("Tahoma", 20F, System.Drawing.FontStyle.Bold);
                this.TitleLabel.Location = new System.Drawing.Point(290, 4);
                this.TitleLabel.Size = new System.Drawing.Size(185, 33);
                this.BackgroundImage = global::GTI.Modules.POS.Properties.Resources.PaperUsageBackground800;
                this.ClientSize = parent.Settings.DisplayMode.FormSize;
                this.ResumeLayout(false);
                this.PerformLayout();
            }
            
            //initialize and set defaults
            m_parent = parent;
            m_owner = owner;
            IsPaperClosed = false;
            m_bankOpenType = bankOpenType;
            PromptOnStartUp = bankOpenType == BankOpenType.New || bankOpenType == BankOpenType.ReOpen;
            m_selectedTextBox = null;
            m_statusLabelTimer = new System.Timers.Timer(5000) {AutoReset = false};
            m_statusLabelTimer.Elapsed += StatusLabelTimerOnElapsed;
            errIcon.Visible = false;
            StatusLabel.Text = string.Empty;
            StaffLabel.Text = string.Format("{0} {1}", m_parent.CurrentStaff.FirstName, m_parent.CurrentStaff.LastName);
            SessionLabel.Text = m_parent.CurrentSession.ToString();
            BtnSave.Enabled = PromptOnStartUp;
            BtnSave.Text = PromptOnStartUp ? "Confirm" : "Save";
            BtnConfirmAndPrint.Visible = PromptOnStartUp;
            BtnClosePaper.Visible = !PromptOnStartUp;
            //BtnRemove.Visible = bankOpenType == BankOpenType.New;
            BtnExit.Visible = !PromptOnStartUp;
            m_version = version;
            m_listOfItemsToBeRemoved = new List<PaperUsageItem>();
            
        }

        #endregion

        #region Properties
        public bool PromptOnStartUp
        {
            get;
            private set;
        }

        public bool IsPaperClosed
        {
            get;
            private set;
        }
        #endregion

        #region Methods

        /// <summary>
        /// Clears the status label.
        /// </summary>
        private void ClearStatusLabel()
        {
            StatusLabel.Text = string.Empty;
        }

        /// <summary>
        /// Sets the status label.
        /// </summary>
        /// <param name="text">The text.</param>
        private void SetStatusLabel(string text)
        {
            m_statusLabelTimer.Stop();
            m_statusLabelTimer.Start();
            StatusLabel.Text = text;
        }

        /// <summary>
        /// Assigns the value into paper usage receipt.
        /// </summary>
        /// <param name="pur">The pur.</param>
        private void AssignValueIntoPaperUsageReceipt(PaperUsageReceipt pur)
        {                
            //check for presale session
            //if the selected session is a presale, then want to find the active sale session
            var session = m_parent.CurrentSession;
            if (m_parent.CurrentSession.IsPreSale && m_parent.ActiveSalesSession != null)
            {
                session = m_parent.ActiveSalesSession;
            }
            pur.Session = session.SessionNumber;
            pur.OperatorName = m_parent.CurrentOperator.Name;
            pur.StartDateTime = m_parent.GamingDate;
            pur.TodaysDateTime = DateTime.Now;
            pur.MachineId = m_parent.MachineId;
            pur.Version = m_version;
            pur.StaffName = m_parent.CurrentStaff.FirstName + " " + m_parent.CurrentStaff.LastName;
            pur.StaffId = m_parent.CurrentStaff.Id;
            pur.OperatorHeader_Line1 = m_parent.Settings.ReceiptHeaderLine1; //"",Welcome to
            pur.OperatorHeader_Line2 = m_parent.Settings.ReceiptHeaderLine2; //"",South Point
            pur.OperatorHeader_Line3 = m_parent.Settings.ReceiptHeaderLine3; //"",Hotel, Casino, and Spa   
        }

        private void AddPaperUsageItem(PaperUsageItem paperUsageItem, bool isNewItem = false)
        {
            //create control
            var control = new PaperUsageItemControl(m_owner, m_parent, paperUsageItem, isNewItem);

            //attach to events
            control.ItemModified += OnPaperUsageItemModified;
            control.TotalModified += OnPaperUsageTotalModified;
            control.TextBoxFocusChanged += OnPaperUsageItemTextBoxFocusChanged;
            control.UnscannedChangedEvent += OnPaperUsageItemUnscannedChanged;
            control.FocusEvent += OnPaperUsageItemControlFocusEvent;
            //add control
            PaperUsageFlowPanel.Controls.Add(control);

            PaperUsageFlowPanel.ScrollControlIntoView(control);

            control.Focus();

            //populate unscanned list box
            foreach (var unscannedPackNumber in paperUsageItem.UnscannedList)
            {
                UnscannedPacksListBox.Items.Add(string.Format("{0}   {1}   {2}", paperUsageItem.Name,
                    paperUsageItem.Serial, unscannedPackNumber.ToString().PadLeft(5, ' ')));
            }
        }

        #region Server Messages
        /// <summary>
        /// Load this machine's paper usage from the server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GetAuditNumbers(object sender, DoWorkEventArgs e)
        {

            var reOpenFlag = (bool)e.Argument;
            //load machine paper usage
            GetMachinePaperUsageMessage message;

            try
            {
                message = GetMachinePaperUsageMessage.GetMachineAuditNumbers(reOpenFlag);
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
        /// Actions that occur when the "GetAuditNumbers" message is complete
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GetAuditNumbersComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                List<PaperUsageItem> paperUsageItems = (List<PaperUsageItem>) e.Result;

                //populate paper usage items
                UpdatePaperUsageItemsUserControl(paperUsageItems);
            }

            //disable close paper button if no paper items
            if (PaperUsageFlowPanel.Controls.Count == 0)
            {
                BtnClosePaper.Enabled = false;
            }

            BtnPrintUnscannedPacks.Enabled = UnscannedPacksListBox.Items.Count != 0;

            //update total
            OnPaperUsageTotalModified(null, EventArgs.Empty);

            m_parent.CloseWaitForm();
        }
        
        /// <summary>
        /// Saves the and get audit numbers.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DoWorkEventArgs"/> instance containing the event data.</param>
        /// <exception cref="POSException">
        /// </exception>
        private void SaveAndGetAuditNumbers(object sender, DoWorkEventArgs e)
        {
            var args= e.Argument as Tuple<List<PaperUsageItem>, bool>;
            if (args == null)
            {
                throw new POSException(string.Format(CultureInfo.CurrentCulture, Resources.GetPaperUsageFailed, "Invalid Arguments to save paper usage"));
            }
            var listOfModifiedItems = args.Item1;
            var createInventoryTransactions = args.Item2;

            if (listOfModifiedItems.Count == 0)
            {
                return;
            }

            try
            {
                //check for presale session
                //if the selected session is a presale, then want to find the active sale session
                var session = m_parent.CurrentSession;
                if (m_parent.CurrentSession.IsPreSale && m_parent.ActiveSalesSession != null)
                {
                    session = m_parent.ActiveSalesSession;
                }

                SetMachinePaperUsageMessage.SetMachinePaperUsage(session.SessionNumber,
                    m_parent.GamingDate, listOfModifiedItems, createInventoryTransactions);
            }
            catch (Exception ex)
            {
                throw new POSException(string.Format(CultureInfo.CurrentCulture, Resources.FailedSetPaperUsage, ServerExceptionTranslator.FormatExceptionMessage(ex)), ex);
            }

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
        /// Actions that occur when the "GetAuditNumbers" message is complete
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveAndGetAuditNumbersComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            //error
            if (e.Error != null)
            {
                SetStatusLabel("Failed! " + e.Error.Message);

                m_parent.CloseWaitForm();
                return;
            }

            if (e.Result != null)
            {
                List<PaperUsageItem> paperUsageItems = (List<PaperUsageItem>)e.Result;

                UpdatePaperUsageItemsUserControl(paperUsageItems);
            }

            //disable close paper button if no paper items
            if (PaperUsageFlowPanel.Controls.Count == 0)
            {
                BtnClosePaper.Enabled = false;
            }
            
            BtnPrintUnscannedPacks.Enabled = UnscannedPacksListBox.Items.Count != 0;

            //clear removal list
            m_listOfItemsToBeRemoved.Clear();

            //update total
            OnPaperUsageTotalModified(null, EventArgs.Empty);

            //set status
            SetStatusLabel("Successfully saved");
            m_parent.CloseWaitForm();
        }

        /// <summary>
        /// Updates Paper Usage items UI. This will remove, add and updated any items.
        /// This method updates UI controls, rather than clearing and creating new controls, to be more efficient
        /// </summary>
        /// <param name="paperUsageItems"></param>
        private void UpdatePaperUsageItemsUserControl(List<PaperUsageItem> paperUsageItems)
        {
            SuspendLayout();
            UnscannedPacksListBox.Items.Clear();
            paperUsageItems.Sort();

            //check if there are more items in the UI, then we want to remove UI items
            if (paperUsageItems.Count < PaperUsageFlowPanel.Controls.Count)
            {
                //we want to remove extra
                while (paperUsageItems.Count != PaperUsageFlowPanel.Controls.Count)
                {
                    PaperUsageFlowPanel.Controls.RemoveAt(PaperUsageFlowPanel.Controls.Count - 1);
                }
            }
            //check to see if there are more items from the server than in the UI
            else if (paperUsageItems.Count > PaperUsageFlowPanel.Controls.Count)
            {
                //add UI paper usage items
                while (paperUsageItems.Count != PaperUsageFlowPanel.Controls.Count)
                {
                    var paperUsageItem = paperUsageItems[PaperUsageFlowPanel.Controls.Count];
                    //create control
                    var control = new PaperUsageItemControl(m_owner, m_parent, paperUsageItem);

                    //attach to events
                    control.ItemModified += OnPaperUsageItemModified;
                    control.TotalModified += OnPaperUsageTotalModified;
                    control.TextBoxFocusChanged += OnPaperUsageItemTextBoxFocusChanged;
                    control.UnscannedChangedEvent += OnPaperUsageItemUnscannedChanged;
                    control.FocusEvent += OnPaperUsageItemControlFocusEvent;

                    //add control
                    PaperUsageFlowPanel.Controls.Add(control);
                }
            }

            //update paper usage items
            for (int i = 0; i < paperUsageItems.Count; i++)
            {
                var control = PaperUsageFlowPanel.Controls[i] as PaperUsageItemControl;
                if (control != null)
                {
                    control.UpdatePaperUsageItem(paperUsageItems[i]);
                }

                //populate unscanned list box
                foreach (var unscannedPackNumber in paperUsageItems[i].UnscannedList)
                {
                    UnscannedPacksListBox.Items.Add(string.Format("{0}   {1}   {2}", paperUsageItems[i].Name,
                        paperUsageItems[i].Serial, unscannedPackNumber.ToString().PadLeft(5, ' ')));
                }
            }
            ResumeLayout();
        }

        #endregion

        #region UI Event Handlers

        /// <summary>
        /// Handles the Load event of the PaperUsageForm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void PaperUsageForm_Load(object sender, EventArgs e)
        {
            m_parent.RunWorker(Resources.WaitFormGetPaperUsage, GetAuditNumbers, m_bankOpenType == BankOpenType.ReOpen, GetAuditNumbersComplete);
            m_parent.ShowWaitForm(this, true); // window has to be initialized first when sending "this" as a parent. Halts here until background worker is complete
        }

        /// <summary>
        /// Handles the Modified event for Paper Usage Items.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OnPaperUsageItemModified(object sender, EventArgs e)
        {
            var usageItem = sender as PaperUsageItemControl;

            if (usageItem != null)
            {
                ValidatePaperUsageItems(usageItem.Item);
            }
            else
            {
                ValidatePaperUsageItems();
            }
        }

        /// <summary>
        /// Handles the Total Modified event for Paper Usage Items.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OnPaperUsageTotalModified(object sender, EventArgs e)
        {
            //update total
            var total = PaperUsageFlowPanel.Controls.OfType<PaperUsageItemControl>().Sum(control => control.Value);

            TotalLabel.Text = string.Format("{0:C}", total);
        }

        /// <summary>
        /// Handles the Total Modified event for Paper Usage Items.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OnPaperUsageItemTextBoxFocusChanged(object sender, EventArgs e)
        {
            m_selectedTextBox = sender as TextBox;
        }

        /// <summary>
        /// Called when [paper usage item unscanned changed].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="PaperUsageUnscannedChangedEventArgs"/> instance containing the event data.</param>
        private void OnPaperUsageItemUnscannedChanged(object sender, PaperUsageUnscannedChangedEventArgs e)
        {
            if (e.IsAdd)
            {
                if(!UnscannedPacksListBox.Items.Contains(e.ToString()))
                {
                    if(e.PaperUsageItem.AuditStart <= e.Audit && e.PaperUsageItem.AuditEnd >= e.Audit)
                    {
                        UnscannedPacksListBox.Items.Add(e.ToString());
                    }
                }
            }
            else
            {
                if (UnscannedPacksListBox.Items.Contains(e.ToString()))
                {
                    UnscannedPacksListBox.Items.Remove(e.ToString());
                }
            }
        }

        /// <summary>
        /// Denom control on focus event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OnPaperUsageItemControlFocusEvent(object sender, EventArgs eventArgs)
        {
            //update current selected denom control
            var control = sender as PaperUsageItemControl;

            //unfocus previous denom control
            if (m_selectedPaperUsageControl != null && m_selectedPaperUsageControl != control)
            {
                m_selectedPaperUsageControl.UpdateSelectedBackground(false);
            }

            m_selectedPaperUsageControl = control;

            if (m_selectedPaperUsageControl != null && !(m_selectedPaperUsageControl.StartAuditTextBox.Focused ||
                                                        m_selectedPaperUsageControl.EndAuditTextBox.Focused || 
                                                        m_selectedPaperUsageControl.PriceTextbox.Focused))
            {
                m_selectedPaperUsageControl.StartAuditTextBox.Focus();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnClosePaper control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void BtnClosePaperClick(object sender, EventArgs e)
        {
            var results = POSMessageForm.Show(this, m_parent, "Are you sure you want to close?", POSMessageFormTypes.YesNo_regular);

            if (results != DialogResult.Yes)
            {
                return;
            }

            //check for presale session
            //if the selected session is a presale, then want to find the active sale session
            var session = m_parent.CurrentSession;
            if (m_parent.CurrentSession.IsPreSale && m_parent.ActiveSalesSession != null)
            {
                session = m_parent.ActiveSalesSession;
            }

            //save and issue paper
            var listOfModifiedItems = PaperUsageFlowPanel.Controls.OfType<PaperUsageItemControl>().Select(paperUsageControl => paperUsageControl.Item).ToList();
            listOfModifiedItems.AddRange(m_listOfItemsToBeRemoved);
            var args = new Tuple<List<PaperUsageItem>, bool>(listOfModifiedItems, true);
            m_parent.RunWorker(Resources.WaitFormGetPaperUsage, SaveAndGetAuditNumbers, args, SaveAndGetAuditNumbersComplete);
            m_parent.ShowWaitForm(this, true); // halts here until the background worker completes

            //US5200 POS: Do not display bank close UI when in money center mode.
            if (m_parent.CurrentOperator.CashMethodID != (int)CashMethod.ByStaffMoneyCenter)
            {
                var closeBankForm =
                    new CloseDropBankForm(
                        string.Format("{0} {1}", m_parent.CurrentStaff.FirstName, m_parent.CurrentStaff.LastName),
                        session.SessionNumber, m_parent)
                    {
                        StartPosition = FormStartPosition.CenterParent,
                        DisplayOptions = CloseDropBankForm.BankCloseDisplayOptions.AllowBoth,
                    };

                results = closeBankForm.ShowDialog(this);
                if (results != DialogResult.OK)
                {
                    return; // they cancelled, assume they want to re-work paper usage, don't save.
                }
            }

            //DE14193: if we closed bank, then send another set to finalize paper usage close.
            SetMachinePaperUsageMessage.SetMachinePaperUsage(session.SessionNumber,
                m_parent.GamingDate, listOfModifiedItems, false, true); // false = create Inventory Transaction, true = close paper usage
            
            BtnClosePaper.Enabled = false;

            IsPaperClosed = true;
            DialogResult = DialogResult.OK;

            m_statusLabelTimer.Stop();
            Close();
        }

        /// <summary>
        /// BTNs the save click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void BtnSaveClick(object sender, EventArgs e)
        {
            //disable button to prevent multiple saves
            if (!BtnSave.Enabled)
            {
                return;
            }

            //disable save button
            BtnSave.Enabled = false;
            
            if (!m_itemModified)
            {
                //we only want to close if its the first time opening
                if (PromptOnStartUp)
                {
                    m_statusLabelTimer.Stop();
                    Close();
                }
                return;
            }

            var listOfModifiedItems = new List<PaperUsageItem>();

            foreach (var paperUsageControl in PaperUsageFlowPanel.Controls.OfType<PaperUsageItemControl>())
            {
                if (paperUsageControl.IsModified)
                {
                    listOfModifiedItems.Add(paperUsageControl.Item);
                }
            }

            //add removal items to the list
            listOfModifiedItems.AddRange(m_listOfItemsToBeRemoved);
            var args = new Tuple<List<PaperUsageItem>, bool>(listOfModifiedItems, false);

            m_parent.RunWorker(Resources.WaitFormGetPaperUsage, SaveAndGetAuditNumbers, args, SaveAndGetAuditNumbersComplete);
            m_parent.ShowWaitForm(this, true); // halts here until the background worker completes

            ////we only want to close if its the first time opening
            if (PromptOnStartUp)
            {
                m_statusLabelTimer.Stop();
                Close();
            }
        }

        /// <summary>
        /// BTNs the exit click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void BtnExitClick(object sender, EventArgs e)
        {
            m_statusLabelTimer.Stop();
            Close();
        }

        /// <summary>
        /// Handles the Click event of the BtnPrint control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void BtnPrintClick(object sender, EventArgs e)
        {
            var items = PaperUsageFlowPanel.Controls.OfType<PaperUsageItemControl>().Select(control => control.Item).ToList();
            if (PromptOnStartUp == false)
            {
                var receipt = new PaperUsageReceipt(items);                                
                AssignValueIntoPaperUsageReceipt(receipt);
                try
                {
                    receipt.Print(new Printer(m_parent.Settings.ReceiptPrinterName), 1);
                }
                catch (Exception ex)
                {
                    POSMessageForm.Show(this, m_parent, string.Format(Resources.FailedToPrintReceipt, ex.Message));
                }
            }
            else
            {
                var receipt = new PaperUsageStartReceipt(items);
                AssignValueIntoPaperUsageReceipt(receipt);
                try
                {
                    receipt.Print(new Printer(m_parent.Settings.ReceiptPrinterName), 1);
                }
                catch (Exception ex)
                {
                    POSMessageForm.Show(this, m_parent, string.Format(Resources.FailedToPrintReceipt, ex.Message));
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the confirm and print button.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void BtnConfirmAndPrintClick(object sender, EventArgs e)
        {
            BtnPrintClick(sender, e);

            BtnSaveClick(sender, e);
        }

        /// <summary>
        /// Handles the Click event of the imgbtnPrintUnscannedPacks control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void BtnPrintUnscannedPacksClick(object sender, EventArgs e)
        {
            var items = PaperUsageFlowPanel.Controls.OfType<PaperUsageItemControl>().Select(control => control.Item).ToList();
            //Do not print receipt if the unscannedpack list = 0
            //if (items.SkipList.Count(i => i >= AuditStart && i <= AuditEnd))
            var receipt = new PaperUsageUnscannedPacksReceipt(items);
            AssignValueIntoPaperUsageReceipt(receipt);

            //Skip printing if theres 0 unscannedpack
            if (receipt.IsThereUnscannedPack)
            {
                try
                {
                    receipt.Print(new Printer(m_parent.Settings.ReceiptPrinterName), 1);
                }
                catch (Exception ex)
                {
                    POSMessageForm.Show(this, m_parent, string.Format(Resources.FailedToPrintReceipt, ex.Message));
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnRemove control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void BtnRemoveClick(object sender, EventArgs e)
        {
            if (m_selectedPaperUsageControl == null)
            {
                return;
            }

            var results = MessageForm.Show(m_owner, m_displayMode, "Are you sure you want to remove item", MessageFormTypes.YesNo);

            if (results == DialogResult.No)
            {
                return;
            }


            //get paper usage item
            var paperUsageItem = m_selectedPaperUsageControl.Item;

            //set removal flag
            paperUsageItem.IsMarkedForRemoval = true;

            //add to remove list
            m_listOfItemsToBeRemoved.Add(paperUsageItem);

            //remove control from UI
            PaperUsageFlowPanel.Controls.Remove(m_selectedPaperUsageControl);

            ValidatePaperUsageItems();
        }

        /// <summary>
        /// Handles the Click event of the Add button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var addForm = new PaperUsageAddItemForm(m_parent, this)
            {
                StartPosition = FormStartPosition.CenterScreen
            };

            var results = addForm.ShowDialog(this);

            if (results == DialogResult.Cancel)
            {
                return;
            }

            AddPaperUsageItem(addForm.NewItem, true);

            BtnPrintUnscannedPacks.Enabled = UnscannedPacksListBox.Items.Count != 0;

            //update total
            OnPaperUsageTotalModified(null, EventArgs.Empty);

            ValidatePaperUsageItems();
        }

        /// <summary>
        /// Handles the Status Label Timer Elapsed event
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="elapsedEventArgs">The <see cref="ElapsedEventArgs"/> instance containing the event data.</param>
        private void StatusLabelTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            if (InvokeRequired) // Line #1
            {
                Invoke(new MethodInvoker(ClearStatusLabel));
                return;
            }

            ClearStatusLabel();
        }

        private void ValidatePaperUsageItems(PaperUsageItem usageItem = null)
        {
            //stop the timer if it is started
            m_statusLabelTimer.Stop();

            //reset all values
            var isValid = true;
            var status = string.Empty;
            foreach (var control in PaperUsageFlowPanel.Controls.OfType<PaperUsageItemControl>())
            {
                //if item is not valid, then set message and flag
                if (!control.IsValid)
                {
                    isValid = false;
                    status = control.InvalidMessage;
                    break;
                }

                if (usageItem == null)
                {
                    continue;
                }

                //make sure we aren't checking the same item
                if (control.Item == usageItem)
                {
                    continue;
                }

                //see if the serial numbers match
                if (control.Item.Serial != usageItem.Serial)
                {
                    continue;
                }

                if ((control.Item.AuditStart <= usageItem.AuditStart &&
                    control.Item.AuditEnd >= usageItem.AuditStart) ||
                    (control.Item.AuditStart < usageItem.AuditEnd &&
                    control.Item.AuditEnd > usageItem.AuditEnd))
                {
                    isValid = false;
                    status = string.Format("Serial {0} cannot have overlapping audit ranges", control.Item.Serial);
                    break;
                }
            }

            //enable save button
            BtnAdd.Enabled = isValid;
            m_itemModified = isValid;
            BtnSave.Enabled = isValid;
            BtnClosePaper.Enabled = isValid;
            //StatusLabel.ForeColor = isValid ? Color.White : Color.Yellow;
            errIcon.Visible = !isValid;
            StatusLabel.Text = status;
        }

        #endregion 

        #region KeyPad Events Handlers

        private void PressThisKey(object sender, EventArgs e)
        {
            SendKeyPress(((Button)sender).Text);
        }

        private void DecimalButtonClick(object sender, EventArgs e)
        {
            if (!m_selectedTextBox.Text.Contains('.') && m_selectedTextBox.Name == "PriceTextbox")
            {
                SendKeyPress(".");
            }
            else
            {
                if (m_selectedTextBox != null)
                    m_selectedTextBox.Focus();
            }
        }

        private void BackButtonClick(object sender, EventArgs e)
        {
            SendKeyPress("\x08");
        }

        private void SendKeyPress(string number)
        {
            //if nothing is selected, then return
            if (m_selectedTextBox == null)
            {
                return;
            }

            m_selectedTextBox.Focus();

            SendKeys.Send(number);
        }

        #endregion End KeyPad Events Handlers

        #endregion End Methods
    }
}
