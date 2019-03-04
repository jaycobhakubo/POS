namespace GTI.Modules.POS.UI
{
    partial class CrystalBallPromptForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CrystalBallPromptForm));
            this.m_quickPickButton = new GTI.Controls.ImageButton();
            this.m_selectionLabel = new System.Windows.Forms.Label();
            this.m_scanButton = new GTI.Controls.ImageButton();
            this.m_handPickButton = new GTI.Controls.ImageButton();
            this.m_kioskIdleTimer = new System.Windows.Forms.Timer(this.components);
            this.m_timeoutProgress = new System.Windows.Forms.ProgressBar();
            this.m_favoritesButton = new GTI.Controls.ImageButton();
            this.m_cancelButton = new GTI.Controls.ImageButton();
            this.SuspendLayout();
            // 
            // m_quickPickButton
            // 
            this.m_quickPickButton.BackColor = System.Drawing.Color.Transparent;
            this.m_quickPickButton.FocusColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.m_quickPickButton, "m_quickPickButton");
            this.m_quickPickButton.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_quickPickButton.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_quickPickButton.Name = "m_quickPickButton";
            this.m_quickPickButton.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_quickPickButton.ShowFocus = false;
            this.m_quickPickButton.UseVisualStyleBackColor = false;
            this.m_quickPickButton.Click += new System.EventHandler(this.QuickPickButtonClick);
            // 
            // m_selectionLabel
            // 
            this.m_selectionLabel.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.m_selectionLabel, "m_selectionLabel");
            this.m_selectionLabel.Name = "m_selectionLabel";
            this.m_selectionLabel.Click += new System.EventHandler(this.UserActivityDetected);
            // 
            // m_scanButton
            // 
            this.m_scanButton.BackColor = System.Drawing.Color.Transparent;
            this.m_scanButton.FocusColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.m_scanButton, "m_scanButton");
            this.m_scanButton.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_scanButton.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_scanButton.Name = "m_scanButton";
            this.m_scanButton.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_scanButton.ShowFocus = false;
            this.m_scanButton.UseVisualStyleBackColor = false;
            this.m_scanButton.Click += new System.EventHandler(this.ScanButtonClick);
            // 
            // m_handPickButton
            // 
            this.m_handPickButton.BackColor = System.Drawing.Color.Transparent;
            this.m_handPickButton.FocusColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.m_handPickButton, "m_handPickButton");
            this.m_handPickButton.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_handPickButton.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_handPickButton.Name = "m_handPickButton";
            this.m_handPickButton.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_handPickButton.ShowFocus = false;
            this.m_handPickButton.UseVisualStyleBackColor = false;
            this.m_handPickButton.Click += new System.EventHandler(this.HandPickButtonClick);
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
            // m_favoritesButton
            // 
            this.m_favoritesButton.BackColor = System.Drawing.Color.Transparent;
            this.m_favoritesButton.FocusColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.m_favoritesButton, "m_favoritesButton");
            this.m_favoritesButton.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_favoritesButton.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_favoritesButton.Name = "m_favoritesButton";
            this.m_favoritesButton.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_favoritesButton.ShowFocus = false;
            this.m_favoritesButton.UseVisualStyleBackColor = false;
            this.m_favoritesButton.Click += new System.EventHandler(this.m_favoritesButton_Click);
            // 
            // m_cancelButton
            // 
            this.m_cancelButton.BackColor = System.Drawing.Color.Transparent;
            this.m_cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.m_cancelButton.FocusColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.m_cancelButton, "m_cancelButton");
            this.m_cancelButton.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_cancelButton.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_cancelButton.Name = "m_cancelButton";
            this.m_cancelButton.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_cancelButton.ShowFocus = false;
            this.m_cancelButton.UseVisualStyleBackColor = false;
            this.m_cancelButton.Click += new System.EventHandler(this.imageButton1_Click);
            // 
            // CrystalBallPromptForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            resources.ApplyResources(this, "$this");
            this.ControlBox = false;
            this.Controls.Add(this.m_cancelButton);
            this.Controls.Add(this.m_favoritesButton);
            this.Controls.Add(this.m_timeoutProgress);
            this.Controls.Add(this.m_handPickButton);
            this.Controls.Add(this.m_scanButton);
            this.Controls.Add(this.m_selectionLabel);
            this.Controls.Add(this.m_quickPickButton);
            this.DrawAsGradient = true;
            this.DrawBorderOuterEdge = true;
            this.DrawRounded = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.GradientBeginColor = System.Drawing.Color.LightGray;
            this.GradientEndColor = System.Drawing.Color.DarkGray;
            this.Name = "CrystalBallPromptForm";
            this.ShowInTaskbar = false;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.CrystalBallPromptForm_FormClosed);
            this.Shown += new System.EventHandler(this.CrystalBallPromptForm_Shown);
            this.Click += new System.EventHandler(this.UserActivityDetected);
            this.ResumeLayout(false);

        }

        #endregion

        private GTI.Controls.ImageButton m_quickPickButton;
        private System.Windows.Forms.Label m_selectionLabel;
        private GTI.Controls.ImageButton m_scanButton;
        private GTI.Controls.ImageButton m_handPickButton;
        private System.Windows.Forms.Timer m_kioskIdleTimer;
        private System.Windows.Forms.ProgressBar m_timeoutProgress;
        private Controls.ImageButton m_favoritesButton;
        private Controls.ImageButton m_cancelButton;
    }
}