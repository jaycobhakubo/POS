// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2016 FortuNet

//US4549: POS: Re-open a bank

using System;
using System.IO;
using System.Text;
using GTI.Modules.Shared;

namespace GTI.Modules.POS.Data
{
    public class CheckForClosedBankMessage : ServerMessage
    {
        #region Member Variables

        private readonly short m_session;
        private int m_bankId;

        #endregion

        #region Constructors


        /// <summary>
        /// Initializes a new instance of the <see cref="CheckForClosedBankMessage"/> class.
        /// </summary>
        /// <param name="session">The session.</param>
        private CheckForClosedBankMessage(short session)
        {
            m_id = 37036;
            m_bankId = 0;
            m_session = session;
        }
        #endregion

        #region Member Methods

        protected override void PackRequest()
        {
            // Create the streams that will be written to
            MemoryStream requestStream = new MemoryStream();
            BinaryWriter requestWriter = new BinaryWriter(requestStream, Encoding.Unicode);

            // Session Number
            requestWriter.Write(m_session);

            // Set the bytes to be sent
            m_requestPayload = requestStream.ToArray();

            // close the streams
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

                m_bankId = responseReader.ReadInt32();

            }
            catch (EndOfStreamException e)
            {
                throw new MessageWrongSizeException("Check For Closed Bank", e);
            }
            catch (Exception e)
            {
                throw new ServerException("Check For Closed Bank", e);
            }

            // Close the streams.
            responseReader.Close();
        }

        #endregion


        /// <summary>
        /// Gets the closed bank identifier for the current staff and gaming date.
        /// zero if there is not a closed bank.
        /// </summary>
        /// <returns></returns>
        public static int GetClosedBankId(short session)
        {
            var msg = new CheckForClosedBankMessage(session);

            msg.Send();

            return msg.m_bankId;
        }
    }
}
