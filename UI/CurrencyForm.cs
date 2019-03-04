#region Copyright
// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2010 GameTech
// International, Inc.
#endregion

// Rally TA7465

using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using GTI.Controls;
using GTI.Modules.POS.Business;
using GTI.Modules.POS.Properties;
using GTI.Modules.Shared;

namespace GTI.Modules.POS.UI
{
    /// <summary>
    /// Represents a that displays all the different currencies available.
    /// </summary>
    internal partial class CurrencyForm : POSForm
    {
        #region Constants and Data Types
        protected const int ScrollDelta = 56;

        protected const int ControlStartX = 3;
        protected const int ControlStartY = 3;
        protected const int MaxVisibleY = 283;

        protected const int ButtonYBuffer = 6;

        protected readonly Size ButtonSize = new Size(220, 50);
        protected readonly Image ButtonUpImage = Resources.BlueButtonUp;
        protected readonly Image ButtonDownImage = Resources.BlueButtonDown;
        protected readonly Color ButtonBackColor = Color.Transparent; //Color.FromArgb(79, 122, 133);
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the CurrencyForm class.
        /// </summary>
        /// <param name="parent">The PointOfSale to which this form 
        /// belongs.</param>
        /// <param name="displayMode">The display mode used to show this 
        /// form.</param>
        /// <param name="message">The message to display.</param>
        /// <exception cref="System.ArgumentNullException">parent or 
        /// displayMode is a null reference.</exception>
        public CurrencyForm(PointOfSale parent, DisplayMode displayMode, string message)
            : base(parent, displayMode)
        {
            InitializeComponent();
            ApplyDisplayMode();
            ArrangeForm();
            m_messageLabel.Text = message;
        }
        #endregion

        #region Member Methods
        /// <summary>
        /// Sets the settings of this form based on the current display mode.
        /// </summary>
        protected override void ApplyDisplayMode()
        {
            base.ApplyDisplayMode();

            // This is a special dialog, so override the default size.
            Size = new Size(303, 286);
        }

        /// <summary>
        /// Removes all controls from the currency panel.
        /// </summary>
        protected void ClearControls()
        {
            // Remove all the controls from the panel and dispose of them.
            foreach(Control control in m_currencyPanel.Controls)
            {
                if(control is Button)
                {
                    ((Button)control).Click -= CurrencyClick;
                }

                control.Dispose();
            }

            m_currencyPanel.Controls.Clear();
        }

        /// <summary>
        /// Arranges the controls on the form based on the defined currencies.
        /// </summary>
        protected void ArrangeForm()
        {
            ClearControls();

            int currY = ControlStartY;

            // The default currency is always at the top.
            ImageButton button = new ImageButton(ButtonUpImage, ButtonDownImage);
            button.AutoEllipsis = true;
            button.BackColor = ButtonBackColor;
            button.ShowFocus = false;
            button.Stretch = true;
            button.TabIndex = 0;
            button.TabStop = false;
            button.UseMnemonic = false;
            button.Size = ButtonSize;
            button.Location = new Point(ControlStartX, currY);
            button.Text = string.Format(CultureInfo.CurrentCulture, Resources.CurrencyName, m_parent.DefaultCurrency.Name, m_parent.DefaultCurrency.Symbol);
            button.Tag = m_parent.DefaultCurrency;
            button.Click += CurrencyClick;

            m_currencyPanel.Controls.Add(button);
            currY += button.Size.Height + ButtonYBuffer;

            foreach(Currency currency in m_parent.Currencies)
            {
                button = new ImageButton(ButtonUpImage, ButtonDownImage);
                button.AutoEllipsis = true;
                button.BackColor = ButtonBackColor;
                button.ShowFocus = false;
                button.Stretch = true;
                button.TabIndex = 0;
                button.TabStop = false;
                button.UseMnemonic = false;
                button.Size = ButtonSize;
                button.Location = new Point(ControlStartX, currY);
                button.Text = string.Format(CultureInfo.CurrentCulture, Resources.CurrencyName, currency.Name, currency.Symbol);
                button.Tag = currency;
                button.Click += CurrencyClick;

                m_currencyPanel.Controls.Add(button);
                currY += button.Size.Height + ButtonYBuffer;
            }
            
            if(currY >= MaxVisibleY)
            {
                m_upButton.Enabled = true;
                m_downButton.Enabled = true;
            }
            else
            {
                m_upButton.Enabled = false;
                m_downButton.Enabled = false;
            }
        }

        /// <summary>
        /// Handles the up button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the 
        /// event data.</param>
        private void UpButtonClick(object sender, EventArgs e)
        {
            int y = m_currencyPanel.AutoScrollPosition.Y;

            y = Math.Abs(y) - ScrollDelta;

            if(y < 0)
                m_currencyPanel.AutoScrollPosition = new Point(m_currencyPanel.AutoScrollPosition.X, 0);
            else
                m_currencyPanel.AutoScrollPosition = new Point(m_currencyPanel.AutoScrollPosition.X, y);
        }

        /// <summary>
        /// Handles the down button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the 
        /// event data.</param>
        private void DownButtonClick(object sender, EventArgs e)
        {
            int y = m_currencyPanel.AutoScrollPosition.Y;

            y = Math.Abs(y) + ScrollDelta;

            m_currencyPanel.AutoScrollPosition = new Point(m_currencyPanel.AutoScrollPosition.X, y);
        }

        /// <summary>
        /// Handles a currency button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the 
        /// event data.</param>
        private void CurrencyClick(object sender, EventArgs e)
        {
            ImageButton button = sender as ImageButton;

            if(button != null)
            {
                DialogResult = DialogResult.OK;
                Currency = (Currency)button.Tag;
                ClearControls();
                Close();
            }
        }
        #endregion

        #region Member Properties
        /// <summary>
        /// Gets the currency chosen by the user.
        /// </summary>
        public Currency Currency
        {
            get;
            private set;
        }
        #endregion
    }
}
