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
        private DBObject _dbObject;
        private IDataAccessObjectEx _daoCache;
        public DAOFactoryEx(DBObject dbObject)
        {
            _dbObject = dbObject;
        }

        public Func<DAOFactoryEx, DBObject, IDataAccessObjectEx> CustomDaoFactory { get; set; }
        public Func<DAOFactoryEx, IDataAccessObjectEx, DBObject, DBAction.IDbAction> CustomDbFactory { get; set; }

        public DBObject DbObject
        {
            get { return _dbObject; }
            set
            {
                if (_daoCache != null)
                {
                    _daoCache.Dispose();
                    _daoCache = null;
                }
                _dbObject = value;
            }
        }



        public IDataAccessObjectEx GetDataAccessObjectFactory()
        {
            if (_dbObject == null)
                throw new NullReferenceException("no DBObject found");

            if (_daoCache == null)
            {
                switch (_dbObject.DataBaseType)
                {
                    case DBObject.DataBase.Oracle:
                    case DBObject.DataBase.ManagedOracle:
                        _daoCache = new OracleDao(this, _dbObject);
                        break;
                    case DBObject.DataBase.Custom:
                        if (CustomDaoFactory == null)
                            throw new NotImplementedException("custom factory is empty");
                        _daoCache = CustomDaoFactory(this, _dbObject);
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
    }
}
