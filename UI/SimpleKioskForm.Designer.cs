namespace GTI.Modules.POS.UI
{
    partial class SimpleKioskForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SimpleKioskForm));
            this.m_idleTimerForVideo = new System.Windows.Forms.Timer(this.components);
            this.m_panelVideo = new GTI.Controls.EliteGradientPanel();
            this.m_panelVideo2 = new GTI.Controls.EliteGradientPanel();
            this.m_axWindowsMediaPlayer = new AxWMPLib.AxWindowsMediaPlayer();
            this.m_lblVideoPrompt = new System.Windows.Forms.Label();
            this.m_panelMain = new GTI.Controls.EliteGradientPanel();
            this.m_panelReceipt = new GTI.Controls.EliteGradientPanel();
            this.m_lblHybridAutoCoupons = new GTI.Controls.OutlinedLabel();
            this.m_lblAutoCoupons = new GTI.Controls.OutlinedLabel();
            this.m_btnReceiptHelp = new GTI.Controls.ImageButton();
            this.m_btnPaperHelp = new GTI.Controls.ImageButton();
            this.m_btnHelp = new GTI.Controls.ImageButton();
            this.m_btnDevice = new GTI.Controls.ImageButton();
            this.m_btnCoupons = new GTI.Controls.ImageButton();
            this.m_panelKioskMenuButtons = new GTI.Controls.EliteGradientPanel();
            this.m_lbReceipt = new System.Windows.Forms.ListBox();
            this.m_lblWelcomeBack = new GTI.Controls.OutlinedLabel();
            this.m_simpleKioskProgress = new System.Windows.Forms.ProgressBar();
            this.m_lblTotal = new System.Windows.Forms.Label();
            this.m_lblTotalLabel = new System.Windows.Forms.Label();
            this.m_btnQuit = new GTI.Controls.ImageButton();
            this.m_btnBuy = new GTI.Controls.ImageButton();
            this.m_btnNoPlayerCard = new GTI.Controls.ImageButton();
            this.m_panelUseCardScanReceiptOrScanPaper = new GTI.Controls.EliteGradientPanel();
            this.m_picUseCardOrScanReceipt = new System.Windows.Forms.PictureBox();
            this.m_lblUseCardOrScanReceipt = new System.Windows.Forms.Label();
            this.m_lblWorking = new GTI.Controls.OutlinedLabel();
            this.m_picLogo = new System.Windows.Forms.PictureBox();
            this.m_picCBBPrintingLogo = new System.Windows.Forms.PictureBox();
            this.m_panelB3OrBingo = new GTI.Controls.EliteGradientPanel();
            this.m_lblB3OrBingoGreeting = new GTI.Controls.OutlinedLabel();
            this.m_btnBingo = new GTI.Controls.ImageButton();
            this.m_btnB3 = new GTI.Controls.ImageButton();
            this.m_olblTestMode = new GTI.Controls.OutlinedLabel();
            this.m_testModeTimer = new System.Windows.Forms.Timer(this.components);
            this.m_burnInPreventionTimer = new System.Windows.Forms.Timer(this.components);
            this.m_panelVideo.SuspendLayout();
            this.m_panelVideo2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.m_axWindowsMediaPlayer)).BeginInit();
            this.m_panelMain.SuspendLayout();
            this.m_panelReceipt.SuspendLayout();
            this.m_panelUseCardScanReceiptOrScanPaper.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.m_picUseCardOrScanReceipt)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_picLogo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_picCBBPrintingLogo)).BeginInit();
            this.m_panelB3OrBingo.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_idleTimerForVideo
            // 
            this.m_idleTimerForVideo.Interval = 60000;
            this.m_idleTimerForVideo.Tick += new System.EventHandler(this.m_idleTimerForVideo_Tick);
            // 
            // m_panelVideo
            // 
            this.m_panelVideo.BackColor = System.Drawing.Color.Red;
            this.m_panelVideo.BorderColor = System.Drawing.Color.Silver;
            this.m_panelVideo.Controls.Add(this.m_panelVideo2);
            this.m_panelVideo.Controls.Add(this.m_lblVideoPrompt);
            this.m_panelVideo.GradientBeginColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(179)))), ((int)(((byte)(213)))));
            this.m_panelVideo.GradientEndColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(186)))), ((int)(((byte)(192)))));
            this.m_panelVideo.InnerBorderEdgeColor = System.Drawing.Color.SlateGray;
            this.m_panelVideo.Location = new System.Drawing.Point(0, 0);
            this.m_panelVideo.Name = "m_panelVideo";
            this.m_panelVideo.OuterBorderEdgeColor = System.Drawing.Color.SlateGray;
            this.m_panelVideo.Size = new System.Drawing.Size(42, 39);
            this.m_panelVideo.TabIndex = 24;
            this.m_panelVideo.Visible = false;
            this.m_panelVideo.Click += new System.EventHandler(this.m_lblVideoPrompt_Click);
            // 
            // m_panelVideo2
            // 
            this.m_panelVideo2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.m_panelVideo2.BackColor = System.Drawing.Color.PaleGreen;
            this.m_panelVideo2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.m_panelVideo2.BorderColor = System.Drawing.Color.Silver;
            this.m_panelVideo2.Controls.Add(this.m_axWindowsMediaPlayer);
            this.m_panelVideo2.GradientBeginColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(179)))), ((int)(((byte)(213)))));
            this.m_panelVideo2.GradientEndColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(186)))), ((int)(((byte)(192)))));
            this.m_panelVideo2.InnerBorderEdgeColor = System.Drawing.Color.SlateGray;
            this.m_panelVideo2.Location = new System.Drawing.Point(0, 0);
            this.m_panelVideo2.Name = "m_panelVideo2";
            this.m_panelVideo2.OuterBorderEdgeColor = System.Drawing.Color.SlateGray;
            this.m_panelVideo2.Size = new System.Drawing.Size(23, 23);
            this.m_panelVideo2.TabIndex = 1;
            // 
            // m_axWindowsMediaPlayer
            // 
            this.m_axWindowsMediaPlayer.Enabled = true;
            this.m_axWindowsMediaPlayer.Location = new System.Drawing.Point(4, 0);
            this.m_axWindowsMediaPlayer.Name = "m_axWindowsMediaPlayer";
            this.m_axWindowsMediaPlayer.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("m_axWindowsMediaPlayer.OcxState")));
            this.m_axWindowsMediaPlayer.Size = new System.Drawing.Size(75, 23);
            this.m_axWindowsMediaPlayer.TabIndex = 0;
            this.m_axWindowsMediaPlayer.MouseDownEvent += new AxWMPLib._WMPOCXEvents_MouseDownEventHandler(this.m_axWindowsMediaPlayer_MouseDownEvent);
            this.m_axWindowsMediaPlayer.MouseUpEvent += new AxWMPLib._WMPOCXEvents_MouseUpEventHandler(this.m_axWindowsMediaPlayer_MouseUpEvent);
            // 
            // m_lblVideoPrompt
            // 
            this.m_lblVideoPrompt.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.m_lblVideoPrompt.BackColor = System.Drawing.Color.Orange;
            this.m_lblVideoPrompt.Font = new System.Drawing.Font("Tahoma", 36F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_lblVideoPrompt.ForeColor = System.Drawing.Color.Blue;
            this.m_lblVideoPrompt.Location = new System.Drawing.Point(0, 0);
            this.m_lblVideoPrompt.Name = "m_lblVideoPrompt";
            this.m_lblVideoPrompt.Size = new System.Drawing.Size(100, 23);
            this.m_lblVideoPrompt.TabIndex = 0;
            this.m_lblVideoPrompt.Text = "Use your player card to buy BINGO packs!";
            this.m_lblVideoPrompt.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.m_lblVideoPrompt.Click += new System.EventHandler(this.m_lblVideoPrompt_Click);
            // 
            // m_panelMain
            // 
            this.m_panelMain.BorderColor = System.Drawing.Color.Silver;
            this.m_panelMain.BorderThickness = 3;
            this.m_panelMain.Controls.Add(this.m_panelReceipt);
            this.m_panelMain.Controls.Add(this.m_simpleKioskProgress);
            this.m_panelMain.Controls.Add(this.m_lblTotal);
            this.m_panelMain.Controls.Add(this.m_lblTotalLabel);
            this.m_panelMain.Controls.Add(this.m_btnQuit);
            this.m_panelMain.Controls.Add(this.m_btnBuy);
            this.m_panelMain.Controls.Add(this.m_btnNoPlayerCard);
            this.m_panelMain.Controls.Add(this.m_panelUseCardScanReceiptOrScanPaper);
            this.m_panelMain.Controls.Add(this.m_lblWorking);
            this.m_panelMain.Controls.Add(this.m_picLogo);
            this.m_panelMain.Controls.Add(this.m_picCBBPrintingLogo);
            this.m_panelMain.Controls.Add(this.m_panelB3OrBingo);
            this.m_panelMain.Controls.Add(this.m_olblTestMode);
            this.m_panelMain.DrawAsGradient = true;
            this.m_panelMain.DrawRounded = true;
            this.m_panelMain.GradientBeginColor = System.Drawing.Color.FromArgb(((int)(((byte)(214)))), ((int)(((byte)(211)))), ((int)(((byte)(216)))));
            this.m_panelMain.GradientEndColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(127)))), ((int)(((byte)(127)))));
            this.m_panelMain.InnerBorderEdgeColor = System.Drawing.Color.SlateGray;
            this.m_panelMain.Location = new System.Drawing.Point(0, 0);
            this.m_panelMain.Name = "m_panelMain";
            this.m_panelMain.OuterBorderEdgeColor = System.Drawing.Color.SlateGray;
            this.m_panelMain.Size = new System.Drawing.Size(1024, 768);
            this.m_panelMain.TabIndex = 26;
            this.m_panelMain.Click += new System.EventHandler(this.UserActivityDetected);
            // 
            // m_panelReceipt
            // 
            this.m_panelReceipt.BackColor = System.Drawing.Color.Transparent;
            this.m_panelReceipt.BorderColor = System.Drawing.Color.Silver;
            this.m_panelReceipt.Controls.Add(this.m_lblHybridAutoCoupons);
            this.m_panelReceipt.Controls.Add(this.m_lblAutoCoupons);
            this.m_panelReceipt.Controls.Add(this.m_btnReceiptHelp);
            this.m_panelReceipt.Controls.Add(this.m_btnPaperHelp);
            this.m_panelReceipt.Controls.Add(this.m_btnHelp);
            this.m_panelReceipt.Controls.Add(this.m_btnDevice);
            this.m_panelReceipt.Controls.Add(this.m_btnCoupons);
            this.m_panelReceipt.Controls.Add(this.m_panelKioskMenuButtons);
            this.m_panelReceipt.Controls.Add(this.m_lbReceipt);
            this.m_panelReceipt.Controls.Add(this.m_lblWelcomeBack);
            this.m_panelReceipt.GradientBeginColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(179)))), ((int)(((byte)(213)))));
            this.m_panelReceipt.GradientEndColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(186)))), ((int)(((byte)(192)))));
            this.m_panelReceipt.InnerBorderEdgeColor = System.Drawing.Color.SlateGray;
            this.m_panelReceipt.Location = new System.Drawing.Point(13, 13);
            this.m_panelReceipt.Name = "m_panelReceipt";
            this.m_panelReceipt.OuterBorderEdgeColor = System.Drawing.Color.SlateGray;
            this.m_panelReceipt.Size = new System.Drawing.Size(999, 660);
            this.m_panelReceipt.TabIndex = 6;
            this.m_panelReceipt.Click += new System.EventHandler(this.UserActivityDetected);
            // 
            // m_lblHybridAutoCoupons
            // 
            this.m_lblHybridAutoCoupons.EdgeColor = System.Drawing.Color.Green;
            this.m_lblHybridAutoCoupons.Font = new System.Drawing.Font("Tahoma", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_lblHybridAutoCoupons.ForeColor = System.Drawing.Color.Chartreuse;
            this.m_lblHybridAutoCoupons.Location = new System.Drawing.Point(796, 549);
            this.m_lblHybridAutoCoupons.Name = "m_lblHybridAutoCoupons";
            this.m_lblHybridAutoCoupons.Size = new System.Drawing.Size(200, 90);
            this.m_lblHybridAutoCoupons.TabIndex = 9;
            this.m_lblHybridAutoCoupons.Text = "Coupons have been applied based on expiration date.";
            this.m_lblHybridAutoCoupons.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.m_lblHybridAutoCoupons.Visible = false;
            this.m_lblHybridAutoCoupons.Click += new System.EventHandler(this.AutoCouponHelp);
            // 
            // m_lblAutoCoupons
            // 
            this.m_lblAutoCoupons.EdgeColor = System.Drawing.Color.Chartreuse;
            this.m_lblAutoCoupons.Font = new System.Drawing.Font("Tahoma", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_lblAutoCoupons.ForeColor = System.Drawing.Color.Black;
            this.m_lblAutoCoupons.Location = new System.Drawing.Point(4, 4);
            this.m_lblAutoCoupons.Name = "m_lblAutoCoupons";
            this.m_lblAutoCoupons.Size = new System.Drawing.Size(340, 96);
            this.m_lblAutoCoupons.TabIndex = 8;
            this.m_lblAutoCoupons.Text = "Coupons have been automatically applied\r\nbased on expiration date.";
            this.m_lblAutoCoupons.Visible = false;
            this.m_lblAutoCoupons.Click += new System.EventHandler(this.AutoCouponHelp);
            // 
            // m_btnReceiptHelp
            // 
            this.m_btnReceiptHelp.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.m_btnReceiptHelp.FocusColor = System.Drawing.Color.Black;
            this.m_btnReceiptHelp.ImageNormal = global::GTI.Modules.POS.Properties.Resources.AnimatedReceiptScan_large_;
            this.m_btnReceiptHelp.ImagePressed = global::GTI.Modules.POS.Properties.Resources.AnimatedReceiptScan_large_;
            this.m_btnReceiptHelp.Location = new System.Drawing.Point(780, 4);
            this.m_btnReceiptHelp.MinimumSize = new System.Drawing.Size(30, 30);
            this.m_btnReceiptHelp.Name = "m_btnReceiptHelp";
            this.m_btnReceiptHelp.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_btnReceiptHelp.ShowFocus = false;
            this.m_btnReceiptHelp.Size = new System.Drawing.Size(65, 96);
            this.m_btnReceiptHelp.Stretch = false;
            this.m_btnReceiptHelp.TabIndex = 7;
            this.m_btnReceiptHelp.Click += new System.EventHandler(this.ScanReceiptHelp_Click);
            // 
            // m_btnPaperHelp
            // 
            this.m_btnPaperHelp.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.m_btnPaperHelp.FocusColor = System.Drawing.Color.Black;
            this.m_btnPaperHelp.ImageNormal = global::GTI.Modules.POS.Properties.Resources.AnimatedScanPaper_large_;
            this.m_btnPaperHelp.ImagePressed = global::GTI.Modules.POS.Properties.Resources.AnimatedScanPaper_large_;
            this.m_btnPaperHelp.Location = new System.Drawing.Point(709, 4);
            this.m_btnPaperHelp.MinimumSize = new System.Drawing.Size(30, 30);
            this.m_btnPaperHelp.Name = "m_btnPaperHelp";
            this.m_btnPaperHelp.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_btnPaperHelp.ShowFocus = false;
            this.m_btnPaperHelp.Size = new System.Drawing.Size(65, 96);
            this.m_btnPaperHelp.Stretch = false;
            this.m_btnPaperHelp.TabIndex = 6;
            this.m_btnPaperHelp.Click += new System.EventHandler(this.ScanPaperHelp_Click);
            // 
            // m_btnHelp
            // 
            this.m_btnHelp.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.m_btnHelp.FitImageIcon = true;
            this.m_btnHelp.FocusColor = System.Drawing.Color.Black;
            this.m_btnHelp.Font = new System.Drawing.Font("Tahoma", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_btnHelp.ImageNormal = global::GTI.Modules.POS.Properties.Resources.PurpleButtonUp;
            this.m_btnHelp.ImagePressed = global::GTI.Modules.POS.Properties.Resources.PurpleButtonDown;
            this.m_btnHelp.Location = new System.Drawing.Point(851, 4);
            this.m_btnHelp.MinimumSize = new System.Drawing.Size(30, 30);
            this.m_btnHelp.Name = "m_btnHelp";
            this.m_btnHelp.Padding = new System.Windows.Forms.Padding(0, 0, 0, 4);
            this.m_btnHelp.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_btnHelp.ShowFocus = false;
            this.m_btnHelp.Size = new System.Drawing.Size(139, 96);
            this.m_btnHelp.TabIndex = 5;
            this.m_btnHelp.Text = "Help";
            this.m_btnHelp.Click += new System.EventHandler(this.m_btnHelp_Click);
            // 
            // m_btnDevice
            // 
            this.m_btnDevice.Alignment = System.Drawing.StringAlignment.Near;
            this.m_btnDevice.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.m_btnDevice.FocusColor = System.Drawing.Color.Black;
            this.m_btnDevice.Font = new System.Drawing.Font("Tahoma", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_btnDevice.ImageNormal = global::GTI.Modules.POS.Properties.Resources.DeviceTEDEUp200x90;
            this.m_btnDevice.ImagePressed = global::GTI.Modules.POS.Properties.Resources.DeviceTEDEDown200x90;
            this.m_btnDevice.LineAlignment = System.Drawing.StringAlignment.Near;
            this.m_btnDevice.Location = new System.Drawing.Point(578, 549);
            this.m_btnDevice.MinimumSize = new System.Drawing.Size(30, 30);
            this.m_btnDevice.Name = "m_btnDevice";
            this.m_btnDevice.Padding = new System.Windows.Forms.Padding(10);
            this.m_btnDevice.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_btnDevice.ShowFocus = false;
            this.m_btnDevice.Size = new System.Drawing.Size(200, 90);
            this.m_btnDevice.TabIndex = 4;
            this.m_btnDevice.Text = "$4.00";
            this.m_btnDevice.Visible = false;
            this.m_btnDevice.Click += new System.EventHandler(this.m_btnDevice_Click);
            // 
            // m_btnCoupons
            // 
            this.m_btnCoupons.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.m_btnCoupons.Debounce = true;
            this.m_btnCoupons.FocusColor = System.Drawing.Color.Black;
            this.m_btnCoupons.Font = new System.Drawing.Font("Tahoma", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_btnCoupons.ImageNormal = global::GTI.Modules.POS.Properties.Resources.GreenButtonUp;
            this.m_btnCoupons.ImagePressed = global::GTI.Modules.POS.Properties.Resources.GreenButtonDown;
            this.m_btnCoupons.Location = new System.Drawing.Point(357, 549);
            this.m_btnCoupons.MinimumSize = new System.Drawing.Size(30, 30);
            this.m_btnCoupons.Name = "m_btnCoupons";
            this.m_btnCoupons.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_btnCoupons.ShowFocus = false;
            this.m_btnCoupons.Size = new System.Drawing.Size(200, 90);
            this.m_btnCoupons.TabIndex = 3;
            this.m_btnCoupons.Text = "Coupons";
            this.m_btnCoupons.Click += new System.EventHandler(this.m_btnCoupons_Click);
            // 
            // m_panelKioskMenuButtons
            // 
            this.m_panelKioskMenuButtons.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_panelKioskMenuButtons.BorderColor = System.Drawing.Color.Silver;
            this.m_panelKioskMenuButtons.GradientBeginColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(179)))), ((int)(((byte)(213)))));
            this.m_panelKioskMenuButtons.GradientEndColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(186)))), ((int)(((byte)(192)))));
            this.m_panelKioskMenuButtons.InnerBorderEdgeColor = System.Drawing.Color.SlateGray;
            this.m_panelKioskMenuButtons.Location = new System.Drawing.Point(357, 114);
            this.m_panelKioskMenuButtons.Name = "m_panelKioskMenuButtons";
            this.m_panelKioskMenuButtons.OuterBorderEdgeColor = System.Drawing.Color.SlateGray;
            this.m_panelKioskMenuButtons.Size = new System.Drawing.Size(618, 411);
            this.m_panelKioskMenuButtons.TabIndex = 2;
            this.m_panelKioskMenuButtons.Click += new System.EventHandler(this.UserActivityDetected);
            // 
            // m_lbReceipt
            // 
            this.m_lbReceipt.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.m_lbReceipt.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.m_lbReceipt.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_lbReceipt.IntegralHeight = false;
            this.m_lbReceipt.ItemHeight = 16;
            this.m_lbReceipt.Items.AddRange(new object[] {
            "Sess  Qty Item                     Total",
            "----------------------------------------",
            "ELECTRONIC",
            "   1    1 Blue                      5.00",
            "----------------------------------------",
            "       Subtotal:                   $5.00",
            "          Taxes:                   $0.40",
            "Sale Total(USD):                   $5.40"});
            this.m_lbReceipt.Location = new System.Drawing.Point(4, 4);
            this.m_lbReceipt.Name = "m_lbReceipt";
            this.m_lbReceipt.ScrollAlwaysVisible = true;
            this.m_lbReceipt.Size = new System.Drawing.Size(325, 656);
            this.m_lbReceipt.TabIndex = 1;
            this.m_lbReceipt.Click += new System.EventHandler(this.UserActivityDetected);
            this.m_lbReceipt.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.m_lbReceipt_DrawItem);
            this.m_lbReceipt.MeasureItem += new System.Windows.Forms.MeasureItemEventHandler(this.m_lbReceipt_MeasureItem);
            // 
            // m_lblWelcomeBack
            // 
            this.m_lblWelcomeBack.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.m_lblWelcomeBack.EdgeColor = System.Drawing.Color.White;
            this.m_lblWelcomeBack.Font = new System.Drawing.Font("Tahoma", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_lblWelcomeBack.ForeColor = System.Drawing.Color.Navy;
            this.m_lblWelcomeBack.Location = new System.Drawing.Point(350, 4);
            this.m_lblWelcomeBack.Name = "m_lblWelcomeBack";
            this.m_lblWelcomeBack.Size = new System.Drawing.Size(353, 106);
            this.m_lblWelcomeBack.TabIndex = 0;
            this.m_lblWelcomeBack.Text = "Welcome back Guywithbiglongname.";
            this.m_lblWelcomeBack.Click += new System.EventHandler(this.UserActivityDetected);
            // 
            // m_simpleKioskProgress
            // 
            this.m_simpleKioskProgress.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.m_simpleKioskProgress.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(127)))), ((int)(((byte)(127)))));
            this.m_simpleKioskProgress.ForeColor = System.Drawing.Color.Gold;
            this.m_simpleKioskProgress.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.m_simpleKioskProgress.Location = new System.Drawing.Point(241, 756);
            this.m_simpleKioskProgress.Name = "m_simpleKioskProgress";
            this.m_simpleKioskProgress.Size = new System.Drawing.Size(542, 10);
            this.m_simpleKioskProgress.TabIndex = 23;
            this.m_simpleKioskProgress.Visible = false;
            this.m_simpleKioskProgress.Click += new System.EventHandler(this.UserActivityDetected);
            // 
            // m_lblTotal
            // 
            this.m_lblTotal.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.m_lblTotal.BackColor = System.Drawing.Color.Transparent;
            this.m_lblTotal.Font = new System.Drawing.Font("Tahoma", 36F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_lblTotal.ForeColor = System.Drawing.Color.Navy;
            this.m_lblTotal.Location = new System.Drawing.Point(457, 689);
            this.m_lblTotal.Name = "m_lblTotal";
            this.m_lblTotal.Size = new System.Drawing.Size(297, 67);
            this.m_lblTotal.TabIndex = 5;
            this.m_lblTotal.Text = "$0.00";
            this.m_lblTotal.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.m_lblTotal.Click += new System.EventHandler(this.UserActivityDetected);
            // 
            // m_lblTotalLabel
            // 
            this.m_lblTotalLabel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.m_lblTotalLabel.BackColor = System.Drawing.Color.Transparent;
            this.m_lblTotalLabel.Font = new System.Drawing.Font("Tahoma", 36F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_lblTotalLabel.ForeColor = System.Drawing.Color.Navy;
            this.m_lblTotalLabel.Location = new System.Drawing.Point(282, 689);
            this.m_lblTotalLabel.Name = "m_lblTotalLabel";
            this.m_lblTotalLabel.Size = new System.Drawing.Size(169, 67);
            this.m_lblTotalLabel.TabIndex = 4;
            this.m_lblTotalLabel.Text = "Total:";
            this.m_lblTotalLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.m_lblTotalLabel.Click += new System.EventHandler(this.UserActivityDetected);
            // 
            // m_btnQuit
            // 
            this.m_btnQuit.BackColor = System.Drawing.Color.Transparent;
            this.m_btnQuit.Debounce = true;
            this.m_btnQuit.FocusColor = System.Drawing.Color.Black;
            this.m_btnQuit.Font = new System.Drawing.Font("Tahoma", 36F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_btnQuit.ImageNormal = global::GTI.Modules.POS.Properties.Resources.RedButtonUp;
            this.m_btnQuit.ImagePressed = global::GTI.Modules.POS.Properties.Resources.RedButtonDown;
            this.m_btnQuit.Location = new System.Drawing.Point(12, 689);
            this.m_btnQuit.MinimumSize = new System.Drawing.Size(30, 30);
            this.m_btnQuit.Name = "m_btnQuit";
            this.m_btnQuit.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_btnQuit.ShowFocus = false;
            this.m_btnQuit.Size = new System.Drawing.Size(220, 67);
            this.m_btnQuit.TabIndex = 3;
            this.m_btnQuit.Text = "Quit";
            this.m_btnQuit.UseVisualStyleBackColor = false;
            this.m_btnQuit.Click += new System.EventHandler(this.m_btnQuit_Click);
            // 
            // m_btnBuy
            // 
            this.m_btnBuy.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.m_btnBuy.BackColor = System.Drawing.Color.Transparent;
            this.m_btnBuy.Debounce = true;
            this.m_btnBuy.FocusColor = System.Drawing.Color.Black;
            this.m_btnBuy.Font = new System.Drawing.Font("Tahoma", 36F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_btnBuy.ImageNormal = global::GTI.Modules.POS.Properties.Resources.GreenButtonUp;
            this.m_btnBuy.ImagePressed = global::GTI.Modules.POS.Properties.Resources.GreenButtonDown;
            this.m_btnBuy.Location = new System.Drawing.Point(792, 689);
            this.m_btnBuy.MinimumSize = new System.Drawing.Size(30, 30);
            this.m_btnBuy.Name = "m_btnBuy";
            this.m_btnBuy.SecondaryTextLineAlignment = System.Drawing.StringAlignment.Center;
            this.m_btnBuy.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_btnBuy.ShowFocus = false;
            this.m_btnBuy.Size = new System.Drawing.Size(220, 67);
            this.m_btnBuy.TabIndex = 2;
            this.m_btnBuy.Text = "Buy";
            this.m_btnBuy.UseVisualStyleBackColor = false;
            this.m_btnBuy.Click += new System.EventHandler(this.m_btnBuy_Click);
            // 
            // m_btnNoPlayerCard
            // 
            this.m_btnNoPlayerCard.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.m_btnNoPlayerCard.AutoBlackOrWhiteText = true;
            this.m_btnNoPlayerCard.FocusColor = System.Drawing.Color.Black;
            this.m_btnNoPlayerCard.Font = new System.Drawing.Font("Tahoma", 36F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_btnNoPlayerCard.ForeColor = System.Drawing.Color.Black;
            this.m_btnNoPlayerCard.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueGelButtonUp;
            this.m_btnNoPlayerCard.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueGelButtonDown;
            this.m_btnNoPlayerCard.Location = new System.Drawing.Point(337, 690);
            this.m_btnNoPlayerCard.MinimumSize = new System.Drawing.Size(30, 30);
            this.m_btnNoPlayerCard.Name = "m_btnNoPlayerCard";
            this.m_btnNoPlayerCard.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_btnNoPlayerCard.ShowFocus = false;
            this.m_btnNoPlayerCard.Size = new System.Drawing.Size(350, 67);
            this.m_btnNoPlayerCard.TabIndex = 25;
            this.m_btnNoPlayerCard.Text = "No Player Card";
            this.m_btnNoPlayerCard.Visible = false;
            this.m_btnNoPlayerCard.Click += new System.EventHandler(this.m_btnNoPlayerCard_Click);
            // 
            // m_panelUseCardScanReceiptOrScanPaper
            // 
            this.m_panelUseCardScanReceiptOrScanPaper.BackColor = System.Drawing.Color.Black;
            this.m_panelUseCardScanReceiptOrScanPaper.BorderColor = System.Drawing.Color.Silver;
            this.m_panelUseCardScanReceiptOrScanPaper.Controls.Add(this.m_picUseCardOrScanReceipt);
            this.m_panelUseCardScanReceiptOrScanPaper.Controls.Add(this.m_lblUseCardOrScanReceipt);
            this.m_panelUseCardScanReceiptOrScanPaper.GradientBeginColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(179)))), ((int)(((byte)(213)))));
            this.m_panelUseCardScanReceiptOrScanPaper.GradientEndColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(186)))), ((int)(((byte)(192)))));
            this.m_panelUseCardScanReceiptOrScanPaper.InnerBorderEdgeColor = System.Drawing.Color.SlateGray;
            this.m_panelUseCardScanReceiptOrScanPaper.Location = new System.Drawing.Point(64, 94);
            this.m_panelUseCardScanReceiptOrScanPaper.Name = "m_panelUseCardScanReceiptOrScanPaper";
            this.m_panelUseCardScanReceiptOrScanPaper.OuterBorderEdgeColor = System.Drawing.Color.SlateGray;
            this.m_panelUseCardScanReceiptOrScanPaper.Size = new System.Drawing.Size(896, 589);
            this.m_panelUseCardScanReceiptOrScanPaper.TabIndex = 0;
            // 
            // m_picUseCardOrScanReceipt
            // 
            this.m_picUseCardOrScanReceipt.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.m_picUseCardOrScanReceipt.BackColor = System.Drawing.Color.Transparent;
            this.m_picUseCardOrScanReceipt.Image = global::GTI.Modules.POS.Properties.Resources.AnimatedPlayerCard_large_;
            this.m_picUseCardOrScanReceipt.Location = new System.Drawing.Point(527, 118);
            this.m_picUseCardOrScanReceipt.Name = "m_picUseCardOrScanReceipt";
            this.m_picUseCardOrScanReceipt.Size = new System.Drawing.Size(366, 352);
            this.m_picUseCardOrScanReceipt.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.m_picUseCardOrScanReceipt.TabIndex = 0;
            this.m_picUseCardOrScanReceipt.TabStop = false;
            // 
            // m_lblUseCardOrScanReceipt
            // 
            this.m_lblUseCardOrScanReceipt.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.m_lblUseCardOrScanReceipt.Font = new System.Drawing.Font("Tahoma", 32.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_lblUseCardOrScanReceipt.ForeColor = System.Drawing.Color.Yellow;
            this.m_lblUseCardOrScanReceipt.Location = new System.Drawing.Point(3, 0);
            this.m_lblUseCardOrScanReceipt.Name = "m_lblUseCardOrScanReceipt";
            this.m_lblUseCardOrScanReceipt.Size = new System.Drawing.Size(518, 589);
            this.m_lblUseCardOrScanReceipt.TabIndex = 1;
            this.m_lblUseCardOrScanReceipt.Text = "Use your player card\r\nto start buying\r\nBINGO packs!";
            this.m_lblUseCardOrScanReceipt.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.m_lblUseCardOrScanReceipt.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.m_lblUseCardOrScanReceipt_MouseDoubleClick);
            this.m_lblUseCardOrScanReceipt.MouseUp += new System.Windows.Forms.MouseEventHandler(this.m_lblUseCardOrScanReceipt_MouseUp);
            // 
            // m_lblWorking
            // 
            this.m_lblWorking.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.m_lblWorking.BackColor = System.Drawing.Color.Transparent;
            this.m_lblWorking.EdgeColor = System.Drawing.Color.Chartreuse;
            this.m_lblWorking.Font = new System.Drawing.Font("Tahoma", 48F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_lblWorking.ForeColor = System.Drawing.Color.MediumBlue;
            this.m_lblWorking.Location = new System.Drawing.Point(222, 75);
            this.m_lblWorking.Name = "m_lblWorking";
            this.m_lblWorking.Size = new System.Drawing.Size(580, 176);
            this.m_lblWorking.TabIndex = 2;
            this.m_lblWorking.Text = "Processing...";
            this.m_lblWorking.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // m_picLogo
            // 
            this.m_picLogo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.m_picLogo.BackColor = System.Drawing.SystemColors.Control;
            this.m_picLogo.BackgroundImage = global::GTI.Modules.POS.Properties.Resources.GameTechLogoForKiosk;
            this.m_picLogo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.m_picLogo.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.m_picLogo.Location = new System.Drawing.Point(547, 15);
            this.m_picLogo.Name = "m_picLogo";
            this.m_picLogo.Size = new System.Drawing.Size(440, 48);
            this.m_picLogo.TabIndex = 30;
            this.m_picLogo.TabStop = false;
            this.m_picLogo.Visible = false;
            // 
            // m_picCBBPrintingLogo
            // 
            this.m_picCBBPrintingLogo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.m_picCBBPrintingLogo.BackColor = System.Drawing.Color.Transparent;
            this.m_picCBBPrintingLogo.Image = global::GTI.Modules.POS.Properties.Resources.CBBKioskLogo;
            this.m_picCBBPrintingLogo.Location = new System.Drawing.Point(64, 680);
            this.m_picCBBPrintingLogo.Name = "m_picCBBPrintingLogo";
            this.m_picCBBPrintingLogo.Size = new System.Drawing.Size(249, 85);
            this.m_picCBBPrintingLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.m_picCBBPrintingLogo.TabIndex = 27;
            this.m_picCBBPrintingLogo.TabStop = false;
            this.m_picCBBPrintingLogo.Visible = false;
            this.m_picCBBPrintingLogo.Click += new System.EventHandler(this.UserActivityDetected);
            // 
            // m_panelB3OrBingo
            // 
            this.m_panelB3OrBingo.BackColor = System.Drawing.Color.Transparent;
            this.m_panelB3OrBingo.BorderColor = System.Drawing.Color.Silver;
            this.m_panelB3OrBingo.Controls.Add(this.m_lblB3OrBingoGreeting);
            this.m_panelB3OrBingo.Controls.Add(this.m_btnBingo);
            this.m_panelB3OrBingo.Controls.Add(this.m_btnB3);
            this.m_panelB3OrBingo.GradientBeginColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(179)))), ((int)(((byte)(213)))));
            this.m_panelB3OrBingo.GradientEndColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(186)))), ((int)(((byte)(192)))));
            this.m_panelB3OrBingo.InnerBorderEdgeColor = System.Drawing.Color.SlateGray;
            this.m_panelB3OrBingo.Location = new System.Drawing.Point(13, 13);
            this.m_panelB3OrBingo.Name = "m_panelB3OrBingo";
            this.m_panelB3OrBingo.OuterBorderEdgeColor = System.Drawing.Color.SlateGray;
            this.m_panelB3OrBingo.Size = new System.Drawing.Size(999, 660);
            this.m_panelB3OrBingo.TabIndex = 27;
            this.m_panelB3OrBingo.Click += new System.EventHandler(this.UserActivityDetected);
            // 
            // m_lblB3OrBingoGreeting
            // 
            this.m_lblB3OrBingoGreeting.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_lblB3OrBingoGreeting.EdgeColor = System.Drawing.Color.White;
            this.m_lblB3OrBingoGreeting.Font = new System.Drawing.Font("Tahoma", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_lblB3OrBingoGreeting.ForeColor = System.Drawing.Color.Navy;
            this.m_lblB3OrBingoGreeting.Location = new System.Drawing.Point(57, 11);
            this.m_lblB3OrBingoGreeting.Name = "m_lblB3OrBingoGreeting";
            this.m_lblB3OrBingoGreeting.Size = new System.Drawing.Size(884, 33);
            this.m_lblB3OrBingoGreeting.TabIndex = 27;
            this.m_lblB3OrBingoGreeting.Text = "Welcome back Richard";
            this.m_lblB3OrBingoGreeting.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.m_lblB3OrBingoGreeting.Visible = false;
            // 
            // m_btnBingo
            // 
            this.m_btnBingo.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.m_btnBingo.BackColor = System.Drawing.Color.Transparent;
            this.m_btnBingo.ContrastFor3D = 0;
            this.m_btnBingo.DrawSecondaryTextAsOutlined = true;
            this.m_btnBingo.FitImageIcon = true;
            this.m_btnBingo.FitSecondaryText = true;
            this.m_btnBingo.FittedImageIconSize = 98;
            this.m_btnBingo.FocusColor = System.Drawing.Color.Black;
            this.m_btnBingo.Font = new System.Drawing.Font("Microsoft Sans Serif", 48F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_btnBingo.ForeColor = System.Drawing.Color.Yellow;
            this.m_btnBingo.ImageIcon = global::GTI.Modules.POS.Properties.Resources.KioskBingoButton;
            this.m_btnBingo.ImageNormal = global::GTI.Modules.POS.Properties.Resources.WhiteFlatButtonUp;
            this.m_btnBingo.ImagePressed = global::GTI.Modules.POS.Properties.Resources.WhiteFlatButtonDown;
            this.m_btnBingo.Location = new System.Drawing.Point(506, 64);
            this.m_btnBingo.MinimumSize = new System.Drawing.Size(30, 30);
            this.m_btnBingo.Name = "m_btnBingo";
            this.m_btnBingo.OutlineColor = System.Drawing.Color.Black;
            this.m_btnBingo.SecondaryText = "PRESS FOR";
            this.m_btnBingo.SecondaryTextAlignment = System.Drawing.StringAlignment.Center;
            this.m_btnBingo.SecondaryTextLineAlignment = System.Drawing.StringAlignment.Near;
            this.m_btnBingo.SecondaryTextPadding = new System.Windows.Forms.Padding(50, 5, 50, 5);
            this.m_btnBingo.ShowFocus = false;
            this.m_btnBingo.Size = new System.Drawing.Size(490, 533);
            this.m_btnBingo.TabIndex = 29;
            this.m_btnBingo.UseSecondaryText = true;
            this.m_btnBingo.UseVisualStyleBackColor = false;
            this.m_btnBingo.Click += new System.EventHandler(this.m_btnBingo_Click);
            // 
            // m_btnB3
            // 
            this.m_btnB3.BackColor = System.Drawing.Color.Transparent;
            this.m_btnB3.ContrastFor3D = 0;
            this.m_btnB3.DrawSecondaryTextAsOutlined = true;
            this.m_btnB3.FitImageIcon = true;
            this.m_btnB3.FitSecondaryText = true;
            this.m_btnB3.FittedImageIconSize = 98;
            this.m_btnB3.FocusColor = System.Drawing.Color.Black;
            this.m_btnB3.Font = new System.Drawing.Font("Microsoft Sans Serif", 48F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_btnB3.ForeColor = System.Drawing.Color.Yellow;
            this.m_btnB3.ImageIcon = global::GTI.Modules.POS.Properties.Resources.B3KioskButton;
            this.m_btnB3.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueFlatButtonUp;
            this.m_btnB3.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlackFlatButtonDown;
            this.m_btnB3.Location = new System.Drawing.Point(3, 64);
            this.m_btnB3.MinimumSize = new System.Drawing.Size(30, 30);
            this.m_btnB3.Name = "m_btnB3";
            this.m_btnB3.OutlineColor = System.Drawing.Color.Black;
            this.m_btnB3.SecondaryText = "PRESS FOR";
            this.m_btnB3.SecondaryTextAlignment = System.Drawing.StringAlignment.Center;
            this.m_btnB3.SecondaryTextLineAlignment = System.Drawing.StringAlignment.Near;
            this.m_btnB3.SecondaryTextPadding = new System.Windows.Forms.Padding(50, 5, 50, 5);
            this.m_btnB3.ShowFocus = false;
            this.m_btnB3.Size = new System.Drawing.Size(490, 533);
            this.m_btnB3.TabIndex = 28;
            this.m_btnB3.UseSecondaryText = true;
            this.m_btnB3.UseVisualStyleBackColor = false;
            this.m_btnB3.Click += new System.EventHandler(this.m_btnB3_Click);
            // 
            // m_olblTestMode
            // 
            this.m_olblTestMode.BackColor = System.Drawing.Color.Transparent;
            this.m_olblTestMode.DrawAsInset3D = true;
            this.m_olblTestMode.EdgeColor = System.Drawing.Color.Black;
            this.m_olblTestMode.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_olblTestMode.Location = new System.Drawing.Point(10, 10);
            this.m_olblTestMode.Name = "m_olblTestMode";
            this.m_olblTestMode.Size = new System.Drawing.Size(179, 32);
            this.m_olblTestMode.TabIndex = 27;
            this.m_olblTestMode.Text = "TEST MODE";
            this.m_olblTestMode.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.m_olblTestMode.Visible = false;
            // 
            // m_testModeTimer
            // 
            this.m_testModeTimer.Interval = 1000;
            this.m_testModeTimer.Tick += new System.EventHandler(this.m_testModeTimer_Tick);
            // 
            // m_burnInPreventionTimer
            // 
            this.m_burnInPreventionTimer.Interval = 600000;
            this.m_burnInPreventionTimer.Tick += new System.EventHandler(this.m_burnInPreventionTimer_Tick);
            // 
            // SimpleKioskForm
            // 
            this.BackColor = System.Drawing.Color.Black;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.BorderColor = System.Drawing.Color.Chartreuse;
            this.ClientSize = new System.Drawing.Size(1024, 768);
            this.Controls.Add(this.m_panelVideo);
            this.Controls.Add(this.m_panelMain);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.GradientBeginColor = System.Drawing.Color.Chartreuse;
            this.GradientEndColor = System.Drawing.Color.Gold;
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(1024, 768);
            this.Name = "SimpleKioskForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SimpleKioskForm_FormClosing);
            this.LocationChanged += new System.EventHandler(this.SimpleKioskForm_LocationChanged);
            this.Click += new System.EventHandler(this.UserActivityDetected);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.SimpleKioskForm_KeyDown);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.KeyPressed);
            this.m_panelVideo.ResumeLayout(false);
            this.m_panelVideo2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.m_axWindowsMediaPlayer)).EndInit();
            this.m_panelMain.ResumeLayout(false);
            this.m_panelReceipt.ResumeLayout(false);
            this.m_panelUseCardScanReceiptOrScanPaper.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.m_picUseCardOrScanReceipt)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_picLogo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_picCBBPrintingLogo)).EndInit();
            this.m_panelB3OrBingo.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private GTI.Controls.EliteGradientPanel m_panelUseCardScanReceiptOrScanPaper;
        private System.Windows.Forms.Label m_lblUseCardOrScanReceipt;
        private System.Windows.Forms.PictureBox m_picUseCardOrScanReceipt;
        private Controls.ImageButton m_btnBuy;
        private Controls.ImageButton m_btnQuit;
        private System.Windows.Forms.Label m_lblTotalLabel;
        private System.Windows.Forms.Label m_lblTotal;
        private GTI.Controls.EliteGradientPanel m_panelReceipt;
        private System.Windows.Forms.ListBox m_lbReceipt;
        private GTI.Controls.OutlinedLabel m_lblWelcomeBack;
        private Controls.ImageButton m_btnCoupons;
        private GTI.Controls.EliteGradientPanel m_panelKioskMenuButtons;
        private Controls.ImageButton m_btnDevice;
        private Controls.ImageButton m_btnHelp;
        private Controls.OutlinedLabel m_lblWorking;
        private Controls.ImageButton m_btnReceiptHelp;
        private Controls.ImageButton m_btnPaperHelp;
        private System.Windows.Forms.ProgressBar m_simpleKioskProgress;
        private System.Windows.Forms.Timer m_idleTimerForVideo;
        private GTI.Controls.EliteGradientPanel m_panelVideo;
        private System.Windows.Forms.Label m_lblVideoPrompt;
        private GTI.Controls.EliteGradientPanel m_panelVideo2;
        private Controls.OutlinedLabel m_lblAutoCoupons;
        private Controls.OutlinedLabel m_lblHybridAutoCoupons;
        private Controls.ImageButton m_btnNoPlayerCard;
        private AxWMPLib.AxWindowsMediaPlayer m_axWindowsMediaPlayer;
        private Controls.EliteGradientPanel m_panelMain;
        private System.Windows.Forms.PictureBox m_picLogo;
        private Controls.EliteGradientPanel m_panelB3OrBingo;
        private Controls.ImageButton m_btnB3;
        private Controls.ImageButton m_btnBingo;
        private Controls.OutlinedLabel m_lblB3OrBingoGreeting;
        private System.Windows.Forms.PictureBox m_picCBBPrintingLogo;
        private Controls.OutlinedLabel m_olblTestMode;
        private System.Windows.Forms.Timer m_testModeTimer;
        private System.Windows.Forms.Timer m_burnInPreventionTimer;
    }
}
