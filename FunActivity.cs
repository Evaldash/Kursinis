using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kursinis {
    [Activity(Label = "FunActivity")]
    public class FunActivity : Activity {
        private void InitialiseFunButtons() {
            FindViewById<Button>(Resource.Id.GoCrazyBtn).Click += (s, e) => { };
            FindViewById<Button>(Resource.Id.TurnAroundBtn).Click += (s, e) => { };
            FindViewById<Button>(Resource.Id.LookAroundBtn).Click += (s, e) => { };
            FindViewById<Button>(Resource.Id.LightShowBtn).Click += (s, e) => { };
            FindViewById<Button>(Resource.Id.LightFlashBtn).Click += (s, e) => { };
            FindViewById<Button>(Resource.Id.TheNoBtn).Click += (s, e) => { };
        }

        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.fun_page);

            InitialiseFunButtons();


            // Create your application here
        }
    }
}