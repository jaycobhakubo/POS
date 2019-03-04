#region Copyright
// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2008 GameTech
// International, Inc.
#endregion

using System;
using System.IO;
using System.Text;
using System.Globalization;
using GTI.Modules.Shared;
using GTI.Modules.POS.Properties;

namespace GTI.Modules.POS.Data
{
    // FIX: DE1930
    /// <summary>
    /// The possible status return codes from the Cash Out Player server 
    /// message.
    /// </summary>
    internal enum CashOutPlayerReturnCode
    {
        IncorrectAuthPassword = -10,
        AuthPasswordHasExpired = -8,
        InactiveAuthStaff = -9,
        AuthStaffNotFound = -7,
        NotAuthorized = -21,
        NoCreditAccount = -73,
        InvalidAmount = 7,
        AuthStaffLocked = -106 // US1955
    }
    // EDN: DE1930

    /// <summary>
    /// Represents a Cash Out Player server message.
    /// </summary>
    internal class CashOutPlayerMessage : ServerMessage
    {
        #region Member Variables
        protected int m_bankId; // FIX: DE1930
        protected int m_playerId;
        protected int m_authStaffId;
        protected int m_authLoginNum;
        protected string m_authMagCardNum;
        protected byte[] m_authPassword;
        protected decimal m_cashOutAmount;
        protected decimal m_refundableCredit;
        protected decimal m_nonRefundableCredit;
        protected int m_transactionNumber;
        protected string m_currencyISO; // Rally TA7465
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the CashOutPlayerMessage class.
        /// </summary>
        public CashOutPlayerMessage()
        {
            m_id = 18068; // Cash Out Player
            m_strMessageName = "Cash Out Player";
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

            // Player Id
            requestWriter.Write(m_playerId);

            // Authorization Staff Id
            requestWriter.Write(m_authStaffId);

            // Authorization Login Number
            requestWriter.Write(m_authLoginNum);

            // Authorization Magnetic Card Number
            if(m_authMagCardNum != null)
            {
                requestWriter.Write((ushort)m_authMagCardNum.Length);
                requestWriter.Write(m_authMagCardNum.ToCharArray());
            }
            else
                requestWriter.Write((ushort)0);

            // Authorization Password Hash
            byte[] hashBuffer = new byte[DataSizes.PasswordHash];

            if(m_authPassword != null)
                Array.Copy(m_authPassword, hashBuffer, DataSizes.PasswordHash);

            requestWriter.Write(hashBuffer);

            // Bank Id
            requestWriter.Write(m_bankId); // FIX: DE1930

            // Cash Out Amount
            string tempDec = m_cashOutAmount.ToString("N", CultureInfo.InvariantCulture);
            requestWriter.Write((ushort)tempDec.Length);
            requestWriter.Write(tempDec.ToCharArray());

            // Currency ISO Code
            if(!string.IsNullOrEmpty(m_currencyISO))
            {
                requestWriter.Write((ushort)m_currencyISO.Length);
                requestWriter.Write(m_currencyISO.ToCharArray());
            }
            else
                requestWriter.Write((ushort)0);

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
            // FIX: DE1930
            m_refundableCredit = 0M;
            m_nonRefundableCredit = 0M;
            m_transactionNumber = 0;

            base.UnpackResponse();
            // END: DE1930

            // Create the streams we will be reading from.
            MemoryStream responseStream = new MemoryStream(m_responsePayload);
            BinaryReader responseReader = new BinaryReader(responseStream, Encoding.Unicode);

            // Try to unpack the data.
            try
            {
                // Seek past return code.
                responseReader.BaseStream.Seek(sizeof(int), SeekOrigin.Begin);

                // Refundable Credit
                ushort stringLen = responseReader.ReadUInt16();
                string tempDec = new string(responseReader.ReadChars(stringLen));

                if(!string.IsNullOrEmpty(tempDec))
                    m_refundableCredit = decimal.Parse(tempDec, CultureInfo.InvariantCulture);

                // Non-Refundable Credit
                stringLen = responseReader.ReadUInt16();
                tempDec = new string(responseReader.ReadChars(stringLen));

                if(!string.IsNullOrEmpty(tempDec))
                    m_nonRefundableCredit = decimal.Parse(tempDec, CultureInfo.InvariantCulture);

                // Transaction Number
                m_transactionNumber = responseReader.ReadInt32();
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
        // FIX: DE1930
        /// <summary>
        /// Gets or sets the id of the bank this credit cash out is made with.
        /// </summary>
        public int BankId
        {
            get
            {
                return m_bankId;
            }
            set
            {
                m_bankId = value;
            }
        }
        // END: DE1930

        /// <summary>
        /// Gets or sets the id of the player to cash out.
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
        /// Gets or sets the id for the staff who is authorizing this cash out 
        /// (if needed).  If 0 then AuthLoginNumber or AuthMagCardNumber will 
        /// be used.
        /// </summary>
        public int AuthStaffId
        {
            get
            {
                return m_authStaffId;
            }
            set
            {
                m_authStaffId = value;
            }
        }

        /// <summary>
        /// Gets or sets the login number for the staff who is authorizing this 
        /// cash out (if needed).  If 0 then AuthStaffId or AuthMagCardNumber 
        /// will be used.
        /// </summary>
        public int AuthLoginNum
        {
            get
            {
                return m_authLoginNum;
            }
            set
            {
                m_authLoginNum = value;
            }
        }

        /// <summary>
        /// Gets or sets the magnetic card number for the staff who is 
        /// authorizing this cash out (if needed).  If null, then AuthStaffId 
        /// or AuthLoginNumber will be used.
        /// </summary>
        public string AuthMagCardNum
        {
            get
            {
                return m_authMagCardNum;
            }
            set
            {
                m_authMagCardNum = value;
            }
        }

        /// <summary>
        /// Gets or sets the password hash for the staff who is authorizing this 
        /// cash out (if needed).
        /// </summary>
        public byte[] AuthPassword
        {
            get
            {
                return m_authPassword;
            }
            set
            {
                if(value != null && value.Length != DataSizes.PasswordHash)
                    throw new ArgumentException("AuthLoginPassword" + Resources.WrongSize);

                m_authPassword = value;
            }
        }

        /// <summary>
        /// Gets or sets the amount that is being cashed out.  This value is
        /// always in the system default currency.
        /// </summary>
        public decimal CashOutAmount
        {
            get
            {
                return m_cashOutAmount;
            }
            set
            {
                // Check to make sure it's not too big to fit in a string.
                string tempDec = value.ToString("N", CultureInfo.InvariantCulture);

                if(tempDec.Length <= StringSizes.MaxDecimalLength)
                    m_cashOutAmount = value;
                else
                    throw new ArgumentException("CashOutAmount" + Resources.TooBig);
            }
        }

        // Rally TA7465
        /// <summary>
        /// Gets or sets the currency ISO code used to cash out.
        /// </summary>
        public string CurrencyISO
        {
            get
            {
                return m_currencyISO;
            }
            set
            {
                m_currencyISO = value;
            }
        }

        /// <summary>
        /// Gets the new refundable credit balance for the player.
        /// </summary>
        public decimal RefundableCredit
        {
            get
            {
                return m_refundableCredit;
            }
        }

        /// <summary>
        /// Gets the new non-refundable credit balance for the player.
        /// </summary>
        public decimal NonRefundableCredit
        {
            get
            {
                return m_nonRefundableCredit;
            }
        }

        /// <summary>
        /// Gets the number of the transaction that was created on the server.
        /// </summary>
        public int TransactionNumber
        {
            get
            {
                return m_transactionNumber;
            }
        }
        #endregion
    }
}