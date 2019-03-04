using System;
using System.Diagnostics.PerformanceData;
using System.IO;
using System.Text;
using GTI.Modules.Shared;

//US4338: (US1592) POS: Redeem B3
//DE13131: Error found in US4395: (US1592) POS: B3 Unlock Accounts > Does not display Wins and Total Due

namespace GTI.Modules.POS.Data
{
    public class B3GetSettingsMessage : ServerMessage
    {
        public B3GetSettingsMessage()
        {
            m_id = 39004;
            m_strMessageName = "Get B3 Settings";
        }

        protected override void PackRequest()
        {
            MemoryStream requestStream = new MemoryStream();
            BinaryWriter requestWriter = new BinaryWriter(requestStream, Encoding.Unicode);

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

                //DE13131 BEGIN
                var count = responseReader.ReadInt16();

                for (int i = 0; i < count; i++)
                {
                    //read settings ID
                    var settingId = responseReader.ReadInt32();

                    //setting category is not used
                    responseReader.ReadInt32();

                    //game id is not used
                    responseReader.ReadInt32();

                    //setting value
                    var settingValue = new string(responseReader.ReadChars(responseReader.ReadInt16()));

                    switch (settingId)
                    {
                        case 53:
                            IsCommonRng = settingValue == "T";
                            break;
                        case 52:
                            IsMultiOperator = settingValue == "T";
                            break;
                        case 51:
                            IsDoubleAccount = settingValue == "T";
                            break;
                        case 41:
                            EnforceMix = settingValue == "T";
                            break;
                        case 30:
                            AllowInSessBallChange = settingValue == "T";
                            break;
                    }
                }
                //DE13131 END
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

        public bool IsMultiOperator { get; private set; }
        public bool IsCommonRng { get; private set; }
        public bool AllowInSessBallChange { get; private set; }
        public bool EnforceMix { get; private set; }
        public bool IsDoubleAccount { get; private set; }
    }
}
