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
            var wrapper = new DataAccessObjectWrapper(dao);
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
            get { return _dao.Factory.Source as DBObject; }
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
            return _dao.QueryWithRowid(objectModel);
        }

        AncestorResult IDataAccessObject.Query<T>(IModel objectModel)
        {
            return _dao.QueryWithRowid<T>(objectModel);
        }

        AncestorResult IDataAccessObject.Query<T>(Expression<Func<T, bool>> predicate)
        {
            return _dao.QueryWithRowid(predicate);
        }

        AncestorResult IDataAccessObject.Query<T>(Expression<Func<T, bool>> predicate, Expression<Func<T, object>> selectCondition)
        {
            return _dao.QueryFromLambda(predicate, selectCondition, null, false, new AncestorOptions { { "AddRowId", true } });
        }

        AncestorResult IDataAccessObject.Query<T1, T2>(Expression<Func<T1, T2, bool>> predicate, Expression<Func<T1, T2, object>> selectCondition)
        {
            return _dao.QueryFromLambda(predicate, selectCondition, null, false, new AncestorOptions { { "AddRowId", true } });
        }

        AncestorResult IDataAccessObject.Query<T1, T2, T3>(Expression<Func<T1, T2, T3, bool>> predicate, Expression<Func<T1, T2, T3, object>> selectCondition)
        {
            return _dao.QueryFromLambda(predicate, selectCondition, null, false, new AncestorOptions { { "AddRowId", true } });
        }

        AncestorResult IDataAccessObject.Query<T1, T2, T3, T4>(Expression<Func<T1, T2, T3, T4, bool>> predicate, Expression<Func<T1, T2, T3, T4, object>> selectCondition)
        {
            return _dao.QueryFromLambda(predicate, selectCondition, null, false, new AncestorOptions { { "AddRowId", true } });
        }

        AncestorResult IDataAccessObject.Query<T1, T2, T3, T4, T5>(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate, Expression<Func<T1, T2, T3, T4, T5, object>> selectCondition)
        {
            return _dao.QueryFromLambda(predicate, selectCondition, null, false, new AncestorOptions { { "AddRowId", true } });
        }

        AncestorResult IDataAccessObject.Query<T1, T2, T3, T4, T5, T6>(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate, Expression<Func<T1, T2, T3, T4, T5, T6, object>> selectCondition)
        {
            return _dao.QueryFromLambda(predicate, selectCondition, null, false, new AncestorOptions { { "AddRowId", true } });
        }

        AncestorResult IDataAccessObject.Query<FakeType>(Expression<Func<FakeType, bool>> predicate, Type realType)
        {
            var map = DataAccessObjectExtensions.CreateProxyMap(DataAccessObjectExtensions.CreateTuple(typeof(FakeType), realType));
            return _dao.QueryFromLambda(predicate, null, map, false, new AncestorOptions { { "AddRowId", true } });
        }

        AncestorResult IDataAccessObject.Query<FakeType>(Expression<Func<FakeType, bool>> predicate, Expression<Func<FakeType, object>> selectCondition, Type realType)
        {
            var map = DataAccessObjectExtensions.CreateProxyMap(DataAccessObjectExtensions.CreateTuple(typeof(FakeType), realType));
            return _dao.QueryFromLambda(predicate, selectCondition, map, false, new AncestorOptions { { "AddRowId", true } });
        }

        AncestorResult IDataAccessObject.Query<FakeType1, FakeType2>(Expression<Func<FakeType1, FakeType2, bool>> predicate, Expression<Func<FakeType1, FakeType2, object>> selectCondition, Type realType1, Type realType2)
        {
            var map = DataAccessObjectExtensions.CreateProxyMap(
                DataAccessObjectExtensions.CreateTuple(typeof(FakeType1), realType1), 
                DataAccessObjectExtensions.CreateTuple(typeof(FakeType2), realType2)
            );
            return _dao.QueryFromLambda(predicate, selectCondition, map, false, new AncestorOptions { { "AddRowId", true } });
        }

        AncestorResult IDataAccessObject.Query<FakeType1, FakeType2, FakeType3>(Expression<Func<FakeType1, FakeType2, FakeType3, bool>> predicate, Expression<Func<FakeType1, FakeType2, FakeType3, object>> selectCondition, Type realType1, Type realType2, Type realType3)
        {
            var map = DataAccessObjectExtensions.CreateProxyMap(
                DataAccessObjectExtensions.CreateTuple(typeof(FakeType1), realType1),
                DataAccessObjectExtensions.CreateTuple(typeof(FakeType2), realType2),
                DataAccessObjectExtensions.CreateTuple(typeof(FakeType3), realType3)
            );
            return _dao.QueryFromLambda(predicate, selectCondition, map, false, new AncestorOptions { { "AddRowId", true } });
        }

        AncestorResult IDataAccessObject.Query<FakeType1, FakeType2, FakeType3, FakeType4>(Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, bool>> predicate, Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, object>> selectCondition, Type realType1, Type realType2, Type realType3, Type realType4)
        {
            var map = DataAccessObjectExtensions.CreateProxyMap(
                DataAccessObjectExtensions.CreateTuple(typeof(FakeType1), realType1),
                DataAccessObjectExtensions.CreateTuple(typeof(FakeType2), realType2),
                DataAccessObjectExtensions.CreateTuple(typeof(FakeType3), realType3),
                DataAccessObjectExtensions.CreateTuple(typeof(FakeType4), realType4)
            );
            return _dao.QueryFromLambda(predicate, selectCondition, map, false, new AncestorOptions { { "AddRowId", true } });
        }

        AncestorResult IDataAccessObject.Query<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5>(Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, bool>> predicate, Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, object>> selectCondition, Type realType1, Type realType2, Type realType3, Type realType4, Type realType5)
        {
            var map = DataAccessObjectExtensions.CreateProxyMap(
                DataAccessObjectExtensions.CreateTuple(typeof(FakeType1), realType1),
                DataAccessObjectExtensions.CreateTuple(typeof(FakeType2), realType2),
                DataAccessObjectExtensions.CreateTuple(typeof(FakeType3), realType3),
                DataAccessObjectExtensions.CreateTuple(typeof(FakeType4), realType4),
                DataAccessObjectExtensions.CreateTuple(typeof(FakeType5), realType5)
            );
            return _dao.QueryFromLambda(predicate, selectCondition, map, false, new AncestorOptions { { "AddRowId", true } });
        }

        AncestorResult IDataAccessObject.Query<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, FakeType6>(Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, FakeType6, bool>> predicate, Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, FakeType6, object>> selectCondition, Type realType1, Type realType2, Type realType3, Type realType4, Type realType5, Type realType6)
        {
            var map = DataAccessObjectExtensions.CreateProxyMap(
                DataAccessObjectExtensions.CreateTuple(typeof(FakeType1), realType1),
                DataAccessObjectExtensions.CreateTuple(typeof(FakeType2), realType2),
                DataAccessObjectExtensions.CreateTuple(typeof(FakeType3), realType3),
                DataAccessObjectExtensions.CreateTuple(typeof(FakeType4), realType4),
                DataAccessObjectExtensions.CreateTuple(typeof(FakeType5), realType5),
                DataAccessObjectExtensions.CreateTuple(typeof(FakeType6), realType6)
            );
            return _dao.QueryFromLambda(predicate, selectCondition, map, false, new AncestorOptions { { "AddRowId", true } });
        }

        AncestorResult IDataAccessObject.Query<FakeType>(Expression<Func<FakeType, bool>> predicate, string name)
        {
            var map = DataAccessObjectExtensions.CreateProxyMap(DataAccessObjectExtensions.CreateTuple(typeof(FakeType), name));
            return _dao.QueryFromLambda(predicate, null, map, false, new AncestorOptions { { "AddRowId", true } });
        }

        AncestorResult IDataAccessObject.Query<FakeType>(Expression<Func<FakeType, bool>> predicate, Expression<Func<FakeType, object>> selectCondition, string name)
        {
            var map = DataAccessObjectExtensions.CreateProxyMap(DataAccessObjectExtensions.CreateTuple(typeof(FakeType), name));
            return _dao.QueryFromLambda(predicate, selectCondition, map, false, new AncestorOptions { { "AddRowId", true } });
        }

        AncestorResult IDataAccessObject.Query<FakeType1, FakeType2>(Expression<Func<FakeType1, FakeType2, bool>> predicate, Expression<Func<FakeType1, FakeType2, object>> selectCondition, string name1, string name2)
        {
            var map = DataAccessObjectExtensions.CreateProxyMap(
                DataAccessObjectExtensions.CreateTuple(typeof(FakeType1), name1),
                DataAccessObjectExtensions.CreateTuple(typeof(FakeType2), name2)
            );
            return _dao.QueryFromLambda(predicate, selectCondition, map, false, new AncestorOptions { { "AddRowId", true } });
        }

        AncestorResult IDataAccessObject.Query<FakeType1, FakeType2, FakeType3>(Expression<Func<FakeType1, FakeType2, FakeType3, bool>> predicate, Expression<Func<FakeType1, FakeType2, FakeType3, object>> selectCondition, string name1, string name2, string name3)
        {
            var map = DataAccessObjectExtensions.CreateProxyMap(
                DataAccessObjectExtensions.CreateTuple(typeof(FakeType1), name1),
                DataAccessObjectExtensions.CreateTuple(typeof(FakeType2), name2),
                DataAccessObjectExtensions.CreateTuple(typeof(FakeType3), name3)
            );
            return _dao.QueryFromLambda(predicate, selectCondition, map, false, new AncestorOptions { { "AddRowId", true } });
        }

        AncestorResult IDataAccessObject.Query<FakeType1, FakeType2, FakeType3, FakeType4>(Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, bool>> predicate, Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, object>> selectCondition, string name1, string name2, string name3, string name4)
        {
            var map = DataAccessObjectExtensions.CreateProxyMap(
                DataAccessObjectExtensions.CreateTuple(typeof(FakeType1), name1),
                DataAccessObjectExtensions.CreateTuple(typeof(FakeType2), name2),
                DataAccessObjectExtensions.CreateTuple(typeof(FakeType3), name3),
                DataAccessObjectExtensions.CreateTuple(typeof(FakeType4), name4)
            );
            return _dao.QueryFromLambda(predicate, selectCondition, map, false, new AncestorOptions { { "AddRowId", true } });
        }

        AncestorResult IDataAccessObject.Query<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5>(Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, bool>> predicate, Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, object>> selectCondition, string name1, string name2, string name3, string name4, string name5)
        {
            var map = DataAccessObjectExtensions.CreateProxyMap(
                DataAccessObjectExtensions.CreateTuple(typeof(FakeType1), name1),
                DataAccessObjectExtensions.CreateTuple(typeof(FakeType2), name2),
                DataAccessObjectExtensions.CreateTuple(typeof(FakeType3), name3),
                DataAccessObjectExtensions.CreateTuple(typeof(FakeType4), name4),
                DataAccessObjectExtensions.CreateTuple(typeof(FakeType5), name5)
            );
            return _dao.QueryFromLambda(predicate, selectCondition, map, false, new AncestorOptions { { "AddRowId", true } });
        }

        AncestorResult IDataAccessObject.Query<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, FakeType6>(Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, FakeType6, bool>> predicate, Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, FakeType6, object>> selectCondition, string name1, string name2, string name3, string name4, string name5, string name6)
        {
            var map = DataAccessObjectExtensions.CreateProxyMap(
                DataAccessObjectExtensions.CreateTuple(typeof(FakeType1), name1),
                DataAccessObjectExtensions.CreateTuple(typeof(FakeType2), name2),
                DataAccessObjectExtensions.CreateTuple(typeof(FakeType3), name3),
                DataAccessObjectExtensions.CreateTuple(typeof(FakeType4), name4),
                DataAccessObjectExtensions.CreateTuple(typeof(FakeType5), name5),
                DataAccessObjectExtensions.CreateTuple(typeof(FakeType6), name6)
            );
            return _dao.QueryFromLambda(predicate, selectCondition, map, false, new AncestorOptions { { "AddRowId", true } });
        }

        AncestorResult IDataAccessObject.QueryNoRowid(IModel objectModel)
        {
            return _dao.Query(objectModel);
        }

        AncestorResult IDataAccessObject.QueryNoRowid<T>(IModel objectModel)
        {
            return _dao.Query<T>(objectModel);
        }

        AncestorResult IDataAccessObject.QueryNoRowid<T>(Expression<Func<T, bool>> predicate)
        {
            return _dao.Query(predicate);
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
