using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Rssdp;
using System;
using System.Collections.Generic;

namespace Kursinis {
    [Activity(Label = "Activity1", ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]
    public class SettingsActivity : Activity{
        BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;
        LinearLayout BTSelection;
        string updated_name = null;

        protected override void OnCreate(Bundle savedInstanceState){
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.settings);
            ICollection<BluetoothDevice> devices = adapter.BondedDevices;

            BTSelection = FindViewById<LinearLayout>(Resource.Id.SelectedBTDevice);
            BTSelection.Click += BTSelection_clicked;

            string camera_adress = Intent.GetStringExtra("camera_adress");

            FindViewById<EditText>(Resource.Id.camera_adress).Text = camera_adress;

            FindViewById<Button>(Resource.Id.BtnCancel).Click += (o, e) =>{
                Finish();
            };

            FindViewById<Button>(Resource.Id.networkScanBtn).Click += (o, e) => {
                var intent = new Intent(this, typeof(Scanning));

                StartActivityForResult(intent, 1234);
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


    }
}