using System.Diagnostics;
using System.IO.Pipelines;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Channels;
using Microsoft.Extensions.Options;
using Minio;
using Minio.Exceptions;
using Sentry;
using ValueTaskSupplement;

namespace StudioFreesia.Vivideo.Server.Modules;

public class TranscodedCache : ITranscodedCache
{
    private const string BucketName = "transcoded-cache";
    private readonly MinioClient minio;
    private readonly ILogger<TranscodedCache> logger;
    private readonly Channel<StoreQueue> chunnel;
    private readonly Task worker;
    private readonly Task queueChecker;
    private readonly int putWorkerThrethold;

    public TranscodedCache(IHttpClientFactory clientFactory, IOptions<MinioOptions> minioOptions, ILogger<TranscodedCache> logger)
    {
        this.minio = new MinioClient()
            .WithHttpClient(clientFactory.CreateClient(nameof(TranscodedCache)))
            .WithEndpoint(minioOptions.Value.Endpoint, minioOptions.Value.Port)
            .WithCredentials(minioOptions.Value.User, minioOptions.Value.Passowrd)
            .Build();
        this.logger = logger;
        this.chunnel = Channel.CreateUnbounded<StoreQueue>();
        this.worker = Task.Run(InfWork);
        this.queueChecker = Task.Run(QueueCheck);
        this.putWorkerThrethold = minioOptions.Value.PutWorkerThrethold;
    }

    public async ValueTask Init()
    {
        if (await minio.BucketExistsAsync(new BucketExistsArgs().WithBucket(BucketName)))
        {
            return;
        }
        await this.minio.MakeBucketAsync(
            new MakeBucketArgs()
                .WithBucket(BucketName));
        await this.minio.SetBucketLifecycleAsync(
            new SetBucketLifecycleArgs()
                .WithBucket(BucketName)
                .WithLifecycleConfiguration(new()
                {
                    Rules = new()
                    {
                        new()
                        {
                            Expiration = new(){ Days = 30 },
                            Status = "Enabled",
                        },
                    },
                }));
    }

    private static string GetObjectName(string key, string file)
        => key + "/" + file;

    public async ValueTask<ReadOnlyMemory<byte>> Get(string key, string file)
    {
        try
        {
            using var ms = new MemoryStream();
            await this.minio.GetObjectAsync(
                new GetObjectArgs()
                    .WithBucket(BucketName)
                    .WithObject(GetObjectName(key, file))
                    .WithCallbackStream((st, ct) => st.CopyToAsync(ms, ct)));
            return ms.ToArray();
        }
        catch (ObjectNotFoundException)
        {
            return ReadOnlyMemory<byte>.Empty;
        }
    }

    public async ValueTask Set(PipeReader reader, string key, string file)
    {
        try
        {
            using var from = reader.AsStream();
            var ms = new MemoryStream();
            await from.CopyToAsync(ms);
            this.logger.LogDebug($"set:{file}:{ms.Length}");
            ms.Seek(0, SeekOrigin.Begin);
            await this.chunnel.Writer.WriteAsync(new(key, file, ms));
        }
        catch (Exception e)
        {
            throw new Exception($"{key}:{file}", e);
        }
    }

    public ValueTask<bool> Exist(string key)
        => this.minio.ListObjectsAsync(
                new ListObjectsArgs()
                    .WithBucket(BucketName)
                    .WithPrefix(key + "/"))
                .Any()
                .ToTask()
                .AsValueTask();

    public async ValueTask Delete(string key)
    {
        var targets = await this.minio.ListObjectsAsync(
            new ListObjectsArgs()
                .WithBucket(BucketName)
                .WithPrefix(key + "/"))
            .Select(i => i.Key)
            .ToList()
            .ToTask();
        await this.minio.RemoveObjectsAsync(
            new RemoveObjectsArgs()
                .WithBucket(BucketName)
                .WithObjects(targets.ToList()));
    }

    private async void QueueCheck()
    {
        var before = 0;
        while (true)
        {
            var now = this.chunnel.Reader.Count;
            if (now - before > this.putWorkerThrethold)
            {
                this.logger.LogDebug($"一時ワーカー開始, queue: {now}");
                _ = Task.Run(Work);
            }
            else
            {
                this.logger.LogDebug($"queue: {now}");
            }
            before = now;
            await Task.Delay(10_000);
        }
    }

    private async Task InfWork()
    {
        while (await this.chunnel.Reader.WaitToReadAsync())
        {
            this.logger.LogDebug("常駐ワーカー開始");
            await Work();
            this.logger.LogDebug("常駐ワーカー待機");
        }
    }

    private async Task Work()
    {
        var id = Environment.CurrentManagedThreadId;
        while (this.chunnel.Reader.TryRead(out var queue))
        {
            var (key, file, st) = queue;
            var sw = Stopwatch.StartNew();
            try
            {
                await this.minio.PutObjectAsync(
                    new PutObjectArgs()
                        .WithBucket(BucketName)
                        .WithObject(GetObjectName(key, file))
                        .WithObjectSize(st.Length)
                        .WithStreamData(st));
            }
            catch (Exception e)
            {
                // 投げれてない気がするから投げる
                SentrySdk.CaptureException(e);
                this.logger.LogError("追加エラッた。Sentryに送られているはず");
            }
            finally
            {
                st.Dispose();
            }
            this.logger.LogDebug($"work{id}:{file}:{sw.Elapsed}");
        }
        this.logger.LogDebug("ワーカー完了");
    }

    private record StoreQueue(string key, string file, Stream buf);
}

public interface ITranscodedCache
{
    ValueTask Init();
    ValueTask Set(PipeReader reader, string key, string file);
    ValueTask<ReadOnlyMemory<byte>> Get(string key, string file);
    ValueTask<bool> Exist(string key);
    ValueTask Delete(string key);
}

public record MinioOptions
{
    public string? Endpoint { get; init; }
    public int Port { get; init; } = 9000;
    public string User { get; init; } = "minioadmin";
    public string Passowrd { get; init; } = "minioadmin";
    public int PutWorkerThrethold { get; init; } = 500;
}