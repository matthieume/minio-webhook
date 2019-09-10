using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace minio_webhook.Services
{
    public static class MediaService
    {
        public static Bitmap GetBipmapFromStream(Stream stream)
        {
            return (Bitmap)Bitmap.FromStream(stream);
        }

        public static Bitmap ResizeBitmap(Bitmap image, int maxWidth, int maxHeight)
        {
            var ratio = Math.Min((double)maxWidth / image.Width, (double)maxHeight / image.Height);

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            var newImage = new Bitmap(newWidth, newHeight);

            using (var graphics = Graphics.FromImage(newImage))
                graphics.DrawImage(image, 0, 0, newWidth, newHeight);

            return newImage;
        }

        public static ImageFormat MimeTypeToImageFormat(string mimeType)
        {

            if (mimeType.Equals("image/tiff"))
            {
                return (ImageFormat.Tiff);
            }
            else if (mimeType.Equals("image/gif"))
            {
                return (ImageFormat.Gif);
            }
            else if (mimeType.Equals("image/png"))
            {
                return (ImageFormat.Png);
            }
            else if (mimeType.Equals("image/jpeg") || mimeType.Equals("image/jpg"))
            {
                return (ImageFormat.Jpeg);
            }
            else if (mimeType.Equals("image/bmp"))
            {
                return (ImageFormat.Bmp);
            }
            else
            {
                return (null);
            }
        }

        public static string[] SupportedMimeTypes()
        {
            return new string[] { "image/tiff", "image/gif", "image/png", "image/jpeg", "image/jpg", "image/bmp"};
        }
    }
}
