using Ancestor.Core;
using Ancestor.DataAccess.DAO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;

namespace System
{
    /// <summary>
    /// Extension methods for <see cref="IDataAccessObjectEx"/>
    /// </summary>
    public static class DataAccessObjectExtensions
    {
        #region Wrapper before 1.4.8
        //public static AncestorResult Query(this IDataAccessObjectEx dao, string sqlString, object paramsObjects)
        //{
        //    return dao.QueryFromSqlString(sqlString, paramsObjects, null, false, null);
        //}
        //public static AncestorResult Query(this IDataAccessObjectEx dao, IModel objectModel)
        //{
        //    return dao.QueryFromModel(objectModel, null, null, false, null);
        //}
        //public static AncestorResult Query<T>(this IDataAccessObjectEx dao, IModel objectModel) where T : class, IModel, new()
        //{
        //    return dao.QueryFromModel(objectModel, typeof(T), null, false, null);
        //}
        //public static AncestorResult QueryNoRowid(this IDataAccessObjectEx dao, IModel objectModel)
        //{
        //    return dao.QueryFromModel(objectModel, null, null, false, new AncestorOptions { HasRowId = false });
        //}
        //public static AncestorResult QueryNoRowid<T>(this IDataAccessObjectEx dao, IModel objectModel) where T : class, IModel, new()
        //{
        //    return dao.QueryFromModel(objectModel, typeof(T), null, false, new AncestorOptions { HasRowId = false });
        //}

        //public static AncestorResult Query<T>(this IDataAccessObjectEx dao, Expression<Func<T, bool>> predicate) where T : class, new()
        //{
        //    return dao.QueryFromLambda(predicate, null, null, false, null);
        //}
        //public static AncestorResult Query<T>(this IDataAccessObjectEx dao, Expression<Func<T, bool>> predicate, Expression<Func<T, object>> selectCondition) where T : class, new()
        //{
        //    return dao.QueryFromLambda(predicate, selectCondition, null, false, null);
        //}
        //public static AncestorResult QueryNoRowid<T>(this IDataAccessObjectEx dao, Expression<Func<T, bool>> predicate) where T : class, new()
        //{
        //    return dao.QueryFromLambda(predicate, null, null, false, new AncestorOptions { HasRowId = false });
        //}
        //public static AncestorResult Query<T1, T2>(this IDataAccessObjectEx dao, Expression<Func<T1, T2, bool>> predicate, Expression<Func<T1, T2, object>> selectCondition) where T1 : class, new() where T2 : class, new()
        //{
        //    return dao.QueryFromLambda(predicate, selectCondition, null, false, null);
        //}
        //public static AncestorResult Query<T1, T2, T3>(this IDataAccessObjectEx dao, Expression<Func<T1, T2, T3, bool>> predicate, Expression<Func<T1, T2, T3, object>> selectCondition) where T1 : class, new() where T2 : class, new() where T3 : class, new()
        //{
        //    return dao.QueryFromLambda(predicate, selectCondition, null, false, null);
        //}
        //public static AncestorResult Query<T1, T2, T3, T4>(this IDataAccessObjectEx dao, Expression<Func<T1, T2, T3, T4, bool>> predicate, Expression<Func<T1, T2, T3, T4, object>> selectCondition) where T1 : class, new() where T2 : class, new() where T3 : class, new() where T4 : class, new()
        //{
        //    return dao.QueryFromLambda(predicate, selectCondition, null, false, null);
        //}
        //public static AncestorResult Query<T1, T2, T3, T4, T5>(this IDataAccessObjectEx dao, Expression<Func<T1, T2, T3, T4, T5, bool>> predicate, Expression<Func<T1, T2, T3, T4, T5, object>> selectCondition) where T1 : class, new() where T2 : class, new() where T3 : class, new() where T4 : class, new() where T5 : class, new()
        //{
        //    return dao.QueryFromLambda(predicate, selectCondition, null, false, null);
        //}
        //public static AncestorResult Query<T1, T2, T3, T4, T5, T6>(this IDataAccessObjectEx dao, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate, Expression<Func<T1, T2, T3, T4, T5, T6, object>> selectCondition) where T1 : class, new() where T2 : class, new() where T3 : class, new() where T4 : class, new() where T5 : class, new() where T6 : class, new()
        //{
        //    return dao.QueryFromLambda(predicate, selectCondition, null, false, null);
        //}
        //public static AncestorResult Query<FakeType>(this IDataAccessObjectEx dao, Expression<Func<FakeType, bool>> predicate, Type realType) where FakeType : class, new()
        //{
        //    var map = CreateProxyMap(CreateTuple(typeof(FakeType), realType));
        //    return dao.QueryFromLambda(predicate, null, map, false, null);
        //}
        //public static AncestorResult Query<FakeType>(this IDataAccessObjectEx dao, Expression<Func<FakeType, bool>> predicate, Expression<Func<FakeType, object>> selectCondition, Type realType) where FakeType : class, new()
        //{
        //    var map = CreateProxyMap(CreateTuple(typeof(FakeType), realType));
        //    return dao.QueryFromLambda(predicate, selectCondition, map, false, null);
        //}
        //public static AncestorResult Query<FakeType1, FakeType2>(this IDataAccessObjectEx dao, Expression<Func<FakeType1, FakeType2, bool>> predicate, Expression<Func<FakeType1, FakeType2, object>> selectCondition, Type realType1, Type realType2) where FakeType1 : class, new() where FakeType2 : class, new()
        //{
        //    var map = CreateProxyMap(CreateTuple(typeof(FakeType1), realType1), CreateTuple(typeof(FakeType2), realType2));
        //    return dao.QueryFromLambda(predicate, selectCondition, map, false, null);
        //}
        //public static AncestorResult Query<FakeType1, FakeType2, FakeType3>(this IDataAccessObjectEx dao, Expression<Func<FakeType1, FakeType2, FakeType3, bool>> predicate, Expression<Func<FakeType1, FakeType2, FakeType3, object>> selectCondition, Type realType1, Type realType2, Type realType3) where FakeType1 : class, new() where FakeType2 : class, new() where FakeType3 : class, new()
        //{
        //    var map = CreateProxyMap(CreateTuple(typeof(FakeType1), realType1), CreateTuple(typeof(FakeType2), realType2), CreateTuple(typeof(FakeType3), realType3));
        //    return dao.QueryFromLambda(predicate, selectCondition, map, false, null);
        //}
        //public static AncestorResult Query<FakeType1, FakeType2, FakeType3, FakeType4>(this IDataAccessObjectEx dao, Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, bool>> predicate, Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, object>> selectCondition, Type realType1, Type realType2, Type realType3, Type realType4) where FakeType1 : class, new() where FakeType2 : class, new() where FakeType3 : class, new() where FakeType4 : class, new()
        //{
        //    var map = CreateProxyMap(CreateTuple(typeof(FakeType1), realType1), CreateTuple(typeof(FakeType2), realType2), CreateTuple(typeof(FakeType3), realType3), CreateTuple(typeof(FakeType4), realType4));
        //    return dao.QueryFromLambda(predicate, selectCondition, map, false, null);
        //}
        //public static AncestorResult Query<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5>(this IDataAccessObjectEx dao, Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, bool>> predicate, Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, object>> selectCondition, Type realType1, Type realType2, Type realType3, Type realType4, Type realType5) where FakeType1 : class, new() where FakeType2 : class, new() where FakeType3 : class, new() where FakeType4 : class, new() where FakeType5 : class, new()
        //{
        //    var map = CreateProxyMap(CreateTuple(typeof(FakeType1), realType1), CreateTuple(typeof(FakeType2), realType2), CreateTuple(typeof(FakeType3), realType3), CreateTuple(typeof(FakeType4), realType4), CreateTuple(typeof(FakeType5), realType5));
        //    return dao.QueryFromLambda(predicate, selectCondition, map, false, null);
        //}
        //public static AncestorResult Query<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, FakeType6>(this IDataAccessObjectEx dao, Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, FakeType6, bool>> predicate, Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, FakeType6, object>> selectCondition, Type realType1, Type realType2, Type realType3, Type realType4, Type realType5, Type realType6) where FakeType1 : class, new() where FakeType2 : class, new() where FakeType3 : class, new() where FakeType4 : class, new() where FakeType5 : class, new() where FakeType6 : class, new()
        //{
        //    var map = CreateProxyMap(CreateTuple(typeof(FakeType1), realType1), CreateTuple(typeof(FakeType2), realType2), CreateTuple(typeof(FakeType3), realType3), CreateTuple(typeof(FakeType4), realType4), CreateTuple(typeof(FakeType5), realType5), CreateTuple(typeof(FakeType6), realType6));
        //    return dao.QueryFromLambda(predicate, selectCondition, map, false, null);
        //}
        //public static AncestorResult Query<FakeType>(this IDataAccessObjectEx dao, Expression<Func<FakeType, bool>> predicate, string name) where FakeType : class, new()
        //{
        //    var map = CreateProxyMap(CreateTuple(typeof(FakeType), name));
        //    return dao.QueryFromLambda(predicate, null, map, false, null);
        //}
        //public static AncestorResult Query<FakeType>(this IDataAccessObjectEx dao, Expression<Func<FakeType, bool>> predicate, Expression<Func<FakeType, object>> selectCondition, string name) where FakeType : class, new()
        //{
        //    var map = CreateProxyMap(CreateTuple(typeof(FakeType), name));
        //    return dao.QueryFromLambda(predicate, selectCondition, map, false, null);
        //}
        //public static AncestorResult Query<FakeType1, FakeType2>(this IDataAccessObjectEx dao, Expression<Func<FakeType1, FakeType2, bool>> predicate, Expression<Func<FakeType1, FakeType2, object>> selectCondition, string name1, string name2) where FakeType1 : class, new() where FakeType2 : class, new()
        //{
        //    var map = CreateProxyMap(CreateTuple(typeof(FakeType1), name1), CreateTuple(typeof(FakeType2), name2));
        //    return dao.QueryFromLambda(predicate, selectCondition, map, false, null);
        //}
        //public static AncestorResult Query<FakeType1, FakeType2, FakeType3>(this IDataAccessObjectEx dao, Expression<Func<FakeType1, FakeType2, FakeType3, bool>> predicate, Expression<Func<FakeType1, FakeType2, FakeType3, object>> selectCondition, string name1, string name2, string name3) where FakeType1 : class, new() where FakeType2 : class, new() where FakeType3 : class, new()
        //{
        //    var map = CreateProxyMap(CreateTuple(typeof(FakeType1), name1), CreateTuple(typeof(FakeType2), name2), CreateTuple(typeof(FakeType3), name3));
        //    return dao.QueryFromLambda(predicate, selectCondition, map, false, null);
        //}
        //public static AncestorResult Query<FakeType1, FakeType2, FakeType3, FakeType4>(this IDataAccessObjectEx dao, Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, bool>> predicate, Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, object>> selectCondition, string name1, string name2, string name3, string name4) where FakeType1 : class, new() where FakeType2 : class, new() where FakeType3 : class, new() where FakeType4 : class, new()
        //{
        //    var map = CreateProxyMap(CreateTuple(typeof(FakeType1), name1), CreateTuple(typeof(FakeType2), name2), CreateTuple(typeof(FakeType3), name3), CreateTuple(typeof(FakeType4), name4));
        //    return dao.QueryFromLambda(predicate, selectCondition, map, false, null);
        //}
        //public static AncestorResult Query<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5>(this IDataAccessObjectEx dao, Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, bool>> predicate, Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, object>> selectCondition, string name1, string name2, string name3, string name4, string name5) where FakeType1 : class, new() where FakeType2 : class, new() where FakeType3 : class, new() where FakeType4 : class, new() where FakeType5 : class, new()
        //{
        //    var map = CreateProxyMap(CreateTuple(typeof(FakeType1), name1), CreateTuple(typeof(FakeType2), name2), CreateTuple(typeof(FakeType3), name3), CreateTuple(typeof(FakeType4), name4), CreateTuple(typeof(FakeType5), name5));
        //    return dao.QueryFromLambda(predicate, selectCondition, map, false, null);
        //}
        //public static AncestorResult Query<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, FakeType6>(this IDataAccessObjectEx dao, Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, FakeType6, bool>> predicate, Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, FakeType6, object>> selectCondition, string name1, string name2, string name3, string name4, string name5, string name6) where FakeType1 : class, new() where FakeType2 : class, new() where FakeType3 : class, new() where FakeType4 : class, new() where FakeType5 : class, new() where FakeType6 : class, new()
        //{
        //    var map = CreateProxyMap(CreateTuple(typeof(FakeType1), name1), CreateTuple(typeof(FakeType2), name2), CreateTuple(typeof(FakeType3), name3), CreateTuple(typeof(FakeType4), name4), CreateTuple(typeof(FakeType5), name5), CreateTuple(typeof(FakeType6), name6));
        //    return dao.QueryFromLambda(predicate, selectCondition, map, false, null);
        //}


        //public static AncestorResult Insert(this IDataAccessObjectEx dao, IModel objectModel)
        //{
        //    return dao.InsertEntity(objectModel, null, null);
        //}
        //public static AncestorResult Update(this IDataAccessObjectEx dao, IModel valueObject, object paramsObjects)
        //{
        //    return dao.UpdateEntity(valueObject, paramsObjects, UpdateMode.Value, null, null);
        //}
        //public static AncestorResult Update(this IDataAccessObjectEx dao, IModel valueObject, IModel whereObject)
        //{
        //    return dao.UpdateEntity(valueObject, whereObject, UpdateMode.Value, null, null);
        //}
        //public static AncestorResult Update<T>(this IDataAccessObjectEx dao, IModel valueObject, Expression<Func<T, bool>> predicate) where T : class, new()
        //{
        //    return dao.UpdateEntity(valueObject, predicate, UpdateMode.Value, null, null);
        //}
        //public static AncestorResult UpdateAll(this IDataAccessObjectEx dao, IModel valueObject, IModel whereObject)
        //{
        //    return dao.UpdateEntity(valueObject, whereObject, UpdateMode.All, null, null);
        //}
        //public static AncestorResult UpdateAll<T>(this IDataAccessObjectEx dao, IModel valueObject, Expression<Func<T, bool>> predicate) where T : class, new()
        //{
        //    return dao.UpdateEntity(valueObject, predicate, UpdateMode.All, null, null);
        //}
        //public static AncestorResult Delete(this IDataAccessObjectEx dao, IModel whereObject)
        //{
        //    return dao.DeleteEntity(whereObject, null, null);
        //}
        //public static AncestorResult Delete<T>(this IDataAccessObjectEx dao, Expression<Func<T, bool>> predicate) where T : class, new()
        //{
        //    return dao.DeleteEntity(predicate, null, null);
        //}
        //public static AncestorResult ExecuteNonQuery(this IDataAccessObjectEx dao, string sqlString, object modelObject)
        //{
        //    return dao.ExecuteNonQuery(sqlString, modelObject, null);
        //}
        //public static AncestorResult ExecuteStoredProcedure(this IDataAccessObjectEx dao, string procedureName, bool bindbyName, List<DBParameter> dBParameter)
        //{
        //    return dao.ExecuteStoredProcedure(procedureName, dBParameter, new AncestorOptions { BindByName = bindbyName });
        //}

        //public static AncestorResult Insert(this IDataAccessObjectEx dao, IModel model, string name)
        //{
        //    return dao.InsertEntity(model, name, null);
        //}
        //public static AncestorResult Update(this IDataAccessObjectEx dao, IModel valueObject, object paramsObjects, string name)
        //{
        //    return dao.UpdateEntity(valueObject, paramsObjects, UpdateMode.Value, name, null);
        //}
        //public static AncestorResult Update(this IDataAccessObjectEx dao, IModel valueObject, IModel whereObject, string name)
        //{
        //    return dao.UpdateEntity(valueObject, whereObject, UpdateMode.Value, name, null);
        //}
        //public static AncestorResult Update<T>(this IDataAccessObjectEx dao, IModel valueObject, Expression<Func<T, bool>> predicate, string name) where T : class, new()
        //{
        //    return dao.UpdateEntity(valueObject, predicate, UpdateMode.Value, name, null);
        //}
        //public static AncestorResult UpdateAll(this IDataAccessObjectEx dao, IModel valueObject, IModel whereObject, string name)
        //{
        //    return dao.UpdateEntity(valueObject, whereObject, UpdateMode.All, name, null);
        //}
        //public static AncestorResult UpdateAll<T>(this IDataAccessObjectEx dao, IModel valueObject, Expression<Func<T, bool>> predicate, string name) where T : class, new()
        //{
        //    return dao.UpdateEntity(valueObject, predicate, UpdateMode.All, name, null);
        //}
        //public static AncestorResult Delete(this IDataAccessObjectEx dao, IModel whereObject, string name)
        //{
        //    return dao.DeleteEntity(whereObject, name, null);
        //}
        //public static AncestorResult Delete<T>(this IDataAccessObjectEx dao, Expression<Func<T, bool>> predicate, string name) where T : class, new()
        //{
        //    return dao.DeleteEntity(predicate, name, null);
        //}

        //public static AncestorResult BulkInsert<T>(this IDataAccessObjectEx dao, List<T> ObjList) where T : class, IModel, new()
        //{
        //    return dao.BulkInsertEntities(ObjList, null, null);
        //}







        //private static IDictionary<Type, object> CreateProxyMap(params Tuple<Type, object>[] args)
        //{
        //    var map = new Dictionary<Type, object>();
        //    foreach (var e in args)
        //        map.Add(e.Item1, e.Item2);
        //    return map;
        //}
        //private static Tuple<Type, object> CreateTuple(Type type, object origin)
        //{
        //    return Tuple.Create(type, origin);
        //}
        #endregion

        #region Extensions 
        private static AncestorOptions CreateRowIdOption()
        {
            return new AncestorOptions { { "HasRowId", true } };
        }
        public static AncestorResult Query(this IDataAccessObjectEx dao, string sqlString, object paramsObjects)
        {
            return dao.QueryFromSqlString(sqlString, paramsObjects, null, false, null);
        }

        public static AncestorResult Query(this IDataAccessObjectEx dao, object objectModel)
        {
            if (objectModel is string)
                return dao.Query((string)objectModel, null);
            return dao.QueryFromModel(objectModel, null, null, false, null);
        }
        public static AncestorResult Query<T>(this IDataAccessObjectEx dao, object objectModel) where T : class, new()
        {
            return dao.QueryFromModel(objectModel, typeof(T), null, false, null);
        }
        public static AncestorResult QueryWithRowid(this IDataAccessObjectEx dao, object objectModel)
        {
            return dao.QueryFromModel(objectModel, null, null, false, new AncestorOptions { { "AddRowId", true } });
        }
        public static AncestorResult QueryWithRowid<T>(this IDataAccessObjectEx dao, object objectModel) where T : class, new()
        {
            return dao.QueryFromModel(objectModel,typeof(T), null, false, new AncestorOptions { { "AddRowId", true } });
        }
        public static AncestorResult Query<T>(this IDataAccessObjectEx dao, Expression<Func<T, bool>> predicate) where T : class, new()
        {
            return dao.QueryFromLambda(predicate, null, null, false, null);
        }
        public static AncestorResult Query<T>(this IDataAccessObjectEx dao, Expression<Func<T, bool>> predicate, Expression<Func<T, object>> selectCondition) where T : class, new()
        {
            return dao.QueryFromLambda(predicate, selectCondition, null, false, null);
        }
        public static AncestorResult QueryWithRowid<T>(this IDataAccessObjectEx dao, Expression<Func<T, bool>> predicate) where T : class, new()
        {
            return dao.QueryFromLambda(predicate, null, null, false, new AncestorOptions { { "AddRowId", true } });
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

        public static AncestorResult QueryFirst(this IDataAccessObjectEx dao, string sqlString, object paramsObjects)
        {
            return dao.QueryFromSqlString(sqlString, paramsObjects, null, true, null);
        }

        public static AncestorResult QueryFirst(this IDataAccessObjectEx dao, object objectModel)
        {
            return dao.QueryFromModel(objectModel, null, null, true, null);
        }
        public static AncestorResult QueryFirst<T>(this IDataAccessObjectEx dao, object objectModel) where T : class, new()
        {
            return dao.QueryFromModel(objectModel,typeof(T), null, true, null);
        }
        public static AncestorResult QueryFirstWithRowid(this IDataAccessObjectEx dao, object objectModel)
        {
            return dao.QueryFromModel(objectModel, null, null, true, new AncestorOptions { { "AddRowId", true } });
        }
        public static AncestorResult QueryFirstWithRowid<T>(this IDataAccessObjectEx dao, object objectModel) where T : class, new()
        {
            return dao.QueryFromModel(objectModel, typeof(T), null, true, new AncestorOptions { { "AddRowId", true } });
        }

        public static AncestorResult QueryFirst<T>(this IDataAccessObjectEx dao, Expression<Func<T, bool>> predicate) where T : class, new()
        {
            return dao.QueryFromLambda(predicate, null, null, true, null);
        }
        public static AncestorResult QueryFirst<T>(this IDataAccessObjectEx dao, Expression<Func<T, bool>> predicate, Expression<Func<T, object>> selectCondition) where T : class, new()
        {
            return dao.QueryFromLambda(predicate, selectCondition, null, true, null);
        }
        public static AncestorResult QueryFirstWithRowid<T>(this IDataAccessObjectEx dao, Expression<Func<T, bool>> predicate) where T : class, new()
        {
            return dao.QueryFromLambda(predicate, null, null, true, new AncestorOptions { { "AddRowId", true } });
        }
        public static AncestorResult QueryFirst<T1, T2>(this IDataAccessObjectEx dao, Expression<Func<T1, T2, bool>> predicate, Expression<Func<T1, T2, object>> selectCondition) where T1 : class, new() where T2 : class, new()
        {
            return dao.QueryFromLambda(predicate, selectCondition, null, true, null);
        }
        public static AncestorResult QueryFirst<T1, T2, T3>(this IDataAccessObjectEx dao, Expression<Func<T1, T2, T3, bool>> predicate, Expression<Func<T1, T2, T3, object>> selectCondition) where T1 : class, new() where T2 : class, new() where T3 : class, new()
        {
            return dao.QueryFromLambda(predicate, selectCondition, null, true, null);
        }
        public static AncestorResult QueryFirst<T1, T2, T3, T4>(this IDataAccessObjectEx dao, Expression<Func<T1, T2, T3, T4, bool>> predicate, Expression<Func<T1, T2, T3, T4, object>> selectCondition) where T1 : class, new() where T2 : class, new() where T3 : class, new() where T4 : class, new()
        {
            return dao.QueryFromLambda(predicate, selectCondition, null, true, null);
        }
        public static AncestorResult QueryFirst<T1, T2, T3, T4, T5>(this IDataAccessObjectEx dao, Expression<Func<T1, T2, T3, T4, T5, bool>> predicate, Expression<Func<T1, T2, T3, T4, T5, object>> selectCondition) where T1 : class, new() where T2 : class, new() where T3 : class, new() where T4 : class, new() where T5 : class, new()
        {
            return dao.QueryFromLambda(predicate, selectCondition, null, true, null);
        }
        public static AncestorResult QueryFirst<T1, T2, T3, T4, T5, T6>(this IDataAccessObjectEx dao, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate, Expression<Func<T1, T2, T3, T4, T5, T6, object>> selectCondition) where T1 : class, new() where T2 : class, new() where T3 : class, new() where T4 : class, new() where T5 : class, new() where T6 : class, new()
        {
            return dao.QueryFromLambda(predicate, selectCondition, null, true, null);
        }
        public static AncestorResult QueryFirst<FakeType>(this IDataAccessObjectEx dao, Expression<Func<FakeType, bool>> predicate, Type realType) where FakeType : class, new()
        {
            var map = CreateProxyMap(CreateTuple(typeof(FakeType), realType));
            return dao.QueryFromLambda(predicate, null, map, true, null);
        }
        public static AncestorResult QueryFirst<FakeType>(this IDataAccessObjectEx dao, Expression<Func<FakeType, bool>> predicate, Expression<Func<FakeType, object>> selectCondition, Type realType) where FakeType : class, new()
        {
            var map = CreateProxyMap(CreateTuple(typeof(FakeType), realType));
            return dao.QueryFromLambda(predicate, selectCondition, map, true, null);
        }
        public static AncestorResult QueryFirst<FakeType1, FakeType2>(this IDataAccessObjectEx dao, Expression<Func<FakeType1, FakeType2, bool>> predicate, Expression<Func<FakeType1, FakeType2, object>> selectCondition, Type realType1, Type realType2) where FakeType1 : class, new() where FakeType2 : class, new()
        {
            var map = CreateProxyMap(CreateTuple(typeof(FakeType1), realType1), CreateTuple(typeof(FakeType2), realType2));
            return dao.QueryFromLambda(predicate, selectCondition, map, true, null);
        }
        public static AncestorResult QueryFirst<FakeType1, FakeType2, FakeType3>(this IDataAccessObjectEx dao, Expression<Func<FakeType1, FakeType2, FakeType3, bool>> predicate, Expression<Func<FakeType1, FakeType2, FakeType3, object>> selectCondition, Type realType1, Type realType2, Type realType3) where FakeType1 : class, new() where FakeType2 : class, new() where FakeType3 : class, new()
        {
            var map = CreateProxyMap(CreateTuple(typeof(FakeType1), realType1), CreateTuple(typeof(FakeType2), realType2), CreateTuple(typeof(FakeType3), realType3));
            return dao.QueryFromLambda(predicate, selectCondition, map, true, null);
        }
        public static AncestorResult QueryFirst<FakeType1, FakeType2, FakeType3, FakeType4>(this IDataAccessObjectEx dao, Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, bool>> predicate, Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, object>> selectCondition, Type realType1, Type realType2, Type realType3, Type realType4) where FakeType1 : class, new() where FakeType2 : class, new() where FakeType3 : class, new() where FakeType4 : class, new()
        {
            var map = CreateProxyMap(CreateTuple(typeof(FakeType1), realType1), CreateTuple(typeof(FakeType2), realType2), CreateTuple(typeof(FakeType3), realType3), CreateTuple(typeof(FakeType4), realType4));
            return dao.QueryFromLambda(predicate, selectCondition, map, true, null);
        }
        public static AncestorResult QueryFirst<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5>(this IDataAccessObjectEx dao, Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, bool>> predicate, Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, object>> selectCondition, Type realType1, Type realType2, Type realType3, Type realType4, Type realType5) where FakeType1 : class, new() where FakeType2 : class, new() where FakeType3 : class, new() where FakeType4 : class, new() where FakeType5 : class, new()
        {
            var map = CreateProxyMap(CreateTuple(typeof(FakeType1), realType1), CreateTuple(typeof(FakeType2), realType2), CreateTuple(typeof(FakeType3), realType3), CreateTuple(typeof(FakeType4), realType4), CreateTuple(typeof(FakeType5), realType5));
            return dao.QueryFromLambda(predicate, selectCondition, map, true, null);
        }
        public static AncestorResult QueryFirst<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, FakeType6>(this IDataAccessObjectEx dao, Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, FakeType6, bool>> predicate, Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, FakeType6, object>> selectCondition, Type realType1, Type realType2, Type realType3, Type realType4, Type realType5, Type realType6) where FakeType1 : class, new() where FakeType2 : class, new() where FakeType3 : class, new() where FakeType4 : class, new() where FakeType5 : class, new() where FakeType6 : class, new()
        {
            var map = CreateProxyMap(CreateTuple(typeof(FakeType1), realType1), CreateTuple(typeof(FakeType2), realType2), CreateTuple(typeof(FakeType3), realType3), CreateTuple(typeof(FakeType4), realType4), CreateTuple(typeof(FakeType5), realType5), CreateTuple(typeof(FakeType6), realType6));
            return dao.QueryFromLambda(predicate, selectCondition, map, true, null);
        }
        public static AncestorResult QueryFirst<FakeType>(this IDataAccessObjectEx dao, Expression<Func<FakeType, bool>> predicate, string name) where FakeType : class, new()
        {
            var map = CreateProxyMap(CreateTuple(typeof(FakeType), name));
            return dao.QueryFromLambda(predicate, null, map, true, null);
        }
        public static AncestorResult QueryFirst<FakeType>(this IDataAccessObjectEx dao, Expression<Func<FakeType, bool>> predicate, Expression<Func<FakeType, object>> selectCondition, string name) where FakeType : class, new()
        {
            var map = CreateProxyMap(CreateTuple(typeof(FakeType), name));
            return dao.QueryFromLambda(predicate, selectCondition, map, true, null);
        }
        public static AncestorResult QueryFirst<FakeType1, FakeType2>(this IDataAccessObjectEx dao, Expression<Func<FakeType1, FakeType2, bool>> predicate, Expression<Func<FakeType1, FakeType2, object>> selectCondition, string name1, string name2) where FakeType1 : class, new() where FakeType2 : class, new()
        {
            var map = CreateProxyMap(CreateTuple(typeof(FakeType1), name1), CreateTuple(typeof(FakeType2), name2));
            return dao.QueryFromLambda(predicate, selectCondition, map, true, null);
        }
        public static AncestorResult QueryFirst<FakeType1, FakeType2, FakeType3>(this IDataAccessObjectEx dao, Expression<Func<FakeType1, FakeType2, FakeType3, bool>> predicate, Expression<Func<FakeType1, FakeType2, FakeType3, object>> selectCondition, string name1, string name2, string name3) where FakeType1 : class, new() where FakeType2 : class, new() where FakeType3 : class, new()
        {
            var map = CreateProxyMap(CreateTuple(typeof(FakeType1), name1), CreateTuple(typeof(FakeType2), name2), CreateTuple(typeof(FakeType3), name3));
            return dao.QueryFromLambda(predicate, selectCondition, map, true, null);
        }
        public static AncestorResult QueryFirst<FakeType1, FakeType2, FakeType3, FakeType4>(this IDataAccessObjectEx dao, Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, bool>> predicate, Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, object>> selectCondition, string name1, string name2, string name3, string name4) where FakeType1 : class, new() where FakeType2 : class, new() where FakeType3 : class, new() where FakeType4 : class, new()
        {
            var map = CreateProxyMap(CreateTuple(typeof(FakeType1), name1), CreateTuple(typeof(FakeType2), name2), CreateTuple(typeof(FakeType3), name3), CreateTuple(typeof(FakeType4), name4));
            return dao.QueryFromLambda(predicate, selectCondition, map, true, null);
        }
        public static AncestorResult QueryFirst<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5>(this IDataAccessObjectEx dao, Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, bool>> predicate, Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, object>> selectCondition, string name1, string name2, string name3, string name4, string name5) where FakeType1 : class, new() where FakeType2 : class, new() where FakeType3 : class, new() where FakeType4 : class, new() where FakeType5 : class, new()
        {
            var map = CreateProxyMap(CreateTuple(typeof(FakeType1), name1), CreateTuple(typeof(FakeType2), name2), CreateTuple(typeof(FakeType3), name3), CreateTuple(typeof(FakeType4), name4), CreateTuple(typeof(FakeType5), name5));
            return dao.QueryFromLambda(predicate, selectCondition, map, true, null);
        }
        public static AncestorResult QueryFirst<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, FakeType6>(this IDataAccessObjectEx dao, Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, FakeType6, bool>> predicate, Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, FakeType6, object>> selectCondition, string name1, string name2, string name3, string name4, string name5, string name6) where FakeType1 : class, new() where FakeType2 : class, new() where FakeType3 : class, new() where FakeType4 : class, new() where FakeType5 : class, new() where FakeType6 : class, new()
        {
            var map = CreateProxyMap(CreateTuple(typeof(FakeType1), name1), CreateTuple(typeof(FakeType2), name2), CreateTuple(typeof(FakeType3), name3), CreateTuple(typeof(FakeType4), name4), CreateTuple(typeof(FakeType5), name5), CreateTuple(typeof(FakeType6), name6));
            return dao.QueryFromLambda(predicate, selectCondition, map, true, null);
        }


        public static AncestorExecuteResult Insert(this IDataAccessObjectEx dao, object objectModel)
        {
            return dao.InsertEntity(objectModel, null, null);
        }
        public static AncestorExecuteResult Update(this IDataAccessObjectEx dao, object valueObject, object paramsObjects, int exceptRows = -1)
        {
            return dao.UpdateEntity(valueObject, paramsObjects, UpdateMode.Value, null, exceptRows, null);
        }
        public static AncestorExecuteResult Update<T>(this IDataAccessObjectEx dao, object valueObject, Expression<Func<T, bool>> predicate, int exceptRows = -1) where T : class, new()
        {
            return dao.UpdateEntity(valueObject, predicate, UpdateMode.Value, null, exceptRows, null);
        }
        public static AncestorExecuteResult UpdateAll(this IDataAccessObjectEx dao, object valueObject, object whereObject, int exceptRows = -1)
        {
            return dao.UpdateEntity(valueObject, whereObject, UpdateMode.All, null, exceptRows, null);
        }
        public static AncestorExecuteResult UpdateAll<T>(this IDataAccessObjectEx dao, object valueObject, Expression<Func<T, bool>> predicate, int exceptRows = -1) where T : class, new()
        {
            return dao.UpdateEntity(valueObject, predicate, UpdateMode.All, null, exceptRows, null);
        }
        public static AncestorExecuteResult Update<T>(this IDataAccessObjectEx dao, T model, object whereObject, T refModel, int exceptRows = -1) where T : class, new()
        {
            return dao.UpdateEntityRef(model, whereObject, refModel, null, exceptRows, null);
        }
        public static AncestorExecuteResult Update<T>(this IDataAccessObjectEx dao, T model, Expression<Func<T, bool>> predicate, T refModel, int exceptRows = -1) where T : class, new()
        {
            return dao.UpdateEntityRef(model, predicate, refModel, null, exceptRows, null);
        }
        public static AncestorExecuteResult Delete(this IDataAccessObjectEx dao, object whereObject, int exceptRows = -1)
        {
            return dao.DeleteEntity(whereObject, null, exceptRows, null);
        }
        public static AncestorExecuteResult Delete<T>(this IDataAccessObjectEx dao, Expression<Func<T, bool>> predicate, int exceptRows = -1) where T : class, new()
        {
            return dao.DeleteEntity(predicate, null, exceptRows, null);
        }
        public static AncestorExecuteResult ExecuteNonQuery(this IDataAccessObjectEx dao, string sqlString, object modelObject, int exceptRows = -1)
        {
            return dao.ExecuteNonQuery(sqlString, modelObject, exceptRows, null);
        }
        public static AncestorExecuteResult ExecuteStoredProcedure(this IDataAccessObjectEx dao, string procedureName, bool bindbyName, IEnumerable<DBParameter> parameters)
        {
            return dao.ExecuteStoredProcedure(procedureName, parameters, new AncestorOptions { { "BindByName", bindbyName } });
        }
        public static AncestorExecuteResult ExecuteStoredProcedure(this IDataAccessObjectEx dao, string procedureName, bool bindbyName, params DBParameter[] parameters)
        {
            return dao.ExecuteStoredProcedure(procedureName, parameters, new AncestorOptions { { "BindByName", bindbyName } });
        }
        public static AncestorExecuteResult ExecuteStoredProcedure(this IDataAccessObjectEx dao, string procedureName, object parameterObject, params DBParameter[] parameters)
        {
            return ExecuteStoredProcedure(dao, procedureName, parameterObject, null, (IEnumerable<DBParameter>)parameters);
        }
        public static AncestorExecuteResult ExecuteStoredProcedure(this IDataAccessObjectEx dao, string procedureName, object parameterObject, IEnumerable<DBParameter> parameters)
        {
            return ExecuteStoredProcedure(dao, procedureName, parameterObject, null, parameters);
        }
        public static AncestorExecuteResult ExecuteStoredProcedure(this IDataAccessObjectEx dao, string procedureName, object parameterObject, Func<string, string> nameResolver, params DBParameter[] parameters)
        {
            return ExecuteStoredProcedure(dao, procedureName, parameterObject, nameResolver, (IEnumerable<DBParameter>)parameters);
        }
        public static AncestorExecuteResult ExecuteStoredProcedure(this IDataAccessObjectEx dao, string procedureName, object parameterObject, Func<string, string> nameResolver, IEnumerable<DBParameter> parameters)
        {
            var internalDao = dao as IInternalDataAccessObject;
            if (internalDao == null)
                throw new InvalidOperationException("invalid internal dao type");
            var options = new AncestorOptions { { "BindByName", true }, { "NameResolver", nameResolver } };
            var @params = internalDao.CreateDBParameters(parameterObject, options);
            var extraParams = internalDao.CreateDBParameters(parameters, options);
            @params.AddRange(extraParams);
            return dao.ExecuteStoredProcedure(procedureName, @params, options);
        }
        public static AncestorExecuteResult Insert(this IDataAccessObjectEx dao, object model, string name)
        {
            return dao.InsertEntity(model, name, null);
        }
        public static AncestorExecuteResult Update(this IDataAccessObjectEx dao, object valueObject, object paramsObjects, string name, int exceptRows = -1)
        {
            return dao.UpdateEntity(valueObject, paramsObjects, UpdateMode.Value, name, exceptRows, null);
        }
        public static AncestorExecuteResult Update<T>(this IDataAccessObjectEx dao, object valueObject, Expression<Func<T, bool>> predicate, string name, int exceptRows = -1) where T : class, new()
        {
            return dao.UpdateEntity(valueObject, predicate, UpdateMode.Value, name, exceptRows, null);
        }
        public static AncestorExecuteResult UpdateAll(this IDataAccessObjectEx dao, object valueObject, object whereObject, string name, int exceptRows = -1)
        {
            return dao.UpdateEntity(valueObject, whereObject, UpdateMode.All, name, exceptRows, null);
        }
        public static AncestorExecuteResult UpdateAll<T>(this IDataAccessObjectEx dao, object valueObject, Expression<Func<T, bool>> predicate, string name, int exceptRows = -1) where T : class, new()
        {
            return dao.UpdateEntity(valueObject, predicate, UpdateMode.All, name, exceptRows, null);
        }
        public static AncestorExecuteResult Delete(this IDataAccessObjectEx dao, object whereObject, string name, int exceptRows = -1)
        {
            return dao.DeleteEntity(whereObject, name, exceptRows, null);
        }
        public static AncestorExecuteResult Delete<T>(this IDataAccessObjectEx dao, Expression<Func<T, bool>> predicate, string name, int exceptRows = -1) where T : class, new()
        {
            return dao.DeleteEntity(predicate, name, exceptRows, null);
        }

        public static AncestorExecuteResult Insert(this IDataAccessObjectEx dao, object model, Type realType)
        {
            return dao.InsertEntity(model, realType, null);
        }
        public static AncestorExecuteResult Update(this IDataAccessObjectEx dao, object valueObject, object paramsObjects, Type realType, int exceptRows = -1)
        {
            return dao.UpdateEntity(valueObject, paramsObjects, UpdateMode.Value, realType, exceptRows, null);
        }
        public static AncestorExecuteResult Update<T>(this IDataAccessObjectEx dao, object valueObject, Expression<Func<T, bool>> predicate, Type realType, int exceptRows = -1) where T : class, new()
        {
            return dao.UpdateEntity(valueObject, predicate, UpdateMode.Value, realType, exceptRows, null);
        }
        public static AncestorExecuteResult UpdateAll(this IDataAccessObjectEx dao, object valueObject, object whereObject, Type realType, int exceptRows = -1)
        {
            return dao.UpdateEntity(valueObject, whereObject, UpdateMode.All, realType, exceptRows, null);
        }
        public static AncestorExecuteResult UpdateAll<T>(this IDataAccessObjectEx dao, object valueObject, Expression<Func<T, bool>> predicate, Type realType, int exceptRows = -1) where T : class, new()
        {
            return dao.UpdateEntity(valueObject, predicate, UpdateMode.All, realType, exceptRows, null);
        }
        public static AncestorExecuteResult Delete(this IDataAccessObjectEx dao, object whereObject, Type realType, int exceptRows = -1)
        {
            return dao.DeleteEntity(whereObject, realType, exceptRows, null);
        }
        public static AncestorExecuteResult Delete<T>(this IDataAccessObjectEx dao, Expression<Func<T, bool>> predicate, Type realType, int exceptRows = -1) where T : class, new()
        {
            return dao.DeleteEntity(predicate, realType, exceptRows, null);
        }

        public static AncestorExecuteResult BulkInsert<T>(this IDataAccessObjectEx dao, List<T> ObjList) where T : class, new()
        {
            return dao.BulkInsertEntities(ObjList, null, null);
        }







        internal static IDictionary<Type, object> CreateProxyMap(params Tuple<Type, object>[] args)
        {
            var map = new Dictionary<Type, object>();
            foreach (var e in args)
                map.Add(e.Item1, e.Item2);
            return map;
        }
        internal static Tuple<Type, object> CreateTuple(Type type, object origin)
        {
            return Tuple.Create(type, origin);
        }
        #endregion

        public static AncestorResult QueryAll<T>(this IDataAccessObjectEx dao)
        {
            return dao.QueryFromModel(null, typeof(T), null, false, null);
        }
        public static AncestorResult QueryAll<T>(this IDataAccessObjectEx dao, string name)
        {
            return dao.QueryFromModel(null, typeof(T), name, false, null);
        }
        public static AncestorResult QueryAll<T>(this IDataAccessObjectEx dao, Type realType)
        {
            return dao.QueryFromModel(null, typeof(T), realType, false, null);
        }

        public static AncestorResult QueryAll<T>(this IDataAccessObjectEx dao, Expression<Func<T, object>> selector)
        {
            return dao.QueryFromLambda(null, selector, null, false, null);
        }
        public static AncestorResult QueryAll<T>(this IDataAccessObjectEx dao, Expression<Func<T, object>> selector, string name)
        {
            return dao.QueryFromLambda(null, selector, CreateProxyMap(CreateTuple(typeof(T), name)), false, null);
        }
        public static AncestorResult QueryAll<T>(this IDataAccessObjectEx dao, Expression<Func<T, object>> selector, Type realType)
        {
            return dao.QueryFromLambda(null, selector, CreateProxyMap(CreateTuple(typeof(T), realType)), false, null);
        }
        public static AncestorResult GroupFrom<T>(this IDataAccessObjectEx dao, Expression<Func<T, bool>> predicate, Expression<Func<T, object>> selector, Expression<Func<T, object>> groupBy)
        {
            return dao.GroupFromLambda(predicate, selector, groupBy, null, null);
        }
        public static AncestorResult GroupFrom<FakeType>(this IDataAccessObjectEx dao, Expression<Func<FakeType, bool>> predicate, Expression<Func<FakeType, object>> selector, Expression<Func<FakeType, object>> groupBy, Type realType) where FakeType : class, new()
        {
            var map = CreateProxyMap(CreateTuple(typeof(FakeType), realType));
            return dao.GroupFromLambda(predicate, selector, groupBy, map, null);
        }
        public static AncestorResult GroupFrom<FakeType>(this IDataAccessObjectEx dao, Expression<Func<FakeType, bool>> predicate, Expression<Func<FakeType, object>> selector, Expression<Func<FakeType, object>> groupBy, string name) where FakeType : class, new()
        {
            var map = CreateProxyMap(CreateTuple(typeof(FakeType), name));
            return dao.GroupFromLambda(predicate, selector, groupBy, map, null);
        }

        public static AncestorExecuteResult GetSequenceNextValue(this IDataAccessObjectEx dao, string name)
        {
            return dao.GetSequenceValue(name, true, null);
        }
        public static AncestorExecuteResult GetSequenceCurrentValue(this IDataAccessObjectEx dao, string name)
        {
            return dao.GetSequenceValue(name, false, null);
        }
    }
}
