/*
    (C) 2020 artandfi 
*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace LongArithmetics
{
    public class LongNumber
    {
        const int BASE = 10;
        bool Sign { get; set; }
        List<int> Digits { get; set; } = new List<int>();   // Reverse order!

        public LongNumber() { }

        public LongNumber(string number)
        {
            Sign = number[0] == '-';
            for (int i = number.Length - 1; i > number.IndexOf('-'); i--)
                Digits.Add(number[i] - '0');
        }

        public int this[int i]
        {
            get => i < Digits.Count ? Digits[i] : 0;
            set => Digits[i] = value;
        }

        public override string ToString()
        {
            string result = Sign ? "-" : "";
            for (int i = Digits.Count - 1; i >= 0; i--)
                result += Digits[i];
            return result;
        }

        public static implicit operator LongNumber(int n) => new LongNumber(n.ToString());

        public static bool operator ==(LongNumber a, LongNumber b)
        {
            return a.Sign == b.Sign && a.Digits.SequenceEqual(b.Digits);
        }

        public static bool operator !=(LongNumber a, LongNumber b) => !(a == b);

        public static bool operator <(LongNumber a, LongNumber b)
        {
            if (a.Sign != b.Sign) return a.Sign;

            if (a.Digits.Count != b.Digits.Count)
                return a.Sign ? a.Digits.Count > b.Digits.Count : a.Digits.Count < b.Digits.Count;

            for (int i = a.Digits.Count; i >= 0; i--)
                if (a[i] != b[i])
                    return a.Sign ? a[i] > b[i] : a[i] < b[i];
            
            return false;
        }

        public static bool operator >(LongNumber a, LongNumber b)  => !(a <= b);

        public static bool operator <=(LongNumber a, LongNumber b) => a == b || a < b;

        public static bool operator >=(LongNumber a, LongNumber b) => !(a < b);

        public static LongNumber operator -(LongNumber a)
        {
            return new LongNumber { Digits = a.Digits, Sign = !a.Sign };
        }

        public static LongNumber operator +(LongNumber a, LongNumber b)
        {
            var res = new LongNumber();

            if (a.Sign == b.Sign)
            {
                int extra = 0;
                res.Sign = a.Sign;

                for (int i = 0; i < Math.Max(a.Digits.Count, b.Digits.Count); i++)
                {
                    int dSum = a[i] + b[i] + extra;
                    res.Digits.Add(dSum % BASE);
                    extra = dSum / BASE;
                }

                if (extra == 1) res.Digits.Add(1);
            }
            else res = a.Sign ? b - (-a) : a - (-b);
            return res;
        }

        public static LongNumber operator -(LongNumber a, LongNumber b)
        {
            if (a == b) return 0;

            var res = new LongNumber();
            if (a.Sign == b.Sign)
            {
                int extra = 0;
                int dDif;

                if (a < b)
                {
                    for (int i = 0; i < Math.Max(a.Digits.Count, b.Digits.Count); i++)
                    {
                        dDif = a.Sign ? a[i] - b[i] - extra : b[i] - a[i] - extra;
                        if (dDif < 0)
                        {
                            dDif += BASE;
                            extra = 1;
                        }
                        else extra = 0;
                        res.Digits.Add(dDif);
                    }
                    res.Sign = true;
                }
                else res = -(b - a);

                int j = res.Digits.Count - 1;
                while (res[j] == 0)
                    res.Digits.RemoveAt(j--);
            }
            else res = a.Sign ? -(-a + b) : a + (-b);
            return res;
        }
    }
}
