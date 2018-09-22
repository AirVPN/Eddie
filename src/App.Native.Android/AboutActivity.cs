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
// 20 June 2018 - author: promind - initial release. (a tribute to the 1859 Perugia uprising occurred on 20 June 1859 and in memory of those brave inhabitants who fought for the liberty of Perugia)

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Net;
using Android.OS;
using Android.Webkit;
using Android.Widget;

namespace Eddie.NativeAndroidApp
{
    [Activity(Label = "About Activity", ConfigurationChanges = ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    public class AboutActivity : Activity
    {
        private string uri = "";

        private LinearLayout llMainLayout = null;

        private ImageButton btnBack = null;
        private WebView webView = null;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.about_activity_layout);

            uri = Resources.GetString(Resource.String.local_about_page);

            llMainLayout = FindViewById<LinearLayout>(Resource.Id.main_layout);

            btnBack = FindViewById<ImageButton>(Resource.Id.btn_back);

            btnBack.Click += delegate
            {
                if(webView != null)
                {
                    if(webView.CanGoBack())
                        webView.GoBack();
                    else
                        this.Finish();
                }
            };

            webView = FindViewById<WebView>(Resource.Id.webview);  

            webView.Settings.JavaScriptEnabled = true;  
            
            webView.SetWebViewClient(new EddieWebViewClient(this));
            
            webView.Settings.BuiltInZoomControls = true;
            webView.Settings.DisplayZoomControls = false;

            if(!uri.Equals(""))
                webView.LoadUrl(uri);
            else
                this.Finish();
        }
        
        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);

            webView.SaveState(outState);
        }

        protected override void OnRestoreInstanceState(Bundle savedInstanceState)
        {
            base.OnRestoreInstanceState(savedInstanceState);
            
            if(savedInstanceState != null)
                webView.RestoreState(savedInstanceState);
        }

        public override void OnConfigurationChanged(Android.Content.Res.Configuration newConfig)
        {
            llMainLayout.RemoveView(webView);

            base.OnConfigurationChanged(newConfig);

            llMainLayout.AddView(webView);
        }

        public override void OnBackPressed()
        {
            Finish();
        }
    }
  
    internal class EddieWebViewClient : WebViewClient  
    {
        private Context appContext = null;
        
        public EddieWebViewClient(Context context) : base()
        {
            appContext = context;
        }

        // We need to use the deprecated ShouldOverrideUrlLoading interface in order to comply to the minimum API 22 requirement

        public override bool ShouldOverrideUrlLoading(WebView view, string url)  
        {
            if(appContext == null || view == null || url.Equals(""))
                return false;

            LoadUri(view, url);

            return true;
        }  

        public override bool ShouldOverrideUrlLoading(WebView view, IWebResourceRequest request)  
        {
            if(appContext == null || view == null || request == null)
                return false;

            LoadUri(view, request.Url.ToString());

            return true;
        }

        private void LoadUri(WebView view, string url)
        {
            Uri uri = Uri.Parse(url);

            if(uri.Scheme.Equals("file"))
            {
                view.LoadUrl(url);
            }
            else
            {
                Intent browserIntent = new Intent(Intent.ActionView);

                browserIntent.SetData(uri);
                
                appContext.StartActivity(browserIntent);
            }
        }
    }
}
