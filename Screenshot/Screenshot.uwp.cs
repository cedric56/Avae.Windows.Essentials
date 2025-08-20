using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Media
{
	partial class ScreenshotImplementation : IPlatformScreenshot, IScreenshot
	{
        public bool IsCaptureSupported =>
          true;

        public Task<IScreenshotResult> CaptureAsync(Window window)
        {
            IScreenshotResult result = new ScreenshotResult(window);
            return Task.FromResult(result);
        }

        public Task<IScreenshotResult?> CaptureAsync(Visual element)
        {
            IScreenshotResult result = new ScreenshotResult(element);
            return Task.FromResult(result);
        }

        public Task<IScreenshotResult> CaptureAsync()
        {
            IScreenshotResult result = new ScreenshotResult(WindowStateManager.Default.GetActiveWindow(false));
            return Task.FromResult(result);
        }

        public static Task<MemoryStream> CaptureToStreamAsync(Visual visual, ScreenshotFormat format, int quality)
        {
            var pixelSize = new PixelSize((int)visual.Bounds.Width, (int)visual.Bounds.Height);
            var dpi = new Vector(96, 96);
            var bitmap = new RenderTargetBitmap(pixelSize, dpi);

            bitmap.Render(visual);

            var stream = new MemoryStream();
            switch (format)
            {
                case ScreenshotFormat.Png:
                    bitmap.Save(stream, quality);
                    break;
                default:
                    throw new NotSupportedException("Unsupported format.");
            }

            stream.Position = 0;
            return Task.FromResult(stream);
        }

        
    }

    partial class ScreenshotResult : IScreenshotResult
    {
        Visual visual;

        internal ScreenshotResult(Visual visual)
        {
            Height = (int)visual.Bounds.Height;
            Width = (int)visual.Bounds.Width;

            this.visual = visual;
        }

        async Task<Stream> PlatformOpenReadAsync(ScreenshotFormat format, int quality) =>
            await ScreenshotImplementation.CaptureToStreamAsync(visual, format, quality);

        public async Task PlatformCopyToAsync(Stream destination, ScreenshotFormat format, int quality)
        {
            var sourceStream = await PlatformOpenReadAsync(format, quality);
            await sourceStream.CopyToAsync(destination);
        }
    }
}
