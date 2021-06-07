using Android.App;
using Android.OS;
using Android.Widget;
using Rssdp;
using System;
using System.Collections.Generic;

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
                Finish();
            };

            adapter.Add("Hello");

            BeginSearch();
        }

        // Define _DeviceLocator as a field so it doesn't get GCed after the method ends, and it can
        // continue to listen for notifications until it is explicitly stopped 
        // (with a call to _DeviceLocator.StopListeningForNotifications();)
        private SsdpDeviceLocator _DeviceLocator;

        // Call this method from somewhere in your code to start the search.
        public void BeginSearch() {
            _DeviceLocator = new SsdpDeviceLocator();

            // (Optional) Set the filter so we only see notifications for devices we care about 
            // (can be any search target value i.e device type, uuid value etc - any value that appears in the 
            // DiscoverdSsdpDevice.NotificationType property or that is used with the searchTarget parameter of the Search method).
            _DeviceLocator.NotificationFilter = "upnp:rootdevice";

            // Connect our event handler so we process devices as they are found
            _DeviceLocator.DeviceAvailable += deviceLocator_DeviceAvailable;

            // Enable listening for notifications (optional)
            _DeviceLocator.StartListeningForNotifications();

            // Perform a search so we don't have to wait for devices to broadcast notifications 
            // again to get any results right away (notifications are broadcast periodically).
            _DeviceLocator.SearchAsync();

            Console.ReadLine();
        }

        // Process each found device in the event handler
        async static void deviceLocator_DeviceAvailable(object sender, DeviceAvailableEventArgs e) {
            //Device data returned only contains basic device details and location of full device description.
            //Console.WriteLine("Found " + e.DiscoveredDevice.Usn + " at " + e.DiscoveredDevice.DescriptionLocation.ToString());

            //Can retrieve the full device description easily though.
            var fullDevice = await e.DiscoveredDevice.GetDeviceInfo();

            //Console.WriteLine(fullDevice.FriendlyName);
            //Console.WriteLine();

            

            //adapter.Add(fullDevice.FriendlyName);


        }




    }
}