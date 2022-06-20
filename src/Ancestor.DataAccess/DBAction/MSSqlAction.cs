using Ancestor.Core;
using Ancestor.DataAccess.DAO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Ancestor.DataAccess.DBAction
{
    public class MSSqlAction : DbActionBase
    {
        public MSSqlAction(DataAccessObjectBase dao) : base(dao)
        {
        }
        protected override IDbDataAdapter CreateAdapter(IDbCommand command)
        {
            throw new NotImplementedException();
        }

        protected override IDbConnection CreateConnection(DBObject dbObject, out string dsn)
        {
            throw new NotImplementedException();
        }

        protected override IDbConnection CreateConnection(string connStr, out string dataSource)
        {
            throw new NotImplementedException();
        }

        protected override IDbConnection CreateConnection(IDbConnection conn, out string dataSource)
        {
            throw new NotImplementedException();
        }

        protected override DbActionOptions CreateOption()
        {
            throw new NotImplementedException();
        }

        protected override IDbDataParameter CreateParameter(DBParameter parameter, DbActionOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
