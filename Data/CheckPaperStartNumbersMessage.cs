#region Copyright
// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2008 GameTech
// International, Inc.
#endregion

// Rally TA5748

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GTI.Modules.Shared;
using GTI.Modules.POS.Business;

namespace GTI.Modules.POS.Data
{
    /// <summary>
    /// Represents a Check Paper Packs message.
    /// </summary>
    internal class CheckPaperStartNumbersMessage : ServerMessage
    {
        #region Member Variables
        protected List<ProductStartNumbers> m_startNumbers = new List<ProductStartNumbers>();
        protected List<ProductStartNumbers> m_checkedNumbers = new List<ProductStartNumbers>();
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the CheckPaperStartNumbersMessage 
        /// class.
        /// </summary>
        public CheckPaperStartNumbersMessage()
        {
            m_id = 6055; // Check Paper Packs
            m_strMessageName = "Check Paper Packs";
        }
        #endregion

        #region Member Methods
        /// <summary>
        /// Adds a product's start numbers to the list to check.
        /// </summary>
        /// <param name="startNumbers">The product's start numbers to
        /// check.</param>
        public void AddStartNumbers(ProductStartNumbers startNumbers)
        {
            m_startNumbers.Add(startNumbers);
        }

        /// <summary>
        /// Prepares the request to be sent to the server.
        /// </summary>
        protected override void PackRequest()
        {
            // Create the streams we will be writing to.
            MemoryStream requestStream = new MemoryStream();
            BinaryWriter requestWriter = new BinaryWriter(requestStream, Encoding.Unicode);

            // Product Count
            requestWriter.Write((ushort)m_startNumbers.Count);

            // Product Id/Start Numbers
            foreach(ProductStartNumbers nums in m_startNumbers)
            {
                // Session Played Id
                requestWriter.Write(nums.SessionPlayedId);

                // Daily Package Product Id
                requestWriter.Write(nums.ProductId);

                // Start Number Count
                requestWriter.Write((ushort)nums.StartNumbers.Count);

                foreach(StartNumber startNumber in nums.StartNumbers)
                {
                    // Start Number
                    requestWriter.Write(startNumber.Number);
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
            // Reset the values.
            m_checkedNumbers.Clear();

            base.UnpackResponse();

            // Create the streams we will be reading from.
            MemoryStream responseStream = new MemoryStream(m_responsePayload);
            BinaryReader responseReader = new BinaryReader(responseStream, Encoding.Unicode);

            // Try to unpack the data.
            try
            {
                // Seek past return code.
                responseReader.BaseStream.Seek(sizeof(int), SeekOrigin.Begin);

                // Get the count of products
                ushort productCount = responseReader.ReadUInt16();

                for(ushort x = 0; x < productCount; x++)
                {
                    ProductStartNumbers nums = new ProductStartNumbers();
                    nums.StartNumbers = new List<StartNumber>();

                    // Session Played Id
                    nums.SessionPlayedId = responseReader.ReadInt32();

                    // Daily Package Product Id
                    nums.ProductId = responseReader.ReadInt32();

                    // Count of start numbers.
                    ushort numCount = responseReader.ReadUInt16();

                    for(ushort y = 0; y < numCount; y++)
                    {
                        StartNumber startNum = new StartNumber();

                        // Status Code
                        startNum.Status = responseReader.ReadInt32();

                        // Paper Start Number
                        startNum.Number = responseReader.ReadInt32();

                        nums.StartNumbers.Add(startNum);
                    }

                    m_checkedNumbers.Add(nums);
                }
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
        /// Gets a list of validated start numbers received from the server.
        /// </summary>
        public IEnumerable<ProductStartNumbers> CheckedNumbers
        {
            get
            {
                return m_checkedNumbers;
            }
        }
        #endregion
    }
}