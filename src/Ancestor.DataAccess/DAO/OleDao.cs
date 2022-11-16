using Ancestor.Core;
using Ancestor.DataAccess.DAO;
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
    public class OleDao : DataAccessObjectBase
    {
        public OleDao(DAOFactoryEx factory) : base(factory)
        {
        }

        public override string ParameterSymbol { get { return "?"; } }
        public override string ConnectorSymbol { get { return "+"; } }
        public override string DateTimeSymbol { get { return "Now()"; } }

        protected override IDbAction CreateDbAction(DBObject dbObject)
        {
            return new OleAction(this);
        }

        protected override IDbAction CreateDbAction(string connStr)
        {
            return new OleAction(this);
        }

        protected override IDbAction CreateDbAction(IDbConnection conn)
        {
            return new OleAction(this);
        }
        protected override bool UseHardword()
        {
            return false;
        }
        protected override ParameterInfo CreateParameter(object value, string parameterNameSeed, bool symbol, string prefix = null, string postfix = null, HardWordAttribute hardWord = null)
        {
            var p = base.CreateParameter(value, parameterNameSeed, symbol, prefix, postfix, hardWord);
            // fix oledb use ? for parameter name, and rollback for hardword
            if(!p.IsSysDateConverted)            
                p.ValueName = "?";                            
            return p;
        }

        protected override ExpressionResolver CreateExpressionResolver(ReferenceInfo reference, ExpressionResolver.ExpressionResolveOption option)
        {
            throw new NotImplementedException();
        }

        protected override string GetSequenceCommand(string name, bool moveToNext)
        {
            throw new NotImplementedException();
        }

        private class OleExpressionResolver : ExpressionResolver
        {
            public OleExpressionResolver(DataAccessObjectBase dao, ReferenceInfo reference, ExpressionResolveOption option) : base(dao, reference, option)
            {
            }

            protected override ExpressionResolver CreateInstance(DataAccessObjectBase dao, ReferenceInfo reference, ExpressionResolveOption option)
            {
                return new OleExpressionResolver((OleDao)dao, reference, option);
            }

            protected override void ProcessConvertToDateTime(Type fromType, Expression objectNode, ReadOnlyCollection<Expression> args)
            {
                
            }

            protected override void ProcessConvertToDecimal(Type fromType, Type toType, Expression objectNode, ReadOnlyCollection<Expression> args)
            {
                
            }

            protected override void ProcessConvertToString(Type fromType, Expression objectNode, ReadOnlyCollection<Expression> args, bool useFmtConvert)
            {
                
            }

            protected override void ProcessJoinMethodCall(Expression left, Expression right, SqlStatement.Joins joins)
            {
                
            }

            protected override void ProcessTruncateMethodCall(Expression nodeObject)
            {
                
            }
        }
    }
}
