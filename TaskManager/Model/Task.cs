using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using SQLite;

namespace TaskManager.Model
{
    public class Task
    {
        [AutoIncrement]
        [PrimaryKey]
        public int Id { get; set; }

        [MaxLength(120)]
        public string Name { get; set; }

        [MaxLength(242)]
        public string Description { get; set; }

        public Status Status { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime DueDate { get; set; }

        public Nullable<DateTime> CompletedDate { get; set; }

        public Priority Priority { get; set; }

        public override string ToString()
        {
            return Name.ToString();
        }
    }

    public enum Status
    {
        New,
        InProgress,
        Completed
    }

    public enum Priority
    {
        Low,
        Medium,
        High
    }

    public enum Crud
    {
        Add,
        Update,
        DeleteAll,
        Delete,
        Get,
        GetAll,
    }
}