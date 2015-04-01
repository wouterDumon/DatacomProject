using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.ComponentModel;
using SharpDX.DirectInput;

namespace Colour_Dome
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SerialPort srp = new SerialPort();
        // TODO: Pas panning speed aan (Byte 5)

        private Joystick joystick;
        private JoystickState joystickState;
       // private SerialPort srp = new SerialPort();
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            GetPortsInCombobox();
        }
        private void btnInit_Click(object sender, RoutedEventArgs e)
        {
            OpenPort(Convert.ToString(cboPorts.SelectedItem));
            System.Threading.Thread.Sleep(500);
            ini();

        }
        private void OpenPort(string portname)
        {
            // Configureer de poort
            srp.BaudRate = 2400;
            srp.DataBits = 8;
            srp.Parity = Parity.None;
            srp.StopBits = StopBits.One;
            srp.PortName = portname;

            // Open de poort
            srp.Open();
        }
        private void GetPortsInCombobox()
        {
            foreach (string port in SerialPort.GetPortNames())
            {
                cboPorts.Items.Add(port);
            }
        }

        public MainWindow()
        {
            InitializeComponent();






            var directInput = new DirectInput();
            joystickState = new JoystickState();
            var joystickGuid = Guid.Empty;
            foreach (var deviceInstance in directInput.GetDevices(DeviceType.Joystick, DeviceEnumerationFlags.AllDevices))
                joystickGuid = deviceInstance.InstanceGuid;
            joystick = new Joystick(directInput, joystickGuid);

            joystick.Acquire();

        }


        BackgroundWorker backgroundWorker;
        private void ini()
        {
            backgroundWorker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            backgroundWorker.DoWork += BackgroundWorkerOnDoWork;
            backgroundWorker.ProgressChanged += BackgroundWorkerOnProgressChanged;
            //  SerialP.Open();
            // SerialP.Write("lol");
            backgroundWorker.RunWorkerAsync();

            //  VeranderZoomSnelheid();
            //  System.Threading.Thread.Sleep(1500);
            // StopCommand();
        }
        private int testje;
        private int testje2;
        private int testje3;
        private int counter = 0;

        private void BackgroundWorkerOnDoWork(object sender, DoWorkEventArgs e)
        {
            while (joystick != null)
            {
                joystick.GetCurrentState(ref joystickState);

                backgroundWorker.ReportProgress(0);
                //Textbox1.Text = joystickState.X.ToString();
                System.Threading.Thread.Sleep(100);
            }
        }
        private void BackgroundWorkerOnProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            int X, Y, plus, min;
            X = 0;
            Y = 0;
            plus = 0;
            min = 0;

            if (counter == 0)
            {

                testje = joystickState.X;
                testje2 = joystickState.Y;
                testje3 = joystickState.Z;

            }
            counter++;

            if (testje - joystickState.X > 1500)
            {
                //Textbox1.Text = "LINKS";
                X = -1;
            }
            else if (testje - joystickState.X < -1500)
            {
                //Textbox1.Text = "RECHTS";
                X = 1;
            }
            else
            {
              //  Textbox1.Text = "";


            }

            if (testje2 - joystickState.Y > 1500)
            {
            //    Textbox2.Text = "OMHOOG";
                Y = 1;
            }
            else if (testje2 - joystickState.Y < -1500)
            {
          //      Textbox2.Text = "BENEDEN";
                Y = -1;
            }
            else
            {
          //      Textbox2.Text = "";

            }


            // 1 uitzoomen
            //2 inzoomen
       //     Textbox3.Text = "" + joystickState.Buttons[1] + " " + joystickState.Buttons[2];
            if (joystickState.Buttons[1]) min = 1;
            if (joystickState.Buttons[2]) plus = 1;

            checkformovement(X, Y, plus, min);



        }
        private void Window_Closed(object sender, EventArgs e)
        {
            srp.Close();
        }
        int previousX;
        int previousY;
        int previousplus;
        int previousmin;
        private void checkformovement(int X, int Y, int plus, int min)
        {
            if (X == 0 && previousX != 0)
            {
                //SEND STOP
                StopCommand();
            }

            previousX = X;
            if (X == -1)
            {
                //LINKS
                MoveLeft();
            }
            if (X == 1)
            {
                //RECHTS
                MoveRight();
            }


            if (Y == 0 && previousY != 0)
            {
                //SEND STOP
                StopCommand();
            }

            previousY = Y;
            if (Y == -1)
            {
                //BENEDEN
                MoveDown();
            }
            if (Y == 1)
            {
                //OMHOOG
                MoveUp();
            }

            if (plus == 0 && previousplus == 1)
            {
                //send STOP
                StopCommand();
            }
            previousplus = plus;
            if (plus == 1)
            {
                //zoom in
                ZoomIn();
            }
            if (min == 0 && previousmin == 1)
            {
                //send STOP
                StopCommand();
            }
            previousmin = min;
            if (min == 1)
            {
                //zoom uit
                ZoomOut();
            }



        }

        
        
      


    

     
        private void btnOmhoog_Click(object sender, RoutedEventArgs e)
        {
            MoveUp();
            Wait();
            StopCommand();
        }

        private void btnRechts_Click(object sender, RoutedEventArgs e)
        {
            MoveRight();
            Wait();
            StopCommand();
        }

        private void btnOmlaag_Click(object sender, RoutedEventArgs e)
        {
            MoveDown();
            Wait();
            StopCommand();
        }

        private void btnLinks_Click(object sender, RoutedEventArgs e)
        {
            MoveLeft();
            Wait();
            StopCommand();
        }

        private void btnZoomIn_Click(object sender, RoutedEventArgs e)
        {
            ZoomIn();
            Wait();
            StopCommand();
        }

        private void btnZoomUit_Click(object sender, RoutedEventArgs e)
        {
            ZoomOut();
            Wait();
            StopCommand();
        }

        private void Wait()
        {
            // Wacht even zodat de camera niet direct stopt met draaien
            System.Threading.Thread.Sleep(500);
        }

        private void MoveLeft()
        {
            byte[] arrBytes = new byte[7];
            srp.DiscardInBuffer();
            srp.DiscardOutBuffer();

            // Verstuur commando
            arrBytes[0] = 0xFF;
            arrBytes[1] = 0x01;
            arrBytes[2] = 0x00;
            arrBytes[3] = 0x04;
            arrBytes[4] = 0x20;
            arrBytes[5] = 0x00;
            arrBytes[6] = 0x25;
            srp.Write(arrBytes, 0, 7);
        }

        private void MoveRight()
        {
            byte[] arrBytes = new byte[7];
            srp.DiscardInBuffer();
            srp.DiscardOutBuffer();

            // Verstuur commando
            arrBytes[0] = 0xFF;
            arrBytes[1] = 0x01;
            arrBytes[2] = 0x00;
            arrBytes[3] = 0x02;
            arrBytes[4] = 0x20;
            arrBytes[5] = 0x00;
            arrBytes[6] = 0x23;
            srp.Write(arrBytes, 0, 7);
        }

        private void MoveDown()
        {
            byte[] arrBytes = new byte[7];
            srp.DiscardInBuffer();
            srp.DiscardOutBuffer();

            // Verstuur commando
            arrBytes[0] = 0xFF;
            arrBytes[1] = 0x01;
            arrBytes[2] = 0x00;
            arrBytes[3] = 0x10;
            arrBytes[4] = 0x00;
            arrBytes[5] = 0x20;
            arrBytes[6] = 0x31;
            srp.Write(arrBytes, 0, 7);
        }

        private void MoveUp()
        {
            byte[] arrBytes = new byte[7];
            srp.DiscardInBuffer();
            srp.DiscardOutBuffer();

            // Verstuur commando
            arrBytes[0] = 0xFF;
            arrBytes[1] = 0x01;
            arrBytes[2] = 0x00;
            arrBytes[3] = 0x08;
            arrBytes[4] = 0x00;
            arrBytes[5] = 0x20;
            arrBytes[6] = 0x29;
            srp.Write(arrBytes, 0, 7);
        }

        private void ZoomIn()
        {
            byte[] arrBytes = new byte[7];
            srp.DiscardInBuffer();
            srp.DiscardOutBuffer();

            // Verstuur commando
            arrBytes[0] = 0xFF;
            arrBytes[1] = 0x01;
            arrBytes[2] = 0x00;
            arrBytes[3] = 0x20;
            arrBytes[4] = 0x00;
            arrBytes[5] = 0x00;
            arrBytes[6] = 0x21;
            srp.Write(arrBytes, 0, 7);
        }

        private void ZoomOut()
        {
            byte[] arrBytes = new byte[7];
            srp.DiscardInBuffer();
            srp.DiscardOutBuffer();

            // Verstuur commando
            arrBytes[0] = 0xFF;
            arrBytes[1] = 0x01;
            arrBytes[2] = 0x00;
            arrBytes[3] = 0x40;
            arrBytes[4] = 0x00;
            arrBytes[5] = 0x00;
            arrBytes[6] = 0x41;
            srp.Write(arrBytes, 0, 7);
        }

        private void StopCommand()
        {            
            byte[] arrStopCommando = new byte[7];
            arrStopCommando[0] = 0xFF;
            arrStopCommando[1] = 0x01;
            arrStopCommando[2] = 0x00;
            arrStopCommando[3] = 0x00;
            arrStopCommando[4] = 0x00;
            arrStopCommando[5] = 0x00;
            arrStopCommando[6] = 0x01;
          //  System.Threading.Thread.Sleep(500);
            srp.Write(arrStopCommando, 0, 7);
        }

      

     /*   private void sldZoomSnelheid_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            VeranderZoomSnelheid();
        }

        private void VeranderZoomSnelheid()
        {
            double zoomSnelheid = sldZoomSnelheid.Value;
            int zoomSnelheidInt = (int)zoomSnelheid;
            string zoomSnelheidHex =  zoomSnelheidInt.ToString("X");

            byte[] arrBytes = new byte[7];
            srp.DiscardInBuffer();
            srp.DiscardOutBuffer();

            // Verstuur commando
            arrBytes[0] = 0xFF;
            arrBytes[1] = 0x01;
            arrBytes[2] = 0x00;
            arrBytes[3] = 0x04;
            arrBytes[4] = 0x3F;
            arrBytes[5] = 0x00;
            arrBytes[6] = 0x44;
            srp.Write(arrBytes, 0, 7);
        }*/
    }
}
