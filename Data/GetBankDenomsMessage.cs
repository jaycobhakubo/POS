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
using GTI.Modules.POS.Business;
using GTI.Modules.Shared;

namespace GTI.Modules.POS.Data
{
    /// Represents a Get Paper Start Numbers message.
    /// </summary>
    internal class GetBankDenomsMessage : ServerMessage
    {
        #region Member Variables

        private readonly int m_bankId;
        private readonly Dictionary<int, int> m_denomIdToCountDictionary;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the GetBankDenomsMessage 
        /// class.
        /// </summary>
        public GetBankDenomsMessage(int bankId)
        {
            m_id = 37040; // Get Paper Start Numbers
            m_strMessageName = "Get POS Bank deniminations count";
            m_bankId = bankId;
            m_denomIdToCountDictionary = new Dictionary<int, int>();
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

            // Product Count
            requestWriter.Write(m_bankId);

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
                ushort count = responseReader.ReadUInt16();
                for (ushort x = 0; x < count; x++)
                {
                    //denom Id
                    var denomId = responseReader.ReadInt32();

                    //denom count
                    var denomCount = responseReader.ReadInt32();

                    //add to list
                    m_denomIdToCountDictionary.Add(denomId, denomCount);

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
        /// Gets a list of start numbers received from the server.
        /// </summary>
        public Dictionary<int, int> DenomIdToCountDictionary
        {
            get
            {
                return m_denomIdToCountDictionary;
            }
        }
        #endregion
    }
}