using System;
using System.Web;
using System.IO;
using Arena.Custom.RC.Utilities;


namespace Arena.Custom.RC.Utilities
{
    /// <summary>
    /// Provides the back-end code to process a request for the
    /// status of an AJAX long-running task.
    /// </summary>
    public class AjaxTaskHandler : IHttpHandler
    {
        /// <summary>
        /// Process the request for information about the AJAX task
        /// specified by the GUID.
        /// </summary>
        /// <param name="context">Context identifying the current web request.</param>
        public void ProcessRequest(HttpContext context)
        {
            AjaxTask task;

            task = new AjaxTask(new Guid(context.Request.QueryString["guid"].ToString()));

            context.Response.ContentType = "text/plain";
            context.Response.Write(
                "{ \"status\": \"" + task.Status.ToString() + "\"" +
                ", \"progress\": \"" + task.Progress.ToString() + "\"" +
                ", \"message\": \"" + task.Message.Replace("\"", "\\\"") + "\" }");
        }


        /// <summary>
        /// I don't think even Microsoft knows what this is for.
        /// </summary>
        public bool IsReusable
        {
            get { return false; }
        }
    }
}