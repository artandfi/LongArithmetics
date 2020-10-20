/*
    (C) 2020 artandfi 
*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace LongArithmetics {
    public class LongNumber {
        #region Fields
        const int BASE = 10;
        bool Sign { get; set; }
        List<int> Digits { get; set; } = new List<int>();   // Reverse order!
        #endregion

        #region Constructors
        public LongNumber() { }

        public LongNumber(string number) {
            Sign = number[0] == '-';

            for (int i = number.Length - 1; i > number.IndexOf('-'); i--)
                Digits.Add(number[i] - '0');
        }

        public LongNumber(int number) {
            Sign = number < 0;
            number = Math.Abs(number);

            do {
                Digits.Add(number % BASE);
                number /= BASE;
            } while (number > 0);
        }
        #endregion

        #region Comparison operators
        public static bool operator ==(LongNumber a, LongNumber b) => a.Sign == b.Sign && a.Digits.SequenceEqual(b.Digits);

        public static bool operator !=(LongNumber a, LongNumber b) => !(a == b);

        public static bool operator <(LongNumber a, LongNumber b) {
            if (a.Sign != b.Sign)
                return a.Sign;

            if (a.Digits.Count != b.Digits.Count)
                return a.Sign ? a.Digits.Count > b.Digits.Count : a.Digits.Count < b.Digits.Count;

            for (int i = a.Digits.Count; i >= 0; i--)
                if (a[i] != b[i])
                    return a.Sign ? a[i] > b[i] : a[i] < b[i];

            return false;
        }

        public static bool operator <=(LongNumber a, LongNumber b) => a == b || a < b;
        
        public static bool operator >(LongNumber a, LongNumber b) => !(a <= b);

        public static bool operator >=(LongNumber a, LongNumber b) => !(a < b);

        #endregion

        #region Unary operators
        public static LongNumber operator +(LongNumber a) => new LongNumber { Digits = a.Digits, Sign = a.Sign };

        public static LongNumber operator -(LongNumber a) => new LongNumber { Digits = a.Digits, Sign = !a.Sign };

        public static LongNumber operator ++(LongNumber a) => a + 1;

        public static LongNumber operator --(LongNumber a) => a - 1;
        #endregion

        #region Binary operators
        public static LongNumber operator +(LongNumber a, LongNumber b) {
            var res = new LongNumber();

            if (a.Sign == b.Sign) {
                AddSameSigned(a, b, ref res);
            }
            else {
                res = a.Sign ? b - (-a) : a - (-b);
            }

            return res;
        }

        public static LongNumber operator -(LongNumber a, LongNumber b) {
            if (a == b) return 0;

            var res = new LongNumber();

            if (a.Sign == b.Sign) {
                SubSameSigned(a, b, ref res);
            }
            else {
                res = a.Sign ? -(-a + b) : a + (-b);
            }

            return res;
        }

        public static LongNumber operator *(LongNumber a, LongNumber b) {
            if (a == 0 || b == 0)
                return 0;

            if (a.Digits.Count < b.Digits.Count)
                Swap(ref a, ref b);

            var blocks = FormMulBlocks(a, b);

            return AddMulBlocks(blocks, a, b);
        }

        public static LongNumber operator /(LongNumber a, LongNumber b) {
            if (b == 0)
                throw new Exception();

            if (a == 0)
                return 0;

            var subA = new LongNumber() { Sign = false };
            var res = ColumnDivide(a, b, ref subA);

            if (a.Sign && b.Sign && subA != 0)
                res++;

            if (a.Sign && !b.Sign && subA != 0)
                res--;

            return res;
        }

        public static LongNumber operator %(LongNumber a, LongNumber b) => a - b * (a / b);
        #endregion

        #region Mathematical functions

        public static LongNumber Divide(LongNumber a, LongNumber b, out LongNumber remainder) {
            remainder = a % b;
            return a / b;
        }

        public static LongNumber Abs(LongNumber a) {
            return new LongNumber() { Digits = a.Digits, Sign = false };
        }

        public static LongNumber Pow(LongNumber a, LongNumber n) {
            if (n < 0) throw new Exception();
            if (n == 0) return 1;
            if (n == 1 || a == 0) return a;

            var res = new LongNumber(1);

            while (n > 0) {
                if (n % 2 == 1)
                    res *= a;
                a *= a;
                n /= 2;
            }

            return res;
        }

        public static LongNumber AddMod(LongNumber a, LongNumber b, LongNumber m) => (a + b) % m;

        public static LongNumber SubMod(LongNumber a, LongNumber b, LongNumber m) => (a - b) % m;

        public static LongNumber MulMod(LongNumber a, LongNumber b, LongNumber m) => (a * b) % m;

        public static LongNumber DivMod(LongNumber a, LongNumber b, LongNumber m) => (a / b) % m;

        public static LongNumber ModMod(LongNumber a, LongNumber b, LongNumber m) => (a % b) % m;

        public static LongNumber PowMod(LongNumber a, LongNumber b, LongNumber m) => Pow(a, b) % m;
        #endregion

        #region Utility methods

        public int this[int i] {
            get => i < Digits.Count ? Digits[i] : 0;
            set => Digits[i] = value;
        }

        public override string ToString() {
            string result = Sign ? "-" : "";
            for (int i = Digits.Count - 1; i >= 0; i--)
                result += Digits[i];
            return result;
        }

        public static void Swap(ref LongNumber a, ref LongNumber b) {
            var tmp = a;
            a = b;
            b = tmp;
        }

        private void ClearZeros() {
            int c = Digits.Count - 1;
            while (c > 0 && Digits[c] == 0)
                Digits.RemoveAt(c--);
        }

        public static implicit operator LongNumber(int n) => new LongNumber(n.ToString());

        public static implicit operator int(LongNumber n) => Int32.Parse(n.ToString());
        #endregion

        #region Inner methods

        private static void AddSameSigned(LongNumber a, LongNumber b, ref LongNumber res) {
            int extra = 0;
            res.Sign = a.Sign;

            for (int i = 0; i < Math.Max(a.Digits.Count, b.Digits.Count); i++) {
                int dSum = a[i] + b[i] + extra;
                res.Digits.Add(dSum % BASE);
                extra = dSum / BASE;
            }

            if (extra == 1) res.Digits.Add(1);
        }

        private static void SubSameSigned(LongNumber a, LongNumber b, ref LongNumber res) {
            int extra = 0;
            int dDif;

            if (a < b) {
                for (int i = 0; i < Math.Max(a.Digits.Count, b.Digits.Count); i++) {
                    dDif = a.Sign ? a[i] - b[i] - extra : b[i] - a[i] - extra;

                    if (dDif < 0) {
                        dDif += BASE;
                        extra = 1;
                    }
                    else
                        extra = 0;

                    res.Digits.Add(dDif);
                }
                res.Sign = true;
            }
            else
                res = -(b - a);

            res.ClearZeros();
        }

        private static LongNumber[] FormMulBlocks(LongNumber a, LongNumber b) {
            var blocks = new LongNumber[b.Digits.Count];

            for (int i = 0; i < blocks.Length; i++) {
                int pCarry = 0;
                blocks[i] = new LongNumber();
                
                for (int j = 0; j < i; j++)
                    blocks[i].Digits.Add(0);
                
                for (int j = 0; j < a.Digits.Count; j++) {
                    int dProd = a[j] * b[i] + pCarry;
                    blocks[i].Digits.Add(dProd % BASE);
                    pCarry = dProd / BASE;
                }

                if (pCarry > 0)
                    blocks[i].Digits.Add(pCarry);
            }

            return blocks;
        }

        private static LongNumber AddMulBlocks(LongNumber[] blocks, LongNumber a, LongNumber b) {
            var res = new LongNumber() { Sign = a.Sign != b.Sign };
            int sCarry = 0;

            for (int i = 0; i < blocks[blocks.Length - 1].Digits.Count; i++) {
                int sum = sCarry;

                for (int j = 0; j < blocks.Length; j++)
                    sum += blocks[j][i];
                
                res.Digits.Add(sum % BASE);
                sCarry = sum / BASE;
            }

            if (sCarry > 0) {
                while (sCarry > 0) {
                    res.Digits.Add(sCarry % BASE);
                    sCarry /= BASE;
                }
            }

            return res;
        }

        private static LongNumber ColumnDivide(LongNumber a, LongNumber b, ref LongNumber subA) {
            var res = new LongNumber() { Sign = a.Sign != b.Sign };
            var absB = Abs(b);
            var i = a.Digits.Count - 1;
            var firstStep = true;

            while (i >= 0) {
                int added = 0;

                do {
                    subA.Digits.Insert(0, a[i]);
                    i--;
                    added++;

                    a.ClearZeros();

                    if (added > 1 && !firstStep)
                        res.Digits.Insert(0, 0);
                } while (subA < absB && i >= 0);

                if (firstStep)
                    firstStep = false;

                ModifyMinuend(ref subA, absB, ref res);
            }

            return res;
        }

        private static void ModifyMinuend(ref LongNumber subA, LongNumber absB, ref LongNumber res) {
            var quot = NaiveDiv(subA, absB);
            res.Digits.Insert(0, quot);
            subA -= absB * quot;

            if (subA == 0)
                subA.Digits.Remove(0);
        }

        public static LongNumber NaiveDiv(LongNumber a, LongNumber b) {
            var res = new LongNumber(0);

            while (a >= b) {
                a -= b;
                res++;
            }
            return res;
        }
        #endregion
    }
}
