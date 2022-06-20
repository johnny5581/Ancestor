using Ancestor.Core;
using Ancestor.DataAccess.DAO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ancestor.DataAccess.Factory
{
    public class DAOFactoryEx : IDAOFactory
    {
        private IDBConnection _conn;
        private string _connStr;
        private DBObject _dbObject;
        private IDataAccessObjectEx _daoCache;
        private DBObject.DataBase _db;
        public DAOFactoryEx(DBObject dbObject)
        {
            ResetSource(dbObject);
        }
        public DAOFactoryEx(string connStr, DBObject.DataBase database)
        {
            ResetSource(connStr, database);
        }
        public DAOFactoryEx(IDBConnection conn, DBObject.DataBase database)
        {
            ResetSource(conn, database);
        }
        public DAOFactoryEx(IDBConnection conn)
        {
            ResetSource(conn);
        }        

        public DAOFactoryEx(DAOFactoryEx otherFactory)
        {
            Mode = otherFactory.Mode;
            _db = otherFactory._db;
            CustomDaoFactory = otherFactory.CustomDaoFactory;
            CustomDbFactory = otherFactory.CustomDbFactory;
            switch(otherFactory.Mode)
            {
                case SourceMode.Connection:
                    throw new InvalidOperationException("factory from connection cannot be clone");
                case SourceMode.ConnectionString:
                    _connStr = otherFactory._connStr;
                    break;
                case SourceMode.DBObject:
                    _dbObject = (DBObject)otherFactory._dbObject.Clone();
                    break;                
            }
        }
        public SourceMode Mode { get; private set; }
        public Func<DAOFactoryEx, IDataAccessObjectEx> CustomDaoFactory { get; set; }
        public Func<DAOFactoryEx, IDataAccessObjectEx, DBAction.IDbAction> CustomDbFactory { get; set; }
        public object Source
        {
            get
            {
                switch(Mode)
                {
                    case SourceMode.Connection:
                        return _conn;
                    case SourceMode.ConnectionString:
                        return _connStr;
                    case SourceMode.DBObject:
                        return _dbObject;
                    default:
                        return null;
                }
            }
        }
        public DBObject.DataBase Database
        {
            get { return _db; }
        }





        public IDataAccessObjectEx GetDataAccessObjectFactory()
        {
            if (_daoCache == null)
            {
                switch (_db)
                {
                    case DBObject.DataBase.Oracle:
                    case DBObject.DataBase.ManagedOracle:
                        _daoCache = new OracleDao(this);
                        break;
                    case DBObject.DataBase.Custom:
                        if (CustomDaoFactory == null)
                            throw new NotImplementedException("custom factory is empty");
                        _daoCache = CustomDaoFactory(this);
                        break;
                    case DBObject.DataBase.MSSQL:
                    case DBObject.DataBase.MySQL:
                    case DBObject.DataBase.Access:
                    case DBObject.DataBase.SQLlite:
                    case DBObject.DataBase.Sybase:
                    default:
                        throw new NotImplementedException("database not implement: " + _dbObject.DataBaseType);
                }
            }
            return _daoCache;
        }

        

        public void ResetSource(DBObject dbObject)
        {
            if (dbObject == null) throw new ArgumentNullException("dbObject");
            _dbObject = dbObject;
            _db = dbObject.DataBaseType;
            Mode = SourceMode.DBObject;
            ResetDaoCache();
        }

        public void ResetSource(string connStr, DBObject.DataBase database)
        {
            if (connStr == null) throw new ArgumentNullException("connStr");
            _connStr = connStr;
            _db = database;
            Mode = SourceMode.ConnectionString;
            ResetDaoCache();
        }

        public void ResetSource(IDBConnection conn)
        {
            if (conn == null) throw new ArgumentNullException("conn");
            var connType = conn.GetType();
            var connTypeName = connType.FullName;
            switch (connTypeName)
            {
                case "Oracle.DataAcess.OracleConnection":
                    ResetSource(conn, DBObject.DataBase.Oracle);
                    break;
                case "Oracle.ManagedDataAcess.OracleConnection":
                    ResetSource(conn, DBObject.DataBase.ManagedOracle);
                    break;
                default:
                    throw new InvalidOperationException("unknown connection: " + connTypeName);
            }
        }
        public void ResetSource(IDBConnection conn, DBObject.DataBase database)
        {
            if (conn == null) throw new ArgumentNullException("conn");
            _conn = conn;
            _db = database;
            Mode = SourceMode.Connection;
            ResetDaoCache();
        }
        protected void ResetDaoCache()
        {
            if (_daoCache != null)
            {
                _daoCache.Dispose();
                _daoCache = null;
            }
        }


        /// <summary>
        /// Connection source mode
        /// </summary>
        public enum SourceMode
        {
            None, DBObject, ConnectionString, Connection
        }
    }
}
