#region Copyright
// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2008 GameTech
// International, Inc.
#endregion

// US4460: (US4428) Product Center: Set primary validation
//US5117: POS: Automatically add package X when package Y has been added Z times

using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using GTI.Modules.Shared;
using GTI.Modules.POS.Business;
using GTI.Modules.POS.Properties;

namespace GTI.Modules.POS.Data
{
    /// <summary>
    /// The possible status return codes from the Add Sale server message.
    /// </summary>
    internal enum AddSaleReturnCode
    {
        // FIX: DE2665
        IncorrectAuthPassword = -10,
        AuthPasswordHasExpired = -8, 
        InactiveAuthStaff = -9, 
        AuthStaffNotFound = -7,
        GameLocked = -87,
        SessionLocked = 6,
        NotEnoughPoints = -88,
        NotAuthorized = -21,
        // END: DE2665
        InsufficientCards = -62, // PDTS 964
        SessionOutOfSync = -64, // Rally DE139
        MissingSerialLookup = -65,
        AuthStaffLocked = -106 // US1955
    }

    /// <summary>
    /// Represents an Add Sale server message.
    /// </summary>
    internal class AddSaleMessage : ServerMessage
    {
        #region Constants And Data Types
        protected const int ResponseMessageLength = 12;

        protected struct AddSaleProductItem
        {
            public int ProductId;
            public List<PaperPackInfo> paperInfo;
            public bool IsValidated;
            public bool UseAltPrice; //US4543: added for alt price coupons
            //public string SerialNumber;
            //public int AuditNumber;
        }

        /// <summary>
        /// Represents a sale item sent to the server.
        /// </summary>
        protected struct AddSaleListItem
        {
            public int PackageId;
            public int DiscountId;
            public int CompAwardedId;
            public short ReceiptLine;
            public short Quantity;
            public int SessionPlayedId;
            public DateTime GamingDate;
            public int SessionNumber;
            public string Amount;
            public string Tax; // FIX: DE1938
//            public List<int> ProductIds; // PDTS 964
            public List<AddSaleProductItem> ProductItems;
            public bool IsQuantityDiscount; //US5117
            public bool IsDefaultValidationPackage;
        }
        #endregion

        #region Member Variables
        protected int m_bankId; // FIX: DE1930
        protected int m_authStaffId;
        protected int m_authLoginNum;
        protected string m_authMagCardNum;
        protected byte[] m_authPassword;
        protected int m_deviceId;
        protected int m_playerId;
        protected string m_saleCurrency; // Rally TA7465
        protected decimal m_amountTendered;
        protected decimal m_prepaidAmount;
        protected decimal m_pointQualifyingAmount;
        protected decimal m_pointsFromQualifyingAmount;
        protected decimal m_taxRate; // FIX: DE1938
        protected TransactionType m_transactionType;
        protected List<AddSaleListItem> m_items = new List<AddSaleListItem>();
        protected int m_registerReceiptId = 0;
        protected int m_transactionNumber;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the AddSaleMessage class.
        /// </summary>
        public AddSaleMessage()
        {
            m_id = 18009; // Add Sale
            m_strMessageName = "Add Sale";
        }
        #endregion

        #region Member Methods
        /// <summary>
        /// Adds a sale item to the message.
        /// </summary>
        /// <param name="registerReceiptId">The sale's register receipt ID (0 if we don't have one yet).</param>
        /// <param name="saleItem">The sale line item.</param>
        /// <param name="receiptLine">The receipt line number this item 
        /// shows up on.</param>
        /// <exception cref="System.ArgumentNullException">saleItem is a null 
        /// reference.</exception>
        public void AddSaleItem(int registerReceiptId, SaleItem saleItem, short receiptLine)
        {
            if(saleItem == null)
                throw new ArgumentNullException("saleItem");

            m_registerReceiptId = registerReceiptId;

            AddSaleListItem item = new AddSaleListItem();

            if(saleItem.IsPackageItem)
            {
                item.PackageId = saleItem.Package.Id;
                item.DiscountId = 0;
                item.CompAwardedId = 0;
                item.IsDefaultValidationPackage = saleItem.Package.IsSessionDefaultValidationPackage;
            }
            else if (saleItem.IsCoupon)              
            {
                if (saleItem.Coupon.CouponType == PlayerComp.CouponTypes.AltPricePackage || saleItem.Coupon.CouponType == PlayerComp.CouponTypes.PercentPackage)
                    item.PackageId = saleItem.Coupon.PackageID;
                else
                    item.PackageId = 0;

                item.DiscountId = 0;
                item.CompAwardedId = saleItem.CouponAwardID;
            }
            else if(saleItem.IsDiscount)
            {
                item.PackageId = 0;
                item.DiscountId = saleItem.Discount.Id;
                item.CompAwardedId = 0;
            }
            else 
            {
                item.PackageId = 0;
                item.DiscountId = 0;
                item.CompAwardedId = 0;
            }

            item.ReceiptLine = receiptLine;
            item.Quantity = (short)saleItem.Quantity;
            item.SessionPlayedId = saleItem.Session.SessionPlayedId;
            item.SessionNumber = saleItem.Session.SessionNumber;
            item.GamingDate = saleItem.Session.GamingDate ?? DateTime.MinValue;

            //set flag if its a quantity discount (BOGO)
            item.IsQuantityDiscount = saleItem.IsPackageItem && saleItem.Package.AppliedDiscountId != 0;

            // FIX: DE2957
            // Amount
            if(saleItem.IsPackageItem && saleItem.Package.HasOpenCredit)
            {
                // If the package contains an open credit product, then that is
                // what the amount is.
                foreach(Product product in saleItem.Package.GetProducts())
                {
                    if(product.Type == ProductType.CreditRefundableOpen ||
                       product.Type == ProductType.CreditNonRefundableOpen)
                    {
                        item.Amount = product.PricePaid.ToString("N", CultureInfo.InvariantCulture);
                        break;
                    }
                }
            }
            else 
            {
                item.Amount = saleItem.PricePaid.ToString("N", CultureInfo.InvariantCulture);
            }
            // END: DE2957

            item.Tax = saleItem.CalculateTaxes(m_taxRate).ToString("N", CultureInfo.InvariantCulture); // FIX: DE1938

            if(item.Amount.Length > StringSizes.MaxDecimalLength)
                throw new ArgumentException("Amount" + Resources.TooBig);

            // PDTS 964
            // Product Ids
            item.ProductItems = new List<AddSaleProductItem>();

            if(saleItem.IsPackageItem)
            {
                foreach(Product product in saleItem.Package.GetProducts())
                {
                    AddSaleProductItem itemData = new AddSaleProductItem
                    {
                        paperInfo = new List<PaperPackInfo>(),
                        ProductId = product.Id,
                        UseAltPrice = product.UseAltPrice, //US4543: added for alt price coupons
                    };

                    if (product is PaperBingoProduct)
                    {
                        PaperBingoProduct paperProduct = product as PaperBingoProduct;

                        // This is a paper bingo product so all of the pack info data needs
                        //  to be set for each product being sold.
                        itemData.IsValidated = paperProduct.IsValidated;
                        itemData.paperInfo.Clear();

                        foreach (PaperPackInfo info in paperProduct.PackInfo)
                        {
                            itemData.paperInfo.Add(info);
                        }
                    }
                    else if (product is ElectronicBingoProduct)
                    {
                        var electronicBingo = product as ElectronicBingoProduct;
                        itemData.IsValidated = electronicBingo.IsValidated;
                    }

                    item.ProductItems.Add(itemData);
                }
            }

            m_items.Add(item);
        }

        /// <summary>
        /// Prepares the request to be sent to the server.
        /// </summary>
        protected override void PackRequest()
        {
            // Create the streams we will be writing to.
            MemoryStream requestStream = new MemoryStream();
            BinaryWriter requestWriter = new BinaryWriter(requestStream, Encoding.Unicode);

            // Register receipt ID
            requestWriter.Write(m_registerReceiptId);

            // Authorization Staff Id
            requestWriter.Write(m_authStaffId);

            // Authorization Login Number
            requestWriter.Write(m_authLoginNum);

            // Authorization Magnetic Card Number
            if(!string.IsNullOrEmpty(m_authMagCardNum))
            {
                requestWriter.Write((ushort)m_authMagCardNum.Length);
                requestWriter.Write(m_authMagCardNum.ToCharArray());
            }
            else
                requestWriter.Write((ushort)0);

            // Authorization Password Hash
            byte[] hashBuffer = new byte[DataSizes.PasswordHash];

            if(m_authPassword != null)
                Array.Copy(m_authPassword, hashBuffer, DataSizes.PasswordHash);

            requestWriter.Write(hashBuffer);

            // Device Id
            requestWriter.Write(m_deviceId);

            // Player Id
            requestWriter.Write(m_playerId);

            // Transaction Type Id
            requestWriter.Write((int)m_transactionType);

            // Bank Id
            requestWriter.Write(m_bankId); // FIX: DE1930

            // Rally TA7465
            // Sale Currency ISO Code
            if (!string.IsNullOrEmpty(m_saleCurrency))
            {
                requestWriter.Write((ushort)m_saleCurrency.Length);
                requestWriter.Write(m_saleCurrency.ToCharArray());
            }
            else
            {
                requestWriter.Write((ushort)0);
            }

            // Amount Tendered
            string tempDec = m_amountTendered.ToString("N", CultureInfo.InvariantCulture);
            requestWriter.Write((ushort)tempDec.Length);
            requestWriter.Write(tempDec.ToCharArray());

            // Prepaid amount
            tempDec = PrepaidAmount.ToString("N", CultureInfo.InvariantCulture);
            requestWriter.Write((ushort)tempDec.Length);
            requestWriter.Write(tempDec.ToCharArray());

            // Point Qualifying Amount
            tempDec = m_pointQualifyingAmount.ToString("N", CultureInfo.InvariantCulture);
            requestWriter.Write((ushort)tempDec.Length);
            requestWriter.Write(tempDec.ToCharArray());

            // Points from Qualifying Amount
            tempDec = m_pointsFromQualifyingAmount.ToString("N", CultureInfo.InvariantCulture);
            requestWriter.Write((ushort)tempDec.Length);
            requestWriter.Write(tempDec.ToCharArray());

            // Count of items.
            requestWriter.Write((ushort)m_items.Count);

            // Add all the items.
            foreach(AddSaleListItem item in m_items)
            {
                // Package Id
                requestWriter.Write(item.PackageId);

                // Discount Id
                requestWriter.Write(item.DiscountId);

                //CompAwarded Id
                requestWriter.Write(item.CompAwardedId);

                // Receipt Line #
                requestWriter.Write(item.ReceiptLine);

                // Quantity
                requestWriter.Write(item.Quantity);

                // Session Played Id
                requestWriter.Write(item.SessionPlayedId);

                //gaming date
                string dateString = item.GamingDate.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);

                requestWriter.Write((ushort)dateString.Length);
                requestWriter.Write(dateString.ToCharArray());

                //session
                requestWriter.Write(item.SessionNumber);

                // Session default validation package
                requestWriter.Write(item.IsDefaultValidationPackage);

                //Is Quantity Discount (BOGO) US5117
                requestWriter.Write(item.IsQuantityDiscount);

                // Amount
                requestWriter.Write((ushort)item.Amount.Length);
                requestWriter.Write(item.Amount.ToCharArray());

                // FIX: DE1938
                requestWriter.Write((ushort)item.Tax.Length);
                requestWriter.Write(item.Tax.ToCharArray());
                // END: DE1938

                // Product Data count
                requestWriter.Write((ushort)item.ProductItems.Count);

                // Product Ids List
                foreach (AddSaleProductItem productItem in item.ProductItems)
                {
                    requestWriter.Write(productItem.ProductId);
                    requestWriter.Write(productItem.IsValidated);
                    requestWriter.Write(productItem.UseAltPrice);

                    requestWriter.Write((ushort)productItem.paperInfo.Count);
                    foreach (PaperPackInfo info in productItem.paperInfo)
                    {
                        if (!string.IsNullOrEmpty(info.SerialNumber))
                        {
                            requestWriter.Write((ushort)info.SerialNumber.Length);
                            requestWriter.Write(info.SerialNumber.ToCharArray());
                        }
                        else
                        {
                            requestWriter.Write((ushort)0);
                        }

                        requestWriter.Write(info.AuditNumber);
                    }
                }
            }

            // Set the bytes to be sent.
            m_requestPayload = requestStream.ToArray();

            // Close the streams.
            requestWriter.Close();
        }

        /// <summary>
        /// Parses the response received from the server.
        /// </summary>
        protected override void UnpackResponse()
        {
            m_registerReceiptId = 0;
            m_transactionNumber = 0;

            base.UnpackResponse();

            // Create the streams we will be reading from.
            MemoryStream responseStream = new MemoryStream(m_responsePayload);
            BinaryReader responseReader = new BinaryReader(responseStream, Encoding.Unicode);

            // Check the response length.
            if(responseStream.Length != ResponseMessageLength)
                throw new MessageWrongSizeException(m_strMessageName);

            // Try to unpack the data.
            try
            {
                // Seek past return code.
                responseReader.BaseStream.Seek(sizeof(int), SeekOrigin.Begin);

                // Register Receipt Id
                m_registerReceiptId = responseReader.ReadInt32();

                // Transaction #
                m_transactionNumber = responseReader.ReadInt32();
            }
            catch(EndOfStreamException e)
            {
                throw new MessageWrongSizeException(m_strMessageName, e);
            }
            catch(Exception e)
            {
                throw new ServerException(m_strMessageName, e);
            }

            // Close the streams.
            responseReader.Close();
        }
        #endregion

        #region Member Properties
        // FIX: DE1930
        /// <summary>
        /// Gets or sets the id bank the sale is made with.
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

        /// <summary>
        /// Gets or sets the id for the staff who is authorizing this sale (if 
        /// needed).  If 0 then AuthLoginNumber or AuthMagCardNumber will be 
        /// used.
        /// </summary>
        public int AuthStaffId
        {
            get
            {
                return m_authStaffId;
            }
            set
            {
                m_authStaffId = value;
            }
        }

        /// <summary>
        /// Gets or sets the login number for the staff who is authorizing this 
        /// sale (if needed).  If 0 then AuthStaffId or AuthMagCardNumber will 
        /// be used.
        /// </summary>
        public int AuthLoginNumber
        {
            get
            {
                return m_authLoginNum;
            }
            set
            {
                m_authLoginNum = value;
            }
        }

        /// <summary>
        /// Gets or sets the magnetic card number for the staff who is 
        /// authorizing this sale (if needed).  If null, then AuthStaffId or 
        /// AuthLoginNumber will be used.
        /// </summary>
        public string AuthMagCardNumber
        {
            get
            {
                return m_authMagCardNum;
            }
            set
            {
                m_authMagCardNum = value;
            }
        }

        /// <summary>
        /// Gets or sets the password hash for the staff who is authorizing this 
        /// sale (if needed).
        /// </summary>
        public byte[] AuthLoginPassword
        {
            get
            {
                return m_authPassword;
            }
            set
            {
                if(value != null && value.Length != DataSizes.PasswordHash)
                    throw new ArgumentException("AuthLoginPassword" + Resources.WrongSize);

                m_authPassword = value;
            }
        }

        /// <summary>
        /// Gets/sets amount prepaid (includes tax).
        /// </summary>
        public decimal PrepaidAmount
        {
            get
            {
                return m_prepaidAmount;
            }

            set
            {
                m_prepaidAmount = value;
            }
        }

        /// <summary>
        /// Gets or sets the id of the device that was sold to.
        /// </summary>
        public int DeviceId
        {
            get
            {
                return m_deviceId;
            }
            set
            {
                m_deviceId = value;
            }
        }

        /// <summary>
        /// Gets or sets the id of the player that was sold to.
        /// </summary>
        public int PlayerId
        {
            get
            {
                return m_playerId;
            }
            set
            {
                m_playerId = value;
            }
        }

        /// <summary>
        /// Gets or sets the transaction type of this sale.
        /// </summary>
        public TransactionType TransactionType
        {
            get
            {
                return m_transactionType;
            }
            set
            {
                m_transactionType = value;
            }
        }

        // Rally TA7465
        /// <summary>
        /// Gets or sets the ISO code of the currency used to make the sale.
        /// </summary>
        public string SaleCurrency
        {
            get
            {
                return m_saleCurrency;
            }
            set
            {
                m_saleCurrency = value;
            }
        }

        /// <summary>
        /// Gets or sets the amount tendered for the sale.
        /// </summary>
        public decimal AmountTendered
        {
            get
            {
                return m_amountTendered;
            }
            set
            {
                // Check to make sure it's not too big to fit in a string.
                string tempDec = value.ToString("N", CultureInfo.InvariantCulture);

                if(tempDec.Length <= StringSizes.MaxDecimalLength)
                    m_amountTendered = value;
                else
                    throw new ArgumentException("AmountTendered" + Resources.TooBig);
            }
        }

        // FIX: DE1938
        /// <summary>
        /// Gets or sets the tax rate charged for the sale.
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
        // END: DE1938

        /// <summary>
        /// Gets the id of the sale on the server.
        /// </summary>
        public int RegisterReceiptId
        {
            get
            {
                return m_registerReceiptId;
            }
        }

        /// <summary>
        /// Gets the transaction number of the sale on the server.
        /// </summary>
        public int TransactionNumber
        {
            get
            {
                return m_transactionNumber;
            }
        }

        /// <summary>
        /// Gets or sets the amount which qualifies for earning additional points.
        /// </summary>
        public decimal PointQualifyingAmount
        {
            get
            {
                return m_pointQualifyingAmount;
            }

            set
            {
                m_pointQualifyingAmount = value;
            }
        }

        /// <summary>
        /// Sets the points earned from the point qualifying amount.
        /// </summary>
        public decimal PointsFromQualifyingAmount
        {
            set
            {
                m_pointsFromQualifyingAmount = value;
            }
        }
        #endregion
    }
}
