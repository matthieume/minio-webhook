using Microsoft.AspNetCore.Mvc;
using minio_webhook.Models;
using System;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Hangfire;

namespace minio_webhook.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebhookController : ControllerBase
    {
        private readonly Services.Minio.Minio _minio;

        public WebhookController(Services.Minio.Minio minio)
        {
            _minio = minio;
        }

        [HttpGet]
        public async Task InitAsync()
        {
            await _minio.CheckBucketRegistration();

            Ok("Buckets registred");
        }

        [HttpPost]
        public async Task PostAsync()
        {
            try
            {
                await _minio.CheckBucketRegistration();

                var bucketNotificationPost = await JsonSerializer.DeserializeAsync<BucketNotificationPost>(Request.Body);

                if (bucketNotificationPost != null)
                {
                    BackgroundJob.Enqueue(() => _minio.ProcessAsync(bucketNotificationPost));
                }
                Ok("");
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                BadRequest(e.Message);
            }

        }
    }
}
