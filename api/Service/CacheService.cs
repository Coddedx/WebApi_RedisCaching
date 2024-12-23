using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using api.Interfaces;
using StackExchange.Redis;

namespace api.Service
{
    public class CacheService : ICacheService
    {
        private IDatabase _cacheDb; //use using StackExchange.Redis; instead of using Microsoft.EntityFrameworkCore.Storage;
        public CacheService()
        {
            // var redisConnection = configuration.GetSection("Redis:ConnectionString").Value;
            // var redis = ConnectionMultiplexer.Connect(redisConnection);
            
            var redis = ConnectionMultiplexer.Connect("localhost:6379"); //actual end point of my redis (on docker terminal we create default port which is 6379 (to see write docker ps (on docker terminal)))
            _cacheDb = redis.GetDatabase();
        }
        public T GetData<T>(string key)
        {
            var value = _cacheDb.StringGet(key); //StringGet -->> it will match the key whatever exists on our redis cache 
            //redis is basicly a key-value pair. for examp if ı called user its gonna get me list of users 
            if(!string.IsNullOrEmpty(value))            
                //for examp we have 100 driver the way as being stored on redis as string (we said key-value pair) so we need to convert to an object(serilize it(list of string to an object)) (we dont want string)
                return JsonSerializer.Deserialize<T>(value);

                return default; //if we have hover over (from other return) it is returning nullable type of object
            
        }

        public bool SetData<T>(string key, T value, DateTimeOffset expirationTime)
        {
            var expiryTime = expirationTime.DateTime.Subtract(DateTime.Now);
            var isSet = _cacheDb.StringSet(key, JsonSerializer.Serialize(value), expiryTime);//we need to convert any object that we get into a string (we are gonna store them as string)
            return isSet;

        }
        public object RemoveData(string key)
        {
            var _exists = _cacheDb.KeyExists(key);
            if(_exists) return _cacheDb.KeyDelete(key);

            return false;
        }

    }
}