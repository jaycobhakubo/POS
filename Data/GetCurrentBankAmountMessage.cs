#region Copyright
// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2008 GameTech
// International, Inc.
#endregion

// TTP 50137
// Rally TA7465

using System;
using System.Globalization;
using System.IO;
using System.Text;
using GTI.Modules.POS.Business;
using GTI.Modules.Shared;
using System.Collections.Generic;

namespace GTI.Modules.POS.Data
{
    // FIX: DE1930
    /// <summary>
    /// Represents a Get Current Bank Amount message.
    /// </summary>
    internal class GetCurrentBankAmountMessage : ServerMessage
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the GetCurrentBankAmountMessage 
        /// class.
        /// </summary>
        /// <param name="type">The type of bank to get.</param>
        public GetCurrentBankAmountMessage(BankType type)
        {
            m_id = 37012; // Get Current Bank Amount
            m_strMessageName = "Get Current Bank Amount";

            BankType = type;
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

            // Bank Type
            requestWriter.Write((int)BankType);

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
            Bank = null;

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
                int bankId = responseReader.ReadInt32();

                if(bankId > 0)
                {
                    Bank = new Bank();

                    Bank.Id = bankId;

                    // Staff Id
                    Bank.StaffId = responseReader.ReadInt32();

                    // Machine Id
                    Bank.MachineId = responseReader.ReadInt32();

                    // Bank Type
                    Bank.Type = (BankType)Enum.Parse(typeof(BankType), responseReader.ReadInt32().ToString(CultureInfo.InvariantCulture));

                    // Bank Name
                    ushort stringLen = responseReader.ReadUInt16();
                    Bank.Name = new string(responseReader.ReadChars(stringLen));

                    // Currency Count
                    ushort currencyCount = responseReader.ReadUInt16();

                    // Currency List
                    for(ushort x = 0; x < currencyCount; x++)
                    {
                        // Currency ISO Code
                        stringLen = responseReader.ReadUInt16();
                        BankCurrency currency = new BankCurrency(new string(responseReader.ReadChars(stringLen)));

                        // Tender Type Count
                        ushort tenderCount = responseReader.ReadUInt16();

                        for(ushort y = 0; y < tenderCount; y++)
                        {
                            // Tender Type Id
                            TenderType tenderType = (TenderType)Enum.Parse(typeof(TenderType), responseReader.ReadInt32().ToString(CultureInfo.InvariantCulture));

                            // Value
                            decimal value = 0M;
                            stringLen = responseReader.ReadUInt16();
                            string tempDec = new string(responseReader.ReadChars(stringLen));

                            if(!string.IsNullOrEmpty(tempDec))
                                value = decimal.Parse(tempDec, CultureInfo.InvariantCulture);

                            // The POS only cares about cash.
                            if(tenderType == TenderType.Cash)
                                currency.Total = value;
                        }

                        Bank.Currencies.Add(currency);
                    }

                    Bank.Sort();
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
        /// Gets or sets the type of bank to get.
        /// </summary>
        public BankType BankType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the bank the staff/machine is currently using or null if there
        /// was no bank.
        /// </summary>
        public Bank Bank
        {
            get;
            private set;
        }
        #endregion
    }
    // END: DE1930
}
