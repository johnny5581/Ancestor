using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Ancestor.DataAccess.DAO
{
    public class DBNullModel
    {
        private readonly HashSet<string> nulls
            = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public DBNullModel(Type modelType, object model)
        {
            Model = model;
            ModelType = modelType;
        }
        public object Model { get; }
        public Type ModelType { get; }


        public bool IsNull(string propertyName)
        {
            return nulls.Contains(propertyName);
        }

        public DBNullModel SetNull(string propertyName)
        {
            nulls.Add(propertyName);
            return this;
        }

        public DBNullModel UnsetNull(string propertyName)
        {
            nulls.Remove(propertyName);
            return this;
        }
    }
    public class DBNullModel<T> : DBNullModel
        where T : class, new()
    {
        public DBNullModel(T model = null) : base(typeof(T), model ?? new T())
        {
        }

        public DBNullModel<T> SetNull(Expression<Func<T, object>> propertySelector)
        {
            var property = GetPropertyInfoFromExpression(propertySelector);
            if (property == null) throw new InvalidOperationException("invalid resolve property");
            return (DBNullModel<T>)SetNull(property.Name);
        }
        public DBNullModel<T> UnsetNull(Expression<Func<T, object>> propertySelector)
        {
            var property = GetPropertyInfoFromExpression(propertySelector);
            if (property == null) throw new InvalidOperationException("invalid resolve property");
            return (DBNullModel<T>)UnsetNull(property.Name);
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
