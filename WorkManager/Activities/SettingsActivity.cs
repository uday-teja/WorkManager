using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Support.V7.Widget;
using Newtonsoft.Json;

namespace TaskManager.Activities
{
    [Activity(Label = "Settings", Theme = "@style/AppTheme", MainLauncher = false)]
    public class SettingsActivity : AppCompatActivity
    {
        public Android.Widget.Switch notificationSwitch { get; set; }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.settings_view);
            SetToolbar();
            notificationSwitch = FindViewById<Android.Widget.Switch>(Resource.Id.notificationSwitch);
            notificationSwitch.Checked = Intent.GetStringExtra("IsNotificationsEnabled") == "True";
        }

        private void SetToolbar()
        {
            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Android.Resource.Id.Home)
            {
                Intent isNotify = new Intent(this, typeof(SettingsActivity));
                isNotify.PutExtra("IsNotificationsEnabled", notificationSwitch.Checked.ToString());
                SetResult(Result.Ok, isNotify);
                this.OnBackPressed();
            }
            return base.OnOptionsItemSelected(item);
        }
    }
}