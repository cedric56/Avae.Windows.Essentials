using Avalonia;
using Microsoft.Maui.ApplicationModel;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;

namespace Microsoft.Maui.Media
{
	partial class ScreenshotImplementation : IPlatformScreenshot, IScreenshot
	{
		public bool IsCaptureSupported =>
			true;

		public Task<IScreenshotResult> CaptureAsync()
		{
			var element = WindowStateManager.Default.GetActiveWindow(true);

			return CaptureAsync(element);
		}

		public Task<IScreenshotResult> CaptureAsync(Avalonia.Controls.Window window) =>
			CaptureAsync(window.Content as Visual);

		public async Task<IScreenshotResult> CaptureAsync(Visual element)
		{
			var bmp = new Avalonia.Media.Imaging.RenderTargetBitmap(new PixelSize((int)element.Bounds.Width, (int)element.Bounds.Height));

			// NOTE: Return to the main thread so we can access view properties such as
			//       width and height. Do not ConfigureAwait!
			bmp.Render(element);

			using var ms = new MemoryStream() ;
            bmp.Save(ms); // saves as PNG
			return new ScreenshotResult((int)element.Bounds.Width, (int)element.Bounds.Height, ms.ToArray(), 96, 96);
		}
	}

	partial class ScreenshotResult
	{
		readonly double _dpiX;
		readonly double _dpiY;
		readonly byte[] _bytes;

		internal ScreenshotResult(int width, int height, byte[] bytes, double dpiX, double dpiY)
		{
			Width = width;
			Height = height;
			_bytes = bytes;
			_dpiX = dpiX;
			_dpiY = dpiY;
		}

		public ScreenshotResult(int width, int height, IBuffer pixels)
		{
			Width = width;
			Height = height;
			_bytes = pixels.ToArray() ?? throw new ArgumentNullException(nameof(pixels));
			_dpiX = 96;
			_dpiY = 96;
		}

		async Task<Stream> PlatformOpenReadAsync(ScreenshotFormat format, int quality)
		{
			var ms = new InMemoryRandomAccessStream();
			await EncodeAsync(format, ms).ConfigureAwait(false);
			return ms.AsStreamForRead();
		}

		Task PlatformCopyToAsync(Stream destination, ScreenshotFormat format, int quality)
		{
			var ms = destination.AsRandomAccessStream();
			return EncodeAsync(format, ms);
		}

		Task<byte[]> PlatformToPixelBufferAsync() =>
			Task.FromResult(_bytes);

		async Task EncodeAsync(ScreenshotFormat format, IRandomAccessStream ms)
		{
			var f = ToBitmapEncoder(format);

			var encoder = await BitmapEncoder.CreateAsync(f, ms).AsTask().ConfigureAwait(false);
			encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore, (uint)Width, (uint)Height, _dpiX, _dpiY, _bytes);
			await encoder.FlushAsync().AsTask().ConfigureAwait(false);
		}

		static Guid ToBitmapEncoder(ScreenshotFormat format) =>
			format switch
			{
				ScreenshotFormat.Jpeg => BitmapEncoder.JpegEncoderId,
				ScreenshotFormat.Png => BitmapEncoder.PngEncoderId,
				_ => throw new ArgumentOutOfRangeException(nameof(format))
			};
	}
}
