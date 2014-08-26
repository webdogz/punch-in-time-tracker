using PunchIn.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SP = Microsoft.SharePoint.Client;
using PunchIn.Extensions;

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
                    SP.FieldGuid guidField = clientContext.CastTo<SP.FieldGuid>(list.Fields.Add(SP.FieldType.Guid, "TimeEntryGuid", false));
                    guidField.Description = "Guid of the TimeEntry item used for syncing purposes";
                    guidField.Indexed = true;
                    guidField.Required = true;
                    guidField.SetShowInDisplayForm(false);
                    guidField.SetShowInEditForm(false);
                    guidField.SetShowInNewForm(false);
                    guidField.Update();
                    SP.FieldNumber idField = clientContext.CastTo<SP.FieldNumber>(list.Fields.Add(SP.FieldType.Integer, "TfsId", true));
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
                    SP.FieldText commentField = clientContext.CastTo<SP.FieldText>(list.Fields.Add(SP.FieldType.Text, "Description", true));
                    commentField.Description = "The time entry's description";
                    commentField.Update();
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
                    SP.FieldDateTime weekStartingField = clientContext.CastTo<SP.FieldDateTime>(list.Fields.Add(SP.FieldType.DateTime, "WeekStarting", true));
                    weekStartingField.Description = "Date of the Monday or first day of the working week";
                    weekStartingField.DisplayFormat = SP.DateTimeFieldFormatType.DateOnly;
                    weekStartingField.Update();
                    SP.FieldNumber woyField = clientContext.CastTo<SP.FieldNumber>(list.Fields.Add(SP.FieldType.Integer, "WeekOfYear", true));
                    woyField.Description = "Week of year";
                    woyField.Update();
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

        public async Task<IEnumerable<SPExportItem>> GetListItems()
        {
            IEnumerable<SPExportItem> list = new List<SPExportItem>();

            await Task.Run(() =>
            {
                try
                {
                    SP.List trackInfo = clientContext.Web.Lists.GetByTitle(listTitle);
                    SP.CamlQuery query = new SP.CamlQuery();
                    SP.ListItemCollection listData = trackInfo.GetItems(query);
                    var queryResults = clientContext.LoadQuery(listData);
                    clientContext.ExecuteQuery();
                    list = queryResults.Select(f => new SPExportItem
                    {
                        TimeEntryGuid = Guid.Parse(f.FieldValues["TimeEntryGuid"].ToString()),
                        TfsId = (int)f.FieldValues["TfsId"],
                        ServiceCall = (int)f.FieldValues["ServiceCall"],
                        Change = (int)f.FieldValues["Change"],
                        Title = f.FieldValues["Title"].ToString(),
                        Description = f.FieldValues["Description"].ToString(),
                        HoursCompleted = (double)f.FieldValues["HoursCompleted"],
                        HoursRemaining = (double)f.FieldValues["HoursRemaining"],
                        State = f.FieldValues["State"].ToString(),
                        Status = f.FieldValues["Status"].ToString(),
                        WorkType = f.FieldValues["WorkType"].ToString(),
                        WeekStarting = (DateTime)f.FieldValues["WeekStarting"],
                        WeekOfYear = (int)f.FieldValues["WeekOfYear"]
                    });
                }
                catch (System.Net.WebException webEx)
                {
                    SetErrorsMessage(webEx);
                }
                catch (Exception ex)
                {
                    SetErrorsMessage(ex);
                }
            });

            return list;
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
}
