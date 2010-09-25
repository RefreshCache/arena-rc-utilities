using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Arena.Custom.RC.Utilities.UI
{
    /// <summary>
    /// The FileUploader class is a .NET front-end to the Javascript
    /// Uploadify plug-in. Provides a simple interface to generating
    /// pretty upload elements and progress feedback to the user.
    /// </summary>
    public class FileUploader : WebControl, INamingContainer
    {
        #region Properties

        private HiddenField fileField;
        private Button submitButton;
        private static readonly object EventUploadKey = new object();

        /// <summary>
        /// The title that is displayed on the button that the user will
        /// click on to perform file uploads. By default this is "Upload".
        /// </summary>
        [Category("Appearance"), DefaultValue("Upload"), Description("The title displayed in the flash button.")]
        public virtual string ButtonText
        {
            get
            {
                string s = (string)ViewState["ButtonText"];
                return (s == null ? String.Empty : s);
            }
            set
            {
                ViewState["ButtonText"] = value;
            }
        }

        /// <summary>
        /// The javascript function that will be executed before the postback
        /// occurs. Return false in the handler to stop the postback.
        /// </summary>
        /// <example>
        /// Specify "myVerify" and define the function with a single parameter, the GUID of the file.
        /// </example>
        [Description("The javascript function that will be executed before the postback occurs. Return false in the handler to stop the postback. e.g. specify \"myVerify\" and define the function with a single parameter, the GUID of the file.")]
        public virtual string OnClientUpload
        {
            get
            {
                string s = (string)ViewState["OnClientUpload"];
                return (s == null ? String.Empty : s);
            }
            set
            {
                ViewState["OnClientUpload"] = value;
            }
        }

        #endregion


        #region WebControl override methods.

        protected override void OnLoad(EventArgs e)
        {
            BasePage.AddJavascriptInclude(Page, BasePage.JQUERY_INCLUDE);
            BasePage.AddJavascriptInclude(Page, "Custom/RC/Utilities/FileUploader/jquery.uploadify.min.js");
            BasePage.AddJavascriptInclude(Page, "Custom/RC/Utilities/FileUploader/swfobject.js");
            BasePage.AddCssLink(Page, "Custom/RC/Utilities/FileUploader/uploadify.css");
        }


        protected override void CreateChildControls()
        {
            Controls.Clear();

            fileField = new HiddenField();
            fileField.ID = "file";
            Controls.Add(fileField);

            submitButton = new Button();
            submitButton.ID = "submit";
            submitButton.Style.Add("display", "none");
            submitButton.Click += new EventHandler(submitButton_Click);

            Controls.Add(submitButton);
        }


        protected override void Render(HtmlTextWriter writer)
        {
            String script, postback, clientScript = "";


            //
            // Emit the file upload tag.
            //
            writer.Write("<input type=\"file\" name=\"{0}\" id=\"{0}\" style=\"display: none;\" />", this.ClientID);

            //
            // Build the uploadify script.
            //
            postback = Page.ClientScript.GetPostBackEventReference(submitButton, "argument");
            if (!String.IsNullOrEmpty(OnClientUpload))
                clientScript = "            if (" + OnClientUpload + "(response) == false) { return true; }\n";
            script = "<script language=\"javascript\" type=\"text/javascript\">" +
                    "    $(document).ready(function() {\n" +
                    "    $('#" + this.ClientID + "').uploadify({\n" +
                    "        'uploader': 'Custom/RC/Utilities/FileUploader/uploadify.swf',\n" +
                    "        'script': 'Custom/RC/Utilities/FileUploader/FileUploader.ashx',\n" +
                    "        'buttonText': '" + (!String.IsNullOrEmpty(ButtonText) ? ButtonText : "Upload") + "',\n" +
                    "        'cancelImg': 'Custom/RC/Utilities/FileUploader/cancel.png',\n" +
                    "        'auto': true,\n" +
                    "        'onComplete': function(event, queueID, fileObj, response, data) {\n" +
                    clientScript +
                    "            $('#" + fileField.ClientID + "').attr('value', response);\n" +
                    "            $('#" + submitButton.ClientID + "').click();\n" +
                    "        }\n" +
                    "    });\n" +
                    "});" +
                    "</script>\n";

            //
            // Emit the script that will translate the file upload tag into an uploadify.
            //
            Page.RegisterStartupScript(this.ClientID, script);

            fileField.RenderControl(writer);
            submitButton.RenderControl(writer);
        }

        #endregion


        #region Events

        /// <summary>
        /// Defines an event handler for the FileUploader class.
        /// </summary>
        /// <param name="sender">The FileUploader object performing the upload.</param>
        /// <param name="e">The event arguments that contain the file uploaded.</param>
        public delegate void FileUploadEventHandler(object sender, FileUploadEventArgs e);

        /// <summary>
        /// The event that is raised when a file is uploaded.
        /// </summary>
        [Category("Action"), Description("Raised when the user uploads a file.")]
        public event FileUploadEventHandler Upload
        {
            add
            {
                base.Events.AddHandler(EventUploadKey, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventUploadKey, value);
            }
        }

        /// <summary>
        /// This method is called when a file is uploaded. It triggers the Upload event.
        /// </summary>
        /// <param name="e">The information about the file that was uploaded.</param>
        protected virtual void OnUpload(FileUploadEventArgs e)
        {
            FileUploadEventHandler handler = (FileUploadEventHandler)base.Events[EventUploadKey];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Called when the submit button is "clicked", it is used to simulate
        /// a postback that we can catch and then process the file upload.
        /// </summary>
        /// <param name="sender">The submit button clicked.</param>
        /// <param name="e">Ignored arguments from the button.</param>
        private void submitButton_Click(object sender, EventArgs e)
        {
            EnsureChildControls();
            OnUpload(new FileUploadEventArgs(new Guid(fileField.Value)));
        }

        #endregion
    }


    /// <summary>
    /// Event Arguments for the OnUpload event in the FileUploader class.
    /// </summary>
    public class FileUploadEventArgs : EventArgs
    {
        /// <summary>
        /// The Blob GUID that contains the information about the file that was
        /// uploaded.
        /// </summary>
        public Guid GUID;


        /// <summary>
        /// Create a new object with the specified GUID.
        /// </summary>
        /// <param name="guid">The Guid that identifies the uploaded file.</param>
        public FileUploadEventArgs(Guid guid)
        {
            this.GUID = guid;
        }
    }
}
