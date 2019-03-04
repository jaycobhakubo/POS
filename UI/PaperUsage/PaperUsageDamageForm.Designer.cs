namespace GTI.Modules.POS.UI.PaperUsage
{
    partial class PaperUsageDamageForm
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
            this.TitleLabel = new System.Windows.Forms.Label();
            this.EnterAuditNumberLabel = new System.Windows.Forms.Label();
            this.AuditNumberTextBox = new System.Windows.Forms.TextBox();
            this.DamagesLabel = new System.Windows.Forms.Label();
            this.CloseButton = new GTI.Controls.ImageButton();
            this.AddButton = new GTI.Controls.ImageButton();
            this.RemoveButton = new GTI.Controls.ImageButton();
            this.m_backButton = new GTI.Controls.ImageButton();
            this.m_decimalButton = new GTI.Controls.ImageButton();
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
            this.DamagedListBox = new System.Windows.Forms.ListBox();
            this.SerialLabel = new System.Windows.Forms.Label();
            this.NameLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // TitleLabel
            // 
            this.TitleLabel.AutoSize = true;
            this.TitleLabel.BackColor = System.Drawing.Color.Transparent;
            this.TitleLabel.Font = new System.Drawing.Font("Tahoma", 20F, System.Drawing.FontStyle.Bold);
            this.TitleLabel.ForeColor = System.Drawing.Color.Black;
            this.TitleLabel.Location = new System.Drawing.Point(154, 9);
            this.TitleLabel.Name = "TitleLabel";
            this.TitleLabel.Size = new System.Drawing.Size(140, 33);
            this.TitleLabel.TabIndex = 17;
            this.TitleLabel.Text = "Damages";
            this.TitleLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // EnterAuditNumberLabel
            // 
            this.EnterAuditNumberLabel.AutoSize = true;
            this.EnterAuditNumberLabel.BackColor = System.Drawing.Color.Transparent;
            this.EnterAuditNumberLabel.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.EnterAuditNumberLabel.ForeColor = System.Drawing.Color.Black;
            this.EnterAuditNumberLabel.Location = new System.Drawing.Point(194, 50);
            this.EnterAuditNumberLabel.Name = "EnterAuditNumberLabel";
            this.EnterAuditNumberLabel.Size = new System.Drawing.Size(210, 18);
            this.EnterAuditNumberLabel.TabIndex = 67;
            this.EnterAuditNumberLabel.Text = "Scan or enter audit number";
            // 
            // AuditNumberTextBox
            // 
            this.AuditNumberTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.AuditNumberTextBox.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.AuditNumberTextBox.HideSelection = false;
            this.AuditNumberTextBox.Location = new System.Drawing.Point(164, 74);
            this.AuditNumberTextBox.Name = "AuditNumberTextBox";
            this.AuditNumberTextBox.Size = new System.Drawing.Size(271, 27);
            this.AuditNumberTextBox.TabIndex = 68;
            this.AuditNumberTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.AuditNumberTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.AuditNumberTextBoxKeyPress);
            // 
            // DamagesLabel
            // 
            this.DamagesLabel.AutoSize = true;
            this.DamagesLabel.BackColor = System.Drawing.Color.Transparent;
            this.DamagesLabel.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DamagesLabel.ForeColor = System.Drawing.Color.Black;
            this.DamagesLabel.Location = new System.Drawing.Point(29, 50);
            this.DamagesLabel.Name = "DamagesLabel";
            this.DamagesLabel.Size = new System.Drawing.Size(109, 18);
            this.DamagesLabel.TabIndex = 73;
            this.DamagesLabel.Text = "Damaged List";
            // 
            // CloseButton
            // 
            this.CloseButton.BackColor = System.Drawing.Color.Transparent;
            this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.CloseButton.FocusColor = System.Drawing.Color.Black;
            this.CloseButton.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.CloseButton.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.CloseButton.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.CloseButton.Location = new System.Drawing.Point(295, 366);
            this.CloseButton.MinimumSize = new System.Drawing.Size(30, 30);
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.CloseButton.ShowFocus = false;
            this.CloseButton.Size = new System.Drawing.Size(135, 50);
            this.CloseButton.TabIndex = 77;
            this.CloseButton.TabStop = false;
            this.CloseButton.Text = "Close";
            this.CloseButton.UseMnemonic = false;
            this.CloseButton.UseVisualStyleBackColor = false;
            this.CloseButton.Click += new System.EventHandler(this.CloseClick);
            // 
            // AddButton
            // 
            this.AddButton.BackColor = System.Drawing.Color.Transparent;
            this.AddButton.FocusColor = System.Drawing.Color.Black;
            this.AddButton.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.AddButton.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.AddButton.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.AddButton.Location = new System.Drawing.Point(159, 366);
            this.AddButton.MinimumSize = new System.Drawing.Size(30, 30);
            this.AddButton.Name = "AddButton";
            this.AddButton.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.AddButton.ShowFocus = false;
            this.AddButton.Size = new System.Drawing.Size(130, 50);
            this.AddButton.TabIndex = 76;
            this.AddButton.TabStop = false;
            this.AddButton.Text = "Add";
            this.AddButton.UseMnemonic = false;
            this.AddButton.UseVisualStyleBackColor = false;
            this.AddButton.Click += new System.EventHandler(this.AddDamagedClick);
            // 
            // RemoveButton
            // 
            this.RemoveButton.BackColor = System.Drawing.Color.Transparent;
            this.RemoveButton.FocusColor = System.Drawing.Color.Black;
            this.RemoveButton.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.RemoveButton.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.RemoveButton.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.RemoveButton.Location = new System.Drawing.Point(19, 366);
            this.RemoveButton.MinimumSize = new System.Drawing.Size(30, 30);
            this.RemoveButton.Name = "RemoveButton";
            this.RemoveButton.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.RemoveButton.ShowFocus = false;
            this.RemoveButton.Size = new System.Drawing.Size(135, 50);
            this.RemoveButton.TabIndex = 75;
            this.RemoveButton.TabStop = false;
            this.RemoveButton.Text = "Remove";
            this.RemoveButton.UseMnemonic = false;
            this.RemoveButton.UseVisualStyleBackColor = false;
            this.RemoveButton.Click += new System.EventHandler(this.RemoveDamagedClick);
            // 
            // m_backButton
            // 
            this.m_backButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(87)))), ((int)(((byte)(83)))));
            this.m_backButton.FocusColor = System.Drawing.Color.Black;
            this.m_backButton.Font = new System.Drawing.Font("Tahoma", 24F, System.Drawing.FontStyle.Bold);
            this.m_backButton.ImageIcon = global::GTI.Modules.POS.Properties.Resources.ArrowLeft;
            this.m_backButton.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_backButton.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_backButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.m_backButton.Location = new System.Drawing.Point(347, 274);
            this.m_backButton.MinimumSize = new System.Drawing.Size(30, 30);
            this.m_backButton.Name = "m_backButton";
            this.m_backButton.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_backButton.ShowFocus = false;
            this.m_backButton.Size = new System.Drawing.Size(87, 52);
            this.m_backButton.TabIndex = 56;
            this.m_backButton.TabStop = false;
            this.m_backButton.UseMnemonic = false;
            this.m_backButton.UseVisualStyleBackColor = false;
            this.m_backButton.Click += new System.EventHandler(this.BackButtonClick);
            // 
            // m_decimalButton
            // 
            this.m_decimalButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(87)))), ((int)(((byte)(83)))));
            this.m_decimalButton.FocusColor = System.Drawing.Color.Black;
            this.m_decimalButton.Font = new System.Drawing.Font("Tahoma", 24F, System.Drawing.FontStyle.Bold);
            this.m_decimalButton.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_decimalButton.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_decimalButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.m_decimalButton.Location = new System.Drawing.Point(163, 274);
            this.m_decimalButton.MinimumSize = new System.Drawing.Size(30, 30);
            this.m_decimalButton.Name = "m_decimalButton";
            this.m_decimalButton.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_decimalButton.ShowFocus = false;
            this.m_decimalButton.Size = new System.Drawing.Size(87, 52);
            this.m_decimalButton.TabIndex = 55;
            this.m_decimalButton.TabStop = false;
            this.m_decimalButton.Text = "X";
            this.m_decimalButton.UseMnemonic = false;
            this.m_decimalButton.UseVisualStyleBackColor = false;
            this.m_decimalButton.Click += new System.EventHandler(this.ClearButtonClick);
            // 
            // m_button2
            // 
            this.m_button2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(87)))), ((int)(((byte)(83)))));
            this.m_button2.FocusColor = System.Drawing.Color.Black;
            this.m_button2.Font = new System.Drawing.Font("Tahoma", 24F, System.Drawing.FontStyle.Bold);
            this.m_button2.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_button2.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_button2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.m_button2.Location = new System.Drawing.Point(255, 218);
            this.m_button2.MinimumSize = new System.Drawing.Size(30, 30);
            this.m_button2.Name = "m_button2";
            this.m_button2.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_button2.ShowFocus = false;
            this.m_button2.Size = new System.Drawing.Size(87, 52);
            this.m_button2.TabIndex = 59;
            this.m_button2.TabStop = false;
            this.m_button2.Text = "2";
            this.m_button2.UseMnemonic = false;
            this.m_button2.UseVisualStyleBackColor = false;
            this.m_button2.Click += new System.EventHandler(this.PressThisKey);
            // 
            // m_button3
            // 
            this.m_button3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(87)))), ((int)(((byte)(83)))));
            this.m_button3.FocusColor = System.Drawing.Color.Black;
            this.m_button3.Font = new System.Drawing.Font("Tahoma", 24F, System.Drawing.FontStyle.Bold);
            this.m_button3.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_button3.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_button3.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.m_button3.Location = new System.Drawing.Point(347, 218);
            this.m_button3.MinimumSize = new System.Drawing.Size(30, 30);
            this.m_button3.Name = "m_button3";
            this.m_button3.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_button3.ShowFocus = false;
            this.m_button3.Size = new System.Drawing.Size(87, 52);
            this.m_button3.TabIndex = 60;
            this.m_button3.TabStop = false;
            this.m_button3.Text = "3";
            this.m_button3.UseMnemonic = false;
            this.m_button3.UseVisualStyleBackColor = false;
            this.m_button3.Click += new System.EventHandler(this.PressThisKey);
            // 
            // m_button6
            // 
            this.m_button6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(87)))), ((int)(((byte)(83)))));
            this.m_button6.FocusColor = System.Drawing.Color.Black;
            this.m_button6.Font = new System.Drawing.Font("Tahoma", 24F, System.Drawing.FontStyle.Bold);
            this.m_button6.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_button6.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_button6.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.m_button6.Location = new System.Drawing.Point(347, 162);
            this.m_button6.MinimumSize = new System.Drawing.Size(30, 30);
            this.m_button6.Name = "m_button6";
            this.m_button6.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_button6.ShowFocus = false;
            this.m_button6.Size = new System.Drawing.Size(87, 52);
            this.m_button6.TabIndex = 63;
            this.m_button6.TabStop = false;
            this.m_button6.Text = "6";
            this.m_button6.UseMnemonic = false;
            this.m_button6.UseVisualStyleBackColor = false;
            this.m_button6.Click += new System.EventHandler(this.PressThisKey);
            // 
            // m_button5
            // 
            this.m_button5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(87)))), ((int)(((byte)(83)))));
            this.m_button5.FocusColor = System.Drawing.Color.Black;
            this.m_button5.Font = new System.Drawing.Font("Tahoma", 24F, System.Drawing.FontStyle.Bold);
            this.m_button5.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_button5.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_button5.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.m_button5.Location = new System.Drawing.Point(255, 162);
            this.m_button5.MinimumSize = new System.Drawing.Size(30, 30);
            this.m_button5.Name = "m_button5";
            this.m_button5.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_button5.ShowFocus = false;
            this.m_button5.Size = new System.Drawing.Size(87, 52);
            this.m_button5.TabIndex = 62;
            this.m_button5.TabStop = false;
            this.m_button5.Text = "5";
            this.m_button5.UseMnemonic = false;
            this.m_button5.UseVisualStyleBackColor = false;
            this.m_button5.Click += new System.EventHandler(this.PressThisKey);
            // 
            // m_button4
            // 
            this.m_button4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(87)))), ((int)(((byte)(83)))));
            this.m_button4.FocusColor = System.Drawing.Color.Black;
            this.m_button4.Font = new System.Drawing.Font("Tahoma", 24F, System.Drawing.FontStyle.Bold);
            this.m_button4.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_button4.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_button4.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.m_button4.Location = new System.Drawing.Point(163, 162);
            this.m_button4.MinimumSize = new System.Drawing.Size(30, 30);
            this.m_button4.Name = "m_button4";
            this.m_button4.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_button4.ShowFocus = false;
            this.m_button4.Size = new System.Drawing.Size(87, 52);
            this.m_button4.TabIndex = 61;
            this.m_button4.TabStop = false;
            this.m_button4.Text = "4";
            this.m_button4.UseMnemonic = false;
            this.m_button4.UseVisualStyleBackColor = false;
            this.m_button4.Click += new System.EventHandler(this.PressThisKey);
            // 
            // m_button7
            // 
            this.m_button7.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(87)))), ((int)(((byte)(83)))));
            this.m_button7.FocusColor = System.Drawing.Color.Black;
            this.m_button7.Font = new System.Drawing.Font("Tahoma", 24F, System.Drawing.FontStyle.Bold);
            this.m_button7.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_button7.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_button7.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.m_button7.Location = new System.Drawing.Point(163, 106);
            this.m_button7.MinimumSize = new System.Drawing.Size(30, 30);
            this.m_button7.Name = "m_button7";
            this.m_button7.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_button7.ShowFocus = false;
            this.m_button7.Size = new System.Drawing.Size(87, 52);
            this.m_button7.TabIndex = 64;
            this.m_button7.TabStop = false;
            this.m_button7.Text = "7";
            this.m_button7.UseMnemonic = false;
            this.m_button7.UseVisualStyleBackColor = false;
            this.m_button7.Click += new System.EventHandler(this.PressThisKey);
            // 
            // m_button8
            // 
            this.m_button8.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(87)))), ((int)(((byte)(83)))));
            this.m_button8.FocusColor = System.Drawing.Color.Black;
            this.m_button8.Font = new System.Drawing.Font("Tahoma", 24F, System.Drawing.FontStyle.Bold);
            this.m_button8.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_button8.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_button8.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.m_button8.Location = new System.Drawing.Point(255, 106);
            this.m_button8.MinimumSize = new System.Drawing.Size(30, 30);
            this.m_button8.Name = "m_button8";
            this.m_button8.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_button8.ShowFocus = false;
            this.m_button8.Size = new System.Drawing.Size(87, 52);
            this.m_button8.TabIndex = 65;
            this.m_button8.TabStop = false;
            this.m_button8.Text = "8";
            this.m_button8.UseMnemonic = false;
            this.m_button8.UseVisualStyleBackColor = false;
            this.m_button8.Click += new System.EventHandler(this.PressThisKey);
            // 
            // m_button9
            // 
            this.m_button9.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(87)))), ((int)(((byte)(83)))));
            this.m_button9.FocusColor = System.Drawing.Color.Black;
            this.m_button9.Font = new System.Drawing.Font("Tahoma", 24F, System.Drawing.FontStyle.Bold);
            this.m_button9.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_button9.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_button9.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.m_button9.Location = new System.Drawing.Point(347, 106);
            this.m_button9.MinimumSize = new System.Drawing.Size(30, 30);
            this.m_button9.Name = "m_button9";
            this.m_button9.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_button9.ShowFocus = false;
            this.m_button9.Size = new System.Drawing.Size(87, 52);
            this.m_button9.TabIndex = 66;
            this.m_button9.TabStop = false;
            this.m_button9.Text = "9";
            this.m_button9.UseMnemonic = false;
            this.m_button9.UseVisualStyleBackColor = false;
            this.m_button9.Click += new System.EventHandler(this.PressThisKey);
            // 
            // m_button0
            // 
            this.m_button0.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(87)))), ((int)(((byte)(83)))));
            this.m_button0.FocusColor = System.Drawing.Color.Black;
            this.m_button0.Font = new System.Drawing.Font("Tahoma", 24F, System.Drawing.FontStyle.Bold);
            this.m_button0.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_button0.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_button0.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.m_button0.Location = new System.Drawing.Point(255, 274);
            this.m_button0.MinimumSize = new System.Drawing.Size(30, 30);
            this.m_button0.Name = "m_button0";
            this.m_button0.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_button0.ShowFocus = false;
            this.m_button0.Size = new System.Drawing.Size(87, 52);
            this.m_button0.TabIndex = 57;
            this.m_button0.TabStop = false;
            this.m_button0.Text = "0";
            this.m_button0.UseMnemonic = false;
            this.m_button0.UseVisualStyleBackColor = false;
            this.m_button0.Click += new System.EventHandler(this.PressThisKey);
            // 
            // m_button1
            // 
            this.m_button1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(87)))), ((int)(((byte)(83)))));
            this.m_button1.FocusColor = System.Drawing.Color.Black;
            this.m_button1.Font = new System.Drawing.Font("Tahoma", 24F, System.Drawing.FontStyle.Bold);
            this.m_button1.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_button1.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_button1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.m_button1.Location = new System.Drawing.Point(163, 218);
            this.m_button1.MinimumSize = new System.Drawing.Size(30, 30);
            this.m_button1.Name = "m_button1";
            this.m_button1.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_button1.ShowFocus = false;
            this.m_button1.Size = new System.Drawing.Size(87, 52);
            this.m_button1.TabIndex = 58;
            this.m_button1.TabStop = false;
            this.m_button1.Text = "1";
            this.m_button1.UseMnemonic = false;
            this.m_button1.UseVisualStyleBackColor = false;
            this.m_button1.Click += new System.EventHandler(this.PressThisKey);
            // 
            // DamagedListBox
            // 
            this.DamagedListBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(74)))), ((int)(((byte)(117)))));
            this.DamagedListBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.DamagedListBox.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DamagedListBox.ForeColor = System.Drawing.Color.Yellow;
            this.DamagedListBox.FormattingEnabled = true;
            this.DamagedListBox.ItemHeight = 19;
            this.DamagedListBox.Location = new System.Drawing.Point(17, 75);
            this.DamagedListBox.Name = "DamagedListBox";
            this.DamagedListBox.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.DamagedListBox.Size = new System.Drawing.Size(131, 247);
            this.DamagedListBox.TabIndex = 78;
            this.DamagedListBox.SelectedValueChanged += new System.EventHandler(this.DamagedListBox_SelectedValueChanged);
            // 
            // SerialLabel
            // 
            this.SerialLabel.AutoSize = true;
            this.SerialLabel.BackColor = System.Drawing.Color.Transparent;
            this.SerialLabel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SerialLabel.Location = new System.Drawing.Point(9, 18);
            this.SerialLabel.Name = "SerialLabel";
            this.SerialLabel.Size = new System.Drawing.Size(45, 13);
            this.SerialLabel.TabIndex = 79;
            this.SerialLabel.Text = "Serial: ";
            // 
            // NameLabel
            // 
            this.NameLabel.AutoSize = true;
            this.NameLabel.BackColor = System.Drawing.Color.Transparent;
            this.NameLabel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.NameLabel.Location = new System.Drawing.Point(9, 3);
            this.NameLabel.Name = "NameLabel";
            this.NameLabel.Size = new System.Drawing.Size(45, 13);
            this.NameLabel.TabIndex = 80;
            this.NameLabel.Text = "Name: ";
            // 
            // PaperUsageDamageForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::GTI.Modules.POS.Properties.Resources.PaperUsageDamages;
            this.ClientSize = new System.Drawing.Size(450, 450);
            this.Controls.Add(this.NameLabel);
            this.Controls.Add(this.SerialLabel);
            this.Controls.Add(this.DamagedListBox);
            this.Controls.Add(this.CloseButton);
            this.Controls.Add(this.AddButton);
            this.Controls.Add(this.RemoveButton);
            this.Controls.Add(this.DamagesLabel);
            this.Controls.Add(this.AuditNumberTextBox);
            this.Controls.Add(this.EnterAuditNumberLabel);
            this.Controls.Add(this.m_backButton);
            this.Controls.Add(this.m_decimalButton);
            this.Controls.Add(this.m_button2);
            this.Controls.Add(this.m_button3);
            this.Controls.Add(this.m_button6);
            this.Controls.Add(this.m_button5);
            this.Controls.Add(this.m_button4);
            this.Controls.Add(this.m_button7);
            this.Controls.Add(this.m_button8);
            this.Controls.Add(this.m_button9);
            this.Controls.Add(this.m_button0);
            this.Controls.Add(this.m_button1);
            this.Controls.Add(this.TitleLabel);
            this.DoubleBuffered = true;
            this.DrawBorderOuterEdge = true;
            this.DrawRounded = true;
            this.ForeColor = System.Drawing.SystemColors.ControlText;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "PaperUsageDamageForm";
            this.Text = "PaperUsageDamageForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label TitleLabel;
        private Controls.ImageButton m_backButton;
        private Controls.ImageButton m_decimalButton;
        private Controls.ImageButton m_button2;
        private Controls.ImageButton m_button3;
        private Controls.ImageButton m_button6;
        private Controls.ImageButton m_button5;
        private Controls.ImageButton m_button4;
        private Controls.ImageButton m_button7;
        private Controls.ImageButton m_button8;
        private Controls.ImageButton m_button9;
        private Controls.ImageButton m_button0;
        private Controls.ImageButton m_button1;
        private System.Windows.Forms.Label EnterAuditNumberLabel;
        private System.Windows.Forms.TextBox AuditNumberTextBox;
        private System.Windows.Forms.Label DamagesLabel;
        private Controls.ImageButton AddButton;
        private Controls.ImageButton RemoveButton;
        private Controls.ImageButton CloseButton;
        private System.Windows.Forms.ListBox DamagedListBox;
        private System.Windows.Forms.Label SerialLabel;
        private System.Windows.Forms.Label NameLabel;

    }
}