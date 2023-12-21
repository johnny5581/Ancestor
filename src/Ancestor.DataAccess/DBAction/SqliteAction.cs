using Ancestor.Core;
using Ancestor.DataAccess.DAO;
using Ancestor.DataAccess.DBAction.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;

namespace Ancestor.DataAccess.DBAction
{
    public class SqliteAction : DbActionBase
    {
        public SqliteAction(DataAccessObjectBase dao) : base(dao)
        {
        }

        protected override IDbDataAdapter CreateAdapter(IDbCommand command)
        {
            throw new NotImplementedException();
        }

        protected override IDbConnection CreateConnection(DBObject dbObject, out string dataSource)
        {
            string dsn;
            var connStrBuilder = new SQLiteConnectionStringBuilder();
            logger.WriteLog(System.Diagnostics.TraceEventType.Verbose, "connection mode: " + dbObject.ConnectedMode);

            // sqlite only use local db
            dsn = dbObject.Node;

            logger.WriteLog(System.Diagnostics.TraceEventType.Verbose, "DataSource=" + dsn);
            connStrBuilder.DataSource = dsn;            
            
            connStrBuilder.Password = dbObject.Password;

            dataSource = dbObject.Node;
            return new SQLiteConnection(connStrBuilder.ConnectionString);
        }

        protected override IDbConnection CreateConnection(string connStr, out string dataSource)
        {
            var connSb = new SQLiteConnectionStringBuilder(connStr);
            dataSource = connSb.DataSource;
            return new SQLiteConnection(connStr);
        }

        protected override IDbConnection CreateConnection(IDbConnection conn, out string dataSource)
        {
            var c = (SQLiteConnection)conn;
            dataSource = c.DataSource;
            return c;
        }

        protected override DbActionOptions CreateOption()
        {
            return new SqliteOptions();
        }

        protected override IDbDataParameter CreateParameter(DBParameter parameter, DbActionOptions options)
        {
            var p = new SQLiteParameter(parameter.Name, parameter.Value);
            if (!parameter.ParameterType.IsLazy)
                p.DbType = (DbType)parameter.ParameterType.Code;
            if(parameter.Size != null)
                p.Size = parameter.Size.Value;
            p.Direction = parameter.ParameterDirection;
            return p;
        }
    }
}
