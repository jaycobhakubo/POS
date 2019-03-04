#region Copyright
// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2008-2009 GameTech
// International, Inc.
#endregion

using System;
using System.Windows.Forms;
using System.Globalization;
using System.Collections.Generic;
using GameTech.Elite.OpticalMarkReader;
using GTI.GTIDevices;
using GTI.GTIDevices.ExternalDevices;
using GTI.Modules.Shared;
using GTI.Modules.POS.UI;
using GTI.Modules.POS.Properties;

namespace GTI.Modules.POS.Business
{
    /// <summary>
    /// This class manages the choosing of Crystal Ball Bingo cards.
    /// </summary>
    internal class CrystalBallManager : IDisposable
    {
        #region Member Variables
        // Manager Related
        protected object m_syncRoot = new object();
        protected bool m_disposed;

        // Scanner Related
        protected PointOfSale m_parent;
        protected IOpticalMarkReader m_scanner;
        protected CardMedia m_media = CardMedia.Electronic;
        protected ProductType m_originalType; // Rally DE2312 - Selling two different types of CBB at the same time returns an error.
        protected int m_numOfCards;
        protected int m_numbersRequired; // Rally DE2961 - Don't fail entire scan sheet if some of the cards don't have enough numbers.
        protected bool m_scanning;
        protected List<CrystalBallCard> m_scannedCards = new List<CrystalBallCard>();
        protected bool m_readError;
        protected string m_readErrorMessage;

        // UI Related
        protected DisplayMode m_displayMode;
        protected CrystalBallScanForm m_scanForm;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the CrystalBallManager class.
        /// </summary>
        /// <param name="parent">The PointOfSale to which this object 
        /// belongs.</param>
        /// <param name="displayMode">The display mode used to show 
        /// forms.</param>
        /// <exception cref="System.ArgumentNullException">parent is a null 
        /// reference.</exception>
        public CrystalBallManager(PointOfSale parent)
        {
            if(parent == null)
                throw new ArgumentNullException("parent");

            m_parent = parent;
            m_displayMode = parent.Settings.DisplayMode;
        }
        #endregion

        #region Member Methods
        // Rally US505 & US507
        /// <summary>
        /// Prompts the user for Crystal Ball numbers based on the product 
        /// types contained in the package.
        /// </summary>
        /// <param name="sale">The sale the package is going to be added
        /// to.</param>
        /// <param name="package">The package that contains crystal ball 
        /// products.</param>
        /// <param name="quantity">The amount of packages being added.</param>
        /// <param name="owner">Any object that implements IWin32Window 
        /// that represents the top-level window that will own any modal 
        /// dialog boxes.</param>
        /// <returns>A list of CrystalBallCardCollection objects.</returns>
        /// <exception cref="System.Exception">sheetDef is not a valid sheet 
        /// definition file.</exception>
        /// <exception cref="GTI.Modules.POS.Business.POSUserCancelException">
        /// The user cancelled the processing of the crystal ball cards.
        /// </exception>
        /// <exception cref="GTI.Modules.POS.Business.POSException">A scanning 
        /// error occurred or an invalid CBB selection type was 
        /// found.</exception>
        public IEnumerable<CrystalBallCardCollection> ProcessCrystalBall(Sale sale, Package package, int quantity, IWin32Window owner)
        {
            if (m_parent.WeAreAPOSKiosk)
                m_parent.SellingForm.NotIdle(true);

            List<CrystalBallCardCollection> cardCollections = new List<CrystalBallCardCollection>();

            Product[] products = package.GetProducts();

            foreach(Product product in products)
            {
                bool useFavorites = false;
                BingoProduct bingoProd = product as BingoProduct;

                // FIX: DE2568 - Selling CBB with anything else causes an error.
                if(bingoProd != null && (bingoProd.Type == ProductType.CrystalBallHandPick ||
                   bingoProd.Type == ProductType.CrystalBallPrompt || bingoProd.Type == ProductType.CrystalBallQuickPick ||
                   bingoProd.Type == ProductType.CrystalBallScan)) // We only care about CBB cards.
                {
                // END: DE2568
                    List<CrystalBallCard> cards = new List<CrystalBallCard>();

                    // FIX: DE2312
                    m_originalType = product.Type;
                    ProductType cbbType = m_originalType;

                    if(cbbType == ProductType.CrystalBallPrompt)
                    {
                        bool allowFavorites = false;

                        if (m_parent.WeAreAPOSKiosk && sale != null && sale.Player != null && m_parent.Settings.EnableCBBFavorites)
                            allowFavorites = sale.Player.GetCBBFavoriteCount(bingoProd.NumbersRequired) - sale.GetCBBFavoriteCount(bingoProd.NumbersRequired) > 0;

                        if (!m_parent.Settings.CBBQuickPickEnabled && m_parent.Settings.CbbScannerType == SupportedOMRDevices.NO_DEVICE && !allowFavorites) //only option is hand pick
                        {
                            cbbType = ProductType.CrystalBallHandPick;
                        }
                        else // We have to ask the user what he wants to do.
                        {
                            cbbType = GetNumberEntryType(owner, allowFavorites, false);
                            useFavorites = cbbType == ProductType.CrystalBallPrompt;
                        }
                    }

                    int numCards = bingoProd.CardCount * bingoProd.Quantity * quantity; // FIX: DE6002 - POS ignores CBB product quantity.

                    if(numCards < 0)
                        numCards = 0;

                    bool isElec = (bingoProd is ElectronicBingoProduct);

                    // Find out if the player wants to use any favorites.
                    if(sale != null && sale.Player != null && m_parent.Settings.EnableCBBFavorites)
                    {
                        int favoritesUsed = sale.GetCBBFavoriteCount(bingoProd.NumbersRequired);

                        // How many favorites does the player have left?
                        int favoritesLeft = sale.Player.GetCBBFavoriteCount(bingoProd.NumbersRequired) - favoritesUsed;

                        // Does the player want to use his/her favorites?
                        if (favoritesLeft > 0) //we have favorites
                        {
                            bool useFavs = useFavorites;

                            if (!useFavs && (m_parent.WeAreNotAPOSKiosk || (m_parent.WeAreAPOSKiosk && m_originalType != ProductType.CrystalBallPrompt)))
                                useFavs = m_parent.ShowMessage(owner, m_displayMode, string.Format(CultureInfo.CurrentCulture, Resources.CBBFavoritePrompt, bingoProd.Name), POSMessageFormTypes.YesNo) == DialogResult.Yes;

                            if (useFavs)
                            {
                                // Use as many favorites as possible.
                                while (favoritesLeft > 0 && numCards > 0)
                                {
                                    cards.Add(new CrystalBallCard(0, isElec ? CardMedia.Electronic : CardMedia.Paper, null, false, true, m_originalType));
                                    favoritesLeft--;
                                    numCards--;
                                }
                            }
                        }
                    }

                    if(numCards > 0)
                    {
                        if (cbbType == ProductType.CrystalBallPrompt)
                            cbbType = GetNumberEntryType(owner, false, true);

                        switch(cbbType)
                        {
                            case ProductType.CrystalBallQuickPick:
                                // Since the server is going to pick the numbers, just
                                // create empty cards.
                                for(int x = 0; x < numCards; x++)
                                {
                                    CrystalBallCard card = new CrystalBallCard(0, isElec ? CardMedia.Electronic : CardMedia.Paper,
                                                                               null, true, false, m_originalType);
                                    cards.Add(card);
                                }
                                break;

                            case ProductType.CrystalBallScan:
                                if (m_parent.Settings.CbbScannerType == SupportedOMRDevices.NO_DEVICE) //no scanner
                                {
                                    m_parent.ShowMessage(owner, m_displayMode, Resources.CBBNoScannerDefined);
                                }
                                else
                                {
                                    // Start the scanning process.
                                    Scan(m_parent.Settings.CBBScannerPort, m_parent.Settings.CBBSheetDefinition,
                                         isElec ? CardMedia.Electronic : CardMedia.Paper,
                                         bingoProd.NumbersRequired, numCards, owner);

                                    // Add them to the collection.
                                    cards.AddRange(ScannedCards);
                                }

                                break;

                            case ProductType.CrystalBallHandPick:
                                // Create empty cards, then pass to the dialog 
                                // to choose numbers.
                                CrystalBallCard[] emptyCards = new CrystalBallCard[numCards];

                                for(int x = 0; x < numCards; x++)
                                {
                                    CrystalBallCard card = new CrystalBallCard(0, isElec ? CardMedia.Electronic : CardMedia.Paper, null, false, false, m_originalType);
                                    emptyCards[x] = card;
                                }
                                // END: DE2312
                                CrystalBallHandPickForm handPickForm = new CrystalBallHandPickForm(m_parent, emptyCards, bingoProd.NumbersRequired);

                                if(DialogResult.OK == handPickForm.ShowDialog(owner))
                                    cards.AddRange(emptyCards);
                                else
                                    throw new POSUserCancelException();

                                handPickForm.Dispose();
                                break;
                        }
                    }

                    // Check for an existing collection.
                    bool found = false;
                    CrystalBallCardCollection coll = new CrystalBallCardCollection(bingoProd.GameCategoryId, bingoProd.NumbersRequired);

                    for(int x = 0; x < cardCollections.Count; x++)
                    {
                        if(cardCollections[x] == coll)
                        {
                            found = true;
                            cardCollections[x].AddRange(cards);
                            break;
                        }
                    }

                    if(!found)
                    {
                        coll.AddRange(cards);
                        cardCollections.Add(coll);
                    }
                }
            }

            if (m_parent.WeAreAPOSKiosk)
                m_parent.SellingForm.NotIdle();

            return cardCollections;
        }

        private ProductType GetNumberEntryType(IWin32Window owner, bool allowFavorites, bool secondPrompt)
        {
            ProductType cbbType = ProductType.CrystalBallHandPick;

            if (!m_parent.Settings.CBBQuickPickEnabled && m_parent.Settings.CbbScannerType == SupportedOMRDevices.NO_DEVICE && !allowFavorites) //only option is hand pick
            {
                cbbType = ProductType.CrystalBallHandPick;
            }
            else // We have to ask the user what he wants to do.
            {
                CrystalBallPromptForm promptForm = new CrystalBallPromptForm(m_parent, allowFavorites, secondPrompt);
                DialogResult result = promptForm.ShowDialog(owner);

                if (result == DialogResult.Abort || result == DialogResult.Cancel)
                    throw new POSUserCancelException();

                if (promptForm.SelectionType != ProductType.CrystalBallQuickPick &&
                   promptForm.SelectionType != ProductType.CrystalBallScan &&
                   promptForm.SelectionType != ProductType.CrystalBallHandPick)
                    throw new POSException(Resources.InvalidCBBSelectionType);
                else
                    cbbType = promptForm.SelectionType;

                promptForm.Dispose();

                if (result == DialogResult.Yes)
                    cbbType = ProductType.CrystalBallPrompt;
            }

            return cbbType;
        }

        /// <summary>
        /// Shows the scanning dialog and starts the scanning process.
        /// </summary>
        /// <param name="portNumber">The port number the scanner is 
        /// attached to.</param>
        /// <param name="sheetDef">The location and name of the sheet definition 
        /// file.</param>
        /// <param name="media">The media the cards to be scanned.</param>
        /// <param name="numbersRequired">The amount of numbers that have to be 
        /// choosen to be considered a valid card.</param>
        /// <param name="numberOfCards">The number of cards to scan before 
        /// stopping.</param>
        /// <param name="owner">Any object that implements IWin32Window 
        /// that represents the top-level window that will own any modal 
        /// dialog boxes.</param>
        /// <exception cref="System.Exception">sheetDef is not a valid sheet 
        /// definition file.</exception>
        /// <exception cref="GTI.Modules.POS.Business.POSException">A scanning 
        /// error occurred.</exception>
        /// <exception cref="GTI.Modules.POS.Business.POSUserCancelException">
        /// The user cancelled the scan.</exception>
        protected void Scan(int portNumber, string sheetDef, CardMedia media, short numbersRequired, int numberOfCards, IWin32Window owner)
        {
            // Are we already scanning?
            lock(m_syncRoot)
            {
                if(m_scanning)
                    return;
            }

            // Rally DE2225 - Buttons with CBB Scan give Unhandled Exception Error when no CBB scanner hooked up.
            try
            {
                // First, reset the scanning objects.
                DisposeScanObjects();

                // Set the game parameters.
                m_media = media;
                m_numOfCards = numberOfCards;
                m_numbersRequired = numbersRequired; // Rally DE2961

                //get scanner
                var type = m_parent.Settings.CbbScannerType;
                m_scanner = OpticalMarkReaderFactory.GetOpticalMarkReader(type, sheetDef, portNumber);
                
                // Hookup to the scanner's events.
                m_scanner.DataAvailable += ScannerDataAvailable;
                m_scanner.ReadException += ScannerReadException;
                m_scanner.SerialDeviceCommError += ScannerSerialCommError;

                // Create the scanning form.
                m_scanForm = new CrystalBallScanForm(m_parent, this, numbersRequired);

                // How many cards do we scan for?
                m_scanForm.SetScanProgress(0, m_numOfCards);
            }
            catch(Exception ex)
            {
                throw new POSException(string.Format(CultureInfo.CurrentCulture, Resources.CBBReadError, ex.Message), ex);
            }

            lock(m_syncRoot)
            {
                m_scannedCards.Clear();

                m_scanner.Start();

                m_scanning = true;
                m_readError = false;
                m_readErrorMessage = null;
            }

            DialogResult result = m_scanForm.ShowDialog(owner);

            if(owner is Form)
                ((Form)owner).Update();

            lock(m_syncRoot)
            {
                m_scanning = false;
                m_scanner.ResetDevice();
            }

            // Clean up the use of scan objects.
            DisposeScanObjects();

            if(m_readError)
                throw new POSException(string.Format(CultureInfo.CurrentCulture, Resources.CBBReadError, m_readErrorMessage));
            else if(result == DialogResult.Cancel || result == DialogResult.Abort)
                throw new POSUserCancelException();
        }

        /// <summary>
        /// Attempt to clear a jam in the scanner.
        /// </summary>
        public void ClearJam()
        {
            m_scanner.ClearJam();

            if (m_scanner.Settings.Device == SupportedOMRDevices.PDI_VMR_138)
            {
                m_scanner.ResetDevice();
                m_scanner.Start();    
            }
        }

        // FIX: DE2478 - CBB Sale (Allow staff initiated finish with quick picks).
        /// <summary>
        /// Adds the difference between the current amout scanned cards with
        /// the total needed with quick picks and closes the scan form.
        /// </summary>
        public void FinishWithQuickPicks()
        {
            lock(m_syncRoot)
            {
                int cardsToAdd = m_numOfCards - m_scannedCards.Count;

                for(int x = 0; x < cardsToAdd; x++)
                {
                    CrystalBallCard card = new CrystalBallCard(0, Media, null, true, false, m_originalType);
                    m_scannedCards.Add(card);
                }
            }
        }
        // END: DE2478

        /// <summary>
        /// Handles the card scanner's DataAvailable event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">A ScanResultEventArgs object that contains the 
        /// event data.</param>
        private void ScannerDataAvailable(object sender, ScanResultEventArgs e)
        {
            for(int x = 0; x < e.ScannedData.CardCount; x++)
            {
                if(e.ScannedData[x] != null)
                {
                    if(e.ScannedData[x].SelectionType == CardSelectionType.QuickPick && m_parent.Settings.CBBQuickPickEnabled) // Rally DE6674 - Don't allow quick picks on sheets if it's disbled.
                    {
                        // Rally DE2312
                        CrystalBallCard card = new CrystalBallCard(0, Media, null, true, false, m_originalType);
                        m_scannedCards.Add(card);
                    }
                    else if(e.ScannedData[x].SelectionType == CardSelectionType.HandPick)
                    {
                        // Rally DE2312
                        CrystalBallCard card = new CrystalBallCard(0, Media, null, false, false, m_originalType);

                        List<byte> cardNums = new List<byte>();

                        foreach(object cardNum in e.ScannedData[x].CardNumberList.Values)
                        {
                            cardNums.Add((byte)cardNum);
                        }

                        // Does this card contain the required amount of 
                        // numbers?
                        if(cardNums.Count == NumbersRequired) // Rally DE2961
                        {
                            card.Face = cardNums.ToArray();

                            // Have these numbers already been picked?
                            bool add = true;

                            // Rally US505
                            if(m_scannedCards.Contains(card) || (m_parent.CurrentSale != null && m_parent.CurrentSale.CheckForCBBCard(card)))
                            {
                                // Prompt the user for what to do.
                                if(m_scanForm.InvokeRequired)
                                {
                                    PromptDelegate prompt = m_scanForm.PromptForDuplicates;
                                    add = (bool)m_scanForm.Invoke(prompt);
                                }
                                else
                                    add = m_scanForm.PromptForDuplicates();
                            }

                            if(add)
                            {
                                lock(m_syncRoot)
                                {
                                    m_scannedCards.Add(card);
                                }
                            }
                        }
                    }
                }

                int count = 0, numCards = 0;

                lock(m_syncRoot)
                {
                    count = m_scannedCards.Count;
                    numCards = m_numOfCards;
                }

                // Update the display.
                if(m_scanForm.InvokeRequired)
                {
                    ScanProgressDelegate scanProgress = new ScanProgressDelegate(m_scanForm.SetScanProgress);
                    m_scanForm.Invoke(scanProgress, new object[] { count, numCards });
                }
                else
                    m_scanForm.SetScanProgress(count, numCards);

                // Have we scanned enough cards in?
                if(count >= numCards)
                {
                    if(m_scanForm.InvokeRequired)
                    {
                        MethodInvoker finished = new MethodInvoker(m_scanForm.Finished);
                        m_scanForm.Invoke(finished);
                    }
                    else
                        m_scanForm.Finished();

                    return;
                }
            }

            if(m_scanner.Settings.ReadMode == ReadModes.SingleSheet)
            {
                m_scanner.ResetDevice();

                if (m_scanner.Settings.Device == SupportedOMRDevices.PDI_VMR_138)
                {
                    m_scanner.Start();
                }
            }
        }

        /// <summary>
        /// Handles the card scanner's ReadException event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">A ReadProcessErrorEventArgs object that contains 
        /// the event data.</param>
        private void ScannerReadException(object sender, ReadProcessErrorEventArgs e)
        {
            m_parent.Log("Crystal Ball scanner read error: " + e.Message, LoggerLevel.Warning);
            
            //START RALLY DE2961 quick pick
            //if both quick pick and hand pick are marked a message will be displayed to the user
            if(e.ReadProcessException == ReadProcessExceptions.CardValidationFailure  &&
                e.Message.Contains("Quick Pick") )
            {
                // Prompt the user for what to do.
                if (m_scanForm.InvokeRequired)
                {
                    CBBFailureDelegate prompt = new CBBFailureDelegate(m_scanForm.CBBQuickPickError);
                    m_scanForm.Invoke(prompt);
                }
                else
                    m_scanForm.CBBQuickPickError();
            }
            //END RALLY DE2961
            m_scanner.ResetDevice();

            if (m_scanner.Settings.Device == SupportedOMRDevices.PDI_VMR_138)
            {
                m_scanner.Start();
            }
        }

        /// <summary>
        /// Handles the serial port's communication error event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">A CommErrorEventArgs object that contains 
        /// the event data.</param>
        private void ScannerSerialCommError(object sender, CommErrorEventArgs e)
        {
            m_parent.Log("Crystal Ball scanner communication error: " + e.Message, LoggerLevel.Severe);

            if(m_scanning)
            {
                lock(m_syncRoot)
                {
                    m_readError = true;
                    m_readErrorMessage = e.Message;
                }

                if(m_scanForm.InvokeRequired)
                {
                    MethodInvoker finished = new MethodInvoker(m_scanForm.Finished);
                    m_scanForm.Invoke(finished);
                }
                else
                    m_scanForm.Finished();
            }
        }

        /// <summary>
        /// Disposes of the object related to scanning for cards.
        /// </summary>
        protected void DisposeScanObjects()
        {
            if(m_scanForm != null)
            {
                m_scanForm.Dispose();
                m_scanForm = null;
            }

            if(m_scanner != null)
            {
                m_scanner.DataAvailable -= ScannerDataAvailable;
                m_scanner.ReadException -= ScannerReadException;
                m_scanner.SerialDeviceCommError -= ScannerSerialCommError;

                //closes the scanner and calls dispose
                m_scanner.Close();
                m_scanner = null;
            }
        }

        /// <summary>
        /// Releases all resources used by CrystalBallManager.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all resources used by CrystalBallManager.
        /// </summary>
        /// <param name="disposing">Whether this function is being called from 
        /// user code.</param>
        private void Dispose(bool disposing)
        {
            if(!m_disposed)
            {
                if(disposing)
                {
                    DisposeScanObjects();
                }
            }

            m_disposed = true;
        }

        /// <summary>
        /// Destroys the instance of this class.
        /// </summary>
        ~CrystalBallManager()      
        {
            Dispose(false);
        }
        #endregion

        #region Member Properties
        /// <summary>
        /// Gets the media for the scanned Crystal Ball cards.
        /// </summary>
        protected CardMedia Media
        {
            get
            {
                lock(m_syncRoot)
                {
                    return m_media;
                }
            }
        }

        /// <summary>
        /// Gets the total number of cards to scan.
        /// </summary>
        protected int NumberOfCards
        {
            get
            {
                lock(m_syncRoot)
                {
                    return m_numOfCards;
                }
            }
        }

        // Rally DE2961
        /// <summary>
        /// Gets or sets the numbers required for the game.
        /// </summary>
        protected int NumbersRequired
        {
            get
            {
                lock(m_syncRoot)
                {
                    return m_numbersRequired;
                }
            }
        }

        /// <summary>
        /// Gets whether the scanner is currently running.
        /// </summary>
        public bool Scanning
        {
            get
            {
                lock(m_syncRoot)
                {
                    return m_scanning;
                }
            }
        }

        /// <summary>
        /// Gets whether a read error occurred while scanning.
        /// </summary>
        public bool ReadError
        {
            get
            {
                lock(m_syncRoot)
                {
                    return m_readError;
                }
            }
        }

        /// <summary>
        /// Gets the error message if a read error has occurred.
        /// </summary>
        public string ReadErrorMessage
        {
            get
            {
                lock(m_syncRoot)
                {
                    return m_readErrorMessage;
                }
            }
        }

        /// <summary>
        /// Gets all the cards scanned in.
        /// </summary>
        protected CrystalBallCard[] ScannedCards
        {
            get
            {
                lock(m_syncRoot)
                {
                    return m_scannedCards.ToArray();
                }
            }
        }
        #endregion
    }
}
