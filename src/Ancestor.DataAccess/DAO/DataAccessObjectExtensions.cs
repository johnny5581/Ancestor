using Ancestor.Core;
using Ancestor.DataAccess.DAO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace System
{
    /// <summary>
    /// Extension methods for <see cref="IDataAccessObjectEx"/>
    /// </summary>
    public static class DataAccessObjectExtensions
    {
        #region Wrapper before 1.4.8
        public static AncestorResult Query(this IDataAccessObjectEx dao, string sqlString, object paramsObjects)
        {
            return dao.QueryFromSqlString(sqlString, paramsObjects, null, false, null);
        }
        public static AncestorResult Query(this IDataAccessObjectEx dao, IModel objectModel)
        {
            return dao.QueryFromModel(objectModel, null, null, false, null);
        }
        public static AncestorResult Query<T>(this IDataAccessObjectEx dao, IModel objectModel) where T : class, IModel, new()
        {
            return dao.QueryFromModel(objectModel, typeof(T), null, false, null);
        }
        public static AncestorResult QueryNoRowid(this IDataAccessObjectEx dao, IModel objectModel)
        {
            return dao.QueryFromModel(objectModel, null, null, false, new AncestorOptions { HasRowId = false });
        }
        public static AncestorResult QueryNoRowid<T>(this IDataAccessObjectEx dao, IModel objectModel) where T : class, IModel, new()
        {
            return dao.QueryFromModel(objectModel, typeof(T), null, false, new AncestorOptions { HasRowId = false });
        }

        public static AncestorResult Query<T>(this IDataAccessObjectEx dao, Expression<Func<T, bool>> predicate) where T : class, new()
        {
            return dao.QueryFromLambda(predicate, null, null, false, null);
        }
        public static AncestorResult Query<T>(this IDataAccessObjectEx dao, Expression<Func<T, bool>> predicate, Expression<Func<T, object>> selectCondition) where T : class, new()
        {
            return dao.QueryFromLambda(predicate, selectCondition, null, false, null);
        }
        public static AncestorResult QueryNoRowid<T>(this IDataAccessObjectEx dao, Expression<Func<T, bool>> predicate) where T : class, new()
        {
            return dao.QueryFromLambda(predicate, null, null, false, new AncestorOptions { HasRowId = false });
        }
        public static AncestorResult Query<T1, T2>(this IDataAccessObjectEx dao, Expression<Func<T1, T2, bool>> predicate, Expression<Func<T1, T2, object>> selectCondition) where T1 : class, new() where T2 : class, new()
        {
            return dao.QueryFromLambda(predicate, selectCondition, null, false, null);
        }
        public static AncestorResult Query<T1, T2, T3>(this IDataAccessObjectEx dao, Expression<Func<T1, T2, T3, bool>> predicate, Expression<Func<T1, T2, T3, object>> selectCondition) where T1 : class, new() where T2 : class, new() where T3 : class, new()
        {
            return dao.QueryFromLambda(predicate, selectCondition, null, false, null);
        }
        public static AncestorResult Query<T1, T2, T3, T4>(this IDataAccessObjectEx dao, Expression<Func<T1, T2, T3, T4, bool>> predicate, Expression<Func<T1, T2, T3, T4, object>> selectCondition) where T1 : class, new() where T2 : class, new() where T3 : class, new() where T4 : class, new()
        {
            return dao.QueryFromLambda(predicate, selectCondition, null, false, null);
        }
        public static AncestorResult Query<T1, T2, T3, T4, T5>(this IDataAccessObjectEx dao, Expression<Func<T1, T2, T3, T4, T5, bool>> predicate, Expression<Func<T1, T2, T3, T4, T5, object>> selectCondition) where T1 : class, new() where T2 : class, new() where T3 : class, new() where T4 : class, new() where T5 : class, new()
        {
            return dao.QueryFromLambda(predicate, selectCondition, null, false, null);
        }
        public static AncestorResult Query<T1, T2, T3, T4, T5, T6>(this IDataAccessObjectEx dao, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate, Expression<Func<T1, T2, T3, T4, T5, T6, object>> selectCondition) where T1 : class, new() where T2 : class, new() where T3 : class, new() where T4 : class, new() where T5 : class, new() where T6 : class, new()
        {
            return dao.QueryFromLambda(predicate, selectCondition, null, false, null);
        }
        public static AncestorResult Query<FakeType>(this IDataAccessObjectEx dao, Expression<Func<FakeType, bool>> predicate, Type realType) where FakeType : class, new()
        {
            var map = CreateProxyMap(CreateTuple(typeof(FakeType), realType));
            return dao.QueryFromLambda(predicate, null, map, false, null);
        }
        public static AncestorResult Query<FakeType>(this IDataAccessObjectEx dao, Expression<Func<FakeType, bool>> predicate, Expression<Func<FakeType, object>> selectCondition, Type realType) where FakeType : class, new()
        {
            var map = CreateProxyMap(CreateTuple(typeof(FakeType), realType));
            return dao.QueryFromLambda(predicate, selectCondition, map, false, null);
        }
        public static AncestorResult Query<FakeType1, FakeType2>(this IDataAccessObjectEx dao, Expression<Func<FakeType1, FakeType2, bool>> predicate, Expression<Func<FakeType1, FakeType2, object>> selectCondition, Type realType1, Type realType2) where FakeType1 : class, new() where FakeType2 : class, new()
        {
            var map = CreateProxyMap(CreateTuple(typeof(FakeType1), realType1), CreateTuple(typeof(FakeType2), realType2));
            return dao.QueryFromLambda(predicate, selectCondition, map, false, null);
        }
        public static AncestorResult Query<FakeType1, FakeType2, FakeType3>(this IDataAccessObjectEx dao, Expression<Func<FakeType1, FakeType2, FakeType3, bool>> predicate, Expression<Func<FakeType1, FakeType2, FakeType3, object>> selectCondition, Type realType1, Type realType2, Type realType3) where FakeType1 : class, new() where FakeType2 : class, new() where FakeType3 : class, new()
        {
            var map = CreateProxyMap(CreateTuple(typeof(FakeType1), realType1), CreateTuple(typeof(FakeType2), realType2), CreateTuple(typeof(FakeType3), realType3));
            return dao.QueryFromLambda(predicate, selectCondition, map, false, null);
        }
        public static AncestorResult Query<FakeType1, FakeType2, FakeType3, FakeType4>(this IDataAccessObjectEx dao, Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, bool>> predicate, Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, object>> selectCondition, Type realType1, Type realType2, Type realType3, Type realType4) where FakeType1 : class, new() where FakeType2 : class, new() where FakeType3 : class, new() where FakeType4 : class, new()
        {
            var map = CreateProxyMap(CreateTuple(typeof(FakeType1), realType1), CreateTuple(typeof(FakeType2), realType2), CreateTuple(typeof(FakeType3), realType3), CreateTuple(typeof(FakeType4), realType4));
            return dao.QueryFromLambda(predicate, selectCondition, map, false, null);
        }
        public static AncestorResult Query<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5>(this IDataAccessObjectEx dao, Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, bool>> predicate, Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, object>> selectCondition, Type realType1, Type realType2, Type realType3, Type realType4, Type realType5) where FakeType1 : class, new() where FakeType2 : class, new() where FakeType3 : class, new() where FakeType4 : class, new() where FakeType5 : class, new()
        {
            var map = CreateProxyMap(CreateTuple(typeof(FakeType1), realType1), CreateTuple(typeof(FakeType2), realType2), CreateTuple(typeof(FakeType3), realType3), CreateTuple(typeof(FakeType4), realType4), CreateTuple(typeof(FakeType5), realType5));
            return dao.QueryFromLambda(predicate, selectCondition, map, false, null);
        }
        public static AncestorResult Query<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, FakeType6>(this IDataAccessObjectEx dao, Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, FakeType6, bool>> predicate, Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, FakeType6, object>> selectCondition, Type realType1, Type realType2, Type realType3, Type realType4, Type realType5, Type realType6) where FakeType1 : class, new() where FakeType2 : class, new() where FakeType3 : class, new() where FakeType4 : class, new() where FakeType5 : class, new() where FakeType6 : class, new()
        {
            var map = CreateProxyMap(CreateTuple(typeof(FakeType1), realType1), CreateTuple(typeof(FakeType2), realType2), CreateTuple(typeof(FakeType3), realType3), CreateTuple(typeof(FakeType4), realType4), CreateTuple(typeof(FakeType5), realType5), CreateTuple(typeof(FakeType6), realType6));
            return dao.QueryFromLambda(predicate, selectCondition, map, false, null);
        }
        public static AncestorResult Query<FakeType>(this IDataAccessObjectEx dao, Expression<Func<FakeType, bool>> predicate, string name) where FakeType : class, new()
        {
            var map = CreateProxyMap(CreateTuple(typeof(FakeType), name));
            return dao.QueryFromLambda(predicate, null, map, false, null);
        }
        public static AncestorResult Query<FakeType>(this IDataAccessObjectEx dao, Expression<Func<FakeType, bool>> predicate, Expression<Func<FakeType, object>> selectCondition, string name) where FakeType : class, new()
        {
            var map = CreateProxyMap(CreateTuple(typeof(FakeType), name));
            return dao.QueryFromLambda(predicate, selectCondition, map, false, null);
        }
        public static AncestorResult Query<FakeType1, FakeType2>(this IDataAccessObjectEx dao, Expression<Func<FakeType1, FakeType2, bool>> predicate, Expression<Func<FakeType1, FakeType2, object>> selectCondition, string name1, string name2) where FakeType1 : class, new() where FakeType2 : class, new()
        {
            var map = CreateProxyMap(CreateTuple(typeof(FakeType1), name1), CreateTuple(typeof(FakeType2), name2));
            return dao.QueryFromLambda(predicate, selectCondition, map, false, null);
        }
        public static AncestorResult Query<FakeType1, FakeType2, FakeType3>(this IDataAccessObjectEx dao, Expression<Func<FakeType1, FakeType2, FakeType3, bool>> predicate, Expression<Func<FakeType1, FakeType2, FakeType3, object>> selectCondition, string name1, string name2, string name3) where FakeType1 : class, new() where FakeType2 : class, new() where FakeType3 : class, new()
        {
            var map = CreateProxyMap(CreateTuple(typeof(FakeType1), name1), CreateTuple(typeof(FakeType2), name2), CreateTuple(typeof(FakeType3), name3));
            return dao.QueryFromLambda(predicate, selectCondition, map, false, null);
        }
        public static AncestorResult Query<FakeType1, FakeType2, FakeType3, FakeType4>(this IDataAccessObjectEx dao, Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, bool>> predicate, Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, object>> selectCondition, string name1, string name2, string name3, string name4) where FakeType1 : class, new() where FakeType2 : class, new() where FakeType3 : class, new() where FakeType4 : class, new()
        {
            var map = CreateProxyMap(CreateTuple(typeof(FakeType1), name1), CreateTuple(typeof(FakeType2), name2), CreateTuple(typeof(FakeType3), name3), CreateTuple(typeof(FakeType4), name4));
            return dao.QueryFromLambda(predicate, selectCondition, map, false, null);
        }
        public static AncestorResult Query<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5>(this IDataAccessObjectEx dao, Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, bool>> predicate, Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, object>> selectCondition, string name1, string name2, string name3, string name4, string name5) where FakeType1 : class, new() where FakeType2 : class, new() where FakeType3 : class, new() where FakeType4 : class, new() where FakeType5 : class, new()
        {
            var map = CreateProxyMap(CreateTuple(typeof(FakeType1), name1), CreateTuple(typeof(FakeType2), name2), CreateTuple(typeof(FakeType3), name3), CreateTuple(typeof(FakeType4), name4), CreateTuple(typeof(FakeType5), name5));
            return dao.QueryFromLambda(predicate, selectCondition, map, false, null);
        }
        public static AncestorResult Query<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, FakeType6>(this IDataAccessObjectEx dao, Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, FakeType6, bool>> predicate, Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, FakeType6, object>> selectCondition, string name1, string name2, string name3, string name4, string name5, string name6) where FakeType1 : class, new() where FakeType2 : class, new() where FakeType3 : class, new() where FakeType4 : class, new() where FakeType5 : class, new() where FakeType6 : class, new()
        {
            var map = CreateProxyMap(CreateTuple(typeof(FakeType1), name1), CreateTuple(typeof(FakeType2), name2), CreateTuple(typeof(FakeType3), name3), CreateTuple(typeof(FakeType4), name4), CreateTuple(typeof(FakeType5), name5), CreateTuple(typeof(FakeType6), name6));
            return dao.QueryFromLambda(predicate, selectCondition, map, false, null);
        }


        public static AncestorResult Insert(this IDataAccessObjectEx dao, IModel objectModel)
        {
            return dao.InsertEntity(objectModel, null, null);
        }
        public static AncestorResult Update(this IDataAccessObjectEx dao, IModel valueObject, object paramsObjects)
        {
            return dao.UpdateEntity(valueObject, paramsObjects, UpdateMode.Value, null, null);
        }
        public static AncestorResult Update(this IDataAccessObjectEx dao, IModel valueObject, IModel whereObject)
        {
            return dao.UpdateEntity(valueObject, whereObject, UpdateMode.Value, null, null);
        }
        public static AncestorResult Update<T>(this IDataAccessObjectEx dao, IModel valueObject, Expression<Func<T, bool>> predicate) where T : class, new()
        {
            return dao.UpdateEntity(valueObject, predicate, UpdateMode.Value, null, null);
        }
        public static AncestorResult UpdateAll(this IDataAccessObjectEx dao, IModel valueObject, IModel whereObject)
        {
            return dao.UpdateEntity(valueObject, whereObject, UpdateMode.All, null, null);
        }
        public static AncestorResult UpdateAll<T>(this IDataAccessObjectEx dao, IModel valueObject, Expression<Func<T, bool>> predicate) where T : class, new()
        {
            return dao.UpdateEntity(valueObject, predicate, UpdateMode.All, null, null);
        }
        public static AncestorResult Delete(this IDataAccessObjectEx dao, IModel whereObject)
        {
            return dao.DeleteEntity(whereObject, null, null);
        }
        public static AncestorResult Delete<T>(this IDataAccessObjectEx dao, Expression<Func<T, bool>> predicate) where T : class, new()
        {
            return dao.DeleteEntity(predicate, null, null);
        }
        public static AncestorResult ExecuteNonQuery(this IDataAccessObjectEx dao, string sqlString, object modelObject)
        {
            return dao.ExecuteNonQuery(sqlString, modelObject, null);
        }
        public static AncestorResult ExecuteStoredProcedure(this IDataAccessObjectEx dao, string procedureName, bool bindbyName, List<DBParameter> dBParameter)
        {
            return dao.ExecuteStoredProcedure(procedureName, dBParameter, new AncestorOptions { BindByName = bindbyName });
        }

        public static AncestorResult Insert(this IDataAccessObjectEx dao, IModel model, string name)
        {
            return dao.InsertEntity(model, name, null);
        }
        public static AncestorResult Update(this IDataAccessObjectEx dao, IModel valueObject, object paramsObjects, string name)
        {
            return dao.UpdateEntity(valueObject, paramsObjects, UpdateMode.Value, name, null);
        }
        public static AncestorResult Update(this IDataAccessObjectEx dao, IModel valueObject, IModel whereObject, string name)
        {
            return dao.UpdateEntity(valueObject, whereObject, UpdateMode.Value, name, null);
        }
        public static AncestorResult Update<T>(this IDataAccessObjectEx dao, IModel valueObject, Expression<Func<T, bool>> predicate, string name) where T : class, new()
        {
            return dao.UpdateEntity(valueObject, predicate, UpdateMode.Value, name, null);
        }
        public static AncestorResult UpdateAll(this IDataAccessObjectEx dao, IModel valueObject, IModel whereObject, string name)
        {
            return dao.UpdateEntity(valueObject, whereObject, UpdateMode.All, name, null);
        }
        public static AncestorResult UpdateAll<T>(this IDataAccessObjectEx dao, IModel valueObject, Expression<Func<T, bool>> predicate, string name) where T : class, new()
        {
            return dao.UpdateEntity(valueObject, predicate, UpdateMode.All, name, null);
        }
        public static AncestorResult Delete(this IDataAccessObjectEx dao, IModel whereObject, string name)
        {
            return dao.DeleteEntity(whereObject, name, null);
        }
        public static AncestorResult Delete<T>(this IDataAccessObjectEx dao, Expression<Func<T, bool>> predicate, string name) where T : class, new()
        {
            return dao.DeleteEntity(predicate, name, null);
        }

        public static AncestorResult BulkInsert<T>(this IDataAccessObjectEx dao, List<T> ObjList) where T : class, IModel, new()
        {
            return dao.BulkInsertEntities(ObjList, null, null);
        }







        private static IDictionary<Type, object> CreateProxyMap(params Tuple<Type, object>[] args)
        {
            var map = new Dictionary<Type, object>();
            foreach (var e in args)
                map.Add(e.Item1, e.Item2);
            return map;
        }
        private static Tuple<Type, object> CreateTuple(Type type, object origin)
        {
            return Tuple.Create(type, origin);
        }
        #endregion

        public static AncestorResult QueryAll<T>(this IDataAccessObjectEx dao)
        {
            return dao.QueryFromModel(null, typeof(T), null, false, null);
        }
    }
}
