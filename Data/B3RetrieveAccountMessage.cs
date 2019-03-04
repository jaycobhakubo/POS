using System;
using System.IO;
using System.Text;
using GTI.Modules.Shared;

//US4397: (US1592) POS: B3 Hand Pay

namespace GTI.Modules.POS.Data
{
    public class B3RetrieveAccountMessage : ServerMessage
    {
        //B3 Retrieve Account
        private readonly int m_accountNumber;
        public B3RetrieveAccountMessage(int account)
        {
            m_id = 39061;
            m_strMessageName = "Retrieve B3 Account Info";

            m_accountNumber = account;
        }

        protected override void PackRequest()
        {
            MemoryStream requestStream = new MemoryStream();
            BinaryWriter requestWriter = new BinaryWriter(requestStream, Encoding.Unicode);
            requestWriter.Write(m_accountNumber);
            m_requestPayload = requestStream.ToArray();
            requestWriter.Close();
        }

        protected override void UnpackResponse()
        {
            base.UnpackResponse();

            MemoryStream responseStream = new MemoryStream(m_responsePayload);
            BinaryReader responseReader = new BinaryReader(responseStream, Encoding.Unicode);
            // Try to unpack the data.
            try
            {
                // Seek past return code.
                responseReader.BaseStream.Seek(sizeof(int), SeekOrigin.Begin);

                Status = responseReader.ReadInt32();

                Credit = responseReader.ReadInt32();

                WinCredit = responseReader.ReadInt32();

                HandPay = responseReader.ReadInt32();

                Taxable = responseReader.ReadInt32();
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

        public int Status { get; private set; }

        /// <summary>
        /// Gets the credit amount.
        /// </summary>
        /// <value>
        /// The credit.
        /// </value>
        public int Credit { get; private set; }
        /// <summary>
        /// Gets the win credit amount.
        /// </summary>
        /// <value>
        /// The win credit.
        /// </value>
        public int WinCredit { get; private set; }
        /// <summary>
        /// Gets the hand pay amount.
        /// </summary>
        /// <value>
        /// The hand pay.
        /// </value>
        public int HandPay { get; private set; }
        /// <summary>
        /// Gets the taxable amount.
        /// </summary>
        /// <value>
        /// The taxable.
        /// </value>
        public int Taxable { get; private set; }
    }
}
