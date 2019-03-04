namespace GTI.Modules.POS.UI
{
    partial class DenomControl
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
            this.ValueLabel = new System.Windows.Forms.Label();
            this.TotalLabel = new System.Windows.Forms.Label();
            this.TypeLabel = new System.Windows.Forms.Label();
            this.NameLabel = new System.Windows.Forms.Label();
            this.CountTextBox = new System.Windows.Forms.TextBox();
            this.IncrementCountButton = new System.Windows.Forms.Button();
            this.DecrementCountButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // ValueLabel
            // 
            this.ValueLabel.BackColor = System.Drawing.Color.Transparent;
            this.ValueLabel.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ValueLabel.ForeColor = System.Drawing.Color.White;
            this.ValueLabel.Location = new System.Drawing.Point(287, 0);
            this.ValueLabel.Name = "ValueLabel";
            this.ValueLabel.Size = new System.Drawing.Size(88, 37);
            this.ValueLabel.TabIndex = 3;
            this.ValueLabel.Text = "$1000.00";
            this.ValueLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.ValueLabel.Click += new System.EventHandler(this.UserControlClicked);
            // 
            // TotalLabel
            // 
            this.TotalLabel.BackColor = System.Drawing.Color.Transparent;
            this.TotalLabel.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TotalLabel.ForeColor = System.Drawing.Color.White;
            this.TotalLabel.Location = new System.Drawing.Point(515, 0);
            this.TotalLabel.Name = "TotalLabel";
            this.TotalLabel.Size = new System.Drawing.Size(109, 36);
            this.TotalLabel.TabIndex = 7;
            this.TotalLabel.Text = "$9,999,999.00";
            this.TotalLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.TotalLabel.Click += new System.EventHandler(this.UserControlClicked);
            // 
            // TypeLabel
            // 
            this.TypeLabel.BackColor = System.Drawing.Color.Transparent;
            this.TypeLabel.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TypeLabel.ForeColor = System.Drawing.Color.White;
            this.TypeLabel.Location = new System.Drawing.Point(170, 0);
            this.TypeLabel.Name = "TypeLabel";
            this.TypeLabel.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.TypeLabel.Size = new System.Drawing.Size(114, 36);
            this.TypeLabel.TabIndex = 2;
            this.TypeLabel.Text = "Credit/Debit";
            this.TypeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.TypeLabel.Click += new System.EventHandler(this.UserControlClicked);
            // 
            // NameLabel
            // 
            this.NameLabel.BackColor = System.Drawing.Color.Transparent;
            this.NameLabel.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.NameLabel.ForeColor = System.Drawing.Color.White;
            this.NameLabel.Location = new System.Drawing.Point(3, 0);
            this.NameLabel.Name = "NameLabel";
            this.NameLabel.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.NameLabel.Size = new System.Drawing.Size(164, 36);
            this.NameLabel.TabIndex = 1;
            this.NameLabel.Text = "Dollar Coin 1";
            this.NameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.NameLabel.Click += new System.EventHandler(this.UserControlClicked);
            // 
            // CountTextBox
            // 
            this.CountTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.CountTextBox.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CountTextBox.Location = new System.Drawing.Point(420, 4);
            this.CountTextBox.Name = "CountTextBox";
            this.CountTextBox.Size = new System.Drawing.Size(60, 26);
            this.CountTextBox.TabIndex = 5;
            this.CountTextBox.Text = "99999";
            this.CountTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.CountTextBox.TextChanged += new System.EventHandler(this.CountTextBox_TextChanged);
            this.CountTextBox.Enter += new System.EventHandler(this.CountTextBox_Enter);
            this.CountTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.CountTextBox_KeyPress);
            this.CountTextBox.Leave += new System.EventHandler(this.CountTextBox_Leave);
            // 
            // IncrementCountButton
            // 
            this.IncrementCountButton.BackColor = System.Drawing.SystemColors.Control;
            this.IncrementCountButton.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.IncrementCountButton.ForeColor = System.Drawing.SystemColors.WindowText;
            this.IncrementCountButton.Location = new System.Drawing.Point(488, 5);
            this.IncrementCountButton.Name = "IncrementCountButton";
            this.IncrementCountButton.Size = new System.Drawing.Size(26, 26);
            this.IncrementCountButton.TabIndex = 6;
            this.IncrementCountButton.Text = "+";
            this.IncrementCountButton.UseVisualStyleBackColor = false;
            this.IncrementCountButton.Click += new System.EventHandler(this.IncrementCountButton_Click);
            this.IncrementCountButton.Enter += new System.EventHandler(this.CountTextBox_Enter);
            // 
            // DecrementCountButton
            // 
            this.DecrementCountButton.BackColor = System.Drawing.SystemColors.Control;
            this.DecrementCountButton.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DecrementCountButton.ForeColor = System.Drawing.SystemColors.WindowText;
            this.DecrementCountButton.Location = new System.Drawing.Point(386, 5);
            this.DecrementCountButton.Name = "DecrementCountButton";
            this.DecrementCountButton.Size = new System.Drawing.Size(26, 26);
            this.DecrementCountButton.TabIndex = 4;
            this.DecrementCountButton.Text = "-";
            this.DecrementCountButton.UseVisualStyleBackColor = false;
            this.DecrementCountButton.Click += new System.EventHandler(this.DecrementCountButton_Click);
            this.DecrementCountButton.Enter += new System.EventHandler(this.CountTextBox_Enter);
            // 
            // DenomControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(87)))), ((int)(((byte)(83)))));
            this.Controls.Add(this.DecrementCountButton);
            this.Controls.Add(this.IncrementCountButton);
            this.Controls.Add(this.CountTextBox);
            this.Controls.Add(this.NameLabel);
            this.Controls.Add(this.TypeLabel);
            this.Controls.Add(this.TotalLabel);
            this.Controls.Add(this.ValueLabel);
            this.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.Name = "DenomControl";
            this.Size = new System.Drawing.Size(627, 36);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label ValueLabel;
        private System.Windows.Forms.Label TotalLabel;
        private System.Windows.Forms.Label TypeLabel;
        private System.Windows.Forms.Label NameLabel;
        private System.Windows.Forms.Button IncrementCountButton;
        private System.Windows.Forms.Button DecrementCountButton;
        internal System.Windows.Forms.TextBox CountTextBox;


    }
}
