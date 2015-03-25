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

namespace Colour_Dome
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SerialPort srp = new SerialPort();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            GetPortsInCombobox();
        }

        private void GetPortsInCombobox()
        {
            foreach (string port in SerialPort.GetPortNames())
            {
                cboPorts.Items.Add(port);
            }
        }

        private void btnInit_Click(object sender, RoutedEventArgs e)
        {
            OpenPort(Convert.ToString(cboPorts.SelectedItem));

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
            System.Threading.Thread.Sleep(500);
            srp.Write(arrStopCommando, 0, 7);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            srp.Close();
        }
    }
}
