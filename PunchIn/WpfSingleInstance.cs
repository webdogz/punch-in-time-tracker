using System;
using System.Threading;
using System.Windows;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Runtime.Hosting;

namespace PunchIn
{
    public static class WpfSingleInstance
    {
        internal static void Make(String name, Application app)
        {
            EventWaitHandle eventWaitHandle = null;
            String eventName = Environment.MachineName + "-" + name;
            bool isFirstInstance = false;

            try
            {
                SaveActivationData();
                eventWaitHandle = EventWaitHandle.OpenExisting(eventName);
            }
            catch
            {
                isFirstInstance = true;
            }

            if (isFirstInstance)
            {
                eventWaitHandle = new EventWaitHandle(
                    false,
                    EventResetMode.AutoReset,
                    eventName);

                ThreadPool.RegisterWaitForSingleObject(eventWaitHandle, waitOrTimerCallback, app, Timeout.Infinite, false);
                eventWaitHandle.Close();
                ProcessActivationData();
            }
            else
            {
                eventWaitHandle.Set();
                Environment.Exit(0);
            }
        }


        private delegate void dispatcherInvoker();

        private static void waitOrTimerCallback(Object state, Boolean timedOut)
        {
            Application app = (Application)state;
            app.Dispatcher.BeginInvoke(
                new dispatcherInvoker(delegate()
                {
                    ProcessActivationData();
                }),
                null
            );
        }

        // Args functionality for test purpose and not developed carefuly
        #region Args

        internal static readonly object StartArgKey = "StartArg";

        private static readonly String isolatedStorageFileName = "SomeFileInTheRoot.txt";

        private static string GetActivationData()
        {
            ActivationArguments activationArgs = AppDomain.CurrentDomain.SetupInformation.ActivationArguments;
            if (activationArgs != null && 
                activationArgs.ActivationData != null && 
                activationArgs.ActivationData.Any())
            {
                var uri = new Uri(activationArgs.ActivationData[0]);
                if (!string.IsNullOrWhiteSpace(uri.LocalPath) &&
                    File.Exists(uri.LocalPath))
                {
                    return uri.LocalPath;
                }
            }
            return string.Empty;
        }

        private static void SaveActivationData()
        {
            string startupArg = GetActivationData();
            if (!string.IsNullOrWhiteSpace(startupArg))
            {
                IsolatedStorageFile isoStore =
                    IsolatedStorageFile.GetStore(
                        IsolatedStorageScope.User | IsolatedStorageScope.Assembly,
                        null,
                        null);

                IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream(isolatedStorageFileName, FileMode.Create, isoStore);
                StreamWriter sw = new StreamWriter(isoStream);
                sw.Write(startupArg);
                sw.Close();
            }
        }

        private static void ProcessActivationData()
        {
            IsolatedStorageFile isoStore =
                IsolatedStorageFile.GetStore(
                    IsolatedStorageScope.User | IsolatedStorageScope.Assembly,
                    null,
                    null);

            IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream(isolatedStorageFileName, FileMode.OpenOrCreate, isoStore);
            StreamReader sr = new StreamReader(isoStream);
            string arg = sr.ReadToEnd();
            sr.Close();

            isoStore.DeleteFile(isolatedStorageFileName);
            App.ProcessArg(arg);
        }

        #endregion


    }
}