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
    // PDTS 964

    /// <summary>
    /// Parses menu data received from the server.
    /// </summary>
    internal static class MenuParser
    {
        #region Static Methods

        /// <summary>
        /// Parses menu data received from the server.
        /// </summary>
        /// <param name="pos">The instance of the PointOfSale to associate 
        /// menu buttons with.</param>
        /// <param name="displayMode">The DisplayMode used to create the 
        /// menus.</param>
        /// <param name="reader">A BinaryReader containing the data to be 
        /// parsed.  The reader's position must start at the count 
        /// of menus.</param>
        /// <param name="gamingDate">Gaming Date</param>
        /// <returns>An array of POSMenuListItems read from the binary 
        /// stream or null if no menus were returned.</returns>
        /// <exception cref="System.ArgumentNullException">pos, dispalyMode, or
        /// reader are null references.</exception>
        /// <exception cref="GTI.Modules.POS.Business.POSException">An unknown 
        /// type related to the button was receieved.</exception>
        public static POSMenuListItem[] Parse(PointOfSale pos, DisplayMode displayMode, BinaryReader reader, DateTime gamingDate)
        {
            if(pos == null)
                throw new ArgumentNullException("pos");
            else if(displayMode == null)
                throw new ArgumentNullException("displayMode");
            else if(reader == null)
                throw new ArgumentNullException("reader");

            List<POSMenuListItem> menus = new List<POSMenuListItem>();

            // Get the count of menus.
            ushort menuCount = reader.ReadUInt16();

            // Get all the menus.
            for(ushort x = 0; x < menuCount; x++)
            {
                // Rally DE1025 - Blank menus if more than one session shared a menu.
                // POS Menu Id
                int menuId = reader.ReadInt32();
                POSMenu existingMenu = new POSMenu(menuId, null, displayMode.MenuButtonsPerPage);
                POSMenuListItem menuListItem = new POSMenuListItem
                {
                    Menu = existingMenu
                };

                // Menu Name
                ushort stringLen = reader.ReadUInt16();
                menuListItem.Menu.Name = new string(reader.ReadChars(stringLen));

                // Session Number
               var sessionNumber = reader.ReadInt16();

                // Session Played Id
                var sessionPlayedId = reader.ReadInt32();

                //US5328: Get Category max card limits per game
                // Tuple:
                //        Item1 = Session Game Played ID
                //        Item2 = Game Category ID
                //        Item3 = Max Card Limt
                var gameCategoriesMaxCardLimitPerGame =
                    GetCategoryMaxCardLimitsPerGameMessage.GetCategoryMaxCardLimitPerGame(sessionPlayedId);

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

                // Program Name
                stringLen = reader.ReadUInt16();
                var programName = new string(reader.ReadChars(stringLen));
                
                //Max Validation
                var isMaxValidationEnabled = reader.ReadBoolean();

                var isAutoDiscountsEnabled = reader.ReadBoolean();

                var isDeviceFeesEnabled = reader.ReadBoolean();

                var pointsMultiplier = reader.ReadInt32();

                //US5287:session Max card limit
                var sessionMaxCardLimit = reader.ReadInt32();

                var session = new SessionInfo(sessionNumber, 
                    sessionPlayedId, 
                    programName, 
                    isMaxValidationEnabled, 
                    isDeviceFeesEnabled,
                    isAutoDiscountsEnabled,
                    pointsMultiplier,
                    sessionMaxCardLimit,
                    false, //isPreSale
                    gameToGameCategoriesDictionary,
                    gameCategoryList,
                    gamingDate); //gaming date

                menuListItem.Session = session;

                menus.Add(menuListItem);
            }

            // Rally TA1045
            // Get the list of buttons.
            menuCount = reader.ReadUInt16();

            // Read all the buttons for a particular menu.
            for(ushort x = 0; x < menuCount; x++)
            {
                // POS Menu Id
                int menuId = reader.ReadInt32();
                List<POSMenuListItem> menuItems = menus.FindAll(i => i.Menu.Id == menuId);
                long menuDefStart = reader.BaseStream.Position;
                bool haveDefaultValidationPackage = false;

                foreach(POSMenuListItem item in menuItems)
                {
                    haveDefaultValidationPackage = false;

                    reader.BaseStream.Position = menuDefStart;

                    // Get the count of buttons.
                    ushort buttonCount = reader.ReadUInt16();

                    // Get all the buttons.
                    for (ushort y = 0; y < buttonCount; y++)
                    {
                        MenuButton button;
                        
                        // Package Id
                        int packageId = reader.ReadInt32();

                        // Function Id
                        int functionId = reader.ReadInt32();

                        // Discount Id
                        int discountId = reader.ReadInt32();

                        // Determine which type based on the id.
                        // PDTS 964
                        if (packageId != 0)
                            button = new PackageButton(pos, new Package(), item.Session);
                        else if (functionId != 0)
                            button = new FunctionButton(pos, functionId, null);
                        else if (discountId != 0)
                            button = new DiscountButton(pos, new FixedDiscount()); // Create a temp. discount for now.
                        else
                            throw new POSException(Resources.UnknownButton);

                        // Create As casts once to increase performance.
                        PackageButton packageButton = button as PackageButton;
                        DiscountButton discountButton = button as DiscountButton;

                        // Page Number
                        byte page = reader.ReadByte();

                        // Key Num
                        byte position = reader.ReadByte();

                        // Are we in another mode?
                        RearrangeButton(displayMode, ref page, ref position);
                        button.Page = page;
                        button.Position = position;

                        // Key Text
                        ushort stringLen = reader.ReadUInt16();
                        button.Text = new string(reader.ReadChars(stringLen));

                        // Key Color
                        button.Color = Color.FromArgb(reader.ReadInt32());

                        // Key Locked
                        button.IsLocked = reader.ReadBoolean();

                        // Player Required
                        // Rally DE129
                        button.IsPlayerRequired = reader.ReadBoolean();

                        // Graphic Id
                        button.GraphicId = reader.ReadInt32();

                        //Use as default validation package for menu
                        bool defaultValidationPackage = reader.ReadBoolean();
                        bool isValidationPackage = false;

                        if (packageButton != null)
                        {
                            packageButton.Package.Id = packageId;
                            packageButton.Package.DisplayText = button.Text;
                        }

                        // Discount Type Id
                        int discountTypeId = reader.ReadInt32();

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
                        stringLen = reader.ReadUInt16();
                        string tempDec = new string(reader.ReadChars(stringLen));

                        if (discountButton != null && !string.IsNullOrEmpty(tempDec))
                            discountButton.Discount.Amount = decimal.Parse(tempDec, CultureInfo.InvariantCulture);

                        // Discount Points Per Dollar
                        stringLen = reader.ReadUInt16();
                        tempDec = new string(reader.ReadChars(stringLen));

                        if (discountButton != null && !string.IsNullOrEmpty(tempDec))
                            discountButton.Discount.PointsPerDollar = decimal.Parse(tempDec, CultureInfo.InvariantCulture) * item.Session.PointsMultiplier;

                        // Get the package data.
                        if (packageButton != null)
                        {
                            // US2018 - Charge Device Fee
                            packageButton.Package.ChargeDeviceFee = reader.ReadBoolean();

                            // Package Receipt Text
                            stringLen = reader.ReadUInt16();
                            packageButton.Package.ReceiptText = new string(reader.ReadChars(stringLen));

                            //Override Validation
                            packageButton.Package.OverrideValidation = reader.ReadBoolean();

                            //Validation Quantity
                            packageButton.Package.OverrideValidationQuantity = reader.ReadInt32();

                            //Requires Validation
                            packageButton.Package.RequiresValidation = reader.ReadBoolean();

                            // Get the products associated to this package.
                            ushort productCount = reader.ReadUInt16();

                            // Get all the products.
                            for (ushort z = 0; z < productCount; z++)
                            {
                                Product product = null;

                                // Product Type
                                ProductType productType = (ProductType)reader.ReadInt32();

                                if(productType == ProductType.Validation)
                                    isValidationPackage = true;
  
                                // Product Id
                                int productId = reader.ReadInt32();

                                // Card Media
                                int cardMedia = reader.ReadInt32();

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
                                int cardType = reader.ReadInt32();

                                // Game Type
                                int gameType = reader.ReadInt32();

                                BingoProduct bingoProd = product as BingoProduct;

                                if (bingoProd != null)
                                {
                                    bingoProd.CardType = (CardType)cardType;
                                    bingoProd.GameType = (GameType)gameType;
                                }

                                // Game Category Id
                                int gameCategoryId = reader.ReadInt32();

                                // Rally US738
                                // Card Level Id
                                int cardLevelId = reader.ReadInt32();

                                // Rally US505
                                if (bingoProd != null)
                                {
                                    bingoProd.GameCategoryId = gameCategoryId;
                                    bingoProd.CardLevelId = cardLevelId;
                                }

                                // Product Name
                                stringLen = reader.ReadUInt16();
                                product.Name = new string(reader.ReadChars(stringLen));

                                // Is Taxed
                                product.IsTaxed = reader.ReadBoolean();

                                // Price
                                stringLen = reader.ReadUInt16();
                                tempDec = new string(reader.ReadChars(stringLen));

                                if (!string.IsNullOrEmpty(tempDec))
                                    product.Price = decimal.Parse(tempDec, CultureInfo.InvariantCulture);

                                // Quantity
                                product.Quantity = reader.ReadByte();

                                // Card Count
                                short cardCount = reader.ReadInt16();

                                // Optional
                                product.Optional = reader.ReadBoolean();

                                // Numbers Required
                                short numsReq = reader.ReadInt16();

                                if (bingoProd != null)
                                {
                                    bingoProd.CardCount = cardCount;
                                    bingoProd.NumbersRequired = numsReq;
                                }

                                // Points Per Dollar
                                stringLen = reader.ReadUInt16();
                                tempDec = new string(reader.ReadChars(stringLen));

                                if (!string.IsNullOrEmpty(tempDec))
                                    product.PointsPerDollar = decimal.Parse(tempDec, CultureInfo.InvariantCulture);

                                if (!((product.Price < 0) != (product.PointsPerDollar < 0))) //points will be positive = earned
                                    product.PointsPerDollar *= item.Session.PointsMultiplier;

                                // Points Per Product
                                stringLen = reader.ReadUInt16();
                                tempDec = new string(reader.ReadChars(stringLen));

                                if (!string.IsNullOrEmpty(tempDec))
                                    product.PointsPerProduct = decimal.Parse(tempDec, CultureInfo.InvariantCulture);

                                if (product.PointsPerProduct >= 0) //points will be positive = earned
                                    product.PointsPerProduct *= item.Session.PointsMultiplier;

                                // Points To Redeem
                                stringLen = reader.ReadUInt16();
                                tempDec = new string(reader.ReadChars(stringLen));

                                if (!string.IsNullOrEmpty(tempDec))
                                    product.PointsToRedeem = decimal.Parse(tempDec, CultureInfo.InvariantCulture);

                                if (product.PointsPerProduct < 0) //points will be negative = redeemed, move them into points to redeem
                                {
                                    product.PointsToRedeem += Math.Abs(product.PointsPerProduct);
                                    product.PointsPerProduct = 0;
                                }
                                    
                                // Skip Package Code
                                stringLen = reader.ReadUInt16();
                                reader.ReadChars(stringLen);

                                //barcoded Paper
                                var isbarcodedPaper = reader.ReadBoolean();

                                //is validated
                                var isValidatable = reader.ReadBoolean();

                                //get alternative price
                                var altPriceLength = reader.ReadInt16();
                                decimal altPrice;
                                decimal.TryParse(new string(reader.ReadChars(altPriceLength)), out altPrice);
                                product.AltPrice = altPrice;

                                //get qualifying product flag
                                product.IsQualifyingProduct = reader.ReadBoolean();

                                //get prepaid flag
                                product.Prepaid = reader.ReadBoolean();

                                if (bingoProd != null)
                                {
                                    // US2826 is this barcoded paper
                                    bingoProd.BarcodedPaper = isbarcodedPaper;
                                    // US3509 Validate 
                                    bingoProd.CanValidateProduct = isValidatable;
                                }

                                // Compatible Devices
                                CompatibleDevices devices = (CompatibleDevices)reader.ReadInt32();

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

            if(menus.Count == 0)
                return null;
            else
                return menus.ToArray();
        }

        /// <summary>
        /// Changes the page and position values based on the specified
        /// DisplayMode.
        /// </summary>
        /// <param name="displayMode">The DisplayMode used to show the 
        /// buttons.</param>
        /// <param name="page">The button's page number.</param>
        /// <param name="position">The button's position number.</param>
        public static void RearrangeButton(DisplayMode displayMode, ref byte page, ref byte position)
        {
            // Are we in another mode?

            if (displayMode.MenuPagesPerPOSPage != 1)
            {
                int newPage = (int)((page - 1) / displayMode.MenuPagesPerPOSPage) + 1;

                position += (byte)(((page - 1) % displayMode.MenuPagesPerPOSPage) * (displayMode.MenuButtonsPerPage / displayMode.MenuPagesPerPOSPage));
                page = (byte)newPage;

                return;
            }

            
            if (displayMode is WideDisplayMode && ((WideDisplayMode)displayMode).TwoPagesPerPage)
            {
                if (page % 2 == 0) //even page
                {
                    page = (byte)(page / 2);
                    position += (byte)(displayMode.MenuButtonsPerPage / 2);
                }
                else
                {
                    page = (byte)((page + 1) / 2);
                }
            }

            if(displayMode is CompactDisplayMode)
            {
                // Rearrange the menu buttons because the menu 
                // size is different.
                if (position < displayMode.MenuButtonsPerPage)
                {
                    page = (byte)((page * 2) - 1);
                }
                else if (position >= displayMode.MenuButtonsPerPage)
                {
                    page *= 2;
                    position -= displayMode.MenuButtonsPerPage;
                }
            }
        }
        #endregion
    }
    
    /// <summary>
    /// Represents a Get Daily Staff Menus server message.
    /// </summary>
    internal class GetStaffMenusMessage : ServerMessage
    {
        #region Constants and Data Types
        protected const int MinResponseMessageLength = 6;
        #endregion

        #region Member Variables
        protected PointOfSale m_pos;
        protected DateTime m_gamingDate;
        protected DisplayMode m_displayMode;
        protected POSMenuListItem[] m_menus;
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
        public GetStaffMenusMessage(PointOfSale pos, DisplayMode displayMode, DateTime gamingDate)
        {
            if(displayMode == null)
                throw new ArgumentNullException("displayMode");

            m_id = 18040; // Get Daily Staff Menus
            m_strMessageName = "Get Daily Staff Menus";
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

            // Gaming Date
            string tempDate = m_gamingDate.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);

            requestWriter.Write((ushort)tempDate.Length);
            requestWriter.Write(tempDate.ToCharArray());

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
            m_menus = null;

            base.UnpackResponse();

            // Create the streams we will be reading from.
            MemoryStream responseStream = new MemoryStream(m_responsePayload);
            BinaryReader responseReader = new BinaryReader(responseStream, Encoding.Unicode);

            // Check the response length.
            if(responseStream.Length < MinResponseMessageLength)
                throw new MessageWrongSizeException(m_strMessageName);

            // Try to unpack the data.
            try
            {
                // Seek past return code.
                responseReader.BaseStream.Seek(sizeof(int), SeekOrigin.Begin);

                // Parse the menu data.
                m_menus = MenuParser.Parse(m_pos, m_displayMode, responseReader, m_gamingDate);
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
                if(m_pos == null)
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
                if(value == null)
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
        public POSMenuListItem[] Menus
        {
            get
            {
                return m_menus;
            }
        }
        #endregion
    }
}
