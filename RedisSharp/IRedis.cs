using System.Runtime.Serialization;

namespace instalist.Core.Model.InstaModels.Base { 
    public enum RedisEntityType
    {
        String = 0, List = 1, SortedList = 2
    }
    public interface IRedis
    {
        double RedisScore { get; set; }

        RedisEntityType RedisEntityType { get; set; }

        string RedisKey { get; set; }

        string MakeRedisKey(string param1, string param2 = null, string param3 = null);
    }
}
