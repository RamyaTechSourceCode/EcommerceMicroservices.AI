using InventoryService.Application.Abstractions;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace InventoryService.Infrastructure.Messaging.Redis
{

    public class RedisService : IRedisService
    {
        private readonly IDatabase _db;

        public RedisService(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
        }
                
        // STOCK CHECK
     
        public async Task<int> GetStock(string productId)
        {
            var value = await _db.StringGetAsync($"stock:{productId}");

            if (!value.HasValue)
                return 0;

            return (int)value;
        }

        public async Task SetStock(string productId, int qty)
        {
            await _db.StringSetAsync($"stock:{productId}", qty);
        }

        public Task SetAsync<T>(string key, T value)
        {
            return _db.StringSetAsync(
                key,
                JsonSerializer.Serialize(value)
            );
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var data = await _db.StringGetAsync(key);
            return data.IsNullOrEmpty
                ? default
                : JsonSerializer.Deserialize<T>(data!);
        }
        /*
        
       // CACHE VALIDATION
   
        public async Task<T?> GetCache<T>(string key)
        {
            var value = await _db.StringGetAsync(key);

            if (!value.HasValue)
                return default;

            return JsonSerializer.Deserialize<T>(value!);
        }

        public async Task SetCache<T>(string key, T value, TimeSpan? ttl = null)
        {
            var json = JsonSerializer.Serialize(value);

            await _db.StringSetAsync(key, json, expiry: ttl ?? TimeSpan.FromMinutes(10));
        }

        public async Task<bool> IsCacheValid(string key)
        {
            return await _db.KeyExistsAsync(key);
        }

        // IDEMPOTENCY CHECK
       
        public async Task<bool> IsDuplicateOrder(string orderId)
        {
            return await _db.KeyExistsAsync($"idempotency:order:{orderId}");
        }

        public async Task MarkOrderProcessed(string orderId)
        {
            await _db.StringSetAsync(
                $"idempotency:order:{orderId}",
                "processed",
                TimeSpan.FromHours(24)
            );
        }*/
    }
}
