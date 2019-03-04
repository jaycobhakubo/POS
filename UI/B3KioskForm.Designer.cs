namespace GTI.Modules.POS.UI
{
    partial class B3KioskForm
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
            this.m_kioskTimer = new System.Windows.Forms.Timer(this.components);
            this.m_lblInstructions1 = new GTI.Controls.OutlinedLabel();
            this.m_lblInstructions2 = new GTI.Controls.OutlinedLabel();
            this.m_btnB3Test10 = new GTI.Controls.ImageButton();
            this.m_btnB3Test5 = new GTI.Controls.ImageButton();
            this.m_btnB3Test1 = new GTI.Controls.ImageButton();
            this.m_timeoutProgress = new System.Windows.Forms.ProgressBar();
            this.m_pictureB3Logo = new System.Windows.Forms.PictureBox();
            this.m_lblTotal = new System.Windows.Forms.Label();
            this.m_lblTotalLabel = new System.Windows.Forms.Label();
            this.m_lblInstructions = new GTI.Controls.OutlinedLabel();
            this.m_btnBuy = new GTI.Controls.ImageButton();
            this.m_btnQuit = new GTI.Controls.ImageButton();
            ((System.ComponentModel.ISupportInitialize)(this.m_pictureB3Logo)).BeginInit();
            this.SuspendLayout();
            // 
            // m_kioskTimer
            // 
            this.m_kioskTimer.Interval = 500;
            this.m_kioskTimer.Tick += new System.EventHandler(this.m_kioskTimer_Tick);
            // 
            // m_lblInstructions1
            // 
            this.m_lblInstructions1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.m_lblInstructions1.BackColor = System.Drawing.Color.Transparent;
            this.m_lblInstructions1.EdgeColor = System.Drawing.Color.Red;
            this.m_lblInstructions1.Font = new System.Drawing.Font("Tahoma", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_lblInstructions1.ForeColor = System.Drawing.Color.Yellow;
            this.m_lblInstructions1.Location = new System.Drawing.Point(177, 491);
            this.m_lblInstructions1.Name = "m_lblInstructions1";
            this.m_lblInstructions1.Size = new System.Drawing.Size(670, 88);
            this.m_lblInstructions1.TabIndex = 29;
            this.m_lblInstructions1.Text = "All money deposited will be used for B3 and\r\nno change will be given.";
            this.m_lblInstructions1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // m_lblInstructions2
            // 
            this.m_lblInstructions2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.m_lblInstructions2.BackColor = System.Drawing.Color.Transparent;
            this.m_lblInstructions2.EdgeColor = System.Drawing.Color.Black;
            this.m_lblInstructions2.Font = new System.Drawing.Font("Tahoma", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_lblInstructions2.ForeColor = System.Drawing.Color.Chartreuse;
            this.m_lblInstructions2.Location = new System.Drawing.Point(177, 591);
            this.m_lblInstructions2.Name = "m_lblInstructions2";
            this.m_lblInstructions2.Size = new System.Drawing.Size(670, 93);
            this.m_lblInstructions2.TabIndex = 28;
            this.m_lblInstructions2.Text = "Print the B3 ticket when you are finished entering money.";
            this.m_lblInstructions2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.m_lblInstructions2.Visible = false;
            // 
            // m_btnB3Test10
            // 
            this.m_btnB3Test10.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.m_btnB3Test10.BackColor = System.Drawing.Color.Transparent;
            this.m_btnB3Test10.FocusColor = System.Drawing.Color.Black;
            this.m_btnB3Test10.Font = new System.Drawing.Font("Microsoft Sans Serif", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_btnB3Test10.ImageNormal = global::GTI.Modules.POS.Properties.Resources.GreenButtonUp;
            this.m_btnB3Test10.Location = new System.Drawing.Point(887, 500);
            this.m_btnB3Test10.MinimumSize = new System.Drawing.Size(30, 30);
            this.m_btnB3Test10.Name = "m_btnB3Test10";
            this.m_btnB3Test10.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_btnB3Test10.Size = new System.Drawing.Size(125, 60);
            this.m_btnB3Test10.TabIndex = 27;
            this.m_btnB3Test10.Text = "$10";
            this.m_btnB3Test10.UseVisualStyleBackColor = false;
            this.m_btnB3Test10.Visible = false;
            this.m_btnB3Test10.Click += new System.EventHandler(this.m_btnB3Test_Click);
            // 
            // m_btnB3Test5
            // 
            this.m_btnB3Test5.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.m_btnB3Test5.BackColor = System.Drawing.Color.Transparent;
            this.m_btnB3Test5.FocusColor = System.Drawing.Color.Black;
            this.m_btnB3Test5.Font = new System.Drawing.Font("Microsoft Sans Serif", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_btnB3Test5.ImageNormal = global::GTI.Modules.POS.Properties.Resources.GreenButtonUp;
            this.m_btnB3Test5.Location = new System.Drawing.Point(887, 402);
            this.m_btnB3Test5.MinimumSize = new System.Drawing.Size(30, 30);
            this.m_btnB3Test5.Name = "m_btnB3Test5";
            this.m_btnB3Test5.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_btnB3Test5.Size = new System.Drawing.Size(125, 60);
            this.m_btnB3Test5.TabIndex = 26;
            this.m_btnB3Test5.Text = "$5";
            this.m_btnB3Test5.UseVisualStyleBackColor = false;
            this.m_btnB3Test5.Visible = false;
            this.m_btnB3Test5.Click += new System.EventHandler(this.m_btnB3Test_Click);
            // 
            // m_btnB3Test1
            // 
            this.m_btnB3Test1.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.m_btnB3Test1.BackColor = System.Drawing.Color.Transparent;
            this.m_btnB3Test1.FocusColor = System.Drawing.Color.Black;
            this.m_btnB3Test1.Font = new System.Drawing.Font("Microsoft Sans Serif", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_btnB3Test1.ImageNormal = global::GTI.Modules.POS.Properties.Resources.GreenButtonUp;
            this.m_btnB3Test1.Location = new System.Drawing.Point(887, 305);
            this.m_btnB3Test1.MinimumSize = new System.Drawing.Size(30, 30);
            this.m_btnB3Test1.Name = "m_btnB3Test1";
            this.m_btnB3Test1.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_btnB3Test1.Size = new System.Drawing.Size(125, 60);
            this.m_btnB3Test1.TabIndex = 25;
            this.m_btnB3Test1.Text = "$1";
            this.m_btnB3Test1.UseVisualStyleBackColor = false;
            this.m_btnB3Test1.Visible = false;
            this.m_btnB3Test1.Click += new System.EventHandler(this.m_btnB3Test_Click);
            // 
            // m_timeoutProgress
            // 
            this.m_timeoutProgress.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.m_timeoutProgress.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(127)))), ((int)(((byte)(127)))));
            this.m_timeoutProgress.ForeColor = System.Drawing.Color.Gold;
            this.m_timeoutProgress.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.m_timeoutProgress.Location = new System.Drawing.Point(241, 756);
            this.m_timeoutProgress.Name = "m_timeoutProgress";
            this.m_timeoutProgress.Size = new System.Drawing.Size(542, 10);
            this.m_timeoutProgress.TabIndex = 24;
            this.m_timeoutProgress.Visible = false;
            this.m_timeoutProgress.Click += new System.EventHandler(this.UserActivityDetected);
            // 
            // m_pictureB3Logo
            // 
            this.m_pictureB3Logo.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.m_pictureB3Logo.BackgroundImage = global::GTI.Modules.POS.Properties.Resources.B3KioskLogo;
            this.m_pictureB3Logo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.m_pictureB3Logo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.m_pictureB3Logo.Location = new System.Drawing.Point(165, 13);
            this.m_pictureB3Logo.Name = "m_pictureB3Logo";
            this.m_pictureB3Logo.Size = new System.Drawing.Size(694, 288);
            this.m_pictureB3Logo.TabIndex = 10;
            this.m_pictureB3Logo.TabStop = false;
            this.m_pictureB3Logo.Click += new System.EventHandler(this.UserActivityDetected);
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
            this.m_lblTotal.TabIndex = 9;
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
            this.m_lblTotalLabel.TabIndex = 8;
            this.m_lblTotalLabel.Text = "Total:";
            this.m_lblTotalLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.m_lblTotalLabel.Click += new System.EventHandler(this.UserActivityDetected);
            // 
            // m_lblInstructions
            // 
            this.m_lblInstructions.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.m_lblInstructions.BackColor = System.Drawing.Color.Transparent;
            this.m_lblInstructions.EdgeColor = System.Drawing.Color.Black;
            this.m_lblInstructions.Font = new System.Drawing.Font("Tahoma", 36F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_lblInstructions.ForeColor = System.Drawing.Color.Aqua;
            this.m_lblInstructions.Location = new System.Drawing.Point(177, 305);
            this.m_lblInstructions.Name = "m_lblInstructions";
            this.m_lblInstructions.Size = new System.Drawing.Size(670, 184);
            this.m_lblInstructions.TabIndex = 7;
            this.m_lblInstructions.Text = "Insert bills to apply toward B3.\r\n";
            this.m_lblInstructions.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.m_lblInstructions.Click += new System.EventHandler(this.UserActivityDetected);
            // 
            // m_btnBuy
            // 
            this.m_btnBuy.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.m_btnBuy.BackColor = System.Drawing.Color.Transparent;
            this.m_btnBuy.Debounce = true;
            this.m_btnBuy.FocusColor = System.Drawing.Color.Black;
            this.m_btnBuy.Font = new System.Drawing.Font("Tahoma", 36F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_btnBuy.ImageNormal = global::GTI.Modules.POS.Properties.Resources.GreenButtonUp;
            this.m_btnBuy.ImagePressed = global::GTI.Modules.POS.Properties.Resources.GreenButtonDown;
            this.m_btnBuy.Location = new System.Drawing.Point(792, 689);
            this.m_btnBuy.MinimumSize = new System.Drawing.Size(30, 30);
            this.m_btnBuy.Name = "m_btnBuy";
            this.m_btnBuy.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_btnBuy.ShowFocus = false;
            this.m_btnBuy.Size = new System.Drawing.Size(220, 67);
            this.m_btnBuy.TabIndex = 5;
            this.m_btnBuy.Text = "Print";
            this.m_btnBuy.UseVisualStyleBackColor = false;
            this.m_btnBuy.Visible = false;
            this.m_btnBuy.Click += new System.EventHandler(this.m_btnBuy_Click);
            // 
            // m_btnQuit
            // 
            this.m_btnQuit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
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
            this.m_btnQuit.TabIndex = 4;
            this.m_btnQuit.Text = "Quit";
            this.m_btnQuit.UseVisualStyleBackColor = false;
            this.m_btnQuit.Click += new System.EventHandler(this.m_btnQuit_Click);
            // 
            // B3KioskForm
            // 
            this.ClientSize = new System.Drawing.Size(1024, 768);
            this.Controls.Add(this.m_lblInstructions1);
            this.Controls.Add(this.m_lblInstructions2);
            this.Controls.Add(this.m_btnB3Test10);
            this.Controls.Add(this.m_btnB3Test5);
            this.Controls.Add(this.m_btnB3Test1);
            this.Controls.Add(this.m_timeoutProgress);
            this.Controls.Add(this.m_pictureB3Logo);
            this.Controls.Add(this.m_lblTotal);
            this.Controls.Add(this.m_lblTotalLabel);
            this.Controls.Add(this.m_lblInstructions);
            this.Controls.Add(this.m_btnBuy);
            this.Controls.Add(this.m_btnQuit);
            this.DoubleBuffered = true;
            this.DrawAsGradient = true;
            this.DrawBorderOuterEdge = true;
            this.DrawRounded = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.GradientBeginColor = System.Drawing.Color.FromArgb(((int)(((byte)(214)))), ((int)(((byte)(211)))), ((int)(((byte)(216)))));
            this.GradientEndColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(127)))), ((int)(((byte)(127)))));
            this.MinimumSize = new System.Drawing.Size(1024, 768);
            this.Name = "B3KioskForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.B3KioskForm_FormClosing);
            this.Shown += new System.EventHandler(this.B3KioskForm_Shown);
            this.Click += new System.EventHandler(this.UserActivityDetected);
            ((System.ComponentModel.ISupportInitialize)(this.m_pictureB3Logo)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.ImageButton m_btnQuit;
        private Controls.ImageButton m_btnBuy;
        private Controls.OutlinedLabel m_lblInstructions;
        private System.Windows.Forms.Label m_lblTotalLabel;
        private System.Windows.Forms.Label m_lblTotal;
        private System.Windows.Forms.PictureBox m_pictureB3Logo;
        private System.Windows.Forms.ProgressBar m_timeoutProgress;
        private System.Windows.Forms.Timer m_kioskTimer;
        private Controls.ImageButton m_btnB3Test1;
        private Controls.ImageButton m_btnB3Test5;
        private Controls.ImageButton m_btnB3Test10;
        private Controls.OutlinedLabel m_lblInstructions2;
        private Controls.OutlinedLabel m_lblInstructions1;
    }
}
