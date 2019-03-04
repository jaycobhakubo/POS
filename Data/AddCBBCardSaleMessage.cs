#region Copyright
// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2008 GameTech
// International, Inc.
#endregion

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using GTI.Modules.Shared;
using GTI.Modules.POS.Business;

// Rally US229
// Rally US505

namespace GTI.Modules.POS.Data
{
    // FIX: DE2679
    /// <summary>
    /// The possible status return codes from the Add CBB Card Sales 
    /// server message.
    /// </summary>
    internal enum AddCBBCardSaleReturnCode
    {
        InsufficientCards = -62,
        ErrorSavingCardsToDb = -81,
        InvalidGameCategory = -97 // FIX: DE6691
    }
    // END: DE2679

    /// <summary>
    /// Represents an Add CBB Card Sales server message.
    /// </summary>
    internal class AddCBBCardSaleMessage : ServerMessage
    {
        #region Constants and Data Types
        protected const int ResponseMessageLength = 8;
        #endregion

        // Rally TA6385
        #region Member Variables
        protected bool m_sendCardNums; 
        protected int m_registerReceiptId;
        protected int m_packNum;
        protected Dictionary<int, Dictionary<short, List<CrystalBallCard>>> m_cardsToSend = new Dictionary<int, Dictionary<short, List<CrystalBallCard>>>();
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the AddCBBCardSaleMessage class 
        /// with the specified register receipt id and pack number.
        /// </summary>
        /// <param name="sendCardNumbers">Whether to send a card number to the
        /// server with the rest of the card data.</param>
        /// <param name="registerReceiptId">The register receipt id that the 
        /// cards will be attached to.</param>
        /// <param name="packNumber">The pack number the sales is attached to
        /// or 0 if no pack number has been created.</param>
        public AddCBBCardSaleMessage(bool sendCardNumbers, int registerReceiptId, int packNumber)
        {
            m_id = 6004; // Add CBB Card Sales
            m_strMessageName = "Add CBB Card Sales";
            m_sendCardNums = sendCardNumbers;
            m_registerReceiptId = registerReceiptId;
            m_packNum = packNumber;
        }
        #endregion

        #region Member Methods
        /// <summary>
        /// Adds CBB cards to be sent to the server.
        /// </summary>
        /// <param name="cards">An list containing card data.</param>
        public void AddCards(IEnumerable<CrystalBallCardCollection> cards)
        {
            if(cards != null)
            {
                foreach(CrystalBallCardCollection coll in cards)
                {
                    // Is this game category new?
                    if(!m_cardsToSend.ContainsKey(coll.GameCategoryId))
                        m_cardsToSend.Add(coll.GameCategoryId, new Dictionary<short, List<CrystalBallCard>>());

                    // Is this pick count new?
                    Dictionary<short, List<CrystalBallCard>> picks = m_cardsToSend[coll.GameCategoryId];

                    if(!picks.ContainsKey(coll.NumbersRequired))
                        picks.Add(coll.NumbersRequired, new List<CrystalBallCard>());

                    picks[coll.NumbersRequired].AddRange(coll);
                }
            }
        }

        /// <summary>
        /// Prepares the request to be sent to the server.
        /// </summary>
        protected override void PackRequest()
        {
            // Create the streams we will be writing to.
            MemoryStream requestStream = new MemoryStream();
            BinaryWriter requestWriter = new BinaryWriter(requestStream, Encoding.Unicode);

            // Register Receipt Id
            requestWriter.Write(m_registerReceiptId);

            // Pack Number
            requestWriter.Write(m_packNum);

            // Count of Game Categories
            requestWriter.Write((ushort)m_cardsToSend.Count);

            // Packages
            foreach(KeyValuePair<int, Dictionary<short, List<CrystalBallCard>>> gameCategories in m_cardsToSend)
            {
                // Game Category Id
                requestWriter.Write(gameCategories.Key);

                // Count of Numbers Required
                requestWriter.Write((ushort)gameCategories.Value.Count);

                foreach(KeyValuePair<short, List<CrystalBallCard>> picks in gameCategories.Value)
                {
                    // Numbers Required (Pick Count)
                    requestWriter.Write(picks.Key);

                    // Count of Cards
                    requestWriter.Write((ushort)picks.Value.Count);

                    foreach(CrystalBallCard card in picks.Value)
                    {
                        // Card Media
                        requestWriter.Write((int)card.Media);

                        // Is Quick Pick
                        requestWriter.Write(card.IsQuickPick);

                        // Is Favorite
                        requestWriter.Write(card.IsFavorite);

                        // Rally DE2312
                        // Product Type Id
                        requestWriter.Write((int)card.ProductType);

                        if(card.Face != null)
                        {
                            // Face Count
                            requestWriter.Write((byte)card.Face.Length);

                            // Face Numbers
                            requestWriter.Write(card.Face);
                        }
                        else
                            requestWriter.Write((byte)0);

                        // Card Number
                        if(m_sendCardNums)
                        {
                            requestWriter.Write(card.Number);
                        }
                    }
                }
            }
            
            // Set the bytes to be sent.
            m_requestPayload = requestStream.ToArray();

            // Close the streams.
            requestWriter.Close();
        }
        // END: TA6385

        /// <summary>
        /// Parses the response received from the server.
        /// </summary>
        protected override void UnpackResponse()
        {
            // Clear any existing data.
            m_packNum = 0;

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

                // Pack Number
                m_packNum = responseReader.ReadInt32();
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
        // Rally TA6385
        /// <summary>
        /// Gets or sets whether to send a card number to the server with the
        /// rest of the card data.
        /// </summary>
        public bool SendCardNumbers
        {
            get
            {
                return m_sendCardNums;
            }
            set
            {
                m_sendCardNums = value;
            }
        }

        /// <summary>
        /// Gets or sets the register receipt id that the cards will be
        /// attached to.
        /// </summary>
        public int RegisterReceiptId
        {
            get
            {
                return m_registerReceiptId;
            }
            set
            {
                m_registerReceiptId = value;
            }
        }

        /// <summary>
        /// Gets or sets the pack number for these cards (if applicable).  If 
        /// pack number was 0 upon send and one needs to be generated, then 
        /// this property will have the new pack number after the message 
        /// returns.
        /// </summary>
        public int PackNumber
        {
            get
            {
                return m_packNum;
            }
            set
            {
                m_packNum = value;
            }
        }
        #endregion
    }
}
