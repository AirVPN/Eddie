using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Lib.Platform.Windows.Elevated;

namespace App.Service.Windows.Elevated
{
	public partial class EddieService : ServiceBase
	{
		private Engine m_engine;

		public EddieService()
		{
			InitializeComponent();

			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
		}

		protected override void OnStart(string[] args)
		{
			if (m_engine != null)
				return;

			try
			{
				m_engine = new Engine();
				m_engine.Start(true);
			}
			catch (Exception ex)
			{
				Utils.DebugLog(ex.Message);
			}
		}

		protected override void OnStop()
		{
			if (m_engine == null)
				return;

			try
			{
				m_engine.Stop(true);
				m_engine = null;
			}
			catch(Exception ex)
			{
				Utils.DebugLog(ex.Message);
			}
		}

		void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			Utils.DebugLog((e.ExceptionObject as Exception).Message);
		}
	}
}
