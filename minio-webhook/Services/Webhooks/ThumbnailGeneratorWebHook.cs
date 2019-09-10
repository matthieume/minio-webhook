using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Minio.DataModel;
using minio_webhook.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace minio_webhook.Services.Webhooks
{
    public class ThumbnailGeneratorWebHook : IWebhook
    {
        private readonly string[] _buckets;
        private readonly int[] _thumbnailSize;
        private readonly string[] _acceptedFormats;

        public ThumbnailGeneratorWebHook(IConfiguration configuration)
        {
            _buckets = configuration.GetSection("MinioSettings:WebhooksOptions:ThumbnailGeneratorWebhook:Buckets").Get<string[]>();
            _thumbnailSize = configuration.GetSection("MinioSettings:WebhooksOptions:ThumbnailGeneratorWebhook:ThumbnailSize").Get<int[]>();
            _acceptedFormats = MediaService.SupportedMimeTypes();
        }

        public List<string> GetBucketsToMonitor()
        {
            return new List<string>(_buckets);
        }

        public async Task ProcessAsync(Services.Minio.Minio minio, Record record)
        {
            if (!_buckets.Contains(record.S3.Bucket.Name)) // Only file in the right bucket
                return;

            var created = record.EventName.StartsWith("s3:ObjectCreated:");
            var removed = !created && record.EventName.StartsWith("s3:ObjectRemoved:");

            if (created || removed)
            {
                var mimeType = record.S3.Object.ContentType;
                if (mimeType == null)
                    new FileExtensionContentTypeProvider().TryGetContentType(record.S3.Object.Key, out mimeType);

                if (!_acceptedFormats.Contains(mimeType)) // Supported mimetypes
                    return;

                var lastDotIndex = record.S3.Object.Key.LastIndexOf('.');
                var ext = record.S3.Object.Key.Substring(lastDotIndex + 1);
                var fileName = System.Web.HttpUtility.UrlDecode(record.S3.Object.Key.Substring(0, lastDotIndex));

                if (Regex.IsMatch(fileName, @".+_\d+$"))
                    return; // File already resized.

                if (created)
                {
                    Bitmap source;
                    using (var memoryStream = new MemoryStream())
                    {
                        await minio.Client.GetObjectAsync(
                            record.S3.Bucket.Name,
                            System.Web.HttpUtility.UrlDecode(record.S3.Object.Key),
                            (stream) => stream.CopyTo(memoryStream)).ConfigureAwait(false);
                        source = MediaService.GetBipmapFromStream(memoryStream);
                    }

                    foreach (var size in _thumbnailSize)
                    {
                        try
                        {
                            using (var memoryStream = new MemoryStream())
                            {
                                var resized = MediaService.ResizeBitmap(source, size, size);
                                var resizedFileName = fileName + "_" + size + "." + ext;
                                resized.Save(memoryStream, MediaService.MimeTypeToImageFormat(record.S3.Object.ContentType));
                                memoryStream.Position = 0;
                                Console.WriteLine("Uploading resized file: " + resizedFileName + ", size: " + memoryStream.Length);
                                await minio.Client.PutObjectAsync(
                                    record.S3.Bucket.Name,
                                    resizedFileName,
                                    memoryStream,
                                    -1,
                                    mimeType,
                                    metaData: record.S3.Object.UserMetadata)
                                    .ConfigureAwait(false);
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine(e.Message);
                        }
                    }
                }
                else if (removed)
                {
                    IObservable<Item> observable = minio.Client.ListObjectsAsync(record.S3.Bucket.Name, fileName, true);

                    observable.Subscribe<Item>(async item =>
                    {
                        if (Regex.IsMatch(item.Key, fileName + @"_\d+\." + ext + "$"))
                        {
                            await minio.Client.RemoveObjectAsync("img", System.Web.HttpUtility.UrlDecode(item.Key)).ConfigureAwait(false);
                        }
                    });
                }
            }
        }
    }
}
