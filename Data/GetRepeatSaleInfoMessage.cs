#region Copyright
// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2016 GameTech
// International, Inc.
#endregion

using System.Collections.Generic;
using System.IO;
using System.Text;
using GTI.Modules.Shared;
using System;

namespace GTI.Modules.POS.Data
{
    public class RepeatSaleInfo
    {
        public int session;
        public int packageID;
        public int discountID;
        public string name;
        public int quantity;
    }

    internal class GetRepeatSaleInfoMessage : ServerMessage
    {
        #region Member Variables
        protected int m_transactionNumber = 0;
        protected int m_playerID = 0;
        #endregion

        #region Constructors

        /// <summary>
        /// Constructor for Repeat Sale Info message.
        /// </summary>
        public GetRepeatSaleInfoMessage()
        {
            m_id = 18234; // Get Repeat Sale Info Message
            m_strMessageName = "Get Repeat Sale Info";
            SaleInfo = new List<RepeatSaleInfo>();
            PlayerID = 0;
            TransactionNumber = 0;
        }
        
        /// <summary>
        /// Constructor for Repeat Sale Info message.
        /// </summary>
        public GetRepeatSaleInfoMessage(int transactionNumber, int playerID = 0)
        {
            m_id = 18234; // Get Repeat Sale Info Message
            m_strMessageName = "Get Repeat Sale Info";
            SaleInfo = new List<RepeatSaleInfo>();
            PlayerID = playerID;
            TransactionNumber = transactionNumber;
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

            requestWriter.Write(PlayerID);
            requestWriter.Write(TransactionNumber);

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
            //// Reset the values.
            base.UnpackResponse();

            // Create the streams we will be reading from.
            MemoryStream responseStream = new MemoryStream(m_responsePayload);
            BinaryReader responseReader = new BinaryReader(responseStream, Encoding.Unicode);

            // Try to unpack the data.
            try
            {
                // Seek past return code.
                responseReader.BaseStream.Seek(sizeof(int), SeekOrigin.Begin);

                DeviceID = responseReader.ReadUInt16();

                var count = responseReader.ReadUInt16();

                for (int i = 0; i < count; i++)
                {
                    RepeatSaleInfo info = new RepeatSaleInfo();

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
                    SaleInfo.Add(info);
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

            //Close the streams.
            responseReader.Close();
        }

        #endregion

        #region Member Properties

        public int PlayerID
        {
            get
            {
                return m_playerID;
            }

            set
            {
                m_playerID = value;
            }
        }

        public int TransactionNumber
        {
            get
            {
                return m_transactionNumber;
            }

            set
            {
                m_transactionNumber = value;
            }
        }

        public List<RepeatSaleInfo> SaleInfo
        {
            get;
            private set;
        }

        public int DeviceID
        {
            get;
            private set;
        }
        #endregion
    }
}