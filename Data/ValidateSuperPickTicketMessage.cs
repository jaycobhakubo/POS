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
    // TODO Revisit ValidateSuperPickTicketMessage.
    /*
    /// <summary>
    /// The possible status return codes from the Validate Super Ticket Message
    /// server message.
    /// </summary>
    public enum ValidateSuperTicketReturnCodes
    {
        WrongGamingDate         = 1,
        ReceiptVoided           = 4,
        TransactionNotFound     = 6
    }

    /// <summary>
    /// A helper class that represents super pick items in a list.
    /// </summary>
    internal class SuperPickListItem
    {
        #region Member Variables
        private short m_receiptLineNumber = 0;
        private int m_hotBallSaleId = 0;
        private SuperPickType m_type = SuperPickType.Single;
        private string m_numbersPicked = string.Empty;
        private string m_itemPrice = string.Empty;
        #endregion

        #region Member Methods
        /// <summary>
        /// Returns a string that represents the current SuperPickListItem.
        /// </summary>
        /// <returns>A string that represents the current 
        /// SuperPickListItem.</returns>
        public override string ToString()
        {
            string returnVal = string.Empty;

            returnVal = " " + m_receiptLineNumber.ToString().PadRight(4, ' ');

            if(m_type == SuperPickType.Single || m_type == SuperPickType.Double)
            {
                if(m_type == SuperPickType.Single)
                    returnVal += Resources.SuperPickSingleText.PadRight(10, ' ');
                else if(m_type == SuperPickType.Double)
                    returnVal += Resources.SuperPickDoubleText.PadRight(10, ' ');
            }
            else
            {
                returnVal += Resources.SuperPickInvalidText.PadRight(10, ' ');
            }

            if(m_numbersPicked != string.Empty)
            {
                returnVal += m_numbersPicked.PadRight(7, ' ');
            }

            if(m_itemPrice != string.Empty)
            {
                returnVal += string.Format("{0:C}", Convert.ToDecimal(m_itemPrice)).PadRight(16, ' ');
            }

            if(returnVal.Trim() == string.Empty)
                returnVal = Resources.NoSuperPickText;

            return returnVal;
        }

        /// <summary>
        /// Determines whether two SuperPickListItem instances are equal.
        /// </summary>
        /// <param name="obj">The SuperPickListItem to compare with the 
        /// current SuperPickListItem.</param>
        /// <returns>true if the specified SuperPickListItem is equal 
        /// to the current SuperPickListItem; otherwise, false. </returns>
        public override bool Equals(object obj)
        {
            if (!(obj is SuperPickListItem)) return false;

            SuperPickListItem item = (SuperPickListItem)obj;

            return (item.m_hotBallSaleId == this.m_hotBallSaleId);
        }

        /// <summary>
        /// Serves as a hash function for a SuperPickListItem. 
        /// GetHashCode is suitable for use in hashing algorithms and data
        /// structures like a hash table. 
        /// </summary>
        /// <returns>A hash code for the current SuperPickListItem.</returns>
        public override int GetHashCode()
        {
            return m_hotBallSaleId.GetHashCode();
        }
        #endregion

        #region Member Properties
        /// <summary>
        /// Gets or sets the receipts line's number
        /// </summary>
        public short ReceiptLineNumber
        {
            get
            {
                return m_receiptLineNumber;
            }
            set
            {
                m_receiptLineNumber = value;
            }
        }

        /// <summary>
        /// Gets or sets the HotBallSaleId
        /// </summary>
        public int HotBallSaleId
        {
            get
            {
                return m_hotBallSaleId;
            }
            set
            {
                m_hotBallSaleId = value;
            }
        }

        /// <summary>
        /// Gets or sets the Super Pick type.
        /// </summary>
        public SuperPickType Type
        {
            get
            {
                return m_type;
            }
            set
            {
                m_type = value;
            }
        }

        /// <summary>
        /// Gets or sets the numbers picked.
        /// </summary>
        public string NumbersPicked
        {
            get
            {
                return m_numbersPicked;
            }
            set
            {
                m_numbersPicked = value;
            }
        }

        /// <summary>
        /// Gets or sets the item price.
        /// </summary>
        public string ItemPrice
        {
            get
            {
                return m_itemPrice;
            }
            set
            {
                m_itemPrice = value;
            }
        }
        #endregion
    }

    /// <summary>
    /// Represents the Validate Super Pick Ticket server message.
    /// </summary>
    internal class ValidateSuperPickTicketMessage : ServerMessage
    {
        #region Member Variables
        protected uint m_authenticationCode = 0;
        protected int m_transactionNumber = 0;
        protected int m_playerId = 0;
        protected string m_playerFName = string.Empty;
        protected string m_playerMInitial = string.Empty;
        protected string m_playerLName = string.Empty;
        protected ArrayList m_superPicks = null;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the ValidateSuperPickTicketMessage 
        /// class.
        /// </summary>
        public ValidateSuperPickTicketMessage()
            : this(0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ValidateSuperPickTicketMessage 
        /// class with the specified authentication code.
        /// </summary>
        public ValidateSuperPickTicketMessage(uint authenticationCode)
        {
            if(authenticationCode <= 0)
                throw new ArgumentException("authenticationCode");
            
            m_id = 18031; // Validate Super Pick Ticket
            m_authenticationCode = authenticationCode;
            m_superPicks = new ArrayList();
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

            // Authentication Code
            requestWriter.Write(m_authenticationCode);

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

                // Transaction #
                m_transactionNumber = responseReader.ReadInt32();

                // Player Id
                m_playerId = responseReader.ReadInt32();

                // Player First Name
                short stringLen = responseReader.ReadInt16();
                m_playerFName = new string(responseReader.ReadChars(stringLen));

                // Player Middle Initial
                stringLen = responseReader.ReadInt16();
                m_playerMInitial = new string(responseReader.ReadChars(stringLen));

                // Player Last Name
                stringLen = responseReader.ReadInt16();
                m_playerLName = new string(responseReader.ReadChars(stringLen));

                // Get the count of Super Picks.
                short pickCount = responseReader.ReadInt16();

                // Clear the Super Pick array.
                m_superPicks.Clear();

                // Get all the Super Picks.
                for(int x = 0; x < pickCount; x++)
                {
                    SuperPickListItem item = new SuperPickListItem();

                    // Receipt Line #
                    item.ReceiptLineNumber = responseReader.ReadInt16();

                    // Super Pick Sale Id
                    item.HotBallSaleId = responseReader.ReadInt32();

                    // Super Pick Type
                    item.Type = (SuperPickType)responseReader.ReadInt16();

                    // Numbers Picked
                    stringLen = responseReader.ReadInt16();
                    item.NumbersPicked = new string(responseReader.ReadChars(stringLen));

                    // Price
                    stringLen = responseReader.ReadInt16();
                    item.ItemPrice = new string(responseReader.ReadChars(stringLen));

                    m_superPicks.Add(item);
                }
            }
            catch(EndOfStreamException e)
            {
                throw new MessageWrongSizeException("Validate Super Pick Ticket", e);
            }
            catch(Exception e)
            {
                throw new ServerException("Validate Super Pick Ticket", e);
            }

            // Close the streams.
            responseReader.Close();
        }
        #endregion

        #region Member Properties
        /// <summary>
        /// Gets or sets the authentication code to search for.
        /// </summary>
        public uint AuthenticationCode
        {
            get
            {
                return m_authenticationCode;
            }
            set
            {
                m_authenticationCode = value;
            }
        }

        /// <summary>
        /// Gets the transaction number received from the server.
        /// </summary>
        public int TransactionNumber
        {
            get
            {
                return m_transactionNumber;
            }
        }

        /// <summary>
        /// Gets the player's id received from the server.
        /// </summary>
        public int PlayerId
        {
            get
            {
                return m_playerId;
            }
        }

        /// <summary>
        /// Gets the player's first name received from the server.
        /// </summary>
        public string PlayerFName
        {
            get
            {
                return m_playerFName;
            }
        }

        /// <summary>
        /// Gets the player's middle initial received from the server.
        /// </summary>
        public string PlayerMInitial
        {
            get
            {
                return m_playerMInitial;
            }
        }

        /// <summary>
        /// Gets the player's last name received from the server.
        /// </summary>
        public string PlayerLName
        {
            get
            {
                return m_playerLName;
            }
        }

        /// <summary>
        /// Gets a list of all super picks received from the server.
        /// </summary>
        public SuperPickListItem[] SuperPicks
        {
            get
            {
                return (SuperPickListItem[])m_superPicks.ToArray(typeof(SuperPickListItem));
            }
        }
        #endregion
    }
    */
}
