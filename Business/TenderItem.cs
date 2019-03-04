#region Copyright
// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2015 Fortunet, Inc.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GTI.Modules.Shared;
using GTI.EliteCreditCards.Data;

namespace GTI.Modules.POS.Business
{
    /// <summary>
    /// Represents a single "tender" action on this sale
    /// </summary>
    internal class TenderItem
    {
        #region Constants and Data Types

        #endregion

        #region Member Variables

        protected CreditCardProcessingReply m_processingReply = new CreditCardProcessingReply();
        protected Sale m_sale; //Weak reference to the sale tied to this tender
        protected SaleTender m_saleTender; //detailed tender information

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a tender for a specific sale
        /// </summary>
        public TenderItem(Sale sale, decimal amount, Currency currency, decimal currencyAmount, SaleTender saleTender = null)
        {
            m_sale = sale;
            Amount = amount;
            Voided = false;
            TenderedCurrency = currency;
            TenderedCurrencyAmount = currencyAmount;
            m_saleTender = saleTender;
        }

        public TenderItem(SaleTender saleTender, Currency currency)
        {
            m_sale = null;
            Amount = saleTender.DefaultAmount;
            Voided = saleTender.TransactionTypeID == TransactionType.Void;
            TenderedCurrency = currency;
            TenderedCurrencyAmount = saleTender.Amount;
            m_saleTender = saleTender;
        }

        #endregion

        #region Member Methods

        #endregion

        #region Member Properties

        /// <summary>
        /// The amount of the tender
        /// </summary>
        public decimal Amount { get; private set; }

        /// <summary>
        /// Gets or sets the type of tender
        /// </summary>
        public Shared.TenderType Type
        {
            get;
            set;
        }

        public bool Voided
        {
            get;
            set;
        }

        public CreditCardProcessingReply ProcessingInfo
        {
            get
            {
                return m_processingReply;
            }
        }

        public decimal TenderedCurrencyAmount
        {
            get;
            set;
        }

        public Currency TenderedCurrency
        {
            get;
            set;
        }

        public SaleTender SaleTenderInfo
        {
            get
            {
                return m_saleTender;
            }

            set
            {
                m_saleTender = value;
            }
        }

        #endregion
    }

    internal class ListBoxTenderItem
    {
        #region Member Variables

        protected decimal m_amount;
        protected decimal m_tax;
        
        #endregion

        #region Constructors

        public ListBoxTenderItem()
        {
            Amount = 0;
            IsRefunded = false;
            PartialRefund = false;
            TimeStamp = DateTime.Now;
            IsTextLine = true;
            LineText = " ";
            IsTotalLine = false;
            ReceiptLine = -1;
            NonTaxedCoupon = false;
        }

        public ListBoxTenderItem(GTI.Modules.Shared.TenderType type, decimal amount)
        {
            Type = type;
            Amount = amount;
            IsRefunded = false;
            PartialRefund = false;
            TimeStamp = DateTime.Now;
            IsTextLine = false;
            LineText = string.Empty;
            IsTotalLine = false;
            ReceiptLine = -1;
            NonTaxedCoupon = false;
        }

        public ListBoxTenderItem(string text, decimal amount, bool isTotalLine = false, int receipLine = -1, bool nonTaxedCoupon = false)
        {
            Type = 0;
            Amount = amount;
            IsRefunded = false;
            PartialRefund = false;
            TimeStamp = DateTime.Now;
            IsTextLine = true;
            LineText = text;
            IsTotalLine = isTotalLine;
            ReceiptLine = receipLine;
            NonTaxedCoupon = nonTaxedCoupon;
        }

        #endregion

        #region Member Methods

        public override string ToString()
        {
            if (IsTextLine)
            {
                if (IsTotalLine)
                    return string.Format("{0, -27} {1, 10}", LineText, POS.DefaultCurrency.FormatCurrencyString(Amount));
                else
                    return string.Format("{0, -38}", LineText);
            }
            else
            {
                string name;

                if (TenderItemObject != null && TenderItemObject.SaleTenderInfo != null)
                    name = TenderItemObject.SaleTenderInfo.ReceiptDescription;
                else
                    name = PosSettings.GetTenderName(Type);

                if (TenderItemObject != null && !TenderItemObject.TenderedCurrency.IsDefault)
                    return string.Format("{0, -15} {1, 10}= {2, 10}", name, TenderItemObject.TenderedCurrency.FormatCurrencyString(TenderItemObject.TenderedCurrencyAmount), POS.DefaultCurrency.FormatCurrencyString(Amount));
                else
                    return string.Format("{0, -15}             {1, 10}", name, POS.DefaultCurrency.FormatCurrencyString(Amount));
            }
        }

        #endregion

        #region Member Properties

        /// <summary>
        /// The amount of the tender
        /// </summary>
        public decimal Amount
        {
            get
            {
                if (TenderItemObject == null)
                    return m_amount;
                else
                    return TenderItemObject.Amount;
            }

            set
            {
                m_amount = value;
            }
        }

        /// <summary>
        /// Tax total for the entire sale in the default currency (needed for Shift4).
        /// </summary>
        public decimal Tax
        {
            get
            {
                return m_tax;
            }

            set
            {
                m_tax = value;
            }
        }

        /// <summary>
        /// Gets or sets the type of tender
        /// </summary>
        public Shared.TenderType Type { get; set; }

        /// <summary>
        /// Gets whether or not this tender is being refunded
        /// </summary>
        public bool IsRefunded
        {
            get
            {
                if(TenderItemObject == null)
                    return false;
                else
                    return TenderItemObject.Voided;
            }

            set
            {
                if (TenderItemObject != null)
                    TenderItemObject.Voided = value;
            }
       }

        public bool PartialRefund
        {
            get;

            set;
        }

        /// <summary>
        /// A timestamp labelling when this object was created?
        /// </summary>
        public DateTime TimeStamp { get; private set; }

        public int ReceiptLine
        {
            get;
            set;
        }

        public bool IsTotalLine
        {
            get;
            set;
        }

        public bool IsTextLine
        {
            get;
            set;
        }

        public string LineText
        {
            get;
            set;
        }

        public static POSSettings PosSettings
        {
            get;
            set;
        }

        public static PointOfSale POS
        {
            get;
            set;
        }

        public TenderItem TenderItemObject
        {
            get;
            set;
        }

        public bool NonTaxedCoupon
        {
            get;
            set;
        }

        #endregion
    }
}
