#region Copyright
// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2016 FortuNet, Inc.
#endregion

//US4955: POS > Paper Usage: Damaged

using System;
using System.Collections.Generic;
using System.Linq;

namespace GTI.Modules.POS.Business
{
    public class PaperUsageItem : IEquatable<PaperUsageItem>, IComparable<PaperUsageItem>
    {
        public int PaperUsageId { get; set; }

        public int InventoryItemId{ get; set; }

        public int InventoryTransactionId { get; set; }
        
        public string Name { get; set; }

        public string Serial { get; set; }

        public int AuditStart { get; set; }

        public int AuditEnd { get; set; }

        public int Quantity 
        {
            get
            {
                //get all skipped from skip list
                var skipped = SkipList.Count(i => i >= AuditStart && i <= AuditEnd);
                var damaged = DamagedList.Count(i => i.AuditNumber >= AuditStart && i.AuditNumber <= AuditEnd);
                //find the difference between audit start and end. 
                //then subtract skipped and dammaged
                return AuditEnd - AuditStart + 1 - skipped - damaged;
            }
        }

        public int Sold { get; set; }

        public decimal Price { get; set; }

        //DE13701
        public List<PaperUsageDamagedItem> DamagedList { get; set; }

        public List<int> CanBeRemovedDamagedList { get; set; } //this is a sub list of damages.

        public int BonazaTrades { get; set; }
        
        public List<int> SkipList { get; set; } 
        
        public List<int> UnscannedList { get; set; }

        public bool IsMarkedForRemoval { get; set; }

        public bool Equals(PaperUsageItem other)
        {
            return other != null && other.PaperUsageId == PaperUsageId;
        }

        public int CompareTo(PaperUsageItem other)
        {
            var result = string.Compare(Name, other.Name, StringComparison.Ordinal);

            if (result == 0)
            {
                result = string.Compare(Serial, other.Serial, StringComparison.Ordinal);
                if (result == 0)
                {
                    if (AuditStart > other.AuditStart)
                        result = 1;
                    if (AuditStart < other.AuditStart)
                        result = - 1;
                }
            }

            return result;
        }

        public void CopyValues(PaperUsageItem item)
        {
            PaperUsageId = item.PaperUsageId;
            InventoryItemId = item.InventoryItemId;
            InventoryTransactionId = item.InventoryTransactionId;
            Name = item.Name;
            Serial = item.Serial;
            AuditStart = item.AuditStart;
            AuditEnd = item.AuditEnd;
            Sold = item.Sold;
            Price = item.Price;
            DamagedList = item.DamagedList;
            CanBeRemovedDamagedList = item.CanBeRemovedDamagedList;
            BonazaTrades = item.BonazaTrades;
            SkipList = item.SkipList;
            UnscannedList = item.UnscannedList;
            IsMarkedForRemoval = item.IsMarkedForRemoval;
            IsBarcodedPaper = item.IsBarcodedPaper;
        }

        public bool IsBarcodedPaper { get; set; }
    }

    //DE13701
    //An exception item will always be tied to a paper usage item
    public class PaperUsageDamagedItem : IEquatable<PaperUsageDamagedItem>
    {
        public int AuditNumber { get; set; }

        public string Comment { get; set; }

        public string DateTimeStamp { get; set; }

        public bool Equals(PaperUsageDamagedItem other)
        {
            return other != null &&
                AuditNumber == other.AuditNumber &&
                Comment == other.Comment &&
                DateTimeStamp == other.DateTimeStamp;
        }

        public override string ToString()
        {
            return AuditNumber.ToString();
        }
    }


}
