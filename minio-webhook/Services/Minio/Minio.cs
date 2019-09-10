using Hangfire;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel;
using minio_webhook.Models;
using minio_webhook.Services.Webhooks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace minio_webhook.Services.Minio
{
    public class Minio
    {
        public MinioClient Client { get; }

        private readonly MinioSettings _options;
        private readonly IEnumerable<IWebhook> _webhooks;

        public Minio(IOptions<MinioSettings> options, IEnumerable<IWebhook> webhooks)
        {
            _options = options.Value;

            Client = new MinioClient(
                _options.Endpoint,
                _options.AccessKey,
                _options.SecretKey,
                _options.Region,
                _options.SessionToken);

            if (_options.WithSSL)
            {
                Client = Client.WithSSL();
            }

            _webhooks = webhooks;
        }

        public async Task CheckBucketRegistration()
        {
            foreach (var bucket in _webhooks.SelectMany(t => t.GetBucketsToMonitor().Distinct()))
            {
                if (!await Client.BucketExistsAsync(bucket))
                {
                    await Client.MakeBucketAsync(bucket).ConfigureAwait(false);
                }

                var currentNotifications = await Client.GetBucketNotificationsAsync(bucket).ConfigureAwait(false);

                bool haveRightConfiguration = false;
                currentNotifications.QueueConfigs.ForEach(qc =>
                {
                    if (qc.Queue == _options.WebhookARN
                    && qc.Events.Select(t => t.value).Contains(EventType.ObjectCreatedAll.value)
                    && qc.Events.Select(t => t.value).Contains(EventType.ObjectRemovedAll.value)
                    && (!_options.HookOnAccessedObjects || qc.Events.Select(t => t.value).Contains(EventType.ObjectAccessedAll.value))
                    )
                    {
                        haveRightConfiguration = true;
                    }
                });

                if (!haveRightConfiguration)
                {
                    //await Client.RemoveAllBucketNotificationsAsync(bucket).ConfigureAwait(false);

                    BucketNotification notification = new BucketNotification();

                    QueueConfig queueConfiguration = new QueueConfig(_options.WebhookARN);
                    var events = new List<EventType>() { EventType.ObjectCreatedAll, EventType.ObjectRemovedAll };
                    if (_options.HookOnAccessedObjects)
                        events.Add(EventType.ObjectAccessedAll);
                    queueConfiguration.AddEvents(events);

                    notification.AddQueue(queueConfiguration);

                    await Client.SetBucketNotificationsAsync(bucket,
                                                        notification);
                }
            }
        }

        [AutomaticRetry(Attempts = 5)]
        public async Task ProcessAsync(BucketNotificationPost bucketNotificationPost)
        {
            foreach (var record in bucketNotificationPost.Records)
            {
                foreach (var t in _webhooks)
                {
                    try
                    {
                        await t.ProcessAsync(this, record);
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine(e.Message, e.StackTrace);
                        throw; // throw it again so the Task runner will try to replay it.
                    }
                }
            }
        }
    }
}
