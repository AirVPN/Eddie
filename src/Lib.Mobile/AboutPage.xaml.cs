using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Eddie
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class AboutPage : ContentPage
	{
		public AboutPage ()
		{
			InitializeComponent();

			Title = "About";

			m_webView.Navigating += OnNavigating;
			m_webView.Source = Utils.GetAssetURI("about.html");
			//InitUI();
		}

		private void OnNavigating(object sender, WebNavigatingEventArgs e)
		{
			e.Cancel = true;

			if (e.Url.EndsWith("eddie.website/files/android/license.html"))
			{
				App.Instance.NavigationPage.Navigation.PushAsync(new LicensePage());
			}
			else if (e.Url.EndsWith("eddie.website/files/android/libraries.html"))
			{
				App.Instance.NavigationPage.Navigation.PushAsync(new LibrariesPage());
			}
			else
				Utils.OpenURL(e.Url);
		}

		/*
		private void InitUI()
		{
			m_buttonWebsite.Clicked += OnWebsite;
			m_buttonLicense.Clicked += OnLicense;
			m_buttonLibraries.Clicked += OnLibraries;
		}

		private void OnLibraries(object sender, EventArgs e)
		{
			App.Instance.NavigationPage.Navigation.PushAsync(new LibrariesPage());
		}

		private void OnLicense(object sender, EventArgs e)
		{
			App.Instance.NavigationPage.Navigation.PushAsync(new LicensePage());
		}

		private void OnWebsite(object sender, EventArgs e)
		{
			Utils.OpenURL("https://eddie.website");
		}
		*/
	}
}
