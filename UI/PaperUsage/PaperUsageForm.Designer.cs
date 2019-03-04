using System.Drawing;
using GTI.Controls;

namespace GTI.Modules.POS.UI
{
    partial class PaperUsageForm
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
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.errIcon = new System.Windows.Forms.PictureBox();
            this.StatusLabel = new System.Windows.Forms.Label();
            this.TotalLabel = new System.Windows.Forms.Label();
            this.StaffLabel = new System.Windows.Forms.Label();
            this.SessionLabel = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.UnscannedPacksListBox = new System.Windows.Forms.ListBox();
            this.label14 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.TitleLabel = new System.Windows.Forms.Label();
            this.BtnAdd = new GTI.Controls.ImageButton();
            this.BtnRemove = new GTI.Controls.ImageButton();
            this.BtnConfirmAndPrint = new GTI.Controls.ImageButton();
            this.BtnPrintUnscannedPacks = new GTI.Controls.ImageButton();
            this.BtnPrint = new GTI.Controls.ImageButton();
            this.m_backButton = new GTI.Controls.ImageButton();
            this.m_decimalButton = new GTI.Controls.ImageButton();
            this.m_button2 = new GTI.Controls.ImageButton();
            this.m_button3 = new GTI.Controls.ImageButton();
            this.m_button6 = new GTI.Controls.ImageButton();
            this.m_button5 = new GTI.Controls.ImageButton();
            this.m_button4 = new GTI.Controls.ImageButton();
            this.m_button7 = new GTI.Controls.ImageButton();
            this.m_button8 = new GTI.Controls.ImageButton();
            this.m_button9 = new GTI.Controls.ImageButton();
            this.m_button0 = new GTI.Controls.ImageButton();
            this.m_button1 = new GTI.Controls.ImageButton();
            this.BtnClosePaper = new GTI.Controls.ImageButton();
            this.PaperUsageFlowPanel = new GTI.Controls.DoubleBufferFlowLayoutPanel();
            this.BtnSave = new GTI.Controls.ImageButton();
            this.BtnExit = new GTI.Controls.ImageButton();
            this.flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.BackColor = System.Drawing.Color.Transparent;
            this.flowLayoutPanel1.Controls.Add(this.errIcon);
            this.flowLayoutPanel1.Controls.Add(this.StatusLabel);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(21, 651);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(488, 29);
            this.flowLayoutPanel1.TabIndex = 61;
            // 
            // errIcon
            // 
            this.errIcon.BackColor = System.Drawing.Color.Transparent;
            this.errIcon.Image = global::GTI.Modules.POS.Properties.Resources.WarningIcon3;
            this.errIcon.Location = new System.Drawing.Point(0, 1);
            this.errIcon.Margin = new System.Windows.Forms.Padding(0, 1, 0, 0);
            this.errIcon.Name = "errIcon";
            this.errIcon.Size = new System.Drawing.Size(22, 20);
            this.errIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.errIcon.TabIndex = 60;
            this.errIcon.TabStop = false;
            this.errIcon.Visible = false;
            // 
            // StatusLabel
            // 
            this.StatusLabel.BackColor = System.Drawing.Color.Transparent;
            this.StatusLabel.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.StatusLabel.ForeColor = System.Drawing.Color.White;
            this.StatusLabel.Location = new System.Drawing.Point(22, 0);
            this.StatusLabel.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(452, 23);
            this.StatusLabel.TabIndex = 38;
            this.StatusLabel.Text = "Status Label";
            this.StatusLabel.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // TotalLabel
            // 
            this.TotalLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.TotalLabel.BackColor = System.Drawing.Color.Transparent;
            this.TotalLabel.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TotalLabel.ForeColor = System.Drawing.Color.Yellow;
            this.TotalLabel.Location = new System.Drawing.Point(566, 655);
            this.TotalLabel.Name = "TotalLabel";
            this.TotalLabel.Size = new System.Drawing.Size(114, 18);
            this.TotalLabel.TabIndex = 42;
            this.TotalLabel.Text = "$9999999.99";
            this.TotalLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // StaffLabel
            // 
            this.StaffLabel.AutoSize = true;
            this.StaffLabel.BackColor = System.Drawing.Color.Transparent;
            this.StaffLabel.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.StaffLabel.ForeColor = System.Drawing.Color.Black;
            this.StaffLabel.Location = new System.Drawing.Point(84, 10);
            this.StaffLabel.Name = "StaffLabel";
            this.StaffLabel.Size = new System.Drawing.Size(79, 16);
            this.StaffLabel.TabIndex = 40;
            this.StaffLabel.Text = "Staff Name";
            // 
            // SessionLabel
            // 
            this.SessionLabel.AutoSize = true;
            this.SessionLabel.BackColor = System.Drawing.Color.Transparent;
            this.SessionLabel.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SessionLabel.ForeColor = System.Drawing.Color.Black;
            this.SessionLabel.Location = new System.Drawing.Point(84, 28);
            this.SessionLabel.Name = "SessionLabel";
            this.SessionLabel.Size = new System.Drawing.Size(16, 16);
            this.SessionLabel.TabIndex = 39;
            this.SessionLabel.Text = "1";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.BackColor = System.Drawing.Color.Transparent;
            this.label8.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.ForeColor = System.Drawing.Color.Black;
            this.label8.Location = new System.Drawing.Point(528, 65);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(35, 18);
            this.label8.TabIndex = 37;
            this.label8.Text = "Bnz";
            this.label8.Visible = false;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.BackColor = System.Drawing.Color.Transparent;
            this.label15.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label15.ForeColor = System.Drawing.Color.Yellow;
            this.label15.Location = new System.Drawing.Point(510, 654);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(58, 19);
            this.label15.TabIndex = 33;
            this.label15.Text = "Total:";
            // 
            // UnscannedPacksListBox
            // 
            this.UnscannedPacksListBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(74)))), ((int)(((byte)(117)))));
            this.UnscannedPacksListBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.UnscannedPacksListBox.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.UnscannedPacksListBox.ForeColor = System.Drawing.Color.Yellow;
            this.UnscannedPacksListBox.FormattingEnabled = true;
            this.UnscannedPacksListBox.ItemHeight = 18;
            this.UnscannedPacksListBox.Location = new System.Drawing.Point(735, 99);
            this.UnscannedPacksListBox.Name = "UnscannedPacksListBox";
            this.UnscannedPacksListBox.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.UnscannedPacksListBox.Size = new System.Drawing.Size(268, 288);
            this.UnscannedPacksListBox.Sorted = true;
            this.UnscannedPacksListBox.TabIndex = 36;
            this.UnscannedPacksListBox.TabStop = false;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.BackColor = System.Drawing.Color.Transparent;
            this.label14.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label14.ForeColor = System.Drawing.Color.Black;
            this.label14.Location = new System.Drawing.Point(732, 65);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(137, 18);
            this.label14.TabIndex = 32;
            this.label14.Text = "Unscanned Packs";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.BackColor = System.Drawing.Color.Transparent;
            this.label17.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label17.ForeColor = System.Drawing.Color.Black;
            this.label17.Location = new System.Drawing.Point(19, 28);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(66, 16);
            this.label17.TabIndex = 30;
            this.label17.Text = "Session: ";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.BackColor = System.Drawing.Color.Transparent;
            this.label16.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label16.ForeColor = System.Drawing.Color.Black;
            this.label16.Location = new System.Drawing.Point(19, 10);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(49, 16);
            this.label16.TabIndex = 29;
            this.label16.Text = "Staff: ";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.BackColor = System.Drawing.Color.Transparent;
            this.label11.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.ForeColor = System.Drawing.Color.Black;
            this.label11.Location = new System.Drawing.Point(537, 65);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(47, 18);
            this.label11.TabIndex = 28;
            this.label11.Text = "Price";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.BackColor = System.Drawing.Color.Transparent;
            this.label7.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.ForeColor = System.Drawing.Color.Black;
            this.label7.Location = new System.Drawing.Point(469, 65);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(41, 18);
            this.label7.TabIndex = 25;
            this.label7.Text = "Dmg";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.BackColor = System.Drawing.Color.Transparent;
            this.label6.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.Color.Black;
            this.label6.Location = new System.Drawing.Point(419, 65);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(36, 18);
            this.label6.TabIndex = 24;
            this.label6.Text = "Skp";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.BackColor = System.Drawing.Color.Transparent;
            this.label10.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.ForeColor = System.Drawing.Color.Black;
            this.label10.Location = new System.Drawing.Point(636, 65);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(50, 18);
            this.label10.TabIndex = 23;
            this.label10.Text = "Value";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.BackColor = System.Drawing.Color.Transparent;
            this.label5.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.Black;
            this.label5.Location = new System.Drawing.Point(301, 65);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(35, 18);
            this.label5.TabIndex = 22;
            this.label5.Text = "End";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.BackColor = System.Drawing.Color.Transparent;
            this.label4.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.Black;
            this.label4.Location = new System.Drawing.Point(363, 65);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(35, 18);
            this.label4.TabIndex = 21;
            this.label4.Text = "Qty";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.Black;
            this.label3.Location = new System.Drawing.Point(229, 65);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(46, 18);
            this.label3.TabIndex = 20;
            this.label3.Text = "Start";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Black;
            this.label2.Location = new System.Drawing.Point(17, 65);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(51, 18);
            this.label2.TabIndex = 19;
            this.label2.Text = "Name";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Black;
            this.label1.Location = new System.Drawing.Point(152, 65);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 18);
            this.label1.TabIndex = 18;
            this.label1.Text = "Serial";
            // 
            // TitleLabel
            // 
            this.TitleLabel.AutoSize = true;
            this.TitleLabel.BackColor = System.Drawing.Color.Transparent;
            this.TitleLabel.Font = new System.Drawing.Font("Tahoma", 20F, System.Drawing.FontStyle.Bold);
            this.TitleLabel.ForeColor = System.Drawing.Color.Black;
            this.TitleLabel.Location = new System.Drawing.Point(420, 7);
            this.TitleLabel.Name = "TitleLabel";
            this.TitleLabel.Size = new System.Drawing.Size(185, 33);
            this.TitleLabel.TabIndex = 16;
            this.TitleLabel.Text = "Paper Usage";
            this.TitleLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // BtnAdd
            // 
            this.BtnAdd.BackColor = System.Drawing.Color.Transparent;
            this.BtnAdd.FocusColor = System.Drawing.Color.Black;
            this.BtnAdd.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.BtnAdd.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.BtnAdd.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.BtnAdd.Location = new System.Drawing.Point(47, 697);
            this.BtnAdd.MinimumSize = new System.Drawing.Size(30, 30);
            this.BtnAdd.Name = "BtnAdd";
            this.BtnAdd.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.BtnAdd.ShowFocus = false;
            this.BtnAdd.Size = new System.Drawing.Size(150, 50);
            this.BtnAdd.TabIndex = 59;
            this.BtnAdd.TabStop = false;
            this.BtnAdd.Text = "Add";
            this.BtnAdd.UseMnemonic = false;
            this.BtnAdd.UseVisualStyleBackColor = false;
            this.BtnAdd.Click += new System.EventHandler(this.BtnAdd_Click);
            // 
            // BtnRemove
            // 
            this.BtnRemove.BackColor = System.Drawing.Color.Transparent;
            this.BtnRemove.FocusColor = System.Drawing.Color.Black;
            this.BtnRemove.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.BtnRemove.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.BtnRemove.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.BtnRemove.Location = new System.Drawing.Point(203, 697);
            this.BtnRemove.MinimumSize = new System.Drawing.Size(30, 30);
            this.BtnRemove.Name = "BtnRemove";
            this.BtnRemove.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.BtnRemove.ShowFocus = false;
            this.BtnRemove.Size = new System.Drawing.Size(150, 50);
            this.BtnRemove.TabIndex = 58;
            this.BtnRemove.TabStop = false;
            this.BtnRemove.Text = "Remove";
            this.BtnRemove.UseMnemonic = false;
            this.BtnRemove.UseVisualStyleBackColor = false;
            this.BtnRemove.Click += new System.EventHandler(this.BtnRemoveClick);
            // 
            // BtnConfirmAndPrint
            // 
            this.BtnConfirmAndPrint.BackColor = System.Drawing.Color.Transparent;
            this.BtnConfirmAndPrint.FocusColor = System.Drawing.Color.Black;
            this.BtnConfirmAndPrint.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.BtnConfirmAndPrint.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.BtnConfirmAndPrint.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.BtnConfirmAndPrint.Location = new System.Drawing.Point(828, 697);
            this.BtnConfirmAndPrint.MinimumSize = new System.Drawing.Size(30, 30);
            this.BtnConfirmAndPrint.Name = "BtnConfirmAndPrint";
            this.BtnConfirmAndPrint.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.BtnConfirmAndPrint.ShowFocus = false;
            this.BtnConfirmAndPrint.Size = new System.Drawing.Size(150, 50);
            this.BtnConfirmAndPrint.TabIndex = 57;
            this.BtnConfirmAndPrint.TabStop = false;
            this.BtnConfirmAndPrint.Text = "Confirm & Print";
            this.BtnConfirmAndPrint.UseMnemonic = false;
            this.BtnConfirmAndPrint.UseVisualStyleBackColor = false;
            this.BtnConfirmAndPrint.Click += new System.EventHandler(this.BtnConfirmAndPrintClick);
            // 
            // BtnPrintUnscannedPacks
            // 
            this.BtnPrintUnscannedPacks.BackColor = System.Drawing.Color.Transparent;
            this.BtnPrintUnscannedPacks.FocusColor = System.Drawing.Color.Black;
            this.BtnPrintUnscannedPacks.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.BtnPrintUnscannedPacks.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.BtnPrintUnscannedPacks.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.BtnPrintUnscannedPacks.Location = new System.Drawing.Point(735, 393);
            this.BtnPrintUnscannedPacks.MinimumSize = new System.Drawing.Size(30, 30);
            this.BtnPrintUnscannedPacks.Name = "BtnPrintUnscannedPacks";
            this.BtnPrintUnscannedPacks.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.BtnPrintUnscannedPacks.ShowFocus = false;
            this.BtnPrintUnscannedPacks.Size = new System.Drawing.Size(271, 41);
            this.BtnPrintUnscannedPacks.TabIndex = 56;
            this.BtnPrintUnscannedPacks.TabStop = false;
            this.BtnPrintUnscannedPacks.Text = "Print Unscanned Packs";
            this.BtnPrintUnscannedPacks.UseMnemonic = false;
            this.BtnPrintUnscannedPacks.UseVisualStyleBackColor = false;
            this.BtnPrintUnscannedPacks.Click += new System.EventHandler(this.BtnPrintUnscannedPacksClick);
            // 
            // BtnPrint
            // 
            this.BtnPrint.BackColor = System.Drawing.Color.Transparent;
            this.BtnPrint.FocusColor = System.Drawing.Color.Black;
            this.BtnPrint.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.BtnPrint.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.BtnPrint.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.BtnPrint.Location = new System.Drawing.Point(515, 697);
            this.BtnPrint.MinimumSize = new System.Drawing.Size(30, 30);
            this.BtnPrint.Name = "BtnPrint";
            this.BtnPrint.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.BtnPrint.ShowFocus = false;
            this.BtnPrint.Size = new System.Drawing.Size(150, 50);
            this.BtnPrint.TabIndex = 55;
            this.BtnPrint.TabStop = false;
            this.BtnPrint.Text = "Print";
            this.BtnPrint.UseMnemonic = false;
            this.BtnPrint.UseVisualStyleBackColor = false;
            this.BtnPrint.Click += new System.EventHandler(this.BtnPrintClick);
            // 
            // m_backButton
            // 
            this.m_backButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(87)))), ((int)(((byte)(83)))));
            this.m_backButton.FocusColor = System.Drawing.Color.Black;
            this.m_backButton.Font = new System.Drawing.Font("Tahoma", 24F, System.Drawing.FontStyle.Bold);
            this.m_backButton.ImageIcon = global::GTI.Modules.POS.Properties.Resources.ArrowLeft;
            this.m_backButton.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_backButton.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_backButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.m_backButton.Location = new System.Drawing.Point(919, 622);
            this.m_backButton.MinimumSize = new System.Drawing.Size(30, 30);
            this.m_backButton.Name = "m_backButton";
            this.m_backButton.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_backButton.ShowFocus = false;
            this.m_backButton.Size = new System.Drawing.Size(87, 52);
            this.m_backButton.TabIndex = 44;
            this.m_backButton.TabStop = false;
            this.m_backButton.UseMnemonic = false;
            this.m_backButton.UseVisualStyleBackColor = false;
            this.m_backButton.Click += new System.EventHandler(this.BackButtonClick);
            // 
            // m_decimalButton
            // 
            this.m_decimalButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(87)))), ((int)(((byte)(83)))));
            this.m_decimalButton.FocusColor = System.Drawing.Color.Black;
            this.m_decimalButton.Font = new System.Drawing.Font("Tahoma", 24F, System.Drawing.FontStyle.Bold);
            this.m_decimalButton.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_decimalButton.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_decimalButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.m_decimalButton.Location = new System.Drawing.Point(735, 622);
            this.m_decimalButton.MinimumSize = new System.Drawing.Size(30, 30);
            this.m_decimalButton.Name = "m_decimalButton";
            this.m_decimalButton.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_decimalButton.ShowFocus = false;
            this.m_decimalButton.Size = new System.Drawing.Size(87, 52);
            this.m_decimalButton.TabIndex = 43;
            this.m_decimalButton.TabStop = false;
            this.m_decimalButton.Text = ".";
            this.m_decimalButton.UseMnemonic = false;
            this.m_decimalButton.UseVisualStyleBackColor = false;
            this.m_decimalButton.Click += new System.EventHandler(this.DecimalButtonClick);
            // 
            // m_button2
            // 
            this.m_button2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(87)))), ((int)(((byte)(83)))));
            this.m_button2.FocusColor = System.Drawing.Color.Black;
            this.m_button2.Font = new System.Drawing.Font("Tahoma", 24F, System.Drawing.FontStyle.Bold);
            this.m_button2.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_button2.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_button2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.m_button2.Location = new System.Drawing.Point(827, 566);
            this.m_button2.MinimumSize = new System.Drawing.Size(30, 30);
            this.m_button2.Name = "m_button2";
            this.m_button2.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_button2.ShowFocus = false;
            this.m_button2.Size = new System.Drawing.Size(87, 52);
            this.m_button2.TabIndex = 47;
            this.m_button2.TabStop = false;
            this.m_button2.Text = "2";
            this.m_button2.UseMnemonic = false;
            this.m_button2.UseVisualStyleBackColor = false;
            this.m_button2.Click += new System.EventHandler(this.PressThisKey);
            // 
            // m_button3
            // 
            this.m_button3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(87)))), ((int)(((byte)(83)))));
            this.m_button3.FocusColor = System.Drawing.Color.Black;
            this.m_button3.Font = new System.Drawing.Font("Tahoma", 24F, System.Drawing.FontStyle.Bold);
            this.m_button3.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_button3.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_button3.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.m_button3.Location = new System.Drawing.Point(919, 566);
            this.m_button3.MinimumSize = new System.Drawing.Size(30, 30);
            this.m_button3.Name = "m_button3";
            this.m_button3.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_button3.ShowFocus = false;
            this.m_button3.Size = new System.Drawing.Size(87, 52);
            this.m_button3.TabIndex = 48;
            this.m_button3.TabStop = false;
            this.m_button3.Text = "3";
            this.m_button3.UseMnemonic = false;
            this.m_button3.UseVisualStyleBackColor = false;
            this.m_button3.Click += new System.EventHandler(this.PressThisKey);
            // 
            // m_button6
            // 
            this.m_button6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(87)))), ((int)(((byte)(83)))));
            this.m_button6.FocusColor = System.Drawing.Color.Black;
            this.m_button6.Font = new System.Drawing.Font("Tahoma", 24F, System.Drawing.FontStyle.Bold);
            this.m_button6.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_button6.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_button6.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.m_button6.Location = new System.Drawing.Point(919, 510);
            this.m_button6.MinimumSize = new System.Drawing.Size(30, 30);
            this.m_button6.Name = "m_button6";
            this.m_button6.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_button6.ShowFocus = false;
            this.m_button6.Size = new System.Drawing.Size(87, 52);
            this.m_button6.TabIndex = 51;
            this.m_button6.TabStop = false;
            this.m_button6.Text = "6";
            this.m_button6.UseMnemonic = false;
            this.m_button6.UseVisualStyleBackColor = false;
            this.m_button6.Click += new System.EventHandler(this.PressThisKey);
            // 
            // m_button5
            // 
            this.m_button5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(87)))), ((int)(((byte)(83)))));
            this.m_button5.FocusColor = System.Drawing.Color.Black;
            this.m_button5.Font = new System.Drawing.Font("Tahoma", 24F, System.Drawing.FontStyle.Bold);
            this.m_button5.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_button5.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_button5.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.m_button5.Location = new System.Drawing.Point(827, 510);
            this.m_button5.MinimumSize = new System.Drawing.Size(30, 30);
            this.m_button5.Name = "m_button5";
            this.m_button5.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_button5.ShowFocus = false;
            this.m_button5.Size = new System.Drawing.Size(87, 52);
            this.m_button5.TabIndex = 50;
            this.m_button5.TabStop = false;
            this.m_button5.Text = "5";
            this.m_button5.UseMnemonic = false;
            this.m_button5.UseVisualStyleBackColor = false;
            this.m_button5.Click += new System.EventHandler(this.PressThisKey);
            // 
            // m_button4
            // 
            this.m_button4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(87)))), ((int)(((byte)(83)))));
            this.m_button4.FocusColor = System.Drawing.Color.Black;
            this.m_button4.Font = new System.Drawing.Font("Tahoma", 24F, System.Drawing.FontStyle.Bold);
            this.m_button4.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_button4.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_button4.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.m_button4.Location = new System.Drawing.Point(735, 510);
            this.m_button4.MinimumSize = new System.Drawing.Size(30, 30);
            this.m_button4.Name = "m_button4";
            this.m_button4.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_button4.ShowFocus = false;
            this.m_button4.Size = new System.Drawing.Size(87, 52);
            this.m_button4.TabIndex = 49;
            this.m_button4.TabStop = false;
            this.m_button4.Text = "4";
            this.m_button4.UseMnemonic = false;
            this.m_button4.UseVisualStyleBackColor = false;
            this.m_button4.Click += new System.EventHandler(this.PressThisKey);
            // 
            // m_button7
            // 
            this.m_button7.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(87)))), ((int)(((byte)(83)))));
            this.m_button7.FocusColor = System.Drawing.Color.Black;
            this.m_button7.Font = new System.Drawing.Font("Tahoma", 24F, System.Drawing.FontStyle.Bold);
            this.m_button7.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_button7.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_button7.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.m_button7.Location = new System.Drawing.Point(735, 454);
            this.m_button7.MinimumSize = new System.Drawing.Size(30, 30);
            this.m_button7.Name = "m_button7";
            this.m_button7.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_button7.ShowFocus = false;
            this.m_button7.Size = new System.Drawing.Size(87, 52);
            this.m_button7.TabIndex = 52;
            this.m_button7.TabStop = false;
            this.m_button7.Text = "7";
            this.m_button7.UseMnemonic = false;
            this.m_button7.UseVisualStyleBackColor = false;
            this.m_button7.Click += new System.EventHandler(this.PressThisKey);
            // 
            // m_button8
            // 
            this.m_button8.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(87)))), ((int)(((byte)(83)))));
            this.m_button8.FocusColor = System.Drawing.Color.Black;
            this.m_button8.Font = new System.Drawing.Font("Tahoma", 24F, System.Drawing.FontStyle.Bold);
            this.m_button8.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_button8.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_button8.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.m_button8.Location = new System.Drawing.Point(827, 454);
            this.m_button8.MinimumSize = new System.Drawing.Size(30, 30);
            this.m_button8.Name = "m_button8";
            this.m_button8.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_button8.ShowFocus = false;
            this.m_button8.Size = new System.Drawing.Size(87, 52);
            this.m_button8.TabIndex = 53;
            this.m_button8.TabStop = false;
            this.m_button8.Text = "8";
            this.m_button8.UseMnemonic = false;
            this.m_button8.UseVisualStyleBackColor = false;
            this.m_button8.Click += new System.EventHandler(this.PressThisKey);
            // 
            // m_button9
            // 
            this.m_button9.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(87)))), ((int)(((byte)(83)))));
            this.m_button9.FocusColor = System.Drawing.Color.Black;
            this.m_button9.Font = new System.Drawing.Font("Tahoma", 24F, System.Drawing.FontStyle.Bold);
            this.m_button9.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_button9.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_button9.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.m_button9.Location = new System.Drawing.Point(919, 454);
            this.m_button9.MinimumSize = new System.Drawing.Size(30, 30);
            this.m_button9.Name = "m_button9";
            this.m_button9.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_button9.ShowFocus = false;
            this.m_button9.Size = new System.Drawing.Size(87, 52);
            this.m_button9.TabIndex = 54;
            this.m_button9.TabStop = false;
            this.m_button9.Text = "9";
            this.m_button9.UseMnemonic = false;
            this.m_button9.UseVisualStyleBackColor = false;
            this.m_button9.Click += new System.EventHandler(this.PressThisKey);
            // 
            // m_button0
            // 
            this.m_button0.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(87)))), ((int)(((byte)(83)))));
            this.m_button0.FocusColor = System.Drawing.Color.Black;
            this.m_button0.Font = new System.Drawing.Font("Tahoma", 24F, System.Drawing.FontStyle.Bold);
            this.m_button0.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_button0.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_button0.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.m_button0.Location = new System.Drawing.Point(827, 622);
            this.m_button0.MinimumSize = new System.Drawing.Size(30, 30);
            this.m_button0.Name = "m_button0";
            this.m_button0.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_button0.ShowFocus = false;
            this.m_button0.Size = new System.Drawing.Size(87, 52);
            this.m_button0.TabIndex = 45;
            this.m_button0.TabStop = false;
            this.m_button0.Text = "0";
            this.m_button0.UseMnemonic = false;
            this.m_button0.UseVisualStyleBackColor = false;
            this.m_button0.Click += new System.EventHandler(this.PressThisKey);
            // 
            // m_button1
            // 
            this.m_button1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(87)))), ((int)(((byte)(83)))));
            this.m_button1.FocusColor = System.Drawing.Color.Black;
            this.m_button1.Font = new System.Drawing.Font("Tahoma", 24F, System.Drawing.FontStyle.Bold);
            this.m_button1.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_button1.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_button1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.m_button1.Location = new System.Drawing.Point(735, 566);
            this.m_button1.MinimumSize = new System.Drawing.Size(30, 30);
            this.m_button1.Name = "m_button1";
            this.m_button1.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_button1.ShowFocus = false;
            this.m_button1.Size = new System.Drawing.Size(87, 52);
            this.m_button1.TabIndex = 46;
            this.m_button1.TabStop = false;
            this.m_button1.Text = "1";
            this.m_button1.UseMnemonic = false;
            this.m_button1.UseVisualStyleBackColor = false;
            this.m_button1.Click += new System.EventHandler(this.PressThisKey);
            // 
            // BtnClosePaper
            // 
            this.BtnClosePaper.BackColor = System.Drawing.Color.Transparent;
            this.BtnClosePaper.FocusColor = System.Drawing.Color.Black;
            this.BtnClosePaper.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.BtnClosePaper.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.BtnClosePaper.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.BtnClosePaper.Location = new System.Drawing.Point(359, 697);
            this.BtnClosePaper.MinimumSize = new System.Drawing.Size(30, 30);
            this.BtnClosePaper.Name = "BtnClosePaper";
            this.BtnClosePaper.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.BtnClosePaper.ShowFocus = false;
            this.BtnClosePaper.Size = new System.Drawing.Size(150, 50);
            this.BtnClosePaper.TabIndex = 41;
            this.BtnClosePaper.TabStop = false;
            this.BtnClosePaper.Text = "Close Paper";
            this.BtnClosePaper.UseMnemonic = false;
            this.BtnClosePaper.UseVisualStyleBackColor = false;
            this.BtnClosePaper.Click += new System.EventHandler(this.BtnClosePaperClick);
            // 
            // PaperUsageFlowPanel
            // 
            this.PaperUsageFlowPanel.AutoScroll = true;
            this.PaperUsageFlowPanel.BackColor = System.Drawing.Color.Transparent;
            this.PaperUsageFlowPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.PaperUsageFlowPanel.Location = new System.Drawing.Point(14, 87);
            this.PaperUsageFlowPanel.Margin = new System.Windows.Forms.Padding(3, 1, 0, 3);
            this.PaperUsageFlowPanel.Name = "PaperUsageFlowPanel";
            this.PaperUsageFlowPanel.Size = new System.Drawing.Size(702, 558);
            this.PaperUsageFlowPanel.TabIndex = 0;
            this.PaperUsageFlowPanel.WrapContents = false;
            // 
            // BtnSave
            // 
            this.BtnSave.BackColor = System.Drawing.Color.Transparent;
            this.BtnSave.FocusColor = System.Drawing.Color.Black;
            this.BtnSave.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.BtnSave.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.BtnSave.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.BtnSave.Location = new System.Drawing.Point(671, 697);
            this.BtnSave.MinimumSize = new System.Drawing.Size(30, 30);
            this.BtnSave.Name = "BtnSave";
            this.BtnSave.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.BtnSave.ShowFocus = false;
            this.BtnSave.Size = new System.Drawing.Size(150, 50);
            this.BtnSave.TabIndex = 15;
            this.BtnSave.TabStop = false;
            this.BtnSave.Text = "Save";
            this.BtnSave.UseMnemonic = false;
            this.BtnSave.UseVisualStyleBackColor = false;
            this.BtnSave.Click += new System.EventHandler(this.BtnSaveClick);
            // 
            // BtnExit
            // 
            this.BtnExit.BackColor = System.Drawing.Color.Transparent;
            this.BtnExit.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.BtnExit.FocusColor = System.Drawing.Color.Black;
            this.BtnExit.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.BtnExit.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.BtnExit.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.BtnExit.Location = new System.Drawing.Point(828, 697);
            this.BtnExit.MinimumSize = new System.Drawing.Size(30, 30);
            this.BtnExit.Name = "BtnExit";
            this.BtnExit.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.BtnExit.ShowFocus = false;
            this.BtnExit.Size = new System.Drawing.Size(150, 50);
            this.BtnExit.TabIndex = 14;
            this.BtnExit.TabStop = false;
            this.BtnExit.Text = "Cancel";
            this.BtnExit.UseMnemonic = false;
            this.BtnExit.UseVisualStyleBackColor = false;
            this.BtnExit.Click += new System.EventHandler(this.BtnExitClick);
            // 
            // PaperUsageForm
            // 
            this.BackgroundImage = global::GTI.Modules.POS.Properties.Resources.PaperExchangeBackground;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.ClientSize = new System.Drawing.Size(1024, 768);
            this.ControlBox = false;
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.BtnAdd);
            this.Controls.Add(this.BtnRemove);
            this.Controls.Add(this.BtnConfirmAndPrint);
            this.Controls.Add(this.BtnPrintUnscannedPacks);
            this.Controls.Add(this.BtnPrint);
            this.Controls.Add(this.m_backButton);
            this.Controls.Add(this.m_decimalButton);
            this.Controls.Add(this.m_button2);
            this.Controls.Add(this.m_button3);
            this.Controls.Add(this.m_button6);
            this.Controls.Add(this.m_button5);
            this.Controls.Add(this.m_button4);
            this.Controls.Add(this.m_button7);
            this.Controls.Add(this.m_button8);
            this.Controls.Add(this.m_button9);
            this.Controls.Add(this.m_button0);
            this.Controls.Add(this.m_button1);
            this.Controls.Add(this.TotalLabel);
            this.Controls.Add(this.BtnClosePaper);
            this.Controls.Add(this.StaffLabel);
            this.Controls.Add(this.SessionLabel);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label15);
            this.Controls.Add(this.UnscannedPacksListBox);
            this.Controls.Add(this.PaperUsageFlowPanel);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.label17);
            this.Controls.Add(this.label16);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.BtnSave);
            this.Controls.Add(this.BtnExit);
            this.Controls.Add(this.TitleLabel);
            this.DoubleBuffered = true;
            this.DrawBorderOuterEdge = true;
            this.DrawRounded = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.KeyPreview = true;
            this.Name = "PaperUsageForm";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Load += new System.EventHandler(this.PaperUsageForm_Load);
            this.flowLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.errIcon)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Controls.ImageButton BtnSave;
        private Controls.ImageButton BtnExit;
        private System.Windows.Forms.Label TitleLabel;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.ListBox UnscannedPacksListBox;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label StatusLabel;
        private System.Windows.Forms.Label SessionLabel;
        private System.Windows.Forms.Label StaffLabel;
        private Controls.ImageButton BtnClosePaper;
        private System.Windows.Forms.Label TotalLabel;
        private Controls.ImageButton m_backButton;
        private Controls.ImageButton m_decimalButton;
        private Controls.ImageButton m_button2;
        private Controls.ImageButton m_button3;
        private Controls.ImageButton m_button6;
        private Controls.ImageButton m_button5;
        private Controls.ImageButton m_button4;
        private Controls.ImageButton m_button7;
        private Controls.ImageButton m_button8;
        private Controls.ImageButton m_button9;
        private Controls.ImageButton m_button0;
        private Controls.ImageButton m_button1;
        private Controls.ImageButton BtnPrint;
        private Controls.ImageButton BtnPrintUnscannedPacks;
        private Controls.ImageButton BtnConfirmAndPrint;
        private Controls.ImageButton BtnRemove;
        private Controls.ImageButton BtnAdd;
        private System.Windows.Forms.PictureBox errIcon;
        internal DoubleBufferFlowLayoutPanel PaperUsageFlowPanel;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;

    }
}