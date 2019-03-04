#region Copyright
// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2008 GameTech
// International, Inc.
#endregion

// US2826 Adding support for being able to sell barcoded paper 
//US3509 (US4428) POS: Validate a pack of paper
//US4321: (US4319) Discount based on quantity
//US5117: POS: Automatically add package X when package Y has been added Z times

using System;
using System.Windows.Forms;
using System.Globalization;
using System.Collections.Generic;
using GTI.Modules.Shared;
using GTI.Modules.POS.UI;
using GTI.Modules.POS.Data;
using GTI.Modules.POS.Properties;

namespace GTI.Modules.POS.Business
{
    /// <summary>
    /// Represents a grouping of products.
    /// </summary>
    internal class Package : IEquatable<Package>
    {
        #region Member Variables

        protected int m_id;
        protected string m_displayText;
        protected string m_receiptText;
        protected bool m_chargeDeviceFee; // US2018
        protected bool m_useAltPrice;
        protected bool m_isSessionDefaultValidationPackage;
        protected List<Product> m_products = new List<Product>();

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the Package class.
        /// </summary>
        public Package()
        {
        }

        /// <summary>
        /// Initializes a new instance of the Package class from an 
        /// existing instance.
        /// </summary>
        /// <param name="package">The existing instance.</param>
        /// <exception cref="System.ArgumentNullException">package is a null
        /// reference.</exception>
        public Package(Package package)
        {
            if (package == null)
                throw new ArgumentNullException("package");

            m_id = package.m_id;
            m_displayText = package.m_displayText;
            m_receiptText = package.m_receiptText;
            m_chargeDeviceFee = package.m_chargeDeviceFee; // US2018
            OverrideValidation = package.OverrideValidation;
            OverrideValidationQuantity = package.OverrideValidationQuantity;
            DiscountDescription = package.DiscountDescription;//US5117
            AppliedDiscountId = package.AppliedDiscountId;//US5117
            DiscountAmount = package.DiscountAmount;//US5117
            RequiresValidation = package.RequiresValidation;
        }

        #endregion

        #region Member Methods

        /// <summary>
        /// Determines whether two Package instances are equal. 
        /// </summary>
        /// <param name="obj">The Package to compare with the 
        /// current Package.</param>
        /// <returns>true if the specified Package is equal to the current 
        /// Package; otherwise, false. </returns>
        public override bool Equals(object obj)
        {
            Package package = obj as Package;

            if (package == null)
                return false;
            else
                return Equals(package);
        }

        /// <summary>
        /// Serves as a hash function for a Package. 
        /// GetHashCode is suitable for use in hashing algorithms and data
        /// structures like a hash table. 
        /// </summary>
        /// <returns>A hash code for the current Package.</returns>
        public override int GetHashCode()
        {
            // Rally US738 / US2018
            int hash = m_id.GetHashCode() ^ m_chargeDeviceFee.GetHashCode();

            if (!string.IsNullOrEmpty(m_displayText))
                hash ^= m_displayText.GetHashCode();

            if (!string.IsNullOrEmpty(m_receiptText))
                hash ^= m_receiptText.GetHashCode();

            foreach (Product product in m_products)
            {
                hash ^= product.GetHashCode();
            }

            return hash;
        }

        /// <summary>
        /// Determines whether two Package instances are equal. 
        /// </summary>
        /// <param name="other">The Package to compare with the 
        /// current Package.</param>
        /// <returns>true if the specified Package is equal to the current 
        /// Package; otherwise, false. </returns>
        public bool Equals(Package other)
        {
            bool equal = false;

            // Are all the members equal?
            equal = (other != null &&
                     m_id == other.m_id &&
                     m_displayText == other.m_displayText &&
                     m_receiptText == other.m_receiptText &&
                     m_chargeDeviceFee == other.m_chargeDeviceFee) && // US2018
                     UseAltPrice == other.UseAltPrice &&
                     AppliedDiscountId == other.AppliedDiscountId && //US5117
                     DiscountDescription == other.DiscountDescription; //US4543

            // Do we have the same number of products?
            if (m_products.Count != other.m_products.Count)
                equal = false;

            // Check all the products.
            if (equal)
            {
                for (int x = 0; x < m_products.Count; x++)
                {
                    if (!m_products[x].Equals(other.m_products[x]))
                    {
                        equal = false;
                        break;
                    }
                }
            }

            return equal;
        }

        /// <summary>
        /// Determines whether two Package instances are equal without regard
        /// for any discount or alt price.  Used for menu button quantity display option. 
        /// </summary>
        /// <param name="other">The Package to compare with the current Package.</param>
        /// <returns>true if the specified Package is equal to the current 
        /// Package; otherwise, false. </returns>
        public bool KindOfEquals(Package other)
        {
            bool equal = false;

            // Are all the members equal?
            equal = other != null &&
                    m_id == other.m_id &&
                    m_displayText == other.m_displayText &&
                    m_receiptText == other.m_receiptText &&
                    m_chargeDeviceFee == other.m_chargeDeviceFee;            

            // Do we have the same number of products?
            if (m_products.Count != other.m_products.Count)
                equal = false;

            // Check all the products.
            if (equal)
            {
                for (int x = 0; x < m_products.Count; x++)
                {
                    if (!m_products[x].Equals(other.m_products[x]))
                    {
                        equal = false;
                        break;
                    }
                }
            }

            return equal;
        }

        /// <summary>
        /// Adds a product to the package.
        /// </summary>
        /// <param name="product">The product to add.</param>
        public void AddProduct(Product product)
        {
            if (m_products.Contains(product))
                throw new ArgumentException(Resources.DuplicateProducts, "product");

            m_products.Add(product);
        }

        /// <summary>
        /// Creates copies and adds all products in the specified package to 
        /// the current instance.  If a product is marked as optional, then 
        /// the user will be prompted whether to add it or not.
        /// </summary>
        /// <param name="package">The existing instance.</param>
        /// <param name="owner">Any object that implements IWin32Window 
        /// that represents the top-level window that will own any modal 
        /// dialog boxes.</param>
        /// <param name="displayMode">The display mode used if prompting is 
        /// needed.</param>
        /// <exception cref="System.ArgumentNullException">package is a null
        /// reference.</exception>
        public void CloneProducts(Package package, IWin32Window owner, PointOfSale parent)
        {
            if (package == null)
                throw new ArgumentNullException("package");

            // Clone all products in the package (prompt for optional ones).
            foreach (Product product in package.m_products)
            {
                bool add = true;
                Product clone = (Product)product.Clone();

                if (clone.Optional)
                {
                    // Ask the user to add.
                    // Rally DE490
                    if (
                        POSMessageForm.Show(owner, parent,
                            string.Format(CultureInfo.CurrentCulture, Resources.OptionalProductPrompt, clone.Name,
                            clone.TotalPrice.ToString("C"), POSMessageFormTypes.YesNo_DefNO, 0)) == DialogResult.Yes)
                    {
                        add = true;
                        clone.Optional = false;
                    }
                    else
                        add = false;
                }

                if (add)
                    AddProduct(clone);
            }
        }

        /// <summary>
        /// Multiplies the price of all products in this package by -1.
        /// </summary>
        public void InvertPrice()
        {
            foreach (Product product in m_products)
            {
                product.Price = decimal.Negate(product.Price);
            }
        }

        /// <summary>
        /// Multiplies the points (earned and redeemed) of all products in this
        /// package by -1;
        /// </summary>
        public void InvertPoints()
        {
            foreach (Product product in m_products)
            {
                // Rally DE7530 - PointsPerDollar is always as it's defined.
                product.PointsPerProduct = decimal.Negate(product.PointsPerProduct);
                product.PointsToRedeem = decimal.Negate(product.PointsToRedeem);
            }
        }

        /// <summary>
        /// Returns all the products contained in the package.
        /// </summary>
        /// <returns>An array of products in the package.</returns>
        public Product[] GetProducts()
        {
            return m_products.ToArray();
        }

        /// <summary>
        /// Returns the names of all the products in the package.
        /// </summary>
        /// <returns>An array of product names.</returns>
        public string[] GetProductNames()
        {
            List<string> names = new List<string>();

            foreach (Product product in m_products)
            {
                names.Add(product.Name);
            }

            return names.ToArray();
        }

        /// <summary>
        /// Gets the dollars to redeem based on negative earned points.
        /// </summary>
        public decimal DollarsToRedeem(int quantity)
        {
            decimal toRedeem = 0M;

            foreach (Product product in m_products)
            {
                if (!product.Optional) //points will be negative = redeemed
                {
                    if (((product.Price < 0) != (product.PointsPerDollar < 0))) //points will be negative = redeemed
                    {
                        if (product.TotalPrice < 0) //price redeemed at (must be negative)
                            toRedeem += -product.TotalPrice * quantity;
                    }
                }
            }

            return toRedeem;
        }

        // FIX: DE2416 - Hall card limit isn't calculated correctly.
        /// <summary>
        /// Returns the total number of cards in this package per each 
        /// different game type found within each different game category.
        /// </summary>
        /// <returns>A dictionary object containing the game categories ids
        /// and dictionaries of game types and their totals.</returns>
        public Dictionary<int, Dictionary<GameType, int>> GetCardsByGameCategoryAndType()
        {
            // Find all the game categories, game types, and calculate the totals.
            Dictionary<int, Dictionary<GameType, int>> totals = new Dictionary<int, Dictionary<GameType, int>>();

            foreach (Product product in m_products)
            {
                BingoProduct bingoProd = product as ElectronicBingoProduct;

                if (bingoProd == null || bingoProd.GameCategoryId == 0)
                {
                    continue;
                }
                
                // Do we have this game category yet?
                if (!totals.ContainsKey(bingoProd.GameCategoryId))
                    totals.Add(bingoProd.GameCategoryId, new Dictionary<GameType, int>());

                Dictionary<GameType, int> gameTypeTotals = totals[bingoProd.GameCategoryId];

                // Do we have this game type yet?
                if (!gameTypeTotals.ContainsKey(bingoProd.GameType))
                    gameTypeTotals.Add(bingoProd.GameType, 0);

                gameTypeTotals[bingoProd.GameType] += (bingoProd.CardCount * bingoProd.Quantity);
            }

            return totals;
        }

        // END: DE2416

        public void SetPackInfoData(PaperBingoProduct product, string serialNumber, int auditNumber)
        {
            PaperPackInfo packInfo = new PaperPackInfo();
            packInfo.SerialNumber = serialNumber;
            packInfo.AuditNumber = auditNumber;

            product.AddPackInfo(packInfo);
        }

        public void RemovePackInfo()
        {
            foreach (PaperBingoProduct pbp in m_products.FindAll(p => p is PaperBingoProduct && ((PaperBingoProduct)p).BarcodedPaper))
            {
                pbp.RemovePackInfo();
            }
        }

        #endregion

        #region Member Properties

        /// <summary>
        /// Gets or sets the package's id.
        /// </summary>
        public int Id
        {
            get { return m_id; }
            set { m_id = value; }
        }

        /// <summary>
        /// Gets or sets the text to display for this package.
        /// </summary>
        public string DisplayText
        {
            get { return m_displayText; }
            set { m_displayText = value; }
        }

        /// <summary>
        /// Gets or sets the text to display on a receipt for this package.
        /// </summary>
        public string ReceiptText
        {
            get { return m_receiptText; }
            set { m_receiptText = value; }
        }

        /// <summary>
        /// Gets the package's price.
        /// </summary>
        public decimal Price
        {
            get
            {
                decimal price = 0M;

                foreach (Product product in m_products)
                {
                    if (!product.Optional) // PDTS 964
                        price += product.TotalPrice;
                }

                return price - DiscountAmount;
            }
        }

        /// <summary>
        /// Gets the package's prepaid price.
        /// </summary>
        public decimal PrepaidPrice
        {
            get
            {
                decimal price = 0M;

                foreach (Product product in m_products)
                {
                    if (!product.Optional && product.Prepaid)
                        price += product.TotalPrice;
                }

                return price;
            }
        }

        /// <summary>
        /// Gets if this package is validated.
        /// </summary>
        public bool IsValidated
        {
            get
            {
                return m_products.Exists(p => ((p as PaperBingoProduct) != null && (p as PaperBingoProduct).IsValidated) || ((p as ElectronicBingoProduct) != null && (p as ElectronicBingoProduct).IsValidated));
            }
        }

        /// <summary>
        /// Gets if there is at least one prepaid product in this package.
        /// </summary>
        public bool IsPrepaid
        {
            get
            {
                return m_products.Exists(p => p.Prepaid);
            }
        }

        /// <summary>
        /// Gets the rate of points a player earns for every dollar
        /// for this package.
        /// </summary>
        public decimal PointsPerDollar
        {
            get
            {
                decimal ptsPerDollar = 0M;

                foreach (Product product in m_products)
                {
                    if (!product.Optional) // PDTS 964
                        ptsPerDollar += product.TotalPointsPerDollar;
                }

                return ptsPerDollar;
            }
        }

        /// <summary>
        /// Gets the rate of points a player earns for each package.
        /// </summary>
        public decimal PointsPerPackage
        {
            get
            {
                decimal ptsPerPackage = 0M;

                foreach (Product product in m_products)
                {
                    if (!product.Optional) // PDTS 964
                        ptsPerPackage += product.TotalPointsPerProduct;
                }

                return ptsPerPackage;
            }
        }

        /// <summary>
        /// Gets the number points required to purchase this package.
        /// </summary>
        public decimal PointsToRedeem
        {
            get
            {
                decimal ptsToRedeem = 0M;

                foreach (Product product in m_products)
                {
                    if (!product.Optional) // PDTS 964
                    {
                        ptsToRedeem += product.TotalPointsToRedeem;

                        if (product.TotalPointsPerDollar < 0)
                            ptsToRedeem += -product.TotalPointsPerDollar;
                    }
                }

                return ptsToRedeem;
            }
        }

        /// <summary>
        /// Gets a bit packed field representing which devices this package 
        /// can be sold to.
        /// </summary>
        public CompatibleDevices CompatibleDevices
        {
            get
            {
                // FIX: DE2623 - Selling CBB in a package does not disable the Tracker Icon
                CompatibleDevices compatDevs = 0;

                // Mark them all on by default.
                foreach (int device in Enum.GetValues(typeof(CompatibleDevices)))
                {
                    compatDevs |= (CompatibleDevices)device;
                }

                foreach (Product product in m_products)
                {
                    IElectronicProduct electProd = product as IElectronicProduct;

                    if (electProd != null)
                        compatDevs &= electProd.CompatibleDevices;
                }
                // END: DE2623

                return compatDevs;
            }
        }

        // US2018
        /// <summary>
        /// Gets or sets whether this package causes the player to be charged a
        /// device usage fee.
        /// </summary>
        public bool ChargeDeviceFee
        {
            get { return m_chargeDeviceFee; }
            set { m_chargeDeviceFee = value; }
        }

        /// <summary>
        /// Gets whether this package contains any bingo products.
        /// </summary>
        public bool HasBingo
        {
            get
            {
                bool found = false;

                foreach (Product product in m_products)
                {
                    if (product is BingoProduct)
                    {
                        found = true;
                        break;
                    }
                }

                return found;
            }
        }

        /// <summary>
        /// Gets whether this package contains any open credit products.
        /// </summary>
        public bool HasOpenCredit
        {
            get
            {
                bool found = false;

                foreach (Product product in m_products)
                {
                    if (product.Type == ProductType.CreditRefundableOpen ||
                        product.Type == ProductType.CreditNonRefundableOpen)
                    {
                        found = true;
                        break;
                    }
                }

                return found;
            }
        }

        /// <summary>
        /// Gets whether this package contains any electronic products.
        /// </summary>
        public bool HasElectronics
        {
            get
            {
                bool found = false;

                foreach (Product product in m_products)
                {
                    if (product is IElectronicProduct)
                    {
                        found = true;
                        break;
                    }
                }

                return found;
            }
        }

        // Rally US505
        /// <summary>
        /// Gets whether this package contains any non-CBB, electronic bingo
        /// products.
        /// </summary>
        public bool HasElectronicBingo
        {
            get
            {
                bool found = false;

                foreach (Product product in m_products)
                {
                    if (product is ElectronicBingoProduct && product.Type != ProductType.CrystalBallQuickPick &&
                        product.Type != ProductType.CrystalBallHandPick && product.Type != ProductType.CrystalBallScan &&
                        product.Type != ProductType.CrystalBallPrompt)
                    {
                        found = true;
                        break;
                    }
                }

                return found;
            }
        }

        /// <summary>
        /// Gets whether this package contains any paper bingo products.
        /// </summary>
        public bool HasPaperBingo
        {
            get
            {
                bool found = false;

                foreach (Product product in m_products)
                {
                    PaperBingoProduct paperProduct = product as PaperBingoProduct;
                    if (paperProduct != null)
                    {
                        found = true;
                        break;
                    }
                }

                return found;
            }
        }

        public bool HasBarcodedPaper
        {
            get
            {
                bool rc = false;
                foreach (Product product in m_products)
                {
                    PaperBingoProduct paperProduct = product as PaperBingoProduct;
                    if (paperProduct != null && paperProduct.BarcodedPaper)
                    {
                        rc = true;
                        break;
                    }
                }

                return rc;
            }
        }

        /// <summary>
        /// Gets or sets if the package is marked in the menu button as the default validation for this session.
        /// </summary>
        public bool IsSessionDefaultValidationPackage
        {
            get
            {
                return m_isSessionDefaultValidationPackage;
            }
            set
            {
                m_isSessionDefaultValidationPackage = value;
            }
        }

        public PaperBingoProduct GetNextBarcodedPaperProductToIdentify(int packQuantity = 1)
        {
            return m_products.Find(p => p != null && p is PaperBingoProduct && ((PaperBingoProduct)p).BarcodedPaper && ((PaperBingoProduct)p).PackInfo.Count != (packQuantity * ((PaperBingoProduct)p).Quantity)) as PaperBingoProduct;
        }

        /// <summary>
        /// Updates the Pack info for the products with the products
        /// in the given package
        /// </summary>
        /// <param name="package">The source package from which to pull the data</param>
        //        public bool UpdatePackInfo(Package package)
        public PaperPackInfo? UpdatePackInfo(Package package)
        {
            foreach (Product srcProduct in package.GetProducts())
            {
                PaperBingoProduct srcPaperProduct = srcProduct as PaperBingoProduct;

                for (int x = 0; x < m_products.Count; x++)
                {
                    PaperBingoProduct destPaperProduct = m_products[x] as PaperBingoProduct;

                    if (destPaperProduct.Equals(srcPaperProduct))
                    {
                        foreach (PaperPackInfo info in srcPaperProduct.PackInfo)
                        {
                            if (destPaperProduct.AddPackInfo(info) == false)
                                return info;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Looks for the given paper pack serial/audit numbers in the pack's barcoded paper products' packs.
        /// </summary>
        /// <param name="packInfo">The paper pack info to look for.</param>
        public bool ContainsPackInfo(PaperPackInfo packInfo)
        {
            for (int x = 0; x < m_products.Count; x++)
            {
                PaperBingoProduct paperProduct = m_products[x] as PaperBingoProduct;

                if (paperProduct.PackInfo.Contains(packInfo))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Determines if all of the packinfo has been set
        /// </summary>
        public bool NeedsPackInfo(int quantity = 1)
        {
            return m_products.Exists(p => p != null && p is PaperBingoProduct && ((PaperBingoProduct)p).BarcodedPaper && ((PaperBingoProduct)p).PackInfo.Count != (quantity * ((PaperBingoProduct)p).Quantity));
        }

        /// <summary>
        /// Returns how many packs need packinfo for the given product
        /// </summary>
        public int PacksThatNeedInfo(PaperBingoProduct product, int quantity = 1)
        {
            return (quantity * product.Quantity) - product.PackInfo.Count;
        }

        /// <summary>
        /// Gets whether this package contains any Crystal Ball bingo products.
        /// </summary>
        public bool HasCrystalBall
        {
            get
            {
                bool found = false;

                foreach (Product product in m_products)
                {
                    BingoProduct bingoProd = product as BingoProduct;

                    // Rally US505
                    // Rally TA6385 - Add support for melange special games.
                    if (bingoProd != null && (bingoProd.GameType == GameType.CrystalBall ||
                                              bingoProd.GameType == GameType.PickYurPlatter))
                    {
                        found = true;
                        break;
                    }
                }

                return found;
            }
        }

        public bool OverrideValidation { get; set; }

        public int OverrideValidationQuantity { get; set; }

        public decimal PackageValidationValue { get; set; }

        public bool RequiresValidation
        {
            get;
            set;
        }

        /// <summary>
        /// Sets hte UseAltPrice flag for all products in the package
        /// </summary>
        /// <param name="isEnabled">if set to <c>true</c> [is enabled].</param>
        public bool UseAltPrice
        {
            get
            {
                return m_useAltPrice;
            }
            set
            {
                m_useAltPrice = value;

                //update alt price for all products in the package
                foreach (var product in GetProducts())
                {
                    product.UseAltPrice = m_useAltPrice;
                }
            }
        }

        //US4321
        public decimal DiscountAmount { get; set; }

        //US4321
        public string DiscountDescription { get; set; }

        //US5117
        public int AppliedDiscountId { get; set; }
        #endregion
    }

}

