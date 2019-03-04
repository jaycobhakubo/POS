// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2007 GameTech
// International, Inc.

using System;
using System.IO;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace GTI.Modules.Shared
{
    public class ScheduledSaleInfo
    {
        public int session;
        public int packageID;
        public int discountID;
        public string name;
        public int quantity;
    }

    /// <summary>
    /// Represents the Find Player by Player Card server message.
    /// </summary>
    public class GetScheduledSalesMessage : ServerMessage
    {
        #region Member Variables
        protected int m_playerId = 0;
        protected string m_accountNumber = string.Empty;
        protected string m_magCard = string.Empty;
        protected string m_scanCode = string.Empty;
        protected int m_sessionPlayedID = -1;
        protected List<ScheduledSaleInfo> m_saleList = new List<ScheduledSaleInfo>();
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the GetScheduledSalesMessage class.
        /// </summary>
        public GetScheduledSalesMessage()
        {
            m_id = 18251; 
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

            //Player ID
            requestWriter.Write(m_playerId);

            //Account #
            requestWriter.Write((ushort)m_accountNumber.Length);
            requestWriter.Write(m_accountNumber.ToCharArray());
            
            // Mag. Card #
            requestWriter.Write((ushort)m_magCard.Length);
            requestWriter.Write(m_magCard.ToCharArray());

            // Scan code
            requestWriter.Write((ushort)m_scanCode.Length);
            requestWriter.Write(m_scanCode.ToCharArray());

            //Session Played ID
            requestWriter.Write(m_sessionPlayedID);

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

                var count = responseReader.ReadUInt16();

                for (int i = 0; i < count; i++)
                {
                    ScheduledSaleInfo info = new ScheduledSaleInfo();

                    //session
                    info.session = responseReader.ReadInt32();

                    //packageID
                    info.packageID = responseReader.ReadInt32();

                    //discountID
                    info.discountID = responseReader.ReadInt32();

                    //name
                    info.name = ReadString(responseReader);

                    //quantity
                    info.quantity = responseReader.ReadInt32();

                    //add to list
                    m_saleList.Add(info);
                }
            }
            catch(EndOfStreamException e)
            {
                throw new MessageWrongSizeException("Get Scheduled Sales", e);
            }
            catch(Exception e)
            {
                throw new ServerException("Get Scheduled Sales", e);
            }           

            // Close the streams.
            responseReader.Close();
        }
        #endregion

        #region Member Properties
        /// <summary>
        /// Gets or sets the magnetic card number to search with.
        /// </summary>
        public string MagCardNumber
        {
            get
            {
                return m_magCard;
            }

            set
            {
                if(value.Length <= StringSizes.MaxMagneticCardLength)
                    m_magCard = value;
                else
                    throw new ArgumentException("MagCardNumber is too big.");
            }
        }

        /// <summary>
        /// Gets or sets the scheduled sale scan code to search with.
        /// </summary>
        public string Scancode
        {
            get
            {
                return m_scanCode;
            }

            set
            {
                if (value.Length <= StringSizes.MaxMagneticCardLength)
                {
                    if (Regex.IsMatch(value.ToUpper(), @"^F[0-9]*SCH") || Regex.IsMatch(value.ToUpper(), @"^F[0-9]*PDS"))
                        m_scanCode = value.ToUpper();
                    else
                        throw new ArgumentException("Not a valid Scancode - Bad format.");
                }
                else
                {
                    throw new ArgumentException("Scancode is too big.");
                }
            }
        }
        
        /// <summary>
        /// Gets or sets the player account number to search with.
        /// </summary>
        public string AccountNumber
        {
            get
            {
                return m_accountNumber;
            }

            set
            {
                if (value.Length <= StringSizes.MaxPlayerIdentLength)
                    m_accountNumber = value;
                else
                    throw new ArgumentException("AccountNumber is too big.");
            }
        }

        /// <summary>
        /// Gets or sets the player's id for searching.
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
        /// Gets or sets the session played ID.  If not using a scan code for the lookup,
        /// the player will be found using the other fields.  If the session played ID is
        /// -1 no session purchases are checked.  If the session played ID is not -1, if 
        /// the player made a purchase in that session (any if 0), no scheduled sale items
        /// will be returned.
        /// </summary>
        public int SessionPlayedID
        {
            get
            {
                return m_sessionPlayedID;
            }

            set
            {
                m_sessionPlayedID = value;
            }
        }

        /// <summary>
        /// Gets or sets the returned list of items to purchase.
        /// </summary>
        public List<ScheduledSaleInfo> SaleList
        {
            get
            {
                return m_saleList;
            }

            set
            {
                m_saleList = value;
            }
        }

        #endregion
    }
}
