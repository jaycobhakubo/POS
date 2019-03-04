namespace GTI.Modules.POS.UI
{
    partial class CrystalBallScanForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CrystalBallScanForm));
            this.m_cardsScannedLabel = new System.Windows.Forms.Label();
            this.m_pictureBox = new System.Windows.Forms.PictureBox();
            this.m_clearJamButton = new GTI.Controls.ImageButton();
            this.m_cancelButton = new GTI.Controls.ImageButton();
            this.m_numbersReqLabel = new System.Windows.Forms.Label();
            this.m_quickFinishButton = new GTI.Controls.ImageButton();
            this.m_kioskIdleTimer = new System.Windows.Forms.Timer(this.components);
            this.m_timeoutProgress = new System.Windows.Forms.ProgressBar();
            ((System.ComponentModel.ISupportInitialize)(this.m_pictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // m_cardsScannedLabel
            // 
            this.m_cardsScannedLabel.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.m_cardsScannedLabel, "m_cardsScannedLabel");
            this.m_cardsScannedLabel.Name = "m_cardsScannedLabel";
            this.m_cardsScannedLabel.Click += new System.EventHandler(this.UserActivityDetected);
            // 
            // m_pictureBox
            // 
            this.m_pictureBox.BackColor = System.Drawing.Color.Transparent;
            this.m_pictureBox.Image = global::GTI.Modules.POS.Properties.Resources.ScanCard;
            resources.ApplyResources(this.m_pictureBox, "m_pictureBox");
            this.m_pictureBox.Name = "m_pictureBox";
            this.m_pictureBox.TabStop = false;
            this.m_pictureBox.Click += new System.EventHandler(this.UserActivityDetected);
            // 
            // m_clearJamButton
            // 
            this.m_clearJamButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(200)))), ((int)(((byte)(208)))));
            this.m_clearJamButton.FocusColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.m_clearJamButton, "m_clearJamButton");
            this.m_clearJamButton.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_clearJamButton.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_clearJamButton.Name = "m_clearJamButton";
            this.m_clearJamButton.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_clearJamButton.ShowFocus = false;
            this.m_clearJamButton.TabStop = false;
            this.m_clearJamButton.UseVisualStyleBackColor = false;
            this.m_clearJamButton.Click += new System.EventHandler(this.ClearJamClick);
            // 
            // m_cancelButton
            // 
            this.m_cancelButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(200)))), ((int)(((byte)(208)))));
            this.m_cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.m_cancelButton.FocusColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.m_cancelButton, "m_cancelButton");
            this.m_cancelButton.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_cancelButton.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_cancelButton.Name = "m_cancelButton";
            this.m_cancelButton.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_cancelButton.ShowFocus = false;
            this.m_cancelButton.TabStop = false;
            this.m_cancelButton.UseVisualStyleBackColor = false;
            // 
            // m_numbersReqLabel
            // 
            this.m_numbersReqLabel.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.m_numbersReqLabel, "m_numbersReqLabel");
            this.m_numbersReqLabel.Name = "m_numbersReqLabel";
            this.m_numbersReqLabel.Click += new System.EventHandler(this.UserActivityDetected);
            // 
            // m_quickFinishButton
            // 
            this.m_quickFinishButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(200)))), ((int)(((byte)(208)))));
            this.m_quickFinishButton.FocusColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.m_quickFinishButton, "m_quickFinishButton");
            this.m_quickFinishButton.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_quickFinishButton.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_quickFinishButton.Name = "m_quickFinishButton";
            this.m_quickFinishButton.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_quickFinishButton.ShowFocus = false;
            this.m_quickFinishButton.TabStop = false;
            this.m_quickFinishButton.UseVisualStyleBackColor = false;
            this.m_quickFinishButton.Click += new System.EventHandler(this.QuickFinishClick);
            // 
            // m_kioskIdleTimer
            // 
            this.m_kioskIdleTimer.Interval = 500;
            this.m_kioskIdleTimer.Tick += new System.EventHandler(this.m_kioskIdleTimer_Tick);
            // 
            // m_timeoutProgress
            // 
            this.m_timeoutProgress.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(186)))), ((int)(((byte)(192)))));
            this.m_timeoutProgress.ForeColor = System.Drawing.Color.Gold;
            resources.ApplyResources(this.m_timeoutProgress, "m_timeoutProgress");
            this.m_timeoutProgress.Name = "m_timeoutProgress";
            this.m_timeoutProgress.Click += new System.EventHandler(this.UserActivityDetected);
            // 
            // CrystalBallScanForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackgroundImage = global::GTI.Modules.POS.Properties.Resources.MessageBack;
            resources.ApplyResources(this, "$this");
            this.ControlBox = false;
            this.Controls.Add(this.m_timeoutProgress);
            this.Controls.Add(this.m_quickFinishButton);
            this.Controls.Add(this.m_numbersReqLabel);
            this.Controls.Add(this.m_cancelButton);
            this.Controls.Add(this.m_clearJamButton);
            this.Controls.Add(this.m_pictureBox);
            this.Controls.Add(this.m_cardsScannedLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "CrystalBallScanForm";
            this.ShowInTaskbar = false;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.CrystalBallScanForm_FormClosed);
            this.Shown += new System.EventHandler(this.CrystalBallScanForm_Shown);
            this.Click += new System.EventHandler(this.UserActivityDetected);
            ((System.ComponentModel.ISupportInitialize)(this.m_pictureBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label m_cardsScannedLabel;
        private System.Windows.Forms.PictureBox m_pictureBox;
        private GTI.Controls.ImageButton m_clearJamButton;
        private GTI.Controls.ImageButton m_cancelButton;
        private System.Windows.Forms.Label m_numbersReqLabel;
        private GTI.Controls.ImageButton m_quickFinishButton;
        private System.Windows.Forms.Timer m_kioskIdleTimer;
        private System.Windows.Forms.ProgressBar m_timeoutProgress;
    }
}