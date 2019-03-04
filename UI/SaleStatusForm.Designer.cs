namespace GTI.Modules.POS.UI
{
    partial class SaleStatusForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SaleStatusForm));
            this.m_waitPicture = new System.Windows.Forms.PictureBox();
            this.m_messageLabel = new GTI.Controls.ImageLabel();
            this.m_changeDueLabel = new GTI.Controls.OutlinedLabel();
            this.m_unitNumLabel = new GTI.Controls.OutlinedLabel();
            this.m_okButton = new GTI.Controls.ImageButton();
            this.m_cancelButton = new GTI.Controls.ImageButton();
            ((System.ComponentModel.ISupportInitialize)(this.m_waitPicture)).BeginInit();
            this.SuspendLayout();
            // 
            // m_waitPicture
            // 
            this.m_waitPicture.Image = global::GTI.Modules.POS.Properties.Resources.Waiting;
            resources.ApplyResources(this.m_waitPicture, "m_waitPicture");
            this.m_waitPicture.Name = "m_waitPicture";
            this.m_waitPicture.TabStop = false;
            // 
            // m_messageLabel
            // 
            this.m_messageLabel.BackColor = System.Drawing.Color.Transparent;
            this.m_messageLabel.Background = global::GTI.Modules.POS.Properties.Resources.SaleStatusField;
            this.m_messageLabel.ForeColor = System.Drawing.Color.Yellow;
            resources.ApplyResources(this.m_messageLabel, "m_messageLabel");
            this.m_messageLabel.Name = "m_messageLabel";
            this.m_messageLabel.Stretch = false;
            // 
            // m_changeDueLabel
            // 
            this.m_changeDueLabel.BackColor = System.Drawing.Color.Transparent;
            this.m_changeDueLabel.EdgeColor = System.Drawing.Color.DarkGreen;
            resources.ApplyResources(this.m_changeDueLabel, "m_changeDueLabel");
            this.m_changeDueLabel.ForeColor = System.Drawing.Color.Lime;
            this.m_changeDueLabel.Name = "m_changeDueLabel";
            // 
            // m_unitNumLabel
            // 
            this.m_unitNumLabel.BackColor = System.Drawing.Color.Transparent;
            this.m_unitNumLabel.EdgeColor = System.Drawing.Color.LightGray;
            resources.ApplyResources(this.m_unitNumLabel, "m_unitNumLabel");
            this.m_unitNumLabel.ForeColor = System.Drawing.Color.Black;
            this.m_unitNumLabel.Name = "m_unitNumLabel";
            // 
            // m_okButton
            // 
            this.m_okButton.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.m_okButton, "m_okButton");
            this.m_okButton.FocusColor = System.Drawing.Color.Black;
            this.m_okButton.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_okButton.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_okButton.Name = "m_okButton";
            this.m_okButton.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_okButton.ShowFocus = false;
            this.m_okButton.TabStop = false;
            this.m_okButton.UseVisualStyleBackColor = false;
            this.m_okButton.Click += new System.EventHandler(this.OkClick);
            // 
            // m_cancelButton
            // 
            this.m_cancelButton.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.m_cancelButton, "m_cancelButton");
            this.m_cancelButton.FocusColor = System.Drawing.Color.Black;
            this.m_cancelButton.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_cancelButton.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_cancelButton.Name = "m_cancelButton";
            this.m_cancelButton.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_cancelButton.ShowFocus = false;
            this.m_cancelButton.TabStop = false;
            this.m_cancelButton.UseVisualStyleBackColor = false;
            this.m_cancelButton.Click += new System.EventHandler(this.CancelClick);
            // 
            // SaleStatusForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            resources.ApplyResources(this, "$this");
            this.BorderColor = System.Drawing.Color.LightGray;
            this.ControlBox = false;
            this.Controls.Add(this.m_cancelButton);
            this.Controls.Add(this.m_okButton);
            this.Controls.Add(this.m_unitNumLabel);
            this.Controls.Add(this.m_changeDueLabel);
            this.Controls.Add(this.m_messageLabel);
            this.Controls.Add(this.m_waitPicture);
            this.DoubleBuffered = true;
            this.DrawAGradientBorder = true;
            this.DrawAsGradient = true;
            this.DrawBorderInnerEdge = true;
            this.DrawBorderOuterEdge = true;
            this.DrawRounded = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.GradientBeginColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.GradientEndColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(127)))), ((int)(((byte)(127)))));
            this.InnerBorderEdgeColor = System.Drawing.Color.Black;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SaleStatusForm";
            this.OuterBorderEdgeColor = System.Drawing.Color.DarkSlateGray;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormClose);
            ((System.ComponentModel.ISupportInitialize)(this.m_waitPicture)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox m_waitPicture;
        private GTI.Controls.ImageLabel m_messageLabel;
        private GTI.Controls.OutlinedLabel m_changeDueLabel;
        private GTI.Controls.OutlinedLabel m_unitNumLabel;
        private GTI.Controls.ImageButton m_okButton;
        private GTI.Controls.ImageButton m_cancelButton;
    }
}