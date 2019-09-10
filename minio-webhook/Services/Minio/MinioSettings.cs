namespace minio_webhook.Services.Minio
{
    public class MinioSettings
    {
        public string Endpoint { get; set; }
        public string AccessKey { get; set; } = "";
        public string SecretKey { get; set; } = "";
        public string Region { get; set; } = "";
        public string SessionToken { get; set; } = "";
        public bool WithSSL { get; set; } = false;
        public string WebhookARN { get; set; } = "arn:minio:sqs::1:webhook";
        public bool HookOnAccessedObjects { get; set; } = false;
        public int DelayBetweenWebhookRegistrationCheck { get; set; } = 5;
    }
}
