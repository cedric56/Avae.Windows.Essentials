#nullable enable
using System;
using Microsoft.UI.Dispatching;

namespace Microsoft.Maui.ApplicationModel
{
	public static partial class MainThread
	{
		static bool PlatformIsMainThread =>
			Avalonia.Threading.Dispatcher.UIThread.CheckAccess();

		static void PlatformBeginInvokeOnMainThread(Action action)
		{
			Avalonia.Threading.Dispatcher.UIThread.Invoke(action);
		}
	}
}
