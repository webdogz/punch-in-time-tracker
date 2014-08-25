using PunchIn.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SP = Microsoft.SharePoint.Client;

namespace PunchIn.Services
{
    internal class SharePointService : ViewModels.ViewModelBase
    {
        #region Fields
        Uri siteUri;
        string siteUrl;
        string listTitle;
        SP.ClientContext clientContext;
        SP.Web clientWeb;
        #endregion
        public SharePointService()
        {
            // the actual url: http://hin-tfsportal/my/personal/{USERNAME}/Lists/{LIST_NAME}/AllItems.aspx
            siteUri = Properties.Settings.Default.SharePointSiteUri;
            listTitle = Properties.Settings.Default.SharePointListName;
            clientContext = new SP.ClientContext(siteUri.AbsoluteUri);
            
            clientWeb = clientContext.Web;
            // TODO: Add reference to Microsoft.Office.Server.UserProfile and get users personal site programmatically
            //var profileManager = new UserProfileManager(ServerContext.GetContext(SP.SPContext.Current.Site));
            //var profile = profileManager.GetUserProfile(string.Format("{0}\\{1}", Environment.UserDomainName, Environment.UserName));
            //using (SP.SPSite personalSite = profile.PersonalSite)
            //{
            //    var personalSiteUrl = personalSite.PersonalUrl;
            //}
            
        }

        // for viewmodels using this service. 
        // TODO: Subscribe to notify changed or should we be good little proggers and throw our exceptions
        private string errors;
        public string Errors
        {
            get { return this.errors; }
            set
            {
                if (this.errors != value)
                {
                    this.errors = value;
                    OnPropertyChanged("Errors");
                }
            }
        }
        void SetErrorsMessage(Exception e)
        {
            Errors = e.Message;
        }

        /// <summary>
        /// Create the Time Trackin SharePoint list
        /// </summary>
        private void AddTimeTrackList()
        {
            clientContext.Load(clientWeb, w => w.Title, w => w.Lists);
            clientContext.ExecuteQuery();
            SP.List list = clientWeb.Lists.FirstOrDefault(l => l.Title == listTitle);

            if (list == null)
            {
                try
                {
                    SP.ListCreationInformation listInfo = new SP.ListCreationInformation();
                    listInfo.Title = listTitle;
                    listInfo.Description = "PunchIn Time Tracker used to track my time";
                    listInfo.TemplateType = (int)SP.ListTemplateType.GenericList;
                    list = clientWeb.Lists.Add(listInfo);

                    //TODO: Don't think we care about this...
                    //SP.FieldGuid guidField = clientContext.CastTo<SP.FieldGuid>(list.Fields.Add(SP.FieldType.Guid, "ItemGuid", false));
                    //guidField.Indexed = true;
                    //guidField.Required = true;
                    //guidField.SetShowInDisplayForm(false);
                    //guidField.SetShowInEditForm(false);
                    //guidField.SetShowInNewForm(false);
                    SP.FieldNumber idField = clientContext.CastTo<SP.FieldNumber>(list.Fields.Add(SP.FieldType.Integer, "WorkItemId", true));
                    idField.Description = "TFS WorkItem Id";
                    idField.Update();
                    SP.FieldNumber scField = clientContext.CastTo<SP.FieldNumber>(list.Fields.Add(SP.FieldType.Integer, "ServiceCall", true));
                    scField.Description = "HPOV Service Call Number";
                    scField.MinimumValue = 0;
                    scField.Update();
                    SP.FieldNumber chField = clientContext.CastTo<SP.FieldNumber>(list.Fields.Add(SP.FieldType.Integer, "Change", true));
                    chField.Description = "HPOV Change Number";
                    chField.MinimumValue = 0;
                    chField.Update();
                    SP.FieldText titleField = clientContext.CastTo<SP.FieldText>(list.Fields.Add(SP.FieldType.Text, "Title", true));
                    titleField.Description = "The work items title";
                    titleField.Update();
                    SP.FieldNumber hrsDoneField = clientContext.CastTo<SP.FieldNumber>(list.Fields.Add(SP.FieldType.Number, "HoursCompleted", true));
                    hrsDoneField.Description = "Hours done on this task";
                    hrsDoneField.MinimumValue = 0;
                    hrsDoneField.Update();
                    SP.FieldNumber hrsRemainField = clientContext.CastTo<SP.FieldNumber>(list.Fields.Add(SP.FieldType.Number, "HoursRemaining", true));
                    hrsRemainField.Description = "Hours remaining on the work item";
                    hrsRemainField.MinimumValue = 0;
                    hrsRemainField.Update();
                    SP.FieldText stateField = clientContext.CastTo<SP.FieldText>(list.Fields.Add(SP.FieldType.Text, "State", true));
                    stateField.Description = "The current state of the work item";
                    stateField.Update();
                    SP.FieldText statusField = clientContext.CastTo<SP.FieldText>(list.Fields.Add(SP.FieldType.Text, "Status", true));
                    statusField.Description = "The current status of the work item";
                    stateField.Update();
                    SP.FieldText wtField = clientContext.CastTo<SP.FieldText>(list.Fields.Add(SP.FieldType.Text, "WorkType", true));
                    wtField.Description = "The type work being done for the current task";
                    wtField.Update();
                    SP.FieldDateTime weekEndingField = clientContext.CastTo<SP.FieldDateTime>(list.Fields.Add(SP.FieldType.DateTime, "WeekEnding", true));
                    weekEndingField.Description = "Date of the Friday or last day of the working week";
                    weekEndingField.DisplayFormat = SP.DateTimeFieldFormatType.DateOnly;
                    weekEndingField.Update();
                    // tell the server and cross our fingers
                    clientContext.ExecuteQuery();
                }
                catch(Exception ex)
                {
                    SetErrorsMessage(ex);
                }
            }
        }

        private void ExportAndSyncTimes()
        {

        }

        public IEnumerable<SPExportItem> GetListItems()
        {
            SP.List trackInfo = clientContext.Web.Lists.GetByTitle(listTitle);
            SP.CamlQuery query = new SP.CamlQuery();
            SP.ListItemCollection listData = trackInfo.GetItems(query);
            var queryResults = clientContext.LoadQuery(listData);
            clientContext.ExecuteQuery();

            return queryResults.Select(f => new SPExportItem
                {
                    ItemGuid = Guid.Parse(f.FieldValues["ItemGuid"].ToString()),
                    WorkItemId = (int)f.FieldValues["WorkItemId"],
                    ServiceCall = (int)f.FieldValues["ServiceCall"],
                    Change = (int)f.FieldValues["Change"],
                    Title = f.FieldValues["Title"].ToString(),
                    HoursCompleted = (double)f.FieldValues["HoursCompleted"],
                    HoursRemaining = (double)f.FieldValues["HoursRemaining"],
                    State = f.FieldValues["State"].ToString(),
                    Status = f.FieldValues["Status"].ToString(),
                    WorkType = f.FieldValues["WorkType"].ToString()
                });
        }

        private void AddItemToList()
        {
            SP.List listInfo = clientContext.Web.Lists.GetByTitle(listTitle);
            clientContext.Load(clientWeb);
            clientContext.Load(listInfo);
            clientContext.ExecuteQuery();

            SP.ListItemCreationInformation createInfo = new SP.ListItemCreationInformation();
            SP.ListItem newItem = listInfo.AddItem(createInfo);
            newItem["Title"] = "My new item created from client";
            newItem["Effort"] = 1.5D;
            newItem["Date_x0020_Started"] = DateTime.Now;
            newItem.Update();
            clientContext.ExecuteQuery();
            Console.WriteLine("Added new item: {0}", newItem);
        }
    }
    /// <summary>
    /// Extension class for creating sharepoint fields. Helps with the creation of SPFields when creating a new SPList
    /// </summary>
    public static class Extensions
    {
        public static SP.Field Add(this SP.FieldCollection fields, SP.FieldType fieldType, String displayName, bool addToDefaultView)
        {
            return fields.AddFieldAsXml(String.Format("<Field DisplayName='{0}' Type='{1}' />", displayName, fieldType), addToDefaultView, SP.AddFieldOptions.DefaultValue);
        }

        public static SP.Field Add(this SP.FieldCollection fields, SP.FieldType fieldType, String displayName, String[] choices, bool addToDefaultView)
        {
            return fields.AddFieldAsXml(
                String.Format("<Field DisplayName='{0}' Type='{1}'><CHOICES>{2}</CHOICES></Field>",
                    displayName,
                    fieldType,
                    String.Concat(Array.ConvertAll<String, String>(choices, choice => String.Format("<CHOICE>{0}</CHOICE>", choice)))),
                addToDefaultView, SP.AddFieldOptions.DefaultValue);
        }
    }
}
