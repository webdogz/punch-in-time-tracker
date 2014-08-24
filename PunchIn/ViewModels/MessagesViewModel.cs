using Microsoft.Lync.Model;
using Microsoft.Lync.Model.Group;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;

namespace PunchIn.ViewModels
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public class MessagesViewModel : ViewModelBase
    {
        LyncClient LyncClient;
        ContactManager ContactManager;

        public ObservableCollection<GroupInfo> Groups { get; set; }

        public MessagesViewModel()
        {
            OnLoaded();
        }
        private void OnLoaded()
        {
            try
            {
                Groups = new ObservableCollection<GroupInfo>();
                LyncClient = LyncClient.GetClient();
                if (LyncClient.State == ClientState.SignedOut)
                {
                    MessageBoxResult msgResult = MessageBox.Show(
                            "You are not currently signed in. Would you like to sign in?",
                            "Sign In",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);
                    if (msgResult == MessageBoxResult.Yes)
                    {
                        LyncClient.BeginSignIn(null, null, null, SignInCallback, null);
                    }
                    else
                        return;
                }
                ContactManager = LyncClient.ContactManager;
                PopulateGroupInfo();
            }
            catch (ClientNotFoundException clientNotFoundException)
            {
                SetErrorsMessage(clientNotFoundException);
                return;
            }
            catch (NotStartedByUserException notStartedByUserException)
            {
                SetErrorsMessage(notStartedByUserException);
                return;
            }
            catch (LyncClientException lyncClientException)
            {
                SetErrorsMessage(lyncClientException);
                return;
            }
            catch (SystemException systemException)
            {
                if (IsLyncException(systemException))
                {
                    // Log the exception thrown by the Lync Model API.
                    SetErrorsMessage(systemException);
                    return;
                }
                else
                {
                    // Rethrow the SystemException which did not come from the Lync Model API.
                    throw;
                }
            }
        }
        
        void SetErrorsMessage(Exception e)
        {
            Errors = e.Message;
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
        /// <summary>
        /// Callback invoked when LyncClient.BeginSignIn is completed
        /// </summary>
        /// <param name="result">The status of the asynchronous operation</param>
        private void SignInCallback(IAsyncResult result)
        {
            try
            {
                LyncClient.EndSignIn(result);
            }
            catch (LyncClientException e)
            {
                SetErrorsMessage(e);
            }
            catch (SystemException systemException)
            {
                if (IsLyncException(systemException))
                {
                    // Log the exception thrown by the Lync Model API.
                    SetErrorsMessage(systemException);
                }
                else
                {
                    // Rethrow the SystemException which did not come from the Lync Model API.
                    throw;
                }
            }

        }

        /// <summary>
        /// Callback invoked when LyncClient.BeginSignOut is completed
        /// </summary>
        /// <param name="result">The status of the asynchronous operation</param>
        private void SignOutCallback(IAsyncResult result)
        {
            try
            {
                LyncClient.EndSignOut(result);
            }
            catch (LyncClientException e)
            {
                SetErrorsMessage(e);
            }
            catch (SystemException systemException)
            {
                if (IsLyncException(systemException))
                {
                    // Log the exception thrown by the Lync Model API.
                    SetErrorsMessage(systemException);
                }
                else
                {
                    // Rethrow the SystemException which did not come from the Lync Model API.
                    throw;
                }
            }

        }

        private void PopulateGroupInfo()
        {
            Groups.Clear();

            Group favoriteGroup = GetSpecialGroup(GroupType.FavoriteContacts);
            Groups.Add(GetGroupInfo(favoriteGroup));
            Group frequentGroup = GetSpecialGroup(GroupType.FrequentContacts);
            Groups.Add(GetGroupInfo(frequentGroup));
            Group otherContactsGroup = GetOtherContactsGroup();
            Groups.Add(GetGroupInfo(otherContactsGroup));

            foreach (Group group in ContactManager.Groups.GetGroupsByType(GroupType.CustomGroup).Where(g=>!g.Name.Equals("Other Contacts")))
            {
                Groups.Add(GetGroupInfo(group));
            }

            foreach (Group group in ContactManager.Groups.GetGroupsByType(GroupType.DistributionGroup))
            {
                Groups.Add(GetGroupInfo(group));
            }

            OnPropertyChanged("Groups");
        }

        private Group GetOtherContactsGroup()
        {
            return ContactManager.Groups.FirstOrDefault(g=>g.Type.Equals(GroupType.CustomGroup) && g.Name.Equals("Other Contacts"));
        }

        private Group GetSpecialGroup(GroupType groupType)
        {
            return ContactManager.Groups.FirstOrDefault(g => g.Type.Equals(groupType));
        }

        private GroupInfo GetGroupInfo(Group group)
        {
            int ContactCounts = 0;

            GroupType type = group.Type;

            if (type != GroupType.DistributionGroup)
            {
                ContactCounts = group.Count;
            }

            return new GroupInfo(group);
        }

        /// <summary>
        /// Identify if a particular SystemException is one of the exceptions which may be thrown
        /// by the Lync Model API.
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        private bool IsLyncException(SystemException ex)
        {
            return
                ex is NotImplementedException ||
                ex is ArgumentException ||
                ex is NullReferenceException ||
                ex is NotSupportedException ||
                ex is ArgumentOutOfRangeException ||
                ex is IndexOutOfRangeException ||
                ex is InvalidOperationException ||
                ex is TypeLoadException ||
                ex is TypeInitializationException ||
                ex is InvalidComObjectException ||
                ex is InvalidCastException;
        }
    }

    public class GroupInfo
    {
        public string GroupName { get; set; }
        public int ContactNumber { get; set; }
        public GroupType Type { get; set; }
        public bool CanRemove { get; set; }

        public Group Group { get; set; }

        public GroupInfo(Group group)
        {
            this.Group = group;
            this.GroupName = group.Name;
            this.ContactNumber = group.Count;
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
