namespace GTI.Modules.POS.UI
{
    partial class PayReceiptsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PayReceiptsForm));
            this.m_superPickList = new GTI.Controls.ColorListBox();
            this.m_receiptListUpButton = new GTI.Controls.ImageButton();
            this.m_receiptListDownButton = new GTI.Controls.ImageButton();
            this.m_keypad = new GTI.Controls.Keypad();
            this.m_exitButton = new GTI.Controls.ImageButton();
            this.m_payButton = new GTI.Controls.ImageButton();
            this.m_pickTypeLabel = new System.Windows.Forms.Label();
            this.m_pickNumbersLabel = new System.Windows.Forms.Label();
            this.m_pricelLabel = new System.Windows.Forms.Label();
            this.m_scanBarcodeButton = new GTI.Controls.ImageButton();
            this.m_winnersListUpButton = new GTI.Controls.ImageButton();
            this.m_winnersListDownButton = new GTI.Controls.ImageButton();
            this.m_addWinnerNumbers = new GTI.Controls.ImageButton();
            this.m_removeWinnerNumbers = new GTI.Controls.ImageButton();
            this.label1 = new System.Windows.Forms.Label();
            this.m_viewPlayerInfo = new GTI.Controls.ImageButton();
            this.m_superPickWinnersList = new GTI.Controls.ColorListBox();
            this.SuspendLayout();
            // 
            // m_superPickList
            // 
            this.m_superPickList.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(75)))), ((int)(((byte)(119)))));
            this.m_superPickList.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.m_superPickList.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            resources.ApplyResources(this.m_superPickList, "m_superPickList");
            this.m_superPickList.ForeColor = System.Drawing.Color.Yellow;
            this.m_superPickList.FormattingEnabled = true;
            this.m_superPickList.HighlightColor = System.Drawing.Color.ForestGreen;
            this.m_superPickList.Name = "m_superPickList";
            // 
            // m_receiptListUpButton
            // 
            this.m_receiptListUpButton.BackColor = System.Drawing.Color.Silver;
            this.m_receiptListUpButton.ImageIcon = global::GTI.Modules.POS.Properties.Resources.ArrowUp;
            this.m_receiptListUpButton.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_receiptListUpButton.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            resources.ApplyResources(this.m_receiptListUpButton, "m_receiptListUpButton");
            this.m_receiptListUpButton.Name = "m_receiptListUpButton";
            this.m_receiptListUpButton.UseVisualStyleBackColor = false;
            this.m_receiptListUpButton.Click += new System.EventHandler(this.ReceiptListUpClick);
            // 
            // m_receiptListDownButton
            // 
            this.m_receiptListDownButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(167)))), ((int)(((byte)(171)))), ((int)(((byte)(178)))));
            this.m_receiptListDownButton.ImageIcon = global::GTI.Modules.POS.Properties.Resources.ArrowDown;
            this.m_receiptListDownButton.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_receiptListDownButton.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            resources.ApplyResources(this.m_receiptListDownButton, "m_receiptListDownButton");
            this.m_receiptListDownButton.Name = "m_receiptListDownButton";
            this.m_receiptListDownButton.UseVisualStyleBackColor = false;
            this.m_receiptListDownButton.Click += new System.EventHandler(this.ReceiptListDownClick);
            // 
            // m_keypad
            // 
            this.m_keypad.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(117)))), ((int)(((byte)(104)))), ((int)(((byte)(99)))));
            this.m_keypad.BigButtonFont = new System.Drawing.Font("Tahoma", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_keypad.BigButtonImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_keypad.BigButtonImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_keypad.BigButtonText = "Enter";
            this.m_keypad.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.m_keypad.CurrencySymbolForeColor = System.Drawing.Color.White;
            resources.ApplyResources(this.m_keypad, "m_keypad");
            this.m_keypad.KeyMode = GTI.Controls.Keypad.KeypadMode.Calculator;
            this.m_keypad.Name = "m_keypad";
            this.m_keypad.NumberDisplayMode = GTI.Controls.Keypad.NumberMode.Integer;
            this.m_keypad.NumbersFont = new System.Drawing.Font("Tahoma", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_keypad.NumbersImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_keypad.NumbersImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_keypad.OptionButtonsFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_keypad.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.m_keypad.ValueBackground = global::GTI.Modules.POS.Properties.Resources.TextBack;
            this.m_keypad.ValueForeColor = System.Drawing.Color.Yellow;
            this.m_keypad.BigButtonClick += new System.EventHandler(this.KeyPadEnterClick);
            // 
            // m_exitButton
            // 
            this.m_exitButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(205)))), ((int)(((byte)(201)))));
            resources.ApplyResources(this.m_exitButton, "m_exitButton");
            this.m_exitButton.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_exitButton.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_exitButton.Name = "m_exitButton";
            this.m_exitButton.UseVisualStyleBackColor = false;
            this.m_exitButton.Click += new System.EventHandler(this.ExitClick);
            // 
            // m_payButton
            // 
            this.m_payButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(135)))), ((int)(((byte)(135)))), ((int)(((byte)(133)))));
            resources.ApplyResources(this.m_payButton, "m_payButton");
            this.m_payButton.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_payButton.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_payButton.Name = "m_payButton";
            this.m_payButton.UseVisualStyleBackColor = false;
            this.m_payButton.Click += new System.EventHandler(this.PayClick);
            // 
            // m_pickTypeLabel
            // 
            this.m_pickTypeLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(205)))), ((int)(((byte)(212)))));
            resources.ApplyResources(this.m_pickTypeLabel, "m_pickTypeLabel");
            this.m_pickTypeLabel.Name = "m_pickTypeLabel";
            // 
            // m_pickNumbersLabel
            // 
            this.m_pickNumbersLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(205)))), ((int)(((byte)(212)))));
            resources.ApplyResources(this.m_pickNumbersLabel, "m_pickNumbersLabel");
            this.m_pickNumbersLabel.Name = "m_pickNumbersLabel";
            // 
            // m_pricelLabel
            // 
            this.m_pricelLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(205)))), ((int)(((byte)(212)))));
            resources.ApplyResources(this.m_pricelLabel, "m_pricelLabel");
            this.m_pricelLabel.Name = "m_pricelLabel";
            // 
            // m_scanBarcodeButton
            // 
            this.m_scanBarcodeButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(205)))), ((int)(((byte)(201)))));
            resources.ApplyResources(this.m_scanBarcodeButton, "m_scanBarcodeButton");
            this.m_scanBarcodeButton.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_scanBarcodeButton.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_scanBarcodeButton.Name = "m_scanBarcodeButton";
            this.m_scanBarcodeButton.UseVisualStyleBackColor = false;
            this.m_scanBarcodeButton.Click += new System.EventHandler(this.ScanBarcodeClick);
            // 
            // m_winnersListUpButton
            // 
            this.m_winnersListUpButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(167)))), ((int)(((byte)(171)))), ((int)(((byte)(178)))));
            this.m_winnersListUpButton.ImageIcon = global::GTI.Modules.POS.Properties.Resources.ArrowUp;
            this.m_winnersListUpButton.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_winnersListUpButton.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            resources.ApplyResources(this.m_winnersListUpButton, "m_winnersListUpButton");
            this.m_winnersListUpButton.Name = "m_winnersListUpButton";
            this.m_winnersListUpButton.UseVisualStyleBackColor = false;
            this.m_winnersListUpButton.Click += new System.EventHandler(this.WinnersListUpButtonClick);
            // 
            // m_winnersListDownButton
            // 
            this.m_winnersListDownButton.BackColor = System.Drawing.Color.Gray;
            this.m_winnersListDownButton.ImageIcon = global::GTI.Modules.POS.Properties.Resources.ArrowDown;
            this.m_winnersListDownButton.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_winnersListDownButton.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            resources.ApplyResources(this.m_winnersListDownButton, "m_winnersListDownButton");
            this.m_winnersListDownButton.Name = "m_winnersListDownButton";
            this.m_winnersListDownButton.UseVisualStyleBackColor = false;
            this.m_winnersListDownButton.Click += new System.EventHandler(this.WinnersListDownButtonClick);
            // 
            // m_addWinnerNumbers
            // 
            this.m_addWinnerNumbers.BackColor = System.Drawing.Color.DarkGray;
            resources.ApplyResources(this.m_addWinnerNumbers, "m_addWinnerNumbers");
            this.m_addWinnerNumbers.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_addWinnerNumbers.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_addWinnerNumbers.Name = "m_addWinnerNumbers";
            this.m_addWinnerNumbers.UseVisualStyleBackColor = false;
            this.m_addWinnerNumbers.Click += new System.EventHandler(this.AddWinnerNumbersClick);
            // 
            // m_removeWinnerNumbers
            // 
            this.m_removeWinnerNumbers.BackColor = System.Drawing.Color.DarkGray;
            resources.ApplyResources(this.m_removeWinnerNumbers, "m_removeWinnerNumbers");
            this.m_removeWinnerNumbers.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_removeWinnerNumbers.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_removeWinnerNumbers.Name = "m_removeWinnerNumbers";
            this.m_removeWinnerNumbers.UseVisualStyleBackColor = false;
            this.m_removeWinnerNumbers.Click += new System.EventHandler(this.RemoveWinnerNumbersClick);
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(205)))), ((int)(((byte)(212)))));
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // m_viewPlayerInfo
            // 
            this.m_viewPlayerInfo.BackColor = System.Drawing.Color.DarkGray;
            resources.ApplyResources(this.m_viewPlayerInfo, "m_viewPlayerInfo");
            this.m_viewPlayerInfo.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_viewPlayerInfo.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_viewPlayerInfo.Name = "m_viewPlayerInfo";
            this.m_viewPlayerInfo.UseVisualStyleBackColor = false;
            this.m_viewPlayerInfo.Click += new System.EventHandler(this.ViewPlayerInfoClick);
            // 
            // m_superPickWinnersList
            // 
            this.m_superPickWinnersList.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(75)))), ((int)(((byte)(119)))));
            this.m_superPickWinnersList.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.m_superPickWinnersList.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            resources.ApplyResources(this.m_superPickWinnersList, "m_superPickWinnersList");
            this.m_superPickWinnersList.ForeColor = System.Drawing.Color.Yellow;
            this.m_superPickWinnersList.FormattingEnabled = true;
            this.m_superPickWinnersList.HighlightColor = System.Drawing.Color.ForestGreen;
            this.m_superPickWinnersList.Name = "m_superPickWinnersList";
            // 
            // PayReceiptsForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackgroundImage = global::GTI.Modules.POS.Properties.Resources.PayReceiptsBack1024;
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.m_superPickWinnersList);
            this.Controls.Add(this.m_viewPlayerInfo);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.m_removeWinnerNumbers);
            this.Controls.Add(this.m_addWinnerNumbers);
            this.Controls.Add(this.m_winnersListDownButton);
            this.Controls.Add(this.m_winnersListUpButton);
            this.Controls.Add(this.m_scanBarcodeButton);
            this.Controls.Add(this.m_pricelLabel);
            this.Controls.Add(this.m_pickNumbersLabel);
            this.Controls.Add(this.m_pickTypeLabel);
            this.Controls.Add(this.m_payButton);
            this.Controls.Add(this.m_exitButton);
            this.Controls.Add(this.m_keypad);
            this.Controls.Add(this.m_receiptListDownButton);
            this.Controls.Add(this.m_receiptListUpButton);
            this.Controls.Add(this.m_superPickList);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "PayReceiptsForm";
            this.ShowInTaskbar = false;
            this.ResumeLayout(false);

        }

        #endregion

        private GTI.Controls.ColorListBox m_superPickList;
        private GTI.Controls.ImageButton m_receiptListUpButton;
        private GTI.Controls.ImageButton m_receiptListDownButton;
        private GTI.Controls.Keypad m_keypad;
        private GTI.Controls.ImageButton m_exitButton;
        private GTI.Controls.ImageButton m_payButton;
        private System.Windows.Forms.Label m_pickTypeLabel;
        private System.Windows.Forms.Label m_pickNumbersLabel;
        private System.Windows.Forms.Label m_pricelLabel;
        private GTI.Controls.ImageButton m_scanBarcodeButton;
        private GTI.Controls.ImageButton m_winnersListUpButton;
        private GTI.Controls.ImageButton m_winnersListDownButton;
        private GTI.Controls.ImageButton m_addWinnerNumbers;
        private GTI.Controls.ImageButton m_removeWinnerNumbers;
        private System.Windows.Forms.Label label1;
        private GTI.Controls.ImageButton m_viewPlayerInfo;
        private GTI.Controls.ColorListBox m_superPickWinnersList;
    }
}