using minio_webhook.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace minio_webhook.Services.Webhooks
{
    public interface IWebhook
    {
        List<string> GetBucketsToMonitor();
        Task ProcessAsync(Services.Minio.Minio minio, Record record);
    }
}
