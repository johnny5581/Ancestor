﻿using Ancestor.Core;
using Ancestor.DataAccess.DBAction;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Ancestor.DataAccess.DAO
{
    public class MsSqlDao : DataAccessObjectBase
    {
        public MsSqlDao(Factory.DAOFactoryEx factory) : base(factory)
        {
        }

        public override string ParameterSymbol
        {
            get { return "@"; }
        }
        public override string ConnectorSymbol
        {
            get { return "+"; }
        }
        public override string DateTimeSymbol
        {
            get { return "GetDate()"; }
        }

        protected override IDbAction CreateDbAction(DBObject dbObject)
        {
            throw new NotImplementedException();
        }

        protected override IDbAction CreateDbAction(string connStr)
        {
            throw new NotImplementedException();
        }

        protected override IDbAction CreateDbAction(IDbConnection conn)
        {
            throw new NotImplementedException();
        }

        protected override ExpressionResolver CreateExpressionResolver(ReferenceInfo reference, ExpressionResolver.ExpressionResolveOption option)
        {
            throw new NotImplementedException();
        }

        protected override string GetSequenceCommand(string name, bool moveToNext)
        {
            throw new NotImplementedException();
        }
    }
}
