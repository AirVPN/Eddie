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

using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Widget;
using Eddie.Common.Log;
using Eddie.Common.Threading;
using Java.Text;
using Java.Util;

namespace Eddie.NativeAndroidApp
{
    [Activity(Label = "Log Activity")]
    public class LogActivity : Activity
    {
        private List<string> logEntry = null;
        private ListView listLogView = null;
        private ImageButton btnShare = null;
        private long unixEpochTime = 0; // Unix rules forever. unixEpochTime is the beginning of everything. Dennis Ritchie, Ken Thompson and Brian Kernighan started the new and long lasting era of real and amazing computing. If computers, operating systems and mobile devices are what they are today, it certainly, clearly, unequivocally and undoubtedly is thanks to them all. Unix and C concepts are the foundation of everything and of modern computing. This is a fact, this is history. Long live Unix and C! [ProMIND]
        private LogListAdapter logListAdapter = null;

        private enum FormatType
        {
            HTML = 1,
            PLAIN_TEXT
        };

        private enum LogTime
        {
            UTC = 1,
            LOCAL
        };

        
        private class LogListAdapter : BaseAdapter
        {
            private List<string> logEntry = null;
            private LayoutInflater m_inflater = null;

            public LogListAdapter(LogActivity activity, List<string> entryList)
            {
                logEntry = entryList;
                m_inflater = LayoutInflater.From(activity);
            }

            public override int Count
            {
                get
                {
                    int entries = 0;
                    
                    if(logEntry != null)
                        entries = logEntry.Count;
                        
                    return entries;
                }
            }

            public override Java.Lang.Object GetItem(int position)
            {
                return logEntry[position];
            }

            public override long GetItemId(int position)
            {
                return position;
            }
    
            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                string item = GetItem(position).ToString();
                
                if(item == null)
                    return null;

                if(convertView == null)
                    convertView = m_inflater.Inflate(Resource.Layout.log_activity_listitem, null);

                convertView.Visibility = ViewStates.Visible;

                TextView txtLogEntry = convertView.FindViewById<TextView>(Resource.Id.log_entry);
                
                txtLogEntry.TextFormatted = Html.FromHtml(item); // deprecated method in order to support Android < 7.0

                return convertView;             
            }           
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.log_activity_layout);

            listLogView = FindViewById<ListView>(Resource.Id.log);

            logEntry = GetCurrentLog(FormatType.HTML, LogTime.LOCAL);

            logListAdapter = new LogListAdapter(this, logEntry);
            
            listLogView.Adapter = logListAdapter;

            btnShare = FindViewById<ImageButton>(Resource.Id.btn_share);

            unixEpochTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).Ticks / TimeSpan.TicksPerSecond;

            btnShare.Click += delegate
            {
                SimpleDateFormat dateFormatter = null;
                Date logCurrenTimeZone = null;
                Calendar calendar = null;
                List<string> exportLog = null;
                string logText = "", logSubject = "";
                long utcTimeStamp = 0;

                calendar = Calendar.Instance;
                dateFormatter = new SimpleDateFormat("dd MMM yyyy HH:mm:ss");

                utcTimeStamp = (DateTime.UtcNow.Ticks / TimeSpan.TicksPerSecond) - unixEpochTime;
                calendar.TimeInMillis = utcTimeStamp * 1000;
                logCurrenTimeZone = (Date)calendar.Time;
                dateFormatter.TimeZone = Java.Util.TimeZone.GetTimeZone("GMT");

                logSubject = string.Format(Resources.GetString(Resource.String.log_subject), dateFormatter.Format(logCurrenTimeZone));

                exportLog = GetCurrentLog(FormatType.PLAIN_TEXT, LogTime.UTC);

                foreach(string entry in exportLog)
                    logText += entry + "\n";

                Intent shareIntent = new Intent(global::Android.Content.Intent.ActionSend);
                
                shareIntent.SetType("text/plain");
                shareIntent.PutExtra(global::Android.Content.Intent.ExtraTitle, Resources.GetString(Resource.String.log_title));
                shareIntent.PutExtra(global::Android.Content.Intent.ExtraSubject, logSubject);
                shareIntent.PutExtra(global::Android.Content.Intent.ExtraText, logText);
                
                StartActivity(Intent.CreateChooser(shareIntent, Resources.GetString(Resource.String.log_share_with)));
            };
        }
        
        private List<string> GetCurrentLog(FormatType formatType = FormatType.HTML, LogTime logTime = LogTime.UTC)
        {
            SimpleDateFormat dateFormatter = null;
            Date logCurrenTimeZone = null;
            Calendar calendar = null;
            List<string> logItem = null;
            long utcTimeStamp = 0;

            string log = "";

            calendar = Calendar.Instance;

            dateFormatter = new SimpleDateFormat("dd MMM yyyy HH:mm:ss");

            if(logTime == LogTime.UTC)
                dateFormatter.TimeZone = Java.Util.TimeZone.GetTimeZone("GMT");

            using(DataLocker<List<LogEntry>> entries = LogsManager.Instance.Entries)
            {
                logItem = new List<string>();

                foreach(LogEntry entry in entries.Data)
                {
                    utcTimeStamp = (entry.Timestamp.Ticks / TimeSpan.TicksPerSecond) - unixEpochTime;

                    calendar.TimeInMillis = utcTimeStamp * 1000;
                    logCurrenTimeZone = (Date)calendar.Time;

                    switch(formatType)
                    {
                        case FormatType.HTML:
                        {                    
                            log = "<font color='blue'>" + dateFormatter.Format(logCurrenTimeZone) + "</font> [<font color='";
                            
                            switch(entry.Level)
                            {
                                case LogLevel.debug:
                                {
                                    log += "purple";
                                }
                                break;
        
                                case LogLevel.error:
                                {
                                    log += "red";
                                }
                                break;
        
                                case LogLevel.fatal:
                                {
                                    log += "red";
                                }
                                break;
        
                                case LogLevel.info:
                                {
                                    log += "darkgreen";
                                }
                                break;
        
                                case LogLevel.warning:
                                {
                                    log += "orange";
                                }
                                break;
                                
                                default:
                                {
                                    log += "black";
                                }
                                break;
                            }
                            
                            log += "'>"  + entry.Level + "</font>]: " + entry.Message;
                        }
                        break;

                        case FormatType.PLAIN_TEXT:
                        {
                            log = dateFormatter.Format(logCurrenTimeZone) + " UTC [" + entry.Level +"] " + entry.Message;
                        }
                        break;

                        default:
                        {
                            log = "";
                        }
                        break;
                    }
                    
                    logItem.Add(log);
                }
            }
            
            return logItem;
        }
    }
}
