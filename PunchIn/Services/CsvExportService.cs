using PunchIn.Models;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace PunchIn.Services
{
    public class CsvExportService : ViewModels.ViewModelBase
    {
        private readonly CsvExporter exporter;
        public CsvExportService()
        {
            string sep = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator;
            exporter = new CsvExporter(sep);
        }

        private bool isBusy = false;
        public bool IsBusy
        {
            get { return this.isBusy; }
            set
            {
                if (this.isBusy != value)
                {
                    this.isBusy = value;
                    OnPropertyChanged("IsBusy");
                }
            }
        }
        void SetErrorsMessage(Exception e)
        {
            Errors = e.Message;
        }

        public void ExportCollection(IList<ReportExportItem> collection, string[] columns, bool withHeading, string exportPath)
        {
            this.ExportAndDisplayAsync(collection, columns, withHeading, exportPath);
        }
        private async void ExportAndDisplayAsync(IList<ReportExportItem> collection, string[] columns, bool withHeading, string exportPath)
        {
            string filename = "";
            await Task.Run(() =>
                {
                    IsBusy = true;
                    try
                    {
                        if (withHeading)
                        {
                            foreach(string column in columns)
                            {
                                exporter.AddColumn(column);
                            }
                            exporter.AddLineBreak();
                        }
                        foreach (ReportExportItem item in collection)
                        {
                            foreach (string column in columns)
                            {
                                string itemValue = string.Empty;
                                PropertyInfo pi = item.GetType().GetProperty(column);
                                if (pi != null)
                                {
                                    object value = pi.GetValue(item);
                                    if (value != null)
                                    {
                                        itemValue = value.ToString();
                                    }
                                    else
                                    {
                                        itemValue = "-";
                                    }
                                }
                                exporter.AddColumn(itemValue);
                            }
                            exporter.AddLineBreak();
                        }
                        filename = exporter.Export(exportPath);
                    }
                    catch (System.IO.IOException ioex)
                    {
                        SetErrorsMessage(ioex);
                    }
                    catch (Exception ex)
                    {
                        SetErrorsMessage(ex);
                    }
                    finally
                    {
                        IsBusy = false;
                    }
                });

            if (!string.IsNullOrWhiteSpace(filename))
            {
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo(filename);
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
                System.Diagnostics.Process start = System.Diagnostics.Process.Start(startInfo);
            }
        }
    }
}
