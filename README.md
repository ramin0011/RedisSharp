# RedisSharp
RedisSharp is C# library to use redis database not only for as a cach database 
so all data TTL is set to -1 unless you use stackexchange APIs itself (which is possible)

benefits:
1. access the same benefits from stackexchange or stackexchange.extentions
2. save your data in sorted sets or sorts or string types 
3. get the last saved items as the first items (like using in feeds) 


#How?

just use IRedis intefrace
and tell us how you make your redisKey in the constructor

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
            return "Employees:"+param1;
        }
    }
    
use this command to save an obj
    new DatabaseHelper.InsertSet<Employee>(redisSetKey, new Employee() { Name = "John Joe", Id =i });
the RedisEntityType is set to string by default and you can change it to sortedset ,if so RedisScore is set automatically when you save 
your object .

check tests for more info
