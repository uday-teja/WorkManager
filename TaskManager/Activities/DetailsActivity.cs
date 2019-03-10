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
using Android.Widget;
using Newtonsoft.Json;
using TaskManager.Model;
using TaskManager.Service;

namespace TaskManager.Activities
{
    [Activity(Label = "Task Details", Theme = "@style/AppTheme", MainLauncher = false)]
    public class DetailsActivity : AppCompatActivity
    {
        private Task SelectedTask { get; set; }
        private TaskService TaskService { get; set; }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            TaskService = new TaskService();
            SetContentView(Resource.Layout.task_details_view);
            SetToolbar();
            SetTaskDetails();
        }

        private void SetToolbar()
        {
            var toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
        }

        private void SetTaskDetails()
        {
            SelectedTask = JsonConvert.DeserializeObject<Task>(Intent.GetStringExtra("taskDetails"));
            FindViewById<TextView>(Resource.Id.taskdetailname).Text = SelectedTask.Name;
            FindViewById<TextView>(Resource.Id.taskDetaildescription).Text = SelectedTask.Description;
            FindViewById<TextView>(Resource.Id.taskDetailpriority).Text = SelectedTask.Priority.ToString();
            FindViewById<TextView>(Resource.Id.taskDetailstatus).Text = SelectedTask.Status.ToString();
            FindViewById<TextView>(Resource.Id.taskDetaildue_date).Text = SelectedTask.DueDate.ToShortDateString();
            FindViewById<TextView>(Resource.Id.taskDetaildue_time).Text = SelectedTask.DueDate.ToString("hh:mm tt").ToUpper();
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    this.OnBackPressed();
                    break;
                case Resource.Id.edit:
                    EditTask();
                    break;
                case Resource.Id.delete:
                    DeleteTask();
                    break;
                case Resource.Id.share:
                    ShareTask();
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }

        private void ShareTask()
        {
            Intent sharingIntent = new Intent(Intent.ActionSend);
            sharingIntent.SetType("text/plain");
            sharingIntent.PutExtra(Intent.ExtraText, $"Name: {SelectedTask.Name}\nDescription: {SelectedTask.Description}\nStatus: {SelectedTask.Status}\nDue Date: {SelectedTask.DueDate}");
            StartActivity(Intent.CreateChooser(sharingIntent, "Share your task"));
        }

        private void EditTask()
        {
            this.TaskService.UpdateTask(this.SelectedTask);
            Intent addTask = new Intent(this, typeof(AddEditActivity));
            addTask.PutExtra("type", JsonConvert.SerializeObject(Crud.Add));
            addTask.PutExtra("SelectedTask", JsonConvert.SerializeObject(this.SelectedTask));
            this.StartActivityForResult(addTask, 1);
        }

        private void DeleteTask()
        {
            this.TaskService.DeleteTask(this.SelectedTask);
            var dialog = new Android.Support.V7.App.AlertDialog.Builder(this);
            dialog.SetTitle("Delete Task").SetMessage("Are you sure to delete the task?").SetPositiveButton("Yes", OnDeleteTask).SetNegativeButton("No", delegate { });
            dialog.Create().Show();
        }

        private void OnDeleteTask(object sender, DialogClickEventArgs e)
        {
            if (this.SelectedTask != null)
            {
                this.TaskService.DeleteTask(this.SelectedTask);
                Intent deleteTask = new Intent(this, typeof(MainActivity));
                deleteTask.PutExtra("type", JsonConvert.SerializeObject(Crud.Delete));
                deleteTask.PutExtra("task", JsonConvert.SerializeObject(SelectedTask));
                SetResult(Result.Ok, deleteTask);
                Finish();
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.action_menu, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (resultCode != Result.Canceled)
            {
                if (requestCode == 1)
                {
                    SetResult(Result.Ok, data);
                    Finish();
                }
            }
        }
    }
}