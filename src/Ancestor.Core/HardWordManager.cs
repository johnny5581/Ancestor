using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Ancestor.Core
{
    public static class HardWordManager
    {
        private static Func<PropertyInfo, HardWordAttribute> _hardwordResolver;
        private static readonly ConcurrentDictionary<PropertyInfo, HardWordAttribute> AttributeCaches
            = new ConcurrentDictionary<PropertyInfo, HardWordAttribute>();
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> PropertyCaches
            = new ConcurrentDictionary<Type, PropertyInfo[]>();

        public static Func<PropertyInfo, HardWordAttribute> HardWordResolver
        {
            get { return _hardwordResolver ?? GetHardWordAttribute; }
            set { _hardwordResolver = value; }
        }
        public static HardWordAttribute Get(PropertyInfo property)
        {
            return HardWordResolver(property);
        }
        public static IDictionary<PropertyInfo, HardWordAttribute> Get(Type type)
        {
            PropertyInfo[] properties;
            if (!PropertyCaches.TryGetValue(type, out properties))
            {
                properties = type.GetProperties().Where(p=>TableManager.GetBrowsable(p)).ToArray();
                PropertyCaches.AddOrUpdate(type, properties, (k, v) => properties);
            }
            return properties.Aggregate(new Dictionary<PropertyInfo, HardWordAttribute>(), (seed, p) =>
            {
                var attr = Get(p);
                if(attr != null)
                    seed.Add(p, attr);
                return seed;
            });
        }
        public static HardWordAttribute GetHardWordAttribute(PropertyInfo property)
        {
            HardWordAttribute attr;
            if (!AttributeCaches.TryGetValue(property, out attr))
            {
                attr = property.GetCustomAttributes(typeof(HardWordAttribute), false).FirstOrDefault() as HardWordAttribute;
                AttributeCaches.AddOrUpdate(property, attr, (k, v) => attr);
            }
            return attr;
        }
    }
}
