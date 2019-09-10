using Microsoft.AspNetCore.Mvc;
using minio_webhook.Models;
using System;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Text.Json;
using System.Threading.Tasks;

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
            await _minio.CheckBucketRegistration().ConfigureAwait(false);

            Ok("Buckets registred");
        }

        [HttpPost]
        public async Task PostAsync()
        {
            try
            {
                await _minio.CheckBucketRegistration().ConfigureAwait(false);

                var bucketNotificationPost = await JsonSerializer.DeserializeAsync<BucketNotificationPost>(Request.Body).ConfigureAwait(false);

                if (bucketNotificationPost != null)
                {
                    _minio.Process(bucketNotificationPost);
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
