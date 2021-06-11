using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Essentials;

namespace Kursinis {
    [Activity(Label = "Activity1", ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]
    public class SettingsActivity : Activity{
        BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;
        LinearLayout BTSelection;
        string updated_name = Preferences.Get("pi_name", "None");
        protected override void OnCreate(Bundle savedInstanceState){
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.settings);
            ICollection<BluetoothDevice> devices = adapter.BondedDevices;

            BTSelection = FindViewById<LinearLayout>(Resource.Id.SelectedBTDevice);
            BTSelection.Click += BTSelection_clicked;

            string camera_adress = Intent.GetStringExtra("camera_adress");

            FindViewById<EditText>(Resource.Id.camera_adress).Text = camera_adress;
            FindViewById<TextView>(Resource.Id.BTselectionText).Text = Preferences.Get("pi_name", "Choose selection");

            FindViewById<Button>(Resource.Id.BtnCancel).Click += (o, e) =>{
                Finish();
            };

            FindViewById<Button>(Resource.Id.networkScanBtn).Click += (o, e) => {

                var current = Connectivity.NetworkAccess;
                if (current == Xamarin.Essentials.NetworkAccess.Internet) { // If internet connection is present
                    var profiles = Connectivity.ConnectionProfiles;
                    if (profiles.Contains(ConnectionProfile.WiFi)) { // if wifi is enabled
                        var intent = new Intent(this, typeof(Scanning));
                        StartActivityForResult(intent, 1234);
                    }
                    else {
                        Toast.MakeText(Application.Context, "No WiFi found. Please connect your phone to the same WiFi network as your camera.", ToastLength.Short).Show();
                    }
                }
                else {
                    Toast.MakeText(Application.Context, "The Network scanner requires an Internet connection. Please make sure your device has an active Internet connection.", ToastLength.Short).Show();

                }
            };

            FindViewById<Button>(Resource.Id.BtnSave).Click += (o, e) =>{
                string updated_adress = FindViewById<EditText>(Resource.Id.camera_adress).Text;

                var rtn = new Intent();
                rtn.PutExtra("updated_name", updated_name);
                rtn.PutExtra("updated_adress", updated_adress);

                SetResult(Result.Ok, rtn);
                Finish();
            };
        }
        private void BTSelection_clicked(object sender, System.EventArgs e) {
            PopupMenu popupMenu = new PopupMenu(this, BTSelection);
            popupMenu.MenuItemClick += PopupMenu_MenuItemClick;

            if (BluetoothAdapter.DefaultAdapter.BondedDevices.Count == 0){
                Toast.MakeText(Application.Context, "No paired devices found. Please pair your Pi via Bluetooth.", ToastLength.Short).Show();
            }

            else {
                int i = 1;
                foreach (var pairedDevice in BluetoothAdapter.DefaultAdapter.BondedDevices){
                    popupMenu.Menu.Add(IMenu.None, i, i, pairedDevice.Name);
                }
                popupMenu.Show();
            }
        }

        private void PopupMenu_MenuItemClick(object sender, PopupMenu.MenuItemClickEventArgs e){
            FindViewById<TextView>(Resource.Id.BTselectionText).Text = e.Item.TitleFormatted.ToString();
            updated_name = e.Item.TitleFormatted.ToString();
        }

        // Call Back method  to get the Message form other Activity  
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data) {
            base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode == 1234) {
                if (resultCode == Result.Ok) {
                    FindViewById<EditText>(Resource.Id.camera_adress).Text = data.GetStringExtra("selected_camera");
                    Toast.MakeText(Application.Context, "Camera adress saved.", ToastLength.Short).Show();
                }
            }
        }
    }
}