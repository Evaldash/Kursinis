using Android.App;
using Android.OS;

namespace Kursinis {
    [Activity(Label = "FunActivity")]
    public class FunActivity : Activity {

        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.fun_page);
        }
    }
}