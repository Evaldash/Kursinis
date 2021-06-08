using Android.App;
using Android.OS;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace Kursinis {
    [Activity(Label = "Activity1", ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]

    public class Scanning : Activity {

        private List<string> mItems;
        private ListView mListView;
        private ArrayAdapter<string> adapter;

        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Scanning);

            mItems = new List<string>();

            mListView = FindViewById<ListView>(Resource.Id.DeviceList);

            adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, mItems);

            mListView.Adapter = adapter;

            FindViewById<Button>(Resource.Id.ExitScan).Click += (o, e) => {
                //FindIPS("192.168.1.");
                Finish();
            };

            FindIPS("192.168.1.");


            //Action action = () => FindIPS("192.168.1.");

            //action.BeginInvoke(ar => action.EndInvoke(ar), null);
            //Invoke(action);
        }

        public async void FindIPS(string subadrr) {
            Thread.Sleep(500);
            Ping ping;
            IPAddress addr;
            PingReply pingReply;
            IPHostEntry host;
            string name;

            Parallel.For(1, 254, (i, loopState) => {

                ping = new Ping();
                string TestingAddr = subadrr + i;
                pingReply = ping.Send(TestingAddr);

                if (pingReply.Status == IPStatus.Success) {
                    string entry;
                    try {
                        addr = IPAddress.Parse(TestingAddr);
                        host = Dns.GetHostEntry(addr);
                        name = host.HostName;

                        entry = name + '(' + addr + ')';
                    }
                    catch (Exception ex) {
                        name = "?";
                        entry = name;
                    }
                    adapter.Add(entry);
                }
            });
        }

        public static bool PingHost(string nameOrAddress) {
            bool pingable = false;
            Ping pinger = null;

            try {
                pinger = new Ping();
                PingReply reply = pinger.Send(nameOrAddress);
                pingable = reply.Status == IPStatus.Success;
            }
            catch (PingException) {
                // Discard PingExceptions and return false;
            }
            finally {
                if (pinger != null) {
                    pinger.Dispose();
                }
            }
            return pingable;
        }
    }
}