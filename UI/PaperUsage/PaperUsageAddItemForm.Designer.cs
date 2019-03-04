namespace GTI.Modules.POS.UI.PaperUsage
{
    partial class PaperUsageAddItemForm
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
            this.CancelButton = new GTI.Controls.ImageButton();
            this.AddButton = new GTI.Controls.ImageButton();
            this.SerialComboBox = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.ProductLabel = new System.Windows.Forms.Label();
            this.StartAuditTextBox = new System.Windows.Forms.TextBox();
            this.ErrorMessageLabel = new System.Windows.Forms.Label();
            this.errIcon = new System.Windows.Forms.PictureBox();
            this.ProductNameLabel = new System.Windows.Forms.Label();
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
            ((System.ComponentModel.ISupportInitialize)(this.errIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // TitleLabel
            // 
            this.TitleLabel.AutoSize = true;
            this.TitleLabel.BackColor = System.Drawing.Color.Transparent;
            this.TitleLabel.Font = new System.Drawing.Font("Tahoma", 20F, System.Drawing.FontStyle.Bold);
            this.TitleLabel.ForeColor = System.Drawing.Color.Black;
            this.TitleLabel.Location = new System.Drawing.Point(265, 9);
            this.TitleLabel.Name = "TitleLabel";
            this.TitleLabel.Size = new System.Drawing.Size(141, 33);
            this.TitleLabel.TabIndex = 17;
            this.TitleLabel.Text = "Add Item";
            this.TitleLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // CancelButton
            // 
            this.CancelButton.BackColor = System.Drawing.Color.Transparent;
            this.CancelButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.CancelButton.FocusColor = System.Drawing.Color.Black;
            this.CancelButton.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.CancelButton.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.CancelButton.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.CancelButton.Location = new System.Drawing.Point(336, 320);
            this.CancelButton.MinimumSize = new System.Drawing.Size(30, 30);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.CancelButton.ShowFocus = false;
            this.CancelButton.Size = new System.Drawing.Size(135, 50);
            this.CancelButton.TabIndex = 8;
            this.CancelButton.TabStop = false;
            this.CancelButton.Text = "Cancel";
            this.CancelButton.UseMnemonic = false;
            this.CancelButton.UseVisualStyleBackColor = false;
            this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // AddButton
            // 
            this.AddButton.BackColor = System.Drawing.Color.Transparent;
            this.AddButton.FocusColor = System.Drawing.Color.Black;
            this.AddButton.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.AddButton.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.AddButton.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.AddButton.Location = new System.Drawing.Point(200, 320);
            this.AddButton.MinimumSize = new System.Drawing.Size(30, 30);
            this.AddButton.Name = "AddButton";
            this.AddButton.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.AddButton.ShowFocus = false;
            this.AddButton.Size = new System.Drawing.Size(130, 50);
            this.AddButton.TabIndex = 7;
            this.AddButton.TabStop = false;
            this.AddButton.Text = "Add";
            this.AddButton.UseMnemonic = false;
            this.AddButton.UseVisualStyleBackColor = false;
            this.AddButton.Click += new System.EventHandler(this.AddButton_Click);
            // 
            // SerialComboBox
            // 
            this.SerialComboBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.SerialComboBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.SerialComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SerialComboBox.FormattingEnabled = true;
            this.SerialComboBox.Location = new System.Drawing.Point(75, 95);
            this.SerialComboBox.Name = "SerialComboBox";
            this.SerialComboBox.Size = new System.Drawing.Size(235, 32);
            this.SerialComboBox.Sorted = true;
            this.SerialComboBox.TabIndex = 2;
            this.SerialComboBox.SelectedIndexChanged += new System.EventHandler(this.SerialComboBox_SelectedIndexChanged);
            this.SerialComboBox.TextChanged += new System.EventHandler(this.SerialComboBox_TextChanged);
            this.SerialComboBox.Enter += new System.EventHandler(this.TextBox_Enter);
            this.SerialComboBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.SerialComboBox_KeyDown);
            this.SerialComboBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.SerialComboBox_KeyPress);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Yellow;
            this.label2.Location = new System.Drawing.Point(70, 70);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(56, 19);
            this.label2.TabIndex = 1;
            this.label2.Text = "Serial";
            this.label2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Yellow;
            this.label1.Location = new System.Drawing.Point(70, 210);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(99, 19);
            this.label1.TabIndex = 5;
            this.label1.Text = "Start Audit";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // ProductLabel
            // 
            this.ProductLabel.AutoSize = true;
            this.ProductLabel.BackColor = System.Drawing.Color.Transparent;
            this.ProductLabel.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ProductLabel.ForeColor = System.Drawing.Color.Yellow;
            this.ProductLabel.Location = new System.Drawing.Point(70, 140);
            this.ProductLabel.Name = "ProductLabel";
            this.ProductLabel.Size = new System.Drawing.Size(72, 19);
            this.ProductLabel.TabIndex = 3;
            this.ProductLabel.Text = "Product";
            this.ProductLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // StartAuditTextBox
            // 
            this.StartAuditTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.StartAuditTextBox.Location = new System.Drawing.Point(74, 235);
            this.StartAuditTextBox.MaxLength = 6;
            this.StartAuditTextBox.Name = "StartAuditTextBox";
            this.StartAuditTextBox.Size = new System.Drawing.Size(121, 29);
            this.StartAuditTextBox.TabIndex = 6;
            this.StartAuditTextBox.TextChanged += new System.EventHandler(this.StartAuditTextBox_TextChanged);
            this.StartAuditTextBox.Enter += new System.EventHandler(this.TextBox_Enter);
            this.StartAuditTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.StartAuditTextBox_KeyPress);
            // 
            // ErrorMessageLabel
            // 
            this.ErrorMessageLabel.BackColor = System.Drawing.Color.Transparent;
            this.ErrorMessageLabel.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ErrorMessageLabel.ForeColor = System.Drawing.Color.Black;
            this.ErrorMessageLabel.Location = new System.Drawing.Point(87, 292);
            this.ErrorMessageLabel.Name = "ErrorMessageLabel";
            this.ErrorMessageLabel.Size = new System.Drawing.Size(571, 26);
            this.ErrorMessageLabel.TabIndex = 18;
            this.ErrorMessageLabel.Text = "Unable to to find serial product in audit range";
            // 
            // errIcon
            // 
            this.errIcon.BackColor = System.Drawing.Color.Transparent;
            this.errIcon.Image = global::GTI.Modules.POS.Properties.Resources.WarningIcon3;
            this.errIcon.Location = new System.Drawing.Point(56, 287);
            this.errIcon.Name = "errIcon";
            this.errIcon.Size = new System.Drawing.Size(25, 25);
            this.errIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.errIcon.TabIndex = 36;
            this.errIcon.TabStop = false;
            this.errIcon.Visible = false;
            // 
            // ProductNameLabel
            // 
            this.ProductNameLabel.BackColor = System.Drawing.SystemColors.Window;
            this.ProductNameLabel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.ProductNameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F);
            this.ProductNameLabel.Location = new System.Drawing.Point(74, 169);
            this.ProductNameLabel.Name = "ProductNameLabel";
            this.ProductNameLabel.Size = new System.Drawing.Size(235, 29);
            this.ProductNameLabel.TabIndex = 37;
            this.ProductNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
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
            this.m_backButton.Location = new System.Drawing.Point(528, 228);
            this.m_backButton.MinimumSize = new System.Drawing.Size(30, 30);
            this.m_backButton.Name = "m_backButton";
            this.m_backButton.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_backButton.ShowFocus = false;
            this.m_backButton.Size = new System.Drawing.Size(87, 52);
            this.m_backButton.TabIndex = 68;
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
            this.m_decimalButton.Location = new System.Drawing.Point(344, 228);
            this.m_decimalButton.MinimumSize = new System.Drawing.Size(30, 30);
            this.m_decimalButton.Name = "m_decimalButton";
            this.m_decimalButton.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_decimalButton.ShowFocus = false;
            this.m_decimalButton.Size = new System.Drawing.Size(87, 52);
            this.m_decimalButton.TabIndex = 67;
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
            this.m_button2.Location = new System.Drawing.Point(436, 172);
            this.m_button2.MinimumSize = new System.Drawing.Size(30, 30);
            this.m_button2.Name = "m_button2";
            this.m_button2.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_button2.ShowFocus = false;
            this.m_button2.Size = new System.Drawing.Size(87, 52);
            this.m_button2.TabIndex = 71;
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
            this.m_button3.Location = new System.Drawing.Point(528, 172);
            this.m_button3.MinimumSize = new System.Drawing.Size(30, 30);
            this.m_button3.Name = "m_button3";
            this.m_button3.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_button3.ShowFocus = false;
            this.m_button3.Size = new System.Drawing.Size(87, 52);
            this.m_button3.TabIndex = 72;
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
            this.m_button6.Location = new System.Drawing.Point(528, 116);
            this.m_button6.MinimumSize = new System.Drawing.Size(30, 30);
            this.m_button6.Name = "m_button6";
            this.m_button6.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_button6.ShowFocus = false;
            this.m_button6.Size = new System.Drawing.Size(87, 52);
            this.m_button6.TabIndex = 75;
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
            this.m_button5.Location = new System.Drawing.Point(436, 116);
            this.m_button5.MinimumSize = new System.Drawing.Size(30, 30);
            this.m_button5.Name = "m_button5";
            this.m_button5.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_button5.ShowFocus = false;
            this.m_button5.Size = new System.Drawing.Size(87, 52);
            this.m_button5.TabIndex = 74;
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
            this.m_button4.Location = new System.Drawing.Point(344, 116);
            this.m_button4.MinimumSize = new System.Drawing.Size(30, 30);
            this.m_button4.Name = "m_button4";
            this.m_button4.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_button4.ShowFocus = false;
            this.m_button4.Size = new System.Drawing.Size(87, 52);
            this.m_button4.TabIndex = 73;
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
            this.m_button7.Location = new System.Drawing.Point(344, 60);
            this.m_button7.MinimumSize = new System.Drawing.Size(30, 30);
            this.m_button7.Name = "m_button7";
            this.m_button7.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_button7.ShowFocus = false;
            this.m_button7.Size = new System.Drawing.Size(87, 52);
            this.m_button7.TabIndex = 76;
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
            this.m_button8.Location = new System.Drawing.Point(436, 60);
            this.m_button8.MinimumSize = new System.Drawing.Size(30, 30);
            this.m_button8.Name = "m_button8";
            this.m_button8.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_button8.ShowFocus = false;
            this.m_button8.Size = new System.Drawing.Size(87, 52);
            this.m_button8.TabIndex = 77;
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
            this.m_button9.Location = new System.Drawing.Point(528, 60);
            this.m_button9.MinimumSize = new System.Drawing.Size(30, 30);
            this.m_button9.Name = "m_button9";
            this.m_button9.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_button9.ShowFocus = false;
            this.m_button9.Size = new System.Drawing.Size(87, 52);
            this.m_button9.TabIndex = 78;
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
            this.m_button0.Location = new System.Drawing.Point(436, 228);
            this.m_button0.MinimumSize = new System.Drawing.Size(30, 30);
            this.m_button0.Name = "m_button0";
            this.m_button0.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_button0.ShowFocus = false;
            this.m_button0.Size = new System.Drawing.Size(87, 52);
            this.m_button0.TabIndex = 69;
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
            this.m_button1.Location = new System.Drawing.Point(344, 172);
            this.m_button1.MinimumSize = new System.Drawing.Size(30, 30);
            this.m_button1.Name = "m_button1";
            this.m_button1.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_button1.ShowFocus = false;
            this.m_button1.Size = new System.Drawing.Size(87, 52);
            this.m_button1.TabIndex = 70;
            this.m_button1.TabStop = false;
            this.m_button1.Text = "1";
            this.m_button1.UseMnemonic = false;
            this.m_button1.UseVisualStyleBackColor = false;
            this.m_button1.Click += new System.EventHandler(this.PressThisKey);
            // 
            // PaperUsageAddItemForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::GTI.Modules.POS.Properties.Resources.PaperUsageAddItem;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(670, 380);
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
            this.Controls.Add(this.ProductNameLabel);
            this.Controls.Add(this.errIcon);
            this.Controls.Add(this.ErrorMessageLabel);
            this.Controls.Add(this.StartAuditTextBox);
            this.Controls.Add(this.ProductLabel);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.SerialComboBox);
            this.Controls.Add(this.CancelButton);
            this.Controls.Add(this.AddButton);
            this.Controls.Add(this.TitleLabel);
            this.DoubleBuffered = true;
            this.DrawBorderOuterEdge = true;
            this.DrawRounded = true;
            this.ForeColor = System.Drawing.SystemColors.ControlText;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "PaperUsageAddItemForm";
            this.Text = "PaperUsageDamageForm";
            this.Load += new System.EventHandler(this.PaperUsageAddItemForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.errIcon)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label TitleLabel;
        private Controls.ImageButton AddButton;
        private Controls.ImageButton CancelButton;
        private System.Windows.Forms.ComboBox SerialComboBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label ProductLabel;
        private System.Windows.Forms.TextBox StartAuditTextBox;
        private System.Windows.Forms.Label ErrorMessageLabel;
        private System.Windows.Forms.PictureBox errIcon;
        private System.Windows.Forms.Label ProductNameLabel;
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

    }
}