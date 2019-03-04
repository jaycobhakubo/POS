namespace GTI.Modules.POS.UI
{
    partial class InputForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InputForm));
            this.m_virtualKeyboard = new GTI.Controls.VirtualKeyboard();
            this.m_textBox = new System.Windows.Forms.TextBox();
            this.m_promptLabel = new System.Windows.Forms.Label();
            this.m_okButton = new GTI.Controls.ImageButton();
            this.m_cancelButton = new GTI.Controls.ImageButton();
            this.m_textBackLabel = new GTI.Controls.ImageLabel();
            this.m_kioskIdleTimer = new System.Windows.Forms.Timer(this.components);
            this.m_timeoutProgress = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // m_virtualKeyboard
            // 
            this.m_virtualKeyboard.AltGrImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_virtualKeyboard.AltGrImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_virtualKeyboard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(75)))), ((int)(((byte)(117)))));
            this.m_virtualKeyboard.BackspaceImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_virtualKeyboard.BackspaceImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_virtualKeyboard.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.m_virtualKeyboard.CapsLockImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_virtualKeyboard.CapsLockImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_virtualKeyboard.EnterImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_virtualKeyboard.EnterImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_virtualKeyboard.KeyImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_virtualKeyboard.KeyImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            resources.ApplyResources(this.m_virtualKeyboard, "m_virtualKeyboard");
            this.m_virtualKeyboard.Name = "m_virtualKeyboard";
            this.m_virtualKeyboard.ShiftImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_virtualKeyboard.ShiftImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_virtualKeyboard.ShowFocus = false;
            this.m_virtualKeyboard.SpaceImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_virtualKeyboard.SpaceImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_virtualKeyboard.TabPipeImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_virtualKeyboard.TabPipeImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_virtualKeyboard.TabStop = false;
            this.m_virtualKeyboard.KeyPressed += new GTI.Controls.KeyboardEventHandler(this.KeyboardKeyPressed);
            // 
            // m_textBox
            // 
            this.m_textBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(19)))), ((int)(((byte)(60)))), ((int)(((byte)(96)))));
            this.m_textBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.m_textBox, "m_textBox");
            this.m_textBox.ForeColor = System.Drawing.Color.Yellow;
            this.m_textBox.Name = "m_textBox";
            this.m_textBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.InputKeyDown);
            // 
            // m_promptLabel
            // 
            this.m_promptLabel.AutoEllipsis = true;
            this.m_promptLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(87)))), ((int)(((byte)(83)))));
            resources.ApplyResources(this.m_promptLabel, "m_promptLabel");
            this.m_promptLabel.ForeColor = System.Drawing.Color.White;
            this.m_promptLabel.Name = "m_promptLabel";
            // 
            // m_okButton
            // 
            this.m_okButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(135)))), ((int)(((byte)(135)))), ((int)(((byte)(133)))));
            this.m_okButton.FocusColor = System.Drawing.Color.Black;
            this.m_okButton.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_okButton.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            resources.ApplyResources(this.m_okButton, "m_okButton");
            this.m_okButton.Name = "m_okButton";
            this.m_okButton.RepeatRate = 150;
            this.m_okButton.RepeatWhenHeldFor = 750;
            this.m_okButton.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_okButton.ShowFocus = false;
            this.m_okButton.TabStop = false;
            this.m_okButton.UseMnemonic = false;
            this.m_okButton.UseVisualStyleBackColor = false;
            this.m_okButton.Click += new System.EventHandler(this.OkClick);
            // 
            // m_cancelButton
            // 
            this.m_cancelButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(135)))), ((int)(((byte)(135)))), ((int)(((byte)(133)))));
            this.m_cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.m_cancelButton.FocusColor = System.Drawing.Color.Black;
            this.m_cancelButton.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_cancelButton.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            resources.ApplyResources(this.m_cancelButton, "m_cancelButton");
            this.m_cancelButton.Name = "m_cancelButton";
            this.m_cancelButton.RepeatRate = 150;
            this.m_cancelButton.RepeatWhenHeldFor = 750;
            this.m_cancelButton.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_cancelButton.ShowFocus = false;
            this.m_cancelButton.TabStop = false;
            this.m_cancelButton.UseMnemonic = false;
            this.m_cancelButton.UseVisualStyleBackColor = false;
            this.m_cancelButton.Click += new System.EventHandler(this.m_cancelButton_Click);
            // 
            // m_textBackLabel
            // 
            this.m_textBackLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(87)))), ((int)(((byte)(83)))));
            this.m_textBackLabel.Background = global::GTI.Modules.POS.Properties.Resources.TextBack;
            resources.ApplyResources(this.m_textBackLabel, "m_textBackLabel");
            this.m_textBackLabel.Name = "m_textBackLabel";
            // 
            // m_kioskIdleTimer
            // 
            this.m_kioskIdleTimer.Interval = 500;
            this.m_kioskIdleTimer.Tick += new System.EventHandler(this.m_kioskIdleTimer_Tick);
            // 
            // m_timeoutProgress
            // 
            this.m_timeoutProgress.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(127)))), ((int)(((byte)(127)))));
            this.m_timeoutProgress.ForeColor = System.Drawing.Color.Gold;
            resources.ApplyResources(this.m_timeoutProgress, "m_timeoutProgress");
            this.m_timeoutProgress.Name = "m_timeoutProgress";
            // 
            // InputForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoValidate = System.Windows.Forms.AutoValidate.Disable;
            this.BackgroundImage = global::GTI.Modules.POS.Properties.Resources.InputBack;
            resources.ApplyResources(this, "$this");
            this.CancelButton = this.m_cancelButton;
            this.ControlBox = false;
            this.Controls.Add(this.m_timeoutProgress);
            this.Controls.Add(this.m_cancelButton);
            this.Controls.Add(this.m_okButton);
            this.Controls.Add(this.m_promptLabel);
            this.Controls.Add(this.m_textBox);
            this.Controls.Add(this.m_textBackLabel);
            this.Controls.Add(this.m_virtualKeyboard);
            this.DoubleBuffered = true;
            this.DrawBorderOuterEdge = true;
            this.DrawRounded = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "InputForm";
            this.OuterBorderEdgeColor = System.Drawing.Color.DimGray;
            this.ShowInTaskbar = false;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.InputForm_FormClosed);
            this.Shown += new System.EventHandler(this.InputForm_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private GTI.Controls.VirtualKeyboard m_virtualKeyboard;
        private GTI.Controls.ImageLabel m_textBackLabel;
        private System.Windows.Forms.TextBox m_textBox;
        private System.Windows.Forms.Label m_promptLabel;
        private GTI.Controls.ImageButton m_okButton;
        private GTI.Controls.ImageButton m_cancelButton;
        private System.Windows.Forms.Timer m_kioskIdleTimer;
        private System.Windows.Forms.ProgressBar m_timeoutProgress;
    }
}