using System;
using System.Collections.Generic;
using System.Linq;
using instalist.Core.Model.InstaModels.Base;
using Newtonsoft.Json;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Newtonsoft;
using IRedis = instalist.Core.Model.InstaModels.Base.IRedis;

namespace RedisSharp
{
    public class DatabaseHelper
    {
        enum Database
        {
            DynamoDb, Redis
        }

        private ConnectionMultiplexer _redis;
        public readonly StackExchangeRedisCacheClient RedisContext;
        readonly Database _databaseType = Database.Redis;
        NewtonsoftSerializer serializer = new NewtonsoftSerializer();

        public DatabaseHelper()
        {

            if (_databaseType == Database.Redis)
            {
                _redis = ConnectionMultiplexer.Connect("localhost");

                RedisContext = new StackExchangeRedisCacheClient(serializer);
            }
            else if (_databaseType == Database.DynamoDb)
            {
                //AmazonDynamoDBClient client = new AmazonDynamoDBClient();
                //DynamoDbContext = new DynamoDBContext(client);
            }
        }

        public T Save<T>(instalist.Core.Model.InstaModels.Base.IRedis redis, string partitionKey = "") where T : class
        {
            var data = redis as T;
                if (redis.RedisEntityType == RedisEntityType.String)
                    RedisContext.Add(redis.RedisKey, data);
                else if (redis.RedisEntityType == RedisEntityType.SortedList)
                {
                    if (string.IsNullOrWhiteSpace(partitionKey))
                        throw new NotImplementedException("partition key must be specified");
                    var sortedkey = redis.MakeRedisKey(partitionKey).Replace("*", "");
                    InsertSortedSetWithAutoScore<T>(sortedkey, ref redis);

                }
                else
                    throw new NotFiniteNumberException("not yet implemented");
            return (T)redis;
        }

        public IEnumerable<T> Query<T>(string partitionKey)
            where T : new()
        {
            return Query<T>(partitionKey, 0, double.MaxValue);
        }

        public IEnumerable<T> Query<T>(string partitionKey, double min, double max) where T : new()
        {

            IRedis redis = (IRedis)new T();
                if (redis.RedisEntityType == RedisEntityType.String)
                {
                    var searchPattern = redis.MakeRedisKey(partitionKey);
                    if (searchPattern[searchPattern.Length - 1] != '*')
                        searchPattern += "*";
                    var keys = RedisContext.SearchKeys(searchPattern);
                    return keys.Select(key => RedisContext.Get<T>(key)).ToList();
                }
                else if (redis.RedisEntityType == RedisEntityType.SortedList)
                {
                    var sortedkey = redis.MakeRedisKey(partitionKey).Replace("*", "");
                    return GetSortedSetByRange<T>(sortedkey, min, max);
                }
                return null;

        }
        public T Load<T>(string partitionKey, string rangeKey) where T : new()
        {

                IRedis redis = (IRedis)new T();
                if (redis.RedisEntityType == RedisEntityType.String)
                    return RedisContext.Get<T>(redis.MakeRedisKey(partitionKey, rangeKey));
                else
                    throw new NotImplementedException("load will not work with lists");
        }
        public T Load<T>(string redisKey) where T : new()
        {
                return RedisContext.Get<T>(redisKey);
        }
        public T Load<T>(string partitionKey, long rangeKey) where T : new()
        {
                IRedis redis = (IRedis)new T();
                if (redis.RedisEntityType == RedisEntityType.String)
                    return RedisContext.Get<T>(redis.MakeRedisKey(partitionKey, rangeKey.ToString()));
                else
                    throw new NotImplementedException("load will not work with lists");
        }


        public IEnumerable<T> Scan<T>(string tableName) where T : new()
        {
            IRedis redis = (IRedis)new T();
            if (string.IsNullOrWhiteSpace(tableName))
                    throw new Exception("scan method on redis needs tablename");
                if (redis.RedisEntityType != RedisEntityType.String)
                    throw new Exception("you can only use this method for RedisEntityType.String models");
                var searchPattern = tableName + ":*";
                var keys = RedisContext.SearchKeys(searchPattern);
                return keys.Select(key => RedisContext.Get<T>(key)).ToList();
        }


        public void InsertSet<T>(string key, IRedis redis) where T : new()
        {
            RedisContext.SetAdd(key: key, item: redis);
        }

        private IEnumerable<T> GetSetMembers<T>(string key) where T : new()
        {
            return RedisContext.SetMembers<T>(key);
        }

        public IEnumerable<T> SetScan<T>(string key, string pattern = "", int pageSize = 10, long cursor = 0, int pageOffset = 0, CommandFlags commandFlags = CommandFlags.None) where T : new()
        {
            var source = RedisContext.Database.SetScan((RedisKey)key, (RedisValue)pattern, pageSize, cursor, pageOffset);
            return source.Select(value => JsonConvert.DeserializeObject<T>(value));
        }
        private void InsertSortedSet<T>(string key, IRedis redis, double score)
        {
            RedisContext.Database.SortedSetAdd(key, (RedisValue)JsonConvert.SerializeObject(redis), score);
        }

        public void InsertSortedSetWithAutoScore<T>(string key, ref IRedis redis)
        {
            if (redis.RedisScore >= 1)
            {
                RemoveFromSortedSet(key, redis.RedisScore, redis.RedisScore);
                InsertSortedSet<T>(key, redis, redis.RedisScore);
                return;
            }
            var maxScore = RedisFirstItemScore();
            var count = SortedSetCount(key);
            var score = maxScore - count;
            redis.RedisScore = score;
            InsertSortedSet<T>(key, redis, score);
        }

        public static double RedisFirstItemScore()
        {
            return (double)(Int32.MaxValue);
        }

        public List<T> SortedSetScan<T>(string key, string pattern = "", int pageSize = 10, long cursor = 0, int pageOffset = 0, CommandFlags commandFlags = CommandFlags.None) where T : new()
        {
            var source = RedisContext.Database.SortedSetScan((RedisKey)key, (RedisValue)pattern);
            return source.Select(value => JsonConvert.DeserializeObject<T>(value.Element.ToString())).ToList();
        }
        /// <summary>
        /// Returns all the elements in the sorted set at key with a score between min and max (including elements with score equal to min or max).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        private IEnumerable<T> GetSortedSetByRange<T>(string key)
        {
            var data = RedisContext.Database.SortedSetRangeByScore(key);
            return data.Select(value => JsonConvert.DeserializeObject<T>(value));
        }

        /// <summary>
        /// Returns all the elements in the sorted set at key with a score between min and max (including elements with score equal to min or max).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public IEnumerable<T> GetSortedSetByRange<T>(string key, double min, double max)
        {
            var data = RedisContext.Database.SortedSetRangeByScore(key, min, max);
            return data.Select(value => JsonConvert.DeserializeObject<T>(value));
        }
        public T GetSortedSetItem<T>(string key, double score)
        {
            var data = RedisContext.Database.SortedSetRangeByScore(key, score, score);
            return data.Select(value => JsonConvert.DeserializeObject<T>(value)).First();
        }

        public long SortedSetCount(string key)
        {
            //Time complexity: O(1)
            return RedisContext.Database.SortedSetLength(key);
        }
        public static long GetSortedSetCount(IRedis redis)
        {
            //Time complexity: O(1)
            return new DatabaseHelper().SortedSetCount(redis.RedisKey);
        }
        public static long GetSortedSetCount(string key)
        {
            //Time complexity: O(1)
            return new DatabaseHelper().SortedSetCount(key);
        }

        public void RemoveFromSortedSet(string key, IRedis redis)
        {
            RedisContext.Database.SortedSetRemove(key, JsonConvert.SerializeObject(redis));
        }
        public void RemoveFromSortedSet(string key, double start, double stop)
        {
            RedisContext.Database.SortedSetRemoveRangeByScore(key, start, stop);
        }
        public bool SortedMemberExists(string key, IRedis redis)
        {
            return RedisContext.Database.SortedSetRank(key, JsonConvert.SerializeObject(redis)) != null;
        }
    }
}
