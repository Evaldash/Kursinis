using Android.Content;
using Android.Webkit;
using Android.Widget;
using Java.Interop;
using System;

namespace Kursinis {
    class MyJsInterface : Java.Lang.Object
    {
        Context context;
        String data = "false";
        public MyJsInterface(Context context)
        {
            this.context = context;
        }

        public String giveData() {
            //Get the string value to process
            return this.data;
        }

        [JavascriptInterface]
        [Export]
        public void ShowToast(string msg)
        {
            Toast.MakeText(context, msg, ToastLength.Short).Show();
        }

        [JavascriptInterface]
        [Export]
        public void SendData(String data) {
            //Get the string value to process
            this.data = data;
        }

    }
}