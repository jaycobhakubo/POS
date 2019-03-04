using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GTI.Modules.POS.Business
{
    internal class B3Credit : IEquatable<B3Credit>
    {
        internal B3Credit()
        {
            Name = "B3 Credit";
        }

        public bool Equals(B3Credit other)
        {
            return other != null &&
                    GetType() == other.GetType() &&
                    Name == other.Name &&
                    Amount == other.Amount;
        }

        public decimal Amount { get; set; }

        public string Name { get; set; }


    }
}
