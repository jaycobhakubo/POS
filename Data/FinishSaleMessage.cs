#region Copyright
// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2008 GameTech
// International, Inc.
#endregion

using System;
using System.IO;
using System.Text;
using GTI.Modules.Shared;
using GTI.Modules.POS.Business;
using GTI.Modules.POS.Properties;
using System.Collections.Generic;

namespace GTI.Modules.POS.Data
{
    /// <summary>
    /// Represents a Finish Sale server message.
    /// </summary>
    internal class FinishSaleMessage : ServerMessage
    {
        protected const int MinResponseMessageLength = 6;

        #region Member Variables
        protected int m_registerReceiptId;
        protected bool m_saleSuccess = true; // Rally TA6177
        protected short m_unitNum;
        protected string m_serialNum;
        protected bool m_isQuantitySale; // Rally US556
        protected List<string> m_afterReceiptText = new List<string>();
        #endregion

        #region Constructors
        // Rally TA6177
        // Rally US556
        /// <summary>
        /// Initializes a new instance of the FinishSaleMessage class.
        /// </summary>
        public FinishSaleMessage()
            : this(0, true, 0, null, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the FinishSaleMessage class 
        /// with the specified parameters.
        /// </summary>
        /// <param name="registerReceptId">The id of the sale to 
        /// finish.</param>
        /// <param name="saleSuccess">true if the sale was completely
        /// successful; otherwise false if some step(s) in the process
        /// failed.</param>
        /// <param name="unitNumber">The unit number that was sold 
        /// to or 0 if no unit.</param>
        /// <param name="serialNumber">The serial number that was sold 
        /// to or null if no unit.</param>
        /// <param name="isQuantitySale">true if this sale is part of a
        /// quantity sale; otherwise false.</param>
        public FinishSaleMessage(int registerReceptId, bool saleSuccess, short unitNumber, string serialNumber, bool isQuantitySale)
        {
            m_id = 18018; // Finish Sale
            m_strMessageName = "Finish Sale";
            m_registerReceiptId = registerReceptId;
            m_saleSuccess = saleSuccess;
            m_unitNum = unitNumber;
            SerialNumber = serialNumber;
            m_isQuantitySale = isQuantitySale;
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

            // Register Receipt Id
            requestWriter.Write(m_registerReceiptId);

            // Sale Status
            requestWriter.Write(m_saleSuccess);

            // Unit Number
            requestWriter.Write(m_unitNum);

            // Unit Serial #
            if(m_serialNum != null)
            {
                requestWriter.Write((ushort)m_serialNum.Length);
                requestWriter.Write(m_serialNum.ToCharArray());
            }
            else
                requestWriter.Write((ushort)0);

            // Rally US556
            // Is Quantity Sale
            requestWriter.Write(m_isQuantitySale);

            // Set the bytes to be sent.
            m_requestPayload = requestStream.ToArray();

            // Close the streams.
            requestWriter.Close();
        }
        // END: TA6177


        protected override void UnpackResponse()
        {
            base.UnpackResponse();

            // Create the streams we will be reading from.
            var responseStream = new MemoryStream(m_responsePayload);
            var responseReader = new BinaryReader(responseStream, Encoding.Unicode);

            // Check the response length.
            if (responseStream.Length < MinResponseMessageLength)
                throw new MessageWrongSizeException(m_strMessageName);

            // Try to unpack the data.
            try
            {
                // Seek past return code.
                responseReader.BaseStream.Seek(sizeof(int), SeekOrigin.Begin);

                //see if there is anything to print after the receipt
                var count = responseReader.ReadInt16();

                for (int i = 0; i < count; i++)
                {
                    int textLength = responseReader.ReadUInt16();
                    string text = new string(responseReader.ReadChars(textLength));

                    m_afterReceiptText.Add(text);
                }
            }
            catch (EndOfStreamException e)
            {
                throw new MessageWrongSizeException(m_strMessageName, e);
            }
            catch (Exception e)
            {
                throw new ServerException(m_strMessageName, e);
            }

            // Close the streams.
            responseReader.Close();
        }
        #endregion

        #region Member Properties
        /// <summary>
        /// Gets or sets the id of the sale on the server.
        /// </summary>
        public int RegisterReceiptId
        {
            get
            {
                return m_registerReceiptId;
            }
            set
            {
                m_registerReceiptId = value;
            }
        }

        /// <summary>
        /// Gets a list of strings to print after the receipt.
        /// A string starting with "Code128=" will print the rest of the string as a code 128 barcode.
        /// </summary>
        public List<string> AfterReceiptText
        {
            get
            {
                return m_afterReceiptText;
            }
        }

        // Rally TA6177
        /// <summary>
        /// Gets or sets whether the sale was completed successfully or if some
        /// step(s) of the sale failed.
        /// </summary>
        public bool SaleSuccess
        {
            get
            {
                return m_saleSuccess;
            }
            set
            {
                m_saleSuccess = value;
            }
        }
        // END: TA6177

        /// <summary>
        /// Gets or sets the unit number sold to (or 0 if no unit).
        /// </summary>
        public short UnitNumber
        {
            get
            {
                return m_unitNum;
            }
            set
            {
                m_unitNum = value;
            }
        }

        /// <summary>
        /// Gets or sets the serial number of the unit sold to 
        /// (or null if no unit).
        /// </summary>
        public string SerialNumber
        {
            get
            {
                return m_serialNum;
            }
            set
            {
                if(value == null)
                    m_serialNum = null;
                else if(value.Length <= StringSizes.MaxSerialNumberLength)
                    m_serialNum = value;
                else
                    throw new ArgumentException("SerialNumber" + Resources.TooBig);
            }
        }

        // Rally US556
        /// <summary>
        /// Gets or sets whether this sale is part of a quantity sale.
        /// </summary>
        public bool IsQuantitySale
        {
            get
            {
                return m_isQuantitySale;
            }
            set
            {
                m_isQuantitySale = value;
            }
        }
        #endregion
    }
}
