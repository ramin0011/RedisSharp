using instalist.Core.Model.InstaModels.Base;
using Redis.Tests;

namespace RedisSharpTests.Models
{
    public class Employee:IRedis
    {
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public int Id { get; set; }

        public double RedisScore { get; set; }
        public RedisEntityType RedisEntityType { get; set; }=RedisEntityType.SortedList;
        public string RedisKey { get; set; }
        public string MakeRedisKey(string param1, string param2 = null, string param3 = null)
        {
            return SortsTypesTests.redisSetKey;
        }
    }
}
