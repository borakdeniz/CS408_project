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
        List<string> nicknames = new List<string>(); //nickname list of the players
        bool terminating = false;
        public bool listening = false;
        int numOfClient = 2;  //maximum number of client is determined here.
        public bool connected = false;
        public string nameOfClient = "";
        private double sum = 0;  // total sum earning from questions
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

                }
            }
        }
        private void Receive(Socket thisClient, string nameOf)
        {
            connected = true;
            while(connected && !terminating)
            {
                try
                {
                    
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
    }

}
