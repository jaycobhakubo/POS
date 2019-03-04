#region Copyright
// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2008 GameTech
// International, Inc.
#endregion

//US4323: (US4319) POS: Automatically award a discount
//US4636: (US4319) POS Multiple discounts

using System;
using GTI.Modules.POS.Properties;
using GTI.Modules.Shared;
using GTI.Modules.Shared.Business;

namespace GTI.Modules.POS.Business
{
    /// <summary>
    /// The abstract base class from which all discounts should derive.
    /// </summary>
    internal abstract class Discount : IEquatable<Discount>
    {
        #region Member Variables
        protected int m_id;
        protected decimal m_amount;
        protected decimal m_ptsPerDollar;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the Discount class.
        /// </summary>
        protected Discount()
            : this(0, 0M, 0M, Resources.Discount, DiscountItem.AwardTypes.Manual)
        {
        }

        /// <summary>
        /// Initializes a new instance of the Discount class with the specified
        /// id, amount, and points per dollar.
        /// </summary>
        /// <param name="id">The id of the discount.</param>
        /// <param name="amount">The discount amount.</param>
        /// <param name="ptsPerDollar">The amount of points calculated for 
        /// every dollar of the discount.</param>
        /// <param name="name">Name of the discount</param>
        protected Discount(int id, decimal amount, decimal ptsPerDollar, string name, DiscountItem.AwardTypes awardType)
        {
            m_id = id;
            m_amount = amount;
            m_ptsPerDollar = ptsPerDollar;
            Name = name; 
            AwardType = awardType;
        }

        /// <summary>
        /// Initializes a new instance of the Discount class from an 
        /// existing instance.
        /// </summary>
        /// <param name="discount">The existing instance.</param>
        /// <exception cref="System.ArgumentNullException">discount is a null
        /// reference.</exception>
        protected Discount(Discount discount)
        {
            if(discount == null)
                throw new ArgumentNullException("discount");

            m_id = discount.m_id;
            m_amount = discount.m_amount;
            m_ptsPerDollar = discount.m_ptsPerDollar;
            AwardType = discount.AwardType;
            DiscountItem = discount.DiscountItem;
        }

        protected Discount(DiscountItem discount)
        {
            if (discount == null)
                throw new ArgumentNullException("discount");

            DiscountItem = discount;
            m_id = discount.DiscountId;
            m_amount = discount.DiscountAmount;
            m_ptsPerDollar = discount.PointsPerDollar;
            AwardType = discount.DiscountAwardType;
            Name = discount.DiscountName;
            IsPlayerRequired = discount.IsPlayerRequired;
            IsActive = discount.IsActive;
        }
        #endregion
        
        #region Member Methods
        /// <summary>
        /// Determines whether two Discount instances are equal.
        /// </summary>
        /// <param name="obj">The Discount to compare with the 
        /// current Discount.</param>
        /// <returns>true if the specified Discount is equal to the current 
        /// Discount; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            Discount discount = obj as Discount;

            if(discount == null) 
                return false;
            else
                return Equals(discount);
        }

        /// <summary>
        /// Serves as a hash function for a Discount. 
        /// GetHashCode is suitable for use in hashing algorithms and data
        /// structures like a hash table. 
        /// </summary>
        /// <returns>A hash code for the current Discount.</returns>
        public override int GetHashCode()
        {
            // Rally US738
            return (m_id.GetHashCode() ^ m_amount.GetHashCode() & m_ptsPerDollar.GetHashCode());
        }

        // Rally US738
        /// <summary>
        /// Determines whether two Discount instances are equal.
        /// </summary>
        /// <param name="other">The Discount to compare with the 
        /// current Discount.</param>
        /// <returns>true if the specified Discount is equal to the current 
        /// Discount; otherwise, false.</returns>
        public virtual bool Equals(Discount other)
        {
            return (other != null &&
                    (GetType().Equals(other.GetType())) &&
                    m_id == other.m_id &&
                    m_amount == other.m_amount &&
                    m_ptsPerDollar == other.m_ptsPerDollar);
        }

        /// <summary>
        /// Calculates the total discount amount for this discount.
        /// </summary>
        /// <returns>The total discount amount.</returns>
        public abstract decimal CalculateTotal();

        /// <summary>
        /// Calculates the total points per dollar based on the discount total.
        /// </summary>
        /// <returns>The points per dollars amount.</returns>
        public abstract decimal CalculatePointsPerDollar();

        /// <summary>
        /// Multiplies the amount of the discount by -1.
        /// </summary>
        public void InvertAmount()
        {
            m_amount = decimal.Negate(m_amount);
        }
        #endregion

        #region Member Properties
        /// <summary>
        /// Gets or sets the id of the discount.
        /// </summary>
        public int Id
        {
            get
            {
                return m_id;
            }
            set
            {
                m_id = value;
            }
        }

        /// <summary>
        /// Gets or sets the discount amount.
        /// </summary>
        public virtual decimal Amount
        {
            get
            {
                return m_amount;
            }
            set
            {
                m_amount = value;
            }
        }

        /// <summary>
        /// Gets or sets the amount of points calculated for every dollar of 
        /// the discount.
        /// </summary>
        public decimal PointsPerDollar
        {
            get
            {
                return m_ptsPerDollar;
            }
            set
            {
                m_ptsPerDollar = value;
            }
        }

        public string Name { get; set; }

        public DiscountItem.AwardTypes AwardType { get; set; }

        public bool IsActive; 
        
        public bool IsPlayerRequired;

        //this is for auto discounts
        //for menu button discount, then this will be null
        public DiscountItem DiscountItem { get; set; }

        #endregion
    }

    /// <summary>
    /// Represents a discount that has a fixed price.
    /// </summary>
    internal class FixedDiscount : Discount
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the FixedDiscount class.
        /// </summary>
        public FixedDiscount() 
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the FixedDiscount class with the 
        /// specified id, amount, and points per dollar.
        /// </summary>
        /// <param name="id">The id of the discount.</param>
        /// <param name="amount">The discount amount.</param>
        /// <param name="ptsPerDollar">The amount of points calculated for 
        /// every dollar of the discount.</param>
        public FixedDiscount(int id, decimal amount, decimal ptsPerDollar, string name, DiscountItem.AwardTypes awardType)
            : base(id, amount, ptsPerDollar, name, awardType)
        {
        }

        /// <summary>
        /// Initializes a new instance of the FixedDiscount class from an 
        /// existing instance.
        /// </summary>
        /// <param name="other">The existing instance.</param>
        /// <exception cref="System.ArgumentNullException">discount is a null
        /// reference.</exception>
        public FixedDiscount(FixedDiscount discount)
            : base(discount)
        {
        }

        public FixedDiscount(DiscountItem discount)
            : base(discount)
        {
        }
        #endregion

        #region Member Methods
        /// <summary>
        /// Calculates the total discount amount for this discount.
        /// </summary>
        /// <returns>The total discount amount.</returns>
        public override decimal CalculateTotal()
        {
            // In this case, the total is just the amount.
            return decimal.Negate(m_amount);
        }

        /// <summary>
        /// Calculates the total points per dollar based on the discount total.
        /// </summary>
        /// <returns>The points per dollars amount.</returns>
        public override decimal CalculatePointsPerDollar()
        {
            return (CalculateTotal() * m_ptsPerDollar);
        }
        #endregion
    }

    /// <summary>
    /// Represents a fixed discount that is defined by the user.
    /// </summary>
    internal class OpenDiscount : Discount
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the OpenDiscount class.
        /// </summary>
        public OpenDiscount() 
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the OpenDiscount class with the 
        /// specified id, amount, and points per dollar.
        /// </summary>
        /// <param name="id">The id of the discount.</param>
        /// <param name="amount">The discount amount.</param>
        /// <param name="ptsPerDollar">The amount of points calculated for 
        /// every dollar of the discount.</param>
        public OpenDiscount(int id, decimal amount, decimal ptsPerDollar, string name, DiscountItem.AwardTypes awardType)
            : base(id, amount, ptsPerDollar, name, awardType)
        {
        }

        /// <summary>
        /// Initializes a new instance of the OpenDiscount class from an 
        /// existing instance.
        /// </summary>
        /// <param name="other">The existing instance.</param>
        /// <exception cref="System.ArgumentNullException">discount is a null
        /// reference.</exception>
        public OpenDiscount(OpenDiscount discount)
            : base(discount)
        {
        }

        public OpenDiscount(DiscountItem discount)
            : base(discount)
        {
        }
        #endregion

        #region Member Methods
        /// <summary>
        /// Calculates the total discount amount for this discount.
        /// </summary>
        /// <returns>The total discount amount.</returns>
        public override decimal CalculateTotal()
        {
            // In this case, the total is just the amount.
            return decimal.Negate(m_amount);
        }

        /// <summary>
        /// Calculates the total points per dollar based on the discount total.
        /// </summary>
        /// <returns>The points per dollars amount.</returns>
        public override decimal CalculatePointsPerDollar()
        {
            return (CalculateTotal() * m_ptsPerDollar);
        }
        #endregion
    }

#pragma warning disable 659
    /// <summary>
    /// Represents a discount that is a percentage of all the items that come 
    /// before it in the sale order.
    /// </summary>
    internal class PercentDiscount : Discount, IEquatable<PercentDiscount>
    {
        #region Constants and Data Types
        protected const int Precision = 2;
        #endregion

        #region Member Variables
        protected Sale m_parentSale;
        private decimal m_discountPercentage;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the PercentDiscount class.
        /// </summary>
        public PercentDiscount()
        {
        }

        /// <summary>
        /// Initializes a new instance of the PercentDiscount class from an 
        /// existing instance.
        /// </summary>
        /// <param name="other">The existing instance.</param>
        /// <exception cref="System.ArgumentNullException">discount is a null
        /// reference.</exception>
        public PercentDiscount(PercentDiscount discount)
            : base(discount)
        {
            m_parentSale = discount.m_parentSale;
            m_discountPercentage = discount.Amount;
        }

        public PercentDiscount(DiscountItem discount)
            : base(discount)
        {
            m_discountPercentage = discount.DiscountAmount;
        }
        #endregion

        #region Member Methods
        /// <summary>
        /// Determines whether two PercentDiscount instances are equal.
        /// </summary>
        /// <param name="obj">The PercentDiscount to compare with the 
        /// current PercentDiscount.</param>
        /// <returns>true if the specified PercentDiscount is equal to the 
        /// current PercentDiscount; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            PercentDiscount discount = obj as PercentDiscount;

            if(discount == null) 
                return false;
            else
                return Equals(discount);
        }

        // Rally US738
        /// <summary>
        /// Determines whether a PercentDiscount and a Discount instance are
        /// equal.
        /// </summary>
        /// <param name="other">The Discount to compare with the 
        /// current PercentDiscount.</param>
        /// <returns>true if the specified Discount is equal to the current 
        /// PercentDiscount; otherwise, false.</returns>
        public override bool Equals(Discount other)
        {
            return Equals((object)other);
        }

        /// <summary>
        /// Determines whether two PercentDiscount instances are equal.
        /// </summary>
        /// <param name="other">The PercentDiscount to compare with the 
        /// current PercentDiscount.</param>
        /// <returns>true if the specified PercentDiscount is equal to the 
        /// current PercentDiscount; otherwise, false.</returns>
        public virtual bool Equals(PercentDiscount other)
        {
            return (other != null &&
                    base.Equals(other));
        }

        /// <summary>
        /// Calculates the total discount amount for this discount.
        /// </summary>
        /// <returns>
        /// The total discount amount.
        /// </returns>
        public override decimal CalculateTotal()
        {
            // In this case, the total is just the amount.
            return decimal.Negate(m_amount);
        }

        /// <summary>
        /// Calculates the total points per dollar based on the discount total.
        /// </summary>
        /// <returns>The points per dollar amount.</returns>
        /// <exception cref="System.NullReferenceException">The 
        /// Parent Sale is a null reference.</exception>
        /// <exception cref="POSException">The 
        /// LineNumber is less than 0.</exception>
        /// <exception cref="System.IndexOutOfRangeException">The 
        /// LineNumber is greater than the number of items in the 
        /// sale.</exception>
        public override decimal CalculatePointsPerDollar()
        {
            return (CalculateTotal() * m_ptsPerDollar);
        }
        #endregion

        #region Member Properties
        /// <summary>
        /// Gets or sets the sale to which this discount belongs.
        /// </summary>
        public Sale Parent
        {
            get
            {
                return m_parentSale;
            }
            set
            {
                m_parentSale = value;
            }
        }
        
        //set the percentage discount
        public decimal DiscountPercentage
        {
            get
            {
                if (m_discountPercentage == 0)
                {
                    m_discountPercentage = Amount;
                }

                return m_discountPercentage;
            }
        }

        public override decimal Amount
        {
            get
            {
                return m_amount;
            }
            set
            {
                //set the percentage discount
                if (value > 0 && m_amount == 0 && m_discountPercentage == 0)
                {
                    m_discountPercentage = value;
                }

                m_amount = value;
            }
        }

        #endregion
    }
#pragma warning restore 659

    internal class DiscountFactory
    {
        public static Discount CreateDiscount(DiscountItem discount)
        {
            Discount posDiscount = null;
            switch (discount.Type)
            {
                case DiscountType.Fixed:
                    posDiscount = new FixedDiscount(discount);
                    break;
                case DiscountType.Open:
                    posDiscount = new OpenDiscount(discount);
                    break;
                case DiscountType.Percent:
                    posDiscount = new PercentDiscount(discount);
                    break;
            }

            return posDiscount;
        }

        public static Discount CreateDiscount(DiscountType type)
        {
            Discount posDiscount = null;
            switch (type)
            {
                case DiscountType.Fixed:
                    posDiscount = new FixedDiscount();
                    break;
                case DiscountType.Open:
                    posDiscount = new OpenDiscount();
                    break;
                case DiscountType.Percent:
                    posDiscount = new PercentDiscount();
                    break;
            }

            return posDiscount;
        }
    }

}
