namespace GTI.Modules.POS.UI
{
    partial class CurrencyForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CurrencyForm));
            this.m_messageLabel = new System.Windows.Forms.Label();
            this.m_currencyPanel = new GTI.Modules.POS.UI.NoScrollBarsPanel();
            this.m_upButton = new GTI.Controls.ImageButton();
            this.m_downButton = new GTI.Controls.ImageButton();
            this.m_panelMessage = new GTI.Controls.EliteGradientPanel();
            this.m_panelMessage.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_messageLabel
            // 
            this.m_messageLabel.BackColor = System.Drawing.Color.Transparent;
            this.m_messageLabel.ForeColor = System.Drawing.Color.White;
            resources.ApplyResources(this.m_messageLabel, "m_messageLabel");
            this.m_messageLabel.Name = "m_messageLabel";
            // 
            // m_currencyPanel
            // 
            resources.ApplyResources(this.m_currencyPanel, "m_currencyPanel");
            this.m_currencyPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(79)))), ((int)(((byte)(122)))), ((int)(((byte)(133)))));
            this.m_currencyPanel.BorderColor = System.Drawing.Color.Silver;
            this.m_currencyPanel.DrawBorderOuterEdge = true;
            this.m_currencyPanel.DrawRounded = true;
            this.m_currencyPanel.GradientBeginColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(179)))), ((int)(((byte)(213)))));
            this.m_currencyPanel.GradientEndColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(186)))), ((int)(((byte)(192)))));
            this.m_currencyPanel.InnerBorderEdgeColor = System.Drawing.Color.SlateGray;
            this.m_currencyPanel.Name = "m_currencyPanel";
            this.m_currencyPanel.OuterBorderEdgeColor = System.Drawing.Color.WhiteSmoke;
            this.m_currencyPanel.OuterRadius = 10;
            // 
            // m_upButton
            // 
            this.m_upButton.BackColor = System.Drawing.Color.Transparent;
            this.m_upButton.FocusColor = System.Drawing.Color.Black;
            this.m_upButton.ImageIcon = global::GTI.Modules.POS.Properties.Resources.ArrowUp;
            this.m_upButton.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_upButton.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            resources.ApplyResources(this.m_upButton, "m_upButton");
            this.m_upButton.Name = "m_upButton";
            this.m_upButton.RepeatRate = 150;
            this.m_upButton.RepeatWhenHeldFor = 750;
            this.m_upButton.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_upButton.ShowFocus = false;
            this.m_upButton.TabStop = false;
            this.m_upButton.UseMnemonic = false;
            this.m_upButton.UseVisualStyleBackColor = false;
            this.m_upButton.Click += new System.EventHandler(this.UpButtonClick);
            // 
            // m_downButton
            // 
            this.m_downButton.BackColor = System.Drawing.Color.Transparent;
            this.m_downButton.FocusColor = System.Drawing.Color.Black;
            this.m_downButton.ImageIcon = global::GTI.Modules.POS.Properties.Resources.ArrowDown;
            this.m_downButton.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_downButton.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            resources.ApplyResources(this.m_downButton, "m_downButton");
            this.m_downButton.Name = "m_downButton";
            this.m_downButton.RepeatRate = 150;
            this.m_downButton.RepeatWhenHeldFor = 750;
            this.m_downButton.SecondaryTextPadding = new System.Windows.Forms.Padding(5);
            this.m_downButton.ShowFocus = false;
            this.m_downButton.TabStop = false;
            this.m_downButton.UseMnemonic = false;
            this.m_downButton.UseVisualStyleBackColor = false;
            this.m_downButton.Click += new System.EventHandler(this.DownButtonClick);
            // 
            // m_panelMessage
            // 
            this.m_panelMessage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(79)))), ((int)(((byte)(122)))), ((int)(((byte)(133)))));
            this.m_panelMessage.BorderColor = System.Drawing.Color.Silver;
            this.m_panelMessage.Controls.Add(this.m_messageLabel);
            this.m_panelMessage.DrawBorderOuterEdge = true;
            this.m_panelMessage.DrawRounded = true;
            this.m_panelMessage.GradientBeginColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(179)))), ((int)(((byte)(213)))));
            this.m_panelMessage.GradientEndColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(186)))), ((int)(((byte)(192)))));
            this.m_panelMessage.InnerBorderEdgeColor = System.Drawing.Color.SlateGray;
            resources.ApplyResources(this.m_panelMessage, "m_panelMessage");
            this.m_panelMessage.Name = "m_panelMessage";
            this.m_panelMessage.OuterBorderEdgeColor = System.Drawing.Color.WhiteSmoke;
            this.m_panelMessage.OuterRadius = 10;
            // 
            // CurrencyForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            resources.ApplyResources(this, "$this");
            this.ControlBox = false;
            this.Controls.Add(this.m_panelMessage);
            this.Controls.Add(this.m_downButton);
            this.Controls.Add(this.m_upButton);
            this.Controls.Add(this.m_currencyPanel);
            this.DoubleBuffered = true;
            this.DrawAsGradient = true;
            this.DrawBorderOuterEdge = true;
            this.DrawRounded = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "CurrencyForm";
            this.ShowInTaskbar = false;
            this.m_panelMessage.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label m_messageLabel;
        private GTI.Modules.POS.UI.NoScrollBarsPanel m_currencyPanel;
        private GTI.Controls.ImageButton m_upButton;
        private GTI.Controls.ImageButton m_downButton;
        private Controls.EliteGradientPanel m_panelMessage;
    }
}