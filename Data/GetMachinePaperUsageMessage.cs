#region Copyright
// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2008-2009 GameTech
// International, Inc.
#endregion

// FIX: DE1930
// Rally TA7465
//US4436: Close a bank from the POS
//US4955: POS > Paper Usage: Damaged

using System.Collections.Generic;
using System.IO;
using System.Text;
using GTI.Modules.POS.Business;
using GTI.Modules.Shared;
using System;

namespace GTI.Modules.POS.Data
{
    internal class GetMachinePaperUsageMessage : ServerMessage
    {
        #region Local

        private readonly bool m_isReOpen;
        #endregion
        #region Constructors
        /// <summary>
        /// Prevents a default instance of the <see cref="GetMachinePaperUsageMessage"/> class from being created.
        /// </summary>
        private GetMachinePaperUsageMessage(bool isReOpen)
        {
            m_isReOpen = isReOpen;
            m_id = 36044; // Get Machine Audit Number Message
            m_strMessageName = "Get Machine Paper Usage";
            PaperUsageItems = new List<PaperUsageItem>();
        }
        #endregion

        #region Member 
        /// <summary>
        /// Prepares the request to be sent to the server.
        /// </summary>
        protected override void PackRequest()
        {
            // Create the streams we will be writing to.
            MemoryStream requestStream = new MemoryStream();
            BinaryWriter requestWriter = new BinaryWriter(requestStream, Encoding.Unicode);

            //re open flag
            requestWriter.Write(m_isReOpen);

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

                var count = responseReader.ReadUInt16();

                for (int i = 0; i < count; i++)
                {
                    //paper usage ID
                    var paperUsageId = responseReader.ReadInt32();

                    //inventory item ID
                    var inventoryItemId = responseReader.ReadInt32();

                    //product name
                    var name = ReadString(responseReader); 
                    
                    //product name
                    var serialNumber = ReadString(responseReader);

                    //start number
                    var startAuditNumber = responseReader.ReadInt32();

                    //end number
                    var endAuditNumber = responseReader.ReadInt32();

                    //bonanza
                    var bonanzaTrades = responseReader.ReadInt32();

                    //product name
                    var price = ReadDecimal(responseReader) ?? 0.00m;

                    var inventoryTransactionId = responseReader.ReadInt32();

                    var isBarcodedPaper = responseReader.ReadBoolean();

                    //skip count
                    var skipCount = responseReader.ReadUInt16();
                    var skipList = new List<int>();
                    for (int index = 0; index < skipCount; index++)
                    {
                        //skip number
                        skipList.Add(responseReader.ReadInt32());
                    }

                    //packs that were not scanned
                    var unscannedCount = responseReader.ReadUInt16();
                    var unscannedList = new List<int>();
                    for (int index = 0; index < unscannedCount; index++)
                    {
                        //skip number
                        unscannedList.Add(responseReader.ReadInt32());
                    }

                    //US4955
                    //damaged
                    var damagedCount = responseReader.ReadUInt16();
                    var damagedList = new List<PaperUsageDamagedItem>();
                    var canBeRemovedDamagedList = new List<int>();
                    for (int index = 0; index < damagedCount; index++)
                    {
                        var audit = responseReader.ReadInt32();
                        //is exchange
                        var canBeRemoved = responseReader.ReadBoolean();
                        var comments = ReadString(responseReader); //DE13701
                        var dateTimeStamp = ReadString(responseReader); //DE13701

                        //damaged number
                        damagedList.Add(new PaperUsageDamagedItem
                        {
                            AuditNumber = audit,
                            Comment = comments,
                            DateTimeStamp = dateTimeStamp
                        });

                        if (canBeRemoved)
                        {
                            canBeRemovedDamagedList.Add(audit);
                        }
                    }

                    //add all unscanned packs to the "CanBeRemovedList"
                    //this allows the user to remove if they damaged an unscanned pack #
                    canBeRemovedDamagedList.AddRange(unscannedList);

                    //create paper usage item
                    var paperUsageItem = new PaperUsageItem
                    {
                        PaperUsageId = paperUsageId,
                        InventoryItemId = inventoryItemId,
                        InventoryTransactionId = inventoryTransactionId,
                        Name = name,
                        Serial = serialNumber,
                        AuditStart = startAuditNumber,
                        AuditEnd = endAuditNumber,
                        DamagedList = damagedList,
                        CanBeRemovedDamagedList = canBeRemovedDamagedList,
                        BonazaTrades = bonanzaTrades,
                        Price = price,
                        SkipList = skipList,
                        UnscannedList = unscannedList,
                        IsBarcodedPaper = isBarcodedPaper
                    };
                    
                    //add
                    PaperUsageItems.Add(paperUsageItem);
                }
            }
            catch(EndOfStreamException e)
            {
                throw new MessageWrongSizeException(m_strMessageName, e);
            }
            catch(Exception e)
            {
                throw new ServerException(m_strMessageName, e);
            }

            //Close the streams.
            responseReader.Close();
        }

        /// <summary>
        /// Gets the machine audit numbers.
        /// </summary>
        /// <returns></returns>
        public static GetMachinePaperUsageMessage GetMachineAuditNumbers(bool reOpen)
        {
            var message = new GetMachinePaperUsageMessage(reOpen);

            message.Send();

            return message;
        }

        #endregion

        #region Member Properties

        public List<PaperUsageItem> PaperUsageItems { get; private set; }

        #endregion
    }
}