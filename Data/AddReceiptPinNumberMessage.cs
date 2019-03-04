using System.IO;
using System.Text;
using GTI.Modules.Shared;

namespace GTI.Modules.POS.Data
{
    internal class SetReceiptPinNumberMessage : ServerMessage
    {
        private readonly int m_receiptId;
        private readonly byte[] m_pin;
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the AddSaleMessage class.
        /// </summary>
        public SetReceiptPinNumberMessage(int receiptId, byte[] pin)
        {
            m_pin = pin;
            m_id = 18216;
            m_strMessageName = "Set Receipt Pin Number";
            m_receiptId = receiptId;
        }
        #endregion

        protected override void PackRequest()
        {// Create the streams we will be writing to.
            MemoryStream requestStream = new MemoryStream();
            BinaryWriter requestWriter = new BinaryWriter(requestStream, Encoding.Unicode);

            //Register Receipt Id
            requestWriter.Write(ReceiptId);
            
            //Receipt Pin Number
            requestWriter.Write(ReceiptPin);

            // Set the bytes to be sent.
            m_requestPayload = requestStream.ToArray();

            // Close the streams.
            requestWriter.Close();
        }

        public int ReceiptId 
        {
            get
            {
                return m_receiptId;
            }
        }

        public byte[] ReceiptPin
        {
            get
            {
                return m_pin;
            }
        }
    }
}
