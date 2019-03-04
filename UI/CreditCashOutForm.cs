#region Copyright
// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2008 GameTech
// International, Inc.
#endregion

// TTP 50433
// Rally TA7465

using System;
using System.Globalization;
using GTI.Modules.POS.Business;
using GTI.Modules.POS.Properties;
using GTI.Modules.Shared;

namespace GTI.Modules.POS.UI
{
    /// <summary>
    /// Represents the form that cashes out a player's credit.
    /// </summary>
    internal partial class CreditCashOutForm : KeypadForm
    {
        // Rally DE498
        #region Constants and Data Types
        protected const decimal MaxCashOut = 999999.99M;
        #endregion

        #region Member Variables
        protected decimal m_balance;
        protected Currency m_cashOutCurrency; 
        #endregion

        #region Constructors
        /// <summary>
        /// Initalizes a new instance of the CreditCashOutForm class.
        /// Required method for Designer support.
        /// </summary>
        protected CreditCashOutForm() 
            : base()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the CreditCashOutForm class.
        /// </summary>
        /// <param name="parent">The PointOfSale to which this form 
        /// belongs.</param>
        /// <param name="displayMode">The display mode used to show this 
        /// form.</param>
        /// <exception cref="System.ArgumentNullException">parent or 
        /// displayMode is a null reference.</exception>
        public CreditCashOutForm(PointOfSale parent, DisplayMode displayMode)
            : base(parent, displayMode, false)
        {
            InitializeComponent();
            ApplyDisplayMode();

            // Set specific credit cash out properties.
            ShowOptionButtons(true, true, true, true, false);
            Option1Text = Resources.CashOutBalance;
            Option2Text = Resources.ButtonOk;
            Option3Text = Resources.ButtonCancel;

            // The cash out currency defaults to the current currency.
            m_cashOutCurrency = new Currency(parent.CurrentCurrency);
            m_currencyButton.Text = m_cashOutCurrency.ISOCode;

            if(!parent.Settings.MultiCurrencies)
            {
                m_keypad.CurrencySymbol = m_cashOutCurrency.Symbol;
                m_currencyButton.Visible = false;
            }
            else
            {
                m_keypad.CurrencySymbol = null;
                m_currencyButton.Visible = true;
            }
        }
        #endregion

        #region Member Methods
        /// <summary>
        /// Handles the keypad's option 1 button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the 
        /// event data.</param>
        protected override void Option1Click(object sender, EventArgs e)
        {
            // Don't use the base class's functionality.
            // Rally DE498
            decimal displayBalance = m_balance;

            if(m_cashOutCurrency != m_parent.DefaultCurrency)
                displayBalance = m_cashOutCurrency.ConvertFromDefaultCurrencyToThisCurrency(displayBalance);

            if(displayBalance <= MaxCashOut)
                m_keypad.Value = displayBalance;
            else
                m_keypad.Value = MaxCashOut;
        }

        /// <summary>
        /// Handles the currency button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The EventArgs object that contains 
        /// the event data.</param>
        private void CurrencyClick(object sender, EventArgs e)
        {
            CurrencyForm currencyForm = new CurrencyForm(m_parent, m_displayMode, Resources.SelectCashOutCurrency);
            currencyForm.ShowDialog(this);

            m_cashOutCurrency = currencyForm.Currency;
            m_currencyButton.Text = m_cashOutCurrency.ISOCode;

            UpdateAmount();
        }

        /// <summary>
        /// Updates the current credit balance displayed on the form.
        /// </summary>
        private void UpdateAmount()
        {
            decimal displayBalance = m_balance;

            if(m_cashOutCurrency != m_parent.DefaultCurrency)
                displayBalance = m_cashOutCurrency.ConvertFromDefaultCurrencyToThisCurrency(displayBalance);

            Message = string.Format(CultureInfo.CurrentCulture, Resources.CreditCashOutAmount, displayBalance);

            // The user cannot cash out 0.
            if(displayBalance <= 0)
            {
                m_keypad.Option1Enabled = false;
                m_keypad.Option2Enabled = false;
            }
            else
            {
                m_keypad.Option1Enabled = true;
                m_keypad.Option2Enabled = true;
            }
        }
        #endregion

        #region Member Properties
        /// <summary>
        /// Gets or sets the refundable credit balance.
        /// </summary>
        public decimal Balance
        {
            get
            {
                return m_balance;
            }
            set
            {
                m_balance = value;
                UpdateAmount();
            }
        }

        /// <summary>
        /// Gets the cash out currency.
        /// </summary>
        public Currency CashOutCurrency
        {
            get
            {
                return m_cashOutCurrency;
            }
        }
        #endregion
    }
}