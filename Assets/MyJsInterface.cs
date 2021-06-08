using Android.Content;
using Android.Webkit;
using Android.Widget;
using Java.Interop;

namespace Kursinis {
    class MyJsInterface : Java.Lang.Object
    {
        Context context;
        public MyJsInterface(Context context)
        {
            this.context = context;
        }

        [JavascriptInterface]
        [Export]
        public void ShowToast(string msg)
        {
            Toast.MakeText(context, msg, ToastLength.Short).Show();
        }
        public bool CameraFound() {
            return true;
        }
        public bool CameraFailure() {
            return false;
        }
    }
}