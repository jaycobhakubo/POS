namespace GTI.Modules.POS.UI
{
    partial class InitialBankForm
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
            if(disposing && (components != null))
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InitialBankForm));
            this.m_panelMain = new GTI.Controls.EliteGradientPanel();
            this.m_errorCountLabel = new System.Windows.Forms.Label();
            this.m_okButton = new GTI.Controls.ImageButton();
            this.m_currencyPanel = new GTI.Modules.POS.UI.NoScrollBarsPanel();
            this.m_downButton = new GTI.Controls.ImageButton();
            this.m_upButton = new GTI.Controls.ImageButton();
            this.m_decimalButton = new GTI.Controls.ImageButton();
            this.m_clearButton = new GTI.Controls.ImageButton();
            this.m_button2 = new GTI.Controls.ImageButton();
            this.m_button3 = new GTI.Controls.ImageButton();
            this.m_button6 = new GTI.Controls.ImageButton();
            this.m_button5 = new GTI.Controls.ImageButton();
            this.m_button4 = new GTI.Controls.ImageButton();
            this.m_button7 = new GTI.Controls.ImageButton();
            this.m_button8 = new GTI.Controls.ImageButton();
            this.m_button9 = new GTI.Controls.ImageButton();
            this.m_button0 = new GTI.Controls.ImageButton();
            this.m_button1 = new GTI.Controls.ImageButton();
            this.m_titleLabel = new System.Windows.Forms.Label();
            this.m_panelMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_panelMain
            // 
            this.m_panelMain.BackColor = System.Drawing.Color.Transparent;
            this.m_panelMain.BackgroundImage = global::GTI.Modules.POS.Properties.Resources.InitialBankBack1024;
            resources.ApplyResources(this.m_panelMain, "m_panelMain");
            this.m_panelMain.BorderColor = System.Drawing.Color.Silver;
            this.m_panelMain.Controls.Add(this.m_errorCountLabel);
            this.m_panelMain.Controls.Add(this.m_okButton);
            this.m_panelMain.Controls.Add(this.m_currencyPanel);
            this.m_panelMain.Controls.Add(this.m_downButton);
            this.m_panelMain.Controls.Add(this.m_upButton);
            this.m_panelMain.Controls.Add(this.m_decimalButton);
            this.m_panelMain.Controls.Add(this.m_clearButton);
            this.m_panelMain.Controls.Add(this.m_button2);
            this.m_panelMain.Controls.Add(this.m_button3);
            this.m_panelMain.Controls.Add(this.m_button6);
            this.m_panelMain.Controls.Add(this.m_button5);
            this.m_panelMain.Controls.Add(this.m_button4);
            this.m_panelMain.Controls.Add(this.m_button7);
            this.m_panelMain.Controls.Add(this.m_button8);
            this.m_panelMain.Controls.Add(this.m_button9);
            this.m_panelMain.Controls.Add(this.m_button0);
            this.m_panelMain.Controls.Add(this.m_button1);
            this.m_panelMain.Controls.Add(this.m_titleLabel);
            this.m_panelMain.DrawBorderOuterEdge = true;
            this.m_panelMain.DrawRounded = true;
            this.m_panelMain.GradientBeginColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(179)))), ((int)(((byte)(213)))));
            this.m_panelMain.GradientEndColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(186)))), ((int)(((byte)(192)))));
            this.m_panelMain.InnerBorderEdgeColor = System.Drawing.Color.SlateGray;
            this.m_panelMain.Name = "m_panelMain";
            this.m_panelMain.OuterBorderEdgeColor = System.Drawing.Color.Black;
            // 
            // m_errorCountLabel
            // 
            this.m_errorCountLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(75)))), ((int)(((byte)(117)))));
            resources.ApplyResources(this.m_errorCountLabel, "m_errorCountLabel");
            this.m_errorCountLabel.ForeColor = System.Drawing.Color.White;
            this.m_errorCountLabel.Name = "m_errorCountLabel";
            // 
            // m_okButton
            // 
            this.m_okButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(129)))), ((int)(((byte)(129)))), ((int)(((byte)(129)))));
            this.m_okButton.FocusColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.m_okButton, "m_okButton");
            this.m_okButton.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_okButton.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_okButton.Name = "m_okButton";
            this.m_okButton.RepeatRate = 150;
            this.m_okButton.RepeatWhenHeldFor = 750;
            this.m_okButton.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_okButton.ShowFocus = false;
            this.m_okButton.TabStop = false;
            this.m_okButton.UseMnemonic = false;
            this.m_okButton.UseVisualStyleBackColor = false;
            this.m_okButton.Click += new System.EventHandler(this.OkClick);
            // 
            // m_currencyPanel
            // 
            resources.ApplyResources(this.m_currencyPanel, "m_currencyPanel");
            this.m_currencyPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(87)))), ((int)(((byte)(83)))));
            this.m_currencyPanel.BorderColor = System.Drawing.Color.Silver;
            this.m_currencyPanel.GradientBeginColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(179)))), ((int)(((byte)(213)))));
            this.m_currencyPanel.GradientEndColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(186)))), ((int)(((byte)(192)))));
            this.m_currencyPanel.InnerBorderEdgeColor = System.Drawing.Color.SlateGray;
            this.m_currencyPanel.Name = "m_currencyPanel";
            this.m_currencyPanel.OuterBorderEdgeColor = System.Drawing.Color.SlateGray;
            // 
            // m_downButton
            // 
            this.m_downButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(87)))), ((int)(((byte)(83)))));
            this.m_downButton.FocusColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.m_downButton, "m_downButton");
            this.m_downButton.ImageIcon = global::GTI.Modules.POS.Properties.Resources.ArrowDown;
            this.m_downButton.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_downButton.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_downButton.Name = "m_downButton";
            this.m_downButton.RepeatRate = 150;
            this.m_downButton.RepeatWhenHeldFor = 750;
            this.m_downButton.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_downButton.ShowFocus = false;
            this.m_downButton.TabStop = false;
            this.m_downButton.UseMnemonic = false;
            this.m_downButton.UseVisualStyleBackColor = false;
            this.m_downButton.Click += new System.EventHandler(this.DownButtonClick);
            // 
            // m_upButton
            // 
            this.m_upButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(87)))), ((int)(((byte)(83)))));
            this.m_upButton.FocusColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.m_upButton, "m_upButton");
            this.m_upButton.ImageIcon = global::GTI.Modules.POS.Properties.Resources.ArrowUp;
            this.m_upButton.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_upButton.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_upButton.Name = "m_upButton";
            this.m_upButton.RepeatRate = 150;
            this.m_upButton.RepeatWhenHeldFor = 750;
            this.m_upButton.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_upButton.ShowFocus = false;
            this.m_upButton.TabStop = false;
            this.m_upButton.UseMnemonic = false;
            this.m_upButton.UseVisualStyleBackColor = false;
            this.m_upButton.Click += new System.EventHandler(this.UpButtonClick);
            // 
            // m_decimalButton
            // 
            this.m_decimalButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(87)))), ((int)(((byte)(83)))));
            this.m_decimalButton.FocusColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.m_decimalButton, "m_decimalButton");
            this.m_decimalButton.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_decimalButton.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_decimalButton.Name = "m_decimalButton";
            this.m_decimalButton.RepeatRate = 150;
            this.m_decimalButton.RepeatWhenHeldFor = 750;
            this.m_decimalButton.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_decimalButton.ShowFocus = false;
            this.m_decimalButton.TabStop = false;
            this.m_decimalButton.UseMnemonic = false;
            this.m_decimalButton.UseVisualStyleBackColor = false;
            this.m_decimalButton.Click += new System.EventHandler(this.NumberClick);
            // 
            // m_clearButton
            // 
            this.m_clearButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(87)))), ((int)(((byte)(83)))));
            this.m_clearButton.FocusColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.m_clearButton, "m_clearButton");
            this.m_clearButton.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_clearButton.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_clearButton.Name = "m_clearButton";
            this.m_clearButton.RepeatRate = 150;
            this.m_clearButton.RepeatWhenHeldFor = 750;
            this.m_clearButton.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_clearButton.ShowFocus = false;
            this.m_clearButton.TabStop = false;
            this.m_clearButton.UseMnemonic = false;
            this.m_clearButton.UseVisualStyleBackColor = false;
            this.m_clearButton.Click += new System.EventHandler(this.ClearClick);
            // 
            // m_button2
            // 
            this.m_button2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(87)))), ((int)(((byte)(83)))));
            this.m_button2.FocusColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.m_button2, "m_button2");
            this.m_button2.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_button2.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_button2.Name = "m_button2";
            this.m_button2.RepeatRate = 150;
            this.m_button2.RepeatWhenHeldFor = 750;
            this.m_button2.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_button2.ShowFocus = false;
            this.m_button2.TabStop = false;
            this.m_button2.UseMnemonic = false;
            this.m_button2.UseVisualStyleBackColor = false;
            this.m_button2.Click += new System.EventHandler(this.NumberClick);
            // 
            // m_button3
            // 
            this.m_button3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(87)))), ((int)(((byte)(83)))));
            this.m_button3.FocusColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.m_button3, "m_button3");
            this.m_button3.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_button3.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_button3.Name = "m_button3";
            this.m_button3.RepeatRate = 150;
            this.m_button3.RepeatWhenHeldFor = 750;
            this.m_button3.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_button3.ShowFocus = false;
            this.m_button3.TabStop = false;
            this.m_button3.UseMnemonic = false;
            this.m_button3.UseVisualStyleBackColor = false;
            this.m_button3.Click += new System.EventHandler(this.NumberClick);
            // 
            // m_button6
            // 
            this.m_button6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(87)))), ((int)(((byte)(83)))));
            this.m_button6.FocusColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.m_button6, "m_button6");
            this.m_button6.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_button6.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_button6.Name = "m_button6";
            this.m_button6.RepeatRate = 150;
            this.m_button6.RepeatWhenHeldFor = 750;
            this.m_button6.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_button6.ShowFocus = false;
            this.m_button6.TabStop = false;
            this.m_button6.UseMnemonic = false;
            this.m_button6.UseVisualStyleBackColor = false;
            this.m_button6.Click += new System.EventHandler(this.NumberClick);
            // 
            // m_button5
            // 
            this.m_button5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(87)))), ((int)(((byte)(83)))));
            this.m_button5.FocusColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.m_button5, "m_button5");
            this.m_button5.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_button5.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_button5.Name = "m_button5";
            this.m_button5.RepeatRate = 150;
            this.m_button5.RepeatWhenHeldFor = 750;
            this.m_button5.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_button5.ShowFocus = false;
            this.m_button5.TabStop = false;
            this.m_button5.UseMnemonic = false;
            this.m_button5.UseVisualStyleBackColor = false;
            this.m_button5.Click += new System.EventHandler(this.NumberClick);
            // 
            // m_button4
            // 
            this.m_button4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(87)))), ((int)(((byte)(83)))));
            this.m_button4.FocusColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.m_button4, "m_button4");
            this.m_button4.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_button4.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_button4.Name = "m_button4";
            this.m_button4.RepeatRate = 150;
            this.m_button4.RepeatWhenHeldFor = 750;
            this.m_button4.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_button4.ShowFocus = false;
            this.m_button4.TabStop = false;
            this.m_button4.UseMnemonic = false;
            this.m_button4.UseVisualStyleBackColor = false;
            this.m_button4.Click += new System.EventHandler(this.NumberClick);
            // 
            // m_button7
            // 
            this.m_button7.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(87)))), ((int)(((byte)(83)))));
            this.m_button7.FocusColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.m_button7, "m_button7");
            this.m_button7.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_button7.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_button7.Name = "m_button7";
            this.m_button7.RepeatRate = 150;
            this.m_button7.RepeatWhenHeldFor = 750;
            this.m_button7.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_button7.ShowFocus = false;
            this.m_button7.TabStop = false;
            this.m_button7.UseMnemonic = false;
            this.m_button7.UseVisualStyleBackColor = false;
            this.m_button7.Click += new System.EventHandler(this.NumberClick);
            // 
            // m_button8
            // 
            this.m_button8.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(87)))), ((int)(((byte)(83)))));
            this.m_button8.FocusColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.m_button8, "m_button8");
            this.m_button8.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_button8.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_button8.Name = "m_button8";
            this.m_button8.RepeatRate = 150;
            this.m_button8.RepeatWhenHeldFor = 750;
            this.m_button8.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_button8.ShowFocus = false;
            this.m_button8.TabStop = false;
            this.m_button8.UseMnemonic = false;
            this.m_button8.UseVisualStyleBackColor = false;
            this.m_button8.Click += new System.EventHandler(this.NumberClick);
            // 
            // m_button9
            // 
            this.m_button9.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(87)))), ((int)(((byte)(83)))));
            this.m_button9.FocusColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.m_button9, "m_button9");
            this.m_button9.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_button9.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_button9.Name = "m_button9";
            this.m_button9.RepeatRate = 150;
            this.m_button9.RepeatWhenHeldFor = 750;
            this.m_button9.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_button9.ShowFocus = false;
            this.m_button9.TabStop = false;
            this.m_button9.UseMnemonic = false;
            this.m_button9.UseVisualStyleBackColor = false;
            this.m_button9.Click += new System.EventHandler(this.NumberClick);
            // 
            // m_button0
            // 
            this.m_button0.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(87)))), ((int)(((byte)(83)))));
            this.m_button0.FocusColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.m_button0, "m_button0");
            this.m_button0.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_button0.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_button0.Name = "m_button0";
            this.m_button0.RepeatRate = 150;
            this.m_button0.RepeatWhenHeldFor = 750;
            this.m_button0.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_button0.ShowFocus = false;
            this.m_button0.TabStop = false;
            this.m_button0.UseMnemonic = false;
            this.m_button0.UseVisualStyleBackColor = false;
            this.m_button0.Click += new System.EventHandler(this.NumberClick);
            // 
            // m_button1
            // 
            this.m_button1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(87)))), ((int)(((byte)(83)))));
            this.m_button1.FocusColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.m_button1, "m_button1");
            this.m_button1.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_button1.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_button1.Name = "m_button1";
            this.m_button1.RepeatRate = 150;
            this.m_button1.RepeatWhenHeldFor = 750;
            this.m_button1.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_button1.ShowFocus = false;
            this.m_button1.TabStop = false;
            this.m_button1.UseMnemonic = false;
            this.m_button1.UseVisualStyleBackColor = false;
            this.m_button1.Click += new System.EventHandler(this.NumberClick);
            // 
            // m_titleLabel
            // 
            this.m_titleLabel.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.m_titleLabel, "m_titleLabel");
            this.m_titleLabel.Name = "m_titleLabel";
            // 
            // InitialBankForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoValidate = System.Windows.Forms.AutoValidate.Disable;
            this.BackColor = System.Drawing.Color.Fuchsia;
            resources.ApplyResources(this, "$this");
            this.ControlBox = false;
            this.Controls.Add(this.m_panelMain);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.GradientBeginColor = System.Drawing.Color.DarkGray;
            this.GradientEndColor = System.Drawing.Color.LightGray;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "InitialBankForm";
            this.OuterBorderEdgeColor = System.Drawing.Color.DimGray;
            this.ShowInTaskbar = false;
            this.TransparencyKey = System.Drawing.Color.Fuchsia;
            this.m_panelMain.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label m_titleLabel;
        private GTI.Controls.ImageButton m_button0;
        private GTI.Controls.ImageButton m_button9;
        private GTI.Controls.ImageButton m_button8;
        private GTI.Controls.ImageButton m_button7;
        private GTI.Controls.ImageButton m_button4;
        private GTI.Controls.ImageButton m_button5;
        private GTI.Controls.ImageButton m_button6;
        private GTI.Controls.ImageButton m_button3;
        private GTI.Controls.ImageButton m_button2;
        private GTI.Controls.ImageButton m_button1;
        private GTI.Controls.ImageButton m_clearButton;
        private GTI.Controls.ImageButton m_decimalButton;
        private GTI.Controls.ImageButton m_upButton;
        private GTI.Controls.ImageButton m_downButton;
        private GTI.Modules.POS.UI.NoScrollBarsPanel m_currencyPanel;
        private GTI.Controls.ImageButton m_okButton;
        private System.Windows.Forms.Label m_errorCountLabel;
        private Controls.EliteGradientPanel m_panelMain;
    }
}