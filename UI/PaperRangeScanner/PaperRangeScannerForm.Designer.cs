namespace GTI.Modules.POS.UI.PaperRangeScanner
{
    partial class PaperRangeScannerForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PaperRangeScannerForm));
            this.lblInstructions = new System.Windows.Forms.Label();
            this.m_textBackLabel = new GTI.Controls.ImageLabel();
            this.imageLabel1 = new GTI.Controls.ImageLabel();
            this.imageLabel2 = new GTI.Controls.ImageLabel();
            this.m_serialNumberLabel = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.m_virtualKeyboard = new GTI.Controls.VirtualKeyboard();
            this.CancelBtn = new GTI.Controls.ImageButton();
            this.OkBtn = new GTI.Controls.ImageButton();
            this.SerialNumberTxtBx = new System.Windows.Forms.TextBox();
            this.StartingAuditTxtBx = new System.Windows.Forms.TextBox();
            this.EndingAuditTxtBx = new System.Windows.Forms.TextBox();
            this.infoMessageSerial = new System.Windows.Forms.Label();
            this.infoMessageAudit1 = new System.Windows.Forms.Label();
            this.infoMessageAudit2 = new System.Windows.Forms.Label();
            this.imageButton1 = new GTI.Controls.ImageButton();
            this.SuspendLayout();
            // 
            // lblInstructions
            // 
            resources.ApplyResources(this.lblInstructions, "lblInstructions");
            this.lblInstructions.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(208)))), ((int)(((byte)(213)))));
            this.lblInstructions.Name = "lblInstructions";
            // 
            // m_textBackLabel
            // 
            this.m_textBackLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(87)))), ((int)(((byte)(83)))));
            this.m_textBackLabel.Background = global::GTI.Modules.POS.Properties.Resources.TextBack;
            resources.ApplyResources(this.m_textBackLabel, "m_textBackLabel");
            this.m_textBackLabel.Name = "m_textBackLabel";
            // 
            // imageLabel1
            // 
            this.imageLabel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(87)))), ((int)(((byte)(83)))));
            this.imageLabel1.Background = global::GTI.Modules.POS.Properties.Resources.TextBack;
            resources.ApplyResources(this.imageLabel1, "imageLabel1");
            this.imageLabel1.Name = "imageLabel1";
            // 
            // imageLabel2
            // 
            this.imageLabel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(87)))), ((int)(((byte)(83)))));
            this.imageLabel2.Background = global::GTI.Modules.POS.Properties.Resources.TextBack;
            resources.ApplyResources(this.imageLabel2, "imageLabel2");
            this.imageLabel2.Name = "imageLabel2";
            // 
            // m_serialNumberLabel
            // 
            this.m_serialNumberLabel.AutoEllipsis = true;
            this.m_serialNumberLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(87)))), ((int)(((byte)(83)))));
            resources.ApplyResources(this.m_serialNumberLabel, "m_serialNumberLabel");
            this.m_serialNumberLabel.ForeColor = System.Drawing.Color.White;
            this.m_serialNumberLabel.Name = "m_serialNumberLabel";
            // 
            // label1
            // 
            this.label1.AutoEllipsis = true;
            this.label1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(87)))), ((int)(((byte)(83)))));
            resources.ApplyResources(this.label1, "label1");
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Name = "label1";
            // 
            // label2
            // 
            this.label2.AutoEllipsis = true;
            this.label2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(87)))), ((int)(((byte)(83)))));
            resources.ApplyResources(this.label2, "label2");
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Name = "label2";
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
            resources.ApplyResources(this.m_virtualKeyboard, "m_virtualKeyboard");
            this.m_virtualKeyboard.KeyImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_virtualKeyboard.KeyImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
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
            // CancelBtn
            // 
            resources.ApplyResources(this.CancelBtn, "CancelBtn");
            this.CancelBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(135)))), ((int)(((byte)(135)))), ((int)(((byte)(133)))));
            this.CancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelBtn.FocusColor = System.Drawing.Color.Black;
            this.CancelBtn.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.CancelBtn.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.CancelBtn.Name = "CancelBtn";
            this.CancelBtn.RepeatRate = 150;
            this.CancelBtn.RepeatWhenHeldFor = 750;
            this.CancelBtn.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.CancelBtn.ShowFocus = false;
            this.CancelBtn.TabStop = false;
            this.CancelBtn.UseMnemonic = false;
            this.CancelBtn.UseVisualStyleBackColor = false;
            this.CancelBtn.Click += new System.EventHandler(this.CancelBtn_Click);
            // 
            // OkBtn
            // 
            resources.ApplyResources(this.OkBtn, "OkBtn");
            this.OkBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(135)))), ((int)(((byte)(135)))), ((int)(((byte)(133)))));
            this.OkBtn.FocusColor = System.Drawing.Color.Black;
            this.OkBtn.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.OkBtn.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.OkBtn.Name = "OkBtn";
            this.OkBtn.RepeatRate = 150;
            this.OkBtn.RepeatWhenHeldFor = 750;
            this.OkBtn.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.OkBtn.ShowFocus = false;
            this.OkBtn.TabStop = false;
            this.OkBtn.UseMnemonic = false;
            this.OkBtn.UseVisualStyleBackColor = false;
            this.OkBtn.Click += new System.EventHandler(this.OkBtn_Click);
            // 
            // SerialNumberTxtBx
            // 
            this.SerialNumberTxtBx.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(19)))), ((int)(((byte)(60)))), ((int)(((byte)(96)))));
            this.SerialNumberTxtBx.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.SerialNumberTxtBx, "SerialNumberTxtBx");
            this.SerialNumberTxtBx.ForeColor = System.Drawing.Color.Yellow;
            this.SerialNumberTxtBx.Name = "SerialNumberTxtBx";
            this.SerialNumberTxtBx.Click += new System.EventHandler(this.ClickTextBox);
            this.SerialNumberTxtBx.KeyDown += new System.Windows.Forms.KeyEventHandler(this.InputKeyDown);
            // 
            // StartingAuditTxtBx
            // 
            this.StartingAuditTxtBx.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(19)))), ((int)(((byte)(60)))), ((int)(((byte)(96)))));
            this.StartingAuditTxtBx.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.StartingAuditTxtBx, "StartingAuditTxtBx");
            this.StartingAuditTxtBx.ForeColor = System.Drawing.Color.Yellow;
            this.StartingAuditTxtBx.Name = "StartingAuditTxtBx";
            this.StartingAuditTxtBx.Click += new System.EventHandler(this.ClickTextBox);
            this.StartingAuditTxtBx.KeyDown += new System.Windows.Forms.KeyEventHandler(this.InputKeyDown);
            this.StartingAuditTxtBx.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.AuditKeyPress);
            // 
            // EndingAuditTxtBx
            // 
            this.EndingAuditTxtBx.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(19)))), ((int)(((byte)(60)))), ((int)(((byte)(96)))));
            this.EndingAuditTxtBx.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.EndingAuditTxtBx, "EndingAuditTxtBx");
            this.EndingAuditTxtBx.ForeColor = System.Drawing.Color.Yellow;
            this.EndingAuditTxtBx.Name = "EndingAuditTxtBx";
            this.EndingAuditTxtBx.Click += new System.EventHandler(this.ClickTextBox);
            this.EndingAuditTxtBx.KeyDown += new System.Windows.Forms.KeyEventHandler(this.InputKeyDown);
            this.EndingAuditTxtBx.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.AuditKeyPress);
            // 
            // infoMessageSerial
            // 
            this.infoMessageSerial.AutoEllipsis = true;
            this.infoMessageSerial.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(87)))), ((int)(((byte)(83)))));
            resources.ApplyResources(this.infoMessageSerial, "infoMessageSerial");
            this.infoMessageSerial.ForeColor = System.Drawing.Color.Yellow;
            this.infoMessageSerial.Name = "infoMessageSerial";
            // 
            // infoMessageAudit1
            // 
            this.infoMessageAudit1.AutoEllipsis = true;
            this.infoMessageAudit1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(87)))), ((int)(((byte)(83)))));
            resources.ApplyResources(this.infoMessageAudit1, "infoMessageAudit1");
            this.infoMessageAudit1.ForeColor = System.Drawing.Color.Yellow;
            this.infoMessageAudit1.Name = "infoMessageAudit1";
            // 
            // infoMessageAudit2
            // 
            this.infoMessageAudit2.AutoEllipsis = true;
            this.infoMessageAudit2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(87)))), ((int)(((byte)(83)))));
            resources.ApplyResources(this.infoMessageAudit2, "infoMessageAudit2");
            this.infoMessageAudit2.ForeColor = System.Drawing.Color.Yellow;
            this.infoMessageAudit2.Name = "infoMessageAudit2";
            // 
            // imageButton1
            // 
            resources.ApplyResources(this.imageButton1, "imageButton1");
            this.imageButton1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(135)))), ((int)(((byte)(135)))), ((int)(((byte)(133)))));
            this.imageButton1.FocusColor = System.Drawing.Color.Black;
            this.imageButton1.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.imageButton1.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.imageButton1.Name = "imageButton1";
            this.imageButton1.RepeatRate = 150;
            this.imageButton1.RepeatWhenHeldFor = 750;
            this.imageButton1.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.imageButton1.ShowFocus = false;
            this.imageButton1.TabStop = false;
            this.imageButton1.UseMnemonic = false;
            this.imageButton1.UseVisualStyleBackColor = false;
            this.imageButton1.Click += new System.EventHandler(this.imageButton1_Click);
            // 
            // PaperRangeScannerForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::GTI.Modules.POS.Properties.Resources.PaperRangeScannerBack;
            this.CancelButton = this.CancelBtn;
            this.Controls.Add(this.imageButton1);
            this.Controls.Add(this.infoMessageAudit2);
            this.Controls.Add(this.infoMessageAudit1);
            this.Controls.Add(this.infoMessageSerial);
            this.Controls.Add(this.EndingAuditTxtBx);
            this.Controls.Add(this.StartingAuditTxtBx);
            this.Controls.Add(this.SerialNumberTxtBx);
            this.Controls.Add(this.CancelBtn);
            this.Controls.Add(this.OkBtn);
            this.Controls.Add(this.m_virtualKeyboard);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.m_serialNumberLabel);
            this.Controls.Add(this.imageLabel2);
            this.Controls.Add(this.imageLabel1);
            this.Controls.Add(this.m_textBackLabel);
            this.Controls.Add(this.lblInstructions);
            this.DoubleBuffered = true;
            this.DrawBorderOuterEdge = true;
            this.DrawRounded = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PaperRangeScannerForm";
            this.OuterBorderEdgeColor = System.Drawing.Color.DimGray;
            this.Load += new System.EventHandler(this.PaperRangeScannerForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblInstructions;
        private GTI.Controls.ImageLabel m_textBackLabel;
        private GTI.Controls.ImageLabel imageLabel1;
        private GTI.Controls.ImageLabel imageLabel2;
        private System.Windows.Forms.Label m_serialNumberLabel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private Controls.VirtualKeyboard m_virtualKeyboard;
        private Controls.ImageButton CancelBtn;
        private Controls.ImageButton OkBtn;
        private System.Windows.Forms.TextBox SerialNumberTxtBx;
        private System.Windows.Forms.TextBox StartingAuditTxtBx;
        private System.Windows.Forms.TextBox EndingAuditTxtBx;
        private System.Windows.Forms.Label infoMessageSerial;
        private System.Windows.Forms.Label infoMessageAudit1;
        private System.Windows.Forms.Label infoMessageAudit2;
        private Controls.ImageButton imageButton1;
    }
}

