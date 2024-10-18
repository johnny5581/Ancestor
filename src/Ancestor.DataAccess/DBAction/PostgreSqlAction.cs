using Ancestor.Core;
using Ancestor.DataAccess.DAO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Npgsql;
using Ancestor.DataAccess.DBAction.Options;

namespace Ancestor.DataAccess.DBAction
{
    public class PostgreSqlAction : DbActionBase
    {        
        public PostgreSqlAction(DataAccessObjectBase dao) : base(dao)
        {
        }

        protected override IDbDataAdapter CreateAdapter(IDbCommand command)
        {
            return new NpgsqlDataAdapter(command as NpgsqlCommand);
        }

        protected override IDbConnection CreateConnection(DBObject dbObject, out string dataSource)
        {
            var connStrBuilder = new NpgsqlConnectionStringBuilder();
#if NET40
            connStrBuilder.UserName = dbObject.ID;
#else
            connStrBuilder.Username = dbObject.ID;
#endif
            connStrBuilder.Password = dbObject.Password;
            connStrBuilder.Host = dbObject.IP ?? dbObject.Node;
            connStrBuilder.Database = dbObject.Hostname;


            if (!int.TryParse(dbObject.Port, out int port))
                port = 5432;
            connStrBuilder.Port = port;
            connStrBuilder.Password = dbObject.Password;
            dataSource = $"{connStrBuilder.Host}/{connStrBuilder.Database}";
            return new NpgsqlConnection(connStrBuilder.ConnectionString);
        }

        protected override IDbConnection CreateConnection(string connStr, out string dataSource)
        {
            var connStrBuilder = new NpgsqlConnectionStringBuilder(connStr);
            dataSource = $"{connStrBuilder.Host}/{connStrBuilder.Database}";
            return new NpgsqlConnection(connStrBuilder.ConnectionString);
        }

        protected override IDbConnection CreateConnection(IDbConnection conn, out string dataSource)
        {
            var c = (NpgsqlConnection)conn;
            dataSource = c.DataSource;
            return c;
        }

        protected override DbActionOptions CreateOption()
        {
            return new PostgreSqlOptions();
        }

        protected override IDbDataParameter CreateParameter(DBParameter parameter, DbActionOptions options)
        {
            var p = new NpgsqlParameter(parameter.Name, parameter.Value);
            p.DbType = (DbType)parameter.ParameterType.Code;
            p.Size = parameter.Size.Value;
            p.Direction = parameter.ParameterDirection;
            return p;
        }

        protected override AncestorException CreateAncestorException(Exception innerException, QueryParameter parameter)
        {
            if(innerException is NpgsqlException)
            {
                var exception = innerException as NpgsqlException;
                switch(exception.ErrorCode)
                {
                    
                }
            }
            return base.CreateAncestorException(innerException, parameter);
        }
    }
}
