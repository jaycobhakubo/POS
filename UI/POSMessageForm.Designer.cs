namespace GTI.Modules.POS.UI
{
    partial class POSMessageForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(POSMessageForm));
            this.m_pauseTimer = new System.Windows.Forms.Timer(this.components);
            this.m_outerPanel = new System.Windows.Forms.TableLayoutPanel();
            this.m_messageLabel = new System.Windows.Forms.Label();
            this.m_pauseProgress = new System.Windows.Forms.ProgressBar();
            this.m_outerPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_pauseTimer
            // 
            this.m_pauseTimer.Tick += new System.EventHandler(this.PauseTimerTick);
            // 
            // m_outerPanel
            // 
            resources.ApplyResources(this.m_outerPanel, "m_outerPanel");
            this.m_outerPanel.BackColor = System.Drawing.Color.Transparent;
            this.m_outerPanel.Controls.Add(this.m_messageLabel, 0, 0);
            this.m_outerPanel.Controls.Add(this.m_pauseProgress, 0, 1);
            this.m_outerPanel.Name = "m_outerPanel";
            this.m_outerPanel.Click += new System.EventHandler(this.SomethingWasClicked);
            // 
            // m_messageLabel
            // 
            resources.ApplyResources(this.m_messageLabel, "m_messageLabel");
            this.m_messageLabel.BackColor = System.Drawing.Color.Transparent;
            this.m_messageLabel.Name = "m_messageLabel";
            this.m_messageLabel.Click += new System.EventHandler(this.SomethingWasClicked);
            // 
            // m_pauseProgress
            // 
            resources.ApplyResources(this.m_pauseProgress, "m_pauseProgress");
            this.m_pauseProgress.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(186)))), ((int)(((byte)(192)))));
            this.m_pauseProgress.ForeColor = System.Drawing.Color.Gold;
            this.m_pauseProgress.Name = "m_pauseProgress";
            this.m_pauseProgress.Click += new System.EventHandler(this.SomethingWasClicked);
            // 
            // POSMessageForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            resources.ApplyResources(this, "$this");
            this.ControlBox = false;
            this.Controls.Add(this.m_outerPanel);
            this.DrawAsGradient = true;
            this.DrawBorderOuterEdge = true;
            this.DrawRounded = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "POSMessageForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Click += new System.EventHandler(this.SomethingWasClicked);
            this.m_outerPanel.ResumeLayout(false);
            this.m_outerPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label m_messageLabel;
        private System.Windows.Forms.TableLayoutPanel m_outerPanel;
        private System.Windows.Forms.Timer m_pauseTimer;
        private System.Windows.Forms.ProgressBar m_pauseProgress;
    }
}