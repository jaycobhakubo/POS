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
    public class ReopenBankMessage : ServerMessage
    {
        #region Member Variables

        private readonly int m_bankId;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ReopenBankMessage"/> class.
        /// </summary>
        /// <param name="bankId">The bank identifier.</param>
        private ReopenBankMessage(int bankId)
        {
            m_id = 37037;
            m_bankId = bankId;
        }
        #endregion

        #region Methods

        protected override void PackRequest()
        {
            // Create the streams that will be written to
            MemoryStream requestStream = new MemoryStream();
            BinaryWriter requestWriter = new BinaryWriter(requestStream, Encoding.Unicode);

            //Bank Id
            requestWriter.Write(m_bankId);

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
            }
            catch (EndOfStreamException e)
            {
                throw new MessageWrongSizeException("Repoen Bank ", e);
            }
            catch (Exception e)
            {
                throw new ServerException("Repoen Bank ", e);
            }

            // Close the streams.
            responseReader.Close();
        }

        /// <summary>
        /// Gets the closed bank identifier for the current staff and gaming date.
        /// Returns server return code
        /// </summary>
        /// <returns></returns>
        public static int ReopenBank(int bankId)
        {
            var msg = new ReopenBankMessage(bankId);

            msg.Send();

            return msg.ReturnCode;
        }

        #endregion

    }
}
