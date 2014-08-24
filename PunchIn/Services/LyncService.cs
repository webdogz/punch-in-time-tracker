using Microsoft.Lync.Model;
using Microsoft.Lync.Model.Group;
using PunchIn.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;

namespace PunchIn.Services
{
    internal class LyncService
    {
        LyncClient LyncClient;
        ContactManager ContactManager;

        public LyncService()
        {
            try
            {
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
            }
            catch (ClientNotFoundException clientNotFoundException)
            {
                throw new Exception("Lync client was not found:" + clientNotFoundException);
            }
            catch (NotStartedByUserException notStartedByUserException)
            {
                throw new Exception("Lync was not started:" + notStartedByUserException);
            }
            catch (LyncClientException lyncClientException)
            {
                throw new Exception("Lync unknown exception was thrown:" + lyncClientException);
            }
            catch (SystemException systemException)
            {
                if (IsLyncException(systemException))
                {
                    // Log the exception thrown by the Lync Model API.
                    throw new Exception("System Error:" + systemException);
                }
                else
                {
                    // Rethrow the SystemException which did not come from the Lync Model API.
                    throw;
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
            catch (LyncClientException)
            {
                throw;
            }
            catch (SystemException systemException)
            {
                if (IsLyncException(systemException))
                {
                    // Log the exception thrown by the Lync Model API.
                    throw new Exception("Lync System Error while signing in:" + systemException);
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
            catch (LyncClientException)
            {
                throw;
            }
            catch (SystemException systemException)
            {
                if (IsLyncException(systemException))
                {
                    // Log the exception thrown by the Lync Model API.
                    throw new Exception("Lync System Error while signing out:" + systemException);
                }
                else
                {
                    // Rethrow the SystemException which did not come from the Lync Model API.
                    throw;
                }
            }

        }

        public ObservableCollection<GroupInfo> GetGroupInfoList()
        {
            ObservableCollection<GroupInfo> Groups = new ObservableCollection<GroupInfo>();

            Group favoriteGroup = GetSpecialGroup(GroupType.FavoriteContacts);
            Groups.Add(GetGroupInfo(favoriteGroup));
            Group frequentGroup = GetSpecialGroup(GroupType.FrequentContacts);
            Groups.Add(GetGroupInfo(frequentGroup));
            Group otherContactsGroup = GetOtherContactsGroup();
            Groups.Add(GetGroupInfo(otherContactsGroup));

            foreach (Group group in ContactManager.Groups.GetGroupsByType(GroupType.CustomGroup).Where(g => !g.Name.Equals("Other Contacts")))
            {
                Groups.Add(GetGroupInfo(group));
            }

            foreach (Group group in ContactManager.Groups.GetGroupsByType(GroupType.DistributionGroup))
            {
                Groups.Add(GetGroupInfo(group));
            }

            return Groups;
        }

        private Group GetOtherContactsGroup()
        {
            return ContactManager.Groups.FirstOrDefault(g => g.Type.Equals(GroupType.CustomGroup) && g.Name.Equals("Other Contacts"));
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
}
