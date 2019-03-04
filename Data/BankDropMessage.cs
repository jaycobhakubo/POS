#region Copyright
// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2008-2009 GameTech
// International, Inc.
#endregion

// FIX: DE1930
// Rally TA7465
//US4436: Close a bank from the POS

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using GTI.Modules.POS.Business;
using GTI.Modules.Shared;
using System;

namespace GTI.Modules.POS.Data
{
    // <summary>
    /// Represents a Bank Drop server message.
    /// </summary>
    internal class BankDropMessage : ServerMessage
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the CloseBankMessage class with the
        /// specified bank id.
        /// </summary>
        /// <param name="destinationBankId">The id of the bank that is the
        /// destination of the drop.</param>
        /// <param name="sourceBankId">The id of the bank that is the source of
        /// the drop.</param>
        /// <param name="closeBank">Whether to close the bank after the
        /// drop.</param>
        public BankDropMessage(int destinationBankId, int sourceBankId, bool closeBank)
        {
            m_id = 37009; // Bank Drop
            m_strMessageName = "Bank Drop";

            DestinationBankId = destinationBankId;
            SourceBankId = sourceBankId;
            CloseBank = closeBank;

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

            // Close Bank
            requestWriter.Write(CloseBank);

            // Source Bank Id
            requestWriter.Write(SourceBankId);

            // Destination Bank Id
            requestWriter.Write(DestinationBankId);

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

                requestWriter.Write((ushort)currency.Denominations.Count);

                foreach (Denomination denom in currency.Denominations)
                {
                    // Denom Id
                    requestWriter.Write(denom.Id);

                    // Count
                    requestWriter.Write(denom.Count);
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
        /// Gets or sets the id of the bank that is the destination of the
        /// drop.
        /// </summary>
        public int DestinationBankId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the id of the bank that is the source of the drop.
        /// </summary>
        public int SourceBankId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether the bank should be closed after the drop.
        /// </summary>
        public bool CloseBank
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the currencies to drop.
        /// </summary>
        private List<BankCurrency> Currencies
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the id of the transaction for the drop received from the
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
}