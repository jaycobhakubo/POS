using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using GTI.Modules.Shared;

namespace GTI.Modules.POS.UI
{
	internal partial class POSNumericInputForm : POSForm
	{
        private DateTime m_idleSince = DateTime.Now;
        private int m_maxIdleTime = 0;

        public POSNumericInputForm(Business.PointOfSale parent):base(parent, parent.Settings.DisplayMode.BasicCopy())
        {
            InitializeComponent();

            if(parent.WeAreAPOSKiosk)
            {
                btnOK.ImageNormal = POS.Properties.Resources.GreenButtonUp;
                btnOK.ImagePressed = POS.Properties.Resources.GreenButtonDown;
                btnCancel.ImageNormal = POS.Properties.Resources.RedButtonUp;
                btnCancel.ImagePressed = POS.Properties.Resources.RedButtonDown;
            }

            eliteKeypad1.TextResultHideSelection = false;
            eliteKeypad1.Select();
        }

        public POSNumericInputForm(Business.PointOfSale parent, int maxDigits):base(parent, parent.Settings.DisplayMode.BasicCopy())
        {
            InitializeComponent();

            if (parent.WeAreAPOSKiosk)
            {
                btnOK.ImageNormal = POS.Properties.Resources.GreenButtonUp;
                btnOK.ImagePressed = POS.Properties.Resources.GreenButtonDown;
                btnCancel.ImageNormal = POS.Properties.Resources.RedButtonUp;
                btnCancel.ImagePressed = POS.Properties.Resources.RedButtonDown;
            }
            
            eliteKeypad1.MaxCharacters = maxDigits;
            eliteKeypad1.TextResultHideSelection = false;
            eliteKeypad1.Select();
        }

        private void POSNumericInputForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            NotIdle();

            if (!"0123456789.-".Contains(e.KeyChar.ToString()))
                e.Handled = true;

            if (e.KeyChar == '-') //treat the minus as a backspace
            {
                eliteKeypad1.DoBackspace();
                e.Handled = true;
            }

            if (!eliteKeypad1.UseDecimalKey && e.KeyChar == '.') //treat the decimal key as a clear
            {
                eliteKeypad1.TextResult = string.Empty;
                e.Handled = true;
            }
        }

        private void POSNumericInputForm_Shown(object sender, EventArgs e)
        {
            NotIdle();

            if (MaxIdleTime > 0)
                m_idleTimer.Start();
        }

        private void POSNumericInputForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            NotIdle();

            if (MaxIdleTime > 0)
                m_idleTimer.Stop();
        }

        private void eliteKeypad1_ValueChanged(object sender, EventArgs e)
        {
            NotIdle();
        }

        private void UserActivityDetected(object sender, EventArgs e)
        {
            NotIdle();
        }

        private void m_idleTimer_Tick(object sender, EventArgs e)
        {
            if (m_parent.GuardianHasUsSuspended) //don't do anything
            {
                NotIdle();
                return;
            }

            TimeSpan idleFor = DateTime.Now - m_idleSince;

            if (idleFor > TimeSpan.FromMilliseconds(MaxIdleTime * 1000 / 3))
            {
                if (!m_timeoutProgress.Visible)
                {
                    m_timeoutProgress.Visible = true;
                    btnCancel.Pulse = m_parent.Settings.KioskTimeoutPulseDefaultButton;
                }

                m_timeoutProgress.Increment(m_idleTimer.Interval);

                if (m_timeoutProgress.Value >= m_timeoutProgress.Maximum)
                {
                    NotIdle();
                    eliteKeypad1.ResetText();
                    btnCancel_Click(sender, e);
                }
            }
        }

        private void NotIdle()
        {
            m_idleSince = DateTime.Now;
            m_timeoutProgress.Hide();
            m_timeoutProgress.Value = 0;
            btnCancel.Pulse = false;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

		// Properties
        public string TextResult
        {
            get
            {
                return eliteKeypad1.TextResult;
            }

            set
            {
                eliteKeypad1.TextResult = value;
            }
        }

        public bool Password
        {
            get
            {
                return eliteKeypad1.Password;
            }

            set
            {
                eliteKeypad1.Password = value;
            }
        }

        public Decimal DecimalResult
        {
            get
            {
                Decimal d = 0;

                try
                {
                    d = Convert.ToDecimal(eliteKeypad1.TextResult);
                }
                catch (Exception)
                {
                }

                return d;
            }
        }

        public Font DescriptionFont
        {
            get
            {
                return groupBox1.Font;
            }

            set
            {
                groupBox1.Font = value;
                groupBox1.Invalidate();
            }
        }

		public string Description
		{
			set { groupBox1.Text = value; }
		}

		public bool UseDecimalKey
		{
			set { eliteKeypad1.UseDecimalKey = value; }
		}

        /// <summary>
        /// Gets or sets the number of seconds of non-activity allowed before the form 
        /// closes with a CANCEL.  If &lt;= 0 there is no time limit.
        /// </summary>
        public int MaxIdleTime
        {
            get
            {
                return m_maxIdleTime;
            }

            set
            {
                m_maxIdleTime = value;

                m_timeoutProgress.Maximum = (value / 3) * 2000;
            }
        }
	}
}