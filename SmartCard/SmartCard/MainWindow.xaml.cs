﻿using System;
using System.Collections.Generic;
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

            // Beschikbare Readers
            returnCode = ModWinsCard.SCardListReaders(hContext, ReaderGroupBuff, ReaderListBuff, ref Readercount);

            //Creates the byte array to receive the name
            byte[] nameBytes = new byte[Readercount];

            //Gets the readers name
            returnCode = ModWinsCard.SCardListReaders(hContext, null, nameBytes, ref Readercount);

            //Decodify the readers name
            sCardName = System.Text.Encoding.ASCII.GetString(nameBytes, 0, Readercount).Replace("\0", "");

            string[] cardNames = sCardName.Split('0');
            foreach (string card in cardNames)
            {
                if (card == "")
                    return;

                string scard = card.Remove(card.Length - 1);
                lstReaders.Items.Add(scard);
             
            }
            
        }

        private void btnConnecteer_Click(object sender, RoutedEventArgs e)
        {
            ConnectReader(lstReaders.SelectedItem.ToString());
        }

        private void ConnectReader(String cardName)
        {
            // Connecteer met de Reader
            returnCode = ModWinsCard.SCardConnect(hContext, cardName, ModWinsCard.SCARD_SHARE_SHARED, ModWinsCard.SCARD_PROTOCOL_T0, ref hCard, ref Protocol);
        }

        private void btnStuurCommando_Click(object sender, RoutedEventArgs e)
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

        private void btnStuurCommandoSchrijf_Click(object sender, RoutedEventArgs e)
        {
            // Maak SendBuffer & ReceivedBuffer leeg
            Array.Clear(SendBuff, 0, 262);
            Array.Clear(RecvBuff, 0, 262);

            SendBuff[0] = 0xFF; // Start
            SendBuff[1] = 0xD0; // Command Instruction (card type)
            SendBuff[2] = 0x0; // Memory adres
            SendBuff[3] = 0x0; // Memory adres
            SendBuff[4] = 0x03;
            SendBuff[5] = 0x74;
            SendBuff[6] = 0x69;
            SendBuff[7] = 0x73;
            SendBuff[8] = 0x74;
            sendLength = 9;

            ioRequest.dwProtocol = Protocol;
            ioRequest.cbPciLength = 8;

            returnCode = ModWinsCard.SCardTransmit(hCard, ref ioRequest, ref SendBuff[0], sendLength, ref ioRequest, ref RecvBuff[0], ref ReceivedLength);
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            ModWinsCard.SCardEndTransaction(hCard, ModWinsCard.SCARD_LEAVE_CARD);
            ModWinsCard.SCardDisconnect(hCard, ModWinsCard.SCARD_UNPOWER_CARD);
            ModWinsCard.SCardReleaseContext(hContext);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SendBuff[0] = 0;
            //ModWinsCard.SCardTransmit(hCard, ioRequest, SendBuff[0])
        }
    }
}