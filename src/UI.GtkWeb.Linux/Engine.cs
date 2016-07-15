using System;
using Gtk;
using Eddie.Core;

namespace UI.GtkWeb.Linux
{
	public class Engine : Eddie.Core.Engine
	{
		public WindowPrimary WindowMain;

		public override void OnDeInit2 ()
		{
			base.OnDeInit2 ();

			if (WindowMain != null)
			{				
				Application.Invoke (delegate {					
					WindowMain.DeInit();
				});
			}
		}
	}
}

