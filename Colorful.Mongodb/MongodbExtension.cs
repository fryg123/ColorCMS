using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using System.Reflection.Emit;

namespace MongoDB.Driver
{
    #region Mongodb Database Helper
    /// <summary>
    /// 生成自增Id
    /// </summary>
    public class Ids
    {
        /// <summary>
        /// 表名
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 自增Id
        /// </summary>
        public long Index { get; set; }
    }
    public partial class MongodbContext : IDisposable
    {
        private MongoClient _client;
        private IMongoDatabase _db;
        private MongoCollectionSettings _collectionSettings;
        private static bool IsInit = false;

        #region 属性
        public IMongoDatabase Database
        {
            get
            {
                return _db;
            }
        }
        public MongoCollectionSettings CollectionSettings
        {
            get
            {
                return _collectionSettings;
            }
            set
            {
                _collectionSettings = value;
            }
        }
        #endregion

        #region 初始化
        public MongodbContext(string server, string username, string password, string dbName)
            : this(string.Format("mongodb://{1}:{2}@{0}/{3}", server, username, password), dbName)
        {
        }
        public MongodbContext(string connStr, string dbName)
        {
            if (!IsInit)
            {
                IsInit = true;
                BsonSerializer.RegisterSerializer(typeof(DateTime), DateTimeSerializer.LocalInstance);
            }
            this.Init();
            _collectionSettings = new MongoCollectionSettings();
            _client = new MongoClient(connStr);
            _db = _client.GetDatabase(dbName);
        }
        #endregion

        #region Virtual Method
        partial void Init();
        #endregion

        #region Public Method
        public long GetId(string tableName)
        {
            var idsCol = this.Database.GetCollection<BsonDocument>("Ids", this.CollectionSettings);
            var filter = Builders<BsonDocument>.Filter.Eq("_id", tableName);
            long inc = 1;
            var update = Builders<BsonDocument>.Update.Inc("Index", inc);
            var ids = idsCol.FindOneAndUpdate(filter, update, new FindOneAndUpdateOptions<BsonDocument, Ids>()
            {
                IsUpsert = true,
                ReturnDocument = ReturnDocument.After
            });
            return ids.Index;
        }
        public string GetObjectId()
        {
            return Bson.ObjectId.GenerateNewId().ToString();
        }
        public string GetUniqueId()
        {
            return ObjectId.GenerateNewId().ToString();
        }
        #endregion

        #region Dispose
        public void Dispose()
        {
            _collectionSettings = null;
            _client = null;
            _db = null;
        }
        #endregion
    }
    #endregion

    #region Mongodb Client Extension
    public static class IMongoCollectionExt
    {
        private delegate object CreateObject();
        private static Dictionary<Type, CreateObject> _constrcache = new Dictionary<Type, CreateObject>();

        #region 查询
        /// <summary>
        /// 获取自增Id
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static long GetId<TDocument>(this IMongoCollection<TDocument> collection)
        {
            var tableName = collection.CollectionNamespace.CollectionName;
            var idsCol = collection.Database.GetCollection<BsonDocument>("Ids", collection.Settings);
            var filter = Builders<BsonDocument>.Filter.Eq("_id", tableName);
            long inc = 1;
            var update = Builders<BsonDocument>.Update.Inc("Index", inc);
            var ids = idsCol.FindOneAndUpdate(filter, update, new FindOneAndUpdateOptions<BsonDocument, Ids>()
            {
                IsUpsert = true,
                ReturnDocument = ReturnDocument.After
            });
            return ids.Index;
        }
        /// <summary>
        /// 获取自增Id
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static int GetIntId<TDocument>(this IMongoCollection<TDocument> collection)
        {
            return (int)collection.GetId<TDocument>();
        }
        /// <summary>
        /// 获取最大Id值
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static long GetMaxId<TDocument>(this IMongoCollection<TDocument> doc, string field = "_id")
        {
            var collection = doc.Database.GetCollection<BsonDocument>(doc.CollectionNamespace.CollectionName, doc.Settings);
            var projection = Builders<BsonDocument>.Projection.Include(field);
            var data = collection.Find(new BsonDocument()).Sort("{" + field + ":-1}").Project(projection).FirstOrDefault();
            if (data == null)
                return 1;
            else
            {
                if (data[field] is MongoDB.Bson.BsonInt32)
                    return (int)data[field] + 1;
                else
                    return (long)data[field] + 1;
            }
        }
        /// <summary>
        /// 根据指定表达式获取Query对象
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <param name="doc"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static IQueryable<TDocument> Where<TDocument>(this IMongoCollection<TDocument> doc, Expression<Func<TDocument, bool>> filter)
        {
            return doc.AsQueryable().Where(filter);
        }
        /// <summary>
        /// 根据表达式获取指定字段
        /// </summary>
        /// <param name="expression">查询表达式</param>
        /// <param name="fields">需要返回的字段</param>
        /// <returns></returns>
        public static IFindFluent<TDocument, TDocument> Select<TDocument>(this IMongoCollection<TDocument> doc, Expression<Func<TDocument, bool>> filter, params string[] fields)
        {
            var projection = Builders<TDocument>.Projection.Include(fields[0]);
            for (var i = 1; i < fields.Length; i++)
                projection = projection.Include(fields[i]);
            return doc.Find(filter).Project<TDocument>(projection);
        }
        public static IQueryable<TResult> Select<TDocument, TResult>(this IMongoCollection<TDocument> doc, Expression<Func<TDocument, TResult>> selector)
        {
            return doc.AsQueryable().Select(selector);
        }
        /// <summary>
        /// 根据指定字段查询
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <param name="doc"></param>
        /// <param name="fields">需要返回的字段</param>
        /// <returns></returns>
        public static IFindFluent<TDocument, TDocument> Select<TDocument>(this IMongoCollection<TDocument> doc, params string[] fields)
        {
            var projection = Builders<TDocument>.Projection.Include(fields[0]);
            for (var i = 1; i < fields.Length; i++)
                projection = projection.Include(fields[i]);
            return doc.Find(new BsonDocument()).Project<TDocument>(projection);
        }
        /// <summary>
        /// 查询是否存在数据
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static bool Any<TDocument>(this IMongoCollection<TDocument> collection)
        {
            return collection.AsQueryable().Any();
        }
        /// <summary>
        /// 根据指向表达式查询是否存在数据
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <param name="collection"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static bool Any<TDocument>(this IMongoCollection<TDocument> collection, Expression<Func<TDocument, bool>> predicate)
        {
            return collection.AsQueryable().Any(predicate);
        }
        /// <summary>
        /// 根据指定字段按正序返回结果
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="collection"></param>
        /// <param name="keySelector"></param>
        /// <returns></returns>
        public static IOrderedQueryable<TDocument> OrderBy<TDocument, TKey>(this IMongoCollection<TDocument> collection, Expression<Func<TDocument, TKey>> keySelector)
        {
            return collection.AsQueryable().OrderBy(keySelector);
        }
        /// <summary>
        /// 根据指定字段按倒序返回结果
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="collection"></param>
        /// <param name="keySelector"></param>
        /// <returns></returns>
        public static IOrderedQueryable<TDocument> OrderByDescending<TDocument, TKey>(this IMongoCollection<TDocument> collection, Expression<Func<TDocument, TKey>> keySelector)
        {
            return collection.AsQueryable().OrderByDescending(keySelector);
        }
        /// <summary>
        /// 根据指定查询获取第一个查询对象
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static TDocument FirstOrDefault<TDocument>(this IMongoCollection<TDocument> collection, Expression<Func<TDocument, bool>> expression = null)
        {
            if (expression == null)
                return collection.AsQueryable().FirstOrDefault();
            else
                return collection.AsQueryable().Where(expression).FirstOrDefault();
        }
        public static List<TDocument> ToList<TDocument>(this IMongoCollection<TDocument> collection)
        {
            return collection.Find(new BsonDocument()).ToList();
        }
        //public static List<TDocument> FindAs<TDocument>(this IMongoCollection<TDocument> collection, Type type, Expression<Func<TDocument, bool>> filter = null)
        //{
        //    IFindFluent<TDocument, TDocument> result;
        //    if (filter == null)
        //        result = collection.Find(new BsonDocument());
        //    else
        //        result = collection.Find(filter);

        //    return collection.Find(new BsonDocument());
        //}

        public static IQueryable<TDocument> GetQuery<TDocument>(this IMongoCollection<TDocument> collection)
        {
            return collection.AsQueryable();
        }
        public static FilterDefinitionBuilder<TDocument> GetFilter<TDocument>(this IMongoCollection<TDocument> collection)
        {
            return Builders<TDocument>.Filter;
        }
        #endregion

        #region 聚合

        #region Max
        /// <summary>
        /// 获取指定字段的最大值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="memberExpression">字段</param>
        /// <returns></returns>
        public static TField Max<TDocument, TField>(this IMongoCollection<TDocument> collection, Expression<Func<TDocument, TField>> memberExpression)
        {
            return collection.AsQueryable().Max(memberExpression);
        }
        #endregion

        #region 合计 Sum
        /// <summary>
        /// 根据查询条件合计指定的字段
        /// </summary>
        /// <param name="query">查询条件</param>
        /// <param name="memberExpression">合计的字段表达式：a=>a.Money</param>
        /// <returns></returns>
        public static double Sum<TDocument>(this IMongoCollection<TDocument> collection, Expression<Func<TDocument, bool>> expression, Expression<Func<TDocument, double>> memberExpression)
        {
            return collection.AsQueryable().Where(expression).Sum(memberExpression);
        }
        public static decimal Sum<TDocument>(this IMongoCollection<TDocument> collection, Expression<Func<TDocument, bool>> expression, Expression<Func<TDocument, decimal>> memberExpression)
        {
            return collection.AsQueryable().Where(expression).Sum(memberExpression);
        }
        #endregion

        #region Group
        static void GetMatchs(string input, int startIndex, List<string> items)
        {
            var index = input.IndexOf("{\"$match\"", startIndex);
            var end = input.IndexOf("{\"$match\"", index + 1);
            string item;
            if (end == -1)
            {
                item = input.Substring(index);
                items.Add(item);
                return;
            }
            else {
                item = input.Substring(index, end - index);
                if (item.EndsWith(","))
                    item = item.Substring(0, item.Length - 1);
                items.Add(item);
                GetMatchs(input, end - 1, items);
            }
        }
        /// <summary>
        /// Group数据
        /// </summary>
        /// <typeparam name="T">返回对象</typeparam>
        /// <param name="filter">查询表达式</param>
        /// <param name="groupFields">分组字段列表，如有别名请用“，”号隔开，如：“firstName,name”</param>
        /// <param name="pageSize">分页大小，不分页为0</param>
        /// <param name="page">当前页，不分页为0</param>
        /// <param name="orderBy">排序表达式，格式："id:1"或"id:-1"</param>
        /// <param name="sumFields">合计字段列表，如有别名请用“，”号隔开，如：“profit,money”</param>
        /// <returns></returns>
        public static List<TResult> Group<TDocument, TResult>(this IMongoCollection<TDocument> collection, IQueryable<TDocument> filter, string[] groupFields, int pageSize, int page, string orderBy, params string[] sumFields)
        {
            var str = filter.ToString();
            str = str.Substring(11, str.Length - 13).Replace(" ", "");
            var list = new List<string>();
            GetMatchs(str, 0, list);
            var bsonDocs = new List<BsonDocument>();
            foreach (var item in list)
            {
                var bd = BsonDocument.Parse(item);
                bsonDocs.Add(bd);
            }
            return Group<TDocument, TResult>(collection, bsonDocs.ToArray(), groupFields, pageSize, page, orderBy, sumFields);
        }
        public static List<TDocument> Group<TDocument>(this IMongoCollection<TDocument> collection, IQueryable<TDocument> filter, string[] groupFields, int pageSize, int page, string orderBy, params string[] sumFields)
        {
            return collection.Group<TDocument, TDocument>(filter, groupFields, pageSize, page, orderBy, sumFields);
        }
        /// <summary>
        /// Group数据
        /// </summary>
        /// <typeparam name="T">返回对象</typeparam>
        /// <param name="filter">查询表达式</param>
        /// <param name="groupFields">分组字段列表，如有别名请用“，”号隔开，如：“firstName,name”</param>
        /// <param name="pageSize">分页大小，不分页为0</param>
        /// <param name="page">当前页，不分页为0</param>
        /// <param name="orderBy">排序表达式，格式："_id:1"或"_id:-1"</param>
        /// <param name="sumFields">合计字段列表，如有别名请用“，”号隔开，如：“profit,money”</param>
        /// <returns></returns>
        public static List<TResult> Group<TDocument, TResult>(this IMongoCollection<TDocument> collection, Expression<Func<TDocument, bool>> filter, string[] groupFields, int pageSize, int page, string orderBy, params string[] sumFields)
        {
            var q = new ExpressionFilterDefinition<TDocument>(filter);
            var bsonDoc = q.Render(collection.DocumentSerializer, collection.Settings.SerializerRegistry);
            var filterBsonDoc = new BsonDocument
                {
                    {
                        "$match",
                        bsonDoc
                    }
                };
            return Group<TDocument, TResult>(collection, new BsonDocument[] { filterBsonDoc }, groupFields, pageSize, page, orderBy, sumFields);
        }
        private static List<TResult> Group<TDocument, TResult>(IMongoCollection<TDocument> collection, BsonDocument[] filters, string[] groupFields, int pageSize, int page, string orderBy, params string[] sumFields)
        {
            var doc = new StringBuilder("{ $group:{_id:");
            //if (groupFields.Length > 1)
            //{
            doc.Append("{");
            #region 生成Group
            foreach (var group in groupFields)
            {
                var items = group.Split(',');
                string field = items[0];
                string alias = field;
                if (items.Length > 1)
                    alias = items[1];
                doc.AppendFormat("{0}:\"${1}\",", alias, field);
            }
            if (doc.ToString().EndsWith(","))
                doc.Remove(doc.Length - 1, 1);
            doc.Append("},");
            #endregion
            //}
            //else
            //{
            //    doc.AppendFormat("\"${0}\"", groupFields[0]);
            //}
            #region 生成Sum
            foreach (var sumField in sumFields)
            {
                var items = sumField.Split(',');
                string field = items[0];
                string alias = field;
                if (items.Length > 1)
                    alias = items[1];
                doc.Append(alias + ":{$sum:" + "\"$" + field + "\"},");
            }
            if (sumFields.Length == 0)
            {
                doc.Append("count:{$sum:1},");
            }
            if (doc.ToString().EndsWith(","))
                doc.Remove(doc.Length - 1, 1);
            doc.Append("}}");
            #endregion
            #region 排序
            string sortDoc = null;
            if (!string.IsNullOrEmpty(orderBy))
            {
                sortDoc = "{ $sort : { " + orderBy + " } }";
            }
            #endregion
            #region 分页
            string pageDoc = null;
            string skipDoc = null;
            if (pageSize > 0)
            {
                pageDoc = "{ $limit : " + pageSize.ToString() + " }";
                skipDoc = "{ $skip : " + string.Format("{0}", (pageSize * (page - 1))) + " }";
            }
            #endregion
            var pipeline = new List<BsonDocument>();
            pipeline.AddRange(filters);
            pipeline.Add(
                MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(doc.ToString())
            );

            if (sortDoc != null)
                pipeline.Add(MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(sortDoc));
            if (pageDoc != null)
            {
                pipeline.Add(MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(skipDoc));
                pipeline.Add(MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(pageDoc));
            }
            var results = collection.Aggregate<BsonDocument>(pipeline).ToList();
            var datas = new List<TResult>();
            var fields = groupFields.ToList();
            if (sumFields.Length == 0)
                fields.Add("Count");
            else
                fields.AddRange(sumFields);
            foreach (var result in results)
            {
                var type = typeof(TResult);
                var o = (TResult)CreateInstance(type);
                foreach (var field in fields)
                {
                    var items = field.Split(',');
                    var p = type.GetProperty(items[0]);
                    if (result.Contains(items[0]))
                        p.SetValue(o, Convert(result[items[0]], p.PropertyType), null);
                    else if (result.Contains("_id") && result["_id"] is BsonDocument)
                    {
                        var bsonDoc = result[0] as BsonDocument;
                        if (bsonDoc.Contains(items[0]))
                            p.SetValue(o, Convert(bsonDoc[items[0]], p.PropertyType), null);
                    }

                }
                datas.Add(o);
            }
            return datas;
        }
        #endregion

        #region Count
        /// <summary>
        /// Count
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static long Count<TDocument>(this IMongoCollection<TDocument> collection)
        {
            return collection.Count(new BsonDocument());
        }
        #endregion

        #endregion

        #region 添加
        /// <summary>
        /// 添加对象
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <param name="collection"></param>
        /// <param name="doc">要添加的对象</param>
        /// <param name="insertOpts"></param>
        public static void Add<TDocument>(this IMongoCollection<TDocument> collection, TDocument doc, InsertOneOptions insertOpts = null)
        {
            collection.InsertOne(doc, insertOpts);
        }
        public static void AddMany<TDocument>(this IMongoCollection<TDocument> collection, IEnumerable<TDocument> documents, InsertManyOptions options = null)
        {
            collection.InsertMany(documents, options);
        }
        public static ReplaceOneResult Save<TDocument>(this IMongoCollection<TDocument> collection, TDocument doc)
        {
            object id;
            if (doc is BsonDocument)
            {
                return collection.ReplaceOne(new BsonDocument("_id", (doc as BsonDocument)["_id"]), doc, new UpdateOptions() { IsUpsert = true });
            }
            else {
                var p = doc.GetType().GetProperty("Id");
                if (p == null)
                    p = doc.GetType().GetProperty("id");
                id = p.GetValue(doc);
                return collection.ReplaceOne(new BsonDocument("_id", BsonValue.Create(id)), doc, new UpdateOptions() { IsUpsert = true });
            }
            //collection.FindOneAndUpdate(new BsonDocument("_id", BsonValue.Create(id)), collection.GetUpdate().)
        }
        public static void Insert<TDocument>(this IMongoCollection<TDocument> collection, TDocument doc, InsertOneOptions insertOpts = null)
        {
            collection.InsertOne(doc, insertOpts);
        }
        /// <summary>
        /// 添加子对象
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="filter">查询表达式</param>
        /// <param name="memberExpression">要更新的字段</param>
        /// <param name="values">更新到该字段的数组</param>
        public static UpdateResult AddChild<TDocument, TValue>(this IMongoCollection<TDocument> collection, Expression<Func<TDocument, bool>> filter, Expression<Func<TDocument, IEnumerable<TValue>>> field, params TValue[] values)
        {
            var update = Builders<TDocument>.Update.PushEach(field, values);
            return collection.Update(filter, update);
        }
        #endregion

        #region 更新
        /// <summary>
        /// 更新一个值
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <typeparam name="TField"></typeparam>
        /// <param name="collection"></param>
        /// <param name="filter">查询表达式</param>
        /// <param name="field">要更新的字段</param>
        /// <param name="value">更新的值</param>
        public static UpdateResult Update<TDocument, TField>(this IMongoCollection<TDocument> collection, Expression<Func<TDocument, bool>> filter, Expression<Func<TDocument, TField>> field, TField value)
        {
            var updater = Builders<TDocument>.Update.Set(field, value);
            return collection.UpdateMany(filter, updater);
        }
        /// <summary>
        /// 更新两个值
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <typeparam name="TField"></typeparam>
        /// <param name="collection"></param>
        /// <param name="filter"></param>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <param name="field1"></param>
        /// <param name="value1"></param>
        /// <returns></returns>
        public static UpdateResult Update<TDocument, T1, T2>(this IMongoCollection<TDocument> collection, Expression<Func<TDocument, bool>> filter, Expression<Func<TDocument, T1>> field, T1 value, Expression<Func<TDocument, T2>> field1, T2 value1)
        {
            var updater = Builders<TDocument>.Update.Set(field, value).Set(field1, value1);
            return collection.UpdateMany(filter, updater);
        }
        /// <summary>
        /// 获取UpdateBuilder对象更新
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <typeparam name="TField"></typeparam>
        /// <param name="collection"></param>
        /// <param name="filter"></param>
        /// <param name="update"></param>
        /// <returns></returns>
        public static UpdateResult Update<TDocument>(this IMongoCollection<TDocument> collection, Expression<Func<TDocument, bool>> filter, UpdateDefinition<TDocument> update, bool? isUpsert = null)
        {
            return collection.UpdateMany(filter, update, isUpsert == null ? null : new UpdateOptions()
            {
                IsUpsert = isUpsert.Value
            });
        }
        /// <summary>
        /// 更新一行记录中的一个值
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <typeparam name="TField"></typeparam>
        /// <param name="collection"></param>
        /// <param name="filter"></param>
        /// <param name="field"></param>
        /// <param name="value"></param>
        public static UpdateResult UpdateOne<TDocument, TField>(this IMongoCollection<TDocument> collection, Expression<Func<TDocument, bool>> filter, Expression<Func<TDocument, TField>> field, TField value)
        {
            var updater = Builders<TDocument>.Update.Set(field, value);
            return collection.UpdateOne(filter, updater);
        }
        /// <summary>
        /// 更新一行记录中的两个值
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <typeparam name="TField"></typeparam>
        /// <param name="collection"></param>
        /// <param name="filter"></param>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <param name="field1"></param>
        /// <param name="value1"></param>
        public static UpdateResult UpdateOne<TDocument, TField>(this IMongoCollection<TDocument> collection, Expression<Func<TDocument, bool>> filter, Expression<Func<TDocument, TField>> field, TField value, Expression<Func<TDocument, TField>> field1, TField value1)
        {
            var updater = Builders<TDocument>.Update.Set(field, value).Set(field1, value1);
            return collection.UpdateOne(filter, updater);
        }
        /// <summary>
        /// 根据UpdateBuilder更新一行值
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <typeparam name="TField"></typeparam>
        /// <param name="collection"></param>
        /// <param name="filter"></param>
        /// <param name="updateBuilder"></param>
        public static UpdateResult UpdateOne<TDocument>(this IMongoCollection<TDocument> collection, Expression<Func<TDocument, bool>> filter, UpdateDefinition<TDocument> updateBuilder, bool isUpsert)
        {
            return collection.UpdateOne(filter, updateBuilder, new UpdateOptions() { IsUpsert = isUpsert });
        }
        /// <summary>
        /// 根据Id更新
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <typeparam name="TField"></typeparam>
        /// <param name="collection"></param>
        /// <param name="id"></param>
        /// <param name="updateBuilder"></param>
        /// <returns></returns>
        public static UpdateResult UpdateByPK<TDocument, TField>(this IMongoCollection<TDocument> collection, TField id, UpdateDefinition<TDocument> updateBuilder)
        {
            var filter = Builders<TDocument>.Filter.Eq("_id", id);
            return collection.UpdateOne(filter, updateBuilder);
        }
        /// <summary>
        /// 根据Id更新
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <typeparam name="TField"></typeparam>
        /// <typeparam name="TUpdateField"></typeparam>
        /// <param name="collection"></param>
        /// <param name="id"></param>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static UpdateResult UpdateByPK<TDocument, TField, TUpdateField>(this IMongoCollection<TDocument> collection, TField id, Expression<Func<TDocument, TUpdateField>> field, TUpdateField value)
        {
            var filter = Builders<TDocument>.Filter.Eq("_id", id);
            var update = Builders<TDocument>.Update.Set(field, value);
            return collection.UpdateOne(filter, update);
        }
        /// <summary>
        /// 更新并返回最新记录
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <param name="doc"></param>
        /// <param name="filter"></param>
        /// <param name="updater"></param>
        /// <returns></returns>
        public static TDocument UpdateAndReturnNew<TDocument>(this IMongoCollection<TDocument> doc, Expression<Func<TDocument, bool>> filter, UpdateDefinition<TDocument> updater)
        {
            var newDoc = doc.FindOneAndUpdate(filter, updater, new FindOneAndUpdateOptions<TDocument, TDocument>() { IsUpsert = true, ReturnDocument = ReturnDocument.After });
            return newDoc;
        }
        /// <summary>
        /// 根据指定字段累加指定值
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <typeparam name="TField"></typeparam>
        /// <param name="collection"></param>
        /// <param name="filter"></param>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static UpdateResult Inc<TDocument, TField>(this IMongoCollection<TDocument> collection, Expression<Func<TDocument, bool>> filter, Expression<Func<TDocument, TField>> field, TField value)
        {
            var updater = Builders<TDocument>.Update.Inc(field, value);
            //            collection.FindOneAndUpdate(filter, updater, new FindOneAndUpdateOptions<TDocument, TField>() { ReturnDocument = ReturnDocument.After });
            return collection.UpdateOne(filter, updater);
        }
        /// <summary>
        /// 根据指定字段累加指定值并返回最新值
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <typeparam name="TField"></typeparam>
        /// <param name="collection"></param>
        /// <param name="filter"></param>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TDocument IncAndReturnNew<TDocument, TField>(this IMongoCollection<TDocument> doc, Expression<Func<TDocument, bool>> filter, Expression<Func<TDocument, TField>> field, TField value)
        {
            var updater = Builders<TDocument>.Update.Inc(field, value);
            return doc.UpdateAndReturnNew(filter, updater);
        }

        #region 子文档操作
        /// <summary>
        /// 更新子文档
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <typeparam name="TField"></typeparam>
        /// <typeparam name="TSaveField"></typeparam>
        /// <param name="collection"></param>
        /// <param name="filter">查询语句</param>
        /// <param name="memberExpression">要更新的字段</param>
        /// <param name="value">要更新的值</param>
        /// <returns></returns>
        public static UpdateResult SaveChildren<TDocument, TField, TSaveField>(this IMongoCollection<TDocument> collection, Expression<Func<TDocument, bool>> filter, Expression<Func<TDocument, TField>> memberExpression, TSaveField value)
        {
            var pInfos = value.GetType().GetProperties();
            var updateBuilder = collection.GetUpdate();
            var field = memberExpression.Body.ToString().Split('.')[1];
            foreach (var p in pInfos)
            {
                if (p.Name == "Id") continue;
                var pValue = p.GetValue(value, null);
                BsonValue bsonValue = null;
                if (pValue != null)
                {
                    if (pValue.GetType().Name == "Decimal")
                        bsonValue = BsonValue.Create((double)pValue);
                    else
                        bsonValue = BsonValue.Create(pValue);
                    updateBuilder = updateBuilder.Set(string.Format("{0}.$.{1}", field, p.Name), bsonValue);
                }
            }
            return collection.Update(filter, updateBuilder);
        }
        #endregion

        /// <summary>
        /// 获取更新对象
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static UpdateDefinition<TDocument> GetUpdate<TDocument>(this IMongoCollection<TDocument> collection)
        {
            return Builders<TDocument>.Update.Combine();
        }
        /// <summary>
        /// 获取更新对象
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <typeparam name="TField"></typeparam>
        /// <param name="collection"></param>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static UpdateDefinition<TDocument> GetUpdate<TDocument, TField>(this IMongoCollection<TDocument> collection, Expression<Func<TDocument, TField>> field, TField value)
        {
            return Builders<TDocument>.Update.Set(field, value);
        }
        public static UpdateDefinition<TDocument> GetUpdate<TDocument, TField>(this IMongoCollection<TDocument> collection, FieldDefinition<TDocument, TField> field, TField value)
        {
            return Builders<TDocument>.Update.Set(field, value);
        }
        #endregion

        #region 删除
        /// <summary>
        /// 删除表
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <param name="collection"></param>
        public static void Drop<TDocument>(this IMongoCollection<TDocument> collection)
        {
            collection.Database.DropCollection(collection.CollectionNamespace.CollectionName);
        }
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <param name="collection"></param>
        /// <param name="filter">查询表达式</param>
        /// <returns></returns>
        public static DeleteResult Delete<TDocument>(this IMongoCollection<TDocument> collection, Expression<Func<TDocument, bool>> filter)
        {
            return collection.DeleteMany(filter);
        }
        /// <summary>
        /// 移除指定项目的子项
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="collection"></param>
        /// <param name="filter"></param>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static UpdateResult DeleteChild<TDocument, TItem>(this IMongoCollection<TDocument> collection, Expression<Func<TDocument, bool>> filter, Expression<Func<TDocument, IEnumerable<TItem>>> field, TItem value)
        {
            var update = Builders<TDocument>.Update.Pull(field, value);
            return collection.UpdateMany(filter, update);
        }
        #endregion

        #region 私有方法
        private static object Convert(BsonValue value, Type type)
        {
            switch (type.Name)
            {
                case "String":
                    return value.ToString();
                case "Int32":
                    return value.ToInt32();
                case "Int64":
                    return value.ToInt64();
                case "Decimal":
                    return (decimal)value.ToDouble();
                case "Double":
                    return value.ToDouble();
                case "DateTime":
                    return value.ToLocalTime();
                case "Boolean":
                    return value.ToBoolean();
                case "Nullable`1":
                    var nullType = type.GetProperty("Value").PropertyType;
                    switch (nullType.Name)
                    {
                        case "DateTime":
                            return value.ToNullableLocalTime();
                        default:
                            return Convert(value, nullType);
                    }
            }
            return value;
        }

        private static object CreateInstance(Type type)
        {
            try
            {
                CreateObject c = null;
                if (_constrcache.TryGetValue(type, out c))
                {
                    return c();
                }
                else
                {
                    if (type.GetTypeInfo().IsClass)
                    {
                        DynamicMethod dynMethod = new DynamicMethod("_", type, null);
                        ILGenerator ilGen = dynMethod.GetILGenerator();
                        ilGen.Emit(OpCodes.Newobj, type.GetConstructor(Type.EmptyTypes));
                        ilGen.Emit(OpCodes.Ret);
                        c = (CreateObject)dynMethod.CreateDelegate(typeof(CreateObject));
                        _constrcache.Add(type, c);
                    }
                    else // structs
                    {
                        DynamicMethod dynMethod = new DynamicMethod("_", typeof(object), null);
                        ILGenerator ilGen = dynMethod.GetILGenerator();
                        var lv = ilGen.DeclareLocal(type);
                        ilGen.Emit(OpCodes.Ldloca_S, lv);
                        ilGen.Emit(OpCodes.Initobj, type);
                        ilGen.Emit(OpCodes.Ldloc_0);
                        ilGen.Emit(OpCodes.Box, type);
                        ilGen.Emit(OpCodes.Ret);
                        c = (CreateObject)dynMethod.CreateDelegate(typeof(CreateObject));
                        _constrcache.Add(type, c);
                    }
                    return c();
                }
            }
            catch (Exception exc)
            {
                throw new Exception(string.Format("Failed to fast create instance for type '{0}' from assembly '{1}'",
                    type.FullName, type.AssemblyQualifiedName), exc);
            }
        }
        #endregion
    }
    #endregion

    #region LINQ OrderBy Extension
    public static class OrderByExt
    {
        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, string property)
        {
            return ApplyOrder<T>(source, property, "OrderBy");
        }
        public static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> source, string property)
        {
            return ApplyOrder<T>(source, property, "OrderByDescending");
        }
        public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> source, string property)
        {
            return ApplyOrder<T>(source, property, "ThenBy");
        }
        public static IOrderedQueryable<T> ThenByDescending<T>(this IOrderedQueryable<T> source, string property)
        {
            return ApplyOrder<T>(source, property, "ThenByDescending");
        }
        static IOrderedQueryable<T> ApplyOrder<T>(IQueryable<T> source, string property, string methodName)
        {
            string[] props = property.Split('.');
            Type type = typeof(T);
            ParameterExpression arg = Expression.Parameter(type, "x");
            Expression expr = arg;
            foreach (string prop in props)
            {
                // use reflection (not ComponentModel) to mirror LINQ
                var pi = type.GetProperty(prop);
                expr = Expression.Property(expr, pi);
                type = pi.PropertyType;
            }
            Type delegateType = typeof(Func<,>).MakeGenericType(typeof(T), type);
            LambdaExpression lambda = Expression.Lambda(delegateType, expr, arg);

            object result = typeof(Queryable).GetMethods().Single(
                    method => method.Name == methodName
                            && method.IsGenericMethodDefinition
                            && method.GetGenericArguments().Length == 2
                            && method.GetParameters().Length == 2)
                    .MakeGenericMethod(typeof(T), type)
                    .Invoke(null, new object[] { source, lambda });
            return (IOrderedQueryable<T>)result;
        }
    }
    #endregion
}
