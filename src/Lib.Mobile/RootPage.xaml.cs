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
    public partial class RootPage : MasterDetailPage
    {
        public RootPage()
        {
            InitializeComponent();
			
			MasterBehavior = MasterBehavior.Popover;
		}

		/*
		public class RootPage : TabbedPage
		{
			private Page m_servicePage = null;
			private Page m_logsPage = null;

			public RootPage(IVPNManager manager)
			{
				switch (Device.RuntimePlatform)
				{
					case Device.iOS:	m_servicePage = new NavigationPage(new ServicePage(manager));
										m_logsPage = new NavigationPage(new LogsPage(manager));				
				
										break;

					default:			m_servicePage = new ServicePage(manager);
										m_logsPage = new LogsPage(manager);

										break;
				}

				m_servicePage.Title = "Service";
				//servicePage.Icon = ...
				m_logsPage.Title = "Logs";
				//logsPage.Icon = ...
	
				Children.Add(m_servicePage);
				Children.Add(m_logsPage);

				Title = Children[0].Title;
			}

			protected override void OnCurrentPageChanged()
			{
				base.OnCurrentPageChanged();

				Title = CurrentPage?.Title ?? string.Empty;
			}
		}
		*/
	}
}