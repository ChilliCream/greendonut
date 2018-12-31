using System;
using System.Linq;

namespace GreenDonut.Benchmark.Tests
{
    public struct CacheKeyA<TKey>
        : IEquatable<CacheKeyA<TKey>>
    {
        private CacheKeyType _keyType;
        private object _objectKey;
        private TKey _originKey;
        private byte[] _primitiveKey;

        public CacheKeyA(object key)
        {
            _keyType = CacheKeyType.ObjectKey;
            _objectKey = key;
            _originKey = default;
            _primitiveKey = null;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return ReferenceEquals(null, this);
            }

            return Equals((CacheKeyA<TKey>)obj);
        }

        public bool Equals(CacheKeyA<TKey> other)
        {
            if (_keyType == other._keyType)
            {
                if (_keyType == CacheKeyType.PrimitiveKey)
                {
                    return _primitiveKey.SequenceEqual(other._primitiveKey);
                }

                if (_keyType == CacheKeyType.ObjectKey)
                {
                    return _objectKey.Equals(other._objectKey);
                }

                return _originKey.Equals(other._originKey);
            }

            return false;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            switch (_keyType)
            {
                case CacheKeyType.OriginKey:
                    return _originKey.GetHashCode();
                case CacheKeyType.ObjectKey:
                    return _objectKey.GetHashCode();
                case CacheKeyType.PrimitiveKey:
                    return _primitiveKey.GetHashCode();
                default:
                    return base.GetHashCode();
            }
        }

        public static bool operator ==(
            CacheKeyA<TKey> left,
            CacheKeyA<TKey> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(
            CacheKeyA<TKey> left,
            CacheKeyA<TKey> right)
        {
            return !left.Equals(right);
        }

        public static implicit operator CacheKeyA<TKey>(TKey key)
        {
            return new CacheKeyA<TKey>
            {
                _keyType = CacheKeyType.OriginKey,
                _originKey = key
            };
        }

        public static implicit operator CacheKeyA<TKey>(int key)
        {
            var cacheKey = new CacheKeyA<TKey>
            {
                _keyType = CacheKeyType.PrimitiveKey,
                _primitiveKey = BitConverter.GetBytes(key)
            };

            return cacheKey;
        }

        #region CacheKeyType

        private enum CacheKeyType
            : byte
        {
            OriginKey = 0,
            ObjectKey = 1,
            PrimitiveKey = 2
        }

        #endregion
    }
}
