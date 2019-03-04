
#region Copyright
// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2008 GameTech
// International, Inc.
// US4028 Adding support for checking card counts when each product is
//  added
#endregion

//US4118 (US4101) Set  PIN number > Sale without player card
//US4119 (US4101) Set PIN number >  Sale with player card and PIN has not been set.
//US4380: (US4337) POS: Display B3 Menu
//US4382: (US4337) POS: B3 Open sale
//US4467: (US4428) POS: Set validation to enabled after cancel transaction
//DE12930: POS: Coupon is not removed from the transaction
//US4656: POS: Allow Exit from "A sale is in progress" prompt
//US4439:POS: Abort a transaction
//DE12973: POS: When validations is off and changing session the background color is incorrect
//US4871: POS: Display discount spend level on UI
//US4962: POS: Open bank close UI when paper is closed
//US3512: Manual exchange
//US5115: POS: Add Register Closing report button
//US5108: POS > Bank Close: Print register closing report
//DE13359: POS: Index out of range error when Sell Electronics is disabled
//US5192/DE13363 POS, B3 receipt not printing instead it print EDGE receipt.
//US5770 Support for using the Banned flag rather than banned string

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using GTI.Controls;
using GTI.EliteCreditCards.Business;
using GTI.EliteCreditCards.Data;
using GTI.Modules.PlayerCenter.UI;
using GTI.Modules.POS.Business;
using GTI.Modules.POS.Data;
using GTI.Modules.POS.Properties;
using GTI.Modules.Shared;
using CashMethod = GTI.Modules.Shared.CashMethod;
using DataSizes = GTI.Modules.Shared.DataSizes;
using EliteModule = GTI.Modules.Shared.EliteModule;
using LoggerLevel = GTI.Modules.Shared.LoggerLevel;
using Player = GTI.Modules.Shared.Player;
using SecurityHelper = GTI.Modules.Shared.SecurityHelper;
using GTI.Modules.POS.UI.PaperRangeScanner;
using GTI.Modules.Shared.Data;
using System.Runtime.InteropServices;
using GTI.Modules.Shared.UI;

namespace GTI.Modules.POS.UI
{
    /// <summary>
    /// The main form of the POS.
    /// </summary>
    internal partial class SellingForm : POSForm
    {
        #region Constants and Data Types
        protected readonly Color SaleNormalBackColor = Color.FromArgb(95, 87, 83);
        protected readonly Color MenuNormalBackColor = Color.FromArgb(39, 75, 119);
        protected readonly Color VersionNormalBackColor = Color.FromArgb(95, 87, 83); // Rally TA7464
        protected readonly Color SmallVersionNormalBackColor = Color.FromArgb(59, 42, 26);
        protected readonly Color ReturnBackColor = Color.FromArgb(198, 12, 11);
        protected readonly Color SmallVersionReturnBackColor = Color.FromArgb(165, 1, 0);
        protected readonly Color B3SessionBackColor = Color.FromArgb(0, 150, 0); //US4380: (US4337) POS: Display B3 Menu
        protected readonly Color ValidationDisabledBackColor = Color.FromArgb(255, 255, 77); //US4467
        // Rally US765
        protected readonly Font PackFont = new Font("Tahoma", 14F, FontStyle.Bold);
        protected const int PackDeviceId = 0;
        protected const int MaxDeviceButtons = 4;

        /// <summary>
        /// Represents information about a device button.
        /// </summary>
        protected class DeviceButtonEntry
        {
            #region Member Properties
            /// <summary>
            /// Gets or sets whether this button is available.
            /// </summary>
            public bool IsAvailable
            {
                get;
                set;
            }

            /// <summary>
            /// The button's text.
            /// </summary>
            public string Text
            {
                get;
                set;
            }

            /// <summary>
            /// Get/set the device ID.
            /// </summary>
            public int Id
            {
                get;
                set;
            }

            /// <summary>
            /// The button's click handler.
            /// </summary>
            public EventHandler Handler
            {
                get;
                set;
            }

            public EventHandler HandlerIfHeld
            {
                get;
                set;
            }

            /// <summary>
            /// The button's normal image.
            /// </summary>
            public Bitmap NormalImage
            {
                get;
                set;
            }

            /// <summary>
            /// The button's pressed image.
            /// </summary>
            public Bitmap PressedImage
            {
                get;
                set;
            }
            #endregion
        }

        protected const int NormalPaddingX = 8;
        protected const int NormalPaddingY = 5;
        protected const int SmallPaddingX = 5;
        protected const int SmallPaddingY = 5;

        // System Button Menu Ids
        protected const string ReturnId = "Return";
        protected const string PlayerMgmtId = "PlayerMgmt";
        protected const string ScanCardId = "ScanCard";
        protected const string QuantitySaleId = "QuantitySale";
        protected const string RepeatLastSaleId = "RepeatLastSale";
        protected const string ReprintLastReceiptId = "ReprintLastReceipt";
        protected const string ReprintPlayerLastReceiptId = "ReprintPlayerLastReceipt";
        protected const string CreditCashOutId = "CreditCashOut";
        protected const string UnitAssignmentId = "UnitAssignment";
        protected const string ViewReceiptsId = "ViewReceipts";
        protected const string VoidReceiptsId = "VoidReceipts";
        protected const string AdjustBankId = "AdjustBank";
        protected const string RegisterSalesReportId = "RegisterSalesReport";
        protected const string RegisterClosingReportId = "RegisterClosingReport";
        protected const string CloseBankId = "CloseBank"; // FIX: DE1930
        protected const string ValidateId = "Validate"; //US3509
        protected const string PaperUsageId = "PaperUsageId"; //US3509
        protected const string PaperExchangeId = "PaperExchangeId";
        protected const string PlayerCouponsId = "PlayerCoupons";
        protected const string UnitSelectId = "UnitSelection";
        protected const string PaperRangeScannerId = "PaperRangeScanner";

        // TODO Revisit other system buttons.
        protected const string PayReceiptsId = "PayReceipts";
        protected const string RedeemCompsId = "RedeemComps";

        // PDTS 964
        /// <summary>
        /// Represents a picture on a menu button.
        /// </summary>
        protected enum MenuButtonGraphic
        {
            None = 0,
            Set = 1,
            Book = 2,
            Paper = 3,
            Credit = 5,
            Discount = 6,
            // FIX: TA3446
            Electronic = 7,
            Concession = 8, 
            Merchandise = 9,
            // END: TA3446
            // US2098
            Brown = 10,
            Green = 11,
            Orange = 12,
            Purple = 13,
            Rainbow = 14,
            Red = 15,
            White = 16,
            Yellow = 17,
            // END: US2098
            // US4885
            Black3D =	18,
            Blue3D =	19,
            Brown3D =	20,
            Gold3D =	21,
            Gray3D =	22,
            Green3D =	23,
            Lavender3D = 24,
            Orange3D =	25,
            Orchid3D =	26,
            Pink3D =	27,
            Rainbow3D =	28,
            Red3D =	29,
            Tan3D =	30,
            White3D =	31,
            Yellow3D =	32,
            BlackFlat = 33,
            BlueFlat = 34,
            BrownFlat = 35,
            GoldFlat = 36,
            GrayFlat = 37,
            GreenFlat = 38,
            LavenderFlat = 39,
            OrangeFlat = 40,
            OrchidFlat = 41,
            PinkFlat = 42,
            RainbowFlat = 43,
            RedFlat = 44,
            TanFlat = 45,
            WhiteFlat = 46,
            YellowFlat = 47,
            PaperBrown = 48,
            PaperOrange = 49,
            PaperPurple = 50,
            PaperGreen = 51,
            PaperRed = 52,
            PaperRainbow = 53,
            PaperTan = 54,
            PaperWhite = 55
            //END US4885
        }

        public enum NVRAMUserDecimal
        {
            All = -1,
            First = 0,                      //First user decimal block number
            FirstTransactional = 0,
            RegisterReceiptID = 0,          //if 0, nothing to worry about
            TransactionTotal = 1,           //set on first tender
            AmountCollected = 2,            //if this >= TransactionTotal, sale might have succeded; otherwise, incomplete sale (might need to give change)
            AmountDispensed = 3,            //if AmountCollected - TransactionTotal < AmountDispensed, need to give more money
            PlayerID = 4,                   //Player ID for transaction (if we have one) else 0.
            AcceptedLate = 5,               //money accepted after POS crash
            DispensedLate = 6,              //money dispensed after POS crash or power failure
            SaleSucceeded = 7,              //sale was posted to server
            CashToDispense = 8,             //Lump sum cash computed for refund
            CashDispensed = 9,              //Amount of CashToDispense that was dispensed
            LastTransactional = 9,
            WriteTimeStamp = 10,
            Last = 10                        //Last user decimal block number
        }

        #endregion

        #region Member Variables
        protected int m_keypadPaddingX = NormalPaddingX;
        protected int m_keypadPaddingY = NormalPaddingY;
        protected bool m_haveDeviceFeesToSelectFrom;
        protected bool isPlayerHaveActiveComp = false;
        private ButtonEntry m_ValidationbuttonEntry; //US3509
        private List<string> m_stairStepDiscountText = new List<string>();
        private string m_version;
        private List<Device> m_availableDevices = new List<Device>();
        private bool m_addDiscountTextToScreenReceipt = false;
        protected DateTime m_idleSince = DateTime.Now;
        private DateTime m_nothingHappeningAndIdleSince = DateTime.Now;
        private Label m_noticeLabel = null;
        private SimpleKioskForm m_simpleKioskForm = null;
        private bool m_allDeviceFeesAreZero = false;
        private Tuple<decimal, bool>[] m_userDecimalNVRamCopy = new Tuple<decimal, bool>[(int)NVRAMUserDecimal.Last + 1]; //value and if it is in a delayed write state
        public decimal[] m_userDecimalNVRamStartup = new decimal[(int)NVRAMUserDecimal.Last + 1];
        protected bool m_giveChangeAsB3Credit = false;
        protected decimal m_changeToGiveAsB3Credit = 0;
        private DateTime m_lastCardSwipedAt = DateTime.Now - TimeSpan.FromMinutes(1);
        private int m_saleListUpdateCount = 0;
        #endregion

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        #region Constructors
        public SellingForm()
        {
        }

        /// <summary>
        /// Initializes a new instance of the SellingForm class.
        /// </summary>
        /// <param name="parent">The PointOfSale to which this form 
        /// belongs.</param>
        /// <param name="displayMode">The display mode used to show this 
        /// form.</param>
        /// <exception cref="System.ArgumentNullException">parent or 
        /// displayMode is a null reference.</exception>
        public SellingForm(PointOfSale parent, DisplayMode displayMode) 
            : base(parent, displayMode)
        {
            InitializeComponent();
            ApplyDisplayMode();

            if (parent.WeAreAPOSKiosk)
            {
                //load the NVRAM blocks (may need to recover from a power failure)
                for (NVRAMUserDecimal block = NVRAMUserDecimal.First; block <= NVRAMUserDecimal.Last; block++)
                    m_userDecimalNVRamCopy[(int)block] = new Tuple<decimal, bool>(ReadNVRAMUserDecimal(block), false);

                decimal lastTimeStamp = m_userDecimalNVRamCopy[(int)NVRAMUserDecimal.WriteTimeStamp].Item1;

                //ask the Guardian for info we don't have
                if (m_parent.Guardian != null && m_parent.Guardian.NeedToCheckForLateMoney)
                {
                    m_userDecimalNVRamCopy[(int)NVRAMUserDecimal.AcceptedLate] = new Tuple<decimal, bool>(m_parent.Guardian.GetAmountAcceptedButNotReported(), true);
                    m_userDecimalNVRamCopy[(int)NVRAMUserDecimal.DispensedLate] = new Tuple<decimal, bool>(m_parent.Guardian.GetAmountDispensedButNotReported(), true);
                    m_parent.Guardian.NVMWriteDecimal((int)NVRAMUserDecimal.AcceptedLate, (int)NVRAMUserDecimal.DispensedLate, m_userDecimalNVRamCopy);
                    m_parent.Guardian.ClearPreviousRequestsFromNVRam();
                }

                IncNVRAMUserDecimal(NVRAMUserDecimal.AmountCollected, m_userDecimalNVRamCopy[(int)NVRAMUserDecimal.AcceptedLate].Item1);
                IncNVRAMUserDecimal(NVRAMUserDecimal.AmountDispensed, m_userDecimalNVRamCopy[(int)NVRAMUserDecimal.DispensedLate].Item1);
                m_parent.Guardian.NVMClearDecimal((int)NVRAMUserDecimal.AcceptedLate, (int)NVRAMUserDecimal.DispensedLate, ref m_userDecimalNVRamCopy);
                
                for (NVRAMUserDecimal block = NVRAMUserDecimal.First; block <= NVRAMUserDecimal.Last; block++)
                    m_userDecimalNVRamStartup[(int)block] = m_userDecimalNVRamCopy[(int)block].Item1;

                m_userDecimalNVRamStartup[(int)NVRAMUserDecimal.WriteTimeStamp] = lastTimeStamp;

                //Now we have the adjusted 
                //      TransactionNumber
                //      TransactionTotal
                //      AmountCollected
                //      AmountDispensed
                //      PlayerID
                //in NVRam

                if (m_parent.WeAreAnAdvancedPOSKiosk)
                {
                    m_playerCardPicture.Image = Resources.AnimatedPlayerCard_large_;
                    m_playerCardPictureLabel.Visible = true;
                    m_playerCardPicture.Visible = true;

                    m_keypad.ClearKeyText = "Clear";
                    m_keypad.ClearKeyFont = new Font(m_keypad.NumbersFont.FontFamily, 20, m_keypad.NumbersFont.Style);

                    m_exitButton.Text = "Help";
                    m_exitButton.ImageNormal = Resources.PurpleButtonUp;
                    m_exitButton.ImagePressed = Resources.PurpleButtonDown;

                    if (!m_parent.HaveMenu)
                    {
                        m_playerCardPicture.Image = null;
                        m_simpleKioskForm = new SimpleKioskForm(m_parent, this);
                        m_simpleKioskForm.Visible = true;
                        m_simpleKioskForm.Focus();
                        m_simpleKioskForm.CloseKiosk();
                    }
                }
                else
                {
                    m_playerCardPicture.Image = null;
                    m_simpleKioskForm = new SimpleKioskForm(m_parent, this);

                    if (!m_parent.HaveMenu || (m_parent.WeAreAB3Kiosk && !m_parent.B3SessionActive))
                    {
                        m_simpleKioskForm.Visible = true;
                        m_simpleKioskForm.Focus();
                        m_simpleKioskForm.CloseKiosk();
                    }
                }
            }

            if (m_parent.Settings.LongPOSDescriptions)
                m_pointsLabel.Hide();

            if (!m_parent.Settings.CouponTaxable) //coupons applied after tax, show in total area
            {
                m_couponTotalLabel.Show();
                m_couponTotal.Show();
            }

            // PDTS 1064
            // Start listening for swipes.
            if (m_parent.WeAreNotAPOSKiosk || m_parent.WeAreAnAdvancedPOSKiosk)
            {
                m_parent.MagCardReader.CardSwiped += new MagneticCardSwipedHandler(CardSwiped);

                // listen for barcode scans
                m_parent.BarcodeScanner.BarcodeScanned += new BarcodeScanHandler(BarcodeScanned);
            }

            // Show/hide device buttons.
            ConfigureTotalButtons();

            // Create the system button menu.
            CreateSystemMenu();

            // Rally TA7465
            if (!m_parent.Settings.MultiCurrencies)
            {
                m_keypad.CurrencySymbol = m_parent.CurrentCurrency.Symbol;

                // Can't change the currency if there is only one.
                m_currencyButton.Visible = false;
            }

            // Change the default text if needed.
            if(m_parent.Settings.EnableAnonymousMachineAccounts)
                m_playerLabel.Text = Resources.SellingFormMachine;

            // Set the system date and start the timer to update it.
            m_kioskTimeoutProgress.Maximum = KioskIdleLimitInSeconds / 3 * 1000;

            if(m_simpleKioskForm != null)
                m_simpleKioskForm.ProgressBar.Maximum = m_kioskTimeoutProgress.Maximum;

            NotIdle();
            m_systemTimeLabel.Text = GetSystemTime();
            m_dateTimeTimer.Start();

            // US4809
            m_parent.GetPlayerCompleted += OnGetPlayerCompleted;
            m_parent.BusyStatusChanged += OnBusyStatusChanged;

            if (m_parent.WeAreAPOSKiosk) //make our buttons use sound
            {
                foreach (Control ctrl in this.Controls)
                {
                    if (ctrl as ImageButton != null)
                        ((ImageButton)ctrl).UseClickSound = m_parent.Settings.UseKeyClickSoundsOnKiosk;
                }

                if (m_parent.WeAreANonAdvancedPOSKiosk)
                {
                    m_simpleKioskForm.Visible = true;
                    m_simpleKioskForm.Focus();
                }
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

            m_panelLastTotal.Location = new Point(13, 613); //move the "last sale total" panel over the sale totals.

            CreateMenuButtons(this, m_menuButtonsPanel, true, m_displayMode);

            if (m_displayMode is CompactDisplayMode)
            {
                m_panelMain.Size = m_displayMode.FormSize;

                // Adjust controls for small screen.
                m_panelMain.BackgroundImage = Resources.POSBack800;

                m_operatorLabel.Size = new Size(306, 25);
                m_operatorLabel.Font = new Font("Tahoma", 14F, FontStyle.Bold);

                m_gamingDateLabel.Size = new Size(153, 15);
                m_gamingDateLabel.Font = new Font("Tahoma", 7F, FontStyle.Bold);

                m_programLabel.Location = new Point(166, 43);
                m_programLabel.Size = new Size(153, 15);
                m_programLabel.Font = new Font("Tahoma", 7F, FontStyle.Bold);

                m_menuUpButton.Location = new Point(13, 61);
                m_menuUpButton.Size = new Size(50, 42);

                m_menuDownButton.Location = new Point(13, 105);
                m_menuDownButton.Size = new Size(50, 42);

                m_menuTop.BackgroundImage = Resources.SmallMainSellingListTop;
                m_menuTop.Location = new Point(65, 61);
                m_menuTop.Size = new Size(255, 8);
                m_menuLeft.Location = new Point(65, 69);
                m_menuLeft.Size = new Size(1, 70);
                m_menuRight.Location = new Point(319, 69);
                m_menuRight.Size = new Size(1, 70);
                m_menuBottom.BackgroundImage = Resources.SmallMainSellingListBottom;
                m_menuBottom.Location = new Point(65, 139);
                m_menuBottom.Size = new Size(255, 8);

                m_menuList.Location = new Point(66, 69);
                m_menuList.Size = new Size(253, 70);
                m_menuList.Font = new Font("Tahoma", 12F, FontStyle.Bold);

                m_playerLabel.Location = new Point(14, 159);
                m_playerLabel.Size = new Size(306, 25);

                m_playerInfoUpButton.Location = new Point(13, 187);
                m_playerInfoUpButton.Size = new Size(50, 42);

                m_playerInfoDownButton.Location = new Point(13, 231);
                m_playerInfoDownButton.Size = new Size(50, 42);

                m_playerInfoTop.BackgroundImage = Resources.SmallMainSellingListTop;
                m_playerInfoTop.Location = new Point(65, 187);
                m_playerInfoTop.Size = new Size(255, 8);
                m_playerInfoLeft.Location = new Point(65, 195);
                m_playerInfoLeft.Size = new Size(1, 70);
                m_playerInfoRight.Location = new Point(319, 195);
                m_playerInfoRight.Size = new Size(1, 70);
                m_playerInfoBottom.BackgroundImage = Resources.SmallMainSellingListBottom;
                m_playerInfoBottom.Location = new Point(65, 265);
                m_playerInfoBottom.Size = new Size(255, 8);

                m_playerInfoList.Location = new Point(66, 195);
                m_playerInfoList.Size = new Size(253, 70);
                m_playerInfoList.Font = new Font("Tahoma", 10F, FontStyle.Bold);

                m_sessionLabel.Location = new Point(62, 286);
                m_sessionLabel.Size = new Size(29, 15);
                m_sessionLabel.Font = new Font("Tahoma", 8F);

                m_quantityLabel.Location = new Point(87, 286);
                m_quantityLabel.Size = new Size(30, 15);
                m_quantityLabel.Font = new Font("Tahoma", 8F);

                // US2148
                m_itemLabel.Location = new Point(121, 286);
                m_itemLabel.Size = new Size(35, 15);
                m_itemLabel.Font = new Font("Tahoma", 8F);

                m_pointsLabel.Location = new Point(196, 286);
                m_pointsLabel.Size = new Size(33, 15);
                m_pointsLabel.Font = new Font("Tahoma", 8F);

                m_subtotalLabel.Location = new Point(252, 286);
                m_subtotalLabel.Size = new Size(33, 15);
                m_subtotalLabel.Font = new Font("Tahoma", 8F);

                m_removeLineButton.ImageNormal = Resources.SmallRemoveLineUp;
                m_removeLineButton.ImagePressed = Resources.SmallRemoveLineDown;
                m_removeLineButton.Location = new Point(13, 288);
                m_removeLineButton.Size = new Size(50, 42);

                m_saleItemUpButton.Location = new Point(13, 336);
                m_saleItemUpButton.Size = new Size(50, 42);

                m_saleItemDownButton.Location = new Point(13, 379);
                m_saleItemDownButton.Size = new Size(50, 42);

                m_startOverButton.ImageNormal = Resources.SmallStartOverUp;
                m_startOverButton.ImagePressed = Resources.SmallStartOverDown;
                m_startOverButton.Location = new Point(13, 427);
                m_startOverButton.Size = new Size(50, 42);

                m_saleTop.BackgroundImage = Resources.SmallMainSellingListTop;
                m_saleTop.Location = new Point(65, 301);
                m_saleTop.Size = new Size(255, 8);
                m_saleLeft.Location = new Point(65, 309);
                m_saleLeft.Size = new Size(1, 151);
                m_saleRight.Location = new Point(319, 309);
                m_saleRight.Size = new Size(1, 151);
                m_saleBottom.BackgroundImage = Resources.SmallMainSellingListBottom;
                m_saleBottom.Location = new Point(65, 460);
                m_saleBottom.Size = new Size(255, 8);

                m_saleList.Location = new Point(66, 309);
                m_saleList.Size = new Size(253, 151);
                m_saleList.Font = new Font("Lucida Console", 7.5F);

                //------------------------------
                //totals area
                m_orderSubtotalLabel.Location = new Point(13, 474);
                m_orderSubtotalLabel.Size = new Size(116, 13);
                m_orderSubtotalLabel.Font = new Font("Tahoma", 8F, FontStyle.Bold);
                m_pointsSubtotal.Location = new Point(129, 474);
                m_pointsSubtotal.Size = new Size(80, 13);
                m_pointsSubtotal.Font = new Font("Tahoma", 8F);
                m_orderSubtotal.Location = new Point(202, 474);
                m_orderSubtotal.Size = new Size(111, 13);
                m_orderSubtotal.Font = new Font("Tahoma", 8F);

                m_salesTaxLabel.Location = new Point(13, 487); // Rally TA7464
                m_salesTaxLabel.Size = new Size(90, 13);
                m_salesTaxLabel.Font = new Font("Tahoma", 8F, FontStyle.Bold);
                m_selectedDeviceName.Location = new Point(103, 487);
                m_selectedDeviceName.Size = new Size(120, 13);
                m_selectedDeviceName.Font = new Font("Tahoma", 8F);
                m_salesTax.Location = new Point(202, 487); // Rally TA7464
                m_salesTax.Size = new Size(111, 13);
                m_salesTax.Font = new Font("Tahoma", 8F);

                m_couponTotalLabel.Location = new Point(13, 500);
                m_couponTotalLabel.Size = new Size(90, 13);
                m_couponTotalLabel.Font = new Font("Tahoma", 8F, FontStyle.Bold);
                m_couponTotal.Location = new Point(202, 500);
                m_couponTotal.Size = new Size(111, 13);
                m_couponTotal.Font = new Font("Tahoma", 8F);

                m_prepaidTotalLabel.Location = new Point(13, 513); // Rally TA7464
                m_prepaidTotalLabel.Size = new Size(90, 13);
                m_prepaidTotalLabel.Font = new Font("Tahoma", 8F, FontStyle.Bold);
                m_prepaidTotal.Location = new Point(202, 513); // Rally TA7464
                m_prepaidTotal.Size = new Size(111, 13);
                m_prepaidTotal.Font = new Font("Tahoma", 8F);

                m_totalLabel.Location = new Point(13, 526); // Rally TA7464
                m_totalLabel.Size = new Size(116, 13);
                m_totalLabel.Font = new Font("Tahoma", 8F, FontStyle.Bold);
                m_pointsTotal.Location = new Point(129, 526); // Rally TA7464
                m_pointsTotal.Size = new Size(80, 13);
                m_pointsTotal.Font = new Font("Tahoma", 8F);
                m_total.Location = new Point(202, 526); // Rally TA7464
                m_total.Size = new Size(111, 13);
                m_total.Font = new Font("Tahoma", 8F);

                // PTDS 583
                m_pointsEarnedLabel.Location = new Point(13, 539); // Rally TA7464
                m_pointsEarnedLabel.Size = new Size(60, 13);
                m_pointsEarnedLabel.Font = new Font("Tahoma", 8F, FontStyle.Bold);
                m_pointsEarnedLabel.Text = "Earned:";
                m_pointsEarnedLabel.TextAlign = m_totalLabel.TextAlign;
                m_pointsEarned.Location = new Point(73, 539); // Rally TA7464
                m_pointsEarned.Size = new Size(111, 13);
                m_pointsEarned.Font = new Font("Tahoma", 8F);
                //------------------------------

                m_quantitySaleInfo.Location = new Point(30, 555);
                m_quantitySaleInfo.Font = new Font("Tahoma", 10F);
                m_quantitySaleInfo.TextAlign = ContentAlignment.MiddleCenter;

                m_versionLabel.Location = new Point(178, 583);
                m_versionLabel.Size = new Size(80, 10);
                m_versionLabel.Font = new Font("Tahoma", 7F);

                // Rally TA7465
                m_currencyButton.Location = new Point(265, 557);
                m_currencyButton.Size = new Size(55, 30);

                m_menuButtonsPanel.Location = new Point(344, 10);
                m_menuButtonsPanel.Size = new Size(447, 248);

                m_pageNavigator.Location = new Point(344, 263);
                m_pageNavigator.PrevNextButtonSize = new Size(50, 50);
                m_pageNavigator.PageButtonSize = new Size(50, 50);
                m_pageNavigator.ButtonSpacing = 5;
                m_pageNavigator.NumberOfButtons = 1;

                m_buttonMenu.Location = new Point(344, 328);
                m_buttonMenu.ButtonSize = new Size(79, 60);
                m_buttonMenu.IsFullSized = false;
                m_buttonMenu.ButtonSpacingWidth = 3;
                m_buttonMenu.ButtonSpacingHeight = 4;
                m_buttonMenu.Font = new Font("Tahoma", 8F, FontStyle.Bold);

                m_keypad.Location = new Point(519, 274);

                //              m_exitButton.ImageNormal = Resources.SmallExitUp;
                //              m_exitButton.ImagePressed = Resources.SmallExitDown;
                m_exitButton.ImageNormal = Resources.RedButtonUp;
                m_exitButton.ImagePressed = Resources.RedButtonDown;
                m_exitButton.Stretch = true;
                m_exitButton.Location = new Point(344, 561);
                m_exitButton.Size = new Size(56, 30);

                m_loginNameLabel.Location = new Point(344, 529);
                m_loginNameLabel.Size = new Size(161, 30);

                m_systemTimeLabel.Location = new Point(402, 561);
                m_systemTimeLabel.Size = new Size(103, 30);
                m_systemTimeLabel.Font = new Font("Tahoma", 8F, FontStyle.Bold);

                m_panelLastTotal.Location = m_orderSubtotalLabel.Location;
                m_panelLastTotal.Size = new System.Drawing.Size(310, 80);
                m_lblLastTotalTitle.Font = m_orderSubtotalLabel.Font;
                m_lblLastTotal.Font = m_total.Font;
                m_lblLastTotal.Location = new Point(m_lblLastTotalTitle.Location.X + 75, m_lblLastTotal.Location.Y);
                m_lblLastTotal.TextAlign = ContentAlignment.TopCenter;

                m_keypadPaddingX = SmallPaddingX;
                m_keypadPaddingY = SmallPaddingY;
            }

            if(m_displayMode is WideDisplayMode)
            {
                m_panelMain.Size = m_displayMode.WideFormSize;
                m_panelMain.Location = new Point(m_displayMode.OffsetForFullScreenX, m_displayMode.OffsetForFullScreenY);

                m_noticeLabel = new Label();
                m_noticeLabel.Parent = this.m_panelMain;
                m_noticeLabel.Size = m_buttonMenu.Size;
                m_noticeLabel.Location = m_buttonMenu.Location;
                m_noticeLabel.BackColor = Color.Transparent;
                m_noticeLabel.ForeColor = Color.Gold;
                m_noticeLabel.Font = m_menuList.Font;
                m_noticeLabel.AutoSize = false;
                m_noticeLabel.Show();
                m_noticeLabel.Click += this.UserActivityDetected;

                m_panelMain.BackgroundImage = Resources.POSBackWide;
                m_buttonMenu.Location = new Point(742, 442);
                m_buttonMenu.IsGiantSized = true;
                m_buttonMenu.ButtonSize = new Size(m_buttonMenu.ButtonSize.Width + 14, m_buttonMenu.ButtonSize.Height);

                m_keypad.Location = new Point(m_keypad.Location.X + m_displayMode.WidthIncreaseFromNormal, m_keypad.Location.Y + m_displayMode.HeightIncreaseFromNormal);

                int buttonColumns = m_displayMode.MenuButtonsPerPage / m_displayMode.MenuButtonsPerColumn;

                m_menuButtonsPanel.Size = new Size(buttonColumns * m_displayMode.MenuButtonSize.Width + (buttonColumns - 1) * m_displayMode.MenuButtonXSpacing, m_menuButtonsPanel.Size.Height);
            }
        }

        /// <summary>
        /// Raises the Shown event.
        /// </summary>
        /// <param name="e">An EventArgs object that contains the 
        /// event data.</param>
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            try
            {
                m_parent.DisposeLoadingForm();
                m_parent.CanUpdateMenus = true; // PDTS 964

                SetForegroundWindow(this.Handle);

                if (m_parent.WeAreAPOSKiosk)
                    HandleCrashedSale();

                if (m_parent.WeAreNotAPOSKiosk && m_parent.CashDrawerIsOpen(true))
                    POSMessageForm.Show(this, m_parent, Resources.PleaseCloseTheCashDrawer, POSMessageFormTypes.CloseWithCashDrawer);

                if (m_parent.WeAreANonAdvancedPOSKiosk)
                {
                    KioskForm.Activate();
                    KioskForm.StartOver();
                }
                else
                {
                    m_parent.MagCardReader.BeginReading(); // PDTS 1064
                    Activate();
                }
            }
            catch (Exception)
            {
            }
        }

        private void HandleCrashedSale()
        {
            bool reprintReceipt = true;

            NotIdle(true);

            //if we had a crash durring a sale (bingo or B3), see if it was long enough ago to require an attendant 
            if (m_userDecimalNVRamStartup[(int)(NVRAMUserDecimal.RegisterReceiptID)] != 0 ||
                (m_userDecimalNVRamStartup[(int)(NVRAMUserDecimal.AmountCollected)] != 0 && m_userDecimalNVRamStartup[(int)(NVRAMUserDecimal.AmountCollected)] != m_userDecimalNVRamStartup[(int)(NVRAMUserDecimal.AmountDispensed)]))
            {
                if (m_userDecimalNVRamStartup[(int)NVRAMUserDecimal.WriteTimeStamp] != 0)
                {
                    int minutesSinceLastNVWriteBeforeCrash = (DateTime.Now - new DateTime((long)m_userDecimalNVRamStartup[(int)NVRAMUserDecimal.WriteTimeStamp])).Minutes;

                    //see if we need to notify the Guardian to get an attendant
                    if (minutesSinceLastNVWriteBeforeCrash >= m_parent.Settings.KioskCrashRecoveryNeedAttendantAfterNMinutes)
                        m_parent.RequestHelpFromGuardian(string.Format("Crash recovery requiers attendant to start.  Crash was more than {0} minute{1} ago.", minutesSinceLastNVWriteBeforeCrash, minutesSinceLastNVWriteBeforeCrash == 1?"":"s"));
                }
            }

            //see if we need to clean up after a failure
            if (m_userDecimalNVRamStartup[(int)(NVRAMUserDecimal.RegisterReceiptID)] != 0) //something didn't finish in a bingo sale
            {
                if (m_userDecimalNVRamStartup[(int)(NVRAMUserDecimal.TransactionTotal)] != 0) //a session bingo sale was started and not completed
                {
                    if (m_userDecimalNVRamStartup[(int)(NVRAMUserDecimal.SaleSucceeded)] == 0) //incomplete sale (need to return money)
                    {
                        //return money based on tenders
                        decimal cashAmountToRefundAtKiosk = m_parent.StartVoidTendersAndReprintReceipt((int)m_userDecimalNVRamStartup[(int)(NVRAMUserDecimal.RegisterReceiptID)]);

                        cashAmountToRefundAtKiosk += m_userDecimalNVRamStartup[(int)(NVRAMUserDecimal.CashToDispense)] - m_userDecimalNVRamStartup[(int)(NVRAMUserDecimal.CashDispensed)];
                        reprintReceipt = false;

                        if (cashAmountToRefundAtKiosk != 0) //already refunded, just need to dispense it
                        {
                            decimal amountNotVended = VendChange(cashAmountToRefundAtKiosk);

                            if (amountNotVended != 0) //not everything vended
                                m_parent.RequestHelpFromGuardian("Crash recovery - Could not dispense " + amountNotVended.ToString("C") + ".");
                        }
                    }
                    else //sale succeeded
                    {
                        //make sure all change was given
                        decimal changeStillDue = m_userDecimalNVRamStartup[(int)(NVRAMUserDecimal.AmountCollected)] - m_userDecimalNVRamStartup[(int)(NVRAMUserDecimal.TransactionTotal)] - m_userDecimalNVRamStartup[(int)(NVRAMUserDecimal.AmountDispensed)];

                        if (changeStillDue >= .01M) //give rest of change
                        {
                            decimal stillDue = VendChange(changeStillDue);

                            if (stillDue >= .01M) //need help
                            {
                                m_parent.RequestHelpFromGuardian("Crash recovery - Could not dispense " + stillDue.ToString("C") + ".");
                            }
                        }
                    }
                }

                if(reprintReceipt)
                    m_parent.StartReprintReceipt((int)m_userDecimalNVRamStartup[(int)(NVRAMUserDecimal.RegisterReceiptID)]);
    
                ClearNVRAMTransactionUserDecimals();
            }
            else if (m_userDecimalNVRamStartup[(int)(NVRAMUserDecimal.AmountCollected)] != 0 && m_userDecimalNVRamStartup[(int)(NVRAMUserDecimal.AmountCollected)] != m_userDecimalNVRamStartup[(int)(NVRAMUserDecimal.AmountDispensed)]) //B3 sale failed
            {
                decimal refundStillDue = m_userDecimalNVRamStartup[(int)(NVRAMUserDecimal.AmountCollected)] - m_userDecimalNVRamStartup[(int)(NVRAMUserDecimal.AmountDispensed)];

                if (refundStillDue >= .01M)
                {
                    //see if we can give B3 credit to finish the sale
                    if (m_parent.B3SessionActive)
                    {
                        m_parent.B3AddCredit(refundStillDue);
                        m_parent.WaitForSaleStatusFormSecondaryThread();

                        if (!CheckForError()) //no error, we dispensed it
                            refundStillDue = 0;
                    }

                    if (refundStillDue >= .01M) //we need to refund the money (but not through B3)
                        refundStillDue = VendChange(refundStillDue, true);

                    if (refundStillDue >= .01M) //need help
                    {
                        m_parent.RequestHelpFromGuardian("Crash recovery - Could not dispense " + refundStillDue.ToString("C") + ".");
                    }
                }

                ClearNVRAMTransactionUserDecimals();
            }

            NotIdle();
        }

        // Rally DE2260 - POS does not allow Travelers and traveler II at the same time.
        // US2018 - Device Fee rework.
        /// <summary>
        /// Shows/hides the device buttons based on device fees and which 
        /// devices can be sold to.
        /// </summary>
        protected void ConfigureTotalButtons()
        {
            // Clear the values.
            m_keypad.Option1Visible = false;
            m_keypad.Option2Visible = false;
            m_keypad.Option3Visible = false;
            m_keypad.Option4Visible = false;

            m_keypad.Option1Tag = -1;
            m_keypad.Option2Tag = -1;
            m_keypad.Option3Tag = -1;
            m_keypad.Option4Tag = -1;

            // Create the information about the buttons.
            Dictionary<int, DeviceButtonEntry> deviceButtons = new Dictionary<int, DeviceButtonEntry>();

            deviceButtons.Add(PackDeviceId, new DeviceButtonEntry());

            if (!m_parent.Settings.MainStageMode)
            {
                deviceButtons[PackDeviceId].Text = Resources.PackButtonText;
            }
            else
            {
                if (m_parent.WeAreAPOSKiosk)
                {
                    deviceButtons[PackDeviceId].Text = Resources.SellingFormPay;
                    deviceButtons[PackDeviceId].NormalImage = Resources.GreenButtonUp;
                }
                else
                {
                    deviceButtons[PackDeviceId].Text = Resources.SellingFormTotal;
                    deviceButtons[PackDeviceId].NormalImage = Resources.BlueButtonUp;
                }
            }

            deviceButtons[PackDeviceId].Handler = new EventHandler(PackButtonClick);
            deviceButtons[PackDeviceId].HandlerIfHeld = new EventHandler(PackButtonHeld);
            deviceButtons[PackDeviceId].Id = PackDeviceId;

            deviceButtons.Add(Device.Traveler.Id, new DeviceButtonEntry());
            deviceButtons[Device.Traveler.Id].Handler = new EventHandler(TravelerButtonClick);
            deviceButtons[Device.Traveler.Id].HandlerIfHeld = new EventHandler(TravelerButtonHeld);
            deviceButtons[Device.Traveler.Id].Id = Device.Traveler.Id;

            deviceButtons.Add(Device.Tracker.Id, new DeviceButtonEntry());
            deviceButtons[Device.Tracker.Id].Handler = new EventHandler(TrackerButtonClick);
            deviceButtons[Device.Tracker.Id].HandlerIfHeld = new EventHandler(TrackerButtonHeld);
            deviceButtons[Device.Tracker.Id].Id = Device.Tracker.Id;

            deviceButtons.Add(Device.Fixed.Id, new DeviceButtonEntry());
            deviceButtons[Device.Fixed.Id].Handler = new EventHandler(FixedButtonClick);
            deviceButtons[Device.Fixed.Id].HandlerIfHeld = new EventHandler(FixedButtonHeld);
            deviceButtons[Device.Fixed.Id].Id = Device.Fixed.Id;

            // Rally TA7729
            deviceButtons.Add(Device.Explorer.Id, new DeviceButtonEntry());
            deviceButtons[Device.Explorer.Id].Handler = new EventHandler(ExplorerButtonClick);
            deviceButtons[Device.Explorer.Id].HandlerIfHeld = new EventHandler(ExplorerButtonHeld);
            deviceButtons[Device.Explorer.Id].Id = Device.Explorer.Id;

            deviceButtons.Add(Device.Traveler2.Id, new DeviceButtonEntry());
            deviceButtons[Device.Traveler2.Id].Handler = new EventHandler(Traveler2ButtonClick);
            deviceButtons[Device.Traveler2.Id].HandlerIfHeld = new EventHandler(Traveler2ButtonHeld);
            deviceButtons[Device.Traveler2.Id].Id = Device.Traveler2.Id;

            //US2908
            deviceButtons.Add(Device.Tablet.Id, new DeviceButtonEntry());
            deviceButtons[Device.Tablet.Id].Handler = new EventHandler(TabletButtonClick);
            deviceButtons[Device.Tablet.Id].HandlerIfHeld = new EventHandler(TabletButtonHeld);
            deviceButtons[Device.Tablet.Id].Id = Device.Tablet.Id;

            int buttonCount = 0, currentButton = 1;

            m_allDeviceFeesAreZero = m_parent.CurrentOperator.TravelerDeviceFee == 0M &&
                                     m_parent.CurrentOperator.TrackerDeviceFee == 0M &&
                                     m_parent.CurrentOperator.FixedDeviceFee == 0M &&
                                     m_parent.CurrentOperator.ExplorerDeviceFee == 0M &&
                                     m_parent.CurrentOperator.Traveler2DeviceFee == 0M &&
                                     m_parent.CurrentOperator.TabletDeviceFee == 0M;

            // First, do we have device fees?
            if(m_parent.Settings.ForceDeviceSelectionWhenNoFees || !m_allDeviceFeesAreZero)
            {
                m_haveDeviceFeesToSelectFrom = true;

                // Since we have device fees we can't show the "Pack" button.
                // How many total buttons do we need to show?
                if(m_parent.Settings.HasTraveler)
                {
                    deviceButtons[Device.Traveler.Id].IsAvailable = true;
                    buttonCount++;
                }

                if(m_parent.Settings.HasTracker)
                {
                    deviceButtons[Device.Tracker.Id].IsAvailable = true;
                    buttonCount++;
                }

                if(m_parent.Settings.HasFixed)
                {
                    deviceButtons[Device.Fixed.Id].IsAvailable = true;
                    buttonCount++;
                }

                if(m_parent.Settings.HasExplorer)
                {
                    deviceButtons[Device.Explorer.Id].IsAvailable = true;
                    buttonCount++;
                }

                // We can only have a certain number of device buttons.
                if(m_parent.Settings.HasTraveler2 && (m_parent.Settings.UseSystemMenuForUnitSelection || buttonCount < MaxDeviceButtons))
                {
                    deviceButtons[Device.Traveler2.Id].IsAvailable = true;
                    buttonCount++;
                }
                //US2908
                if (m_parent.Settings.HasTablet && (m_parent.Settings.UseSystemMenuForUnitSelection || buttonCount < MaxDeviceButtons))
                {
                    deviceButtons[Device.Tablet.Id].IsAvailable = true;
                    buttonCount++;
                }

                 
                // Which buttons can we show?
                if(buttonCount > 0)
                {
                    if(buttonCount == 1)
                    {
                        deviceButtons[Device.Traveler.Id].NormalImage = Resources.DeviceTravelerUp271;
                        deviceButtons[Device.Traveler.Id].PressedImage = Resources.DeviceTravelerDown271;
                        deviceButtons[Device.Tracker.Id].NormalImage = Resources.DeviceTrackerUp271;
                        deviceButtons[Device.Tracker.Id].PressedImage = Resources.DeviceTrackerDown271;
                        deviceButtons[Device.Fixed.Id].NormalImage = Resources.DeviceFixedUp271;
                        deviceButtons[Device.Fixed.Id].PressedImage = Resources.DeviceFixedDown271;
                        deviceButtons[Device.Explorer.Id].NormalImage = Resources.DeviceExplorerUp271;
                        deviceButtons[Device.Explorer.Id].PressedImage = Resources.DeviceExplorerDown271;
                        deviceButtons[Device.Traveler2.Id].NormalImage = Resources.DeviceTraveler2Up271;
                        deviceButtons[Device.Traveler2.Id].PressedImage = Resources.DeviceTraveler2Down271;
                        //US2908
                        deviceButtons[Device.Tablet.Id].NormalImage = Resources.TEDE271;  
                        deviceButtons[Device.Tablet.Id].PressedImage = Resources.TEDE271_down;
                        
                    }
                    else if(buttonCount == 2)
                    {
                        deviceButtons[Device.Traveler.Id].NormalImage = Resources.DeviceTravelerUp133;
                        deviceButtons[Device.Traveler.Id].PressedImage = Resources.DeviceTravelerDown133;
                        deviceButtons[Device.Tracker.Id].NormalImage = Resources.DeviceTrackerUp133;
                        deviceButtons[Device.Tracker.Id].PressedImage = Resources.DeviceTrackerDown133;
                        deviceButtons[Device.Fixed.Id].NormalImage = Resources.DeviceFixedUp133;
                        deviceButtons[Device.Fixed.Id].PressedImage = Resources.DeviceFixedDown133;
                        deviceButtons[Device.Explorer.Id].NormalImage = Resources.DeviceExplorerUp133;
                        deviceButtons[Device.Explorer.Id].PressedImage = Resources.DeviceExplorerDown133;
                        deviceButtons[Device.Traveler2.Id].NormalImage =  Resources.DeviceTraveler2Up133;
                        deviceButtons[Device.Traveler2.Id].PressedImage = Resources.DeviceTraveler2Down133;
                        //US2908
                        deviceButtons[Device.Tablet.Id].NormalImage = Resources.TEDE133;
                        deviceButtons[Device.Tablet.Id].PressedImage = Resources.TEDE133_down; 

                    }
                    else if(buttonCount == 3)
                    {
                        deviceButtons[Device.Traveler.Id].NormalImage = Resources.DeviceTravelerUp87;
                        deviceButtons[Device.Traveler.Id].PressedImage = Resources.DeviceTravelerDown87;
                        deviceButtons[Device.Tracker.Id].NormalImage = Resources.DeviceTrackerUp87;
                        deviceButtons[Device.Tracker.Id].PressedImage = Resources.DeviceTrackerDown87;
                        deviceButtons[Device.Fixed.Id].NormalImage = Resources.DeviceFixedUp87;
                        deviceButtons[Device.Fixed.Id].PressedImage = Resources.DeviceFixedDown87;
                        deviceButtons[Device.Explorer.Id].NormalImage = Resources.DeviceExplorerUp87;
                        deviceButtons[Device.Explorer.Id].PressedImage = Resources.DeviceExplorerDown87;
                        deviceButtons[Device.Traveler2.Id].NormalImage = Resources.DeviceTraveler2Up87;
                        deviceButtons[Device.Traveler2.Id].PressedImage = Resources.DeviceTraveler2Down87;
                        //US2908
                        deviceButtons[Device.Tablet.Id].NormalImage = Resources.TEDE87;
                        deviceButtons[Device.Tablet.Id].PressedImage = Resources.TEDE87_down;
                    }
                    else if(buttonCount == 4)
                    {
                        deviceButtons[Device.Traveler.Id].NormalImage = Resources.DeviceTravelerUp64;
                        deviceButtons[Device.Traveler.Id].PressedImage = Resources.DeviceTravelerDown64;
                        deviceButtons[Device.Tracker.Id].NormalImage = Resources.DeviceTrackerUp64;
                        deviceButtons[Device.Tracker.Id].PressedImage = Resources.DeviceTrackerDown64;
                        deviceButtons[Device.Fixed.Id].NormalImage = Resources.DeviceFixedUp64;
                        deviceButtons[Device.Fixed.Id].PressedImage = Resources.DeviceFixedDown64;
                        deviceButtons[Device.Explorer.Id].NormalImage = Resources.DeviceExplorerUp64;
                        deviceButtons[Device.Explorer.Id].PressedImage = Resources.DeviceExplorerDown64;
                        deviceButtons[Device.Traveler2.Id].NormalImage = Resources.DeviceTraveler2Up64;
                        deviceButtons[Device.Traveler2.Id].PressedImage = Resources.DeviceTraveler2Down64;
                        //US2908
                        deviceButtons[Device.Tablet.Id].NormalImage = Resources.TEDE64;
                        deviceButtons[Device.Tablet.Id].PressedImage = Resources.TEDE64_down; 
                    }

                    foreach(KeyValuePair<int, DeviceButtonEntry> device in deviceButtons)
                    {
                        if(currentButton > MaxDeviceButtons)
                            break;

                        if(device.Value.IsAvailable)
                        {
                            if(currentButton == 1)
                            {
                                m_keypad.Option1Visible = true;
                                m_keypad.Option1Text = device.Value.Text;
                                m_keypad.Option1Tag = device.Key;
                                m_keypad.Option1ButtonClick += device.Value.Handler;
                                m_keypad.Option1ButtonHeld += device.Value.HandlerIfHeld;
                                m_keypad.Option1ImageNormal = device.Value.NormalImage;
                                m_keypad.Option1ImagePressed = device.Value.PressedImage;
                               
                            }
                            else if(currentButton == 2)
                            {
                                m_keypad.Option2Visible = true;
                                m_keypad.Option2Text = device.Value.Text;
                                m_keypad.Option2Tag = device.Key;
                                m_keypad.Option2ButtonClick += device.Value.Handler;
                                m_keypad.Option2ButtonHeld += device.Value.HandlerIfHeld;
                                m_keypad.Option2ImageNormal = device.Value.NormalImage;
                                m_keypad.Option2ImagePressed = device.Value.PressedImage;
                            }
                            else if(currentButton == 3)
                            {
                                m_keypad.Option3Visible = true;
                                m_keypad.Option3Text = device.Value.Text;
                                m_keypad.Option3Tag = device.Key;
                                m_keypad.Option3ButtonClick += device.Value.Handler;
                                m_keypad.Option3ButtonHeld += device.Value.HandlerIfHeld;
                                m_keypad.Option3ImageNormal = device.Value.NormalImage;
                                m_keypad.Option3ImagePressed = device.Value.PressedImage;
                            }
                            else if(currentButton == 4)
                            {
                                m_keypad.Option4Visible = true;
                                m_keypad.Option4Text = device.Value.Text;
                                m_keypad.Option4Tag = device.Key;
                                m_keypad.Option4ButtonClick += device.Value.Handler;
                                m_keypad.Option4ButtonHeld += device.Value.HandlerIfHeld;
                                m_keypad.Option4ImageNormal = device.Value.NormalImage;
                                m_keypad.Option4ImagePressed = device.Value.PressedImage;
                            }
                               
                            currentButton++;
                        }
                    }
                }

                // Make the text in the upper, left corner.
                m_keypad.OptionButtonsAlignment = StringAlignment.Near;
                m_keypad.OptionButtonsLineAlignment = StringAlignment.Near;
            }
            else
            {
                m_haveDeviceFeesToSelectFrom = false;

                // If we don't have device fees, then we show only 3 buttons 
                // at max.
                if(m_parent.Settings.HasTraveler)
                {
                    deviceButtons[Device.Traveler.Id].IsAvailable = true;
                    buttonCount++;
                }

                if(m_parent.Settings.HasTracker)
                {
                    deviceButtons[Device.Tracker.Id].IsAvailable = true;
                    buttonCount++;
                }

                if(m_parent.Settings.HasFixed || m_parent.Settings.HasExplorer || m_parent.Settings.HasTraveler2 || /*US2908*/m_parent.Settings.HasTablet)
                {
                    deviceButtons[PackDeviceId].IsAvailable = true;

                    m_keypad.OptionButtonsFont = PackFont;
                    buttonCount++;
                }
                // END: TA7729

                // Which buttons can we show?
                if(buttonCount > 0)
                {
                    if(buttonCount == 1)
                    {
                        deviceButtons[Device.Traveler.Id].NormalImage = Resources.DeviceTravelerUp271;
                        deviceButtons[Device.Traveler.Id].PressedImage = Resources.DeviceTravelerDown271;
                        deviceButtons[Device.Tracker.Id].NormalImage = Resources.DeviceTrackerUp271;
                        deviceButtons[Device.Tracker.Id].PressedImage = Resources.DeviceTrackerDown271;
                        deviceButtons[PackDeviceId].NormalImage = Resources.DevicePackUp271;
                        deviceButtons[PackDeviceId].PressedImage = Resources.DevicePackDown271;
                    }
                    else if(buttonCount == 2)
                    {
                        deviceButtons[Device.Traveler.Id].NormalImage = Resources.DeviceTravelerUp133;
                        deviceButtons[Device.Traveler.Id].PressedImage = Resources.DeviceTravelerDown133;
                        deviceButtons[Device.Tracker.Id].NormalImage = Resources.DeviceTrackerUp133;
                        deviceButtons[Device.Tracker.Id].PressedImage = Resources.DeviceTrackerDown133;
                        deviceButtons[PackDeviceId].NormalImage = Resources.DevicePackUp133;
                        deviceButtons[PackDeviceId].PressedImage = Resources.DevicePackDown133;
                    }
                    else if(buttonCount == 3)
                    {
                        deviceButtons[Device.Traveler.Id].NormalImage = Resources.DeviceTravelerUp87;
                        deviceButtons[Device.Traveler.Id].PressedImage = Resources.DeviceTravelerDown87;
                        deviceButtons[Device.Tracker.Id].NormalImage = Resources.DeviceTrackerUp87;
                        deviceButtons[Device.Tracker.Id].PressedImage = Resources.DeviceTrackerDown87;
                        deviceButtons[PackDeviceId].NormalImage = Resources.DevicePackUp87;
                        deviceButtons[PackDeviceId].PressedImage = Resources.DevicePackDown87;
                    }

                    foreach(KeyValuePair<int, DeviceButtonEntry> device in deviceButtons)
                    {
                        if(currentButton > MaxDeviceButtons - 1)
                            break;

                        if(device.Value.IsAvailable)
                        {
                            if(currentButton == 1)
                            {
                                m_keypad.Option1Visible = true;
                                m_keypad.Option1Text = device.Value.Text;
                                m_keypad.Option1Tag = device.Key;
                                m_keypad.Option1ButtonClick += device.Value.Handler;
                                m_keypad.Option1ImageNormal = device.Value.NormalImage;
                                m_keypad.Option1ImagePressed = device.Value.PressedImage;
                            }
                            else if(currentButton == 2)
                            {
                                m_keypad.Option2Visible = true;
                                m_keypad.Option2Text = device.Value.Text;
                                m_keypad.Option2Tag = device.Key;
                                m_keypad.Option2ButtonClick += device.Value.Handler;
                                m_keypad.Option2ImageNormal = device.Value.NormalImage;
                                m_keypad.Option2ImagePressed = device.Value.PressedImage;
                            }
                            else if(currentButton == 3)
                            {
                                m_keypad.Option3Visible = true;
                                m_keypad.Option3Text = device.Value.Text;
                                m_keypad.Option3Tag = device.Key;
                                m_keypad.Option3ButtonClick += device.Value.Handler;
                                m_keypad.Option3ImageNormal = device.Value.NormalImage;
                                m_keypad.Option3ImagePressed = device.Value.PressedImage;
                            }

                            currentButton++;
                        }
                    }
                }
            }

            if (m_parent.Settings.UseSystemMenuForUnitSelection)
            {
                // Clear the values.
                m_keypad.Option1Visible = false;
                m_keypad.Option2Visible = false;
                m_keypad.Option3Visible = false;
                m_keypad.Option4Visible = false;

                m_keypad.Option1Tag = -1;
                m_keypad.Option2Tag = -1;
                m_keypad.Option3Tag = -1;
                m_keypad.Option4Tag = -1;

                foreach (KeyValuePair<int, DeviceButtonEntry> dev in deviceButtons)
                {
                    if (dev.Value.IsAvailable)
                        m_availableDevices.Add(Device.FromId(dev.Value.Id));
                }

                //if we don't have a default device that is in the list, set the default device to the first one in our list
                if (!m_availableDevices.Exists(d => d.Id == m_parent.Settings.DefaultElectronicDeviceID) &&
                    m_availableDevices.Count > 0) //DE13359
                {
                    m_parent.Settings.DefaultElectronicDeviceID = m_availableDevices[0].Id;
                }
            }
            else
            {
                PutDeviceFeesOnDeviceButtons();
            }
        }
         
        /// <summary>
        /// Creates the POS menu buttons at the bottom of the screen.
        /// </summary>
        /// <exception cref="GTI.Modules.POS.Business.POSException">The form's 
        /// display mode is invalid.</exception>
        protected void CreateSystemMenu()
        {
            // Figure out which system buttons to display.
            List<ButtonEntry> systemButtons = new List<ButtonEntry>();
      
//Returns
            if(m_parent.WeAreNotAPOSKiosk && m_parent.Settings.AllowReturns)
                systemButtons.Add(new ButtonEntry(ReturnId, Resources.ReturnButtonText, true, ReturnButtonClick));

//Validations
            //US3509
            if (m_parent.Settings.EnabledProductValidation)
            {
                m_ValidationbuttonEntry = new ButtonEntry(ValidateId, Resources.ValidationEnabledString, true, ValidationClick);

                if (m_parent.WeAreAPOSKiosk)
                {
                    m_ValidationbuttonEntry.CustomButtonUpImage = Resources.GrayButtonUp;
                    m_ValidationbuttonEntry.CustomButtonDownImage = Resources.GrayButtonDown;
                }

                systemButtons.Add(m_ValidationbuttonEntry);
            }

//Unit selection
            if (m_parent.Settings.UseSystemMenuForUnitSelection && m_availableDevices.Count > 1)
            {
                ButtonEntry unitSelectionButton = new ButtonEntry(UnitSelectId, Resources.UnitSelectionButtonText, false, UnitSelectionButton, null);

                unitSelectionButton.CustomTextHorizontalAlignment = StringAlignment.Near;
                unitSelectionButton.CustomTextVerticalAlignment = StringAlignment.Near;
                unitSelectionButton.UseCustomAlignment = false;
                unitSelectionButton.OutlinedText = true;
                unitSelectionButton.HeldHandler = ToggleAddOnSale;
                systemButtons.Add(unitSelectionButton);
            }

//Player management
            // TTP 50114
            // Army Release
            // PDTS 584
            // TODO Allow Compact display mode later.
            if (m_parent.WeAreNotAPOSKiosk && !(m_displayMode is CompactDisplayMode) && !m_parent.Settings.EnableAnonymousMachineAccounts && m_parent.IsPlayerCenterInitialized)
                systemButtons.Add(new ButtonEntry(PlayerMgmtId, Resources.ManagePlayersButtonText, true, PlayerMgmtClick));

//Coupons
            if (m_parent.Settings.isCouponManagement)
            {
                ButtonEntry couponButton = new ButtonEntry(PlayerCouponsId, Resources.PlayerCouponButtonText, false, PlayerCouponsClick);
                couponButton.CustomButtonUpImage = Resources.GreenButtonUp;
                couponButton.CustomButtonDownImage = Resources.GreenButtonDown;

                systemButtons.Add(couponButton);
            }

//Repeat last sale
            if(m_parent.WeAreNotAPOSKiosk)
                systemButtons.Add(new ButtonEntry(RepeatLastSaleId, Resources.RepeatLastSaleButtonText, true, RepeatLastSaleClick));
            
//Reprint last sale
            if (m_parent.WeAreNotAPOSKiosk)
                systemButtons.Add(new ButtonEntry(ReprintLastReceiptId, Resources.PrintLastReceiptButtonText, true, ReprintLastReceiptClick));

//Repeat player's last sale
            systemButtons.Add(new ButtonEntry(ReprintPlayerLastReceiptId, m_parent.WeAreAPOSKiosk?Resources.RepeatLastPurchase:Resources.RepeatPlayerLastSaleButtonText, true, RepeatPlayerLastSale));

//Enter player card#
            systemButtons.Add(new ButtonEntry(ScanCardId, m_parent.Settings.EnableAnonymousMachineAccounts? Resources.ScanMachine : Resources.ScanPlayerCard, true, ScanPlayerCardClick));

//Quantity sale
            // PDTS 571
            if (m_parent.WeAreNotAPOSKiosk && m_parent.Settings.AllowQuantitySale && !m_parent.Settings.PlayWithPaper) // Rally TA5748, TA7465
                systemButtons.Add(new ButtonEntry(QuantitySaleId, Resources.QuantitySaleButtonText, true, QuantitySaleClick));

//Credit cash out
            // Rally TA7897
            if(m_parent.WeAreNotAPOSKiosk && m_parent.Settings.CreditEnabled && m_parent.CurrentStaff.CheckModuleFeature(EliteModule.POS, (int)POSFeature.CreditCashOut))
                systemButtons.Add(new ButtonEntry(CreditCashOutId, Resources.CreditCashOutButtonText, false, CreditCashOutClick));

//Unit assignment
            if (m_parent.WeAreNotAPOSKiosk && m_parent.Settings.EnableUnitAssignment && m_parent.IsUnitMgmtInitialized)
                systemButtons.Add(new ButtonEntry(UnitAssignmentId, Resources.UnitAssignmentText, false, UnitAssignmentClick));

//Void receipts (if I can't void, I will need this button to get authorization)
            if (m_parent.WeAreNotAPOSKiosk && m_parent.IsReceiptMgmtInitialized && (!m_parent.CurrentStaff.CheckModuleFeature(EliteModule.ReceiptManagement, (int)ReceiptManagementFeature.VoidSale) || m_parent.Settings.ForceAuthorizationOnVoidsAtPOS))
                systemButtons.Add(new ButtonEntry(VoidReceiptsId, Resources.VoidReceiptsButtonText, true, VoidReceiptsClick));

//View receipts
            if (m_parent.WeAreNotAPOSKiosk && m_parent.IsReceiptMgmtInitialized && m_parent.CurrentStaff.CheckModule(EliteModule.ReceiptManagement))
                systemButtons.Add(new ButtonEntry(ViewReceiptsId, Resources.ViewReceiptsButtonText, true, ViewReceiptsClick));

//Adjust bank
            // TTP 50137
            // Rally TA7465
            if (m_parent.WeAreNotAPOSKiosk && m_parent.CurrentOperator.CashMethodID != (int)CashMethod.ByStaffMoneyCenter)
                systemButtons.Add(new ButtonEntry(AdjustBankId, Resources.AdjustBankButtonText, true, AdjustBankClick));

//Register Sales report
            // Rally US1650
            if (m_parent.WeAreNotAPOSKiosk && m_parent.Settings.EnableRegisterSalesReport)
                systemButtons.Add(new ButtonEntry(RegisterSalesReportId, Resources.RegisterReportButtonText, true, RegisterSalesReportClick));

//Register Closing Report 
            //US5115: POS: Add Register Closing report button
            if (m_parent.WeAreNotAPOSKiosk && m_parent.Settings.EnableRegisterClosingReport)
                systemButtons.Add(new ButtonEntry(RegisterClosingReportId, Resources.RegisterClosingReportButtonText, true, RegisterClosingReportClick));

//Paper Usage
//Paper exchange
//Paper Range Scanner
            if (m_parent.WeAreNotAPOSKiosk && m_parent.Settings.EnablePaperUsage)
			{
                systemButtons.Add(new ButtonEntry(PaperUsageId, Resources.Paper_Usage_Button_Text, true, PaperUsageClick));
                systemButtons.Add(new ButtonEntry(PaperExchangeId, Resources.PaperExchangeString, true, PaperExchangeClick));
                systemButtons.Add(new ButtonEntry(PaperRangeScannerId, Resources.PaperRangeScannerButtonText, true, PaperRangeScannerClick)); // US5347
            }

//Close bank            
            if (m_parent.WeAreNotAPOSKiosk && m_parent.CurrentOperator.CashMethodID != (int)CashMethod.ByStaffMoneyCenter)
                systemButtons.Add(new ButtonEntry(CloseBankId, Resources.CloseBankButtonText, true, CloseBankClick)); // FIX: DE1930
            // END: TA7465
                        
            int page = 0, position = 0;

            m_buttonMenu.SetSinglePageFromItemCount(systemButtons.Count);

            int buttonsPerPage = m_buttonMenu.ButtonsPerPage;

            // Create the system menu based on display mode.
            foreach(ButtonEntry entry in systemButtons)
            {
                m_buttonMenu.AddButton(page, position, entry);

                if (++position >= buttonsPerPage)
                {
                    page++;
                    position = 0;
                }
            }

            if (m_parent.WeAreAPOSKiosk)
                m_buttonMenu.UseClickSound = m_parent.Settings.UseKeyClickSoundsOnKiosk;
        }

        /// <summary>
        /// Inventory click event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void PaperUsageClick(object sender, EventArgs eventArgs)
        {
            var paperUsageForm = new PaperUsageForm(this, m_parent, m_version, BankOpenType.Open);
            paperUsageForm.ShowDialog(this);
            
            //US4962 have to close bank with paper usage. Including backing out if something goes wrong
            if (paperUsageForm.IsPaperClosed)
            {
                // bank close happened in paper usage close. Follow bank close procedures

                //US5241: POS: When cash method is staff money center do not close the bank when closing paper
                if (m_parent.CurrentOperator.CashMethodID != (int) CashMethod.ByStaffMoneyCenter)
                {
                    m_parent.StartCloseBank();
                    m_parent.ShowWaitForm(this); // Block until we are done.

                    if (!CheckForError())
                    {
                        //we only want to print receipt if not auto issue
                        if (m_parent.Settings.PrintRegisterClosingOnBankClose)
                        {
                            m_parent.StartPrintRegisterReport(true); // Rally US1648
                            m_parent.ShowWaitForm(this); // Block until we are done.
                        }

                        CheckForError();

                        Close();
                    }
                
                }
                
            }

        }

        //US3512: Manual exchange
        /// <summary>
        /// Paper exchange event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void PaperExchangeClick(object sender, EventArgs eventArgs)
        {
            m_parent.ShowPaperExchange();
        }

        /// <summary>
        /// Updates the operator label with the current operator.
        /// </summary>
        public void SetOperator()
        {
            m_operatorLabel.Text = m_parent.CurrentOperator.Name;
        }

        /// <summary>
        /// Updates the gaming date label with the current gaming date.
        /// </summary>
        public void SetGamingDate(DateTime? gamingDate = null)
        {
            if (gamingDate == null)
                m_gamingDateLabel.Text = string.Format(CultureInfo.CurrentCulture, Resources.SellingFormGamingDate, m_parent.GamingDate.ToShortDateString());
            else
                m_gamingDateLabel.Text = string.Format(CultureInfo.CurrentCulture, Resources.SellingFormGamingDate, gamingDate.Value.ToShortDateString());
        }

        /// <summary>
        /// Updates the staff label with the current staff.
        /// </summary>
        public void SetStaff()
        {
            m_loginNameLabel.Text = m_parent.CurrentStaff.FirstName + " " + m_parent.CurrentStaff.LastName;
        }

        /// <summary>
        /// Sets the keypad's value to 0.
        /// </summary>
        protected void ClearKeypad()
        {
            m_keypad.Clear();
        }

        /// <summary>
        /// Sets the image of the selected device. 
        /// </summary>
        /// <param name="devPic">No arg or null hides the picture.</param>
        public void SetSelectedDeviceName(int deviceID = 0)
        {
            if (deviceID == 0) //no device
            {
                m_selectedDeviceName.Visible = false;

                if (m_parent.Settings.UseSystemMenuForUnitSelection)
                {
                    ButtonEntry be = m_buttonMenu.GetButtonEntry(UnitSelectId);
                    
                    if (be != null)
                    {
                        if (m_availableDevices.Exists(d => d.Id == PackDeviceId))
                        {
                            be.CustomButtonUpImage = Resources.DevicePackUp87MB;
                            be.CustomButtonDownImage = Resources.DevicePackDown87;
                            be.Text = string.Empty;
                            be.OutlinedText = true;
                            be.UserDataAsInt = deviceID;
                            m_buttonMenu.Refresh(UnitSelectId);
                        }
                        else
                        {
                            be.CustomButtonUpImage = null;
                            be.CustomButtonDownImage = null;
                            be.Text = Resources.UnitSelectionButtonText;
                            be.UserDataAsInt = deviceID;
                            be.UseCustomAlignment = false;
                            be.OutlinedText = false;
                            m_buttonMenu.Refresh(UnitSelectId);
                        }

                        Application.DoEvents();
                    }
                }
                else //using keypad buttons
                {
                    if ((int)m_keypad.Option1Tag == 0 || (int)m_keypad.Option2Tag == 0 || (int)m_keypad.Option3Tag == 0 || (int)m_keypad.Option4Tag == 0)
                    {
                        m_keypad.Option1Dim = (int)m_keypad.Option1Tag != 0;
                        m_keypad.Option2Dim = (int)m_keypad.Option2Tag != 0;
                        m_keypad.Option3Dim = (int)m_keypad.Option3Tag != 0;
                        m_keypad.Option4Dim = (int)m_keypad.Option4Tag != 0;
                    }
                    else //no PACK button
                    {
                        if (m_keypad.Option1Dim != false)
                            m_keypad.Option1Dim = false;

                        if (m_keypad.Option2Dim != false)
                            m_keypad.Option2Dim = false;

                        if (m_keypad.Option3Dim != false)
                            m_keypad.Option3Dim = false;

                        if (m_keypad.Option4Dim != false)
                            m_keypad.Option4Dim = false;
                    }
                }
            }
            else //we have a device
            {
                if (m_parent.Settings.UseSystemMenuForUnitSelection)
                {
                    ButtonEntry be = m_buttonMenu.GetButtonEntry(UnitSelectId);

                    if (be != null)
                    {
                        be.Text = string.Empty;

                        if (deviceID == Device.Traveler.Id)
                        {
                            be.CustomButtonUpImage = Resources.DeviceTravelerUp87;
                            be.CustomButtonDownImage = Resources.DeviceTravelerDown87;
                        }

                        if (deviceID == Device.Tracker.Id)
                        {
                            be.CustomButtonUpImage = Resources.DeviceTrackerUp87;
                            be.CustomButtonDownImage = Resources.DeviceTrackerDown87;
                        }

                        if (deviceID == Device.Fixed.Id)
                        {
                            be.CustomButtonUpImage = Resources.DeviceFixedUp87;
                            be.CustomButtonDownImage = Resources.DeviceFixedDown87;
                        }

                        if (deviceID == Device.Explorer.Id)
                        {
                            be.CustomButtonUpImage = Resources.DeviceExplorerUp87;
                            be.CustomButtonDownImage = Resources.DeviceExplorerDown87;
                        }

                        if (deviceID == Device.Traveler2.Id)
                        {
                            be.CustomButtonUpImage = Resources.DeviceTraveler2Up87;
                            be.CustomButtonDownImage = Resources.DeviceTraveler2Down87;
                        }

                        if (deviceID == Device.Tablet.Id)
                        {
                            be.CustomButtonUpImage = Resources.TEDE87;
                            be.CustomButtonDownImage = Resources.TEDE87_down;
                        }

                        be.UseCustomAlignment = true;
                        be.Text = " "+DeviceFeeText(deviceID);
                        be.OutlinedText = true;
                        be.UserDataAsInt = deviceID;
                        m_buttonMenu.Refresh(UnitSelectId);

                        Application.DoEvents();
                    }
                }
                else //using keypad buttons
                {
                    Device device = Device.FromId(deviceID);

                    if ((int)m_keypad.Option1Tag != deviceID)
                    {
                        if (m_keypad.Option1Dim != true)
                            m_keypad.Option1Dim = true;
                    }
                    else
                    {
                        if (m_keypad.Option1Dim != false)
                            m_keypad.Option1Dim = false;
                    }

                    m_keypad.Option1Text = DeviceFeeText((int)m_keypad.Option1Tag);

                    if ((int)m_keypad.Option2Tag != deviceID)
                    {
                        if (m_keypad.Option2Dim != true)
                            m_keypad.Option2Dim = true;
                    }
                    else
                    {
                        if (m_keypad.Option2Dim != false)
                            m_keypad.Option2Dim = false;
                    }

                    m_keypad.Option2Text = DeviceFeeText((int)m_keypad.Option2Tag);

                    if ((int)m_keypad.Option3Tag != deviceID)
                    {
                        if (m_keypad.Option3Dim != true)
                            m_keypad.Option3Dim = true;
                    }
                    else
                    {
                        if (m_keypad.Option3Dim != false)
                            m_keypad.Option3Dim = false;
                    }

                    m_keypad.Option3Text = DeviceFeeText((int)m_keypad.Option3Tag);

                    if ((int)m_keypad.Option4Tag != deviceID)
                    {
                        if (m_keypad.Option4Dim != true)
                            m_keypad.Option4Dim = true;
                    }
                    else
                    {
                        if (m_keypad.Option4Dim != false)
                            m_keypad.Option4Dim = false;
                    }

                    m_keypad.Option4Text = DeviceFeeText((int)m_keypad.Option4Tag);
                }

                m_selectedDeviceName.Text = (m_displayMode is CompactDisplayMode ? "Using " : "Playing on ") + Device.FromId(deviceID).Name;
                m_selectedDeviceName.Visible = true;
            }
        }


        public void SetQuantitySaleInfo(int qty = 0)
        {
            if (qty <= 1) //no quantity sale, hide it
            {
                m_quantitySaleInfo.Visible = false;
            }
            else //we have a quantity sale, show it
            {
                m_quantitySaleInfo.Text = Resources.QuantitySaleInfo + " " + qty.ToString();
                m_quantitySaleInfo.Visible = true;
            }
        }
        
        /// <summary>
        /// Updates the player's information on screen.
        /// </summary>
        public void SetPlayer()
        {
            // TTP 50114
            ClearPlayer();
            
            if(m_parent.CurrentSale != null && m_parent.CurrentSale.Player != null)
            {
                if (m_parent.WeAreAPOSKiosk)
                {
                    m_playerCardPictureLabel.Visible = false;
                    m_playerCardPicture.Visible = false;
                    Application.DoEvents();
                }

                m_panelLastTotal.Hide();

                m_playerLabel.Text += " " + m_parent.CurrentSale.Player.ToString(false);

                m_playerInfoList.BeginUpdate();

                if (m_parent.WeAreNotAPOSKiosk)
                {
                    // any issues with this player?
                    if (m_parent.CurrentSale.Player.ActiveStatusList.Exists(s => s.IsAlert))
                    {
                        foreach (PlayerStatus ps in m_parent.CurrentSale.Player.ActiveStatusList.Where(s => s.IsAlert))
                            m_playerInfoList.Items.Add(ps.Name);
                    }

                    if (m_parent.CurrentSale.Player.ActiveStatusList.Exists(s => !s.IsAlert))
                    {
                        foreach (PlayerStatus ps in m_parent.CurrentSale.Player.ActiveStatusList.Where(s => !s.IsAlert))
                            m_playerInfoList.Items.Add(ps.Name);
                    }
                }

                // Is today the player's birthday?
                if(!m_parent.Settings.EnableAnonymousMachineAccounts &&
                   m_parent.CurrentSale.Player.BirthDate.Month == DateTime.Now.Month &&
                   m_parent.CurrentSale.Player.BirthDate.Day == DateTime.Now.Day)
                {
                    m_playerInfoList.Items.Add(Resources.SellingFormPlayerBirthday);
                }

                // Rally TA7897
                if(m_parent.Settings.CreditEnabled)
                {
                    // Credit.
                    m_playerInfoList.Items.Add(string.Format(CultureInfo.CurrentCulture, Resources.SellingFormPlayerRefundableCredit, m_parent.CurrentSale.Player.RefundableCredit.ToString("C", CultureInfo.CurrentCulture)));
                    m_playerInfoList.Items.Add(string.Format(CultureInfo.CurrentCulture, Resources.SellingFormPlayerNonRefundableCredit, m_parent.CurrentSale.Player.NonRefundableCredit.ToString("C", CultureInfo.CurrentCulture)));
                }

                if(!m_parent.Settings.EnableAnonymousMachineAccounts)
                {
                    // Points and visits.
                    if(m_parent.CurrentSale.Player.PointsUpToDate)
                        m_playerInfoList.Items.Add(string.Format(CultureInfo.CurrentCulture, Resources.SellingFormPlayerPoints, m_parent.CurrentSale.Player.PointsBalance.ToString("N", CultureInfo.CurrentCulture)));
                    
                    m_playerInfoList.Items.Add(string.Format(CultureInfo.CurrentCulture, Resources.SellingFormPlayerVisitCount, m_parent.CurrentSale.Player.VisitCount));

                    // Important dates.
                    if(m_parent.CurrentSale.Player.LastVisit != DateTime.MinValue)
                        m_playerInfoList.Items.Add(string.Format(CultureInfo.CurrentCulture, Resources.SellingFormPlayerLastVisit, m_parent.CurrentSale.Player.LastVisit.ToShortDateString()));

                    if(m_parent.CurrentSale.Player.JoinDate != DateTime.MinValue)
                        m_playerInfoList.Items.Add(string.Format(CultureInfo.CurrentCulture, Resources.SellingFormPlayerJoinDate, m_parent.CurrentSale.Player.JoinDate.ToShortDateString()));
                }

                m_playerInfoList.EndUpdate();

                if (m_parent.CurrentSale.IsReturn)
                {
                    m_parent.CurrentCouponForm = null;
                    m_buttonMenu.SetEnabled(PlayerCouponsId, false);
                }
                else
                {
                    m_parent.CurrentCouponForm = new PayCouponForm4(m_parent, m_parent.CurrentSale.Device.Id);
                    m_buttonMenu.SetEnabled(PlayerCouponsId, m_parent.CurrentCouponForm != null && m_parent.CurrentCouponForm.LoadPlayerComp(true));
                }

                m_buttonMenu.SetEnabled(ReprintPlayerLastReceiptId, true);
                
                // TODO Might need to check for non-cash-only credit payouts later.
                // Rally TA7897
                if(m_parent.Settings.CreditEnabled && (m_parent.CurrentSale.Player.RefundableCredit != 0M || m_parent.CurrentSale.Player.CashOnlyCredit != 0M))
                    m_buttonMenu.SetEnabled(CreditCashOutId, true);

                m_buttonMenu.SetEnabled(UnitAssignmentId, true);
                // TODO Revisit Comps. m_buttonMenu.SetEnabled(RedeemCompsId, true);
            }
        }

        /// <summary>
        /// Clears out the previous values in the player seciont
        /// </summary>
        /// <param name="clearPlayerName">whether or not to clear the player's name.</param>
        public void ClearPlayer(bool clearPlayerName = true)
        {
            m_pointsEarned.Visible = false;
            m_pointsEarnedLabel.Visible = false;

            if (m_parent.WeAreAPOSKiosk)
            {
                m_playerCardPictureLabel.Visible = true;
                m_playerCardPicture.Visible = true;
            }

            // Clear out the previous values.
            if (clearPlayerName) // don't clear the player name for things like refreshing the player's info
            {
                if (m_parent.Settings.EnableAnonymousMachineAccounts)
                    m_playerLabel.Text = Resources.SellingFormMachine;
                else
                    m_playerLabel.Text = Resources.SellingFormPlayer;
            }

            m_playerInfoList.Items.Clear();

            m_parent.CurrentCouponForm = null;
            m_buttonMenu.SetEnabled(PlayerCouponsId, false);

            m_buttonMenu.SetEnabled(ReprintPlayerLastReceiptId, false);
            
            m_buttonMenu.SetEnabled(CreditCashOutId, false);
            m_buttonMenu.SetEnabled(UnitAssignmentId, false);
            // TODO Revisit Comps. m_buttonMenu.SetEnabled(RedeemCompsId, false);
        }

        /// <summary>
        /// Sets the form's version label.
        /// </summary>
        /// <param name="version">The text to display.</param>
        public void SetVersion(string version)
        {
            m_version = version;
            m_versionLabel.Text = version;
        }

        public void DisableAdvancedKioskCardSwipePic(bool disable = true)
        {
            if (disable)
                m_playerCardPicture.Image = null;
            else
                m_playerCardPicture.Image = Resources.AnimatedPlayerCard_large_;
        }

        /// <summary>
        /// Loads the menu list items into the menu list.
        /// </summary>
        /// <param name="menuList">The list of menu items to be added.</param>
        /// <param name="currentMenu">The item that will be selected by 
        /// default.</param>
        /// <exception cref="System.ArgumentNullException">menuList is a null 
        /// reference.</exception>
        public void LoadMenuList(POSMenuListItem[] menuList, int currentMenu)
        {
            if (m_parent.WeAreAPOSKiosk && (menuList == null || menuList.Length == 0))
                return;
            
            if(menuList == null)
                throw new ArgumentNullException("menuList");

            m_menuList.BeginUpdate();

            // Clear the old list and add the new array.
            m_menuList.Items.Clear();
            m_menuList.Items.AddRange(menuList);

            // Select the current menu or the first one in the list.
            if(currentMenu >= 0 && currentMenu < m_menuList.Items.Count)
                m_menuList.SelectedIndex = currentMenu;
            else if(m_menuList.Items.Count > 0)
                m_menuList.SelectedIndex = 0;

            m_menuList.EndUpdate();

            //figure out where we can put the stairstep discount text
            m_addDiscountTextToScreenReceipt = m_parent.Settings.AutoDiscountInfoGoesOnBottomOfScreenReceipt;

            if (!(m_parent.Settings.DisplayMode is WideDisplayMode)) //stairstep text has its own window if widescreen.
            {
                if (!m_parent.Settings.AutoDiscountInfoGoesOnBottomOfScreenReceipt)
                {
                    //associate small Up/Down buttons with menu control
                    m_menuList.UpButton = m_menuUpButtonSmall;
                    m_menuList.DownButton = m_menuDownButtonSmall;

                    //associate Up/Down buttons with discount text control
                    m_stairstepDiscountBox.UpButton = m_autoDiscountUpButton;
                    m_stairstepDiscountBox.DownButton = m_autoDiscountDownButton;

                    //make the menu box smaller
                    m_menuList.Size = new Size(m_menuList.Width, 52);

                    m_menuMiddle.Visible = true;
                    m_menuUpButton.Visible = false;
                    m_menuDownButton.Visible = false;
                    m_menuUpButtonSmall.Visible = true;
                    m_menuDownButtonSmall.Visible = true;
                    m_autoDiscountUpButton.Visible = true;
                    m_autoDiscountDownButton.Visible = true;
                    m_stairstepDiscountBox.Show();
                }
                else
                {
                    //disassociate Up/Down buttons from discount text control
                    m_stairstepDiscountBox.UpButton = null;
                    m_stairstepDiscountBox.DownButton = null;

                    //associate Up/Down buttons with menu control
                    m_menuList.UpButton = m_menuUpButton;
                    m_menuList.DownButton = m_menuDownButton;

                    m_stairstepDiscountBox.Hide();
                }
            }
        }

        /// <summary>
        /// Loads the specified menu on the form.
        /// </summary>
        /// <param name="menu">The menu to load.</param>
        /// <param name="page">The page to load or 0 for a blank page.</param>
        /// <exception cref="System.ArgumentNullException">menu is a null 
        /// reference.</exception>
        public void LoadMenu(POSMenu menu, byte page)
        {
            if(menu == null && m_parent.WeAreNotAPOSKiosk)
                throw new ArgumentNullException("menu");

            // Either load the menu and set the specified page or clear the 
            // menu to nothing.
            int visiblePagesInTheMenu = menu == null? 0 : (menu.PageCount - (m_parent.WeAreAPOSKiosk ? menu.BlankKioskPagesAtEnd : 0));
            
            if(visiblePagesInTheMenu > 0)
            {
                m_pageNavigator.NumberOfPages = visiblePagesInTheMenu;

                LoadMenuPage(menu, page);

                //maximize usable menu buttons
                if (m_displayMode is CompactDisplayMode)
                {
                    if (visiblePagesInTheMenu < 4)
                    {
                        m_pageNavigator.NumberOfButtons = 3; //RAK shows 3 buttons (unused are empty)
                        m_pageNavigator.ShowNextButton = false;
                        m_pageNavigator.ShowPrevButton = false;
                    }
                    else
                    {
                        m_pageNavigator.NumberOfButtons = 1;
                        m_pageNavigator.ShowNextButton = true;
                        m_pageNavigator.ShowPrevButton = true;
                    }
                }
                else //full size or wide
                {
                    if (visiblePagesInTheMenu < 6)
                    {
                        m_pageNavigator.NumberOfButtons = 5; //menu.PageCount; //RAK shows 5 buttons (unused are empty)
                        m_pageNavigator.ShowNextButton = false;
                        m_pageNavigator.ShowPrevButton = false;
                    }
                    else
                    {
                        m_pageNavigator.NumberOfButtons = 3;
                        m_pageNavigator.ShowNextButton = true;
                        m_pageNavigator.ShowPrevButton = true;
                    }
                }

                if (m_parent.Settings.DisplayMode is WideDisplayMode && ((WideDisplayMode)m_parent.Settings.DisplayMode).TwoPagesPerPage)
                    m_pageNavigator.PagesGroupedByTwo = true;
            }
            else //no pages, use 1
            {
                m_pageNavigator.NumberOfPages = 1;
                LoadMenuPage(menu, 0); // Clear the menu.
            }

            UpdateProgramName();

            //update the kiosk buttons with the prices
            if (m_simpleKioskForm != null)
            {
                foreach (ImageButton button in m_menuButtonsPanel.Controls)
                {
                    if (button.Tag != null)
                        m_simpleKioskForm.UpdateButtonText(button);
                }
            }
        }

        /// <summary>
        /// Update's the program name based on the currenly selected menu.
        /// </summary>
        protected void UpdateProgramName()
        {
            if(m_menuList.SelectedIndex >= 0)
                m_programLabel.Text = ((POSMenuListItem)m_menuList.SelectedItem).Session.ProgramName;
            else
                m_programLabel.Text = string.Empty;
        }

        /// <summary>
        /// Load the specified menu page on the form.
        /// </summary>
        /// <param name="menu">The menu to load.</param>
        /// <param name="page">The page to load or 0 for a blank page.</param>
        /// <exception cref="System.ArgumentNullException">menu is a null 
        /// reference.</exception>
        protected void LoadMenuPage(POSMenu menu, byte page)
        {
            if (m_parent.WeAreAPOSKiosk && menu == null)
                return;

            if(menu == null)
                throw new ArgumentNullException("menu");

            m_menuButtonsPanel.Hide();

            // Clear the menu out.
            foreach(ImageButton button in m_menuButtonsPanel.Controls)
            {
                button.Visible = false;
                button.Stretch = true; // PDTS 964
                button.Tag = null;
                button.UseSecondaryText = false;
            }

            if(page > 0)
            {
                // Associate a button on the panel with a button from the menu.
                MenuButton[] buttons = menu.GetPage(page);

                if(buttons != null)
                {
                    for(int x = 0; x < buttons.Length; x++)
                    {
                        if(buttons[x] != null)
                        {
                            Control[] menuButtons = m_menuButtonsPanel.Controls.Find("MenuButton" + x.ToString(CultureInfo.InvariantCulture), true);

                            if(menuButtons.Length > 0)
                            {
                                // Set the button as visible and it's text.
                                ImageButton imgButton = (ImageButton)menuButtons[0];
                                imgButton.Visible = true;
                                imgButton.Text = buttons[x].Text;
                                SetButtonImagesFromMenuButton(imgButton, buttons[x]);

                                PackageButton packageButton = buttons[x] as PackageButton;
                                DiscountButton discountButton = buttons[x] as DiscountButton;

                                if(packageButton != null)
                                {
                                    // Only print a price if it doesn't have 
                                    // open credit.
                                    if (!packageButton.Package.HasOpenCredit)
                                    {
                                        // Print out the points if the package 
                                        // has no price.
                                        if (packageButton.Package.Price == 0M && packageButton.Package.PointsToRedeem != 0M)
                                        {
                                            imgButton.Text = buttons[x].Text + Environment.NewLine + packageButton.Package.PointsToRedeem.ToString("N", CultureInfo.CurrentCulture) + " " + Resources.ButtonPointsText;
                                        }
                                        else
                                        {
                                            decimal price = m_parent.GetValidatedPackagePrice(packageButton.Package);

                                            imgButton.Text = buttons[x].Text + Environment.NewLine + price.ToString("N", CultureInfo.CurrentCulture); // Rally TA7465
                                        }
                                    }
                                    else
                                    {
                                        imgButton.Text = buttons[x].Text;
                                    }
                                }
                                else if (discountButton != null)
                                {
                                    // Rally TA7465
                                    if (discountButton.Discount.Amount != 0)
                                    {
                                        imgButton.Text = buttons[x].Text + Environment.NewLine + discountButton.Discount.Amount.ToString("N", CultureInfo.CurrentCulture);

                                        if (discountButton.Discount is PercentDiscount)
                                            imgButton.Text = buttons[x].Text + CultureInfo.CurrentCulture.NumberFormat.PercentSymbol;
                                    }
                                    else
                                    {
                                        imgButton.Text = buttons[x].Text;
                                    }
                                }
                                else
                                {
                                    imgButton.Text = buttons[x].Text;
                                }

                                imgButton.Tag = buttons[x];
                            }
                        }
                    }
                }

                UpdateQuantitiesOnButtons();
            }

            UpdateMenuButtonStates();

            m_menuButtonsPanel.Show();
        }

        public void SetButtonImagesFromMenuButton(ImageButton imageButton, MenuButton menuButton)
        {
            // If the button has a graphic then change it.
            switch ((MenuButtonGraphic)menuButton.GraphicId)
            {
                case MenuButtonGraphic.None:
                imageButton.ImageNormal = Resources.GrayButtonUp;
                imageButton.ImagePressed = Resources.GrayButtonDown;
                break;

                case MenuButtonGraphic.Set:
                imageButton.ImageNormal = Resources.SetButtonUp;
                imageButton.ImagePressed = Resources.SetButtonDown;
                break;

                case MenuButtonGraphic.Book:
                imageButton.ImageNormal = Resources.BookButtonUp;
                imageButton.ImagePressed = Resources.BookButtonDown;
                break;

                case MenuButtonGraphic.Paper:
                imageButton.ImageNormal = Resources.PaperButtonUp;
                imageButton.ImagePressed = Resources.PaperButtonDown;
                break;

                case MenuButtonGraphic.PaperBrown:
                imageButton.ImageNormal = Resources.PaperBrownButtonUp;
                imageButton.ImagePressed = Resources.PaperBrownButtonDown;
                break;

                case MenuButtonGraphic.PaperOrange:
                imageButton.ImageNormal = Resources.PaperOrangeButtonUp;
                imageButton.ImagePressed = Resources.PaperOrangeButtonDown;
                break;

                case MenuButtonGraphic.PaperPurple:
                imageButton.ImageNormal = Resources.PaperPurpleButtonUp;
                imageButton.ImagePressed = Resources.PaperPurpleButtonDown;
                break;

                case MenuButtonGraphic.PaperGreen:
                imageButton.ImageNormal = Resources.PaperGreenButtonUp;
                imageButton.ImagePressed = Resources.PaperGreenButtonDown;
                break;

                case MenuButtonGraphic.PaperRed:
                imageButton.ImageNormal = Resources.PaperRedButtonUp;
                imageButton.ImagePressed = Resources.PaperRedButtonDown;
                break;

                case MenuButtonGraphic.PaperRainbow:
                imageButton.ImageNormal = Resources.PaperRainbowButtonUp;
                imageButton.ImagePressed = Resources.PaperRainbowButtonDown;
                break;

                case MenuButtonGraphic.PaperTan:
                imageButton.ImageNormal = Resources.PaperTanButtonUp;
                imageButton.ImagePressed = Resources.PaperTanButtonDown;
                break;

                case MenuButtonGraphic.PaperWhite:
                imageButton.ImageNormal = Resources.PaperWhiteButtonUp;
                imageButton.ImagePressed = Resources.PaperWhiteButtonDown;
                break;

                case MenuButtonGraphic.Credit:
                imageButton.ImageNormal = Resources.CreditButtonUp;
                imageButton.ImagePressed = Resources.CreditButtonDown;
                break;

                case MenuButtonGraphic.Discount:
                imageButton.ImageNormal = Resources.DiscountButtonUp;
                imageButton.ImagePressed = Resources.DiscountButtonDown;
                break;

                case MenuButtonGraphic.Electronic:
                imageButton.ImageNormal = Resources.ElectronicButtonUp;
                imageButton.ImagePressed = Resources.ElectronicButtonDown;
                break;

                case MenuButtonGraphic.Concession:
                imageButton.ImageNormal = Resources.ConcessionButtonUp;
                imageButton.ImagePressed = Resources.ConcessionButtonDown;
                break;

                case MenuButtonGraphic.Merchandise:
                imageButton.ImageNormal = Resources.MerchandiseButtonUp;
                imageButton.ImagePressed = Resources.MerchandiseButtonDown;
                break;

                case MenuButtonGraphic.Brown:
                imageButton.ImageNormal = Resources.BrownButtonUp;
                imageButton.ImagePressed = Resources.BrownButtonDown;
                break;

                case MenuButtonGraphic.Green:
                imageButton.ImageNormal = Resources.GreenButtonUp;
                imageButton.ImagePressed = Resources.GreenButtonDown;
                break;

                case MenuButtonGraphic.Orange:
                imageButton.ImageNormal = Resources.OrangeButtonUp;
                imageButton.ImagePressed = Resources.OrangeButtonDown;
                break;

                case MenuButtonGraphic.Purple:
                imageButton.ImageNormal = Resources.PurpleButtonUp;
                imageButton.ImagePressed = Resources.PurpleButtonDown;
                break;

                case MenuButtonGraphic.Rainbow:
                imageButton.ImageNormal = Resources.RainbowButtonUp;
                imageButton.ImagePressed = Resources.RainbowButtonDown;
                break;

                case MenuButtonGraphic.Red:
                imageButton.ImageNormal = Resources.RedButtonUp;
                imageButton.ImagePressed = Resources.RedButtonDown;
                break;

                case MenuButtonGraphic.White:
                imageButton.ImageNormal = Resources.WhiteButtonUp;
                imageButton.ImagePressed = Resources.WhiteButtonDown;
                break;

                case MenuButtonGraphic.Yellow:
                imageButton.ImageNormal = Resources.YellowButtonUp;
                imageButton.ImagePressed = Resources.YellowButtonDown;
                break;

                case MenuButtonGraphic.Black3D:
                imageButton.ImageNormal = Resources.BlackGelButtonUp;
                imageButton.ImagePressed = Resources.BlackGelButtonDown;
                break;

                case MenuButtonGraphic.BlackFlat:
                imageButton.ImageNormal = Resources.BlackFlatButtonUp;
                imageButton.ImagePressed = Resources.BlackFlatButtonDown;
                break;

                case MenuButtonGraphic.Blue3D:
                imageButton.ImageNormal = Resources.BlueGelButtonUp;
                imageButton.ImagePressed = Resources.BlueGelButtonDown;
                break;

                case MenuButtonGraphic.BlueFlat:
                imageButton.ImageNormal = Resources.BlueFlatButtonUp;
                imageButton.ImagePressed = Resources.BlueFlatButtonDown;
                break;

                case MenuButtonGraphic.Brown3D:
                imageButton.ImageNormal = Resources.BrownGelButtonUp;
                imageButton.ImagePressed = Resources.BrownGelButtonDown;
                break;

                case MenuButtonGraphic.BrownFlat:
                imageButton.ImageNormal = Resources.BrownFlatButtonUp;
                imageButton.ImagePressed = Resources.BrownFlatButtonDown;
                break;

                case MenuButtonGraphic.Gold3D:
                imageButton.ImageNormal = Resources.GoldGelButtonUp;
                imageButton.ImagePressed = Resources.GoldGelButtonDown;
                break;

                case MenuButtonGraphic.GoldFlat:
                imageButton.ImageNormal = Resources.GoldFlatButtonUp;
                imageButton.ImagePressed = Resources.GoldFlatButtonDown;
                break;

                case MenuButtonGraphic.Gray3D:
                imageButton.ImageNormal = Resources.GrayGelButtonUp;
                imageButton.ImagePressed = Resources.GrayGelButtonDown;
                break;

                case MenuButtonGraphic.GrayFlat:
                imageButton.ImageNormal = Resources.GrayFlatButtonUp;
                imageButton.ImagePressed = Resources.GrayFlatButtonDown;
                break;

                case MenuButtonGraphic.Green3D:
                imageButton.ImageNormal = Resources.GreenGelButtonUp;
                imageButton.ImagePressed = Resources.GreenGelButtonDown;
                break;

                case MenuButtonGraphic.GreenFlat:
                imageButton.ImageNormal = Resources.GreenFlatButtonUp;
                imageButton.ImagePressed = Resources.GreenFlatButtonDown;
                break;

                case MenuButtonGraphic.Lavender3D:
                imageButton.ImageNormal = Resources.LavenderGelButtonUp;
                imageButton.ImagePressed = Resources.LavenderGelButtonDown;
                break;

                case MenuButtonGraphic.LavenderFlat:
                imageButton.ImageNormal = Resources.LavenderFlatButtonUp;
                imageButton.ImagePressed = Resources.LavenderFlatButtonDown;
                break;

                case MenuButtonGraphic.Orange3D:
                imageButton.ImageNormal = Resources.OrangeGelButtonUp;
                imageButton.ImagePressed = Resources.OrangeGelButtonDown;
                break;

                case MenuButtonGraphic.OrangeFlat:
                imageButton.ImageNormal = Resources.OrangeFlatButtonUp;
                imageButton.ImagePressed = Resources.OrangeFlatButtonDown;
                break;

                case MenuButtonGraphic.Orchid3D:
                imageButton.ImageNormal = Resources.OrchidGelButtonUp;
                imageButton.ImagePressed = Resources.OrchidGelButtonDown;
                break;

                case MenuButtonGraphic.OrchidFlat:
                imageButton.ImageNormal = Resources.OrchidFlatButtonUp;
                imageButton.ImagePressed = Resources.OrchidFlatButtonDown;
                break;

                case MenuButtonGraphic.Pink3D:
                imageButton.ImageNormal = Resources.PinkGelButtonUp;
                imageButton.ImagePressed = Resources.PinkGelButtonDown;
                break;

                case MenuButtonGraphic.PinkFlat:
                imageButton.ImageNormal = Resources.PinkFlatButtonUp;
                imageButton.ImagePressed = Resources.PinkFlatButtonDown;
                break;

                case MenuButtonGraphic.Rainbow3D:
                imageButton.ImageNormal = Resources.RainbowGelButtonUp;
                imageButton.ImagePressed = Resources.RainbowGelButtonDown;
                break;

                case MenuButtonGraphic.RainbowFlat:
                imageButton.ImageNormal = Resources.RainbowFlatButtonUp;
                imageButton.ImagePressed = Resources.RainbowFlatButtonDown;
                break;

                case MenuButtonGraphic.Red3D:
                imageButton.ImageNormal = Resources.RedGelButtonUp;
                imageButton.ImagePressed = Resources.RedGelButtonDown;
                break;

                case MenuButtonGraphic.RedFlat:
                imageButton.ImageNormal = Resources.RedFlatButtonUp;
                imageButton.ImagePressed = Resources.RedFlatButtonDown;
                break;

                case MenuButtonGraphic.Tan3D:
                imageButton.ImageNormal = Resources.TanGelButtonUp;
                imageButton.ImagePressed = Resources.TanGelButtonDown;
                break;

                case MenuButtonGraphic.TanFlat:
                imageButton.ImageNormal = Resources.TanFlatButtonUp;
                imageButton.ImagePressed = Resources.TanFlatButtonDown;
                break;

                case MenuButtonGraphic.White3D:
                imageButton.ImageNormal = Resources.WhiteGelButtonUp;
                imageButton.ImagePressed = Resources.WhiteGelButtonDown;
                break;

                case MenuButtonGraphic.WhiteFlat:
                imageButton.ImageNormal = Resources.WhiteFlatButtonUp;
                imageButton.ImagePressed = Resources.WhiteFlatButtonDown;
                break;

                case MenuButtonGraphic.Yellow3D:
                imageButton.ImageNormal = Resources.YellowGelButtonUp;
                imageButton.ImagePressed = Resources.YellowGelButtonDown;
                break;

                case MenuButtonGraphic.YellowFlat:
                imageButton.ImageNormal = Resources.YellowFlatButtonUp;
                imageButton.ImagePressed = Resources.YellowFlatButtonDown;
                break;
            }
        }

        public void UpdateMenuButtonPrices()
        {
            if (m_parent.IsB3Sale)
                return;

            foreach (ImageButton button in m_menuButtonsPanel.Controls)
            {
                if (button.Tag != null)
                {
                    MenuButton menuButton = (MenuButton)button.Tag;
                    PackageButton packageButton = menuButton as PackageButton;

                    if (packageButton != null)
                    {
                        if(!packageButton.Package.HasOpenCredit)
                        {
                            if (!(packageButton.Package.Price == 0M && packageButton.Package.PointsToRedeem != 0M))
                            {
                                decimal price = m_parent.GetValidatedPackagePrice(packageButton.Package);
                                string oldText = button.Text;
                                string newText = menuButton.Text + Environment.NewLine + price.ToString("N", CultureInfo.CurrentCulture);

                                if(newText != oldText)
                                    button.Text = newText;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Updates the Enabled state of all the menu buttons based on the 
        /// current system state.
        /// </summary>
        public void UpdateMenuButtonStates()
        {
            bool returnMode = false;

            if(m_parent.CurrentSale != null && m_parent.CurrentSale.IsReturn)
                returnMode = true;

            foreach(ImageButton button in m_menuButtonsPanel.Controls)
            {
                if (button.Tag != null)
                {
                    MenuButton menuButton = (MenuButton)button.Tag;
                    PackageButton packageButton = menuButton as PackageButton;
                    FunctionButton functionButton = menuButton as FunctionButton;

                    // PDTS 571
                    if (menuButton.IsLocked || (m_parent.SaleState != PointOfSale.SellingState.NotSelling &&
                                               m_parent.SaleState != PointOfSale.SellingState.Selling)) // Check the easy cases first.
                    {
                        button.Enabled = false;

                        if (m_parent.WeAreAPOSKiosk && menuButton.IsLocked)
                            button.Visible = false;
                    }
                    else if (menuButton.IsPlayerRequired && (m_parent.CurrentSale == null || m_parent.CurrentSale.Player == null))
                    {
                        // Is a player required for this button?
                        // Rally DE129
                        button.Enabled = false;
                    }
                    else if (packageButton != null)
                    {
                        if (packageButton.Package.RequiresValidation && !m_parent.ValidationEnabled)
                        {
                            button.Enabled = false;
                        }
                        else
                        {
                            if (packageButton.Package.HasElectronics && (returnMode || !m_parent.Settings.AllowElectronicSales)) // Are we returning something or can we sell electronics?
                                button.Enabled = false;
                            else
                                button.Enabled = true;
                        }
                    }
                    else if (functionButton != null)
                    {
                        button.Enabled = functionButton.CheckEnabled();
                    }
                    else
                    {
                        button.Enabled = true;
                    }
                }
                else
                {
                    button.Enabled = false;
                }
            }
        }

        // Rally TA7729
        // PDTS 571
        /// <summary>
        /// Updates the Enabled state of all the system buttons based on the 
        /// current system state.
        /// </summary>
        public void UpdateSystemButtonStates()
        {
            NotIdle();

            // Check to see which total buttons we need to display.
            bool hasElectronics = false, showTraveler = false, showTracker = false, showFixed = false, showExplorer = false, showTraveler2 = false;

            if(m_parent.CurrentSale != null)
            {
                m_parent.CurrentSale.GetCompatibleDevices(out hasElectronics, out showTraveler, out showTracker, out showFixed, out showExplorer, out showTraveler2);

                // Make sure we don't go over max card limits for devices.
                // FIX: DE2416
                int maxCards = m_parent.CurrentSale.CalculateMaxNumCards();

                if(maxCards > m_parent.Settings.TravelerMaxCards)
                    showTraveler = false;

                if(maxCards > m_parent.Settings.TrackerMaxCards)
                    showTracker = false;

                if(maxCards > m_parent.Settings.FixedMaxCards)
                    showFixed = false;

                if(maxCards > m_parent.Settings.ExplorerMaxCards)
                    showExplorer = false;

                if(maxCards > m_parent.Settings.Traveler2MaxCards)
                    showTraveler2 = false; // PDTS 964, Rally US765
                // END: DE2416
            }

            UpdateMenuButtonPrices();

            if (m_parent.IsBusy) // Don't update the UI while server communication is going on.
                return;

            // Update the buttons' state.
            switch(m_parent.SaleState)
            {
                case PointOfSale.SellingState.NotSelling:
                    SetStartOverButton(true);
                    SetValidationButton(true); //US4467
                    EnableValidateButton(true);
                    EnableReturnButton(true);
                    EnableSaleButtons(true);
                    EnabledNonSaleButtons(true);
                    SetKeypadNumberMode(Keypad.NumberMode.Integer); // PDTS 583
                    HandleTotalOrNoSaleButton();
                    ShowTotalButtons(false);
                    EnableDeviceButtons(false, false, false, false, false, false); // Rally US765
                    ClearKeypad();
                    break;

                case PointOfSale.SellingState.Selling:
                    SetStartOverButton(true);

                    //US4380
                    if (m_parent.IsB3Sale) 
                        break;

                    EnableValidateButton(true);
                    EnableSaleButtons(true);
                    EnabledNonSaleButtons(false);
                    SetKeypadNumberMode(Keypad.NumberMode.Integer); // PDTS 583
                    HandleTotalOrNoSaleButton();
                
                    bool sellingToPack = m_allDeviceFeesAreZero && !m_parent.Settings.ForceDeviceSelectionWhenNoFees;

                    if (hasElectronics && m_parent.CurrentSale.Device.Id == 0 && !sellingToPack) //set the default electronic device
                    {
                        m_parent.CurrentSale.Device = Device.FromId(m_parent.Settings.DefaultElectronicDeviceID);

                        //set the selected unit name for the total area 
                        SetSelectedDeviceName(m_parent.CurrentSale.Device.Id);
                    }

                    if (hasElectronics && m_parent.CurrentSale.ChargeDeviceFee && m_parent.CurrentSale.DeviceFee == 0) //set the device fee if we need to and haven't
                        m_parent.UpdateDeviceFeesAndTotals();

                    ShowTotalButtons(hasElectronics);
                    EnableDeviceButtons(showFixed | showExplorer | showTraveler2, showTraveler, showTracker, showFixed, showExplorer, showTraveler2); // Rally US765, DE6569
                    ClearKeypad();
                    break;

                case PointOfSale.SellingState.QuantitySelling:
                    SetStartOverButton(true);
                    EnableSaleButtons(false);
                    EnabledNonSaleButtons(false);
                    SetKeypadNumberMode(Keypad.NumberMode.Integer); // PDTS 583
                    ShowTotalButtons(hasElectronics);
                    EnableDeviceButtons(showFixed | showExplorer | showTraveler2, showTraveler, showTracker, showFixed, showExplorer, showTraveler2); // Rally US765, DE6569
                    ClearKeypad();
                    break;

                case PointOfSale.SellingState.Tendering:
                    if (hasElectronics && m_parent.CurrentSale.ChargeDeviceFee && m_parent.CurrentSale.DeviceFee == 0) //set the device fee if we need to and haven't
                        m_parent.UpdateDeviceFeesAndTotals();

                    SetStartOverButton(false);
                    EnableValidateButton(false);
                    EnableSaleButtons(false);
                    EnabledNonSaleButtons(false);
                    SetKeypadNumberMode(Keypad.NumberMode.Currency);
                    m_keypad.BigButtonImageNormal = Resources.BlueButtonUp;
                    m_keypad.BigButtonImagePressed = Resources.BlueButtonDown;
                    HandleTotalOrNoSaleButton();
                    ShowTotalButtons(false);
                    EnableDeviceButtons(false, false, false, false, false, false); // Rally US765                    
                    SetKeypadInitialValue();
                    break;

                case PointOfSale.SellingState.Finishing:
                    if (hasElectronics && m_parent.CurrentSale.ChargeDeviceFee && m_parent.CurrentSale.DeviceFee == 0) //set the device fee if we need to and haven't
                        m_parent.UpdateDeviceFeesAndTotals();

                    SetStartOverButton(false);
                    EnableValidateButton(false);
                    EnableSaleButtons(false);
                    EnabledNonSaleButtons(false);
                    m_keypad.BigButtonImageNormal = Resources.BlueButtonUp;
                    m_keypad.BigButtonImagePressed = Resources.BlueButtonDown;
                    HandleTotalOrNoSaleButton();
                    ShowTotalButtons(false);
                    EnableDeviceButtons(false, false, false, false, false, false); // Rally US765
                    SetKeypadNumberMode(Keypad.NumberMode.Integer); // PDTS 583
                    ClearKeypad();
                    break;
            }
        }

        private void HandleTotalOrNoSaleButton()
        {
            if (m_parent.SaleState == PointOfSale.SellingState.Finishing)
            {
                SetTotalButtonText(Resources.SellingFormFinish);
            }
            else if (m_parent.SaleState == PointOfSale.SellingState.Tendering)
            {
                SetTotalButtonText(Resources.SellingFormTender);
            }
            else if (m_parent.SaleState == PointOfSale.SellingState.NotSelling)
            {
                if (m_parent.Settings.AllowNoSale && !m_parent.IsB3Sale && m_parent.IsSaleEmpty)
                {
                    SetTotalButtonText(Resources.NoSale, Resources.PurpleButtonUp, Resources.PurpleButtonDown);
                }
                else
                {
                    if (m_parent.WeAreAPOSKiosk)
                        SetTotalButtonText(Resources.SellingFormPay, Resources.GreenButtonUp, Resources.GreenButtonDown);
                    else
                        SetTotalButtonText(Resources.SellingFormTotal);
                }
            }
            else if (m_parent.SaleState == PointOfSale.SellingState.Selling)
            {
                if (m_parent.Settings.AllowNoSale && !m_parent.IsB3Sale && m_parent.IsSaleEmpty && !m_parent.SaleHasTenders)
                {
                    SetTotalButtonText(Resources.NoSale, Resources.PurpleButtonUp, Resources.PurpleButtonDown);
                }
                else
                {
                    if (m_parent.WeAreAPOSKiosk)
                        SetTotalButtonText(Resources.SellingFormPay, Resources.GreenButtonUp, Resources.GreenButtonDown);
                    else
                        SetTotalButtonText(Resources.SellingFormTotal);
                }
            }
        }

        // FIX: US2018
        /// <summary>
        /// Update the form with the sale's information.
        /// </summary>
        public void UpdateSaleInfo()
        {
            // Update the list.
            UpdateSaleList();

            // Rally TA7465
            m_currencyButton.Text = m_parent.CurrentCurrency.ISOCode;
            m_keypad.CurrencySymbol = m_parent.CurrentCurrency.Symbol;

            if(m_parent.SaleState != PointOfSale.SellingState.NotSelling && m_parent.CurrentSale != null)
            {
                m_panelLastTotal.Hide();

                // Update whether we allow return mode.
                if (m_parent.CurrentSale.ItemCount == 0 && m_parent.CurrentMenu.Name != Resources.B3SessionString)//US4380
                    EnableReturnButton(true);
                else
                    EnableReturnButton(false);

                // Update the totals.
                SetSubtotal(m_parent.CurrentSale.CalculateSubtotal());
                SetPointsSubtotal(m_parent.CurrentSale.CalculateTotalPointsToRedeem());
                // Rally TA7465
                SetTaxesAndFees(m_parent.CurrentSale.CalculateTaxes() + m_parent.CurrentSale.CalculateFees());
                SetPrepaidTotal(m_parent.CurrentSale.CalculatePrepaidAmount() + m_parent.CurrentSale.CalculatePrepaidTaxTotal());
                SetTotal(m_parent.CurrentSale.CalculateTotal(true));
                // END: TA7465

                SetCouponTotal(m_parent.CurrentSale.CalculateNonTaxableCouponTotal());

                SetPointsTotal(m_parent.CurrentSale.CalculateTotalPointsToRedeem());
                SetPointsEarned(m_parent.CurrentSale.CalculateTotalEarnedPoints() + m_parent.CurrentSale.CalculatePointsEarnedFromQualifyingSubtotal()); // PDTS 583 - Rework Tender

                // Update the device fees.
                PutDeviceFeesOnDeviceButtons();
            }
            else
            {
                EnableReturnButton(true);
                DisplayReturnMode();

                SetSubtotal(0M);
                SetPointsSubtotal(0M);
                SetTaxesAndFees(0M);
                SetTotal(0M);
                SetPointsTotal(0M);
                SetPointsEarned(0M);
                SetCouponTotal(0M);
                SetPrepaidTotal(0);

                // Update the device fees to nothing.
                PutDeviceFeesOnDeviceButtons();

                ClearKeypad(); // TTP 50063
            }
        }

        /// <summary>
        /// Sets the device fee text on device buttons, if applicable.
        /// </summary>
        protected void PutDeviceFeesOnDeviceButtons()
        {
            if (!m_parent.Settings.UseSystemMenuForUnitSelection)
            {
                m_keypad.Option1Text = DeviceFeeText((int)m_keypad.Option1Tag);
                m_keypad.Option2Text = DeviceFeeText((int)m_keypad.Option2Tag);
                m_keypad.Option3Text = DeviceFeeText((int)m_keypad.Option3Tag);
                m_keypad.Option4Text = DeviceFeeText((int)m_keypad.Option4Tag);
            }
            else
            {
                ButtonEntry be = m_buttonMenu.GetButtonEntry(UnitSelectId);

                if (be != null)
                {
                    if (be.UserDataAsInt == 0 && !m_availableDevices.Exists(d => d.Id == PackDeviceId))
                    {
                        be.UseCustomAlignment = false;
                        be.Text = Resources.UnitSelectionButtonText;
                        m_buttonMenu.Refresh(UnitSelectId);
                        Application.DoEvents();
                    }
                    else
                    {
                        be.UseCustomAlignment = true;
                        be.Text = " " + DeviceFeeText(be.UserDataAsInt);
                        m_buttonMenu.Refresh(UnitSelectId);
                        Application.DoEvents();
                    }
                }
            }
        }

        private string DeviceFeeText(int deviceID)
        {
            string result = string.Empty;

            if (!m_allDeviceFeesAreZero)
            {
                decimal fee = m_parent.CurrentOperator.GetDeviceFeeById(deviceID);

                if (m_parent.CurrentSale != null && !m_parent.CurrentSale.ChargeDeviceFee)
                    fee = 0;

                if (fee == 0M)
                {
                    if (m_parent.Settings.ShowFreeOnDeviceButtonsWithFeeOfZero && deviceID != 0 && m_parent.CurrentSale != null && !m_parent.CurrentSale.AddOnSale)
                        result = Resources.Free;
                }
                else
                {
                    result = fee.ToString("F2", CultureInfo.CurrentCulture);
                }
            }

            return result;
        }

        // END: US2018

        /// <summary>
        /// Updates the list of line items in the sales list.
        /// </summary>
        protected void UpdateSaleList()
        {
            m_saleList.BeginUpdate();
            m_saleListUpdateCount++;

            try
            {
                // Remove all the old items.
                m_saleList.Items.Clear();

                //get the payment processor if we need to send the detail to the PIN pad
                GTI.EliteCreditCards.Interfaces.IEliteCreditCardProcessor processor = null;

                if (m_parent.Settings.PaymentProcessingEnabled && m_parent.Settings.DisplayItemDetailOnPinPad)
                    processor = EliteCreditCardFactory.Instance;

                if (processor != null)
                    processor.ClearPinPadLineItems();

                if (m_parent != null && m_parent.CurrentSale != null) //we have a sale 
                {
                    SaleItem[] saleItems = m_parent.CurrentSale.GetItems();

                    if (processor != null) //send detail to PIN pad
                    {
                        int l = 1;
                        
                        foreach (SaleItem si in saleItems)
                        {
                            PinPadLineItem ppli = new PinPadLineItem("", si.Quantity, si.TotalPrice, l, l++, si.ToStringForPaymentProcessing());
                            processor.AddPinPadLineItem(ppli);
                        }
                    }

                    // Add the sale items to the list.
                    if (m_parent.Settings.PrintReceiptSortedByPackageType) //sort our items to match the receipt
                    {
                        string[] groupNames = SaleItem.GetSortGroupNames();

                        for (int order = (int)SaleItem.SortOrderType.ElectronicAndPaper; order >= (int)SaleItem.SortOrderType.BottomOfList; order--)
                        {
                            bool validatedGroup = saleItems.ToList().Exists(s => s.SortOrder == order && s.IsPackageItem && s.Package.IsValidated);
                            
                            foreach (SaleItem si in saleItems.Where(s => s.SortOrder == order))
                            {
                                bool showAutoValidations = m_parent.Settings.ProductValidationMaxQuantity != 0 && si.Session != null && si.Session.IsMaxValidationEnabled;

                                if (!showAutoValidations && si.IsDefaultValidationPackage)
                                    continue;

                                if (groupNames[order] != string.Empty)
                                {
                                    m_saleList.Items.Add(groupNames[order] + (validatedGroup?" Validated":""));
                                    groupNames[order] = string.Empty;
                                }

                                m_saleList.Items.Add(si);
                            }
                        }                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          
                    }
                    else //just dump the items out as they are while filtering out unwanted validation items
                    {
                        foreach(SaleItem si in saleItems)
                        {
                            bool showAutoValidations = m_parent.Settings.ProductValidationMaxQuantity != 0 && si.Session != null && si.Session.IsMaxValidationEnabled;

                            if (!showAutoValidations && si.IsDefaultValidationPackage)
                                continue;

                            m_saleList.Items.Add(si);
                        }
                    }

                    if (m_addDiscountTextToScreenReceipt) //stairstep discounts are at the bottom of the screen receipt
                    {
                        if (m_stairStepDiscountText.Count > 0)
                        {
                            m_saleList.Items.Add("  "); //needs to be two spaces - used in flextender code to filter out the following text
                            m_saleList.Items.AddRange(m_stairStepDiscountText.ToArray());
                        }
                    }
                }

                if (m_addDiscountTextToScreenReceipt) //stairstep discounts are at the bottom of the screen receipt
                {
                    if (m_stairStepDiscountText.Count > 0) //scroll down to the bottom
                    {
                        m_saleList.TopIndex = m_saleList.Items.Count - 1;

                        m_saleList.SelectedIndex = -1;
                    }
                }
            }
            catch (Exception)
            {
            }

            m_saleList.EndUpdate();
            m_saleListUpdateCount--;

            UpdateQuantitiesOnButtons();

            //update the quantities on the kiosk buttons
            if (m_simpleKioskForm != null)
            {
                if (m_parent.CurrentSale != null)
                {
                    List<SaleItem> saleItems = m_parent.CurrentSale.GetItems().ToList();

                    foreach (ImageButton imageButton in m_simpleKioskForm.MenuButtons)
                    {
                        if (imageButton.Tag != null && imageButton.Tag is PackageButton)
                        {
                            PackageButton p = imageButton.Tag as PackageButton;

//                            int sum = saleItems.FindAll(i => i != null && i.IsPackageItem && i.Package.Id == p.Package.Id && i.Package.DisplayText == p.Package.DisplayText && i.Package.ReceiptText == p.Package.ReceiptText).Sum(n => n.Quantity);
                            int sum = saleItems.FindAll(i => i != null && i.IsPackageItem && i.Package.KindOfEquals(p.Package)).Sum(n => n.Quantity);

                            imageButton.SecondaryText = "Qty: " + sum.ToString();
                            imageButton.UseSecondaryText = sum != 0;
                        }
                    }
                }
                else
                {
                    foreach (ImageButton imageButton in m_simpleKioskForm.MenuButtons)
                        imageButton.UseSecondaryText = false;
                }
            }
        }

        private void UpdateQuantitiesOnButtons()
        {
            //update the quantities on the POS buttons
            if (m_parent.Settings.ShowQuantityOnMenuButtons && (m_parent.WeAreNotAPOSKiosk || m_parent.WeAreAnAdvancedPOSKiosk))
            {
                if (m_parent.CurrentSale != null)
                {
                    List<SaleItem> saleItems = m_parent.CurrentSale.SaleItems;
                    int sum = 0;

                    foreach (ImageButton imageButton in m_menuButtonsPanel.Controls)
                    {
                        sum = 0;

                        if (imageButton.Tag != null && imageButton.Tag is PackageButton)
                        {
                            PackageButton p = imageButton.Tag as PackageButton;

                            sum = saleItems.FindAll(i => i != null && i.Session == m_parent.CurrentSession && i.IsPackageItem && i.Package.KindOfEquals(p.Package)).Sum(n => n.Quantity);

                            imageButton.SecondaryText = "Qty: " + sum.ToString();
                            imageButton.UseSecondaryText = sum != 0;
                        }
                        else
                        {
                            imageButton.UseSecondaryText = false;
                        }
                    }
                }
                else
                {
                    foreach (ImageButton imageButton in m_menuButtonsPanel.Controls)
                        imageButton.UseSecondaryText = false;
                }
            }
        }

        /// <summary>
        /// Get the objects from the sale list items.
        /// </summary>
        /// <returns>An object array from the sale list item collection.</returns>
        public object[] GetSaleListRaw()
        {
            object[] result = new object[m_saleList.Items.Count];
            int index = 0;

            foreach (object o in m_saleList.Items)
                result[index++] = o;

            return result;
        }

        /// <summary>
        /// Gets the text from the sale list box.
        /// </summary>
        /// <returns>A string array of the text in the sale list box.</returns>
        public string[] GetSaleList(bool markNonTaxedCoupons = false)
        {
            string[] text = new string[m_saleList.Items.Count];

            for (int x = 0; x < m_saleList.Items.Count; x++)
            {
                SaleItem item = m_saleList.Items[x] as SaleItem;

                if (markNonTaxedCoupons && !m_parent.Settings.CouponTaxable)
                {
                    if (item != null)
                    {
                        if (item.IsCoupon)
                        {
                            text[x] = '\x00' + item.ToStringForSaleScreen(m_parent.Settings.LongPOSDescriptions);
                        }
                        else
                        {
                            bool mergeValidation = !(m_parent.Settings.ProductValidationMaxQuantity != 0 && item.Session.IsMaxValidationEnabled);

                            text[x] = item.ToStringForSaleScreen(m_parent.Settings.LongPOSDescriptions, mergeValidation);
                        }
                    }
                    else
                    {
                        text[x] = m_saleList.Items[x].ToString();
                    }
                }
                else
                {
                    if (item != null)
                    {
                        bool mergeValidation = !(m_parent.Settings.ProductValidationMaxQuantity != 0 && item.Session.IsMaxValidationEnabled);

                        text[x] = item.ToStringForSaleScreen(m_parent.Settings.LongPOSDescriptions, mergeValidation);
                    }
                    else
                    {
                        text[x] = m_saleList.Items[x].ToString();
                    }
                }
            }

            return text;
        }

        // Rally TA7465
        /// <summary>
        /// Sets and shows/hides the subtotal in the totals area of the selling form.
        /// </summary>
        /// <param name="subtotal">The subtotal value.</param>
        protected void SetSubtotal(decimal subtotal)
        {
            m_orderSubtotal.Text = m_parent.DefaultCurrency.FormatCurrencyString(subtotal);
            m_orderSubtotalLabel.Visible = m_orderSubtotal.Visible = subtotal != 0M;
        }

        /// <summary>
        /// Sets and shows/hides the points redeemed subtotal in the totals area of the selling form.
        /// </summary>
        /// <param name="subtotal">The points subtotal value.</param>
        protected void SetPointsSubtotal(decimal subtotal)
        {
            if (subtotal != 0M)
                m_pointsSubtotal.Text = SaleItem.FormattedPoints(subtotal, false, true, true);
            else
                m_pointsSubtotal.Text = string.Empty;
        }

        /// <summary>
        /// Sets and shows/hides the taxes and fees in the totals area of the selling form.
        /// </summary>
        /// <param name="taxes">The taxes and fees value.</param>
        public void SetTaxesAndFees(decimal taxes)
        {
            m_salesTax.Text = m_parent.DefaultCurrency.FormatCurrencyString(taxes);
            m_salesTaxLabel.Visible = m_salesTax.Visible = taxes != 0M;
        }

        /// <summary>
        /// Sets the coupon total label.
        /// Sets and shows/hides the non-taxed coupon total in the totals area of the selling form.
        /// </summary>
        /// <param name="total">The total coupon value.</param>
        protected void SetCouponTotal(decimal total)
        {
            m_couponTotal.Text = m_parent.DefaultCurrency.FormatCurrencyString(total);
            m_couponTotalLabel.Visible = m_couponTotal.Visible = total != 0M;
        }

        /// <summary>
        /// Sets and shows/hides the prepaid amount in the totals area of the selling form.
        /// </summary>
        /// <param name="amount">Prepaid amount.</param>
        public void SetPrepaidTotal(decimal amount)
        {
            m_prepaidTotal.Text = m_parent.DefaultCurrency.FormatCurrencyString(-amount);
            m_prepaidTotalLabel.Visible = m_prepaidTotal.Visible = amount != 0;
        }

        /// <summary>
        /// Sets the total in the totals area of the selling form.
        /// </summary>
        /// <param name="total">The total due.</param>
        public void SetTotal(decimal total)
        {
            m_total.Text = m_parent.CurrentCurrency.FormatCurrencyString(total);
        }
        // END: TA7465

        /// <summary>
        /// Sets and shows/hides the redeemed points total in the totals area of the selling form.
        /// </summary>
        /// <param name="total">The total redeemed points.</param>
        protected void SetPointsTotal(decimal total)
        {
            if (total != 0M)
                m_pointsTotal.Text = SaleItem.FormattedPoints(total, false, true, true);
            else
                m_pointsTotal.Text = string.Empty;
        }

        // PTDS 583
        /// <summary>
        /// Sets and shows/hides the points earned.
        /// </summary>
        /// <param name="change">The points earned value.</param>
        public void SetPointsEarned(decimal points)
        {
            m_pointsEarned.Text = SaleItem.FormattedPoints(points, false, true, false);
            m_pointsEarnedLabel.Visible = m_pointsEarned.Visible = points != 0 && m_parent.CurrentSale != null && m_parent.CurrentSale.Player != null;
        }

        /// <summary>
        /// Enables or disables the buttons that do not have to do 
        /// with selling.
        /// </summary>
        /// <param name="enable">true to enabled them; otherwise false.</param>
        protected void EnabledNonSaleButtons(bool enable)
        {
            m_buttonMenu.SetEnabled(PaperExchangeId, enable);
            m_buttonMenu.SetEnabled(RegisterClosingReportId, enable);
            m_buttonMenu.SetEnabled(RegisterSalesReportId, enable);
            m_buttonMenu.SetEnabled(AdjustBankId, enable);
            m_buttonMenu.SetEnabled(CloseBankId, enable); // FIX: DE1930
            m_buttonMenu.SetEnabled(ReprintLastReceiptId, enable);
            m_buttonMenu.SetEnabled(PaperUsageId, enable);
        }

        /// <summary>
        /// Enables or disables the return sale button.
        /// </summary>
        /// <param name="enable">true to enabled it; otherwise false.</param>
        protected void EnableReturnButton(bool enable)
        {
            m_buttonMenu.SetEnabled(ReturnId, enable);
        }

        /// <summary>
        /// Enables or disables the buttons that are related with selling.
        /// </summary>
        /// <param name="enable">true to enable the buttons; 
        /// otherwise false.</param>
        protected void EnableSaleButtons(bool enable)
        {
            m_removeLineButton.Enabled = enable;

            // TTP 50114
            m_buttonMenu.SetEnabled(PlayerMgmtId, enable);
            m_buttonMenu.SetEnabled(ScanCardId, enable);
            m_buttonMenu.SetEnabled(RepeatLastSaleId, enable);
            m_buttonMenu.SetEnabled(ViewReceiptsId, enable);
            // PDTS 571
            m_buttonMenu.SetEnabled(QuantitySaleId, enable);
            m_buttonMenu.SetEnabled(PaperRangeScannerId, enable); 

            if(enable)
                m_buttonMenu.SetEnabled(PlayerCouponsId, m_parent.CurrentCouponForm != null && m_parent.CurrentCouponForm.LoadPlayerComp(true));
            else
                m_buttonMenu.SetEnabled(PlayerCouponsId, enable);

            if(enable)
                m_buttonMenu.SetEnabled(ReprintPlayerLastReceiptId, m_parent.CurrentSale != null && m_parent.CurrentSale.Player != null);
            else
                m_buttonMenu.SetEnabled(ReprintPlayerLastReceiptId, enable);
        }

        //US3509
        /// <summary>
        /// Enables or disables the validation button
        /// </summary>
        /// <param name="enable">true to enable the buttons; 
        /// otherwise false.</param>
        protected void EnableValidateButton(bool enable)
        {
            if (!m_parent.Settings.EnabledProductValidation)
            {
                return;
            }

            if (m_parent.CurrentSale != null && m_parent.CurrentSale.GetItems().ToList().Exists(i => i != null && i.IsPackageItem && i.Package.RequiresValidation))
                enable = false;

            m_buttonMenu.SetEnabled(ValidateId, enable);
        }

        //US4467
        /// <summary>
        /// Sets the validation button.
        /// </summary>
        /// <param name="enable">if set to <c>true</c> [enable].</param>
        protected void SetValidationButton(bool enable)
        {
            try
            {
                if (!m_parent.Settings.EnabledProductValidation)
                    return;

                m_parent.ValidationEnabled = !enable;
                ValidationClick(null, new EventArgs());
            }
            catch (Exception ex)
            {
                m_parent.Log(Resources.EnableValidationFailed + ex.Message, LoggerLevel.Severe);
                m_parent.ShowMessage(this, m_displayMode, string.Format(CultureInfo.CurrentCulture, Resources.EnableValidationFailed, ex.Message));
            }
        }

        /// <summary>
        /// Changes the form to indicate a return or a normal sale.
        /// </summary>
        public void DisplayReturnMode()
        {
            this.SuspendLayout();

            if(m_parent.CurrentSale != null && m_parent.CurrentSale.IsReturn)
            {
                if(m_displayMode is NormalDisplayMode)
                {
                    m_panelMain.BackgroundImage = Resources.POSBack1024Red;
                    m_versionLabel.BackColor = ReturnBackColor;
                }
                else if (m_displayMode is WideDisplayMode)
                {
                    m_panelMain.BackgroundImage = Resources.POSBackWideRed;
                    m_versionLabel.BackColor = ReturnBackColor;
                }
                else
                {
                    m_panelMain.BackgroundImage = Resources.POSBack800Red;
                    m_versionLabel.BackColor = SmallVersionReturnBackColor;
                }

                m_sessionLabel.BackColor = ReturnBackColor;
                m_quantityLabel.BackColor = ReturnBackColor;
                m_itemLabel.BackColor = ReturnBackColor;
                m_pointsLabel.BackColor = ReturnBackColor;
                m_subtotalLabel.BackColor = ReturnBackColor;
                m_saleItemUpButton.BackColor = ReturnBackColor;
                m_saleItemDownButton.BackColor = ReturnBackColor;
                m_removeLineButton.BackColor = ReturnBackColor;
                m_startOverButton.BackColor = ReturnBackColor;
                m_saleTop.BackColor = ReturnBackColor;
                m_saleBottom.BackColor = ReturnBackColor;
                m_currencyButton.BackColor = ReturnBackColor; // Rally TA7464
                
                m_keypad.BackColor = ReturnBackColor;

                m_pageNavigator.BackColor = ReturnBackColor;

                m_menuButtonsPanel.BackColor = ReturnBackColor;
            }
            else
            {
                if(m_displayMode is NormalDisplayMode)
                {
                    m_panelMain.BackgroundImage = Resources.POSBack1024;
                    m_versionLabel.BackColor = VersionNormalBackColor;
                }
                else if (m_displayMode is WideDisplayMode)
                {
                    m_panelMain.BackgroundImage = Resources.POSBackWide;
                    m_versionLabel.BackColor = VersionNormalBackColor;
                }
                else
                {
                    m_panelMain.BackgroundImage = Resources.POSBack800;
                    m_versionLabel.BackColor = SmallVersionNormalBackColor;
                }

                m_sessionLabel.BackColor = SaleNormalBackColor;
                m_quantityLabel.BackColor = SaleNormalBackColor;
                m_itemLabel.BackColor = SaleNormalBackColor;
                m_pointsLabel.BackColor = SaleNormalBackColor;
                m_subtotalLabel.BackColor = SaleNormalBackColor;
                m_saleItemUpButton.BackColor = SaleNormalBackColor;
                m_saleItemDownButton.BackColor = SaleNormalBackColor;
                m_removeLineButton.BackColor = SaleNormalBackColor;
                m_startOverButton.BackColor = SaleNormalBackColor;
                m_saleTop.BackColor = SaleNormalBackColor;
                m_saleBottom.BackColor = SaleNormalBackColor;
                m_currencyButton.BackColor = SaleNormalBackColor; // Rally TA7464
                
                m_keypad.BackColor = SaleNormalBackColor;

                m_pageNavigator.BackColor = MenuNormalBackColor;

                m_menuButtonsPanel.BackColor = MenuNormalBackColor;
            }

            this.ResumeLayout();
        }

        //US4380: (US4337) POS: Display B3 Menu
        /// <summary>
        /// Changes the form to indicate a return or a normal sale.
        /// </summary>
        public void DisplayB3SessionMode()
        {
            if (!m_parent.Settings.EnableB3Management)
            {
                return;
            }

            this.SuspendLayout();

            if (m_parent.IsB3Sale)
            {
                EnabledNonSaleButtons(false);
                EnableReturnButton(false);
                EnableSaleButtons(false);
                EnableValidateButton(false);
                m_removeLineButton.Enabled = true;

                if (m_displayMode is NormalDisplayMode)
                {
                    m_panelMain.BackgroundImage = Resources.POSBack1024Green;
                    m_versionLabel.BackColor = B3SessionBackColor;
                }
                else if (m_displayMode is WideDisplayMode)
                {
                    m_panelMain.BackgroundImage = Resources.POSBackWideGreen;
                    m_versionLabel.BackColor = B3SessionBackColor;
                }
                else
                {
                    m_panelMain.BackgroundImage = Resources.POSBack800Green;
                    m_versionLabel.BackColor = B3SessionBackColor;
                }

                m_sessionLabel.BackColor = B3SessionBackColor;
                m_quantityLabel.BackColor = B3SessionBackColor;
                m_itemLabel.BackColor = B3SessionBackColor;
                m_pointsLabel.BackColor = B3SessionBackColor;
                m_subtotalLabel.BackColor = B3SessionBackColor;
                m_saleItemUpButton.BackColor = B3SessionBackColor;
                m_saleItemDownButton.BackColor = B3SessionBackColor;
                m_removeLineButton.BackColor = B3SessionBackColor;
                m_startOverButton.BackColor = B3SessionBackColor;
                m_saleTop.BackColor = B3SessionBackColor;
                m_saleBottom.BackColor = B3SessionBackColor;
                m_currencyButton.BackColor = B3SessionBackColor; // Rally TA7464

                m_keypad.BackColor = B3SessionBackColor;

                m_pageNavigator.BackColor = B3SessionBackColor;

                m_menuButtonsPanel.BackColor = B3SessionBackColor;
            }
            else
            {
                //DE12973
                if (m_parent.Settings.EnabledProductValidation)
                {
                    DisplayValidationDisabledMode(!m_parent.ValidationEnabled);    
                }
                else
                {
                    //set background to normal 
                    if (m_displayMode is NormalDisplayMode)
                    {
                        m_panelMain.BackgroundImage = Resources.POSBack1024;
                        m_versionLabel.BackColor = VersionNormalBackColor;
                    }
                    else if (m_displayMode is WideDisplayMode)
                    {
                        m_panelMain.BackgroundImage = Resources.POSBackWide;
                        m_versionLabel.BackColor = VersionNormalBackColor;
                    }
                    else
                    {
                        m_panelMain.BackgroundImage = Resources.POSBack800;
                        m_versionLabel.BackColor = SmallVersionNormalBackColor;
                    }

                    m_sessionLabel.BackColor = SaleNormalBackColor;
                    m_sessionLabel.ForeColor = Color.White;
                    m_quantityLabel.BackColor = SaleNormalBackColor;
                    m_quantityLabel.ForeColor = Color.White;
                    m_itemLabel.BackColor = SaleNormalBackColor;
                    m_itemLabel.ForeColor = Color.White;
                    m_pointsLabel.BackColor = SaleNormalBackColor;
                    m_pointsLabel.ForeColor = Color.White;
                    m_subtotalLabel.BackColor = SaleNormalBackColor;
                    m_subtotalLabel.ForeColor = Color.White;
                    m_saleItemUpButton.BackColor = SaleNormalBackColor;
                    m_saleItemDownButton.BackColor = SaleNormalBackColor;
                    m_removeLineButton.BackColor = SaleNormalBackColor;
                    m_startOverButton.BackColor = SaleNormalBackColor;
                    m_saleTop.BackColor = SaleNormalBackColor;
                    m_saleBottom.BackColor = SaleNormalBackColor;
                    m_currencyButton.BackColor = SaleNormalBackColor; // Rally TA7464

                    m_keypad.BackColor = SaleNormalBackColor;

                    m_pageNavigator.BackColor = MenuNormalBackColor;

                    m_menuButtonsPanel.BackColor = MenuNormalBackColor;
                }
            }

            this.ResumeLayout();

            HandleTotalOrNoSaleButton();
        }

        //US4466
        /// <summary>
        /// Changes the form to indicate validation mode or a normal sale.
        /// </summary>
        public void DisplayValidationDisabledMode(bool disabled)
        {
            if (!m_parent.Settings.EnabledProductValidation)
            {
                return;
            }

            this.SuspendLayout();

            if (disabled)
            {
                if (m_displayMode is NormalDisplayMode)
                {
                    m_panelMain.BackgroundImage = Resources.POSBack1024Yellow;
                    m_versionLabel.BackColor = ValidationDisabledBackColor;
                }
                else if (m_displayMode is WideDisplayMode)
                {
                    m_panelMain.BackgroundImage = Resources.POSBackWideYellow;
                    m_versionLabel.BackColor = ValidationDisabledBackColor;
                }
                else
                {
                    m_panelMain.BackgroundImage = Resources.POSBack800Yellow;
                    m_versionLabel.BackColor = ValidationDisabledBackColor;
                }

                m_sessionLabel.BackColor = ValidationDisabledBackColor;
                m_sessionLabel.ForeColor = Color.Black;
                m_quantityLabel.BackColor = ValidationDisabledBackColor;
                m_quantityLabel.ForeColor = Color.Black;
                m_itemLabel.BackColor = ValidationDisabledBackColor;
                m_itemLabel.ForeColor = Color.Black;
                m_pointsLabel.BackColor = ValidationDisabledBackColor;
                m_pointsLabel.ForeColor = Color.Black;
                m_subtotalLabel.BackColor = ValidationDisabledBackColor;
                m_subtotalLabel.ForeColor = Color.Black;
                m_saleItemUpButton.BackColor = ValidationDisabledBackColor;
                m_saleItemDownButton.BackColor = ValidationDisabledBackColor;
                m_removeLineButton.BackColor = ValidationDisabledBackColor;
                m_startOverButton.BackColor = ValidationDisabledBackColor;
                m_saleTop.BackColor = ValidationDisabledBackColor;
                m_saleBottom.BackColor = ValidationDisabledBackColor;
                m_currencyButton.BackColor = ValidationDisabledBackColor; // Rally TA7464

                m_keypad.BackColor = ValidationDisabledBackColor;

                m_pageNavigator.BackColor = ValidationDisabledBackColor;

                m_menuButtonsPanel.BackColor = ValidationDisabledBackColor;
            }
            else
            {
                if (m_displayMode is NormalDisplayMode)
                {
                    m_panelMain.BackgroundImage = Resources.POSBack1024;
                    m_versionLabel.BackColor = VersionNormalBackColor;
                }
                else if (m_displayMode is WideDisplayMode)
                {
                    m_panelMain.BackgroundImage = Resources.POSBackWide;
                    m_versionLabel.BackColor = VersionNormalBackColor;
                }
                else
                {
                    m_panelMain.BackgroundImage = Resources.POSBack800;
                    m_versionLabel.BackColor = SmallVersionNormalBackColor;
                }

                m_sessionLabel.BackColor = SaleNormalBackColor;
                m_sessionLabel.ForeColor = Color.White;
                m_quantityLabel.BackColor = SaleNormalBackColor;
                m_quantityLabel.ForeColor = Color.White;
                m_itemLabel.BackColor = SaleNormalBackColor;
                m_itemLabel.ForeColor = Color.White;
                m_pointsLabel.BackColor = SaleNormalBackColor;
                m_pointsLabel.ForeColor = Color.White;
                m_subtotalLabel.BackColor = SaleNormalBackColor;
                m_subtotalLabel.ForeColor = Color.White;
                m_saleItemUpButton.BackColor = SaleNormalBackColor;
                m_saleItemDownButton.BackColor = SaleNormalBackColor;
                m_removeLineButton.BackColor = SaleNormalBackColor;
                m_startOverButton.BackColor = SaleNormalBackColor;
                m_saleTop.BackColor = SaleNormalBackColor;
                m_saleBottom.BackColor = SaleNormalBackColor;
                m_currencyButton.BackColor = SaleNormalBackColor; // Rally TA7464

                m_keypad.BackColor = SaleNormalBackColor;

                m_pageNavigator.BackColor = MenuNormalBackColor;

                m_menuButtonsPanel.BackColor = MenuNormalBackColor;

                if (m_parent.CurrentSale != null && m_parent.CurrentSale.IsReturn)
                    DisplayReturnMode();
            }

            this.ResumeLayout();
        }

        /// <summary>
        /// Sets whether to show the device buttons or the big button.
        /// </summary>
        /// <param name="showDevices">Whether to show the device buttons or 
        /// the big button.</param>
        protected void ShowTotalButtons(bool showDevices)
        {
            if (m_parent.Settings.UseSystemMenuForUnitSelection)
            {
                m_keypad.ShowOptionButtons = false;
                m_buttonMenu.SetEnabled(UnitSelectId, showDevices);
            }
            else
            {
                m_keypad.ShowOptionButtons = showDevices;
            }
        }

        // Rally TA7729
        // Rally US765
        /// <summary>
        /// Enables or disabled the device buttons based on the passed in values.
        /// </summary>
        /// <param name="Pack">true to enable the Pack button; otherwise 
        /// false.</param>
        /// <param name="Traveler">true to enable the Traveler button; otherwise 
        /// false.</param>
        /// <param name="Tracker">true to enable the Tracker button; otherwise 
        /// false.</param>
        /// <param name="Fixed">true to enable the Fixed button; otherwise 
        /// false.</param>
        /// <param name="Explorer">true to enable the Explorer button; otherwise 
        /// false.</param>
        /// <param name="Traveler2">true to enable the Traveler II button;
        /// otherwise false.</param>
        protected void EnableDeviceButtons(bool Pack, bool Traveler, bool Tracker, bool Fixed, bool Explorer, bool Traveler2)
        {
            if (!m_parent.Settings.UseSystemMenuForUnitSelection)
            {
                int devId = (int)m_keypad.Option1Tag;
                if (PackDeviceId == devId)
                    m_keypad.Option1Enabled = Pack;
                else if (Device.Traveler.Id == devId)
                    m_keypad.Option1Enabled = Traveler;
                else if (Device.Tracker.Id == devId)
                    m_keypad.Option1Enabled = Tracker;
                else if (Device.Fixed.Id == devId)
                    m_keypad.Option1Enabled = Fixed;
                else if (Device.Explorer.Id == devId)
                    m_keypad.Option1Enabled = Explorer;
                else if (Device.Traveler2.Id == devId) // PDTS 964
                    m_keypad.Option1Enabled = Traveler2;

                devId = (int)m_keypad.Option2Tag;

                if (PackDeviceId == devId)
                    m_keypad.Option2Enabled = Pack;
                else if (Device.Traveler.Id == devId)
                    m_keypad.Option2Enabled = Traveler;
                else if (Device.Tracker.Id == devId)
                    m_keypad.Option2Enabled = Tracker;
                else if (Device.Fixed.Id == devId)
                    m_keypad.Option2Enabled = Fixed;
                else if (Device.Explorer.Id == devId)
                    m_keypad.Option2Enabled = Explorer;
                else if (Device.Traveler2.Id == devId) // PDTS 964
                    m_keypad.Option2Enabled = Traveler2;

                devId = (int)m_keypad.Option3Tag;

                if (PackDeviceId == devId)
                    m_keypad.Option3Enabled = Pack;
                else if (Device.Traveler.Id == devId)
                    m_keypad.Option3Enabled = Traveler;
                else if (Device.Tracker.Id == devId)
                    m_keypad.Option3Enabled = Tracker;
                else if (Device.Fixed.Id == devId)
                    m_keypad.Option3Enabled = Fixed;
                else if (Device.Explorer.Id == devId)
                    m_keypad.Option3Enabled = Explorer;
                else if (Device.Traveler2.Id == devId) // PDTS 964
                    m_keypad.Option3Enabled = Traveler2;

                devId = (int)m_keypad.Option4Tag;

                if (PackDeviceId == devId)
                    m_keypad.Option4Enabled = Pack;
                else if (Device.Traveler.Id == devId)
                    m_keypad.Option4Enabled = Traveler;
                else if (Device.Tracker.Id == devId)
                    m_keypad.Option4Enabled = Tracker;
                else if (Device.Fixed.Id == devId)
                    m_keypad.Option4Enabled = Fixed;
                else if (Device.Explorer.Id == devId)
                    m_keypad.Option4Enabled = Explorer;
                else if (Device.Traveler2.Id == devId) // PDTS 964
                    m_keypad.Option4Enabled = Traveler2;
            }
        }

        // PDTS 583
        /// <summary>
        /// Sets the keypad's number mode property.
        /// </summary>
        /// <param name="mode">The mode to set the keypad to.</param>
        protected void SetKeypadNumberMode(Keypad.NumberMode mode)
        {
            m_keypad.NumberDisplayMode = mode;

            if (mode == Keypad.NumberMode.Integer)
                m_keypad.MaxDigits = 4;
            else
                m_keypad.ResetMaxDigits();

            m_keypad.CurrencySymbolForeColor = Color.Yellow;
            m_keypad.Use00AsDecimalPoint = mode == Keypad.NumberMode.Currency && !m_parent.Settings.Use00ForCurrencyEntry;
        }

        /// <summary>
        /// Sets the "Start Over" button as a red button with an X if used to clear the sale
        /// or an orange button with an X if used to return to the sale from tendering.
        /// </summary>
        /// <param name="isStartOver">true = red button, false = orange button</param>
        private void SetStartOverButton(bool isStartOver)
        {
            if (isStartOver)
            {
                m_startOverButton.ImageIcon = null;

                if (m_displayMode is CompactDisplayMode)
                {
                    m_startOverButton.ImageNormal = Resources.SmallStartOverUp;
                    m_startOverButton.ImagePressed = Resources.SmallStartOverDown;
                }
                else
                {
                    m_startOverButton.ImageNormal = Resources.StartOverUp;
                    m_startOverButton.ImagePressed = Resources.StartOverDown;
                }
            }
            else //this button backs us out of tendering
            {
                m_startOverButton.ImageIcon = Resources.BackSymbol;
                m_startOverButton.ImageNormal = Resources.DarkOrangeButtonUp;
                m_startOverButton.ImagePressed = Resources.DarkOrangeButtonDown;
            }
        }

        /// <summary>
        /// Sets the keypad's initial value 
        /// </summary>
        protected void SetKeypadInitialValue()
        {
            m_keypad.InitialValue = this.m_parent.CurrentSale.CalculateTotal(true);
        }

        /// <summary>
        /// Sets the keypad's value 
        /// </summary>
        public void SetKeypadValue(decimal amount)
        {
            m_keypad.Value = amount;
        }

        /// <summary>
        /// Clears the keypad's value 
        /// </summary>
        public void ClearSellingKeypadValue()
        {
            m_keypad.Clear();
        }

        /// <summary>
        /// Sets the text on the total button.
        /// </summary>
        /// <param name="text">The string desired on the total button.</param>
        protected void SetTotalButtonText(string text)
        {
            SetTotalButtonText(text, Resources.BlueButtonUp, Resources.BlueButtonDown);
        }

        /// <summary>
        /// Sets the text and image on the total button.
        /// </summary>
        /// <param name="text">The string desired on the total button.</param>
        /// <param name="upImage">Up image for the button.</param>
        /// <param name="downImage">Pressed image for the button.</param>
        protected void SetTotalButtonText(string text, Image upImage, Image downImage)
        {
            m_keypad.BigButtonText = text;

            if (m_keypad.BigButtonImageNormal != upImage)
                m_keypad.BigButtonImageNormal = upImage;

            if (m_keypad.BigButtonImagePressed != downImage)
                m_keypad.BigButtonImagePressed = downImage;
        }

        /// <summary>
        /// Returns the system time in a custom format.
        /// </summary>
        /// <returns>A string representing the system time.</returns>
        private string GetSystemTime()
        {
            if(m_displayMode is NormalDisplayMode || m_displayMode is WideDisplayMode)
                return DateTime.Now.ToShortDateString() + " - " + DateTime.Now.ToLongTimeString();
            else
                return DateTime.Now.ToShortDateString() + Environment.NewLine + DateTime.Now.ToLongTimeString();
        }

        /// <summary>
        /// Handles the Date Time Timer's tick event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the 
        /// event data.</param>
        private void OnDateTimeTimerTick(object sender, EventArgs e)
        {
            string time = GetSystemTime();

            if(m_systemTimeLabel.Text != time) //if the displayed time is incorrect, update it
                m_systemTimeLabel.Text = time;

            if (m_parent.WeAreAPOSKiosk) //see if we are inactive
            {
                if (m_parent.GuardianHasUsSuspended || (KioskForm != null && KioskForm.State == SimpleKioskForm.KioskState.GetB3Funding) || (KioskForm != null && KioskForm.KioskIsClosed) || m_parent.TenderingScreenActive || m_parent.CouponScreenActive || m_parent.HelpScreenActive)
                {
                    m_nothingHappeningAndIdleSince = DateTime.Now;
                    NotIdle(false, true);
                }
                else
                {
                    DateTime now = DateTime.Now;
                    bool forcingTimeout = m_idleSince < now - TimeSpan.FromMinutes(30);
                    bool weHaveSomethingToCancel = (m_parent.HaveBingoMenu && m_menuList.SelectedIndex != m_parent.IndexOfFirstBingoMenu) ||
                                                   (m_parent.Settings.EnabledProductValidation && !m_parent.ValidationEnabled) ||
                                                   !m_parent.IsSaleEmpty ||
                                                   (KioskForm != null && KioskForm.State == SimpleKioskForm.KioskState.GetWhatWeAreSelling) || 
                                                   (m_parent.CurrentSale != null);

                    if (weHaveSomethingToCancel)
                    {
                        m_nothingHappeningAndIdleSince = DateTime.Now;

                        TimeSpan idleFor = now - m_idleSince;

                        if (idleFor > TimeSpan.FromMilliseconds(KioskIdleLimitInSeconds / 3 * 2000))
                        {
                            //if we are forcing a timeout, adjust the time
                            if (forcingTimeout)
                            {
                                now = now - TimeSpan.FromHours(1);

                                if (m_simpleKioskForm != null)
                                {
                                    m_simpleKioskForm.ProgressBar.Value = m_kioskTimeoutProgress.Maximum;
                                    m_simpleKioskForm.ProgressBar.Hide();
                                }
                                else
                                {
                                    m_kioskTimeoutProgress.Value = m_kioskTimeoutProgress.Maximum;
                                    m_kioskTimeoutProgress.Hide();
                                }
                            }
                            else
                            {
                                if (m_simpleKioskForm != null)
                                {
                                    if (!m_simpleKioskForm.ProgressBar.Visible)
                                        m_simpleKioskForm.ProgressBar.Visible = true;

                                    m_simpleKioskForm.ProgressBar.Increment(m_dateTimeTimer.Interval);
                                }
                                else
                                {
                                    if (!m_kioskTimeoutProgress.Visible)
                                        m_kioskTimeoutProgress.Visible = true;

                                    m_kioskTimeoutProgress.Increment(m_dateTimeTimer.Interval);
                                }
                            }

                            int progressValue = 0;
                            int progressMax = 0;

                            if (m_simpleKioskForm != null)
                            {
                                progressValue = m_simpleKioskForm.ProgressBar.Value;
                                progressMax = m_simpleKioskForm.ProgressBar.Maximum;
                            }
                            else
                            {
                                progressValue = m_kioskTimeoutProgress.Value;
                                progressMax = m_kioskTimeoutProgress.Maximum;
                            }

                            if (progressValue >= progressMax)
                            {
                                NotIdle(true); //stop the timer

                                if (POSMessageForm.ShowCustomTwoButton(m_simpleKioskForm != null? (IWin32Window)m_simpleKioskForm : this, m_parent, Resources.KioskIdleQuestion, Resources.KioskIdle, true, 2, Resources.Continue, Resources.CancelSale) == 2)
                                    StartOver(true);

                                NotIdle(); //reset the timer
                            }
                        }
                    }
                    else //nothing to cancel
                    {
                        if (m_parent.WeAreAnAdvancedPOSKiosk && m_simpleKioskForm == null) //handle the attract video
                        {
                            TimeSpan sittingAroundDoingNothing = DateTime.Now - m_nothingHappeningAndIdleSince;

                            if (sittingAroundDoingNothing >= TimeSpan.FromSeconds(KioskIdleLimitInSeconds * 2))
                            {
                                m_dateTimeTimer.Stop();
                                m_parent.MagCardReader.CardSwiped -= CardSwiped;
                                m_parent.BarcodeScanner.BarcodeScanned -= BarcodeScanned;
                                m_playerCardPicture.Image = null;

                                try
                                {
                                    m_simpleKioskForm = new SimpleKioskForm(m_parent, this);
                                    m_simpleKioskForm.StartAttractVideo();
                                    m_simpleKioskForm.ShowDialog(this);
                                    m_simpleKioskForm.Dispose();
                                    m_simpleKioskForm = null;
                                }
                                catch (Exception)
                                {
                                }

                                m_parent.MagCardReader.CardSwiped += new MagneticCardSwipedHandler(CardSwiped);
                                m_parent.BarcodeScanner.BarcodeScanned += new BarcodeScanHandler(BarcodeScanned);
                                m_playerCardPicture.Image = Resources.AnimatedPlayerCard_large_;
                                m_nothingHappeningAndIdleSince = DateTime.Now;
                                m_dateTimeTimer.Start();
                            }
                        }

                        //nothing to do and nobody is using us, don't build up idle time for when we get going
                        NotIdle(false, true);
                    }
                }
            }
        }

        /// <summary>
        /// Handles a menu button click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the 
        /// event data.</param>
        public void MenuButtonClick(object sender, EventArgs e)
        {
            ImageButton button = sender as ImageButton;

            if(button != null && button.Tag != null)
            {
                PackageButton packageButton = button.Tag as PackageButton;
                DiscountButton discountButton = button.Tag as DiscountButton;
                FunctionButton functionButton = button.Tag as FunctionButton;
                B3Button b3QuickSaleButton = button.Tag as B3Button;

                if(packageButton != null)
                {
                    m_parent.CanUpdateMenus = false; // PDTS 964

                    int quantity = 1;

                    if(m_keypad.Value > 1)
                        quantity = (int)m_keypad.Value;

                    try
                    {
                        packageButton.Click(this, quantity); // PDTS 693
                    }
                    catch(POSUserCancelException)
                    {
                    }
                    catch(ServerCommException)
                    {
                        m_parent.ServerCommFailed();
                        return;
                    }
                    catch(POSException ex)
                    {
                        m_parent.ShowMessage(this, m_displayMode, ex.Message);
                    }

                    m_parent.CanUpdateMenus = true; // PDTS 964
                    ClearKeypad();
                }
                else if(discountButton != null)
                {
                    // FIX: DE2957
                    int quantity = 1;

                    if(m_keypad.Value > 1)
                        quantity = (int)m_keypad.Value;

                    try
                    {
                        discountButton.Click(this, quantity); // PDTS 693
                        // END: DE2957
                    }
                    catch(ServerCommException)
                    {
                        m_parent.ServerCommFailed();
                        return;
                    }
                    catch(POSException ex)
                    {
                        m_parent.ShowMessage(this, m_displayMode, ex.Message);
                    }
                }
                else if(functionButton != null)
                {
                    try
                    {
                        functionButton.Click(this, null); // PDTS 693
                    }
                    catch(ServerCommException)
                    {
                        m_parent.ServerCommFailed();
                        return;
                    }
                    catch(POSException ex)
                    {
                        m_parent.ShowMessage(this, m_displayMode, ex.Message);
                    }
                }
                else if (b3QuickSaleButton != null)
                {
                    try
                    {
                        b3QuickSaleButton.Click(this, null); // PDTS 693
                    }
                    catch (ServerCommException)
                    {
                        m_parent.ServerCommFailed();
                        return;
                    }
                    catch (POSException ex)
                    {
                        m_parent.ShowMessage(this, m_displayMode, ex.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Navigator's page changed event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the 
        /// event data.</param>
        private void NavigatorPageChanged(object sender, EventArgs e)
        {
            LoadMenuPage(m_parent.CurrentMenu, (byte)m_pageNavigator.CurrentPage);

            if (KioskForm == null)
                this.Focus();
            else
                KioskForm.Focus();
        }

        /// <summary>
        /// Handles the menu up button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the 
        /// event data.</param>
        private void MenuUpButtonClick(object sender, EventArgs e)
        {
            NotIdle();
            m_menuList.Up();
        }

        /// <summary>
        /// Handles the menu down button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the 
        /// event data.</param>
        private void MenuDownButtonClick(object sender, EventArgs e)
        {
            NotIdle();
            m_menuList.Down();
        }

        private void AutoDiscountInfoUpButtonClick(object sender, EventArgs e)
        {
            NotIdle();
            m_stairstepDiscountBox.Up();
        }

        private void AutoDiscountInfoDownButtonClick(object sender, EventArgs e)
        {
            NotIdle();
            m_stairstepDiscountBox.Down();
        }

        public bool SelectB3Menu(bool selectIt)
        {
            bool success = false;

            POSMenuListItem B3Session = null;
            POSMenuListItem normalSession = null;

            foreach(object obj in m_menuList.Items)
            {
                POSMenuListItem item = obj as POSMenuListItem;

                if (item != null)
                {
                    if (B3Session == null && item.Session.ProgramName == Resources.B3SessionString)
                        B3Session = item;

                    if (normalSession == null && item.Session.ProgramName != Resources.B3SessionString)
                        normalSession = item;
                }
            }

            if (selectIt)
            {
                if (B3Session != null)
                {
                    if (m_menuList.SelectedItem != B3Session)
                        m_menuList.SelectedItem = B3Session;

                    success = true;
                }
            }
            else
            {
                if (normalSession != null)
                {
                    if(m_menuList.SelectedItem != normalSession)
                        m_menuList.SelectedItem = normalSession;

                    success = true;
                }
            }

            return success;
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event for the menu list.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the 
        /// event data.</param>
        private void SelectedMenuChanged(object sender, EventArgs e)
        {
            NotIdle();

            try
            {
                //save the player in case we clear the sale
                Player player = null;

                if (m_parent.CurrentSale != null && m_parent.CurrentSale.Player != null)
                    player = m_parent.CurrentSale.Player;

                //US4380: (US4337) POS: Display B3 Menu
                var selectedMenuItem = m_menuList.SelectedItem as POSMenuListItem;

                //check for null
                if (selectedMenuItem != null && m_parent.CurrentMenu != null)
                {
                    //if we are switching to or from a B3 session, prompt to clear current sale
                    if ((selectedMenuItem.Session.ProgramName == Resources.B3SessionString &&
                        m_parent.CurrentMenu.Name != Resources.B3SessionString) 
                        ||
                        (selectedMenuItem.Session.ProgramName != Resources.B3SessionString &&
                        m_parent.CurrentMenu.Name == Resources.B3SessionString)
                        ||
                        // if we are selecting a different gaming date, prompt to clear sale
                        selectedMenuItem.Session.GamingDate != m_parent.CurrentSession.GamingDate 
                        ||
                        //if we are selecting a different pre-sale session, then prompt to clear sale
                        selectedMenuItem.Session.IsPreSale && m_parent.CurrentSession.SessionNumber != selectedMenuItem.Session.SessionNumber) 
                    {
                        //check to see if anything was sold
                        if (m_parent.CurrentSale != null && m_parent.CurrentSale.ItemCount > 0)
                        {
                            var dialogResults = m_parent.ShowMessage(this, m_displayMode, Resources.B3TransactionWarning, POSMessageFormTypes.YesNo);

                            if (dialogResults == DialogResult.Yes)
                            {
                                m_parent.ClearSale();
                            }
                            else
                            {
                                //do not switch selected items
                                m_menuList.SelectedIndex = m_parent.CurrentMenuIndex;
                                return;
                            }
                        }
                        else
                        {
                            m_parent.ClearSale();
                        }
                    }
                }

                if (m_parent.SetCurrentMenu(m_menuList.SelectedIndex))
                {
                    LoadMenu(m_parent.CurrentMenu, 1);
                }

                if (m_parent.CurrentSale == null && player != null) //a sale with a player was cleared, start a sale and set the player
                {
                    m_parent.StartSale(false);
                    m_parent.CurrentSale.SetPlayer(player, true, true);
                    m_parent.SellingForm.SetPlayer();
                }

                m_parent.UpdateAutoDiscounts();
                UpdateSaleList();

                DisplayB3SessionMode();//US4380: (US4337) POS: Display B3 Menu

                if (m_parent.WeAreAnAdvancedPOSKiosk && m_parent.IsB3Sale) //use the Kiosk B3 sale window
                {
                    B3KioskForm B3 = new B3KioskForm(m_parent);

                    B3.ShowDialog(this);
                }
            }
            catch (POSException ex)
            {
                m_parent.ShowMessage(this, m_displayMode, string.Format(CultureInfo.CurrentCulture, Resources.ChangeMenuFailed, ex.Message));
            }
        }
        
        /// <summary>
        /// Handles the player info up button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the 
        /// event data.</param>
        private void PlayerInfoUpButtonClick(object sender, EventArgs e)
        {
            NotIdle();
            m_playerInfoList.Up();
        }

        /// <summary>
        /// Handles the player info down button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the 
        /// event data.</param>
        private void PlayerInfoDownButtonClick(object sender, EventArgs e)
        {
            NotIdle();
            m_playerInfoList.Down();
        }

        /// <summary>
        /// Handles the sale item up button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the
        /// event data.</param>
        private void SaleItemUpButtonClick(object sender, EventArgs e)
        {
            NotIdle();
            m_saleList.Up();
        }

        /// <summary>
        /// Handles the sale item down button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the 
        /// event data.</param>
        private void SaleItemDownButtonClick(object sender, EventArgs e)
        {
            NotIdle();
            m_saleList.Down();
        }

        /// <summary>
        /// Selects the sale item with the specified line number.
        /// </summary>
        /// <param name="lineNumber">The line number to select.</param>
        public void SelectSaleItem(int lineNumber)
        {
            NotIdle();

            int index = 0;

            try
            {
                SaleItem si = m_parent.CurrentSale.GetItems()[lineNumber];
                index = m_saleList.Items.IndexOf(si);
            }
            catch (Exception)
            {
            }

            m_saleList.SelectedIndex = index;
        }

        /// <summary>
        /// Handles the remove line button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the
        /// event data.</param>
        private void RemoveLineClick(object sender, EventArgs e)
        {
            NotIdle();

            if (m_saleList.SelectedIndex != -1)
                m_parent.RemoveSaleItem(m_saleList.SelectedIndex);

            if (m_saleList.Items.Count == 0 && m_parent.CurrentSale != null && m_parent.CurrentSale.Player == null && m_parent.CurrentSale.GetCurrentTenders().Count == 0)
                StartOver(true); //clear the sale
        }

        public void SelectAndRemoveLine(int line)
        {
            m_saleList.SelectedIndex = line;
            RemoveLineClick(null, new EventArgs());
        }

        /// <summary>
        /// Removes the given package from the sale with no regard for alt prices and discounts.
        /// </summary>
        /// <param name="package"></param>
        public void RemovePackageForNonAdvancedPOSKiosk(Package package)
        {
            if (m_parent.CurrentSale != null)
            {
                List<SaleItem> saleItemsToRemove = m_parent.CurrentSale.SaleItems.FindAll(s => s != null && s.IsPackageItem && s.Package.KindOfEquals(package));

                foreach (SaleItem item in saleItemsToRemove)
                {
                    try
                    {
                        m_parent.RemoveSaleItem(item);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        public void ProcessSale()
        {
            // Are we starting the sale or finishing it?
            if (!(m_parent.WeAreAPOSKiosk && m_parent.IsB3Sale) && (m_parent.SaleState == PointOfSale.SellingState.Selling || m_parent.SaleState == PointOfSale.SellingState.NotSelling))
            {
                Total((m_parent.CurrentSale != null? m_parent.CurrentSale.Device.Id:0)); // No device.
            }
            else
            {
                m_lblLastTotal.Text = m_total.Text; //save the total for if we make the sale

                //Get player card PIN if needed for third party player tracking
                if (m_parent.CurrentSale.Player != null && m_parent.Settings.ThirdPartyPlayerInterfaceID != 0) //we have a player and a third party interface
                {
                    if (m_parent.Settings.ThirdPartyPlayerInterfaceUsesPIN && !m_parent.CurrentSale.Player.WeHaveThePlayerCardPIN) //we use a PIN and don't have it
                    {
                        if ((m_parent.Settings.ThirdPartyPlayerInterfaceNeedPINForRating && m_parent.CurrentSale.CalculateQualifyingSubtotalForPoints() != 0)
                            || (m_parent.Settings.ThirdPartyPlayerInterfaceNeedPINForRedemption && m_parent.CurrentSale.CalculateTotalPointsToRedeem() != 0)
                            || (m_parent.Settings.ThirdPartyPlayerInterfaceNeedPINForRedemptionVoid && m_parent.CurrentSale.CalculateTotalEarnedPoints() != 0)
                           )
                        {
                            UpdatePlayerPoints(true);

                            if (!m_parent.CurrentSale.Player.WeHaveThePlayerCardPIN)
                                return; //no PIN, no sale
                        }
                    }
                }

                //US4118, US4119 
                //DE12749 Electronic CBB 
                //is North Dakota Sales 
                if (m_parent.Settings.NorthDakotaSalesMode && (m_parent.CurrentSale.HasElectronicBingo || m_parent.CurrentSale.HasElectronicCrystalBall))
                {
                    //if pin is set
                    if (m_parent.CurrentSale.Player != null &&
                        m_parent.CurrentSale.Player.PinNumber.Length == DataSizes.PasswordHash &&
                        !m_parent.CurrentSale.Player.PinNumber.All(b => b == 0))
                    {
                        //set  receipt pin
                        m_parent.CurrentSale.ReceiptPin = m_parent.CurrentSale.Player.PinNumber;
                    }
                    //Prompt for password
                    else
                    {
                        //display pin prompt
                        var pinDialog = new TakePIN(m_parent.Settings.DisplayMode);
                        var results = pinDialog.ShowDialog();

                        if (results == DialogResult.Cancel || string.IsNullOrEmpty(pinDialog.PIN)) //DE12758
                        {
                            return;
                        }

                        m_parent.CurrentSale.ReceiptPin = SecurityHelper.HashPassword(pinDialog.PIN);

                        //if player exsists save pin to player account
                        if (m_parent.CurrentSale.Player != null)
                        {
                            m_parent.CurrentSale.UpdatePlayerPin = true;
                            m_parent.CurrentSale.Player.PinNumber = m_parent.CurrentSale.ReceiptPin;
                        }
                    }
                }

                // Do tender?
                // PDTS 583
                bool allowSale = true;
                decimal tenderAmount = 0M;

                if (m_parent.Settings.EnableFlexTendering && !m_parent.IsB3Sale && m_parent.CurrentSale != null)
                    tenderAmount = m_parent.CurrentCurrency.ConvertFromDefaultCurrencyToThisCurrency(m_parent.CurrentSale.CalculateAmountTendered()); //total tendered as current currency

                // TTP 50114
                if (allowSale && m_parent.SaleState == PointOfSale.SellingState.Tendering)
                {
                    tenderAmount = m_keypad.Value; //current currency

                    // Rally TA7465
                    if (m_parent.Settings.Tender != TenderSalesMode.AllowNegative)
                    {
                        decimal total = m_parent.CurrentSale.CalculateTotal(true);

                        if(total < 0M) //negative sale
                        {
                            m_parent.CanUpdateMenus = false; // PDTS 964

                            if (m_parent.Settings.Tender == TenderSalesMode.WarnNegative &&
                               m_parent.ShowMessage(this, m_displayMode, Resources.NegativeSaleWarning, POSMessageFormTypes.YesNo) == DialogResult.No)
                            {
                                allowSale = false;
                                ClearKeypad();
                            }
                            else if (m_parent.Settings.Tender == TenderSalesMode.PreventNegative)
                            {
                                m_parent.ShowMessage(this, m_displayMode, Resources.TenderErrorNegativeSale);
                                allowSale = false;
                                ClearKeypad();
                            }

                            m_parent.CanUpdateMenus = true; // PDTS 964
                        }
                        else if (tenderAmount < total) //not enough tendered to cover sale
                        {
                            m_parent.CanUpdateMenus = false; // PDTS 964

                            if (m_parent.Settings.Tender == TenderSalesMode.WarnNegative &&
                               m_parent.ShowMessage(this, m_displayMode, Resources.TenderWarning, POSMessageFormTypes.YesNo) == DialogResult.No)
                            {
                                allowSale = false;
                                ClearKeypad();
                            }
                            else if (m_parent.Settings.Tender == TenderSalesMode.PreventNegative)
                            {
                                m_parent.ShowMessage(this, m_displayMode, Resources.TenderError);
                                allowSale = false;
                                ClearKeypad();
                            }

                            m_parent.CanUpdateMenus = true; // PDTS 964
                        }
                    }
                }

                // US4028
                if (allowSale && m_parent.Settings.CheckCardCountsPerProduct == false && m_parent.CheckMaxCards() == false)
                {
                    allowSale = false;
                    m_parent.ShowMessage(this, m_displayMode, Resources.MaxCardLimitReached_2);
                }

                if (allowSale)
                {
                    //US4382: (US4337) POS: B3 Open sale
                    //US5192/DE13363
                    if (m_parent.IsB3Sale)
                    {
                        if (m_parent.WeAreAPOSKiosk) //the amount tendered is the amount we have credits for
                        {
                            m_parent.CurrentSale.AmountTendered = 0;

                            foreach (SaleItem item in m_parent.CurrentSale.SaleItems.FindAll(i => i.B3Credit != null))
                            {
                                m_parent.CurrentSale.AmountTendered += item.B3Credit.Amount * item.Quantity;
                            }
                        }
                        else
                        {
                            m_parent.CurrentSale.AmountTendered = tenderAmount;
                        }

                        m_parent.B3AddSale();
                        m_parent.ShowSaleStatusForm(this);
                        
                        
                        
                        bool error= CheckForError();

                        if (m_parent.WeAreAPOSKiosk && error)
                            throw new Exception(m_parent.LastAsyncException.Message);

                        m_parent.OpenCashDrawer(2); //drawer 2 is the B2 cash drawer

                        return; // B3 Sale is not related to EDGE Sale. This is as far as it goes. 
                    }

                    if (m_parent.CurrentSale.GetCurrentTenders().Count == 0) //no tenders - add a cash tender for the "tendered" amount (just prepare the tender record for quantity sales)
                    {
                        if (m_parent.CurrentSale.Quantity == 1) //tender for quantity sale is added later
                        {
                            //this is the first tender, get the registerReceiptID and transaction number for our sale
                            if (m_parent.CurrentSale.Id == 0)
                            {
                                SaleTender startSale = new SaleTender();
                                startSale.TenderTypeID = 0;

                                m_parent.StartAddTenderToTable(startSale);
                                m_parent.ShowWaitForm(this);

                                m_parent.CurrentSale.Id = startSale.RegisterReceiptID;
                            }
                        }

                        decimal tender = tenderAmount;
                        
                        if(tender == 0M)
                            tender = m_parent.CurrentSale.CalculateTotal(true); //total tendered as current currency

                        SaleTender st = new SaleTender(0, m_parent.CurrentSale.Id, DateTime.Now, TenderType.Cash, 0, TransactionType.Sale, m_parent.CurrentCurrency.ISOCode, tender, m_parent.CurrentCurrency.ConvertFromThisCurrencyToDefaultCurrency(tender), m_parent.CurrentSale.CalculateTaxes(), string.Empty, string.Empty, "Cash", 0, m_parent.CurrentCurrency.ExchangeRate, string.Empty, string.Empty, string.Empty);
                        TenderItem newItem = new TenderItem(m_parent.CurrentSale, m_parent.CurrentCurrency.ConvertFromThisCurrencyToDefaultCurrency(tender), m_parent.CurrentCurrency, tender);
                        newItem.SaleTenderInfo = st;
                        newItem.Type = TenderType.Cash;

                        if (m_parent.CurrentSale.Quantity == 1)
                        {
                            m_parent.CurrentSale.AddTender(newItem);

                            //save the tender
                            m_parent.StartAddTenderToTable(st);
                            m_parent.ShowWaitForm(this);

                            if (m_parent.LastAsyncException != null)
                            {
                                POSMessageForm.Show(this, m_parent, "Tender record could not be written.\r\n\r\n" + m_parent.LastAsyncException.Message);
                            }
                        }
                        else //save for quantity sale
                        {
                            m_parent.CurrentSale.QuantitySaleTenderItem = newItem;
                        }
                    }

                    m_parent.CurrentSale.AmountTendered = m_parent.CurrentCurrency.ConvertFromThisCurrencyToDefaultCurrency(tenderAmount); //in default currency

                    m_parent.MagCardReader.EndReading(); // PDTS 1064

                    try
                    {
                        // Rally US507 - CBB Favorites
                        // Ask to save favorites if CBB.
                        if (m_parent.Settings.EnableCBBFavorites && m_parent.CurrentSale.Player != null && !m_parent.CurrentSale.SaveCBBAsFavorites && m_parent.CurrentSale.HasSavableCrystalBall)
                        {
                            if (m_parent.WeAreAPOSKiosk)
                            {
                                NotIdle(true);
                                if (POSMessageForm.ShowCustomTwoButton(KioskForm != null ? (IWin32Window)KioskForm : (IWin32Window)this, m_parent, Resources.SaveCBBFavoritesAtKiosk, "", true, 2, Resources.SaveCBBCardsAsFavorites, Resources.NoThanks, KioskShortIdleLimitInSeconds * 1000) == 1)
                                    m_parent.CurrentSale.MarkCBBAsFavorites();
                                NotIdle();
                            }
                            else
                            {
                                if (m_parent.ShowMessage(this, m_displayMode, Resources.SaveCBBFavorites, POSMessageFormTypes.YesNo) == DialogResult.Yes)
                                    m_parent.CurrentSale.MarkCBBAsFavorites();
                            }
                        }

                        if(m_parent.WeAreNotAPOSKiosk)
                            m_panelLastTotal.Show(); //show the sale total for people who figure the change themselves

                        // Spawn a new thread to make the sale and wait until done.
                        m_parent.KioskChangeDispensingFailed = false;
                        NotIdle(true); //stop the timer
                        m_parent.StartAddSale(); 
                        m_parent.ShowSaleStatusForm(this); // Block until we are done.

                        Application.DoEvents();

                        if(m_parent.WeAreAPOSKiosk && m_parent.KioskChangeDispensingFailed)
                            m_parent.ShowMessage(this, m_displayMode, Resources.KioskWarningAdditionalPayment);

                        // FIX: DE3139 - Reprompt for pack if using PPP.
                        if (CheckForError() && m_parent.Settings.UsePrePrintedPacks && m_parent.CurrentSale != null)
                        {
                            // Was it a pack in use error?
                            if (m_parent.LastAsyncException.InnerException != null &&
                               m_parent.LastAsyncException.InnerException is ServerException &&
                               (AddBingoCardSaleReturnCode)((ServerException)m_parent.LastAsyncException.InnerException).ReturnCode == AddBingoCardSaleReturnCode.PackInUse)
                            {
                                try
                                {
                                    m_parent.CurrentSale.PromptForPrePrintedPackInfo(true);
                                }
                                catch
                                {
                                }
                            }
                        }
                        // END: DE3139
                    }
                    catch (Exception ex)
                    {
                        m_parent.Log("Failed to finish the sale: " + ex.Message, LoggerLevel.Severe);
                        m_parent.ShowMessage(this, m_displayMode, ex.Message);
                    }

                    GetPagesReadyForNextSale();

                    m_parent.MagCardReader.BeginReading(); // PDTS 1064

                    NotIdle();
                }
            }
        }

        #region Guardian routines

        //****************************************************************************************************
        //vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv
        //vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv Guardian Routines vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv
        //vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv

        /// <summary>
        /// Stores a decimal value in volatile memory and writes the value as 16 bytes of data to 
        /// non-volatile memory if the delayedWrite flag is not set.
        /// </summary>
        /// <param name="blockNumber">Block to write the value to.</param>
        /// <param name="value">Decimal number to write.</param>
        /// <param name="delayedWrite">True=save the value in live memory until flushed to NVRAM.</param>
        public void WriteNVRAMUserDecimal(NVRAMUserDecimal blockNumber, decimal value, bool delayedWrite = false)
        {
            if (m_parent.Guardian != null)
            {
                m_userDecimalNVRamCopy[(int)blockNumber] = new Tuple<decimal, bool>(value, delayedWrite);
                m_userDecimalNVRamCopy[(int)NVRAMUserDecimal.WriteTimeStamp] = new Tuple<decimal, bool>((decimal)DateTime.Now.Ticks, delayedWrite);

                if (!delayedWrite)
                {
                    m_parent.Guardian.NVMWriteDecimal((int)blockNumber, value);
                    m_parent.Guardian.NVMWriteDecimal((int)NVRAMUserDecimal.WriteTimeStamp, (decimal)DateTime.Now.Ticks);
                }
            }
        }

        public void ClearNVRAMTransactionUserDecimals()
        {
            if (m_parent.Guardian != null)
            {
                bool needsToBeCleared = false;

                for (NVRAMUserDecimal x = NVRAMUserDecimal.FirstTransactional; x <= NVRAMUserDecimal.LastTransactional; x++)
                {
                    if (m_userDecimalNVRamCopy[(int)x].Item1 != 0 || m_userDecimalNVRamCopy[(int)x].Item2)
                        needsToBeCleared = true;
                }

                if (needsToBeCleared)
                {
                    m_parent.Guardian.NVMClearDecimal((int)NVRAMUserDecimal.FirstTransactional, (int)NVRAMUserDecimal.LastTransactional, ref m_userDecimalNVRamCopy);
                    m_userDecimalNVRamCopy[(int)NVRAMUserDecimal.WriteTimeStamp] = new Tuple<decimal, bool>((decimal)DateTime.Now.Ticks, false);
                    m_parent.Guardian.NVMWriteDecimal((int)NVRAMUserDecimal.WriteTimeStamp, (decimal)DateTime.Now.Ticks);
                }
            }
        }

        /// <summary>
        /// Adds a decimal value in volatile memory and writes the result as 16 bytes of data to 
        /// non-volatile memory if the delayedWrite flag is not set.
        /// </summary>
        /// <param name="blockNumber">Block to add the value to.</param>
        /// <param name="value">Decimal number to add to the value in live memory.</param>
        /// <param name="delayedWrite">True=save the result in live memory until flushed to NVRAM.</param>
        public void IncNVRAMUserDecimal(NVRAMUserDecimal blockNumber, decimal value = 1, bool delayedWrite = false)
        {
            if (m_parent.Guardian != null)
            {
                m_userDecimalNVRamCopy[(int)blockNumber] = new Tuple<decimal, bool>(m_userDecimalNVRamCopy[(int)blockNumber].Item1 + value, delayedWrite);
                m_userDecimalNVRamCopy[(int)NVRAMUserDecimal.WriteTimeStamp] = new Tuple<decimal, bool>((decimal)DateTime.Now.Ticks, delayedWrite);

                if (!delayedWrite)
                {
                    m_parent.Guardian.NVMWriteDecimal((int)blockNumber, m_userDecimalNVRamCopy[(int)blockNumber].Item1);
                    m_parent.Guardian.NVMWriteDecimal((int)NVRAMUserDecimal.WriteTimeStamp, (decimal)DateTime.Now.Ticks);
                }
            }
        }

        /// <summary>
        /// Reads 16 bytes of non-volatile memory as a decimal from the given block and saves the value in live memory.
        /// </summary>
        /// <param name="blockNumber">Block to read.</param>
        /// <returns>Decimal value read from NVRAM.</returns>
        public decimal ReadNVRAMUserDecimal(NVRAMUserDecimal blockNumber)
        {
            if (m_parent.Guardian != null)
            {
                decimal value = m_parent.Guardian.NVMReadDecimal((int)blockNumber);

                m_userDecimalNVRamCopy[(int)blockNumber] = new Tuple<decimal, bool>(value, false);
                return value;
            }
            else
            {
                m_userDecimalNVRamCopy[(int)blockNumber] = new Tuple<decimal, bool>(0, false);
                return 0;
            }
        }

        /// <summary>
        /// Flushes all or a specified block of delayed write live memory to non-volatile memory.
        /// </summary>
        /// <param name="blockNumber">Block to write or ALL.</param>
        public void FlushNVRAMUserDecimal(NVRAMUserDecimal blockNumber = NVRAMUserDecimal.All)
        {
            if (m_parent.Guardian != null)
            {
                if (blockNumber == NVRAMUserDecimal.All) //flush all
                {
                    //write the whole array in one chunk (one write to NV ram)
                    m_parent.Guardian.NVMWriteDecimal((int)NVRAMUserDecimal.First, (int)NVRAMUserDecimal.Last, m_userDecimalNVRamCopy);
                    m_parent.Guardian.NVMWriteDecimal((int)NVRAMUserDecimal.WriteTimeStamp, (decimal)DateTime.Now.Ticks);
                   
                    //mark the elements as written
                    for (int userBlock = (int)NVRAMUserDecimal.First; userBlock <= (int)NVRAMUserDecimal.Last; userBlock++)
                    {
                        if (m_userDecimalNVRamCopy[userBlock].Item2) //delayed
                            m_userDecimalNVRamCopy[userBlock] = new Tuple<decimal, bool>(m_userDecimalNVRamCopy[userBlock].Item1, false);
                    }
                }
                else //flush the requested meter
                {
                    if (m_userDecimalNVRamCopy[(int)blockNumber].Item2) //delayed
                    {
                        m_parent.Guardian.NVMWriteDecimal((int)blockNumber, m_userDecimalNVRamCopy[(int)blockNumber].Item1);
                        m_userDecimalNVRamCopy[(int)blockNumber] = new Tuple<decimal, bool>(m_userDecimalNVRamCopy[(int)blockNumber].Item1, false);
                    }
                }
            }
        }

        /// <summary>
        /// Dispense money.  Uses available payment methodes like a bill dispenser or SAS.
        /// </summary>
        /// <param name="amountToVend">Amount to be dispensed.</param>
        /// <returns>Amount that was not dispensed.</returns>
        public decimal VendChange(decimal amountToVend, bool noB3 = false)
        {
            if (amountToVend == 0M)
                return 0;

            NotIdle(true);

            decimal amountNotVended = amountToVend;
            POSSettings.OptionsForGivingChange option = m_parent.Settings.ChangeDispensing;

            //if we can't use B3 for change and B3 is an option for giving change, revert to normal change dispensing.
            if (noB3 || !m_parent.B3SessionActive)
                option = POSSettings.OptionsForGivingChange.Normal;

            //if we are not allowed to sell B3 and we have the ability to give B3 credit and we are defined
            //to allow change as B3 with add on, change the option to use straight B3 credit
            if (option == POSSettings.OptionsForGivingChange.B3CreditWithAddOn && !m_parent.Settings.AllowB3OnKiosk)
                option = POSSettings.OptionsForGivingChange.B3Credit;

            if (option == POSSettings.OptionsForGivingChange.B3CreditWithAddOnOrNormal && !m_parent.Settings.AllowB3OnKiosk)
                option = POSSettings.OptionsForGivingChange.B3CreditOrNormal;

            this.Invoke(new MethodInvoker(delegate()
            {
                switch (option)
                {
                    case POSSettings.OptionsForGivingChange.B3Credit: //just do the B3 credit
                    {
                        m_parent.B3AddCredit(amountToVend);
                        m_parent.WaitForSaleStatusFormSecondaryThread();
                        
                        if (!CheckForError()) //no error, we dispensed it
                            amountNotVended = 0;
                    }
                    break;

                    case POSSettings.OptionsForGivingChange.B3CreditOrNormal: //ask user if he wants change as B3
                    {
                        if (POSMessageForm.ShowCustomTwoButton(KioskForm != null? (IWin32Window)KioskForm : this, m_parent, "                 Would you like your change applied to B3 play?                 ", "", true, 1, "B3 Play", "Cash") == 1)
                        {
                            m_parent.B3AddCredit(amountToVend);
                            m_parent.WaitForSaleStatusFormSecondaryThread();

                            if (!CheckForError()) //no error, we dispensed it
                                amountNotVended = 0;
                        }
                        else
                        {
                            if (m_parent.Guardian != null) //tell the Guardian to dispense the money
                                amountNotVended = DispenseTheChange(amountToVend, noB3);
                        }
                    }
                    break;

                    case POSSettings.OptionsForGivingChange.B3CreditWithAddOn: //enter B3 sale with the change and allow more to be added
                    {
                        //prepare for a follow-up B3 sale starting with the change
                        GiveChangeAsB3Credit = true;
                        ChangeToGiveAsB3Credit = amountToVend;

                        //finish sale as if all the change were dispensed
                        amountNotVended = 0;
                    }
                    break;

                    case POSSettings.OptionsForGivingChange.B3CreditWithAddOnOrNormal:
                    {
                        if (POSMessageForm.ShowCustomTwoButton(KioskForm != null ? (IWin32Window)KioskForm : this, m_parent, "                 Would you like your change applied to B3 play?                 ", "", true, 1, "B3 Play", "Cash") == 1)
                        {
                            //prepare for a follow-up B3 sale starting with the change
                            GiveChangeAsB3Credit = true;
                            ChangeToGiveAsB3Credit = amountToVend;

                            //finish sale as if all the change were dispensed
                            amountNotVended = 0;
                        }
                        else
                        {
                            if (m_parent.Guardian != null) //tell the Guardian to dispense the money
                                amountNotVended = DispenseTheChange(amountToVend, noB3);
                        }
                    }
                    break;

                    default:
                    case POSSettings.OptionsForGivingChange.Normal:
                    {
                        if (m_parent.Guardian != null) //tell the Guardian to dispense the money
                            amountNotVended = DispenseTheChange(amountToVend, noB3);
                    }
                    break;
                }
            }));

            NotIdle();
            return amountNotVended;
        }

        /// <summary>
        /// Dispenses money through the Guardian and will attempt to issue credit to B3
        /// if available and not all the money was dispensed through the Guardian.
        /// </summary>
        /// <param name="amountToDispense">Amount to be dispensed.</param>
        /// <param name="noB3">True=override to avoid issueing credit through B3 (like when we are refunding a failed B3 sale).</param>
        /// <returns>Amount NOT dispensed.</returns>
        private decimal DispenseTheChange(decimal amountToDispense, bool noB3 = false)
        {
            decimal amountNotDispensed = amountToDispense;

            if (m_parent.WeAreAPOSKiosk && m_parent.Settings.KioskTestWithoutDispenser)
            {
                DialogResult result = DialogResult.Ignore;

                this.Invoke(new MethodInvoker(delegate()
                {
                    POSNumericInputForm nif = new POSNumericInputForm(m_parent);
                    nif.Description = "Enter amount dispensed\r\n(whole $s for bill dispenser)";
                    nif.DescriptionFont = new System.Drawing.Font(nif.DescriptionFont.FontFamily, 10f, FontStyle.Bold); 
                    nif.UseDecimalKey = true;
                    nif.Password = false;
                    nif.MaxIdleTime = KioskShortIdleLimitInSeconds;
                    nif.TopMost = true;

                    result = DialogResult.Ignore;
                    amountNotDispensed = amountToDispense;

                    do
                    {
                        nif.TextResult = amountToDispense.ToString();
                        amountNotDispensed = amountToDispense;
                        result = nif.ShowDialog(KioskForm);
                        Application.DoEvents();

                        if (result == DialogResult.OK)
                            amountNotDispensed = amountToDispense - nif.DecimalResult;

                    } while (result == DialogResult.OK && amountNotDispensed < 0);

                    nif.Dispose();
                }));

                //try to finish up with B3 if needed
                if (amountNotDispensed != 0 && !noB3 && m_parent.B3SessionActive)
                {
                    m_parent.B3AddCredit(amountNotDispensed);
                    m_parent.WaitForSaleStatusFormSecondaryThread();

                    if (!CheckForError()) //no error, we dispensed it
                        amountNotDispensed = 0;
                }
            }
            else
            {
                amountNotDispensed = amountToDispense - m_parent.Guardian.DispenseMoney(amountToDispense);

                IncNVRAMUserDecimal(NVRAMUserDecimal.AmountDispensed, amountToDispense - amountNotDispensed);

                //try to finish up with B3 if needed
                if (amountNotDispensed != 0 && !noB3 && m_parent.B3SessionActive)
                {
                    m_parent.B3AddCredit(amountNotDispensed);
                    m_parent.WaitForSaleStatusFormSecondaryThread();

                    if (!CheckForError()) //no error, we dispensed it
                        amountNotDispensed = 0;
                }
            }

            return amountNotDispensed;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Guardian Routines ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        //****************************************************************************************************

        #endregion

        public void DispenseChangeForKiosk()
        {
            decimal changeDue = m_parent.CurrentSale.CalculateChange();

            if (m_parent.WeAreAPOSKiosk && m_parent.LastAsyncException == null && changeDue != 0)
            {
                decimal outstanding = VendChange(changeDue);

                if (outstanding != 0) //we couldn't give all the money back
                {
                    //write a dummy tender record for a reconciliation
                    SaleTender dummyTender = new SaleTender();

                    dummyTender.RegisterReceiptID = m_parent.CurrentSale.Id;
                    dummyTender.TransactionTypeID = TransactionType.Void;
                    dummyTender.ReceiptDescription = "Reconciliation text";

                    string reconciliationText = "\r\n\r\n**************************************\r\n"
                                         + Resources.AdditionalPayment + "\r\n\r\n"
                                         + Resources.OutstandingChangeDue.PadLeft(20) + outstanding.ToString("C").PadLeft(14) + "\r\n"
                                         + "--------------".PadLeft(34) + "\r\n"
                                         + outstanding.ToString("C").PadLeft(34)
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

                    m_parent.CurrentSale.AddTender(new TenderItem(m_parent.CurrentSale, 0, m_parent.CurrentCurrency, 0, dummyTender));

                    m_parent.KioskChangeDispensingFailed = true;
                }
            }
        }

        /// <summary>
        /// Handles the start over button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the
        /// event data.</param>
        private void StartOverClick(object sender, EventArgs e)
        {
            NotIdle();

            bool clearSale = !(m_parent.SaleState == PointOfSale.SellingState.Tendering || m_parent.SaleState == PointOfSale.SellingState.Finishing);

            if (m_parent.WeAreAnAdvancedPOSKiosk && clearSale && m_parent.CurrentSale != null)
            {
                NotIdle(true);

                if (POSMessageForm.ShowCustomTwoButton(m_simpleKioskForm != null ? (IWin32Window)m_simpleKioskForm : this, m_parent, Resources.CancelSaleQuestion, "", true, 1, Resources.Continue, Resources.CancelSale) == 1)
                {
                    NotIdle();
                    return;
                }

                NotIdle();
            }

            //US4439:POS: Abort a transaction
            StartOver(clearSale);
        }

        public void StartOver(bool clearSale)
        {
            if (!clearSale) //get out of tendering and back to selling
            {
                //if we went through the coupon screen we need to go back through it.
                var buttonEntry = m_buttonMenu.GetButtonEntry(PlayerCouponsId);
                if (m_parent.CurrentSale.Player != null && !m_parent.CurrentSale.Player.UsedCouponScreen && buttonEntry != null && buttonEntry.Enabled)
                {
                    if(Total(m_parent.CurrentSale.Device.Id))
                        return;
                }

                m_parent.ReenterSellingMode();

                //DE13005: POS: coupon screen pops up when returning
                //If re-entering selling mode, we need to check if we are returning and update background
                if (m_parent.CurrentSale.IsReturn)
                {
                    DisplayReturnMode();
                }
            }
            else //wipe out the sale
            {
                m_parent.ClearSale();
                DisplayB3SessionMode(); //US4380: (US4337) POS: Display B3 Menu

                m_panelLastTotal.Hide(); //show our sale totals

                m_pointsEarnedLabel.Visible = false;
                m_pointsEarned.Visible = false;

                if (m_parent.WeAreAnAdvancedPOSKiosk)
                {
                    m_playerCardPictureLabel.Visible = true;
                    m_playerCardPicture.Visible = true;
                }

                GetPagesReadyForNextSale();
            }
        }

        private void m_playerCardPictureLabel_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                if (m_parent.Settings.KioskInTestMode) //we are testing, allow right clicking on main kiosk screen's text to shut POS down.
                {
                    try
                    {
                        m_parent.ClosePOS(sender, e);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        private void m_playerCardPictureLabel_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (m_parent.Settings.KioskInTestMode) //we are testing, allow double click to send suspend message
                m_parent.RequestHelpFromGuardian("Test of suspend request from Kiosk.");
        }

        // Rally TA7465
        /// <summary>
        /// Handles the currency button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the
        /// event data.</param>
        public void CurrencyButtonClick(object sender, EventArgs e)
        {
            m_parent.CanUpdateMenus = false;
            m_parent.MagCardReader.EndReading();

            CurrencyForm currForm = new CurrencyForm(m_parent, m_displayMode, Resources.SelectSaleCurrency);
            currForm.ShowDialog(this);

            m_parent.SetCurrentCurrency(currForm.Currency, true);

            currForm.Dispose();

            m_parent.MagCardReader.BeginReading();
            m_parent.CanUpdateMenus = true;
        }

        /// <summary>
        /// Checks to see if the last async. operation returned an exception and 
        /// show a message box if necessary.
        /// </summary>
        /// <returns>true if there was an exception; otherwise false.</returns>
        public bool CheckForError()
        {
            if(m_parent.LastAsyncException != null)
            {
                if(m_parent.LastAsyncException is ServerCommException)
                    m_parent.ServerCommFailed();
                else if(!(m_parent.LastAsyncException is POSUserCancelException))
                    m_parent.ShowMessage(this, m_displayMode, m_parent.LastAsyncException.Message);

                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Handles the return button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the 
        /// event data.</param>
        private void ReturnButtonClick(object sender, EventArgs e)
        {
            try
            {
                if (m_parent.CurrentSale == null)
                {
                    m_parent.StartSale(true);
                    EnableValidateButton(false);
                }
                else
                {
                    m_parent.CurrentSale.SetIsReturn(!m_parent.CurrentSale.IsReturn, true);
                    EnableValidateButton(m_parent.CurrentSale.IsReturn);
                    m_parent.ClearSale(!m_parent.CurrentSale.IsReturn);
                }

                DisplayReturnMode();
                UpdateMenuButtonStates();
            }
            catch(Exception ex)
            {
                m_parent.Log("Failed change return status: " + ex.Message, LoggerLevel.Severe);
                m_parent.ShowMessage(this, m_displayMode, ex.Message);
            }
        }

        /// <summary>
        /// Handles clicking the Coupons menu button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlayerCouponsClick(object sender, EventArgs e)
        {
            if (m_parent.CurrentCouponForm == null)
                return;

            m_parent.CurrentCouponForm.Prep(m_parent.CurrentSale.Device.Id);
            m_parent.CurrentCouponForm.LoadPlayerComp();
            
            DialogResult dlgResult = m_parent.CurrentCouponForm.ShowDialog(this);     //Open coupon redeem UI.
            Application.DoEvents();

            m_parent.CurrentSale.Player.UsedCouponScreen = true; //we just used the coupon screen, remember in case we need to go back from tendering

            if (dlgResult == DialogResult.Abort) //Kiosk timeout
                ForceKioskTimeout(1);
        }

        /// <summary>
        /// Handles the player management button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the 
        /// event data.</param>
        private void PlayerMgmtClick(object sender, EventArgs e)
        {
            Player CurrentPlayer = new Player();
            if (m_parent.CurrentSale != null && m_parent.CurrentSale.Player != null)
            {
                //save current player 
               CurrentPlayer = m_parent.CurrentSale.Player;
            }
            m_parent.StartPlayerCenter();

            if (m_parent.CurrentSale != null && m_parent.CurrentSale.Player != null)
            {
                if (CurrentPlayer.Id != m_parent.CurrentSale.Player.Id)//New player
                {
                    //updating coupons with empty list will clear all coupons from the sale
                    m_parent.RemoveAllCouponsFromSale();
                }
            }
        }

        // TTP 50066
        /// <summary>
        /// Handles the scan player card button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the 
        /// event data.</param>
        private void ScanPlayerCardClick(object sender, EventArgs e)
        {
            InputForm inputForm = new InputForm(m_parent, m_displayMode, true);
            inputForm.Message = (m_parent.Settings.EnableAnonymousMachineAccounts) ? Resources.ScanMachineMessage : Resources.ScanPlayerCardMessage;
            
            //save current player 
            Player CurrentPlayer = new Player();
            if (m_parent.CurrentSale != null && m_parent.CurrentSale.Player != null)
            {
                //save current player 
                CurrentPlayer = m_parent.CurrentSale.Player;
            }

            if (inputForm.ShowDialog(this) == DialogResult.OK)
            {
                if (inputForm.Input != string.Empty && m_parent.Settings.MSRSettingInfo.MSRStart.Contains(inputForm.Input[0])) //assume they swiped the card
                {
                    string swipeData = inputForm.Input;
                    inputForm.Dispose();
                    m_parent.MagCardReader.CardSwipeDetected(sender, 0, swipeData);
                    return;
                }

                GetPlayer(inputForm.Input);
            }


            if (m_parent.CurrentSale != null && m_parent.CurrentSale.Player != null)
            {
                if (CurrentPlayer.Id != m_parent.CurrentSale.Player.Id)//New player
                {
                    m_parent.RemoveAllCouponsFromSale();
                }
            }

            inputForm.Dispose();
        }

        // PDTS 571
        /// <summary>
        /// Handles the quantity sale button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the 
        /// event data.</param>
        private void QuantitySaleClick(object sender, EventArgs e)
        {
            m_parent.CanUpdateMenus = false;

            if(m_parent.PrepareQuantitySale())
            {
                // Prevent sale changes.
                UpdateSystemButtonStates();
                UpdateMenuButtonStates();
            }

            if(!(m_parent.LastAsyncException is ServerCommException))
                m_parent.CanUpdateMenus = true;
        }

        /// <summary>
        /// Handles the repeat last sale button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the 
        /// event data.</param>
        private void RepeatLastSaleClick(object sender, EventArgs e)
        {
            ProcessRepeatSaleThroughServer(0, 0, false);
        }

        /// <summary>
        /// Handles the reprint last receipt button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the 
        /// event data.</param>
        private void ReprintLastReceiptClick(object sender, EventArgs e)
        {
            m_parent.MagCardReader.EndReading(); // PDTS 1064

            try
            {
                // PDTS 693
                m_parent.StartReprintLastReceipt();
                m_parent.ShowWaitForm(this);
                CheckForError();
            }
            catch (Exception ex)
            {
                m_parent.Log("Failed to print the last receipt: " + ex.Message, LoggerLevel.Severe);
                m_parent.ShowMessage(this, m_displayMode, ex.Message);
            }

            m_parent.MagCardReader.BeginReading(); // PDTS 1064
        }

        /// <summary>
        /// Handles the credit cash out button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the 
        /// event data.</param>
        private void CreditCashOutClick(object sender, EventArgs e)
        {
            // Spawn a new thread to cash out and wait until done.
            try
            {
                m_parent.CanUpdateMenus = false; // PDTS 964

                // TTP 50114
                // TA7576 - Removed player payouts check.

                if(m_parent.CurrentSale.Player.RefundableCredit != 0M)
                {
                    m_parent.MagCardReader.EndReading(); // PDTS 1064

                    // PDTS 693
                    m_parent.StartCreditCashOut();
                    m_parent.ShowWaitForm(this); // Block until we are done.
                }

                m_parent.MagCardReader.BeginReading(); // PDTS 1064

                if(!(m_parent.LastAsyncException is ServerCommException))
                    m_parent.CanUpdateMenus = true; // PDTS 964
            }
            catch(ServerCommException)
            {
                m_parent.ServerCommFailed();
                return;
            }
            catch(Exception ex)
            {
                m_parent.Log("Failed cash out the player/machine's credit: " + ex.Message, LoggerLevel.Severe);
                m_parent.ShowMessage(this, m_displayMode, ex.Message);
                m_parent.CanUpdateMenus = true; // PDTS 964
                m_parent.MagCardReader.BeginReading(); // PDTS 1064
            }

            // TTP 50306 - Clear player after cash out.
            if(!CheckForError())
                m_parent.ClearSale();
        }

        /// <summary>
        /// Handles the unit assignment button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the 
        /// event data.</param>
        private void UnitAssignmentClick(object sender, EventArgs e)
        {
            m_parent.StartUnitAssignment(); // TTP 50114
        }

        /// <summary>
        /// Handles the view receipts button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the 
        /// event data.</param>
        private void ViewReceiptsClick(object sender, EventArgs e)
        {
            //US5711: added parameter for refund presales
            var forceAuthorizationAtPos = m_parent.Settings.ForceAuthorizationOnVoidsAtPOS;
            m_parent.StartViewReceipts(0, forceAuthorizationAtPos, forceAuthorizationAtPos || !m_parent.Settings.EnablePresales);// added license file check for presales
        }

        /// <summary>
        /// Handles the void receipts button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the 
        /// event data.</param>
        private void VoidReceiptsClick(object sender, EventArgs e)
        {
            int authorizedStaffID = 0;

            // Get authorization to void
            m_parent.CanUpdateMenus = false;

            // Ask for an override.
            //US5711: refund presales
            var moduleIdList = new List<int> { (int)EliteModule.ReceiptManagement, (int)EliteModule.POS };
            var staff = POSSecurity.GetOverrideStaffPermissions(m_parent, m_parent.POSWaitForm, this, moduleIdList);

            m_parent.CanUpdateMenus = true;

            if (staff == null)
            {
                return;
            }

            //check to see if staff has permissions to void and refund
            var voidFeature = staff.CheckModuleFeature(EliteModule.ReceiptManagement, (int) ReceiptManagementFeature.VoidSale);
            var refundFeature = staff.CheckModuleFeature(EliteModule.POS, (int)POSFeature.RefundPresales) && m_parent.Settings.EnablePresales; // added license file check for presales
            
            if (!voidFeature && !refundFeature)
            {
                POSMessageForm.Show(this, m_parent, Resources.StaffNotAuthorized);
                return;
            }

            m_parent.StartViewReceipts(staff.Id,!voidFeature, !refundFeature);
        }

        // TTP 50137
        /// <summary>
        /// Handles the adjust bank button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the 
        /// event data.</param>
        private void AdjustBankClick(object sender, EventArgs e)
        {
            // Spawn a new thread to adjust and wait until done.
            try
            {
                // PDTS 693
                m_parent.StartGetCurrentBank();
                m_parent.ShowWaitForm(this);
            }
            catch(Exception ex)
            {
                m_parent.Log("Failed get the current bank amount: " + ex.Message, LoggerLevel.Severe);
                m_parent.ShowMessage(this, m_displayMode, ex.Message);
            }

            if(!CheckForError())
            {
                m_parent.CanUpdateMenus = false; // PDTS 964

                // Now we can update it.
                // Rally TA7464
                AdjustBankForm adjustForm = new AdjustBankForm(m_parent, new NormalDisplayMode(), m_parent.Bank);
                
                if(adjustForm.ShowDialog(this) == DialogResult.OK)
                {
                    m_parent.MagCardReader.EndReading(); // PDTS 1064

                    try
                    {
                        // PDTS 693
                        m_parent.StartAdjustCurrentBank(adjustForm.AdjustmentAmounts);
                        m_parent.ShowWaitForm(this);
                    }
                    catch(Exception ex)
                    {
                        m_parent.Log("Failed adjust the current bank amount: " + ex.Message, LoggerLevel.Severe);
                        m_parent.ShowMessage(this, m_displayMode, ex.Message);
                    }

                    m_parent.MagCardReader.BeginReading(); // PDTS 1064
                    m_parent.CanUpdateMenus = true; // PDTS 964
                    CheckForError();
                }
                else
                    m_parent.CanUpdateMenus = true; // PDTS 964
                // END: TA7464
                adjustForm.Dispose();
            }
        }

        /// <summary>
        /// Handles the register report button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the 
        /// event data.</param>
        private void RegisterSalesReportClick(object sender, EventArgs e)
        {
            m_parent.MagCardReader.EndReading(); // PDTS 1064

            // Spawn a new thread to print the report and wait until done.
            // PDTS 693
            m_parent.StartPrintRegisterReport(false); // Rally US1648
            m_parent.ShowWaitForm(this); // Block until we are done.

            m_parent.MagCardReader.BeginReading(); // PDTS 1064

            CheckForError();
        }

        //US5115: POS: Add Register Closing report button
        /// <summary>
        /// Handles the register report button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the 
        /// event data.</param>
        private void RegisterClosingReportClick(object sender, EventArgs e)
        {
            m_parent.MagCardReader.EndReading(); 

            // Spawn a new thread to print the report and wait until done.
            m_parent.StartPrintRegisterReport(true); 
            m_parent.ShowWaitForm(this); // Block until we are done.

            m_parent.MagCardReader.BeginReading();

            CheckForError();
        }

        // FIX: DE1930
        /// <summary>
        /// Handles the close bank button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the 
        /// event data.</param>
        private void CloseBankClick(object sender, EventArgs e)
        {
            // Spawn a new thread to close the bank and wait until done.
            try
            {
                m_parent.MagCardReader.EndReading();
                m_parent.CanUpdateMenus = false;

                CloseDropBankForm.BankCloseDisplayOptions displayOptions = m_parent.Settings.EnablePaperUsage ? CloseDropBankForm.BankCloseDisplayOptions.DoNotAllowClose:CloseDropBankForm.BankCloseDisplayOptions.AllowBoth;

                //check for presale session
                //if the selected session is a presale, then want to find the active sale session
                var session = m_parent.CurrentSession;
                if (m_parent.CurrentSession.IsPreSale && m_parent.ActiveSalesSession != null)
                {
                    session = m_parent.ActiveSalesSession;
                }

                //US4436: Close a bank from the POS
                //US4698: POS: Denomination receipt
                var closeBankForm = new CloseDropBankForm(string.Format("{0} {1}", m_parent.CurrentStaff.FirstName, m_parent.CurrentStaff.LastName), session.SessionNumber, m_parent)
                {
                    StartPosition = FormStartPosition.CenterParent,
                    DisplayOptions = displayOptions,
                };

                var results = closeBankForm.ShowDialog(this);

                if (results == DialogResult.OK)
                {
                    m_parent.StartCloseBank();
                    m_parent.ShowWaitForm(this); // Block until we are done.

                    if(!CheckForError())
                    {
                        //we only want to print receipt if not auto issue
                        if (m_parent.Settings.PrintRegisterClosingOnBankClose)
                        {
                            m_parent.StartPrintRegisterReport(true); // Rally US1648
                            m_parent.ShowWaitForm(this); // Block until we are done.
                        }

                        CheckForError();

                        Close();
                    }
                }

                m_parent.CanUpdateMenus = true;
                m_parent.MagCardReader.BeginReading();
            }
            catch(Exception ex)
            {
                m_parent.Log("Failed to close the current bank: " + ex.Message, LoggerLevel.Severe);
                m_parent.ShowMessage(this, m_displayMode, string.Format(CultureInfo.CurrentCulture, Resources.CloseBankFailed, ex.Message));
            }   
        }
        // END: DE1930

        /// <summary>
        /// US5347
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PaperRangeScannerClick(object sender, EventArgs e)
        {
            PaperRangeScannerForm scanner = new PaperRangeScannerForm(m_parent, m_displayMode);
            scanner.ShowDialog();
        }

        //US3509
        /// <summary>
        /// Validation click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ValidationClick(object sender, EventArgs e)
        {
            try
            {
                if (m_parent.ValidationEnabled)
                {
                    m_ValidationbuttonEntry.Text = Resources.ValidationDisabledString;

                    if (m_parent.WeAreAPOSKiosk)
                    {
                        m_ValidationbuttonEntry.CustomButtonUpImage = Resources.YellowButtonUp;
                        m_ValidationbuttonEntry.CustomButtonDownImage = Resources.YellowButtonDown;
                    }
                    else
                    {
                        DisplayValidationDisabledMode(true); //US4466
                    }
                }
                else
                {
                    m_buttonMenu.GetButtonEntry(ValidateId).Text = Resources.ValidationEnabledString;

                    if (m_parent.WeAreAPOSKiosk)
                    {
                        m_ValidationbuttonEntry.CustomButtonUpImage = Resources.GrayButtonUp;
                        m_ValidationbuttonEntry.CustomButtonDownImage = Resources.GrayButtonDown;
                    }
                    else
                    {
                        DisplayValidationDisabledMode(false); //US4466
                    }
                }

                m_parent.ValidationEnabled = !m_parent.ValidationEnabled;
                UpdateMenuButtonPrices();
                m_buttonMenu.Refresh(ValidateId);

                if (m_parent.WeAreAPOSKiosk && !m_parent.IsSaleEmpty) //toggle all items on or off
                {
                    m_parent.UpdateValidationSales(null);
                    UpdateSaleInfo();
                }

                UpdateMenuButtonStates();
            }
            catch (Exception ex)
            {
                m_parent.Log("Failed to enable/disable validation: " + ex.Message, LoggerLevel.Severe);
                m_parent.ShowMessage(this, m_displayMode, string.Format(CultureInfo.CurrentCulture, Resources.CloseBankFailed, ex.Message));
            }
        }

        /// <summary>
        /// Handles the pay receipts button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the event data.</param>
        private void PayReceiptsClick(object sender, EventArgs e)
        {
            // TODO Revisit paying receipts.
            /*
            PayReceiptsForm payReceiptsForm = new PayReceiptsForm(m_parent, m_displayMode);

            payReceiptsForm.ShowDialog();
            */
        }

        /// <summary>
        /// Starts the sale totalling process.
        /// </summary>
        /// <param name="deviceId">The id of the device sold.</param>
        /// <returns>False if could not tender sale.</returns>
        protected bool Total(int deviceId)
        {
            if (m_parent.CurrentSale == null)
            {
                if (m_parent.Settings.AllowNoSale && m_keypad.BigButtonText == Resources.NoSale)
                    m_parent.OpenCashDrawer();
                else
                    m_parent.ShowMessage(this, m_displayMode, Resources.NoSaleItems);

                return false;
            }
                
            NotIdle();

            if (m_parent.IsBusy) // a message is still being processed
            {
                m_parent.Log("The user tried to tend while a message was still being sent to the server. " +
                    "This shouldn't happen since the total button should be disabled. Displaying the wait dialog.", LoggerLevel.Warning);
                m_parent.ShowWaitForm(this); // delays until message finishes
                return false;
            }

            bool saleProcessed = false;

            do
            {
                //see if the advanced kiosk user wants to use a player card
                if (m_parent.WeAreAnAdvancedPOSKiosk && m_parent.CurrentSale != null && m_parent.CurrentSale.AskPatronForPlayerCard && m_parent.CurrentSale.Player == null && !m_parent.CurrentSale.IsReturn)
                {
                    bool noThanks = false;

                    NotIdle(true);

                    do
                    {
                        MagCardForm MSRForm = new MagCardForm(m_parent.Settings.DisplayMode, m_parent.MagCardReader);

                        MSRForm.IsPatronFacing = true;
                        MSRForm.PatronFacingCancelDelayInMilliseconds = KioskShortIdleLimitInSeconds * 1000;

                        noThanks = MSRForm.ShowDialog(this) == DialogResult.Cancel;

                        if (!noThanks)
                            GetPlayer(MSRForm.MagCardNumber, true);
                        else
                            m_parent.CurrentSale.AskPatronForPlayerCard = false;

                    } while (m_parent.CurrentSale.Player == null && !noThanks);

                    NotIdle();
                }

                //DE13005: POS: coupon screen pops up when returning
                //check if coupon is set
                if (KioskForm == null && m_parent.Settings.isCouponManagement && m_parent.CurrentSale != null && m_parent.CurrentSale.Player != null && !m_parent.CurrentSale.IsReturn)
                {
                    var buttonEntry = m_buttonMenu.GetButtonEntry(PlayerCouponsId);

                    if (!m_parent.CurrentSale.Player.UsedCouponScreen && buttonEntry != null && buttonEntry.Enabled)
                    {
                        if (m_parent.WeAreAPOSKiosk)
                        {
                            NotIdle(true);

                            if (POSMessageForm.ShowCustomTwoButton(m_simpleKioskForm != null ? (IWin32Window)m_simpleKioskForm : this, m_parent, Resources.ShowCouponsQuestion, "", true, 1, Resources.LookAtCoupons, Resources.NoThanks) == 1)
                            {
                                if (!RedeemCoupon(deviceId)) //timeout in coupon screen
                                {
                                    ForceKioskTimeout(2);
                                    return false;
                                }
                            }
                            else
                            {
                                m_parent.CurrentSale.Player.UsedCouponScreen = true;
                            }

                            NotIdle();
                        }
                        else //not a Kiosk
                        {
                            if (!RedeemCoupon(deviceId))
                                return false;//Cancel this method if the cashier wants to continue sale.
                        }
                    }
                }

                // Check to see if there are any line items.
                if (m_parent.IsSaleEmpty && (m_parent.CurrentSale == null || m_parent.CurrentSale.GetCurrentTenders().Count == 0))
                {
                    if (m_parent.Settings.AllowNoSale && m_keypad.BigButtonText == Resources.NoSale)
                        m_parent.OpenCashDrawer();
                    else
                        m_parent.ShowMessage(this, m_displayMode, Resources.NoSaleItems);

                    return false;
                }

                // Rally US1854 - Check to see if 0 or negative sales are allowed.
                if (m_parent.Settings.MinimumSaleAllowed == MinimumSaleAllowed.GreaterThanZero &&
                    m_parent.CurrentSale.CalculateTotal(false) <= 0M)
                {
                    m_parent.ShowMessage(this, m_displayMode, Resources.SaleGreaterThanZero);
                    return false;
                }
                else if (m_parent.Settings.MinimumSaleAllowed == MinimumSaleAllowed.ZeroOrGreater &&
                        m_parent.CurrentSale.CalculateTotal(false) < 0M)
                {
                    m_parent.ShowMessage(this, m_displayMode, Resources.SaleZeroOrGreater);
                    return false;
                }

                // Rally TA5748
                // If play with paper is enabled, then we have to prompt for
                // start numbers.
                if (m_parent.Settings.PlayWithPaper && m_parent.CurrentSale.HasElectronicBingo)
                {
                    PlayWithPaperForm paperForm = new PlayWithPaperForm(m_parent, m_displayMode);
                    DialogResult result = paperForm.ShowDialog(this);

                    paperForm.Dispose();

                    if (result == DialogResult.Cancel)
                    {
                        m_parent.CurrentSale.ClearStartNumbers();
                        return false;
                    }
                }
                // END: TA5748

                if (m_parent.CurrentSale.NeedsPackInfo)
                {
                    //
                    // TODO JKN
                    //  this is where we will add support for allowing the user
                    //  to select serial and audit numbers if attempting to sell
                    //  a paper inventory product
                    //
                    m_parent.ShowMessage(this, m_displayMode, "Selling items without setting serial & audit numbers");
                    //PaperPackForm paperForm = new PaperPackForm(m_parent, m_displayMode);
                    //DialogResult result = paperForm.ShowDialog(this);

                    //paperForm.Dispose();

                    //if (result == DialogResult.Cancel)
                    //{
                    //    m_parent.CurrentSale.ClearStartNumbers();
                    //    return;
                    //}
                }

                // Start the selling process.
                Tuple<bool, bool> totalSaleResult = m_parent.TotalSale(deviceId);
                saleProcessed = totalSaleResult.Item1;

                if (!saleProcessed && m_parent.Settings.EnableFlexTendering && m_parent.IsSaleEmpty && m_parent.CurrentSale != null && m_parent.CurrentSale.GetCurrentTenders().Count != 0)
                    return false;

                if (!totalSaleResult.Item2) //re-enter sale?
                    return false;

            } while (!saleProcessed);

            return true;
        }

        /// <summary>
        /// Redeems the coupon.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <returns>True to finish sale; False to continue sale</returns>
        private bool RedeemCoupon(int deviceId)
        {
            m_parent.CurrentCouponForm.Prep(deviceId, m_parent.WeAreAPOSKiosk? false : true);

            if (m_parent.CurrentCouponForm.LoadPlayerComp()) //don't show the form if no coupons (stops flicker) RAK
            {
                DialogResult dlgResult = m_parent.CurrentCouponForm.ShowDialog(this);
                
                Application.DoEvents();
                m_parent.CurrentSale.Player.UsedCouponScreen = true;

                if (dlgResult == DialogResult.Abort) //Kiosk timeout
                {
                    ForceKioskTimeout(2);
                    return false; //don't finish the sale
                }
            }

            //return whether to finish or continue with sale
            return m_parent.CurrentCouponForm.FinishSale;
        }
   
        /// <summary>
        /// Handles the total button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the 
        /// event data.</param>
        private void TotalButtonClick(object sender, EventArgs e)
        {
            m_keypad.BigButtonEnabled = false;

            if (m_parent.IsBusy) // a message is still being processed
            {
                m_parent.Log("The user tried to tend while a message was still being sent to the server. " +
                    "This shouldn't happen since the total button should be disabled. Displaying the wait dialog.", LoggerLevel.Warning);
                m_parent.ShowWaitForm(this); // delays until message finishes
                m_keypad.BigButtonEnabled = true;
                return;
            }

            ProcessSale();

            //if they selected quick tendering mode, don't make them hit this button again (which is now FINISH)
            if(m_parent.Settings.Tender == TenderSalesMode.Quick && m_parent.SaleState == PointOfSale.SellingState.Finishing)
                ProcessSale();

            if (m_parent.WeAreAnAdvancedPOSKiosk && GiveChangeAsB3Credit && ChangeToGiveAsB3Credit != 0M)
                SelectB3Menu(true);

            m_keypad.BigButtonEnabled = true;
        }

        /// <summary>
        /// Handles the Pack button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the
        /// event data.</param>
        private void PackButtonClick(object sender, EventArgs e)
        {
            if (m_parent.Settings.EnableFlexTendering)
                m_keypad.BigButtonEnabled = false;

            Total(0);

            if (m_parent.Settings.EnableFlexTendering)
                m_keypad.BigButtonEnabled = true;
        }

        private void PackButtonHeld(object sender, EventArgs e)
        {
            if (m_parent.CurrentSale.Device.Id == PackDeviceId) //this device is already selected, toggle add on mode
            {
                ToggleAddOnSale(sender, e);
            }
            else
            {
                m_parent.CurrentSale.Device = Device.FromId(PackDeviceId);

                //set the selected unit name for the total area 
                SetSelectedDeviceName(m_parent.CurrentSale.Device.Id);

                m_parent.UpdateDeviceFeesAndTotals();
            }
        }

        private void ToggleAddOnSale(object sender, EventArgs e)
        {
            m_parent.CurrentSale.AddOnSale = !m_parent.CurrentSale.AddOnSale;
            SetSelectedDeviceName(m_parent.CurrentSale.Device.Id);
            m_parent.CurrentSale.DeviceFee = 0M;
            m_parent.UpdateDeviceFeesAndTotals();
        }

        public void UnitSelectionButton(object sender, EventArgs e)
        {
            bool OK = false;
            int newID = m_parent.CurrentSale.Device.Id;
            int minID = m_availableDevices.Min(d => d.Id);
            int maxID = m_availableDevices.Max(d => d.Id);
            int passes = 0;

            do
            {
                newID++;

                if (newID > maxID)
                {
                    newID = minID;
                    passes++;
                }

                if(m_availableDevices.Exists(d => d.Id == newID))
                {
                    if (newID == PackDeviceId)
                    {
                        PackButtonHeld(sender, e);
                        OK = true;
                    }

                    if (newID == Device.Traveler.Id)
                    {
                        TravelerButtonHeld(sender, e);
                        OK = true;
                    }

                    if (newID == Device.Tracker.Id)
                    {
                        TrackerButtonHeld(sender, e);
                        OK = true;
                    }

                    if (newID == Device.Fixed.Id)
                    {
                        FixedButtonHeld(sender, e);
                        OK = true;
                    }

                    if (newID == Device.Explorer.Id)
                    {
                        ExplorerButtonHeld(sender, e);
                        OK = true;
                    }

                    if (newID == Device.Traveler2.Id)
                    {
                        Traveler2ButtonHeld(sender, e);
                        OK = true;
                    }

                    if (newID == Device.Tablet.Id)
                    {
                        TabletButtonHeld(sender, e);
                        OK = true;
                    }
                }
            } while (!OK && passes < 2);
        }

        /// <summary>
        /// Handles the Fixed button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the
        /// event data.</param>
        private void FixedButtonClick(object sender, EventArgs e)
        {
            if (m_parent.Settings.EnableFlexTendering)
                m_keypad.BigButtonEnabled = false;

            Total(Device.Fixed.Id);

            if (m_parent.Settings.EnableFlexTendering)
                m_keypad.BigButtonEnabled = true;
        }

        private void FixedButtonHeld(object sender, EventArgs e)
        {
            if (m_parent.CurrentSale.Device.Id == Device.Fixed.Id) //this device is already selected, toggle add on mode
            {
                ToggleAddOnSale(sender, e);
            }
            else
            {
                m_parent.CurrentSale.Device = Device.FromId(Device.Fixed.Id);

                //set the selected unit name for the total area 
                SetSelectedDeviceName(m_parent.CurrentSale.Device.Id);

                m_parent.UpdateDeviceFeesAndTotals();
            }
        }

        /// <summary>
        /// Handles the Traveler button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the
        /// event data.</param>
        private void TravelerButtonClick(object sender, EventArgs e)
        {
            if (m_parent.Settings.EnableFlexTendering)
                m_keypad.BigButtonEnabled = false;

            Total(Device.Traveler.Id);

            if (m_parent.Settings.EnableFlexTendering)
                m_keypad.BigButtonEnabled = true;
        }

        private void TravelerButtonHeld(object sender, EventArgs e)
        {
            if (m_parent.CurrentSale.Device.Id == Device.Traveler.Id) //this device is already selected, toggle add on mode
            {
                ToggleAddOnSale(sender, e);
            }
            else
            {
                m_parent.CurrentSale.Device = Device.FromId(Device.Traveler.Id);

                //set the selected unit name for the total area 
                SetSelectedDeviceName(m_parent.CurrentSale.Device.Id);

                m_parent.UpdateDeviceFeesAndTotals();
            }
        }

        /// <summary>
        /// Handles the Tracker button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the
        /// event data.</param>
        private void TrackerButtonClick(object sender, EventArgs e)
        {
            if (m_parent.Settings.EnableFlexTendering)
                m_keypad.BigButtonEnabled = false;

            Total(Device.Tracker.Id);

            if (m_parent.Settings.EnableFlexTendering)
                m_keypad.BigButtonEnabled = true;
        }

        private void TrackerButtonHeld(object sender, EventArgs e)
        {
            if (m_parent.CurrentSale.Device.Id == Device.Tracker.Id) //this device is already selected, toggle add on mode
            {
                ToggleAddOnSale(sender, e);
            }
            else
            {
                m_parent.CurrentSale.Device = Device.FromId(Device.Tracker.Id);

                //set the selected unit name for the total area 
                SetSelectedDeviceName(m_parent.CurrentSale.Device.Id);

                m_parent.UpdateDeviceFeesAndTotals();
            }
        }

        // Rally TA7729
        /// <summary>
        /// Handles the Explorer button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the
        /// event data.</param>
        private void ExplorerButtonClick(object sender, EventArgs e)
        {
            if (m_parent.Settings.EnableFlexTendering)
                m_keypad.BigButtonEnabled = false;

            Total(Device.Explorer.Id);

            if (m_parent.Settings.EnableFlexTendering)
                m_keypad.BigButtonEnabled = true;
        }

        private void ExplorerButtonHeld(object sender, EventArgs e)
        {
            if (m_parent.CurrentSale.Device.Id == Device.Explorer.Id) //this device is already selected, toggle add on mode
            {
                ToggleAddOnSale(sender, e);
            }
            else
            {
                m_parent.CurrentSale.Device = Device.FromId(Device.Explorer.Id);

                //set the selected unit name for the total area 
                SetSelectedDeviceName(m_parent.CurrentSale.Device.Id);

                m_parent.UpdateDeviceFeesAndTotals();
            }
        }

        // PDTS 964
        // Rally US765
        /// <summary>
        /// Handles the Traveler II button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the
        /// event data.</param>
        private void Traveler2ButtonClick(object sender, EventArgs e)
        {
            if (m_parent.Settings.EnableFlexTendering)
                m_keypad.BigButtonEnabled = false;

            Total(Device.Traveler2.Id);

            if (m_parent.Settings.EnableFlexTendering)
                m_keypad.BigButtonEnabled = true;
        }

        private void Traveler2ButtonHeld(object sender, EventArgs e)
        {
            if (m_parent.CurrentSale.Device.Id == Device.Traveler2.Id) //this device is already selected, toggle add on mode
            {
                ToggleAddOnSale(sender, e);
            }
            else
            {
                m_parent.CurrentSale.Device = Device.FromId(Device.Traveler2.Id);

                //set the selected unit name for the total area 
                SetSelectedDeviceName(m_parent.CurrentSale.Device.Id);

                m_parent.UpdateDeviceFeesAndTotals();
            }
        }

        //US2908
        private void TabletButtonClick(object sender, EventArgs e)
        {
            if(m_parent.Settings.EnableFlexTendering)
                m_keypad.BigButtonEnabled = false;

            Total(Device.Tablet.Id);

            if (m_parent.Settings.EnableFlexTendering)
                m_keypad.BigButtonEnabled = true;
        }

        private void TabletButtonHeld(object sender, EventArgs e)
        {
            if (m_parent.CurrentSale.Device.Id == Device.Tablet.Id) //this device is already selected, toggle add on mode
            {
                ToggleAddOnSale(sender, e);
            }
            else
            {
                m_parent.CurrentSale.Device = Device.FromId(Device.Tablet.Id);

                //set the selected unit name for the total area 
                SetSelectedDeviceName(m_parent.CurrentSale.Device.Id);

                m_parent.UpdateDeviceFeesAndTotals();
            }
        }

        /// <summary>
        /// Handles the exit button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the
        /// event data.</param>
        private void ExitButtonClick(object sender, EventArgs e)
        {
            if (m_parent.WeAreAPOSKiosk && sender as ImageButton == m_exitButton) //this is the help button
            {
                m_parent.HelpScreenActive = true;

                HelpForm help = new HelpForm(m_parent);

                bool timedOut = help.ShowDialog(this) == DialogResult.Abort;

                m_parent.HelpScreenActive = false;

                if (timedOut)
                    ForceKioskTimeout(1);
            }
            else //not help button
            {
                if (m_parent.CurrentSale != null && !m_parent.IsSaleEmpty)
                {
                    //US4656: POS: Allow Exit from "A sale is in progress" prompt
                    if (m_parent.ShowMessage(this, m_displayMode, Resources.SaleInProgress, POSMessageFormTypes.YesNo_DefNO) == DialogResult.No)
                        return;
                }

                StartOver(true); //clear the sale

                //close the PIN pad
                if (m_parent.Settings.PaymentProcessingEnabled)
                {
                    var processor = EliteCreditCardFactory.Instance;

                    if (processor != null)
                    {
                        //create a waiting message
                        WaitForm waitForm = new WaitForm(m_parent.Settings.DisplayMode);
                        waitForm.WaitImage = Resources.Waiting;
                        waitForm.Cursor = Cursors.WaitCursor;
                        waitForm.Message = Resources.ClosingLane;
                        waitForm.StartPosition = FormStartPosition.CenterScreen;
                        waitForm.Show(this);
                        Application.DoEvents(); //let the animation get started

                        //create a thread to perform the lane closing
                        Thread waitThread = new Thread(new ThreadStart(delegate
                        {
                            processor.SetLaneOpenDisplay(false);
                        }));

                        //run the thread
                        waitThread.Start();

                        //wait for the thread to complete
                        while (waitThread.IsAlive)
                            Application.DoEvents();
                    }
                }

                // FIX: DE1930
                Close();
                // END: DE1930
            }
        }

        // PDTS 1064
        /// <summary>
        /// Handles the MagneticCardReader's CardSwiped event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An MagneticCardSwipeArgs object that contains the
        /// event data.</param>
        public void CardSwiped(object sender, MagneticCardSwipeArgs e)
        {
            NotIdle();

            bool tooSoon = m_parent.IsBusy || (DateTime.Now - m_lastCardSwipedAt).CompareTo(TimeSpan.FromSeconds(1)) < 0;

            m_lastCardSwipedAt = DateTime.Now;

            if (m_parent.BarcodeScanner != null)
                m_parent.BarcodeScanner.Reset(); //if we just swiped a card, there shouldn't be anything from a scanner

            if (m_parent.WeAreANonAdvancedPOSKiosk && KioskForm != null && KioskForm.State == SimpleKioskForm.KioskState.GetPlayerCard) //should have gone to kiosk code
            {
                KioskForm.SwipeCard(sender, e);
                return;
            }

            if(!tooSoon && ContainsFocus && m_parent.MagCardReader.ReadingCards && 
               (m_parent.SaleState == PointOfSale.SellingState.NotSelling || m_parent.SaleState == PointOfSale.SellingState.Selling))
            {
                GetPlayer(e.CardData);
            }
        }

        public void BarcodeScanned(object sender, string barcode)
        {
            NotIdle();

            if (m_parent.SaleState == PointOfSale.SellingState.NotSelling || m_parent.SaleState == PointOfSale.SellingState.Selling)
            {
                //see if this is a receipt for repeat sale
                if (Regex.IsMatch(barcode.ToUpper(), @"^F[0-9]*TRN")) //the receipt barcode is in the form: Ftransaction numberTRN
                {
                    int transactionNumber = 0;

                    if (int.TryParse(barcode.Substring(1, barcode.Length - 4), out transactionNumber))
                        ProcessRepeatSaleThroughServer(0, transactionNumber);
                }
                else if (Regex.IsMatch(barcode.ToUpper(), @"^F[0-9]*SCH") || Regex.IsMatch(barcode.ToUpper(), @"^F[0-9]*PDS")) //scheduled sale or pre-defined sale
                {
                    //get scheduled sale using scancode
                    GetScheduledSalesMessage msg = new GetScheduledSalesMessage();

                    msg.Scancode = barcode.ToUpper();

                    m_parent.StartGetSchduledSaleInfo(msg);
                    m_parent.BlockingWaitForm.ShowDialog(m_parent.WeAreANonAdvancedPOSKiosk ? (IWin32Window)m_simpleKioskForm : this);
                    Application.DoEvents();

                    if (msg.SaleList.Count == 0)
                        return;

                    if (m_parent.Settings.EnableActiveSalesSession) //only use the items for our session
                        msg.SaleList = msg.SaleList.FindAll(s => s.session == m_parent.CurrentSession.SessionNumber || s.session == 0);

                    if (msg.SaleList.Count == 0)
                        return;

                    ProcessSaleFromList(msg.SaleList);
                }
                else //try it as a barcode
                {
                    GetProductDataByBarcode(barcode);
                }
            }
        }

        private void RepeatPlayerLastSale(object sender, EventArgs e)
        {
            if (m_parent.SaleState == PointOfSale.SellingState.NotSelling || m_parent.SaleState == PointOfSale.SellingState.Selling)
            {
                if (m_parent.CurrentSale != null && m_parent.CurrentSale.Player != null)
                    ProcessRepeatSaleThroughServer(m_parent.CurrentSale.Player.Id, 0);
            }
        }

        public void ProcessRepeatSaleThroughServer(int playerID, int transaction, bool useDeviceFromSale = true)
        {
            m_parent.CanUpdateMenus = false;

            GetRepeatSaleInfoMessage msg = new GetRepeatSaleInfoMessage(transaction, playerID);

            m_parent.StartGetRepeatSaleInfo(msg);
            m_parent.BlockingWaitForm.ShowDialog(m_parent.WeAreANonAdvancedPOSKiosk ? (IWin32Window)m_simpleKioskForm : this);
            Application.DoEvents();

            if (msg.SaleInfo.Count == 0)
            {
                if (transaction != 0)
                    m_parent.ShowMessage(this, m_parent.Settings.DisplayMode, Resources.NoItemsFoundForRepeatSale);
                else
                    m_parent.ShowMessage(this, m_parent.Settings.DisplayMode, Resources.NoLastSale);

                m_parent.CanUpdateMenus = true;
                return;
            }

            ProcessSaleFromList(msg.SaleInfo);

            //select the electronic unit
            if (useDeviceFromSale && msg.DeviceID != 0)
            {
                m_parent.CanUpdateMenus = false;

                m_parent.CurrentSale.Device = Device.FromId(msg.DeviceID);

                //set the selected unit name for the total area 
                SetSelectedDeviceName(m_parent.CurrentSale.Device.Id);

                m_parent.UpdateDeviceFeesAndTotals();
                m_parent.CanUpdateMenus = true;
            }
        }

        public void ProcessSaleFromList(List<ScheduledSaleInfo> saleInfo, bool clearSale = false)
        {
            List<RepeatSaleInfo> repeatList = new List<RepeatSaleInfo>();
            RepeatSaleInfo newInfo;

            foreach (ScheduledSaleInfo info in saleInfo)
            {
                newInfo = new RepeatSaleInfo();

                newInfo.session = info.session;
                newInfo.packageID = info.packageID;
                newInfo.discountID = info.discountID;
                newInfo.name = info.name;
                newInfo.quantity = info.quantity;

                repeatList.Add(newInfo);
            }

            ProcessSaleFromList(repeatList, clearSale);
        }

        public void ProcessSaleFromList(List<RepeatSaleInfo> saleInfo, bool clearSale = false)
        {
            m_parent.CanUpdateMenus = false;

            if (saleInfo.Count == 0)
            {
                m_parent.CanUpdateMenus = true;
                return;
            }

            if (m_parent.WeAreANonAdvancedPOSKiosk && m_simpleKioskForm != null)
                m_simpleKioskForm.FilterRepeatSale(saleInfo);

            //remember what menu we are on
            int originalMenuIndex = m_menuList.SelectedIndex;

            // If the current sale has a player, save it.
            Player player = null;

            if (m_parent.CurrentSale != null && m_parent.CurrentSale.Player != null)
                player = m_parent.CurrentSale.Player;

            // Clear the existing sale if needed.
            if(m_parent.WeAreNotAPOSKiosk && (m_parent.Settings.ScanningReceiptsStartNewSale || clearSale)) //Kiosks add to sale to avoid confusion, POS is optional
                m_parent.ClearSale();

            if(m_parent.CurrentSale == null)
                m_parent.StartSale(false);

            // Set the player if we had one saved.
            if (m_parent.WeAreNotAPOSKiosk && player != null)
                m_parent.CurrentSale.SetPlayer(player, true, true);

            // Update UI.
            SetPlayer();
            UpdateMenuButtonStates();
            UpdateSaleInfo();
            UpdateSystemButtonStates(); // PDTS 571

            m_saleList.BeginUpdate(); //don't draw the receipt on the screen until all the items are rung-up
            m_saleListUpdateCount++;

            try
            {
                foreach (RepeatSaleInfo info in saleInfo)
                {
                    //set the session
                    if (info.session != 0 &&
                        (m_parent.CurrentSession.SessionNumber != info.session || m_parent.CurrentSession.GamingDate != m_parent.GamingDate)) //we have a session and are not in it
                    {
                        int index = m_parent.GetMenuIndexForSession(info.session, m_parent.GamingDate);

                        if (index > -1) //found it
                        {
                            m_menuList.SelectedIndex = index;

                            do
                            {
                                Application.DoEvents();
                            } while (m_parent.CurrentSession.SessionNumber != info.session);
                        }
                        else //could not find the session
                        {
                            continue;
                        }
                    }

                    //set qty to keypad
                    m_keypad.Value = info.quantity;

                    try
                    {
                        //press button
                        if (info.packageID != 0)
                        {
                            PackageButton pb = m_parent.GetMenuButtonForPackage(info.packageID);

                            if (pb != null)
                            {
                                if (m_parent.WeAreAPOSKiosk && !m_parent.Settings.AllowPaperOnKiosks)
                                {
                                    if (pb.Package.HasBarcodedPaper)
                                        continue;
                                }

                                if (!pb.IsLocked && !(pb.Package.RequiresValidation && !m_parent.ValidationEnabled))
                                    m_parent.PushMenuButton(pb);
                            }
                        }
                        else if (info.discountID != 0)
                        {
                            DiscountButton db = m_parent.GetMenuButtonForDiscount(info.discountID);

                            if (db != null && !db.IsLocked)
                                m_parent.PushMenuButton(db);
                        }
                    }
                    catch (Exception)
                    {
                        //problem pushing button, ignore it and move on to the next button
                    }
                }
            }
            catch (Exception)
            {
                //some problem processing, just use what we got
            }

            //return to the menu/session we started on
            if (m_menuList.SelectedIndex != originalMenuIndex)
                m_menuList.SelectedIndex = originalMenuIndex;

            m_saleList.EndUpdate(); //draw the receipt on the screen
            m_saleListUpdateCount--;
            m_parent.CanUpdateMenus = true;
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
            // PDTS 1064
//            if ((keyData & Keys.Enter) == Keys.Enter && (keyData & Keys.Shift) != Keys.Shift && m_parent.MagCardReader.ReadingCards && m_parent.MagCardReader.MSRInputInProgress)
            //if ((keyData & Keys.Enter) == Keys.Enter && (keyData & Keys.Shift) != Keys.Shift)
            if (keyData == Keys.Enter)
            {
                if(m_parent.MagCardReader.ReadingCards && m_parent.MagCardReader.MSRInputInProgress)
                    return false; //true;
                else
                    return true;
            }
            else 
            {
                return base.ProcessDialogKey(keyData);
            }
        }

        /// <summary>
        /// Pops up a window to get the player card pin input from the user
        /// </summary>
        /// <param name="throwOnCancel"></param>
        /// <returns></returns>
        int GetPlayerCardPINFromUser(bool throwOnCancel = false)
        {
            NotIdle();

            int PIN = 0;
            DialogResult keypadResult;

            if (!m_parent.Settings.ThirdPartyPlayerInterfaceUsesPIN)
                return 0;

            if (m_parent.Settings.ThirdPartyPlayerSyncMode == 3) //player tracking is disconnected
                return 0;

            bool MSRActive = m_parent.MagCardReader.ReadingCards;

            if (MSRActive)
                m_parent.MagCardReader.EndReading();

            NotIdle(true);

            //we need a PIN, get it and get the player points to test the PIN
            POSNumericInputForm PINEntry = new POSNumericInputForm(m_parent, m_parent.Settings.ThirdPartyPlayerInterfacePINLength);
            PINEntry.UseDecimalKey = false;
            PINEntry.Password = true;
            PINEntry.Description = Resources.EnterPlayerCardPIN;
            PINEntry.MaxIdleTime = m_parent.WeAreAPOSKiosk ? KioskShortIdleLimitInSeconds : 0;

            do
            {
                keypadResult = PINEntry.ShowDialog(this);
                Application.DoEvents();

                if (keypadResult == DialogResult.OK)
                    PIN = Convert.ToInt32(PINEntry.DecimalResult);

            } while (keypadResult == DialogResult.OK && PIN == 0);

            PINEntry.Dispose();

            if (MSRActive)
                m_parent.MagCardReader.BeginReading();

            NotIdle();

            if (keypadResult != DialogResult.OK && throwOnCancel)
                throw new POSException(Resources.PlayerCardPINEntryCanceled);

            return PIN;
        }

        /// <summary>
        /// Displays a WaitForm and retrieve's player information from 
        /// the server.  Asks for player card PIN if needed.
        /// </summary>
        /// <param name="cardData">The player's mag. card used to lookup the 
        /// account.</param>
        public void GetPlayer(string cardData, bool usingWaitForm = false)
        {
            NotIdle();

            if (m_parent.IsBusy) // only one player request at a time. It ability to queue them up currently works, but it's confusing for the user.
            {
                return;
            }

            int PIN = 0;

            if (m_parent.CurrentSale != null && m_parent.CurrentSale.Player != null && cardData != m_parent.CurrentSale.Player.PlayerCard) //changing player, abort current sale first
                StartOver(true);

            // Spawn a new thread to find the player and wait until done.
            try
            {
                bool PINProblem = false;
                bool newPIN = false;

                do
                {
                    if (m_parent.Settings.ThirdPartyPlayerSyncMode != 3 && 
                        (m_parent.Settings.ThirdPartyPlayerInterfaceGetPINWhenCardSwiped || (m_parent.CurrentSale != null && m_parent.CurrentSale.NeedPlayerCardPIN))) //we have done something requiring a player and a PIN
                    {
                        newPIN = true;

                        PIN = GetPlayerCardPINFromUser(true);
                    }

                    //we always block when using a PIN since we need to validate the PIN before moving on
                    if (!newPIN) 
                        DisplayGettingPlayer(); //not blocking, tell the user we are working on it
                    
                    m_parent.StartGetPlayer(cardData, PIN);

                    if(!newPIN && usingWaitForm)
                        m_parent.ShowWaitForm(this); // Block until we are done.

                    if (newPIN) //we need to wait here until we get the player so we can validate the PIN
                    {
                        m_parent.ShowWaitForm(this); // Block until we are done.

                        if (m_parent.CurrentSale != null && m_parent.CurrentSale.Player != null)
                        {
                            PINProblem = PIN != 0 && !m_parent.CurrentSale.Player.ThirdPartyInterfaceDown && m_parent.CurrentSale.Player.PlayerCardPINError;

                            if (PINProblem)
                                m_parent.ShowMessage(this, m_parent.Settings.DisplayMode, Resources.PlayerCardPINError);                        }
                    }
                }while(PINProblem);

                if (m_parent.CurrentSale != null && m_parent.CurrentSale.Player != null)
                {
                    m_parent.CurrentSale.NeedPlayerCardPIN = false;
                    m_parent.CurrentSale.Player.WeHaveThePlayerCardPIN = PIN != 0 && !m_parent.CurrentSale.Player.ThirdPartyInterfaceDown && !m_parent.CurrentSale.Player.PlayerCardPINError && m_parent.CurrentSale.Player.PointsUpToDate;

                    if (newPIN && m_parent.CurrentSale.Player.WeHaveThePlayerCardPIN) //save the PIN with the player card number
                    {
                        m_parent.StartSetPlayerCardPIN(m_parent.CurrentSale.Player.Id, PIN);
                        m_parent.ShowWaitForm(this); // Block until we are done.
                    }
                }
            }
            catch(Exception ex)
            {
                m_parent.Log("Failed to get the player/machine: " + ex.Message, LoggerLevel.Severe);
                m_parent.ShowMessage(this, m_displayMode, string.Format(CultureInfo.CurrentCulture, (m_parent.Settings.EnableAnonymousMachineAccounts) ? Resources.GetMachineFailed : Resources.GetPlayerFailed, ex.Message));
            }
        }

        /// <summary>
        /// Displays the "Getting player info" on the sale form instead of popping up a wait form
        /// </summary>
        private void DisplayGettingPlayer()
        {
            if (this.InvokeRequired) // if it's not coming in on the UI thread, move the work to the UI thread.
            {
                this.BeginInvoke((Action)DisplayGettingPlayer);
                return;
            }

            // Clear out the last player's information. Keep the name if updating the current player's information (card was the same)
            bool gettingNewPlayer = m_parent.CurrentSale == null || m_parent.CurrentSale.Player == null;
            ClearPlayer(gettingNewPlayer);

            if (m_parent.WeAreAPOSKiosk)
            {
                m_playerCardPictureLabel.Visible = false;
                m_playerCardPicture.Visible = false;
                Application.DoEvents();
            }

            if (!gettingNewPlayer)
                m_playerInfoList.Items.Add(m_parent.Settings.EnableAnonymousMachineAccounts ? Resources.WaitFormGettingMachine : Resources.WaitFormUpdatingPlayer);
            else
                m_playerInfoList.Items.Add(m_parent.Settings.EnableAnonymousMachineAccounts ? Resources.WaitFormGettingMachine : Resources.WaitFormGettingPlayer);
        }

        /// US4809
        /// <summary>
        /// Handles when a player is returned.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A GetPlayerEventArgs object that contains the
        /// event data.</param>
        private void OnGetPlayerCompleted(object sender, GetPlayerEventArgs e)
        {
            if (e.Player != null)
            {
                // Rally US493
                if(!e.PlayerDidNotChange)
                    m_parent.CheckForAlerts(m_parent.CurrentSale.Player);

                if (!m_parent.Settings.AllowSalesToBannedPlayers && m_parent.CurrentSale.Player.ActiveStatusList.Exists(s => s.IsAlert && s.Banned))
                {
                    m_parent.CurrentSale.SetPlayer(null, true, true);
                    ClearPlayer();
                }

                //recalculate discount if any discounts required a player.
                m_parent.UpdateAutoDiscounts();
                
                SetPlayer();
                UpdateMenuButtonStates();
                UpdateSaleInfo();

                if (!e.PlayerDidNotChange && e.Player.ScheduledSalesObject != null && ((List<ScheduledSaleInfo>)e.Player.ScheduledSalesObject).Count > 0) //process the scheduled sale
                {
                    ProcessSaleFromList((List<ScheduledSaleInfo>)e.Player.ScheduledSalesObject, true);
                }

                if (m_parent.Settings.ThirdPartyPlayerInterfaceID != 0 && m_parent.CurrentSale.Player.ThirdPartyInterfaceDown)
                    throw new POSException(Resources.PlayerTrackingInterfaceDown);
            }
            else if (e.Error != null)
            {
                m_playerInfoList.Items.Clear();

                if (m_parent.WeAreAPOSKiosk)
                {
                    m_playerCardPictureLabel.Visible = true;
                    m_playerCardPicture.Visible = true;
                    m_parent.ShowMessage(this, m_displayMode, Resources.PleaseSeeCashier + (e.Error.Message.ToUpper() != Resources.PleaseSeeCashier.ToUpper() ? "\r\n\r\n" + e.Error.Message : string.Empty));
                }
                else
                {
                    m_playerInfoList.Items.Add(e.Error.Message);
                }

                if (e.Error is ServerCommException)
                    m_parent.ServerCommFailed();
            }
        }

        /// <summary>
        /// Actions that occur when the point of sale is processing something asynchronously
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBusyStatusChanged(object sender, BusyChangedEventArgs e)
        {
            // communication with the server is occurring, disable things that require server communication

            Action updateUI = (() => // JAN - using the lambda functionality instead of creating a new method that's only used here.
            {
                m_keypad.Option1Enabled = m_keypad.Option2Enabled = m_keypad.Option3Enabled = m_keypad.Option4Enabled = !e.IsBusy;
                m_keypad.BigButtonEnabled = !e.IsBusy;
                m_exitButton.Enabled = !e.IsBusy;
                if (e.IsBusy)
                {
                    if (m_buttonMenu != null) // disable the buttons that have to do with player info. In the future, we'd probably want to disable buttons that send messages to the server
                    {
                        m_buttonMenu.SetEnabled(ScanCardId, !e.IsBusy);
                        m_buttonMenu.SetEnabled(PlayerMgmtId, !e.IsBusy);
                        if (e.IsBusy)
                            EnabledNonSaleButtons(false);
                    }
                }
                else
                {
                    UpdateSystemButtonStates();
                }
            } );

            if (this.InvokeRequired) // if it's not coming in on the UI thread, move the work to the UI thread.
                this.BeginInvoke(updateUI);
            else
                updateUI();
        }

        /// <summary>
        /// Displays a WaitForm and retrieve's player information for the existing player from 
        /// the server updating the player points.  Asks for player card PIN if needed.
        /// </summary>
        /// <param name="forceIt">Set to TRUE to force a re-read of up-to-date points.</param>
        /// <returns>false if points were not updated.</returns>
        public bool UpdatePlayerPoints(bool forceIt = false)
        {
            NotIdle();

            if (m_parent.Settings.ThirdPartyPlayerSyncMode == 3) //player tracking is disconnected
            {
                m_parent.ShowMessage(this, m_displayMode, string.Format(CultureInfo.CurrentCulture, (m_parent.Settings.EnableAnonymousMachineAccounts) ? Resources.GetMachineFailed : Resources.FailedToGetPoints, "Player Tracking interface disconnected."));
                return false;
            }

            bool result = true;

            if (m_parent.CurrentSale == null || m_parent.CurrentSale.Player == null)
                return false;

            if (!forceIt && m_parent.CurrentSale.Player.PointsUpToDate)
            {
                m_parent.CurrentSale.NeedPlayerCardPIN = false;
                return true;
            }

            int PIN = 0;
            bool newPIN = false;
            bool PINProblem = false;

            try
            {
                do
                {
                    if (PINProblem)
                        m_parent.ShowMessage(this, m_parent.Settings.DisplayMode, Resources.PlayerCardPINError);

                    if (m_parent.Settings.ThirdPartyPlayerInterfaceUsesPIN && !m_parent.CurrentSale.Player.WeHaveThePlayerCardPIN && (m_parent.Settings.ThirdPartyPlayerInterfaceNeedPINForPoints || m_parent.Settings.ThirdPartyPlayerInterfaceGetPINWhenCardSwiped))
                    {
                        newPIN = true;

                        PIN = GetPlayerCardPINFromUser(true);

                        if (PIN == 0) //PIN entry canceled.
                            return false;
                    }

                    // Spawn a new thread to find the player and wait until done.
                    m_parent.StartUpdatePlayerPoints(PIN);
                    m_parent.ShowWaitForm(this); // Block until we are done.

                    PINProblem = PIN != 0 && !m_parent.CurrentSale.Player.ThirdPartyInterfaceDown && (m_parent.CurrentSale.Player.PlayerCardPINError || !m_parent.CurrentSale.Player.PointsUpToDate);
                } while (PINProblem);

                m_parent.CurrentSale.NeedPlayerCardPIN = false;
                m_parent.CurrentSale.Player.WeHaveThePlayerCardPIN = PIN != 0 && !m_parent.CurrentSale.Player.ThirdPartyInterfaceDown && !m_parent.CurrentSale.Player.PlayerCardPINError && m_parent.CurrentSale.Player.PointsUpToDate;

                if (m_parent.Settings.ThirdPartyPlayerInterfaceID != 0 && m_parent.CurrentSale.Player.ThirdPartyInterfaceDown)
                {
                    if (forceIt)
                    {
                        //replace this throw with code to handle changing PIN with external system down
                        throw new POSException(Resources.PlayerTrackingInterfaceDown);
                    }
                    else
                    {
                        throw new POSException(Resources.PlayerTrackingInterfaceDown);
                    }
                }

                if (newPIN && m_parent.CurrentSale.Player.WeHaveThePlayerCardPIN && m_parent.CurrentSale.Player.PlayerCardPIN != PIN) //save the PIN with the player card number
                {
                    m_parent.StartSetPlayerCardPIN(m_parent.CurrentSale.Player.Id, PIN);
                    m_parent.ShowWaitForm(this); // Block until we are done.
                }
            }
            catch (Exception ex)
            {
                m_parent.Log("Failed to update points for player/machine: " + ex.Message, LoggerLevel.Severe);
                m_parent.ShowMessage(this, m_displayMode, string.Format(CultureInfo.CurrentCulture, (m_parent.Settings.EnableAnonymousMachineAccounts) ? Resources.GetMachineFailed : Resources.FailedToGetPoints, ex.Message));

                result = false;
            }

            if (!CheckForError())
            {
                SetPlayer();
                UpdateMenuButtonStates();
            }
            else
            {
                result = false;
            }

            return result;
        }

        /// <summary>
        /// Displays a WaitForm and retrieve's Product data from the server.
        /// </summary>
        /// <param name="barcode">The barcode data</param>
        private void GetProductDataByBarcode(string barcode)
        {
            try
            {
                m_parent.StartGetProduct(barcode);
                m_parent.ShowWaitForm(this);
                CheckForError();

                if (m_parent.GetProductDataButton != null)
                {
                    if (m_parent.CurrentSale != null && m_parent.GetProductDataButtonValues != null && m_parent.GetProductDataButtonValues.Length == 4)
                    {
                        //see if this serial number/audit number is already in this sale (could be in another package)
                        PaperPackInfo packInfo = new PaperPackInfo();

                        packInfo.SerialNumber = (string)m_parent.GetProductDataButtonValues[2];
                        packInfo.AuditNumber = (int)m_parent.GetProductDataButtonValues[1];

                        foreach (SaleItem s in m_parent.CurrentSale.GetItems())
                        {
                            if (s.IsPackageItem && s.Package.HasBarcodedPaper)
                            {
                                if (s.Package.ContainsPackInfo(packInfo))
                                {
                                    m_parent.GetProductDataButton = null;
                                    m_parent.Log(String.Format(Resources.DuplicateAuditNumber, packInfo.AuditNumber), LoggerLevel.Warning);
                                    m_parent.ShowMessage(this, m_displayMode, string.Format(CultureInfo.CurrentCulture, String.Format(Resources.DuplicateAuditNumber, packInfo.AuditNumber)));
                                    break;
                                }
                            }
                        }
                    }

                    if (m_parent.GetProductDataButton != null)
                        m_parent.GetProductDataButton.Click(this, m_parent.GetProductDataButtonValues);
                }
            }
            catch (POSUserCancelException)
            {
            }
            catch (Exception ex)
            {
                m_parent.Log("Failed to find the product for barcode: " + ex.Message, LoggerLevel.Warning);
                m_parent.ShowMessage(this, m_displayMode, string.Format(CultureInfo.CurrentCulture, Resources.GetProductDataByBarcodeFailed, ex.Message));
            }
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

            // PDTS 1064
            if (m_parent.MagCardReader.ProcessCharacter(e.KeyChar))
            {
                e.Handled = true; // Don't send to the active control.
            }
            else
            {
                m_parent.BarcodeScanner.ProcessCharacter(e.KeyChar);
                e.Handled = true;
            }
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

            if (m_parent.MagCardReader.MSRInputInProgress)
                return false;

            if (keyData == Keys.Enter)
            {
                // Pass the enter key to the barcode class
                m_parent.BarcodeScanner.ProcessCmdKey(keyData);
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the m_saleList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void m_saleList_SelectedIndexChanged(object sender, EventArgs e)
        {
            var salesItem = m_saleList.SelectedItem as SaleItem;

            if (salesItem == null)
            {
                //disable remove line button
                m_removeLineButton.Enabled = false;

                return;
            }

            //US3509
            //if product validation sale item, then prevent the user from deleting
            if (salesItem.IsValidationPackage)
            {
                //disable remove line button
                m_removeLineButton.Enabled = false;
            }
            else //else enable button
            {
                if (m_parent.SaleState == PointOfSale.SellingState.NotSelling || m_parent.SaleState == PointOfSale.SellingState.Selling)
                {
                    m_removeLineButton.Enabled = true;
                }
            }
        }

        private void SellingForm_KeyDown(object sender, KeyEventArgs e)
		{
            e.SuppressKeyPress = false;

            if (m_parent.MagCardReader.MSRInputInProgress)
                e.Handled = true;

            //stop space key from pressing whatever control is active
            if (e.KeyCode == Keys.Space)
                e.Handled = true;
		}

        private void GetPagesReadyForNextSale()
        {
            if (m_parent.WeAreAPOSKiosk) //we will always go back to the first page of the first menu
            {
                int FirstMenu = m_parent.HaveBingoMenu ? m_parent.IndexOfFirstBingoMenu : 0;
 
                if (m_menuList.SelectedIndex != FirstMenu) //we are not on the first menu, select it (will handle the rest)
                {
                    m_menuList.SelectedIndex = FirstMenu;
                }
                else //we are on the first menu, get back to the first page
                {
                    m_pageNavigator.CurrentPage = 1;
                    m_buttonMenu.CurrentPage = 0;
                }

                if (m_parent.WeAreANonAdvancedPOSKiosk)
                {
                    if (m_simpleKioskForm != null)
                        m_simpleKioskForm.StartOver();
                }
                else
                {
                    m_parent.CheckForMessages();
                }

                if (m_parent.WeAreAnAdvancedPOSKiosk && m_parent.WeHaveAStarPrinter && !GiveChangeAsB3Credit) //see if the printer is OK and has paper.
                    m_parent.CheckStarPrinterStatus();
            }
            else //go back to the first page if they want
            {
                if (m_parent.Settings.ReturnToPageOneAfterSale)
                {
                    m_pageNavigator.CurrentPage = 1;
                    m_buttonMenu.CurrentPage = 0;
                }

                if (m_parent.CashDrawerIsOpen(true))
                    POSMessageForm.Show(this, m_parent, Resources.PleaseCloseTheCashDrawer, POSMessageFormTypes.CloseWithCashDrawer);
            }
        }

        /// <summary>
        /// Sets the discount tool tip.
        /// </summary>
        /// <param name="messages">The messages.</param>
        internal void SetStairstepDiscountText(List<string> messages)
        {
            m_stairStepDiscountText = messages;

            if (!m_addDiscountTextToScreenReceipt)
            {
                if (m_parent.Settings.DisplayMode is WideDisplayMode)
                {
                    bool needCR = false;

                    m_noticeLabel.Text = string.Empty;

                    if (messages != null)
                    {
                        foreach (string s in messages)
                        {
                            m_noticeLabel.Text += (needCR ? "\r\n" : "") + s;
                            needCR = true;
                        }
                    }
                }
                else
                {
                    m_stairstepDiscountBox.BeginUpdate();

                    m_stairstepDiscountBox.Items.Clear();

                    if (messages != null)
                        m_stairstepDiscountBox.Items.AddRange(messages.ToArray());

                    m_stairstepDiscountBox.EndUpdate();
                }
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
            if (m_parent.WeAreNotAPOSKiosk || m_parent.WeAreAnAdvancedPOSKiosk)
            {
                m_parent.MagCardReader.CardSwiped -= CardSwiped;

                // Don't listen to the BarcodeScanned event anymore since we 
                // are closing.
                m_parent.BarcodeScanner.BarcodeScanned -= BarcodeScanned;
            }

            // US4809
            m_parent.GetPlayerCompleted -= OnGetPlayerCompleted;
            m_parent.BusyStatusChanged -= OnBusyStatusChanged;

            //look good on the way out
            Hide();
            System.Threading.Thread.Sleep(100);

            if (KioskForm != null)
            {
                KioskForm.Close();
                m_simpleKioskForm = null;
            }
        }

        private void m_saleList_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            if (e.Index > -1 && m_saleList.Items.Count > 0)
            {
                e.ItemWidth = 0;
                e.ItemHeight = 0;

                // Get the size of the string in this item.
                SizeF stringSize;
                
                if(m_saleList.Items[e.Index] as SaleItem == null) //assume it is a string
                    stringSize = e.Graphics.MeasureString(((string)m_saleList.Items[e.Index]), m_saleList.Font);
                else //it is a SaleItem
                    stringSize = e.Graphics.MeasureString(((SaleItem)m_saleList.Items[e.Index]).ToStringForSaleScreen(m_parent.Settings.LongPOSDescriptions), m_saleList.Font);

                // The width of the item is the width of the string (and optionally the image).
                e.ItemWidth = (int)stringSize.Width;

                // The height of the item is the bigger height of the image or string.
                e.ItemHeight = (int)stringSize.Height;
            }
        }

        private void m_saleList_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index > -1 && m_saleList.Items.Count > 0)
            {
                // Draw the highlight color if this item is selected.
                if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                    e.Graphics.FillRectangle(new SolidBrush(m_saleList.HighlightColor), e.Bounds);
                else
                    e.Graphics.FillRectangle(new SolidBrush(m_saleList.BackColor), e.Bounds);

                Rectangle currRect = e.Bounds;

                // Is this item a ColorListBoxItem?
                SaleItem listItem = m_saleList.Items[e.Index] as SaleItem;

                StringFormat format = new StringFormat();

                format.LineAlignment = StringAlignment.Center;
                format.Trimming = StringTrimming.EllipsisCharacter;

                Brush brush;

                if (listItem != null) //SaleItem
                {
                    if (listItem.IsCoupon && !m_parent.Settings.CouponTaxable)
                        brush = new SolidBrush(Color.Orange);
                    else
                        brush = new SolidBrush(m_saleList.ForeColor);

                    bool mergeValidation = !(m_parent.Settings.ProductValidationMaxQuantity != 0 && listItem.Session.IsMaxValidationEnabled);

                    e.Graphics.DrawString(listItem.ToStringForSaleScreen(m_parent.Settings.LongPOSDescriptions, mergeValidation), m_saleList.Font, brush, currRect, format);
                }
                else //string
                {
                    brush = new SolidBrush(Color.White);
                    e.Graphics.DrawString((string)m_saleList.Items[e.Index], m_saleList.Font, brush, currRect, format);
                }
            }
        }

        /// <summary>
        /// Updates the idle start time to now. 
        /// </summary>
        /// <param name="delay">If true, sets the idle start time to now plus a day 
        /// to stop the kiosk idle trigger.</param>
        /// <param name="forceNotIdle">If true, sets the idle start time to now even if a timeout is being forced.</param>
        public void NotIdle(bool delay = false, bool forceNotIdle = false)
        {
            if (m_parent.WeAreNotAPOSKiosk)
                return;

            //handle the progress bar on the main form
            if (m_kioskTimeoutProgress.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(delegate()
                {
                    m_kioskTimeoutProgress.Hide();
                    m_kioskTimeoutProgress.Value = 0;
                }));
            }
            else
            {
                m_kioskTimeoutProgress.Hide();
                m_kioskTimeoutProgress.Value = 0;
            }


            //handle the progress bar on the kiosk form
            if (m_simpleKioskForm != null)
            {
                if(m_simpleKioskForm.ProgressBar.InvokeRequired)
                {
                    MethodInvoker work = new MethodInvoker(m_simpleKioskForm.ProgressBar.Hide);
                    m_simpleKioskForm.Invoke(work);
                }
                else
                {
                    m_simpleKioskForm.ProgressBar.Hide();
                }

                m_simpleKioskForm.ProgressBar.Value = 0;
            }

            if (forceNotIdle)
            {
                m_idleSince = DateTime.Now;
            }
            else
            {
                if (delay)
                {
                    m_idleSince = DateTime.Now + TimeSpan.FromDays(1);
                }
                else
                {
                    if (m_idleSince > DateTime.Now - TimeSpan.FromMinutes(30)) //OK to change, not forcing timeout
                        m_idleSince = DateTime.Now;
                }
            }
        }

        /// <summary>
        /// Sets the Kiosk idle timer to timeout in the given number of seconds.
        /// </summary>
        /// <param name="timeoutIn">Number of seconds from now when timeout will happen.</param>
        public void ForceKioskTimeout(int timeoutIn = 10)
        {
            m_idleSince = DateTime.Now - TimeSpan.FromSeconds(KioskIdleLimitInSeconds + timeoutIn) - TimeSpan.FromHours(1);
        }

        private void UserActivityDetected(object sender, EventArgs e)
        {
            NotIdle();
        }

        private void m_saleList_Click(object sender, EventArgs e)
        {
            NotIdle();

            //incase something went wrong and they can't see the screen receipt, clean-up drawing
            while (m_saleListUpdateCount > 0)
            {
                m_saleList.EndUpdate();
                m_saleListUpdateCount--;
            }
        }

        private void m_total_TextChanged(object sender, EventArgs e)
        {
            if (m_simpleKioskForm != null)
                m_simpleKioskForm.UpdateKioskTotal(m_total.Text);
        }

        private void menuButton_TextChanged(object sender, EventArgs e)
        {
            if (m_simpleKioskForm != null)
                m_simpleKioskForm.UpdateButtonText((ImageButton)sender);
        }

        #endregion

        #region Properties

        public bool WeHaveAUnitSelectionSystemMenuButton
        {
            get
            {
                return m_buttonMenu.GetButtonEntry(UnitSelectId) != null;
            }
        }

        /// <summary>
        /// Gets the value in the selling form keypad.
        /// </summary>
        public decimal SellingKeypadValue
        {
            get
            {
                return m_keypad.Value;
            }
        }

        public SimpleKioskForm KioskForm
        {
            get
            {
                return m_simpleKioskForm;
            }

            set
            {
                m_simpleKioskForm = value;
            }
        }

        /// <summary>
        /// Gets or sets if there is change to give as B3 credit by making a new B3 sale.
        /// </summary>
        public bool GiveChangeAsB3Credit
        {
            get
            {
                return m_giveChangeAsB3Credit;
            }

            set
            {
                m_giveChangeAsB3Credit = value;
            }
        }

        /// <summary>
        /// Gets or sets the change amount to give as B3 credit by making a new B3 sale.
        /// </summary>
        public decimal ChangeToGiveAsB3Credit
        {
            get
            {
                return m_changeToGiveAsB3Credit;
            }

            set
            {
                m_changeToGiveAsB3Credit = value;
            }
        }

        #endregion

        #region Static Methods
        /// <summary>
        /// Creates and adds MenuButtons to the specified panel.
        /// </summary>
        /// <param name="form">The instance of the form to use for event 
        /// wire up. This parameter can be null if doEventWireUp is 
        /// false.</param>
        /// <param name="panel">The panel to add the buttons to.</param>
        /// <param name="doEventWireUp">true if the click events should be 
        /// wired to the form.</param>
        /// <param name="displayMode">The display mode to use when creating 
        /// the buttons.</param>
        public static void CreateMenuButtons(SellingForm form, Panel panel, bool doEventWireUp, DisplayMode displayMode)
        {
            // Remove all the old buttons.
            panel.Controls.Clear();

            int currentColumn = 0;
            int currentRow = 0;

            for(int x = 0; x < displayMode.MenuButtonsPerPage; x++)
            {
                ImageButton button = new ImageButton();

                // Set the common properties of all buttons.
                button.Name = "MenuButton" + x.ToString(CultureInfo.InvariantCulture);
                button.Font = displayMode.MenuButtonFont;
                button.Size = displayMode.MenuButtonSize;
                button.ImageNormal = Resources.GrayButtonUp;
                button.ImagePressed = Resources.GrayButtonDown;
                button.Stretch = true;
                button.ShowFocus = false;
                button.TabIndex = 0;
                button.TabStop = false;
                button.UseMnemonic = false; // Rally DE488

                if (form.m_parent.Settings.ShowQuantityOnMenuButtons)
                {
                    button.Padding = new Padding(0, 5, 0, button.Font.Height + 5 + 3);
                    button.SecondaryTextPadding = new Padding(5, 5, 5, 5);
                }

                if(form.m_parent.WeAreAnAdvancedPOSKiosk)
                    button.AutoBlackOrWhiteText = true;

                if (form.m_parent.WeAreAPOSKiosk)
                    button.UseClickSound = form.m_parent.Settings.UseKeyClickSoundsOnKiosk;

                if(doEventWireUp)
                    button.Click += new EventHandler(form.MenuButtonClick);

                button.TextChanged += new EventHandler(form.menuButton_TextChanged);

                // Determine where the button is going to be.
                Point buttonLoc = new Point(currentColumn * button.Width + displayMode.MenuButtonXSpacing, currentRow * button.Height + displayMode.MenuButtonYSpacing);

                button.Location = buttonLoc;
                panel.Controls.Add(button);

                // Check to see if a new column is needed.
                if(++currentRow >= displayMode.MenuButtonsPerColumn)
                {
                    currentRow = 0;
                    currentColumn++;
                }
            }
        }
        #endregion
    }
}