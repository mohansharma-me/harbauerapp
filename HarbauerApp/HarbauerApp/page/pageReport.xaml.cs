using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HarbauerApp.page
{
    /// <summary>
    /// Interaction logic for pageReport.xaml
    /// </summary>
    public partial class pageReport : UserControl
    {
        public pageReport()
        {
            InitializeComponent();
        }

        private void refreshReports()
        {
            List<classes.Report> reports = classes.Job.Database.getAllReports();
            cmbReports.ItemsSource = reports;
            if(cmbReports.Items.Count>0)
            {
                cmbReports.SelectedIndex = cmbReports.Items.Count - 1;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            refreshReports();
        }

        private void cmbReports_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(cmbReports.SelectedIndex>-1)
            {
                classes.Report report = cmbReports.SelectedItem as classes.Report;
                gvReport.DataContext = report;
            }
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            if(cmbReports.SelectedIndex==-1)
            {
                MessageBox.Show("Please select at least one report from Bottom,Right of screen.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                classes.Report report = cmbReports.SelectedItem as classes.Report;
                if(report!=null)
                {
                    String title = "Report of " + report.DisplayString;
                    String columns = "Contamination,Raw Water Quality,Permissable Limits,Treated Water Quality,Safe/Unsafe";
                    String row1 = "Arsenic (in mg/l)," + report.aRaw + "," + report.aLimit + "," + report.aTreated + "," + report.aSafe;
                    String row2 = "Iron (in mg/l)," + report.iRaw + "," + report.iLimit + "," + report.iTreated + "," + report.iSafe;
                    String row3 = "Bacteriological," + report.bRaw + "," + report.bLimit + "," + report.bTreated + "," + report.bSafe;
                    String row4 = "Report status : ," + report.Sticker;

                    String fileData = title + Environment.NewLine + columns + Environment.NewLine + row1 + Environment.NewLine + row2 + Environment.NewLine + row3 + Environment.NewLine + row4;
                    Microsoft.Win32.SaveFileDialog sd = new Microsoft.Win32.SaveFileDialog();
                    sd.Filter = "CSV File|*.csv";
                    sd.Title = "Save to";
                    bool? retSd = sd.ShowDialog();
                    if(retSd.HasValue && retSd.Value)
                    {
                        System.IO.File.WriteAllText(sd.FileName, fileData);
                        MessageBox.Show("Report successfully exporeted to : " + sd.FileName, "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            } catch(Exception ex)
            {
                MessageBox.Show("Unexpected error occured, please try again. [" + ex.Message + "]", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
