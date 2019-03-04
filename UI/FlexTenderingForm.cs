#region Copyright
// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2015 Fortunet , Inc.
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GTI.Modules.Shared;
using GTI.Modules.Shared.Data;
using GTI.Modules.POS.Properties;
using GTI.Modules.POS.Business;
using GTI.Controls;

namespace GTI.Modules.POS.UI
{
    internal partial class FlexTenderingForm : POSForm
    {
        #region Member variables
        private Sale m_currentSale;
        private PointOfSale m_pos;
        private SellingForm m_sellingForm;
        private int m_blankLinesInDisplay;
        private bool m_voidAllTenders = false;
        private decimal m_leftToRefund = 0M;
        private string m_cashTenderID = string.Empty;
        private string m_cardTenderID = string.Empty;
        private bool m_cardTenderAllowed = false;
        private string m_cardButtonText = string.Empty;
        private bool m_allowTheKeypad = true;
        private DateTime m_idleSince = DateTime.Now;
        private int KioskIdleLimit;
        private int ShortKioskIdleLimit;
        private bool m_keepControlsDisabled = false;
        private bool m_weHaveABillAcceptor = false;
        private bool m_useSimpleFormForAdvancedKiosk = false;
        private object m_tenderingLockObject = new object();

        //public static bool BlackScreenOnCreate = true;
        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="FlexTenderingForm"/>
        /// class.
        /// </summary>
        public FlexTenderingForm(PointOfSale pos, Sale currentSale, SellingForm sellingForm, bool voidAllTenders = false):base(pos, pos.Settings.DisplayMode.BasicCopy())
        {
            m_pos = pos;
            m_currentSale = currentSale;
            m_sellingForm = sellingForm;
            m_voidAllTenders = voidAllTenders;
            KioskIdleLimit = m_sellingForm.KioskIdleLimitInSeconds;
            ShortKioskIdleLimit = m_sellingForm.KioskShortIdleLimitInSeconds;

            if (m_parent.Guardian != null)
            {
                m_parent.Guardian.UpdateDeviceStates();
                m_weHaveABillAcceptor = m_pos.Guardian.AcceptorState != GuardianWrapper.DeviceState.NotInstalled && !m_parent.Settings.KioskTestWithoutAcceptor;
            }

            m_useSimpleFormForAdvancedKiosk = m_pos.Settings.UseSimplePaymentForAdvancedKiosk;

            InitializeComponent();

            if (!IsAtLeastWindows7) //XP can not handle transparent forms with double buffering
            {
                this.DrawAsGradient = true;
                this.DrawBorderOuterEdge = true;
            }

            //*********************************************************************************************
            //we only support 1024x768 (normal) or 1366x768 (widescreen) - no support for 800x600 (compact)
            //*********************************************************************************************
            this.Size = m_displayMode.BaseFormSize;

            m_panelMain.Size = m_displayMode.NormalFormSize;
            m_panelMain.Location = new Point(m_displayMode.EdgeAdjustmentForNormalToWideX, m_displayMode.EdgeAdjustmentForNormalToWideY);
            
            m_panelKiosk.Size = m_displayMode.BaseFormSize;
            m_panelKiosk.Location = new Point(0, 0);
            
            MakeTenderingScreenRightHanded();

            m_pos.TenderingScreenActive = true;

            if (m_pos.WeAreAnAdvancedPOSKiosk && !m_useSimpleFormForAdvancedKiosk)
            {
                m_keypad.ClearKeyText = "Clear";
                m_keypad.ClearKeyFont = new Font(m_keypad.NumbersFont.FontFamily, 20, m_keypad.NumbersFont.Style);

                m_btnHelp.Visible = true;
                m_btnContinueSale.Visible = true;
                m_btnCancelTendering.Visible = false;
                m_btnSwapLeftRightHanded.Visible = false;
                m_btnCancelSale.Visible = true;
                m_lblExchangeRate.Visible = false;
                m_lblPlayerName.Visible = false;
                m_lblPlayerPoints.Visible = false;

                if (m_pos.Settings.KioskTestWithoutAcceptor)
                {
                    m_btnAdvTest1.Visible = true;
                    m_btnAdvTest5.Visible = true;
                    m_btnAdvTest10.Visible = true;
                }

                SetDeviceButton();

                if (!m_pos.Settings.AllowSplitTendering)
                {
                    m_panelMain.BackgroundImage = Resources.POSKioskSplitTenderingBackBig2;
                    m_statusTextbox.Size = new Size(353, 495);
                    m_allowTheKeypad = false;
                }

                NotIdle();
                EnableKioskIdleTimer();
            }
            else if (pos.WeAreAPOSKiosk)
            {
                if (m_pos.WeAreAnAdvancedPOSKiosk)
                {
                    m_btnKioskQuit.Text = "Continue Shopping";
                    m_btnKioskQuit.ImageNormal = Resources.DarkOrangeButtonUp;
                    m_btnKioskQuit.ImagePressed = Resources.DarkOrangeButtonDown;
                    m_btnKioskQuit.Font = new Font(m_btnKioskQuit.Font.FontFamily, 26, m_btnKioskQuit.Font.Style);
                }

                if (m_pos.Settings.KioskTestWithoutAcceptor)
                {
                    m_btnTest1.Visible = true;
                    m_btnTest5.Visible = true;
                    m_btnTest10.Visible = true;
                }

                if (m_pos.Settings.KioskTestWithoutAcceptor || m_weHaveABillAcceptor)
                    m_btnKioskCard.Location = new Point((m_panelKiosk.Width - m_btnKioskCard.Width) / 2, m_btnKioskCard.Location.Y + m_displayMode.EdgeAdjustmentForNormalToWideY);
                else
                    m_btnKioskCard.Location = new Point((m_panelKiosk.Width - m_btnKioskCard.Width) / 2, ((m_panelKiosk.Height - m_btnKioskCard.Height) / 2) + m_displayMode.EdgeAdjustmentForNormalToWideY);

                m_lblKioskTotalDue.Location = new Point(m_lblKioskTotalDue.Location.X, m_lblKioskTotalDue.Location.Y + m_displayMode.EdgeAdjustmentForNormalToWideY);
                m_lblKioskInstructions.Location = new Point(m_lblKioskInstructions.Location.X, m_lblKioskInstructions.Location.Y + m_displayMode.EdgeAdjustmentForNormalToWideY);
                m_lblKioskCardInstructions.Location = new Point(m_lblKioskCardInstructions.Location.X, m_lblKioskCardInstructions.Location.Y + m_displayMode.EdgeAdjustmentForNormalToWideY);
                m_picCreditCardDevice.Location = new Point(m_picCreditCardDevice.Location.X + m_displayMode.EdgeAdjustmentForNormalToWideX, m_picCreditCardDevice.Location.Y + m_displayMode.EdgeAdjustmentForNormalToWideY);

                m_panelKiosk.Visible = true;
                NotIdle();
                EnableKioskIdleTimer();

                m_lblKioskInstructions.Text = "Insert bills";

                if(!string.IsNullOrWhiteSpace(m_pos.Settings.TicketPrinterName))
                    m_lblKioskInstructions.Text += " or tickets";

                if (!m_weHaveABillAcceptor && !m_pos.Settings.KioskTestWithoutAcceptor)
                    m_lblKioskInstructions.Visible = false;

                if (!m_pos.Settings.PaymentProcessingEnabled || (!m_pos.Settings.AllowCreditCardTender && !m_pos.Settings.AllowDebitCardTender))
                    m_btnKioskCard.Visible = false;
                else
                    m_lblKioskInstructions.Text += "\r\n\r\nor";

                m_panelKiosk.BringToFront();
            }

            m_keypad.Visible = m_allowTheKeypad;

            decimal pointsValue = 0;

            if (m_currentSale.Player != null && false)//m_pos.Settings.AllowChipTender) //RAK %%% this needs to be AllowPointsTender
            {
                if (m_currentSale.Player.PointsUpToDate || m_sellingForm.UpdatePlayerPoints())
                    pointsValue = m_currentSale.Player.PointsBalance * m_pos.Settings.PointRedemptionValue;
            }

            ListBoxTenderItem.PosSettings = m_pos.Settings;
            ListBoxTenderItem.POS = m_pos;

            if (m_pos.WeAreNotAPOSKiosk && m_pos != null && m_pos.CurrentSale != null && m_pos.CurrentSale.Player != null)
            {
                m_lblPlayerName.Text = "Player:   " + m_pos.CurrentSale.Player.FirstName + (m_pos.CurrentSale.Player.MiddleInitial == "" ? " " : " " + m_pos.CurrentSale.Player.MiddleInitial + (m_pos.CurrentSale.Player.MiddleInitial.Length == 1 ? "." : "") + " ") + m_pos.CurrentSale.Player.LastName;

                if (m_pos.CurrentSale.Player.PointsUpToDate)
                    m_lblPlayerPoints.Text = "Points:  " + SaleItem.FormattedPoints(m_pos.CurrentSale.Player.PointsBalance, false, true);
            }
            else
            {
                m_lblPlayerName.Text = string.Empty;
                m_lblPlayerPoints.Text = string.Empty;
            }

            if (!m_pos.CurrentCurrency.IsDefault)
            {
                decimal amount = 1M;

                while (m_pos.CurrentCurrency.ConvertFromThisCurrencyToDefaultCurrency(amount) == 0)
                    amount *= 10M;

                m_lblExchangeRate.Text = string.Format("{0} = {1}", m_pos.CurrentCurrency.FormatCurrencyString(amount), m_pos.DefaultCurrency.FormatCurrencyString(m_pos.CurrentCurrency.ConvertFromThisCurrencyToDefaultCurrency(amount)));
            }
            else
            {
                m_lblExchangeRate.Text = string.Empty;
            }

            m_btnCurrency.Visible = m_pos.Settings.MultiCurrencies;

            if (m_pos.CurrentStaff != null && m_pos.CurrentStaff.LeftHanded)
                MakeTenderingScreenLeftHanded();

            RefreshScreen();
            UpdateDisplay(true);
            Application.DoEvents();

            if (voidAllTenders)
            {
                DisableSimpleKioskControls();
                m_keepControlsDisabled = true;
                return;
            }

            m_keypad.Use00AsDecimalPoint = !m_pos.Settings.Use00ForCurrencyEntry;
            m_keypad.CurrencySymbol = m_pos.CurrentCurrency.Symbol;
            m_keypad.CurrencySymbolForeColor = System.Drawing.Color.Yellow;
            
            m_btnCurrency.Text = m_pos.CurrentCurrency.ISOCode;

            m_tenderButtonMenu.SetSinglePageFromItemCount(m_pos.Settings.GetNumberOfValidPOSTenders());

            bool possibleToSplitTender = false;

            foreach (TenderTypeValue ttv in m_pos.Settings.ValidPOSTenders)
            {
                Bitmap bitMap = null;
                string buttonText = ttv.TenderName;

                //just allow credit/debit cards and cash if we are a POS Kiosk

                switch ((TenderType)ttv.TenderTypeID)
                {
                    case TenderType.Cash:
                    {
                        if (m_pos.WeAreAPOSKiosk && !m_weHaveABillAcceptor && !m_pos.Settings.KioskTestWithoutAcceptor) //POS Kiosk with no bill acceptor has no CASH button
                        {
                            continue;
                        }
                        else
                        {
                            //if we are a POS Kiosk, we'll hide the CASH button later
                            m_cashTenderID = ttv.TenderName;
                            bitMap = Resources.Cash;
                        }

                        break;
                    }

                    case TenderType.Check:
                    {
                        if (m_pos.WeAreAPOSKiosk) //no checks on a POS Kiosk
                        {
                            continue;
                        }
                        else
                        {
                            bitMap = Resources.Check;
                            possibleToSplitTender = true;
                        }

                        break;
                    }

                    case TenderType.CreditCard:
                    {
                        if (m_pos.WeAreAPOSKiosk && !m_pos.Settings.PaymentProcessingEnabled) //no non-processed cards on POS Kiosk
                        {
                            continue;
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(m_pos.Settings.ValidPOSTenders.Find((x) => x.TenderTypeID == (int)TenderType.DebitCard).TenderName)) //debit card allowed, use "Credit/Debit" for button text
                                buttonText = Resources.CreditOrDebitCard;
                            else
                                m_picCreditCardDevice.Image = Resources.CreditCardUnit2;

                            bitMap = Resources.CreditCard;

                            m_cardTenderAllowed = true;
                            m_cardButtonText = buttonText;
                            m_cardTenderID = ttv.TenderName;
                            possibleToSplitTender = true;
                        }

                        break;
                    }

                    case TenderType.DebitCard:
                    {
                        if (m_pos.WeAreAPOSKiosk && !m_pos.Settings.PaymentProcessingEnabled) //no non-processed cards on POS Kiosk
                        {
                            continue;
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(m_pos.Settings.ValidPOSTenders.Find((x) => x.TenderTypeID == (int)TenderType.CreditCard).TenderName)) //no credit card allowed, add debit button
                            {
                                m_cardTenderAllowed = true;
                                m_cardButtonText = buttonText;
                                m_cardTenderID = ttv.TenderName;
                                bitMap = Resources.DebitCard;
                                possibleToSplitTender = true;
                            }
                            else
                            {
                                continue;
                            }
                        }

                        break;
                    }

                    case TenderType.GiftCard:
                    {
                        if (m_pos.WeAreAPOSKiosk)
                        {
                            continue;
                        }
                        else
                        {
                            bitMap = Resources.GiftCard;
                            possibleToSplitTender = true;
                        }

                        break;
                    }

                    default:
                    {
                        if (m_pos.WeAreAPOSKiosk)
                            continue;
                        else
                            bitMap = Resources.GenericTender;

                        break;
                    }
                }

                m_tenderButtonMenu.AddButton(new Controls.ButtonEntry(ttv.TenderName, buttonText, true, ProcessSaleTenderButton, bitMap));
            }

            //if we are using a bill acceptor, hide the CASH button.
            //Inserting a bill enters the value and presses CASH.
            if (m_cashTenderID != string.Empty)
            {
                if (m_pos.WeAreAPOSKiosk)
                {
                    m_tenderButtonMenu.HideEmptyButtons = true;
                    m_tenderButtonMenu.HideButton(m_cashTenderID, true);
                }
                else
                {
                    m_tenderButtonMenu.HideEmptyButtons = false;
                }
            }

            if (m_parent.WeAreAPOSKiosk)
            {
                foreach (Control ctrl in m_panelMain.Controls)
                {
                    if (ctrl as ImageButton != null)
                        ((ImageButton)ctrl).UseClickSound = m_parent.Settings.UseKeyClickSoundsOnKiosk;
                }

                foreach (Control ctrl in m_panelKiosk.Controls)
                {
                    if (ctrl as ImageButton != null)
                        ((ImageButton)ctrl).UseClickSound = m_parent.Settings.UseKeyClickSoundsOnKiosk;
                }

                m_keypad.UseClickSound = m_parent.Settings.UseKeyClickSoundsOnKiosk;
            }

            if (m_pos.WeAreAnAdvancedPOSKiosk && !possibleToSplitTender)
            {
                m_allowTheKeypad = false;
                m_keypad.Visible = false;
            }

            m_btnKioskCard.Text = "Press here to pay with " + m_cardButtonText;

            if(m_weHaveABillAcceptor)
                m_pos.Guardian.MoneyAccepted += new EventHandler<Guardian.Acceptors.ItemAcceptResultEventArgs>(Guardian_MoneyAccepted);
           
            UpdateDisplay(true);

            if (m_cashTenderID == string.Empty)
                m_weHaveABillAcceptor = false; //might have one, but can't use it if we can't tender cash

            ActivateBillAcceptor();
        }

        private void EnableKioskIdleTimer(bool enableIt = true)
        {
            if (m_voidAllTenders)
                enableIt = false;

            if (enableIt)
            {
                m_timeoutProgress.Minimum = 0;
                m_timeoutProgress.Maximum = m_sellingForm.KioskMessagePauseInMilliseconds;

                m_simpleKioskProgress.Minimum = 0;
                m_simpleKioskProgress.Maximum = m_sellingForm.KioskMessagePauseInMilliseconds;
            }
            else
            {
                m_timeoutProgress.Hide();
                m_simpleKioskProgress.Hide();
            }

            m_kioskIdleTimer.Enabled = enableIt;
        }

        //protected override CreateParams CreateParams
        //{
        //    get
        //    {
        //        CreateParams cp = base.CreateParams;

        //        if(BlackScreenOnCreate)
        //            cp.ExStyle |= 0x02000000;  // Turn on WS_EX_COMPOSITED

        //        return cp;
        //    }
        //}

        //protected override void OnPaintBackground(PaintEventArgs e)
        //{
        //    e.Graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
        //    e.Graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
        //    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;

        //    base.OnPaintBackground(e);
        //}

        #endregion

        #region Form Events

        private void m_kioskCardInstructionsFlashTimer_Tick(object sender, EventArgs e)
        {
            m_lblKioskCardInstructions.ShowEdge = !m_lblKioskCardInstructions.ShowEdge;
        }

        private void m_kioskIdleTimer_Tick(object sender, EventArgs e)
        {
            if (m_pos.GuardianHasUsSuspended) //don't do anything
            {
                NotIdle();
                return;
            }

            TimeSpan idleFor = DateTime.Now - m_idleSince;

            if (idleFor > TimeSpan.FromMilliseconds(KioskIdleLimit * 1000 - m_sellingForm.KioskMessagePauseInMilliseconds))
            {
                if (!m_timeoutProgress.Visible)
                {
                    m_timeoutProgress.Visible = true;
                    m_simpleKioskProgress.Visible = true;
                }

                m_timeoutProgress.Increment(m_kioskIdleTimer.Interval);
                m_simpleKioskProgress.Increment(m_kioskIdleTimer.Interval);

                if (m_timeoutProgress.Value >= m_timeoutProgress.Maximum)
                {
                    if (m_pos.WeAreAnAdvancedPOSKiosk && m_useSimpleFormForAdvancedKiosk)
                    {
                        m_btnCancelSale_Click(sender, e);
                        return;
                    }

                    m_idleSince = DateTime.Now + TimeSpan.FromDays(1);

                    m_timeoutProgress.Visible = false;
                    m_simpleKioskProgress.Visible = false;

                    if (POSMessageForm.ShowCustomTwoButton(this, m_parent, Resources.KioskIdleQuestion, Resources.KioskIdle, true, 2, Resources.Continue, Resources.CancelSale) == 2)
                        m_btnCancelSale_Click(sender, e);
                    else
                        NotIdle();
                }
            }
        }

        /// <summary>
        /// Event fired before a form is displayed for the first time.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FlexTenderingForm_Load(object sender, EventArgs e)
        {
            if (m_voidAllTenders && m_tendersList.Items.Count == 0) //don't show the screen
            {
                EnableKioskIdleTimer(false);

                ActivateBillAcceptor(false);

                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void FlexTenderingForm_Shown(object sender, EventArgs e)
        {
            this.Location = new Point(m_pos.SellingForm.Location.X + m_pos.Settings.DisplayMode.OffsetForFullScreenX, m_pos.SellingForm.Location.Y + m_pos.Settings.DisplayMode.OffsetForFullScreenY);

            if (m_voidAllTenders)
            {
                if (m_pos.WeAreANonAdvancedPOSKiosk)
                    DisableSimpleKioskControls();

                List<ListBoxTenderItem> tList = new List<ListBoxTenderItem>(m_tendersList.Items.Count);
                string reconciliationText = string.Empty;
                decimal reconciliationAmount = 0;
                decimal cashAmountToRefundAtKiosk = 0;
                decimal cashAmountNotRefundedAtKiosk = 0;
                string receiptNameForCash = "Cash";
                SaleTender st;

                m_leftToRefund = 0M;

                for (int x = 0; x < m_tendersList.Items.Count; x++)
                {
                    ListBoxTenderItem ti = m_tendersList.Items[x] as ListBoxTenderItem;

                    if (ti != null)
                    {
                        //if we are a kiosk, we want to dispense cash as a lump sum
                        if (m_pos.WeAreAPOSKiosk && ti.TenderItemObject != null)
                        {
                            if (!ti.TenderItemObject.Voided && !ti.IsRefunded)
                            {
                                if (ti.TenderItemObject.Type == TenderType.Cash)
                                {
                                    if (ti.TenderItemObject.SaleTenderInfo != null && !string.IsNullOrEmpty(ti.TenderItemObject.SaleTenderInfo.ReceiptDescription))
                                        receiptNameForCash = ti.TenderItemObject.SaleTenderInfo.ReceiptDescription;

                                    cashAmountToRefundAtKiosk += ti.TenderItemObject.Amount;
                                }

                                m_leftToRefund += ti.TenderItemObject.Amount;
                            }
                        }

                        tList.Add(m_tendersList.Items[x] as ListBoxTenderItem);
                    }
                }

                m_sellingForm.WriteNVRAMUserDecimal(SellingForm.NVRAMUserDecimal.CashToDispense, cashAmountToRefundAtKiosk);

                foreach (ListBoxTenderItem t in tList)
                {
                    if (t != null && t.TenderItemObject != null && !t.IsRefunded && t.Amount > 0)
                    {
                        switch (t.Type)
                        {
                            case TenderType.Cash:
                            case TenderType.Check:
                            case TenderType.CreditCard:
                            case TenderType.DebitCard:
                            case TenderType.GiftCard:
                            {
                                m_tendersList.SelectedItem = t;
                                st = ProcessRefundTender();

                                if (st != null && t.Type != TenderType.Cash)
                                    m_leftToRefund -= Math.Abs(st.DefaultAmount);

                                if (st != null && (!t.IsRefunded || t.PartialRefund)) //error or partial refund, generate text for reconsiliation slip
                                {
                                    if (t.PartialRefund)
                                    {
                                        reconciliationText += (t.TenderItemObject.SaleTenderInfo.ReceiptDescription + ":").PadRight(20) + (Math.Abs(t.TenderItemObject.SaleTenderInfo.DefaultAmount) - Math.Abs(st.DefaultAmount)).ToString("C").PadLeft(14) + "\r\n";
                                        reconciliationAmount += Math.Abs(t.TenderItemObject.SaleTenderInfo.DefaultAmount) - Math.Abs(st.DefaultAmount);
                                    }
                                    else //error
                                    {
                                        reconciliationText += (t.TenderItemObject.SaleTenderInfo.ReceiptDescription + ":").PadRight(20) + Math.Abs(t.TenderItemObject.SaleTenderInfo.DefaultAmount).ToString("C").PadLeft(14) + "\r\n";
                                        reconciliationAmount += Math.Abs(t.TenderItemObject.SaleTenderInfo.DefaultAmount);
                                    }
                                }
                            }
                            break;

                            default:
                            {
                                throw new Exception("Unknown tender to void.");
                            }
                        }
                    }
                }

                if (cashAmountToRefundAtKiosk != 0) //already refunded, just need to dispense it
                {
                    NotIdle();
                    EnableKioskIdleTimer(false);
                    
                    cashAmountNotRefundedAtKiosk = m_sellingForm.VendChange(cashAmountToRefundAtKiosk);

                    m_sellingForm.WriteNVRAMUserDecimal(SellingForm.NVRAMUserDecimal.CashDispensed, cashAmountToRefundAtKiosk - cashAmountNotRefundedAtKiosk);

                    m_leftToRefund -= cashAmountToRefundAtKiosk - cashAmountNotRefundedAtKiosk;

                    if (m_voidAllTenders && (m_parent.WeAreANonAdvancedPOSKiosk || (m_parent.WeAreAnAdvancedPOSKiosk && m_parent.Settings.UseSimplePaymentForAdvancedKiosk)))
                        m_dueAmountLabel_TextChanged(null, new EventArgs());

                    if (cashAmountNotRefundedAtKiosk != 0) //not everything vended
                    {
                        reconciliationText += (receiptNameForCash + ":").PadRight(20) + cashAmountNotRefundedAtKiosk.ToString("C").PadLeft(14) + "\r\n";
                        reconciliationAmount += cashAmountNotRefundedAtKiosk;
                    }
                }

                if (reconciliationText != string.Empty) //add the reconciliation text into a dummy tender
                {
                    SaleTender dummyTender = new SaleTender();

                    dummyTender.RegisterReceiptID = m_pos.CurrentSale.Id;
                    dummyTender.TransactionTypeID = TransactionType.Void;
                    dummyTender.ReceiptDescription = "Reconciliation text";

                    reconciliationText = "\r\n\r\n**************************************\r\n"
                                         + Resources.AdditionalPayment + "\r\n\r\n"
                                         + reconciliationText
                                         + "--------------".PadLeft(34) + "\r\n"
                                         + reconciliationAmount.ToString("C").PadLeft(34)
                                         + "\r\n**************************************\r\n";

                    dummyTender.AdditionalCustomerText = reconciliationText;
                    dummyTender.AdditionalMerchantText = reconciliationText;


                    //write the dummy tender record
                    SetReceiptTenderMessage setTender = new SetReceiptTenderMessage(dummyTender);

                    try
                    {
                        setTender.Send();
                    }
                    catch (Exception)
                    {
                    }

                    m_pos.CurrentSale.AddTender(new TenderItem(m_pos.CurrentSale, 0, m_pos.CurrentCurrency, 0, dummyTender));

                    m_sellingForm.IncNVRAMUserDecimal(SellingForm.NVRAMUserDecimal.CashDispensed, cashAmountNotRefundedAtKiosk);

                    ShowMessage(m_pos.WeAreAPOSKiosk? Resources.KioskWarningAdditionalPayment : Resources.WarningAdditionalPayment, Resources.AdditionalPayment);
                }

                EnableKioskIdleTimer(false);

                ActivateBillAcceptor(false);

                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void FlexTenderingForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_kioskIdleTimer.Stop();

            if (m_weHaveABillAcceptor)
            {
                ActivateBillAcceptor(false);
                m_pos.Guardian.MoneyAccepted -= Guardian_MoneyAccepted;
            }

            m_pos.TenderingScreenActive = false;
        }
        #endregion

        #region Private Methods

        private bool ActivateBillAcceptor(bool activate = true)
        {
            bool result = !activate;

            if (m_weHaveABillAcceptor)
            {
                if (activate)
                    result = m_pos.Guardian.ActivateBillAcceptor(true, m_currentSale.CalculateTotal(false) - m_currentSale.CalculateAmountTendered());
                else
                    result = m_pos.Guardian.ActivateBillAcceptor(false);
            }

            return result;
        }

        private void RemoveASaleLine(int line)
        {
            m_sellingForm.SelectAndRemoveLine(line);

            if (m_pos.IsSaleEmpty)
            {
                m_btnCancelTendering_Click(null, new EventArgs());
                return;
            }

            UpdateScreenAfterMajorChange();
        }

        private void UpdateScreenAfterMajorChange()
        {
            int topIndex = m_tendersList.TopIndex;
            int index = m_tendersList.SelectedIndex;
            int adj = m_blankLinesInDisplay;
            RefreshScreen();
            adj = m_blankLinesInDisplay - adj;
            m_tendersList.SelectedIndex = index + adj;
            m_tendersList.TopIndex = topIndex + adj;

            UpdateDisplay();
        }

        private void ScollDevice(object sender, EventArgs e)
        {
            NotIdle();

            m_sellingForm.UnitSelectionButton(null, new EventArgs());

            SetDeviceButton();

            int topIndex = m_tendersList.TopIndex;
            int index = m_tendersList.SelectedIndex;
            int adj = m_blankLinesInDisplay;
            RefreshScreen();
            adj = m_blankLinesInDisplay - adj;
            m_tendersList.SelectedIndex = index + adj;
            m_tendersList.TopIndex = topIndex + adj;

            UpdateDisplay();
        }

        private bool RefreshScreen()
        {
            m_tendersList.BeginUpdate();

            //if voiding all tenders, we only need the tenders in the list
            if (!m_voidAllTenders)
            {
                //Add the sale items to the receipt
                string[] saleItemsList = m_sellingForm.GetSaleList(true); //get the text lines from the main receipt marking non-taxed coupons with a 0x00 as the first character

                //remove any additional information text
                for (int x = 0; x < saleItemsList.Count(); x++)
                {
                    if (saleItemsList[x] == "  ")
                    {
                        Array.Resize<string>(ref saleItemsList, x);
                        break;
                    }
                }

                //we want the receipt to scroll up from the bottom like it is coming out of a printer
                //we will add blank lines to the top of the listbox for this effect
                int maxVisibleLinesInListbox = 20; //(int)(Math.Floor((decimal)m_tendersList.ClientRectangle.Height / (decimal)m_tendersList.Font.Height));
                decimal prepaid = m_pos.CurrentSale.CalculatePrepaidAmount() + m_pos.CurrentSale.CalculatePrepaidTaxTotal();
                int totalAndHeaderLines = 6 - (m_pos.Settings.CouponTaxable ? 1 : 0) - (prepaid == 0M ? 1 : 0);
                int saleLines = 0;

                if (saleItemsList != null)
                {
                    saleLines = saleItemsList.Length;

                    foreach (string s in saleItemsList)
                    {
                        string[] test = s.Split(new char[] { '\n' });

                        saleLines += test.Length - 1;
                    }
                }

                m_blankLinesInDisplay = maxVisibleLinesInListbox - totalAndHeaderLines - saleLines;

                if (m_blankLinesInDisplay < 0)
                    m_blankLinesInDisplay = 0;

                m_tendersList.Items.Clear();

                for (int x = 0; x < m_blankLinesInDisplay; x++)
                    this.m_tendersList.Items.Add(new ListBoxTenderItem());

                this.m_tendersList.TopIndexForScroll = m_blankLinesInDisplay;

                //add the column descriptions
                if (m_pos.Settings.LongPOSDescriptions)
                    this.m_tendersList.Items.Add(new ListBoxTenderItem("Sess Qty Item                 Subtotal", 0));
                else
                    this.m_tendersList.Items.Add(new ListBoxTenderItem("Sess Qty Item        Pts      Subtotal", 0));

                //add the detail lines from the main receipt
                if (saleItemsList != null)
                {
                    int receiptLine = 0;

                    foreach (string s in saleItemsList)
                    {
                        bool nonTaxedCoupon = false;
                        string tmp;

                        if (s[0] == '\x00') //this is a non-taxed coupon, need to mark it so we can draw it in orange
                        {
                            nonTaxedCoupon = true;
                            tmp = s.Substring(1);
                        }
                        else
                        {
                            tmp = s;
                        }

                        this.m_tendersList.Items.Add(new ListBoxTenderItem(tmp, 0, false, (tmp[0] != ' ' || tmp == "  " ? -1 : receiptLine), nonTaxedCoupon));

                        receiptLine++;
                    }
                }

                //Add the totals to the receipt
                decimal feesAndTaxesAmount = m_currentSale.CalculateFees() + m_currentSale.CalculateTaxes();

                this.m_tendersList.Items.Add(new ListBoxTenderItem("Subtotal", m_currentSale.CalculateSubtotal(), true));// m_currentSale.CalculateTotal(false) - feesAndTaxesAmount, true));
                this.m_tendersList.Items.Add(new ListBoxTenderItem("Taxes/Fees" + (m_pos.CurrentSale.Device.Id != 0 ? " (" + m_pos.CurrentSale.Device.Name + ")" : ""), feesAndTaxesAmount, true));

                if (!m_pos.Settings.CouponTaxable)
                    this.m_tendersList.Items.Add(new ListBoxTenderItem("Coupons", m_currentSale.CalculateNonTaxableCouponTotal(), true));

                if(prepaid != 0M)
                    this.m_tendersList.Items.Add(new ListBoxTenderItem("Prepaid", -prepaid, true));

                this.m_tendersList.Items.Add(new ListBoxTenderItem("Total", m_currentSale.CalculateTotal(false), true));
            }

            //Add the tenders to the receipt
            //figure out how much has been tendered and how much of that is in cash
            decimal cashTendered = 0M;
            decimal totalTendered = 0M;

            if (m_currentSale.GetCurrentTenders().Count > 0)
            {
                List<TenderItem> prevTenders = m_currentSale.GetCurrentTenders();

                foreach (var tender in prevTenders)
                {
                    if (tender.SaleTenderInfo != null && (tender.SaleTenderInfo.ReceiptDescription == "Error" || tender.SaleTenderInfo.ReceiptDescription == "Reconciliation text"))
                        continue;

                    if (!tender.Voided)
                    {
                        totalTendered += tender.Amount;

                        if (tender.Type == TenderType.Cash)
                            cashTendered += tender.Amount;
                    }

                    ListBoxTenderItem item = new ListBoxTenderItem(tender.Type, tender.Amount);
                    item.TenderItemObject = tender;
                    this.m_tendersList.Items.Add(item);
                }
            }

            m_tendersList.EndUpdate();

            decimal overTendered = totalTendered - m_currentSale.CalculateTotal(false);

            return overTendered >= 0 && cashTendered >= overTendered; //we have paid enough already and can give any needed change in cash
        }

        /// <summary>
        /// Updates the current display, setting items enabled/disabled as 
        /// appropriate
        /// </summary>
        /// <param name="initializeKeypad"></param>
        private void UpdateDisplay(bool scrollToBottomOfList = false)
        {
            NotIdle();

            if (scrollToBottomOfList)
                m_tendersList.TopIndex = m_tendersList.Items.Count - 1; //scroll to the bottom of the list

            m_tendersList_SelectedIndexChanged(null, new EventArgs()); //force update of usable buttons

            decimal defaultCurrencySaleTotal = m_currentSale.CalculateTotal(false);
            decimal defaultCurrencyTenderAmountRemaining = (defaultCurrencySaleTotal - m_currentSale.CalculateAmountTendered()); //in default currency
            decimal defaultCurrencyCashTendered = m_currentSale.CalculateAmountTendered(true); //in default currency
            decimal currentCurrencyTenderAmountRemaining = m_pos.CurrentCurrency.ConvertFromDefaultCurrencyToThisCurrency(defaultCurrencyTenderAmountRemaining); //in current currency

            // Set the initial amount
            if (defaultCurrencyTenderAmountRemaining > 0 && defaultCurrencyTenderAmountRemaining < m_pos.DefaultCurrency.SmallestAmountForThisCurrency)
                defaultCurrencyTenderAmountRemaining = 0;

            m_keypad.InitialValue = currentCurrencyTenderAmountRemaining;

            m_dueAmountLabel.Text = m_pos.CurrentCurrency.FormatCurrencyString(currentCurrencyTenderAmountRemaining);

            bool overTendered = defaultCurrencyTenderAmountRemaining < 0;
            bool canGiveChange = m_pos.WeAreNotAPOSKiosk && defaultCurrencyCashTendered >= -defaultCurrencyTenderAmountRemaining;
            bool givingMoney = overTendered && !canGiveChange && (defaultCurrencyCashTendered + m_pos.CurrentSale.CalculateTotalDollarsToRedeem() >= defaultCurrencyTenderAmountRemaining) && (m_pos.Settings.Tender == Data.TenderSalesMode.AllowNegative || m_pos.Settings.Tender == Data.TenderSalesMode.WarnNegative);

            if (givingMoney)
                m_dueAmountLabel.ForeColor = System.Drawing.Color.Gold;
            else if (overTendered) //over tendered with out enough cash tendered or redeemed dollars to handle change
                m_dueAmountLabel.ForeColor = System.Drawing.Color.Red;
            else
                m_dueAmountLabel.ForeColor = System.Drawing.Color.Lime;

            if (m_statusTextbox.Font.SizeInPoints != 14)
                m_statusTextbox.Font = new Font("Tahoma", 14.25f, FontStyle.Bold);

            if (defaultCurrencySaleTotal < 0M && m_pos.Settings.MinimumSaleAllowed == MinimumSaleAllowed.ZeroOrGreater)
            {
                m_statusTextbox.Text = Resources.SaleZeroOrGreater;

                m_statusTextbox.ForeColor = System.Drawing.Color.Red;

                m_keypad.Visible = false;
                m_tenderButtonMenu.HideMenu(true);
                m_btnFinishTender.Hide();

                return;
            }

            if (defaultCurrencySaleTotal <= 0M && m_pos.Settings.MinimumSaleAllowed == MinimumSaleAllowed.GreaterThanZero)
            {
                m_statusTextbox.Text = Resources.SaleGreaterThanZero;

                m_statusTextbox.ForeColor = System.Drawing.Color.Red;

                m_keypad.Visible = false;
                m_tenderButtonMenu.HideMenu(true);
                m_btnFinishTender.Hide();

                return;
            }

            // Update text display (if setting available money, waiting for funds choice)
            if (!overTendered)
            {
                if (defaultCurrencyTenderAmountRemaining == 0)
                {
                    if (m_pos.WeAreAPOSKiosk)
                    {
                        m_statusTextbox.Text = "Paid in full.  Press the COMPLETE SALE button to finish the transaction and print a receipt.";
                    }
                    else
                    {
                        m_statusTextbox.Text = "Paid in full.";
                    }

                    m_statusTextbox.ForeColor = System.Drawing.Color.LightGreen;

                    m_keypad.Visible = false;
                    m_tenderButtonMenu.HideMenu(true);
                    m_btnFinishTender.Show();
                }
                else
                {
                    if (m_pos.WeAreAPOSKiosk)
                    {
                        if (m_pos.Settings.AllowSplitTendering)
                            m_statusTextbox.Font = new Font("Tahoma", 8f, FontStyle.Bold);

                        m_statusTextbox.Text = m_pos.CurrentCurrency.FormatCurrencyString(currentCurrencyTenderAmountRemaining) + " payment needed.\r\n\r\n";

                        if (m_pos.Settings.PaymentProcessingEnabled && m_cardTenderAllowed) //credit/debit card payment instructions
                        {
                            m_statusTextbox.Text += "To pay with a card:\r\n   Press the " + m_cardButtonText + " button and follow the instructions on the card payment device";

                            if (m_pos.Settings.AllowSplitTendering)
                                m_statusTextbox.Text += " or enter the amount to pay on the number pad below and press the " + m_cardButtonText + " button and follow the instructions on the card payment device.\r\n\r\n";
                            else
                                m_statusTextbox.Text += ".\r\n\r\n";
                        }

                        if (m_weHaveABillAcceptor || m_pos.Settings.KioskTestWithoutAcceptor) //cash/ticket payment instructions
                        {
                            m_statusTextbox.Text += "To pay with cash:\r\n   Insert bills into the acceptor slot.\r\n\r\n";
                            
                            if(!string.IsNullOrWhiteSpace(m_pos.Settings.TicketPrinterName))
                                m_statusTextbox.Text += "To pay with a ticket:\r\n   Insert the ticket into the acceptor slot.";
                        }

                        m_statusTextbox.ForeColor = System.Drawing.Color.AntiqueWhite;
                    }
                    else
                    {
                        if (m_pos.CurrentSale.GetCurrentTenders().Count == 0)
                            m_statusTextbox.Text = "Payment of " + m_pos.CurrentCurrency.FormatCurrencyString(currentCurrencyTenderAmountRemaining) + " required.";
                        else
                            m_statusTextbox.Text = "Additional Payment of " + m_pos.CurrentCurrency.FormatCurrencyString(currentCurrencyTenderAmountRemaining) + " required.";

                        m_statusTextbox.ForeColor = System.Drawing.Color.White;
                    }

                    m_keypad.Visible = m_allowTheKeypad;
                    m_tenderButtonMenu.HideMenu(false);
                    m_btnFinishTender.Hide();
                }
            }
            else
            {
                if (canGiveChange)
                {
                    m_statusTextbox.Text = "Overpayment of " + m_pos.CurrentCurrency.FormatCurrencyString(-currentCurrencyTenderAmountRemaining) + "!";
                    m_statusTextbox.ForeColor = System.Drawing.Color.Yellow;

                    m_keypad.Visible = false;
                    m_tenderButtonMenu.HideMenu(true);
                    m_btnFinishTender.Show();
                }
                else
                {
                    if (givingMoney && m_cashTenderID != string.Empty)
                    {
                        if (m_pos.WeAreAPOSKiosk)
                            m_statusTextbox.Text = "Paying " + m_pos.CurrentCurrency.FormatCurrencyString(-currentCurrencyTenderAmountRemaining) + ".";
                        else
                            m_statusTextbox.Text = "Paying customer " + m_pos.CurrentCurrency.FormatCurrencyString(-currentCurrencyTenderAmountRemaining) + "!\r\n\r\nMoney can only be paid out in cash.";

                        m_statusTextbox.ForeColor = System.Drawing.Color.Gold;

                        m_tenderButtonMenu.HideMenu(true);
                        m_tenderButtonMenu.HideButton(m_cashTenderID, false);
                        m_keypad.Visible = false;
                        m_btnFinishTender.Hide();
                    }
                    else
                    {
                        if (m_pos.WeAreAPOSKiosk)
                            m_statusTextbox.Text = "Overpayment of " + m_pos.CurrentCurrency.FormatCurrencyString(-currentCurrencyTenderAmountRemaining) + "!\r\n\r\nPlease void one or more of the previous payments to make the total due positive or purchase additional items.";
                        else
                            m_statusTextbox.Text = "Overpayment of " + m_pos.CurrentCurrency.FormatCurrencyString(-currentCurrencyTenderAmountRemaining) + "!\r\n\r\nThe money can not be refunded from the current tenders.  Please void one or more of the previous tenders to make the total due positive and then continue tendering or have the customer purchase additional items.";

                        m_statusTextbox.ForeColor = System.Drawing.Color.Red;

                        m_keypad.Visible = false;
                        m_tenderButtonMenu.HideMenu(true);
                        m_btnFinishTender.Hide();
                    }
                }
            }
        }

        /// <summary>
        /// Finishes the tendering process
        /// </summary>
        private void FinishTendering()
        {
            EnableKioskIdleTimer(false);

            if (m_sellingForm.KioskForm != null)
            {
                m_sellingForm.KioskForm.StartIdleState();
                Application.DoEvents();
            }

            ActivateBillAcceptor(false);

            if ((m_currentSale.CalculateTotal(false) - m_currentSale.CalculateAmountTendered()) > 0) //not paid in full
                DialogResult = DialogResult.Cancel;
            else
                DialogResult = DialogResult.OK;

            Close();
        }

        private SaleTender ProcessTender(TenderType tenderType, decimal currencyAmount, Currency usedCurrency, ListBoxTenderItem tenderItem = null)
        {
            lock (m_tenderingLockObject)
            {
                if (m_pos.WeAreAPOSKiosk)
                    EnableKioskIdleTimer(false);

                m_pos.ProcessingTender = true;

                bool removeTenderEnabled = m_buttonRemoveLine.Enabled;
                bool cancelSaleEnabled = m_btnCancelSale.Enabled;
                bool cancelTenderingEnabled = m_btnCancelTendering.Enabled;
                bool continueSaleEnabled = m_btnContinueSale.Enabled;
                bool currencyEnabled = m_btnCurrency.Enabled;
                bool deviceEnabled = m_btnDevice.Enabled;
                bool finishTenderEnabled = m_btnFinishTender.Enabled;
                bool swapLeftRightHandedEnabled = m_btnSwapLeftRightHanded.Enabled;
                bool scrollDownEnabled = m_buttonScrollDown.Enabled;
                bool scrollUpEnabled = m_buttonScrollUp.Enabled;
                bool keypadEnabled = m_keypad.Enabled;
                bool tenderButtonMenuEnabled = m_tenderButtonMenu.Enabled;

                m_btnCancelSale.Enabled = false;
                m_btnCancelTendering.Enabled = false;
                m_btnContinueSale.Enabled = false;
                m_btnCurrency.Enabled = false;
                m_btnDevice.Enabled = false;
                m_btnFinishTender.Enabled = false;
                m_btnSwapLeftRightHanded.Enabled = false;
                m_buttonRemoveLine.Enabled = false;
                m_buttonScrollDown.Enabled = false;
                m_buttonScrollUp.Enabled = false;
                m_keypad.Enabled = false;
                m_tenderButtonMenu.Enabled = false;

                DisableSimpleKioskControls();

                bool voiding = tenderItem != null;
                decimal tax = m_pos.CurrentSale.CalculateTaxes();

                //if this is the first tender, get the registerReceiptID and transaction number for our sale
                if (m_pos.CurrentSale.Id == 0)
                {
                    SaleTender startSale = new SaleTender();
                    startSale.TenderTypeID = 0;

                    m_pos.StartAddTenderToTable(startSale);
                    m_pos.ShowWaitForm(this);

                    if (m_pos.LastAsyncException != null)
                    {
                        if (m_pos.WeAreAPOSKiosk) //shutdown the kiosk and let crash recovery handle this
                        {
                            POSMessageForm.Show(this, m_pos, "A system error has occurred, restarting kiosk...", POSMessageFormTypes.Pause, 5000);
                            m_pos.ClosePOS(this, new EventArgs());
                            Close();
                        }
                        else
                        {
                            POSMessageForm.Show(this, m_pos, "Transaction could not be started.\r\n\r\n" + m_pos.LastAsyncException.Message);
                        }

                        return null;
                    }

                    m_pos.CurrentSale.Id = startSale.RegisterReceiptID;

                    if (m_pos.WeAreAPOSKiosk) //save the transaction number in NVRAM in case of power failure
                    {
                        m_sellingForm.WriteNVRAMUserDecimal(SellingForm.NVRAMUserDecimal.RegisterReceiptID, m_pos.CurrentSale.Id);
                        m_sellingForm.WriteNVRAMUserDecimal(SellingForm.NVRAMUserDecimal.TransactionTotal, m_pos.CurrentSale.CalculateTotal(false));
                    }
                }

                if (voiding)
                {
                    tenderType = tenderItem.Type;
                    currencyAmount = tenderItem.TenderItemObject.TenderedCurrencyAmount;
                    usedCurrency = tenderItem.TenderItemObject.TenderedCurrency;

                    tenderItem.Tax = tax;
                    m_pos.StartVoidSaleTender(tenderItem);
                }
                else
                {
                    m_pos.StartAddSaleTender(tenderType, currencyAmount, usedCurrency, tax);
                }

                //            decimal requestedAmount = (voiding ? -1M : 1M) * Math.Abs(usedCurrency.ConvertFromThisCurrencyToDefaultCurrency(currencyAmount));
                decimal requestedAmount = (voiding ? -1M : 1M) * usedCurrency.ConvertFromThisCurrencyToDefaultCurrency(currencyAmount);

                m_pos.ShowWaitForm(this);

                SaleTender st = null;

                if (!m_pos.m_lastTenderProcessingReply.IsError) //it worked, add this tender to the receipt
                {
                    decimal amount = m_pos.m_lastTenderItem.Amount;
                    decimal usedCurrencyAmount = m_pos.m_lastTenderItem.TenderedCurrencyAmount;
                    TenderType actualTenderType = (voiding ? tenderType : m_pos.m_lastTenderItem.Type);

                    if (m_pos.WeAreAPOSKiosk) //keep track of money taken
                    {
                        if (actualTenderType != TenderType.Cash) //cash is collected in bill acceptor event and dispensed in dispensing routine
                        {
                            if (!voiding)
                                m_sellingForm.IncNVRAMUserDecimal(SellingForm.NVRAMUserDecimal.AmountCollected, amount);
                            else //voiding
                                m_sellingForm.IncNVRAMUserDecimal(SellingForm.NVRAMUserDecimal.AmountDispensed, Math.Abs(amount));
                        }
                    }

                    if (voiding && amount != 0)
                    {
                        tenderItem.IsRefunded = true;

                        if (Math.Abs(amount) != Math.Abs(requestedAmount))
                            tenderItem.PartialRefund = true;
                    }

                    if (actualTenderType == TenderType.CreditCard && (m_pos.m_lastTenderProcessingReply.CardType.ToUpper() == "DB" || m_pos.m_lastTenderProcessingReply.CardType.ToUpper() == "DEBIT")) //debit card selected
                    {
                        actualTenderType = TenderType.DebitCard;
                    }
                    else
                    {
                        if (actualTenderType == TenderType.DebitCard && m_pos.m_lastTenderProcessingReply.CardType.ToUpper() != "DB" && m_pos.m_lastTenderProcessingReply.CardType.ToUpper() != "DEBIT") //credit card refunded
                            actualTenderType = TenderType.CreditCard;
                    }

                    string receiptText;

                    if (m_pos.Settings.PaymentProcessingEnabled && (actualTenderType == TenderType.CreditCard || actualTenderType == TenderType.DebitCard || actualTenderType == TenderType.GiftCard))
                    {
                        //try to translate the card name into a user friendly name using the sub-type display name
                        string subTypeName;

                        if (!m_pos.SubTenderName.TryGetValue(m_pos.m_lastTenderProcessingReply.CardSubType, out subTypeName))
                        {
                            //we don't have a display name for this sub-type
                            if (actualTenderType == TenderType.DebitCard) //it is a debit card, show the tender type name for debit
                                subTypeName = m_pos.Settings.GetTenderName(actualTenderType);
                            else //don't know what it is, show what was returned
                                subTypeName = m_pos.m_lastTenderProcessingReply.CardType;
                        }

                        receiptText = subTypeName + " " + m_pos.m_lastTenderProcessingReply.DisplayCardNumber;
                    }
                    else //show the tender type name
                    {
                        receiptText = m_pos.Settings.GetTenderName(actualTenderType);
                    }

                    ListBoxTenderItem newTenderItem = new ListBoxTenderItem(actualTenderType, amount);

                    newTenderItem.TenderItemObject = m_pos.m_lastTenderItem;

                    if (voiding)
                        st = new SaleTender(0, m_pos.CurrentSale.Id, DateTime.Now, actualTenderType, m_pos.m_lastTenderProcessingReply.CardSubType, TransactionType.Void, usedCurrency.ISOCode, usedCurrencyAmount, amount, tax, m_pos.m_lastTenderProcessingReply.RefCode, m_pos.m_lastTenderProcessingReply.AuthCode, receiptText, tenderItem.TenderItemObject.SaleTenderInfo.RegisterReceiptTenderID, usedCurrency.ExchangeRate, m_pos.m_lastTenderProcessingReply.CustomerText, m_pos.m_lastTenderProcessingReply.MerchantText, string.Empty);
                    else
                        st = new SaleTender(0, m_pos.CurrentSale.Id, DateTime.Now, actualTenderType, m_pos.m_lastTenderProcessingReply.CardSubType, TransactionType.Sale, usedCurrency.ISOCode, usedCurrencyAmount, amount, tax, m_pos.m_lastTenderProcessingReply.RefCode, m_pos.m_lastTenderProcessingReply.AuthCode, receiptText, 0, usedCurrency.ExchangeRate, m_pos.m_lastTenderProcessingReply.CustomerText, m_pos.m_lastTenderProcessingReply.MerchantText, string.Empty);

                    //save the tender
                    m_pos.StartAddTenderToTable(st);
                    m_pos.ShowWaitForm(this);

                    if (m_pos.LastAsyncException != null)
                    {
                        if (m_pos.WeAreAPOSKiosk) //refund money
                        {
                            POSMessageForm.Show(this, m_pos, "System error, restarting...", POSMessageFormTypes.Pause, 5000);
                            m_pos.ClosePOS(this, new EventArgs());
                            Close();
                            return null;
                        }
                        else
                        {
                            POSMessageForm.Show(this, m_pos, "Tender record could not be written.\r\n\r\n" + m_pos.LastAsyncException.Message);
                        }
                    }

                    if (!voiding)
                        m_pos.CurrentSale.Id = st.RegisterReceiptID;
                    else
                        newTenderItem.IsRefunded = true;

                    newTenderItem.TenderItemObject.SaleTenderInfo = st;
                    m_tendersList.Items.Add(newTenderItem);
                    UpdateDisplay(true);

                    if (amount != requestedAmount)
                        ShowMessage(Resources.PartialTenderApproved);
                }
                else //error
                {
                    ListBoxTenderItem newTenderItem = new ListBoxTenderItem(0, 0);

                    newTenderItem.TenderItemObject = m_pos.m_lastTenderItem;
                    st = m_pos.m_lastTenderItem.SaleTenderInfo;
                    st.DefaultAmount = m_pos.m_lastTenderItem.ProcessingInfo.AmountRequested;

                    //save the tender
                    m_pos.StartAddTenderToTable(st);
                    m_pos.ShowWaitForm(this);

                    if (m_pos.LastAsyncException != null)
                    {
                        if (m_pos.WeAreAPOSKiosk) //refund money
                        {
                            POSMessageForm.Show(this, m_pos, "System error, restarting...", POSMessageFormTypes.Pause, 5000);
                            m_pos.ClosePOS(this, new EventArgs());
                            Close();
                            return null;
                        }
                        else
                        {
                            POSMessageForm.Show(this, m_pos, "Tender record could not be written.\r\n\r\n" + m_pos.LastAsyncException.Message);
                        }
                    }

                    st.DefaultAmount = 0;

                    if (!voiding)
                        m_pos.CurrentSale.Id = st.RegisterReceiptID;

                    UpdateDisplay(true);

                    string errorMessage = m_pos.m_lastTenderProcessingReply.Message;

                    //if this error is to be logged for auditing, remove the AUDIT: tag before showing it
                    if (errorMessage.ToUpper().StartsWith("AUDIT:"))
                        errorMessage = errorMessage.Substring(6);

                    ShowMessage(errorMessage, Resources.TenderingFailure);
                }

                if (m_pos.WeAreAPOSKiosk)
                {
                    NotIdle();
                    EnableKioskIdleTimer();
                }

                m_buttonRemoveLine.Enabled = removeTenderEnabled;
                m_btnCancelSale.Enabled = cancelSaleEnabled;
                m_btnCancelTendering.Enabled = cancelTenderingEnabled;
                m_btnContinueSale.Enabled = continueSaleEnabled;
                m_btnCurrency.Enabled = currencyEnabled;
                m_btnDevice.Enabled = deviceEnabled;
                m_btnFinishTender.Enabled = finishTenderEnabled;
                m_btnSwapLeftRightHanded.Enabled = swapLeftRightHandedEnabled;
                m_buttonScrollDown.Enabled = scrollDownEnabled;
                m_buttonScrollUp.Enabled = scrollUpEnabled;
                m_keypad.Enabled = keypadEnabled;
                m_tenderButtonMenu.Enabled = tenderButtonMenuEnabled;

                if (m_currentSale.CalculateAmountTendered() < m_currentSale.CalculateTotal(false))
                    DisableSimpleKioskControls(false);

                m_pos.ProcessingTender = false;

                return st;
            }
        }

        private void ProcessSaleTenderButton(object sender, EventArgs e)
        {
            ProcessSaleTender(sender, e);
        }

        /// <summary>
        /// Processes a tender
        /// </summary>
        private SaleTender ProcessSaleTender(object sender, EventArgs e)
        {
            var currentCurrencyAmount = m_keypad.Value;
            decimal amount = m_pos.CurrentCurrency.ConvertFromThisCurrencyToDefaultCurrency(currentCurrencyAmount);
            string tenderButtonText = ((Button)sender).Text;
            TenderType tenderType;

            if (tenderButtonText == Resources.CreditOrDebitCard) //if we are using the credit button as credit/debit, process as a credit card
                tenderType = TenderType.CreditCard;
            else
                tenderType = m_pos.Settings.GetTenderType(((GTI.Controls.ImageButton)sender).Text); //find the tender type from the text on the pressed menu button

            if (tenderType == TenderType.Undefined)
            {
                ShowMessage(Resources.FeatureNotImplemented);
                return null;
            }

            if (amount == 0)
            {
                ShowMessage(string.Format(Resources.TenderTooSmall, m_pos.CurrentCurrency.FormatCurrencyString(m_pos.CurrentCurrency.ConvertFromDefaultCurrencyToThisCurrency(.01M))), Resources.InvalidEntry);
                UpdateDisplay();
                return null;
            }

            SaleTender st = null;

            if (WeCanTenderWithThisTypeForThisAmount(tenderType, amount))
                st = ProcessTender(tenderType, currentCurrencyAmount, m_pos.CurrentCurrency);

            CompleteSingleTender();

            return st;
        }

        /// <summary>
        /// Called on completion of a single tender.
        /// </summary>
        private void CompleteSingleTender()
        {
            if (m_currentSale.CalculateAmountTendered() >= m_currentSale.CalculateTotal(false))
                FinishTendering();
            else
                UpdateDisplay();
        }

        /// <summary>
        /// Processes a refund for the selected tender from the tender list
        /// </summary>
        private SaleTender ProcessRefundTender(bool abortedTransactionCleanup = false)
        {
            SaleTender st = null;

            if (m_tendersList.SelectedItem != null)
            {
                ListBoxTenderItem selectedTender = m_tendersList.SelectedItem as ListBoxTenderItem;

                st = ProcessTender(0, 0, null, selectedTender);

                if (abortedTransactionCleanup)
                    UpdateDisplay();
                else
                    CompleteSingleTender();
            }

            return st;
        }

        /// <summary>
        /// Determines if the tender request is allowed, and displays error if
        /// not allowed.
        /// </summary>
        /// <param name="tenderType"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        private bool WeCanTenderWithThisTypeForThisAmount(TenderType tenderType, decimal amount)
        {
            decimal amountTenderRemaining = m_currentSale.CalculateTotal(false) - m_currentSale.CalculateAmountTendered();

            //we can only over-tender with cash
            if (tenderType != TenderType.Cash && amount > amountTenderRemaining)
            {
                ShowMessage(Resources.OverpaymentNotAllowed);
                return false;
            }

            //if we don't allow split tendering, the amount must be enough to cover the sale
            if (!m_pos.Settings.AllowSplitTendering && tenderType != TenderType.Cash && amount < amountTenderRemaining)
            {
                ShowMessage(Resources.TenderError);
                return false;
            }

            return true;
        }

        private void m_btnTestX_Click(object sender, EventArgs e)
        {
            m_keypad.Value = Convert.ToDecimal((sender as ImageButton).Text.Substring(1));
            m_sellingForm.IncNVRAMUserDecimal(SellingForm.NVRAMUserDecimal.AmountCollected, m_keypad.Value); //keep track of how much we collected for this transaction (power failure)
            ProcessSaleTenderButton(m_tenderButtonMenu.GetButton(m_cashTenderID), new EventArgs());
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Swap the display from right handed to left handed or left handed to right handed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void m_btnSwapLeftRightHanded_Click(object sender, EventArgs e)
        {
            if (m_keypad.Location.X > 300) //right handed, go left
                MakeTenderingScreenLeftHanded();
            else //left handed, go right
                MakeTenderingScreenRightHanded();
        }

        /// <summary>
        /// Moves controls to left handed positions
        /// </summary>
        private void MakeTenderingScreenLeftHanded()
        {
            this.SuspendLayout();

            if (m_allowTheKeypad)
                m_panelMain.BackgroundImage = GTI.Modules.POS.Properties.Resources.SplitTenderingBackBig2Left;
            else
                m_panelMain.BackgroundImage = GTI.Modules.POS.Properties.Resources.POSKioskSplitTenderingBackBig2Left;

            m_dueLabel.Location = new Point(520, 531);
            m_dueAmountLabel.Location = new Point(747, 521);
            //m_tendersList.Location = new Point(0, 0);
            m_tendersPanel.Location = new Point(517, 93);
            m_buttonRemoveLine.Location = new Point(447, 93);
            m_buttonScrollUp.Location = new Point(447, 212);
            m_buttonScrollDown.Location = new Point(447, 348);
            m_btnCancelTendering.Location = new Point(447, 465);
            m_statusTextbox.Location = new Point(30, 19);
            m_keypad.Location = new Point(73, 235);
            m_tenderButtonMenu.Location = new Point(26, 559);
            m_btnFinishTender.Location = new Point(17, 607);
            m_lblPlayerName.Location = new Point(517, 607);
            m_lblPlayerPoints.Location = new Point(517, 648);
            m_lblExchangeRate.Location = new Point(517, 715);
            m_btnCurrency.Location = new Point(447, 580);
            m_btnSwapLeftRightHanded.Location = new Point(447, 680);
            m_btnContinueSale.Location = new Point(307, 680);

            this.ResumeLayout();
        }

        /// <summary>
        /// Moves controls to right handed positions
        /// </summary>
        private void MakeTenderingScreenRightHanded()
        {
            this.SuspendLayout();

            if (m_allowTheKeypad)
                m_panelMain.BackgroundImage = GTI.Modules.POS.Properties.Resources.SplitTenderingBackBig2;
            else
                m_panelMain.BackgroundImage = GTI.Modules.POS.Properties.Resources.POSKioskSplitTenderingBackBig2;

            m_dueLabel.Location = new Point(31, 531);
            m_dueAmountLabel.Location = new Point(262, 521);
            //m_tendersList.Location = new Point(0, 0);
            m_tendersPanel.Location = new Point(28, 93);
            m_buttonRemoveLine.Location = new Point(522, 93);
            m_buttonScrollUp.Location = new Point(522, 212);
            m_buttonScrollDown.Location = new Point(522, 348);
            m_btnCancelTendering.Location = new Point(522, 465);
            m_statusTextbox.Location = new Point(640, 19);
            m_keypad.Location = new Point(683, 235);
            m_tenderButtonMenu.Location = new Point(636, 559);
            m_btnFinishTender.Location = new Point(627, 607);
            m_lblPlayerName.Location = new Point(31, 607);
            m_lblPlayerPoints.Location = new Point(31, 648);
            m_lblExchangeRate.Location = new Point(31, 715);
            m_btnCurrency.Location = new Point(522, 580);
            m_btnSwapLeftRightHanded.Location = new Point(522, 680);
            m_btnContinueSale.Location = new Point(378, 680);

            this.ResumeLayout();
        }

        /// <summary>
        /// Close the dialog and tell the caller we want to go back, not proceed with sale
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void m_btnCancelTendering_Click(object sender, EventArgs e)
        {
            EnableKioskIdleTimer(false);

            ActivateBillAcceptor(false);

            DialogResult = DialogResult.Cancel;
            Close();
        }

        /// <summary>
        /// Close the dialog and tell the caller we want to go back, not proceed with sale
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void m_btnCancelSale_Click(object sender, EventArgs e)
        {
            NotIdle();

            if (m_pos.WeAreAnAdvancedPOSKiosk && m_useSimpleFormForAdvancedKiosk)
            {
                m_btnCancelTendering_Click(sender, e);
                return;
            }

            if (sender == m_btnCancelSale)
            {
                m_idleSince = DateTime.Now + TimeSpan.FromDays(1);

                if (POSMessageForm.ShowCustomTwoButton(this, m_parent, Resources.CancelSaleQuestion, "", true, 1, Resources.Continue, Resources.CancelSale) == 1)
                {
                    NotIdle();
                    return;
                }
            }

            //If we are a non-advanced kiosk, we are in a payment state but the idle screen is under us.
            //Before we change states, the idle screen will show briefly.

            DisableSimpleKioskControls();
            m_lblKioskTotalDue.Text = "Sale Canceled";
            m_sellingForm.StartOver(true);
            m_btnCancelTendering_Click(sender, e);
        }

        /// <summary>
        /// Handles the event when the down arrow is clicked
        /// to scroll down the receipt items.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void m_buttonScrollDown_Click(object sender, EventArgs e)
        {
            NotIdle();

            m_tendersList.Down();
        }

        /// <summary>
        /// Handles the event when the up arrow is clicked
        /// to scroll up the receipt items.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void m_buttonScrollUp_Click(object sender, EventArgs e)
        {
            NotIdle();

            m_tendersList.Up();
        }

        /// <summary>
        /// Handles the event when the remove line button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void m_buttonRemoveTender_Click(object sender, EventArgs e)
        {
            NotIdle();

            //see if we can remove or void the selected line in the receipt
            if (m_tendersList.SelectedItem != null)
            {
                ListBoxTenderItem selected = m_tendersList.SelectedItem as ListBoxTenderItem;

                if (selected.ReceiptLine != -1) //this is a sale item line, we can remove it
                {
                    RemoveASaleLine(selected.ReceiptLine);
                }
                else if (!selected.IsTextLine && selected.Type != 0 && !selected.IsRefunded && m_pos.WeAreNotAPOSKiosk) //this is a non-refunded tender line, we can void the tender
                {
                    ProcessRefundTender();

                    m_tendersList.Invalidate();
                    UpdateDisplay();
                }
            }
        }

        /// <summary>
        /// This is used when sale items are removed resulting in the sale being over-tendered and there is enough cash tendered to give change. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void m_btnFinishTender_Click(object sender, EventArgs e)
        {
            NotIdle();

            FinishTendering();
        }

        /// <summary>
        /// Change the active tendering currency.  This actually changes the sale's current currency.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void m_btnCurrency_Click(object sender, EventArgs e)
        {
            m_sellingForm.CurrencyButtonClick(sender, e); //have the user select the new currency to use

            m_btnCurrency.Text = m_pos.CurrentCurrency.ISOCode; //put the 3 character currency identification on the button

            m_keypad.CurrencySymbol = m_pos.CurrentCurrency.Symbol; //change the currency symbol for tendering on the keypad
            m_keypad.CurrencySymbolForeColor = System.Drawing.Color.Yellow;

            if (!m_pos.CurrentCurrency.IsDefault) //the selected currency is not the default currency, show the conversion info on the screen
            {
                decimal amount = 1M;

                while (m_pos.CurrentCurrency.ConvertFromThisCurrencyToDefaultCurrency(amount) == 0)
                    amount *= 10M;

                m_lblExchangeRate.Text = string.Format("{0} = {1}", m_pos.CurrentCurrency.FormatCurrencyString(amount), m_pos.DefaultCurrency.FormatCurrencyString(m_pos.CurrentCurrency.ConvertFromThisCurrencyToDefaultCurrency(amount)));
            }
            else //this is the default currency, no need to show conversion info
            {
                m_lblExchangeRate.Text = string.Empty;
            }

            UpdateDisplay();
        }

        private void m_tendersList_SelectedIndexChanged(object sender, EventArgs e)
        {
            NotIdle();

            bool enableRemoveLine = false;  //assume we can't remove or void the selected line

            if (m_tendersList.SelectedIndex != -1)
            {
                ListBoxTenderItem ti = (ListBoxTenderItem)m_tendersList.Items[m_tendersList.SelectedIndex];

                if ((ti.ReceiptLine != -1 || ti.Type != 0) && !ti.IsRefunded) //the user selected a sale item line or a non-refunded tender, allow the remove button
                {
                    enableRemoveLine = true;

                    if (ti.Type == 0) //not a tender
                    {
                        SaleItem item = m_sellingForm.GetSaleListRaw()[ti.ReceiptLine] as SaleItem;

                        if (item != null && item.IsValidationPackage)
                            enableRemoveLine = false;
                    }
                    else //tender
                    {
                        if(m_pos.WeAreAPOSKiosk) //tender on a kiosk, don't allow removal
                            enableRemoveLine = false;
                    }
                }
            }

            m_buttonRemoveLine.Enabled = enableRemoveLine;
        }

        void m_tendersList_DrawItem(object sender, DrawItemEventArgs e)
        {
            Font myFont;
            int i = e.Index;

            if (i > -1 && m_tendersList.Items.Count > 0)
            {
                ListBoxTenderItem current = m_tendersList.Items[i] as ListBoxTenderItem;

                // Draw the highlight color if this item is selected.
                if ((e.State & DrawItemState.Selected) == DrawItemState.Selected && i >= m_blankLinesInDisplay)
                    e.Graphics.FillRectangle(new SolidBrush(m_tendersList.HighlightColor), e.Bounds);
                else
                    e.Graphics.FillRectangle(new SolidBrush(m_tendersList.BackColor), e.Bounds);

                myFont = e.Font;

                Brush textBrush;

                if (current.IsTextLine) //not a tender line
                {
                    if (current.IsTotalLine) //total lines are white
                    {
                        textBrush = System.Drawing.Brushes.White;
                    }
                    else //not a total or a tender, must be a sale item line
                    {
                        if (current.NonTaxedCoupon) //non-taxed coupons are orange
                            textBrush = System.Drawing.Brushes.Orange;
                        else //sale items are yellow
                        {
                            if (current.ReceiptLine == -1) //text in receipt
                                textBrush = System.Drawing.Brushes.White;
                            else //sale item
                                textBrush = System.Drawing.Brushes.Yellow;
                        }
                    }
                }
                else //tender line, tender lines are turquoise
                {
                    textBrush = System.Drawing.Brushes.Turquoise;
                }

                e.Graphics.DrawString(current.ToString(), myFont, textBrush, e.Bounds);

                e.DrawFocusRectangle();
            }
        }

        private void SetDeviceButton()
        {
            if (m_pos.WeAreAPOSKiosk && m_currentSale.HasElectronics && m_sellingForm.WeHaveAUnitSelectionSystemMenuButton)
            {
                m_lblKioskPlayingOn.Visible = true;
                m_btnDevice.Visible = true;

                if (m_currentSale.Device.Id == Device.Explorer.Id)
                {
                    m_btnDevice.ImageNormal = Resources.DeviceExplorerUp271;
                    m_btnDevice.ImagePressed = Resources.DeviceExplorerDown271;
                    m_btnDevice.Alignment = StringAlignment.Near;
                }
                else if (m_currentSale.Device.Id == Device.Fixed.Id)
                {
                    m_btnDevice.ImageNormal = Resources.DeviceFixedUp271;
                    m_btnDevice.ImagePressed = Resources.DeviceFixedDown271;
                    m_btnDevice.Alignment = StringAlignment.Near;
                }
                else if (m_currentSale.Device.Id == Device.Tablet.Id)
                {
                    m_btnDevice.ImageNormal = Resources.TEDE271v2;
                    m_btnDevice.ImagePressed = Resources.TEDE271v2Down;
                    m_btnDevice.Alignment = StringAlignment.Center;
                }
                else if (m_currentSale.Device.Id == Device.Tracker.Id)
                {
                    m_btnDevice.ImageNormal = Resources.DeviceTrackerUp271;
                    m_btnDevice.ImagePressed = Resources.DeviceTrackerDown271;
                    m_btnDevice.Alignment = StringAlignment.Near;
                }
                else if (m_currentSale.Device.Id == Device.Traveler.Id)
                {
                    m_btnDevice.ImageNormal = Resources.DeviceTravelerUp271;
                    m_btnDevice.ImagePressed = Resources.DeviceTravelerDown271;
                    m_btnDevice.Alignment = StringAlignment.Near;
                }
                else if (m_currentSale.Device.Id == Device.Traveler2.Id)
                {
                    m_btnDevice.ImageNormal = Resources.DeviceTraveler2Up271;
                    m_btnDevice.ImagePressed = Resources.DeviceTraveler2Down271;
                    m_btnDevice.Alignment = StringAlignment.Near;
                }
                else
                {
                    m_lblKioskPlayingOn.Visible = false;
                    m_btnDevice.Visible = false;
                }

                m_btnDevice.Text = (m_currentSale.Device.Id != Device.Tablet.Id ? " " : string.Empty) + (m_currentSale.DeviceFee == 0M ? Resources.Free : m_currentSale.SaleCurrency.FormatCurrencyString(m_currentSale.SaleCurrency.ConvertFromDefaultCurrencyToThisCurrency(m_currentSale.DeviceFee)));
            }
            else
            {
                m_lblKioskPlayingOn.Visible = false;
                m_btnDevice.Visible = false;
            }
        }

        private void m_statusTextbox_Enter(object sender, EventArgs e)
        {
            NotIdle();
            m_tendersList.Focus();
        }

        private void m_keypad_Click(object sender, EventArgs e)
        {
            NotIdle();
        }

        private void FlexTenderingForm_Click(object sender, EventArgs e)
        {
            NotIdle();
        }

        private void m_tenderButtonMenu_Click(object sender, EventArgs e)
        {
            NotIdle();
        }

        private void NotIdle()
        {
            m_idleSince = DateTime.Now;
            m_timeoutProgress.Hide();
            m_timeoutProgress.Value = 0;
            m_simpleKioskProgress.Hide();
            m_simpleKioskProgress.Value = 0;
        }

        private void m_btnHelp_Click(object sender, EventArgs e)
        {
            m_idleSince = DateTime.Now + TimeSpan.FromDays(1);

            HelpForm help = new HelpForm(m_pos, GTI.Modules.POS.UI.HelpForm.HelpTopic.Payment, m_weHaveABillAcceptor);

            if (help.ShowDialog(this) == DialogResult.Abort)
                m_idleSince = DateTime.Now - TimeSpan.FromMinutes(1);
            else
                NotIdle();
        }

        /// <summary>
        /// Handles the form's KeyPress event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An KeyPressEventArgs object that contains the 
        /// event data.</param>
        private void KeyPressed(object sender, KeyPressEventArgs e)
        {
            NotIdle();
            e.Handled = true; // Don't send to the active control.
        }

        /// <summary>
        /// Handles the form's Command key events
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="keyData"></param>
        /// <returns></returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            NotIdle();
            return false;
        }

        private void SellingForm_KeyDown(object sender, KeyEventArgs e)
        {
            e.SuppressKeyPress = false;
            e.Handled = true;
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
            if ((keyData & Keys.Enter) == Keys.Enter && (keyData & Keys.Shift) != Keys.Shift)
                return false;
            else
                return base.ProcessDialogKey(keyData);
        }

        private void m_dueAmountLabel_TextChanged(object sender, EventArgs e)
        {
            if (m_voidAllTenders && (m_parent.WeAreANonAdvancedPOSKiosk || (m_parent.WeAreAnAdvancedPOSKiosk && m_parent.Settings.UseSimplePaymentForAdvancedKiosk)))
            {
                if(m_leftToRefund > 0)
                    m_lblKioskTotalDue.Text = "Refunding: " + m_leftToRefund.ToString("C");
                else
                    m_lblKioskTotalDue.Text = "";
            }
            else
            {
                if (m_dueAmountLabel.Text[0] == '-' || m_dueAmountLabel.Text[0] == '(')
                {
                    m_lblKioskTotalDue.Text = "Paid in Full";
                }
                else
                {
                    m_lblKioskTotalDue.Text = "Total Amount Due: " + m_dueAmountLabel.Text;
                }
            }
        }

        private void DisableSimpleKioskControls(bool disable = true)
        {
            if (m_keepControlsDisabled && !disable)
                return;

            if (m_pos.WeAreANonAdvancedPOSKiosk)
            {
                if (disable || m_voidAllTenders)
                    m_kioskIdleTimer.Stop();
                else
                    m_kioskIdleTimer.Start();

                NotIdle();

                m_lblKioskInstructions.Enabled = !disable;
                m_btnKioskCard.Enabled = !disable;
                m_btnKioskQuit.Enabled = !disable;
                m_btnSimpleKioskHelp.Enabled = !disable;
            }

            if (m_pos.Settings.KioskTestWithoutAcceptor)
            {
                m_btnAdvTest1.Enabled = !disable;
                m_btnAdvTest5.Enabled = !disable;
                m_btnAdvTest10.Enabled = !disable;
                m_btnTest1.Enabled = !disable;
                m_btnTest5.Enabled = !disable;
                m_btnTest10.Enabled = !disable;
            }
        }

        private void m_btnCard_Click(object sender, EventArgs e)
        {
            if (ActivateBillAcceptor(false)) //if the bill acceptor can be deactivated, we are not accepting a bill now so we can process a card
            {
                m_lblKioskInstructions.Visible = false;
                m_lblKioskCardInstructions.Visible = true;
                m_picCreditCardDevice.Visible = true;
                m_btnKioskCard.Visible = false;
                m_btnKioskQuit.Enabled = false;
                m_btnSimpleKioskHelp.Enabled = false;
                m_kioskCardInstructionsFlashTimer.Enabled = true;

                ProcessSaleTenderButton(m_tenderButtonMenu.GetButton(m_cardTenderID), new EventArgs());

                m_kioskCardInstructionsFlashTimer.Enabled = false;
                m_lblKioskCardInstructions.Visible = false;
                m_picCreditCardDevice.Visible = false;
                m_lblKioskInstructions.Visible = m_weHaveABillAcceptor || m_pos.Settings.KioskTestWithoutAcceptor;
                m_btnKioskCard.Visible = true;
                m_btnKioskQuit.Enabled = true;
                m_btnSimpleKioskHelp.Enabled = true;
            }
        }

        private void UserActivityDetected(object sender, EventArgs e)
        {
            NotIdle();
        }

        private void m_btnSimpleKioskHelp_Click(object sender, EventArgs e)
        {
            m_idleSince = DateTime.Now + TimeSpan.FromDays(1);

            m_pos.HelpScreenActive = true;

            HelpForm help = new HelpForm(m_pos, GTI.Modules.POS.UI.HelpForm.HelpTopic.Payment, m_weHaveABillAcceptor);

            bool timedOut = help.ShowDialog(this) == DialogResult.Abort;

            m_pos.HelpScreenActive = false;

            if (timedOut)
                m_idleSince = DateTime.Now - TimeSpan.FromMinutes(1);
            else
                NotIdle();
        }

        public DialogResult ShowMessage(string text, string caption, POSMessageFormTypes type = POSMessageFormTypes.OK, int pause = 0)
        {
            m_idleSince = DateTime.Now + TimeSpan.FromDays(1);

            DialogResult result = m_pos.ShowMessage(this, m_displayMode, text, caption, type, pause);

            NotIdle();

            return result;
        }

        public DialogResult ShowMessage(string text, POSMessageFormTypes type = POSMessageFormTypes.OK, int pause = 0)
        {
            m_idleSince = DateTime.Now + TimeSpan.FromDays(1);

            IWin32Window owner = this;

            if (m_sellingForm.KioskForm != null)
                owner = (IWin32Window)m_sellingForm.KioskForm;

            DialogResult result = m_pos.ShowMessage(this, m_displayMode, text, type, pause);

            NotIdle();

            return result;
        }

        #endregion

        #region Bill acceptor events

        void Guardian_MoneyAccepted(object sender, Guardian.Acceptors.ItemAcceptResultEventArgs e)
        {
            NotIdle();

            if (e == null) //bill acceptor has something in it
            {
                if (m_keypad.InvokeRequired)
                    this.Invoke(new MethodInvoker(delegate()
                    {
                        DisableSimpleKioskControls();
                    }));
                else
                    DisableSimpleKioskControls();

                return;
            }

            if (e.Result.ResultState == Guardian.Acceptors.ItemAcceptResultStates.REJECTED)
            {
                if (m_keypad.InvokeRequired)
                    this.Invoke(new MethodInvoker(delegate()
                    {
                        DisableSimpleKioskControls(false);
                    }));
                else
                    DisableSimpleKioskControls(false);

                return;
            }
            
            if (e.Result.ResultState == Guardian.Acceptors.ItemAcceptResultStates.ACCEPTED) //we accepted something
            {
                decimal value = e.Result.Value == null ? 0M : (decimal)e.Result.Value;

                if (m_parent.Settings.KioskTestTreatTicketAs20 && e.Result.Item.ItemType == Guardian.Acceptors.AcceptorItemTypes.BARCODE)
                {
                    if (e.Result.Barcode == "000000000000000100")
                        value = 1M;
                    else if (e.Result.Barcode == "000000000000000500")
                        value = 5M;
                    else if (e.Result.Barcode == "000000000000001000")
                        value = 10M;
                    else if (e.Result.Barcode == "000000000000002000")
                        value = 20M;
                    else if (e.Result.Barcode == "000000000000005000")
                        value = 50M;
                    else if (e.Result.Barcode == "000000000000010000")
                        value = 100M;
                    else
                        value = 20M;
                }

                m_sellingForm.IncNVRAMUserDecimal(SellingForm.NVRAMUserDecimal.AmountCollected, value); //keep track of how much we collected for this transaction (power failure)

                if (m_keypad.InvokeRequired)
                    this.Invoke(new MethodInvoker(delegate()
                    {
                        m_keypad.Value = value;
                    }));
                else
                    m_keypad.Value = value;

                if (m_btnContinueSale.InvokeRequired)
                    this.Invoke(new MethodInvoker(delegate()
                    {
                        ProcessSaleTenderButton(m_tenderButtonMenu.GetButton(m_cashTenderID), new EventArgs());
                    }));
                else
                    ProcessSaleTenderButton(m_tenderButtonMenu.GetButton(m_cashTenderID), new EventArgs());
            }
        }
        #endregion
    }
}
