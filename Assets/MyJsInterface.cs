using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using Java.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kursinis
{
    class MyJsInterface : Java.Lang.Object
    {
        Context context;
        string camera_adress;

        public MyJsInterface(Context context, string cam)
        {
            camera_adress = cam;
            this.context = context;
        }

        [JavascriptInterface]
        [Export]
        public void ShowToast(string msg)
        {
            Toast.MakeText(context, msg, ToastLength.Short).Show();
        }

        public string GiveCamera() {
            Toast.MakeText(context, camera_adress, ToastLength.Short).Show();
            return camera_adress;
            
        }
    }
}