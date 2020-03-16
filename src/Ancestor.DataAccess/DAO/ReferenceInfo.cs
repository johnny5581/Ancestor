using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ancestor.DataAccess.DAO
{
    public class ReferenceInfo
    {
        private readonly Dictionary<string, ReferenceStruct> _referenceMap;
        public ReferenceInfo()
        {
            _referenceMap = new Dictionary<string, ReferenceStruct>(StringComparer.OrdinalIgnoreCase);
        }
        public ReferenceInfo(ReferenceInfo other)
        {
            _referenceMap = new Dictionary<string, ReferenceStruct>(other._referenceMap, StringComparer.OrdinalIgnoreCase);
        }
        public void Add(Type sourceType, Type referenceType, string referenceName)
        {
            var sourceKey = GetSourceKey(sourceType);
            _referenceMap.Add(sourceKey, new ReferenceStruct(sourceType, referenceType, referenceName));
        }
        public void Add(ReferenceInfo info)
        {
            foreach (var kv in info._referenceMap)
            {
                if (!_referenceMap.ContainsKey(kv.Key))
                    _referenceMap.Add(kv.Key, kv.Value);
            }
        }
        public IEnumerable<Tuple<Type, Type, string>> GetStructs()
        {
            foreach (var value in _referenceMap.Values)
            {
                yield return Tuple.Create(value.SourceType, value.ReferenceType, value.ReferenceName);
            }
        }
        private static string GetSourceKey(Type sourceType)
        {
            var sourceKey = "";
            if (sourceType != null)
                sourceKey = sourceType.FullName;
            return sourceKey;
        }
        public Type GetReferenceType(Type sourceType = null)
        {
            var sourceKey = GetSourceKey(sourceType);
            ReferenceStruct value;
            if (_referenceMap.TryGetValue(sourceKey, out value))
                return value.ReferenceType;
            return null;
        }
        public string GetReferenceName(Type sourceType = null)
        {
            var sourceKey = GetSourceKey(sourceType);
            ReferenceStruct value;
            if (_referenceMap.TryGetValue(sourceKey, out value))
                return value.ReferenceName;
            return null;
        }
        private struct ReferenceStruct
        {
            public Type SourceType;
            public Type ReferenceType;
            public string ReferenceName;

            public ReferenceStruct(Type src, Type refType, string refName)
            {
                SourceType = src;
                ReferenceType = refType;
                ReferenceName = refName;
            }
        }
    }
}
