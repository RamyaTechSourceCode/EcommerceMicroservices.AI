using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryService.Application.Abstractions
{

    public interface IRedisService
    {
        // Stock operations
        Task<int> GetStock(string productId);

        Task SetStock(string productId, int qty);

        // Generic cache operations
        Task SetAsync<T>(string key, T value);

        Task<T?> GetAsync<T>(string key);
    }
}
