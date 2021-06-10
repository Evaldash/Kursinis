using Android.App;
using Android.OS;
using Android.Widget;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Android.Webkit;
using System.Threading;
using Android.Content;
using System;
using Android.Net.Wifi;

namespace Kursinis {
    [Activity(Label = "Activity1", ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]
    public class Scanning : Activity {

        CancellationTokenSource _tokenSource = null;
        List<string> Cameras = new List<string>();
        string selected_camera;

        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Scanning);

            _tokenSource = new CancellationTokenSource();
            var token = _tokenSource.Token;

            FindViewById<Button>(Resource.Id.ExitScan).Click += (o, e) => {
                _tokenSource.Cancel();
                Finish();
            };
            FindViewById<Button>(Resource.Id.SaveCamera).Click += (o, e) => {
                _tokenSource.Cancel();

                var rtn = new Intent();
                rtn.PutExtra("selected_camera", selected_camera);
                SetResult(Result.Ok, rtn);

                Finish();
            };

            WifiManager wifiManager = (WifiManager)Android.App.Application.Context.GetSystemService(Service.WifiService);
            int ip = wifiManager.ConnectionInfo.IpAddress;
            IPAddress ipAddr = new IPAddress(ip);

            string IPaddress = ipAddr.ToString();
            string[] numbers = IPaddress.Split('.');
            string subnet = numbers[0] + '.' + numbers[1] + '.' + numbers[2] + '.';

            Console.WriteLine(subnet);
            FindCameras(subnet, token);
        }

        public async void FindCameras(string subadrr, CancellationToken token) {
            try {
                await Task.Run(() => {
                   Parallel.For(1, 254, new ParallelOptions { CancellationToken = token, }, (i, loopState) => {
                       token.ThrowIfCancellationRequested();
                       string TestingAddr = subadrr + i;
                       string pot_cam_address = "http://" + TestingAddr + ":8080?action=stream";

                       HttpWebRequest request = (HttpWebRequest)WebRequest.Create(pot_cam_address);
                       request.Timeout = 15000;
                       request.Method = "GET";

                       try {
                           using (HttpWebResponse response = (HttpWebResponse)request.GetResponse()) {
                               if (response.StatusCode == HttpStatusCode.OK) {
                                   if (!Cameras.Contains(pot_cam_address)) {
                                       Cameras.Add(pot_cam_address);
                                   }
                               }
                           }
                       }
                       catch (WebException) {
                       }
                   });
                }, token);
            }
            catch (System.OperationCanceledException ex) {

            }

            if (!token.IsCancellationRequested) { // cancel the loop if the cancel button is pressed
                DisplayCameras(Cameras);
            }
        }
        private void DisplayCameras(List<string> cameras) {
            WebView wPlayer = FindViewById<WebView>(Resource.Id.TestCamera);
            WebSettings settings = wPlayer.Settings;
            settings.JavaScriptEnabled = true;

            FindViewById<ProgressBar>(Resource.Id.ScanningProgressBar).Visibility = Android.Views.ViewStates.Invisible;

            if (cameras.Count > 1) {
                FindViewById<TextView>(Resource.Id.ProgressText).Text = "There seems to be more than one device available. Please select the camera feed you wish to use.";
            }
            else if (cameras.Count == 1) {
                FindViewById<TextView>(Resource.Id.ProgressText).Text = "Camera detected. If you can see the feed, click the Save camera button to save the address.";
                FindViewById<Button>(Resource.Id.SaveCamera).Enabled = true;
                wPlayer.LoadUrl("file:///android_asset/Camera.html?camera=" + cameras[0]);
                selected_camera = cameras[0];
            }
            else {
                FindViewById<TextView>(Resource.Id.ProgressText).Text = "Sorry, we couldn't find any cameras :(";
            }
        }
    }
}