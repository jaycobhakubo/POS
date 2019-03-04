#region Copyright
// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2008-2009 GameTech
// International, Inc.
#endregion

using System.Collections.Generic;
using System.IO;
using System.Text;
using GTI.Modules.POS.Business;
using GTI.Modules.Shared;

namespace GTI.Modules.POS.Data
{
    internal class UpdateMachinePaperUsageLevelsMessage : ServerMessage
    {

        private readonly int m_receiptId;

        private UpdateMachinePaperUsageLevelsMessage(int receiptId)
        {
            m_receiptId = receiptId;
            m_id = 36047; // Get Machine Audit Number Message
            m_strMessageName = "Update Machine Paper Usage Levels";
        }

        #region Member Methods
        /// <summary>
        /// Prepares the request to be sent to the server.
        /// </summary>
        protected override void PackRequest()
        {
            // Create the streams we will be writing to.
            MemoryStream requestStream = new MemoryStream();
            BinaryWriter requestWriter = new BinaryWriter(requestStream, Encoding.Unicode);

            //End Audit Number
            requestWriter.Write(m_receiptId);
            
            // Set the bytes to be sent.
            m_requestPayload = requestStream.ToArray();

            // Close the streams.
            requestWriter.Close();
        }

        /// <summary>
        /// Sets the machine audit numbers.
        /// </summary>
        /// <param name="receiptId">Receipt ID</param>
        /// <returns></returns>
        public static int UpdateMachineAuditNumbers(int receiptId)
        {
            var message = new UpdateMachinePaperUsageLevelsMessage(receiptId);

            message.Send();

            return message.ReturnCode;
        }

        #endregion

    }
}
