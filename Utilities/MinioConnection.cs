using System;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Minio;
using Minio.DataModel.Args;

namespace financesApi.utilities
{
    // Generic MinIO connection - replaces your BlobConnection
    public static class MinioConnection
    {
        public static async Task<string> GetQueryAsync(string queryPath)
        {
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("MINIO_ENDPOINT")) ||
                string.IsNullOrEmpty(Environment.GetEnvironmentVariable("MINIO_ACCESS_KEY")) ||
                string.IsNullOrEmpty(Environment.GetEnvironmentVariable("MINIO_SECRET_KEY")) ||
                string.IsNullOrEmpty(Environment.GetEnvironmentVariable("MINIO_BUCKET_NAME")))
            {
                throw new ArgumentNullException("MinIO environment variables must not be null or empty.");
            }

            try
            {
                using var minioClient = new MinioClient()
                    .WithEndpoint(Environment.GetEnvironmentVariable("MINIO_ENDPOINT"))
                    .WithCredentials(
                        Environment.GetEnvironmentVariable("MINIO_ACCESS_KEY")!,
                        Environment.GetEnvironmentVariable("MINIO_SECRET_KEY")!
                        )
                    .WithSSL(bool.Parse(Environment.GetEnvironmentVariable("MINIO_USE_SSL") ?? "false"))
                    .Build();

                string bucketName = Environment.GetEnvironmentVariable("MINIO_BUCKET_NAME")!;
                string objectName = $"{queryPath}.sql";

                string queryContent = "";
                await minioClient.GetObjectAsync(
                    new GetObjectArgs()
                        .WithBucket(bucketName)
                        .WithObject(objectName)
                        .WithCallbackStream(async (stream) =>
                        {
                            using var reader = new StreamReader(stream);
                            queryContent = await reader.ReadToEndAsync();
                        })
                );

                return queryContent;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MinIO Error: {ex}");
                throw new Exception($"Failed to retrieve query '{queryPath}': {ex.Message}");
            }
        }

        public static async Task UploadQueryAsync(string queryPath, string sqlContent)
        {
            try
            {
                using var minioClient = new MinioClient()
                    .WithEndpoint(Environment.GetEnvironmentVariable("MINIO_ENDPOINT")!)
                    .WithCredentials(
                        Environment.GetEnvironmentVariable("MINIO_ACCESS_KEY")!,
                        Environment.GetEnvironmentVariable("MINIO_SECRET_KEY")!)
                    .WithSSL(bool.Parse(Environment.GetEnvironmentVariable("MINIO_USE_SSL") ?? "false"))
                    .Build();

                string bucketName = Environment.GetEnvironmentVariable("MINIO_BUCKET")!;
                string objectName = $"sql-queries/{queryPath}.sql";

                byte[] sqlData = System.Text.Encoding.UTF8.GetBytes(sqlContent);
                using var stream = new MemoryStream(sqlData);

                await minioClient.PutObjectAsync(
                    new PutObjectArgs()
                        .WithBucket(bucketName)
                        .WithObject(objectName)
                        .WithStreamData(stream)
                        .WithObjectSize(sqlData.Length)
                        .WithContentType("text/plain")
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MinIO Upload Error: {ex}");
                throw new Exception($"Failed to upload query '{queryPath}': {ex.Message}");
            }
        }
    }
}