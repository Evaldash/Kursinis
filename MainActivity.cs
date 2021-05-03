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
    public class MainActivity : AppCompatActivity
    {
        BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;
        private BluetoothSocket _socket;
        private bool connected = false;
        BluetoothDevice device;
        IMenuItem BTsymbol;

        private string pi_name = "raspberrypi";
        private string camera_adress = "http://192.168.1.111:8080/?action=stream";

        private async void Connect()
        {
            if (adapter == null)
            {
                connected = false;
                Toast.MakeText(Application.Context, "Error: no Bluetooth device found.", ToastLength.Short).Show();
            }
            else if (!adapter.IsEnabled)
            {
                connected = false;
                Toast.MakeText(Application.Context, "Error: Bluetooth device is turned off.", ToastLength.Short).Show();
            }
            else
            {
                device = (from bd in adapter.BondedDevices
                          where bd.Name == pi_name
                          select bd).FirstOrDefault();

                if (device == null)
                {
                    connected = false;
                    Toast.MakeText(Application.Context, "Error: no " + pi_name + " device found.", ToastLength.Short).Show();
                }
                else
                {
                    try
                    {
                        _socket = device.CreateRfcommSocketToServiceRecord(UUID.FromString("00001101-0000-1000-8000-00805f9b34fb"));
                        await _socket.ConnectAsync();
                        Success();
                    }
                    catch (Java.IO.IOException)
                    {
                        Toast.MakeText(Application.Context, "Error: no response from Bluetooth server.", ToastLength.Short).Show();
                    }
                }
            }
        }
        private void Success()
        {
            Toast.MakeText(Application.Context, "Conneection succesful.", ToastLength.Short).Show();
            
            BTsymbol.SetIcon(Resource.Drawable.BTConnected);
            BTsymbol.SetTitle("Disconnect");
            EnableButtons();
            connected = true;
        }

        private void Disconnect()
        {
            _socket.Close();

            BTsymbol.SetIcon(Resource.Drawable.BTDisconnected);
            BTsymbol.SetTitle("Connect");
            DisableButtons();
            connected = false;
        }
        private async void sendData(string strbuffer)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(strbuffer);

            try
            {
                await _socket.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            }
            catch (Java.IO.IOException) {
                Toast.MakeText(Application.Context, "Conneection lost!", ToastLength.Short).Show();
                Disconnect();

            }
        }
        private void InitialiseButtons()
        {
            FindViewById<ImageButton>(Resource.Id.btnUp).Touch += (s, e) =>
            {
                var handled = false;
                if (e.Event.Action == MotionEventActions.Down)
                {
                    handled = true;
                    sendData("U");
                }
                else if (e.Event.Action == MotionEventActions.Up)
                {
                    handled = true;
                    sendData("u");
                }
                e.Handled = handled;
            };

            FindViewById<ImageButton>(Resource.Id.btnDown).Touch += (s, e) => // atgal
            {
                var handled = false;
                if (e.Event.Action == MotionEventActions.Down)
                {
                    handled = true;
                    sendData("D");
                }
                else if (e.Event.Action == MotionEventActions.Up)
                {
                    handled = true;
                    sendData("d");
                }
                e.Handled = handled;
            }; ;

            FindViewById<ImageButton>(Resource.Id.btnLeft).Touch += (s, e) => // kairen
            {
                var handled = false;
                if (e.Event.Action == MotionEventActions.Down)
                {
                    handled = true;
                    sendData("L");
                }
                else if (e.Event.Action == MotionEventActions.Up)
                {
                    handled = true;
                    sendData("l");
                }
                e.Handled = handled;
            };

            FindViewById<ImageButton>(Resource.Id.btnRight).Touch += (s, e) => // desinen
            {
                var handled = false;
                if (e.Event.Action == MotionEventActions.Down)
                {
                    handled = true;
                    sendData("R");
                }
                else if (e.Event.Action == MotionEventActions.Up)
                {
                    handled = true;
                    sendData("r");
                }
                e.Handled = handled;
            };
        }

        private void EnableButtons()
        {
            FindViewById<ImageButton>(Resource.Id.btnUp).Enabled = true;
            FindViewById<ImageButton>(Resource.Id.btnDown).Enabled = true;
            FindViewById<ImageButton>(Resource.Id.btnLeft).Enabled = true;
            FindViewById<ImageButton>(Resource.Id.btnRight).Enabled = true;
        }

        private void DisableButtons()
        {
            FindViewById<ImageButton>(Resource.Id.btnUp).Enabled = false;
            FindViewById<ImageButton>(Resource.Id.btnDown).Enabled = false;
            FindViewById<ImageButton>(Resource.Id.btnLeft).Enabled = false;
            FindViewById<ImageButton>(Resource.Id.btnRight).Enabled = false;
        }

        private void ConnectCamera()
        {
            var wPlayer = FindViewById<WebView>(Resource.Id.camera);
            //Zoom Out for stretch the content by the width. 
            wPlayer.Settings.LoadWithOverviewMode = true;
            wPlayer.Settings.UseWideViewPort = true;

            wPlayer.SetPadding(0, 0, 0, 0);

            var current = Connectivity.NetworkAccess;
            if (current == NetworkAccess.Internet) // jei yra internetas
            {
                var profiles = Connectivity.ConnectionProfiles;
                if (profiles.Contains(ConnectionProfile.WiFi)) // jei prisijunges prie wifi
                {
                    wPlayer.LoadUrl(camera_adress);
                }
                else
                {
                    wPlayer.LoadUrl("file:///android_asset/NoConnection.html");
                }
            }
            else
            {
                wPlayer.LoadUrl("file:///android_asset/NoConnection.html");
            }
        }

        private void InitialiseToolbar()
        {
            var toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar1);
            SetSupportActionBar(toolbar);
            SupportActionBar.Title = "Kursinis";

        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            InitialiseToolbar();
            ConnectCamera();
            InitialiseButtons();
            DisableButtons();
        }

       public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.settings, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.settingsBtn)
            {
                var intent = new Intent(this, typeof(SettingsActivity));
                intent.PutExtra("pi_name", pi_name);
                intent.PutExtra("camera_adress", camera_adress);

                StartActivityForResult(intent, 1234);
            }

            if (item.ItemId == Resource.Id.menu_info)
            {


            }

            if (item.ItemId == Resource.Id.connectionStatus)
            {
                BTsymbol = item;

                if (!connected)
                {
                    Toast.MakeText(Application.Context, "Connecting...", ToastLength.Short).Show();
                    Connect();
                }
                else
                {
                    Toast.MakeText(Application.Context, "Disconnected.", ToastLength.Short).Show();
                    Disconnect();
                }
            }
            return base.OnOptionsItemSelected(item);
        }

        // Call Back method  to get the Message form other Activity  
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode == 1234)
            {
                if (resultCode == Result.Ok)
                {
                    string updated_name = data.GetStringExtra("updated_name");
                    string updated_adress = data.GetStringExtra("updated_adress");

                    pi_name = updated_name;
                    camera_adress = updated_adress;

                    ConnectCamera();
                    Toast.MakeText(Application.Context, "Settings saved.", ToastLength.Short).Show();
                }
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}