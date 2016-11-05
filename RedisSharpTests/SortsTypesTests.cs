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
    public class SortsTypesTests
    {
        DatabaseHelper DatabaseHelper=new DatabaseHelper();
        Employee Employee=new Employee() ;
        public static string redisSetKey = "Test:Test:Set";
        public static string redisSortedSetKey = "Test:Test:SortedSet2";
        [Test()]
        public void InsertSet()
        {
            Assert.DoesNotThrow(() =>
            {
                for (int i = 0; i < 1000; i++)
                {
                    DatabaseHelper.InsertSet<Employee>(redisSetKey, new Employee() { Name = "John Joe", Id =i });
                }
            });
        }
        [Test()]
        public void InsertSortedSet()
        {
            Assert.DoesNotThrow(() =>
            {
                for (int i = 0; i < 11; i++)
                {
                    IRedis employee = new Employee() { Name = "John Joe", Id = i };
                    DatabaseHelper.InsertSortedSetWithAutoScore<Employee>(redisSortedSetKey, ref employee);
                    Console.WriteLine("Inserted employeee with score : "+employee.RedisScore);
                }
            });
        }

        [Test()]
        public void Replace()
        {
            Assert.DoesNotThrow(() =>
            {
                var emp=new Employee() {Name = "modified",RedisScore = 10};
            DatabaseHelper.Save<Employee>(emp,redisSortedSetKey);
            });
        }
        [Test()]
        public void Save()
        {
            Assert.DoesNotThrow(() =>
            {
                for (int i = 0; i < 1000; i++)
                {
                    DatabaseHelper.Save<Employee>(new Employee() { Name = "John Joe", Id = i },redisSortedSetKey);
                }
            });
        }
        [Test()]
        public void Query()
        {
            var enumerable = DatabaseHelper.Query<Employee>(redisSortedSetKey);
            Assert.IsNotEmpty(enumerable);
        }

        [Test()]
        public void SetScan()
        {
            var re = DatabaseHelper.SetScan<Employee>(redisSetKey,"",10,2,  0);
            Assert.IsNotEmpty(re);
        }
        [Test()]
        public void SortedSetScan()
        {
            var re = DatabaseHelper.SortedSetScan<Employee>(redisSortedSetKey);
            Assert.IsNotEmpty(re);
        }
        [Test()]
        public void SortedSetScanByItem()
        {
            var re = DatabaseHelper.GetSortedSetItem<Employee>(redisSortedSetKey,10);
            Assert.AreEqual(re.RedisScore,10);
        }
        [Test()]
        public void GetSortedSetCount()
        {
            Assert.DoesNotThrow(() =>
            {
                var count = DatabaseHelper.SortedSetCount(redisSortedSetKey);
            });
        }

        [Test()]
        public void GetSortedSetRange()
        {
            Assert.DoesNotThrow(() =>
            {
                var data = DatabaseHelper.GetSortedSetByRange<Employee>(redisSortedSetKey,123,223);
                var count = data.Count();
            });
        }
    }
}