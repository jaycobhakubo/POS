namespace GTI.Modules.POS.UI
{
    partial class PayCouponForm4
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PayCouponForm4));
            this.lblTaxedCouponTotals = new System.Windows.Forms.Label();
            this.lblTaxedCouponLabel = new System.Windows.Forms.Label();
            this.lblDevice = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblNonTaxedCouponTotals = new System.Windows.Forms.Label();
            this.lblNonTaxedCouponLabel = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.lblTaxesAndFees = new System.Windows.Forms.Label();
            this.lblTotal = new System.Windows.Forms.Label();
            this.lblOrderSubTotals = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.FinishSaleButton = new GTI.Controls.ImageButton();
            this.ContinueSaleButton = new GTI.Controls.ImageButton();
            this.gtiListViewPlayerCoupon = new GTI.Controls.GTIListView();
            this.CompName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.CompExprDate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.CompValue = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.CouponListDownButton = new GTI.Controls.ImageButton();
            this.CouponListUpButton = new GTI.Controls.ImageButton();
            this.m_kioskIdleTimer = new System.Windows.Forms.Timer(this.components);
            this.m_timeoutProgress = new System.Windows.Forms.ProgressBar();
            this.m_panelMain = new GTI.Controls.EliteGradientPanel();
            this.lblPrepaidLabel = new System.Windows.Forms.Label();
            this.lblPrepaid = new System.Windows.Forms.Label();
            this.m_panelMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblTaxedCouponTotals
            // 
            resources.ApplyResources(this.lblTaxedCouponTotals, "lblTaxedCouponTotals");
            this.lblTaxedCouponTotals.ForeColor = System.Drawing.Color.White;
            this.lblTaxedCouponTotals.Name = "lblTaxedCouponTotals";
            // 
            // lblTaxedCouponLabel
            // 
            resources.ApplyResources(this.lblTaxedCouponLabel, "lblTaxedCouponLabel");
            this.lblTaxedCouponLabel.ForeColor = System.Drawing.Color.White;
            this.lblTaxedCouponLabel.Name = "lblTaxedCouponLabel";
            // 
            // lblDevice
            // 
            resources.ApplyResources(this.lblDevice, "lblDevice");
            this.lblDevice.ForeColor = System.Drawing.Color.White;
            this.lblDevice.Name = "lblDevice";
            // 
            // lblStatus
            // 
            this.lblStatus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(117)))), ((int)(((byte)(104)))), ((int)(((byte)(99)))));
            resources.ApplyResources(this.lblStatus, "lblStatus");
            this.lblStatus.ForeColor = System.Drawing.Color.Gold;
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Click += new System.EventHandler(this.UserActivityDetected);
            // 
            // lblNonTaxedCouponTotals
            // 
            resources.ApplyResources(this.lblNonTaxedCouponTotals, "lblNonTaxedCouponTotals");
            this.lblNonTaxedCouponTotals.ForeColor = System.Drawing.Color.White;
            this.lblNonTaxedCouponTotals.Name = "lblNonTaxedCouponTotals";
            // 
            // lblNonTaxedCouponLabel
            // 
            resources.ApplyResources(this.lblNonTaxedCouponLabel, "lblNonTaxedCouponLabel");
            this.lblNonTaxedCouponLabel.ForeColor = System.Drawing.Color.White;
            this.lblNonTaxedCouponLabel.Name = "lblNonTaxedCouponLabel";
            // 
            // label13
            // 
            resources.ApplyResources(this.label13, "label13");
            this.label13.ForeColor = System.Drawing.Color.White;
            this.label13.Name = "label13";
            // 
            // lblTaxesAndFees
            // 
            resources.ApplyResources(this.lblTaxesAndFees, "lblTaxesAndFees");
            this.lblTaxesAndFees.ForeColor = System.Drawing.Color.White;
            this.lblTaxesAndFees.Name = "lblTaxesAndFees";
            // 
            // lblTotal
            // 
            resources.ApplyResources(this.lblTotal, "lblTotal");
            this.lblTotal.ForeColor = System.Drawing.Color.Lime;
            this.lblTotal.Name = "lblTotal";
            // 
            // lblOrderSubTotals
            // 
            resources.ApplyResources(this.lblOrderSubTotals, "lblOrderSubTotals");
            this.lblOrderSubTotals.ForeColor = System.Drawing.Color.Yellow;
            this.lblOrderSubTotals.Name = "lblOrderSubTotals";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Name = "label3";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Name = "label2";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Name = "label1";
            // 
            // FinishSaleButton
            // 
            this.FinishSaleButton.BackColor = System.Drawing.Color.Transparent;
            this.FinishSaleButton.FocusColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.FinishSaleButton, "FinishSaleButton");
            this.FinishSaleButton.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.FinishSaleButton.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.FinishSaleButton.Name = "FinishSaleButton";
            this.FinishSaleButton.SecondaryTextPadding = new System.Windows.Forms.Padding(0);
            this.FinishSaleButton.ShowFocus = false;
            this.FinishSaleButton.UseVisualStyleBackColor = false;
            this.FinishSaleButton.Click += new System.EventHandler(this.FinishSaleButton_Click);
            // 
            // ContinueSaleButton
            // 
            this.ContinueSaleButton.BackColor = System.Drawing.Color.Transparent;
            this.ContinueSaleButton.FocusColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.ContinueSaleButton, "ContinueSaleButton");
            this.ContinueSaleButton.ImageIcon = global::GTI.Modules.POS.Properties.Resources.BackSymbol;
            this.ContinueSaleButton.ImageNormal = global::GTI.Modules.POS.Properties.Resources.DarkOrangeButtonUp;
            this.ContinueSaleButton.ImagePressed = global::GTI.Modules.POS.Properties.Resources.DarkOrangeButtonDown;
            this.ContinueSaleButton.Name = "ContinueSaleButton";
            this.ContinueSaleButton.SecondaryTextPadding = new System.Windows.Forms.Padding(0);
            this.ContinueSaleButton.ShowFocus = false;
            this.ContinueSaleButton.UseVisualStyleBackColor = false;
            this.ContinueSaleButton.Click += new System.EventHandler(this.ContinueSaleButton_Click);
            // 
            // gtiListViewPlayerCoupon
            // 
            this.gtiListViewPlayerCoupon.AllowEraseBackground = true;
            this.gtiListViewPlayerCoupon.BackColor = System.Drawing.Color.White;
            this.gtiListViewPlayerCoupon.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.gtiListViewPlayerCoupon.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.CompName,
            this.CompExprDate,
            this.CompValue});
            this.gtiListViewPlayerCoupon.DownButton = this.CouponListDownButton;
            this.gtiListViewPlayerCoupon.DownIconBottomNotVisible = global::GTI.Modules.POS.Properties.Resources.ArrowDownRed;
            this.gtiListViewPlayerCoupon.DownIconBottomVisible = global::GTI.Modules.POS.Properties.Resources.ArrowDown;
            resources.ApplyResources(this.gtiListViewPlayerCoupon, "gtiListViewPlayerCoupon");
            this.gtiListViewPlayerCoupon.FullRowSelect = true;
            this.gtiListViewPlayerCoupon.HideSelection = false;
            this.gtiListViewPlayerCoupon.MultiSelect = false;
            this.gtiListViewPlayerCoupon.Name = "gtiListViewPlayerCoupon";
            this.gtiListViewPlayerCoupon.OwnerDraw = true;
            this.gtiListViewPlayerCoupon.Scrollable = false;
            this.gtiListViewPlayerCoupon.SortColumn = 1;
            this.gtiListViewPlayerCoupon.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.gtiListViewPlayerCoupon.UpButton = this.CouponListUpButton;
            this.gtiListViewPlayerCoupon.UpIconTopNotVisible = global::GTI.Modules.POS.Properties.Resources.ArrowUpRed;
            this.gtiListViewPlayerCoupon.UpIconTopVisible = global::GTI.Modules.POS.Properties.Resources.ArrowUp;
            this.gtiListViewPlayerCoupon.UseCompatibleStateImageBehavior = false;
            this.gtiListViewPlayerCoupon.UseOwnerDrawnSubItemMethod = true;
            this.gtiListViewPlayerCoupon.View = System.Windows.Forms.View.Details;
            this.gtiListViewPlayerCoupon.DrawSubItem += new System.Windows.Forms.DrawListViewSubItemEventHandler(this.gtiListViewPlayerCoupon_DrawSubItem);
            this.gtiListViewPlayerCoupon.Click += new System.EventHandler(this.UserActivityDetected);
            this.gtiListViewPlayerCoupon.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gtiListViewPlayerCoupon_MouseDown);
            this.gtiListViewPlayerCoupon.MouseUp += new System.Windows.Forms.MouseEventHandler(this.gtiListViewPlayerCoupon_MouseUp);
            // 
            // CompName
            // 
            this.CompName.Tag = "0";
            resources.ApplyResources(this.CompName, "CompName");
            // 
            // CompExprDate
            // 
            this.CompExprDate.Tag = "2";
            resources.ApplyResources(this.CompExprDate, "CompExprDate");
            // 
            // CompValue
            // 
            this.CompValue.Tag = "1";
            resources.ApplyResources(this.CompValue, "CompValue");
            // 
            // CouponListDownButton
            // 
            this.CouponListDownButton.BackColor = System.Drawing.Color.Transparent;
            this.CouponListDownButton.FocusColor = System.Drawing.Color.Black;
            this.CouponListDownButton.ImageIcon = global::GTI.Modules.POS.Properties.Resources.ArrowDown;
            this.CouponListDownButton.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.CouponListDownButton.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            resources.ApplyResources(this.CouponListDownButton, "CouponListDownButton");
            this.CouponListDownButton.Name = "CouponListDownButton";
            this.CouponListDownButton.RepeatingIfHeld = true;
            this.CouponListDownButton.SecondaryTextPadding = new System.Windows.Forms.Padding(0);
            this.CouponListDownButton.ShowFocus = false;
            this.CouponListDownButton.TabStop = false;
            this.CouponListDownButton.UseVisualStyleBackColor = false;
            this.CouponListDownButton.Click += new System.EventHandler(this.CouponListDownButton_Click);
            // 
            // CouponListUpButton
            // 
            this.CouponListUpButton.BackColor = System.Drawing.Color.Transparent;
            this.CouponListUpButton.FocusColor = System.Drawing.Color.Black;
            this.CouponListUpButton.ImageIcon = global::GTI.Modules.POS.Properties.Resources.ArrowUp;
            this.CouponListUpButton.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.CouponListUpButton.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            resources.ApplyResources(this.CouponListUpButton, "CouponListUpButton");
            this.CouponListUpButton.Name = "CouponListUpButton";
            this.CouponListUpButton.RepeatingIfHeld = true;
            this.CouponListUpButton.SecondaryTextPadding = new System.Windows.Forms.Padding(0);
            this.CouponListUpButton.ShowFocus = false;
            this.CouponListUpButton.TabStop = false;
            this.CouponListUpButton.UseVisualStyleBackColor = false;
            this.CouponListUpButton.Click += new System.EventHandler(this.CouponListUpButton_Click);
            // 
            // m_kioskIdleTimer
            // 
            this.m_kioskIdleTimer.Interval = 500;
            this.m_kioskIdleTimer.Tick += new System.EventHandler(this.m_kioskIdleTimer_Tick);
            // 
            // m_timeoutProgress
            // 
            this.m_timeoutProgress.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(127)))), ((int)(((byte)(127)))));
            this.m_timeoutProgress.ForeColor = System.Drawing.Color.Gold;
            resources.ApplyResources(this.m_timeoutProgress, "m_timeoutProgress");
            this.m_timeoutProgress.Name = "m_timeoutProgress";
            // 
            // m_panelMain
            // 
            this.m_panelMain.BackColor = System.Drawing.Color.Transparent;
            this.m_panelMain.BorderColor = System.Drawing.Color.Silver;
            this.m_panelMain.Controls.Add(this.CouponListUpButton);
            this.m_panelMain.Controls.Add(this.CouponListDownButton);
            this.m_panelMain.Controls.Add(this.gtiListViewPlayerCoupon);
            this.m_panelMain.Controls.Add(this.ContinueSaleButton);
            this.m_panelMain.Controls.Add(this.FinishSaleButton);
            this.m_panelMain.Controls.Add(this.lblTaxedCouponTotals);
            this.m_panelMain.Controls.Add(this.lblTaxedCouponLabel);
            this.m_panelMain.Controls.Add(this.lblDevice);
            this.m_panelMain.Controls.Add(this.lblStatus);
            this.m_panelMain.Controls.Add(this.lblNonTaxedCouponTotals);
            this.m_panelMain.Controls.Add(this.lblNonTaxedCouponLabel);
            this.m_panelMain.Controls.Add(this.lblPrepaidLabel);
            this.m_panelMain.Controls.Add(this.lblPrepaid);
            this.m_panelMain.Controls.Add(this.label13);
            this.m_panelMain.Controls.Add(this.lblTaxesAndFees);
            this.m_panelMain.Controls.Add(this.lblTotal);
            this.m_panelMain.Controls.Add(this.lblOrderSubTotals);
            this.m_panelMain.Controls.Add(this.label3);
            this.m_panelMain.Controls.Add(this.label2);
            this.m_panelMain.Controls.Add(this.label1);
            this.m_panelMain.Controls.Add(this.m_timeoutProgress);
            this.m_panelMain.GradientBeginColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(179)))), ((int)(((byte)(213)))));
            this.m_panelMain.GradientEndColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(186)))), ((int)(((byte)(192)))));
            this.m_panelMain.InnerBorderEdgeColor = System.Drawing.Color.SlateGray;
            resources.ApplyResources(this.m_panelMain, "m_panelMain");
            this.m_panelMain.Name = "m_panelMain";
            this.m_panelMain.OuterBorderEdgeColor = System.Drawing.Color.SlateGray;
            this.m_panelMain.Click += new System.EventHandler(this.UserActivityDetected);
            // 
            // lblPrepaidLabel
            // 
            resources.ApplyResources(this.lblPrepaidLabel, "lblPrepaidLabel");
            this.lblPrepaidLabel.BackColor = System.Drawing.Color.Transparent;
            this.lblPrepaidLabel.ForeColor = System.Drawing.Color.White;
            this.lblPrepaidLabel.Name = "lblPrepaidLabel";
            // 
            // lblPrepaid
            // 
            this.lblPrepaid.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.lblPrepaid, "lblPrepaid");
            this.lblPrepaid.ForeColor = System.Drawing.Color.White;
            this.lblPrepaid.Name = "lblPrepaid";
            // 
            // PayCouponForm4
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackgroundImage = global::GTI.Modules.POS.Properties.Resources.CouponBack;
            resources.ApplyResources(this, "$this");
            this.BorderThickness = 1;
            this.Controls.Add(this.m_panelMain);
            this.DoubleBuffered = true;
            this.DrawBorderOuterEdge = true;
            this.DrawRounded = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.GradientEndColor = System.Drawing.Color.FromArgb(((int)(((byte)(117)))), ((int)(((byte)(104)))), ((int)(((byte)(99)))));
            this.Name = "PayCouponForm4";
            this.ShowInTaskbar = false;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.PayCouponForm4_FormClosed);
            this.Load += new System.EventHandler(this.PayCouponForm4_Load);
            this.Shown += new System.EventHandler(this.PayCouponForm4_Shown);
            this.Click += new System.EventHandler(this.UserActivityDetected);
            this.MouseLeave += new System.EventHandler(this.PayCouponForm4_MouseLeave);
            this.m_panelMain.ResumeLayout(false);
            this.m_panelMain.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTaxesAndFees;
        private System.Windows.Forms.Label lblTotal;
        private System.Windows.Forms.Label lblOrderSubTotals;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private Controls.ImageButton FinishSaleButton;
        private Controls.ImageButton ContinueSaleButton;
        private System.Windows.Forms.Label label13;
        private Controls.GTIListView gtiListViewPlayerCoupon;
        private System.Windows.Forms.ColumnHeader CompName;
        private System.Windows.Forms.ColumnHeader CompValue;
        private System.Windows.Forms.Label lblNonTaxedCouponTotals;
        private System.Windows.Forms.Label lblNonTaxedCouponLabel;
        private System.Windows.Forms.Label lblStatus;
        private Controls.ImageButton CouponListUpButton;
        private Controls.ImageButton CouponListDownButton;
        private System.Windows.Forms.ColumnHeader CompExprDate;
        private System.Windows.Forms.Label lblDevice;
        private System.Windows.Forms.Label lblTaxedCouponTotals;
        private System.Windows.Forms.Label lblTaxedCouponLabel;
        private System.Windows.Forms.Timer m_kioskIdleTimer;
        private System.Windows.Forms.ProgressBar m_timeoutProgress;
        private GTI.Controls.EliteGradientPanel m_panelMain;
        private System.Windows.Forms.Label lblPrepaidLabel;
        private System.Windows.Forms.Label lblPrepaid;
    }
}