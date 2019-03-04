#region Copyright
// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2013 FortuNet, Inc.
//
// US2828 Adding support for allowing the user to set
//  an active session for sales
// US4028 Adding support for checking card counts when each product is
//  added
//US4338: (US1592) POS: Redeem B3 Receipt requires to get B3 settings for double account flag
//US4698: POS: Denomination receipt
#endregion

using System;
using System.Globalization;
using GTI.Modules.Shared;
using GTI.Modules.POS.Data;
using GTI.Modules.POS.Properties;
using System.Collections.Generic;
using System.Text;
using GTI.EliteCreditCards.Interfaces;
using GTI.EliteCreditCards.Data;
using GTI.GTIDevices.ExternalDevices;
using System.Net;

namespace GTI.Modules.POS.Business
{
    /// <summary>
    /// Contains all the different settings for the POS module.
    /// </summary>
    internal class POSSettings : IEliteCreditCardSettings
    {
        public enum SellAgainOption
        {
            Disallow = 0,
            Allow = 1,
            Ask =2
        }

        public enum OptionsForGivingChange
        {
            Normal = 0,
            B3Credit = 1,
            B3CreditOrNormal = 2,
            B3CreditWithAddOn = 3,
            B3CreditWithAddOnOrNormal = 4
        }

        private enum KioskTestMode //max value of 31
        {                                       //Coresponding letter after Guardian IP:Port- 
            BillAcceptor = 0,                   //   A
            BillDispenser = 1,                  //   D
            TreatTicketsAs20DollarBills = 2,    //   T
            NoB3Server = 3,                     //   B
            OneMonitor = 4,                     //   1
            NoGuardian = 5                      //   G (forces A and D)
        }

        #region Member Variables
        protected object m_syncRoot = new object();
        protected DisplayMode m_displayMode;
        protected MSRSettings m_MSRSettings = new MSRSettings();
        protected bool m_showCursor;
        protected string m_receiptPrinterName;
        protected byte[] m_drawerCode;
        protected byte[] m_drawer2Code;
        protected string m_crateServerName;
        protected TenderSalesMode m_tender;
        protected string m_dbServer;
        protected string m_dbName;
        protected string m_dbUser;
        protected string m_dbPassword;
        protected bool m_forceEnglish;
        private bool m_swipeEntersRaffle; // Rally US658
        protected bool m_enableLogging;
        protected int m_cbbScannerPort;
        protected string m_cbbSheetDef;
        protected bool m_cbbEnableFavorites; //US2418
        protected bool m_allowNoSale;
        protected bool m_allowReturns;
        protected bool m_printNonElecReceipt;
        protected bool m_promptForPlayerCreation; // PDTS 1044
        protected CBBPlayItSheetType m_cbbPlayItSheetType; // Rally US505 - Create the ability to sell CBB cards.
        protected bool m_printFacesToGlobalPrinter;
        protected string m_printerName;
        protected float m_cardFacePointSize;
        protected bool m_printPlayerPoints;
        protected bool m_printSignatureLine;
        protected CBBPlayItSheetPrintMode m_cbbPlayItSheetPrintMode; // Rally US505
        protected bool m_allowElectronicSales;
        protected short m_receiptCopies = 1;
        protected string m_receiptDisclaimer1;
        protected string m_receiptDisclaimer2;
        protected string m_receiptDisclaimer3;
        protected int m_loggingLevel;
        protected int m_fileLogRecycleDays;
        protected string m_clientInstallDrive;
        protected string m_clientInstallRootDir;
        protected bool m_allowUnitCrossTransfers;
        protected bool m_enableUnitAssignment; // TTP 50114
        protected bool m_creditEnabled; // TTP 50138, TA7897
        protected bool m_enableAnonymousMachineAccounts; // TTP 50114
        protected bool m_printStaffFirstNameOnly; // TTP 50097
        protected short m_payoutReceiptCopies = 2; // TTP 50114
        protected bool m_allowQuantitySale; // PDTS 571
        protected bool m_printQuantitySaleReceipts; // PDTS 571
        protected int m_maxPlayerQuantitySale; // PDTS 571
        protected bool m_printProductNames; // PDTS 964
        protected bool m_mainStageMode; // PDTS 964
        protected MagneticCardReaderMode m_magCardMode; // PDTS 1064
        protected string m_magCardModeSettings; // PDTS 1064
        protected BingoPlayType m_playType = BingoPlayType.Bingo; // Rally US419 - Display BINGO or LOTTO on all applicable elements on screen.
        protected bool m_usePrePrintedPacks; // Rally US510
        protected bool m_playWithPaper; // Rally TA5748
        protected bool m_forcePacksToPlayer; // Rally TA6050 - Allow not forcing a pack to a player.
        protected bool m_allowSpecialGames; // Rally TA6385
        // FIX: TA4779
        protected int m_maxCardLimit;
        protected bool m_printCardFaces;
        protected PrintCardNumberMode m_printCardNumbers; // Rally TA5749
        protected string m_receiptHeaderLine1;
        protected string m_receiptHeaderLine2;
        protected string m_receiptHeaderLine3;
        protected string m_receiptFooterLine1;
        protected string m_receiptFooterLine2;
        protected string m_receiptFooterLine3;
        // END: TA4779
        protected bool m_cbbEnabled; // Rally TA7897
        protected bool m_cbbQuickPickEnabled; // FIX: DE4052 - Don't show quick picks if disabled in the license file.
        protected bool m_multiCurrency; // Rally TA7465
        protected bool m_useExchangeRateOnSale; // Rally US1658
        protected bool m_enableRegisterSalesReport; // Rally US1650
        protected MinimumSaleAllowed m_minimumSaleAllowed = MinimumSaleAllowed.All; // Rally US1854
        protected bool m_usePasswordKeypad; // US2057
        protected bool m_printPtsRedeemed; // US2139
        protected bool m_printRegisterSalesByPackage; // US1808
        protected bool m_hasFixed;
        protected bool m_hasTraveler;
        protected bool m_hasTracker;
        protected bool m_hasExplorer; // Rally TA7729
        protected bool m_hasTraveler2; // PDTS 964, Rally US765
        protected bool m_hasTablet; // US2908
        protected int m_defaultElectronicDeviceID;
        protected short m_travelerMaxCards;
        protected short m_trackerMaxCards;
        protected short m_fixedMaxCards;
        protected short m_explorerMaxCards; // Rally TA7729
        protected short m_traveler2MaxCards; // PDTS 964, Rally US765
        protected decimal m_taxRate; // FIX: DE1938
        protected bool m_enableActiveSession; // US2828 
        protected bool m_autoDiscountInfoGoesOnBottomOfScreenReceipt = false;
        protected bool m_checkProductCardCount;  //4028
		protected bool m_isCouponManagement;
        protected bool m_rCouponTaxable; 
        protected int m_playerPinLength;
        protected bool m_northDakotaSalesMode;
        protected int m_ThirdPartyPlayerInterfaceID;
        protected int m_ThirdPartyPlayerInterfacePINLength;
        protected bool m_ThirdPartyPlayerInterfaceGetPINWhenCardSwiped = false;
        protected bool m_ThirdPartyPlayerInterfaceNeedPINForPoints = false;
        protected bool m_ThirdPartyPlayerInterfaceNeedPINForRating = false;
        protected bool m_ThirdPartyPlayerInterfaceNeedPINForRedemption = false;
        protected bool m_ThirdPartyPlayerInterfaceNeedPINForRatingVoid = false;
        protected bool m_ThirdPartyPlayerInterfaceNeedPINForRedemptionVoid = false;
        protected bool m_ThirdPartyPlayerInterfaceDoesExternalRating = false;
        protected int m_ThirdPartyPlayerInterfaceRatingPoints = 0; //For rating: Earn this many points
        protected int m_ThirdPartyPlayerInterfaceRatingPennies = 1;//            per this many pennies
        protected int m_ThirdPartyPlayerInterfacePointPennies = 0; //For tendering: Get this many pennies
        protected int m_ThirdPartyPlayerInterfacePointPoints = 1;//              per this many points
        protected int m_ThirdPartyPlayerSyncMode = 0;
        protected bool m_enableFlexTendering;
        protected bool m_postCashTendering;
        protected bool m_allowCreditCards;
        protected bool m_allowDebitCards;
        protected bool m_allowGiftCards;
        protected bool m_allowChecks;
        protected bool m_allowCash; 
        protected SellAgainOption m_sellPreviouslySoldItem;
        protected bool m_allowSplitTendering;
        protected int m_checkVoidType;
        protected bool m_refundCashOnFailedCardVoid;
        protected bool m_allowManualCardEntry;
        protected string m_pinPadGreeting;
        protected bool m_pinPadEnabled;
        protected string m_pinPadCardFailedMessage;
        protected decimal m_maxTotalNotRequiringSignature;
        protected string m_pinPadPostSaleMessage;
        protected bool m_displayItemDetailOnPinPad;
        protected string m_pinPadStationClosedMessage;
        protected bool m_displaySubtotalOnPinPad;
        protected bool m_printDualReceiptsForNonCashSales;
        protected bool m_refundCashForDebit;
        protected bool m_paymentProcessingEnabled;
        protected bool m_processFundsTransferForChecks;
        protected string m_precidiaDeviceAddress;
        protected int m_precidiaDevicePort;
        protected string m_transnetAddress;
        protected int m_transnetPort;
        protected string m_paymentProcessingListeningAddress;
        protected int m_paymentProcessingListeningPort;
        protected bool m_enableB3Management; //US4380
        protected bool m_enableProductValidation;//US3509
        protected int m_productValidationCardCount;//US3509
        protected int m_productValidationMaxQuantity;//US3509
        protected bool m_autoIssueBank;
        protected bool m_printPosBankDenomReceipt;
        protected List<TenderTypeValue> m_POSTenders = null;
        protected bool m_use00ForCurrencyEntry = true;
        protected bool m_printReceiptSortedByPackageType = false;
        protected bool m_forceDeviceSelectionWhenNoFees = false;
        protected bool m_removePackagesWithCouponsInRepeatSale = false;
        protected bool m_removePaperInRepeatSale = true;
        protected bool m_removeDiscountsInRepeatSale = true;
        protected bool m_longPOSDescriptions = true;
        protected bool m_returnToPageOneAfterSale = true;
        protected bool m_selectElectronicDeviceThroughSystemMenu = true;
        protected OptionsForGivingChange m_changeDispensing = OptionsForGivingChange.Normal;

        protected bool m_useLinearGameNumbering; //US4804
        protected bool m_enablePaperUsage; 
        protected bool m_showPaperUsageAtLogin;
        protected bool m_allowSalesToBannedPlayers;

        protected int m_cbbScannerType;
        protected bool m_enableRegisterClosingReport;
        protected bool m_printRegisterClosingOnBankClose;
        protected int m_bankCloseSignatureLineCount;//DE13632
        protected int m_numberOfBankCloseReceipts;//DE13632

        protected string m_shift4AuthCode;
        protected int m_pinPadDisplayLineCount;
        protected int m_pinPadDisplayColumnCount;
        protected bool m_pinPadDisplayMessages;
        protected int m_paymentProcessorCommunicationsTimeout;

        protected bool m_AllowWidescreenPOS = true;
        protected bool m_showTwoMenuPagesPerPageIfWidescreen = true;
        
        protected bool m_autoApplyCouponsOnSimpleKiosks = true;
        protected bool m_allowPaperOnKiosks = true;
        protected bool m_allowScanningProductsOnSimpleKiosks = true;
        protected bool m_allowCouponButtonOnHybridKiosk = true;
        protected bool m_allowBuyingAtSimpleKioskWithoutPlayerCard = true;

        protected int m_kioskIdleTimeout = 30; //30 seconds
        protected int m_kioskShortIdleTimeout = 15; //15 seconds
        protected int m_kioskMessageTimeout = 10000;  //10 seconds
        protected string m_kioskAttractText = "Use your player card to buy BINGO packs!";
        protected string m_kioskClosedText = "Sorry, closed.";
        protected bool m_useSimplePaymentForAdvancedKiosk = false;
        protected bool m_allowUseLastPurchaseButtonOnKiosk = false;
        protected bool m_allowB3OnKiosk = false;
        protected bool m_useKeyClickSoundsOnKiosk = false;
        protected int m_kioskVideoVolume = 100;
        protected int m_kioskCrashRecoveryNeedAttendantAfterNMinutes = 0; //lock Kiosk on crash recovery
        protected bool m_stabilizeCabinet = false;

        protected bool m_showQuantityOnMenuButtons = false;
        protected bool m_printPlayerIdentityAsAccountNumber = false;
        protected bool m_printPlayerID = true;

        protected bool m_scannedReceiptsStartNewSale = true;
        protected bool m_allowCreditCardsOnKiosks = true;
        protected bool m_kiosksCanOnlySellFromTheirButtons = false;
        protected bool m_allowKiosksToPrintCBBPlayItSheetsFromMainScreen = false;
        protected bool m_kioskTimeoutPulseDefaultButton = true;
        protected string m_ticketPrinterName = string.Empty;

        protected bool m_printIncompleteTransactionReceipts = true;
        protected string m_incompleteTransactionReceiptText1 = "**INCOMPLETE**";
        protected string m_incompleteTransactionReceiptText2 = "**TRANSACTION**";

        protected bool m_showFreeOnDeviceButtonsWithFeeOfZero = true;
        protected bool m_deviceFeesQualifyForPoints = false;

        protected uint m_kioskTestMode = 0; //bit 0 = acceptor and bit 1 = dispenser

        protected int m_operatorID;

        protected bool m_forceAuthorizationOnVoidsAtPOS = false;

        protected string m_GuardianAddressAndPort = string.Empty;
        protected IPEndPoint m_GuardianEndPoint = null;

        protected bool m_doPointQualifyingAmountCalculationOldWay = false;

        protected bool m_enablePresales = false;
        #endregion

        #region Member Methods
        /// <summary>
        /// Loads the current operator's information into the settings
        /// </summary>
        /// <param name="op"></param>
        public void LoadOperatorInfo(GameTech.Elite.Base.Operator op)
        {
            HallName = op.Name;
            HallAddress = String.Format("{0} {1}, {2} {3}", op.PhysicalAddress.Line1, op.PhysicalAddress.City, op.PhysicalAddress.State, op.PhysicalAddress.ZipCode);
        }

        /// <summary>
        /// Parses a setting from the server and loads it into the 
        /// POSSettings, if valid.
        /// </summary>
        /// <param name="setting">The setting to parse.</param>
        public void LoadSetting(SettingValue setting)
        {
            Setting param = (Setting)setting.Id;

            // TODO: Load these settings instead of defaulting:
//            m_paymentProcessorCommunicationsTimeout = 120;
//            m_pinPadDisplayColumnCount = 8;
//            m_pinPadDisplayLineCount = 30;
//            m_shift4AuthCode = "1AC39B0E-C073-F8E0-ABAC58983A889CDB"; 

            try
            {
                switch (param)
                {
                    case Setting.KioskPeripheralsTicketPrinterName:
                        m_ticketPrinterName = setting.Value;
                        break;

                    case Setting.StabilizeCabinet:
                        m_stabilizeCabinet = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.AllowSalesToBannedPlayers:
                        m_allowSalesToBannedPlayers = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.DoPointQualifyingAmountCalculationOldWay:
                        m_doPointQualifyingAmountCalculationOldWay = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.DeviceFeesQualifyForPoints:
                        m_deviceFeesQualifyForPoints = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.KioskCrashRecoveryNeedAttendantAfterNMinutes:
                        m_kioskCrashRecoveryNeedAttendantAfterNMinutes = Convert.ToInt32(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.KioskTimeoutPulseDefaultButton:
                        m_kioskTimeoutPulseDefaultButton = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.UseKeyClickSoundsOnKiosk:
                        m_useKeyClickSoundsOnKiosk = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.KioskVideoVolume:
                        m_kioskVideoVolume = Convert.ToInt32(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.AllowKiosksToPrintCBBPlayItSheetsFromReceiptScan:
                        m_allowKiosksToPrintCBBPlayItSheetsFromMainScreen = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.KioskChangeDispensingMethod:
                        m_changeDispensing = (OptionsForGivingChange)Convert.ToInt32(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.KiosksCanOnlySellFromTheirButtons:
                        m_kiosksCanOnlySellFromTheirButtons = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.AllowB3SalesOnAPOSKiosk:
                        m_allowB3OnKiosk = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
						break;

                    case Setting.ForceAuthorizationOnVoidsAtPOS:
                        m_forceAuthorizationOnVoidsAtPOS = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
						break;

                    case Setting.ShowFreeOnDeviceButtonsWithFeeOfZero:
                        m_showFreeOnDeviceButtonsWithFeeOfZero = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.AllowScanningProductsOnSimplePOSKiosk:
                        m_allowScanningProductsOnSimpleKiosks = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;
        
                    case Setting.ForceDeviceSelectionWhenNoFees:
                        m_forceDeviceSelectionWhenNoFees = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.PrintIncompleteTransactionReceipt:
                        m_printIncompleteTransactionReceipts = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;
                    
                    case Setting.IncompleteTransactionReceiptText1:
                        m_incompleteTransactionReceiptText1 = setting.Value;
                        break;

                    case Setting.IncompleteTransactionReceiptText2:
                        m_incompleteTransactionReceiptText2 = setting.Value;
                        break;
                    
                    case Setting.AllowCreditCardsOnKiosks:
                        m_allowCreditCardsOnKiosks = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;
                    
                    case Setting.ScannedReceiptsStartNewSale:
                        m_scannedReceiptsStartNewSale = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.PrintPlayerIdentityAsAccountNumber:
                        m_printPlayerIdentityAsAccountNumber = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.PrintPlayerID:
                        m_printPlayerID = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.ShowQuantitiesOnMenuButtons:
                        m_showQuantityOnMenuButtons = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.KioskIdleTimeout:
                        m_kioskIdleTimeout = Convert.ToInt32(setting.Value, CultureInfo.InvariantCulture);
                        break;
                    
                    case Setting.KioskShortIdleTimeout:
                        m_kioskShortIdleTimeout = Convert.ToInt32(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.KioskMessageTimeout:
                        m_kioskMessageTimeout = 1000 * Convert.ToInt32(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.KioskAttractText:
                        m_kioskAttractText = setting.Value;
                        break;

                    case Setting.KioskClosedText:
                        m_kioskClosedText = setting.Value;
                        break;

                    case Setting.KioskGuardianAddress:
                        GuardianAddressAndPort = setting.Value;
                        break;

//                    case Setting.KioskPeripheralsTicketPrinterName:
//                        TicketPrinterName = setting.Value;
//                        break;

                    case Setting.WidescreenPOSHasTwoMenuPagesPerPage:
                        m_showTwoMenuPagesPerPageIfWidescreen = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.AllowWidescreenPOS:
                        m_AllowWidescreenPOS = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.AutomaticallyApplyCouponsToSalesOnSimpleKiosks:
                        m_autoApplyCouponsOnSimpleKiosks = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.AllowBarcodedPaperToBeSoldAtSimpleKiosk:
                        m_allowPaperOnKiosks = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.IncludeTheCouponsButtonOnTheHybridKiosk:
                        m_allowCouponButtonOnHybridKiosk = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.AllowUseOfSimpleKioskWithoutPlayerCard:
                        m_allowBuyingAtSimpleKioskWithoutPlayerCard = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.UseSimplePaymentFormForAdvancedKiosk:
                        m_useSimplePaymentForAdvancedKiosk = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.AllowUseLastPurchaseButtonOnKiosk:
                        m_allowUseLastPurchaseButtonOnKiosk = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.MagneticCardFilters:
                        m_MSRSettings.setFilters(setting.Value);
                        break;

                    case Setting.ShowMouseCursor:
                        m_showCursor = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.POSReceiptPrinterName:
                        m_receiptPrinterName = setting.Value;
                        break;

                    case Setting.CashDrawerEjectCode:
                        string code1 = setting.Value, code2 = string.Empty;

                        if (code1.Contains("-"))
                        {
                            int dashAt = code1.IndexOf('-');

                            code2 = code1.Substring(dashAt + 1);
                            code1 = code1.Substring(0, dashAt);
                        }

                        m_drawerCode = GetDrawerCode(code1);
                        m_drawer2Code = GetDrawerCode(code2);

                        break;

                    case Setting.CrateServerAddress:
                        m_crateServerName = setting.Value;
                        break;

                    case Setting.TenderSales:
                        // TTP 50114
                        m_tender = (TenderSalesMode)Convert.ToInt32(setting.Value, CultureInfo.InvariantCulture);

                        if (!Enum.IsDefined(typeof(TenderSalesMode), m_tender))
                            throw new ArgumentException();

                        break;

                    case Setting.DatabaseServer:
                        m_dbServer = setting.Value;
                        break;

                    case Setting.DatabaseName:
                        m_dbName = setting.Value;
                        break;

                    case Setting.DatabaseUser:
                        m_dbUser = setting.Value;
                        break;

                    case Setting.DatabasePassword:
                        m_dbPassword = setting.Value;
                        break;

                    case Setting.ForceEnglish:
                        m_forceEnglish = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    // Rally US658
                    case Setting.SwipeEntersRaffle:
                        m_swipeEntersRaffle = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.EnableLogging:
                        m_enableLogging = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.CBBScannerPort:
                        m_cbbScannerPort = Convert.ToInt32(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.CBBSheetDefinition:
                        m_cbbSheetDef = setting.Value;
                        break;

                    case Setting.EnableCBBFavorites:
                        m_cbbEnableFavorites = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.OpAllowNoSale:
                        m_allowNoSale = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.OpAllowReturns:
                        m_allowReturns = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    // PDTS 1044
                    case Setting.PromptForPlayerCreation:
                        m_promptForPlayerCreation = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    // Rally US505
                    case Setting.CBBPlayItSheetType:
                        m_cbbPlayItSheetType = (CBBPlayItSheetType)Enum.Parse(typeof(CBBPlayItSheetType), setting.Value);
                        break;

                    case Setting.PrintNonElectronicReceipts:
                        m_printNonElecReceipt = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.PrintFacesToGlobalPrinter:
                        m_printFacesToGlobalPrinter = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.GlobalPrinterName:
                        m_printerName = setting.Value;
                        break;

                    case Setting.CardFacePointSize:
                        m_cardFacePointSize = Convert.ToSingle(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.PrintPointInfo:
                        m_printPlayerPoints = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.PrintPlayerSignatureLine:
                        m_printSignatureLine = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    // PDTS 536
                    // Rally US505
                    case Setting.PrintCBBCardsToPlayItSheet:
                        m_cbbPlayItSheetPrintMode = (CBBPlayItSheetPrintMode)Enum.Parse(typeof(CBBPlayItSheetPrintMode), setting.Value);
                        break;

                    case Setting.SellElectronics:
                        m_allowElectronicSales = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.PrintRegisterReceiptsNumber:
                        m_receiptCopies = Convert.ToInt16(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.PrintReceiptSortedByPackageType:
                        m_printReceiptSortedByPackageType = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.DisclaimerLine1:
                        m_receiptDisclaimer1 = setting.Value;
                        break;

                    case Setting.DisclaimerLine2:
                        m_receiptDisclaimer2 = setting.Value;
                        break;

                    case Setting.DisclaimerLine3:
                        m_receiptDisclaimer3 = setting.Value;
                        break;

                    case Setting.LoggingLevel:
                        m_loggingLevel = Convert.ToInt32(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.LogRecycleDays:
                        m_fileLogRecycleDays = Convert.ToInt32(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.ClientInstallDrive:
                        m_clientInstallDrive = setting.Value;
                        break;

                    case Setting.ClientInstallRootDirectory:
                        m_clientInstallRootDir = setting.Value;
                        break;

                    case Setting.AllowUnitCrossTransfers:
                        m_allowUnitCrossTransfers = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    // TTP 50114
                    case Setting.EnableUnitAssignment:
                        m_enableUnitAssignment = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    // TTP 50097
                    case Setting.PrintStaffFirstNameOnly:
                        m_printStaffFirstNameOnly = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    // TTP 50114
                    case Setting.NumberOfPayoutReceipts:
                        m_payoutReceiptCopies = Convert.ToInt16(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    // PDTS 571
                    case Setting.AllowQuantitySale:
                        m_allowQuantitySale = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.PrintQuantitySaleReceipts:
                        m_printQuantitySaleReceipts = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.MaxPlayerQuantitySale:
                        m_maxPlayerQuantitySale = Convert.ToInt32(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    // PDTS 964
                    case Setting.PrintProductNames:
                        m_printProductNames = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    // PDTS 1064
                    case Setting.MagneticCardReaderMode:
                        m_magCardMode = (MagneticCardReaderMode)Convert.ToInt32(setting.Value, CultureInfo.InvariantCulture);

                        if (!Enum.IsDefined(typeof(MagneticCardReaderMode), m_magCardMode))
                            throw new ArgumentException();

                        break;

                    case Setting.MagneticCardReaderParameters: //remote MSR IP/port/track
                        m_magCardModeSettings = setting.Value;
                        break;

                    // Rally DE130
                    case Setting.MSRReadTriggers: //for keyboard wedge MSR, these are the characters that indicate input is from the MSR and when the input stream has stopped
                        m_MSRSettings.setReadTriggers(setting.Value);
                        break;

                    // Rally US419
                    case Setting.PlayType:
                        m_playType = (BingoPlayType)Enum.Parse(typeof(BingoPlayType), setting.Value, true);
                        break;

                    // FIX: TA4779
                    case Setting.MaxCardLimit:
                        m_maxCardLimit = Convert.ToInt32(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.PrintCardFaces:
                        m_printCardFaces = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    // Rally TA5749
                    case Setting.PrintCardNumbers:
                        m_printCardNumbers = (PrintCardNumberMode)Enum.Parse(typeof(PrintCardNumberMode), setting.Value, true);
                        break;
                    // END: TA5749

                    case Setting.ReceiptHeaderLine1:
                        m_receiptHeaderLine1 = setting.Value;
                        break;

                    case Setting.ReceiptHeaderLine2:
                        m_receiptHeaderLine2 = setting.Value;
                        break;

                    case Setting.ReceiptHeaderLine3:
                        m_receiptHeaderLine3 = setting.Value;
                        break;

                    case Setting.ReceiptFooterLine1:
                        m_receiptFooterLine1 = setting.Value;
                        break;

                    case Setting.ReceiptFooterLine2:
                        m_receiptFooterLine2 = setting.Value;
                        break;

                    case Setting.ReceiptFooterLine3:
                        m_receiptFooterLine3 = setting.Value;
                        break;
                    // END: TA4779

                    // Rally TA6050
                    case Setting.ForcePackToPlayer:
                        m_forcePacksToPlayer = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;
                    // END: TA6050

                    // Rally US1658
                    case Setting.UseExchangeRateOnSale:
                        m_useExchangeRateOnSale = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    // Rally US1650
                    case Setting.EnableRegisterSalesReport:
                        m_enableRegisterSalesReport = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    // Rally US1854
                    case Setting.MinimumSaleAllowed:
                        m_minimumSaleAllowed = (MinimumSaleAllowed)Enum.Parse(typeof(MinimumSaleAllowed), setting.Value, true);
                        break;

                    // US2057
                    case Setting.UsePasswordKeypad:
                        m_usePasswordKeypad = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    // US2139
                    case Setting.PrintPointsRedeemed:
                        m_printPtsRedeemed = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    // US1808
                    case Setting.PrintRegisterReportByPackage:
                        m_printRegisterSalesByPackage = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    //US2828
                    case Setting.EnableActiveSalesSession:
                        m_enableActiveSession = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;
                    
                    case Setting.AutoDiscountInfoGoesOnBottomOfScreenReceipt:
                        m_autoDiscountInfoGoesOnBottomOfScreenReceipt = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    //US4028
                    case Setting.CheckProductCardCount:
                        m_checkProductCardCount = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.EnableCouponManagement:
                        m_isCouponManagement = Convert.ToBoolean(setting.Value);
                        break;

                    case Setting.AreCouponsTaxable:
                        m_rCouponTaxable = Convert.ToBoolean(setting.Value);
                        break;

                    //US 4120
                    case Setting.PlayerPinLength:
                        m_playerPinLength = Convert.ToInt32(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.ThirdPartyPlayerInterfaceID:
                        m_ThirdPartyPlayerInterfaceID = Convert.ToInt32(setting.Value);
                        break;

                    case Setting.ThirdPartyPlayerInterfacePINLength:
                        m_ThirdPartyPlayerInterfacePINLength = Convert.ToInt32(setting.Value);
                        break;

                    case Setting.ThirdPartyPlayerInterfaceGetPINWhenCardSwiped:
                        m_ThirdPartyPlayerInterfaceGetPINWhenCardSwiped = Convert.ToBoolean(setting.Value);
                        break;

                    case Setting.ThirdPartyPlayerInterfaceNeedPINForPoints:
                        m_ThirdPartyPlayerInterfaceNeedPINForPoints = Convert.ToBoolean(setting.Value);
                        break;

                    case Setting.ThirdPartyPlayerInterfaceNeedPINForRating:
                        m_ThirdPartyPlayerInterfaceNeedPINForRating = Convert.ToBoolean(setting.Value);
                        break;

                    case Setting.ThirdPartyPlayerInterfaceNeedPINForRedemption:
                        m_ThirdPartyPlayerInterfaceNeedPINForRedemption = Convert.ToBoolean(setting.Value);
                        break;

                    case Setting.ThirdPartyPlayerInterfaceNeedPINForRatingVoid:
                        m_ThirdPartyPlayerInterfaceNeedPINForRatingVoid = Convert.ToBoolean(setting.Value);
                        break;

                    case Setting.ThirdPartyPlayerInterfaceNeedPINForRedemptionVoid:
                        m_ThirdPartyPlayerInterfaceNeedPINForRedemptionVoid = Convert.ToBoolean(setting.Value);
                        break;

                    case Setting.ThirdPartyPlayerInterfaceExternalRating:
                        m_ThirdPartyPlayerInterfaceDoesExternalRating = Convert.ToBoolean(setting.Value);
                        break;

                    case Setting.ThirdPartyPointScaleNumerator:
                        m_ThirdPartyPlayerInterfaceRatingPoints = Convert.ToInt32(setting.Value);
                        break;

                    case Setting.ThirdPartyPointScaleDenominator:
                        m_ThirdPartyPlayerInterfaceRatingPennies = Convert.ToInt32(setting.Value);
                        break;
                    
                    case Setting.ThirdPartyRedeemNumerator:
                        m_ThirdPartyPlayerInterfacePointPennies = Convert.ToInt32(setting.Value);
                        break;

                    case Setting.ThirdPartyRedeemDenominator:
                        m_ThirdPartyPlayerInterfacePointPoints = Convert.ToInt32(setting.Value);
                        break;

                    case Setting.ThirdPartyPlayerSyncMode:
                        {
                            m_ThirdPartyPlayerSyncMode = Convert.ToInt32(setting.Value);

                            //see if the server has the player tracking port enabled (port = 0 if not)
                            GameTech.Elite.Client.GetServerPortsMessage getPorts = new GameTech.Elite.Client.GetServerPortsMessage();
                            getPorts.PortToGet = GameTech.Elite.Client.ServerPortId.ThirdParty;

                            try
                            {
                                getPorts.Send();
                            }
                            catch (Exception)
                            {
                            }

                            ushort port = 0;

                            getPorts.Ports.TryGetValue(GameTech.Elite.Client.ServerPortId.ThirdParty, out port);

                            if (port == 0) //turned off, set sync mode to disconnected
                                m_ThirdPartyPlayerSyncMode = 3;
                        }
                        break;

                    case Setting.EnableValidation: //US3509
                        EnabledProductValidation = Convert.ToBoolean(setting.Value);
                        break;

                    case Setting.ProductValidationCardCount: //US3509
                        ProductValidationCardCount = Convert.ToInt32(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.MaxValidationPerTransaction: //US3509
                        ProductValidationMaxQuantity = Convert.ToInt32(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.AutoIssueBank: // US4434
                        AutoIssueBank = Convert.ToBoolean(setting.Value);
                        break;

                    // Tendering and Payment Processing settings
                    case Setting.EnableFlexTendering:
                        m_enableFlexTendering = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.AllowCreditCards:
                        m_allowCreditCards = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.AllowDebitCards:
                        m_allowDebitCards = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.AllowGiftCards:
                        m_allowGiftCards = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.AllowChecks:
                        m_allowChecks = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.AllowCash:
                        m_allowCash = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.SellPreviouslySoldItemOption:
                        m_sellPreviouslySoldItem = (SellAgainOption)Convert.ToInt32(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.AllowSplitTendering:
                        m_allowSplitTendering = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.PaymentProcessorCommunicationsTimeout:
                        m_paymentProcessorCommunicationsTimeout = Convert.ToInt32(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.PinPadDisplayColumnCount:
                        m_pinPadDisplayColumnCount = Convert.ToInt32(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.AllowManualCardEntry:
                        m_allowManualCardEntry = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.PinPadGreeting:
                        m_pinPadGreeting = setting.Value;
                        break;

                    case Setting.PinPadEnabled:
                        m_pinPadEnabled = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.PinPadCardFailMessage:
                        m_pinPadCardFailedMessage = setting.Value;
                        break;

                    case Setting.MaximumTotalNotRequiringSignature:
                        m_maxTotalNotRequiringSignature = Convert.ToDecimal(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.PinPadAfterSaleMessage:
                        m_pinPadPostSaleMessage = setting.Value;
                        break;

                    case Setting.DisplayItemDetailOnPinPad:
                        m_displayItemDetailOnPinPad = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.PinPadStationClosedMessage:
                        m_pinPadStationClosedMessage = setting.Value;
                        break;

                    case Setting.DisplaySubtotalOnPinPad:
                        m_displaySubtotalOnPinPad = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.PrintCustomerAndHallReceiptsForNonCashSales:
                        m_printDualReceiptsForNonCashSales = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.PINPadDisplayLineCount:
                        m_pinPadDisplayLineCount = Convert.ToInt32(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.PaymentProcessingEnabled:
                        m_paymentProcessingEnabled = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.Shift4AuthToken:
                        m_shift4AuthCode = setting.Value;
                        break;

                    case Setting.ProcessFundsTransferForChecks:
                        m_processFundsTransferForChecks = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.PaymentDeviceAddress:
                        m_precidiaDeviceAddress = setting.Value;
                        break;

                    case Setting.PaymentDevicePort:
                        m_precidiaDevicePort = Convert.ToInt32(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.TransnetAddress:
                        m_transnetAddress = setting.Value;
                        break;

                    case Setting.TransnetPort:
                        m_transnetPort = Convert.ToInt32(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.PaymentProcessingAppAddress:
                        m_paymentProcessingListeningAddress = setting.Value;
                        break;

                    case Setting.PaymentProcessingAppPort:
                        m_paymentProcessingListeningPort = Convert.ToInt32(setting.Value, CultureInfo.InvariantCulture);
						break;

                    case Setting.PrintDenominationReceipt: //US4698
                        m_printPosBankDenomReceipt = Convert.ToBoolean(setting.Value);
                        break;

                    case Setting.Use00ForCurrencyEntry:
                        m_use00ForCurrencyEntry = Convert.ToBoolean(setting.Value);
                        break;

                    case Setting.CreditCardProcessor:
                        int processorId = Convert.ToInt32(setting.Value, CultureInfo.InvariantCulture);

                        if (Enum.IsDefined(typeof(CreditCardProcessors), processorId))
                            CreditCardProcessor = (CreditCardProcessors)processorId;
                        else
                            CreditCardProcessor = CreditCardProcessors.None;

                        switch ((CreditCardProcessors)processorId)
                        {
                            case CreditCardProcessors.Shift4:
                            {
                                m_pinPadDisplayMessages = false;
                                break;
                            }
                            case CreditCardProcessors.Precidia:
                            {
                                m_pinPadDisplayMessages = true;
                                break;
                            }
                            default:
                            {
                                m_pinPadDisplayMessages = false;
                                break;
                            }
                        }

						break;

                    case Setting.UseLinearGameNumbering: //US4804
                        m_useLinearGameNumbering = Convert.ToBoolean(setting.Value);
                        break;

                    case Setting.POSDefaultElectronicUnit: //US4884
                        m_defaultElectronicDeviceID = Convert.ToInt32(setting.Value);
                        break;

                    case Setting.EnablePaperUsage:
                        m_enablePaperUsage = Convert.ToBoolean(setting.Value);
                        break;
                    
                    case Setting.ShowPaperUsageAtLogin:
                        m_showPaperUsageAtLogin = Convert.ToBoolean(setting.Value);
                        break;

                    case Setting.CompactPaperPacksSold:
                        PrintCompactPaperPacksSoldReportOnReceipt = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.ReturnToPageOneAfterSale:
                        m_returnToPageOneAfterSale = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.UseLongDescriptionsOnPOSScreen:
                        m_longPOSDescriptions = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.RepeatSaleRemoveCouponPackages:
                        m_removePackagesWithCouponsInRepeatSale = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.RepeatSaleRemovePaper:
                        m_removePaperInRepeatSale = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.RepeatSaleRemoveDiscounts:
                        m_removeDiscountsInRepeatSale = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.UseSystemMenuForDeviceSelection:
                        m_selectElectronicDeviceThroughSystemMenu = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.PrintOperatorInfoOnReceipt:
                        PrintOperatorInfoOnReceipt = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;
        
                    case Setting.CbbScannerType:
                        //US4511: Support Chatsworth CBB scanner
                        //-1= None
                        //0 = PDI VMR-138
                        //1 = Chatsworth ACP-100
                        //2 = Chatsworth ACP-200

                        if (setting.Value.ToUpper() == "NONE")
                            m_cbbScannerType = -1;
                        else
                            m_cbbScannerType = Convert.ToInt32(setting.Value, CultureInfo.InvariantCulture);
                        break;
                    
                    case Setting.EnableRegisterClosingReport:
                        //US5115: POS: Add Register Closing report button
                        m_enableRegisterClosingReport= Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;
                    
                    case Setting.PrintRegisterClosingOnBankClose:
                        //US5108: POS > Bank Close: Print register closing report
                        m_printRegisterClosingOnBankClose = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;
                    case Setting.BankCloseReceiptSignatureLineCount:
                        //DE13632
                        m_bankCloseSignatureLineCount = Convert.ToInt32(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case Setting.NumberOfBankCloseReceipts:
                        //DE13632
                        m_numberOfBankCloseReceipts = Convert.ToInt32(setting.Value, CultureInfo.InvariantCulture);
                        break;
                }
            }
            catch(Exception e)
            {
                throw new POSException(string.Format(CultureInfo.CurrentCulture, Resources.InvalidSetting, setting.Id, setting.Value), e);
            }
        }

        // Rally TA7897
        /// <summary>
        /// Parses a license setting from the server and loads it into the 
        /// POSSettings, if valid.
        /// </summary>
        /// <param name="setting">The license setting to parse.</param>
        public void LoadSetting(LicenseSettingValue setting)
        {
            LicenseSetting param = (LicenseSetting)setting.Id;

            try
            {
                switch(param)
                {
                    case LicenseSetting.EnableAnonymousMachineAccounts:
                        m_enableAnonymousMachineAccounts = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case LicenseSetting.MainStageMode:
                        m_mainStageMode = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case LicenseSetting.CreditEnabled:
                        m_creditEnabled = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case LicenseSetting.UsePrePrintedPacks:
                        m_usePrePrintedPacks = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case LicenseSetting.PlayWithPaper:
                        m_playWithPaper = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case LicenseSetting.AllowMelangeSpecialGames:
                        m_allowSpecialGames = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case LicenseSetting.CBBEnabled:
                        m_cbbEnabled = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case LicenseSetting.QuickPickEnabled:
                        m_cbbQuickPickEnabled = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case LicenseSetting.NDSalesMode:
                        m_northDakotaSalesMode = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;

                    case LicenseSetting.EnableB3Management:
                        m_enableB3Management = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;
                    case LicenseSetting.Presales:
                        EnablePresales = Convert.ToBoolean(setting.Value, CultureInfo.InvariantCulture);
                        break;
                }
            }
            catch(Exception e)
            {
                throw new POSException(string.Format(CultureInfo.CurrentCulture, Resources.InvalidSetting, setting.Id, setting.Value), e);
            }
        }

        public void LoadB3Settings(B3GetSettingsMessage msg)
        {
            B3IsMultiOperator = msg.IsMultiOperator;
            B3IsCommonRng = msg.IsCommonRng;
            B3AllowInSessBallChange = msg.AllowInSessBallChange;
            B3EnforceMix = msg.EnforceMix;
            B3IsDoubleAccount = msg.IsDoubleAccount;
        }

        private byte[] GetDrawerCode(string printableCode)
        {
            byte[] result = new byte[0];

            if (printableCode == string.Empty)
                return result;

            string[] codes = printableCode.Split(new char[] { ',' });

            if (codes != null && codes.Length > 0)
            {
                result = new byte[codes.Length];

                for (int x = 0; x < codes.Length; x++)
                {
                    try
                    {
                        result[x] = Convert.ToByte(codes[x], CultureInfo.InvariantCulture);
                    }
                    catch (FormatException)
                    {
                        result = new byte[0];
                        break;
                    }
                    catch (OverflowException)
                    {
                        result = new byte[0];
                        break;
                    }
                }
            }

            return result;
        }

        public void SetValidPOSTenders(List<TenderTypeValue> tendersList)
        {
            m_POSTenders = tendersList;
        }

        public int GetNumberOfValidPOSTenders()
        {
            return m_POSTenders.Count;
        }

        public List<TenderTypeValue> ValidPOSTenders
        {
            get
            {
                return m_POSTenders;
            }
        }

        /// <summary>
        /// Tests to see if the kiosk test mode is active.
        /// </summary>
        /// <param name="testMode">Kiosk test mode to check.</param>
        /// <returns>True if the test mode is set or false if not.</returns>
        private bool IsKioskTestModeActive(KioskTestMode testMode)
        {
            uint bit = (uint)Math.Pow(2, (double)testMode);

            return (m_kioskTestMode & bit) == bit;
        }

        /// <summary>
        /// Sets the given kiosk test mode as active or inactive.
        /// </summary>
        /// <param name="testMode">Kiosk test mode to set.</param>
        /// <param name="activate">True=activate and False=deactivate.</param>
        private void KioskTestModeActivate(KioskTestMode testMode, bool activate = true)
        {
            if(activate)
                m_kioskTestMode |= (uint)Math.Pow(2, (double)testMode);
            else
                m_kioskTestMode &= ~(uint)Math.Pow(2, (double)testMode);
        }

        /// <summary>
        /// Get the tender name from the tender type.
        /// </summary>
        /// <param name="tendType">Tender type.</param>
        /// <returns>Name associated with the tender type.</returns>
        public string GetTenderName(TenderType tendType)
        {
            TenderTypeValue ttv;

            ttv.TenderName = "Unknown tender";

            ttv = m_POSTenders.Find(i => i.TenderTypeID == (int)tendType);

            return ttv.TenderName;
        }

        /// <summary>
        /// Get the tender type associated with the tender name.
        /// </summary>
        /// <param name="tenderName">Tender name.</param>
        /// <returns>Tender type for the given name (-1 if no match).</returns>
        public TenderType GetTenderType(string tenderName)
        {
            TenderTypeValue ttv = m_POSTenders.Find(i => i.TenderName.ToUpper() == tenderName.ToUpper());

            if (string.IsNullOrEmpty(ttv.TenderName))
                return TenderType.Undefined;
            else
                return (TenderType)ttv.TenderTypeID;
        }

        private bool IsTenderActive(TenderType tendType)
        {
            TenderTypeValue ttv;

            ttv.IsActive = 0;

            ttv = m_POSTenders.Find(i => i.TenderTypeID == (int)tendType);

            return ttv.IsActive != 0;
        }

        #endregion

        #region Member Properties

        #region Peripheral Settings
        /// <summary>
        /// Gets whether or not this system should use any peripherals
        /// </summary>
        public bool UseGuardian
        {
            get;
            set;
        }

        /// <summary>
        /// Gets whether or not the kiosk allows cash buttons to test without a bill acceptor.
        /// </summary>
        public bool KioskTestWithoutAcceptor
        {
            get
            {
                return IsKioskTestModeActive(KioskTestMode.BillAcceptor);
            }

            set
            {
                KioskTestModeActivate(KioskTestMode.BillAcceptor, value);
            }
        }

        /// <summary>
        /// Gets whether or not the kiosk tries to dispense money.  In test mode, 
        /// the user is asked how much the bill dispenser would have dispensed.
        /// </summary>
        public bool KioskTestWithoutDispenser
        {
            get
            {
                return IsKioskTestModeActive(KioskTestMode.BillDispenser);
            }

            set
            {
                KioskTestModeActivate(KioskTestMode.BillDispenser, value);
            }
        }
        
        /// <summary>
        /// Gets whether or not the kiosk treats tickets as $20 bills for testing or demo.
        /// </summary>
        public bool KioskTestTreatTicketAs20
        {
            get
            {
                return IsKioskTestModeActive(KioskTestMode.TreatTicketsAs20DollarBills);
            }

            set
            {
                KioskTestModeActivate(KioskTestMode.TreatTicketsAs20DollarBills, value);
            }
        }

        /// <summary>
        /// Gets or sets if we are testing B3 without a B3 server.
        /// </summary>
        public bool KioskTestNoB3Server
        {
            get
            {
                return IsKioskTestModeActive(KioskTestMode.NoB3Server);
            }

            set
            {
                KioskTestModeActivate(KioskTestMode.NoB3Server, value);
            }
        }

        /// <summary>
        /// Gets or sets if we are testing and only using one monitor (stops top glass image).
        /// </summary>
        public bool KioskTestOneMonitor
        {
            get
            {
                return IsKioskTestModeActive(KioskTestMode.OneMonitor);
            }

            set
            {
                KioskTestModeActivate(KioskTestMode.OneMonitor, value);
            }
        }

        /// <summary>
        /// Gets or sets if we are testing and do not have a Guardian to connect to.
        /// </summary>
        public bool KioskTestNoGuardian
        {
            get
            {
                return IsKioskTestModeActive(KioskTestMode.NoGuardian);
            }

            set
            {
                KioskTestModeActivate(KioskTestMode.NoGuardian, value);
            }
        }

        /// <summary>
        /// Gets if any of the kiosk test modes is active.
        /// </summary>
        public bool KioskInTestMode
        {
            get
            {
                return m_kioskTestMode != 0;
            }
        }
            
        /// <summary>
        /// Gets the Guardian's IP:Port
        /// </summary>
        public string GuardianAddressAndPort
        {
            get
            {
                return m_GuardianAddressAndPort;
            }

            private set
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(value)) //creating or changing existing
                    {
                        string[] p = value.Split(new char[] { '.', ':', '-' });
                        IPEndPoint ep = new IPEndPoint(new IPAddress(new byte[]{Convert.ToByte(p[0]), Convert.ToByte(p[1]), Convert.ToByte(p[2]), Convert.ToByte(p[3])}), Convert.ToInt32(p[4]));

                        m_GuardianEndPoint = ep;

                        try
                        {
                            string test = p[5].ToUpper();
                            bool testAcceptor = test.Contains("A");
                            bool acceptTicketAs20Dollars = test.Contains("T");
                            bool testDispenser = test.Contains("D");
                            bool noB3Server = test.Contains("B");
                            bool oneMonitor = test.Contains("1");
                            bool noGuardian = test.Contains("G");

                            if (noGuardian)
                            {
                                testAcceptor = true;
                                testDispenser = true;
                            }

                            KioskTestWithoutAcceptor = testAcceptor;
                            KioskTestWithoutDispenser = testDispenser;
                            KioskTestTreatTicketAs20 = acceptTicketAs20Dollars;
                            KioskTestNoB3Server = noB3Server;
                            KioskTestOneMonitor = oneMonitor;
                            KioskTestNoGuardian = noGuardian;
                        }
                        catch (Exception)
                        {
                        }
                    }
                    else //removing existing
                    {
                        m_GuardianEndPoint = null;
                    }
                }
                catch (Exception)
                {
                    m_GuardianEndPoint = null;
                }
                
                m_GuardianAddressAndPort = value;
            }
        }

        /// <summary>
        /// Gets the Guardian's EndPoint
        /// </summary>
        public IPEndPoint GuardianEndPoint
        {
            get
            {
                return m_GuardianEndPoint;
            }
        }

        /// <summary>
        /// Whether or not to use the bill dispenser
        /// </summary>
        public bool UseDispenser
        {
            get;
            private set;
        }

        #endregion

        public bool ScanningReceiptsStartNewSale
        {
            get
            {
                return m_scannedReceiptsStartNewSale;
            }
        }

        public bool AllowSalesToBannedPlayers
        {
            get
            {
                return m_allowSalesToBannedPlayers;
            }
        }

        public bool AllowCreditCardsOnKiosks
        {
            get
            {
                return m_allowCreditCardsOnKiosks;
            }

            set
            {
                m_allowCreditCardsOnKiosks = value;
            }
        }

        /// <summary>
        /// The name of this hall
        /// </summary>
        public string HallName
        {
            get;
            private set;
        }

        /// <summary>
        /// The address of this hall
        /// </summary>
        public string HallAddress
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets an object that can be used to synchronize access to 
        /// the settings.
        /// </summary>
        public object SyncRoot
        {
            get
            {
                return m_syncRoot;
            }
        }
        
        /// <summary>
        /// Gets or sets the display mode to use for user interfaces.
        /// </summary>
        public DisplayMode DisplayMode
        {
            get
            {
                return m_displayMode;
            }
            set
            {
                m_displayMode = value;
            }
        }

        /// <summary>
        /// Gets the acceptable magnetic card sentinel character pairs.
        /// </summary>
        public MSRSettings MSRSettingInfo
        {
            get
            {
                return m_MSRSettings;
            }
        }

        public bool AutoApplyCouponsOnSimpleKiosks
        {
            get
            {
                return m_autoApplyCouponsOnSimpleKiosks;
            }

            set
            {
                m_autoApplyCouponsOnSimpleKiosks = value;
            }
        }

        public bool AllowScanningProductsOnSimpleKiosk
        {
            get
            {
                return m_allowScanningProductsOnSimpleKiosks;
            }
        }

        public bool AllowCouponButtonOnHybridKiosk
        {
            get
            {
                return m_allowCouponButtonOnHybridKiosk;
            }
        }

        public bool AllowBuyingAtSimpleKioskWithoutPlayerCard
        {
            get
            {
                return m_allowBuyingAtSimpleKioskWithoutPlayerCard;
            }
        }

        public bool AllowPaperOnKiosks
        {
            get
            {
                return m_allowPaperOnKiosks;
            }
        }

        /// <summary>
        /// Gets the default electronic device ID (0=none).
        /// </summary>
        public int DefaultElectronicDeviceID
        {
            get
            {
                return m_defaultElectronicDeviceID;
            }

            set
            {
                m_defaultElectronicDeviceID = value;
            }
        }

        /// <summary>
        /// Gets or sets whether to show the mouse cursor.
        /// </summary>
        public bool ShowCursor
        {
            get
            {
                return m_showCursor;
            }
            set
            {
                m_showCursor = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the global receipt printer to use.
        /// </summary>
        public string ReceiptPrinterName
        {
            get
            {
                return m_receiptPrinterName;
            }
            set
            {
                m_receiptPrinterName = value;
            }
        }

        /// <summary>
        /// Gets whether to print the operator's name, address, and phone number on the receipt. 
        /// </summary>
        public bool PrintOperatorInfoOnReceipt
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets or sets the bytes to send to the printer to open the 
        /// cash drawer.
        /// </summary>
        public byte[] DrawerCode
        {
            get
            {
                return m_drawerCode;
            }
            set
            {
                m_drawerCode = value;
            }
        }

        /// <summary>
        /// Gets or sets the bytes to send to the printer to open the 
        /// second cash drawer.
        /// </summary>
        public byte[] Drawer2Code
        {
            get
            {
                return m_drawer2Code;
            }

            set
            {
                m_drawer2Code = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the remote crate server to use.
        /// </summary>
        public string CrateServerName
        {
            get
            {
                return m_crateServerName;
            }
            set
            {
                m_crateServerName = value;
            }
        }

        /// <summary>
        /// Gets or sets whether to tender sales.
        /// </summary>
        public TenderSalesMode Tender
        {
            get
            {
                if (EnableFlexTendering)
                {
                    if (m_tender == TenderSalesMode.Off || m_tender == TenderSalesMode.Quick)
                        return TenderSalesMode.WarnNegative;
                    else
                        return m_tender;
                }
                else
                {
                    return m_tender;
                }
            }

            set
            {
                m_tender = value;
            }
        }

        /// <summary>
        /// Gets or sets the database server name to use for reports.
        /// </summary>
        public string DatabaseServer
        {
            get
            {
                return m_dbServer;
            }
            set
            {
                m_dbServer = value;
            }
        }

        /// <summary>
        /// Gets or sets the database name to use for reports.
        /// </summary>
        public string DatabaseName
        {
            get
            {
                return m_dbName;
            }
            set
            {
                m_dbName = value;
            }
        }

        /// <summary>
        /// Gets or sets the database user to use for reports.
        /// </summary>
        public string DatabaseUser
        {
            get
            {
                return m_dbUser;
            }
            set
            {
                m_dbUser = value;
            }
        }

        /// <summary>
        /// Gets or sets the database password to use for reports.
        /// </summary>
        public string DatabasePassword
        {
            get
            {
                return m_dbPassword;
            }
            set
            {
                m_dbPassword = value;
            }
        }

        /// <summary>
        /// Gets or sets whether to force the program to display in the 
        /// English language.
        /// </summary>
        public bool ForceEnglish
        {
            get
            {
                return m_forceEnglish;
            }
            set
            {
                m_forceEnglish = value;
            }
        }

        // Rally US658
        /// <summary>
        /// Gets or sets whether a swipe enters the player in a raffle.
        /// </summary>
        public bool SwipeEntersRaffle
        {
            get
            {
                return m_swipeEntersRaffle;
            }
            set
            {
                m_swipeEntersRaffle = value;
            }
        }

        /// <summary>
        /// Gets or sets whether to log output.
        /// </summary>
        public bool EnableLogging
        {
            get
            {
                return m_enableLogging;
            }
            set
            {
                m_enableLogging = value;
            }
        }

        /// <summary>
        /// Gets or sets the port used the by the Crystal Ball Bingo scanner.
        /// </summary>
        public int CBBScannerPort
        {
            get
            {
                return m_cbbScannerPort;
            }
            set
            {
                m_cbbScannerPort = value;
            }
        }

        /// <summary>
        /// Gets or sets the sheet definition file used by the Crystal Ball 
        /// Bingo scanner.
        /// </summary>
        public string CBBSheetDefinition
        {
            get
            {
                return m_cbbSheetDef;
            }
            set
            {
                m_cbbSheetDef = value;
            }
        }

        /// <summary>
        /// Gets or sets whether to allow an empty sale.
        /// </summary>
        public bool AllowNoSale
        {
            get
            {
                return m_allowNoSale;
            }
            set
            {
                m_allowNoSale = value;
            }
        }

        /// <summary>
        /// Gets or sets whether CBB favorites are enabled
        /// </summary>
        public bool EnableCBBFavorites
        {
            get
            {
                return m_cbbEnableFavorites;
            }

            set
            {
                m_cbbEnableFavorites = value;
            }
        }

        /// <summary>
        /// Gets or sets whether to allow returns.
        /// </summary>
        public bool AllowReturns
        {
            get
            {
                return m_allowReturns;
            }
            set
            {
                m_allowReturns = value;
            }
        }

        /// <summary>
        /// Gets or sets whether to print a sales receipt if there are 
        /// no electronics on it.
        /// </summary>
        public bool PrintNonElecReceipt
        {
            get
            {
                return m_printNonElecReceipt;
            }
            set
            {
                m_printNonElecReceipt = value;
            }
        }

        // PDTS 1044
        /// <summary>
        /// Gets or sets whether to prompt to create a new player if one does 
        /// not exist.
        /// </summary>
        public bool PromptForPlayerCreation
        {
            get
            {
                return m_promptForPlayerCreation;
            }
            set
            {
                m_promptForPlayerCreation = value;
            }
        }

        // Rally US505
        /// <summary>
        /// Gets or sets the type of CBB Play-It sheet to print.
        /// </summary>
        public CBBPlayItSheetType CBBPlayItSheetType
        {
            get
            {
                return m_cbbPlayItSheetType;
            }
            set
            {
                m_cbbPlayItSheetType = value;
            }
        }

        /// <summary>
        /// Gets or sets whether to print bingo cards to a normal printer 
        /// rather than a receipt printer.
        /// </summary>
        public bool PrintFacesToGlobalPrinter
        {
            get
            {
                return m_printFacesToGlobalPrinter;
            }
            set
            {
                m_printFacesToGlobalPrinter = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the global printer to print to.
        /// </summary>
        public string PrinterName
        {
            get
            {
                return m_printerName;
            }
            set
            {
                m_printerName = value;
            }
        }

        /// <summary>
        /// Gets or sets the point size to use when printing bingo cards to 
        /// the global printer.
        /// </summary>
        public float CardFacePointSize
        {
            get
            {
                return m_cardFacePointSize;
            }
            set
            {
                m_cardFacePointSize = value;
            }
        }

        /// <summary>
        /// Returns a Tuple&LT<bool, int, bool, bool&GT> for PrintPlayerPoints, ThirdPartyPlayerInterfaceID, ThirdPartyDoesRating, and we do the rating.
        /// </summary>
        public Tuple<bool, int, bool, bool> PlayerPointPrintingInfo
        {
            get
            {
                Tuple<bool, int, bool, bool> info = new Tuple<bool, int, bool, bool>(m_printPlayerPoints, m_ThirdPartyPlayerInterfaceID, ThirdPartyDoesRating, !ThirdPartyDoesRating && ThirdPartyRatingPoints != 0);
                return info;
            }
        }

        /// <summary>
        /// Gets or sets whether to print the player point information on 
        /// receipts.
        /// </summary>
        public bool PrintPlayerPoints
        {
            get
            {
                return m_printPlayerPoints;
            }
            set
            {
                m_printPlayerPoints = value;
            }
        }

        /// <summary>
        /// Gets or sets whether to print a signature line on a receipt.
        /// </summary>
        public bool PrintSignatureLine
        {
            get
            {
                return m_printSignatureLine;
            }
            set
            {
                m_printSignatureLine = value;
            }
        }

        /// <summary>
        /// Get if printed receipt is sorted.
        /// </summary>
        public bool PrintReceiptSortedByPackageType
        {
            get
            {
                return m_printReceiptSortedByPackageType;
            }
        }

        // Rally US505
        /// <summary>
        /// Gets or sets whether to print Crystal Ball cards faces on play-it
        /// sheets.
        /// </summary>
        public CBBPlayItSheetPrintMode CBBPlayItSheetPrintMode
        {
            get
            {
                return m_cbbPlayItSheetPrintMode;
            }
            set
            {
                m_cbbPlayItSheetPrintMode = value;
            }
        }

        /// <summary>
        /// Gets or sets whether this POS can sell electronics.
        /// </summary>
        public bool AllowElectronicSales
        {
            get
            {
                return m_allowElectronicSales;
            }
            set
            {
                m_allowElectronicSales = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of copies of receipts to print.
        /// </summary>
        public short ReceiptCopies
        {
            get
            {
                return m_receiptCopies;
            }
            set
            {
                m_receiptCopies = value;
            }
        }

        /// <summary>
        /// Gets or sets the receipts first disclaimer line.
        /// </summary>
        public string ReceiptDisclaimer1
        {
            get
            {
                return m_receiptDisclaimer1;
            }
            set
            {
                m_receiptDisclaimer1 = value;
            }
        }

        /// <summary>
        /// Gets or sets the receipts second disclaimer line.
        /// </summary>
        public string ReceiptDisclaimer2
        {
            get
            {
                return m_receiptDisclaimer2;
            }
            set
            {
                m_receiptDisclaimer2 = value;
            }
        }

        /// <summary>
        /// Gets or sets the receipts third disclaimer line.
        /// </summary>
        public string ReceiptDisclaimer3
        {
            get
            {
                return m_receiptDisclaimer3;
            }
            set
            {
                m_receiptDisclaimer3 = value;
            }
        }

        /// <summary>
        /// Gets or sets level of logging.
        /// </summary>
        public int LoggingLevel
        {
            get
            {
                return m_loggingLevel;
            }
            set
            {
                m_loggingLevel = value;
            }
        }

        public bool DoPointQualifyingAmountCalculationOldWay
        {
            get
            {
                return m_doPointQualifyingAmountCalculationOldWay;
            }
        }

        /// <summary>
        /// Gets or sets the number of days to keep a file log.
        /// </summary>
        public int FileLogRecycleDays
        {
            get
            {
                return m_fileLogRecycleDays;
            }
            set
            {
                m_fileLogRecycleDays = value;
            }
        }

        /// <summary>
        /// Gets or sets the drive letter the client software is installed on.
        /// </summary>
        public string ClientInstallDrive
        {
            get
            {
                return m_clientInstallDrive;
            }
            set
            {
                m_clientInstallDrive = value;
            }
        }

        /// <summary>
        /// Gets or sets the folder the client software is installed in.
        /// </summary>
        public string ClientInstallRootDir
        {
            get
            {
                return m_clientInstallRootDir;
            }
            set
            {
                m_clientInstallRootDir = value;
            }
        }

        /// <summary>
        /// Gets or sets whether to allow cross transfers of units.
        /// </summary>
        public bool AllowUnitCrossTransfers
        {
            get
            {
                return m_allowUnitCrossTransfers;
            }
            set
            {
                m_allowUnitCrossTransfers = value;
            }
        }

        // TTP 50114
        /// <summary>
        /// Gets or sets whether to show the unit assignment button.
        /// </summary>
        public bool EnableUnitAssignment
        {
            get
            {
                return m_enableUnitAssignment;
            }
            set
            {
                m_enableUnitAssignment = value;
            }
        }

        // TTP 50138
        // Rally TA7897
        /// <summary>
        /// Gets or sets whether credit is enabled in this system.
        /// </summary>
        public bool CreditEnabled
        {
            get
            {
                return m_creditEnabled;
            }
            set
            {
                m_creditEnabled = value;
            }
        }

        // TTP 50114
        /// <summary>
        /// Gets or sets whether player accounts are tied to a machine.
        /// </summary>
        public bool EnableAnonymousMachineAccounts
        {
            get
            {
                return m_enableAnonymousMachineAccounts;
            }
            set
            {
                m_enableAnonymousMachineAccounts = value;
            }
        }

        // TTP 50097
        /// <summary>
        /// Gets or sets whether to only print the staff's first name on a 
        /// receipt.
        /// </summary>
        public bool PrintStaffFirstNameOnly
        {
            get
            {
                return m_printStaffFirstNameOnly;
            }
            set
            {
                m_printStaffFirstNameOnly = value;
            }
        }

        // PDTS 571
        /// <summary>
        /// Gets or sets whether to allow quantity sales.
        /// </summary>
        public bool AllowQuantitySale
        {
            get
            {
                return m_allowQuantitySale;
            }
            set
            {
                m_allowQuantitySale = value;
            }
        }

        /// <summary>
        /// Gets or sets whether to print sales/void receipts for quantity 
        /// sales.
        /// </summary>
        public bool PrintQuantitySaleReceipts
        {
            get
            {
                return m_printQuantitySaleReceipts;
            }
            set
            {
                m_printQuantitySaleReceipts = value;
            }
        }

        /// <summary>
        /// Gets or sets how many times a quantity sale can be made to a player 
        /// account.
        /// </summary>
        public int MaxPlayerQuantitySale
        {
            get
            {
                return m_maxPlayerQuantitySale;
            }
            set
            {
                m_maxPlayerQuantitySale = value;
            }
        }

        // PDTS 964
        /// <summary>
        /// Gets or sets whether to print package product item names on a 
        /// receipt.
        /// </summary>
        public bool PrintProductNames
        {
            get
            {
                return m_printProductNames;
            }
            set
            {
                m_printProductNames = value;
            }
        }

        /// <summary>
        /// Gets or sets whether the system is in Main Stage compatibility 
        /// mode.
        /// </summary>
        public bool MainStageMode
        {
            get
            {
                return m_mainStageMode;
            }
            set
            {
                m_mainStageMode = value;
            }
        }

        public bool ForceDeviceSelectionWhenNoFees
        {
            get
            {
                return m_forceDeviceSelectionWhenNoFees;
            }

            set
            {
                m_forceDeviceSelectionWhenNoFees = value;
            }
        }

        public bool ShowFreeOnDeviceButtonsWithFeeOfZero
        {
            get
            {
                return m_showFreeOnDeviceButtonsWithFeeOfZero;
            }

            set
            {
                m_showFreeOnDeviceButtonsWithFeeOfZero = value;
            }
        }

        // PDTS 1064
        /// <summary>
        /// Gets or sets the mode of the MagneticCardReader (which sources 
        /// are enabled).
        /// </summary>
        public MagneticCardReaderMode MagCardMode
        {
            get
            {
                return m_magCardMode;
            }
            set
            {
                m_magCardMode = value;
            }
        }

        /// <summary>
        /// Gets or sets the setting string for the MagneticCardReader and 
        /// its sources.
        /// </summary>
        public string MagCardModeSettings
        {
            get
            {
                return m_magCardModeSettings;
            }
            set
            {
                m_magCardModeSettings = value;
            }
        }

        // Rally US419
        /// <summary>
        /// Gets or sets the bingo play type (i.e. Bingo or Lotto).
        /// </summary>
        public BingoPlayType PlayType
        {
            get
            {
                return m_playType;
            }
            set
            {
                m_playType = value;
            }
        }

        /// <summary>
        /// Gets or sets whether the system can sell to a fixed based unit.
        /// </summary>
        public bool HasFixed
        {
            get
            {
                return m_hasFixed;
            }
            set
            {
                m_hasFixed = value;
            }
        }

        /// <summary>
        /// Gets or sets whether the system can sell to a Traveler.
        /// </summary>
        public bool HasTraveler
        {
            get
            {
                return m_hasTraveler;
            }
            set
            {
                m_hasTraveler = value;
            }
        }

        /// <summary>
        /// Gets or sets whether the system can sell to a Tracker.
        /// </summary>
        public bool HasTracker
        {
            get
            {
                return m_hasTracker;
            }
            set
            {
                m_hasTracker = value;
            }
        }

        // Rally TA7729
        /// <summary>
        /// Gets or sets whether the system can sell to an Explorer.
        /// </summary>
        public bool HasExplorer
        {
            get
            {
                return m_hasExplorer;
            }
            set
            {
                m_hasExplorer = value;
            }
        }

        // PDTS 964
        // Rally US765
        /// <summary>
        /// Gets or sets whether the system can sell to a Traveler II.
        /// </summary>
        public bool HasTraveler2
        {
            get
            {
                return m_hasTraveler2;
            }
            set
            {
                m_hasTraveler2 = value;
            }
        }

        //US2908
        public bool HasTablet
        {
            get
            {
                return m_hasTablet;
            }
            set
            {
                m_hasTablet = value;
            }
        }


        /// <summary>
        /// Gets or sets the maximum cards that can be sold to a Traveler.
        /// </summary>
        public short TravelerMaxCards
        {
            get
            {
                return m_travelerMaxCards;
            }
            set
            {
                m_travelerMaxCards = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum cards that can be sold to a Tracker.
        /// </summary>
        public short TrackerMaxCards
        {
            get
            {
                return m_trackerMaxCards;
            }
            set
            {
                m_trackerMaxCards = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum cards that can be sold to a fixed base.
        /// </summary>
        public short FixedMaxCards
        {
            get
            {
                return m_fixedMaxCards;
            }
            set
            {
                m_fixedMaxCards = value;
            }
        }

        // Rally TA7729
        /// <summary>
        /// Gets or sets the maximum cards that can be sold to an Explorer.
        /// </summary>
        public short ExplorerMaxCards
        {
            get
            {
                return m_explorerMaxCards;
            }
            set
            {
                m_explorerMaxCards = value;
            }
        }

        // PDTS 964
        // Rally US765
        /// <summary>
        /// Gets or sets the maximum cards that can be sold to a Traveler II.
        /// </summary>
        public short Traveler2MaxCards
        {
            get
            {
                return m_traveler2MaxCards;
            }
            set
            {
                m_traveler2MaxCards = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of copies of payout receipts to print.
        /// </summary>
        public short PayoutReceiptCopies
        {
            get
            {
                return m_payoutReceiptCopies;
            }
            set
            {
                m_payoutReceiptCopies = value;
            }
        }

        // Rally US510
        /// <summary>
        /// Gets or sets whether pre-printed packs are being sold.
        /// </summary>
        public bool UsePrePrintedPacks
        {
            get
            {
                return m_usePrePrintedPacks;
            }
            set
            {
                m_usePrePrintedPacks = value;
            }
        }

        // FIX: TA4779
        /// <summary>
        /// Gets or sets the operator's maximum card limit.
        /// </summary>
        public int MaxCardLimit
        {
            get
            {
                return m_maxCardLimit;
            }
            set
            {
                m_maxCardLimit = value;
            }
        }

        /// <summary>
        /// Gets or sets whether to print cards on the receipt.
        /// </summary>
        public bool PrintCardFaces
        {
            get
            {
                return m_printCardFaces;
            }
            set
            {
                m_printCardFaces = value;
            }
        }

        // Rally TA5749
        /// <summary>
        /// Gets or sets which mode to print the card numbers for each game on the
        /// receipt.
        /// </summary>
        public PrintCardNumberMode PrintCardNumbers
        {
            get
            {
                return m_printCardNumbers;
            }
            set
            {
                m_printCardNumbers = value;
            }
        }
        // END: TA5749

        /// <summary>
        /// Gets or sets the first line of the receipt header.
        /// </summary>
        public string ReceiptHeaderLine1
        {
            get
            {
                return m_receiptHeaderLine1;
            }
            set
            {
                m_receiptHeaderLine1 = value;
            }
        }

        /// <summary>
        /// Gets or sets the second line of the receipt header.
        /// </summary>
        public string ReceiptHeaderLine2
        {
            get
            {
                return m_receiptHeaderLine2;
            }
            set
            {
                m_receiptHeaderLine2 = value;
            }
        }

        /// <summary>
        /// Gets or sets the third line of the receipt header.
        /// </summary>
        public string ReceiptHeaderLine3
        {
            get
            {
                return m_receiptHeaderLine3;
            }
            set
            {
                m_receiptHeaderLine3 = value;
            }
        }

        /// <summary>
        /// Gets or sets the first line of the receipt footer.
        /// </summary>
        public string ReceiptFooterLine1
        {
            get
            {
                return m_receiptFooterLine1;
            }
            set
            {
                m_receiptFooterLine1 = value;
            }
        }

        /// <summary>
        /// Gets or sets the second line of the receipt footer.
        /// </summary>
        public string ReceiptFooterLine2
        {
            get
            {
                return m_receiptFooterLine2;
            }
            set
            {
                m_receiptFooterLine2 = value;
            }
        }

        /// <summary>
        /// Gets or sets the third line of the receipt footer.
        /// </summary>
        public string ReceiptFooterLine3
        {
            get
            {
                return m_receiptFooterLine3;
            }
            set
            {
                m_receiptFooterLine3 = value;
            }
        }
        // END: TA4779

        // FIX: DE1938
        /// <summary>
        /// Gets or sets the sales tax rate.
        /// </summary>
        public decimal TaxRate
        {
            get
            {
                return m_taxRate;
            }
            set
            {
                m_taxRate = value;
            }
        }
        // END: DE1938

        // Rally TA5748
        /// <summary>
        /// Gets or sets whether to play with paper.
        /// </summary>
        public bool PlayWithPaper
        {
            get
            {
                return m_playWithPaper;
            }
            set
            {
                m_playWithPaper = value;
            }
        }
        // END: TA5748

        // Rally TA6050
        /// <summary>
        /// Gets or sets whether packs sold to a player account must all be
        /// used on the same machine.
        /// </summary>
        public bool ForcePackToPlayer
        {
            get
            {
                return m_forcePacksToPlayer;
            }
            set
            {
                m_forcePacksToPlayer = value;
            }
        }

        // Rally TA6385
        /// <summary>
        /// Gets or sets whether Melange special games are enabled in the
        /// system.
        /// </summary>
        public bool AllowMelangeSpecialGames
        {
            get
            {
                return m_allowSpecialGames;
            }
            set
            {
                m_allowSpecialGames = value;
            }
        }

        // Rally TA7897
        /// <summary>
        /// Gets or sets whether Crystal Ball Bingo is enabled in the system.
        /// </summary>
        public bool CrystalBallEnabled
        {
            get
            {
                return m_cbbEnabled;
            }
            set
            {
                m_cbbEnabled = value;
            }
        }

        // FIX: DE4052
        /// <summary>
        /// Gets or sets whether CBB Quick Picks are enabled in the system.
        /// </summary>
        public bool CBBQuickPickEnabled
        {
            get
            {
                return m_cbbQuickPickEnabled;
            }
            set
            {
                m_cbbQuickPickEnabled = value;
            }
        }

        // Rally TA7465
        /// <summary>
        /// Gets or sets whether multiple currencies are defined in the system.
        /// </summary>
        public bool MultiCurrencies
        {
            get
            {
                return m_multiCurrency;
            }
            set
            {
                m_multiCurrency = value;
            }
        }

        // Rally US1658
        /// <summary>
        /// Gets or sets whether to apply exchange rates when selling.
        /// </summary>
        public bool UseExchangeRateOnSale
        {
            get
            {
                return m_useExchangeRateOnSale;
            }
            set
            {
                m_useExchangeRateOnSale = value;
            }
        }

        // Rally US1650
        /// <summary>
        /// Gets or sets whether to show the register closing report.
        /// </summary>
        public bool EnableRegisterSalesReport
        {
            get
            {
                return m_enableRegisterSalesReport;
            }
            set
            {
                m_enableRegisterSalesReport = value;
            }
        }

        // US2828
        /// <summary>
        /// Gets the status of the active sales session setting
        /// </summary>
        public bool EnableActiveSalesSession
        {
            get
            {
                return (m_enableActiveSession);
            }
        }

        public bool AutoDiscountInfoGoesOnBottomOfScreenReceipt
        {
            get
            {
                if (DisplayMode is WideDisplayMode) //stairstep text has its own window.
                    return false;

                return m_autoDiscountInfoGoesOnBottomOfScreenReceipt;
            }
        }

        /// US4028
        /// <summary>
        /// Gets the status of if card counts occur per product
        /// </summary>
        public bool CheckCardCountsPerProduct
        {
            get
            {
                return (m_checkProductCardCount);
            }
        }

        // Rally US1854
        /// <summary>
        /// Gets or sets if 0 or below sales are allowed.
        /// </summary>
        public MinimumSaleAllowed MinimumSaleAllowed
        {
            get
            {
                return m_minimumSaleAllowed;
            }
            set
            {
                m_minimumSaleAllowed = value;
            }
        }

        // US2057
        /// <summary>
        /// Gets or sets whether to show the keypad or keyboard when prompting
        /// for passwords.
        /// </summary>
        public bool UsePasswordKeypad
        {
            get
            {
                return m_usePasswordKeypad;
            }
            set
            {
                m_usePasswordKeypad = value;
            }
        }

        // US2139
        /// <summary>
        /// Gets or sets whether to print the points redeemed column header on
        /// the receipt.
        /// </summary>
        public bool PrintPointsRedeemed
        {
            get
            {
                return m_printPtsRedeemed;
            }
            set
            {
                m_printPtsRedeemed = value;
            }
        }

        // US1808
        /// <summary>
        /// Gets or sets whether to print the Register Sales report by package
        /// or product.
        /// </summary>
        public bool PrintRegisterSalesByPackage
        {
            get
            {
                return m_printRegisterSalesByPackage;
            }
            set
            {
                m_printRegisterSalesByPackage = value;
            }
        }

        /// <summary>
        /// Gets or sets whether the user is allowed to process a cash tender
        /// </summary>
        public bool AllowCashTender
        {
            get
            {
                return m_allowCash;
            }
        }

        /// <summary>
        /// Gets or sets whether the user is allowed to process a credit card
        /// tender
        /// </summary>
        public bool AllowCreditCardTender
        {
            get
            {
                return m_allowCreditCards;
            }

            set
            {
                m_allowCreditCards = value;
            }
        }

        /// <summary>
        /// Gets or sets whether the user is allowed to process a check tender
        /// </summary>
        public bool AllowCheckTender 
        {
            get
            {
                return m_allowChecks;
            }
        }

        /// <summary>
        /// Gets or sets whether the user is allowed to process a debit card
        /// tender
        /// </summary>
        public bool AllowDebitCardTender
        {
            get
            {
                return m_allowDebitCards;
            }

            set
            {
                m_allowDebitCards = value;
            }
        }

        /// <summary>
        /// Gets or sets whether the user is allowed to process a money order
        /// tender
        /// </summary>
        public bool AllowMoneyOrderTender
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets or sets whether the user is allowed to process a coupon tender
        /// </summary>
        public bool AllowCouponTender
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets or sets whether the user is allowed to process a gift card
        /// tender
        /// </summary>
        public bool AllowGiftCardTender
        {
            get
            {
                return m_allowGiftCards;
            }
        }

        /// <summary>
        /// Gets or sets whether the user is allowed to process a chip tender
        /// </summary>
        public bool AllowChipTender
        {
            get
            {
                return false;
            }
        }

        public int OperatorID
        {
            get
            {
                return m_operatorID;
            }

            set
            {
                m_operatorID = value;
            }
        }

		/// <summary>
        /// Get or sets value if coupon management is true(enable) or false(disable) 
        /// in system setting.
        /// </summary>
        public bool isCouponManagement
        {
            get
            {
                return m_isCouponManagement && m_displayMode.BaseFormSize != m_displayMode.CompactFormSize;
            }
            set
            {
                m_isCouponManagement = value;
            }
        }


        /// <summary>
        /// Get true or false. If true then pre tax else post tax.
        /// </summary>
        public bool CouponTaxable
        {
            get
            {
                return m_rCouponTaxable;
            }
            set
            {
                m_rCouponTaxable = value;
            }
        }

        //US4120
        public int PlayerPinLength
        {
            get
            {
                return m_playerPinLength;
            }
        }

        public bool NorthDakotaSalesMode
        {
            get
            {
                return m_northDakotaSalesMode;
            }
        }

        public int ThirdPartyPlayerInterfaceID
        {
            get
            {
                return m_ThirdPartyPlayerInterfaceID;
            }
        }
 
        public int ThirdPartyPlayerInterfacePINLength
        {
            get
            {
                return m_ThirdPartyPlayerInterfacePINLength;
            }
        }
 
        public bool ThirdPartyPlayerInterfaceGetPINWhenCardSwiped
        {
            get
            {
                return (m_ThirdPartyPlayerInterfaceID == 0 ? false : m_ThirdPartyPlayerInterfaceGetPINWhenCardSwiped);
            }
        }

        public bool ThirdPartyPlayerInterfaceNeedPINForPoints
        {
            get
            {
                return (m_ThirdPartyPlayerInterfaceID == 0 ? false : m_ThirdPartyPlayerInterfaceNeedPINForPoints);
            }
        }
         
        public bool ThirdPartyPlayerInterfaceNeedPINForRating
        {
            get
            {
                return (m_ThirdPartyPlayerInterfaceID == 0 ? false : m_ThirdPartyPlayerInterfaceNeedPINForRating);
            }
        }

        public bool ThirdPartyPlayerInterfaceNeedPINForRatingVoid
        {
            get
            {
                return (m_ThirdPartyPlayerInterfaceID == 0 ? false : m_ThirdPartyPlayerInterfaceNeedPINForRatingVoid);
            }
        }

        public bool ThirdPartyPlayerInterfaceNeedPINForRedemptionVoid
        {
            get
            {
                return (m_ThirdPartyPlayerInterfaceID == 0 ? false : m_ThirdPartyPlayerInterfaceNeedPINForRedemptionVoid);
            }
        }

        public bool ThirdPartyPlayerInterfaceNeedPINForRedemption
        {
            get
            {
                return (m_ThirdPartyPlayerInterfaceID == 0 ? false : m_ThirdPartyPlayerInterfaceNeedPINForRedemption);
            }
        }

        public bool ThirdPartyPlayerInterfaceUsesPIN
        {
            get
            {
                return ThirdPartyPlayerInterfaceNeedPINForPoints || ThirdPartyPlayerInterfaceNeedPINForRating || ThirdPartyPlayerInterfaceNeedPINForRatingVoid || ThirdPartyPlayerInterfaceNeedPINForRedemption || ThirdPartyPlayerInterfaceNeedPINForRedemptionVoid || ThirdPartyPlayerInterfaceGetPINWhenCardSwiped;
            }
        }

        /// <summary>
        /// Returns true if the third party player tracking system does 
        /// ratings (computes points earned based on amount spent).
        /// </summary>
        public bool ThirdPartyDoesRating
        {
            get
            {
                return m_ThirdPartyPlayerInterfaceDoesExternalRating;
            }
        }

        /// <summary>
        /// Returns the number of points earned for every ThirdPartyRatingDollars in the buy-in.
        /// </summary>
        public decimal ThirdPartyRatingPoints
        {
            get
            {
                if (!m_ThirdPartyPlayerInterfaceDoesExternalRating)
                    return Convert.ToDecimal(m_ThirdPartyPlayerInterfaceRatingPoints);
                else
                    return 0;
            }
        }

        /// <summary>
        /// Returns the dollars in the buy-in required to earn ThirdPartyRatingPoints points.
        /// </summary>
        public decimal ThirdPartyRatingDollars
        {
            get
            {
                if (!m_ThirdPartyPlayerInterfaceDoesExternalRating)
                    return Convert.ToDecimal(m_ThirdPartyPlayerInterfaceRatingPennies) / 100M;
                else
                    return 1M; //avoid division by zero
            }
        }

        public decimal PointRedemptionValue
        {
            get
            {
                return m_ThirdPartyPlayerInterfacePointPennies / m_ThirdPartyPlayerInterfacePointPoints;
            }
        }

        /// <summary>
        /// Gets the player sync mode for third party player tracking interface.
        /// 0=real time
        /// 1=PIN verification and points 
        /// 2=PIN verification, points, and updating
        /// </summary>
        public int ThirdPartyPlayerSyncMode
        {
            get
            {
                return m_ThirdPartyPlayerInterfaceID == 0? 0 : m_ThirdPartyPlayerSyncMode;
            }
        }

        //US4380
        public bool EnableB3Management
        {
            get
            {
                return m_enableB3Management; 
            }

            set
            {
                m_enableB3Management = value;
            }
        }

        public bool B3IsMultiOperator { get; private set; }
        public bool B3IsCommonRng { get; private set; }
        public bool B3AllowInSessBallChange { get; private set; }
        public bool B3EnforceMix { get; private set; }
        public bool B3IsDoubleAccount { get; private set; }

        //US3509
        public bool EnabledProductValidation
        {
            get { return m_enableProductValidation; }
            private set { m_enableProductValidation = value; }
        }

        //US3509
        public int ProductValidationCardCount
        {
            get { return m_productValidationCardCount; }
            private set { m_productValidationCardCount = value; }
        }

        //US3509
        public int ProductValidationMaxQuantity
        {
            get { return m_productValidationMaxQuantity; }
            private set { m_productValidationMaxQuantity = value; }
        }

        //US4434
        public bool AutoIssueBank
        {
            get { return m_autoIssueBank; }
            private set { m_autoIssueBank = value; }
        }

        // Settings for tendering/payment processing
        /// <summary>
        /// Gets or sets whether all tendering is in cash or if multiple tendering methods are available
        /// </summary>
        public bool EnableFlexTendering
        {
            get
            {
                return m_enableFlexTendering && m_displayMode.BaseFormSize != m_displayMode.CompactFormSize;
            }
            set
            {
                m_enableFlexTendering = value;
            }
        }

        public bool KiosksCanOnlySellFromTheirButtons
        {
            get
            {
                return m_kiosksCanOnlySellFromTheirButtons;
            }
        }

        /// <summary>
        /// Gets or sets whether non-flex cash sales will be written to the database
        /// </summary>
        public bool PostCashTendering
        {
            get
            {
                return m_postCashTendering;
            }
            set
            {
                m_postCashTendering = value;
            }
        }

        /// <summary>
        /// Gets or sets how to handle selling a previously
        /// sold item (serial number and audit number sold).
        /// </summary>
        public SellAgainOption SellPreviouslySoldItem
        {
            get
            {
                return m_sellPreviouslySoldItem;
            }
            set
            {
                m_sellPreviouslySoldItem = value;
            }
        }

        public bool ForceAuthorizationOnVoidsAtPOS
        {
            get
            {
                return m_forceAuthorizationOnVoidsAtPOS;
            }
        }

        /// <summary>
        /// Gets or sets if a buyer can pay with multiple 
        /// tendering methods on a single transaction
        /// </summary>
        public bool AllowSplitTendering
        {
            get
            {
                return m_allowSplitTendering;
            }
            set
            {
                m_allowSplitTendering = value;
            }
        }

        /// <summary>
        /// Gets or sets how check refunds are handled
        /// 0 - disallow void, 1 - return check, 2 - refund in cash
        /// </summary>
        public int CheckVoidType
        {
            get
            {
                return m_checkVoidType;
            }
            set
            {
                m_checkVoidType = value;
            }
        }

        /// <summary>
        /// Gets or sets how refunds for failed card voids are handled
        /// If set to 1, cash is refunded for the card amount not voided or refunded
        /// </summary>
        public bool RefundCashOnFailedCardVoid
        {
            get
            {
                return m_refundCashOnFailedCardVoid;
            }
            set
            {
                m_refundCashOnFailedCardVoid = value;
            }
        }

        /// <summary>
        /// Gets or sets if a user can manually enter a card with reading errors
        /// </summary>
        public bool AllowManualCardEntry
        {
            get
            {
                return m_allowManualCardEntry;
            }
            set
            {
                m_allowManualCardEntry = value;
            }
        }

        /// <summary>
        /// Gets or sets what the displayed welcome message on the PIN pad is
        /// </summary>
        public string PinPadGreeting
        {
            get
            {
                return m_pinPadGreeting;
            }
            set
            {
                m_pinPadGreeting = value;
            }
        }

        /// <summary>
        /// Gets or sets if the PIN pad should be used
        /// If set to 0, cards must be entered manually
        /// </summary>
        public bool PinPadEnabled
        {
            get
            {
                return m_pinPadEnabled;
            }
            set
            {
                m_pinPadEnabled = value;
            }
        }

        /// <summary>
        /// Gets or sets what the message displayed when a card 
        /// transaction fails is on the PIN pad is
        /// </summary>
        public string PinPadCardFailedMessage
        {
            get
            {
                return m_pinPadCardFailedMessage;
            }
            set
            {
                m_pinPadCardFailedMessage = value;
            }
        }

        /// <summary>
        /// Gets or sets if a buyer's signature is needed if the total amount of a 
        /// card-based transcation exceeds a certain threshold
        /// </summary>
        public decimal MaximumTotalNotRequiringSignature
        {
            get
            {
                return m_maxTotalNotRequiringSignature;
            }
            set
            {
                m_maxTotalNotRequiringSignature = value;
            }
        }

        /// <summary>
        /// Gets or sets what the message displayed when a  
        /// transaction is finished on the PIN pad is
        /// </summary>
        public string PinPadPostSaleMessage
        {
            get
            {
                return PinPadPostSaleMessage;
            }
            set
            {
                PinPadPostSaleMessage = value;
            }
        }

        /// <summary>
        /// Gets or sets if item descriptions and prices are displayed on the 
        /// PIN pad as they are rung up
        /// </summary>
        public bool DisplayItemDetailOnPinPad
        {
            get
            {
                return m_displayItemDetailOnPinPad;
            }
            set
            {
                m_displayItemDetailOnPinPad = value;
            }
        }

        /// <summary>
        /// Gets or sets what symbol will be displayed to represent money on the PIN pad
        /// </summary>
        public string PinPadCurrencySymbol
        {
            get
            {
                return PinPadCurrencySymbol;
            }
            set
            {
                PinPadCurrencySymbol = value;
            }
        }

        /// <summary>
        /// Gets or sets what the message displayed when a  
        /// PIN pad is closed is
        /// </summary>
        public string PinPadStationClosedMessage
        {
            get
            {
                return m_pinPadStationClosedMessage;
            }
            set
            {
                m_pinPadStationClosedMessage = value;
            }
        }

        /// <summary>
        /// Gets or sets if the subtotal of the sale is displayed on the 
        /// PIN pad as items are being rung up
        /// </summary>
        public bool DisplaySubtotalOnPinPad
        {
            get
            {
                return m_displaySubtotalOnPinPad;
            }
            set
            {
                m_displaySubtotalOnPinPad = value;
            }
        }

        /// <summary>
        /// Gets or sets if two receipts will be printed for non-cash transactions
        /// </summary>
        public bool PrintDualReceiptsForNonCashSales
        {
            get
            {
                return m_printDualReceiptsForNonCashSales;
            }
            set
            {
                m_printDualReceiptsForNonCashSales = value;
            }
        }

        /// <summary>
        /// Gets or sets if a debit refund will be given back in cash or not
        /// </summary>
        public bool RefundCashForDebit
        {
            get
            {
                return m_refundCashForDebit;
            }
            set
            {
                m_refundCashForDebit = value;
            }
        }

        /// <summary>
        /// Gets or sets whether payment processing is available
        /// </summary>
        public bool PaymentProcessingEnabled
        {
            get
            {
                return m_paymentProcessingEnabled && CreditCardProcessor != CreditCardProcessors.None;
            }
            set
            {
                m_paymentProcessingEnabled = value;
            }
        }

        public string PaymentProcessingDeviceAddress
        {
            get { return m_precidiaDeviceAddress; }
            set { m_precidiaDeviceAddress = value; }
        }

        public int PaymentProcessingDevicePort
        {
            get { return m_precidiaDevicePort; }
            set { m_precidiaDevicePort = value; }
        }

        /// <summary>
        /// If ApproveChecksThroughPaymentProcessor = 1:
        /// 0 - checks are verified as good or bad
        /// 1 - checks are processes as fucnds from a checking account
        /// </summary>
        public bool ProcessFundsTransferForChecks
        {
            get
            {
                return m_processFundsTransferForChecks;
            }
            set
            {
                m_processFundsTransferForChecks = value;
            }
        }
        // End settings for tendering/payment processing

        public bool PrintPosBankDenomReceipt
        {
            get
            {
                return m_printPosBankDenomReceipt;
            }
            private set
            {
                m_printPosBankDenomReceipt = value;
            }
        }

        public bool Use00ForCurrencyEntry
        {
            get
            {
                return m_use00ForCurrencyEntry;
            }

            set
            {
                m_use00ForCurrencyEntry = value;
            }
        }

        private CreditCardProcessors _creditCardProcessor;

        public CreditCardProcessors CreditCardProcessor
        {
            get { return _creditCardProcessor; }
            set { _creditCardProcessor = value; }
        }

        public string Shift4AuthCode
        {
            get { return m_shift4AuthCode; }
            set { m_shift4AuthCode = value; }
        }

        public int PinPadDisplayLineCount
        {
            get { return m_pinPadDisplayLineCount; }
            set { m_pinPadDisplayLineCount = value; }
        }

        public int PinPadDisplayColumnCount
        {
            get { return m_pinPadDisplayColumnCount; }
            set { m_pinPadDisplayColumnCount = value; }
        }

        public bool PinPadDisplayMessages
        {
            get { return m_pinPadDisplayMessages; }
            set { m_pinPadDisplayMessages = value; }
        }

        public int PaymentProcessorCommunicationsTimeout
        {
            get { return m_paymentProcessorCommunicationsTimeout; }
            set { m_paymentProcessorCommunicationsTimeout = value; }
        }

        //US4804
        public bool UseLinearGameNumbering {get { return m_useLinearGameNumbering; }}

        public bool RemoveDiscountsInRepeatSale
        {
            get
            {
                return m_removeDiscountsInRepeatSale;
            }

            set
            {
                m_removeDiscountsInRepeatSale = value;
            }
        }
        
        public bool RemovePaperInRepeatSale
        {
            get
            {
                return m_removePaperInRepeatSale;
            }

            set
            {
                m_removePaperInRepeatSale = value;
            }
        }

        public bool RemovePackagesWithCouponsInRepeatSale
        {
            get
            {
                return m_removePackagesWithCouponsInRepeatSale;
            }

            set
            {
                m_removePackagesWithCouponsInRepeatSale = value;
            }
        }

        public bool LongPOSDescriptions
        {
            get
            {
                return m_longPOSDescriptions;
            }

            set
            {
                m_longPOSDescriptions = value;
            }
        }

        public bool ReturnToPageOneAfterSale
        {
            get
            {
                return m_returnToPageOneAfterSale;
            }

            set
            {
                m_returnToPageOneAfterSale = value;
            }
        }

        public bool ShowTwoMenuPagesPerPage
        {
            get
            {
                return m_showTwoMenuPagesPerPageIfWidescreen;
            }
        }

        public bool AllowWidescreenPOS
        {
            get
            {
                return m_AllowWidescreenPOS;
            }
        }

        public bool EnablePaperUsage { get { return m_enablePaperUsage; } }

        public bool ShowPaperUsageAtLogin { get { return m_showPaperUsageAtLogin; } }

        public bool PrintCompactPaperPacksSoldReportOnReceipt
        {
            get;
            protected set;
        }

        public string OperatorZipCode
        {
            get;
            set;
        }

        public bool UseSystemMenuForUnitSelection
        {
            get
            {
                return m_selectElectronicDeviceThroughSystemMenu;
            }

            set
            {
                m_selectElectronicDeviceThroughSystemMenu = value;
            }
        }

        //US4511: Support Chatsworth CBB scanner
        //-1= No scanner
        //0 = PDI VMR-138
        //1 = Chatsworth ACP-100
        //2 = Chatsworth ACP-200
        public SupportedOMRDevices CbbScannerType
        {
            get
            {
                return (SupportedOMRDevices)m_cbbScannerType;
            }
        }

        //US5115: POS: Add Register Closing report button
        public bool EnableRegisterClosingReport
        {
            get
            {
                return m_enableRegisterClosingReport;
            }
        }

        //US5108: POS > Bank Close: Print register closing report
        public bool PrintRegisterClosingOnBankClose
        {
            get
            {
                return m_printRegisterClosingOnBankClose;
            }
        }

        //DE13632
        public int BankCloseSignatureLineCount
        {
            get { return m_bankCloseSignatureLineCount; }
        }

        //DE13632
        public int NumberOfBankCloseReceipts
        {
            get { return m_numberOfBankCloseReceipts; }
        }
        /// <summary>
        /// Get the number of seconds a kiosk can be idle.
        /// </summary>
        public int KioskIdleTimeout
        {
            get
            {
                return m_kioskIdleTimeout;
            }
        }

        /// <summary>
        /// Get the number of seconds a kiosk can be idle for a simple task.
        /// </summary>
        public int KioskShortIdleTimeout
        {
            get
            {
                return m_kioskShortIdleTimeout;
            }
        }

        /// <summary>
        /// Get the number of milliseconds before a message box on a kiosk will select its default button.
        /// </summary>
        public int KioskMessageTimeout
        {
            get
            {
                return m_kioskMessageTimeout;
            }
        }

        public string KioskAttractText
        {
            get
            {
                return m_kioskAttractText;
            }
        }

        public string KioskClosedText
        {
            get
            {
                return m_kioskClosedText;
            }
        }

        public bool UseSimplePaymentForAdvancedKiosk
        {
            get
            {
                return m_useSimplePaymentForAdvancedKiosk;
            }
        }

        public bool AllowUseLastPurchaseButtonOnKiosk
        {
            get
            {
                return m_allowUseLastPurchaseButtonOnKiosk;
            }
        }

        public bool AllowB3OnKiosk
        {
            get
            {
                return m_allowB3OnKiosk;
            }

            set
            {
                m_allowB3OnKiosk = value;
            }
        }

        public bool ShowQuantityOnMenuButtons
        {
            get
            {
                return m_showQuantityOnMenuButtons;
            }

            set
            {
                m_showQuantityOnMenuButtons = value;
            }
        }

        public bool PrintPlayerIdentityAsAccount
        {
            get
            {
                return m_printPlayerIdentityAsAccountNumber;
            }
        }

        public bool PrintPlayerID
        {
            get
            {
                return m_printPlayerID;
            }
        }

        public bool PrintIncompleteTransactionReceipts
        {
            get
            {
                return m_printIncompleteTransactionReceipts;
            }

            set
            {
                m_printIncompleteTransactionReceipts = value;
            }
        }

        public string IncompleteTransactionLine1
        {
            get
            {
                return m_incompleteTransactionReceiptText1;
            }
        }

        public string IncompleteTransactionLine2
        {
            get
            {
                return m_incompleteTransactionReceiptText2;
            }
        }

        public OptionsForGivingChange ChangeDispensing
        {
            get
            {
                return m_changeDispensing;
            }

            set
            {
                m_changeDispensing = value;
            }
        }

        public bool AllowKiosksToPrintCBBPlayItSheetsFromReceiptScan
        {
            get
            {
                return m_allowKiosksToPrintCBBPlayItSheetsFromMainScreen;
            }

            set
            {
                m_allowKiosksToPrintCBBPlayItSheetsFromMainScreen = value;
            }
        }

        public int KioskCrashRecoveryNeedAttendantAfterNMinutes
        {
            get
            {
                return m_kioskCrashRecoveryNeedAttendantAfterNMinutes;
            }
        }

        public bool KioskTimeoutPulseDefaultButton
        {
            get
            {
                return m_kioskTimeoutPulseDefaultButton;
            }
        }

        public bool DeviceFeesQualifyForPoints
        {
            get
            {
                return m_deviceFeesQualifyForPoints;
            }
        }

        public bool UseKeyClickSoundsOnKiosk
        {
            get
            {
                return m_useKeyClickSoundsOnKiosk;
            }
        }

        public bool StabilizeCabinet
        {
            get
            {
                return m_stabilizeCabinet;
            }
        }

        public string TicketPrinterName
        {
            get
            {
                return m_ticketPrinterName;
            }

            set
            {
                m_ticketPrinterName = value;
            }
        }

        public int KioskVideoVolume
        {
            get
            {
                return m_kioskVideoVolume;
            }
        }

        public bool EnablePresales 
        {
            private set
            {
                m_enablePresales = value;
            }
            get
            {
                return m_enablePresales;
            }
        }
        #endregion
    }
}
