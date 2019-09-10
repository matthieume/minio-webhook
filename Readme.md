# Minio-Webhook

Minio-Webhook is a platform to manage you [Minio](https://min.io/)'s webhooks.

Current version is providing thumbnail generation.

## Quick start
Clone project and run
```sh
docker-compose up -d
```

Task queue dashboard is available here http://localhost:9002/hangfire 

## Configuration
Update `appsettings.json` according to you needs:

```
"MinioSettings": {
    "Endpoint": "assets:9000",
    "AccessKey": "Use appsettings.production.json",
    "SecretKey": "Use appsettings.production.json",
    "WebhooksOptions": {
      "ThumbnailGeneratorWebhook": {
        "Buckets": [ "img" ],
        "ThumbnailSize": [ 100, 250, 500, 1024 ]
      }
    }
```

If you are not using the default docker configuration, or you are exposing the API somewhere else, you must edit `.docker/assets/.minio.sys/config.json` and change the webhook endpoint:
```
"webhook": {
	"1": {
		"enable": true,
		"endpoint": "http://webhooks:5000/api/Webhook",
		"queueDir": "/home/queue",
		"queueLimit": 10000
	}
}
```

## Development

Change webhook endpoint in `.docker/assets/.minio.sys/config.json` to:
```
 "endpoint": "http://host.docker.internal:5000/api/Webhook",
```

Change `appsettings.json` to point to your minio local instance.
```
"MinioSettings": {
	"Endpoint": "localhost:9001",
	"AccessKey": "devkey",
	"SecretKey": "devsecret",
}
```

Then run only the minio instance from the docker file:
```sh
docker-compose down && docker-compose up -d assets
```
### Adding new hooks
Create a new service class in `Services/Webhooks` implementing `IWebhook`, use ThumbnailGeneratorWebHook as a template.

Then register the service in `Startup.ConfigureServices`:
```
services.AddSingleton<IWebhook, MyNewWebHook>();
```

