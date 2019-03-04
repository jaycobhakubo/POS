#region Copyright
// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2013 FortuNet

#endregion

// US2718 Adding support for getting the charity list from the server
// US2828 Adding support for being able to specify a specific
//  session for sales
// US4028 Adding support for checking card counts when each product is
//  added
//US4380: (US4337) POS: Display B3 Menu
//US4382: (US4337) POS: B3 Open sale
//US4397: (US1592) POS: B3 Hand Pay
//US4404: (US1592) POS: B3 Jackpot Payment
//US4395: (US1592) POS: B3 Unlock Accounts
//US4338: (US1592) POS: Redeem B3
//US3509: POS Product Validation
// DE12863: POS: Validation fee is not applied when adding a paper product by tapping menu button.
//US4323: (US4319) POS: Automatically award a discount
//US4549: POS: Re-open a bank
//US3509 (US4428) POS: Validate a pack of paper
//US4126: POS: Active sales session is set the POS refreshes when the session is closed at the caller.
//US4588: POS: Show spend needed to reach next discount level
//US4636: (US4319) POS Multiple discounts
//DE12919: Error found in US4588: POS: Show spend needed to reach next discount level.
//US4615: (US4319) POS support max discount
//US4617: (US4319) POS support discount schedule
//US4616: (US4319) POS support min discount
//DE12930: POS: Coupon is not removed from the transaction
//DE12967: POS: Spend level discount is not removed
//DE12968: Error found in US4615: (US4319) POS support max discount
//US4721: (US4324) POS: Support scheduling discounts per session through program calendar
//US4724: (US4722) POS: Support point multiplier
//DE12993: POS: Spend level discount is applied if spend exceeds the the spend range.
//US4321: (US4319) Discount based on quantity
//DE13200: Reopening a bank opens the POS even if selected No in dialog prompt
//US4852: Product Center > Coupons: Require spend
//US4320: (US4319) Limit how many times a discount can be used.
//DE13235: Error found in US4320: (US4319) Limit how many times a discount can be used. Max use = 0
//US4942: Product Center> Discounts: Exclude packages from qualifying spend
//US4958 POS: Display paper usage screen on startup
//US4976: POS: Only display paper usage screen when enter the POS for the firs time
//US1592/DE13358: "B3 Session" showing multiple times in the POS session list.
//US5117: POS: Automatically add package X when package Y has been added Z times
//US5192/DE13378 Implement B3 Sales permission. "Do you have permission to access B3 Sales"?
//US5192/DE13380 Implement B3 Redeem permission. "Do you have permission to access B3 Redeem"
//US5590: Added ability to view receipt from player management

using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Windows.Forms;
using System.Globalization;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using GTI.Controls;
using GTI.Modules.Shared;
using GTI.Modules.POS.UI;
using GTI.Modules.POS.Data;
using GTI.Modules.POS.Properties;
using GTI.Modules.PlayerCenter.Business;
using GTI.Modules.UnitManagement.Business;
using GTI.Modules.ReceiptManagement.Business;
using GTI.Modules.Shared.Business;
using GTI.Modules.Shared.Data;
using GTI.EliteCreditCards.Business;
using GTI.EliteCreditCards.Data;
using System.Collections.Concurrent;
using StarMicronics.StarIO;

namespace GTI.Modules.POS.Business
{
    /// <summary>
    /// Represents the Point of Sale application.
    /// </summary>
    internal sealed class PointOfSale
    {
        #region Constants and Data Types
        private const int ServerCommShutdownWait = 15000; //15 seconds
        private const double GamingDatePoll = 300000; //5 minutes
        private const double MessagesPoll = 5000; //5 seconds
        private const string LogPrefix = "POS - ";
        private const int MinQuantitySaleQty = 2;
        private const int MaxQuantitySaleQty = 999; // US2148
        private const int RefreshMenusMessageId = 2010; // PDTS 964 - UK Support for POS
        private const int UpdateButtonsMessageId = 2011;
        private const int NewGamingDateMessageId = 2013; // Rally US613 - Server initiated message for the POS and Caller
        private const int PlayTypeSwitchMessageId = 2014; // Rally US419
        private const int B3SessionChangedCommandId = 2030; //US4380 POS: Display B3 Menu
        internal const string PlayItSheetFileName = @"\TempCBBPlayItSheet.rpt";
        private const string MachineBankName = "Machine {0}";
        private const string StaffBankName = "Staff {0}";
        private const int Precision = 2; // Rally TA7465
        private const int LicenseExpirationLimit = 7; // Days

        /// <summary>
        /// Represents the current state of a sale.
        /// </summary>
        internal enum SellingState
        {
            NotSelling,
            Selling,
            QuantitySelling,
            Tendering,
            Finishing,
        }

        private class PlayerLookupInfo
        {
            public int playerID = 0;
            public string CardNumber = string.Empty;
            public int PIN = 0;
            public bool UpdateCurrentPlayer = false;
            public bool WaitFormDisplayed = false;
        }
        #endregion
        
        #region Events
        // Attempt at handling asynch message calls similar to how Money Center Handles it

        /// <summary>
        /// Occurs when a session bingo game's payout settings have changed.
        /// </summary>
        public event EventHandler<GetPlayerEventArgs> GetPlayerCompleted;

        /// <summary>
        /// Occurs when the point of sale's 'IsBusy' flag changes value. (normally used when a message is being sent to the server asynchronously)
        /// </summary>
        public event EventHandler<BusyChangedEventArgs> BusyStatusChanged;

        #endregion

        #region Member Variables
        // System Related
        private bool m_initialized;
        private int m_assignedId = -1; // PDTS 966 - Server Initiated Messages Support
        private bool m_shuttingDown = false;

        private int m_deviceId;
        private int m_machineId;
        private string m_machineDesc;

        private bool m_weHaveAStarPrinter = false;
        private IPort m_starPrinterPort;
        private StarPrinterStatus m_starPrinterStatus;
        private System.Timers.Timer m_starPrinterStatusTimer = null;
        private object m_starPrinterStatusLockObject = new object();

        private List<TenderTypeValue> m_tenderTypes;
        private Dictionary<int, string> m_subTenderNames;

        private POSSettings m_settings;
        private bool m_loggingEnabled;
        private object m_logSync = new object();
        
        private BackgroundWorker m_worker;

        private Exception m_asyncException;
        private object m_errorSync = new object();
        
        // POS Related
        private Operator m_currentOp;
        private Staff m_currentStaff;

        private DateTime m_gamingDate;
        private object m_gamingDateSync = new object();
        
        //B3 related
        private bool m_b3SessionActive;
        private B3Session m_currentB3Session;
        private POSMenuListItem m_b3MenuListItem;
        B3RetrieveAccountForm m_b3RetrieveAccountForm;
   

        private bool m_canUpdateMenus = false;
        private POSMenuListItem[] m_menuList;
        private int m_currentMenuIndex;
        private object m_menuSync = new object();

        public CreditCardProcessingReply m_lastTenderProcessingReply = new CreditCardProcessingReply();
        public TenderItem m_lastTenderItem = null;
        public int m_registerReceiptID = 0;

        public class PackInfo
        {
            string serialNumber = string.Empty;
            int auditNumber = 0;

            public PackInfo()
            {
            }

            public PackInfo(string serial, int audit)
            {
                serialNumber = serial;
                auditNumber = audit;
            }

            public string SerialNumber
            {
                get
                {
                    return serialNumber;
                }
             
                set
                {
                    serialNumber = value;
                }
            }

            public int AuditNumber
            {
                get
                {
                    return auditNumber;
                }

                set
                {
                    auditNumber = value;
                }
            }
        }

        // FIX: DE1930 - Ability to track sales by drawer in addition to staff.
        // Rally TA7465
        private Bank m_bank;
        private Currency m_defaultCurrency;
        private Currency m_currentCurrency;
        private List<Currency> m_currencies; 

        // PDTS 966
        private object m_msgSync = new object();
        private Queue<MessageReceivedEventArgs> m_pendingMsgs;
        private System.Timers.Timer m_msgTimer;
        private object m_msgProcessing = new object();
        private System.Timers.ElapsedEventArgs m_dummyTimerElapsedEventArgs = null;
        private DateTime m_lastServerMessageCheck = DateTime.Now;

        // PDTS 1064
        private MagneticCardReader m_magCardReader;

        // US2591
        private BarcodeReader m_barcodeReader;

        // Sale Related
        private Sale m_currentSale;
        private Sale m_lastSale;
        private SalesReceipt m_lastReceipt;
        private bool m_lastReceiptHasPlayIt; // Rally US505
        private SellingState m_sellingState;
        private Dictionary<int, Charity> m_charities = new Dictionary<int, Charity>(); // US2718
        private PayCouponForm4 m_couponForm = null;
        private bool m_processingTender = false;
        private bool m_kioskChangeDispensingFailure = false;

        // Bingo Related
        private CardLevel[] m_cardLevels;
        private CrystalBallManager m_cbbManager;

        // Receipt Mgmt Module.
        private ReceiptManager m_receiptManager;

        // Player Module
        private PlayerManager m_playerCenter;

        // Unit Management Module
        private UnitManager m_unitMgmt;
        private List<Discount> m_autoDiscounts;
        private Dictionary<Discount, List<PackageButton>> m_autoDiscountsByQuantity;

        // TODO Revisit Super Pick payouts.
        /*
        // Super Pick Related
        private SuperPickListItem[] m_lastFindSuperPicksResults = null;
        private object m_findSuperPickSync = new object();

        private int m_lastSuperPickTransactionNumber = 0;
        private string m_lastSuperPickPlayerNumber = null;
        private string m_lastSuperPickPlayerFName = null;
        private string m_lastSuperPickPlayerLName = null;
        private Bitmap m_lastSuperPickPlayerPic = null;
        private object m_findSuperPickPlayerSync = new object();
        private ArrayList m_lastSuperPickWinnerList = null;
        private PaySuperPickListItem[] m_lastPaySuperPicksResults = null;
        private object m_paySuperPickSync = new object();
        */

        // UIs
        private LoadingForm m_loadingForm;
        private SellingForm m_sellingForm;
        private WaitForm m_waitForm;
        private SaleStatusForm m_statusForm; // PDTS 583
        private volatile bool m_isBusy;

        private bool m_tenderingScreenActive = false;
        private bool m_couponScreenActive = false;
        private bool m_helpScreenActive = false;

        private bool m_clearScreenForGuardian = false;
        private bool m_GuardianIsSuspending = false;

        private Screen m_ourScreen = null;
        private Form m_kioskSecondMonitor = null;

        private GuardianWrapper m_Guardian = null;
        private bool m_suspendedWhileInitializing = false;
        private bool m_shutdownWhileInitializing = false;
        private object m_GuardianRequestLockObject = new object();

        /// <summary>
        /// A collection of messages pending to be sent to the server. 
        /// 
        /// NOTE: Should be used to cancel new messages that are duplicates of pending messages. The queue of messages to actually be processed are in EliteMCP
        /// </summary>
        private ConcurrentQueue<ServerMessage> pendingMessages = new ConcurrentQueue<ServerMessage>();
        #endregion

        #region Member Methods
        /// <summary>
        /// Initializes all the POS's data.
        /// </summary>
        /// <param name="assignedId">The id assigned to this module when it 
        /// was started.</param>
        public void Initialize(int assignedId)
        {
            // Check to see if we are already initialized.
            if(m_initialized)
                return;

            m_ourScreen = Screen.FromPoint(Cursor.Position);
            m_assignedId = assignedId;

            ModuleComm modComm = null;

            // Get the system related ids.
            try
            {
                modComm = new ModuleComm();

                m_deviceId = modComm.GetDeviceId();
                //m_deviceId = Device.POSPortable.Id; //%%% force to draw compact for testing
                m_machineId = modComm.GetMachineId();
                m_machineDesc = modComm.GetMachineDescription();
            }
            catch(Exception e)
            {
                MessageBoxOptions options = 0;

                if(CultureInfo.CurrentCulture.TextInfo.IsRightToLeft)
                    options = MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign;

                MessageBox.Show(string.Format(CultureInfo.CurrentCulture, Resources.GetDeviceInfoFailed, e.Message), 
                                Resources.POSName, MessageBoxButtons.OK, MessageBoxIcon.Error, 
                                MessageBoxDefaultButton.Button1, options);               
                return;
            }

            // Create a settings object with the default values.           
            m_settings = new POSSettings();

            // Check to see what resolution to run in.
            if (m_deviceId == Device.POSPortable.Id)
            {
                m_settings.DisplayMode = new CompactDisplayMode();
            }
            else
            {
                Rectangle rect = m_ourScreen.Bounds; //get screen dimmensions

                if (WeAreAPOSKiosk) //showing POS Kiosk overlay screens
                {
                    bool not4x3 = (double)rect.Width / (double)rect.Height != 4D / 3D;

                    if (not4x3 && rect.Width >= 1366 && rect.Height >= 768) //not 4:3 ratio and big enough for widescreen
                    {
                        m_settings.DisplayMode = new WideDisplayMode(false, true); //our form is full screen with widescreen art centered in it
                    }
                    else //4:3 or too small for widescreen
                    {
                        if (not4x3)
                            m_settings.DisplayMode = new NormalDisplayMode(true); //use full screen mode
                        else
                            m_settings.DisplayMode = new NormalDisplayMode();
                    }
                }
                else //showing POS screen
                {
                    GetSettingsMessage settingsMsg = new GetSettingsMessage(m_machineId, modComm.GetOperatorId(), 0, Setting.AllowWidescreenPOS);

                    try
                    {
                        settingsMsg.Send();
                        m_settings.LoadSetting(settingsMsg.Settings[0]);

                        settingsMsg.GlobalSettingId = Setting.WidescreenPOSHasTwoMenuPagesPerPage;
                        settingsMsg.Send();
                        m_settings.LoadSetting(settingsMsg.Settings[0]);
                    }
                    catch (Exception e)
                    {
                        ReformatException(e);
                    }

                    if (Settings.AllowWidescreenPOS && (double)rect.Width / (double)rect.Height == 16D / 9D) //we have a 16:9 monitor
                        m_settings.DisplayMode = new WideDisplayMode(WeAreAPOSKiosk ? false : m_settings.ShowTwoMenuPagesPerPage);
                    else
                        m_settings.DisplayMode = new NormalDisplayMode();
                }
            }

            if (WeAreASimplePOSKiosk) //fit 4 pages on the screen
                m_settings.DisplayMode.MenuPagesPerPOSPage = 4;

            //FlexTenderingForm.BlackScreenOnCreate = !WeAreAPOSKiosk || WeAreAnAdvancedPOSKiosk;

            // Create and show the loading form.
            m_loadingForm = new LoadingForm(m_settings.DisplayMode);

            m_loadingForm.ApplicationName = WeAreAPOSKiosk?Resources.POSKioskName : Resources.POSName;
            m_loadingForm.Version = GetVersionAndCopyright(false);
            m_loadingForm.Cursor = Cursors.WaitCursor;
            m_loadingForm.Show();

            // Get the machine's settings from the server.
            m_loadingForm.Message = Resources.LoadingMachineInfo;
            Application.DoEvents();

            try
            {
                GetSettings(modComm.GetOperatorId());

                if (WeAreAPOSKiosk) //reign in some settings and connect to the Guardian
                {
                    if (Settings.StabilizeCabinet)
                    {
                        //if we are in a cabinet, devices are not available right away and we need to wait
                        //make sure Windows has been up for at least 90 seconds 
                        using (var uptime = new PerformanceCounter("System", "System Up Time"))
                        {
                            uptime.NextValue(); //Call this an extra time before reading its value

                            int secondsUntilStabilized = 90 - (int)TimeSpan.FromSeconds(uptime.NextValue()).TotalSeconds;

                            if (secondsUntilStabilized > 0) //still more waiting to do
                            {
                                while (secondsUntilStabilized > 0)
                                {
                                    m_loadingForm.Message = "Stabilizing cabinet... " + secondsUntilStabilized.ToString();
                                    Application.DoEvents();

                                    System.Threading.Thread.Sleep(1000);
                                    secondsUntilStabilized--;
                                }
                            }
                        }
                    }

                    m_settings.AllowNoSale = false;
                    m_settings.Tender = TenderSalesMode.PreventNegative;

                    if (WeAreAB3Kiosk)
                    {
                        m_settings.AllowB3OnKiosk = false;
                    }
                    else
                    {
                        if(m_settings.EnableB3Management)
                            m_settings.EnableB3Management = m_settings.AllowB3OnKiosk || m_settings.ChangeDispensing != POSSettings.OptionsForGivingChange.Normal;
                    }

                    m_settings.EnableFlexTendering = true;
                    m_settings.AllowSplitTendering = m_settings.AllowSplitTendering && WeAreAnAdvancedPOSKiosk && !m_settings.UseSimplePaymentForAdvancedKiosk;
                    m_settings.UseSystemMenuForUnitSelection = true;
                    m_settings.ReturnToPageOneAfterSale = true;
                    m_settings.Use00ForCurrencyEntry = false;
                    m_settings.UseGuardian = true;
                    m_settings.SellPreviouslySoldItem = POSSettings.SellAgainOption.Disallow;
                    m_settings.ShowQuantityOnMenuButtons = WeAreAnAdvancedPOSKiosk;
                    m_settings.ReceiptCopies = 1;

                    if (!m_settings.AllowCreditCardsOnKiosks)
                    {
                        m_settings.AllowCreditCardTender = false;
                        m_settings.AllowDebitCardTender = false;
                    }
                }
                else //not a kiosk
                {
                    m_settings.UseGuardian = false;
                }
            }
            catch(Exception e)
            {
                ShowMessage(m_loadingForm, m_settings.DisplayMode, string.Format(CultureInfo.CurrentCulture, Resources.GetSettingsFailed, e.Message));
                return;
            }

            if (WeAreAPOSKiosk && m_settings.UseGuardian)
            {
                m_Guardian = new GuardianWrapper(this, "Kiosk Subsystem", m_settings.GuardianEndPoint);

                if (!InitGuardian(m_loadingForm))
                {
                    try
                    {
                        m_Guardian.Dispose();
                        m_Guardian = null;
                    }
                    catch (Exception)
                    {
                    }

                    return;
                }
            }

            // Get the allowed tender types from the server
            try
            {
                GetValidPOSTenderTypesFromServer();
                m_settings.SetValidPOSTenders(m_tenderTypes);

                GetSubTenderNamesFromServer();
            }
            catch (Exception e)
            {
                ShowMessage(m_loadingForm, m_settings.DisplayMode, string.Format(CultureInfo.CurrentCulture, Resources.GetSettingsFailed, e.Message));
                return;
            }

            // Check to see if we want to log everything.
            try
            {
                if(m_settings.EnableLogging)
                {
                    Logger.EnableFileLog(m_settings.LoggingLevel, m_settings.FileLogRecycleDays);
                    Logger.StartLogger(Logger.StandardPrefix);
                    m_loggingEnabled = true;
                    Log(string.Format(CultureInfo.InvariantCulture, "Initializing POS ({0})...", GetVersionAndCopyright(false)), LoggerLevel.Information);
                }
            }
            catch(Exception e)
            {
                ShowMessage(m_loadingForm, m_settings.DisplayMode, string.Format(CultureInfo.CurrentCulture, Resources.LogFailed, e.Message));
                return;
            }

            // Check to see if we only want to display in English.
            if(m_settings.ForceEnglish)
            {
                ForceEnglish();
                Log("Forcing English.", LoggerLevel.Configuration);
            }

            if(!m_settings.ShowCursor)
                Cursor.Hide();

            // FIX: DE1938 - Change how taxes are calculated
            try
            {
                GetHallInfo();
            }
            catch(Exception e)
            {
                ShowMessage(m_loadingForm, m_settings.DisplayMode, string.Format(CultureInfo.CurrentCulture, Resources.GetHallInfoFailed, e.Message));
                Log("Get hall info. failed: " + e.Message, LoggerLevel.Severe);
                return;
            }
            // END: DE1938

            // Get device hardware attributes.
            try
            {
                GetDeviceHardwareAttributes();
            }
            catch(Exception e)
            {
                ShowMessage(m_loadingForm, m_settings.DisplayMode, string.Format(CultureInfo.CurrentCulture, Resources.GetHardwareAttribsFailed, e.Message));
                Log("Get device hardware attributes failed: " + e.Message, LoggerLevel.Severe);
                return;
            }

            // Load the selected operator's info.
            m_loadingForm.Message = Resources.LoadingOperatorInfo;
            Application.DoEvents();

            try
            {
                m_currentOp = GetOperator(modComm.GetOperatorId());
            }
            catch(Exception e)
            {
                ShowMessage(m_loadingForm, m_settings.DisplayMode, string.Format(CultureInfo.CurrentCulture, Resources.GetOperatorDataFailed, e.Message));
                Log("Get operator data failed: " + e.Message, LoggerLevel.Severe);
                return;
            }

            // Get the devices available to sell, if allowed.
            try
            {
                if(m_settings.AllowElectronicSales)
                    GetAvailableDevices();
            }
            catch(Exception e)
            {
                ShowMessage(m_loadingForm, m_settings.DisplayMode, string.Format(CultureInfo.CurrentCulture, Resources.GetDevicesFailed, e.Message));
                Log("Get available devices failed: " + e.Message, LoggerLevel.Severe);
                return;
            }

            // TTP 50138
            // Check to see which features are online.
            try
            {
                GetAvailableFeatures();
            }
            catch(Exception e)
            {
                ShowMessage(m_loadingForm, m_settings.DisplayMode, string.Format(CultureInfo.CurrentCulture, Resources.GetFeaturesFailed, e.Message));
                Log("Get features failed: " + e.Message, LoggerLevel.Severe);
                return;
            }

            // Get the staff info.
            m_loadingForm.Message = Resources.LoadingStaffInfo;
            Application.DoEvents();

            try
            {
                m_currentStaff = GetStaff(modComm.GetStaffId());
            }
            catch(Exception e)
            {
                ShowMessage(m_loadingForm, m_settings.DisplayMode, string.Format(CultureInfo.CurrentCulture, Resources.GetStaffDataFailed, e.Message));
                Log("Get staff info failed: " + e.Message, LoggerLevel.Severe);
                return;
            }

            try
            {
                // Rally TA1583
                // US5192/DE13378
                POSSecurity.GetStaffPermissionList(m_currentStaff, m_settings.EnableB3Management);

            }
            catch(Exception e)
            {
                ShowMessage(m_loadingForm, m_settings.DisplayMode, string.Format(CultureInfo.CurrentCulture, Resources.GetStaffPermsFailed, e.Message));
                Log("Get staff module features failed: " + e.Message, LoggerLevel.Severe);
                return;
            }

            // Get the gaming date from the server.
            m_loadingForm.Message = Resources.LoadingGameDate;
            Application.DoEvents();

            try
            {
                GamingDate = GetGamingDate(m_currentOp.Id);
            }
            catch(Exception e)
            {
                ShowMessage(m_loadingForm, m_settings.DisplayMode, string.Format(CultureInfo.CurrentCulture, Resources.GetGamingDateFailed, e.Message));
                Log("Get gaming date failed: " + e.Message, LoggerLevel.Severe);
                return;
            }

            // Rally TA7465
            // Get the currency info from the server.
            m_loadingForm.Message = Resources.LoadingCurrencies;
            Application.DoEvents();

            try
            {
                if(!GetCurrencies())
                {
                    ShowMessage(m_loadingForm, m_settings.DisplayMode, Resources.RatesNotSet);
                    return;
                }
            }
            catch(Exception e)
            {
                ShowMessage(m_loadingForm, m_settings.DisplayMode, string.Format(CultureInfo.CurrentCulture, Resources.GetCurrenciesFailed, e.Message));
                Log("Get currencies failed: " + e.Message, LoggerLevel.Severe);
                return;
            }
            // END: TA7465

            // Load the card levels.
            m_loadingForm.Message = Resources.LoadingBingoData;
            Application.DoEvents();

            try
            {
                GetCardLevels();
            }
            catch(Exception e)
            {
                ShowMessage(m_loadingForm, m_settings.DisplayMode, string.Format(CultureInfo.CurrentCulture, Resources.GetCardLevelsFailed, e.Message));
                Log("Get card levels failed failed:" + e.Message, LoggerLevel.Severe);
                return;
            }

            // PDTS 537
            // Initialize the CBB Manager.
            try
            {
                m_cbbManager = new CrystalBallManager(this);
            }
            catch(Exception e)
            {
                ShowMessage(m_loadingForm, m_settings.DisplayMode, string.Format(CultureInfo.CurrentCulture, Resources.CBBLoadError, e.Message));
                Log("Failed to initialize the cbb manager:" + e.Message, LoggerLevel.Severe);
                return;
            }

            // PDTS 1064
            // Initialize the mag. card reader.
            try
            {
                m_magCardReader = new MagneticCardReader(m_settings.MSRSettingInfo);

                if(m_deviceId == Device.POSPortable.Id && m_settings.MagCardMode == MagneticCardReaderMode.KeyboardAndCPCLTCP)
                {
                    CPCLPrinterTCPSource printerSource = new CPCLPrinterTCPSource(string.Empty, 1, 1);
                    printerSource.SetSettingsFromString(m_settings.MagCardModeSettings);
                    m_magCardReader.AddSource(printerSource);
                }
            }
            catch(Exception e)
            {
                ShowMessage(m_loadingForm, m_settings.DisplayMode, string.Format(CultureInfo.CurrentCulture, Resources.MagLoadError, e.Message));
                Log("Failed to initialize the mag. card reader:" + e.Message, LoggerLevel.Severe);
                return;
            }

            try
            {
                m_barcodeReader = new BarcodeReader();
            }
            catch (Exception e)
            {
                ShowMessage(m_loadingForm, m_settings.DisplayMode, string.Format(CultureInfo.CurrentCulture, Resources.BarcodeLoadError, e.Message));
                Log("Failed to initialize the barcode scanner:" + e.Message, LoggerLevel.Severe);
                return;
            }

            if (Settings.PaymentProcessingEnabled)
            {
                try
                {
                    // Load the payment processing interface
                    m_loadingForm.Message = Resources.LoadingPaymentProcessing;
                    Application.DoEvents();

                    var initParams = new CreditCardInitializationParameters()
                    {
                        ClientDataStoreAccessor = new ClientDataStoreAccessor(),
                        DebugLogger = (x) =>
                        {
                            Log(x, LoggerLevel.Debug);
                        },
                        MachineID = m_machineId,
                        Settings = m_settings
                    };

                    var creditCardInterface = EliteCreditCardFactory.Create(initParams);
                    if (creditCardInterface != null)
                    {
                        while (!creditCardInterface.IsInitialized)
                            Application.DoEvents();

                        creditCardInterface.SetLaneOpenDisplay(true);
                    }
                    else
                    {
                        // TODO: throw an exception to log the creation of no interface when one was 
                        //       requested (Settings.CreditCardProcessor != CreditCardProcessors.None) 
                        //       is checked before initialization.
                    }
                }
                catch (Exception e)
                {
                    ShowMessage(m_loadingForm, m_settings.DisplayMode, string.Format(CultureInfo.CurrentCulture, Resources.Shift4LoadError, e.Message));
                    Log("Failed to initialize the credit card interface:" + e.Message, LoggerLevel.Severe);
                    return;
                }
            }

            if (WeAreAPOSKiosk || Settings.ForceAuthorizationOnVoidsAtPOS || m_currentStaff.CheckModule(EliteModule.ReceiptManagement)) //Kiosk might need to reprint a receipt from a crash
            {
                // Create our own instance of Receipt Mgmt.
                m_loadingForm.Message = Resources.LoadingReceiptMgmt;
                Application.DoEvents();

                try
                {
                    m_receiptManager = new ReceiptManager();
                    m_receiptManager.Initialize(false, true, -1); // Rally US419

                    // Rally DE1813 - POS sometimes encounters a mag. card error on start up.
                    if (m_deviceId == Device.POSPortable.Id && m_settings.MagCardMode == MagneticCardReaderMode.KeyboardAndCPCLTCP)
                    {
                        // PDTS 1064
                        // Replace ReceiptMgmt's MagCardReader with ours (only
                        // if we are using the TCP mag card reader).
                        m_receiptManager.SetExternalMagCardReader(MagCardReader);
                    }

                    Log("Receipt Mgmt initialized.", LoggerLevel.Debug);
                }
                catch (Exception e)
                {
                    ShowMessage(m_loadingForm, m_settings.DisplayMode, e.Message);
                    Log("Loading Receipt Mgmt failed: " + e.Message, LoggerLevel.Severe);
                    return;
                }
            }
            else
            {
                Log("Loading Receipt Mgmt bypassed.", LoggerLevel.Configuration);
            }

            if (!m_settings.EnableAnonymousMachineAccounts && m_currentStaff.CheckModule(EliteModule.PlayerCenter))
            {
                // Create our own instance of the Player Center.
                m_loadingForm.Message = Resources.LoadingPlayerCenter;
                Application.DoEvents();

                try
                {
                    m_playerCenter = new PlayerManager(null);

                    //US5590: listen for event when receipt number was clicked so we can display receipt managment
                    m_playerCenter.ReceiptClicked += ViewReceiptClicked;
                    
                    // FIX: DE2476
                    m_playerCenter.Initialize(false, true);
                    // END: DE2476

                    // Rally DE1813
                    if (m_deviceId == Device.POSPortable.Id && m_settings.MagCardMode == MagneticCardReaderMode.KeyboardAndCPCLTCP)
                    {
                        // PDTS 1064
                        // Replace PlayerCenter's MagCardReader with ours (only
                        // if we are using the TCP mag card reader).
                        m_playerCenter.SetExternalMagCardReader(MagCardReader);
                    }
                    else
                        m_playerCenter.BeginMagCardReading(); // Rally DE1852

                    Log("Player Center initialized.", LoggerLevel.Debug);
                }
                catch (Exception e)
                {
                    ShowMessage(m_loadingForm, m_settings.DisplayMode, e.Message);
                    Log("Loading Player Center failed: " + e.Message, LoggerLevel.Severe);
                    return;
                }
            }
            else
            {
                Log("Loading Player Center bypassed.", LoggerLevel.Configuration);
            }

            // Create our own instance of the Unit Management object.
            // PDTS 964
            if (m_settings.AllowElectronicSales)
            {
                m_loadingForm.Message = Resources.LoadingUnitMgmt;
                Application.DoEvents();

                try
                {
                    m_unitMgmt = new UnitManager();
                    m_unitMgmt.Initialize(false);

                    if ((m_settings.HasTracker || m_settings.HasTraveler) && !m_unitMgmt.ConnectToCrate())
                        throw new POSException(Resources.CrateModuleNoConnection);

                    // Rally DE1813
                    if (m_deviceId == Device.POSPortable.Id && m_settings.MagCardMode == MagneticCardReaderMode.KeyboardAndCPCLTCP)
                    {
                        // PDTS 1064
                        // Replace UnitMgmt's MagCardReader with ours (only
                        // if we are using the TCP mag card reader).
                        m_unitMgmt.SetExternalMagCardReader(MagCardReader);
                    }

                    Log("Unit Mgmt initialized.", LoggerLevel.Debug);
                }
                catch (Exception e)
                {
                    m_settings.HasTracker = false;
                    m_settings.HasTraveler = false;

                    ShowMessage(m_loadingForm, m_settings.DisplayMode, e.Message + " " + Resources.NoPortableSales);
                    Log("Loading Unit Mgmt failed: " + e.Message, LoggerLevel.Severe);
                }
            }
            else
            {
                Log("Unit Mgmt initialization bypassed because electronic sales are not allowed.", LoggerLevel.Configuration);
            }

            // Get the menus for the selected operator and staff.
            m_loadingForm.Message = Resources.LoadingMenus;
            Application.DoEvents();

            try
            {
                GetStaffMenus(out m_menuList);

                if (m_menuList != null && m_menuList.Length != 0)
                    Array.Sort(m_menuList);

                try
                {
                    //US5192/DE13378
                    GetB3SessionActive();

                    if ((WeAreAPOSKiosk && B3SessionActive && (Settings.AllowB3OnKiosk || WeAreAB3Kiosk) && m_currentStaff.IsKiosk) ||
                        (WeAreNotAPOSKiosk &&
                         ((B3SessionActive && m_currentStaff.CheckModuleFeature(EliteModule.B3Center, (int)POSFeature.B3Sales)) ||
                          (!B3SessionActive && m_currentStaff.CheckModuleFeature(EliteModule.B3Center, (int)POSFeature.B3Redeem))
                         )
                        )
                       )
                    {
                        AddB3SessionToMenuList();
                    }
                }
                catch (Exception e)
                {
                    //catch, display and log exception. We do not necessarily want to prevent POS from opening
                    ShowMessage(m_loadingForm, m_settings.DisplayMode, string.Format(CultureInfo.CurrentCulture, Resources.GetMenusFailed, e.Message));
                    Log("Get B3 Session failed: " + e.Message, LoggerLevel.Severe);
                }

                if (WeAreNotAPOSKiosk)
                {
                    if (m_settings.EnableActiveSalesSession)
                    {
                        if (m_menuList == null || m_menuList.Length == 0)
                        {
                            ShowMessage(m_loadingForm, m_settings.DisplayMode, Resources.NoActiveMenu);
                            return;
                        }
                    }
                    else
                    {
                        if (m_menuList == null || m_menuList.Length == 0)
                        {
                            ShowMessage(m_loadingForm, m_settings.DisplayMode, Resources.NoMenus);
                            return;
                        }
                    }
                }
                else //we are a POS kiosk
                {
                    if (m_menuList == null)
                        m_menuList = new POSMenuListItem[0];
                }

                POSMenuListItem[] presalesMenu;

                GetStaffPreSalesMenus(out presalesMenu);

                if (presalesMenu != null && presalesMenu.Length != 0)
                {
                    Array.Sort(presalesMenu);

                    if (m_menuList != null && m_menuList.Length > 0)
                    {
                        var menuList = new List<POSMenuListItem>(m_menuList.Concat(presalesMenu));

                        m_menuList = menuList.ToArray();
                    }
                }
            }
            catch(Exception e)
            {
                ShowMessage(m_loadingForm, m_settings.DisplayMode, string.Format(CultureInfo.CurrentCulture, Resources.GetMenusFailed, e.Message));
                Log("Get menus failed: " + e.Message, LoggerLevel.Severe);
                return;
            }

            try
            {
                var packageId = 0;
                var getValidationMessage = new GetValidationPackagesMessage();

                getValidationMessage.Send();

                if (getValidationMessage.DefaultValidationPackage != null)
                    packageId = getValidationMessage.DefaultValidationPackage.PackageId;

                if (getValidationMessage.ReturnCode != 0)
                    throw new Exception(Resources.UnableToGetValidationPackageFromServer);

                //US3509: POS: Product Validation
                //Init Product Validation
                ValidationPackage validationPackage = new ValidationPackage
                {
                    CardCount = m_settings.ProductValidationCardCount,
                    MaxQuantity = m_settings.ProductValidationMaxQuantity,
                    PackageId = packageId
                };

                PackageButton defaultValidationPackageButton = null;

                ValidationEnabled = Settings.EnabledProductValidation;

                if (m_menuList != null)
                {
                    foreach (POSMenuListItem item in m_menuList)
                    {
                        for (int i = 1; i <= item.Menu.PageCount; i++)
                        {
                            var buttons = item.Menu.GetPage((byte)i);

                            foreach (var packageButton in buttons.OfType<PackageButton>())
                            {
                                if (packageButton.Package.Id == packageId) //is this the global default validation package?
                                    defaultValidationPackageButton = packageButton;

                                if (packageButton.Package.IsSessionDefaultValidationPackage)
                                {
                                    item.Session.ValidationPackage = new ValidationPackage
                                    {
                                        CardCount = m_settings.ProductValidationCardCount,
                                        MaxQuantity = m_settings.ProductValidationMaxQuantity,
                                        PackageId = packageButton.Package.Id
                                    };

                                    item.Session.DefaultValidationPackageButton = packageButton;
                                }
                            }
                        }
                    }

                    foreach (POSMenuListItem item in m_menuList)
                    {
                        if (item.Session != null && item.Session.ValidationPackage == null)
                        {
                            item.Session.ValidationPackage = validationPackage;
                            item.Session.DefaultValidationPackageButton = defaultValidationPackageButton;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ShowMessage(m_loadingForm, m_settings.DisplayMode, string.Format(CultureInfo.CurrentCulture, Resources.FailedToInitializeProductValidation, e.Message));
                Log(Resources.FailedToInitializeProductValidation + e.Message, LoggerLevel.Severe);
                return;
            }

            try
            {
                m_autoDiscounts = new List<Discount>();
                m_autoDiscountsByQuantity = new Dictionary<Discount, List<PackageButton>>();

                //get all discounts from server and filter by active, automatic, start date, and end date
                var discountItems = GetDiscountMessage.GetDiscountList(0, true).Where(d=>
                                                                                    d.IsActive &&
                                                                                    d.DiscountAwardType == DiscountItem.AwardTypes.Automatic && 
                                                                                    d.Type != DiscountType.Open &&
                                                                                    (d.StartDate == null || d.StartDate.Value.Date <= GamingDate.Date) &&
                                                                                    (d.EndDate == null || d.EndDate.Value.Date > GamingDate.Date));
                foreach (var discountItem in discountItems)
                {
                    if (discountItem.AdvancedType == DiscountItem.AdvanceDiscountType.Quantity)
                    {
                        //US5117: POS: Automatically add package X when package Y has been added Z times
                        //setup discounted package buttons so that we can add when the criteria is met
                        var discount = DiscountFactory.CreateDiscount(discountItem);
                        var quantityDiscountPackageButtons = ClonePackageButtonsForQuantityDiscount(discount, discountItem.AdvancedQuantityDiscount.GetPackageId);

                        if (quantityDiscountPackageButtons != null)
                        {
                            //auto discount by quantity (BOGO)
                            m_autoDiscountsByQuantity.Add(discount, quantityDiscountPackageButtons);
                        }
                    }
                    else
                    {
                        //create and add to auto discount list
                        m_autoDiscounts.Add(DiscountFactory.CreateDiscount(discountItem));
                    }
                }

                //make sure the SpendLevel (stairstep) discounts are processed last so we know the
                //largest discount we are dealing with before processing them 
                //(for "spend $x more to get $ off" display)
                m_autoDiscounts = m_autoDiscounts.OrderBy(i => i.DiscountItem.AdvancedType).ToList();
            }
            catch (Exception e)
            {
                ShowMessage(m_loadingForm, m_settings.DisplayMode, string.Format(CultureInfo.CurrentCulture, Resources.FailedToInitializeAutoDiscount, e.Message));
                Log(Resources.FailedToInitializeAutoDiscount + e.Message, LoggerLevel.Severe);
                return;
            }

            m_loadingForm.Message = Resources.LoadingSellingScreen;
            Application.DoEvents();

            BankOpenType initBank = BankOpenType.None;

            if (!WeAreAB3Kiosk && (WeAreNotAPOSKiosk || HaveBingoMenu))
            {
                try
                {
                    //US4976 get bank status
                    //returns -1 if no bank is opened
                    //returns 0 if gets an already opened bank
                    //returns 1 if opens a new bank
                    //returns 2 if re-opens a closed bank
                    initBank = InitializeBank();

                    if (initBank == BankOpenType.None)
                    {
                        return;
                    }
                }
                catch (Exception e)
                {
                    ShowMessage(m_loadingForm, m_settings.DisplayMode, string.Format(CultureInfo.CurrentCulture, Resources.InitialBankFailed, e.Message));
                    Log("Failed to query/set initial bank amount: " + e.Message, LoggerLevel.Severe);
                    return;
                }
            }
            // END: TA7465

            // PDTS 583
            // Load sale status and wait forms.
            m_statusForm = new SaleStatusForm(this, m_settings.DisplayMode);
            m_waitForm = new WaitForm(m_settings.DisplayMode);
            m_waitForm.WaitImage = Resources.Waiting;
            m_waitForm.Cursor = Cursors.WaitCursor;

            //US4958 POS: Display paper usage screen on startup
            //prompt paper usage
            if (Settings.EnablePaperUsage && Settings.ShowPaperUsageAtLogin && initBank > 0 && WeAreNotAPOSKiosk)
            {
                var paperUsageForm = new PaperUsageForm(m_loadingForm, this, GetVersionAndCopyright(true), initBank);
                paperUsageForm.ShowDialog(m_loadingForm);
            }

            if (m_suspendedWhileInitializing)
            {
                m_loadingForm.Hide();

                while (m_suspendedWhileInitializing)
                {
                    System.Threading.Thread.Sleep(100);
                    Application.DoEvents();
                }
            }

            if (m_shutdownWhileInitializing)
            {
                Shutdown();
                return;
            }

            m_loadingForm.Show();
            Application.DoEvents();

            // Create main selling screen.
            m_currentMenuIndex = HaveBingoMenu ? IndexOfFirstBingoMenu : 0;
            m_sellingForm = new SellingForm(this, m_settings.DisplayMode);
            m_sellingForm.SetGamingDate();
            m_sellingForm.SetStaff();
            m_sellingForm.SetOperator();
            m_sellingForm.SetVersion(GetVersionAndCopyright(true));
            m_sellingState = SellingState.NotSelling;
            m_sellingForm.UpdateSaleInfo();
            m_sellingForm.UpdateSystemButtonStates(); // PDTS 571
            m_sellingForm.LoadMenuList(m_menuList, m_currentMenuIndex);
            m_sellingForm.LoadMenu(CurrentMenu, 1);

            m_magCardReader.SynchronizingObject = m_sellingForm; // PDTS 1064
            m_barcodeReader.SynchronizingObject = m_sellingForm;
            
            m_loadingForm.Cursor = Cursors.Default;

            //If we are a kiosk with a second monitor, use the monitor as a glass pane and put a FortuNet logo on it.
            if (WeAreAPOSKiosk) //see if we have a second monitor to use
            {
                if (!Settings.KioskTestOneMonitor && Screen.AllScreens.Length > 1) //we have another monitor
                {
                    Screen[] screens = Screen.AllScreens;
                    Screen secondScreen = screens.First(s => s.Primary == false);
                    
                    //make a form that fills the monitor 
                    m_kioskSecondMonitor = new Form();
                    m_kioskSecondMonitor.FormBorderStyle = FormBorderStyle.None; //no caption bar
                    m_kioskSecondMonitor.Location = new Point(secondScreen.Bounds.X, secondScreen.Bounds.Y);
                    m_kioskSecondMonitor.Size =  new Size(secondScreen.Bounds.Width, secondScreen.Bounds.Height);
                    
                    //make a picture box to hold the logo and make it fill the screen
                    PictureBox pic = new PictureBox();
                    pic.Image = Resources.FnetWall;
                    pic.Size = m_kioskSecondMonitor.Size;
                    m_kioskSecondMonitor.Controls.Add(pic); //attach the picture box to the form

                    //see if we have an external picture to use
                    DriveInfo[] drives = DriveInfo.GetDrives();
                    List<DriveInfo> driveList = new List<DriveInfo>();

                    driveList.AddRange(drives);

                    driveList.Sort(
                                    delegate(DriveInfo x, DriveInfo y)
                                    {
                                        return -x.Name.CompareTo(y.Name);
                                    }
                                  );

                    foreach (DriveInfo di in driveList.FindAll(i => i.IsReady))
                    {
                        string testPath = di.Name + @"GameTech\Attract\Kiosk Top Glass\";

                        if (Directory.Exists(testPath))
                        {
                            string[] picList = Directory.GetFiles(testPath, "*.*", SearchOption.TopDirectoryOnly);

                            if (picList != null && picList.Length > 0)
                            {
                                try
                                {
                                    Image topGlass = Image.FromFile(picList[0]);

                                    if (topGlass != null)
                                        pic.Image = topGlass;
                                }
                                catch(Exception)
                                {
                                }
                            }
                        }
                    }

                    pic.SizeMode = PictureBoxSizeMode.StretchImage;
                    pic.Show();
                    m_kioskSecondMonitor.Show();

                    //move the form to the second monitor, bring it to the top, and don't let anyone focus on it (will mess up our MSR and scanner).
                    m_kioskSecondMonitor.SetBounds(secondScreen.Bounds.X, secondScreen.Bounds.Y, secondScreen.Bounds.Width, secondScreen.Bounds.Height);
                    m_kioskSecondMonitor.TopMost = true;
                    m_kioskSecondMonitor.BringToFront();
                    m_kioskSecondMonitor.Enabled = false;
                }
            }

            // PDTS 966
            // Create the timer that will check for any pending messages.
            lock(m_msgSync)
            {
                m_pendingMsgs = new Queue<MessageReceivedEventArgs>();
            }

            //see if we have a Star printer
            m_weHaveAStarPrinter = true;

            try
            {
                OpenStarPrinterPort();

                if (m_starPrinterPort == null)
                {
                    m_weHaveAStarPrinter = false;
                }
                else //we opened something, see if we can talk with it
                {
                    try
                    {
                        m_starPrinterPort.GetParsedStatus();
                    }
                    catch (Exception) //couldn't get the status, assume not a Star printer
                    {
                        m_weHaveAStarPrinter = false;
                    }
                    
                    OpenStarPrinterPort(false);
                }
            }
            catch (Exception)
            {
                m_starPrinterPort = null;
                m_weHaveAStarPrinter = false;
            }

            //if we are using the Guardian, prepare to monitor the status of the Star receipt printer (if we have one)
            if (m_Guardian != null && m_weHaveAStarPrinter)
            {
                m_starPrinterStatusTimer = new System.Timers.Timer(5000);
                m_starPrinterStatusTimer.AutoReset = false;
                m_starPrinterStatusTimer.SynchronizingObject = m_sellingForm;
                m_starPrinterStatusTimer.Elapsed += new System.Timers.ElapsedEventHandler(m_starPrinterStatusTimer_Elapsed);
            }

            // Subscribe to server messages.
            m_msgTimer = new System.Timers.Timer(MessagesPoll);
            m_msgTimer.AutoReset = true;
            m_msgTimer.SynchronizingObject = m_sellingForm;
            m_msgTimer.Elapsed += new System.Timers.ElapsedEventHandler(CheckForMessages);

            modComm.SubscribeToMessage(m_assignedId, RefreshMenusMessageId);
            modComm.SubscribeToMessage(m_assignedId, UpdateButtonsMessageId);
            // Rally US419
            modComm.SubscribeToMessage(m_assignedId, PlayTypeSwitchMessageId);
            // Rally US613
            modComm.SubscribeToMessage(m_assignedId, NewGamingDateMessageId);

            //ID: 2030; (US4337) POS: Display B3 Menu 
            modComm.SubscribeToMessage(m_assignedId, B3SessionChangedCommandId);

            Application.DoEvents();
            m_initialized = true;

            Log("POS initialized!", LoggerLevel.Debug);
        }

        /// <summary>
        /// Writes a message to the POS's log.
        /// </summary>
        /// <param name="message">The message to write to the log.</param>
        /// <param name="level">The level of the message.</param>
        /// <returns>true if success; otherwise false.</returns>
        public bool Log(string message, LoggerLevel level)
        {
            lock(m_logSync)
            {
                StackFrame frame = new StackFrame(1, true);
                string fileName = frame.GetFileName();
                int lineNumber = frame.GetFileLineNumber();
                message = PointOfSale.LogPrefix + message;

                if(m_loggingEnabled)
                {
                    try
                    {
                        switch(level)
                        {
                            case LoggerLevel.Severe:
                                Logger.LogSevere(message, fileName, lineNumber);
                                break;

                            case LoggerLevel.Warning:
                                Logger.LogWarning(message, fileName, lineNumber);
                                break;

                            default:
                            case LoggerLevel.Information:
                                Logger.LogInfo(message, fileName, lineNumber);
                                break;

                            case LoggerLevel.Configuration:
                                Logger.LogConfig(message, fileName, lineNumber);
                                break;

                            case LoggerLevel.Debug:
                                Logger.LogDebug(message, fileName, lineNumber);
                                break;

                            case LoggerLevel.Message:
                                Logger.LogMessage(message, fileName, lineNumber);
                                break;

                            case LoggerLevel.SQL:
                                Logger.LogSql(message, fileName, lineNumber);
                                break;
                        }

                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
                else
                    return false;
            }
        }

        /// <summary>
        /// Shows the main selling form modally.
        /// </summary>
        public void Start()
        {
            if(m_initialized && m_sellingForm != null)
            {
                Log("Starting POS.", LoggerLevel.Information);

                // PDTS 966
                if (m_msgTimer != null)
                {
                    m_lastServerMessageCheck = DateTime.Now;
                    m_msgTimer.Start();
                }

                if (m_starPrinterStatusTimer != null)
                    m_starPrinterStatusTimer.Start();

                Application.Run(m_sellingForm);
            }
        }

        /// <summary>
        /// Tells the POS to close the main selling form.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the 
        /// event data.</param>
        public void ClosePOS(object sender, EventArgs e)
        {
            // PDTS 964
            CanUpdateMenus = false;

            // PDTS 966
            if(m_msgTimer != null)
                m_msgTimer.Stop();

            if (m_starPrinterStatusTimer != null)
                m_starPrinterStatusTimer.Stop();

            if (WeHaveAGuardian)
            {
                //disconnect from Guardian
                m_Guardian.GuardianRequestedShutdown -= Guardian_GuardianRequestedShutdown;
                m_Guardian.GuardianRequestedControl -= Guardian_GuardianRequestedControl;
                m_Guardian.GuardianReleasedControl -= Guardian_GuardianReleasedControl;
                m_Guardian.Dispose();
                m_Guardian = null;

                System.Threading.Thread.Sleep(1000); //wait for the Guardian to show itself
            }

            if(m_sellingForm != null)
            {
                if (m_sellingForm.InvokeRequired)
                {
                    MethodInvoker del = m_sellingForm.Close;
                    m_sellingForm.Invoke(del);
                }
                else
                {
                    m_sellingForm.Close();
                }
            }
        }

        /// <summary>
        /// Tells the POS to bring the main form to the front.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the 
        /// event data.</param>
        internal void BringToFront(object sender, EventArgs e)
        {
            if(m_initialized && m_sellingForm != null)
            {
                if(m_sellingForm.InvokeRequired)
                {
                    MethodInvoker del = ActivateSellingForm;
                    m_sellingForm.Invoke(del);
                }
                else
                    ActivateSellingForm();
            }
        }

        /// <summary>
        /// Activates the selling form and sets its window state to Normal.
        /// </summary>
        private void ActivateSellingForm()
        {
            m_sellingForm.WindowState = FormWindowState.Normal;
            m_sellingForm.Activate();
        }

        private void OpenStarPrinterPort(bool openIt = true)
        {
            if (openIt)
            {
                try
                {
                    m_starPrinterPort = StarMicronics.StarIO.Factory.I.GetPort("usbprn:" + Settings.ReceiptPrinterName, string.Empty, 2000);
                }
                catch (Exception)
                {
                    m_starPrinterPort = null;
                }
            }
            else //close it
            {
                try
                {
                    Factory.I.ReleasePort(m_starPrinterPort);
                }
                catch (Exception)
                {
                }

                m_starPrinterPort = null;
            }
        }

        public void CheckStarPrinterStatus(bool allowLowPaper = false)
        {
            if(!m_weHaveAStarPrinter)
                return;

            lock (m_starPrinterStatusLockObject)
            {
                try
                {
                    OpenStarPrinterPort();
                    m_starPrinterStatus = m_starPrinterPort.GetParsedStatus();
                }
                catch (Exception)
                {
                    m_starPrinterStatus = new StarPrinterStatus();
                }

                if (m_starPrinterPort == null)
                    return;

                OpenStarPrinterPort(false);

                StringBuilder sbMessageForGuardian = new StringBuilder();

                if (m_starPrinterStatus.CoverOpen)
                    sbMessageForGuardian.AppendLine("Printer cover is open.");

                if (m_starPrinterStatus.CutterError)
                    sbMessageForGuardian.AppendLine("Printer cutter is jammed.");

                if (m_starPrinterStatus.HeadThermistorError)
                    sbMessageForGuardian.AppendLine("Printer head thermistor error.");

                if (m_starPrinterStatus.HeadUpError)
                    sbMessageForGuardian.AppendLine("Printer head is up.");

                if (m_starPrinterStatus.MechanicalError)
                    sbMessageForGuardian.AppendLine("Printer has a mechanical error.");

                if (m_starPrinterStatus.Offline)
                    sbMessageForGuardian.AppendLine("Printer is offline.");

                if (m_starPrinterStatus.OverTemp)
                    sbMessageForGuardian.AppendLine("Printer overheated.");

                if (m_starPrinterStatus.PresenterPaperJamError)
                    sbMessageForGuardian.AppendLine("Printer paper jam in presenter unit.");

                if (m_starPrinterStatus.ReceiptPaperEmpty)
                    sbMessageForGuardian.AppendLine("Printer is out of paper.");

                if (!allowLowPaper && (m_starPrinterStatus.ReceiptPaperNearEmptyInner))
                    sbMessageForGuardian.AppendLine("Printer paper low.");

                if (m_starPrinterStatus.UnrecoverableError)
                    sbMessageForGuardian.AppendLine("Printer encountered an unrecoverable error.");

                if (m_starPrinterStatus.VoltageError)
                    sbMessageForGuardian.AppendLine("Printer voltage error.");

                if (sbMessageForGuardian.Length != 0) //we have a problem, tell the Guardian
                    RequestHelpFromGuardian(sbMessageForGuardian.ToString());
            }
        }

        public bool CashDrawerIsOpen(bool showWeAreChecking = false)
        {
            if (!m_weHaveAStarPrinter || WeAreAPOSKiosk || !(m_settings.DrawerCode != null && m_settings.DrawerCode.Length > 0))
                return false;

            if (showWeAreChecking)
            {
                StartGetCashDrawerStatus();
                ShowWaitForm(SellingForm);
            }
            else
            {
                GetStarPrinterStatus(true);
            }

            if (!m_starPrinterStatus.CompulsionSwitch) //close the printer port if the drawer is closed
                OpenStarPrinterPort(false);

            return m_starPrinterStatus.CompulsionSwitch;
        }

        public bool GetStarPrinterStatus(bool leavePortOpen = false)
        {
            leavePortOpen = false;

            bool statusIsGood = true;

            if (!WeHaveAStarPrinter)
            {
                m_starPrinterStatus = new StarPrinterStatus();
                return statusIsGood;
            }

            lock (m_starPrinterStatusLockObject)
            {
                while(m_starPrinterPort == null)
                {
                    System.Threading.Thread.Sleep(100);
                    OpenStarPrinterPort();
                }

                try
                {
                    m_starPrinterStatus = m_starPrinterPort.GetParsedStatus();
                }
                catch (Exception)
                {
                    m_starPrinterStatus = new StarPrinterStatus();
                    statusIsGood = false;
                }

                if(!leavePortOpen)
                    OpenStarPrinterPort(false);

                return statusIsGood;
            }
        }

        // PDTS 966
        /// <summary>
        /// Handles when a server initiated message is received.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A MessageReceivedEventArgs that contains the event 
        /// data.</param>
        internal void ServerMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            // Add the message to the queue to be processed later.
            lock(m_msgSync)
            {
                if(m_pendingMsgs != null)
                    m_pendingMsgs.Enqueue(e);
            }
        }

        void Guardian_GuardianRequestedShutdown(object sender, EventArgs e)
        {
            //if we don't already have a shutdown request pending, add it to the server message queue
            lock (m_msgSync)
            {
                if (m_pendingMsgs != null) //we have a queue to look at
                {
                    try
                    {
                        m_pendingMsgs.First(i => i.CommandId == NewGamingDateMessageId && i.MessageData == null); //will throw an exception if there is no match
                    }
                    catch (Exception)
                    {
                        //add a Guardian shutdown message to the queue and treat it like a server message
                        MessageReceivedEventArgs args = new MessageReceivedEventArgs(NewGamingDateMessageId, null);
                        m_pendingMsgs.Enqueue(args);
                    }
                }
                else
                {
                    m_shutdownWhileInitializing = true;
                    m_suspendedWhileInitializing = false;
                }
            }

            if (m_GuardianIsSuspending) //re-activate to finish work and shut down
                Guardian_GuardianReleasedControl(null, e);
            else //process the server message queue now.
                CheckForMessages();
        }

        void Guardian_GuardianRequestedControl(object sender, EventArgs e)
        {
            lock (m_GuardianRequestLockObject)
            {
                if (!IsInitialized)
                {
                    m_suspendedWhileInitializing = true;
                    Guardian.TellGuardianWeSuspended();
                    return;
                }

                if (!m_GuardianIsSuspending) //we are not suspended by the Guardian so we can suspend
                {
                    m_GuardianIsSuspending = true; //handles idle timers and pauses
                    m_msgTimer.Stop(); //stop the timer for processing server messages

                    if (m_starPrinterStatusTimer != null) //don't suspend for a printer error now
                        m_starPrinterStatusTimer.Stop();

                    //cover the windows on the screen while we move them out of the way
                    Form GuardianBlockingForm = GetFormToCoverScreen();

                    //let the POSForm windows know they need to get out of the way (POSForm windows monitor the Kiosk door open state)
                    m_clearScreenForGuardian = true; //handles POSForm windows

                    //wait for all the POSForm windows to move out of the way
                    while (!SellingForm.AllWindowsOffScreen)
                    {
                        Application.DoEvents();
                        System.Threading.Thread.Sleep(100);
                    }

                    //all the windows are out of the way, get rid of the window covering the screen
                    GuardianBlockingForm.Hide();
                    Application.DoEvents();
                    GuardianBlockingForm.Dispose();
                }

                if (sender != null)
                    Guardian.TellGuardianWeSuspended();
            }
        }

        void Guardian_GuardianReleasedControl(object sender, EventArgs e)
        {
            lock (m_GuardianRequestLockObject)
            {
                if (!IsInitialized)
                {
                    m_suspendedWhileInitializing = false;
                    Guardian.TellGuardianWeResumed();
                    return;
                }

                if (m_GuardianIsSuspending) //we are suspended by the Guardian so we can resume
                {
                    //cover the windows on the screen while we move them back on the screen where they were
                    Form GuardianBlockingForm = GetFormToCoverScreen();

                    //let the POSForm windows know they can move back (POSForm windows monitor the Kiosk door open state)
                    m_clearScreenForGuardian = false; //brings windows back

                    //wait for the windows to move back where they were
                    while (!SellingForm.AllWindowsOnScreen)
                    {
                        Application.DoEvents();
                        System.Threading.Thread.Sleep(100);
                    }

                    if (sender != null)
                        Guardian.TellGuardianWeResumed();

                    //the windows are back, get rid of the window covering the screen
                    GuardianBlockingForm.Hide();
                    Application.DoEvents();
                    GuardianBlockingForm.Dispose();

                    //restart the timer looking for server messages and check the message queue immediately
                    m_GuardianIsSuspending = false; //restarts idle timers and pauses
                    m_lastServerMessageCheck = DateTime.Now;
                    m_msgTimer.Start();

                    if (m_starPrinterStatusTimer != null)
                        m_starPrinterStatusTimer.Start();

                    CheckForMessages();
                }
                else
                {
                    if (sender != null)
                        Guardian.TellGuardianWeResumed();
                }
            }
        }

        void WaitForGuardianConnection()
        {
            Form GuardianBlockingForm = null;

            if (!m_GuardianIsSuspending) //we are not suspended by the Guardian so we need to cover the screen
                GuardianBlockingForm = GetFormToCoverScreen(Resources.GuardianConnectionLost);
            
            while (!m_Guardian.ConnectedToGuardian)
            {
                System.Threading.Thread.Sleep(100);
                Application.DoEvents();
            }

            if (GuardianBlockingForm != null) //we are covering the screen, show the screen again
            {
                GuardianBlockingForm.Hide();
                Application.DoEvents();
                GuardianBlockingForm.Dispose();
            }
        }

        private Form GetFormToCoverScreen(Bitmap pictureToUse = null)
        {
            //make a form that fills the monitor 
            Form coverForm = new Form();
            coverForm.FormBorderStyle = FormBorderStyle.None; //no caption bar
            coverForm.StartPosition = FormStartPosition.Manual;
            coverForm.Location = new Point(m_ourScreen.Bounds.X, m_ourScreen.Bounds.Y);
            coverForm.Size = new Size(m_ourScreen.Bounds.Width, m_ourScreen.Bounds.Height);

            //make a picture box to hold the image and make it fill the screen
            PictureBox pic = new PictureBox();
            pic.Image = pictureToUse == null? Resources.GuardianGears : pictureToUse;
            pic.Size = coverForm.Size;
            coverForm.Controls.Add(pic); //attach the picture box to the form
            pic.SizeMode = PictureBoxSizeMode.StretchImage;
            pic.Show();

            //cover the screen with this window
            coverForm.Enabled = false;
            coverForm.Show();
            coverForm.TopMost = true;
            coverForm.BringToFront();
            Application.DoEvents();

            return coverForm;
        }

        public void RequestHelpFromGuardian(string reason)
        {
            if (Settings.KioskTestNoGuardian)
            {
                Form coverScreen = GetFormToCoverScreen();
                m_GuardianIsSuspending = true;
                MessageBox.Show(coverScreen, reason, "The Pretend Guardian", MessageBoxButtons.OK);
                coverScreen.Hide();
                Application.DoEvents();
                coverScreen.Dispose();
                return;
            }

            while (m_GuardianIsSuspending) //wait until we are not suspended
            {
                System.Threading.Thread.Sleep(100);
                Application.DoEvents();
            }

            bool worked = false;

            do
            {
                if (worked = Guardian.TellGuardianWeSuspended(reason))
                {
                    Guardian_GuardianRequestedControl(null, new EventArgs());
                    
                    while (m_GuardianIsSuspending) //wait until we are not suspended
                    {
                        System.Threading.Thread.Sleep(100);
                        Application.DoEvents();
                    }
                }
                else
                {
                    if (ShowMessage(SellingForm, Settings.DisplayMode, "Could not contact Guardian.\r\n\r\n" + reason, POSMessageFormTypes.RetryAbort) != DialogResult.Retry)
                        worked = true;
                }
            }while(!worked);
        }

        public void PumpMessages()
        {
            Application.DoEvents();
            System.Threading.Thread.Sleep(100);
        }

        /// <summary>
        /// If the message is not currently queued up as pending, adds it to the pending message list
        /// </summary>
        /// <param name="message">the message to add to the queue</param>
        /// <returns>false if a message with the same info is already pending</returns>
        private bool ShouldStartProcessingMessage(ServerMessage message)
        {
            bool addMessage = true; // defaults to true as undefined messages should always be added to the queue to be processed
            lock (pendingMessages) // keep the "any" check and the enqueue atomic
            {
                if (message is FindPlayerByCardMessage) // US4809
                {
                    string magCardNum = (message as FindPlayerByCardMessage).MagCardNumber;

                    addMessage = !pendingMessages.Any(x =>
                    {
                        if (!(x is FindPlayerByCardMessage))
                            return false;
                        else
                            return String.Equals((x as FindPlayerByCardMessage).MagCardNumber, magCardNum, StringComparison.CurrentCultureIgnoreCase);
                    });
                }

                if (addMessage)
                {
                    pendingMessages.Enqueue(message);
                    IsBusy = true;
                }
            }

            return addMessage;
        }

        /// <summary>
        /// Removes the message from the pending queue and updates the IsBusy flag
        /// </summary>
        private void DoneProcessingMessage()
        {
            // Since only one message can be sent at a time, we only have to remove the oldest message, we don't have to search and remove
            ServerMessage temp;
            pendingMessages.TryDequeue(out temp);

            if (pendingMessages.Count == 0)
                IsBusy = false;
        }

        /// <summary>
        /// Disposes of the LoadingForm.
        /// </summary>
        internal void DisposeLoadingForm()
        {
            if(m_loadingForm != null)
            {
                m_loadingForm.CloseForm();
                m_loadingForm.Dispose();
                m_loadingForm = null;
            }
        }

        // PDTS 693
        /// <summary>
        /// Shows the wait form modally.
        /// </summary>
        /// <param name="owner">Any object that implements IWin32Window that 
        /// represents the top-level window that will own the modal dialog 
        /// box.</param>
        /// <param name="immediate">whether or not to show immediately or delay before showing</param>
        internal void ShowWaitForm(IWin32Window owner, bool immediate = false)
        {
            if (m_waitForm != null && !m_waitForm.IsDisposed)
            {
                if (m_sellingForm != null && m_sellingForm.KioskForm != null)
                    owner = (IWin32Window)m_sellingForm.KioskForm;

                if (immediate || m_waitForm.WaitToShow())
                {
                    m_waitForm.ShowDialog(owner);
                    Application.DoEvents();
                }
            }
        }

        /// <summary>
        /// Closes the waitform if it is displaying
        /// </summary>
        internal void CloseWaitForm()
        {
            if (m_waitForm != null && !m_waitForm.IsDisposed)
                m_waitForm.CloseForm();
        }

        // PDTS 693
        /// <summary>
        /// Shows the sale status form modally
        /// </summary>
        /// <param name="owner">Any object that implements IWin32Window that 
        /// represents the top-level window that will own the modal dialog 
        /// box.</param>
        internal void ShowSaleStatusForm(IWin32Window owner)
        {
            if (m_sellingForm.KioskForm != null)
                owner = (IWin32Window)m_sellingForm.KioskForm;

            if (m_statusForm != null)
                m_statusForm.ShowDialog(owner);
        }

        public void WaitForSaleStatusFormSecondaryThread()
        {
            if (m_statusForm != null)
            {
                while (!m_statusForm.SecondaryThreadComplete)
                {
                    Application.DoEvents();
                    System.Threading.Thread.Sleep(10);
                }
            }
        }

        /// <summary>
        /// Prepares the system for shutdown because server 
        /// communications failed.
        /// </summary>
        internal void ServerCommFailed()
        {
            // PDTS 964
            CanUpdateMenus = false;

            // PDTS 966
            if(m_msgTimer != null)
                m_msgTimer.Stop();

            // Display a message saying that the POS is closing.
            ShowMessage(m_sellingForm, new NormalDisplayMode(), Resources.ServerCommFailed + Environment.NewLine + Environment.NewLine + Resources.ShuttingDown, POSMessageFormTypes.Pause, ServerCommShutdownWait);

            Log("Server communications failed.  Shutting down.", LoggerLevel.Severe);
            ClosePOS(this, new EventArgs());
        }

        /// <summary>
        /// Gets the settings from the server.
        /// </summary>
        /// <param name="operatorId">The id of the operator to get settings 
        /// for.</param>
        private void GetSettings(int operatorId)
        {
            m_settings.OperatorID = operatorId;

            GetOperatorCompleteMessage opMsg = new GetOperatorCompleteMessage(operatorId);

            try
            {
                opMsg.Send();
                m_settings.OperatorZipCode = opMsg.OperatorList.Find(o => o.Id == operatorId).Zip;
            }
            catch (Exception e)
            {
                ReformatException(e);
            }

            // Send the message with category = 0  to grab all categories
            GetSettingsMessage settingsMsg = new GetSettingsMessage(m_machineId, operatorId, 0);

            try
            {
                settingsMsg.Send();
            }
            catch (Exception e)
            {
                ReformatException(e);
            }

            // Loop through each setting and parse the value.
            SettingValue[] settings = settingsMsg.Settings;

            foreach(SettingValue setting in settings)
            {
                m_settings.LoadSetting(setting);
            }

            // Rally TA7897
            // Send a third message for license settings.
            GetLicenseFileSettingsMessage licSettingsMsg = new GetLicenseFileSettingsMessage(true);

            try
            {
                licSettingsMsg.Send();
            }
            catch (Exception e)
            {
                ReformatException(e);
            }

            // Loop through each setting and parse the value.
            foreach (LicenseSettingValue setting in licSettingsMsg.LicenseSettings)
            {
                m_settings.LoadSetting(setting);
            }
            // END: TA7897

            GetOperatorInfo(operatorId);

            // Rally US1833 - Warn the user if the license file is going to expire.
            TimeSpan span = licSettingsMsg.ExpirationDate - DateTime.Now;

            if (span.Days <= 0)
                ShowMessage(m_loadingForm, m_settings.DisplayMode, string.Format(CultureInfo.CurrentCulture, Resources.LicenseFileExpired));
            else if (span.Days <= LicenseExpirationLimit)
                ShowMessage(m_loadingForm, m_settings.DisplayMode, string.Format(CultureInfo.CurrentCulture, Resources.LicenseFileExpiring, span.Days));

            if (m_settings.EnableB3Management && (WeAreAB3Kiosk || !(WeAreAPOSKiosk && !m_settings.AllowB3OnKiosk)))
            {
                var b3SettingsMessage = new B3GetSettingsMessage();
                try
                {
                    b3SettingsMessage.Send();
                    m_settings.LoadB3Settings(b3SettingsMessage);
                }
                catch (Exception e) // we can continue on if B3 fails.
                {
                    m_settings.EnableB3Management = false;
                    ShowMessage(m_loadingForm, m_settings.DisplayMode, string.Format(CultureInfo.CurrentCulture, Resources.GetB3SessionListFailed, e.Message));
                }
            }
        }

        // FIX: DE1938
        /// <summary>
        /// Gets the hall settings from the server.
        /// </summary>
        private void GetHallInfo()
        {
            GetHallSettingsMessage hallMsg = new GetHallSettingsMessage();

            try
            {
                hallMsg.Send();
            }
            catch(ServerCommException)
            {
                throw; // Don't repackage the ServerCommException.
            }
            catch(Exception e)
            {
                ReformatException(e);
            }

            m_settings.TaxRate = hallMsg.SalesTax;
        }
        // END: DE1938

        /// <summary>
        /// Returns the operator for the sent in ID
        /// </summary>
        /// <param name="operatorID"></param>
        /// <returns></returns>
        public void GetOperatorInfo(int operatorID)
        {
            GameTech.Elite.Base.Operator op = null;

            GameTech.Elite.Client.GetCompleteOperatorDataMessage getMsg = new GameTech.Elite.Client.GetCompleteOperatorDataMessage(operatorID);
            getMsg.Send();
            if (getMsg.ReturnCode == GameTech.Elite.Client.ServerReturnCode.Success && getMsg.Operators.Count > 0)
            {
                op = getMsg.Operators.First();
            }

            m_settings.LoadOperatorInfo(op);
        }

        /// <summary>
        /// Gets the device hardware attributes from the server.
        /// </summary>
        private void GetDeviceHardwareAttributes()
        {
            GetDeviceHardwareAttribsMessage attribMsg = new GetDeviceHardwareAttribsMessage(0, (int)HardwareAttribute.MaxCards);

            try
            {
                attribMsg.Send();
            }
            catch(ServerCommException)
            {
                throw; // Don't repackage the ServerCommException.
            }
            catch(Exception e)
            {
                ReformatException(e);
            }

            // Set the max card limit for any devices found.
            foreach(DeviceHardwareAttribute attrib in attribMsg.Attributes)
            {
                switch((HardwareAttribute)attrib.HardwareAttributeId)
                {
                    case HardwareAttribute.MaxCards:
                        if(attrib.DeviceId == Device.Traveler.Id)
                            m_settings.TravelerMaxCards = Convert.ToInt16(attrib.DataValue, CultureInfo.InvariantCulture);
                        else if(attrib.DeviceId == Device.Tracker.Id)
                            m_settings.TrackerMaxCards = Convert.ToInt16(attrib.DataValue, CultureInfo.InvariantCulture);
                        else if(attrib.DeviceId == Device.Fixed.Id)
                            m_settings.FixedMaxCards = Convert.ToInt16(attrib.DataValue, CultureInfo.InvariantCulture);
                        else if(attrib.DeviceId == Device.Explorer.Id) // Rally TA7729 - Change Mini to Explorer.
                            m_settings.ExplorerMaxCards = Convert.ToInt16(attrib.DataValue, CultureInfo.InvariantCulture);
                        else if(attrib.DeviceId == Device.Traveler2.Id) // PDTS 964, Rally US765 - WiFi now called II
                            m_settings.Traveler2MaxCards = Convert.ToInt16(attrib.DataValue, CultureInfo.InvariantCulture);

                        break;
                }
            }
        }

        /// <summary>
        /// Gets the available devices to sell from the server.
        /// </summary>     
        private void GetAvailableDevices()
        {
            GetDeviceTypeDataMessage deviceMsg = new GetDeviceTypeDataMessage();

            try
            {
                deviceMsg.Send();
            }
            catch(Exception e)
            {
                ReformatException(e);
            }

            // Loop through all the devices we have and see what we can 
            // sell to.
            foreach(Device device in deviceMsg.Devices)
            {
                if (device.Id == Device.Fixed.Id)
                    m_settings.HasFixed = true;
                else if (device.Id == Device.Explorer.Id) // Rally TA7729
                    m_settings.HasExplorer = true;
                else if (device.Id == Device.Tracker.Id)
                    m_settings.HasTracker = true;
                else if (device.Id == Device.Traveler.Id)
                    m_settings.HasTraveler = true;
                else if (device.Id == Device.Traveler2.Id) // PDTS 964, Rally US765
                    m_settings.HasTraveler2 = true;
                else if (device.Id == Device.Tablet.Id)//US2908 
                    m_settings.HasTablet = true;   
            }
        }

        // TTP 50138
        /// <summary>
        /// Gets the available features to use from the server.
        /// </summary>
        private void GetAvailableFeatures()
        {
            // Rally US505
            // Is Crystal Ball enabled?
            try
            {
                // Rally TA7897
                if(m_settings.CrystalBallEnabled && m_settings.CBBPlayItSheetPrintMode != CBBPlayItSheetPrintMode.Off)
                {
                    m_loadingForm.Message = Resources.LoadingPlayItSheet;

                    // Download the play-it sheets.
                    GetReportMessage reportMsg;

                    // Rally TA8688 - Add support for thermal CBB play-it sheet.
                    // US2150 - Add new vertical line thermal.
                    if(m_settings.CBBPlayItSheetType == CBBPlayItSheetType.Card)
                        reportMsg = new GetReportMessage((int)ReportIDs.CrystalBallPlayItSheetCards);
                    else if(m_settings.CBBPlayItSheetType == CBBPlayItSheetType.Line)
                        reportMsg = new GetReportMessage((int)ReportIDs.CrystalBallPlayItSheetLines);
                    else if(m_settings.CBBPlayItSheetType == CBBPlayItSheetType.CardThermal)
                        reportMsg = new GetReportMessage((int)ReportIDs.CrystalBallPlayItSheetCardsThermal);
                    else if(m_settings.CBBPlayItSheetType == CBBPlayItSheetType.LineThermal)
                        reportMsg = new GetReportMessage((int)ReportIDs.CrystalBallPlayItSheetLinesThermal);
                    else // CBBPlayItSheetType.VerticalLineThermal
                        reportMsg = new GetReportMessage((int)ReportIDs.CrystalBallPlayItSheetVerticleLinesThermal);

                    reportMsg.Send();

                    // Save the report to a temporary file.
                    string path = m_settings.ClientInstallDrive + m_settings.ClientInstallRootDir + @"\Temp";

                    if(!Directory.Exists(path))
                        Directory.CreateDirectory(path);

                    path += PlayItSheetFileName;

                    FileStream fileStream = new FileStream(path, FileMode.Create);
                    BinaryWriter writer = new BinaryWriter(fileStream);

                    writer.Write(reportMsg.ReportFile);
                    writer.Flush();
                    writer.Close();

                    // Clean up.
                    writer = null;
                    fileStream.Dispose();
                    fileStream = null;
                }
            }
            catch(Exception e)
            {
                ReformatException(e);
            }
        }

        /// <summary>
        /// Gets the specified staff from the server.
        /// </summary>
        /// <param name="staffId">The id of the staff to retrieve.</param>
        /// <param name="operatorId">The id of the operator the staff is 
        /// currently logged into.</param>
        /// <returns>A Staff object.</returns>
        private Staff GetStaff(int staffId)
        {
            // Rally TA1583
            GetStaffDataMessage staffDataMsg = new GetStaffDataMessage(staffId);

            try
            {
                staffDataMsg.Send();
            }
            catch(Exception e)
            {
                ReformatException(e);
            }

            // We only care about the first staff if more than one was 
            // returned.
            Staff[] staffList = staffDataMsg.StaffList;

            if(staffList == null || staffList.Length == 0)
                throw new POSException(Resources.StaffNotFound);

            return staffList[0];
        }

        // Rally TA7465
        /// <summary>
        /// Gets the currencies from the server.
        /// </summary>
        /// <returns>true if the exchange rates have been set; otherwise
        /// false.</returns>
        private bool GetCurrencies()
        {
            GetCurrencyDefinitionListMessage currListMsg = new GetCurrencyDefinitionListMessage(null, false);
            GetDailyExchangeRatesMessage exchMsg = new GetDailyExchangeRatesMessage();

            try
            {
                currListMsg.Send();

                m_currencies = new List<Currency>();

                // Rally US1658 - Only apply exchange rates if the user wants to.
                foreach(Currency currency in currListMsg.Currencies)
                {
                    if(currency.IsDefault)
                        m_defaultCurrency = currency;
                    else
                        m_currencies.Add(currency);

                    currency.ExchangeRate = 1M; // Default is 1.
                    currency.Precision = Precision; //set the decimal places used for conversions
                }

                m_currentCurrency = m_defaultCurrency;

                // Get the exchange rates.
                //If m_settings.UseExchangeRateOnSale == false, allow selecting a currency but don't change
                //the amount (leave exchange rate as 1:1). Stupid, but used somewhere.
                m_settings.MultiCurrencies = (m_currencies.Count > 0);

                if(m_settings.MultiCurrencies)
                {
                    exchMsg.Send();

                    if(m_settings.UseExchangeRateOnSale)
                    {
                        foreach(KeyValuePair<string, decimal> rate in exchMsg.Rates)
                        {
                            foreach(Currency currency in m_currencies)
                            {
                                if(currency.ISOCode == rate.Key)
                                {
                                    currency.ExchangeRate = rate.Value;
                                    break;
                                }
                            }
                        }
                    }
                }
                // END: US1658
            }
            catch(ServerCommException)
            {
                throw; // Don't repackage the ServerCommException.
            }
            catch(Exception e)
            {
                ReformatException(e);
            }

            return (!m_settings.MultiCurrencies || exchMsg.AreRatesSet);
        }

        /// <summary>
        /// Gets the card levels from the server.
        /// </summary>
        private void GetCardLevels()
        {
            GetCardLevelDataMessage cardMsg = new GetCardLevelDataMessage();

            try
            {
                cardMsg.Send();
            }
            catch(ServerCommException)
            {
                throw; // Don't repackage the ServerCommException.
            }
            catch(Exception e)
            {
                ReformatException(e);
            }

            m_cardLevels = cardMsg.Levels;
        }

        /// <summary>
        /// Initializes the peripherals for this machine
        /// </summary>
        /// <param name="loadingForm"></param>
        /// <returns></returns>
        private bool InitGuardian(LoadingForm loadingForm)
        {
            loadingForm.Message = "Connecting to Guardian...";
            Application.DoEvents();

            bool success = true;

            try
            {
                Log("Connecting to Guardian", LoggerLevel.Information);
                m_Guardian.GuardianRequestedShutdown += new EventHandler(Guardian_GuardianRequestedShutdown);
                m_Guardian.GuardianRequestedControl += new EventHandler(Guardian_GuardianRequestedControl);
                m_Guardian.GuardianReleasedControl += new EventHandler(Guardian_GuardianReleasedControl);

                m_Guardian.ConnectToGuardian(true); //wait for the Guardian to be connected

                if (!m_Guardian.ConnectedToGuardian)
                    throw new Exception("Connection to Guardian failed.");
            }
            catch (Exception ex)
            {
                success = false;
                Log("Error connecting to Guardian", LoggerLevel.Severe);
                POSMessageForm.Show(m_loadingForm, this, string.Format(CultureInfo.CurrentCulture, Resources.PeripheralLoadError, ex.Message));
            }

            return success;
        }

        /// <summary>
        /// Gets a list of menus from the server.
        /// </summary>
        /// <param name="menuList">The association between menu and session 
        /// will be added to this array.</param>
        private void GetStaffMenus(out POSMenuListItem[] menuList)
        {
            GetStaffMenusMessage menuMsg;
            
            lock(m_settings.SyncRoot)
            {
                menuMsg  = new GetStaffMenusMessage(this, m_settings.DisplayMode, GamingDate);
            }

            try
            {
                menuMsg.Send();

                //set the active selling session
                if (Settings.EnableActiveSalesSession && 
                    menuMsg.Menus != null &&
                    menuMsg.Menus.Length == 1)
                {
                    ActiveSalesSession = menuMsg.Menus[0].Session;
                }

            }
            catch(ServerCommException)
            {
                throw; // Don't repackage the ServerCommException.
            }
            catch(Exception e)
            {
                ReformatException(e);
            }

            if (menuMsg.Menus != null)
                menuList = menuMsg.Menus.ToArray();
            else
                menuList = null;
        }

        /// <summary>
        /// Gets a list of pre-sales menus from the server.
        /// </summary>
        /// <param name="menuList">The association between menu and session 
        /// will be added to this array.</param>
        private void GetStaffPreSalesMenus(out POSMenuListItem[] menuList)
        {
            GetPreSaleStaffMenusMessage preSaleMenuMessage = null;

            menuList = null;

            //if there is no active sale session for the day, then we do not want to allow pre sales
            if (WeAreANonAdvancedPOSKiosk) //Removed a check for active sales sessions
                return;

            lock (m_settings.SyncRoot)
            {
                preSaleMenuMessage = new GetPreSaleStaffMenusMessage(this, m_settings.DisplayMode, GamingDate);
            }

            try
            {
                preSaleMenuMessage.Send();
            }
            catch (ServerCommException)
            {
                throw; // Don't repackage the ServerCommException.
            }
            catch (Exception e)
            {
                ReformatException(e);
            }

            if(preSaleMenuMessage.Menus != null)
                menuList = preSaleMenuMessage.Menus.ToArray();
            else
                menuList = null;
        }

        // FIX: DE1930
        // Rally TA7465

        //US4976
        /// <summary>
        /// Initializes the bank.
        /// </summary>
        /// <returns>
        /// Return Values: -1 = fail; 0 = success; 1 success open a new bank and 2 re-opened previous closed bank
        /// </returns>
        /// <exception cref="POSException"></exception>
        private BankOpenType InitializeBank()
        {
            //return value
            var isNewBank = BankOpenType.Open;

            //US4803 Do not issue a bank if only B3 Session 
            lock (m_menuSync)
            {
                if (HaveMenu && !HaveBingoMenu)
                    return BankOpenType.B3Only;
            }

            //US4549: POS: Re-open a bank
            //only want to reopen a bank if cash method is in staff or machine mode
            //auto issue if enabled and active sales session is enabled
            //DE13633: Removed check for auto issue bank
            if (m_currentOp.CashMethodID != (int)CashMethod.ByStaffMoneyCenter &&
                Settings.EnableActiveSalesSession &&
                m_bank == null)
            {
                //get the closed bank. Return zero if none
                var closeBankId = CheckForClosedBankMessage.GetClosedBankId(CurrentSession.SessionNumber);

                //if closed then prompt to reopen bank
                if (closeBankId != 0)
                {
                    //prompt
                    var results = ShowMessage(m_loadingForm, m_settings.DisplayMode, string.Format(Resources.ReopenBankMessage, CurrentStaff.FirstName, CurrentStaff.LastName, CurrentSession), "Reopen Bank",
                        POSMessageFormTypes.YesNo);

                    // they do not want reopen bank
                    //close POS
                    if (results == DialogResult.No)
                    {
                        m_loadingForm.Close();
                        return BankOpenType.None;
                    }

                    //reopen bank
                    ReopenBankMessage.ReopenBank(closeBankId);
                    isNewBank = BankOpenType.ReOpen;
                }
            }

            // TTP 50137
            // Get the Bank Amount.
            m_bank = GetCurrentBankAmount(); // FIX: DE1930

            if (m_bank == null && m_currentOp.CashMethodID != (int)CashMethod.ByStaffPOS &&
               m_currentOp.CashMethodID != (int)CashMethod.ByMachinePOS)
            {
                ShowMessage(m_loadingForm, m_settings.DisplayMode, Resources.NoBank);
                return BankOpenType.None;
            }

            if (m_bank == null)
            {
                isNewBank = BankOpenType.New; //new bank
                Bank bank = new Bank() { StaffId = m_currentStaff.Id, Type = BankType.Master, GamingDate = GamingDate};


                // Create the currencies to prompt for.
                bank.Currencies.Add(new BankCurrency(m_defaultCurrency));


                foreach (Currency currency in m_currencies)
                {
                    bank.Currencies.Add(new BankCurrency(currency.ISOCode));
                }

                bank.Sort();

                //US4434 only init if not auto issue
                if (!Settings.AutoIssueBank)
                {
                    InitialBankForm bankForm = new InitialBankForm(this, m_settings.DisplayMode.BasicCopy(), bank);
                    bankForm.ShowDialog(m_loadingForm);
                    bankForm.Dispose();
                }

                // Send the amount(s) to the server.
                SetInitialBankAmount(bank);
                
                m_bank = GetCurrentBankAmount();

                if (m_bank == null)
                    throw new POSException(Resources.NoBankFound);
            }

            return isNewBank;
        }

        /// <summary>
        /// Sends the initial bank amount(s) to the server.
        /// </summary>
        /// <param name="bank">The bank to create.</param>
        private void SetInitialBankAmount(Bank bank)
        {
            int issueTrans = 0;
            DateTime issueDate = DateTime.MinValue;
            string bankName;

            lock(m_currentOp.SyncRoot)
            {
                if(m_currentOp.CashMethodID == (int)CashMethod.ByMachinePOS)
                    bankName = string.Format(MachineBankName, m_machineId);
                else
                    bankName = string.Format(StaffBankName, bank.StaffId);
            }

            //session
            short session = 0;

            // set session if active sales session 
            //DE13185: removed auto issue check
            if (Settings.EnableActiveSalesSession)
            {
                session = CurrentSession.SessionNumber;
            }

            BankIssueMessage issueMsg = new BankIssueMessage(0, 0, bank.StaffId, bankName, BankType.Master, session);

            foreach(BankCurrency currency in bank.Currencies)
            {
                issueMsg.AddCurrency(currency);
            }

            try
            {
                issueMsg.Send();
                issueTrans = issueMsg.CashTransactionId;
                issueDate = issueMsg.TransactionDate;
            }
            catch(ServerCommException)
            {
                throw; // Don't repackage the ServerCommException.
            }
            catch(Exception e)
            {
                ReformatException(e);
            }

            try
            {
                Dictionary<BankCurrency, decimal> amounts = new Dictionary<BankCurrency, decimal>();

                foreach(BankCurrency currency in bank.Currencies)
                {
                    amounts.Add(currency, currency.Total);
                }

                //US4434 only print if not auto issue
                if (!Settings.AutoIssueBank)
                {
                    PrintBankReceipt(issueTrans, issueDate, 0, DateTime.MinValue, amounts);
                }
                
            }
            catch(Exception ex)
            {
                ShowMessage(m_loadingForm, m_settings.DisplayMode, string.Format(CultureInfo.CurrentCulture, Resources.InitialBankButNoReceipt, ex.Message));
            }
        }

        // TTP 50137
        /// <summary>
        /// Retrieves the current bank from the server.
        /// </summary>
        /// <returns>The currency bank for this staff/machine.</returns>
        private Bank GetCurrentBankAmount()
        {
            CashMethod cashMethod;
            GetCurrentBankAmountMessage getMsg;
            
            lock(m_currentOp.SyncRoot)
            {
                cashMethod = (CashMethod)m_currentOp.CashMethodID;
            }
            
            if(cashMethod == CashMethod.ByStaffPOS || cashMethod == CashMethod.ByMachinePOS)
                getMsg = new GetCurrentBankAmountMessage(BankType.Master);
            else
                getMsg = new GetCurrentBankAmountMessage(BankType.Regular);

            try
            {
                getMsg.Send();
            }
            catch(ServerCommException)
            {
                throw; // Don't repackage the ServerCommException.
            }
            catch(Exception e)
            {
                ReformatException(e);
            }

            return getMsg.Bank;
        }
        // END: TA7465
        // END: DE1930

        void m_starPrinterStatusTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            CheckStarPrinterStatus(true); //check the printer but allow low paper error
            m_starPrinterStatusTimer.Start();
        }

        /// <summary>
        /// Process any pending server messages now.
        /// </summary>
        public void CheckForMessages()
        {
            if (!IsInitialized)
                return;

            TimeSpan timeSinceLastTimerInducedCheck = DateTime.Now - m_lastServerMessageCheck;

            if (!m_shuttingDown && m_pendingMsgs != null && m_pendingMsgs.Count > 0 && timeSinceLastTimerInducedCheck.Milliseconds < 4500) //more than 1/2 second before next automatic check, do it
            {
                try
                {
                    if (!ShuttingDown && SellingForm != null && !SellingForm.IsDisposed)
                    {
                        if (m_dummyTimerElapsedEventArgs != null)
                        {
                            if (SellingForm.InvokeRequired)
                            {
                                SellingForm.Invoke(new MethodInvoker(delegate()
                                {
                                    CheckForMessages(null, m_dummyTimerElapsedEventArgs);
                                }));
                            }
                            else
                            {
                                CheckForMessages(null, m_dummyTimerElapsedEventArgs);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        /// <summary>
        /// Process any pending server messages.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An ElapsedEventArgs object that contains the 
        /// event data.</param>
        private void CheckForMessages(object sender, System.Timers.ElapsedEventArgs e)
        {
            if(sender != null)
                m_lastServerMessageCheck = DateTime.Now;

            if (!m_shuttingDown && Monitor.TryEnter(m_msgProcessing)) //we got the processing lock so we are going to process messages from the queue
            {
                if (m_dummyTimerElapsedEventArgs == null)
                    m_dummyTimerElapsedEventArgs = e;

                bool done = false;

                while (!done)
                {
                    MessageReceivedEventArgs msgArgs = null;

                    lock (m_msgSync)
                    {
                        if (m_pendingMsgs.Count == 0)
                            done = true; // Nothing to process.
                        else
                            msgArgs = m_pendingMsgs.Peek();
                    }

                    if (!done && msgArgs != null)
                    {
                        // We have a message to process.
                        switch (msgArgs.CommandId)
                        {
                            case RefreshMenusMessageId:
                            {
                                if ((m_sellingForm.KioskForm != null ? m_sellingForm.KioskForm.State == SimpleKioskForm.KioskState.GetPlayerCard || m_sellingForm.KioskForm.State == SimpleKioskForm.KioskState.Closed : true) && !TenderingScreenActive && CanUpdateMenus && (m_worker == null || (m_worker != null && !m_worker.IsBusy)))
                                {
                                    // Remove the message that we peeked.
                                    lock (m_msgSync)
                                    {
                                        m_pendingMsgs.Dequeue();
                                    }

                                    if (!ReloadMenus(msgArgs.MessageData))
                                        done = true;
                                }
                                else
                                {
                                    // We can't update right now so leave it on the queue 
                                    // and try again later.
                                    done = true;
                                }
                            }
                            break;

                            case UpdateButtonsMessageId:
                            {
                                if (CanUpdateMenus && (m_worker == null || (m_worker != null && !m_worker.IsBusy)))
                                {
                                    // Remove the message that we peeked.
                                    lock (m_msgSync)
                                    {
                                        m_pendingMsgs.Dequeue();
                                    }

                                    if (!UpdateMenus(msgArgs.MessageData))
                                    {
                                        done = true;
                                    }
                                    else
                                    {
                                        ClearSale();
                                        m_lastSale = null;
                                    }
                                }
                                else
                                {
                                    // We can't update right now so re-enqueue it 
                                    // and try again later.
                                    done = true;
                                }
                            }
                            break;

                            // Rally US419
                            case PlayTypeSwitchMessageId:
                            {// Remove the message that we peeked.
                                lock (m_msgSync)
                                {
                                    m_pendingMsgs.Dequeue();
                                }

                                try
                                {
                                    BinaryReader reader = new BinaryReader(new MemoryStream((byte[])msgArgs.MessageData));

                                    // Return code
                                    if (reader.ReadInt32() == (int)GTIServerReturnCode.Success)
                                    {
                                        lock (m_settings.SyncRoot)
                                        {
                                            m_settings.PlayType = (BingoPlayType)reader.ReadInt32();

                                            if (m_receiptManager != null)
                                                m_receiptManager.SetPlayType(m_settings.PlayType);
                                        }
                                    }

                                    reader.Close();
                                }
                                catch (Exception ex)
                                {
                                    Log("Failed to update the play type: " + ex.Message, LoggerLevel.Severe);
                                }
                            }
                            break;

                            // Rally US613
                            case NewGamingDateMessageId:
                            {
                                bool guardianShutdownRequest = msgArgs.MessageData == null;
                                int operatorId = 0;
                                BinaryReader newDataReader = null;

                                if (!CanUpdateMenus)
                                    break;

                                if (!guardianShutdownRequest) //request from server
                                {
                                    // FIX: DE3146 - System override for a operator affects multiple operators.
                                    // Is this message for us?
                                    newDataReader = new BinaryReader(new MemoryStream((byte[])msgArgs.MessageData));

                                    // Return Code
                                    newDataReader.ReadInt32();

                                    operatorId = newDataReader.ReadInt32();
                                }

                                if (operatorId == CurrentOperator.Id || operatorId == 0)
                                {
                                    bool busy = false;

                                    if (WeAreAPOSKiosk)
                                    {
                                        if (SellingForm.GiveChangeAsB3Credit)
                                            busy = true;

                                        if (SellingForm.KioskForm != null && SellingForm.KioskForm.State == SimpleKioskForm.KioskState.GetB3Funding)
                                            busy = true;
                                    }

                                    if (!busy && !TenderingScreenActive && m_sellingState == SellingState.NotSelling)
                                    {
                                        done = true;

                                        // Remove the message that we peeked.
                                        lock (m_msgSync)
                                        {
                                            m_pendingMsgs.Dequeue();
                                        }

                                        // PDTS 964
                                        CanUpdateMenus = false;

                                        // TTP 50127
                                        // The gaming date has changed, shutdown.
                                        ClearSale();
                                        m_lastSale = null;

                                        // Display a message saying that the POS is closing.
                                        if (SellingForm.KioskForm != null)
                                            SellingForm.KioskForm.StartIdleState();

                                        if (guardianShutdownRequest || WeAreAPOSKiosk)
                                            ShowMessage(m_sellingForm, m_settings.DisplayMode, Resources.ShuttingDown, POSMessageFormTypes.Pause, 1000);
                                        else
                                            ShowMessage(m_sellingForm, m_settings.DisplayMode, Resources.GamingDateChange + Environment.NewLine + Environment.NewLine + Resources.ShuttingDown, POSMessageFormTypes.Pause, ServerCommShutdownWait);

                                        ClosePOS(this, new EventArgs());
                                    }
                                }

                                if (newDataReader != null)
                                    newDataReader.Close();

                                // END: DE3146
                            }
                            break;

                            case B3SessionChangedCommandId://US4380: (US4337) POS: Display B3 Session Menu
                            {
                                if (!CanUpdateMenus)
                                    break;

                                bool busy = false;

                                if (WeAreAPOSKiosk)
                                {
                                    if (SellingForm.GiveChangeAsB3Credit)
                                        busy = true;

                                    if (SellingForm.KioskForm != null && SellingForm.KioskForm.State == SimpleKioskForm.KioskState.GetB3Funding)
                                        busy = true;
                                }

                                if (!busy && !TenderingScreenActive && m_sellingState == SellingState.NotSelling)
                                {
                                    done = true;

                                    // Remove the message that we peeked.
                                    lock (m_msgSync)
                                    {
                                        m_pendingMsgs.Dequeue();
                                    }

                                    if (!m_settings.EnableB3Management)
                                        break;

                                    // First clear the current sale and last sale, if any.
                                    ClearSale();
                                    m_lastSale = null;

                                    ReloadB3Session();
                                }
                            }
                            break;

                            default:
                            {
                                // Don't know what the message is, so just ignore it.
                                lock (m_msgSync)
                                {
                                    m_pendingMsgs.Dequeue();
                                }
                            }
                            break;
                        }
                    }
                }

                Monitor.Exit(m_msgProcessing); //free the lock
            }
        }

        /// <summary>
        /// Reloads the b3 session.
        /// </summary>
        private void ReloadB3Session()
        {
            bool thisIsOurKioskForm = false;

            if (WeAreANonAdvancedPOSKiosk && SellingForm != null && SellingForm.KioskForm != null)
                SellingForm.KioskForm.StartIdleState();

            RunWorker(Resources.WaitFormUpdatingMenus, DoReloadB3Session, null, ReloadB3SessionComplete);

            ShowWaitForm(m_sellingForm);

            if (LastAsyncException != null)
            {
                // We failed to get the menus, close the POS.
                CanUpdateMenus = false;

                if (m_msgTimer != null)
                    m_msgTimer.Stop();

                ShowMessage(m_sellingForm, m_settings.DisplayMode, LastAsyncException.Message + Environment.NewLine + Environment.NewLine + Resources.ShuttingDown,
                                 POSMessageFormTypes.Pause, ServerCommShutdownWait);
                ClosePOS(this, EventArgs.Empty);
                return;
            }

            if ((WeAreANonB3Kiosk && !HaveMenu) || (WeAreAB3Kiosk && !B3SessionActive)) //lost B3 and have nothing to sell, close (or stay closed)
            {
                if (WeAreAnAdvancedPOSKiosk && m_sellingForm.KioskForm == null)
                {
                    MagCardReader.CardSwiped -= m_sellingForm.CardSwiped;
                    BarcodeScanner.BarcodeScanned -= m_sellingForm.BarcodeScanned;
                    SellingForm.DisableAdvancedKioskCardSwipePic();

                    m_sellingForm.KioskForm = new SimpleKioskForm(this, SellingForm);
                    m_sellingForm.KioskForm.StartIdleState();
                    m_sellingForm.KioskForm.Show(SellingForm);
                    m_sellingForm.KioskForm.Focus();
                    thisIsOurKioskForm = true;
                }

                if (m_sellingForm != null && m_sellingForm.KioskForm != null) //close the kiosk
                {
                    m_sellingForm.KioskForm.CloseKiosk();
                    return;
                }

                ClosePOS(this, EventArgs.Empty);
                return;
            }

            if (WeAreAPOSKiosk && SellingForm != null && SellingForm.KioskForm != null)
            {
                if (WeAreANonAdvancedPOSKiosk)
                {
                    SellingForm.KioskForm.OpenKiosk();
                }
                else
                {
                    if (SellingForm.KioskForm.KioskIsClosed) //open it
                    {
                        if (thisIsOurKioskForm)
                        {
                            SellingForm.KioskForm.Close();
                            SellingForm.KioskForm = null;
                            MagCardReader.CardSwiped += m_sellingForm.CardSwiped;
                            BarcodeScanner.BarcodeScanned += m_sellingForm.BarcodeScanned;
                            SellingForm.DisableAdvancedKioskCardSwipePic(false);
                        }
                        else
                        {
                            SellingForm.KioskForm.Close();
                        }
                    }
                    else
                    {
                        SellingForm.KioskForm.Close();
                    }
                }
            }
        }

        /// <summary>
        /// Does the reload b3 session.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DoWorkEventArgs"/> instance containing the event data.</param>
        private void DoReloadB3Session(object sender, DoWorkEventArgs e)
        {
            SetupThread();

            //US4380: (US4337) POS: Display B3 Menu
            GetB3SessionActive();
        }

        /// <summary>
        /// Reloads the b3 session complete.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RunWorkerCompletedEventArgs"/> instance containing the event data.</param>
        private void ReloadB3SessionComplete(object sender, RunWorkerCompletedEventArgs e)
        {            
            LastAsyncException = e.Error;

            if (e.Error == null)
            {
                //US4380: (US4337) POS: Display B3 Menu
                if (m_settings.EnableB3Management)
                {
                    if ((WeAreAPOSKiosk && B3SessionActive && (WeAreAB3Kiosk || Settings.AllowB3OnKiosk) && m_currentStaff.IsKiosk) ||
                        (WeAreNotAPOSKiosk &&
                            ((B3SessionActive && m_currentStaff.CheckModuleFeature(EliteModule.B3Center, (int)POSFeature.B3Sales)) ||
                             (!B3SessionActive && m_currentStaff.CheckModuleFeature(EliteModule.B3Center, (int)POSFeature.B3Redeem))
                            )
                        )
                       )
                    {
                        AddB3SessionToMenuList();
                    }
                    else
                    {
                        RemoveB3SessionFromMenuList();
                    }
                }

                if (!HaveMenu) // DE13538 There are no menus for sale. Close the POS
                {
                    if(WeAreNotAPOSKiosk)
                        LastAsyncException = new POSException(Resources.NoMenus);
                }
                else
                {
                    m_sellingForm.LoadMenuList(m_menuList, m_currentMenuIndex);
                    m_sellingForm.LoadMenu(CurrentMenu, 1);
                }
            }

            // Close the wait form.
            m_waitForm.CloseForm();
        }

        /// <summary>
        /// Starts the process of reloading the menus.
        /// </summary>
        /// <param name="menuData">The payload of the Refresh Menus 
        /// message.</param>
        /// <returns>true if the reload was successful; otherwise 
        /// false.</returns>
        private bool ReloadMenus(object menuData)
        {
            if (m_sellingForm != null && m_sellingForm.KioskForm != null) //put the kiosk in idle state
            {
                m_sellingForm.KioskForm.StartIdleState();
                Application.DoEvents();
            }

            RunWorker(Resources.WaitFormUpdatingMenus, DoReloadMenus, menuData, ReloadMenusComplete);

            ShowWaitForm(m_sellingForm);
            Application.DoEvents();

            if (LastAsyncException != null)
            {
                if (WeAreAnAdvancedPOSKiosk && m_sellingForm.KioskForm == null)
                {
                    MagCardReader.CardSwiped -= m_sellingForm.CardSwiped;
                    BarcodeScanner.BarcodeScanned -= m_sellingForm.BarcodeScanned;
                    SellingForm.DisableAdvancedKioskCardSwipePic();

                    m_sellingForm.KioskForm = new SimpleKioskForm(this, SellingForm);
                    m_sellingForm.KioskForm.StartIdleState();
                    m_sellingForm.KioskForm.Show(SellingForm);
                    m_sellingForm.KioskForm.Focus();
                }

                if (m_sellingForm != null && m_sellingForm.KioskForm != null) //close the kiosk
                {
                    m_sellingForm.KioskForm.CloseKiosk();
                    return true;
                }
                else //We failed to get the menus, close the POS.
                {
                    CanUpdateMenus = false;

                    if (m_msgTimer != null)
                        m_msgTimer.Stop();

                    ShowMessage(m_sellingForm, m_settings.DisplayMode, LastAsyncException.Message + Environment.NewLine + Environment.NewLine + Resources.ShuttingDown,
                                     POSMessageFormTypes.Pause, ServerCommShutdownWait);

                    ClosePOS(this, EventArgs.Empty);
                }

                return false;
            }
            else
            {
                if (m_sellingForm != null && m_sellingForm.KioskForm != null) //re-do the kiosk menu and wake it up
                {
                    m_sellingForm.KioskForm.StartIdleState();
                    Application.DoEvents();
                    Thread.Sleep(1000);
                    ClosePOS(this, EventArgs.Empty); //m_sellingForm.KioskForm.OpenKiosk();
                }
                else if (WeAreAnAdvancedPOSKiosk)
                {
                    ClosePOS(this, EventArgs.Empty);
                }

                return true;
            }
        }

        /// <summary>
        /// Gets the current menus from the server.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The DoWorkEventArgs object that 
        /// contains the event data.</param>
        private void DoReloadMenus(object sender, DoWorkEventArgs e)
        {
            SetupThread();

            Application.DoEvents();
            Thread.Sleep(1000);

            // Unbox the argument.
            byte[] menuData = (byte[])e.Argument;

            if(menuData == null || menuData.Length == 0)
                throw new POSException(Resources.NoMenus);

            // Create the streams we will be reading from.
            MemoryStream responseStream = new MemoryStream(menuData);
            BinaryReader responseReader = new BinaryReader(responseStream, Encoding.Unicode);

            // Check to make sure the return code is okay.
            GTIServerReturnCode returnCode = (GTIServerReturnCode)responseReader.ReadInt32();

            if(returnCode != GTIServerReturnCode.Success)
                throw new POSException(string.Format(CultureInfo.CurrentCulture, Resources.RefreshMenusFailed, ServerExceptionTranslator.GetServerErrorMessage(returnCode)));

            // FIX: DE3229 - User is able to get a menu from a different operator to appear.
            // Check to see if these menus are for us.
            int currentOpId;

            lock(CurrentOperator.SyncRoot)
            {
                currentOpId = CurrentOperator.Id;
            }

            int opId = responseReader.ReadInt32();

            if (opId == currentOpId)
            {
                DisplayMode displayMode;

                lock (m_settings.SyncRoot)
                {
                    displayMode = m_settings.DisplayMode;
                }

                var newMenuList = new List<POSMenuListItem>();
                // Parse the menu data.
                POSMenuListItem[] newMenuArray = MenuParser.Parse(this, displayMode, responseReader, GamingDate);

                if (newMenuArray != null)
                {
                    newMenuList.AddRange(newMenuArray);

                    //set the active selling session
                    if (Settings.EnableActiveSalesSession && newMenuList.Count == 1)
                    {
                        ActiveSalesSession = newMenuList[0].Session;
                    }
                }

                //only add presale menus if there is an active menu for the day
                if (newMenuList.Count > 0)
                {
                    //Get Presale Menus
                    var preSaleMenuMessage = new GetPreSaleStaffMenusMessage(this, m_settings.DisplayMode, GamingDate);
                    preSaleMenuMessage.Send();
                    newMenuList.AddRange(preSaleMenuMessage.Menus);
                }

                // Lock and update the menus now that we have built them.
                // Rally TA8947 - Don't change menus on refresh (if possible).
                POSMenuListItem currentMenuItem = null;

                if (newMenuList.Count != 0) // DE13543 changed not to throw exception. Need to check other menus before determining whether or not to close POS.
                {
                    lock (m_menuSync)
                    {
                        // Save the current menu item.
                        int menuLength = m_menuList != null ? m_menuList.Length : 0;

                        if (m_currentMenuIndex < menuLength)
                        {
                            if (m_menuList != null)
                            {
                                currentMenuItem = m_menuList[m_currentMenuIndex];
                            }
                        }

                        m_menuList = newMenuList.ToArray();
                        Array.Sort(m_menuList); // FIX: DE6117 - Menus not sorted on refresh.
                    }
                }
                else
                {
                    m_menuList = new POSMenuListItem[0];
                }

                e.Result = currentMenuItem;
            }
            else
            {
                e.Result = null;
            }

            // END: TA8947
            // END: DE3229

            // Clean up.
            responseStream.Dispose();
        }

        /// <summary>
        /// Handles the event when the reload menus/buttons BackgroundWorker 
        /// is complete.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The RunWorkerCompletedEventArgs object that 
        /// contains the event data.</param>
        private void ReloadMenusComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            // Set the error that occurred (if any).
            LastAsyncException = e.Error;

            if(e.Error == null)
            {
                // We can update the UI with the new menus/buttons.
                POSMenuListItem previousMenuItem = e.Result as POSMenuListItem;
                m_currentMenuIndex = 0;

                if(previousMenuItem != null)
                {
                    //US4126: POS: Active sales session is set the POS 
                    //refreshes when the session is closed at the caller.
                    //update current sales
                    if (m_currentSale != null)
                    {
                        //get a list of sessions
                        var sessions = CurrentSale.GetSessions();

                        //remove any session in the menu
                        foreach (var posMenuListItem in m_menuList)
                        {
                            if (sessions.Contains(posMenuListItem.Session))
                            {
                                sessions.Remove(posMenuListItem.Session);
                            }
                        }

                        //remove any sale items by session that are not in the menu list
                        foreach (var session in sessions)
                        {
                            RemoveSalesItemsBySession(session);    
                        }
                            
                    }

                    // Attempt to find the previously selected menu in the new list.
                    for(int x = 0; x < m_menuList.Length; x++)
                    {
                        if(m_menuList[x].Equals(previousMenuItem))
                        {
                            m_currentMenuIndex = x;
                            break;
                        }
                    }
                }

                //US5192/DE13378
                if (m_settings.EnableB3Management)
                {
                    if ((WeAreAPOSKiosk && B3SessionActive && (WeAreAB3Kiosk || Settings.AllowB3OnKiosk) && m_currentStaff.IsKiosk) ||
                        (WeAreNotAPOSKiosk &&
                            ((B3SessionActive && m_currentStaff.CheckModuleFeature(EliteModule.B3Center, (int)POSFeature.B3Sales)) ||
                             (!B3SessionActive && m_currentStaff.CheckModuleFeature(EliteModule.B3Center, (int)POSFeature.B3Redeem))
                            )
                        )
                       )
                    {
                        AddB3SessionToMenuList();
                    }
                    else
                    {
                        RemoveB3SessionFromMenuList();
                    }
                }

                if (m_menuList == null || m_menuList.Length == 0)// DE13543 only close POS if there are no menus for sale
                {
                    LastAsyncException = new POSException(Resources.NoMenus);
                }
                else
                {
                    m_sellingForm.LoadMenuList(m_menuList, m_currentMenuIndex);
                    m_sellingForm.LoadMenu(CurrentMenu, 1);
                }
            }

            // Close the wait form.
            m_waitForm.CloseForm();
        }

        /// <summary>
        /// Sets the current menu based on the specified index into 
        /// the menu list.
        /// </summary>
        /// <param name="menuListIndex">The index of the menu list to 
        /// evaluate.</param>
        /// <returns>true if the menu was changed; otherwise false.</returns>
        /// <exception cref="GTI.Modules.POS.Business.POSException">menuListIndex 
        /// specified a menu that doesn't exist.</exception>
        internal bool SetCurrentMenu(int menuListIndex)
        {
            lock(m_menuSync)
            {
                int oldMenu = m_currentMenuIndex;

                if(menuListIndex >= 0 && menuListIndex < m_menuList.Length)
                {
                    m_currentMenuIndex = menuListIndex;
                    return (m_currentMenuIndex != oldMenu);
                }
                else
                    throw new POSException(Resources.NoMenu);
            }
        }

        /// <summary>
        /// Set the current session to the passed session number. Changes menu to correct menu.
        /// </summary>
        /// <param name="session">Session number to change to.</param>
        /// <returns>False if session could not be found.</returns>
        public int GetMenuIndexForSession(int sessionNumber, DateTime gamingDate)
        {
            lock (m_menuSync)
            {
                for (int x = 0; x < m_menuList.Length; x++)
                {
                    if (m_menuList[x].Session.ProgramName != Resources.B3SessionString &&
                        m_menuList[x].Session.SessionNumber == sessionNumber &&
                        m_menuList[x].Session.GamingDate == gamingDate)
                        return x;
                }
            }

            return -1;
        }

        /// <summary>
        /// Starts the process of update the menus.
        /// </summary>
        /// <param name="buttonData">The payload of the Update Menus 
        /// message.</param>
        /// <returns>true if the update was successful; otherwise 
        /// false.</returns>
        private bool UpdateMenus(object data)
        {
            bool returnVal = true;

            try
            {
                byte[] buttonData = (byte[])data;

                // Create the streams we will be reading from.
                MemoryStream responseStream = new MemoryStream(buttonData);
                BinaryReader responseReader = new BinaryReader(responseStream, Encoding.Unicode);

                // Check to make sure the return code is okay.
                GTIServerReturnCode returnCode = (GTIServerReturnCode)responseReader.ReadInt32();

                if(returnCode == GTIServerReturnCode.Success)
                {
                    // Count of buttons.
                    ushort buttonCount = responseReader.ReadUInt16();

                    for(ushort x = 0; x < buttonCount; x++)
                    {
                        // POS Menu Id
                        int menuId = responseReader.ReadInt32();

                        // Page Number
                        byte page = (byte)responseReader.ReadUInt16();

                        // Key Number
                        byte key = (byte)responseReader.ReadUInt16();

                        // Is Locked
                        bool isLocked = responseReader.ReadBoolean();

                        // Change the placement, if needed.
                        MenuParser.RearrangeButton(m_settings.DisplayMode, ref page, ref key);

                        // Find the button.
                        foreach(POSMenuListItem item in m_menuList)
                        {
                            if(item.Menu.Id == menuId)
                            {
                                MenuButton[] buttons = item.Menu.GetPage(page);

                                if(buttons != null && buttons.Length > key)
                                {
                                    buttons[key].IsLocked = isLocked;
                                }

                                break;
                            }
                        }
                    }
                }
                else
                    returnVal = false;

                // Clean up.
                responseStream.Dispose();
            }
            catch(Exception e)
            {
                returnVal = false;
                Log("Failed to update the menu buttons: " + e.Message, LoggerLevel.Severe);
                ShowMessage(m_sellingForm, m_settings.DisplayMode, string.Format(CultureInfo.CurrentCulture, Resources.UpdateMenusFailed, e.Message));
            }

            return returnVal;
        }

        /// <summary>
        /// Forces the current thread's culture to U.S. English.
        /// </summary>
        internal static void ForceEnglish()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;
        }

        /// <summary>
        /// Sets the message on the wait form, creates a new BackgroundWorker 
        ///   and initializes it with the passed in values, and finally runs the 
        ///   worker.
        /// </summary>
        /// <param name="waitFormMessage">
        /// The message to be displayed on the wait form (if needed).
        /// </param>
        /// <param name="doWorkHandler">
        /// A DoWorkEventHandler delegate for the worker method.
        /// </param>
        /// <param name="argument">
        /// An optional argument passed to the worker method.
        /// </param>
        /// <param name="completeHandler">
        /// A RunWorkerCompletedEventHandler delegate for the completed method.
        /// </param>
        internal void RunWorker(string waitFormMessage, DoWorkEventHandler doWorkHandler, object argument, RunWorkerCompletedEventHandler completeHandler)
        {
            // Set the wait message.
            if(m_waitForm != null && !m_waitForm.IsDisposed)
                m_waitForm.Message = waitFormMessage;

            // Create the worker thread and run it.
            m_worker = new BackgroundWorker();
            m_worker.WorkerReportsProgress = true;
            m_worker.WorkerSupportsCancellation = false;
            m_worker.DoWork += doWorkHandler;
            m_worker.ProgressChanged += new ProgressChangedEventHandler(m_waitForm.ReportProgress);
            m_worker.RunWorkerCompleted += completeHandler;
            
            if(argument != null)
                m_worker.RunWorkerAsync(argument);
            else
                m_worker.RunWorkerAsync();
        }

        /// <summary>
        /// Sets the thread's language options and pauses to allow the wait 
        /// form to display.
        /// </summary>
        internal void SetupThread()
        {
            // Set the language.
            lock(m_settings.SyncRoot)
            {
                if(m_settings.ForceEnglish)
                    ForceEnglish();
                else
                    Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;
            }

            // Wait a couple of ticks to let the wait form display.
            System.Windows.Forms.Application.DoEvents();
        }

        /// <summary>
        /// Creates a thread to get a player's data on the server.
        /// </summary>
        /// <param name="magCardNumber">The mag. card number of the player to 
        /// look for.</param>
        internal void StartGetPlayer(string magCardNumber)
        {
            StartGetPlayer(magCardNumber, 0);
        }

        /// <summary>
        /// Creates a thread to get a player's data on the server.
        /// </summary>
        /// <param name="magCardNumber">The mag. card number of the player to 
        /// look for.</param>
        /// <param name="PIN">The PIN for the player card.</param>
        internal void StartGetPlayer(string magCardNumber, int PIN)
        {
            PlayerLookupInfo playerInfo = new PlayerLookupInfo();

            playerInfo.CardNumber = magCardNumber;
            playerInfo.PIN = PIN;

            // TTP 50114
            RunWorker(m_settings.EnableAnonymousMachineAccounts ? Resources.WaitFormGettingMachine : Resources.WaitFormGettingPlayer,
                      new DoWorkEventHandler(SendGetPlayer), (object)playerInfo, new RunWorkerCompletedEventHandler(GetPlayerComplete));
        }

        /// <summary>
        /// Creates a thread to get a player's data on the server.
        /// </summary>
        /// <param name="playerId">The id of the player to look for.</param>
        internal void StartGetPlayer(int playerId)
        {
            StartGetPlayer(playerId, 0);
        }

        /// <summary>
        /// Creates a thread to get a player's data on the server.
        /// </summary>
        /// <param name="playerId">The id of the player to look for.</param>
        /// <param name="PIN">The PIN for the player card.</param>
        internal void StartGetPlayer(int playerId, int PIN)
        {
            PlayerLookupInfo playerInfo = new PlayerLookupInfo();

            playerInfo.playerID = playerId;
            playerInfo.PIN = PIN;

            RunWorker(m_settings.EnableAnonymousMachineAccounts ? Resources.WaitFormGettingMachine : Resources.WaitFormGettingPlayer,
                      new DoWorkEventHandler(SendGetPlayer), (object)playerInfo, new RunWorkerCompletedEventHandler(GetPlayerComplete));
        }

        /// <summary>
        /// Creates a thread to get the current player's data from the server and third party system and 
        /// update the points.
        /// </summary>
        /// <param name="PIN">The PIN for the current player's card.</param>
        internal void StartUpdatePlayerPoints(int PIN)
        {
            if (m_currentSale == null || m_currentSale.Player == null || m_currentSale.Player.Id == 0) //nothing to update
                return;

            PlayerLookupInfo playerInfo = new PlayerLookupInfo();

            playerInfo.playerID = m_currentSale.Player.Id;
            playerInfo.PIN = PIN;
            playerInfo.UpdateCurrentPlayer = true;

            RunWorker(m_settings.EnableAnonymousMachineAccounts ? Resources.WaitFormGettingMachine : Resources.WaitFormGettingPlayer,
                      new DoWorkEventHandler(SendGetPlayer), (object)playerInfo, new RunWorkerCompletedEventHandler(GetPlayerComplete));
        }

        /// <summary>
        /// Gets a player's data from the server.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The DoWorkEventArgs object that 
        /// contains the event data.</param>
        private void SendGetPlayer(object sender, DoWorkEventArgs e)
        {
            SetupThread();

            // FIX: DE2580 - A player swipe should enter the player in the
            // raffle and give them points.
            bool enableMachineAccounts, promptForCreate, enterRaffle;

            // Set the options.
            lock(m_settings.SyncRoot)
            {
                // TTP 50114
                enableMachineAccounts = m_settings.EnableAnonymousMachineAccounts;
                promptForCreate = m_settings.PromptForPlayerCreation; // PDTS 1044
                enterRaffle = m_settings.SwipeEntersRaffle;
            }

            // Unbox the argument.
            PlayerLookupInfo sentPlayer = (PlayerLookupInfo)e.Argument;
            int playerId = sentPlayer.playerID;
            string magCardNum = sentPlayer.CardNumber;
            int PIN = sentPlayer.PIN;
            bool updatePlayer = sentPlayer.UpdateCurrentPlayer;
            bool justSynced = false;

            if (!m_settings.ThirdPartyPlayerInterfaceUsesPIN)
                PIN = 0;

            // Are we getting the player by id or mag. card?
            if (playerId == 0)
            {
                FindPlayerByCardMessage cardMsg = new FindPlayerByCardMessage();
                cardMsg.MagCardNumber = magCardNum;
                cardMsg.PIN = PIN;
                cardMsg.SyncPlayerWithThirdParty = m_settings.ThirdPartyPlayerSyncMode == 0;

                if (!ShouldStartProcessingMessage(cardMsg))
                {
                    Log("FindPlayerByCardMessage with same card already being processed, ignored extra call", LoggerLevel.Message);
                    return; // message is already pending, don't bother trying to send it again
                }

                // Send the message.
                try
                {
                    cardMsg.Send();
                }
                catch (ServerCommException)
                {
                    throw; // Don't repackage the ServerCommException
                }
                catch (Exception ex)
                {
                    // TTP 50114
                    throw new POSException(string.Format(CultureInfo.CurrentCulture, enableMachineAccounts ? Resources.GetMachineFailed : Resources.GetPlayerFailed, ServerExceptionTranslator.FormatExceptionMessage(ex)), ex);
                }

                // Set the id that we got back from the server.
                if (cardMsg.PlayerId == 0)
                {
                    // PDTS 1044
                    // Can we create the account?
                    bool noSyncWithThirdPartySoAddPlayer = Settings.ThirdPartyPlayerInterfaceID != 0 && (!cardMsg.SyncPlayerWithThirdParty || cardMsg.ThirdPartyInterfaceDown);

                    if (WeAreNotAPOSKiosk && !enableMachineAccounts && IsPlayerCenterInitialized && !string.IsNullOrEmpty(magCardNum) && ((promptForCreate && Settings.ThirdPartyPlayerInterfaceID == 0) || noSyncWithThirdPartySoAddPlayer))
                    {
                        bool doCreate = false;

                        if (noSyncWithThirdPartySoAddPlayer)
                        {
                            doCreate = true;
                        }
                        else
                        {
                            if (m_waitForm != null && !m_waitForm.IsDisposed && m_waitForm.InvokeRequired) // if we're using the wait form
                            {
                                CreatePlayerPromptDelegate prompt = new CreatePlayerPromptDelegate(PromptToCreatePlayer);
                                doCreate = ((DialogResult)m_waitForm.Invoke(prompt, new object[] { m_waitForm }) == DialogResult.Yes);
                            }
                            else if (m_sellingForm != null && m_sellingForm.InvokeRequired) // if there's no wait form, but still requires the UI thread
                            {
                                CreatePlayerPromptDelegate prompt = new CreatePlayerPromptDelegate(PromptToCreatePlayer);
                                doCreate = ((DialogResult)m_sellingForm.Invoke(prompt, new object[] { m_sellingForm }) == DialogResult.Yes);
                            }
                            else // Just try it? Hopefully it doesn't get here if the UI thread is required
                            {
                                doCreate = (PromptToCreatePlayer(m_waitForm) == DialogResult.Yes);
                            }
                        }

                        if (doCreate)
                            playerId = m_playerCenter.CreatePlayerForPOS(magCardNum);
                        else
                            throw new POSUserCancelException(Resources.NoPlayersFound);
                    }
                    else
                    {
                        throw new POSException(enableMachineAccounts ? Resources.NoMachineFound : Resources.NoPlayersFound);
                    }
                }
                else
                {
                    playerId = cardMsg.PlayerId;
                   
                    if (cardMsg.SyncPlayerWithThirdParty && cardMsg.PointsUpToDate)
                        justSynced = true;
                }
            }

            Player player = null;
            int opId;

            lock(m_currentOp.SyncRoot)
            {
                opId = m_currentOp.Id;
            }

            if(!enableMachineAccounts)
            {
                PlayerCardSwipeMessage swipeMsg = new PlayerCardSwipeMessage(playerId, null, enterRaffle, PIN);

                try
                {
                    swipeMsg.Send();
                }
                catch(ServerCommException)
                {
                    throw; // Don't repackage the ServerCommException
                }
                catch(Exception ex)
                {
                    throw new POSException(string.Format(CultureInfo.CurrentCulture, Resources.CardSwipeFailed, ServerExceptionTranslator.FormatExceptionMessage(ex)), ex);
                }
            }
            // END: DE2580

            try
            {
                bool syncPlayer = !justSynced && (m_settings.ThirdPartyPlayerSyncMode == 0 || updatePlayer); //realtime or need points

                player = new Player(playerId, opId, PIN, syncPlayer, justSynced);

                if(!updatePlayer)
                    LoadScheduledSales(player);
            }
            catch(ServerCommException)
            {
                throw; // Don't repackage the ServerCommException
            }
            catch(ServerException exc)
            {
                // TTP 50114
                throw new POSException(string.Format(CultureInfo.CurrentCulture, enableMachineAccounts ? Resources.GetMachineFailed : Resources.GetPlayerFailed, ServerExceptionTranslator.FormatExceptionMessage(exc)) + " " + string.Format(CultureInfo.CurrentCulture, Resources.MessageName, exc.Message), exc);
            }
            catch(Exception exc)
            {
                // TTP 50114
                throw new POSException(string.Format(CultureInfo.CurrentCulture, enableMachineAccounts ? Resources.GetMachineFailed : Resources.GetPlayerFailed, ServerExceptionTranslator.FormatExceptionMessage(exc)), exc);
            }

            //US4320
            try
            {
                player.DiscountUsageDictionary = GetDiscountUsageBySessionMessage.GetDiscountUsageBySession(playerId, CurrentSessionPlayedId);
            }
            catch (Exception ex)
            {
                throw new POSException(string.Format(CultureInfo.CurrentCulture, Resources.GetPlayerDiscountUsageFailed, ServerExceptionTranslator.FormatExceptionMessage(ex)), ex);
            }

            //if we are a Kiosk and there is an alert for this player, tell the player to see the cashier.
            if (WeAreAPOSKiosk && player != null && player.ActiveStatusList.Exists(s => s.IsAlert))
                throw new POSException(Resources.PleaseSeeCashier);

            e.Result = new Tuple<Player, bool, bool>(player, updatePlayer, sentPlayer.WaitFormDisplayed);
        }

        // PDTS 1044
        /// <summary>
        /// A delegate that allows cross-thread calls to PromptToCreatePlayer 
        /// on the PointOfSale class.
        /// </summary>
        /// <param name="owner">Any object that implements IWin32Window 
        /// that represents the top-level window that will own any modal 
        /// dialog boxes.</param>
        /// <returns>The DialogResult of the MessageForm (Yes or No).</returns>
        private delegate DialogResult CreatePlayerPromptDelegate(IWin32Window owner);

        /// <summary>
        /// Displays a message box asking if the user would like to create a 
        /// new player account.
        /// </summary>
        /// <param name="owner">Any object that implements IWin32Window 
        /// that represents the top-level window that will own any modal 
        /// dialog boxes.</param>
        /// <returns>The DialogResult of the MessageForm (Yes or No).</returns>
        private DialogResult PromptToCreatePlayer(IWin32Window owner)
        {
            DisplayMode displayMode;

            lock(m_settings.SyncRoot)
            {
                displayMode = m_settings.DisplayMode;
            }

            return POSMessageForm.Show(owner, this, Resources.NoPlayersFound + Environment.NewLine + Resources.CreatePlayer, POSMessageFormTypes.YesNo_DefNO);
        }

        public bool LoadScheduledSales(Player player)
        {
            //get scheduled sale info
            GetScheduledSalesMessage schdSaleMsg = new GetScheduledSalesMessage();

            schdSaleMsg.PlayerId = player.Id;
            schdSaleMsg.MagCardNumber = player.MagneticCardNumber;

            if (Settings.PrintPlayerIdentityAsAccount)
                schdSaleMsg.AccountNumber = player.PlayerIdentity;

            if (Settings.EnableActiveSalesSession)
                schdSaleMsg.SessionPlayedID = CurrentSessionPlayedId;
            else
                schdSaleMsg.SessionPlayedID = 0;

            schdSaleMsg.Send();

            if (schdSaleMsg.SaleList.Count > 0)
                player.ScheduledSalesObject = schdSaleMsg.SaleList;
            else
                player.ScheduledSalesObject = null;

            return schdSaleMsg.SaleList.Count > 0;
        }

        /// <summary>
        /// Handles the event when the get player data BackgroundWorker 
        /// is complete.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The RunWorkerCompletedEventArgs object that 
        /// contains the event data.</param>
        private void GetPlayerComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error == null && e.Result == null) // the message didn't run
            {
//                if (m_waitForm != null && !m_waitForm.IsDisposed && m_waitForm.WaitDialogIsActive)
//                    m_waitForm.CloseForm();

                return;
            }

            try
            {
                // Set the error that occurred (if any).
                LastAsyncException = e.Error;
                Player player = null;
                bool samePlayerID = false;
                
                if (e.Error == null)
                {
                    Tuple<Player, bool, bool> result = (Tuple<Player, bool, bool>)e.Result;
                    player = result.Item1;
                    bool updatePoints = result.Item2;

                    // If there is no sale, then start it.
                    if (m_currentSale == null)
                        StartSale(false);

                    if (m_currentSale.Player != null && m_currentSale.Player.Id == player.Id)
                        samePlayerID = true;

                    if (updatePoints)
                    {
                        if (m_currentSale.Player != null) //we have one, update it
                        {
                            m_currentSale.Player.PlayerCardPINError = player.PlayerCardPINError;
                            m_currentSale.Player.PointsBalance = player.PointsBalance;
                            m_currentSale.Player.PointsUpToDate = player.PointsUpToDate;
                            m_currentSale.Player.ThirdPartyInterfaceDown = player.ThirdPartyInterfaceDown;
                        }
                    }
                    else
                    {
                        // Set the player we retrieved to the current player.
                        try
                        {
                            m_currentSale.SetPlayer(player, true, true);
                        }
                        catch (POSException ex)
                        {
                            // TTP 50114
                            ShowMessage(m_sellingForm, m_settings.DisplayMode, string.Format(CultureInfo.CurrentCulture,
                                                m_settings.EnableAnonymousMachineAccounts ? Resources.MachineSetFailed : Resources.PlayerSetFailed, ex.Message));
                        }
                    }

                    if (player.ErrorMessage != string.Empty) //we have an error message
                    {
                        if (!player.PlayerCardPINError && !(player.ThirdPartyInterfaceDown && updatePoints))
                            ShowMessage(m_sellingForm, m_settings.DisplayMode, string.Format(CultureInfo.CurrentCulture,
                                         Resources.MessageName, player.ErrorMessage));
                    }
                }

                // US4809 ***
                EventHandler<GetPlayerEventArgs> handler = GetPlayerCompleted;
                if (handler != null)
                    handler(this, new GetPlayerEventArgs(player, LastAsyncException, samePlayerID));
            }
            catch (Exception ex)
            {
                Log("Error finishing player lookup " + ex.ToString(), LoggerLevel.Severe);
            }
            finally
            {
                // Close the wait form.
                if (m_waitForm != null && !m_waitForm.IsDisposed)
                    m_waitForm.CloseForm();

                DoneProcessingMessage(); // notify that we're done processing the message.
            }
        }

        /// <summary>
        /// Get the repeat sale information on a thread and block with a waiting message.
        /// </summary>
        /// <param name="msg">Message to send to the server.</param>
        public void StartGetRepeatSaleInfo(GetRepeatSaleInfoMessage msg)
        {
            RunWorker(Resources.WaitFormGettingSaleInfo, new DoWorkEventHandler(SendGetRepeatSaleInfo), (object)msg, new RunWorkerCompletedEventHandler(GetRepeatSaleInfoComplete));
        }

        /// <summary>
        /// Send repeat sale info message to the server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">GetRepeatSaleInfoMessage</param>
        private void SendGetRepeatSaleInfo(object sender, DoWorkEventArgs e)
        {
            SetupThread();

            GetRepeatSaleInfoMessage msg = (GetRepeatSaleInfoMessage)e.Argument;

            try
            {
                msg.Send();
            }
            catch (Exception)
            {
                throw; // Don't repackage
            }

            e.Result = msg;
        }

        /// <summary>
        /// GetRepeatSaleInfoMessage completed.  Unblock processing.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GetRepeatSaleInfoComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            // Set the error that occurred (if any).
            LastAsyncException = e.Error;

            // Close the wait form.
            m_waitForm.CloseForm();
        }

        /// <summary>
        /// Get the schduled sale information on a thread and block with a waiting message.
        /// </summary>
        /// <param name="msg">Message to send to the server.</param>
        public void StartGetSchduledSaleInfo(GetScheduledSalesMessage msg)
        {
            RunWorker(Resources.WaitFormGettingSaleInfo, new DoWorkEventHandler(SendGetScheduledSalesInfo), (object)msg, new RunWorkerCompletedEventHandler(GetScheduledSalesInfoComplete));
        }

        /// <summary>
        /// Send schduled sale info message to the server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">GetScheduledSalesMessage</param>
        private void SendGetScheduledSalesInfo(object sender, DoWorkEventArgs e)
        {
            SetupThread();

            GetScheduledSalesMessage msg = (GetScheduledSalesMessage)e.Argument;

            try
            {
                msg.Send();
            }
            catch (Exception)
            {
                throw; // Don't repackage
            }

            e.Result = msg;
        }

        /// <summary>
        /// GetScheduledSalesMessage completed.  Unblock processing.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GetScheduledSalesInfoComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            // Set the error that occurred (if any).
            LastAsyncException = e.Error;

            // Close the wait form.
            m_waitForm.CloseForm();
        }

        /// <summary>
        /// Creates a thread to get a player card PIN from the server.
        /// </summary>
        /// <param name="playerId">The id of the player to work with.</param>
        internal void StartGetPlayerCardPIN(int playerId)
        {
            PlayerLookupInfo playerInfo = new PlayerLookupInfo();

            playerInfo.playerID = playerId;
            playerInfo.PIN = -1;

            RunWorker(Resources.WaitFormGettingPlayer, new DoWorkEventHandler(SendGetSetPlayerCardPIN), (object)playerInfo, new RunWorkerCompletedEventHandler(GetSetPlayerCardPINComplete));
        }

        /// <summary>
        /// Creates a thread to set a player card PIN on the server.
        /// </summary>
        /// <param name="playerId">The id of the player to work with.</param>
        /// <param name="PIN">The PIN for the player card.</param>
        internal void StartSetPlayerCardPIN(int playerId, int PIN)
        {
            PlayerLookupInfo playerInfo = new PlayerLookupInfo();

            playerInfo.playerID = playerId;
            playerInfo.PIN = PIN;

            RunWorker(Resources.WaitFormUpdatingPlayer, new DoWorkEventHandler(SendGetSetPlayerCardPIN), (object)playerInfo, new RunWorkerCompletedEventHandler(GetSetPlayerCardPINComplete));
        }

        /// <summary>
        /// Gets or sets a player's player card PIN on the server.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The DoWorkEventArgs object that 
        /// contains the event data.</param>
        private void SendGetSetPlayerCardPIN(object sender, DoWorkEventArgs e)
        {
            SetupThread();

            // Unbox the argument.
            int playerId = ((PlayerLookupInfo)(e.Argument)).playerID;
            int PIN = ((PlayerLookupInfo)(e.Argument)).PIN;

            // Are we getting the PIN?
            if (PIN == -1) //yes
            {
                GetPlayerMagCardPINMessage PINMsg = new GetPlayerMagCardPINMessage(playerId);

                PINMsg.Send();

                PIN = PINMsg.PlayerMagCardPIN;
            }
            else //setting the PIN
            {
                SetPlayerMagCardPINMessage PINMsg = new SetPlayerMagCardPINMessage(playerId, PIN);

                PINMsg.Send();
            }

            e.Result = new Tuple<int , int>(playerId, PIN);
        }

        /// <summary>
        /// Handles the event when the get/set player card PIN BackgroundWorker 
        /// is complete.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The RunWorkerCompletedEventArgs object that 
        /// contains the event data.</param>
        private void GetSetPlayerCardPINComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            // Set the error that occurred (if any).
            LastAsyncException = e.Error;

            if (e.Error == null)
            {
                int playerId = ((Tuple<int, int>)e.Result).Item1;
                int PIN = ((Tuple<int, int>)e.Result).Item2;

                if (PIN > 0 && m_currentSale != null && m_currentSale.Player != null && m_currentSale.Player.Id == playerId)
                    m_currentSale.Player.PlayerCardPIN = PIN;
            }

            // Close the wait form.
            m_waitForm.CloseForm();
        }

        /// <summary>
        /// Starts the Player Center and shows the Player Management form.
        /// </summary>
        internal void StartPlayerCenter()
        {
            if (IsPlayerCenterInitialized)
            {
                bool playersSaved = false;
                Player playerToSet = null;

                // Initially load the current player if we have one.
                if (m_currentSale != null)
                    m_playerCenter.CurrentPlayer = m_currentSale.Player;
                else
                    m_playerCenter.CurrentPlayer = null;

                try
                {
                    // PDTS 964
                    CanUpdateMenus = false;

                    // Show the form.
                    m_playerCenter.ShowPlayerManagment(out playersSaved, out playerToSet);

                    CanUpdateMenus = true;

                    // TTP 50120
                    // Did we lose connection?
                    if (m_playerCenter.LastAsyncException is ServerCommException)
                    {
                        ServerCommFailed();
                        return;
                    }
                }
                catch (ServerCommException)
                {
                    ServerCommFailed();
                    return;
                }
                catch (Exception ex)
                {
                    ShowMessage(m_sellingForm, m_settings.DisplayMode, ex.Message);
                    return;
                }

                // FIX: DE2580
                // Did the user want us to set a player?
                if (playerToSet != null || (playersSaved && m_currentSale != null && m_currentSale.Player != null))
                {
                    // Reload the the player if they saved any or we need to set one.
                    //                    try
                    //                    {
                    if (playerToSet != null)
                    {
                        if (m_currentSale != null)
                            m_currentSale.NeedPlayerCardPIN = false;

                        if (playerToSet.MagneticCardNumber != string.Empty)
                        {
                            m_sellingForm.GetPlayer(playerToSet.MagneticCardNumber);
                            //ShowWaitForm(m_sellingForm); // Block until we are done.
                        }
                        else
                        {
                            StartGetPlayer(playerToSet.Id);
                            //ShowWaitForm(m_sellingForm); // Block until we are done.
                        }
                    }
                    else
                    {
                        StartGetPlayer(m_currentSale.Player.Id);
                        //ShowWaitForm(m_sellingForm); // Block until we are done.
                    }
                    //                    }
                    //                    catch(Exception ex)
                    //                    {
                    //                        // TTP 50114
                    //                        Log("Failed to get the player/machine: " + ex.Message, LoggerLevel.Severe);
                    //                        ShowMessage(m_sellingForm, m_settings.DisplayMode, string.Format(CultureInfo.CurrentCulture,
                    //                                         m_settings.EnableAnonymousMachineAccounts ? Resources.GetMachineFailed : Resources.GetPlayerFailed, ex.Message));
                    //                    }

                    /*                    if(LastAsyncException != null)
                                        {
                                            if(LastAsyncException is ServerCommException)
                                                ServerCommFailed();
                                            else
                                                ShowMessage(m_sellingForm, m_settings.DisplayMode, LastAsyncException.Message);
                                        }

                                        if(playerToSet != null)
                                        {
                                            // Rally US493 - Player Center (Player Statuses).
                                            CheckForAlerts(playerToSet);
                                        }
                                    }
                                    // END: DE2580

                                    // Update the UI.
                                    m_sellingForm.SetPlayer();
                                    m_sellingForm.UpdateMenuButtonStates();
                     */
                }
            }
        }

        #region Product By Barcode
        internal void StartGetProduct(string barcode)
        {
            RunWorker(Resources.WaitFormGettingProduct, new DoWorkEventHandler(SendGetProductData), (object)barcode, new RunWorkerCompletedEventHandler(GetProductDataComplete));
        }

        /// <summary>
        /// Gets the product data that is associated with the given
        ///  barcode from the server.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The DoWorkEventArgs object that 
        /// contains the event data.</param>
        private void SendGetProductData(object sender, DoWorkEventArgs e)
        {
            SetupThread();

            // Unbox the argument.
            string barcode = string.Empty;

            barcode = e.Argument as string;

            DecodeBarcodeMessage prodMsg = new DecodeBarcodeMessage(barcode);

            try
            {
                prodMsg.Send();
            }
            catch (ServerCommException)
            {
                throw; // Don't repackage the ServerCommException
            }
            catch (ServerException ex)
            {
                throw new POSException(string.Format(CultureInfo.CurrentCulture, Resources.GetProductDataByBarcodeFailed, ServerExceptionTranslator.FormatExceptionMessage(ex)), ex);
            }

            object[] results = new object[10];

            BarcodeProductItem productItem = prodMsg.BarcodeItem;
            SessionKeyItem keyItem = null;

            //find our key in the list (first package ID we have in our menu)
            PackageButton pkgButton = null;
            List<SessionKeyItem> keys = productItem.SessionKeyItems.Where(k => k.Session == CurrentSessionPlayedId).OrderBy(k => (k.Page.ToString().PadLeft(3, '0')+" "+k.Key.ToString().PadLeft(3, '0'))).ToList();
            bool buttonWasLocked = false;

            foreach(SessionKeyItem key in keys)
            {
                Tuple<PackageButton, bool> result = GetNonLockedMenuButtonForPackage(key.PackageID, true);

                pkgButton = result.Item1;

                if (result.Item2)
                    buttonWasLocked = true;

                if (pkgButton != null)
                {
                    keyItem = key;
                    buttonWasLocked = false;
                    break;
                }
            }

            results[0] = productItem.AuditNumber;
            results[1] = productItem.SerialNumber;
            results[2] = productItem.Name;
            results[3] = productItem.CardCount;//US3509
            results[4] = productItem.Status;

            if (keyItem != null)
            {
                results[5] = keyItem.Page;
                results[6] = keyItem.Key;
                results[7] = pkgButton as MenuButton;
            }

            results[8] = productItem.ItemBarcodeType;
            results[9] = buttonWasLocked;

            e.Result = results;
        }

        /// <summary>
        /// Handles the event when the get product by barcode BackgroundWorker 
        /// is complete.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The RunWorkerCompletedEventArgs object that 
        /// contains the event data.</param>
        private void GetProductDataComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            // Set the error that occurred (if any).
            LastAsyncException = e.Error;
            GetProductDataButton = null;
            GetProductDataButtonValues = null;

            if (e.Error == null)
            {
                // Set the product data we retrieved to the current sale.
                try
                {
                    BarcodeType barcodeType = BarcodeType.UnknownBarcodeType;
                    int page = -1;
                    int key = -1;
                    int auditNumber = 0;
                    int cardcount = 0;
                    int status = 0;
                    string serialNumber = "";
                    string productName = "";
                    MenuButton menuButton = null;
                    bool buttonWasLocked = false;

                    object[] results = (object[])e.Result;

                    if (results[0] != null) auditNumber = (int)results[0];
                    if (results[1] != null) serialNumber = (string)results[1];
                    if (results[2] != null) productName = (string)results[2];
                    if (results[3] != null) cardcount = (int)results[3]; //US3509
                    if (results[4] != null) status = (int)results[4];
                    if (results[5] != null) page = (int)results[5];
                    if (results[6] != null) key = (int)results[6];
                    if (results[7] != null) menuButton = (MenuButton)results[7];
                    if (results[8] != null) barcodeType = (BarcodeType)results[8];
                    if (results[9] != null) buttonWasLocked = (bool)results[9];

                    if (menuButton != null)
                    {
                        page = menuButton.Page;
                        key = menuButton.Position;
                    }

                    if (buttonWasLocked)
                    {
                        ShowMessage(m_sellingForm, m_settings.DisplayMode, Resources.ScannedItemIsOnALockedButton);
                    }
                    else
                    {
                        if (status == -5) //already sold
                        {
                            switch (Settings.SellPreviouslySoldItem)
                            {
                                case POSSettings.SellAgainOption.Allow:
                                {
                                    status = 0; //pretend it wasn't sold
                                }
                                break;

                                case POSSettings.SellAgainOption.Ask:
                                {
                                    if (ShowMessage(m_sellingForm, Settings.DisplayMode, Resources.BarcodeSellAgain, POSMessageFormTypes.YesNo_DefNO) == DialogResult.Yes)
                                    {
                                        status = 0; //pretend it wasn't sold
                                        Application.DoEvents();
                                    }
                                    else
                                    {
                                        // Close the wait form and get out of here.
                                        m_waitForm.CloseForm();
                                        return;
                                    }
                                }
                                break;

                                case POSSettings.SellAgainOption.Disallow:
                                {
                                    //leave the status alone, it will be trapped later and stop the sale.
                                }
                                break;
                            }
                        }

                        if (status >= 0)
                        {
                            if (page == -1 && key == -1)
                            {
                                if (!string.IsNullOrEmpty(productName))
                                {
                                    ShowMessage(m_sellingForm, m_settings.DisplayMode,
                                        string.Format(CultureInfo.CurrentCulture,
                                            Resources.BarcodeMenuButtonError, productName));
                                }
                            }
                            else
                            {
                                if (menuButton != null)
                                {
                                    object[] values = new object[4];
                                    values[0] = 1;
                                    values[1] = auditNumber;
                                    values[2] = serialNumber;
                                    values[3] = cardcount;//US3509

                                    // If there is no sale, then start it.
                                    if (m_currentSale == null)
                                        StartSale(false);

                                    GetProductDataButton = menuButton;
                                    GetProductDataButtonValues = barcodeType == BarcodeType.PaperScanCode ? values : null;
                                }
                                else
                                {
                                    ShowMessage(m_sellingForm, m_settings.DisplayMode, string.Format(CultureInfo.CurrentCulture,
                                                     Resources.PackageNotFound));
                                }
                            }
                        }
                        else
                        {
                            SerialAuditNumberError(status, serialNumber, auditNumber);
                        }
                    }
                }
                catch (BarcodeException ex)
                {
                    //US2628 Something happened when scanning a barcode so let the user know
                    ShowMessage(m_sellingForm, m_settings.DisplayMode, ex.Message);
                }
                catch (POSException ex)
                {
                    // TTP 50114
                    ShowMessage(m_sellingForm, m_settings.DisplayMode, string.Format(CultureInfo.CurrentCulture,
                                     Resources.GetProductDataByBarcodeFailed, ex.Message));
                }
            }

            // Close the wait form.
            m_waitForm.CloseForm();
        }

        internal void StartCheckSerialAuditNumbers(int productId, string serialNumber, int auditNumber, Package package, PackInfo packInfo)//DE12863: Add package as a parameter
        {
            object[] inData = new Object[5];
            inData[0] = productId;
            inData[1] = auditNumber;
            inData[2] = serialNumber;
            inData[3] = package; //DE12863
            inData[4] = packInfo;

            RunWorker(Resources.WaitFormCheckingSerialAuditNumbers, new DoWorkEventHandler(SendCheckSerialAuditNumbers), inData, new RunWorkerCompletedEventHandler(CheckSerialAuditNumbersComplete));
        }

        private void SendCheckSerialAuditNumbers(object sender, DoWorkEventArgs e)
        {
            SetupThread();

            // Unbox the arguments
            string serialNumber = string.Empty;
            int auditNumber = 0;
            int productId = 0;
            Package package = null;
            PackInfo packInfo = null;

            object[] inData = (object[])e.Argument;

            if (inData[0] != null) productId = (int)inData[0];
            if (inData[1] != null) auditNumber = (int)inData[1];
            if (inData[2] != null) serialNumber = (string)inData[2];
            if (inData[3] != null) package = (Package)inData[3]; //DE12863
            if (inData[4] != null) packInfo = (PackInfo)inData[4];

            var msg = new CheckSerialAuditNumbersMessage(productId, serialNumber, auditNumber);

            try
            {
                msg.Send();
            }
            catch (ServerCommException)
            {
                throw; // Don't repackage the ServerCommException
            }
            catch (ServerException ex)
            {
                throw new POSException(string.Format(CultureInfo.CurrentCulture, Resources.GetProductDataByBarcodeFailed, ServerExceptionTranslator.FormatExceptionMessage(ex)), ex);
            }

            if (packInfo != null)
            {
                packInfo.SerialNumber = msg.SerialNumber;
                packInfo.AuditNumber = msg.AuditNumber;
            }

            object[] results = new object[5];
            results[0] = msg.SerialNumber;
            results[1] = msg.AuditNumber;
            results[2] = msg.Status;
            results[3] = msg.CardCount;
            results[4] = packInfo;

            //DE12863
            if (package != null)
            {
                foreach (var product in package.GetProducts().OfType<BingoProduct>())
                {
                    if (product.Id == productId)
                        product.CardCount = (short)msg.CardCount;
                }
            }

            e.Result = results;
        }

        private void CheckSerialAuditNumbersComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            // Set the error that occurred (if any).
            LastAsyncException = e.Error;

            if (e.Error == null)
            {
                // Set the product data we retrieved to the current sale.
                try
                {
                    int status = 0;
                    int cardCount = 0;
                    int auditNumber = 0;
                    string serialNumber = "";

                    object[] results = (object[])e.Result;

                    if (results[0] != null) serialNumber = (string)results[0];
                    if (results[1] != null) auditNumber = (int)results[1];
                    if (results[2] != null) status = (int)results[2];
                    if (results[3] != null) cardCount = (int)results[3];

                    if (status == -5) //already sold
                    {
                        switch (Settings.SellPreviouslySoldItem)
                        {
                            case POSSettings.SellAgainOption.Allow:
                            {
                                status = 0; //pretend it wasn't sold
                            }
                            break;

                            case POSSettings.SellAgainOption.Ask:
                            {
                                if (ShowMessage(m_sellingForm, m_settings.DisplayMode, Resources.BarcodeSellAgain, POSMessageFormTypes.YesNo_DefNO) == DialogResult.Yes)
                                {
                                    status = 0; //pretend it wasn't sold
                                }
                                else
                                {
                                    LastAsyncException = new BarcodeException(Resources.BarcodeAlreadySold);

                                    // Close the wait form and get out of here.
                                    m_waitForm.CloseForm();
                                    return;
                                }
                            }
                            break;

                            case POSSettings.SellAgainOption.Disallow:
                            {
                                //leave the status alone, it will be trapped later and stop the sale.
                            }
                            break;
                        }
                    }

                    if (status < 0)
                        SerialAuditNumberError(status, serialNumber, auditNumber);
                }
                catch (BarcodeException ex)
                {
                    LastAsyncException = ex;

                    //US2628 Something happened when scanning a barcode so let the user know
                    ShowMessage(m_sellingForm, m_settings.DisplayMode, ex.Message);
                }
                catch (POSException ex)
                {
                    LastAsyncException = ex;

                    // TTP 50114
                    ShowMessage(m_sellingForm, m_settings.DisplayMode, string.Format(CultureInfo.CurrentCulture,
                                     Resources.GetProductDataByBarcodeFailed, ex.Message));
                }
            }

            // Close the wait form.
            m_waitForm.CloseForm();
        }


        /// <summary>
        /// Generic function for handling serial and audit number errors
        /// </summary>
        /// <param name="errCode"></param>
        /// <param name="serialNumber"></param>
        /// <param name="auditNumber"></param>
        internal void SerialAuditNumberError(int errCode, string serialNumber, int auditNumber)
        {
            string message = "";

            switch (errCode)
            {
                case -1:
                    message = string.Format(CultureInfo.CurrentCulture, Resources.BarcodeSerialNumberError, serialNumber);
                    break;

                case -2:
                    message = string.Format(CultureInfo.CurrentCulture, Resources.BarcodeSerialNumberRetired, serialNumber);
                    break;

                case -3:
                    message = string.Format(CultureInfo.CurrentCulture, Resources.BarcodeAuditNumberError, auditNumber.ToString(), serialNumber);
                    break;

                case -4:
                    message = string.Format("Unable to find a barcoded product for Audit Number {0} and Serial Number {1}", auditNumber.ToString(), serialNumber);
                    break;

                case -5:
                    message = Resources.BarcodeAlreadySold;
                    break;

                case -6:
                    message = Resources.BarcodeDamagedItem;
                    break;

                default:
                    message = string.Format("Unknown error {0}, for Audit Number {1} and Serial Number {2}", errCode, auditNumber.ToString(), serialNumber);
                    break;
            }

            throw new BarcodeException(message);
        }

        #endregion


        /// <summary>
        /// Starts a new sale.
        /// </summary>
        /// <param name="isReturn">Whether this is a return.</param>
        internal void StartSale(bool isReturn)
        {
            if (CashDrawerIsOpen(true))
                POSMessageForm.Show(SellingForm, this, Resources.PleaseCloseTheCashDrawer, POSMessageFormTypes.CloseWithCashDrawer);

            // TTP 50114
            //DE13201: POS null reference sellilng B3 session. Check bank for null
            m_currentSale = new Sale(this, m_settings.EnableAnonymousMachineAccounts, GamingDate, isReturn,
                                     m_settings.TaxRate, 0M, m_machineId, m_currentStaff, m_bank == null ? 0: m_bank.Id, m_currentCurrency); // FIX: DE1930, DE1938, TA7465

            KioskChangeDispensingFailed = false;

            // PDTS 571
            m_sellingState = SellingState.Selling;
        
            m_sellingForm.UpdateSystemButtonStates();

            // If its a return, some buttons need to be disabled.
            if(isReturn)
            {
                m_sellingForm.DisplayReturnMode();
                m_sellingForm.UpdateMenuButtonStates();
            }
        }

        internal void AddSaleItem(SessionInfo session, int quantity, PlayerComp playerComp)
        {
            // Start the sale if it doesn't exist.
            if (m_currentSale == null)
                StartSale(false);

            // Add the package and update the sales list.
            int lineNum = m_currentSale.AddItem(session, quantity, playerComp);

            m_currentSale.UpdatePercentageDiscounts(session);//US4636
            UpdateAutoDiscounts();
            m_sellingForm.UpdateSaleInfo();
        }

        // Rally US505
        /// <summary>
        /// Adds a package to a sale.
        /// </summary>
        /// <param name="session">The session the package is for.</param>
        /// <param name="sessionPlayedId">The database session played id the 
        /// package is for.</param>
        /// <param name="package">The package to add.</param>
        /// <param name="quantity">The number of packages to add.</param>
        /// <param name="isPlayerRequired">Whether a player is required in 
        /// order to add the item to the sale.</param>
        /// <param name="cbbCards">Any Crystal Ball cards that are to be 
        /// associated with this package (if applicable).</param>
        /// <param name="updateDiscounts"></param>
        /// <param name="alwaysAddNewLineItem"></param>
        internal void AddSaleItem(SessionInfo session, Package package, int quantity,
            bool isPlayerRequired, IEnumerable<CrystalBallCardCollection> cbbCards, bool updateDiscounts = true,
            bool alwaysAddNewLineItem = false)
        {
            m_sellingForm.NotIdle(true);

            //// Start the sale if it doesn't exist.
            //if (m_currentSale == null)
            //    StartSale(false);

            // US2826 Adding barcoded paper support
            //Allow user to enter or scan all paper packs with no pack info
            while (package.HasBarcodedPaper && package.NeedsPackInfo(quantity))
            {
                PaperBingoProduct paperProduct = package.GetNextBarcodedPaperProductToIdentify(quantity);
                BarcodedPaperInputForm paperForm = new BarcodedPaperInputForm(this, Settings.DisplayMode, true,
                    string.Format("{0} (Need {1})", paperProduct.Name, package.PacksThatNeedInfo(paperProduct, quantity)));

                DialogResult result = paperForm.ShowDialog(m_sellingForm.KioskForm != null? (IWin32Window)m_sellingForm.KioskForm : m_sellingForm);

                if (result == DialogResult.OK)
                {
                    bool addPack = true;
                    int auditNumber = 0;
                    PackInfo ourPackInfo = new PackInfo(paperForm.SerialNumber, auditNumber);

                    if (int.TryParse(paperForm.AuditNumber, out auditNumber) && paperForm.SerialNumber.Length > 0)
                    {
                        int productId = paperProduct.Id;

                        ourPackInfo.SerialNumber = paperForm.SerialNumber;
                        ourPackInfo.AuditNumber = auditNumber;

                        if (addPack)
                        {
                            //DE12863: added package parameter
                            StartCheckSerialAuditNumbers(productId, paperForm.SerialNumber, auditNumber, package, ourPackInfo);
                            ShowWaitForm(m_sellingForm);

                            if (CurrentSale != null)
                            {
                                PaperPackInfo packInfo = new PaperPackInfo();

                                packInfo.SerialNumber = ourPackInfo.SerialNumber;
                                packInfo.AuditNumber = ourPackInfo.AuditNumber;

                                //see if this serial number/audit number is already in this sale (could be in another package)
                                foreach (SaleItem s in CurrentSale.GetItems())
                                {
                                    if (s.IsPackageItem && s.Package.HasBarcodedPaper)
                                    {
                                        if (s.Package.ContainsPackInfo(packInfo))
                                        {
                                            addPack = false;
                                            POSMessageForm.Show(m_sellingForm, this, String.Format(Resources.DuplicateAuditNumber, ourPackInfo.AuditNumber), POSMessageFormTypes.OK);
                                            break;
                                        }
                                    }
                                }

                                if (!addPack)
                                    break;
                            }

                            if (LastAsyncException == null) //no error, add the pack info to the package
                            {
                                package.SetPackInfoData(paperProduct, ourPackInfo.SerialNumber, ourPackInfo.AuditNumber);
                            }
                            else //there was an error
                            {
                                if (!(LastAsyncException is BarcodeException || LastAsyncException is POSException)) //this error was not displayed in StartCheckSerialAuditNumbers, show it now
                                    POSMessageForm.Show(m_sellingForm, this, LastAsyncException.Message, POSMessageFormTypes.OK);
                            }
                        }
                    }
                }

                if (result == DialogResult.Cancel)
                {
                    m_sellingForm.NotIdle();
                    throw new POSUserCancelException();
                }
            }

            // Start the sale if it doesn't exist.
            if (m_currentSale == null)
                StartSale(false);

            // Add the package and update the sales list.
            if (m_currentSale == null)
            {
                m_sellingForm.NotIdle();
                return;
            }

            DateTime? gamingDate = null;
            if (CurrentSession.GamingDate != null && CurrentSession.GamingDate.Value != GamingDate)
            {
                gamingDate = CurrentSession.GamingDate.Value;
            }

            int lineNum = m_currentSale.AddItem(session, quantity, package, isPlayerRequired, cbbCards,
                alwaysAddNewLineItem, gamingDate);
            var currentSalesItem = m_currentSale.GetItems()[lineNum];

            UpdateValidationSales(package);
            m_currentSale.UpdatePercentageDiscounts(session); //US4636

            if (updateDiscounts)
            {
                UpdateAutoDiscounts();

                m_sellingForm.UpdateSaleInfo();
                m_sellingForm.SelectSaleItem(m_currentSale.GetIndexOf(currentSalesItem));
                m_sellingForm.UpdateSystemButtonStates();
            }

            m_sellingForm.NotIdle();
        }

        // FIX: DE2957
        /// <summary>
        /// Adds a discount to a sale.
        /// </summary>
        /// <param name="session">The session the discount is for.</param>
        /// <param name="sessionPlayedId">The database session played id the 
        /// discount is for.</param>
        /// <param name="discount">The discount to add.</param>
        /// <param name="quantity">The number of discounts to add.</param>
        /// <param name="isPlayerRequired">Whether a player is required in 
        /// order to add the item to the sale.</param>
        internal void AddSaleItem(SessionInfo session, Discount discount, int quantity, bool isPlayerRequired)
        {
            if(discount == null)
                throw new ArgumentNullException("discount");

            // Check to make sure this staff can add this discount.
            if(discount is OpenDiscount && !m_currentStaff.CheckModuleFeature(EliteModule.POS, (int)POSFeature.OpenDiscounts))
            {
                CanUpdateMenus = false; // PDTS 964

                // Ask for an override.
                if(!POSSecurity.TryOverride(this, m_waitForm, m_sellingForm, null, (int)EliteModule.POS, (int)POSFeature.OpenDiscounts))
                {
                    CanUpdateMenus = true; // PDTS 964
                    return; // Cancel the add.
                }
                else
                    CanUpdateMenus = true; // PDTS 964
            }

            // Start the sale if it doesn't exist.
            if(m_currentSale == null)
                StartSale(false);

            // Add the discount and update the sales list.
            int lineNum = m_currentSale.AddItem(session, discount, quantity, isPlayerRequired);

            m_currentSale.UpdatePercentageDiscounts(session); //US4636
            UpdateAutoDiscounts();
            m_sellingForm.UpdateSaleInfo();
            m_sellingForm.SelectSaleItem(lineNum);
            m_sellingForm.UpdateSystemButtonStates();
        }
        // END: DE2957

        //US4382: (US4337) POS: Open sale

        /// <summary>
        /// Adds the sale item.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="sessionPlayedId">The session played identifier.</param>
        /// <param name="quantity">The quantity.</param>
        /// <param name="b3Credit"></param>
        /// <exception cref="POSUserCancelException"></exception>
        internal void AddSaleItem(SessionInfo session, int quantity, B3Credit b3Credit)
        {
            // Start the sale if it doesn't exist.
            if (m_currentSale == null)
                StartSale(false);

            // Add the package and update the sales list.
            int lineNum = m_currentSale.AddItem(session, quantity, b3Credit);

            m_sellingForm.UpdateSaleInfo();
            m_sellingForm.SelectSaleItem(lineNum);
        }

        /// <summary>
        /// Adds the validation sales.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="sessionPlayedId">The session played identifier.</param>
        /// <param name="package">The package.</param>
        public void UpdateValidationSales(Package package)
        {
            if (CurrentSale == null || CurrentSale.IsReturn)
                return;

            //get a list of all the sessions sold
            var sessions = CurrentSale.GetSessions();

            if (sessions != null)
            {
                //update validations per session
                foreach (var session in sessions)
                    AddValidationSalesPerSession(package, session);
            }
        }

        /// <summary>
        /// Updates the validation sales per session.
        /// </summary>
        /// <param name="package">The sale item package</param>
        /// <param name="session">The session.</param>
        /// <param name="sessionPlayedId">The session played identifier.</param>
        private void AddValidationSalesPerSession(Package package, SessionInfo session)
        {
            //bool individualMode = true;
            var totalQuantityToBeValidated = 0;
            var saleItems = CurrentSale.GetItems();
            var currentValidationQuantity = 0;
            SaleItem validationItem = null;

            if (CurrentSale.IsReturn || session.DefaultValidationPackageButton == null)
                return;

            //get current validation count for the session
            foreach (var item in saleItems.Where(item => item.IsDefaultValidationPackage && item.Session == session))
            {
                currentValidationQuantity = item.Quantity;
                validationItem = item;
                break;
            }

            //validate all items and get count of total items to be validated for the session
            foreach (var item in saleItems.Where(item => item.IsPackageItem && item.Session == session))
            {
                var paperCardCount = 0;
                var electronicCardCount = 0;
                bool itemIsValidated = false;

                item.Package.PackageValidationValue = 0.0m;

                //iterate through all products in the session
                foreach (var bingoProduct in item.Package.GetProducts().OfType<BingoProduct>())
                {
                    if (!bingoProduct.CanValidateProduct)
                        continue;

                    switch (bingoProduct.Type)
                    {
                        case ProductType.Paper:
                        {
                            //check to see if product can be validated. Zero = cannot be validated
                            var quantity = bingoProduct.GetValidationQuantity(session.ValidationPackage);

                            if (package == null || package.HasPaperBingo) //changing paper
                            {
                                bingoProduct.IsValidated = false;

                                if (ValidationEnabled)
                                    paperCardCount += quantity;

                                bingoProduct.IsValidated = ValidationEnabled;
                            }
                            else //not changing paper, use current values
                            {
                                if (bingoProduct.IsValidated)
                                    paperCardCount += quantity;
                            }
                            break;
                        }

                        case ProductType.Electronic:
                        {
                            //check to see if product can be validated. Zero = cannot be validated
                            var quantity = bingoProduct.GetValidationQuantity(session.ValidationPackage);

                            if (package == null || package.HasElectronicBingo) //changing electronics
                            {
                                bingoProduct.IsValidated = false;
                                
                                if (ValidationEnabled)
                                    electronicCardCount += quantity;

                                bingoProduct.IsValidated = ValidationEnabled;
                            }
                            else //not changing electronics, use current values
                            {
                                if (bingoProduct.IsValidated)
                                    electronicCardCount += quantity;
                            }
                            break;
                        }
                    }

                    if (bingoProduct.IsValidated)
                        itemIsValidated = true;
                }

                int paperValidations;
                int electricValidations;

                //if m_validationPackage.CardCount == 0 then we do not want to divide by card count.
                if (session.ValidationPackage.CardCount == 0)
                {
                    paperValidations = paperCardCount * item.Quantity;
                    electricValidations = electronicCardCount * item.Quantity;
                }
                else
                {
                    //else divide by base card count
                    paperValidations = paperCardCount / session.ValidationPackage.CardCount * item.Quantity;
                    electricValidations = electronicCardCount / session.ValidationPackage.CardCount * item.Quantity;
                }

                //get the quantity to add to the package;
                int quantityToAdd;

                //update totalQuantityToBeValidated if package is overridden
                //only override package if there are validations enabled
                if (item.Package.OverrideValidation && itemIsValidated)
                {
                    quantityToAdd = item.Package.OverrideValidationQuantity*item.Quantity;
                    totalQuantityToBeValidated += quantityToAdd;
                }
                else
                {
                    quantityToAdd = paperValidations + electricValidations;
                    totalQuantityToBeValidated += quantityToAdd;
                }
                
                //if quantity exceeds the max, then calculate difference and add to package validation value
                //if no max then always add
                if (session.ValidationPackage.MaxQuantity == 0 || !session.IsMaxValidationEnabled)
                {
                    item.Package.PackageValidationValue = quantityToAdd * ((PackageButton)session.DefaultValidationPackageButton).Package.Price;
                }
                //is less than max then it is safe to add quantity
                else if (totalQuantityToBeValidated <= session.ValidationPackage.MaxQuantity)
                {
                    item.Package.PackageValidationValue = quantityToAdd * ((PackageButton)session.DefaultValidationPackageButton).Package.Price;
                }
                //total minus quantity to add is less than max, then add the difference
                else if (totalQuantityToBeValidated - quantityToAdd < session.ValidationPackage.MaxQuantity)
                {
                    var difference = session.ValidationPackage.MaxQuantity - (totalQuantityToBeValidated - quantityToAdd);
                    item.Package.PackageValidationValue = difference * ((PackageButton)session.DefaultValidationPackageButton).Package.Price;
                }
            }

            //did not add any validations
            if (totalQuantityToBeValidated == 0 || session == null)
            {
                if (validationItem != null)
                    m_currentSale.RemoveItem(validationItem);
             
                return;
            }

            //no max, always add more validations or if max validation is turned off
            if (session.ValidationPackage.MaxQuantity == 0 || !session.IsMaxValidationEnabled)
            {
                totalQuantityToBeValidated = totalQuantityToBeValidated - currentValidationQuantity;
                m_currentSale.AddItem(session, totalQuantityToBeValidated, ((PackageButton)session.DefaultValidationPackageButton).Package, session.ValidationPackage);
                return;
            }

            //if exceed max, then everything is validated for this update both product types with validation
            if (ValidationEnabled && totalQuantityToBeValidated >= session.ValidationPackage.MaxQuantity && session.IsMaxValidationEnabled)
            {
                UpdateBingoProductValidationFlagForCurrentSale(ProductType.Paper);
                UpdateBingoProductValidationFlagForCurrentSale(ProductType.Electronic);
            }

            //add validations
            //if total is greater than max and current is less than max, then we need to validate the difference
            if (totalQuantityToBeValidated > session.ValidationPackage.MaxQuantity && currentValidationQuantity < session.ValidationPackage.MaxQuantity)
            {
                totalQuantityToBeValidated = session.ValidationPackage.MaxQuantity - currentValidationQuantity;
                m_currentSale.AddItem(session, totalQuantityToBeValidated, ((PackageButton)session.DefaultValidationPackageButton).Package, session.ValidationPackage);
            }
            else if (totalQuantityToBeValidated <= session.ValidationPackage.MaxQuantity)
            {
                //calculate difference from what is already added
                totalQuantityToBeValidated = totalQuantityToBeValidated - currentValidationQuantity;
                m_currentSale.AddItem(session, totalQuantityToBeValidated, ((PackageButton)session.DefaultValidationPackageButton).Package, session.ValidationPackage);
            }

            if(session == CurrentSession)
                SellingForm.UpdateMenuButtonPrices();
        }

        /// <summary>
        /// Updates the bingo product validations for current sale.
        /// </summary>
        /// <param name="type">The type.</param>
        public void UpdateBingoProductValidationFlagForCurrentSale(ProductType type)
        {
            //iterate through all sale items
            foreach (var saleItem in CurrentSale.GetItems())
            {
                //if is package then iterate through all products
                if (saleItem.IsPackageItem)
                {
                    //iterate through products
                    foreach (var bingoProduct in saleItem.Package.GetProducts().OfType<BingoProduct>())
                    {
                        //only validate type passed in
                        if (bingoProduct.Type != type)
                            continue;

                        //get quantity of validation sales
                        var quantity = bingoProduct.GetValidationQuantity(saleItem.Session.ValidationPackage);

                        //this product does not meet requirements for validation
                        if (quantity == 0)
                            continue;

                        //bingo product validated or not based on mode
                        bingoProduct.IsValidated = ValidationEnabled;

                        if (!ValidationEnabled)
                            saleItem.Package.PackageValidationValue = 0;
                    }
                }
            }
        }

        /// <summary>
        /// Get the price of a package including validation.
        /// </summary>
        /// <param name="saleItem">The item to process.</param>
        /// <returns>Package price + validation.</returns>
        public decimal GetValidatedPackagePrice(Package package)
        {
            if (!ValidationEnabled || (CurrentSale != null && CurrentSale.IsReturn) || (CurrentSession != null && CurrentSession.DefaultValidationPackageButton == null))
                return package.Price;

            var totalQuantityToBeValidated = 0;
            var saleItems = CurrentSale == null? new SaleItem[0]:CurrentSale.GetItems();
            SaleItem validationItem = CurrentSale == null ? null : CurrentSale.GetItems().FirstOrDefault(i => i.Session == CurrentSession && i.IsDefaultValidationPackage);
            var currentValidationQuantity = (validationItem != null?validationItem.Quantity:0);
            int paperValidations = 0;
            int electricValidations = 0;
            int paperCardCount = 0;
            int electronicCardCount = 0;
            decimal price = package.Price;

            //iterate through products in the package and count cards
            foreach (var bingoProduct in package.GetProducts().OfType<BingoProduct>())
            {
                //get quantity to validate
                var quantity = bingoProduct.GetValidationQuantity(CurrentSession.ValidationPackage);

                //product cannot validate and package is not overridden
                if (quantity == 0 && !package.OverrideValidation)
                    continue;

                //update validation
                switch (bingoProduct.Type)
                {
                    case ProductType.Paper:
                    {
                        paperCardCount += quantity;
                        break;
                    }

                    case ProductType.Electronic:
                    {
                        electronicCardCount += quantity;
                        break;
                    }
                }
            }

            //calculate total validations
            //if default validation card count is zero then do not divide by zero
            if (CurrentSession.ValidationPackage.CardCount == 0)
            {
                paperValidations = paperCardCount;
                electricValidations = electronicCardCount;
            }
            else
            {
                //else divide by total card count by default validation card count
                paperValidations = paperCardCount / CurrentSession.ValidationPackage.CardCount;
                electricValidations = electronicCardCount / CurrentSession.ValidationPackage.CardCount;
            }

            //get the quantity to add to the package;
            int quantityToAdd;

            totalQuantityToBeValidated = currentValidationQuantity;

            //update totalQuantityToBeValidated if package is overridden
            //only override package if there are validations enabled
            if (package.OverrideValidation)
            {
                quantityToAdd = package.OverrideValidationQuantity;
                totalQuantityToBeValidated += quantityToAdd;
            }
            else
            {
                quantityToAdd = paperValidations + electricValidations;
                totalQuantityToBeValidated += quantityToAdd;
            }

            //if quantity exceeds the max, then calculate difference and add to package validation value
            //if no max then always add
            if (CurrentSession.ValidationPackage.MaxQuantity == 0 || !CurrentSession.IsMaxValidationEnabled)
            {
                price += quantityToAdd * ((PackageButton)CurrentSession.DefaultValidationPackageButton).Package.Price;
            }
            //is less than max then it is safe to add quantity
            else if (totalQuantityToBeValidated <= CurrentSession.ValidationPackage.MaxQuantity)
            {
                price += quantityToAdd * ((PackageButton)CurrentSession.DefaultValidationPackageButton).Package.Price;
            }
            //total minus quantity to add is less than max, then add the difference
            else if (totalQuantityToBeValidated - quantityToAdd < CurrentSession.ValidationPackage.MaxQuantity)
            {
                var difference = CurrentSession.ValidationPackage.MaxQuantity - (totalQuantityToBeValidated - quantityToAdd);
                price += difference * ((PackageButton)CurrentSession.DefaultValidationPackageButton).Package.Price;
            }

            return price;
        }
        
        //US4323
        /// <summary>
        /// Updates the automatic discounts.
        /// </summary>
        public void UpdateAutoDiscounts()
        {
            try
            {
                //if there are no auto discounts or quantity discounts (BOGO's), then return
                if ((m_autoDiscounts == null || m_autoDiscounts.Count <= 0) && 
                    (m_autoDiscountsByQuantity == null || m_autoDiscountsByQuantity.Count <= 0))
                {
                    return;
                }

                //add quantity discounts
                AddQuantityDiscounts(CurrentSession);
               
                //add discount for the current session (we do this last so the discount text generated last is for the current session)
                AddAutoDiscounts(CurrentSession); //if nothing was sold in the session, the discount text will be cleared
            }
            catch (Exception ex)
            {
                var message = string.Format(Resources.UnableToUpdateDiscounts, ex.Message);
                ShowMessage(m_sellingForm, m_settings.DisplayMode, message);
                Log(message, LoggerLevel.Warning);
            }
        }

        //US5117: POS: Automatically add package X when package Y has been added Z times
        /// <summary>
        /// Adds the quantity discounts. (BOGO's)
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="sessionPlayedId">The session played identifier.</param>
        public void AddQuantityDiscounts(SessionInfo session)
        {
            if (m_currentSale == null)
                return;

            //return if auto discounts is not enabled for this session
            //US4721: (US4324) POS: Support scheduling discounts
            if (Settings.EnableActiveSalesSession)
            {
                if (!session.IsAutoDiscountsEnabled)
                    return;
            }

            //initialize lists
            var saleItems = new List<SaleItem>();
            var quantityDiscountedItems = new List<SaleItem>();

            //categorize sale items
            foreach (var saleItem in m_currentSale.GetItems().Where(item => item.Session == session && item.IsPackageItem))
            {
                //get a list of all regular sale items
                if (saleItem.Package.AppliedDiscountId == 0)
                {
                    //do not add alt price packages
                    if (saleItem.Package.UseAltPrice)
                        continue;

                    //sale items
                    saleItems.Add(saleItem);
                }

                //get a list of the awarded packages from the quantity discounts
                //if the package was added due to a quantity discount, then the AppliedDiscountId will be set
                else if (m_autoDiscountsByQuantity.Keys.ToList().Exists(item => item.Id == saleItem.Package.AppliedDiscountId))
                {
                    //discounted sale item
                    quantityDiscountedItems.Add(saleItem);
                }
            }
            
            //apply quantity discounts. this goes through all quantity discounts and
            //will re-caclulate and update discounted items.
            //we could possibly pass the added sales item and only update discounts
            //based off the new item added to make this more efficient.
            foreach (var keyValuePair in m_autoDiscountsByQuantity)
            {
                if (!IsDiscountValid(session, keyValuePair.Key))
                    continue;

                //create local variable for easier access and readability
                var discount = keyValuePair.Key;
                var buyPackageId = discount.DiscountItem.AdvancedQuantityDiscount.BuyPackageId;
                var buyQuantity = discount.DiscountItem.AdvancedQuantityDiscount.BuyQuantity;
                var getPackageButton = keyValuePair.Value.FirstOrDefault(b => b.Session == session);
                var getQuantity = discount.DiscountItem.AdvancedQuantityDiscount.GetQuantity;
                var possibleBuyQuantity = 0;
               
                //get the "Buy" sales item if it exists for the discount
                var buySaleItem = saleItems.FirstOrDefault(item => item.Package.Id == buyPackageId);

                //if there is no "Buy" package in the sale or the "Get" package is not in the menu,
                //the criteria is not met for this discount
                if (buySaleItem == null || getPackageButton == null)
                    continue;

                //calculate possible buy quantity
                possibleBuyQuantity = buyQuantity == 0? 0 : buySaleItem.Quantity / buyQuantity;
                
                //see if we already have a discounted item
                //the applied discount ID will be equal to the discount ID
                var getItem = quantityDiscountedItems.FirstOrDefault(item => !item.Package.UseAltPrice && item.Package.AppliedDiscountId == discount.Id);

                //we do not qualify for the discount
                if (possibleBuyQuantity == 0 )
                {
                    //need to remove any discounted item
                    if (getItem != null)
                        CurrentSale.RemoveItem(getItem);

                    continue;
                }

                //if does not exist, then we need to add it
                try
                {
                    if (getItem == null)
                    {
                        getPackageButton.Click(m_sellingForm, new object[] { possibleBuyQuantity * getQuantity, false }); // PDTS 693

                    }
                    else if (getItem.Quantity != possibleBuyQuantity * getQuantity)
                    {
                        //calculate the difference to do a quantity add
                        var addQuantity = possibleBuyQuantity * getQuantity - getItem.Quantity;
                        getPackageButton.Click(m_sellingForm, new object[] { addQuantity, false }); // PDTS 693
                    }
                }
                catch (POSUserCancelException)
                {
                    //do nothing if user cancels
                }
            }         
        }

        //US4323
        /// <summary>
        /// Adds the automatic discounts.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="sessionPlayedId">The session played identifier.</param>
        public void AddAutoDiscounts(SessionInfo session)
        {
            if (m_currentSale == null)
                return;

            //return if auto discounts is not enabled for this session
            //US4721: (US4324) POS: Support scheduling discounts
            if (Settings.EnableActiveSalesSession)
            {
                if (!session.IsAutoDiscountsEnabled)
                    return;
            }
            else
            {
                if (session != null && m_menuList.ToList().Exists(m => m.Session == session && !m.Session.IsAutoDiscountsEnabled))
                {
                    m_sellingForm.SetStairstepDiscountText(new List<string>());
                    return;
                }
            }

            //initialize lists
            Discount selectedDiscount = null;
            var saleItems = new List<SaleItem>();
            var removeItemsList = new List<SaleItem>();
            var percentageDiscounts = new List<SaleItem>();
            var discountStatusList = new List<string>();
            List<decimal> nextLevelDiscount = new List<decimal>();

            //categorize sale items
            foreach (var saleItem in m_currentSale.GetItems().Where(item => item.Session == session))
            {
                if (saleItem.IsDiscount && saleItem.Discount.AwardType == DiscountItem.AwardTypes.Automatic)
                {
                    //auto discounts
                    removeItemsList.Add(saleItem);
                }
                else if (!saleItem.IsCoupon)
                {
                    if (saleItem.IsDiscount)
                    {
                        if (saleItem.Discount is PercentDiscount)
                        {
                            //percent discounts
                            percentageDiscounts.Add(saleItem);
                        }
                    }
                    else
                    {
                        //sale items
                        saleItems.Add(saleItem);
                    }
                }
            }            
            

            //DE12993
            //remove auto discounts
            foreach (var autoDiscount in removeItemsList)
                CurrentSale.RemoveItem(autoDiscount);

            //if no sale items, or only coupons sale items, then no need to add discount.
            if (saleItems.Count == 0)
            {
                //reset next spend level tool tip
                m_sellingForm.SetStairstepDiscountText(discountStatusList);
                return;
            }
            
            //iterate through auto discounts
            foreach (var discount in m_autoDiscounts)
            {
                //check for null
                if (discount.DiscountItem == null)
                    continue;

                //check for player card required
                if (CurrentSale.Player == null && discount.DiscountItem.IsPlayerRequired)
                    continue;

                //DE13235: added a check for max usage = 0 (unlimited)
                //US4320: check for discount usage 
                if (discount.DiscountItem.IsPlayerRequired && CurrentSale.Player != null && discount.DiscountItem.MaximumUsePerSession > 0)
                {
                    //make sure discount id is in the dictionary
                    if (CurrentSale.Player.DiscountUsageDictionary.ContainsKey(discount.Id))
                    {
                        //check to see if used more than max per session
                        if (CurrentSale.Player.DiscountUsageDictionary[discount.Id] >= discount.DiscountItem.MaximumUsePerSession)
                            continue;
                    }
                }

                //US4617: (US4319) POS support discount schedule
                //check to see if the discount is scheduled
                if (!IsDiscountScheduleValid(discount, session, GamingDate))
                    continue;

                //if the discount is zero with no spend level, then unnecessary to add a discount of $0.00
                if (discount.DiscountItem.DiscountAmount == 0 && discount.DiscountItem.SpendLevels.Count == 0)
                    continue;

                if (!AutoDiscountHasRequiredPacks(session, discount.DiscountItem, saleItems))
                    continue;

                //create new discount
                Discount newDiscount = DiscountFactory.CreateDiscount(discount.DiscountItem);

                //sanity check. make sure discount was created properly
                if (newDiscount == null)
                    continue;

                //calculate sales total for spend levels
                var currentSalesTotal = CalculateTotalPriceWithRestrictions(saleItems, discount.DiscountItem.RestrictedProductIds, discount.DiscountItem.RestrictedPackageIds, discount.DiscountItem.IgnoreValidationsForIgnoredPackages);

                //US4616: (US4319) POS support min discount
                //check for minimum spend amount
                if (currentSalesTotal < discount.DiscountItem.MinimumSpend) //not enough spent for this discount
                {
                    //update the display information for the next stairstep if needed
                    if (discount.DiscountItem.SpendLevels.Count > 0)
                    {
                        //Get next spend level
                        var nextSpendLevel = GetNextSpendLevel(discount.DiscountItem, currentSalesTotal, selectedDiscount == null ? 0 : selectedDiscount.Amount);

                        //US4588
                        //Set the next spend level discount status
                        if (nextSpendLevel != null)
                        {
                            discountStatusList.Add(string.Format("Spend an additional {0:C}\r\n to receive {1:C} off",
                                nextSpendLevel.SpendMinValue - currentSalesTotal,
                                nextSpendLevel.SpendValue));
                            nextLevelDiscount.Add(nextSpendLevel.SpendValue);
                        }
                    }

                    continue;
                }

                //support for different discount types
                switch (discount.DiscountItem.Type)
                {
                    case DiscountType.Fixed:
                    {
                        CalculateSpendLevel(newDiscount, discount, currentSalesTotal, discountStatusList, nextLevelDiscount, selectedDiscount == null? 0 : selectedDiscount.Amount);

                        break;
                    }
                    case DiscountType.Percent:
                    {
                        //original total
                        var originalSalesTotal = currentSalesTotal;

                        //cast to percent discount
                        var autoPercentdiscount = discount as PercentDiscount;

                        //check for null
                        if (autoPercentdiscount == null)
                        {
                            break;
                        }

                        //calculate percentage
                        newDiscount.Amount = Math.Truncate(currentSalesTotal * autoPercentdiscount.DiscountPercentage) / 100;

                        CalculateSpendLevel(newDiscount, discount, originalSalesTotal, discountStatusList, nextLevelDiscount, selectedDiscount == null ? 0 : selectedDiscount.Amount);

                        break;
                    }
                    case DiscountType.Open: //we do not handle open auto discounts
                    break;
                }

                //US4615: (US4319) POS support max discount
                //check for max amount
                if (discount.DiscountItem.MaximumDiscount != 0 &&
                    discount.DiscountItem.MaximumDiscount < newDiscount.Amount)
                {
                    newDiscount.Amount = discount.DiscountItem.MaximumDiscount;
                }

                //check for partial discount
                if (discount.DiscountItem.AllowPartialDiscounts)
                {
                    var totalAfterDiscount = currentSalesTotal - newDiscount.Amount;
                    if (totalAfterDiscount < discount.DiscountItem.MinimumSpend)
                    {
                        newDiscount.Amount = newDiscount.Amount -
                                             (discount.DiscountItem.MinimumSpend - totalAfterDiscount);
                    }
                }

                //the new discount has to be greater than 0 in order for us to add it to the sale
                if (newDiscount.Amount == 0)
                    continue;

                //replace selected discount if null, or if new discount is larger
                if (selectedDiscount == null || newDiscount.Amount > selectedDiscount.Amount)
                    selectedDiscount = newDiscount;
            }


            //text for next spend level, remove any discounts less than or equal to the current
            if (selectedDiscount != null)
            {
                for (int x = 0, offset = 0; x < nextLevelDiscount.Count; x++)
                {
                    if (nextLevelDiscount[x] <= selectedDiscount.Amount)
                        discountStatusList.RemoveAt(x + offset--);
                }
            }

            m_sellingForm.SetStairstepDiscountText(discountStatusList);

            //add the best discount
            if (selectedDiscount != null)
            {
                CurrentSale.AddItem(session, selectedDiscount, 1, selectedDiscount.DiscountItem.IsPlayerRequired);

                //if added percent discount we may need to 
                //update other percent discounts in the sale
                if (selectedDiscount is PercentDiscount)
                    m_currentSale.UpdatePercentageDiscounts(session);
            }
        }

        /// <summary>
        /// Determines whether the discount is valid for the specified session.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="discount">The discount.</param>
        /// <returns></returns>
        private bool IsDiscountValid(SessionInfo session, Discount discount)
        {
            //check for null
            if (discount.DiscountItem == null)
                return false;

            //check for player card required
            if (CurrentSale.Player == null && discount.DiscountItem.IsPlayerRequired)
                return false;

            //US4617: (US4319) POS support discount schedule
            //check to see if the discount is scheduled
            if (!IsDiscountScheduleValid(discount, session, GamingDate))
                return false;

            //if the discount is zero with no spend level, then unnecessary to add a discount of $0.00
            if (discount.DiscountItem.DiscountAmount == 0 &&
                discount.DiscountItem.SpendLevels.Count == 0 &&
                discount.DiscountItem.AdvancedType != DiscountItem.AdvanceDiscountType.Quantity)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Calculates the spend level and updates next spend level status.
        /// </summary>
        /// <param name="newDiscount">The new discount.</param>
        /// <param name="original">The original.</param>
        /// <param name="salesTotal">The sales total.</param>
        /// <param name="discountStatusList">The discount status list.</param>
        /// <param name="nextLevelDiscount">The next level discount</param>
        private void CalculateSpendLevel(Discount newDiscount, Discount original, decimal salesTotal, List<string> discountStatusList, List<decimal> nextLevelDiscount, decimal currentDiscount)
        {                        
            //add spend level to the discount amount. Spend level is based off the total before discounts
            newDiscount.Amount += GetSpendLevelAmount(original.DiscountItem, salesTotal);

            //Get next spend level
            var nextSpendLevel = GetNextSpendLevel(original.DiscountItem, salesTotal, currentDiscount);

            //US4588
            //Set the next spend level discount status
            if (nextSpendLevel != null)
            {
                discountStatusList.Add(string.Format("Spend an additional {0:C}\r\n to receive {1:C} off",
                    nextSpendLevel.SpendMinValue - salesTotal,
                    nextSpendLevel.SpendValue));
                nextLevelDiscount.Add(nextSpendLevel.SpendValue);
            }
        }

        //US4323
        /// <summary>
        /// Gets the spend level amount.
        /// </summary>
        /// <param name="discount">The discount.</param>
        /// <param name="total">The total.</param>
        /// <returns></returns>
        private decimal GetSpendLevelAmount(DiscountItem discount, decimal total)
        {
            //iterate though the spend levls
            foreach (var spendLevel in discount.SpendLevels)
            {
                //if spend level applies
                if (total >= spendLevel.SpendMinValue && total <= spendLevel.SpendMaxValue)
                {
                    return spendLevel.SpendValue;
                }
            }

            return 0m;
        }

        //US4588
        //DE12993 - refactored and cleaned up code
        /// <summary>
        /// Gets the next spend level.
        /// </summary>
        /// <param name="discount">The discount.</param>
        /// <param name="total">The total.</param>
        /// <returns></returns>
        private DiscountItem.SpendLevel GetNextSpendLevel(DiscountItem discount, decimal total, decimal currentDiscount)
        {
            foreach (var spendLevel in discount.SpendLevels)
            {
                var difference = total - spendLevel.SpendMinValue;
        
                if (difference < 0 && spendLevel.SpendValue > currentDiscount)
                {
                    return spendLevel;
                }
            }

            return null;
        }

        //US4617: (US4319) POS support discount schedule
        /// <summary>
        /// Determines whether the discount schedule is valid
        /// </summary>
        /// <param name="discount">The discount.</param>
        /// <param name="session">The session.</param>
        /// <param name="date">The gaming date</param>
        /// <returns></returns>
        private bool IsDiscountScheduleValid(Discount discount, SessionInfo session, DateTime date)
        {
            //If null or empty list, then discount for all dates and sessions
            if (discount.DiscountItem.DiscountSchedule == null || discount.DiscountItem.DiscountSchedule.Count == 0)
            {
                return true;
            }

            //Determines if there is a schedule for the matching gaming day and session
            return discount.DiscountItem.DiscountSchedule.Any(schedule => 
                (schedule.SessionNumber == null || schedule.SessionNumber == session.SessionNumber) &&
                (schedule.DayOfWeek == null || schedule.DayOfWeek == date.DayOfWeek));
        }

        //US4323
        /// <summary>
        /// Calculates the total price with restrictions.
        /// </summary>
        /// <param name="saleItems">The sale items.</param>
        /// <param name="rescrictedProductIds">The rescricted product ids.</param>
        /// <param name="restrictedPackageIds">The rescricted package ids</param>
        /// <param name="exludeValidationsFromRestrictedPackages">Flag to determine to exclude the validation from the restricted package</param>
        /// <returns></returns>
        internal decimal CalculateTotalPriceWithRestrictions(List<SaleItem> saleItems, List<int> rescrictedProductIds, List<int> restrictedPackageIds, bool exludeValidationsFromRestrictedPackages)
        {
            var total = 0m;

            //iterate through sale items
            foreach (var item in saleItems)
            {
                //if package iterate through products
                if (item.IsPackageItem)
                {
                    //US4942
                    //check to see if the package is restricted
                    if (restrictedPackageIds.Contains(item.Package.Id))
                    {
                        //if the package has validation and the flag is set, then we also want to exlude the validation from the total
                        if (item.Package.PackageValidationValue > 0 && exludeValidationsFromRestrictedPackages)
                        {
                            //calculate total validations to exclude
                            total -= CalculateTotalValidationCostToExclude(saleItems, item);
                        }

                        //go to next sale item
                        continue;
                    }

                    //iterate through all products in the package and sum total price
                    total += item.Package.GetProducts().Where(product => !rescrictedProductIds.Contains(product.Id)).Sum(product => product.TotalPrice*item.Quantity);
                }
                else
                {
                    total += item.TotalPrice * item.Quantity;
                }
            }

            //return totals
            return total;
        }

        //US4942
        /// <summary>
        /// Calculates the total validation cost to exclude.
        /// </summary>
        /// <param name="saleItems">The sale items.</param>
        /// <param name="currentItem">The current item.</param>
        /// <returns></returns>
        private decimal CalculateTotalValidationCostToExclude(List<SaleItem> saleItems, SaleItem currentItem)
        {                            
            //check for max validation
            if (currentItem.Session.ValidationPackage.MaxQuantity > 0)
            {
                //we need to calculate total of all validations in the sale
                var totalValidationCost = saleItems.Where(i => i.IsPackageItem && i.Session == currentItem.Session).Sum(p => p.Package.PackageValidationValue);

                //total after excluding validations 
                var totalAfterRestrictedValidtionExcluded = totalValidationCost - currentItem.Package.PackageValidationValue;

                //get the max cost
                var maxValidationCost = currentItem.Session.ValidationPackage.MaxQuantity * ((PackageButton)currentItem.Session.DefaultValidationPackageButton).Package.Price;

                if (totalAfterRestrictedValidtionExcluded >= maxValidationCost)
                {
                    //max is reached, do not exclude any cost
                    return 0m;
                }

                return maxValidationCost - totalAfterRestrictedValidtionExcluded;
                
            }

            //total validaiton cost to exclude
            return currentItem.Package.PackageValidationValue;
        }


        internal bool AutoDiscountHasRequiredPacks(SessionInfo session, DiscountItem discount, List<SaleItem> saleItems)
        {
            bool result = true;

            if (discount.MinimumPacks != 0)
            {
                if (discount.MinimumPacksEligibleIds == null || discount.MinimumPacksEligibleIds.Count == 0) //any packages will do
                    result = discount.MinimumPacks <= saleItems.Where(e => e.Session == session && e.IsPackageItem && !e.IsValidationPackage).Sum(i => i.Quantity);
                else //check for specific packages
                    result = discount.MinimumPacks <= saleItems.Where(e => e.Session == session && e.IsPackageItem && discount.MinimumPacksEligibleIds.Contains(e.Package.Id)).Sum(i => i.Quantity);
            }

            return result;
        }

        internal void AddCouponToSale(PlayerComp coupon)
        {
            AddSaleItem(CurrentSession, 1, coupon);

            //update UI
            m_sellingForm.UpdateSaleInfo();
            m_sellingForm.UpdateSystemButtonStates();
        }

        internal void RemoveCouponFromSale(PlayerComp coupon)
        {
            SaleItem salesItemCoupon = m_currentSale.GetItems().ToList().Find(item => item.IsCoupon && item.Coupon.Id == coupon.Id);

            m_currentSale.RemoveItem(salesItemCoupon);

            //update percentage discounts and auto discounts
            m_currentSale.UpdatePercentageDiscounts(salesItemCoupon.Session);
            RemoveAutoDiscounts(salesItemCoupon);

            //update UI
            m_sellingForm.UpdateSaleInfo();
            m_sellingForm.UpdateSystemButtonStates();
        }

        /// <summary>
        /// Updates the coupons.
        /// </summary>
        /// <param name="couponsToAdd">The coupons to add.</param>
        internal void UpdateCoupons(List<PlayerComp> couponsToAdd)
        {
            var couponsToRemove = new List<SaleItem>();

            //iterate through all coupons in the sale
            foreach (var salesItemCoupon in m_currentSale.GetItems().Where(item => item.IsCoupon && item.Session == CurrentSession))
            {
                PlayerComp pc;

                //check to see if couponToAdd is already in the sale
                if (couponsToAdd.Count > 0 && (pc = couponsToAdd.FirstOrDefault(coupon => coupon.Id == salesItemCoupon.Coupon.Id)) != null)
                {
                    //remove coupons that are already added to the sale
                    couponsToAdd.Remove(pc);
                }
                else
                {
                    //the coupon is not in couponsToAdd, but it is in the sale,
                    //we need to remove the sale item. Add to couponsToRemove
                    couponsToRemove.Add(salesItemCoupon);
                }
            }

            //add coupons that are not already added
            foreach (var coupon in couponsToAdd)
            {
                AddSaleItem(CurrentSession, 1, coupon);
            }

            foreach (var saleItem in couponsToRemove)
            {
                m_currentSale.RemoveItem(saleItem);

                //update percentage discounts and auto discounts
                m_currentSale.UpdatePercentageDiscounts(saleItem.Session);
                RemoveAutoDiscounts(saleItem);
            }

            //update UI
            m_sellingForm.UpdateSaleInfo();
            m_sellingForm.UpdateSystemButtonStates();
        }

        /// <summary>
        /// Removes all coupons from sale.
        /// </summary>
        internal void RemoveAllCouponsFromSale()
        {
            //create seperate list, so removing doesn't modify the current collection
            var couponsToRemove = m_currentSale.GetItems().Where(item => item.IsCoupon).ToList();

            //remove the sale items
            foreach (var saleItem in couponsToRemove)
            {
                if (CurrentCouponForm != null)
                    CurrentCouponForm.SetAltPriceForPackage(saleItem.Coupon, false);

                RemoveCouponFromSale(saleItem.Coupon);
            }

            if (CurrentCouponForm != null)
                CurrentCouponForm.ClearSelectedCouponsList();
        }

        //US4323
        /// <summary>
        /// Removes the automatic discounts.
        /// </summary>
        /// <param name="removeItem">The remove item.</param>
        private void RemoveAutoDiscounts(SaleItem removeItem)
        {
            //remove any auto discount and re-add
            var autoDiscount = CurrentSale.GetItems().FirstOrDefault(item =>
                                    item.IsDiscount && 
                                    item.Discount.AwardType == DiscountItem.AwardTypes.Automatic &&
                                    removeItem.Session == item.Session);

            if (autoDiscount != null)
            {
                CurrentSale.RemoveItem(autoDiscount);
            }

            //adding auto discount re-calculates and will remove
            //any auto discount if they do not apply
            AddAutoDiscounts(removeItem.Session);
        }

        private void RemoveQuantityDiscountedItems(SaleItem removeItem)
        {
            //initialize lists
            var saleItems = new List<SaleItem>();
            var quantityDiscountedItems = new List<SaleItem>();

            //categorize sale items
            foreach (var saleItem in m_currentSale.GetItems().Where(item => item.Session == removeItem.Session))
            {
                //get a list of all regular sale items
                if (saleItem.IsPackageItem && saleItem.Package.AppliedDiscountId == 0)
                {
                    //do not add alt price packages
                    if (saleItem.Package.UseAltPrice)
                    {
                        continue;
                    }

                    //sale items
                    saleItems.Add(saleItem);
                }
                //get a list of the awarded packages from the quantity discounts
                //if the package was added due to a quantity discount, then the AppliedDiscountId will be set
                else if (saleItem.IsPackageItem && m_autoDiscountsByQuantity.Keys.ToList().Exists(item => item.Id == saleItem.Package.AppliedDiscountId))
                {
                    //discounted sale item
                    quantityDiscountedItems.Add(saleItem);
                }
            }
            
            //make sure we still qualify for the discount
            foreach (var quantityDiscountedItem in quantityDiscountedItems)
            {
                //find the discount
                var discount =
                    m_autoDiscountsByQuantity.Keys.FirstOrDefault(
                        item => item.Id == quantityDiscountedItem.Package.AppliedDiscountId);
                
                //if the buy package id is the same as the removed item, then we do not qualify for the discounted item anymore
                if (discount != null &&discount.DiscountItem.AdvancedQuantityDiscount.BuyPackageId == removeItem.Package.Id && removeItem.Package.AppliedDiscountId == 0)
                {
                    //remove item
                    m_currentSale.RemoveItem(quantityDiscountedItem);
                }
            }
        }

        /// <summary>
        /// Updates the alt price packages in the sale.
        /// </summary>
        /// <param name="packageId">The package identifier.</param>
        /// <param name="session">The session.</param>
        internal void MergeAltPricePackages(int packageId, SessionInfo session)
        {
            //get all the packages with coupon package id
            var packages = CurrentSale.GetItems().Where(item => item.IsPackageItem &&
                                                                item.Package.Id == packageId &&
                                                                item.Package.AppliedDiscountId == 0 &&
                                                                item.Session == session).ToList();

            //if more than one package, we need to merge
            if (packages.Count > 1)
            {
                //keep the original package (index 0), and sum all other packages to the original
                for (var i = 1; i < packages.Count; i++)
                {
                    packages[0].Quantity += packages[i].Quantity;
                }

                //reverse through list and remove all packages
                for (var i = packages.Count; i > 1; i--)
                {
                    CurrentSale.RemoveItem(packages[i - 1]);
                }
            }

            //update Alt Price
            if (packages.Count > 0)
            {
                packages[0].Package.UseAltPrice = false;
            }

            //we have to check for auto quantity discounts when we merge items back
            AddQuantityDiscounts(CurrentSession);
        }

        /// <summary>
        /// Removes the coupon.
        /// </summary>
        /// <param name="saleItem">The sale item.</param>
        private void RemoveCoupon(SaleItem saleItem)
        {
            var couponsToRemove = new List<SaleItem>();

            //iterate through all the coupons and add any coupons that do not meet min spend requirement to removal list.
            foreach (var coupon in CurrentSale.GetItems().Where(item=> item.IsCoupon && item.Coupon.MinimumSpendToQualify > 0))
            {
                //get the restricted sales total given the coupon restrictions
                var packageSaleItemsForThisSession = CurrentSale.GetItems().Where(item => item.Session == CurrentSession).Where(item => item.IsPackageItem).ToList();
                var restrictedSaleTotal = CalculateTotalPriceWithRestrictions(packageSaleItemsForThisSession, coupon.Coupon.RestrictedProductIds, coupon.Coupon.RestrictedPackageIds, coupon.Coupon.IgnoreValidationsForIgnoredPackages);

                //check min spend level
                if (coupon.Coupon.MinimumSpendToQualify > restrictedSaleTotal)
                {
                    //if the coupon is a alt-price coupon, then we need to merge the package
                    if (coupon.Coupon.CouponType == PlayerComp.CouponTypes.AltPricePackage)
                    {
                        //add to removal list
                        couponsToRemove.Add(coupon);
                        //merge
                        MergeAltPricePackages(coupon.Coupon.PackageID, saleItem.Session);
                    }
                    else
                    {
                        //add to removal list
                        couponsToRemove.Add(coupon);
                    }
                }
            }

            //remove any coupons
            foreach (var item in couponsToRemove)
            {
                m_currentSale.RemoveItem(item);
            }

            //if removing sale item with alt price or precent, then we also need to remove the coupon
            if(saleItem.IsPackageItem)
            {
                SaleItem coupon;

                //if it is using alt price, then we need to remove the coupon
                if (saleItem.Package.UseAltPrice)
                {
                    //find alt price coupon
                    coupon = CurrentSale.GetItems().FirstOrDefault(
                        item => item.IsCoupon &&
                                item.Session == saleItem.Session &&
                                item.Coupon.CouponType == PlayerComp.CouponTypes.AltPricePackage &&
                                item.Coupon.PackageID == saleItem.Package.Id);
                }
                else
                {
                    //we need to check all the coupons to see if
                    //there exists a percent coupon for this item.
                    //if so then we can remove the coupon
                    coupon = CurrentSale.GetItems().FirstOrDefault(
                        item => item.IsCoupon &&
                                item.Session == saleItem.Session &&
                                item.Coupon.CouponType == PlayerComp.CouponTypes.PercentPackage &&
                                item.Coupon.PackageID == saleItem.Package.Id);
                }

                //make sure coupon exists
                if (coupon != null)
                {
                    //remove coupon
                    m_currentSale.RemoveItem(coupon);
                }
            }
            else if (saleItem.IsCoupon) // handle removing Add+Use coupon items or if removing alt price coupon type, then we need to update the package alt price flag
            {
                if (!(m_couponForm != null && m_couponForm.HandleAddAndUsePackageRemoval(saleItem)))
                {
                    if (saleItem.Coupon.CouponType == PlayerComp.CouponTypes.AltPricePackage)
                        MergeAltPricePackages(saleItem.Coupon.PackageID, saleItem.Session);
                }
            }
        }

        /// <summary>
        /// Removes the specified line item from the sale.
        /// </summary>
        /// <param name="lineNumber">The line number of the item to 
        /// remove.</param>
        internal void RemoveSaleItem(int lineNumber)
        {
            try
            {
                // Remove the package and update the sales list.
                if (m_currentSale != null)
                {
                    SaleItem saleItem = m_sellingForm.GetSaleListRaw()[lineNumber] as SaleItem;

                    if (saleItem != null)
                        RemoveSaleItem(saleItem);
                }
            }
            catch (Exception ex)
            {
                ShowMessage(m_sellingForm, m_settings.DisplayMode, ex.Message);
            }
        }

        /// <summary>
        /// Removes the specified object item from the sale.
        /// </summary>
        /// <param name="lineNumber">The line number of the item to 
        /// remove.</param>
        internal void RemoveSaleItem(SaleItem saleItem)
        {
            try
            {
                // Remove the package and update the sales list.
                if (m_currentSale != null)
                {
                    //remove sale item from sale
                    m_currentSale.RemoveItem(saleItem);

                    //if item is associated to a quantity discount (BOGO)
                    //we need to update discounted items
                    if (saleItem.IsPackageItem)
                    {
                        RemoveQuantityDiscountedItems(saleItem);
                    }

                    //remove validations sales associated to the sale item
                    if (saleItem.IsPackageItem)
                        AddValidationSalesPerSession(saleItem.Package, saleItem.Session);

                    //remove coupons associated to the sale item
                    RemoveCoupon(saleItem);

                    //update percentage discounts
                    m_currentSale.UpdatePercentageDiscounts(saleItem.Session); //US4636

                    //only update auto discounts if they are not removing an auto discount
                    if (!saleItem.IsDiscount || saleItem.Discount.AwardType != DiscountItem.AwardTypes.Automatic)
                    {
                        //update auto discounts
                        RemoveAutoDiscounts(saleItem);
                    }

                    CurrentSale.DeviceFee = 0; //remove any device fee. UpdateSystemButtonStates() will fix the fee.

                    //update UI
                    m_sellingForm.UpdateSaleInfo();
                    m_sellingForm.UpdateSystemButtonStates();
                }
            }
            catch (Exception ex)
            {
                ShowMessage(m_sellingForm, m_settings.DisplayMode, ex.Message);
            }
        }

        //US4126
        /// <summary>
        /// Removes the sales items by session.
        /// </summary>
        /// <param name="session">The session.</param>
        internal void RemoveSalesItemsBySession(SessionInfo session)
        {
            if (m_currentSale == null)
            {
                return;
            }

            //get list of all items to remove
            var removeList = m_currentSale.GetItems().Where(saleItem => saleItem.Session == session).ToList();

            foreach (var removeItem in removeList)
            {
                m_currentSale.RemoveItem(removeItem);
            }

            m_sellingForm.UpdateSaleInfo();
            m_sellingForm.UpdateSystemButtonStates();

        }
        
        /// <summary>
        /// Sets the current currency.
        /// </summary>
        /// <param name="currency">The currency to set.</param>
        /// <param name="createSale">true to create a sale if none exists;
        /// otherwise false.</param>
        /// <exception cref="System.ArgumentNullException">currency is a null
        /// reference.</exception>
        internal void SetCurrentCurrency(Currency currency, bool createSale)
        {
            if(currency == null)
                throw new ArgumentNullException("currency");

            m_currentCurrency = currency;

            if(m_currentSale == null && createSale)
                StartSale(false);

            if(m_currentSale != null)
            {
                m_currentSale.SaleCurrency = m_currentCurrency;
                m_sellingForm.UpdateSaleInfo();
            }
        }
        
        /// <summary>
        /// Starts the proccess of making a sale.
        /// </summary>
        /// <param name="deviceId">The id of the device that will be 
        /// sold to.</param>
        /// <returns>First bool = false if tendering "back" pressed and coupon screen was used. 
        /// Second bool = false if there was an error requiring the sale to be re-entered.</returns>
        internal Tuple<bool, bool> TotalSale(int deviceId)
        {
            if (CurrentSale == null)
                return new Tuple<bool, bool>(false, true); //failed, don't re-enter sale

            if (Settings.UseSystemMenuForUnitSelection)
            {
                if (deviceId == 0 && CurrentSale.Device.Id != 0)
                    deviceId = CurrentSale.Device.Id;

                if (!CurrentSale.ThisDeviceIsCompatible(Device.FromId(deviceId)))
                {
                    ShowMessage(m_sellingForm, m_settings.DisplayMode, Resources.IncompatibleDevice);
                    return new Tuple<bool, bool>(false, false); //failed, re-enter sale
                }

                if (CurrentSale.HasElectronics) //check the card limits
                {
                    bool OK = true;
                    int maxCards = CurrentSale.CalculateMaxNumCards();

                    if (maxCards > Settings.TravelerMaxCards && deviceId == Device.Traveler.Id)
                        OK = false;

                    if (maxCards > Settings.TrackerMaxCards && deviceId == Device.Tracker.Id)
                        OK = false;

                    if (maxCards > Settings.FixedMaxCards && deviceId == Device.Fixed.Id)
                        OK = false;

                    if (maxCards > Settings.ExplorerMaxCards && deviceId == Device.Explorer.Id)
                        OK = false;

                    if (maxCards > Settings.Traveler2MaxCards && deviceId == Device.Traveler2.Id)
                        OK = false;

                    if (!OK)
                    {
                        ShowMessage(m_sellingForm, m_settings.DisplayMode, Resources.TooManyCardsForDevice);
                        return new Tuple<bool, bool>(false, false); //failed, re-enter sale
                    }
                }
                else
                {
                    deviceId = 0;
                }
            }

            // PDTS 571
            if (m_settings.Tender != TenderSalesMode.Off && m_settings.Tender != TenderSalesMode.Quick && !m_currentSale.IsReturn && m_currentSale.Quantity == 1) // Rally TA7465
                m_sellingState = SellingState.Tendering;
            else
                m_sellingState = SellingState.Finishing;

            // Set the device & fee.
            m_currentSale.Device = Device.FromId(deviceId);

            //set the selected unit name for the total area 
            m_sellingForm.SetSelectedDeviceName(m_currentSale.Device.Id);

            UpdateDeviceFeesAndTotals();

            // Prevent sale changes.
            m_sellingForm.UpdateMenuButtonStates();
            m_sellingForm.UpdateSystemButtonStates(); // PDTS 571

            // Check if we should use the flexible tendering dialog
            if (m_sellingState == SellingState.Tendering && m_settings.EnableFlexTendering && CurrentSale.Quantity == 1 && !IsB3Sale)
            {
                DialogResult dialogResult = DialogResult.Abort;

                if (WeAreANonAdvancedPOSKiosk && CurrentSale.CalculateAmountTendered() >= CurrentSale.CalculateTotal(false)) //paid in full already
                {
                    dialogResult = DialogResult.OK;
                }
                else
                {
                    FlexTenderingForm stf = new FlexTenderingForm(this, m_currentSale, m_sellingForm);
                    dialogResult = stf.ShowDialog(m_sellingForm.KioskForm == null ? this.m_sellingForm : (IWin32Window)m_sellingForm.KioskForm);
                    Application.DoEvents();
                    stf.Dispose();
                }

                if (dialogResult == DialogResult.OK)
                {
                    m_sellingState = SellingState.Finishing;
                    m_sellingForm.ProcessSale();
                }
                else
                {
                    if(!WeAreAPOSKiosk || WeAreAnAdvancedPOSKiosk)
                        m_sellingState = SellingState.Selling;

                    if (!WeAreANonAdvancedPOSKiosk && CurrentSale != null && CurrentSale.Player != null && CurrentCouponForm != null && CurrentCouponForm.LoadPlayerComp(true) && !CurrentSale.Player.UsedCouponScreen) //we need to go back to the coupon screen
                        return new Tuple<bool, bool>(false, true); //failed, don't re-enter sale
                }
            }

            // PDTS 583
            // Prevent sale changes.
            m_sellingForm.UpdateMenuButtonStates();
            m_sellingForm.UpdateSystemButtonStates(); // PDTS 571

            return new Tuple<bool, bool>(true, true); //total success
        }

        // FIX: DE2538 - Show a quantity sale counter and allow cancelling.
        /// <summary>
        /// Creates a thread to make the sale on the server and sets
        /// the StatusForm's settings.
        /// </summary>
        internal void StartAddSale()
        {
            // PDTS 583
            // Set the sale status form.
            // Rally TA7464
            m_statusForm.Cursor = Cursors.WaitCursor;
            m_statusForm.Message = Resources.WaitFormMakeSale;
            m_statusForm.Sale = m_currentSale;

            // Create the worker thread and run it.
            m_worker = new BackgroundWorker();
            m_worker.WorkerReportsProgress = true;
            m_worker.WorkerSupportsCancellation = true;
            m_worker.DoWork += new DoWorkEventHandler(m_currentSale.SendToServer);
            m_worker.ProgressChanged += new ProgressChangedEventHandler(m_statusForm.ReportProgress);
            m_worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(AddSaleCompleted);

            m_worker.RunWorkerAsync(m_statusForm);
        }

        /// <summary>
        /// Handles the event when the sale BackgroundWorker is complete.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The RunWorkerCompletedEventArgs object that 
        /// contains the event data.</param>
        private void AddSaleCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            m_statusForm.Message = string.Empty;

            // Set the error that occurred (if any).
            LastAsyncException = e.Error;

            if(e.Error == null || e.Error is POSPrintException)
            {
                // PDTS 583
                // Close the status form.
                if(!m_statusForm.OkButtonVisible)
                    m_statusForm.CloseForm();
                else
                {
                    m_statusForm.CancelButtonVisible = false;
                    m_statusForm.OkButtonEnabled = true;
                    m_statusForm.Cursor = Cursors.Default;
                }

                // Set the last sale (only if it's not a quantity sale).
                // PDTS 571
                if(m_currentSale.Quantity == 1)
                    m_lastSale = m_currentSale;
                else if(!m_statusForm.CancelButtonEnabled) // If the cancel button is disabled, then the sale was cancelled.
                    ShowMessage(m_sellingForm, m_settings.DisplayMode, string.Format(CultureInfo.CurrentCulture, Resources.QuantitySaleCancel, m_currentSale.SuccessfulSales));

                m_currentSale.RemoveTenders();

                // Clear the sale.
                ClearSale();
            }
            else
            {
                CurrentSale.Id = 0;
                CurrentSale.ReceiptNumber = 0;

                ResetDeviceAndPaper();
                ReenterSellingMode();
                m_statusForm.CloseForm(); // PDTS 583

                Log("Failed to finish the sale: " + e.Error.Message, LoggerLevel.Message);
            }
        }
        // END: DE2538

        /// <summary>
        /// Resets selected device, unit number, serial number, paper start numbers, and recalculates taxes, fees, and totals.
        /// </summary>
        internal void ResetDeviceAndPaper()
        {
            // Reset which device they selected.
            m_currentSale.Device = Device.FromId(0);
            m_currentSale.DeviceFee = 0M;
            m_sellingForm.SetSelectedDeviceName();

            // PDTS 583
            m_currentSale.UnitNumber = 0;
            m_currentSale.SerialNumber = string.Empty;

            // Rally TA5748
            m_currentSale.ClearStartNumbers();

            // Rally TA7465
            m_sellingForm.SetTaxesAndFees(m_currentSale.CalculateTaxes() + m_currentSale.CalculateFees());
            m_sellingForm.SetPrepaidTotal(m_currentSale.CalculatePrepaidAmount() + m_currentSale.CalculatePrepaidTaxTotal());
            m_sellingForm.SetTotal(m_currentSale.CalculateTotal(true));
        }

        /// <summary>
        /// Sets selling state to SELLING, resets quantity sale, and updates screen.
        /// </summary>
        internal void ReenterSellingMode()
        {
            // PDTS 571
            m_currentSale.Quantity = 1;
            m_sellingForm.SetQuantitySaleInfo();

            m_sellingState = SellingState.Selling;

            // Restore sale modification ability.
            m_sellingForm.UpdateMenuButtonStates();
            m_sellingForm.UpdateSystemButtonStates();

            // Update the display.
            m_sellingForm.UpdateSaleInfo();

            m_sellingForm.DisplayB3SessionMode(); //US4380: (US4337) POS: Display B3 Menu
        }

        /// US4028
        /// <summary>
        /// Allows for checking the max cards for the current sale
        /// </summary>
        /// <returns>false if the current sale exceedes the max card limit</returns>
        internal bool CheckMaxCards()
        {
            bool validCount = true;

            // The number of cards in the sale exceeds the card limit for a sale.
            if (m_currentSale.CalculateMaxNumCards() > Settings.MaxCardLimit)
            {
                validCount = false;

                // Reset which device they selected.
                m_currentSale.Device = Device.FromId(0);
                m_currentSale.DeviceFee = 0M;
                // PDTS 583
                m_currentSale.UnitNumber = 0;
                m_currentSale.SerialNumber = string.Empty;
                // Rally TA5748
                m_currentSale.ClearStartNumbers();

                // Rally TA7465
                m_sellingForm.SetTaxesAndFees(m_currentSale.CalculateTaxes() + m_currentSale.CalculateFees());
                m_sellingForm.SetPrepaidTotal(m_currentSale.CalculatePrepaidAmount() + m_currentSale.CalculatePrepaidTaxTotal());
                m_sellingForm.SetTotal(m_currentSale.CalculateTotal(true));

                // PDTS 571
                m_currentSale.Quantity = 1;
                m_sellingState = SellingState.Selling;

                // Restore sale modification ability.
                m_sellingForm.UpdateMenuButtonStates();
                m_sellingForm.UpdateSystemButtonStates();

                // Update the display.
                m_sellingForm.UpdateSaleInfo();

                Log("Failed to finish the sale: " + Resources.MaxCardLimitReached_2, LoggerLevel.Message);
            }

            return (validCount);
        }

        /// <summary>
        /// Attempts to open the cash drawer.
        /// </summary>
        internal void OpenCashDrawer(int drawer = 1)
        {
            if (WeAreAPOSKiosk)
                return;

            string printerName;
            byte[] drawerCode;
            
            try
            {
                lock(m_settings.SyncRoot)
                {
                    printerName = m_settings.ReceiptPrinterName;
                    drawerCode = drawer == 1 ? m_settings.DrawerCode : m_settings.Drawer2Code;
                }

                // Pop the drawer.
                if(drawerCode != null && drawerCode.Length > 0)
                {
                    Printer printer = new Printer(printerName);

                    // PDTS 1064
                    bool reading = m_magCardReader.ReadingCards;

                    if(reading)
                        m_magCardReader.EndReading();

                    printer.OpenDrawerCode = drawerCode;
                    printer.OpenDrawer();

                    if(reading)
                        m_magCardReader.BeginReading();
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Clears the current sale and resets the UI.
        /// </summary>
        internal void ClearSale(bool clearCurrentSale = true)
        {
            if (!HaveMenu)
                return;

            bool clearNVRAM = WeAreAPOSKiosk;

            if (Settings.EnableFlexTendering)
            {
                //void any tenders
                if (CurrentSale != null && CurrentSale.GetCurrentTenders().Count != 0)
                {
                    //void any tenders
                    FlexTenderingForm tender = new FlexTenderingForm(this, CurrentSale, this.m_sellingForm, true);
                    tender.ShowDialog(m_sellingForm.KioskForm == null? this.m_sellingForm : (IWin32Window)m_sellingForm.KioskForm);
                    Application.DoEvents();
                    tender.Dispose();

                    //refunded receipt, clear out NVRAM
                    if (WeAreAPOSKiosk)
                    {
                        //clear the total first to indicate there is no sale problem
                        SellingForm.WriteNVRAMUserDecimal(SellingForm.NVRAMUserDecimal.TransactionTotal, 0);
                        SellingForm.WriteNVRAMUserDecimal(SellingForm.NVRAMUserDecimal.AmountCollected, 0);
                        SellingForm.WriteNVRAMUserDecimal(SellingForm.NVRAMUserDecimal.AmountDispensed, 0);
                    }

                    //see if we have a successful tender or a tender with receipt text (if not, we don't need a receipt)
                    bool haveAtLeastOneValidTenderForReceipt = false;
                    List<TenderItem> tenders = CurrentSale.GetCurrentTenders();

                    foreach (TenderItem tend in CurrentSale.GetCurrentTenders())
                    {
                        if (!tend.ProcessingInfo.IsError || !string.IsNullOrEmpty(tend.SaleTenderInfo.AdditionalCustomerText))
                            haveAtLeastOneValidTenderForReceipt = true;
                    }

                    if (haveAtLeastOneValidTenderForReceipt)
                    {
                        if (!Settings.PrintIncompleteTransactionReceipts)
                        {
                            bool cardUsed = tenders != null && tenders.Count > 0 && tenders.Exists(t => t.SaleTenderInfo.TenderTypeID == TenderType.CreditCard || t.SaleTenderInfo.TenderTypeID == TenderType.DebitCard || t.SaleTenderInfo.TenderTypeID == TenderType.GiftCard);

                            if(cardUsed)
                                CurrentSale.PrintAbandonedSale();
                        }
                        else
                        {
                            CurrentSale.PrintAbandonedSale();
                        }
                    }

                    //receipt printed, clear out NVRAM
                    if (WeAreAPOSKiosk)
                    {
                        SellingForm.WriteNVRAMUserDecimal(SellingForm.NVRAMUserDecimal.RegisterReceiptID, 0);
                        clearNVRAM = false;
                    }
                }
            }

            //clear out NVRAM for power failure
            if (clearNVRAM)
                SellingForm.ClearNVRAMTransactionUserDecimals();

            if(clearCurrentSale)
                m_currentSale = null;

            m_sellingState = SellingState.NotSelling; // PDTS 571

            SetCurrentCurrency(m_defaultCurrency, false); // Rally TA7465

            m_sellingForm.SetPlayer();
            m_sellingForm.SetSelectedDeviceName();
            m_sellingForm.SetQuantitySaleInfo();
            m_sellingForm.UpdateSaleInfo();
            m_sellingForm.UpdateMenuButtonStates();
            m_sellingForm.UpdateSystemButtonStates(); // PDTS 571

            //clear tooltip
            m_sellingForm.SetStairstepDiscountText(null);

            m_sellingForm.UpdateMenuButtonPrices();
        }

        // PDTS 571
        /// <summary>
        /// Checks for quantity sale permission, whether the sale is valid for 
        /// a quantity sale, and prompts for the amount.
        /// </summary>
        /// <returns>true if everything is prepared; otherwise false.</returns>
        internal bool PrepareQuantitySale()
        {
            LastAsyncException = null;
            bool doQtySale = true;
            int initialSaleQty = (int)SellingForm.SellingKeypadValue;

            SellingForm.ClearSellingKeypadValue();

            // Check to see if there are any line items.
            if(IsSaleEmpty)
            {
                ShowMessage(m_sellingForm, m_settings.DisplayMode, Resources.NoSaleItems);
                doQtySale = false;
            }
            else
            {
                m_currentSale.Quantity = 1;

                // Check to see if there are any electronic pacakages.
                if(!m_currentSale.HasElectronics)
                {
                    ShowMessage(m_sellingForm, m_settings.DisplayMode, Resources.NoElecSaleItems);
                    doQtySale = false;
                }

                // Check to see if there are any barcoded paper packs.
                if (m_currentSale.HasBarcodedPaperBingo)
                {
                    ShowMessage(m_sellingForm, m_settings.DisplayMode, Resources.BarcodedPaperOnQuantitySale);
                    doQtySale = false;
                }
            }
            
            // Are they trying to sell to a player?
            if(doQtySale && m_currentSale.Player != null && m_settings.MaxPlayerQuantitySale < MinQuantitySaleQty)
            {
                ShowMessage(m_sellingForm, m_settings.DisplayMode, m_settings.EnableAnonymousMachineAccounts ? Resources.NoMachineQuantitySale : Resources.NoPlayerQuantitySale);
                doQtySale = false;
            }

            // Does the staff have permission?
            if(doQtySale && !m_currentStaff.CheckModuleFeature(EliteModule.POS, (int)POSFeature.QuantitySale))
            {
                try
                {
                    // Attempt to override.
                    if(!POSSecurity.TryOverride(this, m_waitForm, m_sellingForm, null, (int)EliteModule.POS, (int)POSFeature.QuantitySale))
                        doQtySale = false;
                }
                catch(ServerCommException e)
                {
                    LastAsyncException = e;
                    ServerCommFailed();
                    doQtySale = false;
                }
                catch(Exception e)
                {
                    ShowMessage(m_sellingForm, m_settings.DisplayMode, e.Message);
                    Log("Failed to retrieve a staff's quantity sale permissions: " + e.Message, LoggerLevel.Severe);
                    doQtySale = false;
                }
            }

            if(doQtySale)
            {
                int minQty = MinQuantitySaleQty;
                int maxQty = m_currentSale.Player != null ? m_settings.MaxPlayerQuantitySale : MaxQuantitySaleQty;

                KeypadForm qtyForm = new KeypadForm(this, m_settings.DisplayMode, false);
                qtyForm.NumberDisplayMode = Keypad.NumberMode.Integer;
                qtyForm.Message = Resources.QuantitySalePrompt;
                qtyForm.Option1Text = Resources.ButtonOk;
                qtyForm.Option2Text = Resources.ButtonCancel;
                qtyForm.GetKeypad().SetNewSmallButtonFont(new Font("Tahoma", 24, FontStyle.Bold));
                qtyForm.SetNewMessageFont(new Font("Tahoma", 14, FontStyle.Bold));
                qtyForm.ShowOptionButtons(true, true, true, false, false);
               
                int qty = 1;
                bool done = false;

                do
                {
                    if (minQty == maxQty)
                    {
                        qtyForm.InitialValue = minQty;
                        qtyForm.GetKeypad().DisableEntryKeys();
                    }
                    else
                    {
                        qtyForm.InitialValue = initialSaleQty;
                    }

                    qtyForm.ShowDialog(m_sellingForm);

                    if(qtyForm.Result == KeypadResult.Option1)
                    {
                        qty = Convert.ToInt32(qtyForm.Value);

                        // US2148
                        if(qty < minQty || qty > maxQty)
                        {
                            ShowMessage(m_sellingForm, m_settings.DisplayMode, string.Format(CultureInfo.CurrentCulture,
                                             Resources.InvalidQuantity, minQty, maxQty));
                        }
                        else
                        {
                            done = true;
                            m_sellingState = SellingState.QuantitySelling;
                        }
                    }
                    else
                    {
                        qty = 1;
                        done = true;
                        doQtySale = false;
                    }

                } while(!done);

                qtyForm.Dispose();
                m_currentSale.Quantity = (short)qty;
            }

            m_sellingForm.SetQuantitySaleInfo(IsSaleEmpty? 1 : m_currentSale.Quantity); //show it on the screen

            return doQtySale;
        }

/*        /// <summary>
        /// Adds all the line items from the last sale to the current sale.
        /// </summary>
        internal void RepeatLastSale()
        {
            if(m_lastSale == null)
                throw new POSException(Resources.NoLastSale);

            // If the current sale has a player, save it.
            Player player = null;

            if(m_currentSale != null && m_currentSale.Player != null)
                player = m_currentSale.Player;

            // Clear the existing sale if needed.
            ClearSale();

            // Was the last sale a return?
            StartSale(m_lastSale.IsReturn);

            // Set the player if we had one saved.
            if(player != null)
                m_currentSale.SetPlayer(player, true, true);

            try
            {
                m_currentSale.DuplicateSale(m_lastSale, m_sellingForm);

                // Update UI.
                m_sellingForm.SetPlayer();
                m_sellingForm.UpdateMenuButtonStates();
                m_sellingForm.UpdateSaleInfo();

                if(m_currentSale.ItemCount > 0)
                    m_sellingForm.SelectSaleItem(m_currentSale.ItemCount - 1);

                m_sellingForm.UpdateSystemButtonStates(); // PDTS 571
            }
            catch(ServerCommException)
            {
                ServerCommFailed();
            }
        }
*/
        /// <summary>
        /// Creates a thread to print the last receipt.
        /// </summary>
        internal void StartReprintLastReceipt()
        {
            if(m_lastReceipt == null)
                throw new POSException(Resources.NoLastReceipt);

            RunWorker(Resources.WaitFormPrinting, ReprintLastReceipt, null, ReprintLastReceiptCompleted);
        }

        /// <summary>
        /// Prints the receipt from the last sale.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The DoWorkEventArgs object that 
        /// contains the event data.</param>
        private void ReprintLastReceipt(object sender, DoWorkEventArgs e)
        {
            SetupThread();

            // FIX: TA4779
            // Load the receipt settings.
            string receiptPrinterName = null;
            string printerName = null;
            bool printCardsToGlobal = false;
            float pointSize = 5F;
            Tuple<bool, int, bool, bool> printPoints; //print points, interface ID, external rating, we do the rating
            bool printSigLine;
            short copies = 1;
            CBBPlayItSheetPrintMode playItSheetMode = CBBPlayItSheetPrintMode.Off; // Rally US505
            CBBPlayItSheetType playItSheetType; // Rally TA8688
            bool printFaces;
            // US2139
            bool printPtsRedeemed = true;
            // Rally TA5749 - Add support for print only start numbers option.
            PrintCardNumberMode printNumsMode;
            bool printCustomerAndMerchantReceipts = false;

            lock(m_settings.SyncRoot)
            {
                receiptPrinterName = m_settings.ReceiptPrinterName;
                printerName = m_settings.PrinterName;
                printCardsToGlobal = m_settings.PrintFacesToGlobalPrinter;
                pointSize = m_settings.CardFacePointSize;
                printPoints = m_settings.PlayerPointPrintingInfo;
                printSigLine = m_settings.PrintSignatureLine;
                copies = m_settings.ReceiptCopies;
                playItSheetMode = m_settings.CBBPlayItSheetPrintMode; // Rally US505
                playItSheetType = m_settings.CBBPlayItSheetType; // Rally TA8688
                printNumsMode = m_settings.PrintCardNumbers; 
                printFaces = m_settings.PrintCardFaces;
                printPtsRedeemed = m_settings.PrintPointsRedeemed;
                printCustomerAndMerchantReceipts = m_settings.PrintDualReceiptsForNonCashSales;
            }
            // END: TA4779

            // Do we have a receipt printer to print to?
            if(receiptPrinterName == null)
                return;

            // Create the receipt printer object.
            Printer receiptPrinter = new Printer(receiptPrinterName);

            // This is a reprint.
            m_lastReceipt.IsReprint = true;

            bool haveCreditCardTender = false;

            if (m_lastReceipt.SaleTenders != null)
            {
                foreach (SaleTender st in m_lastReceipt.SaleTenders)
                {
                    if (st.TenderTypeID == TenderType.CreditCard || st.TenderTypeID == TenderType.DebitCard)
                        haveCreditCardTender = true;
                }
            }

            printCustomerAndMerchantReceipts = printCustomerAndMerchantReceipts && haveCreditCardTender;

            // Are we printing the card faces to another printer?
            if (printFaces && printCardsToGlobal && printerName != null)
            {
                // Create the global printer and card receipt.
                Printer globalPrinter = new Printer(printerName);
                BingoCardReceipt cardReceipt = new BingoCardReceipt();

                // Get the information from the original receipt.
                cardReceipt.Number = m_lastReceipt.Number;
                cardReceipt.GamingDate = m_lastReceipt.GamingDate;
                cardReceipt.PrintLotto = m_lastReceipt.PrintLotto;
                cardReceipt.BingoSessions = m_lastReceipt.BingoSessions;
                cardReceipt.PointSize = pointSize;

                // Print both receipts.
                m_lastReceipt.Print(receiptPrinter, m_settings.PrintPlayerIdentityAsAccount, m_settings.PrintPlayerID, printPoints, printSigLine, printPtsRedeemed, printNumsMode, false, copies);

                if (printCustomerAndMerchantReceipts)
                    m_lastReceipt.Print(receiptPrinter, m_settings.PrintPlayerIdentityAsAccount, m_settings.PrintPlayerID, printPoints, printSigLine, printPtsRedeemed, printNumsMode, false, copies, true);

                cardReceipt.Print(globalPrinter, copies);
            }
            else
            {
                m_lastReceipt.Print(receiptPrinter, m_settings.PrintPlayerIdentityAsAccount, m_settings.PrintPlayerID, printPoints, printSigLine, printPtsRedeemed, printNumsMode, printFaces, copies);

                if(printCustomerAndMerchantReceipts)
                    m_lastReceipt.Print(receiptPrinter, m_settings.PrintPlayerIdentityAsAccount, m_settings.PrintPlayerID, printPoints, printSigLine, printPtsRedeemed, printNumsMode, printFaces, copies, true);
            }
            // END: TA5749
            // END: US2139

            // Rally US505
            // Rally TA8688
            if(playItSheetMode != CBBPlayItSheetPrintMode.Off && LastReceiptHasPlayIt)
            {
                if(playItSheetType == CBBPlayItSheetType.Card || playItSheetType == CBBPlayItSheetType.Line)
                    PrintPlayItSheet(m_lastReceipt.RegisterReceiptId, printerName, true);
                else
                    PrintPlayItSheet(m_lastReceipt.RegisterReceiptId, receiptPrinterName, true);
            }
        }

        /// <summary>
        /// Handles the event when the print last receipt BackgroundWorker is 
        /// complete.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The RunWorkerCompletedEventArgs object that 
        /// contains the event data.</param>
        private void ReprintLastReceiptCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // Set the error that occurred (if any).
            LastAsyncException = e.Error;

            // Close the wait form.
            m_waitForm.CloseForm();
        }

        //US5590: View Receipt clicked in player managment
        /// <summary>
        /// View Receipt Clicked handler for player management
        /// </summary>
        /// <param name="receiptNumber"></param>
        private void ViewReceiptClicked(string receiptNumber)
        {
            //view receipt number
            var forceAuthorizationAtPos = Settings.ForceAuthorizationOnVoidsAtPOS;
            StartViewReceipts(0, forceAuthorizationAtPos, forceAuthorizationAtPos || !Settings.EnablePresales, receiptNumber); //5711: Added new parameter for refunds
        }

        //5711: Added new parameter for refunds
        /// <summary>
        /// Displays the view receipts screen in the Receipt Mgmt module.
        /// </summary>
        internal void StartViewReceipts(int authorizedStaffID = 0, bool doNotAllowVoiding = false, bool doNotAllowRefunding = false, string receiptNumber = "")
        {
            try
            {
                if(IsReceiptMgmtInitialized)
                {
                    CanUpdateMenus = false; // PTDS 964
                    
                    //US5590: view recept added receipt number
                    //5711: Added new parameter for refunds
                    m_receiptManager.ShowTouchViewReceipts(authorizedStaffID, doNotAllowVoiding, doNotAllowRefunding, receiptNumber, m_bank.Id);
                    CanUpdateMenus = true; // PTDS 964

                    if(m_receiptManager.LastAsyncException is ServerCommException)
                        ServerCommFailed();
                }
            }
            catch(ServerCommException)
            {
                ServerCommFailed();
            }
            catch(Exception e)
            {
                ShowMessage(m_sellingForm, m_settings.DisplayMode, e.Message);
            }
        }

        /// <summary>
        /// Reprint a receipt through Receipt Mgmt module.
        /// </summary>
        internal void StartReprintReceipt(int registerReceiptID)
        {
            try
            {
                if (IsReceiptMgmtInitialized)
                {
                    CanUpdateMenus = false;
                    m_receiptManager.KioskRecoveryReprintReceipt(registerReceiptID);
                    CanUpdateMenus = true;

                    if (m_receiptManager.LastAsyncException is ServerCommException)
                        ServerCommFailed();
                }
            }
            catch (ServerCommException)
            {
                ServerCommFailed();
            }
            catch (Exception e)
            {
                ShowMessage(m_sellingForm, m_settings.DisplayMode, e.Message);
            }
        }

        /// <summary>
        /// Void tenders and reprint receipt through Receipt Mgmt module.
        /// </summary>
        internal decimal StartVoidTendersAndReprintReceipt(int registerReceiptID)
        {
            decimal result = 0M;

            try
            {
                if (IsReceiptMgmtInitialized)
                {
                    CanUpdateMenus = false;

                    result = m_receiptManager.KioskRecoveryVoidTendersAndReprintReceipt(registerReceiptID);
                    CanUpdateMenus = true;

                    if (m_receiptManager.LastAsyncException is ServerCommException)
                        ServerCommFailed();
                }
            }
            catch (ServerCommException)
            {
                ServerCommFailed();
            }
            catch (Exception e)
            {
                ShowMessage(m_sellingForm, m_settings.DisplayMode, e.Message);
            }

            return result;
        }

        internal void ShowPaperExchange()
        {
            try
            {
                if (IsReceiptMgmtInitialized)
                {
                    CanUpdateMenus = false; // PTDS 964
                    m_receiptManager.ShowPaperExchangeForm();
                    CanUpdateMenus = true; // PTDS 964

                    if (m_receiptManager.LastAsyncException is ServerCommException)
                        ServerCommFailed();
                }
            }
            catch (ServerCommException)
            {
                ServerCommFailed();
            }
            catch (Exception e)
            {
                ShowMessage(m_sellingForm, m_settings.DisplayMode, e.Message);
            }
        }

        // FIX: US1955
        /// <summary>
        /// Creates a thread to cash out a player's credit.
        /// </summary>
        internal void StartCreditCashOut()
        {
            // First check to see if the staff has permission to cash out 
            // credit.
            int loginNum = 0;
            string magCardNum = string.Empty;
            string password = null;

            if(!m_currentStaff.CheckModuleFeature(EliteModule.POS, (int)POSFeature.CreditCashOut))
            {
                if(!POSSecurity.TryOverride(this, m_waitForm, m_sellingForm, null, (int)EliteModule.POS, (int)POSFeature.CreditCashOut, out loginNum, out magCardNum, out password))
                    return;
            }

            // Package the arguments.
            object[] args = new object[3];
            args[0] = loginNum;
            args[1] = magCardNum;
            args[2] = password;

            RunWorker(m_settings.EnableAnonymousMachineAccounts ? Resources.WaitFormGettingMachine : Resources.WaitFormGettingPlayer,
                      SendCreditCashOut, args, CreditCashOutComplete);
        }

        /// <summary>
        /// Updates the existing player.
        /// </summary>
        /// <param name="player">The player.</param>
        internal void UpdateExistingPlayer(Player player)
        {
            m_playerCenter.UpdateExistingPlayer(player);
        }

        /// <summary>
        /// Sets the discount tool tip.
        /// </summary>
        /// <param name="messages">The messages.</param>
        internal void SetDiscountToolTip(List<string> messages)
        {
            m_sellingForm.SetStairstepDiscountText(messages);
        }

        /// <summary>
        /// Cashes out a player's credit on the server.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The DoWorkEventArgs object that 
        /// contains the event data.</param>
        private void SendCreditCashOut(object sender, DoWorkEventArgs e)
        {
            SetupThread();

            BackgroundWorker worker = (BackgroundWorker)sender;
            DisplayMode displayMode;

            lock(m_settings.SyncRoot)
            {
                displayMode = m_settings.DisplayMode;
            }

            // Unpack the arguments.
            object[] args = (object[])e.Argument;
            int loginNum = (int)args[0];
            string magCardNum = (string)args[1];
            string password = (string)args[2];

            int playerId = 0;

            lock(m_currentSale.SyncRoot)
            {
                playerId = m_currentSale.Player.Id;
            }

            // Update the current player's credit.
            GetCreditMessage creditMsg = new GetCreditMessage(playerId);

            try
            {
                creditMsg.Send();
            }
            catch(ServerCommException)
            {
                throw; // Don't repackage the ServerCommException
            }
            catch(Exception ex)
            {
                // TTP 50114
                throw new POSException(string.Format(CultureInfo.CurrentCulture, Resources.GetCreditFailed, ServerExceptionTranslator.FormatExceptionMessage(ex)), ex);
            }

            // Rally TA7465
            // Ask how much we should cash out.
            decimal cashOutAmount = 0M;
            Currency cashOutCurrency = null;

            if(m_waitForm.InvokeRequired)
            {
                CashOutPromptDelegate prompt = new CashOutPromptDelegate(PromptForCashOutAmount);
                object[] delegateArgs = new object[] { m_waitForm, displayMode, creditMsg.RefundableCredit, cashOutAmount, cashOutCurrency };
                m_waitForm.Invoke(prompt, delegateArgs);

                cashOutAmount = (decimal)delegateArgs[3];
                cashOutCurrency = (Currency)delegateArgs[4];
            }
            else
                PromptForCashOutAmount(m_waitForm, displayMode, creditMsg.RefundableCredit, out cashOutAmount, out cashOutCurrency);

            // Attempt the cash out.
            worker.ReportProgress(0, Resources.WaitFormCashOut);
            CashOutPlayerMessage cashOutMsg = new CashOutPlayerMessage();

            cashOutMsg.PlayerId = playerId;

            // FIX: DE1930
            cashOutMsg.BankId = m_bank.Id; // Rally TA7465
            // END: DE1930

            cashOutMsg.AuthStaffId = 0;
            cashOutMsg.AuthLoginNum = loginNum;
            cashOutMsg.AuthMagCardNum = magCardNum;

            if(!string.IsNullOrEmpty(password))
                cashOutMsg.AuthPassword = SecurityHelper.HashPassword(password);
            else
                cashOutMsg.AuthPassword = null;

            // Convert the amount the user specified to the system default.
            if(cashOutCurrency != DefaultCurrency)
                cashOutMsg.CashOutAmount = cashOutCurrency.ConvertFromThisCurrencyToDefaultCurrency(cashOutAmount);
            else
                cashOutMsg.CashOutAmount = cashOutAmount;

            cashOutMsg.CurrencyISO = cashOutCurrency.ISOCode;

            try
            {
                cashOutMsg.Send();
            }
            catch(ServerCommException)
            {
                throw; // Don't repackage the ServerCommException
            }
            catch(ServerException ex) // FIX: DE1930
            {
                switch((CashOutPlayerReturnCode)ex.ReturnCode)
                {
                    case CashOutPlayerReturnCode.AuthStaffNotFound:
                    case CashOutPlayerReturnCode.IncorrectAuthPassword:
                        throw new POSException(Resources.InvalidStaff);

                    case CashOutPlayerReturnCode.InactiveAuthStaff:
                        throw new POSException(Resources.StaffInactive);

                    case CashOutPlayerReturnCode.AuthPasswordHasExpired:
                        throw new POSException(Resources.StaffPasswordExpired);

                    case CashOutPlayerReturnCode.NotAuthorized:
                        throw new POSException(Resources.StaffNotAuthorized);

                    // US1955
                    case CashOutPlayerReturnCode.AuthStaffLocked:
                        throw new POSException(Resources.StaffLocked);

                    case CashOutPlayerReturnCode.NoCreditAccount:
                        // TTP 50114
                        throw new POSException(string.Format(CultureInfo.CurrentCulture, Resources.CashOutFailed, Resources.CashOutErrorNoAccount));

                    case CashOutPlayerReturnCode.InvalidAmount:
                        throw new POSException(Resources.CashOutErrorInvalidAmount);
                        
                    default:
                        throw new POSException(string.Format(CultureInfo.CurrentCulture, Resources.CashOutFailed, ServerExceptionTranslator.FormatExceptionMessage(ex)), ex);

                }
            }  // END: DE1930
            catch(Exception ex)
            {
                // TTP 50114
                throw new POSException(string.Format(CultureInfo.CurrentCulture, Resources.CashOutFailed, ServerExceptionTranslator.FormatExceptionMessage(ex)), ex);
            }

            // Update the player's new credit balances.
            lock(m_currentSale.SyncRoot)
            {
                m_currentSale.Player.RefundableCredit = cashOutMsg.RefundableCredit;
                m_currentSale.Player.NonRefundableCredit = cashOutMsg.NonRefundableCredit;
            }

            // Open the cash drawer.
            OpenCashDrawer();

            // Print out the reciept.
            worker.ReportProgress(0, Resources.WaitFormPrinting);

            try
            {
                // FIX: DE1930
                PrintCashOutReceipt(cashOutMsg.TransactionNumber, GamingDate, creditMsg.RefundableCredit, cashOutMsg.CashOutAmount, cashOutAmount, cashOutCurrency);
                // END: DE1930
            }
            catch(Exception ex)
            {
                throw new POSPrintException(string.Format(CultureInfo.CurrentCulture, Resources.CashOutButNoReceipt, ex.Message), ex);
            }
        }
        // END: US1955

        // PDTS 693
        /// <summary>
        /// A delegate that allows cross-thread calls to PromptForCashOutAmount 
        /// on the PointOfSale class.
        /// </summary>
        /// <param name="owner">Any object that implements IWin32Window 
        /// that represents the top-level window that will own any modal 
        /// dialog boxes.</param>
        /// <param name="displayMode">The DisplayMode for the form to 
        /// use.</param>
        /// <param name="refundableCredit">The player's refundable 
        /// credit.</param>
        /// <param name="amount">The cash out amount entered by the
        /// user.</param>
        /// <param name="cashOutCurrency">The currency of the cash out entered 
        /// by the user.</param>
        private delegate void CashOutPromptDelegate(IWin32Window owner, DisplayMode displayMode, decimal refundableCredit, out decimal amount, out Currency cashOutCurrency);

        /// <summary>
        /// Displays a form asking for the cash out amount.
        /// </summary>
        /// <param name="owner">Any object that implements IWin32Window 
        /// that represents the top-level window that will own any modal 
        /// dialog boxes.</param>
        /// <param name="displayMode">The DisplayMode for the form to 
        /// use.</param>
        /// <param name="refundableCredit">The player's refundable 
        /// credit.</param>
        /// <param name="amount">The cash out amount entered by the
        /// user.</param>
        /// <param name="cashOutCurrency">The currency of the cash out entered 
        /// by the user.</param>
        /// <exception cref="GTI.Modules.POS.Business.POSUserCancelException">
        /// The user pressed the cancel button.</exception>
        private void PromptForCashOutAmount(IWin32Window owner, DisplayMode displayMode, decimal refundableCredit, out decimal amount, out Currency cashOutCurrency)
        {
            // PDTS 693
            // TTP 50433
            CreditCashOutForm amountForm = new CreditCashOutForm(this, displayMode);
            amountForm.Balance = refundableCredit;

            amountForm.ShowDialog(owner);

            KeypadResult result = amountForm.Result;
            amount = (decimal)amountForm.Value;
            cashOutCurrency = amountForm.CashOutCurrency;

            amountForm.Dispose();

            if(result == KeypadResult.Option3)  
                throw new POSUserCancelException();
        }

        public void UpdateDeviceFeesAndTotals()
        {
            // US2018
            if (m_currentSale.ChargeDeviceFee)
            {
                if (m_currentSale.Device.Id == Device.Fixed.Id)
                    m_currentSale.DeviceFee = m_currentOp.FixedDeviceFee;
                else if (m_currentSale.Device.Id == Device.Traveler.Id)
                    m_currentSale.DeviceFee = m_currentOp.TravelerDeviceFee;
                else if (m_currentSale.Device.Id == Device.Tracker.Id)
                    m_currentSale.DeviceFee = m_currentOp.TrackerDeviceFee;
                else if (m_currentSale.Device.Id == Device.Explorer.Id) // Rally TA7729
                    m_currentSale.DeviceFee = m_currentOp.ExplorerDeviceFee;
                else if (m_currentSale.Device.Id == Device.Traveler2.Id) // PDTS 964, Rally US765
                    m_currentSale.DeviceFee = m_currentOp.Traveler2DeviceFee;
                else if (m_currentSale.Device.Id == Device.Tablet.Id)//TA12504
                    m_currentSale.DeviceFee = m_currentOp.TabletDeviceFee;
            }

            // Rally TA7465
            m_sellingForm.SetTaxesAndFees(m_currentSale.CalculateTaxes() + m_currentSale.CalculateFees());
            m_sellingForm.SetPrepaidTotal(m_currentSale.CalculatePrepaidAmount() + m_currentSale.CalculatePrepaidTaxTotal());
            m_sellingForm.SetTotal(m_currentSale.CalculateTotal(true));
            m_sellingForm.SetPointsEarned(m_currentSale.CalculateTotalEarnedPoints() + m_currentSale.CalculatePointsEarnedFromQualifyingSubtotal()); 
        }

        /// <summary>
        /// Prints out a CreditCashOutReceipt with the specified parameters and 
        /// the current system state.
        /// </summary>
        /// <param name="transactionNumber">The receipt number for the cash
        /// out.</param>
        /// <param name="gamingDate">The gaming date of the cash out.</param>
        /// <param name="originalAmount">The player's refundable credit 
        /// balance before the cash out.</param>
        /// <param name="cashOutAmount">The amount that was cashed out in the
        /// system's default currency.</param>
        /// <param name="cashOutAmountConverted">The amount that was cashed out
        /// in the cash out currency.</param>
        /// <param name="cashOutCurrency">The currency used to cash
        /// out.</param>
        private void PrintCashOutReceipt(int transactionNumber, DateTime gamingDate, decimal originalAmount, decimal cashOutAmount, decimal cashOutAmountConverted, Currency cashOutCurrency)
        {
            string printerName = null;
            short copies = 2;
            bool machineAccounts;
            bool staffFirstNameOnly;

            lock(m_settings.SyncRoot)
            {
                printerName = m_settings.ReceiptPrinterName;
                copies = m_settings.PayoutReceiptCopies; // TTP 50114

                // TTP 50114
                machineAccounts = m_settings.EnableAnonymousMachineAccounts;

                // TTP 50097
                // Have an option to print only the staff's first name.
                staffFirstNameOnly = m_settings.PrintStaffFirstNameOnly;
            }

            if(printerName != null)
            {
                Printer printer = new Printer(printerName);

                // Create the receipt object and fill in.
                CreditCashOutReceipt receipt = new CreditCashOutReceipt();
                receipt.Number = transactionNumber;
                receipt.GamingDate = gamingDate;

                lock(m_currentStaff.SyncRoot)
                {
                    receipt.Cashier = m_currentStaff.FirstName;

                    // TTP 50097
                    if(!staffFirstNameOnly)
                        receipt.Cashier += " " + m_currentStaff.LastName;
                }

                lock(m_currentSale.SyncRoot)
                {
                    // TTP 50114
                    receipt.MachineAccount = machineAccounts;
                    receipt.PlayerId = m_currentSale.Player.Id;
                    receipt.PlayerName = m_currentSale.Player.ToString(false);
                    receipt.NewBalance = m_currentSale.Player.RefundableCredit;
                }

                receipt.OriginalBalance = originalAmount;
                receipt.CashOutAmount = cashOutAmount;
                receipt.DefaultCurrency = DefaultCurrency.ISOCode;
                receipt.CashOutCurrency = cashOutCurrency.ISOCode;
                receipt.ExchangeRate = cashOutCurrency.ExchangeRate;
                receipt.CashOutAmountConverted = cashOutAmountConverted;

                // FIX: TA4779
                lock(m_settings.SyncRoot)
                {
                    receipt.OperatorName = CurrentOperator.Name;
                    receipt.OperatorAddress1 = CurrentOperator.Address1;
                    receipt.OperatorAddress2 = CurrentOperator.Address2;
                    receipt.OperatorCityStateZip = CurrentOperator.City + ", " + CurrentOperator.State + " " + CurrentOperator.Zip;
                    receipt.OperatorPhoneNumber = CurrentOperator.Phone;

                    receipt.OperatorHeaderLine1 = m_settings.ReceiptHeaderLine1;
                    receipt.OperatorHeaderLine2 = m_settings.ReceiptHeaderLine2;
                    receipt.OperatorHeaderLine3 = m_settings.ReceiptHeaderLine3;
                    receipt.OperatorFooterLine1 = m_settings.ReceiptFooterLine1;
                    receipt.OperatorFooterLine2 = m_settings.ReceiptFooterLine2;
                    receipt.OperatorFooterLine3 = m_settings.ReceiptFooterLine3;
                }
                // END: TA4779

                receipt.MachineId = m_machineId;

                receipt.Print(printer, copies);
            }
        }
        // END: TA7465

        /// <summary>
        /// Handles the event when the credit cash out BackgroundWorker is 
        /// complete.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The RunWorkerCompletedEventArgs object that 
        /// contains the event data.</param>
        private void CreditCashOutComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            // Set the error that occurred (if any).
            LastAsyncException = e.Error;

            // Close the wait form.
            m_waitForm.CloseForm();
        }

        /// <summary>
        /// Tells the Unit Mgmt module to show the unit assignment form.
        /// </summary>
        internal void StartUnitAssignment()
        {
            try
            {
                if(IsUnitMgmtInitialized && m_currentSale != null && m_currentSale.Player != null)
                {
                    CanUpdateMenus = false; // PDTS 964

                    if(m_unitMgmt.ShowUnitAssignment(m_sellingForm, m_currentSale.Player))
                        ServerCommFailed();
                    else
                        CanUpdateMenus = true; // PDTS 964
                }
                else
                    Log("Failed to start unit assignment because the module isn't initialized or there is no player.", LoggerLevel.Severe);
            }
            catch(ServerCommException)
            {
                ServerCommFailed();
            }
            catch(Exception e)
            {
                ShowMessage(m_sellingForm, m_settings.DisplayMode, e.Message);
                CanUpdateMenus = true; // PDTS 964
            }
        }

        // Rally US1648
        /// <summary>
        /// Creates a thread to print the register report.
        /// </summary>
        /// <param name="closing">true if the closing report should be printed;
        /// otherwise the sales report will be printed.</param>
        internal void StartPrintRegisterReport(bool closing)
        {
            RunWorker(Resources.WaitFormPrinting, PrintRegisterReport, closing, PrintRegisterReportComplete);
        }
        
        /// <summary>
        /// Prints the register report to the current receipt printer.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The DoWorkEventArgs object that 
        /// contains the event data.</param>
        private void PrintRegisterReport(object sender, DoWorkEventArgs e)
        {
            SetupThread();

            bool closing = (bool)e.Argument;

            // US1808
            string printerName, drive, dir, dbServer, dbName, dbUser, dbPass;
            int printByPackage;

            // Set the options.
            lock(m_settings.SyncRoot)
            {
                printerName = m_settings.ReceiptPrinterName;
                drive = m_settings.ClientInstallDrive;
                dir = m_settings.ClientInstallRootDir;
                dbServer = m_settings.DatabaseServer;
                dbName = m_settings.DatabaseName;
                dbUser = m_settings.DatabaseUser;
                dbPass = m_settings.DatabasePassword;
                printByPackage = m_settings.PrintRegisterSalesByPackage ? 1 : 0;
            }

            if(printerName != null)
            {
                // PDTS 584 - Portable POS Support
                // Check to see which report we are going to print.
                int reportId;
                Printer printer = new Printer(printerName);

                if(printer.Using58mmPaper)
                    reportId = closing ? (int)ReportIDs.POS_MiniRegisterCloseReport : (int)ReportIDs.POS_MiniRegisterSalesReport;
                else
                    reportId = closing ? (int)ReportIDs.POS_RegisterCloseReport : (int)ReportIDs.POS_RegisterSalesReport;

                // Ask the server for the report.
                // TTP 50114
                GetReportMessage reportMsg = new GetReportMessage(reportId);

                try
                {
                    reportMsg.Send();
                }
                catch(ServerCommException)
                {
                    throw; // Don't repackage the ServerCommException
                }
                catch(Exception ex)
                {
                    throw new POSException(string.Format(CultureInfo.CurrentCulture, Resources.GetReportFailed, ServerExceptionTranslator.FormatExceptionMessage(ex)), ex);
                }

                // Save the report to a temporary file.
                string path = drive + dir + @"\Temp";

                if(!Directory.Exists(path))
                    Directory.CreateDirectory(path);                

                path += @"\TempClosingReport.rpt";

                FileStream fileStream = new FileStream(path, FileMode.Create);
                BinaryWriter writer = new BinaryWriter(fileStream);

                writer.Write(reportMsg.ReportFile);
                writer.Flush();
                writer.Close();

                // Clean up.
                // TTP 50135
                writer = null;
                fileStream.Dispose();
                fileStream = null;

                // Open the report back up in a Crystal Reports document.
                CrystalDecisions.CrystalReports.Engine.ReportDocument reportDoc = new CrystalDecisions.CrystalReports.Engine.ReportDocument();
                reportDoc.Load(path);

                // Set the database connection information.
                foreach(CrystalDecisions.Shared.IConnectionInfo connInfo in reportDoc.DataSourceConnections)
                {
                    connInfo.SetConnection(dbServer, dbName, dbUser, dbPass);
                }

                // Set the parameters.
                reportDoc.SetParameterValue("@StartDate", GamingDate);
                reportDoc.SetParameterValue("@EndDate", GamingDate);

                lock(m_currentOp.SyncRoot)
                {
                    reportDoc.SetParameterValue("@OperatorID", m_currentOp.Id);
                }

                lock(m_currentStaff.SyncRoot)
                {
                    reportDoc.SetParameterValue("@StaffID", m_currentStaff.Id);
                }

                if (Settings.EnableActiveSalesSession)
                {
                    reportDoc.SetParameterValue("@Session", CurrentSession.SessionNumber);
                }
                else
                {
                    reportDoc.SetParameterValue("@Session", 0);    
                }
                
                if (m_currentOp.CashMethodID == (int)CashMethod.ByMachinePOS)
                {
                    reportDoc.SetParameterValue("@MachineID", m_machineId); // FIX: DE1930
                }
                else
                {
                    reportDoc.SetParameterValue("@MachineID", 0); // FIX: 
                }
                if(!closing)
                    reportDoc.SetParameterValue("@ByPackage", printByPackage);
                // END: US1808

                // Print it out.
                reportDoc.PrintOptions.PrinterName = printerName;
                reportDoc.PrintOptions.PaperSize = CrystalDecisions.Shared.PaperSize.DefaultPaperSize;
                //reportDoc.PrintOptions.DissociatePageSizeAndPrinterPaperSize = true;
                reportDoc.PrintToPrinter(1, true, 0, 0);

                // Clean up.
                reportDoc.Dispose(); // TTP 50135
            }
        }
        // END: US1648

        /// <summary>
        /// Handles the event when the print register report 
        /// BackgroundWorker is complete.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The RunWorkerCompletedEventArgs object that 
        /// contains the event data.</param>
        private void PrintRegisterReportComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            // Set the error that occurred (if any).
            LastAsyncException = e.Error;

            // Close the wait form.
            m_waitForm.CloseForm();
        }

        /// <summary>
        /// Starts the process of transferring a unit.
        /// </summary>
        internal void TransferUnit()
        {
            if(!m_settings.AllowElectronicSales || !IsUnitMgmtInitialized)
                return;

            CanUpdateMenus = false; // PDTS 964

            if(m_unitMgmt.TransferUnit(m_sellingForm, true))
                ServerCommFailed();
            else
                CanUpdateMenus = true; // PDTS 964
        }

        // TTP 50137
        /// <summary>
        /// Creates a thread to get the current bank.
        /// </summary>
        internal void StartGetCurrentBank()
        {
            RunWorker(Resources.WaitFormGettingBank, DoGetCurrentBank, null, GetCurrentBankComplete);
        }

        // Rally TA7465
        /// <summary>
        /// Get the current bank amount from the server.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The DoWorkEventArgs object that 
        /// contains the event data.</param>
        private void DoGetCurrentBank(object sender, DoWorkEventArgs e)
        {
            SetupThread();

            // FIX: DE1930
            e.Result = GetCurrentBankAmount();
            // END: DE1930
        }

        /// <summary>
        /// Handles the event when the get current bank BackgroundWorker is 
        /// complete.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The RunWorkerCompletedEventArgs object that 
        /// contains the event data.</param>
        private void GetCurrentBankComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            // Set the error that occurred (if any).
            LastAsyncException = e.Error;

            // If there was no error, update the bank.
            if(e.Error == null)
                m_bank = (Bank)e.Result;
            // END: TA7465

            // Close the wait form.
            m_waitForm.CloseForm();
        }
        // Rally TA7465

        // Rally TA7465
        /// <summary>
        /// Creates a thread to adjust the current bank.
        /// </summary>
        /// <param name="amount">The amounts to adjust the bank by.</param>
        internal void StartAdjustCurrentBank(IDictionary<BankCurrency, decimal> amounts)
        {
            RunWorker(Resources.WaitFormAdjustingBank, AdjustCurrentBank, amounts, AdjustCurrentBankComplete);
        }

        /// <summary>
        /// Adjusts the current bank amounts on the server.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The DoWorkEventArgs object that 
        /// contains the event data.</param>
        private void AdjustCurrentBank(object sender, DoWorkEventArgs e)
        {
            SetupThread();

            // Unbox the argument.
            BackgroundWorker worker = (BackgroundWorker)sender;
            IDictionary<BankCurrency, decimal> amounts = (IDictionary<BankCurrency, decimal>)e.Argument;

            int issueTrans = 0, dropTrans = 0;
            DateTime issueDate = DateTime.MinValue, dropDate = DateTime.MinValue;

            // FIX: DE1930
            try
            {
                int count = 0;
                BankIssueMessage issueMsg;
                                                                                 
                var sessionNumber = Settings.EnableActiveSalesSession ? CurrentSession.SessionNumber : 0;

                //check for presale session
                //if the selected session is a presale, then want to find the active sale session
                if (CurrentSession.IsPreSale && ActiveSalesSession != null)
                {
                    sessionNumber = ActiveSalesSession.SessionNumber;
                }

                lock(m_currentStaff.SyncRoot)
                {
                    issueMsg = new BankIssueMessage(0, m_bank.Id, m_currentStaff.Id, null, BankType.Master, (short)sessionNumber); //DE13731
                }

                // Find all the positive adjustments.
                foreach(KeyValuePair<BankCurrency, decimal> pair in amounts)
                {
                    BankCurrency adjustAmount = new BankCurrency(pair.Key);

                    if(pair.Value > 0)
                    {
                        adjustAmount.Total = pair.Value;
                        count++;
                    }
                    else
                        adjustAmount.Total = 0M;

                    issueMsg.AddCurrency(adjustAmount);
                }

                if(count > 0)
                {
                    issueMsg.Send();
                    issueTrans = issueMsg.CashTransactionId;
                    issueDate = issueMsg.TransactionDate;
                }

                count = 0;
                BankDropMessage dropMsg = new BankDropMessage(0, m_bank.Id, false);

                // Find all the negative adjustments.
                foreach(KeyValuePair<BankCurrency, decimal> pair in amounts)
                {
                    BankCurrency adjustAmount = new BankCurrency(pair.Key);

                    if(pair.Value < 0)
                    {
                        adjustAmount.Total = pair.Value * decimal.MinusOne;
                        count++;
                    }
                    else
                        adjustAmount.Total = 0M;

                    dropMsg.AddCurrency(adjustAmount);
                }

                if(count > 0)
                {
                    dropMsg.Send();
                    dropTrans = dropMsg.CashTransactionId;
                    dropDate = dropMsg.TransactionDate;
                }
            }
            catch(ServerCommException)
            {
                throw; // Don't repackage the ServerCommException
            }
            catch(Exception ex)
            {
                throw new POSException(string.Format(CultureInfo.CurrentCulture, Resources.AdjustBankFailed, ServerExceptionTranslator.FormatExceptionMessage(ex)), ex);
            }

            // Update the POS's bank amount value.
            m_bank = GetCurrentBankAmount();
            // END: DE1930

            // Open the cash drawer.
            OpenCashDrawer();

            // Print out the reciept.
            worker.ReportProgress(0, Resources.WaitFormPrinting);

            try
            {
                PrintBankReceipt(issueTrans, issueDate, dropTrans, dropDate, amounts);
            }
            catch(Exception ex)
            {
                throw new POSPrintException(string.Format(CultureInfo.CurrentCulture, Resources.AdjustBankButNoReceipt, ex.Message), ex);
            }
        }

        /// <summary>
        /// Prints a bank adjustment receipt.
        /// </summary>
        /// <param name="issueTrans">The transaction id of the bank issue 
        /// (or 0 if there was no issue).</param>
        /// <param name="issueDate">The date of the transaction of the bank 
        /// issue (or DateTime.MinValue if there was no issue).</param>
        /// <param name="dropTrans">The transaction id of the bank drop
        /// (or 0 if there was no drop).</param>
        /// <param name="dropDate">The date of the transaction of the bank 
        /// drop (or DateTime.MinValue if there was no drop).</param>
        /// <param name="amounts">The amounts the bank was adjusted by.</param>
        private void PrintBankReceipt(int issueTrans, DateTime issueDate, int dropTrans, DateTime dropDate, IDictionary<BankCurrency, decimal> amounts)
        {
            // Load the receipt settings.
            string receiptPrinterName = null;
            short copies = 1;
            bool staffFirstNameOnly;

            lock(m_settings.SyncRoot)
            {
                receiptPrinterName = m_settings.ReceiptPrinterName;
                copies = m_settings.ReceiptCopies;
                staffFirstNameOnly = m_settings.PrintStaffFirstNameOnly;
            }

            // Do we have a receipt printer to print to or anything to print?
            if(receiptPrinterName == null || (issueTrans == 0 && dropTrans == 0) || 
               amounts == null || amounts.Count == 0)
                return;

            // Create the receipt printer and receipt objects.
            Printer receiptPrinter = new Printer(receiptPrinterName);
            BankAdjustmentReceipt receipt;

            if(issueTrans > 0)
            {
                receipt = new BankAdjustmentReceipt();
                receipt.TransactionType = TransactionType.InitialBankIssue;

                // FIX: TA4779
                lock(m_settings.SyncRoot)
                {
                    receipt.OperatorName = CurrentOperator.Name;
                    receipt.OperatorAddress1 = CurrentOperator.Address1;
                    receipt.OperatorAddress2 = CurrentOperator.Address2;
                    receipt.OperatorCityStateZip = CurrentOperator.City + ", " + CurrentOperator.State + " " + CurrentOperator.Zip;
                    receipt.OperatorPhoneNumber = CurrentOperator.Phone;

                    receipt.OperatorHeaderLine1 = m_settings.ReceiptHeaderLine1;
                    receipt.OperatorHeaderLine2 = m_settings.ReceiptHeaderLine2;
                    receipt.OperatorHeaderLine3 = m_settings.ReceiptHeaderLine3;
                }
                // END: TA4779

                receipt.MachineId = m_machineId;
                receipt.GamingDate = GamingDate;
                receipt.IssuedDate = issueDate;

                lock(m_currentStaff.SyncRoot)
                {
                    receipt.IssuedTo = m_currentStaff.FirstName;

                    if(!staffFirstNameOnly)
                        receipt.IssuedTo += " " + m_currentStaff.LastName;
                }

                // Only report on the positive values.
                foreach(KeyValuePair<BankCurrency, decimal> pair in amounts)
                {
                    if(pair.Value > 0)
                        receipt.AddAmount(pair.Key.ISOCode, pair.Value);
                    else
                        receipt.AddAmount(pair.Key.ISOCode, 0M);
                }

                receipt.Print(receiptPrinter, copies);
            }

            if(dropTrans > 0)
            {
                receipt = new BankAdjustmentReceipt();
                receipt.TransactionType = TransactionType.BankDrop;

                // FIX: TA4779
                lock(m_settings.SyncRoot)
                {
                    receipt.OperatorName = CurrentOperator.Name;
                    receipt.OperatorAddress1 = CurrentOperator.Address1;
                    receipt.OperatorAddress2 = CurrentOperator.Address2;
                    receipt.OperatorCityStateZip = CurrentOperator.City + ", " + CurrentOperator.State + " " + CurrentOperator.Zip;
                    receipt.OperatorPhoneNumber = CurrentOperator.Phone;

                    receipt.OperatorHeaderLine1 = m_settings.ReceiptHeaderLine1;
                    receipt.OperatorHeaderLine2 = m_settings.ReceiptHeaderLine2;
                    receipt.OperatorHeaderLine3 = m_settings.ReceiptHeaderLine3;
                }
                // END: TA4779

                receipt.MachineId = m_machineId;
                receipt.GamingDate = GamingDate;
                receipt.IssuedDate = dropDate;

                lock(m_currentStaff.SyncRoot)
                {
                    receipt.IssuedTo = m_currentStaff.FirstName;

                    if(!staffFirstNameOnly)
                        receipt.IssuedTo += " " + m_currentStaff.LastName;
                }

                // Only report on the negative values.
                foreach(KeyValuePair<BankCurrency, decimal> pair in amounts)
                {
                    if(pair.Value < 0)
                        receipt.AddAmount(pair.Key.ISOCode, pair.Value * decimal.MinusOne);
                    else
                        receipt.AddAmount(pair.Key.ISOCode, 0M);
                }

                receipt.Print(receiptPrinter, copies);
            }
        }

        /// <summary>
        /// Handles the event when the adjust current bank BackgroundWorker is 
        /// complete.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The RunWorkerCompletedEventArgs object that 
        /// contains the event data.</param>
        private void AdjustCurrentBankComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            // Set the error that occurred (if any).
            LastAsyncException = e.Error;

            // Close the wait form.
            m_waitForm.CloseForm();
        }
        // END: TA7465

        // Rally US505
        /// <summary>
        /// Prints out a Crystal Ball Play-It sheet for the specified sale. 
        /// Assumes the Play-It sheet has already been downloaded to the local disk.
        /// </summary>
        /// <param name="registerReceiptId">The id of the sale to print CBB
        /// cards for.</param>
        /// <param name="printerName">The name of the printer to print
        /// to.</param>
        /// <param name="isReprint">Whether this Play-It sheet is being
        /// reprinted.</param>
        internal void PrintPlayItSheet(int registerReceiptId, string printerName, bool isReprint)
        {
            // Rally TA8688
            if(!string.IsNullOrEmpty(printerName))
            {
                string drive, dir, dbServer, dbName, dbUser, dbPass;
                CBBPlayItSheetType sheetType;
                CBBPlayItSheetPrintMode printMode;

                // Set the options.
                lock(m_settings.SyncRoot)
                {
                    drive = m_settings.ClientInstallDrive;
                    dir = m_settings.ClientInstallRootDir;
                    dbServer = m_settings.DatabaseServer;
                    dbName = m_settings.DatabaseName;
                    dbUser = m_settings.DatabaseUser;
                    dbPass = m_settings.DatabasePassword;
                    printMode = m_settings.CBBPlayItSheetPrintMode;
                    sheetType = m_settings.CBBPlayItSheetType;
                }

                string path = m_settings.ClientInstallDrive + m_settings.ClientInstallRootDir + @"\Temp" + PlayItSheetFileName;

                // Open the report back up in a Crystal Reports document.
                CrystalDecisions.CrystalReports.Engine.ReportDocument playItSheet = new CrystalDecisions.CrystalReports.Engine.ReportDocument();
                playItSheet.Load(path);

                // Set the database connection information.
                foreach(CrystalDecisions.Shared.IConnectionInfo connInfo in playItSheet.DataSourceConnections)
                {
                    connInfo.SetConnection(dbServer, dbName, dbUser, dbPass);
                }

                playItSheet.SetParameterValue("@ReceiptID", registerReceiptId);
                playItSheet.SetParameterValue("@Current", !isReprint);
                playItSheet.SetParameterValue("@PrintType", (int)printMode);

                // Print it out.
                playItSheet.PrintOptions.PrinterName = printerName;
                playItSheet.PrintOptions.PaperSize = CrystalDecisions.Shared.PaperSize.DefaultPaperSize;
//                if(sheetType == CBBPlayItSheetType.CardThermal || sheetType == CBBPlayItSheetType.LineThermal || sheetType == CBBPlayItSheetType.VerticalLineThermal)
//                    playItSheet.PrintOptions.DissociatePageSizeAndPrinterPaperSize = true;

                playItSheet.PrintToPrinter(1, true, 0, 0);

                // Clean up.
                playItSheet.Dispose();
            }
        }

        // Rally US493
        /// <summary>
        /// Checks to see if there are any statuses on the player that need to
        /// be shown.
        /// </summary>
        /// <param name="player">The player to check for alerts.</param>
        public void CheckForAlerts(Player player)
        {
            if(WeAreNotAPOSKiosk && player != null && player.ActiveStatusList != null)
            {
                string alertText = string.Empty;

                foreach(PlayerStatus status in player.ActiveStatusList)
                {
                    if(status.IsAlert)
                        alertText += status.Name + Environment.NewLine;
                }

                if(!string.IsNullOrEmpty(alertText))
                    ShowMessage(m_sellingForm, m_settings.DisplayMode, string.Format(CultureInfo.CurrentCulture, Resources.PlayerAlerts, alertText));
            }
        }

        // FIX: DE1930
        /// <summary>
        /// Creates a thread to close the current bank.
        /// </summary>
        internal void StartCloseBank()
        {
            RunWorker(Resources.WaitFormClosingBank, CloseBank, null, CloseBankComplete);
        }

        // Rally TA7465
        /// <summary>
        /// Closes the current bank on the server.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The DoWorkEventArgs object that 
        /// contains the event data.</param>
        private void CloseBank(object sender, DoWorkEventArgs e)
        {
            SetupThread();

            BankDropMessage closeMsg = new BankDropMessage(0, m_bank.Id, true);

            foreach(BankCurrency currency in m_bank.Currencies)
            {
                closeMsg.AddCurrency(currency);
            }

            try
            {
                closeMsg.Send();
            }
            catch(ServerCommException)
            {
                throw; // Don't repackage the ServerCommException
            }
            catch(Exception ex)
            {
                throw new POSException(string.Format(CultureInfo.CurrentCulture, Resources.CloseBankFailed, ServerExceptionTranslator.FormatExceptionMessage(ex)), ex);
            }
        }

        /// <summary>
        /// Handles the event when the close bank BackgroundWorker is 
        /// complete.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The RunWorkerCompletedEventArgs object that 
        /// contains the event data.</param>
        private void CloseBankComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            // Set the error that occurred (if any).
            LastAsyncException = e.Error;

            // Close the wait form.
            m_waitForm.CloseForm();
        }
        // END: DE1930

        /// <summary>
        /// Creates a thread to check the status from a Star printer.
        /// </summary>
        internal void StartGetCashDrawerStatus()
        {
            RunWorker(Resources.CheckingCashDrawer, GetCashDrawerStatus, null, GetCashDrawerStatusComplete);
        }

        /// <summary>
        /// Gets the status from a Star printer.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The DoWorkEventArgs object that 
        /// contains the event data.</param>
        private void GetCashDrawerStatus(object sender, DoWorkEventArgs e)
        {
            SetupThread();
            GetStarPrinterStatus(true);
        }

        /// <summary>
        /// Handles the event when the GetCashDrawerStatus BackgroundWorker is 
        /// complete.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The RunWorkerCompletedEventArgs object that 
        /// contains the event data.</param>
        private void GetCashDrawerStatusComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            // Close the wait form.
            m_waitForm.CloseForm();
        }

        // Rally TA5748 - Add Play with Paper support.
        /// <summary>
        /// Creates a thread to get start numbers for the current sale.
        /// </summary>
        /// <param name="products">A list of products to get start numbers
        /// for.</param>
        /// <exception cref="System.ArgumentNullException">products is null or
        /// empty.</exception>
        internal void StartGetStartNumbers(IList<ProductStartNumbers> products)
        {
            if(products == null || products.Count == 0)
                throw new ArgumentNullException("products");

            RunWorker(Resources.WaitFormGettingStartNumbers, GetStartNumbers, products, GetStartNumbersComplete);
        }

        /// <summary>
        /// Predicts the next start numbers for the current sale.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The DoWorkEventArgs object that 
        /// contains the event data.</param>
        private void GetStartNumbers(object sender, DoWorkEventArgs e)
        {
            SetupThread();

            // Unbox the argument.
            IList<ProductStartNumbers> products = (IList<ProductStartNumbers>)e.Argument;

            GetPaperStartNumbersMessage numsMsg = new GetPaperStartNumbersMessage();

            foreach(ProductStartNumbers product in products)
            {
                numsMsg.AddProduct(product.SessionPlayedId, product.ProductId, (short)product.StartNumbers.Count);
            }

            try
            {
                numsMsg.Send();
            }
            catch(ServerCommException)
            {
                throw; // Don't repackage the ServerCommException
            }
            catch(ServerException ex)
            {
                // FIX: DE4037 - Incorrect error given for perm lib. problem.
                if((GetPaperStartNumbersReturnCode)ex.ReturnCode == GetPaperStartNumbersReturnCode.MissingPermLib)
                    throw new POSException(Resources.GetStartNumbersErrorMissingPerm);
                else
                    throw new POSException(string.Format(CultureInfo.CurrentCulture, Resources.GetStartNumbersFailed, ServerExceptionTranslator.FormatExceptionMessage(ex)), ex);
            }
            catch(Exception ex)
            {
                throw new POSException(string.Format(CultureInfo.CurrentCulture, Resources.GetStartNumbersFailed, ServerExceptionTranslator.FormatExceptionMessage(ex)), ex);
            }

            e.Result = new List<ProductStartNumbers>(numsMsg.StartNumbers);
        }

        /// <summary>
        /// Handles the event when the get start numbers BackgroundWorker is 
        /// complete.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The RunWorkerCompletedEventArgs object that 
        /// contains the event data.</param>
        private void GetStartNumbersComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            // Set the error that occurred (if any).
            LastAsyncException = e.Error;

            if(LastAsyncException == null)
            {
                // Update the UI with the start numbers received.
                LastStartNumbers = (List<ProductStartNumbers>)e.Result;
            }

            // Close the wait form.
            m_waitForm.CloseForm();
        }

        /// <summary>
        /// Creates a thread to check start numbers for the current sale.
        /// </summary>
        /// <param name="products">A list of products to check start numbers
        /// for.</param>
        /// <exception cref="System.ArgumentNullException">products is null or
        /// empty.</exception>
        internal void StartCheckStartNumbers(IList<ProductStartNumbers> products)
        {
            if(products == null || products.Count == 0)
                throw new ArgumentNullException("products");

            RunWorker(Resources.WaitFormCheckingStartNumbers, CheckStartNumbers, products, CheckStartNumbersComplete);
        }

        /// <summary>
        /// Checks the start numbers for the current sale.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The DoWorkEventArgs object that 
        /// contains the event data.</param>
        private void CheckStartNumbers(object sender, DoWorkEventArgs e)
        {
            SetupThread();

            // Unbox the argument.
            IList<ProductStartNumbers> products = (IList<ProductStartNumbers>)e.Argument;

            CheckPaperStartNumbersMessage checkMsg = new CheckPaperStartNumbersMessage();

            foreach(ProductStartNumbers product in products)
            {
                checkMsg.AddStartNumbers(product);
            }

            try
            {
                checkMsg.Send();
            }
            catch(ServerCommException)
            {
                throw; // Don't repackage the ServerCommException
            }
            catch(Exception ex)
            {
                throw new POSException(string.Format(CultureInfo.CurrentCulture, Resources.CheckStartNumbersFailed, ServerExceptionTranslator.FormatExceptionMessage(ex)), ex);
            }

            e.Result = new List<ProductStartNumbers>(checkMsg.CheckedNumbers);
        }

        /// <summary>
        /// Handles the event when the check start numbers BackgroundWorker is
        /// complete.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The RunWorkerCompletedEventArgs object that 
        /// contains the event data.</param>
        private void CheckStartNumbersComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            // Set the error that occurred (if any).
            LastAsyncException = e.Error;

            if(LastAsyncException == null)
            {
                // Update the UI with the start numbers/statuses received.
                LastStartNumbers = (List<ProductStartNumbers>)e.Result;
            }

            // Close the wait form.
            m_waitForm.CloseForm();
        }
        // END: TA5748

        // TODO Revist Super Pick methods.
        /*
        /// <summary>
        /// Creates a thread to find super pick numbers on the server and sets
        /// the WaitForm's settings.
        /// </summary>
        /// <param name="waitForm">The wait form to be used while 
        /// finding super pick numbers.</param>
        /// <param name="authenticationCode">The authentication code to 
        /// search for.</param>
        public void FindSuperPicks(WaitForm waitForm, uint authenticationCode)
        {
            // Set the current wait form used.
            m_waitForm = waitForm;

            // Set the wait message.
            m_waitForm.Message = Resources.WaitFormFindingSuperPicks;

            // Create the worker thread and run it.
            m_worker = new BackgroundWorker();
            m_worker.WorkerReportsProgress = true;
            m_worker.WorkerSupportsCancellation = false;
            m_worker.DoWork += new DoWorkEventHandler(GetSuperPickList);
            m_worker.ProgressChanged += new ProgressChangedEventHandler(m_waitForm.ReportProgress);
            m_worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(FindSuperPickListComplete);
            m_worker.RunWorkerAsync(authenticationCode);
        }

        /// <summary>
        /// Gets the super pick list from the server.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The DoWorkEventArgs object that 
        /// contains the event data.</param>
        private void GetSuperPickList(object sender, DoWorkEventArgs e)
        {
            // Make sure member variables access is thread safe.

            // Set the language.
            if(m_settings.ForceEnglish)
                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");

            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

            // Wait a couple of ticks to let the wait form display.
            System.Threading.Thread.Sleep(100);

            // Unbox the argument.
            uint authenticationCode = (uint)e.Argument;

            if(authenticationCode != 0)
            {
                ValidateSuperPickTicketMessage valSPMsg = new ValidateSuperPickTicketMessage(authenticationCode);

                try
                {
                    valSPMsg.Send();
                }
                catch (ServerException ex)
                {
                    if (valSPMsg.ReturnCode == (int)ValidateSuperTicketReturnCodes.WrongGamingDate)
                        throw new POSException(string.Format(Resources.WrongGamingDate, FormatExceptionMessage(ex)), ex);

                    if (valSPMsg.ReturnCode == (int)ValidateSuperTicketReturnCodes.ReceiptVoided)
                        throw new POSException(string.Format(Resources.ReceiptVoided, FormatExceptionMessage(ex)), ex);

                    if (valSPMsg.ReturnCode == (int)ValidateSuperTicketReturnCodes.TransactionNotFound)
                        throw new POSException(string.Format(Resources.TransactionNotFound, FormatExceptionMessage(ex)), ex);
                }
                catch (ServerCommException ex)
                {
                    throw ex; // Don't repackage the ServerCommException
                }
                catch (Exception ex)
                {
                    throw new POSException(string.Format(Resources.GetSuperPickListFailed, FormatExceptionMessage(ex)), ex);
                }

                e.Result = valSPMsg.SuperPicks;

                // Set the player's info associated wth this super pick ticket
                m_lastSuperPickTransactionNumber = valSPMsg.TransactionNumber;
                m_lastSuperPickPlayerNumber = valSPMsg.PlayerId.ToString();
                m_lastSuperPickPlayerFName = valSPMsg.PlayerFName;
                m_lastSuperPickPlayerLName = valSPMsg.PlayerLName;

                // Get the player's picture
                if(m_lastSuperPickPlayerNumber != string.Empty)
                {
                    GetPlayerImageMessage getPicMsg = new GetPlayerImageMessage();
                    getPicMsg.PlayerId = valSPMsg.PlayerId;

                    try
                    {
                        getPicMsg.Send();
                    }
                    catch(ServerCommException ex)
                    {
                        throw ex; // Don't repackage the ServerCommException
                    }
                    catch(Exception ex)
                    {
                        throw new POSException(string.Format(Resources.LoadPlayerPictureFailed, FormatExceptionMessage(ex)), ex);
                    }

                    m_lastSuperPickPlayerPic = getPicMsg.Image;
                }
            }
        }

        /// <summary>
        /// Handles the event when the find super pick numbers BackgroundWorker is complete.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The RunWorkerCompletedEventArgs object that 
        /// contains the event data.</param>
        private void FindSuperPickListComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            // Set the error that occurred (if any).
            LastAsyncException = e.Error;

            if(e.Error == null)
            {
                // Set the results of the search.
                LastFindSuperPicksResults = (SuperPickListItem[])e.Result;
            }
            else
            {
                LastFindSuperPicksResults = null;
            }

            // Close the wait form.
            DisposeWaitForm(); // TTP 50135
        }

        /// <summary>
        /// Creates a thread to find super pick numbers on the server and sets
        /// the WaitForm's settings.
        /// </summary>
        /// <param name="waitForm">The wait form to be used while 
        /// finding super pick numbers.</param>
        /// <param name="authenticationCode">The authentication code to 
        /// search for.</param>
        public void PaySuperPicks(WaitForm waitForm)
        {
            // Set the current wait form used.
            m_waitForm = waitForm;

            // Set the wait message.
            m_waitForm.Message = Resources.WaitFormFinishingPayingSuperPicks;

            // Create the worker thread and run it.
            m_worker = new BackgroundWorker();
            m_worker.WorkerReportsProgress = true;
            m_worker.WorkerSupportsCancellation = false;
            m_worker.DoWork += new DoWorkEventHandler(PaySuperPickWinners);
            m_worker.ProgressChanged += new ProgressChangedEventHandler(m_waitForm.ReportProgress);
            m_worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(PaySuperPickListComplete);
            m_worker.RunWorkerAsync();
        }

        /// <summary>
        /// Pays Super Pick winners on the server.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The DoWorkEventArgs object that 
        /// contains the event data.</param>
        private void PaySuperPickWinners(object sender, DoWorkEventArgs e)
        {
            // Make sure member variables access is thread safe.

            // Set the language.
            if(m_settings.ForceEnglish)
                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");

            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

            // Wait a couple of ticks to let the wait form display.
            System.Threading.Thread.Sleep(100);

            PaySuperTicketMessage paySPWinners = new PaySuperTicketMessage(m_currentOp.Id, m_machineId, m_currentStaff.Id);
            paySPWinners.AddWinner(LastSuperPickWinnerList);

            try
            {
                paySPWinners.Send();
            }
            catch(ServerCommException ex)
            {
                throw ex; // Don't repackage the ServerCommException
            }
            catch(Exception ex)
            {
                throw new POSException(string.Format(Resources.GetSuperPickListFailed, FormatExceptionMessage(ex)), ex);
            }

            e.Result = paySPWinners.PaySuperPicks;
        }

        /// <summary>
        /// Handles the event when the find super pick numbers BackgroundWorker is complete.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The RunWorkerCompletedEventArgs object that 
        /// contains the event data.</param>
        private void PaySuperPickListComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            // Set the error that occurred (if any).
            LastAsyncException = e.Error;

            if(e.Error == null)
            {
                // Set the results of the search.
                LastPaySuperPicksResults = (PaySuperPickListItem[])e.Result;
            }
            else
            {
                LastPaySuperPicksResults = null;
            }

            // Close the wait form.
            DisposeWaitForm(); // TTP 50135
        }

        /// <summary>
        /// Prints the specified super pick Payout(s) to a receipt.
        /// </summary>
        /// <param name="sale">The sale to print.</param>
        /// <param name="allowLastReceipt">Whether to use this receipt when the 
        /// user chooses to print the last receipt.</param>
        /// <param name="isReprint">Whether this is a reprint.</param>
        /// <param name="openDrawer">Whether to open the cash drawer after printing.</param>
        public void PrintPayoutReceipt(bool isReprint, bool openDrawer)
        {
            // Thread safe?
            if(m_settings.PrinterName != null)
            {
                // Create a new printer object.
                Printer printer = new Printer(m_settings.PrinterName);

                // Get the last pay super pick(s).
                ArrayList payout = new ArrayList();
                payout.AddRange(m_lastPaySuperPicksResults);
                PaySuperPickListItem[] payoutList = (PaySuperPickListItem[])payout.ToArray(typeof(PaySuperPickListItem));

                for(int i = 0; i < payoutList.Length; i++)
                {
                    // Create a new payout receipt (one per item).
                    PayoutReceipt payoutReceipt = new PayoutReceipt();

                    // Header
                    payoutReceipt.IsReprint = isReprint;
                    payoutReceipt.OperatorLine1 = m_currentOp.ReceiptHeaderLine1;
                    payoutReceipt.OperatorLine2 = m_currentOp.ReceiptHeaderLine2;
                    payoutReceipt.OperatorLine3 = m_currentOp.ReceiptHeaderLine3;

                    payoutReceipt.Number = payoutList[i].PayoutReceiptNumber;

                    // payoutReceipt.wo = m_workstationId;
                    payoutReceipt.GamingDate = GamingDate;
                    payoutReceipt.Cashier = m_currentStaff.FirstName + " " + m_currentStaff.LastName;

                    // Get the player's name.
                    payoutReceipt.PlayerId = payoutList[i].PlayerId;
                    payoutReceipt.PlayerName = payoutList[i].PlayerFirstName + " " + payoutList[i].PlayerLastName;
                    payoutReceipt.AmountWon = Convert.ToDecimal(payoutList[i].AmountWon);

                    // Print out the receipt.
                    payoutReceipt.Print(printer);
                }

                // Pop the drawer, if applicable.
                if(openDrawer && m_settings.DrawerCode != null && m_settings.DrawerCode.Length > 0)
                {
                    printer.OpenDrawerCode = m_settings.DrawerCode;
                    printer.OpenDrawer();
                }
            }
        }
        */

        ////US4382: POS: B3 Open sale
        /// <summary>
        /// Initializes the b3 session session menu and menu item.
        /// </summary>
        private bool InitializeB3Session()
        {
            bool menuIsUsable = false;

            //create b3 menu list
            m_b3MenuListItem = new POSMenuListItem
            {
                Menu = new POSMenu(0, Resources.B3SessionString, 20),
                Session =  new SessionInfo(
                        0, // session number
                        0, // session played id is zero for pre sale menus
                        Resources.B3SessionString, //program name
                        false, //isMaxValidationEnabled
                        false, //isDeviceFeesEnabled
                        false, //isAutoDiscountsEnabled
                        0, //pointsMultiplier
                        0, //sessionMaxCardLimit
                        false, //isPreSale
                        null, //gameToGameCategoriesDictionary
                        null, //gameCategoryList
                        null) //gaming date
            };

            if (WeAreNotAPOSKiosk) //Kiosks don't need any buttons on this menu
            {
                if (B3SessionActive)
                {
                    //manually add buttons to menu
                    m_b3MenuListItem.Menu.AddButton(1, 0, new B3Button(this, 10)); //$10
                    m_b3MenuListItem.Menu.AddButton(1, 1, new B3Button(this, 20)); //$20
                    m_b3MenuListItem.Menu.AddButton(1, 2, new B3Button(this, 40)); //$40
                    m_b3MenuListItem.Menu.AddButton(1, 3, new B3Button(this, 60)); //$60
                    m_b3MenuListItem.Menu.AddButton(1, 4, new B3Button(this, 100));//$100
                    m_b3MenuListItem.Menu.AddButton(1, 19, new B3Button(this));    //user entered amount
                    menuIsUsable = true;
                }

                //US5192/DE13380 Check if the staff has permission to redeem a pack. No need to check if B3 is enable it has been checked from this point.
                if (m_currentStaff.CheckModuleFeature(EliteModule.B3Center, (int)POSFeature.B3Redeem))
                {
                    m_b3MenuListItem.Menu.AddButton(1, 15, new B3Button(this, B3Button.B3ButtonType.RetrieveAccount));
                    menuIsUsable = true;
                }
            }
            else
            {
                menuIsUsable = true;
            }

            return menuIsUsable;
        }

        /// <summary>
        /// Adds the b3 session to menu list.
        /// </summary>
        private void AddB3SessionToMenuList()
        {
            if (!WeAreAB3Kiosk)
            {
                if (WeAreAPOSKiosk && !Settings.AllowB3OnKiosk)
                    return;
            }

            RemoveB3SessionFromMenuList();

            if (!InitializeB3Session())
                return;

            lock (m_menuSync)
            {
                if (m_menuList == null)
                    m_menuList = new POSMenuListItem[] { };

                //find where we need to put it (between normal and pre-sale sessions)
                if (m_menuList.ToList().Exists(e => e.Session.IsPreSale))
                {
                    int here = m_menuList.Length;

                    for (int x = 0; x < m_menuList.Length; x++)
                    {
                        if (m_menuList[x].Session.IsPreSale)
                        {
                            here = x;
                            break;
                        }
                    }

                    List<POSMenuListItem> menuList = m_menuList.ToList();

                    menuList.Insert(here, m_b3MenuListItem);
                    m_menuList = menuList.ToArray();
                }
                else //no pre-sales, B3 goes at bottom
                {
                    if (!HaveMenu && WeAreAnAdvancedPOSKiosk) //all we can do is sell B3 but the kiosk B3 selling screen is transient, we need an empty menu to return to.
                    {
                        POSMenuListItem emptyMenuListItem = new POSMenuListItem
                        {
                            Menu = new POSMenu(0, Resources.BingoSalesClosed, 20),
                            Session = new SessionInfo(
                                    0, // session number
                                    0, // session played id is zero for pre sale menus
                                    Resources.BingoSalesClosed, //program name
                                    false, //isMaxValidationEnabled
                                    false, //isDeviceFeesEnabled
                                    false, //isAutoDiscountsEnabled
                                    0, //pointsMultiplier
                                    0, //sessionMaxCardLimit
                                    false, //isPreSale
                                    null, //gameToGameCategoriesDictionary
                                    null, //gameCategoryList
                                    null) //gaming date
                        };

                        m_menuList = new POSMenuListItem[1];
                        m_menuList[0] = emptyMenuListItem;
                    }

                    var menuList = new List<POSMenuListItem>(m_menuList) { m_b3MenuListItem };
                    m_menuList = menuList.ToArray();
                }
            }
        }

        /// <summary>
        /// Removes the b3 session from menu list.
        /// </summary>
        private void RemoveB3SessionFromMenuList()
        {
            if (m_menuList != null && m_menuList.Length > 0)
            {
                var menuList = new List<POSMenuListItem>(m_menuList);
                lock (m_menuSync)
                {
                    if (menuList.Contains(m_b3MenuListItem))
                    {
                        menuList.Remove(m_b3MenuListItem);
                        m_menuList = menuList.ToArray();
                    }
                }
            }
        }

        /// <summary>
        /// Gets the b3 session active.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="POSException"></exception>
        private bool GetB3SessionActive()
        {
            var msg = new GetB3SessionListMessage();
            
            try
            {
                msg.Send();

                if (msg.ReturnCode != 0)
                {
                    throw new Exception("Unable to Get Active B3 Session from server. Return code: " + msg.ReturnCode);
                }

                m_currentB3Session = msg.SessionList.FirstOrDefault(x => x.Active);
                m_b3SessionActive = m_currentB3Session != null;
            }
            catch (ServerCommException)
            {
                throw; // Don't repackage the ServerCommException
            }
            catch (Exception ex)
            {
                ServerException sEx = ex as ServerException;

                if (sEx != null && sEx.ReturnCode == GTIServerReturnCode.SQLError) //assume SQL error is no B3 database
                {
                    m_currentB3Session = null;
                    m_b3SessionActive = false;
                }
                else
                {
                    throw new POSException(string.Format(CultureInfo.CurrentCulture, "Failed to get B3 session", ServerExceptionTranslator.FormatExceptionMessage(ex)), ex);
                }
            }

            return m_b3SessionActive;
        }

        /// <summary>
        /// Adds the b3 sale.
        /// </summary>
        public void B3AddSale()
        {
            // PDTS 583
            // Set the sale status form.
            // Rally TA7464
            m_statusForm.Cursor = Cursors.WaitCursor;
            m_statusForm.Message = Resources.WaitFormMakeSale;
            m_statusForm.Sale = m_currentSale;

            // Create the worker thread and run it.
            m_worker = new BackgroundWorker();
            m_worker.WorkerReportsProgress = true;
            m_worker.WorkerSupportsCancellation = true;
            m_worker.DoWork += B3AddSaleDoWork;
            m_worker.ProgressChanged += m_statusForm.ReportProgress;
            m_worker.RunWorkerCompleted += AddB3SaleCompleted;

            // Box the arguments.
            var items = new List<SaleItem>(m_currentSale.GetItems().ToList());

            m_worker.RunWorkerAsync(items);
        }

        /// <summary>
        /// B3s the add sale do work.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DoWorkEventArgs"/> instance containing the event data.</param>
        /// <exception cref="Exception">Unable to add B3 sale. Return code:  + msg.ReturnCode</exception>
        /// <exception cref="POSException"></exception>
        private void B3AddSaleDoWork(object sender, DoWorkEventArgs e)
        {
            SetupThread();

            BackgroundWorker worker = (BackgroundWorker)sender;

            // Unpackage the worker and status form.
            List<SaleItem> saleItems = e.Argument as List<SaleItem>;

            // Attempt to add the sale.
            worker.ReportProgress(0, Resources.WaitFormMakeSale);

            var total = 0;

            if (saleItems != null)
            {
                foreach (var saleItem in saleItems)
                {
                    if (!saleItem.IsB3Credit)
                    {
                        continue;
                    }

                    var b3Account = saleItem.B3Credit;
                    total += (int)(b3Account.Amount * 100 * saleItem.Quantity);
                }
            }

            if (total == 0)
            {
                return;
            }

            var msg = new B3AddSaleMessage(total);

            if (!Settings.KioskTestNoB3Server)
            {
                try
                {
                    msg.Send();

                    if (msg.ReturnCode != 0)
                    {
                        throw new Exception("Unable to add B3 sale. Return code: " + msg.ReturnCode);
                    }
                }
                catch (Exception ex)
                {
                    throw new POSException(string.Format(CultureInfo.CurrentCulture, Resources.AddSaleFailed, ServerExceptionTranslator.FormatExceptionMessage(ex)), ex);
                }
            }

            try
            {
                var receipt = new B3SalesReceipt(m_currentB3Session.OperatorName, m_currentB3Session.SessionNumber, Settings.KioskTestNoB3Server ? 1000000 : msg.AccountNumber, Settings.KioskTestNoB3Server? 123456789 : msg.ReceiptNumber, (decimal)total / 100);

                receipt.AmountTendered = m_currentSale.AmountTendered;

                // Set Currency and exchange rate
                receipt.DefaultCurrency = m_currentSale.SaleCurrency.ISOCode;
                receipt.SaleCurrency = m_currentSale.SaleCurrency.ISOCode;
                receipt.ExchangeRate = m_currentSale.SaleCurrency.ExchangeRate;
                receipt.PrintLotto = Settings.PlayType == BingoPlayType.Lotto;
                
                //print receipt
                PrintB3SaleReceipt(receipt);
            }
            catch (Exception ex)
            {
                throw new POSPrintException(string.Format(CultureInfo.CurrentCulture, Resources.SaleButNoReceipt, ex.Message), ex);
            }

            e.Result = msg;
        }

        /// <summary>
        /// Handles the event when the sale BackgroundWorker is complete.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The RunWorkerCompletedEventArgs object that 
        /// contains the event data.</param>
        private void AddB3SaleCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            m_statusForm.Message = string.Empty;

            // Set the error that occurred (if any).
            LastAsyncException = e.Error;

            if (e.Error == null || e.Error is POSPrintException)
            {
                // PDTS 583
                // Close the status form.
                if (!m_statusForm.OkButtonVisible)
                    m_statusForm.CloseForm();
                else
                {
                    m_statusForm.CancelButtonVisible = false;
                    m_statusForm.OkButtonEnabled = true;
                    m_statusForm.Cursor = Cursors.Default;
                }

                // Clear the sale.
                ClearSale();
            }
            else
            {
                // Reset which device they selected.
                m_currentSale.Device = Device.FromId(0);
                m_currentSale.DeviceFee = 0M;
                // PDTS 583
                m_currentSale.UnitNumber = 0;
                m_currentSale.SerialNumber = string.Empty;
                // Rally TA5748
                m_currentSale.ClearStartNumbers();

                // Rally TA7465
                m_sellingForm.SetTaxesAndFees(m_currentSale.CalculateTaxes() + m_currentSale.CalculateFees());
                m_sellingForm.SetPrepaidTotal(m_currentSale.CalculatePrepaidAmount() + m_currentSale.CalculatePrepaidTaxTotal());
                m_sellingForm.SetTotal(m_currentSale.CalculateTotal(true));

                // PDTS 571
                m_currentSale.Quantity = 1;
                m_sellingState = SellingState.Selling;

                // Restore sale modification ability.
                m_sellingForm.UpdateMenuButtonStates();
                m_sellingForm.UpdateSystemButtonStates();

                // Update the display.
                m_sellingForm.UpdateSaleInfo();
                m_statusForm.CloseForm(); // PDTS 583

                Log("Failed to finish the sale: " + e.Error.Message, LoggerLevel.Message);
            }


            m_sellingForm.DisplayB3SessionMode(); //US4380: (US4337) POS: Display B3 Menu
        }

        /// <summary>
        /// Adds a B3 credit without an associated sale.
        /// </summary>
        public void B3AddCredit(decimal creditAmount)
        {
            // Set the sale status form.
            m_statusForm.SecondaryThreadComplete = false;           
            m_statusForm.Message = Resources.WaitFormMakeSale;

            // Create the worker thread and run it.
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = false;
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += B3AddCreditDoWork;
            worker.ProgressChanged += m_statusForm.ReportProgress;
            worker.RunWorkerCompleted += AddB3CreditCompleted;
            worker.RunWorkerAsync(creditAmount);
        }

        /// <summary>
        /// Routine to add B3 credit without associated sale data.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DoWorkEventArgs"/> instance containing the event data.</param>
        /// <exception cref="Exception">Unable to add B3 credit. Return code:  + msg.ReturnCode</exception>
        /// <exception cref="POSException"></exception>
        private void B3AddCreditDoWork(object sender, DoWorkEventArgs e)
        {
            SetupThread();

            BackgroundWorker worker = (BackgroundWorker)sender;

            //get the amount to credit
            decimal creditAmount = (decimal)(e.Argument as decimal?);

            if (creditAmount == 0)
                return;

            var msg = new B3AddSaleMessage((int)(creditAmount * 100));

            if (!Settings.KioskTestNoB3Server)
            {
                try
                {
                    msg.Send();

                    if (msg.ReturnCode != 0)
                        throw new Exception("Unable to add B3 credit. Return code: " + msg.ReturnCode);
                }
                catch (Exception ex)
                {
                    throw new POSException(string.Format(CultureInfo.CurrentCulture, Resources.AddSaleFailed, ServerExceptionTranslator.FormatExceptionMessage(ex)), ex);
                }
            }

            try
            {
                var receipt = new B3SalesReceipt(m_currentB3Session.OperatorName, m_currentB3Session.SessionNumber, Settings.KioskTestNoB3Server ? 1000000 : msg.AccountNumber, Settings.KioskTestNoB3Server? 123456789 : msg.ReceiptNumber, creditAmount);

                receipt.AmountTendered = creditAmount;

                // Set Currency and exchange rate
                if (m_currentSale != null)
                {
                    receipt.DefaultCurrency = m_currentSale.SaleCurrency.ISOCode;
                    receipt.SaleCurrency = m_currentSale.SaleCurrency.ISOCode;
                    receipt.ExchangeRate = m_currentSale.SaleCurrency.ExchangeRate;
                }
                else
                {
                    receipt.DefaultCurrency = DefaultCurrency.ISOCode;
                    receipt.SaleCurrency = DefaultCurrency.ISOCode;
                    receipt.ExchangeRate = DefaultCurrency.ExchangeRate;
                }

                receipt.PrintLotto = Settings.PlayType == BingoPlayType.Lotto;

                //print receipt
                PrintB3SaleReceipt(receipt);

                SellingForm.IncNVRAMUserDecimal(SellingForm.NVRAMUserDecimal.AmountDispensed, creditAmount);
            }
            catch (Exception ex)
            {
                throw new POSPrintException(string.Format(CultureInfo.CurrentCulture, Resources.SaleButNoReceipt, ex.Message), ex);
            }

            e.Result = msg;
        }

        /// <summary>
        /// Handles the event when the sale BackgroundWorker is complete.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The RunWorkerCompletedEventArgs object that 
        /// contains the event data.</param>
        private void AddB3CreditCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            m_statusForm.Message = string.Empty;

            // Set the error that occurred (if any).
            LastAsyncException = e.Error;
            m_statusForm.SecondaryThreadComplete = true;

            if (e.Error != null && !(e.Error is POSPrintException))
                Log("Failed to finish the sale: " + e.Error.Message, LoggerLevel.Message);
        }

        //US4397: (US1592) POS: B3 Hand Pay
        /// <summary>
        /// B3s the retrieve account.
        /// </summary>
        /// <param name="accountNumber">The account number.</param>
        public void B3RetrieveAccount(int accountNumber)
        {    
            // Create the worker thread and run it.
            m_worker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            m_worker.DoWork += B3RetrieveAccountDoWork;
            m_worker.ProgressChanged += m_waitForm.ReportProgress;
            m_worker.RunWorkerCompleted += B3RetrieveAccountComplete;

            m_worker.RunWorkerAsync(accountNumber);
        }

        /// <summary>
        /// B3s the retrieve account do work.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DoWorkEventArgs"/> instance containing the event data.</param>
        private void B3RetrieveAccountDoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = (BackgroundWorker)sender;
            int accountNumber = (int)e.Argument;

            try
            {
                var msg = new B3RetrieveAccountMessage(accountNumber);

                msg.Send();

                if (msg.ReturnCode != 0)
                {
                    throw new Exception("Unable to add B3 sale. Return code: " + msg.ReturnCode);
                }

                if (m_b3RetrieveAccountForm == null)
                {
                    return;
                }

                m_b3RetrieveAccountForm.RetrieveAccountComplete(msg);

            }
            catch (ServerCommException)
            {
                throw; // Don't repackage the ServerCommException
            }
            catch (Exception ex)
            {
                throw new POSException(string.Format(CultureInfo.CurrentCulture, Resources.GetB3SessionListFailed, ServerExceptionTranslator.FormatExceptionMessage(ex)), ex);
            }

            worker.ReportProgress(100, Resources.B3RetriveAccountProgress);
        }

        /// <summary>
        /// B3s the retrieve account complete.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RunWorkerCompletedEventArgs"/> instance containing the event data.</param>
        private void B3RetrieveAccountComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            m_waitForm.CloseForm();
        }

        //US4338: (US1592) POS: Redeem B3
        /// <summary>
        /// B3s the redeem account.
        /// </summary>
        /// <param name="accountNumber">The account number.</param>
        public void B3RedeemAccount(int accountNumber)
        {
            // Create the worker thread and run it.
            m_worker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            m_worker.DoWork += B3RedeemAccountDoWork;
            m_worker.ProgressChanged += m_waitForm.ReportProgress;
            m_worker.RunWorkerCompleted += B3RedeemAccountComplete;

            m_worker.RunWorkerAsync(accountNumber);
        }

        /// <summary>
        /// B3s the redeem account do work.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DoWorkEventArgs"/> instance containing the event data.</param>
        private void B3RedeemAccountDoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = (BackgroundWorker)sender;
            var accountNumber = (int)e.Argument;

            worker.ReportProgress(50, Resources.B3RedeemAccountProgress);
            
            try
            {
                if (m_b3RetrieveAccountForm == null)
                {
                    return;
                }
                
                var msg = new B3RedeemAccountMessage(accountNumber, (int)(m_b3RetrieveAccountForm.Total*100));
                msg.Send();

                if (msg.ReturnCode != 0)
                {
                    throw new Exception("Unable to redeem B3 account. Return code: " + msg.ReturnCode);
                }
                
                B3Receipt receipt;
                if (m_currentB3Session == null || msg.SessionNumber != m_currentB3Session.SessionNumber)
                {
                    //print out of session redemption receipt
                    receipt = new B3OutOfSessionRedeemAccountReceipt(m_currentB3Session == null? "" : m_currentB3Session.OperatorName,
                        msg.SessionNumber,
                        m_currentB3Session == null? 0 : m_currentB3Session.SessionNumber,
                        accountNumber,
                        msg.ReceiptNumber,
                        m_b3RetrieveAccountForm.WinCredit,
                        m_b3RetrieveAccountForm.Credits,
                        m_b3RetrieveAccountForm.Total,
                        m_b3RetrieveAccountForm.IsDoubleAccount);
                }
                else
                {
                    //print regular redeem receipt
                    receipt = new B3RedeemAccountReceipt(m_currentB3Session.OperatorName,
                        msg.SessionNumber,
                        accountNumber,
                        msg.ReceiptNumber,
                        m_b3RetrieveAccountForm.WinCredit, //DE13137
                        m_b3RetrieveAccountForm.Credits, //DE13137
                        m_b3RetrieveAccountForm.Total,
                        m_b3RetrieveAccountForm.IsDoubleAccount); //DE13137
                }

                OpenCashDrawer(2);
                PrintB3SaleReceipt(receipt);
            }
            catch (ServerCommException)
            {
                throw; // Don't repackage the ServerCommException
            }
            catch (Exception ex)
            {
                throw new POSException(string.Format(CultureInfo.CurrentCulture, Resources.GetB3SessionListFailed, ServerExceptionTranslator.FormatExceptionMessage(ex)), ex);
            }

            worker.ReportProgress(100, Resources.B3RedeemAccountProgress);

        }

        /// <summary>
        /// B3s the redeem account complete.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RunWorkerCompletedEventArgs"/> instance containing the event data.</param>
        private void B3RedeemAccountComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            m_waitForm.CloseForm();
        }

        //US4395: (US1592) POS: B3 Unlock Accounts
        /// <summary>
        /// B3s the unlock account.
        /// </summary>
        /// <param name="accountNumber">The account number.</param>
        public void B3UnlockAccount(int accountNumber)
        {
            // Create the worker thread and run it.
            m_worker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            m_worker.DoWork += B3UnlockAccountDoWork;
            m_worker.ProgressChanged += m_waitForm.ReportProgress;
            m_worker.RunWorkerCompleted += B3UnlockAccountComplete;

            m_worker.RunWorkerAsync(accountNumber);
        }

        /// <summary>
        /// B3s the unlock account do work.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DoWorkEventArgs"/> instance containing the event data.</param>
        private void B3UnlockAccountDoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = (BackgroundWorker)sender;
            var accountNumber = (int)e.Argument;

            worker.ReportProgress(50, Resources.B3UnlockAccountProgress);


            try
            {
            
                var msg = new B3UnlockAccountMessage(accountNumber);
                msg.Send();

                if (msg.ReturnCode != 0)
                {
                    throw new Exception("Unable to unlock B3 account. Return code: " + msg.ReturnCode);
                }

                if (m_b3RetrieveAccountForm == null)
                {
                    return;
                }

                m_b3RetrieveAccountForm.UnlockAccountComplete(msg);

                //if taxable amount
                if (m_b3RetrieveAccountForm.IsTaxableAmount)
                {
                    //send message
                    var jackpotMessage = new B3JackpotAccountMessage(accountNumber, msg.SessionNumber);
                    jackpotMessage.Send();

                    //print jackpot receipt
                    var jackpotReceipt = new B3JackpotReceipt(  m_currentB3Session.OperatorName,
                                                                m_currentB3Session.SessionNumber, 
                                                                accountNumber, 
                                                                m_b3RetrieveAccountForm.TaxableAmount,
                                                                (decimal) jackpotMessage.JackpotLimit/100, 
                                                                jackpotMessage.GameName, 
                                                                jackpotMessage.GameNumber,
                                                                jackpotMessage.ClientName, 
                                                                jackpotMessage.JackpotDateTime);

                    PrintB3SaleReceipt(jackpotReceipt);
                }
                
                //print unlock receipt
                var receipt = new B3UnlockAccountReceipt(   m_currentB3Session.OperatorName, 
                                                            m_currentB3Session.SessionNumber, 
                                                            accountNumber, 
                                                            msg.ReceiptNumber,
                                                            m_b3RetrieveAccountForm.Total); //DE13132

                PrintB3SaleReceipt(receipt);
            }
            catch (ServerCommException)
            {
                throw; // Don't repackage the ServerCommException
            }
            catch (Exception ex)
            {
                throw new POSException(string.Format(CultureInfo.CurrentCulture, Resources.GetB3SessionListFailed, ServerExceptionTranslator.FormatExceptionMessage(ex)), ex);
            }

            worker.ReportProgress(100, Resources.B3UnlockAccountProgress);
        }

        /// <summary>
        /// B3s the unlock account complete.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RunWorkerCompletedEventArgs"/> instance containing the event data.</param>
        private void B3UnlockAccountComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            m_waitForm.CloseForm();
        }
        
        /// <summary>
        /// Prints the current sale to a receipt.
        /// </summary>
        private void PrintB3SaleReceipt(B3Receipt receipt)
        {
            // Load the receipt settings.
            string receiptPrinterName;
            short copies;
            string disclaimer1, disclaimer2, disclaimer3;
            bool printReceipt = true; // Rally DE9948

            lock (Settings.SyncRoot)
            {
                // Do we even need to print this sale?
                if (!Settings.PrintNonElecReceipt)
                    printReceipt = false; // Rally DE9948

                receiptPrinterName = Settings.ReceiptPrinterName;
                copies = Settings.ReceiptCopies;
                disclaimer1 = Settings.ReceiptDisclaimer1;
                disclaimer2 = Settings.ReceiptDisclaimer2;
                disclaimer3 = Settings.ReceiptDisclaimer3;
            }

            // Rally DE9948
            if (printReceipt)
            {

                // TTP 50372 - POS isn't able to reprint a receipt it couldn't create the first time.
                //B3SalesReceipt receipt;
                Printer receiptPrinter = null;
                Exception printerException = null;

                // Create the receipt printer and receipt objects.
                if (receiptPrinterName != null)
                {
                    try
                    {
                        receiptPrinter = new Printer(receiptPrinterName);
                    }
                    catch (Exception e)
                    {
                        printerException = e;
                        receiptPrinter = null;
                    }
                }

                receipt.SoldFromMachineId = m_machineId;
                //disclaimer
                receipt.DisclaimerLine1 = disclaimer1;
                receipt.DisclaimerLine2 = disclaimer2;
                receipt.DisclaimerLine3 = disclaimer3;
                
                //print cashier
                receipt.Cashier = m_currentStaff.FirstName;

                // TTP 50097
                if (!Settings.PrintStaffFirstNameOnly)
                    receipt.Cashier += " " + m_currentStaff.LastName;
                
                //set footer
                lock (Settings.SyncRoot)
                {
                    receipt.OperatorName = CurrentOperator.Name;
                    receipt.OperatorAddress1 = CurrentOperator.Address1;
                    receipt.OperatorAddress2 = CurrentOperator.Address2;
                    receipt.OperatorCityStateZip = CurrentOperator.City + ", " + CurrentOperator.State + " " + CurrentOperator.Zip;
                    receipt.OperatorPhoneNumber = CurrentOperator.Phone;

                    receipt.OperatorHeaderLine1 = Settings.ReceiptHeaderLine1;
                    receipt.OperatorHeaderLine2 = Settings.ReceiptHeaderLine2;
                    receipt.OperatorHeaderLine3 = Settings.ReceiptHeaderLine3;
                    receipt.OperatorFooterLine1 = Settings.ReceiptFooterLine1;
                    receipt.OperatorFooterLine2 = Settings.ReceiptFooterLine2;
                    receipt.OperatorFooterLine3 = Settings.ReceiptFooterLine3;
                }
                
                //Date 
                receipt.GamingDate = DateTime.Parse(m_currentB3Session == null? "01/01/1980 00:00:00" : m_currentB3Session.SessionStartTime);
                
                // If we failed while creating the receipt, don't try to print it.
                if (printerException != null)
                    throw printerException;

                if (receiptPrinter != null)
                    receipt.Print(receiptPrinter, copies);
            }
        }

        /// <summary>
        /// Shows the b3 retrieve account form.
        /// </summary>
        public void ShowB3RetrieveAccountForm()
        {
            DisplayMode displayMode;

            lock (Settings.SyncRoot)
            {
                displayMode = Settings.DisplayMode;
            }
            m_b3RetrieveAccountForm = new B3RetrieveAccountForm(this, displayMode, m_settings.B3IsDoubleAccount)
            {
                StartPosition = FormStartPosition.CenterParent
            };

            m_b3RetrieveAccountForm.ShowDialog(m_sellingForm);
        }

        /// <summary>
        /// Finds an unlocked button to go with the package.
        /// </summary>
        /// <param name="packageId">Package to find an unlocked button for.</param>
        /// <param name="currentSession">True=look in current session, False=look through all sessions.</param>
        /// <returns>PackageButton found, true=all buttons were locked.</returns>
        public Tuple<PackageButton, bool> GetNonLockedMenuButtonForPackage(int packageId, bool currentSession = true)
        {
            PackageButton pb = GetMenuButtonForPackage(packageId, currentSession, true);
            PackageButton lockedPb = null;

            if(pb == null) //see if there was a locked one
                lockedPb = GetMenuButtonForPackage(packageId, currentSession, false);

            return new Tuple<PackageButton, bool>(pb, lockedPb != null);
        }

        /// <summary>
        /// Gets the PackageButton for the requested package in the current session.
        /// </summary>
        /// <param name="packageId">Package ID of the package button to find.</param>
        /// <returns>PackageButton of requested package or null if not found.</returns>
        public PackageButton GetMenuButtonForPackage(int packageId, bool currentSession = true, bool notLocked = false)
        {
            PackageButton packageButton = null;

            if (currentSession)
            {
                POSMenuListItem ourSession = m_menuList.ToList().Find(m => m.Session == CurrentSession);

                if (ourSession != null && ourSession.Menu != null)
                {
                    packageButton = ourSession.Menu.GetPackageButton(packageId, notLocked);
                }
            }
            else
            {
                foreach (POSMenuListItem posMenuItem in m_menuList.ToList())
                {
                    packageButton = posMenuItem.Menu.GetPackageButton(packageId, notLocked);

                    if (packageButton != null)
                        break;
                }
            }

            return packageButton;
        }

        /// <summary>
        /// Gets the PackageButton for the requested package in the current session.
        /// </summary>
        /// <param name="packageId">Package ID of the package button to find.</param>
        /// <returns>PackageButton of requested package or null if not found.</returns>
        public DiscountButton GetMenuButtonForDiscount(int discountID, bool currentSession = true)
        {
            if (currentSession)
            {
                return m_menuList.ToList().Find(m => m.Session == CurrentSession).Menu.GetDiscountButton(discountID);
            }
            else
            {
                DiscountButton discountButton;

                foreach (POSMenuListItem posMenuItem in m_menuList.ToList())
                {
                    discountButton = posMenuItem.Menu.GetDiscountButton(discountID);

                    if (discountButton != null)
                        return discountButton;
                }

                return null;
            }
        }

        public void PushMenuButton(MenuButton button)
        {
            ImageButton ib = new ImageButton();

            ib.Tag = button;

//            if (m_sellingForm.m_pageNavigator.CurrentPage != button.Page) //need to change the page
//                m_sellingForm.m_pageNavigator.CurrentPage = button.Page;

            m_sellingForm.MenuButtonClick(ib, new EventArgs()); 
        }

        /// <summary>
        /// Cancels any pending transactions and shuts down the POS.
        /// </summary>
        public void Shutdown()
        {
            m_shuttingDown = true;

            Log("Shutting down.", LoggerLevel.Debug);

            // PDTS 1064
            if(m_magCardReader != null)
                m_magCardReader.EndReading();

            CanUpdateMenus = false;
            m_sellingState = SellingState.NotSelling;
            m_currentSale = null;

            if (m_sellingForm != null)
            {
                m_sellingForm.SetQuantitySaleInfo();
            }

            m_lastSale = null;
            m_lastReceipt = null;
            m_lastReceiptHasPlayIt = false; // Rally US505

            // PDTS 966
            // Unsubscribe from all messages.
            ModuleComm modComm = new ModuleComm();
            modComm.UnsubscribeFromMessage(m_assignedId, 0);

            if(m_msgTimer != null)
            {
                m_msgTimer.Dispose();
                m_msgTimer = null;
            }

            if (m_weHaveAStarPrinter)
            {
                lock (m_starPrinterStatusLockObject)
                {
                    if (m_starPrinterStatusTimer != null)
                    {
                        m_starPrinterStatusTimer.Stop();
                        m_starPrinterStatusTimer.Dispose();
                        m_starPrinterStatusTimer = null;
                    }

                    if (m_starPrinterPort != null)
                        OpenStarPrinterPort(false);
                }
            }

            lock(m_msgSync)
            {
                if(m_pendingMsgs != null)
                {
                    m_pendingMsgs.Clear();
                    m_pendingMsgs = null;
                }
            }

            if(m_worker != null)
            {
                m_worker.Dispose();
                m_worker = null;
            }

            if(m_receiptManager != null)
            {
                try
                {
                    m_receiptManager.Shutdown();
                    m_receiptManager = null;
                }
                catch
                {
                }
            }

            if(m_playerCenter != null)
            {
                try
                {
                    m_playerCenter.Shutdown();
                    m_playerCenter = null;
                }
                catch
                {
                }
            }

            if(m_unitMgmt != null)
            {
                try
                {
                    m_unitMgmt.Shutdown();
                    m_unitMgmt = null;
                }
                catch
                {
                }

            }

            // PDTS 591
            if(m_cbbManager != null)
            {
                m_cbbManager.Dispose();
                m_cbbManager = null;
            }

            // PDTS 693
            if(m_waitForm != null)
            {
                m_waitForm.Dispose();
                m_waitForm = null;
            }

            // PDTS 583
            if(m_statusForm != null)
            {
                m_statusForm.Dispose();
                m_statusForm = null;
            }

            if(m_sellingForm != null)
            {
                m_sellingForm.Dispose();
                m_sellingForm = null;
            }

            if (WeHaveAGuardian)
            {
                //disconnect from Guardian
                m_Guardian.GuardianRequestedShutdown -= Guardian_GuardianRequestedShutdown;
                m_Guardian.GuardianRequestedControl -= Guardian_GuardianRequestedControl;
                m_Guardian.GuardianReleasedControl -= Guardian_GuardianReleasedControl;
                m_Guardian.Dispose();
                m_Guardian = null;
            }

            DisposeLoadingForm();

            // PDTS 1064
            if(m_magCardReader != null)
            {
                m_magCardReader.RemoveAllSources();
                m_magCardReader = null;
            }

            if(!m_settings.ShowCursor)
                Cursor.Show();

            Log("Shutdown complete.", LoggerLevel.Information);

            lock(m_logSync)
            {
                m_loggingEnabled = false;
            }

            LastAsyncException = null;
            m_initialized = false;
            m_assignedId = -1; // PDTS 966
        }

        // Rally TA14495
        /// <summary>
        /// Gets tender settings from the server
        /// </summary>
        public void GetValidPOSTenderTypesFromServer()
        {
            Shared.Data.GetTenderTypesMessage getTenders = new Shared.Data.GetTenderTypesMessage();

            try
            {
                getTenders.Send(); //get all of the tender types
            }
            catch (Exception e)
            {
                ReformatException(e);
            }

            if (getTenders.TenderTypes.Count > 0) //look through all the tender types and remove any that aren't active or allowed at POS
            {
                m_tenderTypes = getTenders.TenderTypes;

                for (int x = 0; x < m_tenderTypes.Count; x++)
                {
                    //remove if inactive
                    if (m_tenderTypes[x].IsActive == 0)
                    {
                        m_tenderTypes.RemoveAt(x--);
                        continue;
                    }

                    //remove if not allowed at this machine
                    switch ((TenderType)((TenderTypeValue)m_tenderTypes[x]).TenderTypeID)
                    {
                        case TenderType.Cash:
                        {
                            if (!m_settings.AllowCashTender)
                                m_tenderTypes.RemoveAt(x--);
                        }
                        break;

                        case TenderType.Check:
                        {
                            if (!m_settings.AllowCheckTender)
                                m_tenderTypes.RemoveAt(x--);
                        }
                        break;

                        case TenderType.CreditCard:
                        {
                            if (!m_settings.AllowCreditCardTender)
                                m_tenderTypes.RemoveAt(x--);
                        }
                        break;
                        
                        case TenderType.DebitCard:
                        {
                            if (!m_settings.AllowDebitCardTender)
                                m_tenderTypes.RemoveAt(x--);
                        }
                        break;
                        
                        case TenderType.GiftCard:
                        {
                            if (!m_settings.AllowGiftCardTender)
                                m_tenderTypes.RemoveAt(x--);
                        }
                        break;
                        
                        default: //remove if we don't know what it is
                        {
                            m_tenderTypes.RemoveAt(x--);
                        }
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Gets sub-tender display names from the server
        /// </summary>
        public void GetSubTenderNamesFromServer()
        {
            GetTenderSubTypesMessage getSubTenders = new GetTenderSubTypesMessage();

            m_subTenderNames = new Dictionary<int,string>();

            try
            {
                getSubTenders.Send();
            }
            catch (Exception e)
            {
                ReformatException(e);
            }

            if (getSubTenders.TenderSubTypes.Count > 0)
            {
                for (int x = 0; x < getSubTenders.TenderSubTypes.Count; x++)
                    m_subTenderNames.Add(getSubTenders.TenderSubTypes[x].ID, getSubTenders.TenderSubTypes[x].displayName);
            }
        }
        
        //US5117: POS: Automatically add package X when package Y has been added Z times
        /// <summary>
        /// Clones the package buttons for quantity discount.
        /// </summary>
        /// <param name="discount">The discount.</param>
        /// <param name="packageButton">The package button.</param>
        /// <returns>A list of cloned buttons by session.</returns>
        private List<PackageButton> ClonePackageButtonsForQuantityDiscount(Discount discount, int packageID)
        {
            //make sure nothing is null
            if (packageID == null || discount == null)
                return null;

            List<PackageButton> buttonList = new List<PackageButton>();

            foreach (POSMenuListItem m in m_menuList)
            {
                PackageButton packageButton = m.Menu.GetPackageButton(packageID);

                if(packageButton != null)
                {
                    //set the discount amount
                    var discountAmount = 0m;

                    switch (discount.DiscountItem.Type)
                    {
                        case DiscountType.Percent:
                        {
                            //calculate percentage
                            var autoPercentdiscount = discount as PercentDiscount;

                            if (autoPercentdiscount != null)
                            {
                                discount.Amount = 0;
                                discountAmount = Math.Truncate(packageButton.Package.Price * autoPercentdiscount.DiscountPercentage) / 100;
                            }
                        }
                        break;
                        
                        case DiscountType.Fixed:
                        {
                            discountAmount = discount.Amount;
                        }
                        break;
                    }

                    /************************************************/
                    /************************************************/
                    /* Discount Amount is always set to zero until  */
                    /* we support overriding price of a package on  */
                    /* the server.                                  */
                    /************************************************/
                    /************************************************/
                    discountAmount = 0;

                    //Make a clone of the package.
                    //Set the discount id, description and amount. This allows us to
                    //differentiate from the regular package vs the discounted package.
                    Package clonedPackage = new Package(packageButton.Package)
                    {
                        DiscountDescription = discount.Name,
                        AppliedDiscountId = discount.Id,
                        DiscountAmount = discountAmount
                    };

                    // Add and check for any optional products.
                    clonedPackage.CloneProducts(packageButton.Package, m_sellingForm, this);

                    //create the package button
                    PackageButton discountPackageButton = new PackageButton(this, clonedPackage, packageButton.Session);

                    buttonList.Add(discountPackageButton);
                }
            }

            return buttonList;
        }

        //US5328
        /// <summary>
        /// Verifies the game maximum card totals.
        /// </summary>
        /// <param name="salesCardCountPerGameCategory">The sales card count per game category.</param>
        /// <returns></returns>
        /// <exception cref="POSException">
        /// </exception>
        internal void VerifyGameMaxCardTotals(Dictionary<int, int> salesCardCountPerGameCategory)
        {
            //go through game categories per game
            //Key: Session Game Played ID; Value: List of Game Category ID's
            foreach (var gameToGameCategoriesPair in CurrentSession.GameToGameCategoriesDictionary)
            {
                var gameMaxCardLimit = 0;
                var salesCardCountPerGame = 0;

                //get max card limit per game and the sales card count per game
                foreach (var gameCategoryId in gameToGameCategoriesPair.Value) //all the categories in this game
                {
                    //calculate sales card count per game
                    if (salesCardCountPerGameCategory.ContainsKey(gameCategoryId))
                        salesCardCountPerGame += salesCardCountPerGameCategory[gameCategoryId]; //add the cards sold for this category
                    
                    //Get the game category
                    var gameCategory = CurrentSession.GameCategoryList.FirstOrDefault(g => g.Id == gameCategoryId);

                    //this should never be null.
                    if (gameCategory == null)
                        throw new POSException(string.Format("Unable to find game category ID {0}", gameCategoryId));

                    //get default max card limit.
                    var defaultMaxCardLimit = Math.Min(CurrentSession.SessionMaxCardLimit, Settings.MaxCardLimit);

                    //check to see if the game category max card limit is set
                    if (gameCategory.MaxCardLimit == 0 || gameCategory.MaxCardLimit > defaultMaxCardLimit)
                        gameMaxCardLimit += defaultMaxCardLimit;
                    else
                        gameMaxCardLimit += gameCategory.MaxCardLimit;
                }

                //check to see if max card limit is exceeded
                if (salesCardCountPerGame > gameMaxCardLimit)
                    throw new POSException(Resources.MaxCardLimitReached3);
            }
        }

        #endregion

        #region Static Methods
        /// <summary>
        /// Returns a string with the version and copyright information of 
        /// the POS.
        /// </summary>
        /// <param name="justVersion">true if just the version is to be 
        /// returned; otherwise false.</param>
        /// <returns>A string with the version and optionally the copyright 
        /// information.</returns>
        public static string GetVersionAndCopyright(bool justVersion)
        {
            // Get version.
            string version = Resources.Version +
                " " + Assembly.GetExecutingAssembly().GetName().Version.Major.ToString(CultureInfo.InvariantCulture) +
                "." + Assembly.GetExecutingAssembly().GetName().Version.Minor.ToString(CultureInfo.InvariantCulture) +
                "." + Assembly.GetExecutingAssembly().GetName().Version.Build.ToString(CultureInfo.InvariantCulture) +
                "." + Assembly.GetExecutingAssembly().GetName().Version.Revision.ToString(CultureInfo.InvariantCulture);

            // TTP 50114
            #if DEBUG
            version += "d";
            #endif

            // Get copyright.
            if(!justVersion)
            {
                string copyright = string.Empty;

                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);

                if(attributes.Length > 0)
                    copyright = ((AssemblyCopyrightAttribute)attributes[0]).Copyright;

                return version + " - " + copyright;
            }
            else
                return version;
        }

        /// <summary>
        /// Based on the exception passed in, this method will translate
        /// the error message to localized text and rethrow the exception as
        /// a POSException.
        /// </summary>
        /// <param name="ex">The exception to reformat.</param>
        internal static void ReformatException(Exception ex)
        {
            throw new POSException(ServerExceptionTranslator.FormatExceptionMessage(ex), ex);
        }

        // FIX: TA4779
        /// <summary>
        /// Gets the specified operator from the server.
        /// </summary>
        /// <param name="operatorId">The id of the operator to 
        /// retrieve.</param>
        /// <returns>An Operator object.</returns>
        private static Operator GetOperator(int operatorId)
        {
            GetOperatorCompleteMessage opDataMsg = new GetOperatorCompleteMessage(operatorId);

            try
            {
                opDataMsg.Send();
            }
            catch(Exception e)
            {
                ReformatException(e);
            }

            Operator newOp = new Operator();
            newOp.Id = operatorId;
            newOp.Name = opDataMsg.OperatorList[0].Name;
            newOp.Address1 = opDataMsg.OperatorList[0].Address1;
            newOp.Address2 = opDataMsg.OperatorList[0].Address2;
            newOp.City = opDataMsg.OperatorList[0].City;
            newOp.State = opDataMsg.OperatorList[0].State;
            newOp.Zip = opDataMsg.OperatorList[0].Zip;
            newOp.Phone = opDataMsg.OperatorList[0].Phone;

            newOp.CashMethodID = opDataMsg.OperatorList[0].CashMethodID; // Rally TA7465
            newOp.FixedDeviceFee = opDataMsg.OperatorList[0].FixedDeviceFee;
            newOp.TravelerDeviceFee = opDataMsg.OperatorList[0].TravelerDeviceFee;
            newOp.TrackerDeviceFee = opDataMsg.OperatorList[0].TrackerDeviceFee;
            newOp.ExplorerDeviceFee = opDataMsg.OperatorList[0].ExplorerDeviceFee; // Rally TA7729
            newOp.Traveler2DeviceFee = opDataMsg.OperatorList[0].Traveler2DeviceFee; // PDTS 964, Rally US765
            newOp.TabletDeviceFee = opDataMsg.OperatorList[0].TabletDeviceFee;

            return newOp;
        }
        // END: TA4779

        /// <summary>
        /// Gets the gaming date from the server.
        /// </summary>
        /// <param name="operatorId">The id of the current operator.</param>
        private static DateTime GetGamingDate(int operatorId)
        {
            GetGamingDateMessage gameDateMsg = new GetGamingDateMessage(operatorId);

            try
            {
                gameDateMsg.Send();
            }
            catch(ServerCommException)
            {
                throw; // Don't repackage the ServerCommException.
            }
            catch(Exception e)
            {
                ReformatException(e);
            }

            return gameDateMsg.GamingDate;
        }

        private static List<SessionCharity> GetCharityData(int receiptId)
        {
            return GetSessionCharityDataMessage.GetList(receiptId);
        }

        #endregion

        #region Member Properties

        /// <summary>
        /// Returns a GuardianWrapper object.  Waits for a connection to the Guardian suspending while waiting.
        /// </summary>
        public GuardianWrapper Guardian
        {
            get
            {
                if (m_Guardian != null && !m_Guardian.ConnectedToGuardian)
                    WaitForGuardianConnection();

                return m_Guardian;
            }
        }

        public bool WeHaveAGuardian
        {
            get
            {
                return m_Guardian != null;
            }
        }

        /// <summary>
        /// Gets whether the PointOfSale was initialized.
        /// </summary>
        public bool IsInitialized
        {
            get
            {
                return m_initialized;
            }
        }

        /// <summary>
        /// Gets the selling form.
        /// </summary>
        public SellingForm SellingForm
        {
            get
            {
                return m_sellingForm;
            }
        }

        /// <summary>
        /// Gets whether the receipt manager is present and running.
        /// </summary>
        public bool IsReceiptMgmtInitialized
        {
            get
            {
                return (m_receiptManager != null && m_receiptManager.IsInitialized);
            }
        }

        /// <summary>
        /// Gets whether the player center is present and running.
        /// </summary>
        public bool IsPlayerCenterInitialized
        {
            
            get
            {
                return (m_playerCenter != null && m_playerCenter.IsInitialized);
            }
        }

        /// <summary>
        /// Gets whether unit management is present and running.
        /// </summary>
        public bool IsUnitMgmtInitialized
        {
            get
            {
                return (m_unitMgmt != null && m_unitMgmt.IsInitialized);
            }
        }

        /// <summary>
        /// Gets the unit management instance.
        /// </summary>
        /// <exception cref="GTI.Modules.POS.Business.POSException">UnitMgmt is
        /// not initialized.</exception>
        public UnitManager UnitMgmt
        {
            get
            {
                if(!IsUnitMgmtInitialized)
                    throw new POSException(Resources.CrateModuleNoConnection);

                return m_unitMgmt;
            }
        }

        /// <summary>
        /// Gets the POS's Crystal Ball Manager instance.
        /// </summary>
        public CrystalBallManager CrystalBallManager
        {
            get
            {
                return m_cbbManager;
            }
        }

        public string MachineDesc
        {
            get
            {
                return m_machineDesc;
            }
        }

        /// <summary>
        /// Gets the POS's current settings.
        /// </summary>
        public POSSettings Settings
        {
            get
            {
                return m_settings;
            }
        }

        /// <summary>
        /// Gets or sets the current gaming date.
        /// </summary>
        internal DateTime GamingDate
        {
            get
            {
                lock(m_gamingDateSync)
                {
                    return m_gamingDate;
                }
            }
            set
            {
                lock(m_gamingDateSync)
                {
                    m_gamingDate = value;
                }
            }
        }

        public Dictionary<int, Charity> CharityData
        {
            get
            {
                return m_charities;
            }
        }

        /// <summary>
        /// Gets the current operator.
        /// </summary>
        public Operator CurrentOperator
        {
            get
            {
                return m_currentOp;
            }
        }

        /// <summary>
        /// Gets the current staff.
        /// </summary>
        public Staff CurrentStaff
        {
            get
            {
                return m_currentStaff;
            }
        }

        /// <summary>
        /// Gets the card levels defined for the current operator.
        /// </summary>
        public CardLevel[] CardLevels
        {
            get
            {
                return m_cardLevels;
            }
        }

        /// <summary>
        /// Gets or sets whether the menus can be updated at this time.
        /// </summary>
        public bool CanUpdateMenus
        {
            get
            {
                lock(m_menuSync)
                {
                    return m_canUpdateMenus;
                }
            }
            set
            {
                lock(m_menuSync)
                {
                    m_canUpdateMenus = value;
                }
            }
        }

        /// <summary>
        /// Gets the current active menu.
        /// </summary>
        public POSMenu CurrentMenu
        {
            get
            {
                lock(m_menuSync)
                {
                    if(m_currentMenuIndex >= 0 && m_currentMenuIndex < m_menuList.Length)
                        return m_menuList[m_currentMenuIndex].Menu;
                    else
                        return null;
                }
            }
        }

        public int CurrentMenuIndex
        {
            get { return m_currentMenuIndex; }
        }

        /// <summary>
        /// Gets the current active session (or 0 for no session).
        /// </summary>
        public SessionInfo CurrentSession
        {
            get
            {
                lock(m_menuSync)
                {
                    if(m_currentMenuIndex >= 0 && m_currentMenuIndex < m_menuList.Length)
                        return m_menuList[m_currentMenuIndex].Session;
                    else
                        return null;
                }
            }
        }

        public SessionInfo ActiveSalesSession { get; private set; }

        /// <summary>
        /// Gets the current active session played id (or 0 for no session).
        /// </summary>
        public int CurrentSessionPlayedId
        {
            get
            {
                lock(m_menuSync)
                {
                    if(m_currentMenuIndex >= 0 && m_currentMenuIndex < m_menuList.Length)
                        return CurrentSession.SessionPlayedId;
                    else
                        return 0;
                }
            }
        }

        // Rally TA7465
        /// <summary>
        /// Gets the current bank for this POS.
        /// </summary>
        public Bank Bank
        {
            get
            {
                return m_bank;
            }
        }

        /// <summary>
        /// Gets the system default currency.
        /// </summary>
        public Currency DefaultCurrency
        {
            get
            {
                return m_defaultCurrency;
            }
        }

        /// <summary>
        /// Gets the list of non-default currencies.
        /// </summary>
        public IList<Currency> Currencies
        {
            get
            {
                return m_currencies;
            }
        }

        /// <summary>
        /// Gets the current sale currency.
        /// </summary>
        public Currency CurrentCurrency
        {
            get
            {
                return m_currentCurrency;
            }
        }
            // END: TA7465

        /// <summary>
        /// Gets the current active sale or null if no sale has been started.
        /// </summary>
        public Sale CurrentSale
        {
            get
            {
                return m_currentSale;
            }
        }

        /// <summary>
        /// Gets or sets whether the change due was dispensed.
        /// </summary>
        public bool KioskChangeDispensingFailed
        {
            get
            {
                return m_kioskChangeDispensingFailure;
            }

            set
            {
                m_kioskChangeDispensingFailure = value;
            }
        }

        /// <summary>
        /// Gets or sets the current player's coupon form.
        /// </summary>
        public PayCouponForm4 CurrentCouponForm
        {
            get
            {
                return m_couponForm;
            }

            set
            {
                m_couponForm = value;
            }
        }

        /// <summary>
        /// Gets whether there are no items in the current sale.
        /// </summary>
        public bool IsSaleEmpty
        {
            get
            {
                return (CurrentSale == null || CurrentSale.ItemCount == 0);
            }
        }

        public bool SaleHasTenders
        {
            get
            {
                return CurrentSale != null && CurrentSale.GetCurrentTenders().Count != 0;
            }
        }

        /// <summary>
        /// Gets whether the B3 menu is active.
        /// </summary>
        public bool IsB3Sale
        {
            get
            {
                return CurrentMenu != null && CurrentMenu.Name == Resources.B3SessionString;
            }
        }

        /// <summary>
        /// Gets state of the current sale.
        /// </summary>
        public SellingState SaleState
        {
            get
            {
                return m_sellingState;
            }
        }

        // PDTS 1064
        /// <summary>
        /// Gets the MagneticCardReader instance for the POS.
        /// </summary>
        public MagneticCardReader MagCardReader
        {
            get
            {
                return m_magCardReader;
            }
        }

        public BarcodeReader BarcodeScanner
        {
            get
            {
                return m_barcodeReader;
            }
        }

        /// <summary>
        /// Returns if there is a last sale in memory to repeat.
        /// </summary>
        public bool CanDoRepeatSale
        {
            get
            {
                return m_lastSale != null;
            }
        }

        /// <summary>
        /// Gets or sets the last sales receipt.
        /// </summary>
        public SalesReceipt LastReceipt
        {
            get
            {
                return m_lastReceipt;
            }
            set
            {
                m_lastReceipt = value;
            }
        }

        // Rally US505
        /// <summary>
        /// Gets or sets whether to print a Play-It sheet with the last
        /// receipt.
        /// </summary>
        public bool LastReceiptHasPlayIt
        {
            get
            {
                return m_lastReceiptHasPlayIt;
            }
            set
            {
                m_lastReceiptHasPlayIt = value;
            }
        }

        public Dictionary<int, string> SubTenderName
        {
            get
            {
                return m_subTenderNames;
            }
        }

        // FIX: DE2538
        /// <summary>
        /// Gets the PointOfSale's current background worker.
        /// </summary>
        public BackgroundWorker Worker
        {
            get
            {
                return m_worker;
            }
        }
        // END: DE2538
        
        /// <summary>
        /// Gets or sets the last exception that was thrown by another thread.
        /// </summary>
        public Exception LastAsyncException
        {
            get
            {
                lock(m_errorSync)
                {
                    return m_asyncException;
                }
            }
            set
            {
                lock(m_errorSync)
                {
                    m_asyncException = value;
                }
            }
        }

        public MenuButton GetProductDataButton
        {
            get;
            set;
        }

        public object[] GetProductDataButtonValues
        {
            get;
            set;
        }

        public bool WeHaveAStarPrinter
        {
            get
            {
                return m_weHaveAStarPrinter;
            }
        }

        public bool WeAreABuyAgainKiosk
        {
            get
            {
                return m_deviceId == Device.BuyAgainKiosk.Id;
            }
        }

        public bool WeAreAnAdvancedPOSKiosk
        {
            get
            {
                return m_deviceId == Device.AdvancedPOSKiosk.Id;
            }
        }

        public bool WeAreANonAdvancedPOSKiosk
        {
            get
            {
                return WeAreAPOSKiosk && !WeAreAnAdvancedPOSKiosk;
            }
        }

        public bool WeAreASimplePOSKiosk
        {
            get
            {
                return m_deviceId == Device.SimplePOSKiosk.Id;
            }
        }
        
        public bool WeAreAHybridKiosk
        {
            get
            {
                return m_deviceId == Device.HybridKiosk.Id;
            }
        }

        public bool WeAreAB3Kiosk
        {
            get
            {
                return m_deviceId == Device.B3Kiosk.Id;
            }
        }

        public bool WeAreANonB3Kiosk
        {
            get
            {
                return WeAreAPOSKiosk && !WeAreAB3Kiosk;
            }
        }

        public bool WeAreAPOSKiosk
        {
            get
            {
                return WeAreAB3Kiosk || WeAreAnAdvancedPOSKiosk || WeAreASimplePOSKiosk || WeAreABuyAgainKiosk || WeAreAHybridKiosk;
            }
        }

        public bool WeAreNotAPOSKiosk
        {
            get
            {
                return !WeAreAPOSKiosk;
            }
        }

        public bool WeCanSellForB3AtKiosk
        {
            get
            {
                return Settings.AllowB3OnKiosk && m_b3SessionActive;
            }
        }

        public bool B3SessionActive
        {
            get
            {
                return m_b3SessionActive;
            }
        }

        /// <summary>
        /// The Guardian has requested control.  POS should hide itself and wait for the Guardian.
        /// </summary>
        public bool ClearScreenForGuardian
        {
            get
            {
                return m_clearScreenForGuardian;
            }
        }

        /// <summary>
        /// The Guardian has requested control.  Timers should stop.
        /// </summary>
        public bool GuardianHasUsSuspended
        {
            get
            {
                return m_GuardianIsSuspending;
            }
        }

        public bool ShuttingDown
        {
            get
            {
                return m_shuttingDown;
            }
        }

        public WaitForm POSWaitForm
        {
            get
            {
                return m_waitForm;
            }
        }

        /// <summary>
        /// Gets or sets whether a tender is being processed.
        /// </summary>
        public bool ProcessingTender
        {
            get
            {
                return m_processingTender;
            }

            set
            {
                m_processingTender = value;
            }
        }

        /// <summary>
        /// Gets whether the controller is performing a long running operation.
        /// 
        /// Note: this is used when performing asynch message calls.
        /// </summary>
        public bool IsBusy
        {
            get
            {
                return m_isBusy;
            }

            internal set
            {
                if (m_isBusy != value)
                {
                    m_isBusy = value;
                    EventHandler<BusyChangedEventArgs> handler = BusyStatusChanged;
                    if (handler != null)
                        handler(this, new BusyChangedEventArgs(value));
                }
            }
        }

        public WaitForm BlockingWaitForm
        {
            get
            {
                return m_waitForm;
            }
        }

        // Rally TA5748
        /// <summary>
        /// Gets or sets the last list of start numbers received from the
        /// server.
        /// </summary>
        public List<ProductStartNumbers> LastStartNumbers
        {
            get;
            set;
        }
        // END: TA5748

        public bool TenderingScreenActive
        {
            get
            {
                return m_tenderingScreenActive;
            }

            set
            {
                m_tenderingScreenActive = value;
            }
        }

        public bool CouponScreenActive
        {
            get
            {
                return m_couponScreenActive;
            }

            set
            {
                m_couponScreenActive = value;
            }
        }

        public bool HelpScreenActive
        {
            get
            {
                return m_helpScreenActive;
            }

            set
            {
                m_helpScreenActive = value;
            }
        }

        public bool HaveMenu
        {
            get
            {
                return m_menuList != null && m_menuList.Length > 0 && !(m_menuList.Length == 1 && m_menuList[0].Menu.Name == Resources.BingoSalesClosed);
            }
        }

        public bool HaveBingoMenu
        {
            get
            {
                if (HaveMenu && m_menuList[0].Menu.Name == Resources.BingoSalesClosed)
                    return false;

                return m_menuList != null && m_menuList.Length > (m_b3MenuListItem != null ? 1 : 0);
            }
        }

        public int IndexOfFirstBingoMenu
        {
            get
            {
                return 0; //m_b3MenuListItem != null ? 1 : 0;
            }
        }

        //US3509
        public bool ValidationEnabled { get; set; }

        // TODO Revisit Super Pick properties.
        /*
        /// <summary>
        /// Gets or sets the results of the last Find super pick call.
        /// </summary>
        public SuperPickListItem[] LastFindSuperPicksResults
        {
            get
            {
                lock (m_findSuperPickSync)
                {
                    return m_lastFindSuperPicksResults;
                }
            }
            set
            {
                lock (m_findSuperPickSync)
                {
                    m_lastFindSuperPicksResults = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the results of the last Find super pick receipt's transaction number.
        /// </summary>
        public int LastSupetPickTransactionNumber
        {
            get
            {
                return m_lastSuperPickTransactionNumber;
            }
            set
            {
                m_lastSuperPickTransactionNumber = value;
            }
        }

        /// <summary>
        /// Gets or sets the results of the last Find super pick player's number.
        /// </summary>
        public string LastSupetPickPlayerNumber
        {
            get
            {
                return m_lastSuperPickPlayerNumber;
            }
            set
            {
                m_lastSuperPickPlayerNumber = value;
            }
        }

        /// <summary>
        /// Gets or sets the results of the last Find super pick player's first name.
        /// </summary>
        public string LastSuperPickPlayerFName
        {
            get
            {
                return m_lastSuperPickPlayerFName;
            }
            set
            {
                m_lastSuperPickPlayerFName = value;
            }
        }

        /// <summary>
        /// Gets or sets the results of the last Find super pick player's last name.
        /// </summary>
        public string LastSuperPickPlayerLName
        {
            get
            {
                return m_lastSuperPickPlayerLName;
            }
            set
            {
                m_lastSuperPickPlayerLName = value;
            }
        }

        /// <summary>
        /// Gets or sets the results of the last Find super pick player's pic.
        /// </summary>
        public Bitmap LastSuperPickPlayerPic
        {
            get
            {
                lock (m_findSuperPickPlayerSync)
                {
                    return m_lastSuperPickPlayerPic;
                }
            }
            set
            {
                lock (m_findSuperPickPlayerSync)
                {
                    m_lastSuperPickPlayerPic = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the results of the last super pick winner list.
        /// </summary>
        public ArrayList LastSuperPickWinnerList
        {
            get
            {
                return m_lastSuperPickWinnerList;
            }
            set
            {
                m_lastSuperPickWinnerList = value;
            }
        }

        /// <summary>
        /// Gets or sets the results of the last pay super pick call.
        /// </summary>
        public  PaySuperPickListItem[] LastPaySuperPicksResults
        {
            get
            {
                lock (m_paySuperPickSync)
                {
                    return m_lastPaySuperPicksResults;
                }
            }
            set
            {
                lock (m_paySuperPickSync)
                {
                    m_lastPaySuperPicksResults = value;
                }
            }
        }
        */

        /// <summary>
        /// Gets the menu list item.
        /// </summary>
        /// <param name="sessionNumber">The session.</param>
        /// <param name="gamingDate">Gaming Date</param>
        /// <returns></returns>
        public POSMenuListItem GetMenuListItem(int sessionNumber, DateTime gamingDate)
        {
            lock (m_menuSync)
            {
                foreach (var posMenuListItem in m_menuList)
                {
                    if (posMenuListItem.Session.SessionNumber == sessionNumber && posMenuListItem.Session.GamingDate == gamingDate)
                    {
                        return posMenuListItem;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the machine identifier.
        /// </summary>
        /// <value>
        /// The machine identifier.
        /// </value>
        public int MachineId
        {
            get
            {
                return m_machineId;
            }
        }

        /// <summary>
        /// Gets the screen we started on.
        /// </summary>
        public Screen OurScreen
        {
            get
            {
                return m_ourScreen;
            }
        }

        /// <summary>
        /// Creates a thread to save a tender record on the server.    
        /// </summary>
        /// </summary>
        /// <param name="saleTender">Tender information to save.</param>
        internal void StartAddTenderToTable(SaleTender saleTender)
        {
            RunWorker(Resources.WaitFormAddingTender
                , new DoWorkEventHandler(SendAddTenderToTable)
                , (object)saleTender
                , new RunWorkerCompletedEventHandler(SendAddTenderToTableComplete));
        }

        /// <summary>
        /// Saves a tender record on the server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">SaleTender object.</param>
        private void SendAddTenderToTable(object sender, DoWorkEventArgs e)
        {
            SetupThread();

            // Unpack our variable
            SaleTender saleTender = e.Argument as SaleTender;
                
            bool updateTransactionNumber = saleTender.RegisterReceiptID == 0;

            //save the tender
            SetReceiptTenderMessage tenderMsg = new SetReceiptTenderMessage(saleTender);

            try
            {
                tenderMsg.Send();
            }
            catch (Exception)
            {
                throw;
            }

            if (updateTransactionNumber && CurrentSale != null)
            {
                lock (CurrentSale)
                {
                    CurrentSale.ReceiptNumber = tenderMsg.TransactionNumber;
                }
            }

            e.Result = saleTender;
        }

        /// <summary>
        /// Completion routine for SendSaveTender.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SendAddTenderToTableComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            // Set the error that occurred (if any).
            LastAsyncException = e.Error; 

            //any error here is fatal if we are a kiosk
            //I have no idea why the server would not process this - critical error, shutdown and refund money.

            // Close the wait form.
            m_waitForm.CloseForm();
        }

        /// Creates a thread to add a sale tender with the server
        /// </summary>
        /// <param name="magCardNumber">The mag. card number of the player to 
        /// look for.</param>
        internal void StartAddSaleTender(TenderType tenderType, decimal amount, Currency currency, decimal tax)
        {
            RunWorker(Resources.WaitFormAddingTender
                , new DoWorkEventHandler(SendTender)
                , (object)Tuple.Create(tenderType, amount, currency, tax)
                , new RunWorkerCompletedEventHandler(SendTenderComplete));
        }

        internal void StartVoidSaleTender(ListBoxTenderItem tender)
        {
            RunWorker(Resources.WaitFormAddingTender
                , new DoWorkEventHandler(SendTender)
                , (object)tender
                , new RunWorkerCompletedEventHandler(SendTenderComplete));
        }

        /// <summary>
        /// Gets a player's data from the server.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The DoWorkEventArgs object that 
        /// contains the event data.</param>
        private void SendTender(object sender, DoWorkEventArgs e)
        {
            SetupThread();

            // Unpack our variable
            TenderType tenderType;
            decimal amountTenderedInCurrentCurrency;
            decimal taxInDefaultCurrency;
            Currency currentCurrency;
            string reference = string.Empty;

            ListBoxTenderItem lbti = e.Argument as ListBoxTenderItem;

            if (lbti != null) //void
            {
                tenderType = lbti.Type;
                amountTenderedInCurrentCurrency = -lbti.TenderItemObject.TenderedCurrencyAmount;
                currentCurrency = lbti.TenderItemObject.TenderedCurrency;
                reference = lbti.TenderItemObject.ProcessingInfo.RefCode;
                taxInDefaultCurrency = lbti.Tax;
            }
            else
            {
                var args = (Tuple<TenderType, decimal, Currency, decimal>)e.Argument;
                
                tenderType = args.Item1;
                amountTenderedInCurrentCurrency = args.Item2;
                currentCurrency = args.Item3;
                taxInDefaultCurrency = args.Item4;
            }
            
            bool voiding = amountTenderedInCurrentCurrency < 0;
            decimal amount = Math.Abs(currentCurrency.ConvertFromThisCurrencyToDefaultCurrency(amountTenderedInCurrentCurrency));

            e.Result = null;

            m_worker.ReportProgress(0, "Processing " + TenderTypeToString(tenderType) + " transaction for " + DefaultCurrency.FormatCurrencyString(amount));

            if (!Settings.PaymentProcessingEnabled)
            {
                m_lastTenderProcessingReply.InitDefaultSuccess(Settings.GetTenderName(tenderType), amount);
            }
            else //go through the payment processor
            {
                switch (tenderType)
                {
                    case TenderType.CreditCard: //if credit and debit allowed, this could be either
                    {
                        var processor = EliteCreditCardFactory.Instance;
                        if (processor != null)
                        {
                            CreditCardProcessingReply reply = null;

                            if (!voiding)
                            {
                                AcceptableTenderTypes acceptedTypes = AcceptableTenderTypes.CreditCard;

                                if (!string.IsNullOrEmpty(Settings.ValidPOSTenders.Find((x) => x.TenderTypeID == (int)TenderType.DebitCard).TenderName)) //we allow debit and credit, let patron decide
                                    acceptedTypes |= AcceptableTenderTypes.DebitCard;

                                // Add all items to the sale request that 
                                // may be needed by payment processor.
                                ProcessorSaleRequestData saleRequestData =
                                    new ProcessorSaleRequestData()
                                    {
                                        RequestAmount = amount,
                                        Tax = taxInDefaultCurrency,
                                        TenderTypesAllowed = acceptedTypes,
                                        TransactionNumber = m_currentSale.ReceiptNumber
                                    };

                                reply = processor.RequestSale(saleRequestData,
                                (x) =>
                                {
                                    m_worker.ReportProgress(0, x);
                                });
                            }
                            else
                            {
                                // Add all items to the void request that 
                                // may be needed by payment processor.
                                ProcessorVoidRequestData voidRequestData =
                                    new ProcessorVoidRequestData()
                                    {
                                        RequestAmount = amount,
                                        Tax = taxInDefaultCurrency,
                                        ReferenceNumber = reference,
                                        TransactionNumber = m_currentSale.ReceiptNumber,
                                        ForceVoid = true,
                                        ForceRefund = false
                                    };

                                reply = processor.RequestVoidOrRefund(voidRequestData,
                                (x) =>
                                {
                                    m_worker.ReportProgress(0, x);
                                });
                            }

                            if (reply == null)
                            {
                                reply = new CreditCardProcessingReply();
                                reply.Message = "Error";
                            }

                            m_lastTenderProcessingReply.CopyReply(reply);
                        }
                        else
                        {
                            throw new Exception(Resources.FailedToInitializePaymentProcessor);
                        }
                    }
                    break;

                    case TenderType.DebitCard:
                    {
                        var processor = EliteCreditCardFactory.Instance;
                        if (processor != null)
                        {
                            CreditCardProcessingReply reply = null;

                            if (!voiding)
                            {
                                // Add all items to the sale request that 
                                // may be needed by ANY processor.
                                ProcessorSaleRequestData saleRequestData =
                                    new ProcessorSaleRequestData()
                                    {
                                        RequestAmount = amount,
                                        Tax = taxInDefaultCurrency,
                                        TenderTypesAllowed = AcceptableTenderTypes.DebitCard,
                                        TransactionNumber = m_currentSale.ReceiptNumber
                                    };

                                reply = processor.RequestSale(saleRequestData,
                                (x) =>
                                {
                                    m_worker.ReportProgress(0, x);
                                });
                            }
                            else
                            {
                                // Add all items to the void request that 
                                // may be needed by ANY processor.
                                ProcessorVoidRequestData voidRequestData =
                                    new ProcessorVoidRequestData()
                                    {
                                        RequestAmount = amount,
                                        Tax = taxInDefaultCurrency,
                                        ReferenceNumber = reference,
                                        TransactionNumber = m_currentSale.ReceiptNumber,
                                        ForceRefund = false,
                                        ForceVoid = true
                                    };

                                reply = processor.RequestVoidOrRefund(voidRequestData,
                                (x) =>
                                {
                                    m_worker.ReportProgress(0, x);
                                });
                            }

                            if (reply == null)
                            {
                                reply = new CreditCardProcessingReply();
                                reply.Message = "Error";
                            }

                            m_lastTenderProcessingReply.CopyReply(reply);
                        }
                        else
                        {
                            throw new Exception(Resources.FailedToInitializePaymentProcessor);
                        }
                    }
                    break;

                    default:
                    {
                        m_lastTenderProcessingReply.InitDefaultSuccess(Settings.GetTenderName(tenderType), amount);
                    }
                    break;
                }
            }

            m_lastTenderProcessingReply.AmountRequested = amount;

            if (!m_lastTenderProcessingReply.IsError)
            {
                TenderItem newItem;

                if (Math.Abs(amount) != Math.Abs(m_lastTenderProcessingReply.AmountAuthorized)) //partial approval
                    newItem = new TenderItem(m_currentSale, (voiding ? -1M : 1M) * Math.Abs(m_lastTenderProcessingReply.AmountAuthorized), currentCurrency, currentCurrency.ConvertFromDefaultCurrencyToThisCurrency((voiding ? -1M : 1M) * Math.Abs(m_lastTenderProcessingReply.AmountAuthorized)));
                else
                    newItem = new TenderItem(m_currentSale, (voiding ? -1M : 1M) * Math.Abs(m_lastTenderProcessingReply.AmountAuthorized), currentCurrency, (voiding ? -1M : 1M) * Math.Abs(amountTenderedInCurrentCurrency));

                newItem.Type = tenderType;
                newItem.ProcessingInfo.CopyReply(m_lastTenderProcessingReply);
                m_currentSale.AddTender(newItem);
                e.Result = newItem;
            }
            else //error
            {
                TenderItem newItem = new TenderItem(m_currentSale, 0, currentCurrency, 0, new SaleTender(0, CurrentSale.Id, DateTime.Now, tenderType, 0, (voiding ? TransactionType.Void : TransactionType.Sale), currentCurrency.ISOCode, 0, 0, 0, m_lastTenderProcessingReply.RefCode, m_lastTenderProcessingReply.AuthCode, "Error", (voiding ? lbti.TenderItemObject.SaleTenderInfo.RegisterReceiptTenderID : 0), currentCurrency.ExchangeRate, m_lastTenderProcessingReply.CustomerText, m_lastTenderProcessingReply.MerchantText, m_lastTenderProcessingReply.Message));
                newItem.ProcessingInfo.CopyReply(m_lastTenderProcessingReply);
                m_currentSale.AddTender(newItem);
                e.Result = newItem;
            }       
        }

        /// <summary>
        /// Event handler called when a notice is received from the processor.
        /// </summary>
        /// <param name="noticeString"></param>
        private void HandleProcessorNotice(string noticeString)
        {
        }

        /// <summary>
        /// Handles the event when the reload menus/buttons BackgroundWorker 
        /// is complete.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The RunWorkerCompletedEventArgs object that 
        /// contains the event data.</param>
        private void SendTenderComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            // Set the error that occurred (if any).
            LastAsyncException = e.Error;

            if (e.Error == null)
                m_lastTenderItem = (TenderItem)e.Result;
            else
                m_lastTenderItem = new TenderItem(null, 0, null, 0);

            if (Settings.PaymentProcessingEnabled)
            {
                var processor = EliteCreditCardFactory.Instance;
                if (processor != null)
                {
                    while (!processor.ReadyToProcessPayment)
                        Application.DoEvents();   
                }                
            }

            // Close the wait form.
            m_waitForm.CloseForm();
        }

        /// <summary>
        /// Returns a string representation of a tender type
        /// </summary>
        /// <param name="tenderType"></param>
        /// <returns></returns>
        public static string TenderTypeToString(TenderType tenderType)
        {
            switch (tenderType)
            {
                case TenderType.Cash: return "Cash";
                case TenderType.Check: return "Check";
                case TenderType.Chip: return "Chip";
                case TenderType.Coupon: return "Coupon";
                case TenderType.CreditCard: return "Credit Card";
                case TenderType.DebitCard: return "Debit Card";
                case TenderType.GiftCard: return "Gift Card";
                case TenderType.MoneyOrder: return "Money Order";
                default: return "Unknown";
            }
        }

        public DialogResult ShowMessage(IWin32Window owner, DisplayMode displayMode, string text, string caption, POSMessageFormTypes type = POSMessageFormTypes.OK, int pause = 0)
        {
            if (m_sellingForm != null)
            {
                m_sellingForm.NotIdle(true);

                if (SellingForm.KioskForm != null)
                    owner = SellingForm.KioskForm;
            }

            bool wait = m_waitForm != null? m_waitForm.Visible : false;

            if(wait)
                m_waitForm.Visible = false;

            DialogResult result = POSMessageForm.Show(owner, this, text, caption, type, pause);

            if (wait)
                m_waitForm.Visible = true;

            if (m_sellingForm != null)
                m_sellingForm.NotIdle();

            return result;
        }

        public DialogResult ShowMessage(IWin32Window owner, DisplayMode displayMode, string text, POSMessageFormTypes type = POSMessageFormTypes.OK, int pause = 0)
        {
            if (m_sellingForm != null)
            {
                m_sellingForm.NotIdle(true);

                if (SellingForm.KioskForm != null)
                    owner = SellingForm.KioskForm;
            }

            bool wait = m_waitForm != null ? m_waitForm.Visible : false;

            if (wait)
                m_waitForm.Visible = false;

            DialogResult result = POSMessageForm.Show(owner, this, text, type, pause);

            if (wait)
                m_waitForm.Visible = true;

            if (m_sellingForm != null)
                m_sellingForm.NotIdle();

            return result;
        }
        #endregion
    }
}