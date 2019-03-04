namespace GTI.Modules.POS.UI
{
    partial class StaffSummaryControl
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
            this.NameLabel = new System.Windows.Forms.Label();
            this.ValueLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // NameLabel
            // 
            this.NameLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(74)))), ((int)(((byte)(117)))));
            this.NameLabel.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.NameLabel.ForeColor = System.Drawing.Color.Yellow;
            this.NameLabel.Location = new System.Drawing.Point(0, 0);
            this.NameLabel.Name = "NameLabel";
            this.NameLabel.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.NameLabel.Size = new System.Drawing.Size(149, 30);
            this.NameLabel.TabIndex = 37;
            this.NameLabel.Text = "Pull tab Prizes(+)";
            this.NameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // ValueLabel
            // 
            this.ValueLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(74)))), ((int)(((byte)(117)))));
            this.ValueLabel.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ValueLabel.ForeColor = System.Drawing.Color.Yellow;
            this.ValueLabel.Location = new System.Drawing.Point(146, 0);
            this.ValueLabel.Name = "ValueLabel";
            this.ValueLabel.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.ValueLabel.Size = new System.Drawing.Size(126, 30);
            this.ValueLabel.TabIndex = 38;
            this.ValueLabel.Text = "$999,999,999.99";
            this.ValueLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // StaffSummaryControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.ValueLabel);
            this.Controls.Add(this.NameLabel);
            this.Name = "StaffSummaryControl";
            this.Size = new System.Drawing.Size(270, 30);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label NameLabel;
        private System.Windows.Forms.Label ValueLabel;
    }
}
