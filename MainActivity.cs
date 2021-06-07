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
using System.Linq;
using System.Text;
using Xamarin.Essentials;

namespace Kursinis
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true, ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]
    public class MainActivity : AppCompatActivity{
        BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;
        private BluetoothSocket _socket;
        private bool BTconnected = false;

        BluetoothDevice device;
        IMenuItem BTsymbol;

        private string pi_name = Preferences.Get("pi_name", "raspberrypi");
        private string camera_adress = Preferences.Get("camera_adress", "http://192.168.1.111:8080/?action=stream");

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
                        Toast.MakeText(Application.Context, "Error: no response from Bluetooth server.", ToastLength.Short).Show();
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
                Toast.MakeText(Application.Context, "Conneection lost!", ToastLength.Short).Show();
                BTDisconnect();
            }
        }
        private void InitialiseButtons(){
            FindViewById<ImageButton>(Resource.Id.btnUp).Touch += (s, e) => { // Up
                var handled = false;
                if (e.Event.Action == MotionEventActions.Down){
                    handled = true;
                    sendData("U");
                }
                else if (e.Event.Action == MotionEventActions.Up) {
                    handled = true;
                    sendData("u");
                }
                e.Handled = handled;
            };

            FindViewById<ImageButton>(Resource.Id.btnDown).Touch += (s, e) => { // Down
                var handled = false;
                if (e.Event.Action == MotionEventActions.Down){
                    handled = true;
                    sendData("D");
                }
                else if (e.Event.Action == MotionEventActions.Up){
                    handled = true;
                    sendData("d");
                }
                e.Handled = handled;
            };

            FindViewById<ImageButton>(Resource.Id.btnLeft).Touch += (s, e) => { // Left
                var handled = false;
                if (e.Event.Action == MotionEventActions.Down) {
                    handled = true;
                    sendData("L");
                }
                else if (e.Event.Action == MotionEventActions.Up) {
                    handled = true;
                    sendData("l");
                }
                e.Handled = handled;
            };

            FindViewById<ImageButton>(Resource.Id.btnRight).Touch += (s, e) => { // Right
                var handled = false;
                if (e.Event.Action == MotionEventActions.Down){
                    handled = true;
                    sendData("R");
                }
                else if (e.Event.Action == MotionEventActions.Up){
                    handled = true;
                    sendData("r");
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
            if (current == NetworkAccess.Internet) { // If internet connection is present
                var profiles = Connectivity.ConnectionProfiles;
                if (profiles.Contains(ConnectionProfile.WiFi)){ // if wifi is enabled
                    WebSettings settings = wPlayer.Settings;
                    settings.JavaScriptEnabled = true;
                    
                    wPlayer.AddJavascriptInterface(new Kursinis.MyJsInterface(this, camera_adress), "CSharp");

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
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            InitialiseToolbar();

            if (Preferences.Get("CamEnabled", false)) {
                ConnectCamera();
            }
            InitialiseButtons();
            DisableButtons();
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
            if (item.ItemId == Resource.Id.settingsBtn){
                var intent = new Intent(this, typeof(SettingsActivity));
                intent.PutExtra("pi_name", pi_name);
                intent.PutExtra("camera_adress", camera_adress);

                StartActivityForResult(intent, 1234);
            }

            else if (item.ItemId == Resource.Id.menu_info){
                var intent = new Intent(this, typeof(InfoActivity));
                StartActivity(intent);
            }

            else if (item.ItemId == Resource.Id.connectionStatus){
                BTsymbol = item;

                if (!BTconnected){
                    BTConnect();
                }
                else{
                    Toast.MakeText(Application.Context, "Disconnected.", ToastLength.Short).Show();
                    BTDisconnect();
                }
            }

            if (item.ItemId == Resource.Id.CameraIcon) {
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

                    Preferences.Set("pi_name", pi_name);
                    Preferences.Set("camera_adress", camera_adress);

                    if (Preferences.Get("CamEnabled", false)) {
                        ConnectCamera();
                    }

                    Toast.MakeText(Application.Context, "Settings saved.", ToastLength.Short).Show();
                }
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults){
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}