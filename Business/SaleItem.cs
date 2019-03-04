#region Copyright
// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2008 GameTech
// International, Inc.
#endregion

//US4382: (US4337) POS: B3 Open sale
//US3509 (US4428) POS: Validate a pack of paper
//US4636: (US4319) POS Multiple discounts
//US4321: (US4319) Discount based on quantity

using System;
using System.Globalization;
using System.Collections.Generic;
using GTI.Modules.Shared;
using GTI.Modules.POS.Properties;
using System.Collections.ObjectModel;
using GTI.Modules.Shared.Business;
using System.Text;

namespace GTI.Modules.POS.Business
{
    /// <summary>
    /// Represents a line item in a Sale.
    /// </summary>
    internal class SaleItem : IEquatable<SaleItem>, IComparable<SaleItem>
    {
        #region Constants and Data Types
        // US2148
        protected const int SessionTextLength = 3;
        protected const int QuantityTextLength = 4;
        protected const int DisplayItemTextLength = 10;
        // PDTS 584
        protected const int DisplayPointsTextLength = 8;
        protected const int DisplayPriceTextLength = 9;
        protected const int PrintPointsTextLength = 11;
        protected const int PrintPriceTextLength = 11;
        protected const int PrintItemTextLength = 18;
        protected const int PrintItemSmallTextLength = 10;
        protected const int Spacer = 7;
        protected const string TextSplitCharacters = " ,+-$([{@=:;/";
        // END: US2148

        protected const int Precision = 2; // FIX: DE1938

        public enum SortOrderType
        {
            BottomOfList = 0,
            B3Credit = 1,
            Discount = 2,
            Coupon = 3,
            Validations = 4,
            Misc = 5,
            Concessions = 6,
            Merchandise = 7,
            Other = 8,
            CrystalBall = 9,
            MultipleTypePackage = 10,
            Paper = 11,
            Electronic = 12,
            ElectronicAndPaper = 13
        }

        #endregion

        #region Member Variables  

        private bool m_isCouponTaxable;
        protected int m_sessionPlayedId;
        protected int m_quantity;
        protected Package m_package;
        protected Discount m_discount;
        protected B3Credit m_b3Credit;
        protected PlayerComp m_coupon;
        protected List<CrystalBallCardCollection> m_cbbCards = new List<CrystalBallCardCollection>(); // Rally US505
        protected bool m_isPlayerRequired;
        private readonly ValidationPackage m_validatedPackage;
        protected int m_sortOrder = (int)SortOrderType.BottomOfList;

        #endregion

        #region Constructors


        public SaleItem(SessionInfo session, int quantity, bool isCouponTaxable, PlayerComp playerComp)
        {
            Session = session;
            m_quantity = quantity;
            m_isCouponTaxable = isCouponTaxable;
            m_coupon = playerComp;
            m_sortOrder = (int)SortOrderType.Coupon;
        }

        /// <summary>
        /// Initializes a new instance of the SaleItem class with the 
        /// specified session, package, and quantity.
        /// </summary>
        /// <param name="session">The session being sold for (0 for no 
        /// session).</param>
        /// <param name="sessionPlayedId">The database session played id being 
        /// sold for (0 for no session).</param>
        /// <param name="quantity">The number of packages being sold.</param>
        /// <param name="package">The package being sold.</param>
        /// <param name="isPlayerRequired">Whether a player is required to add 
        /// this item to the sale.</param>
        /// <param name="isValidationPackage"></param>
        /// <exception cref="System.ArgumentNullException">package is a null 
        /// reference.</exception>
        public SaleItem(SessionInfo session, int quantity, Package package, bool isPlayerRequired)
        {
            if(package == null)
                throw new ArgumentNullException("package");

            Session = session;
            m_quantity = quantity;
            m_package = package;
            m_isPlayerRequired = isPlayerRequired;
            SetSortOrder();
        }

        public SaleItem(SessionInfo session, int quantity, Package package, ValidationPackage validatedPackage)
        {
            if (package == null)
                throw new ArgumentNullException("package");

            Session = session;
            m_quantity = quantity;
            m_validatedPackage = ValidationPackage.Clone(validatedPackage);
            m_package = package;
            SetSortOrder();
        }

        // FIX: DE2957
        /// <summary>
        /// Initializes a new instance of the SaleItem class with the 
        /// specified discount.
        /// </summary>
        /// <param name="session">The session being sold for (0 for no 
        /// session).</param>
        /// <param name="sessionPlayedId">The database session played id being 
        /// sold for (0 for no session).</param>
        /// <param name="quantity">The number of discounts being sold (only
        /// applicable for fixed discounts).</param>
        /// <param name="discount">The discount being applied to 
        /// the sale.</param>
        /// <param name="isPlayerRequired">Whether a player is required to add 
        /// this item to the sale.</param>
        /// <exception cref="System.ArgumentNullException">discount is a null 
        /// reference.</exception>
        public SaleItem(SessionInfo session, int quantity, Discount discount, bool isPlayerRequired)
        {
            if(discount == null)
                throw new ArgumentNullException("discount");

            Session = session;

            if(discount is FixedDiscount)
                m_quantity = quantity;
            else
                m_quantity = 1;

            m_discount = discount;
            m_isPlayerRequired = isPlayerRequired;

            m_sortOrder = (int)SortOrderType.Discount;
        }
        // END: DE2957


        //US4382: (US4337) POS: Open sale
        /// <summary>
        /// Initializes a new instance of the <see cref="SaleItem"/> class.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="sessionPlayedId">The session played identifier.</param>
        /// <param name="quantity">The quantity.</param>
        /// <param name="credit">The credit.</param>
        public SaleItem(SessionInfo session, int quantity, B3Credit credit)
        {
            Session = session;
            m_quantity = quantity;
            m_b3Credit = credit;
            m_isPlayerRequired = false;

            m_sortOrder = (int)SortOrderType.B3Credit;
        }

        #endregion

        #region Static Methods

        public static string[] GetSortGroupNames()
        {
            return new string[] { " ", "B3 CREDITS", "DISCOUNTS", "COUPONS", "VALIDATIONS", "MISC", "CONCESSIONS", "MERCHANDISE", "OTHER", "CRYSTAL BALL", "MULTIPLE TYPE PACKAGE", "PAPER", "ELECTRONIC", "ELECTRONIC & PAPER" };
        }

        public static string FormattedPoints(decimal points, bool padIntegers = false, bool useParensOnNegatives = false, bool addPts = false)
        {
            string result = string.Empty;
            decimal tmpPoints = System.Math.Abs(points);
            bool pointsAreNegative = tmpPoints != points;
            bool pointsAreWhole = System.Math.Floor(tmpPoints) == tmpPoints;

            if (pointsAreWhole) //no decimal portion, show as integer
            {
                int nPoints = Convert.ToInt32((useParensOnNegatives ? tmpPoints : points));

                result = nPoints.ToString("D");
            }
            else //need to show decimals
            {
                result = (useParensOnNegatives ? tmpPoints : points).ToString("N", CultureInfo.CurrentCulture);
            }

            if (useParensOnNegatives)
            {
                if (pointsAreNegative)
                    result = "(" + result + ")";
                else
                    result = " " + result + " ";
            }

            if (padIntegers && pointsAreWhole)
                result = result + "   ";

            if (addPts)
            {
                if (tmpPoints == 1)
                    result = result + " pt";
                else
                    result = result + " pts";
            }
            return result;
        }

        #endregion

        #region Member Methods

        // PDTS 584
        /// <summary>
        /// Returns a string that represents the current SaleItem.
        /// </summary>
        /// <returns>A string that represents the current SaleItem.</returns>
        public override string ToString()
        {
            return ToStringForSaleScreen();
        }

        public string ToStringForSaleScreen(bool longDescription = false, bool addValidation = false)
        {
            string returnVal = string.Empty;
            string temp = string.Empty;

            int descLen = DisplayItemTextLength;

            if (longDescription)
                descLen += DisplayPointsTextLength + 1;

            // Session
            if (Session.SessionNumber != 0)
            {
                temp = Session.SessionNumber.ToString(CultureInfo.CurrentCulture);

                if (temp.Length > SessionTextLength)
                    returnVal += temp.Substring(0, SessionTextLength - 1) + "… ";
                else
                    returnVal += temp.PadLeft(SessionTextLength) + " ";
            }
            else
                returnVal += string.Empty.PadLeft(SessionTextLength) + " ";

            // Quantity
            temp = m_quantity.ToString(CultureInfo.CurrentCulture);

            if (temp.Length > QuantityTextLength)
                returnVal += temp.Substring(0, QuantityTextLength - 1) + "… ";
            else
                returnVal += temp.PadLeft(QuantityTextLength) + " ";

            // Get the name.
            if (m_package != null)
            {
                // Only display the text before a new line.
                int index = m_package.DisplayText.IndexOf(Environment.NewLine);

                if (index > 0)
                    temp = m_package.DisplayText.Substring(0, index);
                else
                    temp = m_package.DisplayText;
            }
            else if (m_discount != null && m_discount is PercentDiscount)
            {
                temp = string.IsNullOrEmpty(m_discount.Name) ? Resources.PercentDiscount : m_discount.Name;
            }
            else if (m_discount != null)
            {
                temp = string.IsNullOrEmpty(m_discount.Name) ? Resources.Discount : m_discount.Name;
            }
            else if (m_coupon != null)
            {
                temp = m_coupon.Name;
            }
            else if (IsB3Credit) //US4382: (US4337) POS: B3 Open sale
            {
                temp = m_b3Credit.Name;
            }
            else
            {
                temp = "???";
            }

            //if there is a discount amount for the package, 
            //then we add an extra line with the discount description
            if (IsPackageItem && Package.AppliedDiscountId != 0 && !string.IsNullOrEmpty(Package.DiscountDescription))
                temp = temp + "\r\n" + Package.DiscountDescription;

            temp = temp.Trim().Replace("\r\n", "\n").Replace('\r', '\n');

            List<string> extraDescriptionLines = new List<string>();
            string[] lines = temp.Split('\n');
            bool firstLine = true;
            bool splitIt = true;

            foreach (string line in lines)
            {
                splitIt = true;

                string ourLine = line.Trim();

                while(ourLine.Length > descLen && splitIt)
                {
                    //split the name
                    splitIt = false;

                    string temp2 = string.Empty;

                    for (int x = descLen; x > 0; x--)
                    {
                        if (TextSplitCharacters.Contains(Convert.ToString(ourLine[x])))
                        {
                            while(ourLine[x] != ' ' && x > 0 && TextSplitCharacters.Contains(Convert.ToString(ourLine[x-1])))
                                x--;

                            splitIt = true;

                            temp2 = ourLine.Substring(x).Trim();
                            ourLine = ourLine.Substring(0, x).Trim();

                            break;
                        }
                    }

                    if (!splitIt)
                    {
                        if (firstLine)
                        {
                            ourLine = ourLine.Substring(0, descLen - 1) + "… ";
                            temp = ourLine;
                            firstLine = false;
                        }
                        else
                        {
                            ourLine = ourLine.Substring(0, descLen - 1) + "… ";
                            ourLine = "\r\n" + "".PadLeft(SessionTextLength + QuantityTextLength + 2) + ourLine;
                            extraDescriptionLines.Add(ourLine);
                        }

                        ourLine = string.Empty;
                    }
                    else
                    {
                        if (firstLine)
                        {
                            temp = ourLine;
                            firstLine = false;
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(ourLine))
                            {
                                ourLine = "\r\n" + "".PadLeft(SessionTextLength + QuantityTextLength + 2) + ourLine;
                                extraDescriptionLines.Add(ourLine);
                            }
                        }

                        ourLine = temp2;
                    }
                }

                if (!string.IsNullOrWhiteSpace(ourLine))
                {
                    if (firstLine)
                    {
                        temp = ourLine;
                        firstLine = false;
                    }
                    else
                    {
                        ourLine = "\r\n" + "".PadLeft(SessionTextLength + QuantityTextLength + 2) + ourLine;
                        extraDescriptionLines.Add(ourLine);
                    }
                }
            }

            if (temp.Length > descLen)
                returnVal += temp.Substring(0, descLen - 1) + "… ";
            else
                returnVal += temp.PadRight(descLen) + " ";

            if (!longDescription)
            {
                // Points
                if (IsPackageItem && TotalPointsToRedeem != 0M)
                {
                    temp = FormattedPoints(TotalPointsToRedeem, true);

                    if (temp.Length > DisplayPointsTextLength)
                        returnVal += temp.Substring(0, DisplayPointsTextLength - 1) + "… ";
                    else
                        returnVal += temp.PadLeft(DisplayPointsTextLength) + " ";
                }
                else
                {
                    returnVal += string.Empty.PadLeft(DisplayPointsTextLength) + " ";
                }
            }

            // Price
            temp = (TotalPrice + (IsPackageItem && addValidation?Package.PackageValidationValue:0M)).ToString("0.00", CultureInfo.CurrentCulture);

            //do not print alt coupon price if 0.00
            if (m_coupon != null &&
                m_coupon.CouponType == PlayerComp.CouponTypes.AltPricePackage &&
                temp == "0.00")
            {
                temp = string.Empty;
            }

            if (temp.Length > DisplayPriceTextLength)
                returnVal += temp.Substring(0, DisplayPriceTextLength - 1) + "…";
            else
                returnVal += temp.PadLeft(DisplayPriceTextLength);

            foreach(string text in extraDescriptionLines)
                returnVal += text;

            return returnVal;
        }

        /// <summary>
        /// Returns an array of strings that represents the current SaleItem.
        /// </summary>       
        /// <param name="smallText">Whether this string will be 
        /// shown on a small receipt.</param>
        /// <returns>An array of strings that represents the current 
        /// SaleItem.</returns>
        public string[] ToStringForReceipt(bool smallText, decimal validationTotal = 0.0M)
        {
            // FIX: DE2546 - Change the way the receipt prints package names.
            string[] returnVal;

            if (IsPackageItem && Package.AppliedDiscountId != 0 && !string.IsNullOrEmpty(Package.DiscountDescription))
                returnVal = new string[] { " ", "   ", "   " };
            else
                returnVal = new string[] { " ", "   " };

            // END: DE2546

            string temp = string.Empty;

            //Line 1
            // Session
            if (Session.SessionNumber != 0)
            {
                temp = Session.SessionNumber.ToString(CultureInfo.CurrentCulture);

                if (temp.Length > SessionTextLength)
                    returnVal[0] += temp.Substring(0, SessionTextLength - 1) + "… ";
                else
                    returnVal[0] += temp.PadLeft(SessionTextLength) + " ";
            }
            else
            {
                returnVal[0] += string.Empty.PadLeft(SessionTextLength) + " ";
            }

            // Quantity
            temp = m_quantity.ToString(CultureInfo.CurrentCulture);

            if(temp.Length > QuantityTextLength)
                returnVal[0] += temp.Substring(0, QuantityTextLength - 1) + "… ";
            else
                returnVal[0] += temp.PadLeft(QuantityTextLength) + " ";

            // Do we need a spacer?
            if(!smallText)
                returnVal[0] += string.Empty.PadLeft(Spacer);
            
            // Points
            if(IsPackageItem && TotalPointsToRedeem != 0M)
            {
                temp = TotalPointsToRedeem.ToString("0.00", CultureInfo.CurrentCulture);

                if(temp.Length > PrintPointsTextLength)
                    returnVal[0] += temp.Substring(0, PrintPointsTextLength - 1) + "… ";
                else
                    returnVal[0] += temp.PadLeft(PrintPointsTextLength) + " ";
            }
            else
                returnVal[0] += string.Empty.PadLeft(PrintPointsTextLength) + " ";

            // Price
            temp = (validationTotal + TotalPrice).ToString("0.00", CultureInfo.CurrentCulture);

            //do not print alt coupon price if 0.00
            if (m_coupon != null &&
                m_coupon.CouponType == PlayerComp.CouponTypes.AltPricePackage &&
                temp == "0.00")
            {
                temp = string.Empty;
            }

            if(temp.Length > PrintPriceTextLength)
                returnVal[0] += temp.Substring(0, PrintPriceTextLength - 1) + "…";
            else
                returnVal[0] += temp.PadLeft(PrintPriceTextLength);

            //Line 2
            // Get the name.
            if(m_package != null)
                returnVal[1] += m_package.ReceiptText;
            else if(m_discount != null && m_discount is PercentDiscount)
                returnVal[1] += string.IsNullOrEmpty(m_discount.Name) ? Resources.PercentDiscount : m_discount.Name;
            else if(m_discount != null)
                returnVal[1] += string.IsNullOrEmpty(m_discount.Name) ? Resources.Discount : m_discount.Name;
            else if (m_coupon != null)
            { returnVal[1] += m_coupon.Name; }
            else if (IsB3Credit)
            {//US4382: (US4337) POS: B3 Open sale
                returnVal[1] += m_b3Credit.Name;
            }
            else
            {
                returnVal[1] += "???";    
            }

            //if there is a discount amount for the package, 
            //then we add an extra line with the discount description
            if (IsPackageItem && Package.AppliedDiscountId != 0 && !string.IsNullOrEmpty(Package.DiscountDescription))
                returnVal[2] += Package.DiscountDescription;

            return returnVal;
        }

        /// <summary>
        /// Returns a single line of receipt text unless the item description is too long, 
        /// then the description is continued on the next line(s).
        /// </summary>
        /// <param name="smallText"></param>
        /// <param name="validationTotal"></param>
        /// <returns>Array of receipt text.</returns>
        public string[] ToStringForReceiptWithNoRedeemedPoints(bool smallText, decimal validationTotal = 0.0M)
        {
            StringBuilder line1 = new StringBuilder(" ");
            string temp = string.Empty;

            // Session
            if (Session.SessionNumber != 0)
            {
                temp = Session.SessionNumber.ToString(CultureInfo.CurrentCulture);

                if (temp.Length > SessionTextLength)
                    line1.Append(temp.Substring(0, SessionTextLength - 1) + "… ");
                else
                    line1.Append(temp.PadLeft(SessionTextLength) + " ");
            }
            else
            {
                line1.Append(string.Empty.PadLeft(SessionTextLength) + " ");
            }

            // Quantity
            temp = m_quantity.ToString(CultureInfo.CurrentCulture);

            if (temp.Length > QuantityTextLength)
                line1.Append(temp.Substring(0, QuantityTextLength - 1) + "… ");
            else
                line1.Append(temp.PadLeft(QuantityTextLength) + " ");

            // Get the name.
            string name = string.Empty;

            if (m_package != null)
                name = m_package.ReceiptText;
            else if (m_discount != null && m_discount is PercentDiscount)
                name = string.IsNullOrEmpty(m_discount.Name) ? Resources.PercentDiscount : m_discount.Name;
            else if (m_discount != null)
                name = string.IsNullOrEmpty(m_discount.Name) ? Resources.Discount : m_discount.Name;
            else if (m_coupon != null)
                name = m_coupon.Name;
            else if (IsB3Credit)
                name = m_b3Credit.Name;
            else
                name = "???";

            //if there is a discount amount for the package, 
            //then we add an extra line with the discount description
            if (IsPackageItem && Package.AppliedDiscountId != 0 && !string.IsNullOrEmpty(Package.DiscountDescription))
                name = name + "\r\n" + Package.DiscountDescription;

            name = name.Trim().Replace("\r\n", "\n").Replace('\r', '\n');

            List<string> extraDescriptionLines = new List<string>();
            string[] lines = name.Split('\n');
            bool firstLine = true;
            bool splitIt = true;
            int itemTextLength = smallText ? PrintItemSmallTextLength : PrintItemTextLength;

            foreach (string line in lines)
            {
                splitIt = true;

                string ourLine = line.Trim();

                while (ourLine.Length > itemTextLength && splitIt)
                {
                    //split the name
                    splitIt = false;

                    temp = string.Empty;

                    for (int x = itemTextLength; x > 0; x--)
                    {
                        if (TextSplitCharacters.Contains(Convert.ToString(ourLine[x])))
                        {
                            while (ourLine[x] != ' ' && x > 0 && TextSplitCharacters.Contains(Convert.ToString(ourLine[x-1])))
                                x--;

                            splitIt = true;

                            temp = ourLine.Substring(x).Trim();
                            ourLine = ourLine.Substring(0, x).Trim();

                            break;
                        }
                    }

                    if (!splitIt)
                    {
                        if (firstLine)
                        {
                            ourLine = ourLine.Substring(0, itemTextLength - 1) + "… ";
                            name = ourLine;
                            firstLine = false;
                        }
                        else
                        {
                            ourLine = ourLine.Substring(0, itemTextLength - 1) + "… ";
                            ourLine = "".PadLeft(SessionTextLength + QuantityTextLength + 3) + ourLine;
                            extraDescriptionLines.Add(ourLine);
                        }

                        ourLine = string.Empty;
                    }
                    else
                    {
                        if (firstLine)
                        {
                            name = ourLine;
                            firstLine = false;
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(ourLine))
                            {
                                ourLine = "".PadLeft(SessionTextLength + QuantityTextLength + 3) + ourLine;
                                extraDescriptionLines.Add(ourLine);
                            }
                        }

                        ourLine = temp;
                    }
                }

                if (!string.IsNullOrWhiteSpace(ourLine))
                {
                    if (firstLine)
                    {
                        name = ourLine;
                        firstLine = false;
                    }
                    else
                    {
                        ourLine = "".PadLeft(SessionTextLength + QuantityTextLength + 3) + ourLine;
                        extraDescriptionLines.Add(ourLine);
                    }
                }
            }

            if (name.Length > itemTextLength)
                line1.Append(name.Substring(0, itemTextLength - 1) + "… ");
            else
                line1.Append(name.PadRight(itemTextLength)+" ");

            // Price
            temp = (validationTotal + TotalPrice).ToString("0.00", CultureInfo.CurrentCulture);

            //do not print alt coupon price if 0.00
            if (m_coupon != null &&
                m_coupon.CouponType == PlayerComp.CouponTypes.AltPricePackage &&
                temp == "0.00")
            {
                temp = string.Empty;
            }

            if (temp.Length > PrintPriceTextLength)
                line1.Append(temp.Substring(0, PrintPriceTextLength - 1) + "… ");
            else
                line1.Append(temp.PadLeft(PrintPriceTextLength)+" ");

            string[] returnVal;

            if (extraDescriptionLines.Count == 0) //one line
            {
                returnVal = new string[] { line1.ToString() };
            }
            else //more than one line
            {
                returnVal = new string[extraDescriptionLines.Count + 1];

                returnVal[0] = line1.ToString();

                int index = 1;

                foreach (string line in extraDescriptionLines)
                    returnVal[index++] = line;
            }

            return returnVal;
        }

        /// <summary>
        /// Returns a string that represents the current SaleItem.
        /// </summary>
        /// <returns>A string that represents the current SaleItem.</returns>
        public string ToStringForPaymentProcessing()
        {
            string returnVal = string.Empty;
            string temp = string.Empty;

            // Session
            if (Session.SessionNumber != 0)
            {
                temp = Session.SessionNumber.ToString(CultureInfo.CurrentCulture);

                returnVal += "S" + temp.PadLeft(SessionTextLength, '0') + " ";
            }
            else
            {
                returnVal += string.Empty.PadLeft(SessionTextLength) + " ";
            }

            // Get the name.
            if (m_package != null)
            {
                // Only display the text before a new line.
                int index = m_package.DisplayText.IndexOf(Environment.NewLine);

                if (index > 0)
                    temp = m_package.DisplayText.Substring(0, index);
                else
                    temp = m_package.DisplayText;
            }
            else if (m_discount != null && m_discount is PercentDiscount)
            {
                temp = string.IsNullOrEmpty(m_discount.Name) ? Resources.PercentDiscount : m_discount.Name;
            }
            else if (m_discount != null)
            {
                temp = string.IsNullOrEmpty(m_discount.Name) ? Resources.Discount : m_discount.Name;
            }
            else if (m_coupon != null)
            {
                temp = m_coupon.Name;
            }
            else if (IsB3Credit) //US4382: (US4337) POS: B3 Open sale
            {
                temp = m_b3Credit.Name;
            }
            else
            {
                temp = "???";
            }

            returnVal += temp.PadRight(DisplayItemTextLength) + " ";

            return returnVal;
        }

        // PDTS 964
        // Rally US26
        // Rally DE452
        /// <summary>
        /// Returns the names of the products in this item or null if there 
        /// are no products or if this is a discount item.
        /// </summary>
        /// <param name="forReceipt">Whether the strings returned are to be 
        /// indented for printing on a receipt.</param>
        /// <param name="showQuantityOrCardCount">Whether to include quantity
        /// or card count behind the product's name.</param>
        /// <returns>An array of product names.</returns>
        public string[] GetProductNames(bool forReceipt, bool showQuantityOrCardCount)
        {
            List<string> names = new List<string>();

            if(IsPackageItem)
            {
                if(forReceipt)
                {
                    Product[] products = m_package.GetProducts();

                    if(products != null)
                    {
                        foreach(Product product in products)
                        {
                            string temp = null;

                            if(products.Length > 1 || showQuantityOrCardCount || (products.Length == 1 && product.Name != m_package.ReceiptText))
                                temp = string.Empty.PadLeft(SessionTextLength + 1) + product.Name;

                            if(showQuantityOrCardCount && !string.IsNullOrEmpty(temp))
                            {
                                if(product is BingoProduct && ((BingoProduct)product).CardCount > 0)
                                {
                                    int totalCards = ((BingoProduct)product).CardCount * m_quantity;
                                    temp += string.Format(" ({0})", totalCards);
                                }
                                else if((product.Quantity * m_quantity) > 1)
                                {
                                    temp += string.Format(" ({0})", product.Quantity * m_quantity);
                                }
                            }

                            if(!string.IsNullOrEmpty(temp))
                                names.Add(temp);
                        }
                    }
                }
                else
                    names.AddRange(m_package.GetProductNames());
            }

            if(names.Count > 0)
                return names.ToArray();
            else
                return null;
        }

        /// <summary>
        /// Determines whether two SaleItem instances are equal. 
        /// </summary>
        /// <param name="obj">The SaleItem to compare with the 
        /// current SaleItem.</param>
        /// <returns>true if the specified SaleItem is equal to the current 
        /// SaleItem; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            SaleItem item = obj as SaleItem;

            if(item == null) 
                return false;
            else
                return Equals(item);
        }

        /// <summary>
        /// Serves as a hash function for a SaleItem. 
        /// GetHashCode is suitable for use in hashing algorithms and data
        /// structures like a hash table. 
        /// </summary>
        /// <returns>A hash code for the current SaleItem.</returns>
        public override int GetHashCode()
        {
            // Rally US738
            if(IsPackageItem)
                return m_package.GetHashCode() ^ Session.GetHashCode() ^ m_sessionPlayedId.GetHashCode();
            else // Discount
                return m_discount.GetHashCode() ^ Session.GetHashCode() ^ m_sessionPlayedId.GetHashCode();
        }

        /// <summary>
        /// Determines whether two SaleItem instances are equal.
        /// </summary>
        /// <param name="other">The SaleItem to compare with the 
        /// current SaleItem.</param>
        /// <returns>true if the specified SaleItem is equal to the current
        /// SaleItem; otherwise, false.</returns>
        public bool Equals(SaleItem other)
        {
            if (other.m_package != null && m_package != null)
            {
                if (other.IsValidationPackage && IsValidationPackage)//US3509
                {
                    return other.m_package != null &&
                           other.m_package.Equals(m_package) &&
                           other.ValidatedPackage != null &&
                           other.ValidatedPackage.Equals(ValidatedPackage) &&
                           other.Session == Session &&
                           other.m_sessionPlayedId == m_sessionPlayedId;
                }

                return other.m_package.Equals(m_package) &&
                       other.Session == Session && 
                       other.m_sessionPlayedId == m_sessionPlayedId;
            }

            if (other.m_discount != null && m_discount != null)
            {
                return other.m_discount.Equals(m_discount) &&
                        other.Session == Session && 
                        other.m_sessionPlayedId == m_sessionPlayedId;
            }
            
            if (other.m_b3Credit != null && m_b3Credit != null)
            {
                return other.m_b3Credit.Equals(m_b3Credit) &&
                       other.Session == Session && 
                       other.m_sessionPlayedId == m_sessionPlayedId;
            }

            if (other.IsCoupon && IsCoupon)
            {
                return other.Coupon.Id == Coupon.Id &&
                       other.Coupon.Name == Coupon.Name &&
                       other.Session == Session &&
                       other.m_sessionPlayedId == m_sessionPlayedId;
            }

            return false;
        }

        // FIX: DE1938
        /// <summary>
        /// Calculates the taxes of this item based on the specified tax rate.
        /// </summary>
        /// <param name="taxRate">The tax rate to use when calculating.</param>
        /// <returns>The taxes for this item.</returns>
        public decimal CalculateTaxes(decimal taxRate)
        {
            decimal taxes = 0M;

            if(IsPackageItem)
            {
                foreach(Product product in m_package.GetProducts())
                {
                    if(product.IsTaxed)
                        taxes += product.TotalPrice * (taxRate / 100M);         
                }
            }

            if (IsCoupon)
            {
                if (m_isCouponTaxable) //Is coupon pre tax
                {
                    taxes += -CouponValue * (taxRate / 100M);
                }
            }

            return decimal.Round(taxes, Precision, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Calculates the taxes for the prepaid amount of this item based on the specified tax rate.
        /// </summary>
        /// <param name="taxRate">The tax rate to use when calculating.</param>
        /// <returns>The prepaid taxes for this item.</returns>
        public decimal CalculatePrepaidTaxes(decimal taxRate)
        {
            decimal taxes = 0M;

            if (IsPackageItem)
            {
                foreach (Product product in m_package.GetProducts())
                {
                    if (product.IsTaxed && product.Prepaid)
                        taxes += product.TotalPrice * (taxRate / 100M) * Quantity;
                }
            }

            return decimal.Round(taxes, Precision, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Calculates the total taxes of this item based on the specified tax
        /// rate.
        /// </summary>
        /// <param name="taxRate">The tax rate to use when calculating.</param>
        /// <returns>The total taxes for this item.</returns>
        public decimal CalculateTotalTaxes(decimal taxRate)
        {
            return (CalculateTaxes(taxRate) * m_quantity);
        }
        // END: DE1938

        /// <summary>
        /// Calculates the total prepaid tax of this item based on the specified tax
        /// rate.
        /// </summary>
        /// <param name="taxRate">The tax rate to use when calculating.</param>
        /// <returns>The total prepaid tax for this item.</returns>
        public decimal CalculateTotalPrepaidTaxes(decimal taxRate)
        {
            return (CalculatePrepaidTaxes(taxRate) * m_quantity);
        }

        /// <summary>
        /// Adds a collection of CrystalBallCards to this SaleItem.  If this is 
        /// a discount item, nothing will happen.
        /// </summary>
        /// <param name="cardColls">An array of CrystalBallCards to add.</param>
        public void AddCrystalBallCards(IEnumerable<CrystalBallCardCollection> cardColls)
        {
            if(IsPackageItem && cardColls != null)
            {
                foreach(CrystalBallCardCollection coll in cardColls)
                {
                    // Is this category/nums req already in the list?
                    bool found = false;

                    for(int x = 0; x < m_cbbCards.Count; x++)
                    {
                        if(m_cbbCards[x].Equals(coll)) // Rally TA6385
                        {
                            found = true;
                            m_cbbCards[x].AddRange(coll);
                            break;
                        }
                    }

                    if(!found)
                        m_cbbCards.Add(coll);
                }
            }
        }

        /// <summary>
        /// Returns all Crystal Ball card collections associated with this
        /// sale item or null if it is a discount item or has no cards.
        /// </summary>
        /// <returns>A list of CrystalBallCardCollection objects or
        /// null.</returns>
        public IEnumerable<CrystalBallCardCollection> GetCrystalBallCards()
        {
            if(IsPackageItem && m_cbbCards.Count > 0)
                return m_cbbCards;
            else
                return null;
        }

        public void SetSortOrder()
        {
            SortOrderType sortOrder = SortOrderType.BottomOfList; //default to bottom of the list

            if (IsPackageItem && Package != null)
            {
                Product[] pp = Package.GetProducts();

                int paper = 0;
                int electronic = 0;
                int other = 0;
                int concessions = 0;
                int merchandise = 0;
                int crystalBall = 0;
                int misc = 0;
                int negativePriceNonBarcodedPaper = 0;
                int paperPackCount = 0;
                int validation = 0;

                foreach (Product p in pp)
                {
                    if (p.Type == ProductType.Validation)
                    {
                        validation = 1;
                    }
                    else if (p.Type == ProductType.Electronic)
                    {
                        electronic = 1;
                    }
                    else if (p.Type == ProductType.Paper)
                    {
                        paper = 1;
                        paperPackCount++;

                        if (p.AltPrice <= 0 && p.Price < 0 && p.Quantity == 1 && p.PointsPerDollar == 0 && p.PointsPerProduct == 0 && p.PointsToRedeem == 0)
                            negativePriceNonBarcodedPaper++;
                    }
                    else if (p.Type == ProductType.Concessions)
                    {
                        concessions = 1;
                    }
                    else if (p.Type == ProductType.BingoOther)
                    {
                        other = 1;
                    }
                    else if (p.Type == ProductType.Merchandise)
                    {
                        merchandise = 1;
                    }
                    else if (p.Type == ProductType.CrystalBallPrompt || p.Type == ProductType.CrystalBallHandPick || p.Type == ProductType.CrystalBallQuickPick || p.Type == ProductType.CrystalBallScan)
                    {
                        crystalBall = 1;
                    }
                    else
                    {
                        misc = 1;
                    }
                }

                if (paperPackCount == negativePriceNonBarcodedPaper) //no real paper packs
                    paper = 0;

                if (paper == 1 && electronic == 1 && other + concessions + merchandise + crystalBall + misc == 0) //only paper and electronics
                    sortOrder = SortOrderType.ElectronicAndPaper;
                else if (paper + electronic + other + concessions + merchandise + crystalBall + misc > 1) //multiple types in the package
                    sortOrder = SortOrderType.MultipleTypePackage;
                else if (paper == 1)
                    sortOrder = SortOrderType.Paper;
                else if (electronic == 1)
                    sortOrder = SortOrderType.Electronic;
                else if (crystalBall == 1)
                    sortOrder = SortOrderType.CrystalBall;
                else if (other == 1)
                    sortOrder = SortOrderType.Other;
                else if (merchandise == 1)
                    sortOrder = SortOrderType.Merchandise;
                else if (concessions == 1)
                    sortOrder = SortOrderType.Concessions;
                else if (misc == 1)
                    sortOrder = SortOrderType.Misc;
                else if (validation == 1)
                    sortOrder = SortOrderType.Validations;
            }
            else
            {
                if (IsB3Credit)
                    sortOrder = SortOrderType.B3Credit;
                else if (IsDiscount)
                    sortOrder = SortOrderType.Discount;
                else if (IsCoupon)
                    sortOrder = SortOrderType.Coupon;
            }

            m_sortOrder = (int)sortOrder;
        }

        #endregion

        #region Member Properties

        public int SortOrder
        {
            get
            {
                return m_sortOrder;
            }

            set
            {
                m_sortOrder = value;
            }
        }

        public PlayerComp Coupon
        {
            get
            {
                return m_coupon;
            }
            set
            {
                m_coupon = value;
            }
        }

        public decimal CouponValue
        {
            get
            {
                return m_coupon.Value;
            }
            set
            {
                m_coupon.Value = value;
            }
        }

        public string CouponName
        {
            get
            {
                return m_coupon.Name;
            }
            set
            {
                m_coupon.Name = value;
            }
        }

        public DateTime GamingDate { get; set; }

        // FIX: DE2957
        /// <summary>
        /// Gets or sets the quantity of packages to sell.  This value cannot 
        /// be set if the SaleItem is an open or percent discount.
        /// </summary>
        public int Quantity
        {
            get
            {
                return m_quantity;
            }
            set
            {
                if(m_discount == null || m_discount is FixedDiscount)
                    m_quantity = value;
            }
        }
        // END: DE2957

        /// <summary>
        /// Gets or sets the package for this SaleItem.  
        /// </summary>
        /// <remarks>SaleItem can only be a Package or a Discount; 
        /// it cannot be both at the same time.</remarks>
        public Package Package
        {
            get
            {
                return m_package;
            }
            set
            {
                if(value == null)
                    throw new ArgumentNullException("Package");
                else
                {
                    m_package = value;

                    m_discount = null;
                }
            }
        }

        // FIX: DE2957
        /// <summary>
        /// Gets or sets the discount for this SaleItem.
        /// </summary>
        /// <remarks>SaleItem can only be a Package or a Discount; 
        /// it cannot be both at the same time.  If an open or percent discount
        /// is set using this property, the quantity will be set to
        /// 1.</remarks>
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
                else
                {
                    m_discount = value;

                    m_package = null;

                    if(!(m_discount is FixedDiscount))
                        m_quantity = 1;
                }
            }
        }
        // END: DE2957
        
        public B3Credit B3Credit
        {
            get
            {
                return IsB3Credit ? m_b3Credit : null;
            }
        }

        /// <summary>
        /// Gets or sets whether a player is required for this sale item.
        /// </summary>
        public bool IsPlayerRequired
        {
            get
            {
                return m_isPlayerRequired;
            }
            set
            {
                m_isPlayerRequired = value;
            }
        }

        /// <summary>
        /// Gets whether this SaleItem is a Package item.
        /// </summary>
        public bool IsPackageItem
        {
            get
            {
                return (m_package != null);
            }
        }

        /// <summary>
        /// Gets whether this SaleItem is for the default validation package.
        /// </summary>
        public bool IsDefaultValidationPackage
        {
            get
            {
                return (m_package != null && Session != null && Session.ValidationPackage != null && m_package.Id == Session.ValidationPackage.PackageId);
            }
        }

        public bool IsCoupon
        {
            get
            {
                return (m_coupon != null); 
            }
        }

        public bool IsDiscount
        {
            get
            {
                return m_discount != null;
            }
        }

        public bool IsB3Credit {
            get
            {
                return m_b3Credit != null;
            }
        }

        //US3509: POS: Validate a pack of paper
        public bool IsValidationPackage
        {
            get
            {
                return m_validatedPackage != null;
            }
        }

        public ValidationPackage ValidatedPackage
        {
            get
            {
                return m_validatedPackage;
            }
        }

        public int CouponAwardID
        {
            get
            {
                return m_coupon.CompAwardId;
            }
            set
            {
                m_coupon.CompAwardId = value;
            }
        }

        public decimal TotalPrepaid
        {
            get
            {
                if (IsPackageItem)
                    return m_package.PrepaidPrice * m_quantity;
                else
                    return 0M;
            }
        }

        // FIX: DE2957
        /// <summary>
        /// Gets the total price of this SaleItem based on the package price 
        /// and quantity or discount price.
        /// </summary>
        public decimal TotalPrice
        {
            get
            {
                if (IsPackageItem)
                {
                    return m_package.Price * m_quantity;
                }

                if (IsCoupon)
                {
                    return decimal.Negate(m_coupon.Value);
                }

                if (IsB3Credit) //US4382: (US4337) POS: B3 Open sale
                {
                    return m_b3Credit.Amount * m_quantity;
                }

                if (IsDiscount)
                {
                    return m_discount.CalculateTotal() * m_quantity;
                }

                return 0;

            }
        }

        /// <summary>
        /// Gets the price paid for one (handles AltPrice).
        /// </summary>
        public decimal PricePaid
        {
            get
            {
                if (IsPackageItem)
                {
                    return m_package.Price;
                }

                if (IsCoupon)
                {
                    return decimal.Negate(m_coupon.Value);
                }

                if (IsB3Credit) //US4382: (US4337) POS: B3 Open sale
                {
                    return m_b3Credit.Amount;
                }

                if (IsDiscount)
                {
                    return m_discount.CalculateTotal();
                }

                return 0;
            }
        }

        /// <summary>
        /// Gets the total points earned for this SaleItem per dollar based on 
        /// the package and quantity or discount price.
        /// </summary>
        public decimal TotalPointsPerDollar
        {
            get
            {
                if (IsPackageItem)
                {
                    decimal earned = m_package.PointsPerDollar * m_quantity;

                    return (earned < 0 ? 0 : earned);
                }
                
                if (IsDiscount)
                {
                    return m_discount.CalculatePointsPerDollar() * m_quantity;
                }

                return 0.00M;
            }
        }
        // END: DE2957

        /// <summary>
        /// Gets the total points earned for this SaleItem based on quantity.
        /// </summary>
        public decimal TotalPointsPerItem
        {
            get
            {
                if (IsPackageItem)
                    return m_package.PointsPerPackage * m_quantity;

                return 0M;
            }
        }

        /// <summary>
        /// Gets the total points earned for this SaleItem.
        /// </summary>
        public decimal TotalPointsEarned
        {
            get
            {
                if (IsPackageItem)
                    return TotalPointsPerDollar + TotalPointsPerItem;

                return TotalPointsPerDollar;
            }
        }

        /// <summary>
        /// Get the total points needed to purchase this SaleItem.
        /// </summary>
        public decimal TotalPointsToRedeem
        {
            get
            {
                if (IsPackageItem)
                    return m_package.PointsToRedeem * m_quantity;

                return 0M;
            }
        }
        #endregion

        public int CompareTo(SaleItem other)
        {
            return Session.CompareTo(other.Session);
        }

        public SessionInfo Session { get; set; }
    }

    // Rally US505
    /// <summary>
    /// A list of Crystal Ball cards linked with a pick count (numbers
    /// required) and game category id.
    /// </summary>
    internal class CrystalBallCardCollection : List<CrystalBallCard>, IEquatable<CrystalBallCardCollection>
    {
        #region Member Variables
        protected int m_gameCategoryId;
        protected short m_numbersRequired;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the CrystalBallCardCollection class.
        /// </summary>
        /// <param name="gameCategoryId">The id of the game category in which
        /// these CBB cards are to be sold.</param>
        /// <param name="numbersRequired">The numbers required to be chosen for
        /// the face of the cards.</param>
        public CrystalBallCardCollection(int gameCategoryId, short numbersRequired)
        {
            m_gameCategoryId = gameCategoryId;
            m_numbersRequired = numbersRequired;
        }
        #endregion

        #region Member Methods
        /// <summary>
        /// Determines whether two CrystalBallCardCollection instances are
        /// equal. 
        /// </summary>
        /// <param name="obj">The CrystalBallCardCollection to compare with the 
        /// current CrystalBallCardCollection.</param>
        /// <returns>true if the specified CrystalBallCardCollection is equal
        /// to the current CrystalBallCardCollection; otherwise,
        /// false.</returns>
        public override bool Equals(object obj)
        {
            CrystalBallCardCollection coll = obj as CrystalBallCardCollection;

            if(coll == null)
                return false;
            else
                return Equals(coll);
        }

        /// <summary>
        /// Serves as a hash function for a CrystalBallCardCollection. 
        /// GetHashCode is suitable for use in hashing algorithms and data
        /// structures like a hash table. 
        /// </summary>
        /// <returns>A hash code for the current
        /// CrystalBallCardCollection.</returns>
        public override int GetHashCode()
        {
            return (m_gameCategoryId.GetHashCode() ^ m_numbersRequired.GetHashCode());
        }

        /// <summary>
        /// Determines whether two CrystalBallCardCollection instances are
        /// equal.
        /// </summary>
        /// <param name="other">The CrystalBallCardCollection to compare with
        /// the current CrystalBallCardCollection.</param>
        /// <returns>true if the specified CrystalBallCardCollection is equal
        /// to the current CrystalBallCardCollection; otherwise,
        /// false.</returns>
        public bool Equals(CrystalBallCardCollection other)
        {
            return (m_gameCategoryId == other.m_gameCategoryId && m_numbersRequired == other.m_numbersRequired);
        }
        #endregion

        #region Member Properties
        /// <summary>
        /// Gets or sets the id of the game category in which these CBB cards
        /// are to be sold.
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
        /// Gets or sets the numbers required to be chosen for the face of the
        /// cards.
        /// </summary>
        public short NumbersRequired
        {
            get
            {
                return m_numbersRequired;
            }
            set
            {
                m_numbersRequired = value;
            }
        }
        #endregion
    }
}
