using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GTI.Modules.POS.UI.PaperRangeScanner
{
    public class BaseConverter
    {
        #region Normal Base Methods

        /// <summary>
        /// The list of symbols to be used for the "normal" lookup (36 characters)
        /// </summary>
        public static string SYMBOL_STRING = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        /// <summary>
        /// Converts one base value to another value in a different base
        /// </summary>
        /// <param name="value"></param>
        /// <param name="valueBase"></param>
        /// <param name="returnBase"></param>
        /// <returns></returns>
        public static string BaseConvert(string value, UInt32 valueBase, UInt32 returnBase)
        {
            if (returnBase < 2 || returnBase >= SYMBOL_STRING.Length)
                throw new ArgumentException("The Base must be >= 2 and < " + SYMBOL_STRING.Length.ToString());
            else if (valueBase < 2 || valueBase >= SYMBOL_STRING.Length)
                throw new ArgumentException("The Base must be >= 2 and < " + SYMBOL_STRING.Length.ToString());

            return BaseConvertFromDec(BaseConvertToDec(value, valueBase), returnBase);
        }

        /// <summary>
        /// Converts the sent in base to decimal
        /// </summary>
        /// <param name="value"></param>
        /// <param name="valueBase"></param>
        /// <returns></returns>
        public static ulong BaseConvertToDec(string value, UInt32 valueBase)
        {
            if (valueBase == 10)
                return Convert.ToUInt64(value);

            ulong retValue = 0;
            foreach (char d in value.ToUpperInvariant())
            {
                retValue *= valueBase;
                int idx = Array.IndexOf(SYMBOL_STRING.ToArray(), d);

                if (idx == -1)
                    throw new Exception("Provided number contains invalid characters");

                retValue += (ulong)idx;
            }
            return retValue;
        }

        /// <summary>
        /// Converts the sent in base 10 value to the sent in base
        /// </summary>
        /// <param name="value"></param>
        /// <param name="returnBase"></param>
        /// <returns></returns>
        public static string BaseConvertFromDec(ulong value, UInt32 returnBase)
        {
            if (returnBase == 10)
                return value.ToString();

            string retValue = "";
            ulong temp = value;
            while (temp > 0)
            {
                int remainder = (int)(temp % returnBase);
                retValue = SYMBOL_STRING[remainder] + retValue;// Array.IndexOf(SYMBOL_STRING.ToArray(), remainder);
                temp /= returnBase;
            }
            return retValue;
        }

        /// <summary>
        /// Same as the BaseConvertToDec except it doesn't throw any exceptions and returns false if it fails
        /// </summary>
        /// <param name="value"></param>
        /// <param name="valueBase"></param>
        /// <param name="retValue"></param>
        /// <returns></returns>
        public static bool TryParseBaseConvertToDec(string value, UInt32 valueBase, out ulong retValue)
        {
            retValue = 0;
            if (valueBase == 10)
            {
                retValue = Convert.ToUInt64(value);
                return true;
            }

            if (valueBase > SYMBOL_STRING.Length)
                return false;

            foreach (char d in value.ToUpperInvariant())
            {
                retValue *= valueBase;
                int idx = Array.IndexOf(SYMBOL_STRING.ToArray(), d);

                if (idx == -1)
                    return false;

                retValue += (ulong)idx;
            }
            return true;
        }

        #endregion

        #region Assignable Symbol base

        /// <summary>
        /// Converts one base value to another value in a different base
        /// </summary>
        /// <param name="value">the value (using valueBaseString) to convert into returnBaseString</param>
        /// <param name="valueBaseString">the symbol characters used for the first base (in order)</param>
        /// <param name="returnBaseString">the symbol characters used for the second base (in order)</param>
        /// <returns>the string representation of the value using the second base</returns>
        public static string BaseConvert(string value, string valueBaseString, string returnBaseString)
        {
            ulong decVal = 0;
            if (TryParseBaseConvertToDec(value, valueBaseString, out decVal))
                return BaseConvertFromDec(decVal, returnBaseString);
            else
                return null;
        }

        /// <summary>
        /// Converts any base value into a base 10 value
        /// </summary>
        /// <param name="value">the value to convert</param>
        /// <param name="symbolString">the list of symbols to use for the lookup when converting</param>
        /// <param name="retValue">the base 10 value of the sent in value</param>
        /// <returns>whether or not the conversion was successful</returns>
        public static bool TryParseBaseConvertToDec(string value, string symbolString, out ulong retValue)
        {
            retValue = 0;

            foreach (char d in value)
            {
                retValue *= (ulong)symbolString.Length;
                int idx = Array.IndexOf(symbolString.ToArray(), d);

                if (idx == -1)
                    return false;

                retValue += (ulong)idx;
            }
            return true;
        }

        /// <summary>
        /// Converts the sent in base 10 value to the sent in base
        /// </summary>
        /// <param name="value">the base value to convert to the sent in base</param>
        /// <param name="symbolString">the symbol characters for the base to convert to</param>
        /// <returns></returns>
        public static string BaseConvertFromDec(ulong value, string symbolString)
        {
            string retValue = "";
            ulong temp = value;
            while (temp > 0)
            {
                int remainder = (int)(temp % (ulong)symbolString.Length);
                retValue = SYMBOL_STRING[remainder] + retValue;
                temp /= (ulong)symbolString.Length;
            }
            return retValue;
        }

        #endregion
    }
}
