#nullable enable
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Microsoft.Maui.Devices.Sensors;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.Maui.ApplicationModel
{
	/// <summary>
	/// A static class that contains platform-specific helper methods.
	/// </summary>
	public static class Platform
	{
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, ref COPYDATASTRUCT lParam);

        const int WM_COPYDATA = 0x004A;

        [StructLayout(LayoutKind.Sequential)]
        public struct COPYDATASTRUCT
        {
            public IntPtr dwData;   // custom identifier
            public int cbData;      // size of data in bytes
            public IntPtr lpData;   // pointer to data
        }

        private static void Patch()
        {
            var proc = Process.GetCurrentProcess();
            //get all other (possible) running instances
            Process[] processes = Process.GetProcessesByName(proc.ProcessName);

            if (processes.Length > 1)
            {
                //iterate through all running target applications      
                foreach (Process p in processes)
                {
                    if (p.Id != proc.Id)
                    {
                        var args = Environment.GetCommandLineArgs();
                        var arg = args.FirstOrDefault(a => a.StartsWith(AppActionsExtensions.AppActionPrefix));
                        if (arg != null)
                        {
                            SendMessage(p.MainWindowHandle, arg);
                        }
                    }
                }
            }

        }

        private static void SendMessage(IntPtr handle, string args)
        {
            IntPtr lpData = Marshal.StringToHGlobalUni(args);

            var cds = new COPYDATASTRUCT
            {
                dwData = IntPtr.Zero, // you can put your message type here
                cbData = (args.Length) * 2, // Unicode bytes, + null terminator
                lpData = lpData
            };

            SendMessage(handle, WM_COPYDATA, IntPtr.Zero, ref cds);

            Marshal.FreeHGlobal(lpData);

            Environment.Exit(0);
        }

        public static void Register()
        {
            if (Application.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Startup += (_, _) =>
                {
                    Win32Properties.AddWndProcHookCallback(desktop.MainWindow!, WndProc);
                };
            }
        }

        private async static Task Decode(IntPtr msg)
        {
            var cds = Marshal.PtrToStructure<COPYDATASTRUCT>(msg);
            string received = Marshal.PtrToStringUni(cds.lpData, cds.cbData / 2);
            var args = Decode(received);
            var actions = await AppActions.GetAsync();
            var action = actions.FirstOrDefault(a => a.Id == args);
            if (action != null)
            {
                OnLaunched(action);
            }
        }

        private static string Decode(string args)
        {
            return Encoding.Default.GetString(Convert.FromBase64String(args.Substring(AppActionsExtensions.AppActionPrefix.Length)));
        }

        private static IntPtr WndProc(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_COPYDATA)
            {
                MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Decode(lParam);
                });
                handled = true;
            }

            return IntPtr.Zero;
        }

        static List<Window> _windows = new List<Window>();

        public static void Initialize()
        {
            Patch();
            Register();
            Window.GotFocusEvent.AddClassHandler(typeof(Window), (sender, args) =>
            {
                var window = (Window)sender!;
                OnActivated(window);
            });
            Window.WindowOpenedEvent.AddClassHandler(typeof(Window), (sender, args) =>
            {
                var window = (Window)sender!;
                if (!_windows.Contains(window))
                {
                    _windows.Add(window);
                    OnActivated(window);
                }
            });
            Window.WindowClosedEvent.AddClassHandler(typeof(Window), (sender, _) =>
            { 
                var window = (Window)sender!;
                _windows.Remove(window);
                if (_windows.Count > 0)
                    OnActivated(_windows.Last());
            });
        }

        /// <summary>
        /// Gets or sets the map service API key for this platform.
        /// </summary>
        public static string? MapServiceToken
		{
			get => Geocoding.Default.GetMapServiceToken();
			set => Geocoding.Default.SetMapServiceToken(value);
		}

		/// <inheritdoc cref="IPlatformAppActions.OnLaunched(AppAction)"/>
		public static void OnLaunched(AppAction a) =>
			AppActions.Current.OnLaunched(a);

		/// <inheritdoc cref="IWindowStateManager.OnPlatformWindowInitialized(UI.Xaml.Window)"/>
		public static void OnPlatformWindowInitialized(Avalonia.Controls.Window window) =>
			WindowStateManager.Default.OnPlatformWindowInitialized(window);

		/// <inheritdoc cref="IWindowStateManager.OnActivated(UI.Xaml.Window, UI.Xaml.WindowActivatedEventArgs)"/>
		public static void OnActivated(Avalonia.Controls.Window window) =>
			WindowStateManager.Default.OnActivated(window);
	}
}
