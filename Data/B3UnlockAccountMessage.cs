using System;
using System.IO;
using System.Text;
using GTI.Modules.Shared;

//US4395: (US1592) POS: B3 Unlock Accounts

namespace GTI.Modules.POS.Data
{
    public class B3UnlockAccountMessage : ServerMessage
    {
        //B3 Unlock Account
        private readonly int m_account;
        public B3UnlockAccountMessage(int account)
        {
            m_id = 39063;
            m_strMessageName = "Unlock B3 Account";

            m_account = account;
        }

        protected override void PackRequest()
        {
            MemoryStream requestStream = new MemoryStream();
            BinaryWriter requestWriter = new BinaryWriter(requestStream, Encoding.Unicode);
            requestWriter.Write(m_account);
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
