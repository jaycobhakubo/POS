#region Copyright
// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2008 GameTech
// International, Inc.
#endregion

// Rally TA7465
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using GTI.Modules.POS.Business;
using GTI.Modules.Shared;

namespace GTI.Modules.POS.Data
{
    // FIX: DE1930
    /// <summary>
    /// The possible status return codes from the Bank Issue server message.
    /// </summary>
    internal enum BankIssueReturnCode
    {
        MissingCashMethod = -42
    }

    /// <summary>
    /// Represents a Bank Issue message.
    /// </summary>
    internal class BankIssueMessage : ServerMessage
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the BankIssueMessage class.
        /// </summary>
        /// <param name="sourceBankId">The id of the bank that is the source of
        /// the issue.</param>
        /// <param name="destinationBankId">The id of the bank that is the
        /// destination of the issue (or 0 if it is a new bank).</param>
        /// <param name="destinationStaffId">The id of the staff that owns the
        /// destination bank.</param>
        /// <param name="bankName">The name of the bank (if it is a master
        /// bank).</param>
        /// <param name="bankType">The type of the destination bank.</param>
        /// <param name="session">The session that the bank is for (or 0 if not
        /// for a session).</param>
        public BankIssueMessage(int sourceBankId, int destinationBankId, int destinationStaffId, string bankName, BankType bankType, short session)
        {
            m_id = 37005; // Bank Issue
            m_strMessageName = "Bank Issue";

            SourceBankId = sourceBankId;
            DestinationBankId = destinationBankId;
            DestinationStaffId = destinationStaffId;
            BankName = bankName;
            BankType = bankType;
            Session = session;

            Currencies = new List<BankCurrency>();
        }
        #endregion

        #region Member Methods 
        /// <summary>
        /// Adds a currency to the list to be issued.
        /// </summary>
        /// <param name="currency">The currency to issue.</param>
        /// <remarks>The Total of the BankCurrency should be 0 if the
        /// denominations in the currency have counts.</remarks>
        public void AddCurrency(BankCurrency currency)
        {
            // Does this currency already exist?
            if(!Currencies.Exists((parameter) => currency.ISOCode == parameter.ISOCode))
                Currencies.Add(currency);
        }

        /// <summary>
        /// Removes all the currencies to be sent to the server.
        /// </summary>
        public void ClearCurrencies()
        {
            Currencies.Clear();
        }

        /// <summary>
        /// Prepares the request to be sent to the server.
        /// </summary>
        protected override void PackRequest()
        {
            // Create the streams we will be writing to.
            MemoryStream requestStream = new MemoryStream();
            BinaryWriter requestWriter = new BinaryWriter(requestStream, Encoding.Unicode);

            // Source Bank Id
            requestWriter.Write(SourceBankId);

            // Destination Bank Id
            requestWriter.Write(DestinationBankId);

            // Destination Staff Id
            requestWriter.Write(DestinationStaffId);

            // Bank Name
            if(!string.IsNullOrEmpty(BankName))
            {
                requestWriter.Write((ushort)BankName.Length);
                requestWriter.Write(BankName.ToCharArray());
            }
            else
                requestWriter.Write((ushort)0);

            // Bank Type
            requestWriter.Write((byte)BankType);

            // Session Number
            requestWriter.Write(Session);

            // Currency Count
            requestWriter.Write((ushort)Currencies.Count);

            // Currency List
            foreach(BankCurrency currency in Currencies)
            {
                // Currency ISO Code
                requestWriter.Write((ushort)currency.ISOCode.Length);
                requestWriter.Write(currency.ISOCode.ToCharArray());

                // The POS only supports cash tender types.
                // Tender Type Count
                requestWriter.Write((ushort)1);

                // Tender Type Id
                requestWriter.Write((int)TenderType.Cash);

                // Bank Total
                string tempDec = currency.Total.ToString("N", CultureInfo.InvariantCulture);
                requestWriter.Write((ushort)tempDec.Length);
                requestWriter.Write(tempDec.ToCharArray());

                // The POS only supports totals.
                // Denom Count
                requestWriter.Write((ushort)0);
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
            CashTransactionId = 0;
            TransactionDate = DateTime.MinValue;

            base.UnpackResponse();

            // Create the streams we will be reading from.
            MemoryStream responseStream = new MemoryStream(m_responsePayload);
            BinaryReader responseReader = new BinaryReader(responseStream, Encoding.Unicode);

            // Try to unpack the data.
            try
            {
                // Seek past return code.
                responseReader.BaseStream.Seek(sizeof(int), SeekOrigin.Begin);

                // Bank Id
                DestinationBankId = responseReader.ReadInt32();

                // Cash Transaction Id
                CashTransactionId = responseReader.ReadInt32();

                // Transaction Date
                ushort stringLen = responseReader.ReadUInt16();
                string tempDate = new string(responseReader.ReadChars(stringLen));

                if(!string.IsNullOrEmpty(tempDate))
                    TransactionDate = DateTime.Parse(tempDate, CultureInfo.InvariantCulture);
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
        /// Gets or sets the id of the bank that is the source of the issue.
        /// </summary>
        public int SourceBankId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the id of the bank that is the destination of the
        /// issue (or 0 if it is a new bank).
        /// </summary>
        /// <remarks>This value will be populated with the newly created bank
        /// id if 0 is specified before Send was called.</remarks>
        public int DestinationBankId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the id of the staff that owns the destination bank.
        /// </summary>
        public int DestinationStaffId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the bank (if it is a master bank).
        /// </summary>
        public string BankName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the type of the destination bank.
        /// </summary>
        public BankType BankType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the session that the bank is for (or 0 if not for a
        /// session).
        /// </summary>
        public short Session
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the currencies to issue.
        /// </summary>
        private List<BankCurrency> Currencies
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the id of the transaction for the issue received from the
        /// server.
        /// </summary>
        public int CashTransactionId
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the date/time stamp of the transaction received from the
        /// server.
        /// </summary>
        public DateTime TransactionDate
        {
            get;
            private set;
        }
        #endregion
    }
    // END: DE1930
    // END: TA7465
}
