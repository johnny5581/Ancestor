using Ancestor.Core;
using Ancestor.DataAccess.DBAction;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Ancestor.DataAccess.DAO
{
    public class SqliteDao : DataAccessObjectBase
    {
        public SqliteDao(Factory.DAOFactoryEx factory) : base(factory)
        {
        }

        public override string ParameterSymbol { get { return "@"; } }
        public override string ConnectorSymbol { get { return "||"; } }
        public override string DateTimeSymbol { get { return "DateTime('now', 'localtime')"; } }

        protected override IDbAction CreateDbAction(DBObject dbObject)
        {
            return new SqliteAction(this);
        }

        protected override IDbAction CreateDbAction(string connStr)
        {
            return new SqliteAction(this);
        }

        protected override IDbAction CreateDbAction(IDbConnection conn)
        {
            return new SqliteAction(this);
        }

        protected override ExpressionResolver CreateExpressionResolver(ReferenceInfo reference, ExpressionResolver.ExpressionResolveOption option)
        {
            return new SqliteExpressionResolver(this, reference, option);
        }

        protected override string GetSequenceCommand(string name, bool moveToNext)
        {
            throw new NotImplementedException();
        }

        private class SqliteExpressionResolver : ExpressionResolver
        {
            public SqliteExpressionResolver(DataAccessObjectBase dao, ReferenceInfo reference, ExpressionResolveOption option) : base(dao, reference, option)
            {
            }

            protected override ExpressionResolver CreateInstance(DataAccessObjectBase dao, ReferenceInfo reference, ExpressionResolveOption option)
            {
                return new SqliteExpressionResolver((SqliteDao)dao, reference, option);
            }

            protected override void ProcessConvertToDateTime(Type fromType, Expression objectNode, ReadOnlyCollection<Expression> args)
            {
                if(fromType == typeof(string))
                {
                    Write("DateTime(");
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
                    switch(Type.GetTypeCode(toType))
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
                if(fromType == typeof(DateTime))
                {
                    Write("Strftime(");

                    var formatExpression = args.ElementAtOrDefault(0);
                    if(formatExpression != null)
                    {
                        object value;
                        if(TryResolveValue(formatExpression, out value) && value is string)
                        {
                            var formattedValue = value as string;
                            if (useFmtConvert)
                                formattedValue = ConvertFromDateFormat(formattedValue);
                            Write("'{0}',", formattedValue);                            
                        }                        
                    }
                    Visit(objectNode);
                    Write(")");
                }
                else
                {
                    Write("Cast(");
                    Visit(objectNode);
                    Write(" As Text)");
                }
            }

            protected override void ProcessJoinMethodCall(Expression left, Expression right, SqlStatement.Joins joins)
            {
                Visit(left);
                Write("=");
                Visit(right);
            }

            protected override void ProcessTruncateMethodCall(Expression nodeObject)
            {
                throw new NotImplementedException();
            }
            private string ConvertFromDateFormat(string formattedValue)
            {
                formattedValue = formattedValue.Replace("yyyy", "%Y");
                formattedValue = formattedValue.Replace("MM", "%m");
                formattedValue = formattedValue.Replace("dd", "%d");
                formattedValue = formattedValue.Replace("HH", "%h");
                formattedValue = formattedValue.Replace("mm", "%M");
                formattedValue = formattedValue.Replace("ss", "%S");
                return formattedValue;
            }
        }
    }
}
