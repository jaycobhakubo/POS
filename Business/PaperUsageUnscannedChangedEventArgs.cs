#region Copyright
// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2016 FortuNet, Inc.
#endregion

//US4955: POS > Paper Usage: Damaged

using System;
using GTI.Modules.POS.Business;

namespace GTI.Modules.POS.UI.PaperUsage
{
    public class PaperUsageUnscannedChangedEventArgs : EventArgs
    {
        public PaperUsageUnscannedChangedEventArgs(PaperUsageItem item, int audit, bool isAdd)
        {
            PaperUsageItem = item;
            IsAdd = isAdd;
            Audit = audit;
        }
        public bool IsAdd { get; private set; }

        public int Audit { get; private set; }

        public PaperUsageItem PaperUsageItem { get; private set; }

        public override string ToString()
        {
            return string.Format("{0}   {1}   {2}", PaperUsageItem.Name, PaperUsageItem.Serial, Audit.ToString().PadLeft(5, ' '));
        }
    }
}
