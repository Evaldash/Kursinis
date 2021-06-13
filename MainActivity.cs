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
using System.Timers;

namespace Kursinis {
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true, ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]
    public class MainActivity : AppCompatActivity{
        BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;
        private BluetoothSocket _socket;
        private bool BTconnected = false;

        BluetoothDevice device;
        IMenuItem BTsymbol;

        private string pi_name = Preferences.Get("pi_name", "raspberrypi");
        private string camera_adress = Preferences.Get("camera_adress", "http://192.168.1.111:8080/?action=stream");
        private string snapshot_address = Preferences.Get("snapshot_address", "http://192.168.1.111:8080/?action=snapshot");
        private bool BTadapterOK() {
            if (adapter == null){
                BTconnected = false;
                Toast.MakeText(Application.Context, "Error: no Bluetooth device found.", ToastLength.Short).Show();
                return false;
            }
            else if (!adapter.IsEnabled){
                BTconnected = false;
                Toast.MakeText(Application.Context, "Error: Bluetooth device is turned off.", ToastLength.Short).Show();
                return false;
            }
            return true;
        }

        private async void BTConnect(){
            if (BTadapterOK()){
                device = (from bd in adapter.BondedDevices
                          where bd.Name == pi_name
                          select bd).FirstOrDefault();

                if (device == null){
                    BTconnected = false;
                    Toast.MakeText(Application.Context, "Error: no " + pi_name + " device found.", ToastLength.Short).Show();
                }
                else{
                    try{
                        _socket = device.CreateRfcommSocketToServiceRecord(UUID.FromString("00001101-0000-1000-8000-00805f9b34fb"));
                        await _socket.ConnectAsync();
                        BTSuccess();
                    }
                    catch (Java.IO.IOException){
                        Toast.MakeText(Application.Context, "Error: couldn't connect to Bluetooth server.", ToastLength.Short).Show();
                    }
                }
            }
        }
        private void BTSuccess(){
            Toast.MakeText(Application.Context, "Conneection succesful.", ToastLength.Short).Show();
            BTsymbol.SetIcon(Resource.Drawable.BTConnected);
            EnableButtons();
            BTconnected = true;
        }

        private void BTDisconnect(){
            _socket.Close();
            BTsymbol.SetIcon(Resource.Drawable.BTDisconnected);
            DisableButtons();
            BTconnected = false;
        }
        private async void sendData(string strbuffer){
            byte[] buffer = Encoding.ASCII.GetBytes(strbuffer);

            try{
                await _socket.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            }
            catch (Java.IO.IOException) {
                Toast.MakeText(Application.Context, "Connection lost!", ToastLength.Short).Show();
                BTDisconnect();
            }
        }
        
        private void InitialiseButtons(){ // initialises buttons to work with the Pi
            System.Timers.Timer fwdtimer = new System.Timers.Timer(); // data sending timer. Checks whether the buttons are still pressed, if so, sends data every second.
            fwdtimer.Interval = 1000;
            fwdtimer.Elapsed += (s, e) => sendData("U");

            FindViewById<ImageButton>(Resource.Id.btnUp).Touch += (s, e) => { // Up
                var handled = false;
                if (e.Event.Action == MotionEventActions.Down){
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

            System.Timers.Timer bcktimer = new System.Timers.Timer();
            bcktimer.Interval = 1000;
            bcktimer.Elapsed += (s, e) => sendData("D");

            FindViewById<ImageButton>(Resource.Id.btnDown).Touch += (s, e) => { // Down
                var handled = false;
                if (e.Event.Action == MotionEventActions.Down){
                    bcktimer.Start();
                    sendData("D");
                    handled = true;
                }
                else if (e.Event.Action == MotionEventActions.Up){
                    bcktimer.Stop();
                    sendData("d");
                    handled = true;
                }
                e.Handled = handled;
            };

            System.Timers.Timer Lefttimer = new System.Timers.Timer();
            Lefttimer.Interval = 1000;
            Lefttimer.Elapsed += (s, e) => sendData("L");

            FindViewById<ImageButton>(Resource.Id.btnLeft).Touch += (s, e) => { // Left
                var handled = false;
                if (e.Event.Action == MotionEventActions.Down) {
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

            System.Timers.Timer Righttimer = new System.Timers.Timer();
            Righttimer.Interval = 1000;
            Righttimer.Elapsed += (s, e) => sendData("R");

            FindViewById<ImageButton>(Resource.Id.btnRight).Touch += (s, e) => { // Right
                var handled = false;
                if (e.Event.Action == MotionEventActions.Down){
                    sendData("R");
                    handled = true;
                    Righttimer.Start();
                }
                else if (e.Event.Action == MotionEventActions.Up){
                    sendData("r");
                    handled = true;
                    Righttimer.Stop();
                }
                e.Handled = handled;
            };
        }

        private void EnableButtons(){
            FindViewById<ImageButton>(Resource.Id.btnUp).Enabled = true;
            FindViewById<ImageButton>(Resource.Id.btnDown).Enabled = true;
            FindViewById<ImageButton>(Resource.Id.btnLeft).Enabled = true;
            FindViewById<ImageButton>(Resource.Id.btnRight).Enabled = true;
        }

        private void DisableButtons(){
            FindViewById<ImageButton>(Resource.Id.btnUp).Enabled = false;
            FindViewById<ImageButton>(Resource.Id.btnDown).Enabled = false;
            FindViewById<ImageButton>(Resource.Id.btnLeft).Enabled = false;
            FindViewById<ImageButton>(Resource.Id.btnRight).Enabled = false;
        }

        private void ConnectCamera(){
            var wPlayer = FindViewById<WebView>(Resource.Id.camera);
            wPlayer.Settings.LoadWithOverviewMode = true;
            wPlayer.Settings.UseWideViewPort = true;

            wPlayer.SetPadding(0, 0, 0, 0);

            var current = Connectivity.NetworkAccess;
            if (current == Xamarin.Essentials.NetworkAccess.Internet) { // If internet connection is present
                var profiles = Connectivity.ConnectionProfiles;
                if (profiles.Contains(ConnectionProfile.WiFi)){ // if wifi is enabled
                    WebSettings settings = wPlayer.Settings;
                    settings.JavaScriptEnabled = true;

                    wPlayer.LoadUrl("file:///android_asset/Camera.html?camera="+camera_adress);
                }
                else{
                    wPlayer.LoadUrl("file:///android_asset/NoWifi.html");
                }
            }
            else{
                wPlayer.LoadUrl("file:///android_asset/NoInternet.html");
            }
        }
        private void DisconnectCamera() {
            var wPlayer = FindViewById<WebView>(Resource.Id.camera);
            wPlayer.Settings.LoadWithOverviewMode = true;
            wPlayer.Settings.UseWideViewPort = true;
            wPlayer.LoadUrl("");
        }

        private void InitialiseToolbar(){
            var toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar1);
            SetSupportActionBar(toolbar);
            SupportActionBar.Title = "Kursinis";
        }

        protected override void OnCreate(Bundle savedInstanceState){
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

                alert.SetPositiveButton("OK", (senderAlert, args) => {});

                Dialog dialog = alert.Create();
                dialog.Show();

                return false;
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu){
            MenuInflater.Inflate(Resource.Menu.settings, menu);

            var CamIcon = menu.FindItem(Resource.Id.CameraIcon);

            if (Preferences.Get("CamEnabled", false)) { // change the camera icon to enabled if the user enabled it
                CamIcon.SetIcon(Resource.Drawable.outline_videocam_24);
            }
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item){
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
                    Toast.MakeText(Application.Context, "Disconnected.", ToastLength.Short).Show();
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
                    Toast.MakeText(Application.Context, "Please turn on the camera, if you want to take a screenshot.", ToastLength.Short).Show();
                }
                else {
                    DownloadFileByDownloadManager(snapshot_address);
                }
            }
            return base.OnOptionsItemSelected(item);
        }

        // Call Back method  to get the Message form other Activity  
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data){
            base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode == 1234){
                if (resultCode == Result.Ok){
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

                    Toast.MakeText(Application.Context, "Settings saved.", ToastLength.Short).Show();
                }
            }
        }

        private string get_snapshot_address(string stream_addr) {
            string snap_address = stream_addr.Replace("?action=stream", "?action=snapshot");
            return snap_address;
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults){
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}