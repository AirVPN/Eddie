using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;
using AirVPN.Core;

namespace AirVPN.UI.Osx
{
	public partial class WindowPreferencesController : MonoMac.AppKit.NSWindowController
	{
		private string m_mode_protocol;
		private int m_mode_port;
		private int m_mode_alternate;

		#region Constructors
		// Called when created from unmanaged code
		public WindowPreferencesController (IntPtr handle) : base (handle)
		{
			Initialize ();
		}
		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public WindowPreferencesController (NSCoder coder) : base (coder)
		{
			Initialize ();
		}
		// Call to load from the XIB/NIB file
		public WindowPreferencesController () : base ("WindowPreferences")
		{
			Initialize ();
		}
		// Shared initialization code
		void Initialize ()
		{
		}
		#endregion
		//strongly typed window accessor
		public new WindowPreferences Window {
			get {
				return (WindowPreferences)base.Window;
			}
		}

		public override void AwakeFromNib()
		{
			base.AwakeFromNib ();

			Window.Title = Constants.Name + " - " + Messages.WindowsSettingsTitle;

			ReadOptions ();

			EnableIde ();



			CmdSave.Activated += (object sender, EventArgs e) => {
				SaveOptions ();
				Close ();
			};

			CmdCancel.Activated += (object sender, EventArgs e) => {
				Close ();
			};

			CmdGeneralTos.Activated += (object sender, EventArgs e) => {
				WindowTosController tos = new WindowTosController ();
				tos.Window.ReleasedWhenClosed = true;
				NSApplication.SharedApplication.RunModalForWindow (tos.Window);
				tos.Window.Close ();
			};

			// Modes
			ChkModeUdp443.Activated += (object sender, EventArgs e) => {
				m_mode_protocol = "UDP";
				m_mode_port = 443;
				m_mode_alternate = 0;
				ChangeMode();
			};
			ChkModeTcp443.Activated += (object sender, EventArgs e) => {
				m_mode_protocol = "TCP";
				m_mode_port = 443;
				m_mode_alternate = 0;
				ChangeMode();
			};

			ChkModeUdp80.Activated += (object sender, EventArgs e) =>
			{
				m_mode_protocol = "UDP";
				m_mode_port = 80;
				m_mode_alternate = 0;
				ChangeMode();
			};

			ChkModeTcp80.Activated += (object sender, EventArgs e) =>
			{
				m_mode_protocol = "TCP";
				m_mode_port = 80;
				m_mode_alternate = 0;
				ChangeMode();
			};

			ChkModeUdp53.Activated += (object sender, EventArgs e) =>
			{
				m_mode_protocol = "UDP";
				m_mode_port = 53;
				m_mode_alternate = 0;
				ChangeMode();
			};

			ChkModeTcp53.Activated += (object sender, EventArgs e) =>
			{
				m_mode_protocol = "TCP";
				m_mode_port = 53;
				m_mode_alternate = 0;
				ChangeMode();
			};

			ChkModeUdp2018.Activated += (object sender, EventArgs e) =>
			{
				m_mode_protocol = "UDP";
				m_mode_port = 2018;
				m_mode_alternate = 0;
				ChangeMode();
			};

			ChkModeTcp2018.Activated += (object sender, EventArgs e) =>
			{
				m_mode_protocol = "TCP";
				m_mode_port = 2018;
				m_mode_alternate = 0;
				ChangeMode();
			};



			ChkModeUdp443Alt.Activated += (object sender, EventArgs e) =>
			{
				m_mode_protocol = "UDP";
				m_mode_port = 443;
				m_mode_alternate = 1;
				ChangeMode();
			};

			ChkModeUdp80Alt.Activated += (object sender, EventArgs e) =>
			{
				m_mode_protocol = "UDP";
				m_mode_port = 80;
				m_mode_alternate = 1;
				ChangeMode();
			};

			ChkModeUdp53Alt.Activated += (object sender, EventArgs e) =>
			{
				m_mode_protocol = "UDP";
				m_mode_port = 53;
				m_mode_alternate = 1;
				ChangeMode();
			};

			ChkModeUdp2018Alt.Activated += (object sender, EventArgs e) =>
			{
				m_mode_protocol = "UDP";
				m_mode_port = 2018;
				m_mode_alternate = 1;
				ChangeMode();
			};

			ChkModeTcp2018Alt.Activated += (object sender, EventArgs e) =>
			{
				m_mode_protocol = "TCP";
				m_mode_port = 2018;
				m_mode_alternate = 1;
				ChangeMode();
			};


			ChkModeSsh22.Activated += (object sender, EventArgs e) => {
				m_mode_protocol = "SSH";
				m_mode_port = 22;
				m_mode_alternate = 0;
				ChangeMode();
			};
			ChkModeSsh22Alt.Activated += (object sender, EventArgs e) =>
			{
				m_mode_protocol = "SSH";
				m_mode_port = 22;
				m_mode_alternate = 1;
				ChangeMode();
			};
			ChkModeSsh80.Activated += (object sender, EventArgs e) =>
			{
				m_mode_protocol = "SSH";
				m_mode_port = 80;
				m_mode_alternate = 1;
				ChangeMode();
			};
			ChkModeSsh53.Activated += (object sender, EventArgs e) =>
			{
				m_mode_protocol = "SSH";
				m_mode_port = 53;
				m_mode_alternate = 1;
				ChangeMode();
			};
			ChkModeSsl443.Activated += (object sender, EventArgs e) => {
				m_mode_protocol = "SSL";
				m_mode_port = 443;
				m_mode_alternate = 1;
				ChangeMode();
			};
		}

		void ChangeMode()
		{
			GuiUtils.SetCheck (ChkModeUdp443, ((m_mode_protocol == "UDP") && (m_mode_port == 443) && (m_mode_alternate == 0)));
			GuiUtils.SetCheck (ChkModeTcp443, ((m_mode_protocol == "TCP") && (m_mode_port == 443) && (m_mode_alternate == 0)));
			GuiUtils.SetCheck (ChkModeUdp80, ((m_mode_protocol == "UDP") && (m_mode_port == 80) && (m_mode_alternate == 0)));
			GuiUtils.SetCheck (ChkModeTcp80, ((m_mode_protocol == "TCP") && (m_mode_port == 80) && (m_mode_alternate == 0)));
			GuiUtils.SetCheck (ChkModeUdp53, ((m_mode_protocol == "UDP") && (m_mode_port == 53) && (m_mode_alternate == 0)));
			GuiUtils.SetCheck (ChkModeTcp53, ((m_mode_protocol == "TCP") && (m_mode_port == 53) && (m_mode_alternate == 0)));
			GuiUtils.SetCheck (ChkModeUdp2018, ((m_mode_protocol == "UDP") && (m_mode_port == 2018) && (m_mode_alternate == 0)));
			GuiUtils.SetCheck (ChkModeTcp2018, ((m_mode_protocol == "TCP") && (m_mode_port == 2018) && (m_mode_alternate == 0)));

			GuiUtils.SetCheck(ChkModeUdp443Alt, ((m_mode_protocol == "UDP") && (m_mode_port == 443) && (m_mode_alternate == 1)));
			GuiUtils.SetCheck(ChkModeUdp80Alt, ((m_mode_protocol == "UDP") && (m_mode_port == 80) && (m_mode_alternate == 1)));
			GuiUtils.SetCheck(ChkModeUdp53Alt, ((m_mode_protocol == "UDP") && (m_mode_port == 53) && (m_mode_alternate == 1)));
			GuiUtils.SetCheck(ChkModeUdp2018Alt, ((m_mode_protocol == "UDP") && (m_mode_port == 2018) && (m_mode_alternate == 1)));
			GuiUtils.SetCheck(ChkModeTcp2018Alt, ((m_mode_protocol == "TCP") && (m_mode_port == 2018) && (m_mode_alternate == 1)));
			
			GuiUtils.SetCheck (ChkModeSsh22, ((m_mode_protocol == "SSH") && (m_mode_port == 22) && (m_mode_alternate == 0)));
			GuiUtils.SetCheck (ChkModeSsh22Alt, ((m_mode_protocol == "SSH") && (m_mode_port == 22) && (m_mode_alternate == 1)));
			GuiUtils.SetCheck (ChkModeSsh80, ((m_mode_protocol == "SSH") && (m_mode_port == 80) && (m_mode_alternate == 1)));
			GuiUtils.SetCheck (ChkModeSsh53, ((m_mode_protocol == "SSH") && (m_mode_port == 53) && (m_mode_alternate == 1)));
			GuiUtils.SetCheck (ChkModeSsl443, ((m_mode_protocol == "SSL") && (m_mode_port == 443) && (m_mode_alternate == 1)));
		}

		void ReadOptions()
		{
			Storage s = Engine.Instance.Storage;

			// General

			GuiUtils.SetCheck (ChkAutoStart, s.GetBool ("connect")); 
			GuiUtils.SetCheck (ChkGeneralStartLast, s.GetBool("servers.startlast"));
			GuiUtils.SetCheck (ChkGeneralOsxNotifications, s.GetBool ("gui.osx.notifications"));

			// Mode
			m_mode_protocol = s.Get ("mode.protocol").ToUpperInvariant ();
			m_mode_port = s.GetInt ("mode.port");
			m_mode_alternate = s.GetInt ("mode.alt");
			ChangeMode ();

			// Proxy

			GuiUtils.SetSelected (CboProxyType, s.Get("proxy.mode"));
			TxtProxyHost.StringValue = s.Get ("proxy.host");
			TxtProxyPort.StringValue = s.Get ("proxy.port");
			GuiUtils.SetSelected (CboProxyAuthentication, s.Get ("proxy.auth"));
			TxtProxyLogin.StringValue = s.Get ("proxy.login");
			TxtProxyPassword.StringValue = s.Get ("proxy.password");

			// Advanced
		}

		void SaveOptions()
		{
			Storage s = Engine.Instance.Storage;

			// General

			s.SetBool ("connect", GuiUtils.GetCheck (ChkAutoStart));
			s.SetBool ("servers.startlast", GuiUtils.GetCheck (ChkGeneralStartLast));
			s.SetBool ("gui.osx.notifications", GuiUtils.GetCheck (ChkGeneralOsxNotifications));

			// Mode

			s.Set ("mode.protocol", m_mode_protocol);
			s.SetInt ("mode.port", m_mode_port);
			s.SetInt ("mode.alt", m_mode_alternate);

			// Proxy

			s.Set ("proxy.mode", GuiUtils.GetSelected (CboProxyType));
			s.Set ("proxy.host", TxtProxyHost.StringValue);
			s.Set ("proxy.port", TxtProxyPort.StringValue);
			s.Set ("proxy.auth", GuiUtils.GetSelected (CboProxyAuthentication));
			s.Set ("proxy.login", TxtProxyLogin.StringValue);
			s.Set ("proxy.password", TxtProxyPassword.StringValue);

			// Routes

			// Advanced
		}

		void EnableIde()
		{
		}
	}
}

