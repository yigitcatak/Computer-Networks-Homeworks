using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace server
{
    public partial class Form1 : Form
    {
        Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        List<Socket> clientSockets = new List<Socket>(), waitingClientSockets = new List<Socket>();
        List<string> clientNames = new List<string>(), waitingClientNames = new List<string>(), removedPlayers = new List<string>(), namesToRemove = new List<string>(), questions = new List<string>(), playerNames = new List<string>();
        List<int> answers = new List<int>();

        List<bool> isAnswered;
        List<int> correctness, userAnswers;
        List<double> points;

        int game_length = 0, question_count = 0, current_question_index = 0;
        bool start_game = false, game_ended = false, terminating = false, listening = false, updating = false;

        public Form1()
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);
            InitializeComponent();
        }

        private void Reinitialize()
        {
            playerNames.Clear();
            isAnswered.Clear();
            removedPlayers.Clear();

            for (int i = 0; i < waitingClientNames.Count(); i++)
            {
                clientNames.Add(waitingClientNames[i]);
                clientSockets.Add(waitingClientSockets[i]);
            }

            questions.Clear();
            answers.Clear();
            waitingClientNames.Clear();
            waitingClientNames.Clear();
            question_count = current_question_index = 0;
            start_game = game_ended = false;
            updating = false;
        }

        private void button_listen_Click(object sender, EventArgs e)
        {
            int serverPort, numQuestions;
            IPEndPoint endPoint;
            Thread acceptThread, gameMaster;

            if (Int32.TryParse(textBox_port.Text, out serverPort))
            {
                if (Int32.TryParse(textBox_num.Text, out numQuestions))
                { 
                    if (numQuestions > 0)
                    {
                        logs.AppendText("The game will be played with " + numQuestions + " questions.\n");
                        game_length = numQuestions;

                        endPoint = new IPEndPoint(IPAddress.Any, serverPort);
                        serverSocket.Bind(endPoint);
                        serverSocket.Listen(3);

                        listening = true;
                        textBox_port.Enabled = textBox_num.Enabled = button_start.Enabled = false;

                        acceptThread = new Thread(Accept);
                        gameMaster = new Thread(Game);
                        acceptThread.Start();
                        gameMaster.Start();
                        logs.AppendText("Started listening on port: " + serverPort + "\n");
                    }
                    else
                    {
                        logs.AppendText("Please enter a valid number for number of questions!\n");
                    }
                }
                else
                {
                    logs.AppendText("Please enter a valid number for number of questions!\n");
                }
            }
            else
            {
                logs.AppendText("Please enter a valid number for port!\n");
            }
        }

        private void buttonStartGame_Click(object sender, EventArgs e)
        {
            if (clientNames.Count() >= 2)
            {
                LoadQuestions();
                start_game = true;
            }
            else
            {
                logs.AppendText("Not enough players to start the game.\n");
            }
        }

        private void Accept()
        {
            Thread receiveThread;
            Socket newClientHandler;
            Byte[] buffer_name, buffer_reply;
            string input_name;
            while (!terminating && listening)
            {
                while (updating) ; // acts like a mutex, prevents operation during state reset
                try
                {
                    newClientHandler = serverSocket.Accept();
                    buffer_name = new Byte[64];
                    buffer_reply = new Byte[8];
                    newClientHandler.Receive(buffer_name);
                    input_name = Encoding.Default.GetString(buffer_name);
                    input_name = input_name.Substring(0, input_name.IndexOf("\0"));

                    if (start_game == false)
                    {
                        if (clientNames.Contains(input_name) == false)
                        {
                            buffer_reply = Encoding.Default.GetBytes("Yes");
                            newClientHandler.Send(buffer_reply);
                            clientSockets.Add(newClientHandler);
                            logs.AppendText("Client \"" + input_name + "\" is connected.\n");
                            receiveThread = new Thread(() => Receive(newClientHandler, input_name));
                            receiveThread.Start();
                            clientNames.Add(input_name);
                        }
                        else
                        {
                            buffer_reply = Encoding.Default.GetBytes("No");
                            newClientHandler.Send(buffer_reply);
                            newClientHandler.Disconnect(reuseSocket: false);
                            logs.AppendText("A client tried to connect with already existing name. Connection failed.\n");
                        }
                    }
                    else
                    {
                        buffer_reply = Encoding.Default.GetBytes("Game");
                        newClientHandler.Send(buffer_reply);
                        waitingClientSockets.Add(newClientHandler);
                        logs.AppendText("Client \"" + input_name + "\" is connected and waiting.\n");
                        receiveThread = new Thread(() => Receive(newClientHandler, input_name));
                        receiveThread.Start();
                        waitingClientNames.Add(input_name);
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
                        logs.AppendText("The server socket stopped working.\n");
                    }
                }
            }
        }

        private void Receive(Socket thisClient, string thisName) 
        {
            Byte[] buffer;
            string incomingMessage;
            bool connected = true;
            int index;

            while(connected && !terminating)
            {
                while (updating) ;
                try
                {
                    buffer = new Byte[1024]; // I have checked the web for freeing up the memmory here but they say .NET garbage collector does a good enough job
                    thisClient.Receive(buffer);

                    incomingMessage = Encoding.Default.GetString(buffer);
                    incomingMessage = incomingMessage.Substring(0, incomingMessage.IndexOf("\0"));

                    index = clientNames.FindIndex(x => x == thisName);
                    userAnswers[index] = Int32.Parse(incomingMessage);
                    isAnswered[index] = true;
                }
                catch
                {
                    if(!terminating)
                    {
                        logs.AppendText("Client \"" + thisName + "\" has disconnected.\n");
                        namesToRemove.Add(thisName);
                        clientSockets.Remove(thisClient);
                        clientNames.Remove(thisName);
                        connected = false;
                    }
                    thisClient.Disconnect(reuseSocket: false);
                }
            }
        }
        private void Game()
        {
            while (!terminating)
            {
                while (!start_game) ; // spin endlesly until game starts

                foreach (string name in clientNames) // create a deepcopy of the clientNames for the leaderboard
                {
                    playerNames.Add(name);
                }

                // allocate the lists for required variables for the game based on player count
                correctness = new List<int>(new int[playerNames.Count()]);
                points = new List<double>(new double[playerNames.Count()]);
                isAnswered = new List<bool>(new bool[playerNames.Count()]);
                userAnswers = new List<int>(new int[playerNames.Count()]);

                namesToRemove.Clear();

                while (!game_ended && (clientSockets.Count() >= 2))
                {
                    if (current_question_index < game_length)
                    {
                        logs.AppendText("Sent question " + (current_question_index+1) + ".\n");
                        SendPrompt(current_question_index + 1 + ") " + questions[current_question_index % question_count]); // send the next question
                        if (WaitAnswer())
                        {
                            // remove the players who left
                            for (int i = 0; i < namesToRemove.Count(); i++)
                            {
                                int index = playerNames.FindIndex(x => x == namesToRemove[i]);
                                correctness.RemoveAt(index);
                                points.RemoveAt(index);
                                isAnswered.RemoveAt(index);
                                userAnswers.RemoveAt(index);
                                playerNames.RemoveAt(index);
                                removedPlayers.Add(namesToRemove[i]);
                            }
                            namesToRemove.Clear();

                            for (int i = 0; i < playerNames.Count(); i++)
                            {
                                isAnswered[i] = false;
                                correctness[i] = Math.Abs(answers[current_question_index % question_count] - userAnswers[i]);
                            }

                            // this finds the indices of all elements that are equal to the minimum (taken from SO)
                            int[] minimums = correctness.Select((b, i) => b == correctness.Min() ? i : -1).Where(i => i != -1).ToArray();


                            double p = 1.0 / minimums.Count();
                            foreach (int i in minimums)
                            {
                                points[i] += p;
                            }
                            
                            SendPrompt("Correct answer is: " + answers[current_question_index % question_count]);
                            PrintBoard();
                            current_question_index += 1;
                        }
                        // remove the players who left
                        for (int i = 0; i < namesToRemove.Count(); i++)
                        {
                            int index = playerNames.FindIndex(x => x == namesToRemove[i]);
                            correctness.RemoveAt(index);
                            points.RemoveAt(index);
                            isAnswered.RemoveAt(index);
                            userAnswers.RemoveAt(index);
                            removedPlayers.Add(namesToRemove[i]);
                        }
                        namesToRemove.Clear();
                    }

                    if (current_question_index == game_length || game_ended)
                    {
                        // remove the players who left
                        for (int i = 0; i < namesToRemove.Count(); i++)
                        {
                            int index = playerNames.FindIndex(x => x == namesToRemove[i]);
                            correctness.RemoveAt(index);
                            points.RemoveAt(index);
                            isAnswered.RemoveAt(index);
                            userAnswers.RemoveAt(index);
                            removedPlayers.Add(namesToRemove[i]);
                        }
                        namesToRemove.Clear();

                        game_ended = updating = true;
                        logs.AppendText("Game ended.\n");
                        Results();
                        break;
                    }
                }
            }
        }

        private void PrintBoard()
        {
            string prompt = "Leaderboard:" + "\n";

            // create a key-value pair list for points-name and order it to print a descending order leaderboard
            List<KeyValuePair<double, string>> leaderboard_merged = Enumerable.Range(0, points.Count).Select(i => new KeyValuePair<double, string>(points[i], playerNames[i])).ToList();
            leaderboard_merged = leaderboard_merged.OrderByDescending(x => x.Key).ToList(); 
            
            for (int i = 0; i < leaderboard_merged.Count(); i++)
            {
                prompt += leaderboard_merged[i].Value + ": " + leaderboard_merged[i].Key + "\n";
            }
            foreach (string name in removedPlayers)
            {
                prompt += name + ": " + 0 + "\n";
            }
            SendPrompt(prompt + "\n");
        }
        private void Results()
        {
            string prompt = "";
            if (clientSockets.Count() == 1) // There is only 1 player, left declare them as the winner
            {
                int index = playerNames.FindIndex(x => x == clientNames[0]);
                prompt += "Other players disconnected.\nWinner is " + playerNames[index] + " with " + points[index] + " points.";
            }
            else
            {
                int[] maximums = points.Select((b, i) => b == points.Max() ? i : -1).Where(i => i != -1).ToArray();
                if (maximums.Count() == 1) // if only 1 max pointsexist, declare winner
                    prompt += "Winner is " + playerNames[maximums[0]] + " with " + points[maximums[0]] + " points.";
                
                else // it is a tie, declare winners
                {
                    prompt += "It is a tie.\nWinners are ";
                    foreach (int i in maximums)
                    {
                        prompt += playerNames[i] + ", ";
                    }
                    prompt = prompt.Substring(0, prompt.Length - 2) + ".";
                }
            }
            logs.AppendText(prompt+"\n");
            SendPrompt(prompt+"\x4"); // send End of Transmission character at the end of results, so client can check it and disconnect
            Reinitialize(); // go to initial state for the next game
        }

        private bool WaitAnswer()
        {
            // spin endlesly until all clients send an answer
            int answer_count;
            while (clientSockets.Count() > 1) { // if all players but one quit, waiting will abort
                answer_count = 0;
                foreach (bool temp in isAnswered.ToList()) 
                {
                    if (temp == true)
                    {
                        answer_count++;
                    }
                }
                if (answer_count == clientSockets.Count()) // if everyone currently remaining in the game answers the question, return true that answering is complete
                {
                    return true;
                }
            }
            // if there are less than or equal to 1 people, end the game
            game_ended = updating = true;  
            return false;
        }

        private void SendPrompt(string prompt)
        {
            Byte[] buffer_prompt = Encoding.Default.GetBytes(prompt);
            //logs.AppendText("Sent: " + prompt);
            foreach (Socket client in clientSockets)
            {
                client.Send(buffer_prompt);
            }
        }

        private void LoadQuestions() 
        {
            bool isQuestion = true;
            foreach (string line in System.IO.File.ReadLines("questions.txt"))
            {
                if (isQuestion == true) // every first line is question
                {
                    questions.Add(line);
                    question_count += 1;
                    isQuestion = false;
                }
                else if (isQuestion == false) // every second line is answer
                {
                    answers.Add(Int32.Parse(line));
                    isQuestion = true;
                }
            }
        }

        private void Form1_FormClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            listening = false;
            terminating = true;
            Environment.Exit(0);
        }

        
        private void logs_TextChanged(object sender, EventArgs e)
        {
            // Scroll the textbox automatically (taken from SO)

            // set the current caret position to the end
            logs.SelectionStart = logs.Text.Length;
            // scroll it automatically
            logs.ScrollToCaret();
        }
    }
}
