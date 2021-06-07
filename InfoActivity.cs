using Android.App;
using Android.OS;
using Android.Widget;

namespace Kursinis {
    [Activity(Label = "Activity1", ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]
    public class InfoActivity : Activity{
        protected override void OnCreate(Bundle savedInstanceState){
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Info);

            TextView textview = FindViewById<TextView>(Resource.Id.Information);

            string tekstas = "Welcome! To use this app, you need to have a pi, or arduino with a running bluetooth server and mjpeg camera server. (you can use a headless camera too, as long as it supports the mjpeg format) \n" +
                "Once a succesful Bluetooth connection with your pi/arduino is established, these commands are sent to start, and stop the motors: \n" +
                "U - up start \n" +
                "u - up stop \n" +
                "D - down start \n" +
                "d - down stop \n" +
                "L - left start \n" +
                "l - left stop \n" +
                "R - right start \n" +
                "r - right stop \n" +
                "\n" +
                "Typically the mjpeg camera adress will look something like this: \n http://192.168.1.111:8080/?action=stream \n You can change this setting, and your pi name in the settings page. \n" +
                "\n" + " This project is open source, and can be found on GitHub: https://github.com/Evaldash/Kursinis " + "\n" +
                "If you encounter a bug, feel free to report it on GitHub in the issues section. Feel free to contribute by adding features, if you wish.";

            textview.Text=tekstas;
        }
    }
}