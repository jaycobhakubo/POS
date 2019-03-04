namespace GTI.Modules.POS.UI
{
    partial class KioskKeypadForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(KioskKeypadForm));
            this.m_kioskIdleTimer = new System.Windows.Forms.Timer(this.components);
            this.m_btnClose = new GTI.Controls.ImageButton();
            this.m_btnRemove = new GTI.Controls.ImageButton();
            this.m_timeoutProgress = new System.Windows.Forms.ProgressBar();
            this.m_keypad = new GTI.Controls.Keypad();
            this.SuspendLayout();
            // 
            // m_kioskIdleTimer
            // 
            this.m_kioskIdleTimer.Interval = 500;
            this.m_kioskIdleTimer.Tick += new System.EventHandler(this.m_kioskIdleTimer_Tick);
            // 
            // m_btnClose
            // 
            this.m_btnClose.BackColor = System.Drawing.Color.Transparent;
            this.m_btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.m_btnClose.FocusColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.m_btnClose, "m_btnClose");
            this.m_btnClose.ImageNormal = global::GTI.Modules.POS.Properties.Resources.YellowButtonUp;
            this.m_btnClose.ImagePressed = global::GTI.Modules.POS.Properties.Resources.YellowButtonDown;
            this.m_btnClose.Name = "m_btnClose";
            this.m_btnClose.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_btnClose.ShowFocus = false;
            this.m_btnClose.UseVisualStyleBackColor = false;
            this.m_btnClose.Click += new System.EventHandler(this.m_btnClose_Click);
            // 
            // m_btnRemove
            // 
            this.m_btnRemove.BackColor = System.Drawing.Color.Transparent;
            this.m_btnRemove.DialogResult = System.Windows.Forms.DialogResult.No;
            this.m_btnRemove.FocusColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.m_btnRemove, "m_btnRemove");
            this.m_btnRemove.ImageNormal = global::GTI.Modules.POS.Properties.Resources.RedButtonUp;
            this.m_btnRemove.ImagePressed = global::GTI.Modules.POS.Properties.Resources.RedButtonDown;
            this.m_btnRemove.Name = "m_btnRemove";
            this.m_btnRemove.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_btnRemove.ShowFocus = false;
            this.m_btnRemove.UseVisualStyleBackColor = false;
            this.m_btnRemove.Click += new System.EventHandler(this.m_btnRemove_Click);
            // 
            // m_timeoutProgress
            // 
            this.m_timeoutProgress.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(127)))), ((int)(((byte)(127)))));
            this.m_timeoutProgress.ForeColor = System.Drawing.Color.Gold;
            resources.ApplyResources(this.m_timeoutProgress, "m_timeoutProgress");
            this.m_timeoutProgress.Name = "m_timeoutProgress";
            this.m_timeoutProgress.Click += new System.EventHandler(this.UserActivityDetected);
            // 
            // m_keypad
            // 
            this.m_keypad.BackColor = System.Drawing.Color.Transparent;
            this.m_keypad.BigButtonFont = new System.Drawing.Font("Tahoma", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_keypad.BigButtonImageNormal = global::GTI.Modules.POS.Properties.Resources.GreenButtonUp;
            this.m_keypad.BigButtonImagePressed = global::GTI.Modules.POS.Properties.Resources.GreenButtonDown;
            this.m_keypad.BigButtonText = "Add to Order";
            this.m_keypad.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.m_keypad.ClearKeyFont = new System.Drawing.Font("Tahoma", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_keypad.ClearKeyText = "CLR";
            this.m_keypad.CurrencySymbolForeColor = System.Drawing.Color.White;
            resources.ApplyResources(this.m_keypad, "m_keypad");
            this.m_keypad.InitialValue = null;
            this.m_keypad.KeyMode = GTI.Controls.Keypad.KeypadMode.Calculator;
            this.m_keypad.Name = "m_keypad";
            this.m_keypad.NumberDisplayMode = GTI.Controls.Keypad.NumberMode.Integer;
            this.m_keypad.NumbersFont = new System.Drawing.Font("Tahoma", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_keypad.NumbersImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_keypad.NumbersImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_keypad.Option1ImageNormal = global::GTI.Modules.POS.Properties.Resources.RedButtonUp;
            this.m_keypad.Option1ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_keypad.Option1Text = "Don\'t Change";
            this.m_keypad.Option2ImageNormal = global::GTI.Modules.POS.Properties.Resources.YellowButtonUp;
            this.m_keypad.Option2ImagePressed = global::GTI.Modules.POS.Properties.Resources.YellowButtonDown;
            this.m_keypad.Option2Text = "Remove Item from Order";
            this.m_keypad.Option3ImageNormal = global::GTI.Modules.POS.Properties.Resources.GreenButtonUp;
            this.m_keypad.Option3ImagePressed = global::GTI.Modules.POS.Properties.Resources.GreenButtonDown;
            this.m_keypad.Option3Text = "Add to Order";
            this.m_keypad.Option4ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_keypad.Option4ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_keypad.Option4Visible = false;
            this.m_keypad.OptionButtonsFont = new System.Drawing.Font("Tahoma", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_keypad.OptionButtonsPadding = new System.Windows.Forms.Padding(0);
            this.m_keypad.ShowFocus = false;
            this.m_keypad.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.m_keypad.ValueBackground = global::GTI.Modules.POS.Properties.Resources.TextBack;
            this.m_keypad.ValueForeColor = System.Drawing.Color.Yellow;
            this.m_keypad.BigButtonClick += new System.EventHandler(this.BigButtonClick);
            this.m_keypad.Click += new System.EventHandler(this.UserActivityDetected);
            // 
            // KioskKeypadForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackgroundImage = global::GTI.Modules.POS.Properties.Resources.KioskKeypadBack;
            resources.ApplyResources(this, "$this");
            this.ControlBox = false;
            this.Controls.Add(this.m_btnClose);
            this.Controls.Add(this.m_btnRemove);
            this.Controls.Add(this.m_timeoutProgress);
            this.Controls.Add(this.m_keypad);
            this.DoubleBuffered = true;
            this.DrawBorderOuterEdge = true;
            this.DrawRounded = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "KioskKeypadForm";
            this.OuterBorderEdgeColor = System.Drawing.Color.DimGray;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormClose);
            this.Shown += new System.EventHandler(this.KeypadForm_Shown);
            this.Click += new System.EventHandler(this.UserActivityDetected);
            this.ResumeLayout(false);

        }

        #endregion

        protected GTI.Controls.Keypad m_keypad;
        private System.Windows.Forms.Timer m_kioskIdleTimer;
        private System.Windows.Forms.ProgressBar m_timeoutProgress;
        private Controls.ImageButton m_btnRemove;
        private Controls.ImageButton m_btnClose;
    }
}