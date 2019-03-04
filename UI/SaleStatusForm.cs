#region Copyright
// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2008 GameTech
// International, Inc.
#endregion

// PDTS 583
// Rally TA7464

using System;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;
using System.Globalization;
using GTI.Modules.Shared;
using GTI.Modules.POS.Business;
using GTI.Modules.POS.Properties;

namespace GTI.Modules.POS.UI
{
    /// <summary>
    /// A form that displays progress for the sale proccess.
    /// </summary>
    internal partial class SaleStatusForm : POSForm
    {
        #region Constants and Data Types
        protected readonly Size StartFormSize = new Size(464, 200); //starts short and grows in RearrangeForm()
        protected const int StartYPos = 139;
        protected const int ControlPadding = 5;
        protected const int FormPadding = 10;
        protected readonly Color ChangeDueColor = Color.Lime;
        protected readonly Color ChangeDueEdgeColor = Color.DarkGreen;
        protected readonly Color AmountDueColor = Color.Yellow;
        protected readonly Color AmountDueEdgeColor = Color.DimGray;
        protected readonly Color AmountToPayColor = Color.Maroon;
        protected readonly Color AmountToPayEdgeColor = Color.LightGray;

        /// <summary>
        /// Represents the state of the sale shown on the SaleStatusForm.
        /// </summary>
        public struct SaleStatusState
        {
            #region Member Variables
            public short UnitNumber;
            public string Message;
            #endregion

            #region Constructors
            /// <summary>
            /// Initializes the SaleStatusState struct.
            /// </summary>
            /// <param name="unitNum">The unit number sold to.</param>
            /// <param name="message">The message to display.</param>
            public SaleStatusState(short unitNum, string message)
            {
                UnitNumber = unitNum;
                Message = message;
            }
            #endregion
        }
        #endregion

        #region Member Variables
        protected bool m_allowClose;
        protected Sale m_sale;
        protected bool m_showChangeDue; 
        protected bool m_unitNumVisible;
        protected short m_unitNum;
        protected bool m_okButtonVisible;
        protected bool m_cancelButtonVisible; // FIX: DE2538
        protected bool m_secondaryThreadComplete = true; 
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the SaleStatusForm class.
        /// </summary>
        /// <param name="parent">The PointOfSale to which this form 
        /// belongs.</param>
        /// <param name="displayMode">The display mode used to show this 
        /// form.</param>
        /// <exception cref="System.ArgumentNullException">parent or 
        /// displayMode is a null reference.</exception>
        public SaleStatusForm(PointOfSale parent, DisplayMode displayMode)
            : base(parent, displayMode)
        {
            InitializeComponent();
            ApplyDisplayMode();
            RearrangeForm();
        }
        #endregion

        #region Member Methods
        /// <summary>
        /// Call this method to close the form programmatically so it won't be 
        /// prevented by the FormClosing event.
        /// </summary>
        public void CloseForm()
        {
            m_allowClose = true;
            Close();
        }

        /// <summary>
        /// Handles the FormClosing event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An FormClosingEventArgs object that 
        /// contains the event data.</param>
        private void FormClose(object sender, FormClosingEventArgs e)
        {
            // Don't allow the closing of this form.
            if(e.CloseReason == CloseReason.UserClosing)
                e.Cancel = !m_allowClose;

            // Reset the allow close & clear the sale.
            if(!e.Cancel)
            {
                Sale = null;
                m_allowClose = false;
            }
        }

        /// <summary>
        /// Will visually update the form to signify progress.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The ProgressChangedEventArgs object that 
        /// contains the event data.</param>
        public void ReportProgress(object sender, ProgressChangedEventArgs e)
        {
            if(e.UserState != null)
            {
                if(e.UserState is string)
                    Message = (string)e.UserState;
                else if(e.UserState is SaleStatusState)
                {
                    // Update the unit num and message.
                    SaleStatusState state = (SaleStatusState)e.UserState;
                    UnitNumber = state.UnitNumber;
                    Message = state.Message;
                }
            }
        }

        /// <summary>
        /// Shows a MessageForm with the specified message.
        /// </summary>
        /// <param name="message">The message to show.</param>
        /// <param name="yesNo">Whether the MessageForm should be a YesNo type 
        /// (otherwise it is an Ok type).</param>
        /// <remarks>If No is the result of the MessageForm, then a 
        /// POSUserCancelException is thrown.</remarks>
        /// <exception cref="GTI.Modules.POS.Business.POSUserCancelException">
        /// The user selected no on the message form.</exception>
        public void ShowPrompt(string message, bool yesNo)
        {
            m_parent.SellingForm.NotIdle(true);

            POSMessageFormTypes type = yesNo ? POSMessageFormTypes.YesNo_DefNO : POSMessageFormTypes.OK;

            if (POSMessageForm.Show(this, m_parent, message, type) == DialogResult.No)
            {
                m_parent.SellingForm.NotIdle();
                throw new POSUserCancelException();
            }

            m_parent.SellingForm.NotIdle();
        }

        /// <summary>
        /// Will place the form's controls based on what is visible.
        /// </summary>
        protected void RearrangeForm()
        {
            SuspendLayout();

            // Start with the default form size.
            Size size = StartFormSize;
            int yPos = StartYPos;

            // Should we show the change due?
            if(!m_showChangeDue)
            {
                m_changeDueLabel.Visible = false;
            }
            else
            {
                m_changeDueLabel.Visible = true;
                yPos += m_changeDueLabel.Size.Height;
            }

            // Position the unit number, if needed.
            if(m_unitNumVisible)
            {
                m_unitNumLabel.Location = new Point(m_unitNumLabel.Location.X, yPos);
                yPos += m_unitNumLabel.Size.Height + ControlPadding;
            }

            // Position the message label.
            m_messageLabel.Location = new Point(m_messageLabel.Location.X, yPos);
            yPos += m_messageLabel.Size.Height + ControlPadding;

            // FIX: DE2538
            // Position the buttons, if needed.
            if(m_okButtonVisible || m_cancelButtonVisible)
            {
                m_okButton.Location = new Point(m_okButton.Location.X, yPos);
                m_cancelButton.Location = new Point(m_cancelButton.Location.X, yPos);
                yPos += m_okButton.Size.Height;
            }
            // END: DE2538

            if(size.Height < yPos + FormPadding)
                size.Height = yPos + FormPadding;

            Size = size;

            ResumeLayout();

            Application.DoEvents();
        }

        /// <summary>
        /// Updates the change due displayed based on the sale.
        /// </summary>
        public void UpdateChangeDue()
        {
            m_changeDueLabel.Text = null;

            if(m_sale != null)
            {
                if (m_sale.AmountTendered < 0M) //paying cash out to customer
                {
                    m_changeDueLabel.Text = string.Format(CultureInfo.CurrentCulture, Resources.CashToCustomer, m_parent.DefaultCurrency.FormatCurrencyString(-m_sale.AmountTendered));
                    m_changeDueLabel.ForeColor = AmountToPayColor;
                    m_changeDueLabel.EdgeColor = AmountToPayEdgeColor;
                }
                else
                {
                    decimal change = m_sale.CalculateChange();

                    if (change > 0M)
                    {
                        m_changeDueLabel.Text = string.Format(CultureInfo.CurrentCulture, Resources.ChangeDue, m_parent.DefaultCurrency.FormatCurrencyString(change));
                        m_changeDueLabel.ForeColor = ChangeDueColor;
                        m_changeDueLabel.EdgeColor = ChangeDueEdgeColor;
                    }
                    else
                    {
                        m_changeDueLabel.Text = string.Format(CultureInfo.CurrentCulture, Resources.AmountDue, m_sale.SaleCurrency.FormatCurrencyString(decimal.Negate(m_sale.SaleCurrency.ConvertFromDefaultCurrencyToThisCurrency(change))));
                        m_changeDueLabel.ForeColor = AmountDueColor;
                        m_changeDueLabel.EdgeColor = AmountDueEdgeColor;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the ok button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The EventArgs object that contains 
        /// the event data.</param>
        private void OkClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            m_allowClose = true;
            Close();
        }

        /// <summary>
        /// Handles the cancel button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The EventArgs object that contains 
        /// the event data.</param>
        private void CancelClick(object sender, EventArgs e)
        {
            try
            {
                CancelButtonEnabled = false;

                if(m_parent.Worker != null && m_parent.Worker.IsBusy && m_parent.Worker.WorkerSupportsCancellation)
                    m_parent.Worker.CancelAsync();
            }
            catch
            {
            }
        }
        #endregion

        #region Member Properties
        /// <summary>
        /// Gets or sets the visible state of the waiting image.
        /// </summary>
        public bool ShowWaitingPic
        {
            get
            {
                return m_waitPicture.Visible;
            }

            set
            {
                m_waitPicture.Visible = value;
            }
        }

        /// <summary>
        /// Gets or sets the waiting image.
        /// </summary>
        public Image WaitingPic
        {
            set
            {
                m_waitPicture.Image = value;
            }

            get
            {
                return m_waitPicture.Image;
            }
        }

        /// <summary>
        /// Gets or sets the visible state of the message.
        /// </summary>
        public bool ShowMessage
        {
            get
            {
                return m_messageLabel.Visible;
            }

            set
            {
                m_messageLabel.Visible = value;
            }
        }

        /// <summary>
        /// Gets or sets the sale the form needs to display information about.
        /// </summary>
        public Sale Sale
        {
            get
            {
                return m_sale;
            }
            set
            {
                // Reset the form back to the default.
                m_sale = null;

                UnitNumberVisible = false;
                UnitNumber = 0;
                OkButtonEnabled = false;
                OkButtonVisible = false;
                CancelButtonEnabled = true;
                CancelButtonVisible = false;
                ShowChangeDue = false;

                m_sale = value;

                if(m_sale != null)
                {
                    // Show change due only if they are tendered something.
                    if(m_sale.AmountTendered != 0M && (m_sale.CalculateChange() != 0M || m_sale.AmountTendered < 0M))
                    {
                        ShowChangeDue = m_parent.WeAreNotAPOSKiosk;
                        OkButtonVisible = m_parent.WeAreNotAPOSKiosk;
                        UpdateChangeDue();
                    }

                    // Are we selling to something with a unit number?
                    if(m_sale.Device.Id == Device.Traveler.Id ||
                       m_sale.Device.Id == Device.Tracker.Id)
                    {
                        UnitNumberVisible = true;
                        OkButtonVisible = true;
                    }

                    // Are we performing a quantity sale?
                    if(m_sale.Quantity > 1)
                        CancelButtonVisible = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the text to display on the form.
        /// </summary>
        public string Message
        {
            get
            {
                return m_messageLabel.Text;
            }
            set
            {
                m_messageLabel.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets whether change due is displayed.
        /// </summary>
        public bool ShowChangeDue
        {
            get
            {
                return m_showChangeDue;
            }
            set
            {
                if (m_showChangeDue != value)
                {
                    m_showChangeDue = value;
                    RearrangeForm();
                }
            }
        }

        /// <summary>
        /// Gets or sets the unit number shown on the form.
        /// </summary>
        public short UnitNumber
        {
            get
            {
                return m_unitNum;
            }
            set
            {
                m_unitNum = value;

                if(m_unitNum != 0)
                    m_unitNumLabel.Text = string.Format(CultureInfo.CurrentCulture, Resources.UnitNumber, m_unitNum);
                else
                    m_unitNumLabel.Text = string.Format(CultureInfo.CurrentCulture, Resources.UnitNumber, string.Empty);
            }
        }

        /// <summary>
        /// Gets or sets whether the unit number is visible on the form.
        /// </summary>
        public bool UnitNumberVisible
        {
            get
            {
                return m_unitNumLabel.Visible;
            }
            set
            {
                m_unitNumLabel.Visible = m_unitNumVisible = value;
                RearrangeForm();
            }
        }

        /// <summary>
        /// Gets or sets whether the ok button is visible.
        /// </summary>
        public bool OkButtonVisible
        {
            get
            {
                return m_okButton.Visible;
            }
            set
            {
                m_okButton.Visible = m_okButtonVisible = value;
                RearrangeForm();
            }
        }

        /// <summary>
        /// Gets or sets whether the ok button is enabled.
        /// </summary>
        public bool OkButtonEnabled
        {
            get
            {
                return m_okButton.Enabled;
            }
            set
            {
                m_okButton.Enabled = value;
            }
        }

        // FIX: DE2538
        /// <summary>
        /// Gets or sets whether the cancel button is visible.
        /// </summary>
        public bool CancelButtonVisible
        {
            get
            {
                return m_cancelButton.Visible;
            }
            set
            {
                m_cancelButton.Visible = m_cancelButtonVisible = value;
                RearrangeForm();
            }
        }

        /// <summary>
        /// Gets or sets whether the cancel button is enabled.
        /// </summary>
        public bool CancelButtonEnabled
        {
            get
            {
                return m_cancelButton.Enabled;
            }
            set
            {
                m_cancelButton.Enabled = value;
            }
        }
        // END: DE2538

        public bool SecondaryThreadComplete
        {
            get
            {
                return m_secondaryThreadComplete;
            }

            set
            {
                m_secondaryThreadComplete = value;
            }
        }

        #endregion
    }

    /// <summary>
    /// A delegate that allows cross-thread calls to ShowPrompt on the 
    /// SaleStatusForm class.
    /// </summary>
    /// <param name="message">The message to show.</param>
    /// <param name="yesNo">Whether the MessageForm should be a YesNo type 
    /// (otherwise it is an Ok type).</param>
    internal delegate void SalePromptDelegate(string message, bool yesNo);
}
