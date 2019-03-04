namespace GTI.Modules.POS.UI.PaperUsage
{
    partial class PaperUsageItemControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.ProductLabel = new System.Windows.Forms.Label();
            this.SerialLabel = new System.Windows.Forms.Label();
            this.QuantityLabel = new System.Windows.Forms.Label();
            this.SkipsLabel = new System.Windows.Forms.Label();
            this.ValueLabel = new System.Windows.Forms.Label();
            this.StartAuditTextBox = new System.Windows.Forms.TextBox();
            this.EndAuditTextBox = new System.Windows.Forms.TextBox();
            this.BonanzaLabel = new System.Windows.Forms.Label();
            this.PriceTextbox = new System.Windows.Forms.TextBox();
            this.DamageButton = new System.Windows.Forms.Label();
            this.DamagedToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.SkipsToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.ProductNameToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.SerialNumberToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.QuantityToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.ValueToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // ProductLabel
            // 
            this.ProductLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.ProductLabel.AutoEllipsis = true;
            this.ProductLabel.BackColor = System.Drawing.Color.Transparent;
            this.ProductLabel.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ProductLabel.ForeColor = System.Drawing.Color.White;
            this.ProductLabel.Location = new System.Drawing.Point(0, 4);
            this.ProductLabel.Name = "ProductLabel";
            this.ProductLabel.Size = new System.Drawing.Size(124, 27);
            this.ProductLabel.TabIndex = 0;
            this.ProductLabel.Text = "0123456789..";
            this.ProductLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ProductLabel.Click += new System.EventHandler(this.UserControlClicked);
            // 
            // SerialLabel
            // 
            this.SerialLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.SerialLabel.AutoEllipsis = true;
            this.SerialLabel.BackColor = System.Drawing.Color.Transparent;
            this.SerialLabel.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SerialLabel.ForeColor = System.Drawing.Color.White;
            this.SerialLabel.Location = new System.Drawing.Point(124, 4);
            this.SerialLabel.Name = "SerialLabel";
            this.SerialLabel.Size = new System.Drawing.Size(68, 27);
            this.SerialLabel.TabIndex = 1;
            this.SerialLabel.Text = "12345";
            this.SerialLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.SerialLabel.Click += new System.EventHandler(this.UserControlClicked);
            // 
            // QuantityLabel
            // 
            this.QuantityLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.QuantityLabel.AutoEllipsis = true;
            this.QuantityLabel.BackColor = System.Drawing.Color.Transparent;
            this.QuantityLabel.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.QuantityLabel.ForeColor = System.Drawing.Color.White;
            this.QuantityLabel.Location = new System.Drawing.Point(335, 4);
            this.QuantityLabel.Name = "QuantityLabel";
            this.QuantityLabel.Size = new System.Drawing.Size(64, 27);
            this.QuantityLabel.TabIndex = 4;
            this.QuantityLabel.Text = "99999";
            this.QuantityLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.QuantityLabel.Click += new System.EventHandler(this.UserControlClicked);
            // 
            // SkipsLabel
            // 
            this.SkipsLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.SkipsLabel.AutoEllipsis = true;
            this.SkipsLabel.BackColor = System.Drawing.Color.Transparent;
            this.SkipsLabel.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SkipsLabel.ForeColor = System.Drawing.Color.White;
            this.SkipsLabel.Location = new System.Drawing.Point(408, 4);
            this.SkipsLabel.Name = "SkipsLabel";
            this.SkipsLabel.Size = new System.Drawing.Size(40, 27);
            this.SkipsLabel.TabIndex = 5;
            this.SkipsLabel.Text = "999";
            this.SkipsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.SkipsLabel.Click += new System.EventHandler(this.UserControlClicked);
            // 
            // ValueLabel
            // 
            this.ValueLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.ValueLabel.AutoEllipsis = true;
            this.ValueLabel.BackColor = System.Drawing.Color.Transparent;
            this.ValueLabel.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ValueLabel.ForeColor = System.Drawing.Color.White;
            this.ValueLabel.Location = new System.Drawing.Point(579, 4);
            this.ValueLabel.Name = "ValueLabel";
            this.ValueLabel.Size = new System.Drawing.Size(84, 27);
            this.ValueLabel.TabIndex = 10;
            this.ValueLabel.Text = "999999";
            this.ValueLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.ValueLabel.Click += new System.EventHandler(this.UserControlClicked);
            // 
            // StartAuditTextBox
            // 
            this.StartAuditTextBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.StartAuditTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.StartAuditTextBox.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.StartAuditTextBox.Location = new System.Drawing.Point(198, 5);
            this.StartAuditTextBox.MaxLength = 6;
            this.StartAuditTextBox.Name = "StartAuditTextBox";
            this.StartAuditTextBox.Size = new System.Drawing.Size(65, 26);
            this.StartAuditTextBox.TabIndex = 2;
            this.StartAuditTextBox.Text = "123456";
            this.StartAuditTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.StartAuditTextBox.WordWrap = false;
            this.StartAuditTextBox.Enter += new System.EventHandler(this.TextBoxEnter);
            this.StartAuditTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.AuditNumberTextBoxKeyPress);
            // 
            // EndAuditTextBox
            // 
            this.EndAuditTextBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.EndAuditTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.EndAuditTextBox.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.EndAuditTextBox.Location = new System.Drawing.Point(267, 5);
            this.EndAuditTextBox.MaxLength = 6;
            this.EndAuditTextBox.Name = "EndAuditTextBox";
            this.EndAuditTextBox.Size = new System.Drawing.Size(64, 26);
            this.EndAuditTextBox.TabIndex = 3;
            this.EndAuditTextBox.Text = "123456";
            this.EndAuditTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.EndAuditTextBox.WordWrap = false;
            this.EndAuditTextBox.Enter += new System.EventHandler(this.TextBoxEnter);
            this.EndAuditTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.AuditNumberTextBoxKeyPress);
            // 
            // BonanzaLabel
            // 
            this.BonanzaLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.BonanzaLabel.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BonanzaLabel.ForeColor = System.Drawing.Color.White;
            this.BonanzaLabel.Location = new System.Drawing.Point(503, 8);
            this.BonanzaLabel.Name = "BonanzaLabel";
            this.BonanzaLabel.Size = new System.Drawing.Size(40, 17);
            this.BonanzaLabel.TabIndex = 7;
            this.BonanzaLabel.Text = "999";
            this.BonanzaLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.BonanzaLabel.Visible = false;
            // 
            // PriceTextbox
            // 
            this.PriceTextbox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.PriceTextbox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.PriceTextbox.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PriceTextbox.Location = new System.Drawing.Point(503, 5);
            this.PriceTextbox.MaxLength = 8;
            this.PriceTextbox.Name = "PriceTextbox";
            this.PriceTextbox.Size = new System.Drawing.Size(76, 26);
            this.PriceTextbox.TabIndex = 9;
            this.PriceTextbox.Text = "9999.99";
            this.PriceTextbox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.PriceTextbox.WordWrap = false;
            this.PriceTextbox.TextChanged += new System.EventHandler(this.PriceTextBoxTextChanged);
            this.PriceTextbox.Enter += new System.EventHandler(this.TextBoxEnter);
            this.PriceTextbox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.PriceTextBoxKeyPress);
            // 
            // DamageButton
            // 
            this.DamageButton.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.DamageButton.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.DamageButton.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.DamageButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.DamageButton.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DamageButton.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.DamageButton.Location = new System.Drawing.Point(446, 4);
            this.DamageButton.Name = "DamageButton";
            this.DamageButton.Size = new System.Drawing.Size(48, 28);
            this.DamageButton.TabIndex = 8;
            this.DamageButton.Text = "0";
            this.DamageButton.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.DamageButton.Click += new System.EventHandler(this.DamageButton_Click);
            // 
            // DamagedToolTip
            // 
            this.DamagedToolTip.AutoPopDelay = 5000;
            this.DamagedToolTip.InitialDelay = 250;
            this.DamagedToolTip.ReshowDelay = 100;
            // 
            // SkipsToolTip
            // 
            this.SkipsToolTip.AutoPopDelay = 10000;
            this.SkipsToolTip.InitialDelay = 500;
            this.SkipsToolTip.ReshowDelay = 100;
            // 
            // PaperUsageItemControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(87)))), ((int)(((byte)(83)))));
            this.Controls.Add(this.DamageButton);
            this.Controls.Add(this.PriceTextbox);
            this.Controls.Add(this.BonanzaLabel);
            this.Controls.Add(this.EndAuditTextBox);
            this.Controls.Add(this.StartAuditTextBox);
            this.Controls.Add(this.ValueLabel);
            this.Controls.Add(this.SkipsLabel);
            this.Controls.Add(this.QuantityLabel);
            this.Controls.Add(this.SerialLabel);
            this.Controls.Add(this.ProductLabel);
            this.Name = "PaperUsageItemControl";
            this.Size = new System.Drawing.Size(666, 36);
            this.Click += new System.EventHandler(this.UserControlClicked);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label ProductLabel;
        private System.Windows.Forms.Label SerialLabel;
        private System.Windows.Forms.Label QuantityLabel;
        private System.Windows.Forms.Label SkipsLabel;
        private System.Windows.Forms.Label ValueLabel;
        public System.Windows.Forms.TextBox EndAuditTextBox;
        private System.Windows.Forms.Label BonanzaLabel;
        public System.Windows.Forms.TextBox PriceTextbox;
        public System.Windows.Forms.TextBox StartAuditTextBox;
        private System.Windows.Forms.Label DamageButton;
        private System.Windows.Forms.ToolTip DamagedToolTip;
        private System.Windows.Forms.ToolTip SkipsToolTip;
        private System.Windows.Forms.ToolTip ProductNameToolTip;
        private System.Windows.Forms.ToolTip SerialNumberToolTip;
        private System.Windows.Forms.ToolTip QuantityToolTip;
        private System.Windows.Forms.ToolTip ValueToolTip;
    }
}
