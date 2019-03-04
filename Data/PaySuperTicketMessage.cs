// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2007 GameTech
// International, Inc.

using System;
using System.IO;
using System.Text;
using System.Collections;
using GTI.Modules.Shared;
using GTI.Modules.POS.Business;
using GTI.Modules.POS.Properties;

namespace GTI.Modules.POS.Data
{
    // TODO Revisit PaySuperTicketMessage.
    /*
    /// <summary>
    /// A helper class that represents super pick item to be paid in a list.
    /// </summary>
    internal class PaySuperPickListItem
    {
        #region Member variables
        private int m_playerId = 0;
        private int m_hotballSaleId = 0;
        private string m_amountWon = string.Empty;
        private int m_payoutReceiptNumber = 0;
        private string m_magCard = string.Empty;
        private string m_playerFirstName = string.Empty;
        private string m_playerLastName = string.Empty;
        #endregion

        #region Member Methods
        ///<sumary>
        /// Returns a string that represents the current PaySuperPickListItem.
        ///</sumary>
        ///<returns>
        /// Returns a string that represents the current PaySuperPickListItem.
        ///</returns>
        ///
        public override string ToString()
        {
            return "";
        }
        #endregion

        #region Member Properties

        /// <summary>
        /// Gets or sets the PlayerId
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
        /// Gets or sets the HotBallSaleId
        /// </summary>
        public int HotBallSaleId
        {
            get
            {
                return m_hotballSaleId;
            }
            set
            {
                m_hotballSaleId = value;
            }
        }

        /// <summary>
        /// Gets or sets the AmountWon
        /// </summary>
        public string AmountWon
        {
            get
            {
                return m_amountWon;
            }
            set
            {
                m_amountWon = value;
            }
        }

        /// <summary>
        /// Gets or sets the PayoutReceiptNumber
        /// </summary>
        public int PayoutReceiptNumber
        {
            get
            {
                return m_payoutReceiptNumber;
            }
            set
            {
                m_payoutReceiptNumber = value;
            }
        }

        /// <summary>
        /// Gets or sets the Player's MagCard
        /// </summary>
        public string MagCard
        {
            get
            {
                return m_magCard;
            }
            set
            {
                m_magCard = value;
            }
        }

        /// <summary>
        /// Gets or sets the Player's First Name
        /// </summary>
        public string PlayerFirstName
        {
            get
            {
                return m_playerFirstName;
            }
            set
            {
                m_playerFirstName = value;
            }
        }

        /// <summary>
        /// Gets or sets the Player's Last Name
        /// </summary>
        public string PlayerLastName
        {
            get
            {
                return m_playerLastName;
            }
            set
            {
                m_playerLastName = value;
            }
        }

        #endregion
    }

    /// <summary>
    /// Represents the Pay Super Ticket Server Message.
    /// </summary>
    internal class PaySuperTicketMessage : ServerMessage 
    {
        #region Constants And Data Types

        /*
        /// <summary>
        /// Represents winners sent to the server.
        /// </summary>
        protected struct AddSuperPickWinnerList
        {
            public int PlayerId;
            public short ProductTypeId;
            public int HotballSaleId;
        }
        
        #endregion

        #region Member variables

        protected int m_operatorId = 0;
        protected int m_machineId = 0;
        protected int m_staffId = 0;
        protected ArrayList m_winnerList = null;
        protected ArrayList m_payList = null;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the PaySuperTicketMessage class.
        /// </summary>
        //public PaySuperTicketMessage()
        //    : this(0)
        //{
        //}

        /// <summary>
        /// Initializes a new instance of the PaySuperTicketMessage class with parameters.
        /// </summary>
        /// <param name="operatorId"></param>
        /// <param name="machineId"></param>
        /// <param name="staffId"></param>
        /// <param name="winnerList"></param>
        public PaySuperTicketMessage(int operatorId, int machineId, int staffId)
        {
            m_id = 21004; // Pay Super Pick Winners
            m_operatorId = operatorId;
            m_machineId = machineId;
            m_staffId = staffId;
            m_winnerList = new ArrayList();
            m_payList = new ArrayList();
        }

        #endregion

        #region Member Methods

        public void AddWinner(ArrayList winnerList)
        {

            m_winnerList.AddRange(winnerList);

        }

        /// <summary>
        /// Prepares the request to be sent to the server.
        /// </summary>
        protected override void PackRequest()
        {
            // Create the streams we will be writing to.
            MemoryStream requestStream = new MemoryStream();
            BinaryWriter requestWriter = new BinaryWriter(requestStream, Encoding.Unicode);

            // Operator Id
            requestWriter.Write(m_operatorId);

            // Machine Id
            requestWriter.Write(m_machineId);

            // Staff Id
            requestWriter.Write(m_staffId);

            // Count of winners
            requestWriter.Write((short)m_winnerList.Count);

            // Add all the winners
            foreach (string value in m_winnerList)
            {
                string[] values = new string[3];
                values = value.Split(',');

                // Player Id
                requestWriter.Write(Convert.ToInt32(values[0]));

                // Product Type Id
                requestWriter.Write(Convert.ToInt16(values[1]));

                // Hotball Sale Id
                requestWriter.Write(Convert.ToInt32(values[2]));
            }

            // Set the bytes to be sent.
            m_requestPayload = requestStream.ToArray();

            // Close the streams
            requestWriter.Close();
        }

        protected override void UnpackResponse()
        {
            base.UnpackResponse();

            short stringLen = 0;

            // Create the streams we will be reading from.
            MemoryStream responseStream = new MemoryStream(m_responsePayload);
            BinaryReader responseReader = new BinaryReader(responseStream, Encoding.Unicode);

            // Try to unpack the data.
            try
            {
                // Seek past return code.
                responseReader.BaseStream.Seek(sizeof(int), SeekOrigin.Begin);

                // Winner Count
                short winnerCount = responseReader.ReadInt16();

                // Clear the winner list array
                //m_winnerList.Clear();
                m_payList.Clear();


                // Get all the winners
                for (int x = 0; x < winnerCount; x++)
                {
                    PaySuperPickListItem item = new PaySuperPickListItem();

                    // Player Id
                    item.PlayerId = responseReader.ReadInt32();

                    // Hotball Sale Id
                    item.HotBallSaleId = responseReader.ReadInt32();

                    // Amount Won
                    stringLen = responseReader.ReadInt16();
                    item.AmountWon = new string(responseReader.ReadChars(stringLen));

                    // Payout receipt Number
                    item.PayoutReceiptNumber = responseReader.ReadInt32();

                    // Player's Mag Card
                    stringLen = responseReader.ReadInt16();
                    item.MagCard = new string(responseReader.ReadChars(stringLen));

                    // Player's First Name
                    stringLen = responseReader.ReadInt16();
                    item.PlayerFirstName = new string(responseReader.ReadChars(stringLen));

                    // Player's Last Name
                    stringLen = responseReader.ReadInt16();
                    item.PlayerLastName = new string(responseReader.ReadChars(stringLen));

                    //m_winnerList.Add(item);
                    m_payList.Add(item);
                }
            }
            catch (EndOfStreamException e)
            {
                throw new MessageWrongSizeException("Pay Super Pick Ticket", e);
            }
            catch (Exception e)
            {
                throw new ServerException ("Pay Super Pick Ticket", e);
            }
        }

        #endregion

        #region Member Properties

        public PaySuperPickListItem[] PaySuperPicks
        {
            get
            {
                return (PaySuperPickListItem[])m_payList.ToArray(typeof(PaySuperPickListItem));
            }
        }

        #endregion
    }
    */
}
