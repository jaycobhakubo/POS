using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GTI.Modules.Shared;
using System.IO;

namespace GTI.Modules.POS.Data
{
    internal class CheckSerialAuditNumbersMessage : ServerMessage
    {
        #region Member Variables
        private int m_productId;
        private string m_serialNumber;
        private int m_auditNumber;
        private int m_status;
        private int m_cardCount;
        #endregion

        #region Constructors

        public CheckSerialAuditNumbersMessage(int productId, string serialNumber, int auditNumber)
        {
            m_id = 18214;
            m_strMessageName = "Check Serial and Audit Numbers";
            m_productId = productId;
            m_serialNumber = serialNumber;
            m_auditNumber = auditNumber;
            m_status = 0;
        }

        #endregion

        #region Member Methods
        protected override void PackRequest()
        {
            // Create the streams we will be writing to.
            MemoryStream requestStream = new MemoryStream();
            BinaryWriter requestWriter = new BinaryWriter(requestStream, Encoding.Unicode);

            requestWriter.Write(m_productId);
            requestWriter.Write(m_auditNumber);

            requestWriter.Write((ushort)m_serialNumber.Length);
            requestWriter.Write(m_serialNumber.ToCharArray());

            // Set the bytes to be sent.
            m_requestPayload = requestStream.ToArray();

            // Close the streams.
            requestWriter.Close();
        }

        protected override void UnpackResponse()
        {
            m_status = 0;

            base.UnpackResponse();

            // Create the streams we will be reading from.
            MemoryStream responseStream = new MemoryStream(m_responsePayload);
            BinaryReader responseReader = new BinaryReader(responseStream, Encoding.Unicode);

            // Try to unpack the data.
            try
            {
                // Seek past return code.
                responseReader.BaseStream.Seek(sizeof(int), SeekOrigin.Begin);

                // the status of the check
                m_status = responseReader.ReadInt32();

                m_cardCount = responseReader.ReadInt32();

                m_serialNumber = ReadString(responseReader);
                m_serialNumber = m_serialNumber.TrimStart(new char[] { ' ', '0' });

                m_auditNumber = responseReader.ReadInt32();
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
        #endregion

        #region Member Properties
        public int Status
        {
            get { return m_status; }
        }

        public int CardCount
        {
            get { return m_cardCount; }
        }

        public string SerialNumber
        {
            get
            {
                return m_serialNumber;}
        }

        public int AuditNumber
        {
            get
            {
                return m_auditNumber;
            }
        }


        #endregion
    }
}

