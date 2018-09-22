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
			
		private List<ApplicationItem> applicationList = new List<ApplicationItem>();
		private ApplicationsListAdapter applicationListAdapter = null;
	
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

		private class ApplicationsListAdapter : BaseAdapter<ApplicationItem>
		{
            private Activity activity = null;
            private List<ApplicationItem> appItems = null;

            private class ListViewHolder : Java.Lang.Object
            {
                public ImageView itemIcon { get; set; }
                public CheckBox itemCheckbox { get; set; }
                public TextView itemTitle { get; set; }
                public TextView itemDescription { get; set; }
            }

			public ApplicationsListAdapter(Activity a, List<ApplicationItem> items)
			{
                activity = a;

				appItems = items;
			}

            public void DataSet(List<ApplicationItem> appList)
            {
                appItems = appList;
                
                NotifyDataSetChanged();
            }

            public override int Count
            {
                get
                {
                    int entries = 0;
                    
                    if(appItems != null)
                        entries = appItems.Count;
                        
                    return entries;
                }
            }

            public override ApplicationItem this[int position]
            {
                get
                {
                    return appItems[position];
                }
            }

			public override long GetItemId(int position)
			{
				return position;
			}
	
			public override View GetView(int position, View convertView, ViewGroup parent)
			{
                ApplicationItem item = appItems[position];

                ListViewHolder listViewHolder = null;
 
                if(convertView != null) 
                    listViewHolder = convertView.Tag as ListViewHolder;
  
                if(listViewHolder == null)
                {
                    listViewHolder = new ListViewHolder();

                    convertView = activity.LayoutInflater.Inflate(Resource.Layout.package_chooser_item, null);

                    listViewHolder.itemIcon = convertView.FindViewById<ImageView>(Resource.Id.packages_picker_item_icon);
                    listViewHolder.itemCheckbox = convertView.FindViewById<CheckBox>(Resource.Id.packages_picker_item_selection);
                    listViewHolder.itemTitle = convertView.FindViewById<TextView>(Resource.Id.packages_picker_item_title);
                    listViewHolder.itemDescription = convertView.FindViewById<TextView>(Resource.Id.packages_picker_item_description);
    
                    convertView.Tag = listViewHolder;
				}

				listViewHolder.itemIcon.SetImageDrawable(item.Icon);

				listViewHolder.itemCheckbox.Checked = item.Selected;
                listViewHolder.itemCheckbox.Clickable = false;
                listViewHolder.itemCheckbox.Selected = false;

				listViewHolder.itemTitle.Text = item.Title;

				listViewHolder.itemDescription.Text = item.Description;

				return convertView;	      		
			}			
		}
		
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

            EddieLogger.Init(this);

            title = Intent.GetStringExtra(CHOOSER_TITLE);

			LoadApplications(Intent.GetStringExtra(PARAM_PACKAGES));
			
            InitUI();			
		}

        public override void OnBackPressed()
        {
            Intent resultIntent = new Intent(this, typeof(PackageChooserActivity));
            resultIntent.PutExtra(PARAM_PACKAGES, GetSelectedApplicationList());

            SetResult(Result.Ok, resultIntent);

            Finish();
        }

        private void InitUI()
		{
			SetContentView(Resource.Layout.package_chooser_activity_layout);
			
            txtTitle = FindViewById<TextView>(Resource.Id.chooser_title);
			applicationsListView = FindViewById<ListView>(Resource.Id.packages_picker_applications);

            txtTitle.Text = title;

			applicationListAdapter = new ApplicationsListAdapter(this, applicationList);
			
            applicationsListView.Adapter = applicationListAdapter;

            applicationsListView.ItemClick += (object sender, ItemClickEventArgs e) =>
            {
                applicationList[e.Position].Selected = !applicationList[e.Position].Selected;

                if(applicationListAdapter != null)
                    applicationListAdapter.NotifyDataSetChanged();
            };
		}
		
		private string GetSelectedApplicationList()
		{
			string selectedApplications = "";

			foreach(ApplicationItem app in applicationList)
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

			applicationList.Clear();

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
					EddieLogger.Error(e);
				}

				applicationList.Add(item);
			}
	
			applicationList.Sort((a, b) => a.Title.CompareTo(b.Title));
		}
	}
}
