using AppKit;
using Foundation;
using System;
using System.Threading;
using Eddie.Core;

namespace Eddie.UI.Cocoa.Osx
{
    public partial class WindowSplashController : NSWindowController
    {
        private Timer m_timer = null;
        private bool m_closePending = false;

        public WindowSplashController(IntPtr handle) : base(handle)
        {
        }

        [Export("initWithCoder:")]
        public WindowSplashController(NSCoder coder) : base(coder)
        {
        }

        public WindowSplashController() : base("WindowSplash")
        {
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();
        }

        public new WindowSplash Window
        {
            get { return (WindowSplash)base.Window; }
        }

        private void OnTimerElapsed(Object state)
        {
            m_timer.Dispose();

            UiClient.Instance.SplashWindow = null;
            new NSObject().InvokeOnMainThread(() => // BeginInvokeOnMainThread
            {
                Close();
            });
        }

        public void SetStatus(string t)
        {
            new NSObject().InvokeOnMainThread(() => // BeginInvokeOnMainThread
            {
                LblStatus.StringValue = t;
            });
        }

        public void MessageError(string message)
        {
            new NSObject().InvokeOnMainThread(() => // BeginInvokeOnMainThread
            {
                GuiUtils.MessageBoxError(message);
            });
        }

        public void RequestShow()
        {
            new NSObject().InvokeOnMainThread(() => // BeginInvokeOnMainThread
            {
                GuiUtils.ShowWindowWithFocus(this, null);
            });
        }

        public void RequestCloseForReady()
        {
            SetStatus(LanguageManager.GetText("Ready"));

			if (m_closePending)
                return;

            new NSObject().InvokeOnMainThread(() => // BeginInvokeOnMainThread
            {
                m_closePending = true;

                m_timer = new Timer(OnTimerElapsed, null, 1000, Timeout.Infinite);
            });
        }

        public string AskUnlockPassword(bool authFailed)
        {
            string result = "";
            new NSObject().InvokeOnMainThread(() => // BeginInvokeOnMainThread
            {
                WindowUnlockController w = new WindowUnlockController();
                w.AuthFailed = authFailed;
                NSApplication.SharedApplication.RunModalForWindow(w.Window);
                result = w.Body;
            });
            return result;
        }
    }
}
