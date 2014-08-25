using Microsoft.Lync.Model.Group;
using PunchIn.Services;
using System;
using System.Collections.ObjectModel;

namespace PunchIn.ViewModels
{
    public class MessagesViewModel : ViewModelBase
    {
        private readonly LyncService service;
        public MessagesViewModel()
        {
            try
            {
                service = new LyncService();
                Groups = service.GetGroupInfoList();
            }
            catch (Exception e)
            {
                SetErrorsMessage(e);
            }
        }

        private ObservableCollection<GroupInfo> groups;
        public ObservableCollection<GroupInfo> Groups
        {
            get { return this.groups; }
            set
            {
                if (this.groups != value)
                {
                    this.groups = value;
                    OnPropertyChanged("Groups");
                }
            }
        }
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
        
        private GroupInfo selectedGroupInfo;
        public GroupInfo SelectedGroupInfo
        {
            get { return this.selectedGroupInfo; }
            set
            {
                if (this.selectedGroupInfo != value)
                {
                    this.selectedGroupInfo = value;
                    OnPropertyChanged("SelectedGroupInfo");
                }
            }
        }
    }

    public class GroupInfo
    {
        public string GroupName { get; set; }
        public int ContactCount { get; set; }
        public GroupType Type { get; set; }
        public bool CanRemove { get; set; }

        public Group Group { get; set; }

        public GroupInfo(Group group)
        {
            this.Group = group;
            this.GroupName = group.Name;
            this.ContactCount = group.Count;
            this.Type = group.Type;
            if (group.Type == GroupType.DistributionGroup ||
                (group.Type == GroupType.CustomGroup && group.Name != "Other Contacts"))
            {
                this.CanRemove = true;
            }
            else
            {
                this.CanRemove = false;
            }
        }
    }
}
