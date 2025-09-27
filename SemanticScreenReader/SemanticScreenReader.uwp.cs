using Microsoft.Maui.ApplicationModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Microsoft.Maui.Accessibility
{
    partial class SemanticScreenReaderImplementation : ISemanticScreenReader
    {
        [Flags]
        public enum ProviderOptions
        {
            ClientSideProvider = 0x0001,
            ServerSideProvider = 0x0002,
            NonClientAreaProvider = 0x0004,
            OverrideProvider = 0x0008,
            ProviderOwnsSetFocus = 0x0010,
            UseComThreading = 0x0020
        }

        // AutomationNotificationKind (from UIA)
        public enum AutomationNotificationKind
        {
            ItemAdded = 0,
            ItemRemoved = 1,
            ActionCompleted = 2,
            ActionAborted = 3,
            Other = 4
        }

        // AutomationNotificationProcessing
        public enum AutomationNotificationProcessing
        {
            ImportantAll = 0,
            ImportantMostRecent = 1,
            All = 2,
            MostRecent = 3,
            CurrentThenMostRecent = 4
        }

        [ComImport]
        [TypeLibType(256)]
        [InterfaceType(1)]
        [Guid("D6DD68D1-86FD-4332-8666-9ABEDEA2D24C")]
        public interface IRawElementProviderSimple
        {
            [DispId(1610678272)]
            ProviderOptions ProviderOptions
            {
                [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                get;
            }

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [return: MarshalAs(UnmanagedType.IUnknown)]
            object GetPatternProvider([In] int patternId);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetPropertyValue([In] int propertyId);

            [DispId(1610678275)]
            IRawElementProviderSimple HostRawElementProvider
            {
                [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                [return: MarshalAs(UnmanagedType.Interface)]
                get;
            }
        }

        [DllImport("UIAutomationCore.dll", CharSet = CharSet.Unicode)]
        private static extern int UiaRaiseNotificationEvent(
    IRawElementProviderSimple provider,
    AutomationNotificationKind notificationKind,
    AutomationNotificationProcessing notificationProcessing,
    [MarshalAs(UnmanagedType.BStr)] string displayString,
    [MarshalAs(UnmanagedType.BStr)] string activityId);

        [DllImport("UIAutomationCore.dll")]
        public static extern int UiaHostProviderFromHwnd(IntPtr hwnd, [MarshalAs(UnmanagedType.Interface)] out IRawElementProviderSimple provider);

        public class SimpleNotificationProvider(IRawElementProviderSimple simple) : IRawElementProviderSimple
        {
            public ProviderOptions ProviderOptions => ProviderOptions.ServerSideProvider;

            public object GetPatternProvider(int patternId) => null;

            public object GetPropertyValue(int propertyId)
            {
                return null;
            }

            public IRawElementProviderSimple HostRawElementProvider
            {
                get
                {
                    return simple;
                }
            }
        }
        public void Announce(string text)
        {
            // Get HWND for your active window
            IntPtr windowHwnd = WindowStateManager.Default.GetActiveWindowHandle(false);
            if (windowHwnd == IntPtr.Zero)
                return;

            UiaHostProviderFromHwnd(windowHwnd, out IRawElementProviderSimple simple);

            UiaRaiseNotificationEvent(
                 new SimpleNotificationProvider(simple),
                AutomationNotificationKind.ActionAborted,
                AutomationNotificationProcessing.ImportantMostRecent,
                text,
                string.Empty);
        }
    }
}
