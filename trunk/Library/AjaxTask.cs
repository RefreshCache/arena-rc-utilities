using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using Arena.Core;
using Arena.DataLayer.Organization;

namespace Arena.Custom.RC.Utilities
{
    /// <summary>
    /// The AjaxTask class provides a way to give feedback to the user
    /// about a long-running task. It is meant to be used with web
    /// threads that continue to run after the page has finished rendering.
    /// There is a web interface that can be used by client web pages
    /// to perform ajax queries on the status of a task.
    /// </summary>
    public class AjaxTask
    {
        #region Properties

        /// <summary>
        /// The ID number of this task in the database.
        /// </summary>
        public Int32 TaskID { get { return _TaskID; } }
        private Int32 _TaskID;

        /// <summary>
        /// A unique identifier for this task.
        /// </summary>
        public Guid GUID { get; set; }

        /// <summary>
        /// The current status of the task, such as Running or Success.
        /// </summary>
        public AjaxTaskStatus Status { get; set; }

        /// <summary>
        /// A textual message that can be displayed to the user to let them
        /// know what is currently going on with the task.
        /// </summary>
        public String Message { get; set; }

        /// <summary>
        /// The progress of the task as a percentage. The value can range
        /// between 0.0 and 100.0.
        /// </summary>
        public double Progress { get; set; }

        #endregion


        #region Constructors

        /// <summary>
        /// Initialize all the basic properties of any task.
        /// </summary>
        private void InitBasic()
        {
            _TaskID = -1;
            GUID = Guid.NewGuid();
            Status = AjaxTaskStatus.Running;
            Message = "New Task";
            Progress = 0.0f;
        }


        /// <summary>
        /// Populate the internal properties with the information from
        /// the SqlDataReader.
        /// </summary>
        /// <param name="rdr">The reader that contains the SQL row informations.</param>
        private void InitReader(SqlDataReader rdr)
        {
            _TaskID = (int)rdr["task_id"];
            GUID = (Guid)rdr["guid"];
            Status = (AjaxTaskStatus)rdr["status"];
            Progress = Convert.ToDouble(rdr["progress"]);
            Message = rdr["message"].ToString();
        }


        /// <summary>
        /// Create a new AjaxTask that can be saved to create a new entry.
        /// </summary>
        public AjaxTask()
        {
            InitBasic();
        }


        /// <summary>
        /// Load an AjaxTask from the database identified by the specified
        /// task ID number.
        /// </summary>
        /// <param name="task">The ID number of the task to load.</param>
        public AjaxTask(Int32 task)
        {
            ArrayList parms = new ArrayList();
            SqlDataReader rdr;


            InitBasic();

            //
            // Run the stored procedure to load the task.
            //
            parms.Add(new SqlParameter("@TaskID", task));
            rdr = new OrganizationData().ExecuteReader("cust_rc_util_sp_getAjaxTaskByID", parms);
            if (rdr.Read())
            {
                InitReader(rdr);
            }
            rdr.Close();
        }


        /// <summary>
        /// Instantiate a new AjaxTask object from the database.
        /// </summary>
        /// <param name="guid">The GUID to use when loading from the database.</param>
        public AjaxTask(Guid guid)
        {
            ArrayList parms = new ArrayList();
            SqlDataReader rdr;


            InitBasic();

            //
            // Run the stored procedure to load the task.
            //
            parms.Add(new SqlParameter("@Guid", guid));
            rdr = new OrganizationData().ExecuteReader("cust_rc_util_sp_getAjaxTaskByGUID", parms);
            if (rdr.Read())
            {
                InitReader(rdr);
            }
            rdr.Close();
        }

        #endregion


        /// <summary>
        /// Save the Ajax Task into the database. If this is a new task then
        /// the TaskID is set to the new ID number generated for this record
        /// before this method returns.
        /// </summary>
        /// <returns>The ID number of this ajax task.</returns>
        public Int32 Save()
        {
            ArrayList parms = new ArrayList();
            SqlParameter ID = new SqlParameter(@"ID", System.Data.SqlDbType.Int);


            parms.Add(new SqlParameter("@TaskID", _TaskID));
            parms.Add(new SqlParameter("@Guid", GUID));
            parms.Add(new SqlParameter("@Status", (int)Status));
            parms.Add(new SqlParameter("@Progress", Progress));
            parms.Add(new SqlParameter("@Message", Message));
            ID.Direction = System.Data.ParameterDirection.Output;
            parms.Add(ID);
            new OrganizationData().ExecuteNonQuery("cust_rc_util_sp_saveAjaxTask", parms);

            if (_TaskID == -1)
                _TaskID = (int)ID.Value;

            return _TaskID;
        }


        /// <summary>
        /// Prune the database by removing any tasks that are more than
        /// 8 hours old.
        /// </summary>
        static public void Prune()
        {
            ArrayList parms = new ArrayList();


            parms.Add(new SqlParameter("@Age", 8));
            new OrganizationData().ExecuteNonQuery("cust_rc_util_sp_pruneAjaxTasks", new ArrayList());
        }


        /// <summary>
        /// Begins a new task in it's own thread. The target should be a static function
        /// that takes a single argument of type Object (which is a Guid object that
        /// identifies the AjaxTask object).
        /// </summary>
        /// <param name="target">The function to execute in a new thread.</param>
        /// <returns>The Guid that identifies the task being executed.</returns>
        /// <example>
        /// static void Worker(Object guid)
        /// {
        ///     // Do work
        /// }
        /// 
        /// AjaxTask.BeginTask(MyClass.Worker);
        /// </example>
        static public Guid BeginTask(WaitCallback target)
        {
            AjaxTask task;


            task = new AjaxTask();
            task.Save();
            ThreadPool.QueueUserWorkItem(target, task.GUID);

            return task.GUID;
        }
    }


    /// <summary>
    /// The status of an Ajax Task.
    /// </summary>
    public enum AjaxTaskStatus
    {
        /// <summary>
        /// The task is still running, keep checking the status.
        /// </summary>
        Running = 0,

        /// <summary>
        /// The task has finished successfully.
        /// </summary>
        Success = 1,

        /// <summary>
        /// The task has finished with an error and did not complete successfully.
        /// </summary>
        Failed = 2
    }
}
