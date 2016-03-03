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
using System.Windows.Shapes;

namespace HarbauerApp.screens
{
    /// <summary>
    /// Interaction logic for winEditReport.xaml
    /// </summary>
    public partial class winEditReport : Window
    {
        public winEditReport()
        {
            InitializeComponent();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if(btnSave.Tag is long)
            {
                long reportId = (long)btnSave.Tag;

                string arsenicRawWaterQuality = txtRawWaterQuality1.Text.Trim();
                string ironRawWaterQuality = txtRawWaterQuality2.Text.Trim();

                string arsenicTreatedWaterQuality = txtTreatedWaterQuality1.Text.Trim();
                string ironTreatedWaterQuality = txtTreatedWaterQuality2.Text.Trim();

                double arsenicRaw = 0, arsenicTreated = 0;
                if (!double.TryParse(arsenicRawWaterQuality, out arsenicRaw))
                {
                    MessageBox.Show("Please enter valid data into Raw Water Quality of Arsenic.", "Invalid Data", MessageBoxButton.OK, MessageBoxImage.Error);
                    txtRawWaterQuality1.Focus();
                    return;
                }

                if (!double.TryParse(arsenicTreatedWaterQuality, out arsenicTreated))
                {
                    MessageBox.Show("Please enter valid data into Treated Water Quality of Arsenic.", "Invalid Data", MessageBoxButton.OK, MessageBoxImage.Error);
                    txtTreatedWaterQuality1.Focus();
                    return;
                }

                double ironRaw = 0, ironTreated = 0;
                if (!double.TryParse(ironRawWaterQuality, out ironRaw))
                {
                    MessageBox.Show("Please enter valid data into Raw Water Quality of Iron.", "Invalid Data", MessageBoxButton.OK, MessageBoxImage.Error);
                    txtRawWaterQuality2.Focus();
                    return;
                }

                if (!double.TryParse(ironTreatedWaterQuality, out ironTreated))
                {
                    MessageBox.Show("Please enter valid data into Treated Water Quality of Iron.", "Invalid Data", MessageBoxButton.OK, MessageBoxImage.Error);
                    txtTreatedWaterQuality2.Focus();
                    return;
                }

                bool bacterRaw = cmbRawWaterQuality.SelectedIndex == 0;
                bool bacterTreated = cmbTreatedWaterQuality.SelectedIndex == 0;

                try
                {
                    #region Code
                    if (classes.Job.Database.updateReport(reportId, arsenicRaw, arsenicTreated, ironRaw, ironTreated, bacterRaw, bacterTreated))
                    {
                        MessageBox.Show("Report successfully saved", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        refreshReports();
                    }
                    else
                    {
                        MessageBox.Show("Unable to save report please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    #endregion
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            refreshReports();
        }

        private void refreshReports()
        {
            List<classes.Report> reports = classes.Job.Database.getAllReports();
            cmbReports.ItemsSource = reports;
            btnSave.IsEnabled = false;
            txtRawWaterQuality1.Text = txtRawWaterQuality2.Text = txtTreatedWaterQuality1.Text = txtTreatedWaterQuality2.Text = "";
            cmbRawWaterQuality.SelectedIndex = cmbTreatedWaterQuality.SelectedIndex = 0;
        }

        private void cmbReports_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(cmbReports.SelectedIndex>-1)
            {
                classes.Report report = cmbReports.SelectedItem as classes.Report;
                txtRawWaterQuality1.Text = report.reportContaminations[0].rawQ.ToString();
                txtRawWaterQuality2.Text = report.reportContaminations[1].rawQ.ToString();

                txtTreatedWaterQuality1.Text = report.reportContaminations[0].treatedQ.ToString();
                txtTreatedWaterQuality2.Text = report.reportContaminations[1].treatedQ.ToString();

                cmbRawWaterQuality.SelectedIndex = report.reportContaminations[2].rawQ.Equals("positive") ? 0 : 1;
                cmbTreatedWaterQuality.SelectedIndex = report.reportContaminations[2].treatedQ.Equals("positive") ? 0 : 1;

                btnSave.Tag = report.reportId;
                btnSave.IsEnabled = true;
            }
        }

        private void btnDeleteReport_Click(object sender, RoutedEventArgs e)
        {
            if(cmbReports.SelectedIndex==-1)
            {
                MessageBox.Show("Please select report from list to delete it.", "No report selected", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBoxResult sure = MessageBox.Show("Are you sure to delete selected report ?", "Delete confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (sure == MessageBoxResult.Yes)
            {
                try
                {
                    classes.Report report = cmbReports.SelectedItem as classes.Report;
                    if(report.delete())
                    {
                        MessageBox.Show("Report successfully deleted", "Deleted", MessageBoxButton.OK, MessageBoxImage.Information);
                    } else
                    {
                        throw new Exception("Can't delete report.");
                    }
                    refreshReports();
                } catch(Exception ex)
                {
                    MessageBox.Show("Unexpected error occured, please try again. [" + ex.Message + "]", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

        }
    }
}
