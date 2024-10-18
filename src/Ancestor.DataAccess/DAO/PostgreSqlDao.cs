using Ancestor.Core;
using Ancestor.DataAccess.DBAction;
using Ancestor.DataAccess.Factory;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Ancestor.DataAccess.DAO
{
    public class PostgreSqlDao : DataAccessObjectBase
    {
        public PostgreSqlDao(DAOFactoryEx factory) : base(factory)
        {
        }

        public override string ParameterSymbol { get { return "@"; } }
        public override string ConnectorSymbol { get { return "||"; } }
        public override string DateTimeSymbol { get { return "NOW()"; } }

        protected override IDbAction CreateDbAction(DBObject dbObject)
        {
            return new PostgreSqlAction(this);
        }

        protected override IDbAction CreateDbAction(string connStr)
        {
            return new PostgreSqlAction(this);
        }

        protected override IDbAction CreateDbAction(IDbConnection conn)
        {
            return new PostgreSqlAction(this);
        }

        protected override ExpressionResolver CreateExpressionResolver(ReferenceInfo reference, ExpressionResolver.ExpressionResolveOption option)
        {
            return new PostgreSqlExpressionResolver(this, reference, option);
        }

        protected override string GetSequenceCommand(string name, bool moveToNext)
        {
            throw new NotImplementedException();
        }

        private class PostgreSqlExpressionResolver : ExpressionResolver
        {
            public PostgreSqlExpressionResolver(DataAccessObjectBase dao, ReferenceInfo reference, ExpressionResolveOption option) : base(dao, reference, option)
            {
            }

            protected override ExpressionResolver CreateInstance(DataAccessObjectBase dao, ReferenceInfo reference, ExpressionResolveOption option)
            {
                return new PostgreSqlExpressionResolver((PostgreSqlDao)dao, reference, option);
            }

            protected override void ProcessConvertToDateTime(Type fromType, Expression objectNode, ReadOnlyCollection<Expression> args)
            {
                if(fromType == typeof(string))
                {
                    Write("To_Timestamp(");
                    Visit(objectNode);
                    Write(")");
                }
            }

            protected override void ProcessConvertToDecimal(Type fromType, Type toType, Expression objectNode, ReadOnlyCollection<Expression> args)
            {
                if(fromType == typeof(string))
                {
                    Write("Cast(");
                    Visit(objectNode);
                    string type = null;
                    switch (Type.GetTypeCode(toType))
                    {
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.UInt16:
                        case TypeCode.UInt32:
                        case TypeCode.UInt64:
                        case TypeCode.Decimal:
                            type = "Integer";
                            break;
                        case TypeCode.Single:
                        case TypeCode.Double:
                        default:
                            type = "Numeric";
                            break;
                    }
                    Write("as");
                    Write(type);
                    Write(")");
                }
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
            private static string ConvertFromDateFormat(string format)
            {
                // TODO: Convert c# DateTime.ToString format to Oracle.To_Char format
                format = format.Replace("HH", "HH24");
                format = format.Replace("mm", "MI");
                format = format.ToUpper();
                return format;
            }
            protected override void ProcessJoinMethodCall(Expression left, Expression right, SqlStatement.Joins joins)
            {
                if(joins != SqlStatement.Joins.Inner)
                    throw new NotImplementedException("postgresql unsupported except inner join");
                Visit(left);
                Write("=");
                Visit(right);
            }

            protected override void ProcessTruncateMethodCall(Expression nodeObject)
            {
                throw new NotImplementedException();
            }
        }
    }
}
