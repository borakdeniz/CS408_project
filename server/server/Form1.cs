using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;


namespace server
{
    public partial class Form1 : Form
    {
        Socket server_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        List<Socket> client_Socket = new List<Socket>();
        Dictionary<string, Socket> socketDictionary = new Dictionary<string, Socket>();
        List<string> nicknames = new List<string>(); //nickname list of the players
        bool terminating = false;
        public bool listening = false;
        int numOfClient = 2;  //maximum number of client is determined here.
        public bool connected = false;
        public string nameOfClient = "";
        private double sum = 0;  // total sum earning from questions
        private string correctAnswer;
        public Form1()
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            this.FormClosing += new FormClosingEventHandler(server_FormClosing);
            InitializeComponent();
        }

        private void server_FormClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            listening = false;
            terminating = true;

            Environment.Exit(0);
        }

        private void path_button_Click(object sender, EventArgs e)
        {

        }

        private void listen_button_Click(object sender, EventArgs e)
        {
            int port_Num;

            if (Int32.TryParse(port_textBox.Text, out port_Num))
            {
                
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, port_Num);
                server_Socket.Bind(endPoint);
                server_Socket.Listen(numOfClient); // socket listens as the max number of client
                listening = true;
                listen_button.Enabled = false;
                quiz_question_textbox.Enabled = false;

                Thread acceptThread = new Thread(Accept);
                acceptThread.Start();
                

            }
        }
        private void Accept()
        {
            while (listening)
            {
                try
                {
                    Socket newClient = server_Socket.Accept();
                    Byte[] buffer = new Byte[64];
                    newClient.Receive(buffer);
                    nameOfClient = Encoding.Default.GetString(buffer);
                    nameOfClient = nameOfClient.Substring(0, nameOfClient.IndexOf("\0"));

                    if (nicknames.Contains(nameOfClient))  //checking whether there is such a name in the nickname list.
                    {
                        richTextBox1.AppendText(nameOfClient + " is already in the game");
                        newClient.Close();
                    }
                    else
                    {
                        client_Socket.Add(newClient);   
                        nicknames.Add(nameOfClient);

                        richTextBox1.AppendText(nameOfClient + " is connected to the server");
                        Thread receiveThread = new Thread(() => Receive(newClient, nameOfClient));
                        receiveThread.Start();
                    }
                    

                }
                catch
                {
                    if (terminating)
                    {
                        listening = false;

                    }
                    else
                    {
                        richTextBox1.AppendText("Socket stopped working \n");
                        listen_button.Enabled = true;
                        listening = false;                      
                    }
                }
            }
        }
        private void Receive(Socket thisClient, string nameOf)
        {
            connected = true;
            Dictionary<Socket, string> revDictionary = socketDictionary.ToDictionary(pair => pair.Value, pair => pair.Key);
            string UsernameVar = revDictionary[thisClient];
            while (connected && !terminating)
            {
                try
                {
                    Byte[] buffer = new Byte[64];
                    thisClient.Receive(buffer);
                    string incomingMessage = Encoding.Default.GetString(buffer);
                    incomingMessage = incomingMessage.Substring(0, incomingMessage.IndexOf("\0"));
                    if(incomingMessage == correctAnswer)
                    {
                        Byte[] sendBuffer = new Byte[1024];
                        thisClient.Receive(sendBuffer);
                    }
                }
                catch
                {
                    if (!terminating)
                    {
                        richTextBox1.AppendText("Client: " + nameOf + " has left the game");
                    }
                    thisClient.Close();
                    client_Socket.Remove(thisClient);
                    nicknames.Remove(nameOf);
                    connected = false;
                    sum = 0;
                }
            }
        }
        private void checkAnswerEvent(object sender, EventArgs e)
        {

        }
        private void askQuestion(int qNum)
        {
            switch (qNum)
            {
                case 1:
                    richTextBox1.AppendText("How many times has DIEGO Maradona participated in World Cups as team captain?");
                    correctAnswer = "4";
                    break;
                case 2:
                    richTextBox1.AppendText("How many children does Madonna have?");
                    correctAnswer = "6";

                    break;
                case 3:
                    richTextBox1.AppendText("How many countries are there in the Asian continent?");
                    correctAnswer = "52";
                    break;
                case 4:
                    richTextBox1.AppendText("How many children does Bruce Lee have?");
                    correctAnswer = "2";
                    break;
                case 5:
                    richTextBox1.AppendText("How many liters of milk does a goat produce on average per year?");
                    correctAnswer = "1000";
                    break;
                case 6:
                    richTextBox1.AppendText("In which year did Turkish coffee enter on the UNESCO list?");
                    correctAnswer = "2013";
                    break;
                case 7:
                    richTextBox1.AppendText("How many known species of scorpions are poisonous?");
                    correctAnswer = "25";
                    break;
                case 8:
                    richTextBox1.AppendText("How many months, on average, does a person spend waiting for the traffic light to change from red to green??");
                    correctAnswer = "6";
                    break;
                case 9:
                    richTextBox1.AppendText("How many months, on average, does a person spend waiting for the traffic light to change from red to green??");
                    correctAnswer = "15";
                    break;
                case 10:
                    richTextBox1.AppendText("How many months, on average, does a person spend waiting for the traffic light to change from red to green??");
                    correctAnswer = "26";
                    break;
                case 11:
                    richTextBox1.AppendText("How many months, on average, does a person spend waiting for the traffic light to change from red to green??");
                    correctAnswer = "92";
                    break;
            }
        }
    }

}
