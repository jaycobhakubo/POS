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
    /// <summary>
    /// Represents a Get Auto Discount Usage
    /// </summary>
    internal class GetDiscountUsageBySessionMessage : ServerMessage
    {
        #region Member Variables
        private readonly int m_sessionPlayedId;
        private readonly int m_playerId;
        private readonly Dictionary<int, int> m_discountUsageList;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the GetPaperStartNumbersMessage 
        /// class.
        /// </summary>
        private GetDiscountUsageBySessionMessage(int playerId, int sessionPlayedId)
        {
            m_playerId = playerId;
            m_sessionPlayedId = sessionPlayedId;
            m_id = 18232; 
            m_strMessageName = "Get Discount Usage By Session Message";
            m_discountUsageList = new Dictionary<int, int>();
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
            requestWriter.Write(m_playerId);

            requestWriter.Write(m_sessionPlayedId);

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
            //clear list
            m_discountUsageList.Clear();

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
                ushort discounts = responseReader.ReadUInt16();

                for (ushort x = 0; x < discounts; x++)
                {
                    // Discount Id
                    var discountId = responseReader.ReadInt32();

                    //Usage Count
                    var usageCount = responseReader.ReadInt32();

                    //if key already exists, then we need to add sum
                    if (m_discountUsageList.ContainsKey(discountId))
                    {
                        m_discountUsageList[discountId] += usageCount;
                        continue;
                    }

                    //add new list
                    m_discountUsageList.Add(discountId, usageCount);
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

        public static Dictionary<int, int> GetDiscountUsageBySession(int playerId, int sessionPlayedId)
        {
            var message = new GetDiscountUsageBySessionMessage(playerId, sessionPlayedId);

            message.Send();
            
            return message.m_discountUsageList;
        }

        #endregion

        #region Member Properties
        #endregion
    }
}