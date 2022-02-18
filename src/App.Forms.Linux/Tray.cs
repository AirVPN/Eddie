using System;
using System.Collections.Generic;
using System.IO;
using Eddie.Core;

namespace Eddie.Forms.Linux
{
	// X Tray is a separate process, because otherwise there are conflict between GTK version used by WinForms/Mono.
	public class Tray : Eddie.Core.Thread
	{
		string m_pathWrite = "";

		// Sometime eddie_tray can't start (issue with GTK, X11/Wayland permission etc).
		// But there is a loop to recover them if start but crash/killed.
		// So we mark when a real start is occured.
		bool m_oneStart = false;

		public Tray()
		{
		}

		public bool IsStarted()
		{
			//return ( (m_pathWrite != "") && (m_oneStart) ); // 2.17.3
			return (m_pathWrite != "");
		}

		public void SendCommand(string cmd)
		{
			if (IsStarted() == false)
				return;

			// Mono treats FIFOs as if they are seekable (it's a bug), even though they aren't.
			Platform.Linux.NativeMethods.PipeWrite(m_pathWrite, cmd + "\n");
		}

		public override void OnRun()
		{
			try
			{
				for (; ; )
				{

					//string controlReadPath = Core.Engine.Instance.Storage.GetPathInData("eddie_tray_r.tmp");
					//string controlWritePath = Core.Engine.Instance.Storage.GetPathInData("eddie_tray_w.tmp");
					//string controlReadPath = Path.GetTempPath() + "eddie_tray_r.tmp";
					//string controlWritePath = Path.GetTempPath() + "eddie_tray_w.tmp";
					string controlReadPath = Core.Platform.Instance.FileTempName("eddie_tray_r.tmp");
					string controlWritePath = Core.Platform.Instance.FileTempName("eddie_tray_w.tmp");

					string pathRes = Core.Engine.Instance.GetPathResources();
					string pathExe = Core.Engine.Instance.GetPathTools() + "/eddie-tray";

					if (Core.Platform.Instance.FileExists(pathExe) == false)
						return;

					List<string> arguments = new List<string>();
					arguments.Add("-r " + controlReadPath);
					arguments.Add("-w " + controlWritePath);
					arguments.Add("-p " + pathRes);

					string[] arguments2 = arguments.ToArray();

					Process processTray;
					processTray = new Process();

					processTray.StartInfo.FileName = Core.Platform.Instance.FileAdaptProcessExec(pathExe);
					processTray.StartInfo.Arguments = String.Join(" ", arguments2);
					processTray.StartInfo.WorkingDirectory = "";

					processTray.StartInfo.CreateNoWindow = true;
					processTray.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
					processTray.StartInfo.UseShellExecute = false;

					processTray.Start();

					if (processTray.HasExited) // If can't start, for example missing appindicator library.
						break;

					// Wait until control file is ready
					for (; ; )
					{
						if (CancelRequested)
							break;

						if (Core.Platform.Instance.FileExists(controlWritePath))
							break;

						Sleep(100);
					}

					m_pathWrite = controlReadPath;

					try
					{
						for (; ; )
						{
							using (StreamReader streamRead = new StreamReader(controlWritePath))
							{
								string cmd = streamRead.ReadLine();

								if (cmd == null) // Closed
									break;

								if (m_oneStart == false)
									m_oneStart = true;

								if (UiClient.Instance.MainWindow != null)
								{
									if (cmd == "menu.status")
									{
										UiClient.Instance.MainWindow.OnMenuStatus();
									}
									else if (cmd == "menu.connect")
									{
										UiClient.Instance.MainWindow.OnMenuConnect();
									}
									else if (cmd == "menu.preferences")
									{
										UiClient.Instance.MainWindow.OnShowPreferences();
									}
									else if (cmd == "menu.about")
									{
										UiClient.Instance.MainWindow.OnShowAbout();
									}
									else if (cmd == "menu.restore")
									{
										UiClient.Instance.MainWindow.OnMenuRestore();
									}
									else if (cmd == "menu.exit")
									{
										UiClient.Instance.MainWindow.OnMenuExit();
									}
								}
							}
						}
					}
					catch
					{
					}

					m_pathWrite = "";

					if (m_oneStart == false)
					{
						break;
					}

					if (this.CancelRequested)
						break;
				}
			}
			catch (Exception ex)
			{
				Eddie.Core.Engine.Instance.Logs.LogVerbose("Issue with the tray icon: " + ex.Message);

				m_pathWrite = "";
			}
		}
	}
}

