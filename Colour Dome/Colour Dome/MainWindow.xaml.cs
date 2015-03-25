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
            
        }

        private void btnRechts_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnOmlaag_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnLinks_Click(object sender, RoutedEventArgs e)
        {
            srp.Write("FF0100043F00");
        }
    }
}
