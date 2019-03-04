using System;
using System.IO;
using System.Text;
using GTI.Modules.Shared;

//US4404: (US1592) POS: B3 Jackpot Payment

namespace GTI.Modules.POS.Data
{
    public class B3JackpotAccountMessage : ServerMessage
    {
        //B3 Unlock Account
        private readonly int m_account;
        private readonly int m_session;
        public B3JackpotAccountMessage(int account, int session)
        {
            m_id = 39064;
            m_strMessageName = "B3 Jackpot Account";

            m_account = account;
            m_session = session;
        }

        protected override void PackRequest()
        {
            MemoryStream requestStream = new MemoryStream();
            BinaryWriter requestWriter = new BinaryWriter(requestStream, Encoding.Unicode);
            requestWriter.Write(m_account);
            requestWriter.Write(m_session);
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
                
                //session #
                SessionNumber = responseReader.ReadInt32();

                //Active session
                ActiveSession = responseReader.ReadBoolean();

                //session start
                var length = responseReader.ReadInt16();
                SessionStart = new string(responseReader.ReadChars(length));

                //session end
                length = responseReader.ReadInt16();
                SessionEnd = new string(responseReader.ReadChars(length));

                //Operator Name
                length = responseReader.ReadInt16();
                OperatorName = new string(responseReader.ReadChars(length));

                //Client MAC
                length = responseReader.ReadInt16();
                ClientMac = new string(responseReader.ReadChars(length));

                //Client name
                length = responseReader.ReadInt16();
                ClientName = new string(responseReader.ReadChars(length));

                //Account  #
                AccountNumber = responseReader.ReadInt32();

                //Game name
                length = responseReader.ReadInt16();
                GameName = new string(responseReader.ReadChars(length));

                //Game  #
                GameNumber = responseReader.ReadInt32();

                //Jackpot limit
                JackpotLimit = responseReader.ReadInt32();

                //Jackpot event
                length = responseReader.ReadInt16();
                JackpotDateTime = new string(responseReader.ReadChars(length));


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

        public int SessionNumber { get; private set; }

        public bool ActiveSession { get; private set; }

        public string SessionStart { get; private set; }

        public string SessionEnd { get; private set; }

        public string OperatorName { get; private set; }

        public string ClientMac { get; private set; }

        public string ClientName { get; private set; }

        public int AccountNumber { get; private set; }

        public string GameName { get; private set; }
        
        public int GameNumber { get; private set; }

        public int JackpotLimit { get; private set; }

        public string JackpotDateTime { get; private set; }
    }
}
