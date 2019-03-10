using System;
using System.Collections.Generic;
using System.IO;
using SQLite;
using TaskManager.Model;
using Environment = System.Environment;

namespace TaskManager.Service
{
    public class TaskService
    {
        private string databaseFileName;
        private List<Task> tasks;
        private Task task;

        public TaskService()
        {
            string applicationFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "TaskManager");
            Directory.CreateDirectory(applicationFolderPath);
            databaseFileName = Path.Combine(applicationFolderPath, "TaskManager.db");
            var db = new SQLiteConnection(databaseFileName);
            db.CreateTable<Task>();
        }

        public void AddTask(Task task)
        {
            PerformCRUD(task, Crud.Add);
        }

        public void UpdateTask(Task task)
        {
            PerformCRUD(task, Crud.Update);
        }

        public void DeleteTask(Task task)
        {
            PerformCRUD(task, Crud.Delete);
        }

        public List<Task> GetAll()
        {
            PerformCRUD(task, Crud.GetAll);
            return this.tasks;
        }

        public Task Get(Task task)
        {
            PerformCRUD(task, Crud.Get);
            return this.task;
        }

        public void DeleteAll()
        {
            PerformCRUD(task, Crud.DeleteAll);
        }

        public Task GetLast()
        {
            using (var db = new SQLiteConnection(databaseFileName))
            {
                return db.Table<Task>().OrderByDescending(s => s.Id).FirstOrDefault();
            }
        }

        private void PerformCRUD(Task task, Crud crud)
        {
            try
            {
                using (var db = new SQLiteConnection(databaseFileName))
                {
                    switch (crud)
                    {
                        case Crud.Add:
                            db.Insert(task);
                            break;
                        case Crud.Delete:
                            db.Execute("delete from Task where Id = ?", task.Id);
                            break;
                        case Crud.Update:
                            db.Update(task);
                            break;
                        case Crud.Get:
                            this.task = db.Get<Task>(task.Id);
                            break;
                        case Crud.GetAll:
                            this.tasks = new List<Task>(db.Table<Task>());
                            break;
                        case Crud.DeleteAll:
                            db.Table<Task>().Delete();
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception e)
            {

            }
        }
    }
}