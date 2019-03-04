namespace GTI.Modules.POS.UI
{
    partial class ViewPlayerInfoForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ViewPlayerInfoForm));
            this.m_firstName = new System.Windows.Forms.TextBox();
            this.m_lastName = new System.Windows.Forms.TextBox();
            this.m_comments = new System.Windows.Forms.TextBox();
            this.m_close = new GTI.Controls.ImageButton();
            this.m_noPic = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.m_noPic)).BeginInit();
            this.SuspendLayout();
            // 
            // m_firstName
            // 
            this.m_firstName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(19)))), ((int)(((byte)(60)))), ((int)(((byte)(96)))));
            this.m_firstName.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.m_firstName, "m_firstName");
            this.m_firstName.ForeColor = System.Drawing.Color.Yellow;
            this.m_firstName.Name = "m_firstName";
            // 
            // m_lastName
            // 
            this.m_lastName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(19)))), ((int)(((byte)(60)))), ((int)(((byte)(96)))));
            this.m_lastName.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.m_lastName, "m_lastName");
            this.m_lastName.ForeColor = System.Drawing.Color.Yellow;
            this.m_lastName.Name = "m_lastName";
            // 
            // m_comments
            // 
            this.m_comments.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(19)))), ((int)(((byte)(60)))), ((int)(((byte)(96)))));
            this.m_comments.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.m_comments, "m_comments");
            this.m_comments.ForeColor = System.Drawing.Color.Yellow;
            this.m_comments.Name = "m_comments";
            // 
            // m_close
            // 
            this.m_close.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            resources.ApplyResources(this.m_close, "m_close");
            this.m_close.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_close.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_close.Name = "m_close";
            this.m_close.UseVisualStyleBackColor = false;
            this.m_close.Click += new System.EventHandler(this.m_close_Click);
            // 
            // m_noPic
            // 
            this.m_noPic.Image = global::GTI.Modules.POS.Properties.Resources.NoPic;
            resources.ApplyResources(this.m_noPic, "m_noPic");
            this.m_noPic.Name = "m_noPic";
            this.m_noPic.TabStop = false;
            // 
            // ViewPlayerInfoForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackgroundImage = global::GTI.Modules.POS.Properties.Resources.ViewPlayerInfoBack;
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.m_noPic);
            this.Controls.Add(this.m_close);
            this.Controls.Add(this.m_comments);
            this.Controls.Add(this.m_lastName);
            this.Controls.Add(this.m_firstName);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.KeyPreview = true;
            this.Name = "ViewPlayerInfoForm";
            this.ShowInTaskbar = false;
            ((System.ComponentModel.ISupportInitialize)(this.m_noPic)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox m_firstName;
        private System.Windows.Forms.TextBox m_lastName;
        private System.Windows.Forms.TextBox m_comments;
        private GTI.Controls.ImageButton m_close;
        private System.Windows.Forms.PictureBox m_noPic;

    }
}