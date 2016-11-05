# RedisSharp
RedisSharp is C# library to use redis database not only for as a cach database 
so all data TTL is set to -1 unless you use stackexchange APIs itself (which is possible)

benefits:
1. access the same benefits from stackexchange or stackexchange.extentions
2. save your data in sorted sets or sorts or string types 
3. get the last saved items as the first items (like using in feeds) 


#How it works
install from nuget 

    Install-Package RedisSharp

inherit from IRedis which is:

    public interface IRedis
    {
        double RedisScore { get; set; }
        RedisEntityType RedisEntityType { get; set; }
        string RedisKey { get; set; }
        string MakeRedisKey(string param1, string param2 = null, string param3 = null);
    }

just use IRedis intefrace
and tell us how you make your redisKey and your data type
the RedisEntityType is set to string by default and you can change it to sortedset ,if so RedisScore is set automatically when you save 
your object .

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
            return "Employees:";
        }
     }
    
 
use this command to save an obj 
    
    var savedData = new DatabaseHelper.InsertSet<Employee>(redisSetKey, new Employee() { Name = "John Joe", Id =i });
    
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
 

check tests for more info
