using Eddie.Common.Log;
using System;
using System.Windows.Input;
using Xamarin.Forms;

namespace Eddie
{
	public class MenuPageViewModel {

		public ICommand Website { get; set; }
		public ICommand Logs { get; set; }
		public ICommand Preferences { get; set; }
		public ICommand About { get; set; }
		//public ICommand Exit { get; set; }
		//public ICommand DevelopedBy { get; set; }

		public MenuPageViewModel()
		{
			Website = new Command(OnWebsite);
			Logs = new Command(OnLogs);
			Preferences = new Command(OnPreferences);
			About = new Command(OnAbout);
			//Exit = new Command(OnExit);
			//DevelopedBy = new Command(OnDevelopedBy);
		}

		private void OnWebsite(object obj)
		{
			Utils.OpenURL("https://eddie.website");
			App.Instance.RootPage.IsPresented = false;
		}
	
		private void OnLogs(object obj)
		{
			App.Instance.NavigationPage.Navigation.PushAsync(new LogsPage());
			App.Instance.RootPage.IsPresented = false;
		}

		private void OnPreferences(object obj)
		{
			App.Instance.VPNManager.EditOptions();
			App.Instance.RootPage.IsPresented = false;
		}

		private void OnAbout(object obj)
		{
			App.Instance.NavigationPage.Navigation.PushAsync(new AboutPage());
			App.Instance.RootPage.IsPresented = false;
		}

		/*
		private void OnExit(object obj)
		{
			App.Instance.VPNManager.Stop();
			App.Instance.RootPage.IsPresented = false;
		}
		*/

		/*
		private void OnDevelopedBy(object obj)
		{
			Utils.OpenURL("https://airvpn.org");
			App.Instance.RootPage.IsPresented = false;
		}
		*/
	}
}
