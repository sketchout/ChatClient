using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Json;
using CenteredMessageboxDemo;

namespace ChatClient
{
    public partial class FormMain : Form
    {
        //
        // http://www.geekpedia.com/tutorial239_Csharp-Chat-Part-1---Building-the-Chat-Client.html
        //
        private bool bConnected=false;

        private MyNetwork myNetwork;
        //private Socket clientSocket;

        private Queue<string> sQueue;

        private string UserName = "UnKnown";

        private Thread readerThread;
        private Thread joinThread;

        // Needed to update the form with messages from another thread
        private delegate void UpdateLogCallback(string strMessage);

        // Needed to set the form to a "disconnected" state from another thread
        private delegate void CloseConnectionCallback(string strReason);

        public FormMain()
        {
            Application.ApplicationExit += new EventHandler(OnApplicationExit);
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            myNetwork = new MyNetwork();
            sQueue = new Queue<string>();
            ConnectOff();
        }

        public void OnApplicationExit(object sender, EventArgs e)
        {
            if (bConnected == true)
            {
                ConnectOff();
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            Handle_ConnectClick();
        }

        private void Handle_ConnectClick()
        {
            if (txtIp.Text.Trim() == "" 
                || txtPort.Text.Trim() == ""
                || txtUser.Text.Trim() == ""
                || txtPw.Text.Trim() == "" )
            {
                MsgBox.Show("Enter Ip/Port,Username/Password before connecting","Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            // if we are not currently connected but awaiting to connect
            if (bConnected == false)
            {
                // Initialize the connection
                InitializeConnection();
            }
            else // We are connected, thus disconnect
            {
                this.Invoke(new CloseConnectionCallback(this.CloseConnection), 
                    new object[] { "Disconnected at user's request!" });
                //CloseConnection("Disconnected at user's request");
            }
        }
        private void InitializeConnection()
        {
            try {
                // clientSocket.Connect(ipEndPoint);
                myNetwork.GetConnect(txtIp.Text.Trim(), Convert.ToInt32(txtPort.Text.Trim()) );

                Send_Login_Message();

                string readMessage = myNetwork.ReadMessage();

                JsonTextParser parser = new JsonTextParser();
                JsonObject obj = parser.Parse(readMessage);
                JsonObjectCollection col = (JsonObjectCollection)obj;

                string cmd = (string)col["cmd"].GetValue();
                if (cmd == "login")
                {
                    string rt = (string)col["result"].GetValue();
                    if (rt == "fail")
                    {
                        MsgBox.Show((string)col["reason"].GetValue() , "Error", 
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

            } catch ( Exception e ) {
                Console.WriteLine("{0}", e.ToString() );
                MsgBox.Show("in connecting...","Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error); 
                return;
            }
            ConnectOn();
            GetThread();
        }

        private void Send_Login_Message()
        {
            JsonObjectCollection coll = new JsonObjectCollection();
            coll.Add(new JsonStringValue("cmd", "login"));

            coll.Add(new JsonStringValue( "user", txtUser.Text.Trim() ) );
            coll.Add(new JsonStringValue( "passwd", txtPw.Text.Trim() ) );

            myNetwork.SendMessage(coll.ToString());
        }
        private void GetThread()
        {
            //throw new NotImplementedException();
            readerThread = new Thread(new ThreadStart(ReadMessage));
            readerThread.Start();

            joinThread = new Thread(new ThreadStart(joinLoop) );
            joinThread.Start();
        }

        private void joinLoop()
        {
            readerThread.Join();
            //CloseConnection("Disconnected from Server");
            //this.Invoke(new UpdateLogCallback(this.UpdateLog), new object[] { "Disconnected from Server" });
            this.Invoke(new CloseConnectionCallback(this.CloseConnection), 
                new object[] { "Disconnected from Server" });
        }

        private void UpdateLog(string strMessage)
        {
            //throw new NotImplementedException();
            if ( btnSend.Enabled )
                txtLog.AppendText(strMessage + "\r\n");
        }

        private void ConnectOn()
        {
            //throw new NotImplementedException();
            this.bConnected = true;
            this.UserName = txtUser.Text;

            // disable and enable the appropriate fields
            txtIp.Enabled = false;
            txtUser.Enabled = false;

            txtMessage.Enabled = true;
            btnSend.Enabled = true;

            btnConnect.Text = "Disconnect";

            this.Invoke(new UpdateLogCallback(this.UpdateLog), 
                            new object[] { "Connected Successfully!" });
        }

        private void ConnectOff()
        {
            //throw new NotImplementedException();
            this.bConnected = false;
            this.UserName = "";

            // disable and enable the appropriate fields
            txtIp.Enabled = true;
            txtUser.Enabled = true;

            txtMessage.Enabled = false;
            btnSend.Enabled = false;
            
            btnConnect.Text = "Connec";
        }

        private void CloseConnection(string Reason)
        {
            //txtLog.AppendText( Reason + "\r\n");
            UpdateLog( Reason + "\r\n" );
            // Enable and disable the appropriate controls on the form
            ConnectOff();
            // Close the objects
            CloseObject();
        }

        private void CloseObject()
        {
            //throw new NotImplementedException();
            bConnected = false;
            //clientSocket.Close();
            myNetwork.CloseConnect();
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            Send_Chat_Message();
        }


        private void Send_Chat_Message()
        {
            JsonObjectCollection coll = new JsonObjectCollection();
            coll.Add(new JsonStringValue("cmd", "chat"));

            if (txtMessage.Lines.Length >= 1)
            {
                coll.Add(new JsonStringValue("msg",
                    " [ "+ txtUser.Text.Trim() +" ]  " + txtMessage.Text.Trim() ));

                // send message
                myNetwork.SendMessage(coll.ToString());

                txtMessage.Lines = null;
            }
            txtMessage.Text = "";
        }

        private void ReadMessage()
        {
            while (bConnected)
            {
                try {
                    string readMessage = myNetwork.ReadMessage();

                    JsonTextParser parser = new JsonTextParser();
                    JsonObject obj = parser.Parse(readMessage);
                    JsonObjectCollection col = (JsonObjectCollection)obj;

                    string cmd = (string)col["cmd"].GetValue();

                    if (cmd == "chat") {
                        this.Invoke(new UpdateLogCallback(this.UpdateLog),
                            new object[] { (string)col["msg"].GetValue() });
                    }
                }
                catch (Exception e) {
                    Console.WriteLine("{0}", e.ToString());
                    //MessageBox.Show("Error in receiving...");
                    break;
                }
                Thread.Sleep(100);
            }
        }

        private void txtMessage_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                Send_Chat_Message();
            }
            
        }
    }
}
