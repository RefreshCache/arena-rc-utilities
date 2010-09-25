using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;

using Arena.Core;
using Arena.Portal;
using Arena.Security;

namespace Arena.Custom.RC.Utilities
{
    public partial class BooleanLogic : PortalControl
    {
        public PlaceHolder True { get { return TrueContent; } }
        public PlaceHolder False { get { return FalseContent; } }

        [TextSetting("SQL Query", "The SQL Query (or stored procedure) to run to determine if the true or the false content should be shown. This should return a single row single column result of 0 or 1.", true)]
        public String SQLQuerySetting { get { return Setting("SQLQuery", "", true); } }

        protected override void OnInit(EventArgs e)
        {
            ModuleInstanceCollection input = new ModuleInstanceCollection();

            input.LoadByParentModuleInstanceID(base.CurrentModule.ModuleInstanceID, (base.CurrentPerson != null) ? base.CurrentPerson.PersonID : -1);
            foreach (ModuleInstance instance in input)
            {
                if (instance.Permissions.Allowed(OperationType.View, base.CurrentUser))
                {
                    PropertyInfo property = base.GetType().GetProperty(instance.TemplateFrameName);
                    if (property == null)
                    {
                        throw new ApplicationException(string.Format("Could not find frame named '{0}' in {1}", instance.TemplateFrameName, base.GetType().Name));
                    }

                    Control control = (Control)property.GetValue(this, null);
                    if (control == null)
                    {
                        throw new ApplicationException(string.Format("Could not load frame named '{0}'", instance.TemplateFrameName));
                    }

                    PortalControl portalControl = this.LoadModule(instance);
                    if (portalControl != null)
                    {
                        control.Controls.Add(portalControl);
                    }
                }
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Boolean logic = false;
            String query = SQLQuerySetting;


            //
            // Do query string replacement.
            // Todo: Not until we know how to prevent SQL Injection.
            //
//            foreach (String k in Page.Request.QueryString.AllKeys)
//            {
//                query = query.ReplaceNonCaseSensitive(String.Format("##{0}##", k), Page.Request.QueryString[k]);
//            }
//            query = Regex.Replace(query, "##.+##", "");

            //
            // Do custom replacement.
            //
            query = query.ReplaceNonCaseSensitive("@@PersonID@@", (ArenaContext.Current.Person != null ? ArenaContext.Current.Person.PersonID.ToString() : "-1"));

            //
            // Execute the query. If it does not begin with SELECT then
            // execute it as a stored procedure.
            //
            if (query.IndexOf("SELECT", StringComparison.InvariantCultureIgnoreCase) == 0)
                logic = Convert.ToBoolean(new Arena.DataLayer.Organization.OrganizationData().ExecuteScalar(query.ToString()));
            else
                logic = Convert.ToBoolean(new Arena.DataLayer.Organization.OrganizationData().ExecuteScalar(query, new ArrayList()));

            TrueContent.Visible = logic;
            FalseContent.Visible = !logic;
        }
    }
}