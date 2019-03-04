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
using GTI.Modules.POS.Properties;

// Rally DE139 & DE2416

namespace GTI.Modules.POS.Data
{
    /// <summary>
    /// The possible status return codes from the Check Max Cards server message.
    /// </summary>
    internal enum CheckMaxCardsReturnCode
    {
        MaxCardsExceeded = -63
    }

    /// <summary>
    /// Represents a Check Max Cards server message.
    /// </summary>
    internal class CheckMaxCardsMessage : ServerMessage
    {
        #region Member Variables
        protected int m_playerId;
        protected Dictionary<int, Dictionary<int, int>> m_packageCounts = new Dictionary<int, Dictionary<int, int>>();
        protected int m_numCardOverLimit;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the CheckMaxCardsMessage class.
        /// </summary>
        /// <param name="playerId">The id of the player who is buying these 
        /// packages or 0 for no player.</param>
        public CheckMaxCardsMessage(int playerId)
        {
            m_id = 18136; // Check Max Cards
            m_strMessageName = "Check Max Cards";
            m_playerId = playerId;
        }
        #endregion

        #region Member Methods
        // Rally DE1908
        /// <summary>
        /// Adds a package quantity to the message.
        /// </summary>
        /// <param name="sessionPlayedId">The session played id to which the
        /// package belongs.</param>
        /// <param name="packageId">The id of the package to add.</param>
        /// <param name="quantity">The quantity of the package to add.</param>
        public void AddPackageQuantity(int sessionPlayedId, int packageId, int quantity)
        {
            // Check to see if this session is already in the dictionary.
            if(!m_packageCounts.ContainsKey(sessionPlayedId))
                m_packageCounts.Add(sessionPlayedId, new Dictionary<int, int>());

            // Check to see if this package is already in the dictionary.
            if(!m_packageCounts[sessionPlayedId].ContainsKey(packageId))
                m_packageCounts[sessionPlayedId].Add(packageId, 0);

            m_packageCounts[sessionPlayedId][packageId] += quantity;
        }

        /// <summary>
        /// Prepares the request to be sent to the server.
        /// </summary>
        protected override void PackRequest()
        {
            // Create the streams we will be writing to.
            MemoryStream requestStream = new MemoryStream();
            BinaryWriter requestWriter = new BinaryWriter(requestStream, Encoding.Unicode);

            // Player Id
            requestWriter.Write(m_playerId);

            // Count of sessions.
            requestWriter.Write((ushort)m_packageCounts.Keys.Count);

            foreach(KeyValuePair<int, Dictionary<int, int>> session in m_packageCounts)
            {
                // Rally DE2067 - SQL error when selling more than 5 sessions.
                // Session Played Id
                requestWriter.Write(session.Key);
                
                // Count of packages.
                requestWriter.Write((ushort)session.Value.Keys.Count);

                // Add all the packages.
                foreach(KeyValuePair<int, int> package in session.Value)
                {
                    // Package Id.
                    requestWriter.Write(package.Key);

                    // Quantity
                    requestWriter.Write(package.Value);
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
            m_numCardOverLimit = 0;

            base.UnpackResponse();

            // Create the streams we will be reading from.
            MemoryStream responseStream = new MemoryStream(m_responsePayload);
            BinaryReader responseReader = new BinaryReader(responseStream, Encoding.Unicode);

            // Try to unpack the data.
            try
            {
                // Seek past return code.
                responseReader.BaseStream.Seek(sizeof(int), SeekOrigin.Begin);

                // Number of card over limit.
                m_numCardOverLimit = responseReader.ReadInt32();
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
        /// Gets or sets the id of the player who is buying these packages or 0
        /// for no player.
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
        /// Gets the number of cards that would be over the limit or 0 if the 
        /// max cards was not exceeded.
        /// </summary>
        public int NumCardOverLimit
        {
            get
            {
                return m_numCardOverLimit;
            }           
        }
        #endregion
    }
}
