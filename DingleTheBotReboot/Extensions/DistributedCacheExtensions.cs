using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace DingleTheBotReboot.Extensions;

public static class DistributedCacheExtensions
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        ReferenceHandler = ReferenceHandler.Preserve
    };

    public static async Task SetRecordAsync<T>(this IDistributedCache cache, string recordId, T data,
        TimeSpan? absoluteExpirationTime = null, TimeSpan? unusedExpirationTime = null)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = absoluteExpirationTime ?? TimeSpan.FromSeconds(60),
            SlidingExpiration = unusedExpirationTime
        };
        var jsonData = JsonSerializer.Serialize(data, JsonSerializerOptions);
        await cache.SetStringAsync(recordId, jsonData, options);
    }

    public static async Task<T> GetRecordAsync<T>(this IDistributedCache cache, string recordId)
    {
        if (string.IsNullOrEmpty(recordId)) return default;
        if (recordId.Contains("AniDbService_")) recordId = recordId.Replace("DingleTheBot_", string.Empty);
        var jsonData = await cache.GetStringAsync(recordId);
        return jsonData is null ? default : JsonSerializer.Deserialize<T>(jsonData, JsonSerializerOptions);
    }
}