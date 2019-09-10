using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace minio_webhook.Models
{
    public partial class BucketNotificationPost
    {
        public string EventName { get; set; }

        public string Key { get; set; }

        public Record[] Records { get; set; }
    }

    public partial class Record
    {
        [JsonPropertyName("eventVersion")]
        public string EventVersion { get; set; }

        [JsonPropertyName("eventSource")]
        public string EventSource { get; set; }

        [JsonPropertyName("awsRegion")]
        public string AwsRegion { get; set; }

        [JsonPropertyName("eventTime")]
        public DateTimeOffset? EventTime { get; set; }

        [JsonPropertyName("eventName")]
        public string EventName { get; set; }

        [JsonPropertyName("userIdentity")]
        public ErIdentity UserIdentity { get; set; }

        [JsonPropertyName("requestParameters")]
        public RequestParameters RequestParameters { get; set; }

        [JsonPropertyName("responseElements")]
        public ResponseElements ResponseElements { get; set; }

        [JsonPropertyName("s3")]
        public S3 S3 { get; set; }

        [JsonPropertyName("source")]
        public Source Source { get; set; }
    }

    public partial class RequestParameters
    {
        [JsonPropertyName("accessKey")]
        public string AccessKey { get; set; }

        [JsonPropertyName("region")]
        public string Region { get; set; }

        [JsonPropertyName("sourceIPAddress")]
        public string SourceIpAddress { get; set; }
    }

    public partial class ResponseElements
    {
        [JsonPropertyName("x-amz-request-id")]
        public string XAmzRequestId { get; set; }

        [JsonPropertyName("x-minio-deployment-id")]
        public Guid? XMinioDeploymentId { get; set; }

        [JsonPropertyName("x-minio-origin-endpoint")]
        public Uri XMinioOriginEndpoint { get; set; }
    }

    public partial class S3
    {
        [JsonPropertyName("s3SchemaVersion")]
        public string S3SchemaVersion { get; set; }

        [JsonPropertyName("configurationId")]
        public string ConfigurationId { get; set; }

        [JsonPropertyName("bucket")]
        public Bucket Bucket { get; set; }

        [JsonPropertyName("object")]
        public Object Object { get; set; }
    }

    public partial class Bucket
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("ownerIdentity")]
        public ErIdentity OwnerIdentity { get; set; }

        [JsonPropertyName("arn")]
        public string Arn { get; set; }
    }

    public partial class ErIdentity
    {
        [JsonPropertyName("principalId")]
        public string PrincipalId { get; set; }
    }

    public partial class Object
    {
        [JsonPropertyName("key")]
        public string Key { get; set; }

        [JsonPropertyName("size")]
        public long? Size { get; set; }

        [JsonPropertyName("eTag")]
        public string ETag { get; set; }

        [JsonPropertyName("contentType")]
        public string ContentType { get; set; }

        [JsonPropertyName("userMetadata")]
        public Dictionary<string, string> UserMetadata { get; set; }

        [JsonPropertyName("versionId")]
        public string VersionId { get; set; }

        [JsonPropertyName("sequencer")]
        public string Sequencer { get; set; }
    }


    public partial class Source
    {
        [JsonPropertyName("host")]
        public string Host { get; set; }

        [JsonPropertyName("port")]
        public string Port { get; set; }

        [JsonPropertyName("userAgent")]
        public string UserAgent { get; set; }
    }
}
