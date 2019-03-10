using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using Android.Support.Design.Widget;
using TaskManager.Model;
using System.Collections.Generic;
using TaskManager.Service;
using System.Linq;
using Android.Views;
using Android.Content;
using TaskManager.Activities;
using TaskManager.Adaptors;
using System;
using Newtonsoft.Json;
using ListViewSwipeItem;
using Android.Support.V4.Widget;
using Android.Graphics;
using System.ComponentModel;
using System.Threading;

namespace TaskManager
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = false)]
    public class MainActivity : AppCompatActivity
    {
        public List<Task> RawTasks { get; set; }
        public ListView TaskListView { get; set; }
        private TaskService TaskService { get; set; }
        private TaskListAdaptor TaskListAdaptor { get; set; }
        private Status currentStatus { get; set; }
        private SearchView SearchView { get; set; }
        private bool isNotificationsEnabled { get; set; }
        private PendingIntent pendingIntent;
        private TextView noTasksFound;
        GestureDetector gestureDetector;
        GestureListener gestureListener;
        private SwipeRefreshLayout refreshLayout;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);
            this.TaskService = new TaskService();
            this.RawTasks = TaskService.GetAll();
            this.TaskListView = FindViewById<ListView>(Resource.Id.mainlistview);
            SetToolbar();
            InitializeClickEvents();
            TaskListAdaptor = new TaskListAdaptor(this, this.RawTasks.Where(s => s.Status == Status.New).ToList());
            TaskListView.Adapter = TaskListAdaptor;
            CreateNotificationChannel();
            noTasksFound = FindViewById<TextView>(Resource.Id.no_tasks_found);
            CheckNoTasksFound();
            this.isNotificationsEnabled = Intent.GetStringExtra("IsNotificationsEnabled") == "True";
            SetRefresh();
            //gestureListener = new GestureListener();
            //gestureDetector = new GestureDetector(this,gestureListener);
        }

        private void SetRefresh()
        {
            refreshLayout = FindViewById<SwipeRefreshLayout>(Resource.Id.refreshOnSwipe);
            refreshLayout.SetColorSchemeColors(Color.LightSkyBlue);
            refreshLayout.Refresh += RefreshLayoutRefresh;
        }

        private void RefreshLayoutRefresh(object sender, EventArgs e)
        {
            BackgroundWorker work = new BackgroundWorker();
            work.DoWork += WorkDoWork;
            work.RunWorkerCompleted += WorkRunWorkerCompleted;
            work.RunWorkerAsync();
        }

        private void WorkRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.TaskListAdaptor.Tasks = TaskService.GetAll().Where(s => s.Status == currentStatus).ToList();
            refreshLayout.Refreshing = false;
        }

        private void WorkDoWork(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(1000);
        }

        private void GestureListenerRightEvent()
        {
            Toast.MakeText(this, "Gesture Right", ToastLength.Short).Show();
        }

        private void CreateNotificationChannel()
        {
            var channel = new NotificationChannel("Task", "TaskNotification", NotificationImportance.Default);
            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.CreateNotificationChannel(channel);
            if (isNotificationsEnabled)
                StartAlarm();
        }

        private void StartAlarm()
        {
            var alarmManager = (AlarmManager)GetSystemService(Context.AlarmService);
            alarmManager.SetRepeating(AlarmType.RtcWakeup, SystemClock.ElapsedRealtime(), 2 * 60 * 1000, GetPendingIntent());
        }

        private PendingIntent GetPendingIntent()
        {
            var intent = new Intent(this, typeof(AlarmReceiver));
            var task = RawTasks.Where(t => t.Status == Status.New || t.Status == Status.InProgress).FirstOrDefault();
            intent.PutExtra("title", task.Name);
            intent.PutExtra("messsage", SetNotificationMessage(task.DueDate));
            pendingIntent = PendingIntent.GetBroadcast(this, 0, intent, 0);
            return pendingIntent;
        }

        private string SetNotificationMessage(DateTime dueDate)
        {
            if (dueDate.Date.Date < DateTime.Now.Date)
                return $"Overdue by - {dueDate.ToString("dddd, dd MMMM yyyy")}";
            else if (dueDate.Date.Date.ToShortDateString() == DateTime.Now.Date.ToShortDateString())
                return $"Due today @{dueDate.ToString("hh:mm tt")}";
            else if (dueDate.Date.Date < DateTime.Now.AddDays(2).Date)
                return $"Due tomorrow @{dueDate.ToString("hh:mm tt")}";
            else if (dueDate.Date.Date < DateTime.Now.AddDays(3).Date)
                return $"Completed by {dueDate.ToString("dddd, hh:mm tt")}";
            return string.Empty;
        }

        private void SetToolbar()
        {
            var toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
        }

        private void InitializeClickEvents()
        {
            TaskListView.ItemClick -= TaskList_ItemClick;
            TaskListView.ItemClick += TaskList_ItemClick;
            TaskListView.ItemLongClick += TaskListViewItemLongClick;
            var floatingActionButton = FindViewById<FloatingActionButton>(Resource.Id.floating_add_button);
            floatingActionButton.Click += FloatingActionButton_Click;
            var bottomNavigation = FindViewById<BottomNavigationView>(Resource.Id.bottom_navigation);
            bottomNavigation.NavigationItemSelected += BottomNavigation_NavigationItemSelected;
            SearchView = FindViewById<SearchView>(Resource.Id.searchView1);
            SearchView.QueryTextChange += SearchQueryTextChange;
        }

        bool isListClick;

        private void TaskListViewItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e)
        {
            isListClick = true;
            SetToolbar();
        }

        private void SearchQueryTextChange(object sender, SearchView.QueryTextChangeEventArgs e)
        {
            this.TaskListAdaptor.NotifyDataSetChanged();
            TaskListAdaptor.Filter.InvokeFilter(e.NewText);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.main_activity_actions_menu, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        private void TaskList_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            var taskDetails = new Intent(this, typeof(DetailsActivity));
            taskDetails.PutExtra("taskDetails", JsonConvert.SerializeObject(this.TaskListAdaptor.Tasks[e.Position]));
            this.StartActivityForResult(taskDetails, 2);
        }

        private void FloatingActionButton_Click(object sender, EventArgs e)
        {
            var intent = new Intent(this, typeof(AddEditActivity));
            this.StartActivityForResult(intent, 1);
        }

        private void BottomNavigation_NavigationItemSelected(object sender, BottomNavigationView.NavigationItemSelectedEventArgs e)
        {
            LoadSelectedTasks(e.Item.ItemId);
            SearchView.SetQuery("", false);
            this.TaskListAdaptor.NotifyDataSetChanged();
            CheckNoTasksFound();
        }

        private void LoadSelectedTasks(int id)
        {
            this.TaskListAdaptor.Tasks.Clear();
            switch (id)
            {
                case Resource.Id.action_new:
                    currentStatus = Status.New;
                    this.TaskListAdaptor.Tasks.AddRange(this.RawTasks.Where(s => s.Status == Status.New));
                    break;
                case Resource.Id.action_in_progress:
                    currentStatus = Status.InProgress;
                    this.TaskListAdaptor.Tasks.AddRange(this.RawTasks.Where(s => s.Status == Status.InProgress));
                    break;
                case Resource.Id.action_completed:
                    currentStatus = Status.Completed;
                    this.TaskListAdaptor.Tasks.AddRange(this.RawTasks.Where(s => s.Status == Status.Completed));
                    break;
            }
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.Settings:
                    var settings = new Intent(this, typeof(SettingsActivity));
                    settings.PutExtra("IsNotificationsEnabled", isNotificationsEnabled.ToString());
                    this.StartActivityForResult(settings, 3);
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            if (resultCode != Result.Canceled)
            {
                switch (requestCode)
                {
                    case 1:
                        var newTask = JsonConvert.DeserializeObject<Task>(data.GetStringExtra("newtask")) ?? null;
                        if (newTask != null)
                        {
                            this.RawTasks.Add(TaskService.GetLast());
                            if (newTask.Status == currentStatus)
                                this.TaskListAdaptor.Tasks.Add(TaskService.GetLast());
                        }
                        break;
                    case 2:
                        var task = JsonConvert.DeserializeObject<Task>(data.GetStringExtra("task")) ?? null;
                        if (task != null)
                        {
                            switch (JsonConvert.DeserializeObject<Crud>(data.GetStringExtra("type")))
                            {
                                case Crud.Update:
                                    var updateRawData = this.RawTasks.FirstOrDefault(s => s.Id == task.Id);
                                    var updateTask = this.TaskListAdaptor.Tasks.FirstOrDefault(s => s.Id == task.Id);
                                    if (updateRawData.Status == task.Status)
                                        UpdateTaskData(task, updateTask);
                                    else
                                        this.TaskListAdaptor.Tasks.Remove(this.TaskListAdaptor.Tasks.FirstOrDefault(t => t.Id == task.Id));
                                    UpdateTaskData(task, updateRawData);
                                    break;
                                case Crud.Delete:
                                    this.RawTasks.Remove(RawTasks.FirstOrDefault(t => t.Id == task.Id));
                                    this.TaskListAdaptor.Tasks.Remove(this.TaskListAdaptor.Tasks.FirstOrDefault(t => t.Id == task.Id));
                                    break;
                            }
                        }
                        break;
                    case 3:
                        if (data != null)
                        {
                            var enable = data.GetStringExtra("IsNotificationsEnabled") ?? null;
                            if (enable != null)
                            {
                                isNotificationsEnabled = enable == "True";
                                if (isNotificationsEnabled)
                                    StartAlarm();
                            }
                        }
                        break;
                }
            }
            CheckNoTasksFound();
            this.TaskListAdaptor.NotifyDataSetChanged();
            base.OnActivityResult(requestCode, resultCode, data);
        }

        private void CheckNoTasksFound()
        {
            noTasksFound.Visibility = this.TaskListAdaptor.Tasks.Count > 0 ? ViewStates.Gone : ViewStates.Visible;
        }

        private void UpdateTaskData(Task source, Task destination)
        {
            destination.Name = source.Name;
            destination.Description = source.Description;
            destination.DueDate = source.DueDate;
            destination.Status = source.Status;
            destination.Priority = source.Priority;
        }
    }
}