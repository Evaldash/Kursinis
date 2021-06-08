using Android.App;
using Android.OS;
using Android.Widget;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Net.Sockets;
using Android.Webkit;
using Xamarin.Essentials;
using System.Linq;
using System;

namespace Kursinis {
    [Activity(Label = "Activity1", ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]
    public class Scanning : Activity {

        private List<string> mItems;
        private ListView mListView;
        private ArrayAdapter<string> adapter;

        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Scanning);

            mItems = new List<string>();
            mListView = FindViewById<ListView>(Resource.Id.DeviceList);
            adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, mItems);
            mListView.Adapter = adapter;

            FindViewById<Button>(Resource.Id.ExitScan).Click += (o, e) => {
                Finish();
            };

            FindIPS("192.168.1.");
        }

        public async void FindIPS(string subadrr) {
            Ping ping;
            IPAddress addr;
            PingReply pingReply;
            IPHostEntry host;
            string name;

            List<string> entries = new List<string>();

            await Task.Run(() => {
                Parallel.For(1, 254, (i, loopState) => {

                    ping = new Ping();
                    string TestingAddr = subadrr + i;
                    pingReply = ping.Send(TestingAddr);

                    if (pingReply.Status == IPStatus.Success) {
                        string entry;
                        try {
                            addr = IPAddress.Parse(TestingAddr);
                            host = Dns.GetHostEntry(addr);
                            name = host.HostName;

                            entry = name + '(' + addr + ')';
                        }
                        catch (SocketException ex) {
                            name = "?";
                            entry = name;
                        }
                        entries.Add(entry);
                    }
                });
            });

            DoneSearching();
            TestCamera(entries);
           // foreach (string entry in entries) {
            //    adapter.Add(entry);
           // }
        }
        private void DoneSearching() {
            //FindViewById<ProgressBar>(Resource.Id.ScanningProgressBar).Visibility = Android.Views.ViewStates.Invisible;
            FindViewById<TextView>(Resource.Id.ProgressText).Text = "Scanning completed. Trying to connect to camera...";
        }

        private void TestCamera(List<string>candidates) {
            string cam_address;

            foreach (string candidate in candidates) {
                string pot_cam_address = candidate + ":8080/?action=stream";

                if (ConnectCamera(pot_cam_address)) {
                    cam_address = pot_cam_address;
                    break;

                }
            }
        }

        private bool ConnectCamera(string camera_adress) {
            var wPlayer = new WebView(this);
            wPlayer.Settings.LoadWithOverviewMode = true;
            wPlayer.Settings.UseWideViewPort = true;

            wPlayer.SetPadding(0, 0, 0, 0);

            var current = Connectivity.NetworkAccess;
            if (current == Xamarin.Essentials.NetworkAccess.Internet) { // If internet connection is present
                var profiles = Connectivity.ConnectionProfiles;
                if (profiles.Contains(ConnectionProfile.WiFi)) { // if wifi is enabled
                    WebSettings settings = wPlayer.Settings;
                    settings.JavaScriptEnabled = true;

                    wPlayer.AddJavascriptInterface(new Kursinis.MyJsInterface(this), "CSharp");
                    wPlayer.LoadUrl("file:///android_asset/TestCamera.html?camera=" + camera_adress);
                    
                    
                    //wPlayer.EvaluateJavascript("document.getElementById('is_camera_connected').value;");



                    return true;
                }
            }
            return false;
        }
    }
}