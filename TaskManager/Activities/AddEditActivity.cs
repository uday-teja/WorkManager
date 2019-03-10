using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Text;
using Java.Util;
using Newtonsoft.Json;
using TaskManager.Model;
using TaskManager.Service;
using static Android.App.DatePickerDialog;
using static Android.App.TimePickerDialog;

namespace TaskManager.Activities
{
    [Activity(Label = "@string/add_task", Theme = "@style/AppTheme", MainLauncher = false)]
    public class AddEditActivity : AppCompatActivity, IOnDateSetListener, IOnTimeSetListener
    {
        private EditText dueDate;
        private EditText dueTime;
        private Calendar calendar;
        private Task currentTask;
        private bool isUpdate;
        private TaskService taskService;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.add_task_view);
            SetToolbar();
            SetStatusSpinner();
            SetDueDatePicker();
            SetPrioritySpinner();
            currentTask = new Task();
            this.taskService = new TaskService();
            calendar = Calendar.Instance;
            var selectedTask = Intent.GetStringExtra("SelectedTask") ?? string.Empty;
            if (selectedTask != string.Empty)
            {
                isUpdate = true;
                EditTask();
            }
            FindViewById<LinearLayout>(Resource.Id.priority_layout).Visibility = isUpdate ? ViewStates.Visible : ViewStates.Gone;
        }

        private void SetToolbar()
        {
            var toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
        }

        private void EditTask()
        {
            currentTask = JsonConvert.DeserializeObject<Task>(Intent.GetStringExtra("SelectedTask"));
            FindViewById<TextView>(Resource.Id.name).Text = currentTask.Name;
            FindViewById<TextView>(Resource.Id.description).Text = currentTask.Description;
            FindViewById<Spinner>(Resource.Id.status).SetSelection((int)currentTask.Status);
            FindViewById<Spinner>(Resource.Id.priority).SetSelection((int)currentTask.Priority);
            FindViewById<TextView>(Resource.Id.due_date).Text = currentTask.DueDate.ToShortDateString();
            FindViewById<TextView>(Resource.Id.due_time).Text = currentTask.DueDate.ToString("hh:mm tt").ToUpper();
            this.SetTitle(Resource.String.update_task);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.item_actions_menu, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        private void SetPrioritySpinner()
        {
            var priority = FindViewById<Spinner>(Resource.Id.priority);
            priority.Adapter = new ArrayAdapter<string>(this, Resource.Layout.support_simple_spinner_dropdown_item, Enum.GetValues(typeof(Priority)).Cast<Priority>().Select(e => e.ToString()).ToArray());
        }

        private void AddTaskClick()
        {
            currentTask.Name = FindViewById<EditText>(Resource.Id.name).Text;
            currentTask.Description = FindViewById<EditText>(Resource.Id.description).Text;
            currentTask.Priority = (Priority)FindViewById<Spinner>(Resource.Id.priority).SelectedItemPosition;
            currentTask.Status = (Status)FindViewById<Spinner>(Resource.Id.status).SelectedItemPosition;
            var date = $"{FindViewById<EditText>(Resource.Id.due_date).Text} {FindViewById<EditText>(Resource.Id.due_time).Text}";
            currentTask.DueDate = date != " " ? currentTask.DueDate = Convert.ToDateTime(date) : DateTime.Now.AddDays(1);
            if (IsValidTask())
                AddEditTask();
        }

        private bool IsValidTask()
        {
            bool valid = true;
            if (currentTask.Name.Trim() == string.Empty)
            {
                valid = false;
                FindViewById<EditText>(Resource.Id.name).Error = "Requried Field";
            }
            if (currentTask.Description.Trim() == string.Empty)
            {
                valid = false;
                FindViewById<EditText>(Resource.Id.description).Error = "Requried Field";
            }
            return valid;
        }

        private void AddEditTask()
        {
            if (isUpdate)
            {
                this.taskService.UpdateTask(this.currentTask);
                Intent updateTask = new Intent(this, typeof(AddEditActivity));
                updateTask.PutExtra("type", JsonConvert.SerializeObject(Crud.Update));
                updateTask.PutExtra("task", JsonConvert.SerializeObject(this.currentTask));
                SetResult(Result.Ok, updateTask);
            }
            else
            {
                this.taskService.AddTask(this.currentTask);
                Intent newTask = new Intent(this, typeof(MainActivity));
                newTask.PutExtra("newtask", JsonConvert.SerializeObject(this.currentTask));
                SetResult(Result.Ok, newTask);
            }
            Finish();
        }

        private void SetDueDatePicker()
        {
            dueDate = FindViewById<EditText>(Resource.Id.due_date);
            dueDate.Click -= DueDate_Click;
            dueDate.Click += DueDate_Click;
            dueTime = FindViewById<EditText>(Resource.Id.due_time);
            dueTime.Click -= DueTimeClick;
            dueTime.Click += DueTimeClick;
        }

        private void DueTimeClick(object sender, EventArgs e)
        {
            int hour = 7, minutes = 0;
            if (this.isUpdate)
            {
                hour = this.currentTask.DueDate.Hour;
                minutes = this.currentTask.DueDate.Minute;
            }
            var timePickerDialog = new TimePickerDialog(this, this, hour, minutes, false);
            timePickerDialog.Show();
        }

        private void DueDate_Click(object sender, EventArgs e)
        {
            int year, month, day;
            if (this.isUpdate)
            {
                year = this.currentTask.DueDate.Year;
                month = this.currentTask.DueDate.Month -1;
                day = this.currentTask.DueDate.Day;
            }
            else
            {
                year = calendar.Get(CalendarField.Year);
                month = calendar.Get(CalendarField.Month);
                day = calendar.Get(CalendarField.DayOfMonth);
            }
            var datePickerDialog = new DatePickerDialog(this, this, year, month, day);
            datePickerDialog.Show();
        }

        private void SetStatusSpinner()
        {
            var spinner = FindViewById<Spinner>(Resource.Id.status);
            spinner.Adapter = new ArrayAdapter<string>(this, Resource.Layout.support_simple_spinner_dropdown_item, Enum.GetValues(typeof(Status)).Cast<Status>().Select(e => e.ToString()).ToArray());
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    this.OnBackPressed();
                    break;
                case Resource.Id.action_new:
                    this.AddTaskClick();
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }

        public void OnDateSet(DatePicker view, int year, int month, int dayOfMonth)
        {
            dueDate.Text = $"{dayOfMonth}-{month + 1}-{year}";
            calendar.Set(year, month, dayOfMonth);
        }

        public void OnTimeSet(TimePicker view, int hourOfDay, int minutes)
        {
            //var date = new DateTime(0, 0, 0, hourOfDay, minutes, 0);
            //dueTime.Text = date.ToString("hh:mm tt").ToUpper();

            var simpleDateFormat = new SimpleDateFormat("hh:mm a");
            var date = new Date(0, 0, 0, hourOfDay, minutes);
            dueTime.Text = simpleDateFormat.Format(date);
        }
    }
}