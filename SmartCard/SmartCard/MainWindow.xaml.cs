using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SmartCard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int returnCode = 0, Protocol = 0, hContext = 0, hCard = 0, Readercount = 0;
        byte[] ReaderListBuff = new byte[262];
        byte[] ReaderGroupBuff;
        bool diFlag;
        ModWinsCard.SCARD_IO_REQUEST ioRequest;
        int sendLength, ReceivedLength;
        byte[] RecvBuff = new byte[262];
        byte[] SendBuff = new byte[262];
        string sCardName;
        System.Text.ASCIIEncoding encoding = new ASCIIEncoding();
        private static Timer aTimer;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadReaderInList();
        }

        private void LoadReaderInList()
        {
            // Testen of connectie lukt
            returnCode = ModWinsCard.SCardEstablishContext(ModWinsCard.SCARD_SCOPE_USER, 0, 0, ref hContext);
            //ZONIET THROW ERROR
            if (returnCode != ModWinsCard.SCARD_S_SUCCESS)
                throw new Exception(returnCode.ToString());
            int nameSize = 0;
            // Beschikbare Readers
            returnCode = ModWinsCard.SCardListReaders(hContext, null, null, ref nameSize);

            //Creates the byte array to receive the name
            byte[] nameBytes = new byte[nameSize];

            //Gets the readers name
            returnCode = ModWinsCard.SCardListReaders(hContext, null, nameBytes, ref nameSize);

            //Decodify the readers name
            sCardName = System.Text.Encoding.ASCII.GetString(nameBytes, 0, nameSize).Replace("\0", "");
            lstReaders.Items.Add(sCardName);
            
        }

        private void btnConnecteer_Click(object sender, RoutedEventArgs e)
        {
            ConnectReader(lstReaders.SelectedItem.ToString());
        }

        private void ConnectReader(String cardName)
        {
            // Connecteer met de Reader
            returnCode = ModWinsCard.SCardConnect(hContext, cardName, ModWinsCard.SCARD_SHARE_SHARED, ModWinsCard.SCARD_PROTOCOL_T0, ref hCard, ref Protocol);

            SelectCardType();
            ReadPresentationErrorCounter();
        }

        #region I2C Protocol Methodes


        private void StartTransactieI2C()
        {
            SendBuff[0] = 0xFF; // Start (Comand Class)
            SendBuff[1] = 0xA4; // Commando Instruction: Vraag card type op
            SendBuff[2] = 0x0; // 
            SendBuff[3] = 0x0;
            SendBuff[4] = 0x01; // Lengte data -> 1 byte
            SendBuff[5] = 0x1; // I2C card
            sendLength = 6;

            ioRequest.dwProtocol = Protocol;
            ioRequest.cbPciLength = 8;

            returnCode = ModWinsCard.SCardTransmit(hCard, ref ioRequest, ref SendBuff[0], sendLength, ref ioRequest, ref RecvBuff[0], ref ReceivedLength);
            txbError.Text = ModWinsCard.GetScardErrMsg(returnCode);
        }
        private void LeesCommandoI2C()
        {
            // Maak SendBuffer & ReceivedBuffer leeg
            Array.Clear(SendBuff, 0, 262);
            Array.Clear(RecvBuff, 0, 262);

            SendBuff[0] = 0xFF; // Start (Comand Class)
            SendBuff[1] = 0xB0; // Commando Instruction: Lees card
            SendBuff[2] = 0x01;
            SendBuff[3] = 0x08;
            SendBuff[4] = 0xFF; // Lengte data -> 1 byte
            sendLength = 5;

            ioRequest.dwProtocol = Protocol;
            ioRequest.cbPciLength = 8;

            ReceivedLength = RecvBuff.Length;

            returnCode = ModWinsCard.SCardTransmit(hCard, ref ioRequest, ref SendBuff[0], sendLength, ref ioRequest, ref RecvBuff[0], ref ReceivedLength);

            //Decodify the readers name
            string received = System.Text.Encoding.ASCII.GetString(RecvBuff, 0, ReceivedLength).Replace("\0", "");
            txtIngelezen.Text = received;

            ModWinsCard.SCardEndTransaction(hCard, ModWinsCard.SCARD_LEAVE_CARD);
        }

        private void SchrijfCommandoI2C()
        {
            // Maak SendBuffer & ReceivedBuffer leeg
            Array.Clear(SendBuff, 0, 262);
            Array.Clear(RecvBuff, 0, 262);

            SendBuff[0] = 0xFF; // Start (Comand Class)
            SendBuff[1] = 0xD0; // Commando Instruction: Schrijf card
            SendBuff[2] = 0x00;
            SendBuff[3] = 0x00;
            SendBuff[4] = 0x04; // Lengte data -> 1 byte
            SendBuff[5] = 0x74;
            SendBuff[6] = 0x74;
            SendBuff[7] = 0x73;
            SendBuff[8] = 0x74;
            sendLength = 9;

            ReceivedLength = RecvBuff.Length;

            ioRequest.dwProtocol = Protocol;
            ioRequest.cbPciLength = 8;

            returnCode = ModWinsCard.SCardTransmit(hCard, ref ioRequest, ref SendBuff[0], sendLength, ref ioRequest, ref RecvBuff[0], ref ReceivedLength);
        }

        #endregion


        private void btnStuurCommandoSchrijf_Click(object sender, RoutedEventArgs e)
        {
            // I2C Protocol (Kaartje zonder chip)
            //StartTransactieI2C();
            //LeesCommandoI2C();
            //StopTransactie();

            // SLE4442 Kaartje
            string text = Lees(0x33) + " / "+ Lees(0x53); 
            txtIngelezen.Text = text;
        }

        private void SelectCardType()
        {
            StartTransactieSLE();

            SendBuff[0] = 0xFF; // Start (Comand Class)
            SendBuff[1] = 0xA4; // Commando Instruction: Instruction card type
            SendBuff[2] = 0x0; // Memory adres 
            SendBuff[3] = 0x0; // Memory adres 
            SendBuff[4] = 0x01; // Memory Length
            SendBuff[5] = 0x06; // SLE4442 card
            sendLength = 6;

            ioRequest.dwProtocol = Protocol;
            ioRequest.cbPciLength = 8;

            ReceivedLength = RecvBuff.Length;

            returnCode = ModWinsCard.SCardTransmit(hCard, ref ioRequest, ref SendBuff[0], sendLength, ref ioRequest, ref RecvBuff[0], ref ReceivedLength);

            StopTransactie();
        }

        private void StartTransactieSLE()
         {
            // Start Transactie
             returnCode = ModWinsCard.SCardBeginTransaction(hCard);
         }

        private void ReadPresentationErrorCounter()
        {
            StartTransactieSLE();

            // Maak SendBuffer & ReceivedBuffer leeg
            Array.Clear(SendBuff, 0, 262);
            Array.Clear(RecvBuff, 0, 262);

            SendBuff[0] = 0xFF; // Start (Comand Class)
            SendBuff[1] = 0xB1; // Commando Instruction: Vraag card type op
            SendBuff[2] = 0x00; // Memory adres
            SendBuff[3] = 0x00; // Memory adres
            SendBuff[4] = 0x04; // Memory Length
            sendLength = 5;

            ReceivedLength = RecvBuff.Length & 0x9000;

            ioRequest.dwProtocol = Protocol;
            ioRequest.cbPciLength = 8;

            returnCode = ModWinsCard.SCardTransmit(hCard, ref ioRequest, ref SendBuff[0], sendLength, ref ioRequest, ref RecvBuff[0], ref ReceivedLength);
            txbError.Text = ModWinsCard.GetScardErrMsg(returnCode);

            StopTransactie();
        }

        private void PresentCodeMemoryCard()
        {
            StartTransactieSLE();

            // Maak SendBuffer & ReceivedBuffer leeg
            Array.Clear(SendBuff, 0, 262);
            Array.Clear(RecvBuff, 0, 262);

            SendBuff[0] = 0xFF; // Start (Comand Class)
            SendBuff[1] = 0x20; // Commando Instruction: Vraag card type op
            SendBuff[2] = 0x00; // Memory adres
            SendBuff[3] = 0x00; // Memory adres
            SendBuff[4] = 0x03; // Memory Length
            SendBuff[5] = 0xFF; // PIN Code
            SendBuff[6] = 0xFF; // PIN Code
            SendBuff[7] = 0xFF; // PIN Code
            sendLength = 8;

            ioRequest.dwProtocol = Protocol;
            ioRequest.cbPciLength = 8;

            ReceivedLength = RecvBuff.Length;

            returnCode = ModWinsCard.SCardTransmit(hCard, ref ioRequest, ref SendBuff[0], sendLength, ref ioRequest, ref RecvBuff[0], ref ReceivedLength);
            txbError.Text = ModWinsCard.GetScardErrMsg(returnCode);
        }

        private string Lees(byte memoryAdres)
        {
            StartTransactieSLE();

            // Maak SendBuffer & ReceivedBuffer leeg
            Array.Clear(SendBuff, 0, 262);
            Array.Clear(RecvBuff, 0, 262);

            SendBuff[0] = 0xFF; // Start (Comand Class)
            SendBuff[1] = 0xB0; // Commando Instruction: Lees card
            SendBuff[2] = 0x00; // Vanaf byte 32 beginnen lezen (Alles daarvoor is Chip Coding, Application ID)
            SendBuff[3] = memoryAdres; 
            SendBuff[4] = Convert.ToByte(20); // Lengte data
            sendLength = 5;

            ioRequest.dwProtocol = Protocol;
            ioRequest.cbPciLength = 8;

            ReceivedLength = RecvBuff.Length;

            returnCode = ModWinsCard.SCardTransmit(hCard, ref ioRequest, ref SendBuff[0], sendLength, ref ioRequest, ref RecvBuff[0], ref ReceivedLength);

            //Decodify the readers name
            string received = encoding.GetString(RecvBuff).Substring(0, 0x20);

            StopTransactie();
            return received;
        }

        private void btnSchrijf_Click(object sender, RoutedEventArgs e)
        {
            // I2C Protocol (Kaartje zonder chip)
            //StartTransactieI2C();
            //SchrijfCommandoI2C();
            //StopTransactie();

            // SLE4442 Kaartje
            Schrijf(encoding.GetBytes(txtSchrijf.Text),0x53);
        }

        private void Schrijf(byte[] karakters, byte msb)
        {
            StartTransactieSLE();

            byte length = Convert.ToByte(karakters.Length);

            // Maak SendBuffer & ReceivedBuffer leeg
            Array.Clear(SendBuff, 0, 262);
            Array.Clear(RecvBuff, 0, 262);

            SendBuff[0] = 0xFF;  // Start (Comand Class)
            SendBuff[1] = 0xD0; // Commando Instruction: Schrijf op card
            SendBuff[2] = 0x00; // Memory adres
            SendBuff[3] = msb; // Memory adres
            SendBuff[4] = length; // Memory Length

            for (int i = 5; i < length + 5; i++)
            {
                SendBuff[i] = karakters[i - 5];
            }

            sendLength = length + 5;

            ioRequest.dwProtocol = Protocol;
            ioRequest.cbPciLength = 8;

            ReceivedLength = RecvBuff.Length;

            returnCode = ModWinsCard.SCardTransmit(hCard, ref ioRequest, ref SendBuff[0], sendLength, ref ioRequest, ref RecvBuff[0], ref ReceivedLength);

            StopTransactie();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            PresentCodeMemoryCard();
            SchrijfDatumEnUur();
            StartBackgroundWorker();
        }

        private void StartBackgroundWorker()
        {
            BackgroundWorker backgroundWorker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            backgroundWorker.DoWork += backgroundWorker_DoWork;
            backgroundWorker.ProgressChanged += backgroundWorker_ProgressChanged;
            backgroundWorker.RunWorkerAsync();
        }

        void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = (BackgroundWorker)sender;
            aTimer = new System.Timers.Timer(3000);
            aTimer.Elapsed += aTimer_Elapsed;
            aTimer.Enabled = true;
        }

        void aTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // Zorgt ervoor dat de kaart niet gewist wordt -> Om de 3 seconden lezen
            string text = Lees(0x33);
        }

        void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            
        }


        private void SchrijfDatumEnUur()
        {
            string huidigeTijd = DateTime.Now.ToString(@"dd/MM/yy H:mm:ss");
            Schrijf(encoding.GetBytes(huidigeTijd),0x33);
        }

        private void StopTransactie()
        {
            ModWinsCard.SCardEndTransaction(hCard, ModWinsCard.SCARD_LEAVE_CARD);
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            ModWinsCard.SCardDisconnect(hCard, ModWinsCard.SCARD_UNPOWER_CARD);
            ModWinsCard.SCardReleaseContext(hContext);
        }

        private void btnSchrijfNaarSmartcard_Click(object sender, RoutedEventArgs e)
        {
            SchrijfNaamEnAdresGebruiker();
        }

        private void SchrijfNaamEnAdresGebruiker()
        {
            
            //Schrijf(encoding.GetBytes(text + "/" + txtNaamGebruiker.Text + "/" + txtAdresGebruiker.Text));
            Schrijf(encoding.GetBytes(txtNaamGebruiker.Text + " / " + txtAdresGebruiker.Text),0x53);
        } 

        private void ToonInhoudSmartcard()
        {
            string text = Lees(0x33) + " / " + Lees(0x53);
            txtInhoudSmartcard.Text = text;
        }

        private void btnToonInhoud_Click(object sender, RoutedEventArgs e)
        {
            ToonInhoudSmartcard();
        }
    }
    
}
