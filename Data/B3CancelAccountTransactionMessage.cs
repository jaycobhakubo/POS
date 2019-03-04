using System.IO;
using System.Text;
using GTI.Modules.Shared;

//US4395: (US1592) POS: B3 Unlock Accounts

namespace GTI.Modules.POS.Data
{
    public class B3CancelAccountTransaction : ServerMessage
    {
        //B3 Unlock Account
        private readonly int m_account;
        private readonly int m_status;
        public B3CancelAccountTransaction(int account, int status)
        {
            m_id = 39062;
            m_strMessageName = "Cancel B3 Account Transaction";

            m_account = account;
            m_status = status;
        }

        protected override void PackRequest()
        {
            MemoryStream requestStream = new MemoryStream();
            BinaryWriter requestWriter = new BinaryWriter(requestStream, Encoding.Unicode);
            
            requestWriter.Write(m_account);
            requestWriter.Write(m_status);

            m_requestPayload = requestStream.ToArray();
            requestWriter.Close();
        }
    }
}
