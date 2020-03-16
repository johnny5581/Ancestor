using Ancestor.Core;
using Ancestor.DataAccess.DBAction;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Ancestor.DataAccess.DAO
{
    public class OracleDao : DataAccessObjectBase
    {
        public OracleDao(DBObject dbObject) : base(dbObject)
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

        protected override IDbAction CreateDbAction(DBObject dbObject)
        {
            switch(dbObject.DataBaseType)
            {
                case DBObject.DataBase.Oracle:
                    return new OracleAction(this, dbObject);
                case DBObject.DataBase.ManagedOracle:
                default:
                    return null;
            }            
        }

        protected override DbActionOptions CreateDbOptions(AncestorOptions options, DbActionOptions dbOptions)
        {
            if (options != null)
            {
                var opt = dbOptions as DBAction.Options.OracleOptions;
                if (opt != null)
                {
                    opt.AddRowid = options.HasRowId;
                    opt.BindByName = options.BindByName;
                }
            }
            return dbOptions;
        }
        public override string ConvertFromHardWord(string name, HardWordAttribute attribute)
        {
            return string.Format("RawToHex({0})", name);
        }
        public override string ConvertToHardWord(string name, HardWordAttribute attribute)
        {
            return string.Format("UTL_RAW.Cast_To_VARCHAR2({0})", name);
        }
        public override string GetServerTime()
        {
            return "SYSDATE";
        }

        protected override ExpressionResolver CreateExpressionResolver(ReferenceInfo reference)
        {
            return new OracleExpressionResolver(this, reference);
        }

        private class OracleExpressionResolver : ExpressionResolver
        {
            public OracleExpressionResolver(OracleDao dao, ReferenceInfo reference) : base(dao, reference)
            {
            }

            protected override ExpressionResolver CreateInstance(DataAccessObjectBase dao, ReferenceInfo reference)
            {
                return new OracleExpressionResolver((OracleDao)dao, reference);
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
                        Write(DataAccessObject.GetServerTime());
                        return;
                }
            }

            protected override void ProcessConvertToString(Type fromType, Expression objectNode, ReadOnlyCollection<Expression> args)
            {
                if(fromType == typeof(DateTime))
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
                            var formattedValue = ConvertFromDateFormat((string)value);
                            Write(formattedValue);
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
                if(fromType == typeof(string))
                {
                    Write("To_Date(");
                    Visit(objectNode);
                    var formatExpression = args.ElementAtOrDefault(0);
                    if(formatExpression != null)
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
                else if(InternalHelper.IsDecimalType(InternalHelper.GetUnderlyingType(fromType)))
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
                format = format.Replace("HH", "HH24");
                format = format.Replace("mm", "MI");
                return format;
            }
            private static string ConvertFromDecimalFormat(string format)
            {
                return format;
            }
        }
    }
}
