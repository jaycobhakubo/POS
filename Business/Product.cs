#region Copyright
// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2008 GameTech
// International, Inc.
//
// US2826 Adding support for being able to sell barcoded paper 
#endregion

using System;
using GTI.Modules.Shared;
using GTI.Modules.POS.Data;
using System.Collections.Generic;
using GTI.Modules.Shared.Business;

namespace GTI.Modules.POS.Business
{
    /// <summary>
    /// Represents a purchaseable item.
    /// </summary>
    /// <remarks>All derived classes should implement IEquatable(Of T) and 
    /// ICloneable.</remarks>
    internal class Product : IEquatable<Product>, ICloneable
    {
        #region Member Variables
        protected int m_id;
        protected ProductType m_type;
        protected string m_name;
        protected bool m_isTaxed;
        protected decimal m_price;
        protected byte m_quantity;
        protected decimal m_ptsPerDollar;
        protected decimal m_ptsPerProduct;
        protected decimal m_ptsToRedeem;
        protected bool m_optional; // PDTS 964
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the Product class.
        /// </summary>
        public Product()
        {
        }

        /// <summary>
        /// Initializes a new instance of the Product class from an 
        /// existing instance.
        /// </summary>
        /// <param name="product">The existing instance.</param>
        /// <exception cref="System.ArgumentNullException">product is a null 
        /// reference.</exception>
        public Product(Product product)
        {
            if(product == null)
                throw new ArgumentNullException("product");

            m_id = product.m_id;
            m_type = product.m_type;
            m_name = product.m_name;
            m_isTaxed = product.m_isTaxed;
            m_price = product.m_price;
            m_quantity = product.m_quantity;
            m_ptsPerDollar = product.m_ptsPerDollar;
            m_ptsPerProduct = product.m_ptsPerProduct;
            m_ptsToRedeem = product.m_ptsToRedeem;
            m_optional = product.m_optional;
            AltPrice = product.AltPrice;
            UseAltPrice = product.UseAltPrice;
            IsQualifyingProduct = product.IsQualifyingProduct;
            Prepaid = product.Prepaid;
        }
        #endregion

        #region Member Methods
        /// <summary>
        /// Determines whether two Product instances are equal.
        /// </summary>
        /// <param name="obj">The Product to compare with the 
        /// current Product.</param>
        /// <returns>true if the specified Product is equal to the current 
        /// Product; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            Product product = obj as Product;

            if(product == null) 
                return false;
            else
                return Equals(product);
        }

        // Rally US738
        /// <summary>
        /// Serves as a hash function for a Product. 
        /// GetHashCode is suitable for use in hashing algorithms and data
        /// structures like a hash table. 
        /// </summary>
        /// <returns>A hash code for the current Product.</returns>
        public override int GetHashCode()
        {
            return (m_id.GetHashCode() ^ m_type.GetHashCode() ^ ((m_name != null) ? m_name.GetHashCode() : 0) ^
                    m_isTaxed.GetHashCode() ^ m_price.GetHashCode() ^ m_quantity.GetHashCode() ^
                    m_ptsPerDollar.GetHashCode() ^ m_ptsPerProduct.GetHashCode() ^ m_ptsToRedeem.GetHashCode() ^
                    m_optional.GetHashCode());
        }

        // Rally US738
        /// <summary>
        /// Determines whether two Product instances are equal.
        /// </summary>
        /// <param name="other">The Product to compare with the 
        /// current Product.</param>
        /// <returns>true if the specified Product is equal to the current 
        /// Product; otherwise, false.</returns>
        public virtual bool Equals(Product other)
        {
            return (other != null &&
                    (GetType().Equals(other.GetType())) &&
                    m_id == other.m_id &&
                    m_type == other.m_type &&
                    m_name == other.m_name &&
                    m_isTaxed == other.m_isTaxed &&
                    m_price == other.m_price &&
                    m_quantity == other.m_quantity &&
                    m_ptsPerDollar == other.m_ptsPerDollar &&
                    m_ptsPerProduct == other.m_ptsPerProduct &&
                    m_ptsToRedeem == other.m_ptsToRedeem &&
                    m_optional == other.m_optional);
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public virtual object Clone()
        {
            return new Product(this);
        }
        #endregion

        #region Member Properties
        /// <summary>
        /// Gets or sets the daily id of the product.
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
        /// Gets or sets the type of product.
        /// </summary>
        public ProductType Type
        {
            get
            {
                return m_type;
            }
            set
            {
                m_type = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the product.
        /// </summary>
        public string Name
        {
            get
            {
                return m_name;
            }
            set
            {
                m_name = value;
            }
        }

        /// <summary>
        /// Gets or sets whether this product is taxable. 
        /// </summary>
        public bool IsTaxed
        {
            get
            {
                return m_isTaxed;
            }
            set
            {
                m_isTaxed = value;
            }
        }

        /// <summary>
        /// Get or sets the product's price.
        /// </summary>
        public decimal Price
        {
            get
            {
                return m_price;
            }
            set
            {
                m_price = value;
            }
        }

        /// <summary>
        /// Gets the price paid for one (handles AltPrice).
        /// </summary>
        public decimal PricePaid
        {
            get
            {
                if (UseAltPrice)
                    return AltPrice;

                return m_price;
            }
        }

        /// <summary>
        /// Gets or sets the amount of products.
        /// </summary>
        public byte Quantity
        {
            get
            {
                return m_quantity;
            }
            set
            {
                m_quantity = value;
            }
        }

        /// <summary>
        /// Gets the total price of the product based on Price and Quantity.
        /// </summary>
        public decimal TotalPrice
        {
            get
            {
                return PricePaid * m_quantity;
            }
        }

        /// <summary>
        /// Gets or sets the alternate price.
        /// </summary>
        /// <value>
        /// The alt price.
        /// </value>
        public decimal AltPrice { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to Alternate Pricing.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [use alt price]; otherwise, <c>false</c>.
        /// </value>
        public bool UseAltPrice { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is a qualifying product.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is qualifying product; otherwise, <c>false</c>.
        /// </value>
        public bool IsQualifyingProduct { get; set; }

        public bool Prepaid { get; set; }

        /// <summary>
        /// Gets or sets the rate of points a player earns for every dollar
        /// for this product.
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

        /// <summary>
        /// Gets the total number of points earned for this product based on 
        /// PointsPerDollar, Price, and Quantity.
        /// </summary>
        public decimal TotalPointsPerDollar
        {
            get
            {
                return m_ptsPerDollar * TotalPrice; // FIX: DE2994 - Points shown on receipt do not agree with the server.
            }
        }

        /// <summary>
        /// Gets or sets the rate of points a player earns for each product.
        /// </summary>
        public decimal PointsPerProduct
        {
            get
            {
                return m_ptsPerProduct;
            }
            set
            {
                m_ptsPerProduct = value;
            }
        }

        /// <summary>
        /// Gets the total number of points earned for this product based on 
        /// PointsPerProduct and Quantity.
        /// </summary>
        public decimal TotalPointsPerProduct
        {
            get
            {
                return (m_ptsPerProduct * m_quantity);
            }
        }

        /// <summary>
        /// Gets or sets the number points required to purchase this product.
        /// </summary>
        public decimal PointsToRedeem
        {
            get
            {
                return m_ptsToRedeem;
            }
            set
            {
                m_ptsToRedeem = value;
            }
        }

        /// <summary>
        /// Gets the total number of points to purchase this product based on 
        /// PointsToRedeem and Quantity.
        /// </summary>
        public decimal TotalPointsToRedeem
        {
            get
            {
                return (m_ptsToRedeem * m_quantity);
            }
        }

        // PDTS 964
        /// <summary>
        /// Gets or sets whether this product is optional when included in a 
        /// package.
        /// </summary>
        public bool Optional
        {
            get
            {
                return m_optional;
            }
            set
            {
                m_optional = value;
            }
        }

        /// <summary>
        /// Gets or sets the object that contains data about the control.
        /// Returns:
        ///     An System.Object that contains data about the control. The default is null.
        /// </summary>
        public object Tag
        {
            get;
            set;
        }

        #endregion
    }

    // Rally TA5748
    /// <summary>
    /// Represents a paper pack starting card number.
    /// </summary>
    internal struct StartNumber
    {
        /// <summary>
        /// The start number of the pack.
        /// </summary>
        public int Number;

        /// <summary>
        /// Any applicable status code about the start number.
        /// </summary>
        public int Status;
    }
    // END: TA5748

    /// <summary>
    /// Represents a product that is played with a bingo game.
    /// </summary>
    internal class BingoProduct : Product, IEquatable<BingoProduct>, ICloneable
    {
        #region Member Variables
        protected int m_gameCategoryId;
        protected short m_numsRequired;
        protected short m_cardCount;
        protected CardType m_cardType;
        protected GameType m_gameType;
        protected int m_cardLevelId; // Rally US738 - Duplicate Product Criteria
        protected bool m_canValidateProduct;
        protected bool m_barcodedPaper; // US2826
        protected List<StartNumber> m_startNumbers = new List<StartNumber>(); // Rally TA5748
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the BingoProduct class.
        /// </summary>
        public BingoProduct()
        {
        }

        /// <summary>
        /// Initializes a new instance of the BingoProduct class from
        /// an existing instance.
        /// </summary>
        /// <param name="bingoProduct">An existing instance.</param>
        /// <exception cref="System.ArgumentNullException">bingoProduct is a 
        /// null reference.</exception>
        public BingoProduct(BingoProduct bingoProduct)
            : base(bingoProduct)
        {
            m_gameCategoryId = bingoProduct.m_gameCategoryId;
            m_numsRequired = bingoProduct.m_numsRequired;
            m_cardCount = bingoProduct.m_cardCount;
            m_cardType = bingoProduct.m_cardType;
            m_gameType = bingoProduct.m_gameType;
            m_cardLevelId = bingoProduct.m_cardLevelId; // Rally US738
            m_canValidateProduct = bingoProduct.m_canValidateProduct;
            m_barcodedPaper = bingoProduct.m_barcodedPaper;// US2826
        }
        #endregion

        #region Member Methods
        /// <summary>
        /// Determines whether two BingoProduct instances are equal.
        /// </summary>
        /// <param name="obj">The BingoProduct to compare with the 
        /// current BingoProduct.</param>
        /// <returns>true if the specified BingoProduct is equal to the 
        /// current BingoProduct; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            BingoProduct product = obj as BingoProduct;

            if(product == null) 
                return false;
            else
                return Equals(product);
        }

        // Rally US738
        /// <summary>
        /// Serves as a hash function for a BingoProduct. 
        /// GetHashCode is suitable for use in hashing algorithms and data
        /// structures like a hash table. 
        /// </summary>
        /// <returns>A hash code for the current BingoProduct.</returns>
        public override int GetHashCode()
        {
            return (base.GetHashCode() ^ m_gameCategoryId.GetHashCode() ^ m_numsRequired.GetHashCode() ^ 
                    m_cardCount.GetHashCode() ^ m_cardType.GetHashCode() ^ m_gameType.GetHashCode() ^
                    m_cardLevelId.GetHashCode()^ m_barcodedPaper.GetHashCode());
        }

        // Rally US738
        /// <summary>
        /// Determines whether a BingoProduct and a Product instance are equal.
        /// </summary>
        /// <param name="other">The Product to compare with the 
        /// current BingoProduct.</param>
        /// <returns>true if the specified Product is equal to the current 
        /// BingoProduct; otherwise, false.</returns>
        public override bool Equals(Product other)
        {
            return Equals((object)other);
        }

        /// <summary>
        /// Determines whether two BingoProduct instances are equal.
        /// </summary>
        /// <param name="other">The BingoProduct to compare with the 
        /// current BingoProduct.</param>
        /// <returns>true if the specified BingoProduct is equal to the 
        /// current BingoProduct; otherwise, false.</returns>
        public virtual bool Equals(BingoProduct other)
        {
            return (other != null &&
                    base.Equals(other) &&
                    m_gameCategoryId == other.m_gameCategoryId &&
                    m_numsRequired == other.m_numsRequired &&
                    m_cardCount == other.m_cardCount &&
                    m_cardType == other.m_cardType &&
                    m_gameType == other.m_gameType &&
                    m_cardLevelId == other.m_cardLevelId &&
                    m_barcodedPaper == other.m_barcodedPaper && 
                    m_canValidateProduct == other.m_canValidateProduct); // Rally US738
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public override object Clone()
        {
            return new BingoProduct(this);
        }

        //US3509
        /// <summary>
        /// Gets the validation quantity.
        /// </summary>
        /// <param name="productValidation">The product validation.</param>
        /// <returns></returns>
        public int GetValidationQuantity(ValidationPackage productValidation)
        {
            if (!CanValidateProduct)
                return 0;

            //if productValidation card count is zero, then all products validate
            if (productValidation.CardCount == 0)
                return Quantity;

            return CardCount * Quantity;
        }
        #endregion

        #region Member Properties
        /// <summary>
        /// The id of the game category this product is for.
        /// </summary>
        public int GameCategoryId
        {
            get
            {
                return m_gameCategoryId;
            }
            set
            {
                m_gameCategoryId = value;
            }
        }

        /// <summary>
        /// Gets or sets the numbers required to play this bingo product
        /// (i.e. the user must choose 8 numbers if this is a Crystal Ball 
        /// card).
        /// </summary>
        public short NumbersRequired
        {
            get
            {
                return m_numsRequired;
            }
            set
            {
                m_numsRequired = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of cards sold in this bingo product.
        /// </summary>
        public short CardCount
        {
            get
            {
                return m_cardCount;
            }
            set
            {
                m_cardCount = value;
            }
        }

        /// <summary>
        /// Gets or sets the type of card sold in this bingo product.
        /// </summary>
        public CardType CardType
        {
            get
            {
                return m_cardType;
            }
            set
            {
                m_cardType = value;
            }
        }

        /// <summary>
        /// Gets or sets the type of game this bingo product is for.
        /// </summary>
        public GameType GameType
        {
            get
            {
                return m_gameType;
            }
            set
            {
                m_gameType = value;
            }
        }

        // Rally US738
        /// <summary>
        /// Gets or sets the id of the card level for this bingo product.
        /// </summary>
        public int CardLevelId
        {
            get
            {
                return m_cardLevelId;
            }
            set
            {
                m_cardLevelId = value;
            }
        }

        /// US2826 Barcoded Paper support
        /// <summary>
        /// Gets or sets whether this product is a barcoded paper product
        /// </summary>
        public bool BarcodedPaper
        {
            get
            {
                return m_barcodedPaper;
            }

            set
            {
                m_barcodedPaper = value;
            }
        }

        public bool CanValidateProduct
        {
            get
            {
                return m_canValidateProduct;
            }
            set
            {
                m_canValidateProduct = value;
            }
        }

        public bool IsValidated { get; set; }

        // Rally TA5748
        /// <summary>
        /// A list of bingo pack start numbers used with this product (if
        /// applicable).
        /// </summary>
        public IList<StartNumber> StartNumbers
        {
            get
            {
                return m_startNumbers;
            }
        }
        // END: TA5748
        #endregion
    }

    internal struct PaperPackInfo
    {
        /// <summary>
        /// The serial number of the Pack
        /// </summary>
        public string SerialNumber;

        /// <summary>
        /// The audit number of the pack
        /// </summary>
        public int AuditNumber;
    }

    /// <summary>
    /// Represents a paper product that is played with a bingo game.
    /// </summary>
    internal class PaperBingoProduct : BingoProduct, IEquatable<PaperBingoProduct>, ICloneable
    {
        #region Member Variables
        protected List<PaperPackInfo> m_paperPackInfo = new List<PaperPackInfo>();
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the BingoProduct class.
        /// </summary>
        public PaperBingoProduct()
        {
        }

        /// <summary>
        /// Initializes a new instance of the PaperBingoProduct class from
        /// an existing instance.
        /// </summary>
        /// <param name="paperBingoProduct">An existing instance.</param>
        /// <exception cref="System.ArgumentNullException">bingoProduct is a 
        /// null reference.</exception>
        public PaperBingoProduct(PaperBingoProduct paperBingoProduct)
            : base(paperBingoProduct)
        {
            m_paperPackInfo = new List<PaperPackInfo>(paperBingoProduct.m_paperPackInfo);
        }
        #endregion

        #region Member Methods
        /// <summary>
        /// Determines whether two PaperBingoProduct instances are equal.
        /// </summary>
        /// <param name="obj">The PaperBingoProduct to compare with the 
        /// current PaperBingoProduct.</param>
        /// <returns>true if the specified PaperBingoProduct is equal to the 
        /// current PaperBingoProduct; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            PaperBingoProduct product = obj as PaperBingoProduct;

            if (product == null)
                return false;
            else
                return Equals(product);
        }

        /// <summary>
        /// Serves as a hash function for a PaperBingoProduct. 
        /// GetHashCode is suitable for use in hashing algorithms and data
        /// structures like a hash table. 
        /// </summary>
        /// <returns>A hash code for the current PaperBingoProduct.</returns>
        public override int GetHashCode()
        {
            return (base.GetHashCode());
        }

        /// <summary>
        /// Determines whether a PaperBingoProduct and a Product instance are equal.
        /// </summary>
        /// <param name="other">The Product to compare with the 
        /// current PaperBingoProduct.</param>
        /// <returns>true if the specified Product is equal to the current 
        /// PaperBingoProduct; otherwise, false.</returns>
        public override bool Equals(Product other)
        {
            return Equals((object)other);
        }

        /// <summary>
        /// Determines whether two PaperBingoProduct instances are equal.
        /// </summary>
        /// <param name="other">The PaperBingoProduct to compare with the 
        /// current PaperBingoProduct.</param>
        /// <returns>true if the specified PaperBingoProduct is equal to the 
        /// current PaperBingoProduct; otherwise, false.</returns>
        public virtual bool Equals(PaperBingoProduct other)
        {
            return (other != null &&
                    GetType().Equals(other.GetType()) &&
                    base.Equals(other));
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public override object Clone()
        {
            return new PaperBingoProduct(this);
        }

        /// <summary>
        /// Adds the given list to the local PackInfo list
        /// </summary>
        /// <param name="packInfo"></param>
        public bool AddPackInfo(PaperPackInfo packInfo)
        {
            bool rc = false;
            // check to make sure that this pack info is not
            // already in the list
            if(PackInfo.Contains(packInfo) == false)
            {
                PackInfo.Add(packInfo);
                rc = true;
            }

            return (rc);
        }

        public void RemovePackInfo()
        {
            m_paperPackInfo.Clear();
        }
        #endregion

        #region Member Properties
        /// <summary>
        /// A list of bingo pack start numbers used with this product (if
        /// applicable).
        /// </summary>
        public IList<PaperPackInfo> PackInfo
        {
            get
            {
                return m_paperPackInfo;
            }

            set
            {
                m_paperPackInfo.Clear();
                foreach (PaperPackInfo info in value)
                {
                    m_paperPackInfo.Add(info);
                }
            }
        }
        #endregion
    }

    /// <summary>
    /// Represents a product that can be sold to an electronic device.
    /// </summary>
    internal interface IElectronicProduct
    {
        #region Member Properties
        /// <summary>
        /// Gets or sets a bit packed field representing which devices this 
        /// product can be sold to.
        /// </summary>
        CompatibleDevices CompatibleDevices
        {
            get;
            set;
        }
        #endregion
    }

    /// <summary>
    /// Represents a generic product that can be sold to an electronic device.
    /// </summary>
    internal class ElectronicProduct : Product, IElectronicProduct, IEquatable<ElectronicProduct>, ICloneable
    {
        #region Member Variables
        protected CompatibleDevices m_compatibleDevices;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the ElectronicProduct class.
        /// </summary>
        public ElectronicProduct()
        {
        }

        /// <summary>
        /// Initializes a new instance of the ElectronicProduct class from
        /// an existing instance.
        /// </summary>
        /// <param name="electronic">An existing instance.</param>
        /// <exception cref="System.ArgumentNullException">electronic is a null 
        /// reference.</exception>
        public ElectronicProduct(ElectronicProduct electronic)
            : base(electronic)
        {
            m_compatibleDevices = electronic.m_compatibleDevices;
        }
        #endregion

        #region Member Methods
        /// <summary>
        /// Determines whether two ElectronicProduct instances are equal.
        /// </summary>
        /// <param name="obj">The ElectronicProduct to compare with the 
        /// current ElectronicProduct.</param>
        /// <returns>true if the specified ElectronicProduct is equal to 
        /// the current ElectronicProduct; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            ElectronicProduct product = obj as ElectronicProduct;

            if(product == null)
                return false;
            else
                return Equals(product);
        }

        // Rally US738
        /// <summary>
        /// Serves as a hash function for a ElectronicProduct. 
        /// GetHashCode is suitable for use in hashing algorithms and data
        /// structures like a hash table. 
        /// </summary>
        /// <returns>A hash code for the current 
        /// ElectronicProduct.</returns>
        public override int GetHashCode()
        {
            return (base.GetHashCode() ^ m_compatibleDevices.GetHashCode());
        }

        // Rally US738
        /// <summary>
        /// Determines whether an ElectronicProduct and a Product instance are
        /// equal.
        /// </summary>
        /// <param name="other">The Product to compare with the 
        /// current ElectronicProduct.</param>
        /// <returns>true if the specified Product is equal to the current 
        /// ElectronicProduct; otherwise, false.</returns>
        public override bool Equals(Product other)
        {
            return Equals((object)other);
        }

        // Rally US738
        /// <summary>
        /// Determines whether two ElectronicProduct instances are equal.
        /// </summary>
        /// <param name="other">The ElectronicProduct to compare with the 
        /// current ElectronicProduct.</param>
        /// <returns>true if the specified ElectronicProduct is equal to 
        /// the current ElectronicProduct; otherwise, false.</returns>
        public virtual bool Equals(ElectronicProduct other)
        {
            return (other != null &&
                    base.Equals(other) &&
                    m_compatibleDevices == other.m_compatibleDevices);
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public override object Clone()
        {
            return new ElectronicProduct(this);
        }
        #endregion

        #region Member Properties
        /// <summary>
        /// Gets or sets a bit packed field representing which devices this 
        /// product can be sold to.
        /// </summary>
        public CompatibleDevices CompatibleDevices
        {
            get
            {
                return m_compatibleDevices;
            }
            set
            {
                m_compatibleDevices = value;
            }
        }
        #endregion
    }

    /// <summary>
    /// Represents a bingo product that can be sold to an electronic device.
    /// </summary>
    internal class ElectronicBingoProduct : BingoProduct, IElectronicProduct, IEquatable<ElectronicBingoProduct>, ICloneable
    {
        #region Member Variables
        protected CompatibleDevices m_compatibleDevices;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the ElectronicBingoProduct class.
        /// </summary>
        public ElectronicBingoProduct()
        {
        }

        /// <summary>
        /// Initializes a new instance of the ElectronicBingoProduct class from
        /// an existing instance.
        /// </summary>
        /// <param name="electronic">An existing instance.</param>
        /// <exception cref="System.ArgumentNullException">electronic is a null 
        /// reference.</exception>
        public ElectronicBingoProduct(ElectronicBingoProduct electronic)
            : base(electronic)
        {
            m_compatibleDevices = electronic.m_compatibleDevices;
        }
        #endregion

        #region Member Methods
        /// <summary>
        /// Determines whether two ElectronicBingoProduct instances are equal.
        /// </summary>
        /// <param name="obj">The ElectronicBingoProduct to compare with the 
        /// current ElectronicBingoProduct.</param>
        /// <returns>true if the specified ElectronicBingoProduct is equal to 
        /// the current ElectronicBingoProduct; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            ElectronicBingoProduct product = obj as ElectronicBingoProduct;

            if(product == null) 
                return false;
            else
                return Equals(product);
        }

        // Rally US738
        /// <summary>
        /// Serves as a hash function for a ElectronicBingoProduct. 
        /// GetHashCode is suitable for use in hashing algorithms and data
        /// structures like a hash table. 
        /// </summary>
        /// <returns>A hash code for the current 
        /// ElectronicBingoProduct.</returns>
        public override int GetHashCode()
        {
            return (base.GetHashCode() ^ m_compatibleDevices.GetHashCode());
        }

        // Rally US738
        /// <summary>
        /// Determines whether an ElectronicBingoProduct and a BingoProduct
        /// instance are equal.
        /// </summary>
        /// <param name="other">The Product to compare with the 
        /// current ElectronicProduct.</param>
        /// <returns>true if the specified Product is equal to the current 
        /// ElectronicProduct; otherwise, false.</returns>
        public override bool Equals(BingoProduct other)
        {
            return Equals((object)other);
        }

        /// <summary>
        /// Determines whether two ElectronicBingoProduct instances are equal.
        /// </summary>
        /// <param name="other">The ElectronicBingoProduct to compare with the 
        /// current ElectronicBingoProduct.</param>
        /// <returns>true if the specified ElectronicBingoProduct is equal to 
        /// the current ElectronicBingoProduct; otherwise, false.</returns>
        public bool Equals(ElectronicBingoProduct other)
        {
            return (other != null &&
                    base.Equals(other) &&
                    m_compatibleDevices == other.m_compatibleDevices);
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public override object Clone()
        {
            return new ElectronicBingoProduct(this);
        }
        #endregion

        #region Member Properties
        /// <summary>
        /// Gets or sets a bit packed field representing which devices this 
        /// product can be sold to.
        /// </summary>
        public CompatibleDevices CompatibleDevices
        {
            get
            {
                return m_compatibleDevices;
            }
            set
            {
                m_compatibleDevices = value;
            }
        }
        #endregion
    }
}
