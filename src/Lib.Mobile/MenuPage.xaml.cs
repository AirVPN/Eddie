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
	public partial class MenuPage : ContentPage
	{
		public MenuPage()
		{
			InitializeComponent();

			Title = "Menu";
			BackgroundColor = Color.FromRgba(255, 255, 255, 200);

			BindingContext = new MenuPageViewModel();
		}		
	}
}