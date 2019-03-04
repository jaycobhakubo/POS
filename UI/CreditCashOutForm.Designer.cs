namespace GTI.Modules.POS.UI
{
    partial class CreditCashOutForm
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
            this.m_currencyButton = new GTI.Controls.ImageButton();
            this.SuspendLayout();
            // 
            // m_currencyButton
            // 
            this.m_currencyButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(79)))), ((int)(((byte)(122)))), ((int)(((byte)(133)))));
            this.m_currencyButton.FocusColor = System.Drawing.Color.Black;
            this.m_currencyButton.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_currencyButton.ImageNormal = global::GTI.Modules.POS.Properties.Resources.BlueButtonUp;
            this.m_currencyButton.ImagePressed = global::GTI.Modules.POS.Properties.Resources.BlueButtonDown;
            this.m_currencyButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.m_currencyButton.Location = new System.Drawing.Point(4, 99);
            this.m_currencyButton.MinimumSize = new System.Drawing.Size(30, 30);
            this.m_currencyButton.Name = "m_currencyButton";
            this.m_currencyButton.RepeatRate = 150;
            this.m_currencyButton.RepeatWhenHeldFor = 750;
            this.m_currencyButton.ShowFocus = false;
            this.m_currencyButton.Size = new System.Drawing.Size(50, 34);
            this.m_currencyButton.Stretch = false;
            this.m_currencyButton.TabIndex = 0;
            this.m_currencyButton.TabStop = false;
            this.m_currencyButton.UseMnemonic = false;
            this.m_currencyButton.UseVisualStyleBackColor = false;
            this.m_currencyButton.Click += new System.EventHandler(this.CurrencyClick);
            // 
            // CreditCashOutForm
            // 
            this.ClientSize = new System.Drawing.Size(300, 437);
            this.Controls.Add(this.m_currencyButton);
            this.DoubleBuffered = true;
            this.Name = "CreditCashOutForm";
            this.Text = "CreditCashOutForm";
            this.Controls.SetChildIndex(this.m_keypad, 0);
            this.Controls.SetChildIndex(this.m_currencyButton, 0);
            this.ResumeLayout(false);

        }

        #endregion

        private GTI.Controls.ImageButton m_currencyButton;
    }
}