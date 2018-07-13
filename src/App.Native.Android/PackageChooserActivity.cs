// <eddie_source_header>
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
//
// 20 June 2018 - author: promind - initial release. Based on revised code from com.eddie.android. (a tribute to the 1859 Perugia uprising occurred on 20 June 1859 and in memory of those brave inhabitants who fought for the liberty of Perugia)

using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Widget;
using Eddie.Common.Log;
using System.Collections.Generic;
using Android.Views;
using Android.Graphics.Drawables;
using Android.Content;
using static Android.Widget.AdapterView;

namespace Eddie.NativeAndroidApp
{
	[Activity(Label = "Package Chooser Activity", Theme = "@style/AppTheme")]
	public class PackageChooserActivity : Activity
	{
		public const string PARAM_PACKAGES = "PACKAGES";
        public const string CHOOSER_TITLE = "TITLE";
			
		private List<ApplicationItem> m_applications = new List<ApplicationItem>();
		private ApplicationsListAdapter m_applicationsAdapter = null;
	
        private ListView applicationsListView = null;
        private TextView txtTitle = null;

        private string title = "";

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
			private PackageChooserActivity m_activity = null;
			private LayoutInflater m_inflater = null;

			public ApplicationsListAdapter(PackageChooserActivity activity)
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
	
			public override View GetView(int position, View convertView, ViewGroup parent)
			{
				ApplicationItem item = GetItem(position) as ApplicationItem;
				
                if(item == null)
					return null;

				if(convertView == null)
					convertView = m_inflater.Inflate(Resource.Layout.package_chooser_item, null);

				convertView.Visibility = ViewStates.Visible;

				ImageView itemIcon = convertView.FindViewById<ImageView>(Resource.Id.packages_picker_item_icon);
				itemIcon.SetImageDrawable(item.Icon);

				CheckBox itemCheckbox = convertView.FindViewById<CheckBox>(Resource.Id.packages_picker_item_selection);
				itemCheckbox.Checked = item.Selected;
                itemCheckbox.Clickable = false;
                itemCheckbox.Selected = false;

				TextView itemTitle = convertView.FindViewById<TextView>(Resource.Id.packages_picker_item_title);
				itemTitle.Text = item.Title;

				TextView itemDescription = convertView.FindViewById<TextView>(Resource.Id.packages_picker_item_description);
				itemDescription.Text = item.Description;

				return convertView;	      		
			}			
		}
		
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			LogsManager.Instance.Debug("OnCreate6");

            title = Intent.GetStringExtra(CHOOSER_TITLE);

			LoadApplications(Intent.GetStringExtra(PARAM_PACKAGES));
			
            InitUI();			
		}

        public override void OnBackPressed()
        {
            Intent resultIntent = new Intent(this, typeof(PackageChooserActivity));
            resultIntent.PutExtra(PARAM_PACKAGES, GenerateSelectedApplications());

            SetResult(Result.Ok, resultIntent);

            base.OnBackPressed();

            Finish();
        }

        private void InitUI()
		{
			SetContentView(Resource.Layout.package_chooser_activity_layout);
			
            txtTitle = FindViewById<TextView>(Resource.Id.chooser_title);
			applicationsListView = FindViewById<ListView>(Resource.Id.packages_picker_applications);

            txtTitle.Text = title;

			m_applicationsAdapter = new ApplicationsListAdapter(this);
			
            applicationsListView.Adapter = m_applicationsAdapter;

            applicationsListView.ItemClick += (object sender, ItemClickEventArgs e) =>
            {
                m_applications[e.Position].Selected = !m_applications[e.Position].Selected;

                if(m_applicationsAdapter != null)
                    m_applicationsAdapter.NotifyDataSetChanged();
            };
		}
		
		private string GenerateSelectedApplications()
		{
			string selectedApplications = "";

			foreach(ApplicationItem app in m_applications)
			{
				if(app.Selected)
				{
					if(selectedApplications.Length > 0)
						selectedApplications += SettingsManager.DEFAULT_SPLIT_SEPARATOR;

					selectedApplications += app.Info.PackageName;
				}
			}

			return selectedApplications;
		}

		private void LoadApplications(string appList)
		{
            List<string> selectedPackages = new List<string>();

            string[] valArray = appList.Split(SettingsManager.DEFAULT_SPLIT_SEPARATOR);

            foreach(string item in valArray)
                selectedPackages.Add(item);

			m_applications.Clear();

            IList<ApplicationInfo> applications = base.PackageManager.GetInstalledApplications(global::Android.Content.PM.PackageInfoFlags.MetaData);
			
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
