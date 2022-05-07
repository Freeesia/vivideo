using Microsoft.AspNetCore.Mvc.Filters;
using Nito.AsyncEx;
using StackExchange.Redis;

namespace StudioFreesia.Vivideo.Server.Modules;

public class TranscodedCache : ITranscodedCache
{
    private static readonly RedisKey KeyPrefix = nameof(TranscodedCache) + ":";
    private readonly string connectionString;
    private readonly AsyncLazy<ConnectionMultiplexer> con;

    static TranscodedCache()
    {
        ConnectionMultiplexer.SetFeatureFlag("preventthreadtheft", true);
    }

    public TranscodedCache(IConfiguration configuration)
    {
        this.connectionString = configuration.GetConnectionString("Redis");
        this.con = new(() => ConnectionMultiplexer.ConnectAsync(this.connectionString));
    }

    private async Task<IDatabase> GetDatabase()
    {
        var con = await this.con;
        return con.GetDatabase();
    }

    public async Task<ReadOnlyMemory<byte>> Get(string key, string file)
    {
        var db = await GetDatabase();
        return await db.HashGetAsync(KeyPrefix.Append(key), file);
    }

    public async Task Set(string key, string file, ReadOnlyMemory<byte> buf)
    {
        var db = await GetDatabase();
        await db.HashSetAsync(KeyPrefix.Append(key), file, buf);
    }

    public async Task<bool> Exist(string key)
    {
        var db = await GetDatabase();
        return await db.KeyExistsAsync(KeyPrefix.Append(key));
    }

    public async Task Delete(string key)
    {
        var db = await GetDatabase();
        await db.KeyDeleteAsync(KeyPrefix.Append(key));
    }
}

public interface ITranscodedCache
{
    Task Set(string key, string file, ReadOnlyMemory<byte> buf);
    Task<ReadOnlyMemory<byte>> Get(string key, string file);
    Task<bool> Exist(string key);
    Task Delete(string key);
}
