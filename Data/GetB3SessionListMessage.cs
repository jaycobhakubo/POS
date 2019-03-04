using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GTI.Modules.POS.Business;
using GTI.Modules.Shared;

namespace GTI.Modules.POS.Data
{
    public class GetB3SessionListMessage : ServerMessage
    {
        #region Contructors
        public GetB3SessionListMessage()
        {
            m_id = 39005;
            m_strMessageName = "Get B3 Session List";

            SessionList = new List<B3Session>();
        }
        #endregion

        #region Member Methods

        /// <summary>
        /// packs the request to send to the server
        /// </summary>
        /// <param name="requestWriter"></param>
        protected override void PackRequest()
        {

        }


        protected override void UnpackResponse()
        {
            base.UnpackResponse();

            // Create the streams we will be reading from.
            MemoryStream responseStream = new MemoryStream(m_responsePayload);
            BinaryReader responseReader = new BinaryReader(responseStream, Encoding.Unicode);

            // Seek past return code.
            responseReader.BaseStream.Seek(sizeof(int), SeekOrigin.Begin);

            if (ReturnCode == 0)
            {
                var count = responseReader.ReadInt16();

                for (int i = 0; i < count; i++)
                {
                    var number = responseReader.ReadInt32();

                    var nameLength = responseReader.ReadInt16();

                    var name = responseReader.ReadChars(nameLength);

                    var startLength = responseReader.ReadInt16();

                    var startTime = responseReader.ReadChars(startLength);

                    var endLength = responseReader.ReadInt16();

                    var endTime = responseReader.ReadChars(endLength);

                    var active = responseReader.ReadBoolean();

                    var session = new B3Session(number, active, new string(name), new string(startTime), new string(endTime));

                    SessionList.Add(session);
                }
            }

            responseStream.Close();
            // Close the streams.
            responseReader.Close();
        }
        
        public string Name
        {
            get
            {
                return "Get B3 Session List";
            }
        }

        public List<B3Session> SessionList
        {
            get;
            private set;
        }

        #endregion
    }
}
