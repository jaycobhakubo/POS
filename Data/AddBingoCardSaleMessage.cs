#region Copyright
// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2008 - 2014 FortuNet dba
// GameTech International, Inc.
#endregion

// US2826 Adding support for being able to sell barcoded paper

using System;
using System.IO;
using System.Text;
using GTI.Modules.Shared;
using GTI.Modules.POS.Business;
using GTI.Modules.POS.Properties;
using System.Collections.Generic;

namespace GTI.Modules.POS.Data
{
    // PDTS 964
    /// <summary>
    /// The possible status return codes from the Add [UK] Bingo Card Sale 
    /// server message.
    /// </summary>
    internal enum AddBingoCardSaleReturnCode
    {
        InsufficientCards = -62,
        SessionOutOfSync = -64,
        MissingSerialLookup = -65,
        MissingPermLib = -71, // FIX: DE4037
        // Rally US510 - Pre-Printed Pack Sales
        ErrorInvalidPermRange = -80,
        ErrorSavingCardsToDb = -81,
        InvalidHallId = -85,
        PackInUse = -89, // FIX: DE2951
        InvalidGameCategory = -97 // FIX: DE6691
    }

    internal enum AddBingoCardSaleType
    {
        ElectronicSaleType  = 0,
        PrePrintedPackType  = 1,
        PlayWithPaperType   = 2,
        BarcodedPaperType   = 3,
    }

    // Rally TA5748
    /// <summary>
    /// Represents an Add [UK] Bingo Card Sale server message.
    /// </summary>
    internal class AddBingoCardSaleMessage : ServerMessage
    {
        #region Member Variables
        protected bool m_mainStageMode;
        protected bool m_prePrintedPack;
        protected bool m_playWithPaper;
        protected int m_registerReceiptId;
        // Rally US510
        protected int m_prePrintedHallId;
        protected byte m_prePrintedPermVersion;
        protected int m_prePrintedStartsNum;
        protected ushort m_prePrintedCardCount;
        protected int m_prePrintedDAStartsNum;
        protected ushort m_prePrintedDACardCount;
        protected string m_prePrintedBarcode;       // DE2951
        protected int m_packNumber;
        protected Dictionary<int, Dictionary<int, List<int>>> m_startNums = new Dictionary<int, Dictionary<int, List<int>>>();

        protected Dictionary<int, Dictionary<int, List<PaperPackInfo>>> m_paperInfo = new Dictionary<int, Dictionary<int, List<PaperPackInfo>>>();
        protected AddBingoCardSaleType m_saleType;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the AddBingoCardSaleMessage class 
        /// with the specified register receipt id.
        /// </summary>
        /// <param name="mainStageMode">Whether the UK version of this message 
        /// should be called.</param>
        /// <param name="prePrintedPack">Whether pre-printed pack information
        /// is to be sent to the server.</param>
        /// <param name="playWithPaper">Whether Play with Paper start numbers
        /// are to be sent to the server.</param>
        /// <param name="registerReceptId">The register receipt id that the 
        /// cards will be attached to.</param>
        public AddBingoCardSaleMessage(bool mainStageMode, bool prePrintedPack, bool playWithPaper, bool barcodedPaper, int registerReceptId)
        {
            if(!mainStageMode)
            {
                m_id = 6019; // Add Bingo Card Sale
                m_strMessageName = "Add Bingo Card Sale";
            }
            else
            {
                m_id = 32012; // Add UK Bingo Card Sale
                m_strMessageName = "Add UK Bingo Card Sale";
            }

            m_mainStageMode = mainStageMode;
            m_prePrintedPack = prePrintedPack;
            m_playWithPaper = playWithPaper;
            m_registerReceiptId = registerReceptId;

            m_saleType = AddBingoCardSaleType.ElectronicSaleType;
            if (prePrintedPack) m_saleType = AddBingoCardSaleType.PrePrintedPackType;
            if (playWithPaper) m_saleType = AddBingoCardSaleType.PlayWithPaperType;
            //if (barcodedPaper) m_saleType = AddBingoCardSaleType.BarcodedPaperType;
        }

        #endregion

        #region Member Methods
        /// <summary>
        /// Adds start number to the list to be sent to the server.
        /// </summary>
        /// <param name="sessionPlayedId">The session the product is being sold
        /// in.</param>
        /// <param name="productId">The product that is being sold.</param>
        /// <param name="startNumbers">A collection of start numbers for the
        /// session/product.</param>
        public void AddStartNumbers(int sessionPlayedId, int productId, IEnumerable<StartNumber> startNumbers)
        {
            // Does the session exist?
            if(!m_startNums.ContainsKey(sessionPlayedId))
                m_startNums.Add(sessionPlayedId, new Dictionary<int, List<int>>());

            Dictionary<int, List<int>> products = m_startNums[sessionPlayedId];

            // Does the product exist?
            if(!products.ContainsKey(productId))
                products.Add(productId, new List<int>());

            List<int> nums = products[productId];

            if(startNumbers != null)
            {
                foreach(StartNumber startNumber in startNumbers)
                {
                    nums.Add(startNumber.Number);
                }
            }
        }

        /// <summary>
        /// Adds the pack info to the data that is to be sent to the server
        /// </summary>
        /// <param name="sessionPlayedId">The session the product is being sold in.</param> 
        /// <param name="productId">The product that is being sold</param>
        /// <param name="paperPacks">A collection of Paper pack info for the session/product</param>
        public void AddPackInfo(int sessionPlayedId, int productId, IEnumerable<PaperPackInfo> paperPacks)
        {
            if (paperPacks != null && m_saleType == AddBingoCardSaleType.BarcodedPaperType)
            {
                // Does this session Exist
                if(!m_paperInfo.ContainsKey(sessionPlayedId))
                    m_paperInfo.Add(sessionPlayedId, new Dictionary<int,List<PaperPackInfo>>());

                Dictionary<int, List<PaperPackInfo>> products = m_paperInfo[sessionPlayedId];
                
                // Does this product exist
                if(!products.ContainsKey(productId))
                    products.Add(productId, new List<PaperPackInfo>());

                List<PaperPackInfo> packInfo = products[productId];

                foreach(PaperPackInfo info in paperPacks)
                {
                    packInfo.Add(info);
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

            // Write the sale type so that the server knows how to 
            //  unpack the request
            requestWriter.Write((byte)m_saleType);
            
            // Register Receipt Id
            requestWriter.Write(m_registerReceiptId);

            // Rally US510
            if(!m_mainStageMode && m_prePrintedPack)
            {
                // Pre-Printed Pack Hall Id
                requestWriter.Write(m_prePrintedHallId);

                // Pre-Printed Pack Perm Version
                requestWriter.Write(m_prePrintedPermVersion);

                // Pre-Printed Starting Card Number
                requestWriter.Write(m_prePrintedStartsNum);

                // Pre-Printed Card Count
                requestWriter.Write(m_prePrintedCardCount);

                // Pre-Printed DA Starting Card Number
                requestWriter.Write(m_prePrintedDAStartsNum);

                // Pre-Printed DA Card Count
                requestWriter.Write(m_prePrintedDACardCount);

                // DE2951 
                // Write the Pre-Printed Pack Barcode 
                if (!string.IsNullOrEmpty(m_prePrintedBarcode))
                {
                    requestWriter.Write((ushort)m_prePrintedBarcode.Length);
                    requestWriter.Write(m_prePrintedBarcode.ToCharArray());
                }
                else
                {
                    requestWriter.Write((ushort)0);
                }
            }
            else if(!m_mainStageMode && m_playWithPaper)
            {
                // Session Count
                requestWriter.Write((ushort)m_startNums.Keys.Count);

                foreach(KeyValuePair<int, Dictionary<int, List<int>>> session in m_startNums)
                {
                    // Session Played Id
                    requestWriter.Write(session.Key);

                    // Product Count
                    requestWriter.Write((ushort)session.Value.Count);

                    foreach(KeyValuePair<int, List<int>> product in session.Value)
                    {
                        // Product Id
                        requestWriter.Write(product.Key);

                        // Start Number Count
                        requestWriter.Write((ushort)product.Value.Count);

                        foreach(int number in product.Value)
                        {
                            // Start Number
                            requestWriter.Write(number);
                        }
                    }
                }
            }
            else if (!m_mainStageMode && m_saleType == AddBingoCardSaleType.BarcodedPaperType)
            {
                // Session Count
                requestWriter.Write((ushort)m_paperInfo.Keys.Count);

                foreach (KeyValuePair<int, Dictionary<int, List<PaperPackInfo>>> session in m_paperInfo)
                {
                    // Session Id
                    requestWriter.Write(session.Key);

                    // Product Count
                    requestWriter.Write((ushort)session.Value.Count);

                    foreach (KeyValuePair<int, List<PaperPackInfo>> product in session.Value)
                    {
                        // Product Id
                        requestWriter.Write(product.Key);

                        // Pack Info Count
                        requestWriter.Write((ushort)product.Value.Count);

                        foreach (PaperPackInfo packInfo in product.Value)
                        {
                            // Audit number
                            requestWriter.Write(packInfo.AuditNumber);

                            // Serial number
                            if (!string.IsNullOrEmpty(packInfo.SerialNumber))
                            {
                                requestWriter.Write((ushort)packInfo.SerialNumber.Length);
                                requestWriter.Write(packInfo.SerialNumber.ToCharArray());
                            }
                            else
                            {
                                requestWriter.Write((ushort) 0);
                            }
                        }

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
            m_packNumber = 0;

            base.UnpackResponse();

            // Create the streams we will be reading from.
            MemoryStream responseStream = new MemoryStream(m_responsePayload);
            BinaryReader responseReader = new BinaryReader(responseStream, Encoding.Unicode);

            // Try to unpack the data.
            try
            {
                // Seek past return code.
                responseReader.BaseStream.Seek(sizeof(int), SeekOrigin.Begin);

                // Pack Number (skip if in Main Stage mode).
                if(!m_mainStageMode)
                    m_packNumber = responseReader.ReadInt32();
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
        /// Gets whether this message is the UK version.
        /// </summary>
        public bool MainStageMode
        {
            get
            {
                return m_mainStageMode;
            }
        }

        /// <summary>
        /// Gets or sets the id of the sale on the server.
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

        // Rally US510
        /// <summary>
        /// Gets or sets the id of the hall the pre-printed pack belongs to
        /// (or 0 if not using it).
        /// </summary>
        public int PrePrintedHallId
        {
            get
            {
                return m_prePrintedHallId;
            }
            set
            {
                m_prePrintedHallId = value;
            }
        }

        /// <summary>
        /// Gets or sets the perm. version to use with pre-printed pack sales
        /// (or 0 if not using it).
        /// </summary>
        public byte PrePrintedPermVersion
        {
            get
            {
                return m_prePrintedPermVersion;
            }
            set
            {
                m_prePrintedPermVersion = value;
            }
        }

        /// <summary>
        /// Gets or sets the starting standard card number to use with
        /// pre-printed pack sales (or 0 if not using it or have no cards).
        /// </summary>
        public int PrePrintedStartsNumber
        {
            get
            {
                return m_prePrintedStartsNum;
            }
            set
            {
                m_prePrintedStartsNum = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of standard cards to sell with pre-printed
        /// pack sales (or 0 if not using it or have no cards).
        /// </summary>
        public ushort PrePrintedCardCount
        {
            get
            {
                return m_prePrintedCardCount;
            }
            set
            {
                m_prePrintedCardCount = value;
            }
        }

        /// <summary>
        /// Gets or sets the starting Double Action card number to use with
        /// pre-printed pack sales (or 0 if not using it or have no cards).
        /// </summary>
        public int PrePrintedDAStartsNumber
        {
            get
            {
                return m_prePrintedDAStartsNum;
            }
            set
            {
                m_prePrintedDAStartsNum = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of Double Action cards to sell with
        /// pre-printed pack sales (or 0 if not using it or have no cards).
        /// </summary>
        public ushort PrePrintedDACardCount
        {
            get
            {
                return m_prePrintedDACardCount;
            }
            set
            {
                m_prePrintedDACardCount = value;
            }
        }

        // DE2951
        /// <summary>
        /// Gets or sets the pre-printed pack barcode or blank if not
        /// using pre-printed pack sales.
        /// </summary>
        public string PrePrintedBarcode
        {
            get
            {
                return m_prePrintedBarcode;
            }
            set
            {
                m_prePrintedBarcode = value;
            }
        }
        
        
        /// <summary>
        /// Gets the pack number generated for the sale.
        /// </summary>
        public int PackNumber
        {
            get
            {
                return m_packNumber;
            }
        }
        #endregion
    }
    // END: TA5748
}
