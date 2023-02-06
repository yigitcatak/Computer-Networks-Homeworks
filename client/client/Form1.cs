using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace client
{
    public partial class Form1 : Form
    {
        bool terminating = false, connected = false;
        Socket clientSocket;

        public Form1()
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);
            InitializeComponent();
        }

        private void button_connect_Click(object sender, EventArgs e)
        {
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            string IP = textBox_ip.Text;

            int portNum;
            if(Int32.TryParse(textBox_port.Text, out portNum))
            {
                try
                {
                    clientSocket.Connect(IP, portNum);
                    string name = textBox_name.Text;
                    if (name != "" && name.Length <= 64)
                    {
                        Byte[] buffer_name = Encoding.Default.GetBytes(name);
                        clientSocket.Send(buffer_name);
                    }

                    Byte[] buffer_server_response = new Byte[8];

                    clientSocket.Receive(buffer_server_response);
                    string server_response = Encoding.Default.GetString(buffer_server_response);
                    server_response = server_response.Substring(0, server_response.IndexOf("\0"));
                       
                    if (server_response == "Yes")
                    {
                        textBox_ip.Enabled = textBox_name.Enabled = textBox_port.Enabled = button_connect.Enabled = false;
                        connected = true;

                        logs.AppendText("Connected to the server!\n\n");
                        Thread receiveThread = new Thread(Receive);
                        receiveThread.Start();
                    }                
                    else if (server_response == "No")
                    {
                        logs.AppendText("Name is already taken. Please try another name!\n");
                    }
                    else if (server_response == "Game")
                    {
                        textBox_ip.Enabled = textBox_name.Enabled = textBox_port.Enabled = button_connect.Enabled = false;
                        connected = true;

                        logs.AppendText("Connected to the server!\n\n");
                        Thread receiveThread = new Thread(Receive);
                        receiveThread.Start();
                        logs.AppendText("Game already started, Wait until next game!\n");
                    }
                }
                catch
                {
                    logs.AppendText("Could not connect to the server!\n");
                }
            }
            else
            {
                logs.AppendText("Please enter a valid number for port!\n");
            }
        }

        private void Receive()
        {
            while(connected)
            {
                try
                {
                    Byte[] buffer_question = new Byte[1024];
                    clientSocket.Receive(buffer_question);
                    string incomingMessage = Encoding.Default.GetString(buffer_question);
                    incomingMessage = incomingMessage.Substring(0, incomingMessage.IndexOf("\0"));
                    if (incomingMessage.Contains("\x4"))
                    {
                        incomingMessage = incomingMessage.Substring(0, incomingMessage.IndexOf("\x4"));
                        logs.AppendText(incomingMessage + "\nGame Ended.\n\n");
                    }
                    else
                    {
                        logs.AppendText(incomingMessage + "\n");
                        textBox_message.Enabled = button_send.Enabled = true;
                    }
                }
                catch
                {
                    if (!terminating)
                    {
                        logs.AppendText("The server has disconnected\n\n");
                        textBox_ip.Enabled = textBox_name.Enabled = textBox_port.Enabled = button_connect.Enabled = true;
                        textBox_message.Enabled = button_send.Enabled = false;
                    }
                    connected = false;
                    clientSocket.Close();
                }

            }
        }

        private void Form1_FormClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            connected = false;
            terminating = true;
            Environment.Exit(0);
        }

        private void button_send_Click(object sender, EventArgs e)
        {
            string message = textBox_message.Text;
            int answer;

            if(message != "" && message.Length <= 64 && Int32.TryParse(message, out answer))
            {
                Byte[] buffer_message = Encoding.Default.GetBytes(message);
                clientSocket.Send(buffer_message);
                logs.AppendText("Answered: " + message + "\n\n");
                textBox_message.Enabled = button_send.Enabled = false;
            }
            else
            {
                logs.AppendText("Enter a valid integer as answer!\n");
            }

        }

        private void textBox_name_TextChanged(object sender, EventArgs e)
        {

        }

        // taken directly from stackoverflow
        private void logs_TextChanged(object sender, EventArgs e)
        {
            // set the current caret position to the end
            logs.SelectionStart = logs.Text.Length;
            // scroll it automatically
            logs.ScrollToCaret();
        }
    }
}
