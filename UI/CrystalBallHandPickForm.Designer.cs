namespace GTI.Modules.POS.UI
{
    partial class CrystalBallHandPickForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CrystalBallHandPickForm));
            this.m_kioskIdleTimer = new System.Windows.Forms.Timer(this.components);
            this.m_panelMain = new System.Windows.Forms.Panel();
            this.m_timeoutProgress = new System.Windows.Forms.ProgressBar();
            this.m_numbersChosenLabel = new System.Windows.Forms.Label();
            this.m_currentCardLabel = new System.Windows.Forms.Label();
            this.m_prevLabel = new GTI.Controls.OutlinedLabel();
            this.m_nextLabel = new GTI.Controls.OutlinedLabel();
            this.m_bingoNumberBoard = new GTI.Controls.BingoFlashBoard();
            this.m_clearCurrentButton = new GTI.Controls.ImageButton();
            this.m_nextCardButton = new GTI.Controls.ImageButton();
            this.m_prevCardButton = new GTI.Controls.ImageButton();
            this.m_clearAllButton = new GTI.Controls.ImageButton();
            this.m_cancelButton = new GTI.Controls.ImageButton();
            this.m_finishedButton = new GTI.Controls.ImageButton();
            this.m_panelMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_kioskIdleTimer
            // 
            this.m_kioskIdleTimer.Interval = 500;
            this.m_kioskIdleTimer.Tick += new System.EventHandler(this.m_kioskIdleTimer_Tick);
            // 
            // m_panelMain
            // 
            resources.ApplyResources(this.m_panelMain, "m_panelMain");
            this.m_panelMain.BackColor = System.Drawing.Color.Transparent;
            this.m_panelMain.Controls.Add(this.m_prevLabel);
            this.m_panelMain.Controls.Add(this.m_nextLabel);
            this.m_panelMain.Controls.Add(this.m_timeoutProgress);
            this.m_panelMain.Controls.Add(this.m_bingoNumberBoard);
            this.m_panelMain.Controls.Add(this.m_clearCurrentButton);
            this.m_panelMain.Controls.Add(this.m_numbersChosenLabel);
            this.m_panelMain.Controls.Add(this.m_currentCardLabel);
            this.m_panelMain.Controls.Add(this.m_nextCardButton);
            this.m_panelMain.Controls.Add(this.m_prevCardButton);
            this.m_panelMain.Controls.Add(this.m_clearAllButton);
            this.m_panelMain.Controls.Add(this.m_cancelButton);
            this.m_panelMain.Controls.Add(this.m_finishedButton);
            this.m_panelMain.Name = "m_panelMain";
            this.m_panelMain.Click += new System.EventHandler(this.UserActivityDetected);
            // 
            // m_timeoutProgress
            // 
            resources.ApplyResources(this.m_timeoutProgress, "m_timeoutProgress");
            this.m_timeoutProgress.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(116)))), ((int)(((byte)(135)))), ((int)(((byte)(152)))));
            this.m_timeoutProgress.ForeColor = System.Drawing.Color.Gold;
            this.m_timeoutProgress.Name = "m_timeoutProgress";
            this.m_timeoutProgress.Click += new System.EventHandler(this.UserActivityDetected);
            // 
            // m_numbersChosenLabel
            // 
            this.m_numbersChosenLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(147)))), ((int)(((byte)(174)))), ((int)(((byte)(214)))));
            resources.ApplyResources(this.m_numbersChosenLabel, "m_numbersChosenLabel");
            this.m_numbersChosenLabel.Name = "m_numbersChosenLabel";
            this.m_numbersChosenLabel.Click += new System.EventHandler(this.UserActivityDetected);
            // 
            // m_currentCardLabel
            // 
            this.m_currentCardLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(147)))), ((int)(((byte)(174)))), ((int)(((byte)(214)))));
            resources.ApplyResources(this.m_currentCardLabel, "m_currentCardLabel");
            this.m_currentCardLabel.Name = "m_currentCardLabel";
            this.m_currentCardLabel.Click += new System.EventHandler(this.UserActivityDetected);
            // 
            // m_prevLabel
            // 
            this.m_prevLabel.EdgeColor = System.Drawing.Color.Teal;
            resources.ApplyResources(this.m_prevLabel, "m_prevLabel");
            this.m_prevLabel.ForeColor = System.Drawing.Color.Chartreuse;
            this.m_prevLabel.Name = "m_prevLabel";
            // 
            // m_nextLabel
            // 
            this.m_nextLabel.EdgeColor = System.Drawing.Color.Teal;
            resources.ApplyResources(this.m_nextLabel, "m_nextLabel");
            this.m_nextLabel.ForeColor = System.Drawing.Color.Chartreuse;
            this.m_nextLabel.Name = "m_nextLabel";
            // 
            // m_bingoNumberBoard
            // 
            this.m_bingoNumberBoard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(116)))), ((int)(((byte)(135)))), ((int)(((byte)(152)))));
            this.m_bingoNumberBoard.BallColumnSize = 15;
            this.m_bingoNumberBoard.ButtonFont = new System.Drawing.Font("Tahoma", 16.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_bingoNumberBoard.ButtonMargin = new System.Windows.Forms.Padding(2, 6, 3, 8);
            this.m_bingoNumberBoard.ButtonPadding = new System.Windows.Forms.Padding(2, 6, 3, 8);
            this.m_bingoNumberBoard.ButtonSize = new System.Drawing.Size(60, 60);
            resources.ApplyResources(this.m_bingoNumberBoard, "m_bingoNumberBoard");
            this.m_bingoNumberBoard.IsFlashBoardLocked = false;
            this.m_bingoNumberBoard.Name = "m_bingoNumberBoard";
            this.m_bingoNumberBoard.ScreenSize = new System.Drawing.Size(1024, 768);
            this.m_bingoNumberBoard.ShowFocus = false;
            this.m_bingoNumberBoard.TotalBallNumber = 75;
            this.m_bingoNumberBoard.OnBallClick += new GTI.Controls.BallClickEventHandler(this.NumberClick);
            // 
            // m_clearCurrentButton
            // 
            this.m_clearCurrentButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(207)))), ((int)(((byte)(201)))));
            this.m_clearCurrentButton.FocusColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.m_clearCurrentButton, "m_clearCurrentButton");
            this.m_clearCurrentButton.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_clearCurrentButton.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_clearCurrentButton.Name = "m_clearCurrentButton";
            this.m_clearCurrentButton.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_clearCurrentButton.ShowFocus = false;
            this.m_clearCurrentButton.UseVisualStyleBackColor = false;
            this.m_clearCurrentButton.Click += new System.EventHandler(this.ClearCurrentClick);
            // 
            // m_nextCardButton
            // 
            this.m_nextCardButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(116)))), ((int)(((byte)(135)))), ((int)(((byte)(152)))));
            this.m_nextCardButton.FocusColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.m_nextCardButton, "m_nextCardButton");
            this.m_nextCardButton.ImageIcon = global::GTI.Modules.POS.Properties.Resources.ArrowRight;
            this.m_nextCardButton.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_nextCardButton.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_nextCardButton.Name = "m_nextCardButton";
            this.m_nextCardButton.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_nextCardButton.ShowFocus = false;
            this.m_nextCardButton.UseVisualStyleBackColor = false;
            this.m_nextCardButton.Click += new System.EventHandler(this.NextCardClick);
            // 
            // m_prevCardButton
            // 
            this.m_prevCardButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(116)))), ((int)(((byte)(135)))), ((int)(((byte)(152)))));
            this.m_prevCardButton.FocusColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.m_prevCardButton, "m_prevCardButton");
            this.m_prevCardButton.ImageIcon = global::GTI.Modules.POS.Properties.Resources.ArrowLeft;
            this.m_prevCardButton.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_prevCardButton.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_prevCardButton.Name = "m_prevCardButton";
            this.m_prevCardButton.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_prevCardButton.ShowFocus = false;
            this.m_prevCardButton.UseVisualStyleBackColor = false;
            this.m_prevCardButton.Click += new System.EventHandler(this.PrevCardClick);
            // 
            // m_clearAllButton
            // 
            this.m_clearAllButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(207)))), ((int)(((byte)(201)))));
            this.m_clearAllButton.FocusColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.m_clearAllButton, "m_clearAllButton");
            this.m_clearAllButton.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_clearAllButton.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_clearAllButton.Name = "m_clearAllButton";
            this.m_clearAllButton.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_clearAllButton.ShowFocus = false;
            this.m_clearAllButton.UseVisualStyleBackColor = false;
            this.m_clearAllButton.Click += new System.EventHandler(this.ClearAllClick);
            // 
            // m_cancelButton
            // 
            this.m_cancelButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(207)))), ((int)(((byte)(201)))));
            this.m_cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.m_cancelButton.FocusColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.m_cancelButton, "m_cancelButton");
            this.m_cancelButton.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_cancelButton.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_cancelButton.Name = "m_cancelButton";
            this.m_cancelButton.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_cancelButton.ShowFocus = false;
            this.m_cancelButton.UseVisualStyleBackColor = false;
            // 
            // m_finishedButton
            // 
            this.m_finishedButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(207)))), ((int)(((byte)(201)))));
            this.m_finishedButton.FocusColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.m_finishedButton, "m_finishedButton");
            this.m_finishedButton.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_finishedButton.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_finishedButton.Name = "m_finishedButton";
            this.m_finishedButton.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_finishedButton.ShowFocus = false;
            this.m_finishedButton.UseVisualStyleBackColor = false;
            this.m_finishedButton.Click += new System.EventHandler(this.FinishedClick);
            // 
            // CrystalBallHandPickForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackgroundImage = global::GTI.Modules.POS.Properties.Resources.CBBHandPickBack1024;
            resources.ApplyResources(this, "$this");
            this.ControlBox = false;
            this.Controls.Add(this.m_panelMain);
            this.DrawBorderOuterEdge = true;
            this.DrawRounded = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "CrystalBallHandPickForm";
            this.ShowInTaskbar = false;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.CrystalBallHandPickForm_FormClosed);
            this.Shown += new System.EventHandler(this.CrystalBallHandPickForm_Shown);
            this.Click += new System.EventHandler(this.UserActivityDetected);
            this.m_panelMain.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private GTI.Controls.ImageButton m_finishedButton;
        private GTI.Controls.ImageButton m_cancelButton;
        private GTI.Controls.ImageButton m_clearAllButton;
        private GTI.Controls.BingoFlashBoard m_bingoNumberBoard;
        private GTI.Controls.ImageButton m_prevCardButton;
        private GTI.Controls.ImageButton m_nextCardButton;
        private System.Windows.Forms.Label m_currentCardLabel;
        private System.Windows.Forms.Label m_numbersChosenLabel;
        private GTI.Controls.ImageButton m_clearCurrentButton;
        private System.Windows.Forms.Panel m_panelMain;
        private System.Windows.Forms.Timer m_kioskIdleTimer;
        private System.Windows.Forms.ProgressBar m_timeoutProgress;
        private Controls.OutlinedLabel m_prevLabel;
        private Controls.OutlinedLabel m_nextLabel;
    }
}