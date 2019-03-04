namespace GTI.Modules.POS.UI
{
    partial class KeypadForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(KeypadForm));
            this.m_messageLabel = new System.Windows.Forms.Label();
            this.m_keypad = new GTI.Controls.Keypad();
            this.m_kioskIdleTimer = new System.Windows.Forms.Timer(this.components);
            this.m_timeoutProgress = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // m_messageLabel
            // 
            this.m_messageLabel.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.m_messageLabel, "m_messageLabel");
            this.m_messageLabel.ForeColor = System.Drawing.Color.Black;
            this.m_messageLabel.Name = "m_messageLabel";
            // 
            // m_keypad
            // 
            this.m_keypad.BackColor = System.Drawing.Color.Transparent;
            this.m_keypad.BigButtonFont = new System.Drawing.Font("Tahoma", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_keypad.BigButtonImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_keypad.BigButtonImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_keypad.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.m_keypad.ClearKeyFont = new System.Drawing.Font("Tahoma", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_keypad.CurrencySymbolForeColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.m_keypad, "m_keypad");
            this.m_keypad.InitialValue = null;
            this.m_keypad.KeyMode = GTI.Controls.Keypad.KeypadMode.Calculator;
            this.m_keypad.Name = "m_keypad";
            this.m_keypad.NumbersFont = new System.Drawing.Font("Tahoma", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_keypad.NumbersImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_keypad.NumbersImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_keypad.Option1ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_keypad.Option1ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_keypad.Option2ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_keypad.Option2ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_keypad.Option3ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_keypad.Option3ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_keypad.Option4ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_keypad.Option4ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_keypad.OptionButtonsFont = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.m_keypad.OptionButtonsPadding = new System.Windows.Forms.Padding(0);
            this.m_keypad.ShowFocus = false;
            this.m_keypad.Value = new decimal(new int[] {
            0,
            0,
            0,
            131072});
            this.m_keypad.ValueBackground = global::GTI.Modules.POS.Properties.Resources.TextBack;
            this.m_keypad.ValueForeColor = System.Drawing.Color.Yellow;
            this.m_keypad.Option1ButtonClick += new System.EventHandler(this.Option1Click);
            this.m_keypad.Option2ButtonClick += new System.EventHandler(this.Option2Click);
            this.m_keypad.Option3ButtonClick += new System.EventHandler(this.Option3Click);
            this.m_keypad.Option4ButtonClick += new System.EventHandler(this.Option4Click);
            this.m_keypad.BigButtonClick += new System.EventHandler(this.BigButtonClick);
            this.m_keypad.Click += new System.EventHandler(this.m_keypad_Click);
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
            // KeypadForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(127)))), ((int)(((byte)(127)))));
            resources.ApplyResources(this, "$this");
            this.ControlBox = false;
            this.Controls.Add(this.m_timeoutProgress);
            this.Controls.Add(this.m_messageLabel);
            this.Controls.Add(this.m_keypad);
            this.DrawAsGradient = true;
            this.DrawBorderOuterEdge = true;
            this.DrawRounded = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.GradientBeginColor = System.Drawing.Color.FromArgb(((int)(((byte)(211)))), ((int)(((byte)(207)))), ((int)(((byte)(211)))));
            this.GradientEndColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(127)))), ((int)(((byte)(127)))));
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "KeypadForm";
            this.OuterBorderEdgeColor = System.Drawing.Color.DimGray;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormClose);
            this.Shown += new System.EventHandler(this.KeypadForm_Shown);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.OnKeyPress);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label m_messageLabel;
        protected GTI.Controls.Keypad m_keypad;
        private System.Windows.Forms.Timer m_kioskIdleTimer;
        private System.Windows.Forms.ProgressBar m_timeoutProgress;
    }
}