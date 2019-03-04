namespace GTI.Modules.POS.UI
{
	partial class POSNumericInputForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(POSNumericInputForm));
            this.m_idleTimer = new System.Windows.Forms.Timer(this.components);
            this.btnOK = new GTI.Controls.ImageButton();
            this.btnCancel = new GTI.Controls.ImageButton();
            this.m_timeoutProgress = new System.Windows.Forms.ProgressBar();
            this.eliteKeypad1 = new GTI.Controls.EliteKeypad();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.SuspendLayout();
            // 
            // m_idleTimer
            // 
            this.m_idleTimer.Interval = 500;
            this.m_idleTimer.Tick += new System.EventHandler(this.m_idleTimer_Tick);
            // 
            // btnOK
            // 
            this.btnOK.BackColor = System.Drawing.Color.Transparent;
            this.btnOK.FocusColor = System.Drawing.Color.Black;
            this.btnOK.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
            this.btnOK.ImageNormal = ((System.Drawing.Image)(resources.GetObject("btnOK.ImageNormal")));
            this.btnOK.ImagePressed = ((System.Drawing.Image)(resources.GetObject("btnOK.ImagePressed")));
            this.btnOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.btnOK.Location = new System.Drawing.Point(37, 400);
            this.btnOK.MinimumSize = new System.Drawing.Size(30, 40);
            this.btnOK.Name = "btnOK";
            this.btnOK.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.btnOK.Size = new System.Drawing.Size(115, 50);
            this.btnOK.TabIndex = 3;
            this.btnOK.Text = "&OK";
            this.btnOK.UseVisualStyleBackColor = false;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = System.Drawing.Color.Transparent;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.FocusColor = System.Drawing.Color.Black;
            this.btnCancel.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
            this.btnCancel.ImageNormal = ((System.Drawing.Image)(resources.GetObject("btnCancel.ImageNormal")));
            this.btnCancel.ImagePressed = ((System.Drawing.Image)(resources.GetObject("btnCancel.ImagePressed")));
            this.btnCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.btnCancel.Location = new System.Drawing.Point(166, 400);
            this.btnCancel.MinimumSize = new System.Drawing.Size(30, 40);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.btnCancel.Size = new System.Drawing.Size(115, 50);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "&Cancel";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // m_timeoutProgress
            // 
            this.m_timeoutProgress.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(186)))), ((int)(((byte)(192)))));
            this.m_timeoutProgress.ForeColor = System.Drawing.Color.Gold;
            this.m_timeoutProgress.Location = new System.Drawing.Point(12, 453);
            this.m_timeoutProgress.Name = "m_timeoutProgress";
            this.m_timeoutProgress.Size = new System.Drawing.Size(294, 10);
            this.m_timeoutProgress.TabIndex = 6;
            this.m_timeoutProgress.Visible = false;
            // 
            // eliteKeypad1
            // 
            this.eliteKeypad1.BackColor = System.Drawing.Color.Transparent;
            this.eliteKeypad1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.eliteKeypad1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.eliteKeypad1.Location = new System.Drawing.Point(41, 43);
            this.eliteKeypad1.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.eliteKeypad1.MaxCharacters = 32767;
            this.eliteKeypad1.Name = "eliteKeypad1";
            this.eliteKeypad1.Password = false;
            this.eliteKeypad1.Size = new System.Drawing.Size(236, 334);
            this.eliteKeypad1.TabIndex = 0;
            this.eliteKeypad1.TextResult = "";
            this.eliteKeypad1.TextResultHideSelection = true;
            this.eliteKeypad1.UseDecimalKey = true;
            this.eliteKeypad1.ValueChanged += new System.EventHandler(this.eliteKeypad1_ValueChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.Color.Transparent;
            this.groupBox1.Font = new System.Drawing.Font("Tahoma", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(12, 7);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(294, 387);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Enter amount dispensed\\r\\n(whole $s for bill dispenser)";
            // 
            // POSNumericInputForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.SystemColors.ControlDark;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(319, 463);
            this.ControlBox = false;
            this.Controls.Add(this.m_timeoutProgress);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.eliteKeypad1);
            this.Controls.Add(this.groupBox1);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "POSNumericInputForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.POSNumericInputForm_FormClosed);
            this.Shown += new System.EventHandler(this.POSNumericInputForm_Shown);
            this.Click += new System.EventHandler(this.UserActivityDetected);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.POSNumericInputForm_KeyPress);
            this.ResumeLayout(false);

		}

		#endregion

		private GTI.Controls.EliteKeypad eliteKeypad1;
		private GTI.Controls.ImageButton btnCancel;
		private GTI.Controls.ImageButton btnOK;
		private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Timer m_idleTimer;
        private System.Windows.Forms.ProgressBar m_timeoutProgress;

	}
}