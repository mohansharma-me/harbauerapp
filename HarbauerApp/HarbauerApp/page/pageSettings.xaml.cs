using HarbauerApp.classes;
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
    /// Interaction logic for pageSettings.xaml
    /// </summary>
    public partial class pageSettings : UserControl
    {
        public pageSettings()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            txtTimeInterval.Text = Properties.Settings.Default.appTimeInterval.ToString();
            txtArsenicLimit.Text = Properties.Settings.Default.appALimit.ToString();
            txtIronLimit.Text = Properties.Settings.Default.appILimit.ToString();
            if (Properties.Settings.Default.appBLimit == 0)
                cmbBacterLimit.SelectedIndex = 0;
            else
                cmbBacterLimit.SelectedIndex = 1;

            if (Properties.Settings.Default.appLogin.Trim().Length>0)
            {
                dpChangePassword.Visibility = Visibility.Visible;

                new System.Threading.Thread(() => {
                    try
                    {
                        #region Code
                        bool isAdmin = false;
                        Job.Database.validateUserToken(ref isAdmin);
                        if(isAdmin)
                        {
                            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() => {
                                dpChangePermissableLimits.Visibility = Visibility.Visible;
                            }));
                        }
                        else
                        {
                            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                            {
                                dpChangePermissableLimits.Visibility = Visibility.Collapsed;
                            }));
                        }
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() => {
                            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }));
                    }
                }) { ApartmentState=System.Threading.ApartmentState.STA }.Start();

            }
            else
            {
                dpChangePassword.Visibility = Visibility.Collapsed;
            }

            try
            {
                string[] ports = System.IO.Ports.SerialPort.GetPortNames();
                List<Ports> _ports = new List<Ports>();
                int index = -1, count = 0;
                foreach(string port in ports)
                {
                    if(port.ToLower().Equals(Properties.Settings.Default.appComPort.ToLower().Trim()))
                    {
                        index = count;
                    }
                    _ports.Add(new Ports() { PortName = port });
                    count++;
                }
                cmbPorts.ItemsSource = _ports;
                cmbPorts.SelectedIndex = index;
            }
            catch (Exception ex)
            {
                
            }

        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {

            if (cmbPorts.SelectedIndex == -1)
            {
                MessageBox.Show("Please select com port to establish connection with machine/panel.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            decimal timeInterval = Properties.Settings.Default.appTimeInterval;
            if(decimal.TryParse(txtTimeInterval.Text.Trim(), out timeInterval))
            {

                try
                {
                    #region ChangePasswordCode
                    if(txtCurrentPassword.Password.ToString().Trim().Length>0 && dpChangePassword.Visibility==Visibility.Visible &&  !classes.Job.Database.changePassword(txtCurrentPassword.Password,txtNewPassword.Password))
                    {
                        MessageBox.Show("Unable to change passowrd.", "Failed operation", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    #endregion
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error while PasswordChange operation", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                if(dpChangePermissableLimits.Visibility==Visibility.Visible)
                {
                    #region PermissableLimits
                    double aLimit = Properties.Settings.Default.appALimit;
                    double iLimit = Properties.Settings.Default.appILimit;
                    int bLimit = Properties.Settings.Default.appBLimit;

                    if(double.TryParse(txtArsenicLimit.Text.Trim(), out aLimit))
                    {
                        if(double.TryParse(txtIronLimit.Text.Trim(), out iLimit))
                        {
                            bLimit = cmbBacterLimit.SelectedIndex;

                            Properties.Settings.Default.appALimit = aLimit;
                            Properties.Settings.Default.appILimit = iLimit;
                            Properties.Settings.Default.appBLimit = bLimit;
                        }
                        else
                        {
                            MessageBox.Show("Please enter valid Iron Permissable Limit.", "Invalid Data", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Please enter valid Arsenic Permissable Limit.", "Invalid Data", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    #endregion
                }

                if(cmbPorts.SelectedIndex>-1)
                {
                    classes.Ports port = cmbPorts.SelectedItem as classes.Ports;
                    Properties.Settings.Default.appComPort = port.PortName;
                }

                Properties.Settings.Default["appTimeInterval"] = timeInterval;
                Properties.Settings.Default.Save();
                MessageBox.Show("Settings saved successfully.", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Please enter valid time interval (only digits are allowed).", "Invalid Time Interval", MessageBoxButton.OK, MessageBoxImage.Hand);
                txtTimeInterval.Text = timeInterval.ToString();
            }

        }

        private void UserControl_MouseMove(object sender, MouseEventArgs e)
        {
            
        }

    }
}
