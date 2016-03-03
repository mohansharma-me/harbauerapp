using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace HarbauerApp.page
{
    /// <summary>
    /// Interaction logic for pageHome.xaml
    /// </summary>
    public partial class pageHome : UserControl
    {
        private MediaPlayer mediaPlayer = null;

        public enum ConnectionStatus
        {
            NOT_CONNECTED, DISCONNECTED, CONNECTED, CONNECTING, FAILED
        }

        public SerialPort _serialPort = null;

        public static bool UpdateImageList = false;
        private bool pauseSlider = true, stopSlider = false;
        public System.Threading.Thread sliderThread;

        public pageHome()
        {
            InitializeComponent();
            _serialPort = new SerialPort();
            initPortFields(ref _serialPort);
            _serialPort.DataReceived += _serialPort_DataReceived;
            _serialPort.ErrorReceived += _serialPort_ErrorReceived;
            _serialPort.PinChanged += _serialPort_PinChanged;
            
            mediaPlayer = new MediaPlayer();
            mediaPlayer.MediaFailed += MediaPlayer_MediaFailed;
            mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
        }

        private void MediaPlayer_MediaFailed(object sender, ExceptionEventArgs e)
        {
            ((MediaPlayer)sender).Position = TimeSpan.Zero;
            ((MediaPlayer)sender).Play();
        }

        private void MediaPlayer_MediaEnded(object sender, EventArgs e)
        {
            ((MediaPlayer)sender).Position = TimeSpan.Zero;
            ((MediaPlayer)sender).Play();
        }

        private void _serialPort_PinChanged(object sender, SerialPinChangedEventArgs e)
        {
            classes.Job.log("portPinChanged:" + e.EventType, null);
        }

        private void _serialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            classes.Job.log("portErrorReceived:" + e.EventType, null);
        }

        private void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            classes.Job.log("portDataReceived:" + e.EventType, null);
            try
            {

                /*
                
                0A 30 0D       TANK FULL MODE
                0A 31 0D       BACK WASH1 MODE
                0A 32 0D       BACK WASH2 MODE
                0A 33 0D       BACK WASH3 MODE
                0A 34 0D       BACK WASH4 MODE
                0A 39 0D       SERVICE MODE


                */
                byte[] data = new byte[3];
                int byteCount = _serialPort.Read(data, 0, 3);
                if (byteCount == 3)
                {
                    switch(data[1])
                    {

                        case 0x30: // TANK FULL
                            startVideo("tank_full");
                            break;

                        case 0x31: // mpv1
                            startVideo("mpv1");
                            break;

                        case 0x32: // mpv2
                            startVideo("mpv2");
                            break;

                        case 0x33: // mpv3
                            startVideo("mpv3");
                            break;

                        case 0x34: // mpv4
                            startVideo("mpv4");
                            break;

                        case 0x39: // service mode
                            startVideo("service_mode");
                            break;

                        default: break;
                    }
                }
            } catch(Exception ex)
            {
                classes.Job.log("serialPort::DataReceived::", ex, "serialPort");
            }
        }

        private void btnAnimation_Click(object sender, RoutedEventArgs e)
        {
            dpAnimation.Visibility = Visibility.Visible;
            dpImageGallery.Visibility = Visibility.Collapsed;
        }

        private void btnImageGallery_Click(object sender, RoutedEventArgs e)
        {
            dpAnimation.Visibility = Visibility.Collapsed;
            dpImageGallery.Visibility = Visibility.Visible;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            pauseSlider = !pauseSlider;
            if(sliderThread==null)
            {
                #region Slider Thread Init
                sliderThread = new System.Threading.Thread(() =>
                {
                    try
                    {
                        #region SlideRcode
                        updateImageList:
                        UpdateImageList = false;
                        List<classes.Image> images = new List<classes.Image>();
                        try
                        {
                            if (System.IO.Directory.Exists("images"))
                            {
                                string[] files = System.IO.Directory.GetFiles("images");
                                foreach (string file in files)
                                {
                                    images.Add(new classes.Image() { ImagePath = file });
                                }
                            }
                        }
                        catch(System.Threading.ThreadAbortException tae) { throw tae; }
                        catch (Exception) { }

                        while (!stopSlider)
                        {
                            while(images.Count==0)
                            {
                                Thread.Sleep(2000);
                                goto updateImageList;
                            }
                            int index = 0;
                            while (!pauseSlider && images.Count > 0 && dpImageGallery.Visibility==Visibility.Visible)
                            {
                                if (UpdateImageList) goto updateImageList;
                                Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                                {
                                    lblImageName.Text = images[index].ImageName;
                                    imgGallery.Source = images[index].ImageObject;
                                }));
                                System.Threading.Thread.Sleep((int)Properties.Settings.Default.appTimeInterval * 1000);
                                index++;
                                if (index >= images.Count)
                                    index = 0;
                            }
                        }
                        #endregion
                    }
                    catch (System.Threading.ThreadAbortException ex) { Console.WriteLine("Abort Called:"+ex); }
                    catch (Exception ex) { Console.WriteLine("Exception :"+ex); }
                });
                sliderThread.Name = "SliderThread";
                sliderThread.Start();
                #endregion
            }
            startVideo();

            if(btnConnect.Content.ToString().StartsWith("Connect to USB"))
            {
                btnConnect_Click(btnConnect, new RoutedEventArgs());
            }

        }

        private void startVideo(string videoID=null)
        {
            new Thread(() => {

                try
                {
                    #region VideoInit
                    if (videoID != null)
                    {
                        Properties.Settings.Default.appLastVideoID = videoID;
                        Properties.Settings.Default.Save();
                    }
                    else
                    {
                        videoID = Properties.Settings.Default.appLastVideoID;
                    }
                    //mediaPlayer.Stop();
                    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() => {
                        Uri videoUri = new Uri("media/video/" + videoID + ".mp4", UriKind.Relative);
                        mediaPlayer.Open(videoUri);
                        VideoDrawing drawing = new VideoDrawing();
                        drawing.Rect = new Rect(0, 0, 300, 200);
                        drawing.Player = mediaPlayer;
                        mediaPlayer.Play();
                        DrawingBrush brush = new DrawingBrush(drawing);
                        dpAnimation.Background = brush;
                    }));
                    #endregion
                }
                catch (Exception ex)
                {
                    classes.Job.log("startVideo::", ex);
                    throw new Exception("Unable to render video. [" + ex.Message + "]");
                }

            }) { ApartmentState = ApartmentState.STA }.Start();
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            if(btnConnect.Content.ToString().StartsWith("Connected"))
            {
                try
                {
                    if(_serialPort.IsOpen)
                    {
                        _serialPort.Close();
                    }
                    btnConnect.Content = "Connect To USB";
                } catch(Exception ex)
                {
                    classes.Job.log("PortSearch::", ex, "portSearch");
                }
                return;
            }

            string port = Properties.Settings.Default.appComPort.Trim().ToLower();
            if(port.Length==0)
            {
                btnConnect.Content = "Select COMPORT from Settings!!";
                return;
            }

            btnConnect.Content = "Connecting with " + port.ToUpper();

            try
            {

                new Thread(() => {

                    try
                    {

                        _serialPort.PortName = port;
                        _serialPort.Open();
                        if (_serialPort.IsOpen)
                        {
                            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                            {
                                btnConnect.Content = "Connected to " + port.ToUpper();
                            }));
                        }
                        else
                        {
                            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                            {
                                btnConnect.Content = "Failed to connected with " + port.ToUpper() + "!!";
                            }));
                            classes.Job.log("PortSearch::" + port + ":: skiped", null, "portSearch");
                        }

                    } catch(Exception ex)
                    {
                        Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                        {
                            btnConnect.Content = "Failed to connected with " + port.ToUpper() + "!!";
                            MessageBox.Show("Error occured while establishing connection with comport:" + port, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }));
                        classes.Job.log("PortSearch::" + port, ex, "portSearch");
                    }

                }) { ApartmentState=ApartmentState.STA }.Start();

            }
            catch (Exception ex)
            {
                classes.Job.log("PortConnect::" + port, ex, "portSearch");
                MessageBox.Show("Unable to establish communication with machine at comport " + port + "., please try again.", "Connection failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void btnConnectAutoSearch_Click(object sender, RoutedEventArgs e)
        {
            IsEnabled = false;
            String original = btnConnect.Content.ToString();
            if(original.StartsWith("Connect to USB"))
            {
                int codeUsedtoCommit;
                btnConnect.Content = "Connecting...";
                new Thread(() => {
                    #region Code
                    try
                    {
                        #region Code
                        string[] ports = SerialPort.GetPortNames();
                        foreach (string _port in ports)
                        {
                            SerialPort port = new SerialPort(_port);
                            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                            {
                                btnConnect.Content = "Connecting with " + _port;
                            }));
                            try
                            {
                                initPortFields(ref port);
                                port.Open();
                                if (port.IsOpen)
                                {
                                    port.WriteLine("hi");
                                    Thread.Sleep(3000);
                                    string ret = port.ReadExisting();
                                    if (ret.ToLower().Contains("org"))
                                    {
                                        port.BaseStream.Flush();
                                        port.Close();

                                        _serialPort.PortName = _port;
                                        _serialPort.Open();
                                        if (_serialPort.IsOpen)
                                        {
                                            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                                            {
                                                btnConnect.Content = "Connected (" + _port + ")";
                                            }));
                                            break;
                                        }
                                        else
                                        {
                                            classes.Job.log("PortSearch::" + _port + ":: skiped", null, "portSearch");
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                classes.Job.log("PortSearch::" + _port + "::", ex, "portSearch");
                            }
                            finally
                            {
                                if (port.IsOpen) port.Close();
                            }
                        }

                        #endregion
                    }
                    catch (Exception ex)
                    {
                        Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() => {
                            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }));
                    }
                    finally
                    {
                        Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() => {
                            IsEnabled = true;
                            if(!_serialPort.IsOpen)
                            {
                                btnConnect.Content = "Connect to USB, search failed, try again!!";
                            }
                        }));
                    }
                    #endregion
                })
                { ApartmentState = ApartmentState.STA }.Start();
            }
            else if(original.StartsWith("Connected"))
            {
                if (_serialPort.IsOpen)
                    _serialPort.Close();
                btnConnect.Content = "Connect to USB";
                IsEnabled = true;
            }
            else
            {

            }
        }

        public static void initPortFields(ref SerialPort port)
        {
            port.BaudRate = 9600;
            port.DataBits = 8;
            port.Parity = Parity.None;
            port.StopBits = StopBits.One;
            port.RtsEnable = false;
            port.DtrEnable = false;
            port.Handshake = Handshake.None;
            port.ReceivedBytesThreshold = 1;
            port.NewLine = Environment.NewLine;
            port.ReadTimeout = port.WriteTimeout = 10000;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            pauseSlider = !pauseSlider;
        }
    }
}
