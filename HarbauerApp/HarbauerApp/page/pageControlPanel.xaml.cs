using System;
using System.Collections.Generic;
using System.IO;
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
    /// Interaction logic for pageControlPanel.xaml
    /// </summary>
    public partial class pageControlPanel : UserControl
    {

        public delegate void LogoutDelegate();
        public LogoutDelegate LogoutNow;

        public pageControlPanel(string userToken, LogoutDelegate logoutDelegate)
        {
            InitializeComponent();
            LogoutNow = logoutDelegate;            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LogoutNow();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            lblPreLoader.Visibility = Visibility.Visible;
            lblPreLoader.Text = "LOGGING IN...";
            new System.Threading.Thread(() => {
                bool isValidated = false;
                try
                {
                    bool isAdmin = false;
                    isValidated = classes.Job.Database.validateUserToken(ref isAdmin);
                    try
                    {
                        #region MyRegion
                        Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                        {
                            lblPreLoader.Text = "Welcome " + Properties.Settings.Default.appLoginName;
                            lblStatus.Text = lblPreLoader.Text;
                            lblPreLoader.Visibility = Visibility.Collapsed;
                            frmPanel.Visibility = Visibility.Visible;

                            if(!isAdmin)
                            {
                                btnDeleteImages.Visibility = btnEditReport.Visibility = btnManageTechnician.Visibility = Visibility.Collapsed;
                            }
                        }));
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() => {
                            MessageBox.Show("Error occured while logging you in : " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            isValidated = false;
                        }));
                    }
                }
                catch (Exception ex)
                {
                    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(()=> {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        lblPreLoader.Text = "Unexpected error so please restart software.";
                        lblPreLoader.Visibility = Visibility.Visible;
                        frmPanel.Visibility = Visibility.Collapsed;
                    }));
                }
                finally
                {
                    if(!isValidated)
                    {
                        Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                        {
                            MessageBox.Show("Sorry, your logged session is expired, please login again.", "Session Expired", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                            LogoutNow();
                        }));
                    }
                }
            }) { ApartmentState = System.Threading.ApartmentState.STA }.Start();
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            LogoutNow();
        }

        private void btnDeleteImages_Click(object sender, RoutedEventArgs e)
        {
            new screens.winImages().ShowDialog();
            page.pageHome.UpdateImageList = true;
        }

        private void btnEditReport_Click(object sender, RoutedEventArgs e)
        {
            new screens.winEditReport().ShowDialog();
        }

        private void btnAddImages_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog od = new Microsoft.Win32.OpenFileDialog();
            od.Title = "Select image files...";
            od.Filter = "Image files|*.jpg;*.jpeg;*.bmp;*.gif;*.png;|All files|*.*";
            od.Multiselect = true;
            od.CheckFileExists = true;
            od.ReadOnlyChecked = true;
            bool? flag = od.ShowDialog();
            if(flag.HasValue && flag.Value)
            {
                string[] filenames = od.FileNames;

                new System.Threading.Thread((fileList) =>
                {
                    int i = 1;
                    string data = "";
                    foreach(string file in (string[])fileList)
                    {
                        try
                        {
                            if(!Directory.Exists("images"))
                            {
                                Directory.CreateDirectory("images");
                            }
                            FileInfo fi = new FileInfo(file);

                            string outFile = "images/" + fi.Name;
                            File.Move(file, outFile);
                            data += i + ". " + fi.Name + Environment.NewLine;
                            i++;
                        }
                        catch (Exception){}
                    }

                    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() => {
                        MessageBox.Show("Following images are added : " + Environment.NewLine + data, "Status", MessageBoxButton.OK, MessageBoxImage.Information);
                    }));

                })
                { ApartmentState = System.Threading.ApartmentState.STA }.Start(filenames);
            }
            page.pageHome.UpdateImageList = true;
        }

        private void btnAddReport_Click(object sender, RoutedEventArgs e)
        {
            new screens.winAddReport().ShowDialog();
        }

        private void btnManageTechnician_Click(object sender, RoutedEventArgs e)
        {
            new screens.winManageTechnicians().ShowDialog();
        }
    }
}
