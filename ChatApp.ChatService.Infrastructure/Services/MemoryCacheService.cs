using ChatApp.ChatService.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChatApp.ChatService.Infrastructure.Services
{
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _cache;
        private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(10);

        public MemoryCacheService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public Task<T?> GetAsync<T>(string key) where T : class
        {
            if(_cache.TryGetValue<string>(key, out var json) && !string.IsNullOrEmpty(json))
            {
                var value = JsonSerializer.Deserialize<T>(json);
                return Task.FromResult(value);
            }
            return Task.FromResult<T?>(null);
        }  

        public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
        {
            var json = JsonSerializer.Serialize(value);
            _cache.Set(key, json, expiration ?? DefaultExpiration);
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key)
        {
            _cache.Remove(key);
            return Task.CompletedTask;
        }

    }
}
