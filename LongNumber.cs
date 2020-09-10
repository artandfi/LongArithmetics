/*
    (C) 2020 artandfi 
*/

using System;
using System.Collections.Generic;

namespace LongArithmetics
{
    public class LongNumber
    {
        const int BASE = 10;
        bool Negative { get; set; }
        List<int> Digits { get; set; } = new List<int>();   // Reverse order!
        int Length { get => Digits.Count; }

        public LongNumber() { }

        public LongNumber(string number)
        {
            Negative = number[0] == '-' ? true : false;
            for (int i = number.Length - 1; i > number.IndexOf('-'); i--)
                Digits.Add(number[i] - '0');
        }

        public int this[int i]
        {
            get => i < Length ? Digits[i] : 0;
            set => Digits[i] = value;
        }

        public override string ToString()
        {
            string result = Negative ? "-" : "";
            for (int i = Length - 1; i >= 0; i--)
                result += Digits[i];
            return result;
        }

        public static LongNumber operator -(LongNumber a)
        {
            return new LongNumber { Digits = a.Digits, Negative = !a.Negative };
        }

        public static LongNumber operator +(LongNumber a, LongNumber b)
        {
            var res = new LongNumber();

            if (a.Negative == b.Negative)
            {
                int extra = 0;
                res.Negative = a.Negative;

                for (int i = 0; i < Math.Max(a.Length, b.Length); i++)
                {
                    int dSum = a[i] + b[i] + extra;
                    res.Digits.Add(dSum % BASE);
                    extra = dSum / BASE == 0 ? 0 : 1;
                }

                if (extra == 1) res.Digits.Add(1);
            }
            else res = a.Negative ? b - (-a) : a - (-b);

            return res;
        }

        public static LongNumber operator -(LongNumber a, LongNumber b)
        {
            throw new NotImplementedException();    // TODO
        }
    }
}
