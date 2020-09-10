using System;
using System.Collections.Generic;

namespace LongArithmetics
{
    public class LongNumber
    {
        bool Negative { get; }
        List<int> Digits { get; } // Reverse order!

        public LongNumber(string number)
        {
            Digits = new List<int>();
            Negative = number[0] == '-' ? true : false;

            for (int i = number.Length - 1; i > number.IndexOf('-'); i--)
                Digits.Add(number[i] - '0');
        }

        public override string ToString()
        {
            string result = Negative ? "-" : "";
            for (int i = Digits.Count - 1; i >= 0; i--)
                result += Digits[i];
            return result;
        }
    }
}
