#region Copyright
// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2014 FortuNet, Inc.
//
// US2977 Adding support for selling barcoded paper
#endregion

using System;
using System.Collections.Generic;

using System.Text;
using System.IO;
using GTI.Modules.Shared;

namespace GTI.Modules.POS.Data
{
    internal class BarcodeProductItem : IEquatable<BarcodeProductItem>
    {
        #region Member Variables
        protected string m_serialNumber;
        protected string m_productName;
        protected int m_auditNumber;
        protected int m_status;
        protected int m_cardCount;
        protected BarcodeType m_barcodeType = BarcodeType.UnknownBarcodeType;
        #endregion

        #region Constructors
        public BarcodeProductItem()
        {
            AuditNumber = 0;
            Status = 0;
            SessionKeyItems = new List<SessionKeyItem>();
        }

        public BarcodeProductItem(int auditNumber, int status, string serialNumber, string productName)
        {
            AuditNumber = auditNumber;
            Status = status;
            SerialNumber = serialNumber;
            Name = productName;
            SessionKeyItems = new List<SessionKeyItem>();
        }

        ~BarcodeProductItem()
        {
        }

        public override bool Equals(Object obj)
        {
            if (obj == null)
                return false;

            BarcodeProductItem barcodeObj = obj as BarcodeProductItem;
            if (barcodeObj == null)
                return false;
            else
                return Equals(barcodeObj);
        }
        #endregion

        #region Member Methods

        public SessionKeyItem FindSessionItem(int sessionId)
        {
            SessionKeyItem item = null;

            foreach (SessionKeyItem keyItem in SessionKeyItems)
            {
                if (keyItem.Session == sessionId)
                {
                    item = keyItem;
                    break;
                }
            }

            return item;
        }

        public bool Equals(BarcodeProductItem other)
        {
            if (other == null)
                return false;

            if (this.AuditNumber == other.AuditNumber &&
                this.SerialNumber == other.SerialNumber &&
                this.Name == other.Name &&
                this.Status == other.Status &&
                this.CardCount == other.CardCount)//US3509
                return true;
            else
                return false;
        }

        public override int GetHashCode()
        {
            return (base.GetHashCode() ^ m_auditNumber.GetHashCode() ^ m_status.GetHashCode() ^
                    m_productName.GetHashCode() ^ m_serialNumber.GetHashCode());
        }
        #endregion

        #region Member Properties

        /// <summary>
        /// Gets or sets the Barcode type for this item
        /// </summary>
        public BarcodeType ItemBarcodeType
        {
            get
            {
                return m_barcodeType;
            }
            set
            {
                m_barcodeType = value;
            }
        }

        /// <summary>
        /// Gets or sets the Serial number for this item
        /// </summary>
        public string SerialNumber
        {
            get { return m_serialNumber; }
            set { m_serialNumber = value; }
        }

        /// <summary>
        /// Gets or sets the Audit number of this item
        /// </summary>
        public int AuditNumber
        {
            get { return m_auditNumber; }
            set { m_auditNumber = value; }
        }

        /// <summary>
        /// Gets or set the name of this product
        /// </summary>
        public string Name
        {
            get { return m_productName; }
            set { m_productName = value; }
        }

        /// <summary>
        /// Gets or sets the status of this item
        /// </summary>
        public int Status
        {
            get { return m_status; }
            set { m_status = value; }
        }

        //US3509
        /// <summary>
        /// Gets or sets the card count.
        /// </summary>
        /// <value>
        /// The card count.
        /// </value>
        public int CardCount
        {
            get { return m_cardCount; }
            set { m_cardCount = value; }
        }

        public List<SessionKeyItem> SessionKeyItems
        {
            get;
            set;
        }

        #endregion
    }

    /// <summary>
    /// Represents a Get Product by barcode message.
    /// </summary>
    internal class DecodeBarcodeMessage : ServerMessage
    {
        #region Member Variables
        protected string m_barcode;
        protected BarcodeType m_barcodeType = 0;
        protected BarcodeProductItem m_item;
        #endregion

        #region Constructors
        public DecodeBarcodeMessage(string barcode)
        {
            m_id = 18202;
            m_barcode = barcode;
        }
        #endregion

        #region Member Methods
        protected override void PackRequest()
        {
            MemoryStream requestStream = new MemoryStream();
            BinaryWriter requestWriter = new BinaryWriter(requestStream, Encoding.Unicode);

            requestWriter.Write((ushort)m_barcode.Length);
            requestWriter.Write(m_barcode.ToCharArray());

            // set the bytes to be sent
            m_requestPayload = requestStream.ToArray();

            requestWriter.Close();
        }

        protected override void UnpackResponse()
        {
            base.UnpackResponse();

            m_item = null;

            // Create the streams that will be read from
            MemoryStream responseStream = new MemoryStream(m_responsePayload);
            BinaryReader responseReader = new BinaryReader(responseStream, Encoding.Unicode);

            try
            {
                // Seek past the return code
                responseReader.BaseStream.Seek(sizeof(int), SeekOrigin.Begin);

                // Determine which barcode type we are dealing with
                m_barcodeType = (BarcodeType)responseReader.ReadInt32();
                
                BarcodeProductItem item = new BarcodeProductItem();

                switch(m_barcodeType)
                {
                    case BarcodeType.PaperScanCode:
                        item = ParseBarcodedPaperResult(responseReader);
                        break;

                    case BarcodeType.ProductScanCode:
                        item = ParseProductScanCodeResult(responseReader);
                        break;
                }

                m_item = item;

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

        private static BarcodeProductItem ParseProductScanCodeResult(BinaryReader reader)
        {
            BarcodeProductItem item = new BarcodeProductItem();
            item.ItemBarcodeType = BarcodeType.ProductScanCode;

            item.Name = "scanned";

            ushort itemCount = reader.ReadUInt16();

            for(ushort n = 0; n < itemCount; ++n)
            {
                SessionKeyItem keyItem = new SessionKeyItem();

                keyItem.Session = reader.ReadInt32();
                keyItem.Page = reader.ReadInt32();
                keyItem.Key = reader.ReadInt32();
                keyItem.PackageID = reader.ReadInt32();

                item.SessionKeyItems.Add(keyItem);
            }

            return item;
        }

        private static BarcodeProductItem ParseBarcodedPaperResult(BinaryReader reader)
        {
            BarcodeProductItem item = new BarcodeProductItem();

            item.ItemBarcodeType = BarcodeType.PaperScanCode;

            item.Status = reader.ReadInt32();
            item.AuditNumber = reader.ReadInt32();

            ushort stringLen = reader.ReadUInt16();
            item.SerialNumber = new string(reader.ReadChars(stringLen));

            stringLen = reader.ReadUInt16();
            item.Name = new string(reader.ReadChars(stringLen));

            item.CardCount = reader.ReadInt32();//US3509

            ushort itemCount = reader.ReadUInt16();
            for(ushort n = 0; n < itemCount; ++n)
            {
                SessionKeyItem keyItem = new SessionKeyItem();

                keyItem.Session = reader.ReadInt32();
                keyItem.Page = reader.ReadInt32();
                keyItem.Key = reader.ReadInt32();
                keyItem.PackageID = reader.ReadInt32();

                item.SessionKeyItems.Add(keyItem);
            }

            return item;
        }
        #endregion

        #region Member Properties
        public BarcodeProductItem BarcodeItem
        {
            get { return m_item;}
        }
        #endregion
    }
}
