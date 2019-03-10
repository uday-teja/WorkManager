using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;

namespace TaskManager.Activities
{
    [Activity(MainLauncher = true, Theme = "@style/Theme.Splash", NoHistory = true, Icon = "@drawable/logo")]
    public class SplashActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            StartMainActivity();
        }

        private async void StartMainActivity()
        {
            await Task.Delay(500);
            StartActivity(typeof(MainActivity));
        }
    }
}