namespace GTI.Modules.POS.UI
{
    partial class HelpForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HelpForm));
            this.m_helpTab = new System.Windows.Forms.TabControl();
            this.m_tabPageAdvancedKioskQuickAndEasy = new System.Windows.Forms.TabPage();
            this.m_tabPageAdvancedKioskMainScreen = new System.Windows.Forms.TabPage();
            this.m_tabPageCoupons = new System.Windows.Forms.TabPage();
            this.m_tabPageAdvancedKioskPayment = new System.Windows.Forms.TabPage();
            this.m_tabPageOrderingElectronics = new System.Windows.Forms.TabPage();
            this.m_tabPageOrderingPaper = new System.Windows.Forms.TabPage();
            this.m_tabPageSimpleKioskPayment = new System.Windows.Forms.TabPage();
            this.m_tabPageOrderingFromReceipt = new System.Windows.Forms.TabPage();
            this.m_tabPageHybridOrderingReceipt = new System.Windows.Forms.TabPage();
            this.m_tabPageHybridCoupons = new System.Windows.Forms.TabPage();
            this.m_tabPageAutoCoupons = new System.Windows.Forms.TabPage();
            this.m_tabPageAutoCouponsWithCouponButton = new System.Windows.Forms.TabPage();
            this.m_tabPageQuantityControl = new System.Windows.Forms.TabPage();
            this.m_tabPageAdvancedKioskPaymentSimple = new System.Windows.Forms.TabPage();
            this.m_btnClose = new GTI.Controls.ImageButton();
            this.m_kioskTimer = new System.Windows.Forms.Timer(this.components);
            this.m_timeoutProgress = new System.Windows.Forms.ProgressBar();
            this.m_lblProgressExplanation = new GTI.Controls.OutlinedLabel();
            this.m_helpTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_helpTab
            // 
            this.m_helpTab.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.m_helpTab.Controls.Add(this.m_tabPageAdvancedKioskQuickAndEasy);
            this.m_helpTab.Controls.Add(this.m_tabPageAdvancedKioskMainScreen);
            this.m_helpTab.Controls.Add(this.m_tabPageCoupons);
            this.m_helpTab.Controls.Add(this.m_tabPageAdvancedKioskPayment);
            this.m_helpTab.Controls.Add(this.m_tabPageOrderingElectronics);
            this.m_helpTab.Controls.Add(this.m_tabPageOrderingPaper);
            this.m_helpTab.Controls.Add(this.m_tabPageSimpleKioskPayment);
            this.m_helpTab.Controls.Add(this.m_tabPageOrderingFromReceipt);
            this.m_helpTab.Controls.Add(this.m_tabPageHybridOrderingReceipt);
            this.m_helpTab.Controls.Add(this.m_tabPageHybridCoupons);
            this.m_helpTab.Controls.Add(this.m_tabPageAutoCoupons);
            this.m_helpTab.Controls.Add(this.m_tabPageAutoCouponsWithCouponButton);
            this.m_helpTab.Controls.Add(this.m_tabPageQuantityControl);
            this.m_helpTab.Controls.Add(this.m_tabPageAdvancedKioskPaymentSimple);
            this.m_helpTab.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            this.m_helpTab.Font = new System.Drawing.Font("Tahoma", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_helpTab.Location = new System.Drawing.Point(25, 16);
            this.m_helpTab.Multiline = true;
            this.m_helpTab.Name = "m_helpTab";
            this.m_helpTab.SelectedIndex = 0;
            this.m_helpTab.Size = new System.Drawing.Size(976, 667);
            this.m_helpTab.SizeMode = System.Windows.Forms.TabSizeMode.FillToRight;
            this.m_helpTab.TabIndex = 0;
            this.m_helpTab.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.m_helpTab_DrawItem);
            this.m_helpTab.SelectedIndexChanged += new System.EventHandler(this.m_helpTab_SelectedIndexChanged);
            this.m_helpTab.Click += new System.EventHandler(this.SomethingWasClicked);
            // 
            // m_tabPageAdvancedKioskQuickAndEasy
            // 
            this.m_tabPageAdvancedKioskQuickAndEasy.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(208)))), ((int)(((byte)(213)))));
            this.m_tabPageAdvancedKioskQuickAndEasy.BackgroundImage = global::GTI.Modules.POS.Properties.Resources.HelpScreen0a;
            this.m_tabPageAdvancedKioskQuickAndEasy.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.m_tabPageAdvancedKioskQuickAndEasy.Location = new System.Drawing.Point(4, 94);
            this.m_tabPageAdvancedKioskQuickAndEasy.Name = "m_tabPageAdvancedKioskQuickAndEasy";
            this.m_tabPageAdvancedKioskQuickAndEasy.Padding = new System.Windows.Forms.Padding(3);
            this.m_tabPageAdvancedKioskQuickAndEasy.Size = new System.Drawing.Size(968, 569);
            this.m_tabPageAdvancedKioskQuickAndEasy.TabIndex = 0;
            this.m_tabPageAdvancedKioskQuickAndEasy.Text = "Quick & Easy";
            this.m_tabPageAdvancedKioskQuickAndEasy.Click += new System.EventHandler(this.SomethingWasClicked);
            // 
            // m_tabPageAdvancedKioskMainScreen
            // 
            this.m_tabPageAdvancedKioskMainScreen.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(208)))), ((int)(((byte)(213)))));
            this.m_tabPageAdvancedKioskMainScreen.BackgroundImage = global::GTI.Modules.POS.Properties.Resources.HelpScreen1a;
            this.m_tabPageAdvancedKioskMainScreen.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.m_tabPageAdvancedKioskMainScreen.Location = new System.Drawing.Point(4, 94);
            this.m_tabPageAdvancedKioskMainScreen.Name = "m_tabPageAdvancedKioskMainScreen";
            this.m_tabPageAdvancedKioskMainScreen.Padding = new System.Windows.Forms.Padding(3);
            this.m_tabPageAdvancedKioskMainScreen.Size = new System.Drawing.Size(968, 569);
            this.m_tabPageAdvancedKioskMainScreen.TabIndex = 1;
            this.m_tabPageAdvancedKioskMainScreen.Text = "Main Screen";
            this.m_tabPageAdvancedKioskMainScreen.Click += new System.EventHandler(this.SomethingWasClicked);
            // 
            // m_tabPageCoupons
            // 
            this.m_tabPageCoupons.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(208)))), ((int)(((byte)(213)))));
            this.m_tabPageCoupons.BackgroundImage = global::GTI.Modules.POS.Properties.Resources.HelpScreen2a;
            this.m_tabPageCoupons.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.m_tabPageCoupons.Location = new System.Drawing.Point(4, 94);
            this.m_tabPageCoupons.Name = "m_tabPageCoupons";
            this.m_tabPageCoupons.Padding = new System.Windows.Forms.Padding(3);
            this.m_tabPageCoupons.Size = new System.Drawing.Size(968, 569);
            this.m_tabPageCoupons.TabIndex = 2;
            this.m_tabPageCoupons.Text = "Coupons";
            this.m_tabPageCoupons.Click += new System.EventHandler(this.SomethingWasClicked);
            // 
            // m_tabPageAdvancedKioskPayment
            // 
            this.m_tabPageAdvancedKioskPayment.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(208)))), ((int)(((byte)(213)))));
            this.m_tabPageAdvancedKioskPayment.BackgroundImage = global::GTI.Modules.POS.Properties.Resources.HelpScreen3a;
            this.m_tabPageAdvancedKioskPayment.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.m_tabPageAdvancedKioskPayment.Location = new System.Drawing.Point(4, 94);
            this.m_tabPageAdvancedKioskPayment.Name = "m_tabPageAdvancedKioskPayment";
            this.m_tabPageAdvancedKioskPayment.Padding = new System.Windows.Forms.Padding(3);
            this.m_tabPageAdvancedKioskPayment.Size = new System.Drawing.Size(968, 569);
            this.m_tabPageAdvancedKioskPayment.TabIndex = 3;
            this.m_tabPageAdvancedKioskPayment.Text = "Checkout";
            this.m_tabPageAdvancedKioskPayment.Click += new System.EventHandler(this.SomethingWasClicked);
            // 
            // m_tabPageOrderingElectronics
            // 
            this.m_tabPageOrderingElectronics.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(208)))), ((int)(((byte)(213)))));
            this.m_tabPageOrderingElectronics.BackgroundImage = global::GTI.Modules.POS.Properties.Resources.HelpScreenOrderingFromButtons;
            this.m_tabPageOrderingElectronics.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.m_tabPageOrderingElectronics.Location = new System.Drawing.Point(4, 94);
            this.m_tabPageOrderingElectronics.Name = "m_tabPageOrderingElectronics";
            this.m_tabPageOrderingElectronics.Padding = new System.Windows.Forms.Padding(3);
            this.m_tabPageOrderingElectronics.Size = new System.Drawing.Size(968, 569);
            this.m_tabPageOrderingElectronics.TabIndex = 4;
            this.m_tabPageOrderingElectronics.Text = "Electronic Packs";
            this.m_tabPageOrderingElectronics.Click += new System.EventHandler(this.SomethingWasClicked);
            // 
            // m_tabPageOrderingPaper
            // 
            this.m_tabPageOrderingPaper.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(208)))), ((int)(((byte)(213)))));
            this.m_tabPageOrderingPaper.BackgroundImage = global::GTI.Modules.POS.Properties.Resources.HelpScreenOrderingPaper;
            this.m_tabPageOrderingPaper.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.m_tabPageOrderingPaper.Location = new System.Drawing.Point(4, 94);
            this.m_tabPageOrderingPaper.Name = "m_tabPageOrderingPaper";
            this.m_tabPageOrderingPaper.Padding = new System.Windows.Forms.Padding(3);
            this.m_tabPageOrderingPaper.Size = new System.Drawing.Size(968, 569);
            this.m_tabPageOrderingPaper.TabIndex = 5;
            this.m_tabPageOrderingPaper.Text = "Paper Packs";
            this.m_tabPageOrderingPaper.Click += new System.EventHandler(this.SomethingWasClicked);
            // 
            // m_tabPageSimpleKioskPayment
            // 
            this.m_tabPageSimpleKioskPayment.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(208)))), ((int)(((byte)(213)))));
            this.m_tabPageSimpleKioskPayment.BackgroundImage = global::GTI.Modules.POS.Properties.Resources.HelpScreen6a;
            this.m_tabPageSimpleKioskPayment.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.m_tabPageSimpleKioskPayment.Location = new System.Drawing.Point(4, 94);
            this.m_tabPageSimpleKioskPayment.Name = "m_tabPageSimpleKioskPayment";
            this.m_tabPageSimpleKioskPayment.Padding = new System.Windows.Forms.Padding(3);
            this.m_tabPageSimpleKioskPayment.Size = new System.Drawing.Size(968, 569);
            this.m_tabPageSimpleKioskPayment.TabIndex = 6;
            this.m_tabPageSimpleKioskPayment.Text = "Payment";
            this.m_tabPageSimpleKioskPayment.Click += new System.EventHandler(this.SomethingWasClicked);
            // 
            // m_tabPageOrderingFromReceipt
            // 
            this.m_tabPageOrderingFromReceipt.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(208)))), ((int)(((byte)(213)))));
            this.m_tabPageOrderingFromReceipt.BackgroundImage = global::GTI.Modules.POS.Properties.Resources.HelpScreenOrderingReceipt;
            this.m_tabPageOrderingFromReceipt.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.m_tabPageOrderingFromReceipt.Location = new System.Drawing.Point(4, 94);
            this.m_tabPageOrderingFromReceipt.Name = "m_tabPageOrderingFromReceipt";
            this.m_tabPageOrderingFromReceipt.Padding = new System.Windows.Forms.Padding(3);
            this.m_tabPageOrderingFromReceipt.Size = new System.Drawing.Size(968, 569);
            this.m_tabPageOrderingFromReceipt.TabIndex = 7;
            this.m_tabPageOrderingFromReceipt.Text = "Buy-Again from Receipt";
            this.m_tabPageOrderingFromReceipt.Click += new System.EventHandler(this.SomethingWasClicked);
            // 
            // m_tabPageHybridOrderingReceipt
            // 
            this.m_tabPageHybridOrderingReceipt.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(208)))), ((int)(((byte)(213)))));
            this.m_tabPageHybridOrderingReceipt.BackgroundImage = global::GTI.Modules.POS.Properties.Resources.HelpScreenOrderingReceiptHybrid;
            this.m_tabPageHybridOrderingReceipt.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.m_tabPageHybridOrderingReceipt.Location = new System.Drawing.Point(4, 94);
            this.m_tabPageHybridOrderingReceipt.Name = "m_tabPageHybridOrderingReceipt";
            this.m_tabPageHybridOrderingReceipt.Padding = new System.Windows.Forms.Padding(3);
            this.m_tabPageHybridOrderingReceipt.Size = new System.Drawing.Size(968, 569);
            this.m_tabPageHybridOrderingReceipt.TabIndex = 8;
            this.m_tabPageHybridOrderingReceipt.Text = "Buy-Again from Receipt";
            this.m_tabPageHybridOrderingReceipt.Click += new System.EventHandler(this.SomethingWasClicked);
            // 
            // m_tabPageHybridCoupons
            // 
            this.m_tabPageHybridCoupons.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(208)))), ((int)(((byte)(213)))));
            this.m_tabPageHybridCoupons.BackgroundImage = global::GTI.Modules.POS.Properties.Resources.HelpScreenCouponsHybrid;
            this.m_tabPageHybridCoupons.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.m_tabPageHybridCoupons.Location = new System.Drawing.Point(4, 94);
            this.m_tabPageHybridCoupons.Name = "m_tabPageHybridCoupons";
            this.m_tabPageHybridCoupons.Padding = new System.Windows.Forms.Padding(3);
            this.m_tabPageHybridCoupons.Size = new System.Drawing.Size(968, 569);
            this.m_tabPageHybridCoupons.TabIndex = 9;
            this.m_tabPageHybridCoupons.Text = "Coupons";
            this.m_tabPageHybridCoupons.Click += new System.EventHandler(this.SomethingWasClicked);
            // 
            // m_tabPageAutoCoupons
            // 
            this.m_tabPageAutoCoupons.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(208)))), ((int)(((byte)(213)))));
            this.m_tabPageAutoCoupons.BackgroundImage = global::GTI.Modules.POS.Properties.Resources.HelpScreenAutoCoupons;
            this.m_tabPageAutoCoupons.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.m_tabPageAutoCoupons.Location = new System.Drawing.Point(4, 94);
            this.m_tabPageAutoCoupons.Name = "m_tabPageAutoCoupons";
            this.m_tabPageAutoCoupons.Padding = new System.Windows.Forms.Padding(3);
            this.m_tabPageAutoCoupons.Size = new System.Drawing.Size(968, 569);
            this.m_tabPageAutoCoupons.TabIndex = 10;
            this.m_tabPageAutoCoupons.Text = "Auto Coupons";
            // 
            // m_tabPageAutoCouponsWithCouponButton
            // 
            this.m_tabPageAutoCouponsWithCouponButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(208)))), ((int)(((byte)(213)))));
            this.m_tabPageAutoCouponsWithCouponButton.BackgroundImage = global::GTI.Modules.POS.Properties.Resources.HelpScreenAutoCouponsWithCouponButton;
            this.m_tabPageAutoCouponsWithCouponButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.m_tabPageAutoCouponsWithCouponButton.Location = new System.Drawing.Point(4, 94);
            this.m_tabPageAutoCouponsWithCouponButton.Name = "m_tabPageAutoCouponsWithCouponButton";
            this.m_tabPageAutoCouponsWithCouponButton.Padding = new System.Windows.Forms.Padding(3);
            this.m_tabPageAutoCouponsWithCouponButton.Size = new System.Drawing.Size(968, 569);
            this.m_tabPageAutoCouponsWithCouponButton.TabIndex = 11;
            this.m_tabPageAutoCouponsWithCouponButton.Text = "Auto Coupons";
            // 
            // m_tabPageQuantityControl
            // 
            this.m_tabPageQuantityControl.BackgroundImage = global::GTI.Modules.POS.Properties.Resources.HelpScreenQuantityControl;
            this.m_tabPageQuantityControl.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.m_tabPageQuantityControl.Location = new System.Drawing.Point(4, 94);
            this.m_tabPageQuantityControl.Name = "m_tabPageQuantityControl";
            this.m_tabPageQuantityControl.Padding = new System.Windows.Forms.Padding(3);
            this.m_tabPageQuantityControl.Size = new System.Drawing.Size(968, 569);
            this.m_tabPageQuantityControl.TabIndex = 12;
            this.m_tabPageQuantityControl.Text = "Changing Item Quantity";
            this.m_tabPageQuantityControl.UseVisualStyleBackColor = true;
            // 
            // m_tabPageAdvancedKioskPaymentSimple
            // 
            this.m_tabPageAdvancedKioskPaymentSimple.BackgroundImage = global::GTI.Modules.POS.Properties.Resources.HelpPaymentAdvancedKioskSimpleForm;
            this.m_tabPageAdvancedKioskPaymentSimple.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.m_tabPageAdvancedKioskPaymentSimple.Location = new System.Drawing.Point(4, 94);
            this.m_tabPageAdvancedKioskPaymentSimple.Name = "m_tabPageAdvancedKioskPaymentSimple";
            this.m_tabPageAdvancedKioskPaymentSimple.Padding = new System.Windows.Forms.Padding(3);
            this.m_tabPageAdvancedKioskPaymentSimple.Size = new System.Drawing.Size(968, 569);
            this.m_tabPageAdvancedKioskPaymentSimple.TabIndex = 13;
            this.m_tabPageAdvancedKioskPaymentSimple.Text = "Checkout";
            this.m_tabPageAdvancedKioskPaymentSimple.UseVisualStyleBackColor = true;
            // 
            // m_btnClose
            // 
            this.m_btnClose.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.m_btnClose.BackColor = System.Drawing.Color.Transparent;
            this.m_btnClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.m_btnClose.FocusColor = System.Drawing.Color.Black;
            this.m_btnClose.Font = new System.Drawing.Font("Tahoma", 26.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_btnClose.ImageNormal = global::GTI.Modules.POS.Properties.Resources.GrayButtonUp;
            this.m_btnClose.ImagePressed = global::GTI.Modules.POS.Properties.Resources.GrayButtonDown;
            this.m_btnClose.Location = new System.Drawing.Point(12, 689);
            this.m_btnClose.MinimumSize = new System.Drawing.Size(30, 30);
            this.m_btnClose.Name = "m_btnClose";
            this.m_btnClose.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_btnClose.ShowFocus = false;
            this.m_btnClose.Size = new System.Drawing.Size(220, 67);
            this.m_btnClose.TabIndex = 1;
            this.m_btnClose.Text = "Close";
            this.m_btnClose.UseVisualStyleBackColor = false;
            this.m_btnClose.Click += new System.EventHandler(this.m_btnClose_Click);
            // 
            // m_kioskTimer
            // 
            this.m_kioskTimer.Enabled = true;
            this.m_kioskTimer.Interval = 500;
            this.m_kioskTimer.Tick += new System.EventHandler(this.m_kioskTimer_Tick);
            // 
            // m_timeoutProgress
            // 
            this.m_timeoutProgress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.m_timeoutProgress.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(127)))), ((int)(((byte)(127)))));
            this.m_timeoutProgress.ForeColor = System.Drawing.Color.Gold;
            this.m_timeoutProgress.Location = new System.Drawing.Point(39, 756);
            this.m_timeoutProgress.Name = "m_timeoutProgress";
            this.m_timeoutProgress.Size = new System.Drawing.Size(931, 10);
            this.m_timeoutProgress.TabIndex = 2;
            this.m_timeoutProgress.Visible = false;
            this.m_timeoutProgress.Click += new System.EventHandler(this.SomethingWasClicked);
            // 
            // m_lblProgressExplanation
            // 
            this.m_lblProgressExplanation.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.m_lblProgressExplanation.BackColor = System.Drawing.Color.Transparent;
            this.m_lblProgressExplanation.DrawAsRaised3D = true;
            this.m_lblProgressExplanation.EdgeColor = System.Drawing.Color.White;
            this.m_lblProgressExplanation.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_lblProgressExplanation.ForeColor = System.Drawing.Color.Blue;
            this.m_lblProgressExplanation.Location = new System.Drawing.Point(238, 695);
            this.m_lblProgressExplanation.Name = "m_lblProgressExplanation";
            this.m_lblProgressExplanation.ShowEdge = false;
            this.m_lblProgressExplanation.Size = new System.Drawing.Size(763, 59);
            this.m_lblProgressExplanation.TabIndex = 3;
            this.m_lblProgressExplanation.Text = resources.GetString("m_lblProgressExplanation.Text");
            this.m_lblProgressExplanation.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // HelpForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.ClientSize = new System.Drawing.Size(1024, 768);
            this.ControlBox = false;
            this.Controls.Add(this.m_lblProgressExplanation);
            this.Controls.Add(this.m_timeoutProgress);
            this.Controls.Add(this.m_btnClose);
            this.Controls.Add(this.m_helpTab);
            this.DoubleBuffered = true;
            this.DrawAsGradient = true;
            this.DrawBorderOuterEdge = true;
            this.DrawRounded = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MinimumSize = new System.Drawing.Size(1024, 768);
            this.Name = "HelpForm";
            this.ShowIcon = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "HelpForm";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.HelpForm_FormClosing);
            this.Shown += new System.EventHandler(this.HelpForm_Shown);
            this.Click += new System.EventHandler(this.SomethingWasClicked);
            this.m_helpTab.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl m_helpTab;
        private System.Windows.Forms.TabPage m_tabPageAdvancedKioskQuickAndEasy;
        private System.Windows.Forms.TabPage m_tabPageAdvancedKioskMainScreen;
        private Controls.ImageButton m_btnClose;
        private System.Windows.Forms.TabPage m_tabPageCoupons;
        private System.Windows.Forms.TabPage m_tabPageAdvancedKioskPayment;
        private System.Windows.Forms.Timer m_kioskTimer;
        private System.Windows.Forms.ProgressBar m_timeoutProgress;
        private GTI.Controls.OutlinedLabel m_lblProgressExplanation;
        private System.Windows.Forms.TabPage m_tabPageOrderingElectronics;
        private System.Windows.Forms.TabPage m_tabPageOrderingPaper;
        private System.Windows.Forms.TabPage m_tabPageSimpleKioskPayment;
        private System.Windows.Forms.TabPage m_tabPageOrderingFromReceipt;
        private System.Windows.Forms.TabPage m_tabPageHybridOrderingReceipt;
        private System.Windows.Forms.TabPage m_tabPageHybridCoupons;
        private System.Windows.Forms.TabPage m_tabPageAutoCoupons;
        private System.Windows.Forms.TabPage m_tabPageAutoCouponsWithCouponButton;
        private System.Windows.Forms.TabPage m_tabPageQuantityControl;
        private System.Windows.Forms.TabPage m_tabPageAdvancedKioskPaymentSimple;
    }
}