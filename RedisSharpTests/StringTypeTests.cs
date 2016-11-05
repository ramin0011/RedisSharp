using NUnit.Framework;
using System;
using System.Linq;
using instalist.Core.Model.InstaModels.Base;
using RedisSharp;
using RedisSharpTests;
using RedisSharpTests.Models;

namespace Redis.Tests
{
    [TestFixture()]
    public class StringTypeTests
    {
        DatabaseHelper DatabaseHelper=new DatabaseHelper();
        Employee Employee=new Employee() ;
        public static string redisSetKey = "Test:Test:Set";
        public static string redisSortedSetKey = "Test:Test:SortedSet2";

        [Test()]
        public void Insert()
        {
            Assert.DoesNotThrow(() =>
            {
                for (int i = 0; i < 20; i++)
                {
                    DatabaseHelper.Save<Employee_StringType>(new Employee_StringType("john " + i * 5));
                }
            });
        }
        [Test()]
        public void Query()
        {
           var items= DatabaseHelper.Query<Employee_StringType>("john 1");
           Assert.Greater(items.Count(),0);
        }
        [Test()]
        public void Get()
        {
            var item = DatabaseHelper.Load<Employee_StringType>(Employee_StringType.partitionKey, "john 55");
            Assert.IsNotNull(item.RedisKey);
        }
        [Test()]
        public void Update()
        {
         
            var item =  DatabaseHelper.Query<Employee_StringType>("john").First();
            var data=new Employee_StringType("david")
            {
                RedisKey = item.RedisKey,
            };
            var saved= DatabaseHelper.Save<Employee_StringType>(data);
            Assert.Pass("updated");
        }

    }
}