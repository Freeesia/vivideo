using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using CommunityToolkit.HighPerformance;
using Microsoft.Extensions.Options;
using Minio;
using Minio.Exceptions;
using ValueTaskSupplement;

namespace StudioFreesia.Vivideo.Server.Modules
{
    public class TranscodedCache : ITranscodedCache
    {
        private const string BucketName = "transcoded-cache";
        private readonly MinioClient minio;
        private readonly ILogger<TranscodedCache> logger;

        public TranscodedCache(IHttpClientFactory clientFactory, IOptions<MinioOptions> minioOptions, ILogger<TranscodedCache> logger)
        {
            this.minio = new MinioClient()
                .WithHttpClient(clientFactory.CreateClient(nameof(TranscodedCache)))
                .WithEndpoint(minioOptions.Value.Endpoint, minioOptions.Value.Port)
                .WithCredentials(minioOptions.Value.User, minioOptions.Value.Passowrd)
                .Build();
            this.logger = logger;
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
            this.logger.LogDebug($"get {GetObjectName(key, file)}");
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

        public async ValueTask Set(string key, string file, ReadOnlyMemory<byte> buf)
        {
            this.logger.LogDebug($"set {GetObjectName(key, file)}");
            using var st = buf.AsStream();
            await this.minio.PutObjectAsync(
                new PutObjectArgs()
                    .WithBucket(BucketName)
                    .WithObject(GetObjectName(key, file))
                    .WithObjectSize(buf.Length)
                    .WithStreamData(st));
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
    }

    public interface ITranscodedCache
    {
        ValueTask Init();
        ValueTask Set(string key, string file, ReadOnlyMemory<byte> buf);
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
    }
}