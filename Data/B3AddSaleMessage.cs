using System;
using System.IO;
using System.Text;
using GTI.Modules.Shared;

//US4382: (US4337) POS: B3 Open sale

namespace GTI.Modules.POS.Data
{
    public class B3AddSaleMessage : ServerMessage
    {
        //B3 Create Account message (39060)
        private int m_amount;
        public B3AddSaleMessage(int amount)
        {
            m_id = 39060;
            m_strMessageName = "Add B3 Sale";

            m_amount = amount;
        }

        protected override void PackRequest()
        {
            MemoryStream requestStream = new MemoryStream();
            BinaryWriter requestWriter = new BinaryWriter(requestStream, Encoding.Unicode);
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

                AccountNumber = responseReader.ReadInt32();

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

        public int AccountNumber { get; private set;  }

        public int ReceiptNumber { get; private set;  }
    }
}
