using Android.App;
using Android.OS;
using Android.Widget;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Android.Webkit;
using System;

namespace Kursinis {
    [Activity(Label = "Activity1", ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]
    public class Scanning : Activity {
        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Scanning);

            FindViewById<Button>(Resource.Id.ExitScan).Click += (o, e) => {
                Finish();
            };
            FindCameras("192.168.1.");
        }

        public async void FindCameras(string subadrr) {
            Ping ping;
            PingReply pingReply;
            List<string> Cameras = new List<string>();

            List<string> ActiveIPS = new List<string>();

            await Task.Run(() => {
                Parallel.For(1, 254, (i, loopState) => {
                    ping = new Ping();
                    string TestingAddr = subadrr + i;
                    pingReply = ping.Send(TestingAddr);

                    if (pingReply.Status == IPStatus.Success) {
                        ActiveIPS.Add(TestingAddr);
                    }
                });
            });
            FindViewById<TextView>(Resource.Id.ProgressText).Text = "Scanning completed. Finding cameras...";

            foreach (string ActiveIP in ActiveIPS) {
                string pot_cam_address = "http://" + ActiveIP + ":8080?action=stream";
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(pot_cam_address);
                request.Timeout = 15000;
                request.Method = "GET"; // As per Lasse's comment

                try {
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse()) {
                        if (response.StatusCode == HttpStatusCode.OK) {
                            Cameras.Add(pot_cam_address);
                        }
                    }
                }
                catch (WebException) {
                }
            }

            DisplayCameras(Cameras);
        }
        private void DisplayCameras(List<string> cameras) {
            WebView wPlayer = FindViewById<WebView>(Resource.Id.TestCamera);
            WebSettings settings = wPlayer.Settings;
            settings.JavaScriptEnabled = true;

            FindViewById<ProgressBar>(Resource.Id.ScanningProgressBar).Visibility = Android.Views.ViewStates.Invisible;

            if (cameras.Count > 1) {
                FindViewById<TextView>(Resource.Id.ProgressText).Text = "Looks like there's more than one camera. Please select one out of the cameras.";
            }
            else if (cameras.Count == 1) {
                FindViewById<TextView>(Resource.Id.ProgressText).Text = "Camera found. If you want to use it, confirm by clicking the button at the bottom.";
                wPlayer.LoadUrl("file:///android_asset/Camera.html?camera=" + cameras[0]);
            }
            else {
                FindViewById<TextView>(Resource.Id.ProgressText).Text = "Sorry, we couldn't find any cameras :(";
            }
            
        }
    }
}