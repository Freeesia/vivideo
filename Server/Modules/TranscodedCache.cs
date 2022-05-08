using Nito.AsyncEx;
using StackExchange.Redis;
using StackExchange.Redis.KeyspaceIsolation;

namespace StudioFreesia.Vivideo.Server.Modules;

public class TranscodedCache : ITranscodedCache
{
    private static readonly TimeSpan expiry = TimeSpan.FromDays(30);
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
        return con.GetDatabase().WithKeyPrefix(nameof(TranscodedCache) + ":");
    }

    public async Task<ReadOnlyMemory<byte>> Get(string key, string file)
    {
        var db = await GetDatabase();
        var lease = await db.HashGetLeaseAsync(key, file);
        return lease.Memory;
    }

    public async Task Set(string key, string file, ReadOnlyMemory<byte> buf)
    {
        var db = await GetDatabase();
        await db.HashSetAsync(key, file, buf);
        await db.KeyExpireAsync(key, expiry);
    }

    public async Task<bool> Exist(string key)
    {
        var db = await GetDatabase();
        return await db.KeyExistsAsync(key);
    }

    public async Task Delete(string key)
    {
        var db = await GetDatabase();
        await db.KeyDeleteAsync(key);
    }
}

public interface ITranscodedCache
{
    Task Set(string key, string file, ReadOnlyMemory<byte> buf);
    Task<ReadOnlyMemory<byte>> Get(string key, string file);
    Task<bool> Exist(string key);
    Task Delete(string key);
}
