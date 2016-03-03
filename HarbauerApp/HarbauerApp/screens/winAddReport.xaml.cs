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
    /// Interaction logic for winAddReport.xaml
    /// </summary>
    public partial class winAddReport : Window
    {
        public winAddReport()
        {
            InitializeComponent();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
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
                if (classes.Job.Database.addReport(arsenicRaw, arsenicTreated, ironRaw, ironTreated, bacterRaw, bacterTreated))
                {
                    MessageBox.Show("Report successfully added", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    txtRawWaterQuality1.Text = txtRawWaterQuality2.Text = txtTreatedWaterQuality1.Text = txtTreatedWaterQuality2.Text = "";
                    txtRawWaterQuality1.Focus();
                    classes.Job.Database.rpLast = null;
                }
                else
                {
                    MessageBox.Show("Unable to add new report please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                #endregion
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
