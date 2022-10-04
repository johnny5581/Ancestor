using Ancestor.Core;
using Ancestor.DataAccess.DAO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;

namespace Ancestor.DataAccess.DBAction
{
    public class OleAction : DbActionBase
    {
        private static readonly Dictionary<string, OleDbType> TypeNameMap
              = new Dictionary<string, OleDbType>(StringComparer.OrdinalIgnoreCase)
              {
                { "BYTE[]", OleDbType.LongVarBinary },
                { "CHAR[]", OleDbType.LongVarWChar },
                { "NUMBER", OleDbType.Decimal },
                { "VARCHAR2", OleDbType.VarWChar},
                { "SYSTEM.STRING", OleDbType.VarWChar},
                { "STRING", OleDbType.VarWChar },
                { "SYSTEM.DATETIME", OleDbType.Date },
                { "DATETIME", OleDbType.Date },
                { "DATE", OleDbType.Date },
                { "INT64", OleDbType.BigInt },
                { "INT32", OleDbType.Integer },
                { "INT16", OleDbType.SmallInt },
                { "BYTE", OleDbType.Binary },
                { "DECIMAL", OleDbType.Decimal },
                { "FLOAT", OleDbType.Single },
                { "DOUBLE", OleDbType.Double },
                { "CHAR", OleDbType.WChar },
                { "TIMESTAMP", OleDbType.DBTimeStamp },
                //{ "REFCURSOR", OleDbType.RefCursor },
                //{ "CLOB", OleDbType.Clob },
                //{ "LONG", OleDbType.Long },
                { "LONGRAW", OleDbType.LongVarWChar },
                { "BOOLEAN", OleDbType.Boolean }
              };
        public OleAction(DataAccessObjectBase dao) : base(dao)
        {
        }
        #region Protected / Private
        protected override IDbDataAdapter CreateAdapter(IDbCommand command)
        {
            return new OleDbDataAdapter((OleDbCommand)command);            
        }

        protected override IDbConnection CreateConnection(DBObject dbObject, out string dataSource)
        {
            var builder = new OleDbConnectionStringBuilder();
            builder.FileName = dbObject.Node;
            dataSource = builder.DataSource;
            return new OleDbConnection(builder.ConnectionString);            
        }

        protected override IDbConnection CreateConnection(string connStr, out string dataSource)
        {
            var builder = new OleDbConnectionStringBuilder(connStr);
            dataSource = builder.DataSource;
            return new OleDbConnection(builder.ConnectionString);
        }

        protected override IDbConnection CreateConnection(IDbConnection conn, out string dataSource)
        {
            var oleConn = (OleDbConnection)conn;
            dataSource = oleConn.DataSource;
            return oleConn;
        }

        protected override DbActionOptions CreateOption()
        {
            return null;
        }

        protected override IDbDataParameter CreateParameter(DBParameter parameter, DbActionOptions options)
        {
            var p = new OleDbParameter(parameter.Name, parameter.Value);
            if (!parameter.ParameterType.IsLazy)
                p.OleDbType = GetParameterType(parameter.ParameterType, parameter.Value);
            if (parameter.Size != null)
                p.Size = parameter.Size.Value;
            p.Direction = parameter.ParameterDirection;            
            return p;
        }
        #endregion

        private static OleDbType GetParameterType(DBParameterType parameterType, object value)
        {
            OleDbType type;
            if (parameterType.IsDbType)
            {
                return DbTypeToOracleDbType((DbType)parameterType.Code);
            }
            else if (TryGetOleDbType(parameterType.Name, out type))
            {
                return type;
            }
            throw new ArgumentException("Can not convert parameter to OracleParameter:" + parameterType);
        }
        private static bool TryGetOleDbType(string name, out OleDbType type)
        {
            return Enum.TryParse(name, true, out type) || TypeNameMap.TryGetValue(name, out type);
        }
        private static OleDbType DbTypeToOracleDbType(DbType dbType)
        {
            switch (dbType)
            {
                case DbType.AnsiString:
                case DbType.String:
                    return OleDbType.LongVarWChar;
                case DbType.AnsiStringFixedLength:
                case DbType.StringFixedLength:
                    return OleDbType.VarWChar;
                case DbType.Byte:
                case DbType.SByte:
                    return OleDbType.Binary;
                case DbType.UInt16:
                    return OleDbType.UnsignedSmallInt;
                case DbType.Int16:
                    return OleDbType.SmallInt;
                case DbType.Int32:
                    return OleDbType.Integer;
                case DbType.UInt32:
                    return OleDbType.UnsignedInt;
                case DbType.Int64:
                    return OleDbType.BigInt;
                case DbType.UInt64:
                    return OleDbType.UnsignedBigInt;
                case DbType.Single:
                    return OleDbType.Single;
                case DbType.Double:
                    return OleDbType.Double;
                case DbType.Date:
                case DbType.DateTime:
                case DbType.Time:
                    return OleDbType.Date;
                case DbType.Binary:
                    return OleDbType.Binary;
                case DbType.VarNumeric:
                case DbType.Decimal:
                case DbType.Currency:
                    return OleDbType.Decimal;
                case DbType.Object:
                    return OleDbType.LongVarBinary;
                case DbType.Guid:
                    return OleDbType.Guid;
                case DbType.Boolean:
                default:
                    throw new NotSupportedException("not supported type: " + dbType);
            }
        }
    }
}
