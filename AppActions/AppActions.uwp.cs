using Avalonia.Controls;
using Avalonia.Platform;
using System.Text;
using Windows.UI.StartScreen;

namespace Microsoft.Maui.ApplicationModel
{
	class AppActionsImplementation : IAppActions, IPlatformAppActions
	{
		public bool IsSupported => true;

		public async Task<IEnumerable<AppAction>> GetAsync()
		{
			// Load existing items
			var jumpList = await JumpList.LoadCurrentAsync();

			var actions = new List<AppAction>();
			foreach (var item in jumpList.Items)
				actions.Add(item.ToAction());

			return actions;
		}

		public async Task SetAsync(IEnumerable<AppAction> actions)
		{
			// Load existing items
			var jumpList = await JumpList.LoadCurrentAsync();

			// Set as custom, not system or frequent
			jumpList.SystemGroupKind = JumpListSystemGroupKind.None;

			// Clear the existing items
			jumpList.Items.Clear();

			// Add each action
			foreach (var a in actions)
				jumpList.Items.Add(a.ToJumpListItem());

			// Save the changes
			await jumpList.SaveAsync();
		}

		public event EventHandler<AppActionEventArgs> AppActionActivated;

		public Task OnLaunched(AppAction a)
		{
			AppActionActivated?.Invoke(null, new AppActionEventArgs(a));
			return Task.CompletedTask;
		}
	}

	static partial class AppActionsExtensions
	{
		internal const string AppActionPrefix = "XE_APP_ACTIONS-";

		internal const string iconDirectory = "";
		internal const string iconExtension = ".png";

		internal static string ArgumentsToId(this string arguments)
		{
			if (arguments?.StartsWith(AppActionPrefix) ?? false)
				return Encoding.Default.GetString(Convert.FromBase64String(arguments.Substring(AppActionPrefix.Length)));

			return default;
		}

		internal static AppAction ToAction(this JumpListItem item)
			=> new AppAction(ArgumentsToId(item.Arguments), item.DisplayName, item.Description);

		internal static JumpListItem ToJumpListItem(this AppAction action)
		{
			var id = AppActionPrefix + Convert.ToBase64String(Encoding.Default.GetBytes(action.Id));
			var item = JumpListItem.CreateWithArguments(id, action.Title);

			if (!string.IsNullOrEmpty(action.Subtitle))
				item.Description = action.Subtitle;

			if (!string.IsNullOrEmpty(action.Icon))
			{
				//var dir = iconDirectory?.Trim('/', '\\').Replace('\\', '/');
				//if (!string.IsNullOrEmpty(dir))
				//	dir += "/";

				//var ext = iconExtension;
				//if (!string.IsNullOrEmpty(ext) && !ext.StartsWith("."))
				//	ext = "." + ext;

				try
				{
                    //string logoPath = action.Icon.TrimStart('/', '\\').Replace("\\", "/");

                    // Check if the path is already an absolute URI
      //              if (Uri.IsWellFormedUriString(logoPath, UriKind.Absolute))
      //              {
      //                  item.Logo = new Uri(logoPath, UriKind.Absolute);
      //              }
      //              else
      //              {
      //                  // Construct avares:// URI for embedded resource
      //                  string assemblyName = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;
      //                  string resourcePath = $"ms-appx:///{assemblyName}/{logoPath}";

      //                  // Validate URI format
      //                  if (!Uri.IsWellFormedUriString(resourcePath, UriKind.Absolute))
      //                  {
      //                      throw new UriFormatException($"Constructed URI is invalid: {resourcePath}");
      //                  }

						//// Attempt to verify resource existence (optional, for debugging)
						////var resourceStream = System.Reflection.Assembly.GetEntryAssembly()
						////    .GetManifestResourceStream($"{assemblyName}.{logoPath.Replace("/", ".")}");
						////if (resourceStream == null)
						////{
						////    throw new InvalidOperationException($"Resource not found: {logoPath}");
						////}

						//var stream = AssetLoader.GetAssets(new Uri(resourcePath), null);


						item.Logo = new Uri($"ms-appx:///{action.Icon}");							
                    //}

                    //string logoPath = action.Icon;

                    //// If action.Icon is a relative path, prepend the appropriate scheme
                    //if (!Uri.IsWellFormedUriString(logoPath, UriKind.Absolute))
                    //{
                    //    // Assuming the logo is a resource in the app package
                    //    logoPath = $"avares://{logoPath.TrimStart('/', '\\')}";
                    //}

                    //item.Logo = new Uri(logoPath, UriKind.Absolute);

                    //item.Logo = new Uri(action.Icon);// $"ms-appx:///{dir}{action.Icon}{ext}");
                }
				catch(Exception ex)
				{

				}
			}

			return item;
		}
	}
}
