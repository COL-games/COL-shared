using System;
using COLShared.Security.Integrity;

namespace COLShared.Security.Economy
{
    [Serializable]
    public struct ObfuscatedInt : IEquatable<ObfuscatedInt>, IComparable<ObfuscatedInt>, IComparable<int>
    {
        private int obfuscatedValue;
        private int xorKey;
        private const int MaxReasonableValue = 100000000; // esempio limite per currency

        public int Value
        {
            get
            {
                int val = obfuscatedValue ^ xorKey;
                if (val < 0 || val > MaxReasonableValue)
                {
                    TamperDetector.Instance?.ReportExcessiveCurrencyDelta();
                }
                return val;
            }
            set
            {
                xorKey = GenerateKey();
                obfuscatedValue = value ^ xorKey;
            }
        }

        public static ObfuscatedInt Zero => new ObfuscatedInt(0);

        public ObfuscatedInt(int value)
        {
            xorKey = GenerateKey();
            obfuscatedValue = value ^ xorKey;
        }

        private static int GenerateKey()
        {
            var rng = new Random(Guid.NewGuid().GetHashCode());
            return rng.Next(int.MinValue, int.MaxValue);
        }

        // Implicit conversions
        public static implicit operator int(ObfuscatedInt o) => o.Value;
        public static implicit operator ObfuscatedInt(int v) => new ObfuscatedInt(v);

        // Arithmetic operators
        public static ObfuscatedInt operator +(ObfuscatedInt a, ObfuscatedInt b) => new ObfuscatedInt(a.Value + b.Value);
        public static ObfuscatedInt operator -(ObfuscatedInt a, ObfuscatedInt b) => new ObfuscatedInt(a.Value - b.Value);
        public static ObfuscatedInt operator *(ObfuscatedInt a, ObfuscatedInt b) => new ObfuscatedInt(a.Value * b.Value);
        public static ObfuscatedInt operator /(ObfuscatedInt a, ObfuscatedInt b) => new ObfuscatedInt(a.Value / b.Value);

        // Comparison operators
        public static bool operator ==(ObfuscatedInt a, ObfuscatedInt b) => a.Value == b.Value;
        public static bool operator !=(ObfuscatedInt a, ObfuscatedInt b) => a.Value != b.Value;
        public static bool operator <(ObfuscatedInt a, ObfuscatedInt b) => a.Value < b.Value;
        public static bool operator >(ObfuscatedInt a, ObfuscatedInt b) => a.Value > b.Value;
        public static bool operator <=(ObfuscatedInt a, ObfuscatedInt b) => a.Value <= b.Value;
        public static bool operator >=(ObfuscatedInt a, ObfuscatedInt b) => a.Value >= b.Value;

        public override string ToString() => Value.ToString();
        public override bool Equals(object obj) => obj is ObfuscatedInt o && this == o;
        public bool Equals(ObfuscatedInt other) => this == other;
        public override int GetHashCode() => Value.GetHashCode();
        public int CompareTo(ObfuscatedInt other) => Value.CompareTo(other.Value);
        public int CompareTo(int other) => Value.CompareTo(other);
    }
}