using System;

using Foundation;
using AppKit;

using Eddie.Common;
using Eddie.Core;

namespace Eddie.UI.Cocoa.Osx
{
    public partial class WindowShellExternalPermissionController : NSWindowController
    {
        public Json Data;
        public Json Answer;

        public WindowShellExternalPermissionController(IntPtr handle) : base(handle)
        {
        }

        [Export("initWithCoder:")]
        public WindowShellExternalPermissionController(NSCoder coder) : base(coder)
        {
        }

        public WindowShellExternalPermissionController() : base("WindowShellExternalPermission")
        {
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            Window.Title = Constants.Name + " - " + LanguageManager.GetText("WindowsShellExternalPermissionTitle");

			LblMessage.StringValue = LanguageManager.GetText("WindowsShellExternalPermissionTop", Data["path"].Value as string);

            CmdNo.Title = LanguageManager.GetText("WindowsShellExternalPermissionNo");
			CmdYes.Title = LanguageManager.GetText("WindowsShellExternalPermissionYes");
			CmdRuleSign.Title = LanguageManager.GetText("WindowsShellExternalPermissionRuleSign", Data["sign -id"].Value as string);
            if ((Data["sign-id"].Value as string).StartsWith("No: "))
                CmdRuleSign.Enabled = false;
            CmdRuleHash.Title = LanguageManager.GetText("WindowsShellExternalPermissionRuleHash", Data["sha256"].Value as string);
            CmdRulePath.Title = LanguageManager.GetText("WindowsShellExternalPermissionRulePath", Data["path"].Value as string);
            CmdRuleAll.Title = LanguageManager.GetText("WindowsShellExternalPermissionRuleAll");

			Answer = new Json();
            Answer["allow"].Value = false;

            CmdNo.Activated += (object sender, EventArgs e) => {
                Answer["allow"].Value = false;
                DialogClose();
            };

            CmdYes.Activated += (object sender, EventArgs e) => {
                Answer["allow"].Value = true;
                DialogClose();
            };

            CmdRuleSign.Activated += (object sender, EventArgs e) => {
                Answer.RemoveKey("allow");
                Answer["type"].Value = "sign";
                Answer["id"].Value = Data["sign-id"].Value;
                DialogClose();
            };

            CmdRuleHash.Activated += (object sender, EventArgs e) => {
                Answer.RemoveKey("allow");
                Answer["type"].Value = "sha256";
                Answer["hash"].Value = Data["sha256"].Value;
                DialogClose();
            };

            CmdRulePath.Activated += (object sender, EventArgs e) => {
                Answer.RemoveKey("allow");
                Answer["type"].Value = "path";
                Answer["path"].Value = Data["path"].Value;
                DialogClose();
            };

            CmdRuleAll.Activated += (object sender, EventArgs e) => {
                Answer.RemoveKey("allow");
                Answer["type"].Value = "all";
                DialogClose();
            };
        }

        private void DialogClose()
        {
            Window.Close();
            NSApplication.SharedApplication.StopModal();
        }

        public new WindowShellExternalPermission Window
        {
            get { return (WindowShellExternalPermission)base.Window; }
        }
    }
}
