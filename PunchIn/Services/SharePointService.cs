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
    internal class SharePointService
    {
        #region Fields
        private Uri siteUri;
        private string listTitle;
        private SP.ClientContext clientContext;
        private SP.Web clientWeb;
        #endregion

        #region ctor
        public SharePointService()
        {
            // the actual url: http://hin-tfsportal/my/personal/{USERNAME}/Lists/{LIST_NAME}/AllItems.aspx
            siteUri = Properties.Settings.Default.SharePointSiteUri;
            listTitle = Properties.Settings.Default.SharePointListName;
            clientContext = new SP.ClientContext(siteUri.AbsoluteUri);
            clientWeb = clientContext.Web;
        }
        #endregion

        #region Methods
        #region > Get SharePoint ListItems
        /// <summary>
        /// Gets a list of <see cref="SPExportItem"/>'s asyncronously
        /// </summary>
        /// <returns>List of <see cref="SPExportItem"/>'s</returns>
        internal async Task<IEnumerable<SPExportItem>> GetListItems()
        {
            IEnumerable<SPExportItem> list = new List<SPExportItem>();
            string help = string.Empty;
            try
            {
                await Task.Run(() =>
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
                });
            }
            catch (System.Net.WebException webEx)
            {
                throw new System.Net.WebException(
                    string.Format("An error occurred while connecting to SharePoint: {0}", webEx.Message));
            }
            catch (ArgumentException aex)
            {
                if (string.IsNullOrWhiteSpace(listTitle))
                    help = "Try going to the settings page and enter a name for the SharePoint List.";
                throw new ArgumentException(
                    string.Format("An error occurred while retrieving the SharePoint list: {0}\n{1}", aex.Message, help));
            }
            catch (SP.ServerException sex)
            {
                if (sex.HResult.Equals(-2146233088))
                {
                    throw new EntryPointNotFoundException(sex.Message);
                }
                throw new Exception(sex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format("An unexpected error occurred while trying to retrieve SharePoint list: {0}", ex.Message));
            }

            return list;
        }
        #endregion // Get SharePoint ListItems

        #region > Create SharePoint List
        /// <summary>
        /// Create the Time Trackin SharePoint list
        /// </summary>
        internal async void CreateTimeTrackList()
        {
            try
            {
                await Task.Run(() =>
                    {
                        clientContext.Load(clientWeb, w => w.Title, w => w.Lists);
                        clientContext.ExecuteQuery();
                        SP.List list = clientWeb.Lists.FirstOrDefault(l => l.Title == listTitle);

                        if (list == null)
                        {
                            SP.ListCreationInformation listInfo = new SP.ListCreationInformation();
                            listInfo.Title = listTitle;
                            listInfo.Description = "PunchIn Time Tracker used to track my time";
                            listInfo.TemplateType = (int)SP.ListTemplateType.GenericList;
                            list = clientWeb.Lists.Add(listInfo);

                            //TODO: Don't think we need to care about this...but will keep just in case we need to sync data
                            SP.FieldGuid guidField = clientContext.CastTo<SP.FieldGuid>(list.Fields.Add(SP.FieldType.Guid, "TimeEntryGuid", false));
                            guidField.Description = "Guid of the TimeEntry item used for syncing purposes";
                            //guidField.Indexed = true;
                            //guidField.Required = true;
                            //guidField.SetShowInDisplayForm(false);
                            //guidField.SetShowInEditForm(false);
                            //guidField.SetShowInNewForm(true);
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

                            // add description to the default Title field
                            SP.FieldText titleField = clientContext.CastTo<SP.FieldText>(list.Fields.GetByInternalNameOrTitle("Title"));
                            titleField.Description = "The work items title";
                            titleField.Update();

                            clientContext.ExecuteQuery();
                        }
                    });
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format("An error occurred while trying to add a new SharePoint list: {0}",
                        ex.Message));
            }
        }
        #endregion //Create SharePoint List

        #region > Export Items to SharePoint List
        internal async void ExportCollectionToSharePointList(IList<SPExportItem> collection)
        {
            try
            {
                await Task.Run(() =>
                {
                    SP.List trackInfo = clientContext.Web.Lists.GetByTitle(listTitle);
                    clientContext.Load(clientWeb);
                    clientContext.Load(trackInfo);
                    clientContext.ExecuteQuery();

                    foreach (SPExportItem item in collection)
                    {
                        SP.ListItemCreationInformation createInfo = new SP.ListItemCreationInformation();
                        SP.ListItem newItem = trackInfo.AddItem(createInfo);
                        newItem["TimeEntryGuid"] = item.TimeEntryGuid;
                        newItem["TfsId"] = item.TfsId;
                        newItem["ServiceCall"] = item.ServiceCall;
                        newItem["Change"] = item.Change;
                        newItem["Title"] = item.Title;
                        newItem["Description"] = item.Description;
                        newItem["HoursCompleted"] = item.HoursCompleted;
                        newItem["HoursRemaining"] = item.HoursRemaining;
                        newItem["State"] = item.State;
                        newItem["Status"] = item.Status;
                        newItem["WorkType"] = item.WorkType;
                        newItem["WeekStarting"] = item.WeekStarting;
                        newItem["WeekOfYear"] = item.WeekOfYear;
                        newItem.Update();
                    }
                    clientContext.ExecuteQuery();

                });
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format("An error occurred while exporting list to SharePoint list [{0}]: {1}",
                        listTitle,
                        ex.Message));
            }
        }
        #endregion // Export Items to SharePoint List
        #endregion
    }
}
