using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GTI.Modules.Shared;
using GTI.Modules.POS.Business;
using GTI.Modules.POS.Properties;
using GTI.Controls;
using System.Text.RegularExpressions;
using GTI.Modules.POS.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using Microsoft.Win32;
using GTI.EliteCreditCards.Business;

namespace GTI.Modules.POS.UI
{
    internal partial class SimpleKioskForm : POSForm
    {
        #region Member variables and types

        public enum KioskState
        {
            Idle = 0,
            GetPlayerCard,
            GetWhatWeAreSelling,
            GetReceiptFromBarcode,
            GetPaperFromBarcode,
            GetItems,
            GetB3Funding,
            GetQuitOrBuyForReceipt,
            GetPayment,
            PrintingCBBSheets,
            Closed
        }

        private KioskState m_state = KioskState.Idle;
        private PointOfSale m_pos = null;
        private SellingForm m_sellingForm = null;
        private MagneticCardReader m_MSR = null;
        private StringBuilder m_barcodeBuffer = new StringBuilder();
        private bool m_weHavePaperButtons = false;
        private bool m_kioskVideoEnabled = true;
        private bool m_kioskClosed = false;
        private bool m_couponButtonUsed = false;
        private List<ImageButton> m_buttonsDisabledForNoPlayer = new List<ImageButton>();
        private string[] m_videoList = new string[0];
        private int m_lastVideoIndex = 0;
        private object m_videoLock = new object();
        private object m_videoBurnInLock = new object();
        private Thread m_videoChangeThread = null;
        private Point m_swipeStart = new Point(0, 0);
        private Point m_swipeEnd = new Point(0, 0);
        private bool m_weCanPrintCBBPlayItSheets;

        #endregion

        #region Constructors

        public SimpleKioskForm(PointOfSale pos, SellingForm sell)
            : base(pos, pos.Settings.DisplayMode)
        {
            m_pos = pos;
            m_sellingForm = sell;
            m_weCanPrintCBBPlayItSheets = m_pos.Settings.AllowKiosksToPrintCBBPlayItSheetsFromReceiptScan && !string.IsNullOrWhiteSpace(m_pos.Settings.PrinterName) && m_pos.Settings.CBBPlayItSheetPrintMode != CBBPlayItSheetPrintMode.Off;
            ListBoxTenderItem.PosSettings = m_pos.Settings;
            ListBoxTenderItem.POS = m_pos;

            InitializeComponent();

            m_idleTimerForVideo.Interval = KioskIdleLimitInSeconds * 2000;

            ApplyKioskDisplayMode();

            m_panelVideo.Size = m_displayMode.FormSize;
            m_panelVideo.Location = new Point(0, 0);
            m_panelVideo.BackColor = Color.Black;

            //figure out the video text size based on the user defined text.
            m_lblVideoPrompt.Size = new Size(m_displayMode.FormSize.Width, 80);

            Graphics g = m_lblVideoPrompt.CreateGraphics();
            SizeF attractLabelSize = g.MeasureString(m_pos.Settings.KioskAttractText, m_lblVideoPrompt.Font, m_displayMode.FormSize.Width);
            SizeF closedLabelSize = g.MeasureString(m_pos.Settings.KioskClosedText, m_lblVideoPrompt.Font, m_displayMode.FormSize.Width);

            m_lblVideoPrompt.Size = new Size(m_displayMode.FormSize.Width, (int)Math.Ceiling((Double)Math.Max(attractLabelSize.Height, closedLabelSize.Height)));
            m_lblVideoPrompt.Location = new Point(0, 0);

            m_panelVideo2.Size = new Size(m_displayMode.FormSize.Width, m_displayMode.FormSize.Height - m_lblVideoPrompt.Size.Height);
            m_panelVideo2.Location = new Point(0, m_lblVideoPrompt.Size.Height);
            m_panelVideo2.BackColor = Color.Black;

            if (m_pos.WeAreAHybridKiosk)
            {
                m_lblWelcomeBack.Location = new Point(350, 4);
                m_lblAutoCoupons.Location = new Point(m_lblWelcomeBack.Location.X, m_lblWelcomeBack.Location.Y + m_lblWelcomeBack.Size.Height);
                m_lbReceipt.Size = new Size(325, m_lbReceipt.Size.Height);
                m_lbReceipt.Font = new Font(m_lbReceipt.Font.FontFamily, 9, m_lbReceipt.Font.Style);

                if (!m_pos.Settings.AllowCouponButtonOnHybridKiosk)
                {
                    m_btnCoupons.Visible = false;
                    m_lblHybridAutoCoupons.Location = m_btnDevice.Location;
                    m_btnDevice.Location = m_btnCoupons.Location;
                }
            }
            else if (m_pos.WeAreASimplePOSKiosk)
            {
                m_lblWelcomeBack.Location = new Point((m_panelReceipt.Width - m_lblWelcomeBack.Width) / 2, 4);
                m_lblWelcomeBack.TextAlign = ContentAlignment.TopCenter;
                m_lbReceipt.Visible = false;
                m_panelKioskMenuButtons.Location = new Point(13, 114);
                m_panelKioskMenuButtons.Size = new Size(m_panelReceipt.Width, m_panelReceipt.Height - 114);
            }
            else if (m_pos.WeAreABuyAgainKiosk)
            {
                m_lblWelcomeBack.Location = new Point(547, 145);
                m_lblAutoCoupons.Location = new Point(m_lblWelcomeBack.Location.X, m_lblWelcomeBack.Location.Y + m_lblWelcomeBack.Size.Height);
                m_lbReceipt.Size = new Size(537, m_lbReceipt.Size.Height);
                m_lbReceipt.Font = new Font(m_lbReceipt.Font.FontFamily, 15, m_lbReceipt.Font.Style);
                m_btnPaperHelp.Visible = false;
                m_btnReceiptHelp.Visible = false;
            }
            else //advanced Kiosk
            {
                m_btnCoupons.Visible = false;
                m_lbReceipt.Visible = false;
                m_btnPaperHelp.Visible = false;
                m_btnReceiptHelp.Visible = false;
                m_simpleKioskProgress.Visible = false;
            }

            m_panelB3OrBingo.Size = m_panelReceipt.Size;
            m_panelB3OrBingo.Location = m_panelReceipt.Location;

            int widthOfOneHalf = m_panelB3OrBingo.Size.Width / 2; 
            int offsetForCentering = (widthOfOneHalf - m_btnB3.Size.Width) / 2;

            m_btnB3.Location = new Point(offsetForCentering, m_btnB3.Location.Y);
            m_btnBingo.Location = new Point(widthOfOneHalf + offsetForCentering, m_btnBingo.Location.Y);

            m_MSR = m_pos.MagCardReader;

            m_MSR.CardSwiped += CardSwiped;

            if(m_pos.CurrentMenu != null && !m_pos.WeAreAnAdvancedPOSKiosk)
                CreateMenuButtons();

            m_axWindowsMediaPlayer.PlayStateChange += new AxWMPLib._WMPOCXEvents_PlayStateChangeEventHandler(m_axWindowsMediaPlayer_PlayStateChange);
            m_axWindowsMediaPlayer.uiMode = "none";
            m_axWindowsMediaPlayer.stretchToFit = true;
            m_axWindowsMediaPlayer.settings.enableErrorDialogs = false;
            m_axWindowsMediaPlayer.settings.volume = m_parent.Settings.KioskVideoVolume;
            m_axWindowsMediaPlayer.Ctlenabled = false;
            m_idleTimerForVideo.Enabled = true;

            HandleKeyAudio(m_panelMain);

            //if (!m_pos.WeAreAnAdvancedPOSKiosk)
            //{
            //    if (m_pos.CurrentMenu == null)
            //        CloseKiosk();
            //    else
            //        StartOver();
            //}
            //else
            //{
                StartIdleState();
            //}
        }

        #endregion

        #region Public Methods

        #region State change - External

        public void SwipeCard(object sender, MagneticCardSwipeArgs e)
        {
            Focus();
            CardSwiped(sender, e);
        }

        /// <summary>
        /// Bring the kiosk back to the start of a transaction.
        /// </summary>
        public void StartOver(bool updateMenu = false)
        {
            m_sellingForm.NotIdle();
            m_testModeTimer.Stop();

            if (m_parent.SellingForm != null && m_parent.SellingForm.GiveChangeAsB3Credit)
            {
                StartWaitingForB3();
                return;
            }

            if (m_pos.CurrentSale != null) //something went wrong with the sale, keep in it
            {
                if (m_pos.WeAreABuyAgainKiosk)
                    StartWaitingForQuitOrBuy();
                else if (m_pos.WeAreAHybridKiosk || m_pos.WeAreASimplePOSKiosk)
                    StartWaitingForItems();

                return;
            }

            m_picUseCardOrScanReceipt.Image = null;

            m_idleTimerForVideo.Stop();

            if (m_buttonsDisabledForNoPlayer.Count > 0)
            {
                foreach (ImageButton i in m_buttonsDisabledForNoPlayer)
                    i.Enabled = true;

                m_buttonsDisabledForNoPlayer.Clear();
            }

            if (m_kioskClosed)
            {
                if (m_panelVideo.Visible && m_state != KioskState.Closed) //attract video playing, stop it
                    StopVideo();

                if (!m_panelVideo.Visible) //need to show the Closed video
                    StartClosedVideo();

                m_state = KioskState.Closed;
                m_parent.CheckForMessages();
                return;
            }

            StopVideo();

            m_couponButtonUsed = false;
            m_lblAutoCoupons.Visible = false;
            m_lblHybridAutoCoupons.Visible = false;

            if (m_pos.WeAreANonAdvancedPOSKiosk)
            {
                if (m_pos.WeHaveAStarPrinter)
                {
                    StartIdleState();
                    Application.DoEvents();
                    m_pos.CheckStarPrinterStatus();
                }
                 
                StartWaitingForPlayerCard();
            }
            else
            {
                throw new Exception("Unknown Kiosk type.  Can't start processing.");
            }
        }

        /// <summary>
        /// Close the kiosk to users.
        /// </summary>
        public void CloseKiosk()
        {
            if (m_pos.Settings.PaymentProcessingEnabled)
            {
                var paymentDevice = EliteCreditCardFactory.Instance;

                if(paymentDevice != null)
                    paymentDevice.SetLaneOpenDisplay(false);
            }

            if (m_panelKioskMenuButtons.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(delegate()
                {
                    m_kioskClosed = true;

                    if (m_pos.WeAreAnAdvancedPOSKiosk)
                        StartClosedVideo();
                    else
                        StartOver();
                }));
            }
            else
            {
                m_kioskClosed = true;

                if (m_pos.WeAreAnAdvancedPOSKiosk)
                    StartClosedVideo();
                else
                    StartOver();
            }
        }

        /// <summary>
        /// Open the kiosk to users for sales.
        /// </summary>
        public void OpenKiosk()
        {
            if (m_pos.Settings.PaymentProcessingEnabled && m_pos.WeAreANonB3Kiosk)
            {
                var paymentDevice = EliteCreditCardFactory.Instance;

                if (paymentDevice != null)
                    paymentDevice.SetLaneOpenDisplay(true);
            }

            if (m_panelKioskMenuButtons.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(delegate()
                {
                    lock (m_videoLock)
                        m_lastVideoIndex = 0;

                    m_kioskClosed = false;
                    StartOver(true);
                }));
            }
            else
            {
                lock (m_videoLock)
                    m_lastVideoIndex = 0;

                m_kioskClosed = false;
                StartOver(true);
            }
        }

        /// <summary>
        /// Enter an idle state with the screen showing "Working...".
        /// </summary>
        public void StartIdleState()
        {
            m_testModeTimer.Stop();
            m_idleTimerForVideo.Stop();
            m_sellingForm.NotIdle();

            m_picUseCardOrScanReceipt.Image = null;
            m_btnNoPlayerCard.Visible = false;
            m_btnBuy.Visible = false;
            m_btnQuit.Visible = false;
            m_lblTotalLabel.Visible = false;
            m_lblTotal.Visible = false;
            m_panelReceipt.Visible = false;
            m_panelB3OrBingo.Visible = false;
            StopVideo();
            m_panelUseCardScanReceiptOrScanPaper.Visible = false;
            m_picLogo.Visible = true;
            m_picCBBPrintingLogo.Visible = false;

            //if (m_displayMode is WideDisplayMode)
            //    BackgroundImage = Resources.KioskBackWithLogoWide;
            //else
            //    BackgroundImage = Resources.KioskBackWithLogo;

            m_lblWorking.Visible = true;

            m_MSR.EndReading();

            Focus();
            m_state = KioskState.Idle;
            Application.DoEvents();
        }

        /// <summary>
        /// Changes state to wait for a player card to be swiped.
        /// Plays attraction videos while waiting for a card or the 
        /// user to touch the screen.
        /// </summary>
        public void StartAttractVideo()
        {
            m_testModeTimer.Stop();
            m_picUseCardOrScanReceipt.Image = null;
            m_idleTimerForVideo.Stop();
            m_btnNoPlayerCard.Visible = false;
            m_lblWorking.Visible = false;
            m_btnBuy.Visible = false;
            m_btnQuit.Visible = false;
            m_lblTotalLabel.Visible = false;
            m_lblTotal.Visible = false;
            m_panelReceipt.Visible = false;
            m_panelB3OrBingo.Visible = false;
            m_panelUseCardScanReceiptOrScanPaper.Visible = false;
            //BackgroundImage = null;
            m_MSR.BeginReading();
            Focus();
            m_state = KioskState.GetPlayerCard;

            m_lblVideoPrompt.Text = m_pos.Settings.KioskAttractText;
            m_lblVideoPrompt.BackColor = Color.Orange;
            m_lblVideoPrompt.ForeColor = Color.Blue;

            //prepare the attract videos
            string path = AttractPath();

            if (path != "")
            {
                m_videoList = Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly);

                if (m_videoList.Length != 0) //put them in order
                {
                    List<string> videos = m_videoList.ToList();
                    videos.Sort();
                    m_videoList = videos.ToArray();
                }
            }
            else
            {
                m_videoList = new string[0];
            }

            lock (m_videoLock)
            {
                try
                {
                    if (m_videoList.Length == 0) //no videos
                        throw new Exception("No videos");

                    if (m_videoList.Length - 1 < m_lastVideoIndex)
                        m_lastVideoIndex = 0;

                    bool videoLoaded = false;

                    do
                    {
                        videoLoaded = PrepVideo(m_videoList[m_lastVideoIndex], true, m_videoList.Length == 1);

                        if (!videoLoaded) //video didn't load, try the next one
                        {
                            m_lastVideoIndex++;

                            if (m_videoList.Length - 1 < m_lastVideoIndex) //no good videos found
                                throw new Exception("No videos");
                        }
                    } while (!videoLoaded);

                    try
                    {
                        m_axWindowsMediaPlayer.Ctlcontrols.play();
                        m_lblVideoPrompt.Visible = true;
                        m_panelVideo.Show();
                        Focus();
                    }
                    catch (Exception)
                    {
                        m_lblVideoPrompt.Visible = false;
                        m_panelVideo.Hide();
                        m_kioskVideoEnabled = false;
                        StartWaitingForPlayerCard();
                    }
                }
                catch (Exception)
                {
                    m_kioskVideoEnabled = false;
                    StartWaitingForPlayerCard();
                }
            }

            m_burnInPreventionTimer.Start();
        }

        #endregion

        ///// <summary>
        ///// Build a new menu for the kiosk (probably a session change).
        ///// </summary>
        //public void UpdateMenu()
        //{
        //    StartIdleState();
        //    m_weHavePaperButtons = false;
        //    m_panelKioskMenuButtons.Controls.Clear();
        //    CreateMenuButtons();
        //}

        /// <summary>
        /// Remove sale items not available on the kiosk.
        /// </summary>
        /// <param name="msg">GetRepeatSaleInfoMessage for the sale to filter.</param>
        public void FilterRepeatSale(List<RepeatSaleInfo> saleInfo)
        {
            //we can only sell for the current session
            saleInfo.RemoveAll(i => i.session != 0 && i.session != m_pos.CurrentSession.SessionNumber);

            //no discounts
            saleInfo.RemoveAll(i => i.discountID != 0 || i.packageID == 0);

            //get rid of packages we don't have buttons for
            saleInfo.RemoveAll(i => m_pos.CurrentMenu.GetPackageButton(i.packageID) == null || m_pos.CurrentMenu.GetPackageButton(i.packageID).Package == null);

            //remove paper if needed
            if (!m_pos.Settings.AllowPaperOnKiosks)
                saleInfo.RemoveAll(i => m_pos.CurrentMenu.GetPackageButton(i.packageID) != null && m_pos.CurrentMenu.GetPackageButton(i.packageID).Package != null && m_pos.CurrentMenu.GetPackageButton(i.packageID).Package.HasBarcodedPaper);

            if (m_pos.CurrentSale.Player == null) //no player card, filter packages requiring player or points
                saleInfo.RemoveAll(i => m_pos.CurrentMenu.GetPackageButton(i.packageID) != null && m_pos.CurrentMenu.GetPackageButton(i.packageID).Package != null && (m_pos.CurrentMenu.GetPackageButton(i.packageID).Package.PointsToRedeem != 0 || m_pos.CurrentMenu.GetPackageButton(i.packageID).IsPlayerRequired));

            if (m_pos.Settings.KiosksCanOnlySellFromTheirButtons) //filter out anything that isn't one of our buttons
            {
                List<RepeatSaleInfo> newList = new List<RepeatSaleInfo>();

                foreach (RepeatSaleInfo r in saleInfo)
                {
                    if (r.packageID != 0)
                    {
                        foreach (ImageButton i in m_panelKioskMenuButtons.Controls)
                        {
                            if (i.Tag != null && ((PackageButton)i.Tag).Package.Id == r.packageID && i.Enabled)
                            {
                                newList.Add(r);
                                break;
                            }
                        }
                    }
                }

                saleInfo.Clear();
                saleInfo.AddRange(newList);
            }
        }

        /// <summary>
        /// Updates the total at the bottom of the kiosk screen and
        /// controls the visibility of the BUY button.
        /// </summary>
        /// <param name="text"></param>
        public void UpdateKioskTotal(string text)
        {
            m_lblTotal.Text = text;

            for (int x = 0; x < text.Length; x++)
            {
                if (text[x] >= '0' && text[x] <= '9')
                {
                    decimal total = 0;

                    decimal.TryParse(text.Substring(x), out total);

                    m_btnBuy.Visible = m_pos.CurrentSale != null && m_pos.CurrentSale.GetItems().Count() != 0;

                    break;
                }
            }
        }

        /// <summary>
        /// Searches the kiosk menu buttons for the sister to the POS menu button supplied
        /// and updates the kiosk button's text to match the POS menu button.
        /// </summary>
        /// <param name="imageButton"></param>
        public void UpdateButtonText(ImageButton imageButton)
        {
            System.Windows.Forms.Control.ControlCollection controls = m_panelKioskMenuButtons.Controls;

            if (controls != null)
            {
                foreach (ImageButton ourImageButton in controls)
                {
                    if (ourImageButton.Tag == imageButton.Tag)
                    {
                        ourImageButton.Text = imageButton.Text;
                        break;
                    }
                }
            }
        }

        #endregion

        #region Methods

        #region Video playback support
        private void StopVideo()
        {
            lock (m_videoLock)
            {
                m_burnInPreventionTimer.Stop();

                if (m_panelVideo.Visible) //video playing, stop it
                {
                    try
                    {
                        if (m_axWindowsMediaPlayer.playState != WMPLib.WMPPlayState.wmppsStopped)
                            m_axWindowsMediaPlayer.Ctlcontrols.stop();

                        m_panelVideo.Visible = false;
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        /// <summary>
        /// Finds the path to the video files.  Searches all available ready drives 
        /// from Z to A returning the first drive with our folder.
        /// </summary>
        /// <returns>Path ending in \ or "".</returns>
        private string AttractPath(bool closeVideo = false)
        {
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
                string testPath = di.Name + @"GameTech\Attract\";

                if (closeVideo)
                    testPath += @"Kiosk Closed Videos\";
                else
                    testPath += @"Kiosk Videos\";


                if (Directory.Exists(testPath))
                    return testPath;
            }

            return "";
        }

        /// <summary>
        /// Changes video on a thread outside of the video's processing thread.
        /// </summary>
        /// <param name="inThread">A bool object for if the method is running in a thread or starting the thread
        /// or a Tuple:lt:bool, bool:gt: where the first item is the above bool and the second
        /// item is whether the index should be left alone.</param>
        private void PlayNextVideo(object inThread)
        {
            if ((inThread is bool && !(bool)inThread) || inThread is Tuple<bool, bool> && ((Tuple<bool, bool>)inThread).Item1 == false) //prep a thread to restart the video
            {
                m_videoChangeThread = new Thread(PlayNextVideo);
                m_videoChangeThread.Start(new Tuple<bool, bool>(true, inThread is Tuple<bool, bool> && ((Tuple<bool, bool>)inThread).Item2 == true));
            }
            else //running in thread
            {
                bool adjustIndex = !((Tuple<bool, bool>)inThread).Item2;

                this.Invoke(new MethodInvoker(delegate()
                {
                    //wait for the video player to free-up, get rid of it, and start a new one
                    lock (m_videoLock)
                    {
                        if (!m_axWindowsMediaPlayer.settings.getMode("loop")) //not set to loop, play next attract video
                        {
                            if (adjustIndex)
                            {
                                m_lastVideoIndex++;

                                if (m_videoList.Length - 1 < m_lastVideoIndex) //end of list
                                    m_lastVideoIndex = 0;
                            }

                            bool videoLoaded = false;

                            do
                            {
                                videoLoaded = PrepVideo(m_videoList[m_lastVideoIndex], true, m_videoList.Length == 1);

                                if (!videoLoaded) //video didn't load, try the next one
                                {
                                    m_lastVideoIndex++;

                                    if (m_videoList.Length - 1 < m_lastVideoIndex) //no good videos found
                                        m_lastVideoIndex = 0;
                                }
                            } while (!videoLoaded);

                            try
                            {
                                m_axWindowsMediaPlayer.Ctlcontrols.play();
                            }
                            catch (Exception)
                            {
                            }

                            m_panelVideo2.Visible = true;
                            Focus();
                        }
                    }
                }));
            }
        }

        private bool PrepVideo(string file, bool withBanner, bool loop)
        {
            bool loaded = true;

            try
            {
                m_axWindowsMediaPlayer.Ctlcontrols.stop();
                m_axWindowsMediaPlayer.currentPlaylist.clear();

                if (!IsVideo(file))
                    throw new Exception("Not a video file.");

                m_axWindowsMediaPlayer.URL = file;

                lock (m_videoBurnInLock)
                {
                    Size maxVideo = m_panelVideo.Size;

                    if (withBanner)
                        maxVideo.Height -= m_lblVideoPrompt.Size.Height;

                    m_panelVideo2.Size = maxVideo; //set panel size for video
                    m_panelVideo2.BackgroundImage = null;
                    m_panelVideo2.Location = new Point(0, withBanner ? (m_lblVideoPrompt.Location.Y == 0 ? m_lblVideoPrompt.Size.Height : 0) : 0);

                    m_axWindowsMediaPlayer.Size = maxVideo;
                    m_axWindowsMediaPlayer.Location = new Point(0, 0);

                    m_axWindowsMediaPlayer.settings.setMode("loop", loop);
                    m_axWindowsMediaPlayer.settings.setMode("showFrame", loop);
                }
            }
            catch (Exception)
            {
                loaded = false;
            }

            m_axWindowsMediaPlayer.Visible = true;

            return loaded;
        }

        private void m_axWindowsMediaPlayer_PlayStateChange(object sender, AxWMPLib._WMPOCXEvents_PlayStateChangeEvent e)
        {
            if (m_axWindowsMediaPlayer.playState == WMPLib.WMPPlayState.wmppsMediaEnded)
            {
                lock (m_videoLock)
                {
                    if (!m_axWindowsMediaPlayer.settings.getMode("loop"))
                        PlayNextVideo(false);
                }
            }
        }

        private void m_idleTimerForVideo_Tick(object sender, EventArgs e)
        {
            if (m_parent.GuardianHasUsSuspended)
                m_idleTimerForVideo.Start();
            else
                StartAttractVideo();
        }

        private void m_burnInPreventionTimer_Tick(object sender, EventArgs e)
        {
            lock (m_videoBurnInLock)
            {
                if (m_lblVideoPrompt.Location.Y == 0) //text over video
                {
                    m_lblVideoPrompt.Location = new Point(0, m_panelVideo2.Size.Height);
                    m_panelVideo2.Location = new Point(0, 0);
                }
                else //text under video
                {
                    m_lblVideoPrompt.Location = new Point(0, 0);
                    m_panelVideo2.Location = new Point(0, m_lblVideoPrompt.Size.Height);
                }
            }
        }

        private void m_lblVideoPrompt_Click(object sender, EventArgs e)
        {
            if (!m_kioskClosed)
            {
                if (m_pos.WeAreAnAdvancedPOSKiosk)
                {
                    Close();
                }
                else
                {
                    m_idleTimerForVideo.Start();
                    UserActivityDetected(sender, e);
                }
            }
        }

        private bool IsVideo(string fileName)
        {
            bool reply = false;

            try
            {
                FileInfo fileInfo = new FileInfo(fileName);
                RegistryKey regKey = Registry.ClassesRoot.OpenSubKey(fileInfo.Extension.ToLower());

                if (regKey != null)
                {
                    object contentType = regKey.GetValue("Content Type");

                    if (contentType != null)
                        reply = contentType.ToString().StartsWith("video/", StringComparison.CurrentCultureIgnoreCase);
                }
            }
            catch (Exception)
            {
            }

            return reply;
        }

        private void m_axWindowsMediaPlayer_MouseDownEvent(object sender, AxWMPLib._WMPOCXEvents_MouseDownEvent e)
        {
            m_swipeStart = MousePosition;
        }

        private void m_axWindowsMediaPlayer_MouseUpEvent(object sender, AxWMPLib._WMPOCXEvents_MouseUpEvent e)
        {
            m_swipeEnd = MousePosition;

            int gap = m_swipeStart.X - m_swipeEnd.X;

            if (Math.Abs(gap) > 20) //we have a swipe
            {
                lock (m_videoLock)
                {
                    do
                    {
                        m_lastVideoIndex += Math.Sign(gap);

                        if (m_lastVideoIndex < 0)
                            m_lastVideoIndex = m_videoList.Length - 1;
                        else if (m_lastVideoIndex > m_videoList.Length - 1)
                            m_lastVideoIndex = 0;

                    } while (!PrepVideo(m_videoList[m_lastVideoIndex], true, m_videoList.Length == 1));

                    PlayNextVideo(new Tuple<bool, bool>(false, true));
                }
            }
            else
            {
                if (m_state == KioskState.GetPlayerCard)
                    m_lblVideoPrompt_Click(sender, new EventArgs());
            }
        }

        #endregion

        #region State change - Internal

        /// <summary>
        /// Changes state to wait for a receipt to be scanned.
        /// Used for Buy-Again kiosk and receipt scan "help" button.
        /// </summary>
        private void StartWaitingForReceipt()
        {
            m_testModeTimer.Stop();
            m_idleTimerForVideo.Stop();
            m_sellingForm.NotIdle();

            m_btnNoPlayerCard.Visible = false;
            m_lblWorking.Visible = false;
            SetBuyToBuyAgain(false);
            m_btnBuy.Visible = false;
            SetQuitToClose(false);
            SetBuyToBuyAgain(false);
            m_btnQuit.Visible = true;
            m_lblTotalLabel.Visible = false;
            m_lblTotal.Visible = false;
            m_panelReceipt.Visible = false;
            m_panelB3OrBingo.Visible = false;

            if (m_pos.WeAreABuyAgainKiosk)
            {
                m_lblUseCardOrScanReceipt.Text = "Scan a recent receipt to re-purchase eligible items.";
                SetQuitToClose(false);
            }
            else
            {
                m_lblUseCardOrScanReceipt.Text = "Scan a recent receipt and all the kiosk eligible items from the receipt will be added to your order.\r\n\r\nYou may also scan receipts at any time on the previous screen.";
                SetQuitToClose();
            }

            if (m_pos.Settings.AllowUseLastPurchaseButtonOnKiosk && m_pos.CurrentSale != null && m_pos.CurrentSale.Player != null)
            {
                SetBuyToBuyAgain();
                m_btnBuy.Visible = true;
            }

            m_picUseCardOrScanReceipt.Image = Resources.AnimatedReceiptScan_large_;
            m_picLogo.Visible = true;
            m_picCBBPrintingLogo.Visible = false;

            //if (m_displayMode is WideDisplayMode)
            //    BackgroundImage = Resources.KioskBackWithLogoWide;
            //else
            //    BackgroundImage = Resources.KioskBackWithLogo;

            m_panelUseCardScanReceiptOrScanPaper.Visible = true;

            m_MSR.EndReading();
            Focus();
            m_state = KioskState.GetReceiptFromBarcode;
        }

        /// <summary>
        /// Changes state to wait for barcoded paper to be scanned.
        /// Used for barcoded paper "help" button.
        /// </summary>
        private void StartWaitingForPaperScan()
        {
            m_testModeTimer.Stop();
            m_idleTimerForVideo.Stop();
            m_sellingForm.NotIdle();

            m_btnNoPlayerCard.Visible = false;
            m_lblWorking.Visible = false;
            m_btnBuy.Visible = false;
            SetQuitToClose(false);
            SetBuyToBuyAgain(false);
            m_btnQuit.Visible = true;
            m_lblTotalLabel.Visible = false;
            m_lblTotal.Visible = false;
            m_panelReceipt.Visible = false;
            m_panelB3OrBingo.Visible = false;
            m_lblUseCardOrScanReceipt.Text = "To purchase a paper pack, scan the barcode on the pack and the item will be added to your order.\r\n\r\nYou may also scan paper packs at any time on the previous screen.";
            m_picUseCardOrScanReceipt.Image = Resources.AnimatedScanPaper_large_;
            SetQuitToClose();
            m_picLogo.Visible = true;
            m_picCBBPrintingLogo.Visible = false;

            //if (m_displayMode is WideDisplayMode)
            //    BackgroundImage = Resources.KioskBackWithLogoWide;
            //else
            //    BackgroundImage = Resources.KioskBackWithLogo;

            m_panelUseCardScanReceiptOrScanPaper.Visible = true;

            m_MSR.EndReading();
            Focus();
            m_state = KioskState.GetPaperFromBarcode;
        }

        /// <summary>
        /// Changes state to wait for a player card to be swiped.
        /// Main entry point to a sale along with StartAttractVideo().
        /// </summary>
        private void StartWaitingForPlayerCard(bool allowBuyingWithoutACard = true)
        {
            m_idleTimerForVideo.Stop();
            m_sellingForm.NotIdle();

            m_btnNoPlayerCard.Visible = false;
            m_lblWorking.Visible = false;
            m_btnBuy.Visible = false;
            m_btnQuit.Visible = false;
            m_lblTotalLabel.Visible = false;
            m_lblTotal.Visible = false;
            m_panelReceipt.Visible = false;
            m_panelB3OrBingo.Visible = false;

            if (m_pos.WeAreAB3Kiosk || m_pos.WeCanSellForB3AtKiosk)
            {
                if(m_pos.WeAreAB3Kiosk || !m_parent.HaveBingoMenu)
                    m_lblUseCardOrScanReceipt.Text = "Use your player card\r\nto start buying B3!";
                else
                    m_lblUseCardOrScanReceipt.Text = "Use your player card\r\nto get started!";
            }
            else
            {
                m_lblUseCardOrScanReceipt.Text = "Use your player card\r\nto start buying\r\nBINGO packs!";
            }

            m_picUseCardOrScanReceipt.Image = Resources.AnimatedPlayerCard_large_;

            if (m_pos.Settings.AllowBuyingAtSimpleKioskWithoutPlayerCard && allowBuyingWithoutACard)
                m_btnNoPlayerCard.Visible = true;

            m_picLogo.Visible = true;

            if (m_weCanPrintCBBPlayItSheets)
                m_picCBBPrintingLogo.Visible = true;

            m_MSR.BeginReading();

            if (m_kioskVideoEnabled)
                m_idleTimerForVideo.Start();

            if (m_parent.Settings.KioskInTestMode)
            {
                m_olblTestMode.Visible = true;
                m_testModeTimer.Start();
            }

            m_panelUseCardScanReceiptOrScanPaper.Visible = true;
            m_state = KioskState.GetPlayerCard;
            m_parent.CheckForMessages(); //Process any waiting messages from the server or Guardian now.
            Focus();
        }

        /// <summary>
        /// Changes state to wait for a selection of B3 or Bingo.
        /// </summary>
        private void StartWaitingForBingoOrB3Selection()
        {
            m_testModeTimer.Stop();

            if (!m_parent.HaveBingoMenu) //no bingo menu so no choice, do B3
            {
                m_btnB3_Click(null, new EventArgs());
                return;
            }

            m_idleTimerForVideo.Stop();
            m_sellingForm.NotIdle();

            m_btnNoPlayerCard.Visible = false;
            m_lblWorking.Visible = false;
            m_btnBuy.Visible = false;
            m_btnQuit.Visible = true;
            m_lblTotalLabel.Visible = false;
            m_lblTotal.Visible = false;
            m_panelReceipt.Visible = false;
            m_panelB3OrBingo.Visible = true;
            m_picLogo.Visible = false;
            m_picCBBPrintingLogo.Visible = false;

            if (m_pos.CurrentSale != null && m_pos.CurrentSale.Player != null && !string.IsNullOrWhiteSpace(m_pos.CurrentSale.Player.FirstName))
            {
                m_lblB3OrBingoGreeting.Text = "Welcome back " + m_pos.CurrentSale.Player.FirstName;
                m_lblB3OrBingoGreeting.Visible = true;
            }
            else
            {
                m_lblB3OrBingoGreeting.Visible = false;
            }

            m_MSR.EndReading();

            if (m_kioskVideoEnabled)
                m_idleTimerForVideo.Start();

            m_panelUseCardScanReceiptOrScanPaper.Visible = false;

            Focus();
            m_state = KioskState.GetWhatWeAreSelling;
        }

        /// <summary>
        /// Changes state to close the kiosk for sales and plays
        /// the closed kiosk videos until the kiosk is opened.
        /// </summary>
        private void StartClosedVideo()
        {
            m_testModeTimer.Stop();
            m_picUseCardOrScanReceipt.Image = null;
            m_idleTimerForVideo.Stop();
            m_btnNoPlayerCard.Visible = false;
            m_lblWorking.Visible = false;
            m_btnBuy.Visible = false;
            m_btnQuit.Visible = false;
            m_lblTotalLabel.Visible = false;
            m_lblTotal.Visible = false;
            m_panelReceipt.Visible = false;
            m_panelB3OrBingo.Visible = false;
            m_panelUseCardScanReceiptOrScanPaper.Visible = false;
            m_lblVideoPrompt.Visible = true;
            //BackgroundImage = null;
            m_MSR.EndReading();

            m_lblVideoPrompt.Text = m_pos.Settings.KioskClosedText;
            m_lblVideoPrompt.BackColor = Color.Black;
            m_lblVideoPrompt.ForeColor = Color.Yellow;

            //prepare the closed videos
            string path = AttractPath(true);

            if (path != "")
            {
                //                m_videoList = Directory.GetFiles(path, "*.wmv", SearchOption.TopDirectoryOnly);
                m_videoList = Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly);

                if (m_videoList.Length != 0) //put them in order
                {
                    List<string> videos = m_videoList.ToList();
                    videos.Sort();
                    m_videoList = videos.ToArray();
                }
            }
            else
            {
                m_videoList = new string[0];
            }

            lock (m_videoLock)
            {
                m_lastVideoIndex = 0;

                if (m_videoList.Length == 0) //no video, use internal picture
                {
                    m_axWindowsMediaPlayer.Visible = false;
                    m_lblVideoPrompt.Visible = true;
                    m_panelVideo2.BackgroundImage = Resources.SorryClosed;
                    m_panelVideo2.BackgroundImageLayout = ImageLayout.Stretch;
                    m_state = KioskState.Closed;
                    m_panelVideo.Show();
                    Focus();
                }
                else
                {
                    bool showPic = true;

                    try
                    {
                        if (PrepVideo(m_videoList[0], true, m_videoList.Length == 1))
                        {
                            m_axWindowsMediaPlayer.Ctlcontrols.play();
                            showPic = false;
                        }
                    }
                    catch (Exception)
                    {
                        showPic = true;
                    }
                    finally
                    {
                        if (showPic) //use internal picture
                        {
                            m_axWindowsMediaPlayer.Visible = false;
                            m_lblVideoPrompt.Visible = true;
                            m_panelVideo2.BackgroundImage = Resources.SorryClosed;
                            m_panelVideo2.BackgroundImageLayout = ImageLayout.Stretch;
                        }

                        m_state = KioskState.Closed;
                        m_panelVideo.Show();
                        Focus();
                    }
                }
            }

            m_burnInPreventionTimer.Start();
        }

        /// <summary>
        /// Changes state to get funding for B3 games.
        /// </summary>
        private void StartWaitingForB3()
        {
            m_testModeTimer.Stop();
            StartIdleState();
            m_state = KioskState.GetB3Funding;
            B3KioskForm B3 = new B3KioskForm(m_pos);

            if (!B3.IsDisposed)
            {
                DialogResult result = B3.ShowDialog(this);

                if (result == DialogResult.Cancel && !m_pos.WeAreAB3Kiosk)
                {
                    if (m_parent.HaveBingoMenu)
                    {
                        StartWaitingForBingoOrB3Selection();
                    }
                    else
                    {
                        if (m_pos.CurrentSale != null)
                            m_pos.ClearSale();

                        StartOver();
                    }
                }
                else
                {
                    if (m_pos.CurrentSale != null)
                        m_pos.ClearSale();

                    StartOver();
                }
            }
            else //couldn't do it
            {
                if (m_pos.CurrentSale != null)
                    m_pos.ClearSale();

                StartOver();
            }
        }

        /// <summary>
        /// Changes state to wait for items to be selected via any available
        /// method. Used for simple and hybrid kiosks.
        /// </summary>
        private void StartWaitingForItems()
        {
            m_testModeTimer.Stop();
            m_idleTimerForVideo.Stop();
            m_sellingForm.NotIdle();

            m_panelB3OrBingo.Visible = false;
            m_picUseCardOrScanReceipt.Image = null;
            m_btnNoPlayerCard.Visible = false;
            m_lblWorking.Visible = false;
            SetQuitToClose(false);
            SetBuyToBuyAgain(false);

            if (m_pos.WeAreAHybridKiosk)
            {
                m_btnBuy.Visible = m_pos.CurrentSale != null && m_pos.CurrentSale.GetItems().Count() != 0;
                m_btnQuit.Visible = true;
                m_btnCoupons.Visible = m_pos.Settings.AllowCouponButtonOnHybridKiosk;
                m_btnCoupons.Enabled = m_pos.CurrentCouponForm != null && m_pos.CurrentCouponForm.LoadPlayerComp(true);
                m_lblTotalLabel.Visible = true;
                m_lblTotal.Visible = true;
                m_panelUseCardScanReceiptOrScanPaper.Visible = false;

                if (m_pos.CurrentSale.Player != null)
                    m_lblWelcomeBack.Text = "Welcome back" + (m_pos.CurrentSale.Player.FirstName != "" ? " " + m_pos.CurrentSale.Player.FirstName : "") + "!";
                else
                    m_lblWelcomeBack.Text = "Welcome!";

                m_btnHelp.Visible = true;
                m_btnPaperHelp.Visible = m_pos.Settings.AllowPaperOnKiosks;
                m_btnReceiptHelp.Visible = true;
                m_panelKioskMenuButtons.Visible = true;
                m_panelReceipt.Visible = true;
                m_sellingForm.UpdateSaleInfo();
                FillInReceipt();
                SetDeviceButton();
            }
            else //simple POS kiosk
            {
                m_btnBuy.Visible = m_pos.CurrentSale != null && m_pos.CurrentSale.GetItems().Count() != 0;
                m_btnQuit.Visible = true;
                m_lblTotalLabel.Visible = true;
                m_lblTotal.Visible = true;
                m_btnCoupons.Visible = false;
                m_btnCoupons.Enabled = false;
                m_panelUseCardScanReceiptOrScanPaper.Visible = false;

                if (m_pos.CurrentSale.Player != null)
                    m_lblWelcomeBack.Text = "Welcome back" + (m_pos.CurrentSale.Player.FirstName != "" ? " " + m_pos.CurrentSale.Player.FirstName : "") + "!";
                else
                    m_lblWelcomeBack.Text = "Welcome!";

                m_btnHelp.Visible = true;
                m_btnPaperHelp.Visible = m_weHavePaperButtons;
                m_btnReceiptHelp.Visible = true;
                m_panelKioskMenuButtons.Visible = true;
                m_panelReceipt.Visible = true;
                m_sellingForm.UpdateSaleInfo();
            }

            m_picLogo.Visible = false;
            m_picCBBPrintingLogo.Visible = false;

            //if (m_displayMode is WideDisplayMode)
            //    BackgroundImage = Resources.KioskBackWide;
            //else
            //    BackgroundImage = Resources.KioskBack;

            m_MSR.EndReading();
            Focus();
            m_state = KioskState.GetItems;
        }

        /// <summary>
        /// Changes state to wait for the user to press QUIT or BUY
        /// button. Used for Buy-Again kiosk.
        /// </summary>
        private void StartWaitingForQuitOrBuy()
        {
            m_testModeTimer.Stop();
            m_idleTimerForVideo.Stop();
            m_sellingForm.NotIdle();

            m_panelB3OrBingo.Visible = false;
            m_picUseCardOrScanReceipt.Image = null;
            m_btnNoPlayerCard.Visible = false;
            m_lblWorking.Visible = false;
            m_btnBuy.Visible = true;
            SetQuitToClose(false);
            SetBuyToBuyAgain(false);
            m_btnQuit.Visible = true;
            m_btnCoupons.Visible = false;
            m_lblTotalLabel.Visible = true;
            m_lblTotal.Visible = true;
            m_panelUseCardScanReceiptOrScanPaper.Visible = false;
            m_panelKioskMenuButtons.Visible = false;
            m_btnHelp.Visible = false;
            m_btnPaperHelp.Visible = false;
            m_btnReceiptHelp.Visible = false;

            if (m_pos.CurrentSale.Player != null)
                m_lblWelcomeBack.Text = "Welcome back" + (m_pos.CurrentSale.Player.FirstName != "" ? " " + m_pos.CurrentSale.Player.FirstName : "") + "!";
            else
                m_lblWelcomeBack.Text = "Welcome!";

            m_panelReceipt.Visible = true;
            m_picLogo.Visible = true;
            m_picCBBPrintingLogo.Visible = false;

            //if (m_displayMode is WideDisplayMode)
            //    BackgroundImage = Resources.KioskBackWithLogoWide;
            //else
            //    BackgroundImage = Resources.KioskBackWithLogo;

            m_MSR.EndReading();
            Focus();
            m_state = KioskState.GetQuitOrBuyForReceipt;
        }

        #endregion

        #region Initialization

        private void HandleKeyAudio(GTI.Controls.EliteGradientPanel panel)
        {
            foreach (System.Windows.Forms.Control ctrl in panel.Controls)
            {
                if (ctrl as GTI.Controls.EliteGradientPanel != null)
                {
                    HandleKeyAudio((GTI.Controls.EliteGradientPanel)ctrl);
                }
                else if (ctrl as ImageButton != null)
                {
                    ((ImageButton)ctrl).UseClickSound = m_parent.Settings.UseKeyClickSoundsOnKiosk;
                }
            }
        }

        private void ApplyKioskDisplayMode()
        {
            Size = m_displayMode.FormSize;
            m_panelMain.Size = m_displayMode.BaseFormSize;
            m_panelMain.Location = new Point(m_displayMode.OffsetForFullScreenX, m_displayMode.OffsetForFullScreenY);

            if (m_displayMode is WideDisplayMode)
            {
                //things have gotten wider
                m_btnBuy.Location = new Point(792 + m_displayMode.WidthIncreaseFromNormal, 689);
                m_lblTotalLabel.Location = new Point(282 + m_displayMode.EdgeAdjustmentForNormalToWideX, 689);
                m_lblTotal.Location = new Point(457 + m_displayMode.EdgeAdjustmentForNormalToWideX, 689);
                m_lblWorking.Location = new Point(222 + m_displayMode.EdgeAdjustmentForNormalToWideX, 75);
                m_panelUseCardScanReceiptOrScanPaper.Size = new Size(m_panelUseCardScanReceiptOrScanPaper.Size.Width + m_displayMode.WidthIncreaseFromNormal, m_panelUseCardScanReceiptOrScanPaper.Size.Height);
                m_panelReceipt.Size = new Size(m_panelReceipt.Size.Width + m_displayMode.WidthIncreaseFromNormal, m_panelReceipt.Size.Height);
            }
        }

        private void CreateMenuButtons()
        {
            if (m_pos.WeAreAHybridKiosk) //we show buttons for only the first page of the menu
            {
                //get all of the visible buttons
                int buttonRows = m_displayMode.MenuButtonsPerColumn;
                int buttonSpacing = 4;
                List<MenuButton> buttons = m_pos.CurrentMenu.HybridPOSKioskButtons; //sorted by location
                int buttonColumns = m_displayMode.MenuButtonsPerPage / buttonRows;
                int buttonWidth = (m_panelKioskMenuButtons.Width - ((buttonColumns - 1) * buttonSpacing)) / buttonColumns;
                int buttonHeight = (m_panelKioskMenuButtons.Height - ((buttonRows - 1) * buttonSpacing)) / buttonRows;

                foreach (MenuButton menuButton in buttons)
                {
                    PackageButton pb = menuButton as PackageButton;

                    ImageButton button = new ImageButton();

                    Font font = m_displayMode.MenuButtonFont;

                    button.ShowFocus = false;
                    button.Padding = new Padding(buttonSpacing);

                    button.Size = new Size(buttonWidth, buttonHeight);
                    button.Font = font;
                    button.AutoBlackOrWhiteText = true;
                    button.UseClickSound = m_parent.Settings.UseKeyClickSoundsOnKiosk;

                    int ourColumn = (int)(menuButton.Position / buttonRows) + 1;
                    int ourRow = (int)(menuButton.Position % buttonRows) + 1;

                    button.Location = new Point((ourColumn - 1) * (buttonWidth + buttonSpacing), (ourRow - 1) * (buttonHeight + buttonSpacing));

                    button.Text = menuButton.Text;

                    m_sellingForm.SetButtonImagesFromMenuButton(button, menuButton);

                    button.Tag = menuButton;
                    button.Click += new EventHandler(button_Click);

                    button.ButtonHeld += new EventHandler(button_ButtonHeld);

                    m_panelKioskMenuButtons.Controls.Add(button);

                    if (pb != null && !m_pos.Settings.AllowPaperOnKiosks && pb.Package.HasBarcodedPaper)
                        button.Enabled = false;

                    if (m_pos.Settings.AllowPaperOnKiosks && pb.Package.HasBarcodedPaper)
                        m_weHavePaperButtons = true;
                }
            }
            else if (m_pos.WeAreASimplePOSKiosk)
            {
                //Menus are 5 rows by n columns. Multiple pages will be placed left to right with empty pages removed. 
                //Buttons will be sized to fit horizontally.

                //get all of the visible buttons
                Tuple<int, List<MenuButton>> menuInfo = m_pos.CurrentMenu.SimplePOSKioskButtons;

                if (menuInfo.Item1 > 0)
                {
                    int buttonRows = m_displayMode.MenuButtonsPerColumn;
                    int buttonSpacing = 4;
                    List<MenuButton> buttons = menuInfo.Item2; //sorted by page & location
                    int buttonColumns = menuInfo.Item1 * ((m_displayMode.MenuButtonsPerPage / m_displayMode.MenuPagesPerPOSPage) / buttonRows);
                    int buttonWidth = (m_panelKioskMenuButtons.Width - ((buttonColumns - 1) * buttonSpacing)) / buttonColumns;
                    int buttonHeight = (m_panelKioskMenuButtons.Height - ((buttonRows - 1) * buttonSpacing)) / buttonRows;
                    int currentPage = 0;
                    int lastPage = 0;

                    foreach (MenuButton menuButton in buttons)
                    {
                        PackageButton pb = menuButton as PackageButton;

                        if (menuButton.Page != lastPage)
                        {
                            lastPage = menuButton.Page;
                            currentPage++;
                        }

                        ImageButton button = new ImageButton();

                        Font font = new Font(m_displayMode.MenuButtonFont.FontFamily, 16, m_displayMode.MenuButtonFont.Style);

                        button.ShowFocus = false;
                        button.Padding = new Padding(buttonSpacing);

                        button.Size = new Size(buttonWidth, buttonHeight);
                        button.Font = font;
                        button.AutoBlackOrWhiteText = true;

                        int ourColumn = ((currentPage - 1) * ((m_displayMode.MenuButtonsPerPage / m_displayMode.MenuPagesPerPOSPage) / buttonRows)) + ((int)(menuButton.Position / buttonRows) + 1);
                        int ourRow = (int)(menuButton.Position % buttonRows) + 1;

                        button.Location = new Point((ourColumn - 1) * (buttonWidth + buttonSpacing), (ourRow - 1) * (buttonHeight + buttonSpacing));

                        button.Text = menuButton.Text;

                        m_sellingForm.SetButtonImagesFromMenuButton(button, menuButton);

                        button.Tag = menuButton;
                        button.Click += new EventHandler(button_Click);

                        button.ButtonHeld += new EventHandler(button_ButtonHeld);

                        m_panelKioskMenuButtons.Controls.Add(button);

                        if (pb != null && !m_pos.Settings.AllowPaperOnKiosks && pb.Package.HasBarcodedPaper)
                            button.Enabled = false;

                        if (pb.Package.HasBarcodedPaper)
                            m_weHavePaperButtons = true;
                    }
                }
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;  // Turn on WS_EX_COMPOSITED
                return cp;
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            e.Graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
            e.Graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;
            base.OnPaintBackground(e);
        }

        #endregion

        #region MSR and barcode scanner

        private void HandleScannerInput()
        {
            string barcode = m_barcodeBuffer.ToString();

            m_barcodeBuffer.Clear();

            if (m_kioskClosed)
                return;

            bool isReceipt = Regex.IsMatch(barcode.ToUpper(), @"^F[0-9]*TRN");
            bool isScheduledSale = Regex.IsMatch(barcode.ToUpper(), @"^F[0-9]*SCH");
            bool isPreDefinedSale = Regex.IsMatch(barcode.ToUpper(), @"^F[0-9]*PDS");

            if (m_weCanPrintCBBPlayItSheets && m_state == KioskState.GetPlayerCard && isReceipt) //print CBB play it sheets
            {
                m_state = KioskState.PrintingCBBSheets;

                //need to get the register receipt id, we have the transaction number
                int transactionNumber = 0;

                if (int.TryParse(barcode.Substring(1, barcode.Length - 4), out transactionNumber))
                {
                    //make a form that fits the CBB printing picture 
                    Form coverForm = new Form();
                    coverForm.FormBorderStyle = FormBorderStyle.None; //no caption bar
                    coverForm.StartPosition = FormStartPosition.CenterScreen;
                    coverForm.Size = m_parent.OurScreen.Bounds.Size;
                    coverForm.BackColor = Color.Black;

                    //make a picture box to hold the image and make it fill the screen
                    PictureBox pic = new PictureBox();
                    pic.Image = Resources.PrintingCBBSheets;
                    pic.Size = coverForm.Size;
                    coverForm.Controls.Add(pic); //attach the picture box to the form
                    pic.SizeMode = PictureBoxSizeMode.CenterImage;
                    pic.Show();

                    //cover the screen with this window
                    coverForm.Enabled = false;
                    coverForm.Show();
                    coverForm.TopMost = true;
                    coverForm.BringToFront();
                    Application.DoEvents();

                    GetCBBInfoFromTransactionMessage findReceipt = new GetCBBInfoFromTransactionMessage(transactionNumber);

                    try
                    {
                        findReceipt.Send();

                        bool printSheets = false;

                        if(findReceipt.ReceiptID != 0) //found the receipt, see what's in it
                        {
                            if (m_pos.Settings.CBBPlayItSheetPrintMode == CBBPlayItSheetPrintMode.All)
                            {
                                printSheets = true;
                            }
                            else //see if we can print anything
                            {
                                if (m_pos.Settings.CBBPlayItSheetPrintMode == CBBPlayItSheetPrintMode.ElectronicOnly && findReceipt.HasElectronic)
                                    printSheets = true;

                                if (m_pos.Settings.CBBPlayItSheetPrintMode == CBBPlayItSheetPrintMode.PaperOnly && findReceipt.HasPaper)
                                    printSheets = true;
                            }
                        }

                        if (printSheets)
                        {
                            if (m_pos.Settings.CBBPlayItSheetType == CBBPlayItSheetType.Card || m_pos.Settings.CBBPlayItSheetType == CBBPlayItSheetType.Line) //80 column printer
                                m_parent.PrintPlayItSheet(findReceipt.ReceiptID, m_parent.Settings.PrinterName, false);
                            else
                                m_parent.PrintPlayItSheet(findReceipt.ReceiptID, m_parent.Settings.ReceiptPrinterName, false);
                        }
                        else
                        {
                            POSMessageForm.Show(coverForm, m_parent, Resources.CBBNoCardsToPrint, POSMessageFormTypes.OK);
                        }
                    }
                    catch (Exception)
                    {
                    }

                    coverForm.Hide();
                    Application.DoEvents();
                    coverForm.Dispose();
                }

                m_state = KioskState.GetPlayerCard;
                return;
            }

            if (isReceipt || isScheduledSale || isPreDefinedSale) //receipt scan, scheduled sale scan, or pre-defined sale scan
            {
                if (m_state == KioskState.GetPaperFromBarcode) //not looking for this, ignore it
                    return;

                if (isReceipt)
                {
                    int transactionNumber = 0;

                    if (int.TryParse(barcode.Substring(1, barcode.Length - 4), out transactionNumber))
                        m_sellingForm.ProcessRepeatSaleThroughServer(0, transactionNumber);
                }
                else //scheduled sale or pre-defined sale
                {
                    m_sellingForm.BarcodeScanned(null, barcode.ToUpper());
                }

                if (m_pos.CurrentSale != null && m_pos.CurrentSale.GetItems().Length > 0) //update the sale in case we got something
                {
                    ReapplyCoupons();

                    if (m_pos.WeAreASimplePOSKiosk)
                    {
                        StartWaitingForItems();
                        return;
                    }

                    FillInReceipt();
                    SetDeviceButton();

                    if (m_pos.WeAreABuyAgainKiosk)
                        StartWaitingForQuitOrBuy();
                    else
                        StartWaitingForItems();
                }
            }
            else //not a receipt
            {
                if (m_pos.WeAreABuyAgainKiosk || m_state == KioskState.GetReceiptFromBarcode) //only a receipt may be scanned
                    return; //ignore the scan

                try
                {
                    //ask the server what this is
                    m_sellingForm.NotIdle(true);

                    m_pos.StartGetProduct(barcode);
                    m_pos.ShowWaitForm(this);

                    if (!m_sellingForm.CheckForError()) //no errors
                    {
                        PackageButton pb = m_pos.GetProductDataButton as PackageButton;

                        if (pb != null) //we have a package button
                        {
                            if (m_pos.GetProductDataButtonValues == null && m_state == KioskState.GetPaperFromBarcode) //waiting for paper from barcode and this is a scan code
                            {
                                m_sellingForm.NotIdle();
                                return; //ignore the scan
                            }

                            //if the package has barcoded paper in it, see if we are allowed to use it
                            if (pb.Package.HasBarcodedPaper && ((m_pos.WeAreASimplePOSKiosk && !m_weHavePaperButtons) || !m_pos.Settings.AllowPaperOnKiosks || (m_state != KioskState.GetItems && m_state != KioskState.GetPaperFromBarcode))) //we don't want paper now
                            {
                                m_sellingForm.NotIdle();
                                return; //ignore the scan
                            }

                            //if this came from a scan code, make sure we are allowed to use it
                            if (m_pos.GetProductDataButtonValues == null && (!m_pos.Settings.AllowScanningProductsOnSimpleKiosk || m_state != KioskState.GetItems)) //we don't want scan codes now
                            {
                                m_sellingForm.NotIdle();
                                return; //ignore the scan
                            }

                            if (m_pos.Settings.KiosksCanOnlySellFromTheirButtons) //only push the button if it is represented by a kiosk button
                            {
                                System.Windows.Forms.Control.ControlCollection controls = m_panelKioskMenuButtons.Controls;

                                if (controls != null)
                                {
                                    foreach (ImageButton ourImageButton in controls)
                                    {
                                        if (ourImageButton.Tag == m_pos.GetProductDataButton && ourImageButton.Enabled) //found it
                                        {
                                            m_pos.GetProductDataButton.Click(this, m_pos.GetProductDataButtonValues);
                                            FillInReceipt();
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    m_pos.Log("Failed to find the product for barcode: " + ex.Message, LoggerLevel.Warning);
                    m_pos.ShowMessage(this, m_displayMode, ex.Message);
                }

                m_sellingForm.NotIdle();

                if (m_state == KioskState.GetPaperFromBarcode)
                    StartWaitingForItems();
            }
        }

        private void CardSwiped(object sender, MagneticCardSwipeArgs e)
        {
            if (m_kioskClosed)
                return;

            m_sellingForm.NotIdle();

            m_idleTimerForVideo.Stop();

            StopVideo();

            if (!m_pos.WeAreAnAdvancedPOSKiosk)
                StartIdleState();

            m_sellingForm.GetPlayer(e.CardData, true);

            if (m_pos.WeAreAnAdvancedPOSKiosk)
            {
                m_lblVideoPrompt_Click(null, new EventArgs());
                return;
            }

            if (m_pos.CurrentSale != null && m_pos.CurrentSale.Player != null)
            {
                if (m_pos.WeAreAB3Kiosk) //do B3
                {
                    StartWaitingForB3();
                }
                else if (m_pos.WeCanSellForB3AtKiosk) //Ask if bingo or B3
                {
                    StartWaitingForBingoOrB3Selection();
                }
                else if (m_pos.WeAreASimplePOSKiosk || m_pos.WeAreAHybridKiosk) //get items
                {
                    StartWaitingForItems();
                }
                else if (m_pos.WeAreABuyAgainKiosk) //get receipt to buy again from
                {
                    StartWaitingForReceipt();
                }
            }
            else //bad player card
            {
                m_idleTimerForVideo.Start();
                StartWaitingForPlayerCard();
            }
        }

        private void SimpleKioskForm_KeyDown(object sender, KeyEventArgs e)
        {
            e.SuppressKeyPress = false;

            if (m_state == KioskState.GetPlayerCard && m_MSR.MSRInputInProgress)
                e.Handled = true;

            if (m_state == KioskState.GetReceiptFromBarcode || m_state == KioskState.GetItems || m_state == KioskState.GetPaperFromBarcode)
                e.Handled = true;
        }

        /// <summary>
        /// Handles the form's KeyPress event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An KeyPressEventArgs object that contains the 
        /// event data.</param>
        private void KeyPressed(object sender, KeyPressEventArgs e)
        {
            m_sellingForm.NotIdle();

            if (m_state == KioskState.GetPlayerCard)
            {
                if (m_MSR.ProcessCharacter(e.KeyChar))
                    e.Handled = true; // Don't send to the active control.
            }

            if (!e.Handled && (m_state == KioskState.GetReceiptFromBarcode || m_state == KioskState.GetItems || m_state == KioskState.GetPaperFromBarcode || (m_state == KioskState.GetPlayerCard && m_weCanPrintCBBPlayItSheets)))
            {
                m_barcodeBuffer.Append(e.KeyChar);
                e.Handled = true; // Don't send to the active control.
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
            m_sellingForm.NotIdle();

            if (m_MSR.MSRInputInProgress)
                return false;

            if (keyData == Keys.Enter)
            {
                if (m_state == KioskState.GetReceiptFromBarcode || m_state == KioskState.GetItems || m_state == KioskState.GetPaperFromBarcode || (m_state == KioskState.GetPlayerCard && m_weCanPrintCBBPlayItSheets))
                    HandleScannerInput();

                return false;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if ((keyData & Keys.Enter) == Keys.Enter && (keyData & Keys.Shift) != Keys.Shift)
            {
                if (m_MSR.ReadingCards && m_MSR.MSRInputInProgress)
                    return false;
                else
                    return true;
            }
            else
            {
                return base.ProcessDialogKey(keyData);
            }
        }

        #endregion

        #region Help support

        private void ScanReceiptHelp_Click(object sender, EventArgs e)
        {
            m_sellingForm.NotIdle();
            StartWaitingForReceipt();
        }

        private void ScanPaperHelp_Click(object sender, EventArgs e)
        {
            m_sellingForm.NotIdle();
            StartWaitingForPaperScan();
        }

        private void m_btnHelp_Click(object sender, EventArgs e)
        {
            m_pos.HelpScreenActive = true;

            HelpForm help = new HelpForm(m_pos);

            bool timedOut = help.ShowDialog(this) == DialogResult.Abort;

            m_pos.HelpScreenActive = false;

            if (timedOut)
                m_sellingForm.ForceKioskTimeout(1);
        }

        private void AutoCouponHelp(object sender, EventArgs e)
        {
            m_pos.HelpScreenActive = true;

            HelpForm help = new HelpForm(m_pos, HelpForm.HelpTopic.AutoCoupons);

            bool timedOut = help.ShowDialog(this) == DialogResult.Abort;

            m_pos.HelpScreenActive = false;

            if (timedOut)
                m_sellingForm.ForceKioskTimeout(1);
        }

        #endregion
        
        #region Receipt support

        private void FillInReceipt()
        {
            //fill in the receipt
            m_lbReceipt.Items.Clear();

            string[] saleItemsList = m_sellingForm.GetSaleList(true); //get the text lines from the main receipt marking non-taxed coupons with a 0x00 as the first character

            //remove any additional information text
            for (int x = 0; x < saleItemsList.Length; x++)
            {
                if (saleItemsList[x] == "  ")
                {
                    Array.Resize<string>(ref saleItemsList, x);
                    break;
                }
            }

            //add the column descriptions
            if (m_pos.Settings.LongPOSDescriptions)
                m_lbReceipt.Items.Add(new ListBoxTenderItem("Sess Qty Item                 Subtotal", 0));
            else
                m_lbReceipt.Items.Add(new ListBoxTenderItem("Sess Qty Item        Pts      Subtotal", 0));

            m_lbReceipt.Items.Add(new ListBoxTenderItem("--------------------------------------", 0));

            //add the detail lines from the main receipt
            if (saleItemsList != null && saleItemsList.Length > 0)
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

                    m_lbReceipt.Items.Add(new ListBoxTenderItem(tmp, 0, false, (tmp[0] != ' ' || tmp == "  " ? -1 : receiptLine), nonTaxedCoupon));

                    receiptLine++;
                }

                //Add the totals to the receipt
                m_lbReceipt.Items.Add(new ListBoxTenderItem("--------------------------------------", 0));

                decimal feesAndTaxesAmount = m_pos.CurrentSale.CalculateFees() + m_pos.CurrentSale.CalculateTaxes();

                m_lbReceipt.Items.Add(new ListBoxTenderItem("Subtotal", m_pos.CurrentSale.CalculateSubtotal(), true));
                m_lbReceipt.Items.Add(new ListBoxTenderItem("Taxes/Fees" + (m_pos.CurrentSale.Device.Id != 0 ? " (" + m_pos.CurrentSale.Device.Name + ")" : ""), feesAndTaxesAmount, true));

                if (!m_pos.Settings.CouponTaxable)
                    m_lbReceipt.Items.Add(new ListBoxTenderItem("Coupons", m_pos.CurrentSale.CalculateNonTaxableCouponTotal(), true));

                m_lbReceipt.Items.Add(new ListBoxTenderItem("Prepaid", -(m_pos.CurrentSale.CalculatePrepaidAmount()+m_pos.CurrentSale.CalculatePrepaidTaxTotal()), true));
                m_lbReceipt.Items.Add(new ListBoxTenderItem("Total", m_pos.CurrentSale.CalculateTotal(false), true));
            }
        }
        private void m_lbReceipt_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            try
            {
                if (e.Index > -1 && m_lbReceipt.Items.Count > 0)
                {
                    ListBoxTenderItem current = m_lbReceipt.Items[e.Index] as ListBoxTenderItem;

                    if (current != null)
                    {
                        e.ItemWidth = 0;
                        e.ItemHeight = 0;

                        // Get the size of the string in this item.
                        SizeF stringSize;

                        stringSize = e.Graphics.MeasureString(current.ToString(), m_lbReceipt.Font);

                        // The width of the item is the width of the string (and optionally the image).
                        e.ItemWidth = (int)stringSize.Width;

                        // The height of the item is the bigger height of the image or string.
                        e.ItemHeight = (int)stringSize.Height;
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private void m_lbReceipt_DrawItem(object sender, DrawItemEventArgs e)
        {
            m_sellingForm.NotIdle();

            try
            {
                Font myFont;
                int i = e.Index;

                if (m_lbReceipt.Items.Count > 0 && i > -1)
                {
                    ListBoxTenderItem current = m_lbReceipt.Items[i] as ListBoxTenderItem;

                    if (current != null)
                    {
                        // Draw the highlight color if this item is selected.
                        if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                            e.Graphics.FillRectangle(new SolidBrush(Color.Yellow), e.Bounds);
                        else
                            e.Graphics.FillRectangle(new SolidBrush(Color.White), e.Bounds);

                        myFont = e.Font;

                        Brush textBrush = System.Drawing.Brushes.Black;

                        if (current.IsTextLine && !current.IsTotalLine && current.NonTaxedCoupon) //non-taxed coupons are orange
                            textBrush = System.Drawing.Brushes.Orange;

                        e.Graphics.DrawString(current.ToString(), myFont, textBrush, e.Bounds);

                        e.DrawFocusRectangle();
                    }
                }
            }
            catch (Exception)
            {
            }
        }

#endregion

        #region Coupon support

        private void m_btnCoupons_Click(object sender, EventArgs e)
        {
            m_pos.CurrentCouponForm.Prep(m_pos.CurrentSale.Device.Id);
            m_pos.CurrentCouponForm.LoadPlayerComp();
            DialogResult result = m_pos.CurrentCouponForm.ShowDialog(this);
            FillInReceipt();
            SetDeviceButton();
            m_couponButtonUsed = true;
            m_lblHybridAutoCoupons.Visible = false;

            if (result == DialogResult.Abort)
                m_sellingForm.ForceKioskTimeout(1);
        }

        private void ReapplyCoupons()
        {
            if (!m_couponButtonUsed && m_pos.CurrentCouponForm != null && m_pos.Settings.AutoApplyCouponsOnSimpleKiosks)
            {
                bool couponsApplied = false;

                m_pos.RemoveAllCouponsFromSale();
                m_pos.CurrentCouponForm.Prep(m_pos.CurrentSale != null ? m_pos.CurrentSale.Device.Id : 0);
                couponsApplied = m_pos.CurrentCouponForm.AutoApply();

                if (m_pos.WeAreAHybridKiosk)
                    m_lblHybridAutoCoupons.Visible = couponsApplied;
                else
                    m_lblAutoCoupons.Visible = couponsApplied;

                m_sellingForm.UpdateSaleInfo();
            }
        }

        #endregion

        private void button_Click(Object sender, EventArgs e)
        {
            m_pos.PushMenuButton((MenuButton)((ImageButton)sender).Tag);
            ReapplyCoupons();

            if (m_pos.WeAreAHybridKiosk)
            {
                FillInReceipt();
                SetDeviceButton();
            }
        }

        private void button_ButtonHeld(object sender, EventArgs e)
        {
            m_sellingForm.NotIdle(true);

            ImageButton button = sender as ImageButton;

            if (button != null && button.Tag != null && button.Tag is PackageButton)
            {
                PackageButton p = ((ImageButton)sender).Tag as PackageButton;

                if (p.Package != null)
                {
                    int qty = 0;

                    //see how many we have now
                    if (button.UseSecondaryText && !string.IsNullOrWhiteSpace(button.SecondaryText))
                        qty = GetIntAtEnd(button.SecondaryText);

                    //see what they want to do
                    KioskKeypadForm qtyControl = new KioskKeypadForm(m_pos, m_pos.Settings.DisplayMode.BasicCopy(), qty);
                    DialogResult result = qtyControl.ShowDialog(this);
                    qty = qtyControl.QuantityToAdd;
                    qtyControl.Dispose();

                    if (result == DialogResult.No) //remove
                    {
                        m_sellingForm.RemovePackageForNonAdvancedPOSKiosk(p.Package);
                        ReapplyCoupons();

                        if (m_pos.WeAreAHybridKiosk)
                        {
                            FillInReceipt();
                            SetDeviceButton();
                        }
                    }
                    else if (result == DialogResult.OK) //add items
                    {
                        m_sellingForm.SetKeypadValue(qty);
                        button_Click(sender, e);
                    }
                }
            }

            m_sellingForm.NotIdle();
        }

        private void m_btnBuy_Click(object sender, EventArgs e)
        {
            if (m_state == KioskState.GetReceiptFromBarcode && string.Compare(m_btnBuy.Text, "Buy", true) != 0)
            {
                if (!m_pos.WeAreABuyAgainKiosk)
                    StartWaitingForItems();

                m_sellingForm.ProcessRepeatSaleThroughServer(m_pos.CurrentSale.Player.Id, 0);

                if (m_pos.CurrentSale != null && m_pos.CurrentSale.GetItems().Length > 0) //we got something
                {
                    ReapplyCoupons();

                    if (!m_pos.WeAreASimplePOSKiosk)
                    {
                        FillInReceipt();
                        SetDeviceButton();
                    }

                    if (m_pos.WeAreABuyAgainKiosk)
                        StartWaitingForQuitOrBuy();
                }
            }
            else
            {
                StartIdleState();
                m_state = KioskState.GetPayment;
                m_sellingForm.ProcessSale();
            }
        }

        private void m_btnQuit_Click(object sender, EventArgs e)
        {
            if (m_state == KioskState.GetPayment)
            {
            }
            else
            {
                if (string.Compare(m_btnQuit.Text, "CLOSE", true) == 0)
                {
                    //return to the sale
                    StartWaitingForItems();
                    return;
                }

                if (m_pos.WeCanSellForB3AtKiosk)
                {
                    if (m_state == KioskState.GetWhatWeAreSelling)
                    {
                        m_pos.ClearSale();
                        StartOver(true);
                    }
                    else
                    {
                        StartWaitingForBingoOrB3Selection();
                    }
                }
                else
                {
                    m_sellingForm.StartOver(true);
                    StartWaitingForPlayerCard();
                }
            }
        }

        private void m_btnDevice_Click(object sender, EventArgs e)
        {
            m_sellingForm.NotIdle();

            m_sellingForm.UnitSelectionButton(null, new EventArgs());

            SetDeviceButton();

            FillInReceipt();
        }

        private void m_btnNoPlayerCard_Click(object sender, EventArgs e)
        {
            m_sellingForm.NotIdle();

            if (m_pos.WeAreAB3Kiosk)
            {
                StartWaitingForB3();
                return;
            }

            System.Windows.Forms.Control.ControlCollection controls = m_panelKioskMenuButtons.Controls;

            if (controls != null)
            {
                foreach (ImageButton ourImageButton in controls)
                {
                    if (((PackageButton)(ourImageButton.Tag)).IsPlayerRequired || ((PackageButton)(ourImageButton.Tag)).Package.PointsToRedeem > 0)
                    {
                        ourImageButton.Enabled = false;
                        m_buttonsDisabledForNoPlayer.Add(ourImageButton);
                        break;
                    }
                }
            }

            if (m_pos.WeCanSellForB3AtKiosk)
            {
                StartWaitingForBingoOrB3Selection();
                return;
            }

            m_pos.StartSale(false);

            if (m_pos.CurrentSale != null)
            {
                if (m_pos.WeAreASimplePOSKiosk || m_pos.WeAreAHybridKiosk)
                    StartWaitingForItems();
                else if (m_pos.WeAreABuyAgainKiosk)
                    StartWaitingForReceipt();
            }
        }
        
        private void m_btnB3_Click(object sender, EventArgs e)
        {
            StartWaitingForB3();
        }

        private void m_btnBingo_Click(object sender, EventArgs e)
        {
            Player player = null;

            if (m_pos.CurrentSale != null)
            {
                if (m_pos.CurrentSale.Player != null)
                    player = m_pos.CurrentSale.Player;

                m_pos.ClearSale();
            }

            m_pos.StartSale(false);

            if (player != null)
            {
                m_pos.CurrentSale.SetPlayer(player, true, true);
                m_pos.SellingForm.SetPlayer();
            }

            if (m_parent.WeAreABuyAgainKiosk)
                StartWaitingForReceipt();
            else if(m_parent.WeAreAHybridKiosk || m_parent.WeAreASimplePOSKiosk)
                StartWaitingForItems();
        }
        
        /// <summary>
        /// Test mode Kiosk shutdown.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void m_lblUseCardOrScanReceipt_MouseUp(object sender, MouseEventArgs e)
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

        private void m_lblUseCardOrScanReceipt_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (m_parent.Settings.KioskInTestMode) //we are testing, allow double click to send suspend message
                m_parent.RequestHelpFromGuardian("Test of suspend request from Kiosk.");
        }

        private void m_testModeTimer_Tick(object sender, EventArgs e)
        {
            m_olblTestMode.Visible = !m_olblTestMode.Visible;
            m_olblTestMode.Update();
        }

        private int GetIntAtEnd(string text)
        {
            if(string.IsNullOrWhiteSpace(text))
                return 0;

            StringBuilder sb = new StringBuilder();

            for (int x = text.Length - 1; x >= 0 && text[x] >= '0' && text[x] <= '9'; x--)
                sb.Insert(0, text[x]);

            if (sb.Length == 0)
                return 0;

            int result = 0;

            Int32.TryParse(sb.ToString(), out result);

            return result;
        }

        private void UserActivityDetected(object sender, EventArgs e)
        {
            m_sellingForm.NotIdle();

            if (m_panelVideo.Visible)
            {
                StopVideo();
                StartWaitingForPlayerCard();
            }
        }

        private void SetDeviceButton()
        {
            m_sellingForm.NotIdle();

            if (m_pos.WeAreAHybridKiosk && m_pos.CurrentSale != null && m_pos.CurrentSale.HasElectronics && m_parent.SellingForm.WeHaveAUnitSelectionSystemMenuButton)
            {
                m_btnDevice.Visible = true;

                if (m_pos.CurrentSale.Device.Id == Device.Explorer.Id)
                {
                    m_btnDevice.ImageNormal = Resources.DeviceExplorerUp200x90;
                    m_btnDevice.ImagePressed = Resources.DeviceExplorerDown200x90;
                }
                else if (m_pos.CurrentSale.Device.Id == Device.Fixed.Id)
                {
                    m_btnDevice.ImageNormal = Resources.DeviceFixedUp200x90;
                    m_btnDevice.ImagePressed = Resources.DeviceFixedDown200x90;
                }
                else if (m_pos.CurrentSale.Device.Id == Device.Tablet.Id)
                {
                    m_btnDevice.ImageNormal = Resources.DeviceTEDEUp200x90;
                    m_btnDevice.ImagePressed = Resources.DeviceTEDEDown200x90;
                }
                else if (m_pos.CurrentSale.Device.Id == Device.Tracker.Id)
                {
                    m_btnDevice.ImageNormal = Resources.DeviceTrackerUp200x90;
                    m_btnDevice.ImagePressed = Resources.DeviceTrackerDown200x90;
                }
                else if (m_pos.CurrentSale.Device.Id == Device.Traveler.Id)
                {
                    m_btnDevice.ImageNormal = Resources.DeviceTravelerUp200x90;
                    m_btnDevice.ImagePressed = Resources.DeviceTravelerDown200x90;
                }
                else if (m_pos.CurrentSale.Device.Id == Device.Traveler2.Id)
                {
                    m_btnDevice.ImageNormal = Resources.DeviceTraveler2Up200x90;
                    m_btnDevice.ImagePressed = Resources.DeviceTraveler2Down200x90;
                }
                else
                {
                    m_btnDevice.ImageNormal = Resources.DevicePackUp200x90;
                    m_btnDevice.ImagePressed = Resources.DevicePackDown200x90;
                }

                m_btnDevice.Text = " "+ (m_pos.CurrentSale.DeviceFee == 0M ? Resources.Free : m_pos.CurrentSale.SaleCurrency.FormatCurrencyString(m_pos.CurrentSale.SaleCurrency.ConvertFromDefaultCurrencyToThisCurrency(m_pos.CurrentSale.DeviceFee)));
            }
            else
            {
                m_btnDevice.Visible = false;
            }
        }

        private void SetQuitToClose(bool makeClose = true)
        {
            if (makeClose)
            {
                m_btnQuit.Text = "Close";
                m_btnQuit.ImageNormal = Resources.GrayButtonUp;
                m_btnQuit.ImagePressed = Resources.GrayButtonDown;
            }
            else
            {
                m_btnQuit.Text = "Quit";
                m_btnQuit.ImageNormal = Resources.RedButtonUp;
                m_btnQuit.ImagePressed = Resources.RedButtonDown;
            }
        }

        private void SetBuyToBuyAgain(bool makeBuyAgain = true)
        {
            if (makeBuyAgain)
            {
                m_btnBuy.Text = "Use Last Purchase";
                m_btnBuy.Font = new Font(m_btnBuy.Font.FontFamily, 28, m_btnBuy.Font.Style);
                m_btnBuy.ImageNormal = Resources.PurpleButtonUp;
                m_btnBuy.ImagePressed = Resources.PurpleButtonDown;
            }
            else
            {
                m_btnBuy.Text = "Buy";
                m_btnBuy.Font = new Font(m_btnBuy.Font.FontFamily, 36, m_btnBuy.Font.Style);
                m_btnBuy.ImageNormal = Resources.GreenButtonUp;
                m_btnBuy.ImagePressed = Resources.GreenButtonDown;
            }
        }

        private void SimpleKioskForm_LocationChanged(object sender, EventArgs e)
        {
            if (m_parent.WeAreANonAdvancedPOSKiosk && m_parent.GuardianHasUsSuspended)
                UserActivityDetected(sender, e);
        }
        
        private void SimpleKioskForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            m_idleTimerForVideo.Stop();
            StopVideo();
            m_MSR.CardSwiped -= CardSwiped;
            m_testModeTimer.Stop();

            Hide();
            Application.DoEvents();
            System.Threading.Thread.Sleep(100);
        }

        #endregion

        #region Properties

        public System.Windows.Forms.Control.ControlCollection MenuButtons
        {
            get
            {
                return m_panelKioskMenuButtons.Controls;
            }
        }

        public System.Windows.Forms.ProgressBar ProgressBar
        {
            get
            {
                return m_simpleKioskProgress;
            }
        }

        public bool KioskIsClosed
        {
            get
            {
                return m_kioskClosed;
            }
        }

        public KioskState State
        {
            get
            {
                return m_state;
            }
        }

        #endregion
    }
}
