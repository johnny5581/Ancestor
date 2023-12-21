using Ancestor.Core;
using Ancestor.DataAccess.DBAction;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Ancestor.DataAccess.DAO
{
    public class OracleDao : DataAccessObjectBase
    {
        public OracleDao(Factory.DAOFactoryEx factory) : base(factory)
        {
        }

        public override string ParameterSymbol
        {
            get { return ":"; }
        }
        public override string ConnectorSymbol
        {
            get { return "||"; }
        }
        public override string DateTimeSymbol
        {
            get { return "Sysdate"; }
        }


        protected override IDbAction CreateDbAction(DBObject dbObject)
        {
            switch (dbObject.DataBaseType)
            {
                case DBObject.DataBase.Oracle:
                    return new OracleAction(this);
                case DBObject.DataBase.ManagedOracle:
                    return new ManagedOracleAction(this);                
                default:
                    return null;
            }
        }
        protected override IDbAction CreateDbAction(string connStr)
        {
            if(Factory.Arguments != null && "managed".Equals(Factory.Arguments.ElementAtOrDefault(0), StringComparison.OrdinalIgnoreCase))
                return new ManagedOracleAction(this);
            else
                return new OracleAction(this);
        }
        protected override IDbAction CreateDbAction(IDbConnection conn)
        {
            if (Factory.Arguments != null && "managed".Equals(Factory.Arguments.ElementAtOrDefault(0), StringComparison.OrdinalIgnoreCase))
                return new ManagedOracleAction(this);
            else
                return new OracleAction(this);
        }
        public override string ConvertFromHardWord(string name, HardWordAttribute attribute)
        {
            return string.Format("RawToHex({0})", name);
        }        
        public override string ConvertToHardWord(string name, HardWordAttribute attribute)
        {
            if (attribute.IgnorePrefix)
                return base.ConvertToHardWord(name, attribute);
            return string.Format("UTL_RAW.Cast_To_VARCHAR2({0})", name);

        }
        //public override string GetServerTime()
        //{
        //    return "SYSDATE";
        //}

        protected override ExpressionResolver CreateExpressionResolver(ReferenceInfo reference, ExpressionResolver.ExpressionResolveOption option)
        {
            return new OracleExpressionResolver(this, reference, option);
        }

        protected override string GetSequenceCommand(string name, bool moveToNext)
        {
            var obj = string.Format("{0}.{1}", name, moveToNext ? "NEXTVAL" : "CURRVAL");
            return string.Format("Select {0} From {1}", obj, DummyTable);
        }

        private class OracleExpressionResolver : ExpressionResolver
        {
            public OracleExpressionResolver(OracleDao dao, ReferenceInfo reference, ExpressionResolveOption option) : base(dao, reference, option)
            {
            }

            protected override ExpressionResolver CreateInstance(DataAccessObjectBase dao, ReferenceInfo reference, ExpressionResolveOption option)
            {
                return new OracleExpressionResolver((OracleDao)dao, reference, option);
            }
            protected override Expression VisitBinary(BinaryExpression node)
            {
                if (node.Left.NodeType == ExpressionType.Call)
                {
                    var methodNode = node.Left as MethodCallExpression;
                    var method = methodNode.Method;
                    Expression leftNode = null, rightNode = null;
                    var compareFlag = false;
                    string @operator = null;
                    if (method.DeclaringType == typeof(string) && method.Name == "CompareTo")
                    {
                        leftNode = methodNode.Object;
                        rightNode = methodNode.Arguments[0];
                        compareFlag = true;
                    }
                    else if (method.IsStatic && method.DeclaringType == typeof(string) && method.Name == "Compare")
                    {
                        leftNode = methodNode.Arguments[0];
                        rightNode = methodNode.Arguments[1];
                        compareFlag = true;
                    }

                    if (compareFlag)
                    {
                        switch (node.NodeType)
                        {
                            case ExpressionType.Equal:
                                @operator = "=";
                                break;
                            case ExpressionType.NotEqual:
                                @operator = "<>";
                                break;
                            case ExpressionType.LessThan:
                                @operator = "<";
                                break;
                            case ExpressionType.LessThanOrEqual:
                                @operator = "<=";
                                break;
                            case ExpressionType.GreaterThan:
                                @operator = ">";
                                break;
                            case ExpressionType.GreaterThanOrEqual:
                                @operator = ">=";
                                break;
                            default:
                                throw new NotSupportedException(string.Format("string comparison symbol '{0}' is not supported", node.NodeType));
                        }
                        ProcessBinaryComparison(leftNode, rightNode, @operator);
                        return node;
                    }
                }
                return base.VisitBinary(node);
            }


            protected override Expression VisitStaticMethodCall(MethodCallExpression node)
            {
                switch (node.Method.Name)
                {
                    case "NotNull":
                        Write("Nvl(");
                        Visit(node.Arguments[0]);
                        Write(",");
                        if (node.Arguments.Count > 1)
                            Visit(node.Arguments[1]);
                        else
                            Write("'!EMPTY!'");
                        Write(")");
                        return node;
                    case "Plus":
                        Visit(node.Arguments[0]);
                        Write("(+)");
                        return node;
                }
                return base.VisitStaticMethodCall(node);
            }
            protected override void ProcessTruncateMethodCall(Expression nodeObject)
            {
                Write("Trunc(");
                Visit(nodeObject);
                Write(")");
            }
            protected override void ProcessJoinMethodCall(Expression left, Expression right, SqlStatement.Joins joins = SqlStatement.Joins.Inner)
            {
                switch (joins)
                {
                    case SqlStatement.Joins.Inner:
                        Visit(left);
                        Write("=");
                        Visit(right);
                        break;
                    case SqlStatement.Joins.Outer:
                        Visit(left);
                        Write("(+)");
                        Write("=");
                        Visit(right);
                        Write("(+)");
                        break;
                    case SqlStatement.Joins.Left:
                        Visit(left);                        
                        Write("=");
                        Visit(right);
                        Write("(+)");
                        break;
                    case SqlStatement.Joins.Right:
                        Visit(left);
                        Write("(+)");
                        Write("=");
                        Visit(right);                        
                        break;
                }
            }

            protected override void ProcessBinaryCoalesce(Expression left, Expression right)
            {
                Write("Nvl(");
                Visit(left);
                Write(",");
                Visit(right);
                Write(")");
            }

            protected override void ProcessMethodCall(Expression objectNode, MethodInfo method, ReadOnlyCollection<Expression> args)
            {
                switch (method.Name)
                {
                    case "ToString":
                        Write("To_Char(");
                        Visit(objectNode);
                        Write(")");
                        return;
                }


                base.ProcessMethodCall(objectNode, method, args);
            }

            protected override void ProcessServerMemberAccess(MemberExpression node)
            {
                switch (node.Member.Name)
                {
                    case "Now":
                        object now;
                        if (TryResolveValue(node, out now))
                            ProcessConstant(now);
                        throw new InvalidOperationException("can not resolve Server.Now");
                    case "SysDate":
                        Write(DataAccessObject.DateTimeSymbol);
                        break;
                }
            }
            protected override void ProcessDateTimeMethodCall(Expression objectNode, MethodInfo method, ReadOnlyCollection<Expression> args)
            {
                switch (method.Name)
                {
                    case "AddYears":
                    case "AddMonths":
                        Write("Add_Months(");
                        Visit(objectNode);
                        Write(", ");
                        Visit(args[0]);
                        if (method.Name == "AddYears")
                            Write(" * 12");
                        Write(")");
                        return;
                    case "AddDays":
                    case "AddHours":
                    case "AddMinutes":
                    case "AddSeconds":
                        Visit(objectNode);
                        Write(" + ");
                        Visit(args[0]);
                        if (method.Name == "AddHours")
                            Write(" / 24");
                        else if (method.Name == "AddMinutes")
                            Write(" / 1440");
                        else if (method.Name == "AddSeconds")
                            Write(" / 86400");
                        return;
                }
                base.ProcessDateTimeMethodCall(objectNode, method, args);
            }

            protected override void ProcessConvertToString(Type fromType, Expression objectNode, ReadOnlyCollection<Expression> args, bool useFmtConvert)
            {
                if (fromType == typeof(DateTime))
                {
                    Write("To_Char(");
                    Visit(objectNode);

                    // convert from 
                    var formatExpression = args.ElementAtOrDefault(0);
                    if (formatExpression != null)
                    {
                        Write(",");

                        object value;
                        if (TryResolveValue(formatExpression, out value) && value is string)
                        {
                            var formattedValue = value as string;
                            if (useFmtConvert)
                                formattedValue = ConvertFromDateFormat(formattedValue);
                            Write("'{0}'", formattedValue);
                        }
                        else
                            Visit(formatExpression);
                    }
                    Write(")");
                }
                else
                {
                    Write("To_Char(");
                    Visit(objectNode);
                    Write(")");
                }
            }
            protected override void ProcessConvertToDateTime(Type fromType, Expression objectNode, ReadOnlyCollection<Expression> args)
            {
                if (fromType == typeof(string))
                {
                    Write("To_Date(");
                    Visit(objectNode);
                    var formatExpression = args.ElementAtOrDefault(0);
                    if (formatExpression != null)
                    {
                        Write(",");
                        object value;
                        if (TryResolveValue(formatExpression, out value) && value is string)
                        {
                            var formattedValue = ConvertFromDateFormat((string)value);
                            Write(formattedValue);
                        }
                        else
                            Visit(formatExpression);
                    }
                    Write(")");
                }
            }
            protected override void ProcessConvertToDecimal(Type fromType, Type toType, Expression objectNode, ReadOnlyCollection<Expression> args)
            {
                if (fromType == typeof(string))
                {
                    Write("To_Number(");
                    Visit(objectNode);
                    var formatExpression = args.ElementAtOrDefault(0);
                    if (formatExpression != null)
                    {
                        Write(",");
                        object value;
                        if (TryResolveValue(formatExpression, out value) && value is string)
                        {
                            var formattedValue = ConvertFromDecimalFormat((string)value);
                            Write(formattedValue);
                        }
                        else
                            Visit(formatExpression);
                    }
                    Write(")");
                }
                else if (InternalHelper.IsDecimalType(InternalHelper.GetUnderlyingType(fromType)))
                {
                    Visit(objectNode);
                }
                else
                {
                    Write("To_Number(");
                    Visit(objectNode);
                    Write(")");
                }
            }


            private static string ConvertFromDateFormat(string format)
            {
                // TODO: Convert c# DateTime.ToString format to Oracle.To_Char format
                format = format.Replace("HH", "HH24");
                format = format.Replace("mm", "MI");
                format = format.ToUpper();
                return format;
            }
            private static string ConvertFromDecimalFormat(string format)
            {
                return format;
            }


        }
    }
}
