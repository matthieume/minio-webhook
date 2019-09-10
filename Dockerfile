FROM mcr.microsoft.com/dotnet/core/sdk:3.0

# https://github.com/dotnet/corefx/issues/25102
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
    libc6-dev \
    apt-utils \
    libgdiplus
#RUN ln -s /usr/lib/libgdiplus.so/usr/lib/gdiplus.dll

EXPOSE 5000

WORKDIR /app

HEALTHCHECK --interval=5s --timeout=3s --retries=3 CMD curl -f http://localhost/hc || exit 1

CMD ["dotnet", "minio-webhook.dll"]