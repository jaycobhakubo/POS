#region Copyright
// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2008 GameTech
// International, Inc.
#endregion

using System;
using System.Windows.Forms;
using System.Globalization;
using GTI.Modules.Shared;
using GTI.Modules.POS.UI;
using GTI.Modules.POS.Properties;
using GTI.Modules.Shared.Business;
using System.Linq;
using System.Collections.Generic;

namespace GTI.Modules.POS.Business
{
    /// <summary>
    /// A menu button that represents a discount.
    /// </summary>
    internal class DiscountButton : MenuButton
    {
        #region Member Variables
        protected Discount m_discount;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the DiscountButton class.
        /// </summary>
        /// <param name="pos">The instance of the PointOfSale class.</param>
        /// <param name="discount">The discount to associate with this 
        /// button.</param>
        /// <exception cref="System.ArgumentNullException">pos is a null 
        /// reference.</exception>
        /// <exception cref="System.ArgumentNullException">discount is a null 
        /// reference.</exception>
        public DiscountButton(PointOfSale pos, Discount discount)
            : base(pos)
        {
            if(discount == null)
                throw new ArgumentNullException("discount");

            m_discount = discount;
        }
        #endregion

        #region Member Methods
        /// <summary>
        /// Handles when the discount button is clicked.
        /// </summary>
        /// <param name="sender">Any object that implements IWin32Window 
        /// that represents the top-level window that will own any modal 
        /// dialog boxes.</param>
        /// <param name="argument">Reserved for future use.</param>
        public override void Click(IWin32Window sender, object argument)
        {
            // FIX: DE2957 - Attempting to sell a multiple of a discount only 1 will get recorded.
            int quantity = 1;

            if (argument is int)
                quantity = (int)argument;

            // Make a clone of the discount to add to the sale.
            Discount clone;

            if (m_discount is FixedDiscount)
            {
                clone = new FixedDiscount((FixedDiscount)m_discount);
            }
            else if (m_discount is OpenDiscount)
            {
                clone = new OpenDiscount((OpenDiscount)m_discount);

                DisplayMode displayMode;
                bool use00;
                decimal openDiscountAmount = 0M;

                lock (m_pos.Settings.SyncRoot)
                {
                    displayMode = m_pos.Settings.DisplayMode;
                    use00 = m_pos.Settings.Use00ForCurrencyEntry;
                }

                m_pos.CanUpdateMenus = false; // PDTS 964

                // Prompt for the discount amount.
                KeypadForm amountForm = new KeypadForm(m_pos, displayMode, false);
                amountForm.GetKeypad().Use00AsDecimalPoint = !use00;

                // Rally TA7464
                if (m_pos.CurrentSale != null)
                {
                    amountForm.CurrencySymbol = m_pos.CurrentSale.SaleCurrency.Symbol;
                    amountForm.Message = string.Format(CultureInfo.CurrentCulture, Resources.OpenDiscountAmount, m_pos.CurrentSale.SaleCurrency.ISOCode);
                }
                else
                {
                    amountForm.CurrencySymbol = m_pos.DefaultCurrency.Symbol;
                    amountForm.Message = string.Format(CultureInfo.CurrentCulture, Resources.OpenDiscountAmount, m_pos.DefaultCurrency.ISOCode);
                }

                amountForm.BigButtonText = Resources.ButtonOk;

                amountForm.ShowDialog(sender);

                openDiscountAmount = (decimal)amountForm.Value;

                amountForm.Dispose();

                // Rally TA7464
                if (m_pos.CurrentSale != null)
                    clone.Amount = m_pos.CurrentSale.RemoveSalesExchangeRate(openDiscountAmount);
                else
                    clone.Amount = openDiscountAmount;

                m_pos.CanUpdateMenus = true; // PDTS 964

                if (clone.Amount == 0M)
                    return;
            }
            else if (m_discount is PercentDiscount)
            {
                clone = new PercentDiscount((PercentDiscount)m_discount);
            }
            else
            {
                return;
            }

            clone.Name = Text;
            
            // Rally DE129
            m_pos.AddSaleItem(m_pos.CurrentSession, clone, quantity, m_isPlayerRequired);
            // END: DE2957
        }
        #endregion

        #region Member Properties
        /// <summary>
        /// Gets or sets the discount assoicated with this button.
        /// </summary>
        /// <exception cref="System.NullReferenceException">The discount is a 
        /// null reference.</exception>
        public Discount Discount
        {
            get
            {
                return m_discount;
            }
            set
            {
                if(value == null)
                    throw new ArgumentNullException("Discount");

                m_discount = value;
            }
        }
        #endregion
    }
}
