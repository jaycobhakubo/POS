#region Copyright
// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2008-2009 GameTech
// International, Inc.
#endregion

// FIX: DE1930
// Rally TA7465
//US4436: Close a bank from the POS

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using GTI.Modules.POS.Business;
using GTI.Modules.Shared;
using System;
using System.Linq;

namespace GTI.Modules.POS.Data
{
    internal class SetMachinePaperUsageMessage : ServerMessage
    {
        #region Local Variables

        private readonly int m_session;
        private readonly DateTime m_gamingDate;
        private readonly List<PaperUsageItem> m_items;
        private readonly bool m_createInventoryTransaction;
        private readonly bool m_closePaperUsage;
        

        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the CloseBankMessage class with the
        /// specified bank id.
        /// </summary>
        private SetMachinePaperUsageMessage(int session, DateTime gamingDate, List<PaperUsageItem> items, bool createInventoryTransaction, bool closePaperUsage)
        {
            m_id = 36045; // Set Machine Audit Number Message
            m_strMessageName = "Set Machine Paper Usage Message";

            m_session = session;
            m_gamingDate = gamingDate;
            m_items = items;
            m_createInventoryTransaction = createInventoryTransaction;
            m_closePaperUsage = closePaperUsage;
        }
        #endregion

        #region Member Methods
        /// <summary>
        /// Prepares the request to be sent to the server.
        /// </summary>
        protected override void PackRequest()
        {
            // Create the streams we will be writing to.
            MemoryStream requestStream = new MemoryStream();
            BinaryWriter requestWriter = new BinaryWriter(requestStream, Encoding.Unicode);


            if (m_items == null)
            {
                return;
            }

            //create an inventory transaction
            requestWriter.Write(m_createInventoryTransaction);

            //close paper usage
            requestWriter.Write(m_closePaperUsage);

            //session
            requestWriter.Write(m_session);

            //gaming date
            WriteDateTime(requestWriter, m_gamingDate);

            //iventory item count
            requestWriter.Write((ushort)m_items.Count);


            foreach (var item in m_items)
            {
                //paper usage ID
                requestWriter.Write(item.PaperUsageId);

                //inventory item ID
                requestWriter.Write(item.InventoryItemId);

                //Start Audit Number
                requestWriter.Write(item.AuditStart);

                //End Audit Number
                requestWriter.Write(item.AuditEnd);

                //bonanza
                requestWriter.Write(item.BonazaTrades);

                //price
                WriteString(requestWriter, item.Price.ToString(CultureInfo.InvariantCulture));

                //Quantity
                requestWriter.Write(item.Quantity);

                //Remove Item Flag
                //we never want to remove updated Items
                requestWriter.Write(item.IsMarkedForRemoval);

                requestWriter.Write(item.InventoryTransactionId);

                //US4955
                //damaged
                var paperUsageItem = item;
                var damageList = item.DamagedList.Where(i => i.AuditNumber >= paperUsageItem.AuditStart && i.AuditNumber <= paperUsageItem.AuditEnd).ToList();

                requestWriter.Write((short)damageList.Count);
                foreach (var audit in damageList)
                {
                    requestWriter.Write(audit.AuditNumber);


                    //DE13701 comments
                    WriteString(requestWriter, audit.Comment);

                    //DE13701 date time stamp
                    WriteString(requestWriter, audit.DateTimeStamp);
                }
            }
            
            // Set the bytes to be sent.
            m_requestPayload = requestStream.ToArray();

            // Close the streams.
            requestWriter.Close();
        }

        /// <summary>
        /// Sets the machine paper usage.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="sessionNumber">Session Number</param>
        /// <param name="gamingDate">The gaming date.</param>
        /// <param name="items">The update items.</param>
        /// <param name="removeItems">The remove items.</param>
        /// <param name="createInventoryTransaction">if set to <c>true</c> [create inventory transaction].</param>
        /// <param name="closePaperUsage">Close paper Usage flag</param>
        /// <returns></returns>
        public static int SetMachinePaperUsage(int sessionNumber, DateTime gamingDate, List<PaperUsageItem> items, bool createInventoryTransaction, bool closePaperUsage = false)
        {
            var message = new SetMachinePaperUsageMessage(sessionNumber, gamingDate, items, createInventoryTransaction, closePaperUsage);

            message.Send();

            return message.ReturnCode;
        }

        #endregion
    }
}