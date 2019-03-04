#region Copyright
// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2008 GameTech
// International, Inc.
#endregion

//US4543: Award products in coupons
//US4721: (US4324) POS: Support scheduling discounts & coupons
//US4719: (US4717) POS: Support scheduling device fees
//US4724: (US4722) POS: Support point multiplier

using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Globalization;
using System.Collections.Generic;
using GTI.Modules.Shared;
using GTI.Modules.POS.Business;
using GTI.Modules.POS.Properties;
using GTI.Modules.Shared.Data;

namespace GTI.Modules.POS.Data
{
    /// <summary>
    /// Represents a Get Daily Staff Menus server message.
    /// </summary>
    internal class GetPreSaleStaffMenusMessage : ServerMessage
    {
        #region Constants and Data Types
        protected const int MinResponseMessageLength = 6;
        #endregion

        #region Member Variables
        protected PointOfSale m_pos;
        protected DateTime m_gamingDate;
        protected DisplayMode m_displayMode;
        protected List<POSMenuListItem> m_menus;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the GetStaffMenusMessage class
        /// with the specified display mode and gaming date.
        /// </summary>
        /// <param name="pos">The instance of the PointOfSale to associate 
        /// menu buttons with.</param>
        /// <param name="displayMode">The DisplayMode used to create 
        /// menus.</param>
        /// <param name="gamingDate">The gaming date to get the menus 
        /// for.</param>
        /// <exception cref="System.ArgumentNullException">display mode is a 
        /// null reference.</exception>
        public GetPreSaleStaffMenusMessage(PointOfSale pos, DisplayMode displayMode, DateTime gamingDate)
        {
            if (displayMode == null)
                throw new ArgumentNullException("displayMode");

            m_id = 18257; // Get Daily Staff Menus
            m_strMessageName = "Get Daily Pre-sale Staff Menus";
            m_pos = pos;
            m_displayMode = displayMode;
            m_gamingDate = gamingDate;
        }
        #endregion

        #region Member Methods
        /// <summary>
        /// Prepares the request to be sent to the server.
        /// </summary>
        protected override void PackRequest()
        {
            // Create the streams we will be writing to.
            MemoryStream requestStream = new MemoryStream();
            BinaryWriter requestWriter = new BinaryWriter(requestStream, Encoding.Unicode);

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
            // Clear the menu array.
            m_menus = new List<POSMenuListItem>();

            base.UnpackResponse();

            // Create the streams we will be reading from.
            MemoryStream responseStream = new MemoryStream(m_responsePayload);
            BinaryReader responseReader = new BinaryReader(responseStream, Encoding.Unicode);

            // Check the response length.
            if (responseStream.Length < MinResponseMessageLength)
                throw new MessageWrongSizeException(m_strMessageName);

            // Try to unpack the data.
            try
            {
                // Seek past return code.
                responseReader.BaseStream.Seek(sizeof(int), SeekOrigin.Begin);

                // Parse the menu data.

                // Get the count of menus.
                ushort menuCount = responseReader.ReadUInt16();
                // Get all the menus.
                for (ushort x = 0; x < menuCount; x++)
                {
                    int menuId = responseReader.ReadInt32();

                    POSMenu posMenu = new POSMenu(menuId, null, m_displayMode.MenuButtonsPerPage);
                    POSMenuListItem menuListItem = new POSMenuListItem
                    {
                        Menu = posMenu,
                    };

                    // Menu Name
                    ushort stringLen = responseReader.ReadUInt16();
                    menuListItem.Menu.Name = new string(responseReader.ReadChars(stringLen));
                    
                    //Gaming Date
                    stringLen = responseReader.ReadUInt16();
                    var gamingDate = DateTime.Parse(new string(responseReader.ReadChars(stringLen)));
                    
                    // Session Number
                    var sessionNumber = responseReader.ReadInt16();
                    
                    // Program Name
                    stringLen = responseReader.ReadUInt16();
                    var programName = new string(responseReader.ReadChars(stringLen));

                    //Max Validation
                    var isMaxValidationEnabled = responseReader.ReadBoolean();

                    var isAutoDiscountsEnabled = responseReader.ReadBoolean();

                    var isDeviceFeesEnabled = responseReader.ReadBoolean();

                    var pointsMultiplier = responseReader.ReadInt32();

                    //US5287:session Max card limit
                    var sessionMaxCardLimit = responseReader.ReadInt32();
                    
                    //US5328: Get Category max card limits per game
                    // Tuple:
                    //        Item1 = Session Game Played ID
                    //        Item2 = Game Category ID
                    //        Item3 = Max Card Limt
                    var gameCategoriesMaxCardLimitPerGame =
                        GetPreSaleCategoryMaxCardLimitsPerGameMessage.GetCategoryMaxCardLimitPerGame(sessionNumber, gamingDate);

                    var gameToGameCategoriesDictionary = new Dictionary<int, List<int>>();
                    var gameCategoryList = new List<GameCategory>();
                    foreach (var tuple in gameCategoriesMaxCardLimitPerGame)
                    {
                        //create a dictionary of games : game categories
                        //if session game played id does not exists
                        if (!gameToGameCategoriesDictionary.ContainsKey(tuple.Item1))
                        {
                            //add key: session game played ID and value: game category ID
                            gameToGameCategoriesDictionary[tuple.Item1] = new List<int> { tuple.Item2.Id };
                        }
                        else
                        {
                            //add to existing session game played ID
                            gameToGameCategoriesDictionary[tuple.Item1].Add(tuple.Item2.Id);
                        }

                        //create a disctionary of game category : max card limit
                        //if game category ID does not exist
                        if (!gameCategoryList.Exists(g => g.Id == tuple.Item2.Id))
                        {
                            //add key: game category ID and Value: max card limit
                            gameCategoryList.Add(tuple.Item2);
                        }
                    }
                    var session = new SessionInfo(sessionNumber,
                        0, // session played id is zero for pre sale menus
                        programName,
                        isMaxValidationEnabled,
                        isDeviceFeesEnabled,
                        isAutoDiscountsEnabled,
                        pointsMultiplier,
                        sessionMaxCardLimit,
                        true, //isPreSale
                        gameToGameCategoriesDictionary,
                        gameCategoryList,
                        gamingDate); //gaming date

                    menuListItem.Session = session;

                    m_menus.Add(menuListItem);
                }

                // Get the count of menus.
                menuCount = responseReader.ReadUInt16();

                // Read all the buttons for a particular menu.
                for (ushort x = 0; x < menuCount; x++)
                {
                    // POS Menu Id
                    int menuId = responseReader.ReadInt32();
                    List<POSMenuListItem> menuItems = m_menus.FindAll(i => i.Menu.Id == menuId);
                    long menuDefStart = responseReader.BaseStream.Position;
                    bool haveDefaultValidationPackage = false;

                    foreach (POSMenuListItem item in menuItems)
                    {
                        haveDefaultValidationPackage = false;

                        responseReader.BaseStream.Position = menuDefStart;

                        // Get the count of buttons.
                        ushort buttonCount = responseReader.ReadUInt16();

                        // Get all the buttons.
                        for (ushort y = 0; y < buttonCount; y++)
                        {
                            MenuButton button;

                            // Package Id
                            int packageId = responseReader.ReadInt32();

                            // Function Id
                            int functionId = responseReader.ReadInt32();

                            // Discount Id
                            int discountId = responseReader.ReadInt32();

                            // Determine which type based on the id.
                            // PDTS 964
                            if (packageId != 0)
                                button = new PackageButton(m_pos, new Package(), item.Session);
                            else if (functionId != 0)
                                button = new FunctionButton(m_pos, functionId, null);
                            else if (discountId != 0)
                                button = new DiscountButton(m_pos, new FixedDiscount()); // Create a temp. discount for now.
                            else
                                throw new POSException(Resources.UnknownButton);

                            // Create As casts once to increase performance.
                            PackageButton packageButton = button as PackageButton;
                            DiscountButton discountButton = button as DiscountButton;

                            // Page Number
                            byte page = responseReader.ReadByte();

                            // Key Num
                            byte position = responseReader.ReadByte();

                            // Are we in another mode?
                            MenuParser.RearrangeButton(m_displayMode, ref page, ref position);
                            button.Page = page;
                            button.Position = position;

                            // Key Text
                            ushort stringLen = responseReader.ReadUInt16();
                            button.Text = new string(responseReader.ReadChars(stringLen));

                            // Key Color
                            button.Color = Color.FromArgb(responseReader.ReadInt32());

                            // Key Locked
                            button.IsLocked = responseReader.ReadBoolean();

                            // Player Required
                            // Rally DE129
                            button.IsPlayerRequired = responseReader.ReadBoolean();

                            // Graphic Id
                            button.GraphicId = responseReader.ReadInt32();

                            //Use as default validation package for menu
                            bool defaultValidationPackage = responseReader.ReadBoolean();
                            bool isValidationPackage = false;

                            if (packageButton != null)
                            {
                                packageButton.Package.Id = packageId;
                                packageButton.Package.DisplayText = button.Text;
                            }

                            // Discount Type Id
                            int discountTypeId = responseReader.ReadInt32();

                            if (discountButton != null)
                            {
                                // Determine the type of discount.
                                var discount = DiscountFactory.CreateDiscount((DiscountType)discountTypeId);

                                if (discount == null)
                                {
                                    throw new POSException(Resources.UnknownDiscount);
                                }

                                discountButton.Discount = discount;
                                discountButton.Discount.Id = discountId;
                            }

                            // Discount Amount
                            stringLen = responseReader.ReadUInt16();
                            string tempDec = new string(responseReader.ReadChars(stringLen));

                            if (discountButton != null && !string.IsNullOrEmpty(tempDec))
                                discountButton.Discount.Amount = decimal.Parse(tempDec, CultureInfo.InvariantCulture);

                            // Discount Points Per Dollar
                            stringLen = responseReader.ReadUInt16();
                            tempDec = new string(responseReader.ReadChars(stringLen));

                            if (discountButton != null && !string.IsNullOrEmpty(tempDec))
                                discountButton.Discount.PointsPerDollar = decimal.Parse(tempDec, CultureInfo.InvariantCulture) * item.Session.PointsMultiplier;

                            // Get the package data.
                            if (packageButton != null)
                            {
                                // US2018 - Charge Device Fee
                                packageButton.Package.ChargeDeviceFee = responseReader.ReadBoolean();

                                // Package Receipt Text
                                stringLen = responseReader.ReadUInt16();
                                packageButton.Package.ReceiptText = new string(responseReader.ReadChars(stringLen));

                                //Override Validation
                                packageButton.Package.OverrideValidation = responseReader.ReadBoolean();

                                //Validation Quantity
                                packageButton.Package.OverrideValidationQuantity = responseReader.ReadInt32();

                                //Requires Validation
                                //packageButton.Package.RequiresValidation = reader.ReadBoolean();

                                // Get the products associated to this package.
                                ushort productCount = responseReader.ReadUInt16();

                                // Get all the products.
                                for (ushort z = 0; z < productCount; z++)
                                {
                                    Product product = null;

                                    // Product Type
                                    ProductType productType = (ProductType)responseReader.ReadInt32();

                                    if (productType == ProductType.Validation)
                                        isValidationPackage = true;

                                    // Product Id
                                    int productId = responseReader.ReadInt32();

                                    // Card Media
                                    int cardMedia = responseReader.ReadInt32();

                                    // Create a new object based on the type.
                                    switch (productType)
                                    {
                                        case ProductType.CrystalBallQuickPick:
                                        case ProductType.CrystalBallScan:
                                        case ProductType.CrystalBallHandPick:
                                        case ProductType.CrystalBallPrompt:
                                            // Rally TA9165
                                            if ((CardMedia)cardMedia == CardMedia.Electronic)
                                                product = new ElectronicBingoProduct();
                                            else
                                                product = new BingoProduct();

                                            break;

                                        case ProductType.Electronic: // Rally TA7626
                                            product = new ElectronicBingoProduct();
                                            break;
                                        // END: TA9165

                                        case ProductType.Paper: // Barcoded paper support
                                            product = new PaperBingoProduct();
                                            break;

                                        default:
                                            product = new Product();
                                            break;
                                    }

                                    product.Type = productType;
                                    product.Id = productId;

                                    // Card Type
                                    int cardType = responseReader.ReadInt32();

                                    // Game Type
                                    int gameType = responseReader.ReadInt32();

                                    BingoProduct bingoProd = product as BingoProduct;

                                    if (bingoProd != null)
                                    {
                                        bingoProd.CardType = (CardType)cardType;
                                        bingoProd.GameType = (GameType)gameType;
                                    }

                                    // Game Category Id
                                    int gameCategoryId = responseReader.ReadInt32();

                                    // Rally US738
                                    // Card Level Id
                                    int cardLevelId = responseReader.ReadInt32();

                                    // Rally US505
                                    if (bingoProd != null)
                                    {
                                        bingoProd.GameCategoryId = gameCategoryId;
                                        bingoProd.CardLevelId = cardLevelId;
                                    }

                                    // Product Name
                                    stringLen = responseReader.ReadUInt16();
                                    product.Name = new string(responseReader.ReadChars(stringLen));

                                    // Is Taxed
                                    product.IsTaxed = responseReader.ReadBoolean();

                                    // Price
                                    stringLen = responseReader.ReadUInt16();
                                    tempDec = new string(responseReader.ReadChars(stringLen));

                                    if (!string.IsNullOrEmpty(tempDec))
                                        product.Price = decimal.Parse(tempDec, CultureInfo.InvariantCulture);

                                    // Quantity
                                    product.Quantity = responseReader.ReadByte();

                                    // Card Count
                                    short cardCount = responseReader.ReadInt16();

                                    // Optional
                                    product.Optional = responseReader.ReadBoolean();

                                    // Numbers Required
                                    short numsReq = responseReader.ReadInt16();

                                    if (bingoProd != null)
                                    {
                                        bingoProd.CardCount = cardCount;
                                        bingoProd.NumbersRequired = numsReq;
                                    }

                                    // Points Per Dollar
                                    stringLen = responseReader.ReadUInt16();
                                    tempDec = new string(responseReader.ReadChars(stringLen));

                                    if (!string.IsNullOrEmpty(tempDec))
                                        product.PointsPerDollar = decimal.Parse(tempDec, CultureInfo.InvariantCulture);

                                    if (!((product.Price < 0) != (product.PointsPerDollar < 0))) //points will be positive = earned
                                        product.PointsPerDollar *= item.Session.PointsMultiplier;

                                    // Points Per Product
                                    stringLen = responseReader.ReadUInt16();
                                    tempDec = new string(responseReader.ReadChars(stringLen));

                                    if (!string.IsNullOrEmpty(tempDec))
                                        product.PointsPerProduct = decimal.Parse(tempDec, CultureInfo.InvariantCulture);

                                    if (product.PointsPerProduct >= 0) //points will be positive = earned
                                        product.PointsPerProduct *= item.Session.PointsMultiplier;

                                    // Points To Redeem
                                    stringLen = responseReader.ReadUInt16();
                                    tempDec = new string(responseReader.ReadChars(stringLen));

                                    if (!string.IsNullOrEmpty(tempDec))
                                        product.PointsToRedeem = decimal.Parse(tempDec, CultureInfo.InvariantCulture);

                                    if (product.PointsPerProduct < 0) //points will be negative = redeemed, move them into points to redeem
                                    {
                                        product.PointsToRedeem += Math.Abs(product.PointsPerProduct);
                                        product.PointsPerProduct = 0;
                                    }

                                    // Skip Package Code
                                    stringLen = responseReader.ReadUInt16();
                                    responseReader.ReadChars(stringLen);

                                    //barcoded Paper
                                    var isbarcodedPaper = responseReader.ReadBoolean();

                                    //is validated
                                    var isValidatable = responseReader.ReadBoolean();

                                    //get alternative price
                                    var altPriceLength = responseReader.ReadInt16();
                                    decimal altPrice;
                                    decimal.TryParse(new string(responseReader.ReadChars(altPriceLength)), out altPrice);
                                    product.AltPrice = altPrice;

                                    //get qualifying product flag
                                    product.IsQualifyingProduct = responseReader.ReadBoolean();

                                    //get prepaid flag
                                    product.Prepaid = responseReader.ReadBoolean();

                                    if (bingoProd != null)
                                    {
                                        // US2826 is this barcoded paper
                                        bingoProd.BarcodedPaper = isbarcodedPaper;
                                        // US3509 Validate 
                                        bingoProd.CanValidateProduct = isValidatable;
                                    }

                                    // Compatible Devices
                                    CompatibleDevices devices = (CompatibleDevices)responseReader.ReadInt32();

                                    IElectronicProduct elecProd = product as IElectronicProduct;

                                    if (elecProd != null)
                                        elecProd.CompatibleDevices = devices;

                                    packageButton.Package.AddProduct(product);
                                }
                            }

                            if (!haveDefaultValidationPackage && defaultValidationPackage && isValidationPackage)
                            {
                                packageButton.Package.IsSessionDefaultValidationPackage = true;
                                haveDefaultValidationPackage = true;
                            }

                            item.Menu.AddButton(button.Page, button.Position, button);
                        }
                    }
                }
            }
            catch
                (EndOfStreamException e)
            {
                throw new MessageWrongSizeException(m_strMessageName, e);
            }
            catch (Exception e)
            {
                throw new ServerException(m_strMessageName, e);
            }

            // Close the streams.
             responseReader.Close();
        }
        #endregion

        #region Member Properties
        /// <summary>
        /// Gets or sets the PointOfSale instance to assoicate buttons to.
        /// </summary>
        public PointOfSale POS
        {
            get
            {
                return m_pos;
            }
            set
            {
                if (m_pos == null)
                    throw new ArgumentNullException("POS");

                m_pos = value;
            }
        }

        /// <summary>
        /// Gets or sets the DisplayMode used to create menus.
        /// </summary>
        public DisplayMode DisplayMode
        {
            get
            {
                return m_displayMode;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("DisplayMode");
                else
                    m_displayMode = value;
            }
        }

        /// <summary>
        /// Gets or sets the gaming date to get the menus for.
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
        /// Gets the list of menus retrieved from the server.
        /// </summary>
        public List<POSMenuListItem> Menus
        {
            get
            {
                return m_menus;
            }
        }
        #endregion
    }
}
