# RedisSharp
RedisSharp is a C# library to use redis database not only as a cach database, and also it makes it easier to work with redis value types(sets ,sortedsset etc),
RedisSharp uses [StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis) and [StackExchange.Redis.Extensions](https://github.com/imperugo/StackExchange.Redis.Extensions) ,
so all data TTLs are set to -1 unless you use stackexchange APIs itself to change them(which is possible from RedisSharp)

benefits:
1. access the same benefits from stackexchange or stackexchange.extentions
2. save your data in sorted sets or sorts or string types 
3. get the last saved items as the first retreiving items in sortedLists (like using in feeds meaning the first items you get are the recent saved items) 

#How it works
install from nuget 

    Install-Package RedisSharp
    
add these lines to your web or app.config

      <configSections>
        <section name="redisCacheClient" type="StackExchange.Redis.Extensions.Core.Configuration.RedisCachingSectionHandler, StackExchange.Redis.Extensions.Core"/>
      </configSections>
      <redisCacheClient allowAdmin="true" ssl="false" connectTimeout="5000">
            <hosts>
                 <add host="127.0.0.1" cachePort="6379"/>
            </hosts>
      </redisCacheClient>

your models should inherit from IRedis which is:

    public interface IRedis
    {
        double RedisScore { get; set; }
        RedisEntityType RedisEntityType { get; set; }
        string RedisKey { get; set; }
        string MakeRedisKey(string param1, string param2 = null, string param3 = null);
    }

just use IRedis intefrace
and determine how you make your redisKey and your data type
the RedisEntityType is set to string by default (string is an boject converted to json) and you can change it to sortedset ,if so RedisScore is sautomatically set when you save the your object .

1.here the redis type is sortedList
  
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
            return "Employees";
        }
     }
    
create an DatabaseHelper object and start using Redis
if you wanted to uset StackExchange just call DatabseHelper.StackExchangeRedisCacheClient

use this command to save an obj 
    
    var savedData = new DatabaseHelper.Save<Employee>( new Employee() { Name = "John Joe", Id =i });
    
 2.strings
 in string mode an object is converted to a json string and will be deserialized from string to your class type thanx newtonsoft and stackexchange.extention
 
 this is another sample for string types
 
 
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
 
#loading data
 you can load data with 'Query' and 'Load' methods to load an item or bunch of items

#indexing
this is diffrent with indexing , its just a way to index your rediskeys of string types models in a sorted set so you can retireive them 
in sorted by order they are listed , meaning the first items you get are the recent saved items
you can index your keys in a sorted set with RedisIndexes class , so no need to create another model , but consider that the index
key would be 'Index:*'
      
      
       for (int i = 0; i < 20; i++)
                {
                    var obj=  DatabaseHelper.Save<Employee_StringType>(new Employee_StringType("Jack " + i * 5));
            DatabaseHelper.Save<RedisIndexes(RedisIndexes.CreateNewIntance(redisIndexPartitionKey,obj.RedisKey),redisIndexPartitionKey);
                }


   and you can retrieve items from indexes
   
         
         var result=new List<Employee_StringType>();
            var indexedItems = DatabaseHelper.Query<RedisIndexes>(redisIndexPartitionKey);
            foreach (var index in indexedItems)
            {
                result.Add(DatabaseHelper.Load<Employee_StringType>(index.Value));
            }
            
 check tests for more info
