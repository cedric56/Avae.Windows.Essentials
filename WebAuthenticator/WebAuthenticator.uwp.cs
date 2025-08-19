using Microsoft.Maui.ApplicationModel;
using System.Net;
using System.Text;

namespace Microsoft.Maui.Authentication
{
	partial class WebAuthenticatorImplementation : IWebAuthenticator
	{
		public async Task<WebAuthenticatorResult> AuthenticateAsync(WebAuthenticatorOptions webAuthenticatorOptions)
		{
            if (webAuthenticatorOptions.CallbackUrl.Scheme == "http" || webAuthenticatorOptions.CallbackUrl.Scheme == "https")
            {
                using var listener = new HttpListener();

                listener.Prefixes.Add(webAuthenticatorOptions.CallbackUrl.OriginalString);
                listener.Start();

                await Launcher.OpenAsync(webAuthenticatorOptions.Url);

                var cancelToken = new CancellationTokenSource();
                var context = await listener.GetContextAsync().WaitAsync(TimeSpan.FromMinutes(1), cancelToken.Token);

                var response = context.Response;
                string responseString = "<html><head><style>h1{color:green;font-size:20px;}</style></head><body><h1>You can now close this window.</h1></body></html>";
                var buffer = Encoding.UTF8.GetBytes(responseString);
                response.ContentLength64 = buffer.Length;
                var responseOutput = response.OutputStream;
                await responseOutput.WriteAsync(buffer, 0, buffer.Length);
                responseOutput.Close();
                listener.Stop();

                if (webAuthenticatorOptions.ResponseDecoder is not null)
                {
                    var dictionary = webAuthenticatorOptions.ResponseDecoder.DecodeResponse(context.Request.Url);
                    return new WebAuthenticatorResult(dictionary);
                }

                return new WebAuthenticatorResult(context.Request.Url);
            }

            throw new PlatformNotSupportedException("This implementation of WebAuthenticator does not support Windows. See https://github.com/microsoft/WindowsAppSDK/issues/441 for more details.");
		}
	}
}
