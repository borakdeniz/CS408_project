using Microsoft.VisualBasic.Logging;
using System;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace client
{
    public partial class Form1 : Form
    {
        bool terminating = false;
        bool connected = false;
        Socket clientSocket;
        string name = "";
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
                    //connecting to the server
                    name = NameText.Text;
                    clientSocket.Connect(IP, portNum);
                    ConnectButton.Enabled = false;
                    DisconnectButton.Enabled = true;
                    AnswerText.Enabled = true;
                    SendButton.Enabled = true;
                    connected = true;
                    Logs.AppendText("Connection established...\n");
                    if (name != "" && name.Length <= 64)
                    {
                        Byte[] buffer = Encoding.Default.GetBytes(name);
                        clientSocket.Send(buffer);
                    }

                    //call recieve function to process the questions and send answers
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

                        //Parse the incomming messages
                        string incomingMessage = Encoding.Default.GetString(buffer);    
                        string test = incomingMessage.Substring(0, 16);                 
                                                                                        
                        //If the message is "TERMINATE CLIENT" terminate the connection to the server
                        if (test == "TERMINATE CLIENT")
                        {
                            connected = false;
                            string answer = name + ": Has Disconnected\n";
                            if (answer != "" && answer.Length <= 64)
                            {
                                buffer = Encoding.Default.GetBytes(answer);
                                clientSocket.Send(buffer);
                            }
                            clientSocket.Close();
                            DisconnectButton.Enabled = false;
                            ConnectButton.Enabled = true;
                            AnswerText.Enabled = false;
                            SendButton.Enabled = false;
                            Logs.AppendText("Server: Game Ended \n");
                            break;
                        }

                        //get rid of empty spaces and print the message to logs
                        incomingMessage = incomingMessage.Trim('\0');
                        Logs.AppendText("Server: " + incomingMessage + "\n");
                        SendButton.Enabled = true;
                    }
                    catch
                    {
                        if (!terminating)
                        {
                            Logs.AppendText("The server has disconnected\n");
                            ConnectButton.Enabled = true;
                            string answer = name + ": Has Disconnected\n";
                            if (answer != "" && answer.Length <= 64)
                            {
                                Byte[] buffer = Encoding.Default.GetBytes(answer);
                                clientSocket.Send(buffer);
                            }
                        }

                        clientSocket.Close();
                        connected = false;
                    }
                }
            }
        }

        private void SendButton_Click(object sender, EventArgs e)
        {
            //get rid of empty spaces and send the answer to the server
            string answer = AnswerText.Text;
            answer.Trim('\0');
            Logs.AppendText("Answer: " + answer + "\n");
            answer = name + ": " + answer;
            if (answer != "" && answer.Length <= 64)
            {
                Byte[] buffer = Encoding.Default.GetBytes(answer);
                clientSocket.Send(buffer);
            }
            AnswerText.Text = "";
            SendButton.Enabled = false;
        }

        private void Disconnect_Click(object sender, EventArgs e)
        {
            //disconnect from the server and print a message accordingly
            connected = false;
            terminating = true;

            string answer = name + ": Has Disconnected\n";
            if (answer != "" && answer.Length <= 64)
            {
                Byte[] buffer = Encoding.Default.GetBytes(answer);
                clientSocket.Send(buffer);
            }
            clientSocket.Close();
            DisconnectButton.Enabled = false;
            ConnectButton.Enabled = true;
            AnswerText.Enabled = false;
            SendButton.Enabled = false;
            Logs.AppendText("Server: Game Ended \n");
        }
        private void Form1_FormClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            connected = false;
            terminating = true;
            Environment.Exit(0);
        }
    }
}