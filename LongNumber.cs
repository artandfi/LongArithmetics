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
        public List<int> Digits { get; set; } = new List<int>();   // Reverse order!
        public bool Sign { get; set; }
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
                return null;
            if (a == 0)
                return 0;
            if (b == 1)
                return a;
            if (b == -1)
                return -a;

            var subA = new LongNumber() { Sign = false };
            var res = ColumnDivide(a, b, ref subA);
            var modIs0 = subA != 0 && subA.Digits.Count != 0;

            if (a.Sign && b.Sign && modIs0)
                res++;
            if (a.Sign && !b.Sign && modIs0)
                res--;
            if (res[0] == 0)
                res.Sign = false;

            return res;
        }

        public static LongNumber operator %(LongNumber a, LongNumber b) {
            if (b == 0)
                return null;

            var r = a - b * (a / b);
            return r == b ? new LongNumber(0) : r;
        }
        #endregion

        #region Mathematical functions

        public static LongNumber Abs(LongNumber a) {
            return new LongNumber() { Digits = a.Digits, Sign = false };
        }

        public static LongNumber Pow(LongNumber a, LongNumber n) {
            if (n < 0)
                return a == 0 ? null : new LongNumber(0);
            if (n == 0)
                return 1;
            if (n == 1 || a == 0)
                return a;

            var res = new LongNumber(1);

            while (n > 0) {
                if (n % 2 == 1)
                    res *= a;
                a *= a;
                n /= 2;
            }

            return res;
        }

        public static LongNumber Sqrt(LongNumber a) {
            if (a.Sign)
                return null;
            if (a < 4)
                return a == 0 ? 0 : 1;

            var k = 2 * Sqrt((a - a % 4) / 4);
            return a < Pow((k + 1), 2) ? k : k + 1; 
        }

        public static LongNumber Gcd(LongNumber a, LongNumber b) {
            a = Abs(a);
            b = Abs(b);

            while (b != 0) {
                b = a % (a = b);
            }
            return a;
        }

        public static (LongNumber K1, LongNumber K2) GcdLinearRepresentation(LongNumber a, LongNumber b) {
            if (a == 0)
                return (0, 1);
            if (b == 0)
                return (1, 0);

            var q = a / b;
            var r = a - q * b;

            if (r == 0)
                return (1, 1 - q);

            a = b;
            b = r;
            var u = new LongNumber(1);
            var u1 = new LongNumber(1);
            var u2 = new LongNumber(0);
            var v = -q;
            var v1 = -q;
            var v2 = new LongNumber(1);

            while (a % b != 0) {
                q = a / b;
                r = a - q * b;
                a = b;
                b = r;
                u = -q * u1 + u2;
                v = -q * v1 + v2;
                u2 = u1;
                u1 = u;
                v2 = v1;
                v1 = v;
            }

            return (u, v);
        }

        #region Functions by modulo
        public static LongNumber AddMod(LongNumber a, LongNumber b, LongNumber m) => (a + b) % m;

        public static LongNumber SubMod(LongNumber a, LongNumber b, LongNumber m) => (a - b) % m;

        public static LongNumber MulMod(LongNumber a, LongNumber b, LongNumber m) => (a * b) % m;

        public static LongNumber DivMod(LongNumber a, LongNumber b, LongNumber m) {
            var d = a / b;
            return d is null ? null : (a / b) % m;
        }

        public static LongNumber ModMod(LongNumber a, LongNumber b, LongNumber m) {
            var d = a % b;
            return d is null ? null : (a % b) % m;
        }

        public static LongNumber PowMod(LongNumber a, LongNumber b, LongNumber m) {
            if (b < 0)
                return null;
            if (b == 0)
                return 1;
            if (b == 1 || a == 0)
                return a % m;

            var res = new LongNumber(1);

            while (b > 0) {
                if (b % 2 == 1)
                    res = (res * a) % m;
                a = (a * a) % m;
                b /= 2;
            }

            return res;
        }
        #endregion

        #endregion

        #region Congruences

        public static LongNumber MulInverse(LongNumber a, LongNumber m) => GcdLinearRepresentation(a, m).K1;

        public static (LongNumber r1, LongNumber r2) SolveCongruence(LongNumber a, LongNumber b, LongNumber m) {
            if (m == 0) {
                return (null, null);
            }
            if (b == 0) {
                if (a == 0)
                    return (0, 1);
                return (0, m);
            }
            if (a < 0 || a >= m)
                a %= m;
            if (b < 0 || b >= m)
                b %= m;

            var d = Gcd(a, m);
            if (b % d != 0)
                return (null, null);

            var (k1, k2) = GcdLinearRepresentation(a, m);
            var f = b / d;
            return (k1 * f, m / d);
        }

        public static void NormalizeCongruenceSol(ref (LongNumber r1, LongNumber r2) sol) => sol.r1 %= sol.r2;

        public static (LongNumber r1, LongNumber r2) SolveCongruenceSystem(LongNumber[] a, LongNumber[] b, LongNumber[] m) {
            if (a.Length != b.Length || a.Length != m.Length || b.Length != m.Length) {
                return (null, null);
            }

            var sols = new (LongNumber r1, LongNumber r2)[a.Length];
            for (int i = 0; i < sols.Length; i++) {
                sols[i] = SolveCongruence(a[i], b[i], m[i]);

                if (sols[i].r1 is null && sols[i].r2 is null) {
                    return (null, null);
                }
            }

            var prevSol = sols[0];
            NormalizeCongruenceSol(ref prevSol);
            for (int i = 1; i < sols.Length; i++) {
                var r1 = prevSol.r1;
                var r2 = prevSol.r2; // prev: x = r1 (mod r2)
                var bi = sols[i].r1;
                var mi = sols[i].r2; // current: x = bi (mod mi)
                var sol = SolveCongruence(r2 % mi, (bi - r1) % mi, mi);

                if (sol.r1 is null) {
                    return (null, null);
                }

                NormalizeCongruenceSol(ref sol);
                var mr = r2 * sol.r2;
                prevSol = ((r1 + r2 * sol.r1) % mr, mr);
            }

            return prevSol;
        }

        public static void OutputCongruenceSol(LongNumber r1, LongNumber r2) {
            Console.WriteLine("Solution is x = " + r1 + " + " + r2 + "k");
            Console.WriteLine("Alternate form: x = " + r1 + " (mod " + r2 + ")");
        }

        public static void OutputCongruenceSol(LongNumber r1, LongNumber r2, LongNumber m) {
            OutputCongruenceSol(r1, r2);
            Console.Write("Solutions in Z" + m + ": x = ");
            
            LongNumber x;
            var sols = "";
            var d = m / r2;

            for (var k = new LongNumber(0); k < d; k++) {
                x = (r1 + r2 * k) % m;
                sols += x + ", ";
            }
            sols = sols.Substring(0, sols.Length - 2);
            Console.Write(sols + "\n");
        }

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

        public void ClearZeros() {
            int c = Digits.Count - 1;
            while (c > 0 && Digits[c] == 0)
                Digits.RemoveAt(c--);
        }

        public static LongNumber Rand(LongNumber a, LongNumber b) {
            var rnd = new Random();
            var res = new LongNumber();
            var len = rnd.Next(a.Digits.Count, b.Digits.Count + 1);

            if (len == 1)
                res.Digits.Add(rnd.Next(0, BASE));
            else
                res.Digits.Add(rnd.Next(1, BASE));

            for (int i = 1; i < a.Digits.Count; i++) {
                res.Digits.Add(rnd.Next(a[i], BASE));
            }

            var eq = len == b.Digits.Count;
            for (int i = 0; i < len - a.Digits.Count; i++) {
                var d = eq ? rnd.Next(0, b[a.Digits.Count + i]) : rnd.Next(0, BASE);
                res.Digits.Add(d);
            }

            return res;
        }

        public static implicit operator LongNumber(int n) => new LongNumber(n);

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

                    subA.ClearZeros();

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
