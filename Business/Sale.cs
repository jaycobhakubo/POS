#region Copyright
// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2008 GameTech
// International, Inc.
//
// US4028 Adding support for checking card counts when each product is
//  added
#endregion

// US1955
// US4119 (US4101) Set PIN number >  Sale with player card and PIN has not been set.
// US4120 (ND) Add setting for Player PIN Required
// US2826 Adding support for being able to sell barcoded paper
// US3996 Don't add barcoded paper products to a repeat last sale
// US4027 Add the serial number and pack number to the receipt
//US4382: (US4337) POS: B3 Open sale
//US4458: (US4428) POS: Display validation status on receipt
//US4636: (US4319) POS Multiple discounts
//DE12930: POS: Coupon is not removed from the transaction
//  transaction
//US3509 (US4428) POS: Validate a pack of paper
//DE12968: Error found in US4615: (US4319) POS support max discount
//US4720: Product Center > Coupons: Award coupons automatically
//DE12999: POS: Error when selling product types Concessions, Merchandise, Bingo Other, Pull Tabs
//US4321: (US4319) Discount based on quantity

using System;
using System.Threading;
using System.Globalization;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using GTI.Modules.Shared;
using GTI.Modules.POS.UI;
using GTI.Modules.POS.Data;
using GTI.Modules.POS.Properties;
using GTI.Modules.UnitManagement.Business;
using GTI.Modules.Shared.Business;
using GTI.Modules.Shared.Data;
using System.Text;

namespace GTI.Modules.POS.Business
{
    /// <summary>
    /// Represents a sale made at the POS.
    /// </summary>
    internal class Sale
    {
        #region Constants and Data Types
        public const int MaxQuantity = 9999; // US2148
        protected const int Precision = 2; // Rally TA7465

        public class ExtraPointsBySession
        {
            public SessionInfo Session;
            public int PointMultiplier = 1;
            public decimal QualifyingAmount = 0;
            public decimal Discount = 0;
        }

        public class PaperPackInformation
        {
            public int Session;
            public string SerialNumber;
            public int AuditNumber;
            public string PackageName;
            public string ProductName;
        }
        #endregion

        #region Member Variables
        protected object m_syncRoot = new object();
        protected PointOfSale m_parent;
        protected int m_bankId; // FIX: DE1930
        protected short m_quantity = 1; // PDTS 571
        protected int m_id;
        protected bool m_isReturn;
        protected int m_receiptNum;
        protected int m_soldFromMachineId;
        protected DateTime m_gamingDate;
        protected DateTime m_transactionDate;
        protected Staff m_cashier;
        protected int m_packNumber;
        protected Device m_device;
        protected short m_unitNumber;
        protected string m_serialNumber;
        protected Player m_player;
        protected bool m_needPlayerCardPIN = false;
        protected bool m_machineSale;
        protected decimal m_preSalePlayerPts;
        protected List<SaleItem> m_items = new List<SaleItem>();
        protected List<TenderItem> m_tenders = new List<TenderItem>();
        protected decimal m_deviceFee;
        protected decimal m_taxRate;
        protected decimal m_amountTendered;
        protected BingoSession[] m_sessions;
        protected bool m_saveCBBFavorites; // Rally US507
        protected PrePrintedPackInfo m_prePrintedInfo; // Rally US510
        protected int m_successfulSales; // FIX: DE2538
        // Rally TA7465
        protected Currency m_saleCurrency;
        protected TenderItem m_quantitySaleTenderItem;
        protected bool m_patronDoesNotWantToUsePlayerCard = false;
        #endregion

        #region Constructors
        // FIX: DE1930
        /// <summary>
        /// Initializes a new instance of the Sale class.
        /// </summary>
        /// <param name="parent">The PointOfSale instance to which this sale 
        /// belongs.</param>
        /// <param name="machineSale">true if this sale could potentially be 
        /// sold to a machine instead of a player account.</param>
        /// <param name="gamingDate">The date for this Sale.</param>
        /// <param name="isReturn">Whether this Sale is a return.</param>
        /// <param name="taxRate">The tax rate for this Sale.</param>
        /// <param name="deviceFee">The device fee for this Sale.</param>
        /// <param name="machineId">The id of the machine that made the 
        /// sale.</param>
        /// <param name="cashier">The staff who made the sale.</param>
        /// <param name="bankId">The id of the bank this sale is made
        /// with.</param>
        /// <param name="saleCurrency">The currency used to make this
        /// sale.</param>
        /// <exception cref="System.ArgumentNullException">saleCurrency is a
        /// null reference.</exception>
        public Sale(PointOfSale parent, bool machineSale, DateTime gamingDate, bool isReturn, decimal taxRate, decimal deviceFee, int machineId, Staff cashier, int bankId, Currency saleCurrency) 
        {
            m_parent = parent;
            m_machineSale = machineSale; // TTP 50114
            m_gamingDate = gamingDate;
            m_isReturn = isReturn;
            m_taxRate = taxRate;
            m_deviceFee = deviceFee;
            m_soldFromMachineId = machineId;
            m_cashier = cashier;
            m_bankId = bankId;
            SaleCurrency = saleCurrency;
        }
        // END: TA7465
        // END: DE1930
        #endregion

        #region Member Methods

        public int AddItem(SessionInfo session, int quantity, PlayerComp playerComp)
        {
            int index;
            SaleItem newItem = new SaleItem(session, quantity, m_parent.Settings.CouponTaxable, playerComp);

            if (playerComp.CouponType == PlayerComp.CouponTypes.AltPricePackage) //put this coupon under its package
            {
                index = m_items.ToList().FindIndex(item => item.IsPackageItem && item.Package.Id == playerComp.PackageID && item.Package.UseAltPrice) + 1;

                m_items.Insert(index, newItem);
            }
            else
            {
                m_items.Add(newItem);
                index = m_items.Count - 1;
            }

            return index;
        }

        /// <summary>
        /// Adds a tender to this sale
        /// </summary>
        /// <param name="tenderItem"></param>
        internal void AddTender(TenderItem tenderItem)
        {
            m_tenders.Add(tenderItem);
        }

        /// <summary>
        /// Adds a package to a sale.
        /// </summary>
        /// <param name="session">The session this package is for or 0 for no 
        /// session.</param>
        /// <param name="sessionPlayedId">The database session played id this 
        /// package is for or 0 for no session.</param>
        /// <param name="quantity">The number of packages to be added
        /// (a negative or 0 amount will have no affect).</param>
        /// <param name="package">The package to be added.</param>
        /// <param name="isPlayerRequired">Whether a player is required in 
        /// order to add the item to the sale.</param>
        /// <param name="cbbCards">A list of CrystalBallCardCollection objects
        /// that are associated with this package.</param>
        /// <param name="alwaysAddNewLineItem"></param>
        /// <param name="gamingDate">Gaming date of sales item</param>
        /// <returns>The line item of the package just added or -1 if it 
        /// was removed.</returns>
        /// <exception cref="GTI.Modules.POS.Business.POSException">The package 
        /// cannot be added with the current player or max card limit has been 
        /// reached.  The description of the exception will state the 
        /// reason.</exception>
        public int AddItem(SessionInfo session, int quantity, Package package, bool isPlayerRequired, IEnumerable<CrystalBallCardCollection> cbbCards, bool alwaysAddNewLineItem = false, DateTime? gamingDate = null)
        {
            if(package == null) //no package to add
                throw new ArgumentNullException("package");

            if(session == null) //no session to add to
                throw new ArgumentNullException("session");

            if (quantity <= 0)
                return -1;

            // US4028
            // Only do this check if we are checking card counts when adding each package
            if (m_parent.Settings.CheckCardCountsPerProduct)
            {
                //US5328: Added support for game category max card limit
                // FIX: DE2416
                // Check to see if we can add this package based on operator card limit.
                Dictionary<int, Dictionary<GameType, int>> packageTotals = package.GetCardsByGameCategoryAndType();
                Dictionary<int, Dictionary<GameType, int>> sessionTotals = CalculateNumCardsBySession(session);
                Dictionary<int, int> saleCardCountTotalPerGameCategory = new Dictionary<int, int>(); //US5328

                //go through all the session totals and get a total card count per game category
                foreach (var categoryPair in sessionTotals)
                {
                    var totalCardCountPerCategory = categoryPair.Value.Sum(gameTypeTotal => gameTypeTotal.Value);

                    saleCardCountTotalPerGameCategory.Add(categoryPair.Key, totalCardCountPerCategory);
                }

                // Make sure the number of cards per game category and type doesn't go over the limit.
                foreach (KeyValuePair<int, Dictionary<GameType, int>> categoryPair in packageTotals)
                {
                    var gameCategory = session.GameCategoryList.FirstOrDefault(g => g.Id == categoryPair.Key);

                    if (gameCategory == null)
                        throw new POSException(string.Format("Unable to find game category ID {0}", categoryPair.Key));

                    var categoryTotal = 0;

                    foreach (KeyValuePair<GameType, int> typePair in categoryPair.Value)
                    {
                        // Modify the totals by the quantity they want to add.
                        //int totalInSession = 0;
                        //int totalToAdd = (typePair.Value * quantity);
                        categoryTotal += typePair.Value*quantity;

                        // Find how many are already in the session.
                        if (sessionTotals.ContainsKey(categoryPair.Key))
                        {
                            if (sessionTotals[categoryPair.Key].ContainsKey(typePair.Key))
                                categoryTotal += sessionTotals[categoryPair.Key][typePair.Key];
                        }
                    }

                    //add to the sales card count total
                    if (saleCardCountTotalPerGameCategory.ContainsKey(categoryPair.Key))
                        saleCardCountTotalPerGameCategory[categoryPair.Key] = categoryTotal; //update existing
                    else
                        saleCardCountTotalPerGameCategory.Add(categoryPair.Key, categoryTotal); //add new category

                    //get the default max card limit per session/global setting using the smaller of the two
                    var defaultMaxCardLimit = Math.Min(session.SessionMaxCardLimit, m_parent.Settings.MaxCardLimit);

                    //if game category max card limit is zero, then use default max card limit.
                    //Don't let the default limit be exceeded.
                    var maxCardLimit = gameCategory.MaxCardLimit == 0 || gameCategory.MaxCardLimit > defaultMaxCardLimit ? defaultMaxCardLimit : gameCategory.MaxCardLimit;

                    //check to make sure the game category limit is not exceeded
                    if (categoryTotal > maxCardLimit)
                        throw new POSException(string.Format(Resources.MaxCardLimitReached, gameCategory.Name));
                }

                //US5328 verify game max card limit totals
                m_parent.VerifyGameMaxCardTotals(saleCardCountTotalPerGameCategory); //should never fail since we don't allow the game category card limit to be exceeded
            }
            // END: DE2416

            // Make the prices and points negative if it's a return.
            if(m_isReturn)
            {
                package.InvertPrice();
                package.InvertPoints();
            }

            // Does this package require a player?
            if (m_player == null && (package.PointsToRedeem != 0M || isPlayerRequired)) //we need a player
            {
                NeedPlayerCardPIN = m_parent.Settings.ThirdPartyPlayerInterfaceNeedPINForPoints;

                throw new POSException(package.DisplayText + "\r\n\r\n\r\n" + Resources.PlayerRequired);            
            }
            else if (m_player != null && package.PointsToRedeem != 0M) //we have a player and need points
            {
                if (!m_player.PointsUpToDate) //we need to get the player points for the current player from the third party system
                {
                    NeedPlayerCardPIN = m_parent.Settings.ThirdPartyPlayerInterfaceNeedPINForPoints;

                    if (NeedPlayerCardPIN)
                        m_parent.SellingForm.UpdatePlayerPoints();
                }

                // Does this player have enough points?
                decimal pointsNeededForThisPackage = quantity * package.PointsToRedeem;
                decimal adjustedPlayerPointBalance = m_player.PointsBalance - CalculateTotalPointsToRedeem();

                if (adjustedPlayerPointBalance < pointsNeededForThisPackage)
                    throw new POSException(package.DisplayText + "\r\n\r\n\r\n" + string.Format(Resources.PlayerNotEnoughPoints, pointsNeededForThisPackage, adjustedPlayerPointBalance));
            }

            // Rally US510
            if(m_parent.Settings.UsePrePrintedPacks && package.HasElectronicBingo)
                PromptForPrePrintedPackInfo(false);
            
            // Create the new item.
            SaleItem newItem = new SaleItem(session, quantity, package, isPlayerRequired);

            // Check to see if we already have a package of that type and session.
            int index = m_items.IndexOf(newItem);

            if(index != -1 && !alwaysAddNewLineItem)
            {
                // It already exists, so just add to the quantity.
                SaleItem existingItem = m_items[index];

                existingItem.Quantity += quantity;

                if(existingItem.Quantity > MaxQuantity)
                    existingItem.Quantity = MaxQuantity;

                // Have we gone below 1, if so remove.
                if(existingItem.Quantity < 1)
                {
                    m_items.RemoveAt(index);
                    index = -1;
                }
                else if (existingItem.Package.HasBarcodedPaper)
                {
                    // need to add the serial and audit numbers to the existing item
                    PaperPackInfo? info = existingItem.Package.UpdatePackInfo(package);
                    if (info.HasValue) //should not happen since entire sale duplicate check was done before getting here
                    {
                        PaperPackInfo retValue = info.Value;
                        existingItem.Quantity -= 1; //DE12739: if we have duplicates we need to subtract quantity since it was already added above
                        string msg = String.Format(Resources.DuplicateAuditNumber, retValue.AuditNumber);
                        throw new BarcodeException(msg);
                    }
                }
            }
            else
            {
                // It is a new line item so add it.
                if(quantity <= MaxQuantity)
                    newItem.Quantity = quantity;
                else
                    newItem.Quantity = MaxQuantity;

                if(newItem.Quantity > 0)
                {
                    m_items.Add(newItem);
                    index = m_items.Count - 1;
                }
                else
                    index = -1;
            }

            // Rally US505
            // Attach any Crystal Ball Cards as needed.
            if(index != -1 && cbbCards != null)
                m_items[index].AddCrystalBallCards(cbbCards);

            return index;
        }

        //US4458: (US4428) POS: Display validation status on receipt
        public int AddItem(SessionInfo session, int quantity, Package package, ValidationPackage validationPackage)
        {
            SaleItem newItem = new SaleItem(session, quantity, package, validationPackage);

            int index = m_items.IndexOf(newItem);


            //if already exist
            if (index != -1)
            {

                m_items[index].Quantity += quantity;

                //if zero quantity then remove item
                if (m_items[index].Quantity <= 0)
                {
                    m_items.RemoveAt(index);
                }
            }
            else
            {
                m_items.Add(newItem);
                index = m_items.Count - 1;
            }

            return index;
        }

        // FIX: DE2957
        /// <summary>
        /// Adds a discount to a sale.
        /// </summary>
        /// <param name="session">The session this discount is for or 0 for no 
        /// session.</param>
        /// <param name="sessionPlayedId">The database session played id this 
        /// discount is for or 0 for no session.</param>
        /// <param name="discount">The discount to add to the sale.</param>
        /// <param name="quantity">The number of discounts to be added
        /// (a negative or 0 amount will have no affect).</param>
        /// <param name="isPlayerRequired">Whether a player is required in 
        /// order to add the item to the sale.</param>
        /// <returns>The line number of the discount just added.</returns>
        /// <exception cref="System.ArgumentNullException">discount is a null 
        /// reference.</exception>
        /// <exception cref="GTI.Modules.POS.Business.POSException">The discount 
        /// cannot be added with the current player.  The description of the 
        /// exception will state the reason.</exception>
        public int AddItem(SessionInfo session, Discount discount, int quantity, bool isPlayerRequired)
        {
            if(discount == null)
                throw new ArgumentNullException("discount");

            if(m_player == null && isPlayerRequired)
                throw new POSException((m_machineSale) ? Resources.MachineRequired : Resources.PlayerRequired); // TTP 50114

            if(quantity <= 0)
                return -1;

            // Make the prices negative if it's a return.
            if(m_isReturn)
                discount.InvertAmount();

            // Set the discount's line number if it's a percentage.
            PercentDiscount percent = discount as PercentDiscount;

            if(percent != null)
            {
                percent.Parent = this;
            }
            
            // If this is a fixed discount check to see if it already exists.
            SaleItem newItem = new SaleItem(session, quantity, discount, isPlayerRequired);

            int index = -1;

            if (discount is FixedDiscount)
                index = m_items.IndexOf(newItem);

            if (discount.AwardType == DiscountItem.AwardTypes.Automatic)
            {
                for (int i = 0; i < m_items.Count; i++)
                {
                    if (m_items[i].Discount == null || m_items[i].Discount.DiscountItem == null)
                    {
                        continue;
                    }

                    if (m_items[i].Discount.DiscountItem.DiscountAwardType == DiscountItem.AwardTypes.Automatic && 
                        m_items[i].Session == session &&
                        m_items[i].Discount.DiscountItem.AdvancedType != DiscountItem.AdvanceDiscountType.Quantity)
                    {
                        index = i;
                        m_items[i] = newItem;
                        break;
                    }
                }
            }

            if (index != -1 && discount.AwardType == DiscountItem.AwardTypes.Manual)
            {
                // It already exists, so just add to the quantity.
                SaleItem existingItem = m_items[index];

                existingItem.Quantity += quantity;

                if (existingItem.Quantity > MaxQuantity)
                    existingItem.Quantity = MaxQuantity;

                // Have we gone below 1, if so remove.
                if (existingItem.Quantity < 1)
                {
                    m_items.RemoveAt(index);
                    index = -1;
                }
            }
            else if(index == -1)
            {
                // It is a new line item so add it.
                if(newItem.Quantity > MaxQuantity)
                    newItem.Quantity = MaxQuantity;

                if(newItem.Quantity > 0)
                {
                    m_items.Add(newItem);
                    index = m_items.Count - 1;

                }
                else
                    index = -1;
            }

            return index;
        }
        // END: DE2957

        //US4382: (US4337) POS: B3 Open sale

        /// <summary>
        /// Adds the B3 item.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="sessionPlayedId">The session played identifier.</param>
        /// <param name="quantity">The quantity.</param>
        /// <param name="credit">The credit.</param>
        /// <returns></returns>
        public int AddItem(SessionInfo session, int quantity, B3Credit credit)
        {
            SaleItem newItem = new SaleItem(session, quantity, credit);
            int index = m_items.IndexOf(newItem);

            //if already exist
            if (index != -1)
            {
                m_items[index].Quantity ++;
            }
            else
            {
                m_items.Add(newItem);
                index = m_items.Count - 1;
            }
            
            return index;
        }

/*        /// <summary>
        /// Adds all the line items from the passed in sale, to the current 
        /// one.
        /// </summary>
        /// <param name="sale">The sale to duplicate.</param>
        /// <param name="owner">Any object that implements IWin32Window 
        /// that represents the top-level window that will own any modal 
        /// dialog boxes.</param>
        public void DuplicateSale(Sale sale, IWin32Window owner)
        {
            List<SaleItem> packagePercentCoupons = null;

            if (m_parent.Settings.RemovePackagesWithCouponsInRepeatSale)
                packagePercentCoupons = sale.GetItems().Where(i => i.IsCoupon && i.Coupon.CouponType == PlayerComp.CouponTypes.PercentPackage).ToList();
            else
                packagePercentCoupons = new List<SaleItem>();
            
            // Add all the items (if possible).
            foreach(SaleItem item in sale.GetItems())
            {
                try
                {
                    if(item.IsPackageItem)
                    {
                        //validation packages get automatically re-added
                        //see if applied discount ID is not null. They get re-added automatically
                        if (item.IsValidationPackage || item.Package.AppliedDiscountId != 0)
                            continue;

                        if (m_parent.Settings.RemovePackagesWithCouponsInRepeatSale)
                        {
                            if (item.Package.UseAltPrice) //this goes with a coupon, skip it
                                continue;

                            SaleItem couponItem = packagePercentCoupons.Find(i => i.Coupon.PackageID == item.Package.Id);
                            
                            if (couponItem != null) //we have a coupon used on this package
                            {
                                packagePercentCoupons.Remove(couponItem);

                                if(item.Quantity == 1) //just one, skip it
                                    continue;

                                //remove the package the % coupon was for
                                item.Quantity--;
                            }
                        }

                        if (m_parent.Settings.RemovePaperInRepeatSale && item.Package.HasBarcodedPaper)
                            continue;

                        // Make a clone of the package to add to the sale.
                        Package clone = new Package(item.Package);

                        // PDTS 964
                        // Add and check for any optional products.
                        clone.CloneProducts(item.Package, owner, m_parent.Settings.DisplayMode);

                        if (clone.HasPaperBingo)
                            clone.RemovePackInfo(); //we will need new pack info

                        // If there are any Crystal Ball Bingo products we have to process 
                        // those before adding the package to the sale.
                        // Rally US505
                        IEnumerable<CrystalBallCardCollection> cbbCards = null;

                        if (clone.HasCrystalBall)
                            cbbCards = m_parent.CrystalBallManager.ProcessCrystalBall(m_parent.CurrentSale, clone, item.Quantity, owner);

                        m_parent.AddSaleItem(item.Session, item.SessionPlayedId, clone, item.Quantity, item.IsPlayerRequired, cbbCards);
                    }
                    else if (item.IsCoupon)
                    {
                        //Do not re-add coupons.
                    }
                    else if (item.IsDiscount && !m_parent.Settings.RemoveDiscountsInRepeatSale)// Discount
                    {
                        // Make a clone of the discount to add to the sale.
                        Discount clone = null;

                        if (item.Discount is FixedDiscount)
                            clone = new FixedDiscount((FixedDiscount)item.Discount);
                        else if (item.Discount is OpenDiscount)
                            clone = new OpenDiscount((OpenDiscount)item.Discount);
                        else if (item.Discount is PercentDiscount)
                        {
                            clone = new PercentDiscount((PercentDiscount)item.Discount);

                            PercentDiscount percent = clone as PercentDiscount;

                            // Clear out the existing sale and line.
                            percent.Parent = null;
                        }

                        // FIX: DE2957
                        m_parent.AddSaleItem(item.Session, item.SessionPlayedId, clone, item.Quantity, item.IsPlayerRequired);
                        // END: DE2957
                    }
                }
                catch(ServerCommException) // Rethrow the ServerCommException.
                {
                    throw;
                }
                catch(POSException) // Silently fail on other exceptions.
                {
                }

                System.Windows.Forms.Application.DoEvents();
            }

            if (!m_parent.Settings.RemovePackagesWithCouponsInRepeatSale)
            {
                //check for alt price coupons
                //if so then we need to reset the packages to use regular prices
                var coupons = sale.GetItems().Where(
                                                item => item.IsCoupon &&
                                                item.Coupon.CouponType == PlayerComp.CouponTypes.AltPricePackage);

                //iterate through all alt price coupons
                foreach (var saleItem in coupons)
                {
                    var coupon = saleItem;

                    //iterate through all packages in same session and set alt price to false
                    foreach (var package in m_items.Where(i => i.IsPackageItem && i.Session == coupon.Session))
                    {
                        //Set package to use regular price
                        package.Package.UseAltPrice = false;
                    }

                    //update percentages since subtotal is now changed
                    UpdatePercentageDiscounts(saleItem.Session);
                }
            }
        }
*/
        /// <summary>
        /// Removes an item from the sale.
        /// </summary>
        /// <param name="lineNumber">The line number of the item to 
        /// remove.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">lineNumber is 
        /// less than 0 or greater than the number of items in the 
        /// sale.</exception>
        public void RemoveItem(int lineNumber)
        {
            m_items.RemoveAt(lineNumber);

            // FIX: DE2980
            // Should we remove the pre-printed pack info?
            if(m_parent.Settings.UsePrePrintedPacks && !HasElectronicBingo)
                m_prePrintedInfo = null;
            // END: DE2980
        }

        /// <summary>
        /// Removes an item from the sale.
        /// </summary>
        /// <param name="item"></param>
        /// <exception cref="System.ArgumentOutOfRangeException">lineNumber is 
        /// less than 0 or greater than the number of items in the 
        /// sale.</exception>
        public void RemoveItem(SaleItem item)
        {
            if (!m_items.Contains(item))
            {
                return;
            }

            m_items.Remove(item);

            // FIX: DE2980
            // Should we remove the pre-printed pack info?
            if (m_parent.Settings.UsePrePrintedPacks && !HasElectronicBingo)
                m_prePrintedInfo = null;
            // END: DE2980
        }

        //US4636: (US4319) POS Multiple discounts
        /// <summary>
        /// Updates the line number for all the percentage discounts in the 
        /// sale.
        /// </summary>
        public void UpdatePercentageDiscounts(SessionInfo session)
        {
            var percentDiscounts = new List<SaleItem>();
            var saleItems = new List<SaleItem>();

            //categorize all sales items into lists
            foreach (var item in m_items.Where(item => item.Session == session))
            {
                if (item.IsDiscount)
                {
                    //only add percent discounts. We ignore fixed discounts for now
                    if (item.Discount is PercentDiscount && item.Discount.AwardType == DiscountItem.AwardTypes.Manual)
                    {
                        //manual percent discounts
                        percentDiscounts.Add(item);
                    }
                    else if (item.Discount is PercentDiscount && item.Discount.AwardType == DiscountItem.AwardTypes.Automatic)
                    {
                        //auto discounts. No need to recalculate
                        saleItems.Add(item);
                    }
                }
                else if (!item.IsCoupon)
                {
                    saleItems.Add(item);
                }
            }

            //return if no percentage discounts
            if (percentDiscounts.Count == 0)
            {
                return;
            }

            //first get total of sale items
            var total = saleItems.Where(item => item.Session == session).Sum(item => item.TotalPrice);

            var percentDiscountsDescending = percentDiscounts.OrderByDescending(item =>
            {
                var percentDiscount = item.Discount as PercentDiscount;
                return percentDiscount != null ? percentDiscount.DiscountPercentage : 0;
            });

            //calculate each percent discount
            foreach (var item in percentDiscountsDescending)
            {
                var percent = item.Discount as PercentDiscount;
                if (percent == null)
                {       
                    continue;
                }

                percent.Amount = Math.Truncate(total * percent.DiscountPercentage) / 100;

                //US4615: (US4319) POS support max discount
                //check for max amount
                if (percent.DiscountItem != null &&
                    percent.DiscountItem.MaximumDiscount != 0 &&
                    percent.DiscountItem.MaximumDiscount < percent.Amount)
                {
                    percent.Amount = percent.DiscountItem.MaximumDiscount;
                }

                total += percent.CalculateTotal();
            }
        }

        /// <summary>
        /// Gets an array of all the SaleItems in the Sale
        /// </summary>
        /// <returns>And array of sale items.</returns>
        public SaleItem[] GetItems()
        {
            return m_items.ToArray();
        }

        /// <summary>
        /// Sets the current player who is purchasing this Sale.
        /// </summary>
        /// <param name="player">The player or null if no player is purchasing 
        /// this Sale.</param>
        /// <param name="checkPoints">true if the player has to have the 
        /// miniumum of points to redeem packages in this sale; otherwise the 
        /// player will be set regardless.</param>
        /// <param name="setPreSalePoints">If true the PlayerPreSalePoints 
        /// property will be set with the player's current PointsBalance.</param>
        /// <exception cref="GTI.Modules.POS.Business.POSException">The player 
        /// is not valid.  The description of the exception will state the 
        /// reason.</exception>
        public void SetPlayer(Player player, bool checkPoints, bool setPreSalePoints)
        {
            // Check the points to redeem.
            if(checkPoints)
                ValidatePlayer(player);

            // JAN: SellingForm.cs now calls PointOfSale.ClearSale() when a player card is swipped.

            //DE12981: POS: Transaction list is not cleared when the player is changed
            //if (m_player != null && m_player.Id != player.Id)
            //{
            //    foreach (var saleItem in GetItems())
            //    {
            //        RemoveItem(saleItem);
            //    }
            //    m_parent.SetDiscountToolTip(null);
            //}

            m_player = player;

            if(setPreSalePoints)
                PlayerPreSalePoints = m_player != null? m_player.PointsBalance : 0;
        }

        /// <summary>
        /// Checks to see if the passed in player is able to purchase this Sale.
        /// </summary>
        /// <param name="player">The player to validate.</param>
        /// <exception cref="GTI.Modules.POS.Business.POSException">The player 
        /// is not valid.  The description of the exception will state the 
        /// reason.</exception>
        public void ValidatePlayer(Player player)
        {
            if(player == null)
            {
                // Since there is no player, make sure nothing requires 
                // points to redeem.
                if(CalculateTotalPointsToRedeem() != 0M)
                    throw new POSException((m_machineSale) ? Resources.MachineRequired : Resources.PlayerRequired); // TTP 50114
            }
            else
            {
                // Check to make sure the player has enough points.
                if(player.PointsBalance < CalculateTotalPointsToRedeem())
                    throw new POSException(string.Format(Resources.PlayerNotEnoughPoints, CalculateTotalPointsToRedeem(), player.PointsBalance));
            }
        }

        /// <summary>
        /// Sets whether this sale is a return or not.
        /// </summary>
        /// <param name="isReturn">true if the sale is a return; otherwise 
        /// false.</param>
        /// <param name="checkPoints">true to check the current players points 
        /// base on the new state; otherwise false.</param>
        /// <exception cref="GTI.Modules.POS.Business.POSException">The player 
        /// is not valid.  The description of the exception will state the 
        /// reason.</exception>
        public void SetIsReturn(bool isReturn, bool checkPoints)
        {
            if(m_isReturn != isReturn)
            {
                m_isReturn = isReturn;

                foreach(SaleItem item in m_items)
                {
                    if(item.IsPackageItem)
                    {
                        item.Package.InvertPrice();
                        item.Package.InvertPoints();
                    }
                    else
                    {
                        item.Discount.InvertAmount();
                    }
                }
            }

            if(checkPoints)
                ValidatePlayer(m_player);
        }

        // Rally US510
        /// <summary>
        /// Prompts the user for the pre-printed pack information.
        /// </summary>
        /// <param name="forcePrompt">Prompts for pack info even if there is
        /// existing info.</param>
        /// <exception cref="GTI.Modules.POS.Business.POSUserCancelException">
        /// The user cancelled the processing of entering the information.
        /// </exception>
        public void PromptForPrePrintedPackInfo(bool forcePrompt)
        {
            // Have we already asked?
            if(m_prePrintedInfo == null || forcePrompt)
            {
                PrePrintedPackForm packForm = null;

                try
                {
                    packForm = new PrePrintedPackForm(m_parent);

                    if(packForm.ShowDialog() == DialogResult.OK)
                        m_prePrintedInfo = packForm.PrePrintedPackInfo;
                    else
                        throw new POSUserCancelException();
                }
                finally
                {
                    if(packForm != null)
                        packForm.Dispose();
                }
            }
        }

        /// <summary>
        /// Returns the amount that has so far been tendered
        /// </summary>
        /// <returns></returns>
        public decimal CalculateAmountTendered(bool justCashTenders = false)
        {
            decimal retVal = 0.00M;

            foreach (var item in m_tenders)
            {
                if (justCashTenders)
                {
                    if (item.Type == TenderType.Cash)
                        retVal += item.Amount;
                }
                else
                {
                    retVal += item.Amount;
                }
            }

            return retVal;
        }

        /// <summary>
        /// Calculates the subtotal for the Sale.
        /// </summary>
        /// <returns>The subtotal.</returns>
        public decimal CalculateSubtotal()
        {
            decimal subtotal = 0M;

            foreach(SaleItem item in m_items)
            {
                if (item.IsCoupon && !m_parent.Settings.CouponTaxable) //if coupons are not taxable, do not include them in the subtotal
                    continue;

                subtotal += item.TotalPrice;
            }            

            return subtotal;
        }

        /// <summary>
        /// Calculates the subtotal for the prepaid amount including taxes.
        /// </summary>
        /// <returns>The prepaid amount (which includes taxes).</returns>
        public decimal CalculatePrepaidAmount()
        {
            decimal prepaidAmount = 0M;

            foreach (SaleItem item in m_items)
            {
                if (item.IsPackageItem && item.Package.IsPrepaid)
                    prepaidAmount += item.TotalPrepaid;
            }

            return prepaidAmount;
        }

        public decimal CalculatePrepaidTaxTotal()
        {
            decimal prepaidTax = 0M;

            foreach (SaleItem item in m_items)
            {
                if (item.IsPackageItem && item.Package.IsPrepaid)
                    prepaidTax += item.CalculatePrepaidTaxes(m_taxRate);
            }

            return prepaidTax;
        }

        public decimal CalculateNonTaxableCouponTotal()
        {
            decimal total = 0M;

            if (m_parent.Settings.CouponTaxable)
                return total;

            foreach (SaleItem item in m_items)
            {
                if(item.IsCoupon)
                    total += item.TotalPrice;
            }

            return total;
        }

        /// <summary>
        /// Calculates the taxes for this Sale.
        /// </summary>
        /// <returns>The total of the taxes.</returns>
        public decimal CalculateTaxes()
        {
            decimal taxes = 0M;

            // FIX: DE1938
            foreach(SaleItem item in m_items)
                taxes += item.CalculateTotalTaxes(m_taxRate);

            return taxes;
        }

        // Rally TA7465
        /// <summary>
        /// Calculates the Sale's fee amount.
        /// </summary>
        /// <returns>The fees for this sale.</returns>
        public decimal CalculateFees()
        {
            return DeviceFee;
        }

        /// <summary>
        /// Converts the specified value to a new value using the sales
        /// exchange rate.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The newly converted value.</returns>
        public decimal ApplySalesExchangeRate(decimal value)
        {
            return m_saleCurrency.ConvertFromDefaultCurrencyToThisCurrency(value);
        }

        /// <summary>
        /// Converts the specified value to a new value by removing the sales
        /// exchange rate.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The newly converted value.</returns>
        public decimal RemoveSalesExchangeRate(decimal value)
        {
            return m_saleCurrency.ConvertFromThisCurrencyToDefaultCurrency(value);
        }

        /// <summary>
        /// Calculates the Sale's grand total.
        /// </summary>
        /// <param name="applyExchangeRate">true apply the sale currency's
        /// exchange rate; otherwise false.</param>
        /// <returns>The total of this sale.</returns>
        public decimal CalculateTotal(bool applyExchangeRate)
        {
            decimal total = CalculateSubtotal() + CalculateTaxes() + CalculateFees() + CalculateNonTaxableCouponTotal() - CalculatePrepaidAmount() - CalculatePrepaidTaxTotal();

            if(applyExchangeRate)
                total = ApplySalesExchangeRate(total);

            return total;
        }

        /// <summary>
        /// Calculates the change due based on the total and amount tendered.
        /// </summary>
        /// <returns>The amount of change due.</returns>
        public decimal CalculateChange()
        {
            decimal change = (AmountTendered - CalculateTotal(false)); //in default currency
            decimal pointShift = (decimal)Math.Pow(10, Sale.Precision);

            // The change due is always in the favor of the hall, so round down.
            change = Math.Floor(change * pointShift) / pointShift;

            return change; 
        }
        // END: TA7465

        /// <summary>
        /// Calculates the total points to redeem in this sale.
        /// </summary>
        /// <returns>The total points to redeem.</returns>
        public decimal CalculateTotalPointsToRedeem()
        {
            decimal total = 0M;

            foreach (SaleItem item in m_items)
                total += item.TotalPointsToRedeem;

            return total;
        }

        /// <summary>
        /// Calculates the total dollars to redeem based on products
        /// with negative earned points.
        /// </summary>
        /// <returns>The total points to redeem.</returns>
        public decimal CalculateTotalDollarsToRedeem()
        {
            decimal total = 0M;

            foreach (SaleItem item in m_items.Where(i => i.IsPackageItem))
                total += item.Package.DollarsToRedeem(item.Quantity);

            return total;
        }

        /// <summary>
        /// Calculates the total points earned in this sale from buttons.
        /// </summary>
        /// <returns>The total points earned from buttons.</returns>
        public decimal CalculateTotalEarnedPoints()
        {
            decimal total = 0M;

            //Add up the points earned per session.
            //Discounts that remove points do so for the session sold in and remove points from points earned
            //in that session.  Points earned can not be negative.
            foreach (var session in GetSessions())
            {
                decimal sessionTotal = 0M;

                var session1 = session;
                foreach (SaleItem item in m_items.Where(i => i.Session == session1))
                    sessionTotal += item.TotalPointsEarned;

                if (sessionTotal < 0M)
                    sessionTotal = 0M;

                total += sessionTotal;
            }

            return total;
        }

        /// <summary>
        /// Calculates the amount of the sale which qualifies for points.
        /// </summary>
        /// <returns>The subtotal.</returns>
        public decimal CalculateQualifyingSubtotalForPoints()
        {
            decimal subtotal = 0M;
            decimal discountTotal = 0M;

            foreach (SaleItem item in m_items)
            {
                if (item.IsDiscount)
                {
                    discountTotal += item.Discount.CalculateTotal() * item.Quantity;
                }
                else if (item.IsCoupon && item.Coupon.CouponType == PlayerComp.CouponTypes.FixedValue)
                {
                    discountTotal -= item.Coupon.Value * item.Quantity;
                }
                else if (item.IsPackageItem)
                {
                    decimal itemCouponDiscount = 0M;
                    decimal itemQualifyingAmount = 0M;
                    Product[] products = item.Package.GetProducts();

                    //find the total percent off coupon discount for this package
                    List<SaleItem> coupons = m_items.FindAll(i => i.IsCoupon && i.Coupon.CouponType == PlayerComp.CouponTypes.PercentPackage && i.Coupon.PackageID == item.Package.Id && i.Session == item.Session);

                    foreach (SaleItem couponItem in coupons)
                        itemCouponDiscount += couponItem.Coupon.Value * couponItem.Quantity;

                    //find the total qualifying amount for this package
                    foreach (Product itemProduct in products)
                    {
                        if (itemProduct.IsQualifyingProduct)
                            itemQualifyingAmount += itemProduct.TotalPrice * item.Quantity;
                    }

                    if(m_parent.Settings.DoPointQualifyingAmountCalculationOldWay)
                    {
                        itemQualifyingAmount -= itemCouponDiscount;

                        if (!m_parent.CurrentSale.IsReturn && itemQualifyingAmount < 0)
                            itemQualifyingAmount = 0;
                    }
                    else
                    {
                        if (itemCouponDiscount != 0)
                        {
                            if (m_parent.CurrentSale.IsReturn)
                            {
                                if (itemCouponDiscount < itemQualifyingAmount)
                                    itemQualifyingAmount = 0;
                                else
                                    itemQualifyingAmount -= itemCouponDiscount;
                            }
                            else
                            {
                                if (itemCouponDiscount > itemQualifyingAmount)
                                    itemQualifyingAmount = 0;
                                else
                                    itemQualifyingAmount -= itemCouponDiscount;
                            }
                        }
                    }

                    subtotal += itemQualifyingAmount;
                }
            }

            subtotal += discountTotal;

            if (m_parent.Settings.DeviceFeesQualifyForPoints)
                subtotal += CalculateFees();

            if (!m_parent.CurrentSale.IsReturn && subtotal < 0)
                subtotal = 0;

            return subtotal;
        }

        /// <summary>
        /// Computes the additional points earned per session due to session point multipliers.
        /// </summary>
        /// <returns>Total number of additional points earned due to session point multipliers.</returns>
        public decimal CalculateAdditionalPointsFromQualifying()
        {
            decimal additionalPoints = 0;
            List<ExtraPointsBySession> additionalPointsBySession = new List<ExtraPointsBySession>();

            //find all of the non-bound discounts by session
            foreach (SaleItem item in m_items)
            {
                decimal discountTotal = 0;
                ExtraPointsBySession extra = additionalPointsBySession.Find(ep => ep.Session == item.Session);

                if (item.IsDiscount)
                    discountTotal = item.Discount.CalculateTotal() * item.Quantity;
                else if (item.IsCoupon && item.Coupon.CouponType == PlayerComp.CouponTypes.FixedValue)
                    discountTotal = item.Coupon.Value * -item.Quantity;

                if (extra != null)
                {
                    extra.Discount += discountTotal;
                }
                else
                {
                    extra = new ExtraPointsBySession();

                    extra.Session = item.Session;
                    extra.Discount = discountTotal;

                    additionalPointsBySession.Add(extra);
                }
            }

            //find the qualifying amounts and point multipliers by session
            foreach (SaleItem item in m_items)
            {
                int pointMultiplier = item.Session.PointsMultiplier;

                if (item.IsPackageItem && pointMultiplier != 1)
                {
                    decimal itemCouponDiscount = 0M;
                    decimal itemQualifyingAmount = 0M;
                    Product[] products = item.Package.GetProducts();

                    //find the total percent off coupon discount for this package
                    List<SaleItem> coupons = m_items.FindAll(i => i.IsCoupon && i.Coupon.CouponType == PlayerComp.CouponTypes.PercentPackage && i.Coupon.PackageID == item.Package.Id);

                    foreach (SaleItem couponItem in coupons)
                        itemCouponDiscount += couponItem.Coupon.Value * couponItem.Quantity;

                    //find the total qualifying amount for this package
                    foreach (Product itemProduct in products)
                    {
                        if (itemProduct.IsQualifyingProduct)
                            itemQualifyingAmount += itemProduct.TotalPrice * item.Quantity;
                    }

                    if (m_parent.Settings.DoPointQualifyingAmountCalculationOldWay)
                    {
                        //take away the % discount but don't let the qualifying amount go negative
                        itemQualifyingAmount -= itemCouponDiscount;

                        if (!m_parent.CurrentSale.IsReturn && itemQualifyingAmount < 0)
                            itemQualifyingAmount = 0;
                    }
                    else
                    {
                        if (itemCouponDiscount != 0)
                        {
                            if (m_parent.CurrentSale.IsReturn)
                            {
                                if (itemCouponDiscount < itemQualifyingAmount)
                                    itemQualifyingAmount = 0;
                                else
                                    itemQualifyingAmount -= itemCouponDiscount;
                            }
                            else
                            {
                                if (itemCouponDiscount > itemQualifyingAmount)
                                    itemQualifyingAmount = 0;
                                else
                                    itemQualifyingAmount -= itemCouponDiscount;
                            }
                        }
                    }

                    ExtraPointsBySession extra = additionalPointsBySession.Find(ep => ep.Session == item.Session);

                    if (extra != null)
                    {
                        extra.QualifyingAmount += itemQualifyingAmount;
                        extra.PointMultiplier = pointMultiplier;
                    }
                    else
                    {
                        extra = new ExtraPointsBySession
                        {
                            Session = item.Session,
                            PointMultiplier = pointMultiplier,
                            QualifyingAmount = itemQualifyingAmount
                        };


                        additionalPointsBySession.Add(extra);
                    }
                }
            }

            //add up all the additional points for the sessions with point multipliers
            decimal qualAmt = 0;

            foreach (var extraPoints in additionalPointsBySession)
            {
                if (m_parent.Settings.DoPointQualifyingAmountCalculationOldWay)
                {
                    qualAmt = extraPoints.QualifyingAmount + extraPoints.Discount;

                    if (!m_parent.CurrentSale.IsReturn && qualAmt < 0)
                        qualAmt = 0;
                }
                else
                {
                    if (extraPoints.Discount != 0)
                    {
                        if (m_parent.CurrentSale.IsReturn)
                        {
                            if (-extraPoints.Discount < extraPoints.QualifyingAmount)
                                qualAmt = 0;
                            else
                                qualAmt = extraPoints.QualifyingAmount + extraPoints.Discount;
                        }
                        else
                        {
                            if (-extraPoints.Discount > extraPoints.QualifyingAmount)
                                qualAmt = 0;
                            else
                                qualAmt = extraPoints.QualifyingAmount + extraPoints.Discount;
                        }
                    }
                }

                decimal points = Math.Sign(qualAmt)*Math.Floor(Math.Abs(qualAmt) / m_parent.Settings.ThirdPartyRatingDollars) * m_parent.Settings.ThirdPartyRatingPoints; //regular player rating points from this session
                
                additionalPoints += (points * extraPoints.PointMultiplier) - points; //extra player rating points from this session
            }

            return additionalPoints;
        }

        /// <summary>
        /// Computes the points earned from the qualifying amount spent (player rating).  If a third party
        /// casino system computes the rating, the returned value will always be zero.
        /// </summary>
        /// <returns>Points earned from qualifying amount spent (player rating).</returns>
        public decimal CalculatePointsEarnedFromQualifyingSubtotal()
        {
            decimal qualAmt = CalculateQualifyingSubtotalForPoints();

            return (System.Math.Sign(qualAmt) * System.Math.Floor(System.Math.Abs(qualAmt) / m_parent.Settings.ThirdPartyRatingDollars) * m_parent.Settings.ThirdPartyRatingPoints) + CalculateAdditionalPointsFromQualifying(); //player rating points
        }

        /// <summary>
        /// Returns a list of different sessions is the sale
        /// </summary>
        /// <returns>A List of SessionInfo</returns>
        public List<SessionInfo> GetSessions()
        {
            List<SessionInfo> sessions = new List<SessionInfo>();

            // Loop through the sale and find each different session.
            foreach(SaleItem item in m_items)
            {
                if (!sessions.Contains(item.Session))
                    sessions.Add(item.Session);
            }

            return sessions;
        }

        // FIX: DE2416
        /// <summary>
        /// Calculates the total number of cards currently in this sale per 
        /// game category and game type for all sessions.
        /// </summary>
        /// <returns>The number of cards per game type per game category per
        /// session.</returns>
        public Dictionary<SessionInfo, Dictionary<int, Dictionary<GameType, int>>> CalculateNumCards()
        {
            Dictionary<SessionInfo, Dictionary<int, Dictionary<GameType, int>>> totals = new Dictionary<SessionInfo, Dictionary<int, Dictionary<GameType, int>>>();

            foreach(SaleItem item in m_items)
            {
                if(item.IsPackageItem && item.Session != null)
                {
                    // Do we have this session?
                    if(!totals.ContainsKey(item.Session))
                        totals.Add(item.Session, new Dictionary<int, Dictionary<GameType, int>>());

                    // Get the card counts for the current package.
                    Dictionary<int, Dictionary<GameType, int>> packageTotal = item.Package.GetCardsByGameCategoryAndType();

                    // Loop through and add any cards to the appropriate game 
                    // category.
                    foreach(KeyValuePair<int, Dictionary<GameType, int>> categoryPair in packageTotal)
                    {
                        // Is this game category in this session?
                        if(!totals[item.Session].ContainsKey(categoryPair.Key))
                            totals[item.Session].Add(categoryPair.Key, new Dictionary<GameType, int>());

                        Dictionary<GameType, int> typeTotals = totals[item.Session][categoryPair.Key];

                        foreach(KeyValuePair<GameType, int> typePair in categoryPair.Value)
                        {
                            // Is this game type in this session?
                            if(!typeTotals.ContainsKey(typePair.Key))
                                typeTotals.Add(typePair.Key, 0);

                            // Add the card count.
                            typeTotals[typePair.Key] += (typePair.Value * item.Quantity);
                        }
                    }
                }
            }

            return totals;
        }

        /// <summary>
        /// Calculates the total number of cards currently in this sale per 
        /// game category and game type for a single session.
        /// </summary>
        /// <param name="session">The session to calculate cards for.</param>
        /// <returns>The number of cards per game category and game
        /// type.</returns>
        public Dictionary<int, Dictionary<GameType, int>> CalculateNumCardsBySession(SessionInfo session)
        {
            Dictionary<SessionInfo, Dictionary<int, Dictionary<GameType, int>>> totals = CalculateNumCards();

            if(totals == null || !totals.ContainsKey(session))
                return new Dictionary<int, Dictionary<GameType, int>>();
            else
                return totals[session];
        }

        /// <summary>
        /// Returns the maximum number of cards for any category/game type
        /// combination in any session.
        /// </summary>
        /// <returns>The maximum number of cards in the sale.</returns>
        public int CalculateMaxNumCards()
        {
            int maxCards = 0;
            Dictionary<SessionInfo, Dictionary<int, Dictionary<GameType, int>>> numCards = CalculateNumCards();

            foreach (KeyValuePair<SessionInfo, Dictionary<int, Dictionary<GameType, int>>> sessionPair in numCards)
            {
                foreach(KeyValuePair<int, Dictionary<GameType, int>> categoryPair in sessionPair.Value)
                {
                    foreach(KeyValuePair<GameType, int> typePair in categoryPair.Value)
                    {
                        maxCards = Math.Max(typePair.Value, maxCards);
                    }
                }
            }

            return maxCards;
        }
        // END: DE2416

        // Rally TA7729
        /// <summary>
        /// Gets if a Sale has electronic packages in it and which devices
        /// it can be sold to.
        /// </summary>
        /// <param name="hasElectronics">Whether this Sale has electronic 
        /// packages.</param>
        /// <param name="Tracker">Whether this Sale can be sold to 
        /// a Tracker.</param>
        /// <param name="Traveler">Whether this Sale can be sold to 
        /// a Traveler.</param>
        /// <param name="Fixed">Whether this Sale can be sold to 
        /// a Fixed Unit.</param>
        /// <param name="Explorer">Whether this Sale can be sold to 
        /// an Explorer.</param>
        /// <param name="Traveler2">Whether this Sale can be sold to a 
        /// Traveler II.</param>
        public void GetCompatibleDevices(out bool hasElectronics, out bool Traveler, out bool Tracker, out bool Fixed, out bool Explorer, out bool Traveler2)
        {
            hasElectronics = false;
            Traveler = true;
            Tracker = true;
            Fixed = true;
            Explorer = true;
            Traveler2 = true; // PDTS 964, Rally US765
            

            // Check each package and if that package doesn't support a device,
            // disable it.
            foreach(SaleItem item in m_items)
            {
                if(item.IsPackageItem && item.Package.HasElectronics)
                {
                    hasElectronics = true;

                    if((item.Package.CompatibleDevices & CompatibleDevices.Traveler) == 0)
                        Traveler = false;

                    if((item.Package.CompatibleDevices & CompatibleDevices.Tracker) == 0)
                        Tracker = false;

                    if((item.Package.CompatibleDevices & CompatibleDevices.Fixed) == 0)
                        Fixed = false;

                    if((item.Package.CompatibleDevices & CompatibleDevices.Explorer) == 0)
                        Explorer = false;

                    // Rally US765
                    if((item.Package.CompatibleDevices & CompatibleDevices.Traveler2) == 0)
                        Traveler2 = false;
                }
            }

            // If a Sale has no electronics, disable all devices.
            if(!hasElectronics)
            {
                Traveler = false;
                Tracker = false;
                Fixed = false;
                Explorer = false;
                Traveler2 = false; // Rally US765
            }
        }

        public bool ThisDeviceIsCompatible(Device device)
        {
            bool canSell = true;

            if (device.Id != Device.Traveler.Id && device.Id != Device.Tracker.Id && device.Id != Device.Fixed.Id && device.Id != Device.Explorer.Id && device.Id != Device.Traveler2.Id)
                return true;

            foreach (SaleItem item in m_items)
            {
                if (item.IsPackageItem && item.Package.HasElectronics)
                {
                    if ((item.Package.CompatibleDevices & CompatibleDevices.Traveler) == 0 && device.Id == Device.Traveler.Id)
                        canSell = false;

                    if ((item.Package.CompatibleDevices & CompatibleDevices.Tracker) == 0 && device.Id == Device.Tracker.Id)
                        canSell = false;

                    if ((item.Package.CompatibleDevices & CompatibleDevices.Fixed) == 0 && device.Id == Device.Fixed.Id)
                        canSell = false;

                    if ((item.Package.CompatibleDevices & CompatibleDevices.Explorer) == 0 && device.Id == Device.Explorer.Id)
                        canSell = false;

                    if ((item.Package.CompatibleDevices & CompatibleDevices.Traveler2) == 0 && device.Id == Device.Traveler2.Id)
                        canSell = false;

                    if (!canSell)
                        return false;
                }
            }

            return canSell;
        }

        /// <summary>
        /// Creates a sale on the server.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The DoWorkEventArgs object that 
        /// contains the event data.</param>
        public void SendToServer(object sender, DoWorkEventArgs e)
        {
            // Rally TA6050
            bool isMachineAccount, printQuantitySaleReceipts, mainStageMode, forcePackToPlayer;

            // Set the language and options.
            lock(m_parent.Settings.SyncRoot)
            {
                if(m_parent.Settings.ForceEnglish)
                    PointOfSale.ForceEnglish();
                else
                    Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

                isMachineAccount = m_parent.Settings.EnableAnonymousMachineAccounts;
                printQuantitySaleReceipts = m_parent.Settings.PrintQuantitySaleReceipts;
                mainStageMode = m_parent.Settings.MainStageMode;
                forcePackToPlayer = m_parent.Settings.ForcePackToPlayer;
            }

            // Let the sale status form display.
//            System.Windows.Forms.Application.DoEvents();

            // Unpackage the worker and status form.
            BackgroundWorker worker = (BackgroundWorker)sender;
            SaleStatusForm statusForm = (SaleStatusForm)e.Argument;

            POSPrintException printException = null;
            bool cashDrawerOpened = false;

            bool success = false;
            int authStaffId = 0;
            int loginNum = 0;
            string magCardNum = string.Empty;
            string password = null;
            string message = null;

            // FIX: DE2665 - CBB sales were not locked when playing a session.
            // Can the current staff override?
            bool canOverride, tryCurrentUser;

            lock(m_cashier.SyncRoot)
            {
                canOverride = m_cashier.CheckModuleFeature(EliteModule.POS, (int)POSFeature.SaleLockOverride);
                tryCurrentUser = canOverride;
            }
            // END: DE2665

            // PDTS 571
            // FIX: DE2538
            m_successfulSales = 0;

            for(int currSale = 0; currSale < m_quantity; currSale++)
            {
                string counterText = null;

                // Does the user want us to stop?
                if(worker.CancellationPending)
                    break;

                if(m_quantity > 1)
                    counterText = string.Format(CultureInfo.CurrentCulture, Resources.QuantitySaleCounter, currSale + 1, m_quantity);

                do
                {
                    // Attempt to add the sale.
                    worker.ReportProgress(0, Resources.WaitFormMakeSale + counterText);

                    m_parent.SellingForm.NotIdle();

                    if(forcePackToPlayer)
                    {
                        // Rally DE139
                        CheckPlayerMaxCards();
                    }
                    // END: TA6050

                    if (m_quantity > 1) //quantity sale, remove any receipt ID and transaction #
                    {
                        m_id = 0;
                        m_receiptNum = 0;
                    }

                    AddSaleReturnCode returnCode = AddSale(authStaffId, loginNum, magCardNum, password);

                    // Did the add succeed?
                    switch(returnCode)
                    {
                        case (AddSaleReturnCode)GTIServerReturnCode.Success:
                            success = true;
                            break;

                        // Rally DE139
                        case AddSaleReturnCode.SessionOutOfSync:
                            throw new POSException(Resources.AddSaleErrorSessionSync);

                        case AddSaleReturnCode.MissingSerialLookup:
                            throw new POSException(Resources.AddSaleErrorMissingSerial);

                        // PDTS 964
                        case AddSaleReturnCode.InsufficientCards:
                            throw new POSException(Resources.AddSaleErrorNoCards);

                        case AddSaleReturnCode.NotEnoughPoints:
                            throw new POSException(Resources.AddSaleErrorNotEnoughPoints);

                        // FIX: DE2665
                        case AddSaleReturnCode.SessionLocked:
                            if(canOverride)
                                message = Resources.AddSaleErrorSessionLocked + Environment.NewLine + Resources.TryOverride;
                            else
                                message = Resources.AddSaleErrorSessionLocked + Environment.NewLine + Resources.NotAllowed + Environment.NewLine + Resources.EnterOverride;

                            break;

                        case AddSaleReturnCode.GameLocked:
                            if(canOverride)
                                message = Resources.AddSaleErrorGameLocked + Environment.NewLine + Resources.TryOverride;
                            else
                                message = Resources.AddSaleErrorGameLocked + Environment.NewLine + Resources.NotAllowed + Environment.NewLine + Resources.EnterOverride;

                            break;

                        case AddSaleReturnCode.AuthStaffNotFound:
                        case AddSaleReturnCode.IncorrectAuthPassword:
                            message = Resources.InvalidStaff;
                            tryCurrentUser = false;
                            break;

                        case AddSaleReturnCode.InactiveAuthStaff:
                            message = Resources.StaffInactive;
                            tryCurrentUser = false;
                            break;

                        case AddSaleReturnCode.AuthPasswordHasExpired:
                            message = Resources.StaffPasswordExpired;
                            tryCurrentUser = false;
                            break;

                        case AddSaleReturnCode.NotAuthorized:
                            message = Resources.StaffNotAuthorized;
                            tryCurrentUser = false;
                            break;

                        // US1955
                        case AddSaleReturnCode.AuthStaffLocked:
                            message = Resources.StaffLocked;
                            tryCurrentUser = false;
                            break;
                        
                        default:
                            throw new POSException(string.Format(CultureInfo.CurrentCulture, Resources.AddSaleFailed, ServerExceptionTranslator.GetServerErrorMessage((int)returnCode)));
                    }

                    // We have to show the user the error and prompt for info.
                    if (!success)
                    {
                        bool locked = (returnCode == AddSaleReturnCode.SessionLocked || returnCode == AddSaleReturnCode.GameLocked);

                        if (statusForm.InvokeRequired)
                        {
                            SalePromptDelegate prompt = new SalePromptDelegate(statusForm.ShowPrompt);
                            statusForm.Invoke(prompt, new object[] { message, locked });
                        }
                        else
                            statusForm.ShowPrompt(message, locked);

                        authStaffId = 0;
                        loginNum = 0;
                        magCardNum = string.Empty;
                        password = null;

                        if (tryCurrentUser)
                            authStaffId = m_cashier.Id;
                        else
                        {
                            m_parent.MagCardReader.BeginReading();

                            if (statusForm.InvokeRequired)
                            {
                                PromptForLoginDelegate prompt = new PromptForLoginDelegate(POSSecurity.PromptForLogin);
                                object[] args = new object[] { m_parent, statusForm, loginNum, magCardNum, password };

                                statusForm.Invoke(prompt, args);

                                loginNum = (int)args[2];
                                magCardNum = (string)args[3];
                                password = (string)args[4];
                            }
                            else
                                POSSecurity.PromptForLogin(m_parent, statusForm, out loginNum, out magCardNum, out password);

                            m_parent.MagCardReader.EndReading();
                        }
                        // END: DE2665
                    }
                    else
                    {
                        if (m_quantity > 1) //quantity sale (mass sale), create "cash" tender for each transaction
                        {
                            QuantitySaleTenderItem.SaleTenderInfo.RegisterReceiptID = m_id;
                            QuantitySaleTenderItem.SaleTenderInfo.RegisterReceiptTenderID = 0;

                            //save the tender
                            SetReceiptTenderMessage tenderMsg = new SetReceiptTenderMessage(QuantitySaleTenderItem.SaleTenderInfo);

                            try
                            {
                                tenderMsg.Send();
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                } while(!success);

                m_parent.SellingForm.NotIdle();

                // Open the cash drawer now that we have a record of the sale.
                // PDTS 583
                if(!cashDrawerOpened)
                {
                    m_parent.OpenCashDrawer();
                    cashDrawerOpened = true;
                }

                // Rally US229
                // PDTS 571
                // Rally US505
                m_packNumber = 0; // FIX: DE2553 - Clear the pack number out for CBB

                // Are we doing an electronic sale?
                // US2826 Adding support for barcoded paper
                //if (HasElectronicBingo || HasBarcodedPaperBingo)

                if (HasElectronicBingo)
                {
                    m_parent.SellingForm.NotIdle();
                    AddBingoCards(mainStageMode);
                }

                m_parent.SellingForm.NotIdle();
                AddCBBBingoCards();

                if(HasElectronics)
                {
                    // Attempt to find a device to sell in the crate.
                    if (m_device.Id == Device.Traveler.Id || m_device.Id == Device.Tracker.Id)
                    {
                        m_parent.SellingForm.NotIdle();
                        SellToDevice(worker); // FIX: Rally US510, DE2596
                    }
                }

                // PDTS 964
                if (!mainStageMode && m_parent.Settings.PrintCardNumbers != PrintCardNumberMode.DoNotPrintOnReceipt)
                {
                    m_parent.SellingForm.NotIdle();
                    worker.ReportProgress(0, Resources.WaitFormGettingCards + counterText);
                    GetBingoGameCards();
                }

                // Mark the sale as valid.
                worker.ReportProgress(0, new SaleStatusForm.SaleStatusState(m_unitNumber, Resources.WaitFormFinishingSale + counterText)); // PDTS 583
                m_parent.SellingForm.NotIdle();
                List<string> afterSaleText = FinishSale(true, false);
                m_parent.SellingForm.NotIdle();

                if (HasBarcodedPaperBingo)
                {
                    var returnValue = UpdateMachinePaperUsageLevelsMessage.UpdateMachineAuditNumbers(m_id);
                    if (returnValue != 0)
                    {
                        m_parent.Log("Unable to update machine paper usage levels. Return Code: " + returnValue, LoggerLevel.Severe);
                    }
                }

                m_successfulSales++;

                if (m_parent.WeAreAPOSKiosk && m_successfulSales == 1) //give change so we can update the tenders records if needed before printing
                {
                    m_parent.SellingForm.WriteNVRAMUserDecimal(SellingForm.NVRAMUserDecimal.SaleSucceeded, 1);
                    m_parent.SellingForm.DispenseChangeForKiosk();

                    //we made it through dispensing without a power failure so there is a receipt on the server, clear our data
                    //clear the total first to indicate there is no sale problem
                    m_parent.SellingForm.WriteNVRAMUserDecimal(SellingForm.NVRAMUserDecimal.TransactionTotal, 0);
                    m_parent.SellingForm.WriteNVRAMUserDecimal(SellingForm.NVRAMUserDecimal.AmountCollected, 0);
                    m_parent.SellingForm.WriteNVRAMUserDecimal(SellingForm.NVRAMUserDecimal.AmountDispensed, 0);
                }

                // Create a receipt and print it out.
                try
                {
                    // PDTS 571
                    //PrintSaleToReceipt will generate the receipt and save receipt text to the DB and print or not based on settings (needed for pre-sales reprints).
                    worker.ReportProgress(0, new SaleStatusForm.SaleStatusState(m_unitNumber, Resources.WaitFormPrinting + counterText)); // PDTS 583
                    m_parent.SellingForm.NotIdle();
                    PrintSaleToReceipt(afterSaleText);
                }
                catch(Exception ex)
                {
                    printException = new POSPrintException(string.Format(CultureInfo.CurrentCulture, Resources.SaleButNoReceipt, ex.Message), ex);
                }

                if(m_parent.WeAreAPOSKiosk)
                    m_parent.SellingForm.ClearNVRAMTransactionUserDecimals();
            }
            // END: DE2538

            // Throw a print exception if one occurred during sale.
            if(printException != null)
                throw printException;
        }

        // Rally DE139
        /// <summary>
        /// Checks whether the the sale will exceed the maximum card limit for a player.
        /// </summary>
        private void CheckPlayerMaxCards()
        {
            // Prepare the message.
            CheckMaxCardsMessage cardsMsg = new CheckMaxCardsMessage((m_player != null) ? m_player.Id : 0);

            // Add the package quantities to the message.
            for(int x = 0; x < m_items.Count; x++)
            {
                // Rally DE1908 - Checking max card limit for game fails.
                if(m_items[x].IsPackageItem)
                    cardsMsg.AddPackageQuantity(m_items[x].Session.SessionPlayedId, m_items[x].Package.Id, m_items[x].Quantity);
            }

            // Send the message.
            try
            {
                cardsMsg.Send();
            }
            catch(ServerCommException)
            {
                throw; // Don't repackage the ServerCommException
            }
            catch(ServerException ex)
            {
                if((CheckMaxCardsReturnCode)ex.ReturnCode == CheckMaxCardsReturnCode.MaxCardsExceeded)
                    throw new POSException(Resources.AddSaleErrorMaxCards);
                else
                    throw new POSException(string.Format(CultureInfo.CurrentCulture, Resources.CheckMaxCardsFailed, ServerExceptionTranslator.FormatExceptionMessage(ex)), ex);
            }
            catch(Exception ex)
            {
                throw new POSException(string.Format(CultureInfo.CurrentCulture, Resources.CheckMaxCardsFailed, ServerExceptionTranslator.FormatExceptionMessage(ex)), ex);
            }
        }

        // Rally US505
        /// <summary>
        /// Checks to see if the sale already contains the specified Crystal
        /// Ball card.
        /// </summary>
        /// <param name="card">The card to search for.</param>
        /// <returns>true if the card was found; otherwise false.</returns>
        public bool CheckForCBBCard(CrystalBallCard card)
        {
            bool found = false;

            foreach(SaleItem item in m_items)
            {
                IEnumerable<CrystalBallCardCollection> cards = item.GetCrystalBallCards();

                if(cards != null)
                {
                    foreach(CrystalBallCardCollection coll in cards)
                    {
                        foreach(CrystalBallCard cbbCard in coll)
                        {
                            if(card.Equals(cbbCard))
                            {
                                found = true;
                                break;
                            }
                        }
                    }
                }
            }

            return found;
        }

        // Rally US507
        /// <summary>
        /// Gets how many CBB cards in this sale (for the specified pick count)
        /// are a "favorite" card.
        /// </summary>
        /// <param name="numbersRequired">The pick count type of the card
        /// needed (since CBB favorites are stored per pick count).</param>
        public int GetCBBFavoriteCount(short numbersRequired)
        {
            int favorites = 0;

            foreach(SaleItem item in m_items)
            {
                IEnumerable<CrystalBallCardCollection> cbbCards = item.GetCrystalBallCards();

                if(cbbCards != null)
                {
                    foreach(CrystalBallCardCollection cardColl in cbbCards)
                    {
                        if(cardColl.NumbersRequired == numbersRequired)
                        {
                            foreach(CrystalBallCard card in cardColl)
                            {
                                if(card.IsFavorite && card.Face == null)
                                    favorites++;
                            }
                        }
                    }
                }
            }

            return favorites;
        }

        /// <summary>
        /// Marks all the CBB cards (that aren't Quick Pick) to be saved as
        /// favorites.
        /// </summary>
        public void MarkCBBAsFavorites()
        {
            foreach(SaleItem item in m_items)
            {
                IEnumerable<CrystalBallCardCollection> cards = item.GetCrystalBallCards();

                if(cards != null)
                {
                    foreach(CrystalBallCardCollection coll in cards)
                    {
                        foreach(CrystalBallCard card in coll)
                        {
                            if(!card.IsQuickPick)
                                card.IsFavorite = true;
                        }
                    }
                }
            }

            m_saveCBBFavorites = true;
        }

        /// <summary>
        /// Sends the Add Sale message to the server.
        /// </summary>
        /// <param name="authStaffId">The id of the override staff or 0 if 
        /// loginNum or magCardNum are used (or override isn't needed).</param>
        /// <param name="loginNum">The login number of the override staff or 0 
        /// if authStaffId or magCardNum are used (or override isn't 
        /// needed).</param>
        /// <param name="magCardNum">The magnetic card number of the override 
        /// staff or 0 if the authStaffId or loginNum are used (or override isn't 
        /// needed).</param>
        /// <param name="password">The password of the override staff or null if 
        /// override isn't needed.</param>
        /// <returns>The return code received from the server.</returns>
        private AddSaleReturnCode AddSale(int authStaffId, int loginNum, string magCardNum, string password)
        {
            AddSaleMessage addSaleMsg = new AddSaleMessage();

            // FIX: DE1930
            addSaleMsg.BankId = m_bankId;
            addSaleMsg.DeviceId = m_device.Id;

            // Set the player, if applicable.
            if(m_player != null)
                addSaleMsg.PlayerId = m_player.Id;

            // Set the gaming date, type, and workstation id.
            addSaleMsg.TransactionType = m_isReturn ? TransactionType.Return : TransactionType.Sale;
            // END: DE1930

            // Rally TA7465
            addSaleMsg.SaleCurrency = m_saleCurrency.ISOCode;

            // Set the amount tendered & tax.
            addSaleMsg.AmountTendered = m_amountTendered;
            addSaleMsg.PrepaidAmount = CalculatePrepaidAmount() + CalculatePrepaidTaxTotal();
            addSaleMsg.TaxRate = m_taxRate; // FIX: DE1938

            addSaleMsg.PointQualifyingAmount = CalculateQualifyingSubtotalForPoints();
            addSaleMsg.PointsFromQualifyingAmount = CalculatePointsEarnedFromQualifyingSubtotal(); //player rating points

            // Add the packages to the sale message.
            for(int x = 0; x < m_items.Count; x++)
            {
                addSaleMsg.AddSaleItem(m_id, m_items[x], (short)(x + 1));
            }

            // Add override parameters.
            addSaleMsg.AuthStaffId = authStaffId;
            addSaleMsg.AuthLoginNumber = loginNum;
            addSaleMsg.AuthMagCardNumber = magCardNum;

            if(!string.IsNullOrEmpty(password))
                addSaleMsg.AuthLoginPassword = SecurityHelper.HashPassword(password);
            else
                addSaleMsg.AuthLoginPassword = null;

            // Send the message.
            try
            {
                addSaleMsg.Send();
            }
            catch(ServerCommException)
            {
                throw; // Don't repackage the ServerCommException
            }
            catch(ServerException) // FIX: DE2665
            {
                // ServerExceptions will be dealt with in a higher method.
            } // END: DE2665
            catch(Exception ex)
            {
                throw new POSException(string.Format(CultureInfo.CurrentCulture, Resources.AddSaleFailed, ServerExceptionTranslator.FormatExceptionMessage(ex)), ex);
            }

            // Update the sale with the register and transaction numbers.
            if(addSaleMsg.ServerReturnCode == GTIServerReturnCode.Success)
            {
                m_id = addSaleMsg.RegisterReceiptId;
                m_receiptNum = addSaleMsg.TransactionNumber;

                // US4120/US4119
                //if north dakota sales mode then save pin if necessary
                if (m_parent.Settings.NorthDakotaSalesMode && m_parent.CurrentSale.HasElectronicBingo)
                {
                    if (ReceiptPin != null &&
                        ReceiptPin.Length == DataSizes.PasswordHash)
                    {
                        if (m_player == null)
                        {
                            var setReceiptPin = new SetReceiptPinNumberMessage(addSaleMsg.RegisterReceiptId, ReceiptPin);

                            try
                            {
                                setReceiptPin.Send();
                            }
                            catch (Exception ex)
                            {
                                throw new POSException(
                                    string.Format(CultureInfo.CurrentCulture, Resources.AddSaleFailed,
                                        ServerExceptionTranslator.FormatExceptionMessage(ex)), ex);
                            }

                        }
                        else if (m_parent.CurrentSale.UpdatePlayerPin)
                        {
                            //save player pin
                            m_parent.UpdateExistingPlayer(m_parent.CurrentSale.Player);
                        }
                    }
                    
                }
            }
            else
            {
                m_id = 0;
                m_receiptNum = 0;
            }

            return (AddSaleReturnCode)addSaleMsg.ReturnCode;
        }

        private void AddReceiptPin()
        {

            AddSaleMessage addSaleMsg = new AddSaleMessage();
        }

        // Rally US229
        /// <summary>
        /// Creates bingo cards for a sale on the server.
        /// </summary>
        /// <param name="mainStageMode">If true, then this method will call the
        /// UK version of add bingo card sales.</param>
        private void AddBingoCards(bool mainStageMode)
        {
            // Rally TA5748
            bool playWithPaper = false;
            bool barcodedPaper = HasBarcodedPaperBingo;

            lock(m_parent.Settings.SyncRoot)
            {
                playWithPaper = m_parent.Settings.PlayWithPaper;
            }

            // Prepare the bingo sales message.
            AddBingoCardSaleMessage bingoMsg = new AddBingoCardSaleMessage(mainStageMode, (m_prePrintedInfo != null), playWithPaper, barcodedPaper, m_id);

            // Rally US510
            if(m_prePrintedInfo != null)
            {
                bingoMsg.PrePrintedHallId = m_prePrintedInfo.HallId;
                bingoMsg.PrePrintedPermVersion = m_prePrintedInfo.PermVersion;
                bingoMsg.PrePrintedStartsNumber = m_prePrintedInfo.StartsNumber;
                bingoMsg.PrePrintedCardCount = m_prePrintedInfo.CardCount;
                bingoMsg.PrePrintedDAStartsNumber = m_prePrintedInfo.DAStartsNumber;
                bingoMsg.PrePrintedDACardCount = m_prePrintedInfo.DACardCount;
                bingoMsg.PrePrintedBarcode = m_prePrintedInfo.Barcode;
            }

            if(playWithPaper)
            {
                List<SaleItem> elecPackages = m_items.FindAll(i => i.IsPackageItem && i.Package.HasElectronicBingo);

                foreach (SaleItem item in elecPackages)
                {
                    Product[] products = item.Package.GetProducts();

                    List<Product> elecProducts = products.ToList<Product>().FindAll(p => p is ElectronicBingoProduct && p != null);
                    
                    foreach(Product product in elecProducts)
                    {
                        ElectronicBingoProduct elecProduct = product as ElectronicBingoProduct;

                        bingoMsg.AddStartNumbers(item.Session.SessionPlayedId, elecProduct.Id, elecProduct.StartNumbers);
                    }
                }
            }
            // END: TA5748

            // US2826
            if (barcodedPaper)
            {
                List<SaleItem> paperPackages = m_items.FindAll(i => i.IsPackageItem && i.Package.HasPaperBingo);

                foreach (SaleItem item in paperPackages)
                {
                    Product[] products = item.Package.GetProducts();

                    List<Product> paperProducts = products.ToList<Product>().FindAll(p => p is PaperBingoProduct && p != null);

                    foreach (Product product in paperProducts)
                    {
                        PaperBingoProduct paperProduct = product as PaperBingoProduct;
                        
                        bingoMsg.AddPackInfo(item.Session.SessionPlayedId, paperProduct.Id, paperProduct.PackInfo);
                    }
                }
            }

            // Send the message.
            try
            {
                bingoMsg.Send();
            }
            catch(ServerCommException)
            {
                throw; // Don't repackage the ServerCommException
            }
            catch(ServerException ex)
            {
                // Rally TA6177 - Finish failed sales so the server can delete cards.
                // Signal a failed sale.
                FinishSale(false, true);

                if((AddBingoCardSaleReturnCode)ex.ReturnCode == AddBingoCardSaleReturnCode.InsufficientCards)
                    throw new POSException(Resources.AddSaleErrorNoCards);
                else if((AddBingoCardSaleReturnCode)ex.ReturnCode == AddBingoCardSaleReturnCode.SessionOutOfSync)
                    throw new POSException(Resources.AddSaleErrorSessionSync);
                else if((AddBingoCardSaleReturnCode)ex.ReturnCode == AddBingoCardSaleReturnCode.MissingSerialLookup)
                    throw new POSException(Resources.AddSaleErrorMissingSerial);
                else if((AddBingoCardSaleReturnCode)ex.ReturnCode == AddBingoCardSaleReturnCode.ErrorInvalidPermRange) // Rally US510
                    throw new POSException(Resources.AddSaleErrorInvalidPerm);
                else if((AddBingoCardSaleReturnCode)ex.ReturnCode == AddBingoCardSaleReturnCode.ErrorSavingCardsToDb) // Rally US510
                    throw new POSException(Resources.AddSaleErrorFailedToSaveCards);
                else if((AddBingoCardSaleReturnCode)ex.ReturnCode == AddBingoCardSaleReturnCode.InvalidHallId) // Rally US510
                    throw new POSException(Resources.AddSaleErrorInvalidHallId);
                else if((AddBingoCardSaleReturnCode)ex.ReturnCode == AddBingoCardSaleReturnCode.PackInUse) // FIX: DE2951 - (PPP) User receives invalid error dialog.
                    throw new POSException(Resources.AddSaleErrorPackInUse, ex);
                else if((AddBingoCardSaleReturnCode)ex.ReturnCode == AddBingoCardSaleReturnCode.MissingPermLib) // FIX: DE4037
                    throw new POSException(Resources.AddSaleErrorMissingPerm, ex);
                else if((AddBingoCardSaleReturnCode)ex.ReturnCode == AddBingoCardSaleReturnCode.InvalidGameCategory) // FIX: DE6691
                    throw new POSException(Resources.AddSaleInvalidGameCategory);
                else
                    throw new POSException(string.Format(CultureInfo.CurrentCulture, Resources.AddBingoCardsFailed, ServerExceptionTranslator.FormatExceptionMessage(ex)), ex);
            }
            catch(Exception ex)
            {
                // Rally TA6177
                // Signal a failed sale.
                FinishSale(false, true);

                throw new POSException(string.Format(CultureInfo.CurrentCulture, Resources.AddBingoCardsFailed, ServerExceptionTranslator.FormatExceptionMessage(ex)), ex);
            }

            // Get the pack number.
            m_packNumber = bingoMsg.PackNumber;

            // Clear out the sessions.
            m_sessions = null;
        }

        /// <summary>
        /// Creates Crystal Ball bingo cards for a sale on the server. 
        /// </summary>
        private void AddCBBBingoCards()
        {
            List<SaleItem> cbbItems = m_items.FindAll(i => i.IsPackageItem && i.GetCrystalBallCards() != null);
            
            if (cbbItems == null || cbbItems.Count == 0) //no cards to process
                return;

            // Rally TA6385
            bool allowMelange = false;

            lock(m_parent.Settings.SyncRoot)
            {
                allowMelange = m_parent.Settings.AllowMelangeSpecialGames;
            }

            // Send any CBB cards we have created.
            AddCBBCardSaleMessage cbbMsg = new AddCBBCardSaleMessage(allowMelange, m_id, m_packNumber);
            // END: TA6385

            foreach(SaleItem s in cbbItems)
                cbbMsg.AddCards(s.GetCrystalBallCards());

            try
            {
                cbbMsg.Send();
            }
            catch(ServerCommException)
            {
                throw; // Don't repackage the ServerCommException
            }
            catch(ServerException ex) // FIX: DE2679 - User received -81 error when selling CBB with favorites.
            {
                // Rally TA6177
                // Signal a failed sale.
                FinishSale(false, true);

                if((AddCBBCardSaleReturnCode)ex.ReturnCode == AddCBBCardSaleReturnCode.InsufficientCards)
                    throw new POSException(Resources.AddSaleErrorNoCBBCards);
                else if((AddCBBCardSaleReturnCode)ex.ReturnCode == AddCBBCardSaleReturnCode.ErrorSavingCardsToDb)
                    throw new POSException(Resources.AddSaleErrorFailedToSaveCBBCards);
                else if((AddCBBCardSaleReturnCode)ex.ReturnCode == AddCBBCardSaleReturnCode.InvalidGameCategory) // FIX: DE6691
                    throw new POSException(Resources.AddSaleInvalidGameCategory);
                else
                    throw new POSException(string.Format(CultureInfo.CurrentCulture, Resources.AddCBBCardsFailed, ServerExceptionTranslator.FormatExceptionMessage(ex)), ex);
            } // END: DE2679
            catch(Exception ex)
            {
                // Rally TA6177
                // Signal a failed sale.
                FinishSale(false, true);

                throw new POSException(string.Format(CultureInfo.CurrentCulture, Resources.AddCBBCardsFailed, ServerExceptionTranslator.FormatExceptionMessage(ex)), ex);
            }

            // Get the pack number.
            if(cbbMsg.PackNumber > 0)
                m_packNumber = cbbMsg.PackNumber;
        }

        /// <summary>
        /// Retrieves the bingo cards for a sale from the server.
        /// </summary>
        private void GetBingoGameCards()
        {
            // Find out all the different sessions in the sale.
            List<SessionInfo> sessions = GetSessions();
            List<BingoSession> tempSessions = new List<BingoSession>();

            // Get the cards for each session.
            bool printFaces = false;

            // FIX: TA4779
            lock(m_parent.Settings.SyncRoot)
            {
                printFaces = m_parent.Settings.PrintCardFaces;
            }
            // END: TA4779

            foreach (var session in sessions)
            {
                // Prepare and send the message to get the cards.
                GetGameCardsMessage cardsMsg = new GetGameCardsMessage(m_id, session.SessionPlayedId, printFaces, m_parent.CardLevels);

                try
                {
                    cardsMsg.Send();

                    //US4804
                    //update Linear Display Numbers
                    if (m_parent.Settings.UseLinearGameNumbering)
                    {
                        //get list of linear games
                        var linearGames = GetSessionLinearGameNumbers.GetLinearBingoGameList(session.SessionPlayedId);

                        //find the match game and update the linear display number
                        foreach (var bingoGame in cardsMsg.Games)
                        {
                            var matchingGame = linearGames.FirstOrDefault(i => i.DisplayNumber == bingoGame.DisplayNumber &&
                                                                           i.LinearNumber == bingoGame.LinearNumber);

                            //if null then continue
                            if (matchingGame == null)
                            {
                                continue;
                            }

                            //update number
                            bingoGame.LinearDisplayNumber = matchingGame.LinearDisplayNumber;
                            bingoGame.ContinuationGameCount = matchingGame.ContinuationGameCount;
                            bingoGame.UseLinearGameNumbers = m_parent.Settings.UseLinearGameNumbering;
                        }
                    }
                }
                catch(ServerCommException)
                {
                    throw; // Don't repackage the ServerCommException
                }
                catch(Exception ex)
                {
                    // Rally TA6177
                    // Signal a failed sale.
                    FinishSale(false, true);

                    throw new POSException(string.Format(CultureInfo.CurrentCulture, Resources.GetBingoGameCardsFailed, ServerExceptionTranslator.FormatExceptionMessage(ex)), ex);
                }

                BingoSession tempSession = new BingoSession(session.SessionNumber, cardsMsg.SameCards);

                tempSession.AddGames(cardsMsg.Games);

                if(tempSession.GameCount > 0)
                    tempSessions.Add(tempSession);
            }

            // Sort the session numbers.
            tempSessions.Sort();

            if(tempSessions.Count > 0)
                m_sessions = tempSessions.ToArray();
        }

        // FIX: DE2596
        /// <summary>
        /// Attempts to find a device to sell to.
        /// </summary>
        /// <param name="worker">The background worker to report progress 
        /// to.</param>
        private void SellToDevice(BackgroundWorker worker)
        {
            m_unitNumber = 0;
            m_serialNumber = string.Empty;

            try
            {
                // Rally US510
                m_parent.UnitMgmt.SellUnit(worker, m_device.Id, m_id, out m_unitNumber, out m_serialNumber);
            }
            catch(Exception ex)
            {
                m_parent.Log("Failed to sell to a unit: " + ex.Message, LoggerLevel.Warning);

                // Rally TA6177
                // Signal a failed sale.
                FinishSale(false, true);

                throw new POSException(Resources.SellToCrateFailed + Environment.NewLine + ex.Message, ex);
            }
        }
        // END: DE2596

        // Rally TA6177
        /// <summary>
        /// Sends the Finish Sale message.
        /// </summary>
        /// <param name="saleSuccess">true if the sale completed; otherwise
        /// false.</param>
        /// <param name="dontThrow">true to suppress non-ServerCommExceptions;
        /// otherwise all exceptions will be thrown.</param>
        private List<string> FinishSale(bool saleSuccess, bool dontThrow)
        {
            FinishSaleMessage finishSaleMsg = new FinishSaleMessage();
            
            finishSaleMsg.RegisterReceiptId = m_id;
            finishSaleMsg.SaleSuccess = saleSuccess;
            finishSaleMsg.UnitNumber = m_unitNumber;
            finishSaleMsg.SerialNumber = m_serialNumber;
            finishSaleMsg.IsQuantitySale = (m_quantity > 1); // Rally US556

            // Send the message.
            try
            {
                //US4720: Product Center > Coupons: Award coupons automatically
                if (saleSuccess && m_player != null)
                {
                    //DE12999
                    //get bingo session
                    var bingoSession = GetSessions().OrderBy(i => i.SessionNumber);
                    foreach (var session in bingoSession)
                    {
                        //get session played ID
                        var sessionplayedId = session.SessionPlayedId;
                        if (sessionplayedId != 0)
                        {
                            //Create and Send award Auto Coupon Message
                            var awardAutoCouponsMessage = new AwardAutoPlayerCouponMessage(m_player.Id, CalculateTotal(false), sessionplayedId);
                            awardAutoCouponsMessage.Send();
                        }

                        //we only want to do this for the first element in the collection
                        break;
                    }
                }

                finishSaleMsg.Send();
            }
            catch (ServerCommException)
            {
                throw; // Don't repackage the ServerCommException
            }
            catch (ServerException ex2)
            {
                if (dontThrow)
                    m_parent.Log(string.Format(CultureInfo.CurrentCulture, Resources.FinishSaleFailed, ServerExceptionTranslator.FormatExceptionMessage(ex2)), LoggerLevel.Severe);
                else if (ex2.ReturnCode == GTIServerReturnCode.InsufficientPoints)
                    throw new POSException(Resources.AddSaleErrorNotEnoughPoints);
				else
                    throw new POSException(string.Format(CultureInfo.CurrentCulture, Resources.FinishSaleFailed, ServerExceptionTranslator.FormatExceptionMessage(ex2)), ex2);
            }
            catch (Exception ex)
            {
                if (dontThrow)
                    m_parent.Log(string.Format(CultureInfo.CurrentCulture, Resources.FinishSaleFailed, ServerExceptionTranslator.FormatExceptionMessage(ex)), LoggerLevel.Severe);
                else
                    throw new POSException(string.Format(CultureInfo.CurrentCulture, Resources.FinishSaleFailed, ServerExceptionTranslator.FormatExceptionMessage(ex)), ex);
            }

            return finishSaleMsg.AfterReceiptText;
        }
        // END: TA6177

        public void PrintAbandonedSale()
        {
            PrintSaleToReceipt(true);
        }

        /// <summary>
        /// Prints the current sale to a receipt.
        /// </summary>
        private void PrintSaleToReceipt(bool abandonedSale = false)
        {
            PrintSaleToReceipt(new List<string>(), true);
        }

        private void PrintSaleToReceipt(List<string> afterReceiptText, bool abandonedSale = false)
        {
            // Load the receipt settings.
            string receiptPrinterName;
            string printerName;
            bool printCardsToGlobal;
            float pointSize = 5F;
            Tuple<bool, int, bool, bool> printPoints; //print points, interface ID, external rating, we do rating
            bool printSigLine;
            short copies = 1;
            string disclaimer1, disclaimer2, disclaimer3;
            bool printLotto;
            bool printReceipt = true; // Rally DE9948
            // US2139
            bool printPtsRedeemed = true;
            CBBPlayItSheetPrintMode playItSheetMode; // Rally US505
            CBBPlayItSheetType playItSheetType; // Rally TA8688
            bool printCustomerAndMerchantReceipts = false;
            bool printQuantitySaleReceipts = true;

            lock(m_parent.Settings.SyncRoot)
            {
                // Do we even need to print this sale?
                if (!m_parent.Settings.PrintNonElecReceipt && !HasElectronics)
                    printReceipt = abandonedSale; // Rally DE9948

                receiptPrinterName = m_parent.Settings.ReceiptPrinterName;
                printerName = m_parent.Settings.PrinterName;
                printCardsToGlobal = m_parent.Settings.PrintFacesToGlobalPrinter && !abandonedSale; //don't print card faces if sale was abandoned
                pointSize = m_parent.Settings.CardFacePointSize;
                printPoints = m_parent.Settings.PlayerPointPrintingInfo;
                printSigLine = m_parent.WeAreNotAPOSKiosk && (m_parent.Settings.PrintSignatureLine || abandonedSale); //Add a signature line to the abandoned sale
                copies = m_parent.Settings.ReceiptCopies;
                disclaimer1 = m_parent.Settings.ReceiptDisclaimer1;
                disclaimer2 = m_parent.Settings.ReceiptDisclaimer2;
                disclaimer3 = m_parent.Settings.ReceiptDisclaimer3;
                // Rally US419
                printLotto = (m_parent.Settings.PlayType == BingoPlayType.Lotto) && !abandonedSale;
                playItSheetMode = m_parent.Settings.CBBPlayItSheetPrintMode; // Rally US505
                playItSheetType = m_parent.Settings.CBBPlayItSheetType; // Rally TA8688
                printPtsRedeemed = m_parent.Settings.PrintPointsRedeemed;
                printCustomerAndMerchantReceipts = m_parent.Settings.PrintDualReceiptsForNonCashSales;
                printQuantitySaleReceipts = m_parent.Settings.PrintQuantitySaleReceipts;
            }

            // Rally DE9948
            // TTP 50372 - POS isn't able to reprint a receipt it couldn't create the first time.
            SalesReceipt receipt = null;
            Printer receiptPrinter = null;
            Exception printerException = null;

            // Create the receipt printer and receipt objects. This saves the receipt text for reprinting pre-sales.
            if (receiptPrinterName != null)
            {
                try
                {
                    receiptPrinter = new Printer(receiptPrinterName);
                    receipt = CreateReceipt(receiptPrinter.Using58mmPaper);
                }
                catch (Exception e)
                {
                    printerException = e;
                    receiptPrinter = null;
                    receipt = CreateReceipt(false);
                }
            }
            else // Create the receipt with default size.
            {
                receipt = CreateReceipt(false);
            }

            if (m_quantity > 1 && !printQuantitySaleReceipts) //don't print quantity sale receipt
                printReceipt = false;

            if (printReceipt)
            {
                receipt.SaleSuccess = !abandonedSale;

                bool haveCreditCardTender = false;

                if (receipt.SaleTenders != null)
                {
                    foreach (SaleTender st in receipt.SaleTenders)
                    {
                        if (st.TenderTypeID == TenderType.CreditCard || st.TenderTypeID == TenderType.DebitCard)
                            haveCreditCardTender = true;
                    }
                }

                printCustomerAndMerchantReceipts = printCustomerAndMerchantReceipts && haveCreditCardTender;

                receipt.DisclaimerLine1 = disclaimer1;
                receipt.DisclaimerLine2 = disclaimer2;
                receipt.DisclaimerLine3 = disclaimer3;

                receipt.AfterReceiptText = afterReceiptText;

                // Rally US419
                receipt.PrintLotto = printLotto;

                // FIX: TA4779
                bool printFaces = false;

                // Rally TA5748
                PrintCardNumberMode printNumsMode;

                lock (m_parent.Settings.SyncRoot)
                {
                    bool cardUsed = m_tenders != null && m_tenders.Count > 0 && m_tenders.Exists(t => t.SaleTenderInfo.TenderTypeID == TenderType.CreditCard || t.SaleTenderInfo.TenderTypeID == TenderType.DebitCard || t.SaleTenderInfo.TenderTypeID == TenderType.GiftCard);

                    //force operator info onto the receipt if tendered with a credit, debit, or gift card.
                    if (m_parent.Settings.PrintOperatorInfoOnReceipt || cardUsed)
                    {
                        receipt.OperatorName = m_parent.CurrentOperator.Name;
                        receipt.OperatorAddress1 = m_parent.CurrentOperator.Address1;
                        receipt.OperatorAddress2 = m_parent.CurrentOperator.Address2;
                        receipt.OperatorCityStateZip = m_parent.CurrentOperator.City + ", " + m_parent.CurrentOperator.State + " " + m_parent.CurrentOperator.Zip;
                        receipt.OperatorPhoneNumber = m_parent.CurrentOperator.Phone;
                    }

                    receipt.OperatorHeaderLine1 = m_parent.Settings.ReceiptHeaderLine1;
                    receipt.OperatorHeaderLine2 = m_parent.Settings.ReceiptHeaderLine2;
                    receipt.OperatorHeaderLine3 = m_parent.Settings.ReceiptHeaderLine3;
                    receipt.OperatorFooterLine1 = m_parent.Settings.ReceiptFooterLine1;
                    receipt.OperatorFooterLine2 = m_parent.Settings.ReceiptFooterLine2;
                    receipt.OperatorFooterLine3 = m_parent.Settings.ReceiptFooterLine3;
                    receipt.IncompleteTransactionLine1 = m_parent.Settings.IncompleteTransactionLine1;
                    receipt.IncompleteTransactionLine2 = m_parent.Settings.IncompleteTransactionLine2;
                    printNumsMode = m_parent.Settings.PrintCardNumbers;
                    printFaces = m_parent.Settings.PrintCardFaces && !abandonedSale; //don't print card faces if sale was abandoned
                }
                // END: TA4779
                receipt.TransactionDate = DateTime.Now;
                // Save for reprinting.
                m_parent.LastReceipt = receipt;

                // If we failed while creating the receipt, don't try to print it.
                if (printerException != null)
                    throw printerException;

                // Are we printing the card faces to another printer?
                if (printFaces && printCardsToGlobal)
                {
                    // Create the card receipt.
                    BingoCardReceipt cardReceipt = new BingoCardReceipt();

                    // Get the information from the original receipt.
                    cardReceipt.Number = receipt.Number;
                    cardReceipt.GamingDate = receipt.GamingDate;
                    cardReceipt.PrintLotto = receipt.PrintLotto;
                    cardReceipt.BingoSessions = receipt.BingoSessions;
                    cardReceipt.PointSize = pointSize;

                    // Print both receipts.
                    if (printerName != null)
                    {
                        Printer globalPrinter = new Printer(printerName);
                        cardReceipt.Print(globalPrinter, copies);
                    }

                    if (receiptPrinter != null)
                    {
                        receipt.Print(receiptPrinter, m_parent.Settings.PrintPlayerIdentityAsAccount, m_parent.Settings.PrintPlayerID, printPoints, printSigLine, printPtsRedeemed, printNumsMode, false, copies);

                        if (printCustomerAndMerchantReceipts) //print merchant receipt
                            receipt.Print(receiptPrinter, m_parent.Settings.PrintPlayerIdentityAsAccount, m_parent.Settings.PrintPlayerID, printPoints, printSigLine, printPtsRedeemed, printNumsMode, false, copies, true);
                    }
                }
                else if (receiptPrinter != null)
                {
                    receipt.Print(receiptPrinter, m_parent.Settings.PrintPlayerIdentityAsAccount, m_parent.Settings.PrintPlayerID, printPoints, printSigLine, printPtsRedeemed, printNumsMode, printFaces, copies);

                    if (printCustomerAndMerchantReceipts) //print merchant receipt
                        receipt.Print(receiptPrinter, m_parent.Settings.PrintPlayerIdentityAsAccount, m_parent.Settings.PrintPlayerID, printPoints, printSigLine, printPtsRedeemed, printNumsMode, printFaces, copies, true);
                }
                // END: TA5748
                // END: US2139

                // Rally US505
                // Rally TA8688
                if (!abandonedSale && playItSheetMode != CBBPlayItSheetPrintMode.Off) //don't print play-it sheet if sale was abandoned
                {
                    if ((playItSheetMode == CBBPlayItSheetPrintMode.All && (CheckCrystalBallMedia(CardMedia.Electronic) || CheckCrystalBallMedia(CardMedia.Paper))) ||
                        (playItSheetMode == CBBPlayItSheetPrintMode.ElectronicOnly && CheckCrystalBallMedia(CardMedia.Electronic)) ||
                        (playItSheetMode == CBBPlayItSheetPrintMode.PaperOnly && CheckCrystalBallMedia(CardMedia.Paper)))
                    {
                        m_parent.LastReceiptHasPlayIt = true;

                        if (playItSheetType == CBBPlayItSheetType.Card || playItSheetType == CBBPlayItSheetType.Line)
                            m_parent.PrintPlayItSheet(m_id, printerName, false);
                        else
                            m_parent.PrintPlayItSheet(m_id, receiptPrinterName, false);
                    }
                    else
                        m_parent.LastReceiptHasPlayIt = false;
                }
                else
                    m_parent.LastReceiptHasPlayIt = false;
                // END: TA8688
            }
            else
            {
                // Check if we need to print the play it sheet still
                if (!abandonedSale && (playItSheetMode == CBBPlayItSheetPrintMode.All || playItSheetMode == CBBPlayItSheetPrintMode.PaperOnly) && CheckCrystalBallMedia(CardMedia.Paper)) //don't print play-it sheet is sale was abandoned
                {
                    if (playItSheetType == CBBPlayItSheetType.Card || playItSheetType == CBBPlayItSheetType.Line)
                        m_parent.PrintPlayItSheet(m_id, printerName, false);
                    else
                        m_parent.PrintPlayItSheet(m_id, receiptPrinterName, false);
                }
            }
            // END: DE9948
        }

        /// <summary>
        /// Generates a receipt based on the current sale.
        /// </summary>
        /// <param name="smallText">Whether this is for a small 
        /// receipt.</param>
        /// <returns>A receipt based on the current sale.</returns>
        public SalesReceipt CreateReceipt(bool smallText)
        {
            SalesReceipt receipt = new SalesReceipt();

            // Header
            // Rally DE1863 - Add the wording "Return" on a receipt printed when in return mode.
            receipt.UseLinearDisplayNumbers = m_parent.Settings.UseLinearGameNumbering; //US4804
            receipt.IsReturn = m_isReturn;
            receipt.RegisterReceiptId = Id; // Rally US505
            receipt.Number = ReceiptNumber;
            receipt.GamingDate = GamingDate;
            receipt.SoldFromMachineId = SoldFromMachineId;
            receipt.TransactionDate = TransactionDate;

            if (m_parent.CurrentSession.IsPreSale)
            {
                receipt.IsPreSale = true;

                if (m_parent.CurrentSession.GamingDate != null)
                {
                    receipt.PresalesProgramName = m_parent.CurrentSession.ProgramName;
                    receipt.PresalesGamingDate = m_parent.CurrentSession.GamingDate.Value;
                }
            }

            if(Cashier != null)
            {
                receipt.Cashier = Cashier.FirstName;

                // TTP 50097
                if(!m_parent.Settings.PrintStaffFirstNameOnly)
                    receipt.Cashier += " " + Cashier.LastName;
            }

            receipt.PackNumber = PackNumber;

            // Device
            if(Device.Id != 0)
            {
                receipt.DeviceName = Device.Name;
                receipt.UnitNumber = UnitNumber;
                receipt.SerialNumber = SerialNumber;
            }

            // Player
            if(Player != null)
            {
                // TTP 50114
                receipt.MachineAccount = m_machineSale;
                receipt.PlayerId = Player.Id;
                receipt.PlayerIdentity = Player.PlayerIdentity;
                receipt.PlayerName = Player.ToString(false);
                receipt.PlayerPoints = Player.PointsBalance;
                receipt.PlayerPointsEarned = CalculateTotalEarnedPoints();
                receipt.PlayerPointsRedeemed = CalculateTotalPointsToRedeem();

                //add qualifying amount and points from qualifying amount
                receipt.QualifyingAmountForPoints = CalculateQualifyingSubtotalForPoints();
                receipt.PointsForQualifyingAmount = CalculatePointsEarnedFromQualifyingSubtotal();
            }

            if (m_tenders != null && m_tenders.Count > 0) //pass our tenders to the receipt
            {
                //build an array of our tenders and pass it off
                SaleTender[] st = new SaleTender[m_tenders.Count];

                for (int i = 0; i < m_tenders.Count; i++)
                    st[i] = m_tenders[i].SaleTenderInfo;

                receipt.SaleTenders = st;
            }

            /**********************************************************************************/
            // merge the validation line item price back into the sale items
            /**********************************************************************************/
            // Sale Info US4458
            //save the line item detail for reprinting
            List<LineDetail> lineItemDetail = new List<LineDetail>();
            List<PaperPackInformation> paperPackInfo = new List<PaperPackInformation>();
            string[] sortGroupNames = SaleItem.GetSortGroupNames();
            bool showValidationGroup = m_parent.Settings.ProductValidationMaxQuantity != 0 && m_items.Exists(i => i.Session.IsMaxValidationEnabled);

            if (!m_parent.Settings.PrintReceiptSortedByPackageType)
                sortGroupNames[0] = string.Empty;

            foreach (SaleItem item in m_items)
            {
                if (!m_parent.Settings.PrintReceiptSortedByPackageType)
                    item.SortOrder = 0; //items will not be sorted

                bool mergeValidation = !(showValidationGroup && item.Session.IsMaxValidationEnabled);

                if (mergeValidation && item.IsDefaultValidationPackage)
                    continue;

                //US3509
                if (item.IsPackageItem)
                {
                    foreach (var bingoProduct in item.Package.GetProducts().OfType<BingoProduct>())
                    {
                        if (!bingoProduct.IsValidated)
                            continue;

                        if (bingoProduct.Type == ProductType.Paper)
                            receipt.IsPaperProductsValidated = true;
                        else if (bingoProduct.Type == ProductType.Electronic)
                            receipt.IsElectronicProductsValidated = true;
                    }
                }

                // PDTS 964
                // PDTS 584

                List<LineDetail> itemText = new List<LineDetail>();

                if (sortGroupNames[item.SortOrder] != string.Empty)
                {
                    itemText.Add(new LineDetail(item.SortOrder, sortGroupNames[item.SortOrder]));
                    sortGroupNames[item.SortOrder] = string.Empty;
                }

                string[] lines;

                if (m_parent.Settings.PrintPointsRedeemed)
                    lines = item.ToStringForReceipt(smallText, item.IsPackageItem && mergeValidation? item.Package.PackageValidationValue : 0.0m);
                else
                    lines = item.ToStringForReceiptWithNoRedeemedPoints(smallText, item.IsPackageItem && mergeValidation? item.Package.PackageValidationValue : 0.0m);

                foreach (string s in lines)
                    itemText.Add(new LineDetail(item.SortOrder, s));

                if (item.IsPackageItem && m_parent.Settings.PrintProductNames)
                {
                    // Rally US26 - Book count on Receipt
                    string[] prodNames = item.GetProductNames(true, m_parent.Settings.MainStageMode);

                    if (prodNames != null)
                    {
                        foreach (string s in prodNames)
                            itemText.Add(new LineDetail(item.SortOrder, s));
                    }
                }

                foreach (LineDetail ld in itemText)
                {
                    receipt.AddLineItem(ld);

                    lineItemDetail.Add(ld);
                }

                //US2826
                //check to see if item is a barcoded paper pack
                if (item.Package != null) //DE12761: Discounts failed to print
                {
                    var products = item.Package.GetProducts();

                    bool needProductName = products.Count(p => p as PaperBingoProduct != null && ((PaperBingoProduct)p).BarcodedPaper) > 1;

                    foreach (var product in products)
                    {
                        var paperProduct = product as PaperBingoProduct;

                        if (paperProduct == null || !paperProduct.BarcodedPaper) //not barcoded paper
                            continue;

                        //iterate through pack infos
                        foreach (var paperInfo in paperProduct.PackInfo)
                        {
                            PaperPackInformation pack = new PaperPackInformation();

                            pack.Session = item.Session.SessionNumber;
                            pack.SerialNumber = paperInfo.SerialNumber;
                            pack.AuditNumber = paperInfo.AuditNumber;
                            pack.PackageName = item.Package.ReceiptText;

                            if (needProductName)
                                pack.ProductName = " - "+product.Name;
                            else
                                pack.ProductName = string.Empty;

                            paperPackInfo.Add(pack);
                        }
                    }
                }
            }

            //sort the paper pack info
            if (paperPackInfo.Count > 0)
            {
                paperPackInfo.Sort(delegate(PaperPackInformation x, PaperPackInformation y)
                {
                    if (x.Session == y.Session)
                    {
                        if (x.PackageName.CompareTo(y.PackageName) == 0)
                        {
                            if (x.ProductName.CompareTo(y.ProductName) == 0)
                            {
                                int serialX = 0;
                                int serialY = 0;

                                int.TryParse(x.SerialNumber, out serialX);
                                int.TryParse(y.SerialNumber, out serialY);

                                if (serialX == serialY)
                                {
                                    if (x.AuditNumber == y.AuditNumber)
                                        return 0;
                                    else if (x.AuditNumber < y.AuditNumber)
                                        return -1;
                                    else
                                        return 1;
                                }
                                else
                                {
                                    if (serialX < serialY)
                                        return -1;
                                    else
                                        return 1;
                                }
                            }
                            else
                            {
                                if (x.ProductName.CompareTo(y.ProductName) < 0)
                                    return -1;
                                else
                                    return 1;
                            }
                        }
                        else
                        {
                            if (x.PackageName.CompareTo(y.PackageName) < 0)
                                return -1;
                            else
                                return 1;
                        }
                    }
                    else
                    {
                        if (x.Session < y.Session)
                            return -1;
                        else
                            return 1;
                    }
                });

                //build text for receipt
                //
                //Session
                // Package/product 
                //  audit numbers
                //
                //or compact
                //
                //Sess Serial# Pack numbers

                bool useCompactFormat = m_parent.Settings.PrintCompactPaperPacksSoldReportOnReceipt;
                int lastProcessedSession = -1;
                string lastProcessedSerialNumber = string.Empty;
                string lastProcessedPackageProduct = string.Empty;
                StringBuilder auditNumbers = new StringBuilder("");
                List<string> text = new List<string>();
                bool haveAuditNumber = false;
                bool multipleSessions = false;

                if (useCompactFormat)
                    text.Add(Resources.ReceiptCompactPaperPacksSoldHeader);
                else
                    multipleSessions = paperPackInfo.Exists(p => p.Session != paperPackInfo[0].Session);

                foreach (PaperPackInformation pack in paperPackInfo)
                {
                    if (multipleSessions && pack.Session != lastProcessedSession)
                    {
                        if (haveAuditNumber)
                        {
                            text.Add(auditNumbers.ToString());
                            auditNumbers.Clear();
                            auditNumbers.Append("");
                            haveAuditNumber = false;
                        }

                        if (useCompactFormat)
                        {
                            auditNumbers.Clear();
                            auditNumbers.Append(pack.Session.ToString().PadRight(4) + pack.SerialNumber.PadLeft(8) + " ");
                        }
                        else
                        {
                            text.Add(string.Format(Resources.ReceiptSession, pack.Session.ToString()));
                        }

                        lastProcessedSerialNumber = string.Empty;
                        lastProcessedPackageProduct = string.Empty;
                    }

                    if (!useCompactFormat && (pack.PackageName + pack.ProductName).CompareTo(lastProcessedPackageProduct) != 0)
                    {
                        if (haveAuditNumber)
                        {
                            text.Add(auditNumbers.ToString());
                            auditNumbers.Clear();
                            auditNumbers.Append("");
                            haveAuditNumber = false;
                        }

                        text.Add(" " + pack.PackageName+pack.ProductName);

                        lastProcessedSerialNumber = string.Empty;
                    }

                    if (pack.SerialNumber.CompareTo(lastProcessedSerialNumber) != 0)
                    {
                        if (haveAuditNumber)
                        {
                            text.Add(auditNumbers.ToString());
                            auditNumbers.Clear();
                            auditNumbers.Append("");
                            haveAuditNumber = false;
                        }

                        if (useCompactFormat)
                        {
                            auditNumbers.Clear();
                            auditNumbers.Append(pack.Session.ToString().PadRight(4) + pack.SerialNumber.PadLeft(8) + " ");
                        }
                        else
                        {
                            auditNumbers.Append("  " + pack.SerialNumber + ": ");
                        }
                    }

                    if (auditNumbers.Length + (haveAuditNumber? 2 : 0) + pack.AuditNumber.ToString().Length > 39)
                    {
                        auditNumbers.Append(",");
                        text.Add(auditNumbers.ToString());
                        auditNumbers.Clear();
                        
                        if(useCompactFormat)
                            auditNumbers.Append("             ");
                        else
                            auditNumbers.Append("   ");

                        haveAuditNumber = false;
                    }

                    auditNumbers.Append((haveAuditNumber? ", " : "") + pack.AuditNumber.ToString());

                    haveAuditNumber = true;
                    lastProcessedSession = pack.Session;
                    lastProcessedSerialNumber = pack.SerialNumber;
                    lastProcessedPackageProduct = pack.PackageName+pack.ProductName;
                }

                if (haveAuditNumber)
                    text.Add(auditNumbers.ToString());

                //add paper pack info to receipt
                foreach (string line in text)
                    receipt.AddPaperPackInfo(line);

                //save paper pack detail to the server
                GetSetSaleTextMessage paperTextMsg = new GetSetSaleTextMessage(Id);
                paperTextMsg.SaveText = true;
                paperTextMsg.TextType = 2; //paper pack detail
                paperTextMsg.ReceiptText = text.ToArray();
                paperTextMsg.Send();
            }
                        
            //save line detail to server
            GetSetSaleTextMessage textMsg = new GetSetSaleTextMessage(Id);
            textMsg.SaveText = true;
            textMsg.TextType = 1; //item detail
            textMsg.ReceiptText = LineDetail.GetSortedDetail(lineItemDetail).ToArray();
            textMsg.Send();

            receipt.DeviceFee = DeviceFee;
            receipt.PrepaidAmount = CalculatePrepaidAmount() + CalculatePrepaidTaxTotal();
            receipt.Taxes = CalculateTaxes();
            // Rally TA7464
            receipt.Total = CalculateTotal(false);
            receipt.DefaultCurrency = m_parent.DefaultCurrency.ISOCode;
            receipt.SaleCurrency = m_saleCurrency.ISOCode;
            receipt.ExchangeRate = m_saleCurrency.ExchangeRate;
            receipt.GrandTotal = CalculateTotal(true);
            receipt.NonTaxedCouponTotal = CalculateNonTaxableCouponTotal();
            receipt.AmountTendered = AmountTendered;
            receipt.PrepaidAmount = CalculatePrepaidAmount() + CalculatePrepaidTaxTotal();
            receipt.ChangeDue = CalculateChange();
            // END: TA7464

            receipt.BingoSessions = BingoSessions;
//            receipt.Charities = m_parent.CharityData;
            receipt.SessionCharities = GetSessionCharities(receipt.RegisterReceiptId);
            receipt.MachineDesc = m_parent.MachineDesc;
            return receipt;
        }

        private List<SessionCharity> GetSessionCharities(int receiptId)
        {
            return GetSessionCharityDataMessage.GetList(receiptId);
        }

        // Rally US505
        /// <summary>
        /// Checks to see if this sale has any Crystal Ball cards with the
        /// specified media.
        /// </summary>
        /// <param name="media">The media type of CBB cards to check
        /// for.</param>
        /// <returns>true if the sale has CBB cards of the specified media;
        /// otherwise false.</returns>
        private bool CheckCrystalBallMedia(CardMedia media)
        {
            bool hasCBB = false;

            foreach(SaleItem item in m_items)
            {
                IEnumerable<CrystalBallCardCollection> cards = item.GetCrystalBallCards();

                if(cards != null)
                {
                    foreach(CrystalBallCardCollection coll in cards)
                    {
                        foreach(CrystalBallCard card in coll)
                        {
                            if(card.Media == media)
                            {
                                hasCBB = true;
                                break;
                            }
                        }
                    }
                }
            }

            return hasCBB;
        }

        // Rally TA5748
        /// <summary>
        /// Adds start numbers for each of the products in the specified list.
        /// </summary>
        /// <param name="productStartNumbers">The list of start numbers to
        /// set.</param>
        public void SetStartNumbers(IList<ProductStartNumbers> productStartNumbers)
        {
            if(productStartNumbers != null && productStartNumbers.Count > 0)
            {
                foreach(ProductStartNumbers productStartNum in productStartNumbers)
                {
                    foreach(SaleItem item in m_items)
                    {
                        if(item.IsPackageItem && item.Package.HasElectronicBingo && item.Session.SessionPlayedId == productStartNum.SessionPlayedId)
                        {
                            Product[] products = item.Package.GetProducts();

                            if(products != null)
                            {
                                foreach(Product product in products)
                                {
                                    ElectronicBingoProduct electProd = product as ElectronicBingoProduct;

                                    if(electProd != null && electProd.Id == productStartNum.ProductId)
                                    {
                                        foreach(StartNumber num in productStartNum.StartNumbers)
                                        {
                                            electProd.StartNumbers.Add(num);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void SetPackInfo(IDictionary<int, List<PaperPackInfo>> productPackInfo)
        {
            if (productPackInfo != null && productPackInfo.Count > 0)
            {
                foreach (KeyValuePair<int, List<PaperPackInfo>> pair in productPackInfo)
                {
                    foreach (SaleItem item in m_items)
                    {
                        if (item.IsPackageItem)
                        {
                            Product[] products = item.Package.GetProducts();
                            if (products != null)
                            {
                                foreach (Product product in products)
                                {
                                    PaperBingoProduct paperProduct = product as PaperBingoProduct;
                                    if (paperProduct != null && pair.Key == paperProduct.Id)
                                    {
                                        paperProduct.PackInfo = pair.Value;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Removes all the start numbers from every product in the sale.
        /// </summary>
        public void ClearStartNumbers()
        {
            foreach(SaleItem item in m_items)
            {
                if(item.IsPackageItem)
                {
                    Product[] products = item.Package.GetProducts();

                    if(products != null)
                    {
                        foreach(Product product in products)
                        {
                            BingoProduct bingoProd = product as BingoProduct;

                            if(bingoProd != null)
                            {
                                bingoProd.StartNumbers.Clear();
                            }
                        }
                    }
                }
            }
        }
        // END: TA5748

        public void SetProductPackInfo(IList<PaperPackInfo> paperPackInfo)
        {
            if (paperPackInfo != null && paperPackInfo.Count > 0)
            {
                foreach (PaperPackInfo packInfo in paperPackInfo)
                {
                    foreach (SaleItem item in m_items)
                    {
                        if (item.IsPackageItem)
                        {
                            Product[] products = item.Package.GetProducts();

                            if (products != null)
                            {
                                foreach (Product product in products)
                                {
                                    PaperBingoProduct paperProd = product as PaperBingoProduct;

                                    if (paperProd != null)
                                    {
                                        foreach (PaperPackInfo info in paperPackInfo)
                                        {
                                            paperProd.PackInfo.Add(info);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public int GetIndexOf(SaleItem item)
        {
            if (m_items.Contains(item))
            {
                return m_items.IndexOf(item);
            }

            return 0;
        }

        public List<TenderItem> GetCurrentTenders()
        {
            List<TenderItem> rc = new List<TenderItem>();
            rc.AddRange(m_tenders);
            return rc;
        }

        public void RemoveTenders()
        {
            m_tenders = new List<TenderItem>();
        }
        #endregion

        #region Member Properties
        
        /// <summary>
        ///Gets or sets if a kiosk user should be asked to use his
        ///player card.
        /// </summary>
        public bool AskPatronForPlayerCard
        {
            get
            {
                return !m_patronDoesNotWantToUsePlayerCard;
            }

            set
            {
                m_patronDoesNotWantToUsePlayerCard = !value;
            }
        }

        /// <summary>
        /// Gets an object that can be used to synchronize access to 
        /// the sale.
        /// </summary>
        public object SyncRoot
        {
            get
            {
                return m_syncRoot;
            }
        }

        // FIX: DE1930
        /// <summary>
        /// Gets or sets the id of the bank this sale is made with.
        /// </summary>
        public int BankId
        {
            get
            {
                return m_bankId;
            }
            set
            {
                m_bankId = value;
            }
        }
        // END: DE1930

        // PDTS 571
        /// <summary>
        /// Gets or sets the number of sales to make with the values in this 
        /// instance.  The default is one.
        /// </summary>
        public short Quantity
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
        /// Gets or sets the sale's id.
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
        /// Gets whether this is a return.
        /// </summary>
        public bool IsReturn
        {
            get
            {
                return m_isReturn;
            }
        }

        /// <summary>
        /// Gets or sets the receipt number for this sale.
        /// </summary>
        public int ReceiptNumber
        {
            get
            {
                return m_receiptNum;
            }
            set
            {
                m_receiptNum = value;
            }
        }

        /// <summary>
        /// Gets or sets the machine id that made the sale.
        /// </summary>
        public int SoldFromMachineId
        {
            get
            {
                return m_soldFromMachineId;
            }
            set
            {
                m_soldFromMachineId = value;
            }
        }

        /// <summary>
        /// Gets or sets the gaming date for this sale.
        /// </summary>
        public DateTime GamingDate
        {
            get
            {
                return m_gamingDate;
            }
            set
            {
                m_gamingDate = value;
            }
        }

        /// <summary>
        /// Gets or sets the transaction date for this sale
        /// </summary>
        public DateTime TransactionDate
        {
            get
            {
                return m_transactionDate;
            }
            set
            {
                m_transactionDate = value;
            }
        }
        /// <summary>
        /// Gets or sets the staff who made the sale.
        /// </summary>
        public Staff Cashier
        {
            get
            {
                return m_cashier;
            }
            set
            {
                m_cashier = value;
            }
        }

        /// <summary>
        /// Gets or sets the pack number associated with this sale 
        /// (or 0 if no pack).
        /// </summary>
        public int PackNumber
        {
            get
            {
                return m_packNumber;
            }
            set
            {
                m_packNumber = value;
            }
        }

        /// <summary>
        /// Gets or sets which device this sale is going to.
        /// </summary>
        public Device Device
        {
            get
            {
                return m_device;
            }
            set
            {
                m_device = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of the unit sold to (or 0 if no unit).
        /// </summary>
        public short UnitNumber
        {
            get
            {
                return m_unitNumber;
            }
            set
            {
                m_unitNumber = value;
            }
        }

        /// <summary>
        /// Gets or sets the serial number of the unit sold to 
        /// (or null if no unit).
        /// </summary>
        public string SerialNumber
        {
            get
            {
                return m_serialNumber;
            }
            set
            {
                m_serialNumber = value;
            }
        }

        // TTP 50114
        /// <summary>
        /// Gets the player sold to.
        /// </summary>
        public Player Player
        {
            get
            {
                return m_player;
            }
        }

        /// <summary>
        /// Gets or sets whether this sale is potentially sold to a machine 
        /// instead of a player.
        /// </summary>
        public bool MachineSale
        {
            get
            {
                return m_machineSale;
            }
            set
            {
                m_machineSale = value;
            }
        }

        /// <summary>
        /// Gets or sets the amount of points a player had before the sale.
        /// </summary>
        public decimal PlayerPreSalePoints
        {
            get
            {
                return m_preSalePlayerPts;
            }
            set
            {
                m_preSalePlayerPts = value;
            }
        }

        /// <summary>
        /// Gets the number of items currently in the sale.
        /// </summary>
        public int ItemCount
        {
            get
            {
                return m_items.Count;
            }
        }

        public bool AddOnSale
        {
            get
            {
                return false;
            }

            set
            {
            }
//            get;
//            set;
        }

        // US2018
        /// <summary>
        /// Gets whether the player should be charged a device fee for this
        /// sale.
        /// </summary>
        public bool ChargeDeviceFee
        {
            get
            {
                if (AddOnSale)
                    return false;

                return m_items.Exists(i => i.IsPackageItem && i.Package.ChargeDeviceFee && i.Session.IsDeviceFeesEnabled);
 
/*                foreach(SaleItem item in m_items)
                {
                    //US4719: (US4717) POS: Support scheduling device fees
                    if(item.IsPackageItem && item.Package.ChargeDeviceFee && m_parent.GetMenuListItem(item.Session).IsDeviceFeesEnabled)
                        return true;
                }

                return false;
 */
            }
        }

        /// <summary>
        /// Gets or sets the device fee for this sale.
        /// </summary>
        public decimal DeviceFee
        {
            get
            {
                //find the number of sessions sold that require a device fee
                int deviceFeeSessions = 0;
                List<SessionInfo> sessions = GetSessions();

                foreach (var session in sessions)
                {
                    if(session.IsDeviceFeesEnabled)
                    {
                        if(m_items.Exists(i => i.Session == session && i.IsPackageItem && i.Package.ChargeDeviceFee))
                            deviceFeeSessions++;
                    }
                }

                return m_deviceFee * deviceFeeSessions;
            }
            set
            {
                m_deviceFee = value;
            }
        }

        /// <summary>
        /// Gets or sets the tax rate for this sale.
        /// </summary>
        public decimal TaxRate
        {
            get
            {
                return m_taxRate;
            }
            set
            {
                m_taxRate = value;
            }
        }

        /// <summary>
        /// Gets or sets the amount tendered for this sale.
        /// </summary>
        public decimal AmountTendered
        {
            get
            {
                return m_amountTendered;
            }
            set
            {
                m_amountTendered = value;
            }
        }

        /// <summary>
        /// Gets or sets an array of bingo sessions related to this sale.
        /// </summary>
        public BingoSession[] BingoSessions
        {
            get
            {
                return m_sessions;
            }
            set
            {
                m_sessions = value;
            }
        }

        /// <summary>
        /// Gets whether this sale contains any electronic items.
        /// </summary>
        public bool HasElectronics
        {
            get
            {
                return m_items.Exists(i => i.IsPackageItem && i.Package.HasElectronics);
            }
        }
        
        // Rally US505
        /// <summary>
        /// Gets whether this sale contains non-CBB, electronic bingo cards.
        /// </summary>
        public bool HasElectronicBingo
        {
            get
            {
                return m_items.Exists(i => i.IsPackageItem && i.Package.HasElectronicBingo);
            }
        }

        /// <summary>
        /// Get whether there is a barcoded paper bingo product in the 
        /// sale list
        /// </summary>
        public bool HasBarcodedPaperBingo
        {
            get
            {
                return m_items.Exists(i => i.IsPackageItem && i.Package.HasPaperBingo);
            }
        }

        /// <summary>
        /// Get whether this sale requires more paper auditing information
        /// </summary>
        public bool NeedsPackInfo
        {
            get
            {
                foreach (SaleItem item in m_items)
                {
                    if (item.IsPackageItem && item.Package.NeedsPackInfo(item.Quantity))
                        return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Gets whether this sale has any savable Crystal Ball cards.
        /// </summary>
        public bool HasSavableCrystalBall
        {
            get
            {
                List<SaleItem> cbbItems = m_items.FindAll(i => i.GetCrystalBallCards() != null);

                foreach(SaleItem item in cbbItems)
                {
                    IEnumerable<CrystalBallCardCollection> cards = item.GetCrystalBallCards();

                    foreach(CrystalBallCardCollection coll in cards)
                    {
                        foreach(CrystalBallCard card in coll)
                        {
                            if(!card.IsFavorite && !card.IsQuickPick)
                                return true;
                        }
                    }
                }

                return false;
            }
        }

        //DE12749
        /// <summary>
        /// Gets whether this sale has any Electronic Crystal Ball cards.
        /// </summary>
        public bool HasElectronicCrystalBall
        {
            get
            {
                List<SaleItem> cbbItems = m_items.FindAll(i => i.GetCrystalBallCards() != null);

                foreach (SaleItem item in cbbItems)
                {
                    IEnumerable<CrystalBallCardCollection> cards = item.GetCrystalBallCards();

                    foreach(CrystalBallCardCollection coll in cards)
                    {
                        foreach(CrystalBallCard card in coll)
                        {
                            if(card.Media == CardMedia.Electronic)
                                return true;
                        }
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Gets whether the Crystal ball cards in this sale are to be saved
        /// as favorites.
        /// </summary>
        public bool SaveCBBAsFavorites
        {
            get
            {
                return m_saveCBBFavorites;
            }
        }

        // FIX: DE2538
        /// <summary>
        /// Gets how many successful sales were made the last time SendToServer
        /// was called.
        /// </summary>
        public int SuccessfulSales
        {
            get
            {
                return m_successfulSales;
            }
        }
        // END: DE2538

        /// <summary>
        /// Gets or Sets the TenderItem used for the cash sale for quantity sales. 
        /// </summary>
        public TenderItem QuantitySaleTenderItem
        {
            get
            {
                return m_quantitySaleTenderItem;
            }

            set
            {
                m_quantitySaleTenderItem = value;
            }
        }

        // Rally TA7465
        /// <summary>
        /// Gets or sets the currency used to make the sale.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">SaleCurrency is set
        /// to a null reference.</exception>
        public Currency SaleCurrency
        {
            get
            {
                return m_saleCurrency;
            }
            set
            {
                if(value == null)
                    throw new ArgumentNullException("SaleCurrency");

                m_saleCurrency = value;
            }
        }

        // END: TA7465

        public byte[] ReceiptPin { get; set; }

        public bool UpdatePlayerPin { get; set; }

        public bool NeedPlayerCardPIN
        {
            get
            {
                return m_needPlayerCardPIN;
            }

            set
            {
                m_needPlayerCardPIN = value;
            }
        }

        /// <summary>
        /// Get list of sale items.
        /// </summary>
        public List<SaleItem> SaleItems
        {
            get
            {
                return m_items;
            }
        }

        #endregion
    }
}
