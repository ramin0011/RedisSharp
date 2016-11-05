using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using instalist.Core.Model.InstaModels.Base;

namespace RedisSharpTests.Models
{
    class Employee_StringType:IRedis
    {
        public static string partitionKey = "our_empoyees";

        public Employee_StringType(string name)
        {
            Name = name;
            CreatedDateTime=DateTime.Now;
            RedisKey = MakeRedisKey(name);
        }

        public Employee_StringType()
        {
          
        }

        public DateTime CreatedDateTime { get; set; }
        public string Name { get; set; }

        public double RedisScore { get; set; }
        public RedisEntityType RedisEntityType { get; set; }=RedisEntityType.String;
        public string RedisKey { get; set; }
        public string MakeRedisKey(string param1, string param2 = null, string param3 = null)
        {
            if (!string.IsNullOrWhiteSpace(param2))
                return param1 +":"+ param2;
            return partitionKey + ":" +param1;
        }
    }
}
