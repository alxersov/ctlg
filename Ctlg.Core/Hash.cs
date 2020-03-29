using System;
using System.Linq;
using Ctlg.Service.Utils;

namespace Ctlg.Core
{
    public class Hash: IEquatable<Hash>
    {
        public Hash(int hashAlgorithmId, byte[] value)
        {
            HashAlgorithmId = hashAlgorithmId;
            Value = value;
        }

        public Hash(int hashAlgorithmId, uint value)
        {
            HashAlgorithmId = hashAlgorithmId;
            Value = BitConverter.GetBytes(value).Reverse().ToArray();
        }

        protected Hash()
        {
        }

        public int HashId { get; protected set; }
        public int HashAlgorithmId { get; protected set; }
        public HashAlgorithm HashAlgorithm { get; protected set; }
        public byte[] Value { get; protected set; }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = HashId;

                foreach (var b in Value)
                {
                    hash = hash*23 + b;
                }

                return hash;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            var other = obj as Hash;

            return Equals(other);
        }

        public virtual bool Equals(Hash other)
        {
            if ((object)other == null)
            {
                return false;
            }

            if (HashAlgorithmId != other.HashAlgorithmId)
            {
                return false;
            }

            if (Value.Length != other.Value.Length)
            {
                return false;
            }

            for (var i = 0; i < Value.Length; ++i)
            {
                if (Value[i] != other.Value[i])
                {
                    return false;
                }
            }

            return true;
        }

        public static bool operator ==(Hash x, Hash y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (((object)x == null) || ((object)y == null))
            {
                return false;
            }

            return x.Equals(y);
        }

        public static bool operator !=(Hash x, Hash y)
        {
            return !(x == y);
        }

        public override string ToString()
        {
            return FormatBytes.ToHexString(Value);
        }
    }
}
