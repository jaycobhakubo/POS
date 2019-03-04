namespace GTI.Modules.POS.UI
{
    partial class FlexTenderingForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FlexTenderingForm));
            this.m_kioskIdleTimer = new System.Windows.Forms.Timer(this.components);
            this.m_kioskCardInstructionsFlashTimer = new System.Windows.Forms.Timer(this.components);
            this.m_panelMain = new GTI.Controls.EliteGradientPanel();
            this.m_btnAdvTest1 = new GTI.Controls.ImageButton();
            this.m_btnAdvTest10 = new GTI.Controls.ImageButton();
            this.m_btnAdvTest5 = new GTI.Controls.ImageButton();
            this.m_btnHelp = new GTI.Controls.ImageButton();
            this.m_timeoutProgress = new System.Windows.Forms.ProgressBar();
            this.m_btnDevice = new GTI.Controls.ImageButton();
            this.m_lblKioskPlayingOn = new System.Windows.Forms.Label();
            this.m_btnCancelSale = new GTI.Controls.ImageButton();
            this.m_btnContinueSale = new GTI.Controls.ImageButton();
            this.m_btnCurrency = new GTI.Controls.ImageButton();
            this.m_btnFinishTender = new GTI.Controls.ImageButton();
            this.m_lblExchangeRate = new System.Windows.Forms.Label();
            this.m_lblPlayerPoints = new System.Windows.Forms.Label();
            this.m_lblPlayerName = new System.Windows.Forms.Label();
            this.m_keypad = new GTI.Controls.Keypad();
            this.m_btnSwapLeftRightHanded = new GTI.Controls.ImageButton();
            this.m_btnCancelTendering = new GTI.Controls.ImageButton();
            this.m_tenderButtonMenu = new GTI.Controls.ButtonMenu();
            this.m_buttonRemoveLine = new GTI.Controls.ImageButton();
            this.m_dueLabel = new System.Windows.Forms.Label();
            this.m_dueAmountLabel = new System.Windows.Forms.Label();
            this.m_buttonScrollDown = new GTI.Controls.ImageButton();
            this.m_buttonScrollUp = new GTI.Controls.ImageButton();
            this.m_statusTextbox = new System.Windows.Forms.TextBox();
            this.m_tendersPanel = new System.Windows.Forms.Panel();
            this.m_tendersList = new GTI.Controls.ColorListBox();
            this.m_panelKiosk = new GTI.Controls.EliteGradientPanel();
            this.m_picCreditCardDevice = new System.Windows.Forms.PictureBox();
            this.m_btnTest10 = new GTI.Controls.ImageButton();
            this.m_btnTest5 = new GTI.Controls.ImageButton();
            this.m_btnTest1 = new GTI.Controls.ImageButton();
            this.m_simpleKioskProgress = new System.Windows.Forms.ProgressBar();
            this.m_btnSimpleKioskHelp = new GTI.Controls.ImageButton();
            this.m_lblKioskCardInstructions = new GTI.Controls.OutlinedLabel();
            this.m_btnKioskQuit = new GTI.Controls.ImageButton();
            this.m_lblKioskInstructions = new GTI.Controls.OutlinedLabel();
            this.m_lblKioskTotalDue = new System.Windows.Forms.Label();
            this.m_btnKioskCard = new GTI.Controls.ImageButton();
            this.m_picLogo = new System.Windows.Forms.PictureBox();
            this.m_panelMain.SuspendLayout();
            this.m_tendersPanel.SuspendLayout();
            this.m_panelKiosk.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.m_picCreditCardDevice)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_picLogo)).BeginInit();
            this.SuspendLayout();
            // 
            // m_kioskIdleTimer
            // 
            this.m_kioskIdleTimer.Interval = 500;
            this.m_kioskIdleTimer.Tick += new System.EventHandler(this.m_kioskIdleTimer_Tick);
            // 
            // m_kioskCardInstructionsFlashTimer
            // 
            this.m_kioskCardInstructionsFlashTimer.Interval = 250;
            this.m_kioskCardInstructionsFlashTimer.Tick += new System.EventHandler(this.m_kioskCardInstructionsFlashTimer_Tick);
            // 
            // m_panelMain
            // 
            resources.ApplyResources(this.m_panelMain, "m_panelMain");
            this.m_panelMain.BackColor = System.Drawing.SystemColors.Control;
            this.m_panelMain.BackgroundImage = global::GTI.Modules.POS.Properties.Resources.SplitTenderingBackBig2;
            this.m_panelMain.BorderColor = System.Drawing.Color.Silver;
            this.m_panelMain.Controls.Add(this.m_btnAdvTest1);
            this.m_panelMain.Controls.Add(this.m_btnAdvTest10);
            this.m_panelMain.Controls.Add(this.m_btnAdvTest5);
            this.m_panelMain.Controls.Add(this.m_btnHelp);
            this.m_panelMain.Controls.Add(this.m_timeoutProgress);
            this.m_panelMain.Controls.Add(this.m_btnDevice);
            this.m_panelMain.Controls.Add(this.m_lblKioskPlayingOn);
            this.m_panelMain.Controls.Add(this.m_btnCancelSale);
            this.m_panelMain.Controls.Add(this.m_btnContinueSale);
            this.m_panelMain.Controls.Add(this.m_btnCurrency);
            this.m_panelMain.Controls.Add(this.m_btnFinishTender);
            this.m_panelMain.Controls.Add(this.m_lblExchangeRate);
            this.m_panelMain.Controls.Add(this.m_lblPlayerPoints);
            this.m_panelMain.Controls.Add(this.m_lblPlayerName);
            this.m_panelMain.Controls.Add(this.m_keypad);
            this.m_panelMain.Controls.Add(this.m_btnSwapLeftRightHanded);
            this.m_panelMain.Controls.Add(this.m_btnCancelTendering);
            this.m_panelMain.Controls.Add(this.m_tenderButtonMenu);
            this.m_panelMain.Controls.Add(this.m_buttonRemoveLine);
            this.m_panelMain.Controls.Add(this.m_dueLabel);
            this.m_panelMain.Controls.Add(this.m_dueAmountLabel);
            this.m_panelMain.Controls.Add(this.m_buttonScrollDown);
            this.m_panelMain.Controls.Add(this.m_buttonScrollUp);
            this.m_panelMain.Controls.Add(this.m_statusTextbox);
            this.m_panelMain.Controls.Add(this.m_tendersPanel);
            this.m_panelMain.DrawBorderOuterEdge = true;
            this.m_panelMain.DrawRounded = true;
            this.m_panelMain.GradientBeginColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(179)))), ((int)(((byte)(213)))));
            this.m_panelMain.GradientEndColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(186)))), ((int)(((byte)(192)))));
            this.m_panelMain.InnerBorderEdgeColor = System.Drawing.Color.SlateGray;
            this.m_panelMain.Name = "m_panelMain";
            this.m_panelMain.OuterBorderEdgeColor = System.Drawing.Color.SlateGray;
            this.m_panelMain.Click += new System.EventHandler(this.UserActivityDetected);
            // 
            // m_btnAdvTest1
            // 
            resources.ApplyResources(this.m_btnAdvTest1, "m_btnAdvTest1");
            this.m_btnAdvTest1.BackColor = System.Drawing.Color.Transparent;
            this.m_btnAdvTest1.DebounceThreshold = 0;
            this.m_btnAdvTest1.FocusColor = System.Drawing.Color.Black;
            this.m_btnAdvTest1.ForeColor = System.Drawing.Color.MidnightBlue;
            this.m_btnAdvTest1.ImageNormal = global::GTI.Modules.POS.Properties.Resources.GreenButtonUp;
            this.m_btnAdvTest1.ImagePressed = global::GTI.Modules.POS.Properties.Resources.GreenButtonDown;
            this.m_btnAdvTest1.Name = "m_btnAdvTest1";
            this.m_btnAdvTest1.SecondaryTextPadding = new System.Windows.Forms.Padding(0);
            this.m_btnAdvTest1.UseVisualStyleBackColor = false;
            this.m_btnAdvTest1.Click += new System.EventHandler(this.m_btnTestX_Click);
            // 
            // m_btnAdvTest10
            // 
            resources.ApplyResources(this.m_btnAdvTest10, "m_btnAdvTest10");
            this.m_btnAdvTest10.BackColor = System.Drawing.Color.Transparent;
            this.m_btnAdvTest10.DebounceThreshold = 0;
            this.m_btnAdvTest10.FocusColor = System.Drawing.Color.Black;
            this.m_btnAdvTest10.ForeColor = System.Drawing.Color.MidnightBlue;
            this.m_btnAdvTest10.ImageNormal = global::GTI.Modules.POS.Properties.Resources.GreenButtonUp;
            this.m_btnAdvTest10.ImagePressed = global::GTI.Modules.POS.Properties.Resources.GreenButtonDown;
            this.m_btnAdvTest10.Name = "m_btnAdvTest10";
            this.m_btnAdvTest10.SecondaryTextPadding = new System.Windows.Forms.Padding(0);
            this.m_btnAdvTest10.UseVisualStyleBackColor = false;
            this.m_btnAdvTest10.Click += new System.EventHandler(this.m_btnTestX_Click);
            // 
            // m_btnAdvTest5
            // 
            resources.ApplyResources(this.m_btnAdvTest5, "m_btnAdvTest5");
            this.m_btnAdvTest5.BackColor = System.Drawing.Color.Transparent;
            this.m_btnAdvTest5.DebounceThreshold = 0;
            this.m_btnAdvTest5.FocusColor = System.Drawing.Color.Black;
            this.m_btnAdvTest5.ForeColor = System.Drawing.Color.MidnightBlue;
            this.m_btnAdvTest5.ImageNormal = global::GTI.Modules.POS.Properties.Resources.GreenButtonUp;
            this.m_btnAdvTest5.ImagePressed = global::GTI.Modules.POS.Properties.Resources.GreenButtonDown;
            this.m_btnAdvTest5.Name = "m_btnAdvTest5";
            this.m_btnAdvTest5.SecondaryTextPadding = new System.Windows.Forms.Padding(0);
            this.m_btnAdvTest5.UseVisualStyleBackColor = false;
            this.m_btnAdvTest5.Click += new System.EventHandler(this.m_btnTestX_Click);
            // 
            // m_btnHelp
            // 
            this.m_btnHelp.BackColor = System.Drawing.Color.Transparent;
            this.m_btnHelp.DebounceThreshold = 0;
            this.m_btnHelp.FocusColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.m_btnHelp, "m_btnHelp");
            this.m_btnHelp.ImageNormal = global::GTI.Modules.POS.Properties.Resources.PurpleButtonUp;
            this.m_btnHelp.ImagePressed = global::GTI.Modules.POS.Properties.Resources.PurpleButtonDown;
            this.m_btnHelp.Name = "m_btnHelp";
            this.m_btnHelp.SecondaryTextPadding = new System.Windows.Forms.Padding(0);
            this.m_btnHelp.ShowFocus = false;
            this.m_btnHelp.UseVisualStyleBackColor = false;
            this.m_btnHelp.Click += new System.EventHandler(this.m_btnHelp_Click);
            // 
            // m_timeoutProgress
            // 
            this.m_timeoutProgress.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(127)))), ((int)(((byte)(127)))));
            this.m_timeoutProgress.ForeColor = System.Drawing.Color.Gold;
            resources.ApplyResources(this.m_timeoutProgress, "m_timeoutProgress");
            this.m_timeoutProgress.Name = "m_timeoutProgress";
            // 
            // m_btnDevice
            // 
            this.m_btnDevice.Alignment = System.Drawing.StringAlignment.Near;
            this.m_btnDevice.BackColor = System.Drawing.Color.Transparent;
            this.m_btnDevice.DebounceThreshold = 0;
            this.m_btnDevice.FocusColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.m_btnDevice, "m_btnDevice");
            this.m_btnDevice.ImageNormal = global::GTI.Modules.POS.Properties.Resources.DeviceExplorerUp271;
            this.m_btnDevice.LineAlignment = System.Drawing.StringAlignment.Near;
            this.m_btnDevice.Name = "m_btnDevice";
            this.m_btnDevice.SecondaryTextPadding = new System.Windows.Forms.Padding(0);
            this.m_btnDevice.ShowFocus = false;
            this.m_btnDevice.UseVisualStyleBackColor = false;
            this.m_btnDevice.Click += new System.EventHandler(this.ScollDevice);
            // 
            // m_lblKioskPlayingOn
            // 
            this.m_lblKioskPlayingOn.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.m_lblKioskPlayingOn, "m_lblKioskPlayingOn");
            this.m_lblKioskPlayingOn.ForeColor = System.Drawing.Color.Black;
            this.m_lblKioskPlayingOn.Name = "m_lblKioskPlayingOn";
            // 
            // m_btnCancelSale
            // 
            this.m_btnCancelSale.BackColor = System.Drawing.Color.Transparent;
            this.m_btnCancelSale.DebounceThreshold = 0;
            this.m_btnCancelSale.FocusColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.m_btnCancelSale, "m_btnCancelSale");
            this.m_btnCancelSale.ImageNormal = global::GTI.Modules.POS.Properties.Resources.RedButtonUp;
            this.m_btnCancelSale.ImagePressed = global::GTI.Modules.POS.Properties.Resources.RedButtonDown;
            this.m_btnCancelSale.Name = "m_btnCancelSale";
            this.m_btnCancelSale.SecondaryTextPadding = new System.Windows.Forms.Padding(0);
            this.m_btnCancelSale.ShowFocus = false;
            this.m_btnCancelSale.UseVisualStyleBackColor = false;
            this.m_btnCancelSale.Click += new System.EventHandler(this.m_btnCancelSale_Click);
            // 
            // m_btnContinueSale
            // 
            this.m_btnContinueSale.BackColor = System.Drawing.Color.Transparent;
            this.m_btnContinueSale.DebounceThreshold = 0;
            this.m_btnContinueSale.FocusColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.m_btnContinueSale, "m_btnContinueSale");
            this.m_btnContinueSale.ImageNormal = global::GTI.Modules.POS.Properties.Resources.DarkOrangeButtonUp;
            this.m_btnContinueSale.ImagePressed = global::GTI.Modules.POS.Properties.Resources.DarkOrangeButtonDown;
            this.m_btnContinueSale.Name = "m_btnContinueSale";
            this.m_btnContinueSale.SecondaryTextPadding = new System.Windows.Forms.Padding(0);
            this.m_btnContinueSale.ShowFocus = false;
            this.m_btnContinueSale.UseVisualStyleBackColor = false;
            this.m_btnContinueSale.Click += new System.EventHandler(this.m_btnCancelTendering_Click);
            // 
            // m_btnCurrency
            // 
            this.m_btnCurrency.BackColor = System.Drawing.Color.Transparent;
            this.m_btnCurrency.DebounceThreshold = 0;
            this.m_btnCurrency.FocusColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.m_btnCurrency, "m_btnCurrency");
            this.m_btnCurrency.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_btnCurrency.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_btnCurrency.Name = "m_btnCurrency";
            this.m_btnCurrency.SecondaryTextPadding = new System.Windows.Forms.Padding(0);
            this.m_btnCurrency.ShowFocus = false;
            this.m_btnCurrency.UseVisualStyleBackColor = false;
            this.m_btnCurrency.Click += new System.EventHandler(this.m_btnCurrency_Click);
            // 
            // m_btnFinishTender
            // 
            this.m_btnFinishTender.BackColor = System.Drawing.Color.Transparent;
            this.m_btnFinishTender.Debounce = true;
            this.m_btnFinishTender.FocusColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.m_btnFinishTender, "m_btnFinishTender");
            this.m_btnFinishTender.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_btnFinishTender.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_btnFinishTender.Name = "m_btnFinishTender";
            this.m_btnFinishTender.SecondaryTextPadding = new System.Windows.Forms.Padding(0);
            this.m_btnFinishTender.ShowFocus = false;
            this.m_btnFinishTender.UseVisualStyleBackColor = false;
            this.m_btnFinishTender.Click += new System.EventHandler(this.m_btnFinishTender_Click);
            // 
            // m_lblExchangeRate
            // 
            resources.ApplyResources(this.m_lblExchangeRate, "m_lblExchangeRate");
            this.m_lblExchangeRate.BackColor = System.Drawing.Color.Transparent;
            this.m_lblExchangeRate.Name = "m_lblExchangeRate";
            // 
            // m_lblPlayerPoints
            // 
            resources.ApplyResources(this.m_lblPlayerPoints, "m_lblPlayerPoints");
            this.m_lblPlayerPoints.BackColor = System.Drawing.Color.Transparent;
            this.m_lblPlayerPoints.Name = "m_lblPlayerPoints";
            // 
            // m_lblPlayerName
            // 
            resources.ApplyResources(this.m_lblPlayerName, "m_lblPlayerName");
            this.m_lblPlayerName.BackColor = System.Drawing.Color.Transparent;
            this.m_lblPlayerName.Name = "m_lblPlayerName";
            // 
            // m_keypad
            // 
            this.m_keypad.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(117)))), ((int)(((byte)(104)))), ((int)(((byte)(99)))));
            resources.ApplyResources(this.m_keypad, "m_keypad");
            this.m_keypad.BigButtonEnabled = false;
            this.m_keypad.BigButtonFont = new System.Drawing.Font("Tahoma", 24F, System.Drawing.FontStyle.Bold);
            this.m_keypad.BigButtonImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_keypad.BigButtonImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_keypad.BigButtonStretch = false;
            this.m_keypad.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.m_keypad.ClearKeyFont = new System.Drawing.Font("Tahoma", 24F, System.Drawing.FontStyle.Bold);
            this.m_keypad.CurrencySymbolForeColor = System.Drawing.Color.White;
            this.m_keypad.InitialValue = null;
            this.m_keypad.KeyMode = GTI.Controls.Keypad.KeypadMode.Calculator;
            this.m_keypad.Name = "m_keypad";
            this.m_keypad.NumbersFont = new System.Drawing.Font("Tahoma", 24F, System.Drawing.FontStyle.Bold);
            this.m_keypad.NumbersImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_keypad.NumbersImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_keypad.Option1Enabled = false;
            this.m_keypad.Option1Stretch = false;
            this.m_keypad.Option1Visible = false;
            this.m_keypad.Option2Enabled = false;
            this.m_keypad.Option2Stretch = false;
            this.m_keypad.Option2Visible = false;
            this.m_keypad.Option3Enabled = false;
            this.m_keypad.Option3Stretch = false;
            this.m_keypad.Option3Visible = false;
            this.m_keypad.Option4Enabled = false;
            this.m_keypad.Option4Stretch = false;
            this.m_keypad.Option4Visible = false;
            this.m_keypad.OptionButtonsFont = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
            this.m_keypad.OptionButtonsPadding = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.m_keypad.ShowFocus = false;
            this.m_keypad.ShowOptionButtons = true;
            this.m_keypad.TabStop = false;
            this.m_keypad.Value = new decimal(new int[] {
            0,
            0,
            0,
            131072});
            this.m_keypad.ValueBackground = global::GTI.Modules.POS.Properties.Resources.TextBack;
            this.m_keypad.ValueForeColor = System.Drawing.Color.Yellow;
            this.m_keypad.ValueChanged += new System.EventHandler(this.FlexTenderingForm_Click);
            this.m_keypad.Click += new System.EventHandler(this.m_keypad_Click);
            // 
            // m_btnSwapLeftRightHanded
            // 
            this.m_btnSwapLeftRightHanded.BackColor = System.Drawing.Color.Transparent;
            this.m_btnSwapLeftRightHanded.FocusColor = System.Drawing.Color.Black;
            this.m_btnSwapLeftRightHanded.ImageNormal = ((System.Drawing.Image)(resources.GetObject("m_btnSwapLeftRightHanded.ImageNormal")));
            this.m_btnSwapLeftRightHanded.ImagePressed = ((System.Drawing.Image)(resources.GetObject("m_btnSwapLeftRightHanded.ImagePressed")));
            resources.ApplyResources(this.m_btnSwapLeftRightHanded, "m_btnSwapLeftRightHanded");
            this.m_btnSwapLeftRightHanded.Name = "m_btnSwapLeftRightHanded";
            this.m_btnSwapLeftRightHanded.SecondaryTextPadding = new System.Windows.Forms.Padding(0);
            this.m_btnSwapLeftRightHanded.ShowFocus = false;
            this.m_btnSwapLeftRightHanded.UseVisualStyleBackColor = false;
            this.m_btnSwapLeftRightHanded.Click += new System.EventHandler(this.m_btnSwapLeftRightHanded_Click);
            // 
            // m_btnCancelTendering
            // 
            this.m_btnCancelTendering.BackColor = System.Drawing.Color.Transparent;
            this.m_btnCancelTendering.Debounce = true;
            this.m_btnCancelTendering.FocusColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.m_btnCancelTendering, "m_btnCancelTendering");
            this.m_btnCancelTendering.ImageIcon = global::GTI.Modules.POS.Properties.Resources.BackSymbol;
            this.m_btnCancelTendering.ImageNormal = global::GTI.Modules.POS.Properties.Resources.DarkOrangeButtonUp;
            this.m_btnCancelTendering.ImagePressed = global::GTI.Modules.POS.Properties.Resources.DarkOrangeButtonDown;
            this.m_btnCancelTendering.Name = "m_btnCancelTendering";
            this.m_btnCancelTendering.SecondaryTextPadding = new System.Windows.Forms.Padding(0);
            this.m_btnCancelTendering.ShowFocus = false;
            this.m_btnCancelTendering.UseVisualStyleBackColor = false;
            this.m_btnCancelTendering.Click += new System.EventHandler(this.m_btnCancelTendering_Click);
            // 
            // m_tenderButtonMenu
            // 
            this.m_tenderButtonMenu.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(117)))), ((int)(((byte)(104)))), ((int)(((byte)(99)))));
            resources.ApplyResources(this.m_tenderButtonMenu, "m_tenderButtonMenu");
            this.m_tenderButtonMenu.ButtonImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_tenderButtonMenu.ButtonImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_tenderButtonMenu.ButtonSize = new System.Drawing.Size(117, 60);
            this.m_tenderButtonMenu.FitIcons = true;
            this.m_tenderButtonMenu.ForeColor = System.Drawing.SystemColors.ControlText;
            this.m_tenderButtonMenu.HideDisabledButtons = false;
            this.m_tenderButtonMenu.HideEmptyButtons = true;
            this.m_tenderButtonMenu.IsGiantSized = false;
            this.m_tenderButtonMenu.IsMiniSized = false;
            this.m_tenderButtonMenu.Name = "m_tenderButtonMenu";
            this.m_tenderButtonMenu.NextButtonText = " ";
            this.m_tenderButtonMenu.NextImageIcon = global::GTI.Modules.POS.Properties.Resources.ArrowRight;
            this.m_tenderButtonMenu.NextImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_tenderButtonMenu.NextImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_tenderButtonMenu.PrevButtonText = " ";
            this.m_tenderButtonMenu.PrevImageIcon = global::GTI.Modules.POS.Properties.Resources.ArrowLeft;
            this.m_tenderButtonMenu.PrevImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_tenderButtonMenu.PrevImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_tenderButtonMenu.ShowFocus = false;
            this.m_tenderButtonMenu.Click += new System.EventHandler(this.m_tenderButtonMenu_Click);
            // 
            // m_buttonRemoveLine
            // 
            this.m_buttonRemoveLine.BackColor = System.Drawing.Color.Transparent;
            this.m_buttonRemoveLine.Debounce = true;
            resources.ApplyResources(this.m_buttonRemoveLine, "m_buttonRemoveLine");
            this.m_buttonRemoveLine.FocusColor = System.Drawing.Color.Black;
            this.m_buttonRemoveLine.ImageNormal = global::GTI.Modules.POS.Properties.Resources.RemoveLineUp;
            this.m_buttonRemoveLine.ImagePressed = global::GTI.Modules.POS.Properties.Resources.RemoveLineDown;
            this.m_buttonRemoveLine.Name = "m_buttonRemoveLine";
            this.m_buttonRemoveLine.SecondaryTextPadding = new System.Windows.Forms.Padding(0);
            this.m_buttonRemoveLine.ShowFocus = false;
            this.m_buttonRemoveLine.UseVisualStyleBackColor = false;
            this.m_buttonRemoveLine.Click += new System.EventHandler(this.m_buttonRemoveTender_Click);
            // 
            // m_dueLabel
            // 
            resources.ApplyResources(this.m_dueLabel, "m_dueLabel");
            this.m_dueLabel.BackColor = System.Drawing.Color.Transparent;
            this.m_dueLabel.Name = "m_dueLabel";
            // 
            // m_dueAmountLabel
            // 
            resources.ApplyResources(this.m_dueAmountLabel, "m_dueAmountLabel");
            this.m_dueAmountLabel.BackColor = System.Drawing.Color.Transparent;
            this.m_dueAmountLabel.ForeColor = System.Drawing.Color.Lime;
            this.m_dueAmountLabel.Name = "m_dueAmountLabel";
            this.m_dueAmountLabel.TextChanged += new System.EventHandler(this.m_dueAmountLabel_TextChanged);
            // 
            // m_buttonScrollDown
            // 
            this.m_buttonScrollDown.BackColor = System.Drawing.Color.Transparent;
            this.m_buttonScrollDown.DebounceThreshold = 0;
            this.m_buttonScrollDown.FocusColor = System.Drawing.Color.Black;
            this.m_buttonScrollDown.ForeColor = System.Drawing.SystemColors.ControlText;
            this.m_buttonScrollDown.ImageIcon = global::GTI.Modules.POS.Properties.Resources.ArrowDown;
            this.m_buttonScrollDown.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_buttonScrollDown.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            resources.ApplyResources(this.m_buttonScrollDown, "m_buttonScrollDown");
            this.m_buttonScrollDown.Name = "m_buttonScrollDown";
            this.m_buttonScrollDown.RepeatingIfHeld = true;
            this.m_buttonScrollDown.RepeatRate = 10;
            this.m_buttonScrollDown.SecondaryTextPadding = new System.Windows.Forms.Padding(0);
            this.m_buttonScrollDown.ShowFocus = false;
            this.m_buttonScrollDown.UseVisualStyleBackColor = false;
            this.m_buttonScrollDown.Click += new System.EventHandler(this.m_buttonScrollDown_Click);
            // 
            // m_buttonScrollUp
            // 
            this.m_buttonScrollUp.BackColor = System.Drawing.Color.Transparent;
            this.m_buttonScrollUp.DebounceThreshold = 0;
            this.m_buttonScrollUp.FocusColor = System.Drawing.Color.Black;
            this.m_buttonScrollUp.ImageIcon = global::GTI.Modules.POS.Properties.Resources.ArrowUp;
            this.m_buttonScrollUp.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_buttonScrollUp.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            resources.ApplyResources(this.m_buttonScrollUp, "m_buttonScrollUp");
            this.m_buttonScrollUp.Name = "m_buttonScrollUp";
            this.m_buttonScrollUp.RepeatingIfHeld = true;
            this.m_buttonScrollUp.RepeatRate = 10;
            this.m_buttonScrollUp.SecondaryTextPadding = new System.Windows.Forms.Padding(0);
            this.m_buttonScrollUp.ShowFocus = false;
            this.m_buttonScrollUp.UseVisualStyleBackColor = false;
            this.m_buttonScrollUp.Click += new System.EventHandler(this.m_buttonScrollUp_Click);
            // 
            // m_statusTextbox
            // 
            resources.ApplyResources(this.m_statusTextbox, "m_statusTextbox");
            this.m_statusTextbox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(19)))), ((int)(((byte)(60)))), ((int)(((byte)(96)))));
            this.m_statusTextbox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.m_statusTextbox.Cursor = System.Windows.Forms.Cursors.Default;
            this.m_statusTextbox.ForeColor = System.Drawing.Color.Yellow;
            this.m_statusTextbox.Name = "m_statusTextbox";
            this.m_statusTextbox.ReadOnly = true;
            this.m_statusTextbox.Enter += new System.EventHandler(this.m_statusTextbox_Enter);
            // 
            // m_tendersPanel
            // 
            this.m_tendersPanel.BackColor = System.Drawing.Color.White;
            this.m_tendersPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.m_tendersPanel.Controls.Add(this.m_tendersList);
            resources.ApplyResources(this.m_tendersPanel, "m_tendersPanel");
            this.m_tendersPanel.Name = "m_tendersPanel";
            // 
            // m_tendersList
            // 
            this.m_tendersList.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(19)))), ((int)(((byte)(60)))), ((int)(((byte)(96)))));
            this.m_tendersList.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.m_tendersList, "m_tendersList");
            this.m_tendersList.DownButton = this.m_buttonScrollDown;
            this.m_tendersList.DownIconBottomNotVisible = global::GTI.Modules.POS.Properties.Resources.ArrowDownRed;
            this.m_tendersList.DownIconBottomVisible = global::GTI.Modules.POS.Properties.Resources.ArrowDown;
            this.m_tendersList.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.m_tendersList.ForeColor = System.Drawing.Color.Yellow;
            this.m_tendersList.FormattingEnabled = true;
            this.m_tendersList.HighlightColor = System.Drawing.Color.ForestGreen;
            this.m_tendersList.ImageList = null;
            this.m_tendersList.Name = "m_tendersList";
            this.m_tendersList.SuppressVerticalScroll = true;
            this.m_tendersList.TopIndexForScroll = 0;
            this.m_tendersList.UpButton = this.m_buttonScrollUp;
            this.m_tendersList.UpIconTopNotVisible = global::GTI.Modules.POS.Properties.Resources.ArrowUpRed;
            this.m_tendersList.UpIconTopVisible = global::GTI.Modules.POS.Properties.Resources.ArrowUp;
            this.m_tendersList.UseOwnerDrawnMethod = true;
            this.m_tendersList.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.m_tendersList_DrawItem);
            this.m_tendersList.SelectedIndexChanged += new System.EventHandler(this.m_tendersList_SelectedIndexChanged);
            // 
            // m_panelKiosk
            // 
            resources.ApplyResources(this.m_panelKiosk, "m_panelKiosk");
            this.m_panelKiosk.BackColor = System.Drawing.SystemColors.Control;
            this.m_panelKiosk.BorderColor = System.Drawing.Color.Silver;
            this.m_panelKiosk.Controls.Add(this.m_picCreditCardDevice);
            this.m_panelKiosk.Controls.Add(this.m_btnTest10);
            this.m_panelKiosk.Controls.Add(this.m_btnTest5);
            this.m_panelKiosk.Controls.Add(this.m_btnTest1);
            this.m_panelKiosk.Controls.Add(this.m_simpleKioskProgress);
            this.m_panelKiosk.Controls.Add(this.m_btnSimpleKioskHelp);
            this.m_panelKiosk.Controls.Add(this.m_lblKioskCardInstructions);
            this.m_panelKiosk.Controls.Add(this.m_btnKioskQuit);
            this.m_panelKiosk.Controls.Add(this.m_lblKioskInstructions);
            this.m_panelKiosk.Controls.Add(this.m_lblKioskTotalDue);
            this.m_panelKiosk.Controls.Add(this.m_btnKioskCard);
            this.m_panelKiosk.Controls.Add(this.m_picLogo);
            this.m_panelKiosk.DrawAsGradient = true;
            this.m_panelKiosk.DrawBorderOuterEdge = true;
            this.m_panelKiosk.DrawRounded = true;
            this.m_panelKiosk.GradientBeginColor = System.Drawing.Color.FromArgb(((int)(((byte)(214)))), ((int)(((byte)(211)))), ((int)(((byte)(216)))));
            this.m_panelKiosk.GradientEndColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(127)))), ((int)(((byte)(127)))));
            this.m_panelKiosk.InnerBorderEdgeColor = System.Drawing.Color.SlateGray;
            this.m_panelKiosk.Name = "m_panelKiosk";
            this.m_panelKiosk.OuterBorderEdgeColor = System.Drawing.Color.SlateGray;
            this.m_panelKiosk.Click += new System.EventHandler(this.UserActivityDetected);
            // 
            // m_picCreditCardDevice
            // 
            this.m_picCreditCardDevice.BackColor = System.Drawing.Color.Transparent;
            this.m_picCreditCardDevice.Image = global::GTI.Modules.POS.Properties.Resources.CreditCardUnit;
            resources.ApplyResources(this.m_picCreditCardDevice, "m_picCreditCardDevice");
            this.m_picCreditCardDevice.Name = "m_picCreditCardDevice";
            this.m_picCreditCardDevice.TabStop = false;
            this.m_picCreditCardDevice.Click += new System.EventHandler(this.UserActivityDetected);
            // 
            // m_btnTest10
            // 
            resources.ApplyResources(this.m_btnTest10, "m_btnTest10");
            this.m_btnTest10.BackColor = System.Drawing.Color.Transparent;
            this.m_btnTest10.DebounceThreshold = 0;
            this.m_btnTest10.FocusColor = System.Drawing.Color.Black;
            this.m_btnTest10.ForeColor = System.Drawing.Color.MidnightBlue;
            this.m_btnTest10.ImageNormal = global::GTI.Modules.POS.Properties.Resources.GreenButtonUp;
            this.m_btnTest10.ImagePressed = global::GTI.Modules.POS.Properties.Resources.GreenButtonDown;
            this.m_btnTest10.Name = "m_btnTest10";
            this.m_btnTest10.SecondaryTextPadding = new System.Windows.Forms.Padding(0);
            this.m_btnTest10.UseVisualStyleBackColor = false;
            this.m_btnTest10.Click += new System.EventHandler(this.m_btnTestX_Click);
            // 
            // m_btnTest5
            // 
            resources.ApplyResources(this.m_btnTest5, "m_btnTest5");
            this.m_btnTest5.BackColor = System.Drawing.Color.Transparent;
            this.m_btnTest5.DebounceThreshold = 0;
            this.m_btnTest5.FocusColor = System.Drawing.Color.Black;
            this.m_btnTest5.ForeColor = System.Drawing.Color.MidnightBlue;
            this.m_btnTest5.ImageNormal = global::GTI.Modules.POS.Properties.Resources.GreenButtonUp;
            this.m_btnTest5.ImagePressed = global::GTI.Modules.POS.Properties.Resources.GreenButtonDown;
            this.m_btnTest5.Name = "m_btnTest5";
            this.m_btnTest5.SecondaryTextPadding = new System.Windows.Forms.Padding(0);
            this.m_btnTest5.UseVisualStyleBackColor = false;
            this.m_btnTest5.Click += new System.EventHandler(this.m_btnTestX_Click);
            // 
            // m_btnTest1
            // 
            resources.ApplyResources(this.m_btnTest1, "m_btnTest1");
            this.m_btnTest1.BackColor = System.Drawing.Color.Transparent;
            this.m_btnTest1.DebounceThreshold = 0;
            this.m_btnTest1.FocusColor = System.Drawing.Color.Black;
            this.m_btnTest1.ForeColor = System.Drawing.Color.MidnightBlue;
            this.m_btnTest1.ImageNormal = global::GTI.Modules.POS.Properties.Resources.GreenButtonUp;
            this.m_btnTest1.ImagePressed = global::GTI.Modules.POS.Properties.Resources.GreenButtonDown;
            this.m_btnTest1.Name = "m_btnTest1";
            this.m_btnTest1.SecondaryTextPadding = new System.Windows.Forms.Padding(0);
            this.m_btnTest1.UseVisualStyleBackColor = false;
            this.m_btnTest1.Click += new System.EventHandler(this.m_btnTestX_Click);
            // 
            // m_simpleKioskProgress
            // 
            resources.ApplyResources(this.m_simpleKioskProgress, "m_simpleKioskProgress");
            this.m_simpleKioskProgress.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(127)))), ((int)(((byte)(127)))));
            this.m_simpleKioskProgress.ForeColor = System.Drawing.Color.Gold;
            this.m_simpleKioskProgress.Name = "m_simpleKioskProgress";
            this.m_simpleKioskProgress.Click += new System.EventHandler(this.UserActivityDetected);
            // 
            // m_btnSimpleKioskHelp
            // 
            resources.ApplyResources(this.m_btnSimpleKioskHelp, "m_btnSimpleKioskHelp");
            this.m_btnSimpleKioskHelp.BackColor = System.Drawing.Color.Transparent;
            this.m_btnSimpleKioskHelp.DebounceThreshold = 0;
            this.m_btnSimpleKioskHelp.FocusColor = System.Drawing.Color.Black;
            this.m_btnSimpleKioskHelp.ImageNormal = global::GTI.Modules.POS.Properties.Resources.PurpleButtonUp;
            this.m_btnSimpleKioskHelp.ImagePressed = global::GTI.Modules.POS.Properties.Resources.PurpleButtonDown;
            this.m_btnSimpleKioskHelp.Name = "m_btnSimpleKioskHelp";
            this.m_btnSimpleKioskHelp.SecondaryTextPadding = new System.Windows.Forms.Padding(0);
            this.m_btnSimpleKioskHelp.ShowFocus = false;
            this.m_btnSimpleKioskHelp.UseVisualStyleBackColor = false;
            this.m_btnSimpleKioskHelp.Click += new System.EventHandler(this.m_btnSimpleKioskHelp_Click);
            // 
            // m_lblKioskCardInstructions
            // 
            resources.ApplyResources(this.m_lblKioskCardInstructions, "m_lblKioskCardInstructions");
            this.m_lblKioskCardInstructions.BackColor = System.Drawing.Color.Transparent;
            this.m_lblKioskCardInstructions.EdgeColor = System.Drawing.Color.Yellow;
            this.m_lblKioskCardInstructions.ForeColor = System.Drawing.Color.MediumBlue;
            this.m_lblKioskCardInstructions.Name = "m_lblKioskCardInstructions";
            // 
            // m_btnKioskQuit
            // 
            resources.ApplyResources(this.m_btnKioskQuit, "m_btnKioskQuit");
            this.m_btnKioskQuit.BackColor = System.Drawing.Color.Transparent;
            this.m_btnKioskQuit.DebounceThreshold = 0;
            this.m_btnKioskQuit.FocusColor = System.Drawing.Color.Black;
            this.m_btnKioskQuit.ImageNormal = global::GTI.Modules.POS.Properties.Resources.RedButtonUp;
            this.m_btnKioskQuit.ImagePressed = global::GTI.Modules.POS.Properties.Resources.RedButtonDown;
            this.m_btnKioskQuit.Name = "m_btnKioskQuit";
            this.m_btnKioskQuit.SecondaryTextPadding = new System.Windows.Forms.Padding(0);
            this.m_btnKioskQuit.ShowFocus = false;
            this.m_btnKioskQuit.UseVisualStyleBackColor = false;
            this.m_btnKioskQuit.Click += new System.EventHandler(this.m_btnCancelSale_Click);
            // 
            // m_lblKioskInstructions
            // 
            resources.ApplyResources(this.m_lblKioskInstructions, "m_lblKioskInstructions");
            this.m_lblKioskInstructions.BackColor = System.Drawing.Color.Transparent;
            this.m_lblKioskInstructions.EdgeColor = System.Drawing.Color.Gold;
            this.m_lblKioskInstructions.ForeColor = System.Drawing.Color.MediumBlue;
            this.m_lblKioskInstructions.Name = "m_lblKioskInstructions";
            this.m_lblKioskInstructions.Click += new System.EventHandler(this.UserActivityDetected);
            // 
            // m_lblKioskTotalDue
            // 
            resources.ApplyResources(this.m_lblKioskTotalDue, "m_lblKioskTotalDue");
            this.m_lblKioskTotalDue.BackColor = System.Drawing.Color.Black;
            this.m_lblKioskTotalDue.ForeColor = System.Drawing.Color.Chartreuse;
            this.m_lblKioskTotalDue.Name = "m_lblKioskTotalDue";
            this.m_lblKioskTotalDue.Click += new System.EventHandler(this.UserActivityDetected);
            // 
            // m_btnKioskCard
            // 
            resources.ApplyResources(this.m_btnKioskCard, "m_btnKioskCard");
            this.m_btnKioskCard.BackColor = System.Drawing.Color.Transparent;
            this.m_btnKioskCard.DebounceThreshold = 0;
            this.m_btnKioskCard.FocusColor = System.Drawing.Color.Black;
            this.m_btnKioskCard.ForeColor = System.Drawing.Color.MediumBlue;
            this.m_btnKioskCard.ImageNormal = global::GTI.Modules.POS.Properties.Resources.OrangeButtonUp;
            this.m_btnKioskCard.ImagePressed = global::GTI.Modules.POS.Properties.Resources.GrayButtonDown;
            this.m_btnKioskCard.Name = "m_btnKioskCard";
            this.m_btnKioskCard.SecondaryTextPadding = new System.Windows.Forms.Padding(0);
            this.m_btnKioskCard.ShowFocus = false;
            this.m_btnKioskCard.UseVisualStyleBackColor = false;
            this.m_btnKioskCard.Click += new System.EventHandler(this.m_btnCard_Click);
            // 
            // m_picLogo
            // 
            resources.ApplyResources(this.m_picLogo, "m_picLogo");
            this.m_picLogo.BackColor = System.Drawing.SystemColors.Control;
            this.m_picLogo.BackgroundImage = global::GTI.Modules.POS.Properties.Resources.GameTechLogoForKiosk;
            this.m_picLogo.Name = "m_picLogo";
            this.m_picLogo.TabStop = false;
            // 
            // FlexTenderingForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.Fuchsia;
            resources.ApplyResources(this, "$this");
            this.ControlBox = false;
            this.Controls.Add(this.m_panelKiosk);
            this.Controls.Add(this.m_panelMain);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.GradientBeginColor = System.Drawing.Color.LightGray;
            this.GradientEndColor = System.Drawing.Color.DarkGray;
            this.KeyPreview = true;
            this.Name = "FlexTenderingForm";
            this.OuterBorderEdgeColor = System.Drawing.Color.DimGray;
            this.ShowInTaskbar = false;
            this.TransparencyKey = System.Drawing.Color.Fuchsia;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FlexTenderingForm_FormClosed);
            this.Load += new System.EventHandler(this.FlexTenderingForm_Load);
            this.Shown += new System.EventHandler(this.FlexTenderingForm_Shown);
            this.Click += new System.EventHandler(this.FlexTenderingForm_Click);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.KeyPressed);
            this.m_panelMain.ResumeLayout(false);
            this.m_panelMain.PerformLayout();
            this.m_tendersPanel.ResumeLayout(false);
            this.m_panelKiosk.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.m_picCreditCardDevice)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_picLogo)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.Keypad m_keypad;
        private Controls.ColorListBox m_tendersList;
        private System.Windows.Forms.TextBox m_statusTextbox;
        private Controls.ImageButton m_buttonScrollUp;
        private Controls.ImageButton m_buttonScrollDown;
        private System.Windows.Forms.Label m_dueLabel;
        private System.Windows.Forms.Panel m_tendersPanel;
        private Controls.ImageButton m_buttonRemoveLine;
        private System.Windows.Forms.Label m_dueAmountLabel;
        private Controls.ButtonMenu m_tenderButtonMenu;
        private Controls.ImageButton m_btnCancelTendering;
        private Controls.ImageButton m_btnSwapLeftRightHanded;
        private System.Windows.Forms.Label m_lblPlayerName;
        private System.Windows.Forms.Label m_lblPlayerPoints;
        private System.Windows.Forms.Label m_lblExchangeRate;
        private Controls.ImageButton m_btnFinishTender;
        private Controls.ImageButton m_btnCurrency;
        private Controls.ImageButton m_btnContinueSale;
        private System.Windows.Forms.Timer m_kioskIdleTimer;
        private Controls.ImageButton m_btnCancelSale;
        private System.Windows.Forms.Label m_lblKioskPlayingOn;
        private Controls.ImageButton m_btnDevice;
        private System.Windows.Forms.ProgressBar m_timeoutProgress;
        private Controls.ImageButton m_btnHelp;
        private System.Windows.Forms.Label m_lblKioskTotalDue;
        private Controls.ImageButton m_btnKioskCard;
        private Controls.ImageButton m_btnKioskQuit;
        private System.Windows.Forms.Timer m_kioskCardInstructionsFlashTimer;
        private Controls.OutlinedLabel m_lblKioskCardInstructions;
        private System.Windows.Forms.PictureBox m_picCreditCardDevice;
        private Controls.ImageButton m_btnSimpleKioskHelp;
        private System.Windows.Forms.ProgressBar m_simpleKioskProgress;
        private Controls.OutlinedLabel m_lblKioskInstructions;
        private Controls.ImageButton m_btnTest10;
        private Controls.ImageButton m_btnTest5;
        private Controls.ImageButton m_btnTest1;
        private Controls.EliteGradientPanel m_panelKiosk;
        private Controls.ImageButton m_btnAdvTest10;
        private Controls.ImageButton m_btnAdvTest5;
        private Controls.ImageButton m_btnAdvTest1;
        private System.Windows.Forms.PictureBox m_picLogo;
        private Controls.EliteGradientPanel m_panelMain;
    }
}