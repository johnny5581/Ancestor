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
        private static readonly ConcurrentDictionary<PropertyInfo, HardWordAttribute> AttributeCaches
            = new ConcurrentDictionary<PropertyInfo, HardWordAttribute>();
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> PropertyCaches
            = new ConcurrentDictionary<Type, PropertyInfo[]>();
        public static HardWordAttribute Get(PropertyInfo info)
        {
            HardWordAttribute attr;
            if (!AttributeCaches.TryGetValue(info, out attr))
            {
                attr = info.GetCustomAttributes(typeof(HardWordAttribute), false).FirstOrDefault() as HardWordAttribute;
                AttributeCaches.AddOrUpdate(info, attr, (k, v) => attr);
            }
            return attr;
        }
        public static IDictionary<PropertyInfo, HardWordAttribute> Get(Type type)
        {
            PropertyInfo[] properties;
            var ps = new List<PropertyInfo>();
            if (!PropertyCaches.TryGetValue(type, out properties))
            {
                properties = type.GetProperties().Where(p=>TableManager.GetBrowsable(p)).ToArray();
                foreach (var property in properties)
                {
                    var attr = property.GetCustomAttributes(typeof(HardWordAttribute), false).FirstOrDefault() as HardWordAttribute;
                    AttributeCaches.TryAdd(property, attr);
                    if (attr != null)
                        ps.Add(property);
                }
                properties = ps.ToArray();
                PropertyCaches.AddOrUpdate(type, properties, (k, v) => properties);
            }
            if (properties == null)
                return null;
            return properties.Aggregate(new Dictionary<PropertyInfo, HardWordAttribute>(), (seed, p) =>
            {
                var attr = Get(p);
                seed.Add(p, attr);
                return seed;
            });
        }
    }
}
