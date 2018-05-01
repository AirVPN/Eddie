// <eddie_sour//ce_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2018 AirVPN (support@airvpn.org) / https://airvpn.org
//
// Eddie is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Eddie is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Eddie. If not, see <http://www.gnu.org/licenses/>.
// </eddie_source_header>

using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Widget;
using Eddie.Common.Log;
using System.Collections.Generic;
using Android.Views;
using Android.Graphics.Drawables;
using Android.Content;

namespace Eddie.Droid
{
	[Activity(Label = "Applications list", Theme = "@style/EddieTheme.Light")]
	public class PackagesPickerActivity : Activity
	{
		public const string PARAM_PACKAGES = "PACKAGES";
			
		private List<ApplicationItem> m_applications = new List<ApplicationItem>();
		private ApplicationsListAdapter m_applicationsAdapter = null;
	
		private class ApplicationItem : Java.Lang.Object
		{
			private string m_title;

			public ApplicationItem(Context context, ApplicationInfo info)
			{
				Info = info;
				m_title = info.LoadLabel(context.PackageManager);
			}

			public ApplicationInfo Info { get; private set; }
			public bool Selected { get; set; }
			public Drawable Icon { get; set; }

			public string Title
			{
				get
				{
					return m_title;					
				}
			}

			public string Description
			{
				get
				{
					return Info.PackageName;
				}
			}
		}

		private class ApplicationsListAdapter : BaseAdapter
		{
			private PackagesPickerActivity m_activity = null;
			private LayoutInflater m_inflater = null;

			public ApplicationsListAdapter(PackagesPickerActivity activity)
			{
				m_activity = activity;
				m_inflater = LayoutInflater.From(activity);
			}

			public override int Count
			{
				get
				{
					return m_activity.m_applications != null ? m_activity.m_applications.Count : 0;
				}
			}

			public override Java.Lang.Object GetItem(int position)
			{
				return m_activity.m_applications[position];
			}

			public override long GetItemId(int position)
			{
				return position;
			}
	
			public override View GetView(int position, View view, ViewGroup parent)
			{
				ApplicationItem item = GetItem(position) as ApplicationItem;
				if(item == null)
					return null;

				if(view == null)
					view = m_inflater.Inflate(Resource.Layout.PackagesPickerItem, null);

				view.Visibility = ViewStates.Visible;

				ImageView itemIcon = view.FindViewById<ImageView>(Resource.Id.packages_picker_item_icon);
				itemIcon.SetImageDrawable(item.Icon);

				CheckBox itemCheckbox = view.FindViewById<CheckBox>(Resource.Id.packages_picker_item_selection);
				itemCheckbox.Checked = item.Selected;

				TextView itemTitle = view.FindViewById<TextView>(Resource.Id.packages_picker_item_title);
				itemTitle.Text = item.Title;

				TextView itemDescription = view.FindViewById<TextView>(Resource.Id.packages_picker_item_description);
				itemDescription.Text = item.Description;

				// TODO: fixme
				//view.Click += (object sender, EventArgs e) => m_activity.changeAppSelection(item, !item.Selected);
				itemCheckbox.CheckedChange += (object sender, CompoundButton.CheckedChangeEventArgs e) => m_activity.changeAppSelection(item, e.IsChecked);
				
				return view;	      		
			}			
		}
		
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			LogsManager.Instance.Debug("OnCreate6");
						
			LoadApplications(Utils.SpliValues(Intent.GetStringExtra(PARAM_PACKAGES)));
			InitUI();			
		}
		/*
		public override bool OnNavigateUp()
		{
			LogsManager.Instance.Debug("OnNavigateUp");
	
			return base.OnNavigateUp();
		}

		public override bool OnOptionsItemSelected(IMenuItem item)
		{
			LogsManager.Instance.Debug("OnOptionsItemSelected");

			return base.OnOptionsItemSelected(item); 
		}
		
		public override bool OnCreateOptionsMenu(IMenu menu)
		{
			LogsManager.Instance.Debug("OnCreateOptionsMenu");

			MenuInflater.Inflate(Resource.Menu.PackagesPickerMenu, menu);
			return base.OnCreateOptionsMenu(menu);
		}

		public override bool OnPrepareOptionsMenu(IMenu menu)
		{
			LogsManager.Instance.Debug("OnPrepareOptionsMenu");

			return base.OnPrepareOptionsMenu(menu);
		}
		*/
		private void InitUI()
		{
			SetContentView(Resource.Layout.PackagesPicker);
			
			ListView applicationsListView = FindViewById<ListView>(Resource.Id.packages_picker_applications);
		
			m_applicationsAdapter = new ApplicationsListAdapter(this);
			applicationsListView.Adapter = m_applicationsAdapter;

			Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.packages_picker_toolbar);
			toolbar.SetTitleTextColor(Android.Graphics.Color.White);
			SetActionBar(toolbar);
			//ActionBar.SetDisplayHomeAsUpEnabled(true);
			//ActionBar.SetHomeButtonEnabled(true);
			//ActionBar.SetDisplayShowHomeEnabled(true);
		}
		/*
		private void OnItemClick(object sender, ItemClickEventArgs e)
		{
			LogsManager.Instance.Debug("Item click");

			if(e.Position < m_applications.Count)
			{
				ApplicationItem app = m_applications[e.Position];
				if(app != null)
					changeAppSelection(app, !app.Selected);
			}						
		}
		*/
		private void changeAppSelection(ApplicationItem app, bool selected)
		{
			LogsManager.Instance.Debug(string.Format("Application '{0}' is now {1}", app.Title, selected ? "selected" : "deselect"));

			app.Selected = selected;
			// TODO: allows to save on demand
			SaveOptions();

			if(m_applicationsAdapter != null)
				m_applicationsAdapter.NotifyDataSetChanged();
		}

		private string GenerateSelectedApplications()
		{
			string selectedApplications = "";

			foreach(ApplicationItem app in m_applications)
			{
				if(app.Selected)
				{
					if(selectedApplications.Length > 0)
						selectedApplications += Utils.DEFAULT_SPLIT_SEPARATOR;

					selectedApplications += app.Info.PackageName;
				}
			}

			return selectedApplications;
		}

		private void SaveOptions()
		{
			OptionsManager.SystemApplicationsFilter = GenerateSelectedApplications();
		}

		private void LoadApplications(List<string> selectedPackages)
		{
			m_applications.Clear();

			IList<ApplicationInfo> applications = PackageManager.GetInstalledApplications(Android.Content.PM.PackageInfoFlags.MetaData);
			foreach(ApplicationInfo app in applications)
			{
				ApplicationItem item = new ApplicationItem(this, app);
				item.Selected = selectedPackages.Contains(app.PackageName);
				
				try
				{
					item.Icon = app.LoadIcon(PackageManager);
				}
				catch(System.Exception e)
				{
					LogsManager.Instance.Error(e);
				}

				m_applications.Add(item);
			}
	
			m_applications.Sort((a, b) => a.Title.CompareTo(b.Title));
		}
	}
}
