using System;
using System.IO;
using System.Text;
using GTI.Modules.Shared;

//US4338: (US1592) POS: Redeem B3

namespace GTI.Modules.POS.Data
{
    public class B3RedeemAccountMessage : ServerMessage
    {
        //B3 Redeem Account
        private readonly int m_account;
        private readonly int m_amount;
        public B3RedeemAccountMessage(int account, int amount)
        {
            m_id = 39065;
            m_strMessageName = "Redeem B3 Account";

            m_account = account;
            m_amount = amount;
        }

        protected override void PackRequest()
        {
            MemoryStream requestStream = new MemoryStream();
            BinaryWriter requestWriter = new BinaryWriter(requestStream, Encoding.Unicode);
            requestWriter.Write(m_account);
            requestWriter.Write(m_amount);
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

                SessionNumber = responseReader.ReadInt32();

                ReceiptNumber = responseReader.ReadInt32();

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

        public int ReceiptNumber { get; private set; }

        public int SessionNumber { get; private set; }
    }
}
