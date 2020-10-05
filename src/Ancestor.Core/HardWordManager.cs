using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
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
        private static readonly Dictionary<PropertyInfo, HardWordAttribute> RegisteredProperties
            = new Dictionary<PropertyInfo, HardWordAttribute>();
        public static Func<PropertyInfo, HardWordAttribute> HardWordResolver
        {
            get { return _hardwordResolver ?? GetHardWordAttribute; }
            set { _hardwordResolver = value; }
        }

        public static HardWordAttribute Get(PropertyInfo property)
        {
            return HardWordResolver(property);
        }
        public static void RegisterProperty(PropertyInfo property, HardWordAttribute attr = null)
        {
            if (!RegisteredProperties.ContainsKey(property))
                RegisteredProperties.Add(property, attr ?? new HardWordAttribute());
        }
        public static void RegisterProperty<T>(Expression<Func<T, object>> propertySelector, HardWordAttribute attr = null)
        {
            var property = GetPropertyInfoFromExpression(propertySelector);
            RegisterProperty(property, attr);
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
            HardWordAttribute attr = null;
            if (property != null && !AttributeCaches.TryGetValue(property, out attr))
            {
                attr = property.GetCustomAttributes(typeof(HardWordAttribute), false).FirstOrDefault() as HardWordAttribute;
                if (attr == null)
                    RegisteredProperties.TryGetValue(property, out attr);
                AttributeCaches.AddOrUpdate(property, attr, (k, v) => attr);
            }
            return attr;
        }
        private static PropertyInfo GetPropertyInfoFromExpression(LambdaExpression lambda)
        {
            var memberExp = ExtractMemberExpression(lambda.Body);
            if (memberExp == null)
                throw new ArgumentException("must be member access expression");
            if (memberExp.Member.DeclaringType == null)
                throw new InvalidOperationException("property does not have declaring type");
            return memberExp.Member.DeclaringType.GetProperty(memberExp.Member.Name);
        }
        private static MemberExpression ExtractMemberExpression(Expression expression)
        {
            if (expression.NodeType == ExpressionType.MemberAccess)
                return ((MemberExpression)expression);
            if (expression.NodeType == ExpressionType.Convert)
            {
                var operand = ((UnaryExpression)expression).Operand;
                return ExtractMemberExpression(operand);
            }
            return null;
        }
    }
}
