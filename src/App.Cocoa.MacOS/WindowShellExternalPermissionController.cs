using System;

using Foundation;
using AppKit;

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

            Window.Title = Constants.Name + " - " + LanguageManager.GetText(LanguageItems.WindowsExecExternalPermissionTitle);

			LblMessage.StringValue = LanguageManager.GetText(LanguageItems.WindowsExecExternalPermissionTop, Data["path"].Value as string);

            CmdNo.Title = LanguageManager.GetText(LanguageItems.WindowsExecExternalPermissionNo);
			CmdYes.Title = LanguageManager.GetText(LanguageItems.WindowsExecExternalPermissionYes);
			CmdRuleSign.Title = LanguageManager.GetText(LanguageItems.WindowsExecExternalPermissionRuleSign, Data["sign -id"].Value as string);
            if ((Data["sign-id"].Value as string).StartsWith("No: "))
                GuiUtils.SetEnabled(CmdRuleSign, false);
            CmdRuleHash.Title = LanguageManager.GetText(LanguageItems.WindowsExecExternalPermissionRuleHash, Data["sha256"].Value as string);
            CmdRulePath.Title = LanguageManager.GetText(LanguageItems.WindowsExecExternalPermissionRulePath, Data["path"].Value as string);
            CmdRuleAll.Title = LanguageManager.GetText(LanguageItems.WindowsExecExternalPermissionRuleAll);

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
