using Ancestor.Core;
using Ancestor.DataAccess.DAO;
using Ancestor.DataAccess.DBAction;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Ancestor.DataAccess.Factory
{
    public class DAOFactory
    {
        private DBObject _dbObject;
        public DAOFactory(DBObject dbObject)
        {
            _dbObject = dbObject;
        }

        public IDataAccessObject GetDataAccessObjectFactory()
        {
            if (_dbObject == null)
                throw new NullReferenceException("no DBObject found");

            var dao = new DAOFactoryEx(_dbObject).GetDataAccessObjectFactory();
            var daoBase = dao as DataAccessObjectBase;
            var wrapper = new DataAccessObjectWrapper(daoBase);
            return wrapper;
        }
    }

    internal class DataAccessObjectWrapper : IDataAccessObject
    {
        private DataAccessObjectBase _dao;
        private DbActionBase _db;
        public DataAccessObjectWrapper(IDataAccessObjectEx dao)
        {
            _dao = (DataAccessObjectBase)dao;
            _db = (DbActionBase)_dao.DbAction;
        }

        DBObject IDataAccessObject.DbObject
        {
            get { return _dao.DbObject; }
        }
        IDbConnection IDataAccessObject.DBConnection
        {
            get { return ((IDataAccessObjectEx)_dao).DBConnection; }
        }

        IDbTransaction IDataAccessObject.BeginTransaction()
        {
            _dao.BeginTransaction();
            return _db.Transaction;
        }

        IDbTransaction IDataAccessObject.BeginTransaction(IsolationLevel isoLationLevel)
        {
            _dao.BeginTransaction(isoLationLevel);
            return _db.Transaction;
        }

        AncestorResult IDataAccessObject.BulkInsert<T>(List<T> ObjList)
        {
            return _dao.BulkInsert(ObjList);
        }

        void IDataAccessObject.Commit()
        {
            _dao.Commit();
        }

        AncestorResult IDataAccessObject.Delete(IModel whereObject)
        {
            return _dao.Delete(whereObject);
        }

        AncestorResult IDataAccessObject.Delete<T>(Expression<Func<T, bool>> predicate)
        {
            return _dao.Delete(predicate);
        }

        AncestorResult IDataAccessObject.Delete(IModel whereObject, string name)
        {
            return _dao.Delete(whereObject, name);
        }

        AncestorResult IDataAccessObject.Delete<T>(Expression<Func<T, bool>> predicate, string name)
        {
            return _dao.Delete(predicate, name);
        }

        void IDisposable.Dispose()
        {
            _dao.Dispose();
        }

        AncestorResult IDataAccessObject.ExecuteNonQuery(string sqlString, object modelObject)
        {
            return _dao.ExecuteNonQuery(sqlString, modelObject);
        }

        AncestorResult IDataAccessObject.ExecuteStoredProcedure(string procedureName, bool bindbyName, List<DBParameter> dBParameter)
        {
            return _dao.ExecuteStoredProcedure(procedureName, bindbyName, dBParameter);
        }

        IDbAction IDataAccessObject.GetActionFactory()
        {
            return _dao.DbAction;
        }

        AncestorResult IDataAccessObject.Insert(IModel objectModel)
        {
            return _dao.Insert(objectModel);
        }

        AncestorResult IDataAccessObject.Insert(IModel model, string name)
        {
            return _dao.Insert(model, name);
        }

        AncestorResult IDataAccessObject.Query(string sqlString, object paramsObjects)
        {
            return _dao.Query(sqlString, paramsObjects);
        }

        AncestorResult IDataAccessObject.Query(IModel objectModel)
        {
            return _dao.Query(objectModel);
        }

        AncestorResult IDataAccessObject.Query<T>(IModel objectModel)
        {
            return _dao.Query<T>(objectModel);
        }

        AncestorResult IDataAccessObject.Query<T>(Expression<Func<T, bool>> predicate)
        {
            return _dao.Query(predicate);
        }

        AncestorResult IDataAccessObject.Query<T>(Expression<Func<T, bool>> predicate, Expression<Func<T, object>> selectCondition)
        {
            return _dao.Query(predicate, selectCondition);
        }

        AncestorResult IDataAccessObject.Query<T1, T2>(Expression<Func<T1, T2, bool>> predicate, Expression<Func<T1, T2, object>> selectCondition)
        {
            return _dao.Query(predicate, selectCondition);
        }

        AncestorResult IDataAccessObject.Query<T1, T2, T3>(Expression<Func<T1, T2, T3, bool>> predicate, Expression<Func<T1, T2, T3, object>> selectCondition)
        {
            return _dao.Query(predicate, selectCondition);
        }

        AncestorResult IDataAccessObject.Query<T1, T2, T3, T4>(Expression<Func<T1, T2, T3, T4, bool>> predicate, Expression<Func<T1, T2, T3, T4, object>> selectCondition)
        {
            return _dao.Query(predicate, selectCondition);
        }

        AncestorResult IDataAccessObject.Query<T1, T2, T3, T4, T5>(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate, Expression<Func<T1, T2, T3, T4, T5, object>> selectCondition)
        {
            return _dao.Query(predicate, selectCondition);
        }

        AncestorResult IDataAccessObject.Query<T1, T2, T3, T4, T5, T6>(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate, Expression<Func<T1, T2, T3, T4, T5, T6, object>> selectCondition)
        {
            return _dao.Query(predicate, selectCondition);
        }

        AncestorResult IDataAccessObject.Query<FakeType>(Expression<Func<FakeType, bool>> predicate, Type realType)
        {
            return _dao.Query(predicate, realType);
        }

        AncestorResult IDataAccessObject.Query<FakeType>(Expression<Func<FakeType, bool>> predicate, Expression<Func<FakeType, object>> selectCondition, Type realType)
        {
            return _dao.Query(predicate, selectCondition, realType);
        }

        AncestorResult IDataAccessObject.Query<FakeType1, FakeType2>(Expression<Func<FakeType1, FakeType2, bool>> predicate, Expression<Func<FakeType1, FakeType2, object>> selectCondition, Type realType1, Type realType2)
        {
            return _dao.Query(predicate, selectCondition, realType1, realType2);
        }

        AncestorResult IDataAccessObject.Query<FakeType1, FakeType2, FakeType3>(Expression<Func<FakeType1, FakeType2, FakeType3, bool>> predicate, Expression<Func<FakeType1, FakeType2, FakeType3, object>> selectCondition, Type realType1, Type realType2, Type realType3)
        {
            return _dao.Query(predicate, selectCondition, realType1, realType2, realType3);
        }

        AncestorResult IDataAccessObject.Query<FakeType1, FakeType2, FakeType3, FakeType4>(Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, bool>> predicate, Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, object>> selectCondition, Type realType1, Type realType2, Type realType3, Type realType4)
        {
            return _dao.Query(predicate, selectCondition, realType1, realType2, realType3, realType4);
        }

        AncestorResult IDataAccessObject.Query<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5>(Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, bool>> predicate, Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, object>> selectCondition, Type realType1, Type realType2, Type realType3, Type realType4, Type realType5)
        {
            return _dao.Query(predicate, selectCondition, realType1, realType2, realType3, realType4, realType5);
        }

        AncestorResult IDataAccessObject.Query<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, FakeType6>(Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, FakeType6, bool>> predicate, Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, FakeType6, object>> selectCondition, Type realType1, Type realType2, Type realType3, Type realType4, Type realType5, Type realType6)
        {
            return _dao.Query(predicate, selectCondition, realType1, realType2, realType3, realType4, realType5, realType6);
        }

        AncestorResult IDataAccessObject.Query<FakeType>(Expression<Func<FakeType, bool>> predicate, string name)
        {
            return _dao.Query(predicate, name);
        }

        AncestorResult IDataAccessObject.Query<FakeType>(Expression<Func<FakeType, bool>> predicate, Expression<Func<FakeType, object>> selectCondition, string name)
        {
            return _dao.Query(predicate, selectCondition, name);
        }

        AncestorResult IDataAccessObject.Query<FakeType1, FakeType2>(Expression<Func<FakeType1, FakeType2, bool>> predicate, Expression<Func<FakeType1, FakeType2, object>> selectCondition, string name1, string name2)
        {
            return _dao.Query(predicate, selectCondition, name1, name2);
        }

        AncestorResult IDataAccessObject.Query<FakeType1, FakeType2, FakeType3>(Expression<Func<FakeType1, FakeType2, FakeType3, bool>> predicate, Expression<Func<FakeType1, FakeType2, FakeType3, object>> selectCondition, string name1, string name2, string name3)
        {
            return _dao.Query(predicate, selectCondition, name1, name2, name3);
        }

        AncestorResult IDataAccessObject.Query<FakeType1, FakeType2, FakeType3, FakeType4>(Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, bool>> predicate, Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, object>> selectCondition, string name1, string name2, string name3, string name4)
        {
            return _dao.Query(predicate, selectCondition, name1, name2, name3, name4);
        }

        AncestorResult IDataAccessObject.Query<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5>(Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, bool>> predicate, Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, object>> selectCondition, string name1, string name2, string name3, string name4, string name5)
        {
            return _dao.Query(predicate, selectCondition, name1, name2, name3, name4, name5);
        }

        AncestorResult IDataAccessObject.Query<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, FakeType6>(Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, FakeType6, bool>> predicate, Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, FakeType6, object>> selectCondition, string name1, string name2, string name3, string name4, string name5, string name6)
        {
            return _dao.Query(predicate, selectCondition, name1, name2, name3, name4, name5, name6);
        }

        AncestorResult IDataAccessObject.QueryNoRowid(IModel objectModel)
        {
            return _dao.QueryNoRowid(objectModel);
        }

        AncestorResult IDataAccessObject.QueryNoRowid<T>(IModel objectModel)
        {
            return _dao.QueryNoRowid<T>(objectModel);
        }

        AncestorResult IDataAccessObject.QueryNoRowid<T>(Expression<Func<T, bool>> predicate)
        {
            return _dao.QueryNoRowid(predicate);
        }

        void IDataAccessObject.Rollback()
        {
            _dao.Rollback() ;
        }

        AncestorResult IDataAccessObject.Update(IModel valueObject, object paramsObjects)
        {
            return _dao.Update(valueObject, paramsObjects);
        }

        AncestorResult IDataAccessObject.Update(IModel valueObject, IModel whereObject)
        {
            return _dao.Update(valueObject, whereObject);
        }

        AncestorResult IDataAccessObject.Update<T>(IModel valueObject, Expression<Func<T, bool>> predicate)
        {
            return _dao.Update(valueObject, predicate);
        }

        AncestorResult IDataAccessObject.Update(IModel valueObject, object paramsObjects, string name)
        {
            return _dao.Update(valueObject, paramsObjects, name);
        }

        AncestorResult IDataAccessObject.Update(IModel valueObject, IModel whereObject, string name)
        {
            return _dao.Update(valueObject, whereObject, name);
        }

        AncestorResult IDataAccessObject.Update<T>(IModel valueObject, Expression<Func<T, bool>> predicate, string name)
        {
            return _dao.Update(valueObject, predicate, name);
        }

        AncestorResult IDataAccessObject.UpdateAll(IModel valueObject, IModel whereObject)
        {
            return _dao.UpdateAll(valueObject, whereObject);
        }

        AncestorResult IDataAccessObject.UpdateAll<T>(IModel valueObject, Expression<Func<T, bool>> predicate)
        {
            return _dao.UpdateAll(valueObject, predicate);
        }

        AncestorResult IDataAccessObject.UpdateAll(IModel valueObject, IModel whereObject, string name)
        {
            return _dao.UpdateAll(valueObject, whereObject, name);
        }

        AncestorResult IDataAccessObject.UpdateAll<T>(IModel valueObject, Expression<Func<T, bool>> predicate, string name)
        {
            return _dao.UpdateAll(valueObject, predicate, name);
        }

        IDataAccessObjectEx IDataAccessObject.GetDataAccessObjectEx()
        {
            return _dao;
        }
    }
}
