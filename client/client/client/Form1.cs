using Microsoft.VisualBasic.Logging;
using System.Net.Sockets;
using System.Text;

namespace client
{
    public partial class Form1 : Form
    {
        bool terminating = false;
        bool connected = false;
        Socket clientSocket;
        bool awaitingAnswer = false;
        public Form1()
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);
            InitializeComponent();
            AnswerText.Enabled = false;
            SendButton.Enabled = false;
        }
        private void ConnectButton_Click(object sender, EventArgs e)
        {
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            string IP = IPText.Text;

            int portNum;
            if (Int32.TryParse(PortText.Text, out portNum))
            {
                try
                {
                    clientSocket.Connect(IP, portNum);
                    ConnectButton.Enabled = false;
                    DisconnectButton.Enabled = true;
                    AnswerText.Enabled = true;
                    SendButton.Enabled = true;
                    connected = true;
                    Logs.AppendText("Connection established...\n");


                    Thread receiveThread = new Thread(Receive);
                    receiveThread.Start();
                }
                catch
                {
                    Logs.AppendText("Problem occurred while connecting...\n");
                }
            }
            else
            {
                Logs.AppendText("Check the port\n");
            }
        }

        private void Receive()
        {
            
            {
                while (connected)
                {
                    try
                    {
                        Byte[] buffer = new Byte[64];
                        clientSocket.Receive(buffer);

                        string incomingMessage = Encoding.Default.GetString(buffer);
                        string test = incomingMessage.Substring(0, 16);
                        if (test == "TERMINATE CLIENT")
                        {
                            connected = false;
                            clientSocket.Close();
                            DisconnectButton.Enabled = false;
                            ConnectButton.Enabled = true;
                            AnswerText.Enabled = false;
                            SendButton.Enabled = false;
                            Logs.AppendText("Server: Game Ended \n");
                            break;
                        }
                        incomingMessage = incomingMessage.Trim('\0');
                        Logs.AppendText("Server: " + incomingMessage + "\n");
                    }
                    catch
                    {
                        if (!terminating)
                        {
                            Logs.AppendText("The server has disconnected\n");
                            ConnectButton.Enabled = true;
                        }

                        clientSocket.Close();
                        connected = false;
                    }
                }
            }
        }

        private void SendButton_Click(object sender, EventArgs e)
        {

        }

        private void Disconnect_Click(object sender, EventArgs e)
        {

        }
        private void Form1_FormClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            connected = false;
            terminating = true;
            Environment.Exit(0);
        }
    }
}