using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Google.Android.Material.TextField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kursinis
{
    [Activity(Label = "Activity1", ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]
    public class SettingsActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.settings);

            string camera_adress = Intent.GetStringExtra("camera_adress");
            string pi_name = Intent.GetStringExtra("pi_name");

            FindViewById<EditText>(Resource.Id.pi_name).Text = pi_name;
            FindViewById<EditText>(Resource.Id.camera_adress).Text = camera_adress;

            FindViewById<Button>(Resource.Id.BtnCancel).Click += (o, e) =>
            {
                Finish();
            };

            FindViewById<Button>(Resource.Id.BtnSave).Click += (o, e) =>
            {
                string updated_name = FindViewById<EditText>(Resource.Id.pi_name).Text;
                string updated_adress = FindViewById<EditText>(Resource.Id.camera_adress).Text;

                var rtn = new Intent();
                rtn.PutExtra("updated_name", updated_name);
                rtn.PutExtra("updated_adress", updated_adress);

                SetResult(Result.Ok, rtn);
                Finish();
            };
        }
    }
}