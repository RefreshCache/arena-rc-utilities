using System;
using System.Web;
using System.IO;
using Arena.Core;
using Arena.Utility;

namespace Arena.Custom.RC.Utilities
{
    /// <summary>
    /// Provides the back-end code to the FileUploader system. This handler
    /// stores the file data into the util_blob table and then returns the
    /// generated GUID of the blob to the caller.
    /// </summary>
    public class FileUploadHandler : IHttpHandler
    {
        /// <summary>
        /// Process the web request.
        /// </summary>
        /// <param name="context">The HttpContext identifying this connection.</param>
        public void ProcessRequest(HttpContext context)
        {
            ArenaDataBlob blob;
            BinaryReader rdr;
            HttpPostedFile file = context.Request.Files["Filedata"];


            //
            // Create the Arena Blob to store the uploaded file.
            //
            blob = new ArenaDataBlob();
            rdr = new BinaryReader(file.InputStream);
            blob.ByteArray = rdr.ReadBytes(file.ContentLength);
            blob.OriginalFileName = file.FileName;
            if (file.FileName.LastIndexOf('.') != -1)
                blob.FileExtension = file.FileName.Substring(file.FileName.LastIndexOf('.') + 1);
            blob.MimeType = "rcfile/uploader";
            blob.Save("RC:FileUploader");

            //
            // Cleanup stale files over 8 hours old.
            //
            new Arena.DataLayer.Organization.OrganizationData().ExecuteNonQuery(
                "DELETE FROM [util_blob] WHERE [mime_type] = 'rcfile/uploader' AND [date_modified] < DATEADD(hh, -8, GETDATE())");

            //
            // Send the response to the caller.
            //
            context.Response.ContentType = "text/plain";
            context.Response.Write(blob.GUID.ToString());
        }


        /// <summary>
        /// Couldn't tell you why this is needed. Don't reuse us, whatever
        /// that means.
        /// </summary>
        public bool IsReusable
        {
            get { return false; }
        }
    }
}
