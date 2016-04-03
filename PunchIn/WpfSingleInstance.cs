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
        internal static void Make(string name, Application app)
        {
            EventWaitHandle eventWaitHandle = null;
            string eventName = Environment.MachineName + "-" + name;
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

        private static void waitOrTimerCallback(object state, bool timedOut)
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

        #region Application Startup Args

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

        #region Store Application Startup Args - TODO: Remove this shit
        private static readonly string isolatedStorageFileName = "PunchStartupArgs.tmp";

        private static void SaveActivationData()
        {
            string startupArg = GetActivationData();
            if (!string.IsNullOrWhiteSpace(startupArg))
            {
                IsolatedStorageFileStream isoStream = GetIsolatedStorageFileStream(GetIsolatedStorageFile(), isolatedStorageFileName, FileMode.OpenOrCreate);
                StreamWriter sw = new StreamWriter(isoStream);
                sw.Write(startupArg);
                sw.Close();
            }
        }

        private static void ProcessActivationData()
        {
            IsolatedStorageFile isoFile = GetIsolatedStorageFile();
            IsolatedStorageFileStream isoStream = GetIsolatedStorageFileStream(isoFile, isolatedStorageFileName, FileMode.OpenOrCreate);
            StreamReader sr = new StreamReader(isoStream);
            string arg = sr.ReadToEnd();
            sr.Close();
            
            isoFile.DeleteFile(isolatedStorageFileName);
            App.ProcessArg(arg);
        }
        private static IsolatedStorageFile GetIsolatedStorageFile()
        {
            return IsolatedStorageFile.GetStore(
                    IsolatedStorageScope.User | IsolatedStorageScope.Assembly,
                    null,
                    null);
        }
        private static IsolatedStorageFileStream GetIsolatedStorageFileStream(IsolatedStorageFile isoStore, string filename, FileMode fileMode)
        {
            return new IsolatedStorageFileStream(filename, fileMode, isoStore);
        }
        #endregion

        #endregion


    }
}