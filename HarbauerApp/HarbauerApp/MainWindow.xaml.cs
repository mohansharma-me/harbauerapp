using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HarbauerApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        System.Threading.Thread threadLastReport = null;

        #region Pages

        page.pageHome homePage = null;
        page.pageLogin loginPage = null;
        page.pageReport reportPage = null;
        page.pageSettings settingsPage = null;

        #endregion

        public MainWindow()
        {
            InitializeComponent();
        }

        private void showPage(UIElement elem)
        {
            frmPage.Children.Clear();
            frmPage.Children.Add(elem);
        }

        private void btnHome_Click(object sender, RoutedEventArgs e)
        {
            selectNav(sender as Button);
            if (homePage == null)
                homePage = new page.pageHome();
            showPage(homePage);
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            selectNav(sender as Button);
            if (loginPage == null)
                loginPage = new page.pageLogin();
            showPage(loginPage);
        }

        private void btnReport_Click(object sender, RoutedEventArgs e)
        {
            selectNav(sender as Button);
            if (reportPage == null)
                reportPage = new page.pageReport();
            showPage(reportPage);
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            selectNav(sender as Button);
            if (settingsPage == null)
                settingsPage = new page.pageSettings();
            showPage(settingsPage);
        }

        private void selectNav(Button showThis)
        {
            btnHome.IsCancel = btnLogin.IsCancel = btnReport.IsCancel = btnSettings.IsCancel = false;
            if (showThis != null) showThis.IsCancel = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            System.Threading.Thread thread = new System.Threading.Thread(() =>
            {
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                {
                    homePage = new page.pageHome();
                    loginPage = new page.pageLogin();
                    reportPage = new page.pageReport();
                    settingsPage = new page.pageSettings();

                    btnHome_Click(btnHome, new RoutedEventArgs());
                }));
            });
            thread.SetApartmentState(System.Threading.ApartmentState.STA);
            thread.Start();


            threadLastReport = new System.Threading.Thread(() =>
              {

                  try
                  {
                      while (true)
                      {
                          classes.Job.Database.rpLast = null;
                          classes.Job.Database.getLastReport();
                          Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() => {
                              gvSticker.DataContext = classes.Job.Database.getLastReport();
                          }));
                          System.Threading.Thread.Sleep(2500);

                          try
                          {
                              
                          }
                          catch(System.Threading.ThreadAbortException abortException) { throw abortException; }
                          catch (Exception ex) { Console.WriteLine("gSliderError#2:" + ex); }
                      }
                  }
                  catch (Exception ex) { Console.WriteLine("gSliderError#1:" + ex); }

              });

            threadLastReport.Start();

            startMarquee(canvas1, marquee1, 20);
            startMarquee(canvas2, marquee2, 20);
            startMarquee(canvas3, marquee3, 20);
            startMarquee(canvas4, marquee4, 20);
        }

        private void startMarquee(Canvas canMain, TextBlock tbmarquee, double _marqueeTimeInSeconds)
        {
            double height = canMain.ActualHeight - tbmarquee.ActualHeight;
            tbmarquee.Margin = new Thickness(0, height / 2, 0, 0);
            System.Windows.Media.Animation.DoubleAnimation doubleAnimation = new DoubleAnimation();
            doubleAnimation.From = -tbmarquee.ActualWidth;
            doubleAnimation.To = canMain.ActualWidth;
            doubleAnimation.RepeatBehavior = RepeatBehavior.Forever;
            doubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(_marqueeTimeInSeconds));
            tbmarquee.BeginAnimation(Canvas.RightProperty, doubleAnimation);

        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            if (threadLastReport != null)
                threadLastReport.Abort();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (homePage != null && homePage.sliderThread != null)
            {
                try
                {
                    homePage.sliderThread.Abort();
                }
                catch (Exception ex) { Console.WriteLine("Exception1:" + ex); }
            }

        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure to quit application ?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) 
            {
                Close();
            }
        }
    }
}
