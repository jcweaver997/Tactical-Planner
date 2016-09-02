using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Threading;
using System.Text;
using Android.Locations;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Graphics;
using System.IO;

namespace Tactical_Plan
{

    [Activity(Label = "Tactical Planner", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity, ILocationListener
    {

        private LocationManager locationManager;
        private string locationProvider;
        private Location currentLocation;
        private LinearLayout ll;
        private List<BoxThing> boxlist;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            //Thread t = new Thread(GetStartTactics);
            //t.Start();
            boxlist = new List<BoxThing>();
            ll = FindViewById<LinearLayout>(Resource.Id.linearLayout1);
            SetupLocation();
            GetStartTactics();
        }
        #region location
        private void SetupLocation()
        {
            locationManager = (LocationManager)GetSystemService(LocationService);
            Criteria criteriaForLocationService = new Criteria { Accuracy = Accuracy.Fine };

            IList<string> acceptableLocationProviders = locationManager.GetProviders(criteriaForLocationService, true);
            if (acceptableLocationProviders.Any())
            {
                locationProvider = acceptableLocationProviders.First();
                locationManager.RequestLocationUpdates(locationProvider, 0, 0, this);
            }
            else
            {
                locationProvider = string.Empty;
            }
        }

        public void OnLocationChanged(Location location) {
            if (location == null)
            {
            }
            else
            {

                if (currentLocation==null)
                {
                    currentLocation = location;
                    ThreadPool.QueueUserWorkItem(o => GetStartTactics());
                }

                currentLocation = location;
            }
        }

        public void OnProviderDisabled(string provider) { }

        public void OnProviderEnabled(string provider) { }

        public void OnStatusChanged(string provider, Availability status, Bundle extras) {
            if (provider.Equals(locationProvider)&&status==Availability.OutOfService || status==Availability.TemporarilyUnavailable) {
                SetupLocation();
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            locationManager.RequestLocationUpdates(locationProvider, 0, 0, this);
        }

        protected override void OnPause()
        {
            base.OnPause();
            locationManager.RemoveUpdates(this);
        }
        #endregion
        private void GetStartTactics()
        {
            /*
                if (currentLocation == null)
                {
                    return;
                }
                
                double lat = currentLocation.Latitude;
                double lon = currentLocation.Longitude;
                */
            double lat = 32;
            double lon = -98;

                Tactical_Provider.TacticalCom connection = new Tactical_Provider.TacticalCom(Tactical_Provider.TacticalCom.State.Client);
                try
                {
                    connection.Start();
                }
                catch (Exception)
                {
                    RunOnUiThread(() => Toast.MakeText(this.ApplicationContext, Resource.String.ConError, ToastLength.Long));
                    return;
                }
                connection.Send(Tactical_Provider.TacticalCom.GETFIVE);
                connection.Send(BitConverter.GetBytes(lat));
                connection.Send(BitConverter.GetBytes(lon));
                while (connection.Recv(1)[0] == Tactical_Provider.TacticalCom.DATA)
                {
                    LinearLayout hl = new LinearLayout(this.ApplicationContext);
                    hl.Orientation = Orientation.Horizontal;
                    hl.SetVerticalGravity(GravityFlags.Center);
                    byte addressLength = connection.Recv(1)[0];

                    string address = Encoding.ASCII.GetString(connection.Recv(addressLength));
                byte nameLength = connection.Recv(1)[0];

                string name = Encoding.ASCII.GetString(connection.Recv(nameLength));
                int zip = BitConverter.ToInt32(connection.Recv(4), 0);
                    int id = BitConverter.ToInt32(connection.Recv(4), 0);
                    int piclength = BitConverter.ToInt32(connection.Recv(4), 0);
                    byte[] pic = connection.Recv(piclength);
                BoxThing b = BoxThing.Findboxthing(id);
                if (b != null)
                {
                    b.Show();
                }
                else {
                    new BoxThing(this, ll, boxlist, name, address, zip, id, pic).Show();
                }


                    //Console.WriteLine(address + ", " + zip + " at id " + id);
                }
                connection.Close();
            
        }
    }
}

