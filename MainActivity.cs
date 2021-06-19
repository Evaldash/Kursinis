using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using AndroidX.AppCompat.App;
using Java.Util;
using System;
using System.Linq;
using System.Text;
using Xamarin.Essentials;

namespace Kursinis {
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true, ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]
    public class MainActivity : AppCompatActivity {
        BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;
        private BluetoothSocket _socket;
        private bool BTconnected = false;

        BluetoothDevice device;
        IMenuItem BTsymbol;

        private string pi_name = Preferences.Get("pi_name", "raspberrypi");
        private string camera_adress = Preferences.Get("camera_adress", "http://192.168.1.111:8080/?action=stream");
        private string snapshot_address = Preferences.Get("snapshot_address", "http://192.168.1.111:8080/?action=snapshot");
        private bool BTadapterOK() {
            if (adapter == null) {
                BTconnected = false;
                showShortToast(Application.Context, "Error: no Bluetooth device found.");
                return false;
            }
            else if (!adapter.IsEnabled) {
                BTconnected = false;
                showShortToast(Application.Context, "Error: Bluetooth device is turned off.");
                return false;
            }
            return true;
        }

        private void showShortToast(Context context, string text) {
            Toast toast = null;
            try {
                if (toast != null) {
                    toast.SetText(text);
                }
                else {
                    toast = Toast.MakeText(context, text, ToastLength.Short);
                }
                toast.Show();
            }
            catch (Exception e) {
                Looper.Prepare();
                Toast.MakeText(context, text, ToastLength.Short).Show();
                Looper.Loop();
            }
        }

        private async void BTConnect() {
            if (BTadapterOK()) {
                device = (from bd in adapter.BondedDevices
                          where bd.Name == pi_name
                          select bd).FirstOrDefault();

                if (device == null) {
                    BTconnected = false;
                    string message = "Error: no " + pi_name + " device found.";
                    showShortToast(Application.Context, message);
                }
                else {
                    try {
                        _socket = device.CreateRfcommSocketToServiceRecord(UUID.FromString("00001101-0000-1000-8000-00805f9b34fb"));
                        await _socket.ConnectAsync();
                        BTSuccess();
                    }
                    catch (Java.IO.IOException) {
                        showShortToast(Application.Context, "Error: couldn't connect to Bluetooth server.");
                    }
                }
            }
        }
        private void BTSuccess() {
            showShortToast(Application.Context, "Connection succesful.");
            BTsymbol.SetIcon(Resource.Drawable.BTConnected);
            EnableButtons();
            BTconnected = true;
        }

        private void BTDisconnect() {
            if (_socket != null) {
                _socket.Close();
                BTsymbol.SetIcon(Resource.Drawable.BTDisconnected);
                DisableButtons();
                BTconnected = false;
            }
        }
        private async void sendData(string strbuffer) {
            if (BTconnected) {
                byte[] buffer = Encoding.ASCII.GetBytes(strbuffer);
                try {
                    await _socket.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                }
                catch (Java.IO.IOException) {
                    showShortToast(Application.Context, "Connection lost!");
                    BTDisconnect();
                }
            }
        }

        private void InitialiseButtons() { // initialises buttons to work with the Pi
            System.Timers.Timer fwdtimer = new System.Timers.Timer(); // data sending timer. Checks whether the buttons are still pressed, if so, sends data every second.
            fwdtimer.Interval = 1000;
            fwdtimer.Elapsed += (s, e) => sendData("U");

            System.Timers.Timer bcktimer = new System.Timers.Timer();
            bcktimer.Interval = 1000;
            bcktimer.Elapsed += (s, e) => sendData("D");

            System.Timers.Timer Lefttimer = new System.Timers.Timer();
            Lefttimer.Interval = 1000;
            Lefttimer.Elapsed += (s, e) => sendData("L");

            System.Timers.Timer Righttimer = new System.Timers.Timer();
            Righttimer.Interval = 1000;
            Righttimer.Elapsed += (s, e) => sendData("R");

            FindViewById<ImageButton>(Resource.Id.btnUp).Touch += (s, e) => { // Up
                var handled = false;
                if (e.Event.Action == MotionEventActions.Down) {
                    bcktimer.Stop(); // just in case
                    sendData("U");
                    fwdtimer.Start();
                    handled = true;
                }
                else if (e.Event.Action == MotionEventActions.Up) {
                    sendData("u");
                    fwdtimer.Stop();
                    handled = true;
                }
                e.Handled = handled;
            };

            FindViewById<ImageButton>(Resource.Id.btnDown).Touch += (s, e) => { // Down
                var handled = false;
                if (e.Event.Action == MotionEventActions.Down) {
                    fwdtimer.Stop(); // just in case
                    bcktimer.Start();
                    sendData("D");
                    handled = true;
                }
                else if (e.Event.Action == MotionEventActions.Up) {
                    bcktimer.Stop();
                    sendData("d");
                    handled = true;
                }
                e.Handled = handled;
            };

            FindViewById<ImageButton>(Resource.Id.btnLeft).Touch += (s, e) => { // Left
                var handled = false;
                if (e.Event.Action == MotionEventActions.Down) {
                    Righttimer.Stop(); // just in case
                    sendData("L");
                    Lefttimer.Start();
                    handled = true;
                }
                else if (e.Event.Action == MotionEventActions.Up) {
                    sendData("l");
                    Lefttimer.Stop();
                    handled = true;
                }
                e.Handled = handled;
            };

            FindViewById<ImageButton>(Resource.Id.btnRight).Touch += (s, e) => { // Right
                var handled = false;
                if (e.Event.Action == MotionEventActions.Down) {
                    Lefttimer.Stop(); // just in case
                    sendData("R");
                    handled = true;
                    Righttimer.Start();
                }
                else if (e.Event.Action == MotionEventActions.Up) {
                    sendData("r");
                    handled = true;
                    Righttimer.Stop();
                }
                e.Handled = handled;
            };
        }

        private void EnableButtons() {
            FindViewById<ImageButton>(Resource.Id.btnUp).Enabled = true;
            FindViewById<ImageButton>(Resource.Id.btnDown).Enabled = true;
            FindViewById<ImageButton>(Resource.Id.btnLeft).Enabled = true;
            FindViewById<ImageButton>(Resource.Id.btnRight).Enabled = true;
        }

        private void DisableButtons() {
            FindViewById<ImageButton>(Resource.Id.btnUp).Enabled = false;
            FindViewById<ImageButton>(Resource.Id.btnDown).Enabled = false;
            FindViewById<ImageButton>(Resource.Id.btnLeft).Enabled = false;
            FindViewById<ImageButton>(Resource.Id.btnRight).Enabled = false;
        }

        private void ConnectCamera() {
            var wPlayer = FindViewById<WebView>(Resource.Id.camera);
            wPlayer.Settings.LoadWithOverviewMode = true;
            wPlayer.Settings.UseWideViewPort = true;

            wPlayer.SetPadding(0, 0, 0, 0);

            var current = Connectivity.NetworkAccess;
            if (current == NetworkAccess.Internet) { // If internet connection is present
                var profiles = Connectivity.ConnectionProfiles;
                if (profiles.Contains(ConnectionProfile.WiFi)) { // if wifi is enabled
                    WebSettings settings = wPlayer.Settings;
                    settings.JavaScriptEnabled = true;

                    wPlayer.LoadUrl("file:///android_asset/Camera.html?camera=" + camera_adress);
                }
                else {
                    wPlayer.LoadUrl("file:///android_asset/NoWifi.html");
                }
            }
            else {
                wPlayer.LoadUrl("file:///android_asset/NoInternet.html");
            }
        }
        private void DisconnectCamera() {
            var wPlayer = FindViewById<WebView>(Resource.Id.camera);
            wPlayer.Settings.LoadWithOverviewMode = true;
            wPlayer.Settings.UseWideViewPort = true;
            wPlayer.LoadUrl("");
        }

        private void InitialiseToolbar() {
            var toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar1);
            SetSupportActionBar(toolbar);
            SupportActionBar.Title = "Kursinis";
        }

        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
            Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            InitialiseToolbar();

            if (Preferences.Get("CamEnabled", false)) {
                ConnectCamera();
            }
            InitialiseButtons();
            DisableButtons();
        }

        protected override void OnPause() {
            BTDisconnect();
            base.OnPause();
        }

        public bool DownloadFileByDownloadManager(string imageURL) {
            try {
                string file_ext = ".jpg";
                string file_name = "Snapshot" + DateTime.Now.ToString("yyyy MM dd hh mm ss").Replace(" ", "") + file_ext;
                var path = Android.OS.Environment.DirectoryDcim;

                Android.Net.Uri uri = Android.Net.Uri.Parse(imageURL);
                DownloadManager.Request request = new DownloadManager.Request(uri);
                request.SetDestinationInExternalPublicDir(path, file_name);
                request.SetTitle(file_name);
                request.SetNotificationVisibility(DownloadVisibility.VisibleNotifyCompleted); // to notify when download is complete

                // Notify the user about the completed "download"
                var downloadManager = DownloadManager.FromContext(Android.App.Application.Context);

                var d_id = downloadManager.Enqueue(request);
                return true;
            }
            catch (Exception ex) {
                Android.App.AlertDialog.Builder alert = new Android.App.AlertDialog.Builder(this);
                alert.SetTitle("Error");
                alert.SetMessage("There has been an error downloading the screenshot. :(");

                alert.SetPositiveButton("OK", (senderAlert, args) => { });

                Dialog dialog = alert.Create();
                dialog.Show();

                return false;
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu) {
            MenuInflater.Inflate(Resource.Menu.settings, menu);

            var CamIcon = menu.FindItem(Resource.Id.CameraIcon);

            if (Preferences.Get("CamEnabled", false)) { // change the camera icon to enabled if the user enabled it
                CamIcon.SetIcon(Resource.Drawable.outline_videocam_24);
            }
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item) {
            if (item.ItemId == Resource.Id.settingsBtn) {
                var intent = new Intent(this, typeof(SettingsActivity));
                intent.PutExtra("pi_name", pi_name);
                intent.PutExtra("camera_adress", camera_adress);

                StartActivityForResult(intent, 1234);
            }

            else if (item.ItemId == Resource.Id.menu_info) {
                var intent = new Intent(this, typeof(InfoActivity));
                StartActivity(intent);
            }

            else if (item.ItemId == Resource.Id.connectionStatus) {
                BTsymbol = item;

                if (!BTconnected) {
                    BTConnect();
                }
                else {
                    BTDisconnect();
                    showShortToast(Application.Context, "Disconnected.");
                }
            }
            else if (item.ItemId == Resource.Id.CameraIcon) {
                if (Preferences.Get("CamEnabled", false) == false) { // jei buvo isjungta
                    Preferences.Set("CamEnabled", true);
                    item.SetIcon(Resource.Drawable.outline_videocam_24);
                    ConnectCamera();
                }
                else { // jei buvo ijungta
                    Preferences.Set("CamEnabled", false);
                    item.SetIcon(Resource.Drawable.outline_videocam_off_24);
                    DisconnectCamera();
                }
            }
            else if (item.ItemId == Resource.Id.snapshot_button) {
                if (!Preferences.Get("CamEnabled", false)) {
                    showShortToast(Application.Context, "Please turn on the camera, if you want to take a screenshot.");
                }
                else {
                    DownloadFileByDownloadManager(snapshot_address);
                }
            }
            else if (item.ItemId == Resource.Id.FunPageIcon) {
                Android.App.AlertDialog.Builder dialog = new Android.App.AlertDialog.Builder(this);
                Android.App.AlertDialog alert = dialog.Create();
                TextView title = new TextView(this) {
                    Text = "Let's have some fun!"
                };
                title.SetPadding(10, 10, 10, 10);
                title.Gravity = GravityFlags.Center;
                title.TextSize = 20;
                alert.SetCustomTitle(title);

                LayoutInflater inflater = (LayoutInflater)GetSystemService(LayoutInflaterService);
                View view = inflater.Inflate(Resource.Layout.fun_page, null);
                alert.SetView(view);
                alert.Show();
                InitialiseFunButtons(alert);
            }
            return base.OnOptionsItemSelected(item);
        }

        private void InitialiseFunButtons(Android.App.AlertDialog alert) {
            alert.FindViewById<Button>(Resource.Id.SurpriseBtn).Click += (s, e) => { if (BTconnected) sendData("Sr"); };
            alert.FindViewById<Button>(Resource.Id.LookAroundBtn).Click += (s, e) => { if (BTconnected) sendData("LA"); };
            alert.FindViewById<Button>(Resource.Id.TheNoBtn).Click += (s, e) => { if (BTconnected) sendData("No"); };
            alert.FindViewById<Button>(Resource.Id.YesBtn).Click += (s, e) => { if (BTconnected) sendData("Yes"); };
        }

        // Call Back method  to get the Message form other Activity  
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data) {
            base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode == 1234) {
                if (resultCode == Result.Ok) {
                    string updated_name = data.GetStringExtra("updated_name");
                    string updated_adress = data.GetStringExtra("updated_adress");

                    pi_name = updated_name;
                    camera_adress = updated_adress;
                    snapshot_address = get_snapshot_address(camera_adress);

                    Preferences.Set("pi_name", pi_name);
                    Preferences.Set("camera_adress", camera_adress);
                    Preferences.Set("snapshot_address", snapshot_address);

                    if (Preferences.Get("CamEnabled", false)) {
                        ConnectCamera();
                    }

                    showShortToast(Application.Context, "Settings saved.");
                }
            }
        }

        private string get_snapshot_address(string stream_addr) { // returns snapshot address from stream address
            string snap_address = stream_addr.Replace("?action=stream", "?action=snapshot");
            return snap_address;
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults) {
            Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }



}