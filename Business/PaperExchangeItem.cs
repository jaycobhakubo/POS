#region Copyright
// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2016 FortuNet, Inc.
#endregion

namespace GTI.Modules.POS.Business
{
    public class PaperExchangeItem
    {
        public string Name { get; set; }

        public string Serial { get; set; }

        public int Audit { get; set; }

        public int Reciept { get; set; }

        public int Machine { get; set; }

        public string Cashier { get; set; }
    }
}
