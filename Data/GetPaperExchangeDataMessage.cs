

using System;
using System.IO;
using System.Text;
using GTI.Modules.POS.Business;
using GTI.Modules.Shared;

namespace GTI.Modules.POS.Data
{
    public class GetPaperExchangeDataMessage : ServerMessage
    {
        #region Local Fields

        private readonly string m_barcode;
        #endregion

        #region Constructor

        private GetPaperExchangeDataMessage(string barcode)
        {
            m_id = 36046;
            m_strMessageName = "Get Paper Exchange Item";

            //init with default values
            m_barcode = barcode;
        }
        
        #endregion
     
        #region Properties

        public PaperExchangeItem Item { get; set; }

        #endregion

        #region Methods
        /// <summary>
        /// Prepares the request to be sent to the server.  All subclasses must
        /// implement this method.
        /// </summary>
        protected override void PackRequest()
        {            // Create the streams we will be writing to.
            MemoryStream requestStream = new MemoryStream();
            BinaryWriter requestWriter = new BinaryWriter(requestStream, Encoding.Unicode);

            // Barcode
            requestWriter.Write((ushort)m_barcode.Length);
            requestWriter.Write(m_barcode.ToCharArray());
            
            // Set the bytes to be sent.
            m_requestPayload = requestStream.ToArray();

            // Close the streams.
            requestWriter.Close();
        }

        /// <summary>
        /// Parses the response received from the server.
        /// </summary>
        protected override void UnpackResponse()
        {
            //// Reset the values.
            base.UnpackResponse();

            // Create the streams we will be reading from.
            MemoryStream responseStream = new MemoryStream(m_responsePayload);
            BinaryReader responseReader = new BinaryReader(responseStream, Encoding.Unicode);

            // Try to unpack the data.
            try
            {
                // Seek past return code.
                responseReader.BaseStream.Seek(sizeof(int), SeekOrigin.Begin);

                //receipt ID
                var receiptId = responseReader.ReadInt32();

                //Machine ID
                var machineId = responseReader.ReadInt32();

                //Audit #
                var audit = responseReader.ReadInt32();

                //Serial #
                ushort stringLen = responseReader.ReadUInt16();
                var serial = new string(responseReader.ReadChars(stringLen));
                
                //Product Name
                stringLen = responseReader.ReadUInt16();
                var productName = new string(responseReader.ReadChars(stringLen));

                //Staff first name
                stringLen = responseReader.ReadUInt16();
                var staffFirstName = new string(responseReader.ReadChars(stringLen));

                //Staff last name
                stringLen = responseReader.ReadUInt16();
                var staffLastName = new string(responseReader.ReadChars(stringLen));

                //staff full name
                var staffName = string.Format("{0} {1}", staffFirstName, staffLastName);

                Item = new PaperExchangeItem
                {
                    Name = productName,
                    Serial = serial,
                    Audit = audit,
                    Cashier = staffName,
                    Reciept = receiptId,
                    Machine = machineId
                };
            }
            catch (EndOfStreamException e)
            {
                Item = null;
                throw new MessageWrongSizeException(m_strMessageName, e);
            }
            catch (Exception e)
            {
                Item = null;
                throw new ServerException(m_strMessageName, e);
            }

            //Close the streams.
            responseReader.Close();
        }

        #endregion

        internal static int GetPaperExchangeData(string barcode, out PaperExchangeItem item)
        {
            var msg = new GetPaperExchangeDataMessage(barcode);

            try
            {
                msg.Send();
            }
            catch (Exception)
            {
                item = null;

                //general error return code
                return -1;
            }

            item = msg.Item;

            return msg.ReturnCode;
        }
    }
}
