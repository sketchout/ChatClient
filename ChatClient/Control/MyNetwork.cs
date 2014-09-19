using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient
{
    class MyNetwork
    {
        //private String myEncoding = "ks_c_5601-1987"; //"MS949";

        private String myEncoding = "UTF-8"; //"MS949";
        private IPAddress ipAddr;
        private IPEndPoint ipEndPoint;
        private Socket clientSocket;

        public Socket GetConnect(string host, int port)
        {
            // Parse the IP address from the TextBox into an IPAddress object
            ipAddr = IPAddress.Parse(host);
            ipEndPoint = new IPEndPoint(ipAddr, port);

            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream,
                                        ProtocolType.Tcp);
            try {
                clientSocket.Connect(ipEndPoint);
            } catch (Exception e) {
                throw e;
            }
            return clientSocket;
        }

        public void SendMessage(string message)
        {
            // Default UTF8
            int iSendLen = Encoding.GetEncoding(myEncoding).GetByteCount(message);
            byte[] bSendLen = BitConverter.GetBytes(iSendLen);

            // littleEndian to BigEndian
            if (BitConverter.IsLittleEndian) Array.Reverse(bSendLen);
            
            Console.WriteLine(">iSendLen" + myEncoding + " :  " + iSendLen);

            // DEBUG
            //Console.WriteLine(">message :" + message );

            //Console.WriteLine("Encoding.Default.GetByteCount(message) : " 
                //+ Encoding.Default.GetByteCount(message));
            //Console.WriteLine("Encoding.GetEncoding(myEncoding).GetByteCount(message) : "
                //+ Encoding.GetEncoding(myEncoding).GetByteCount(message));
            try {
                clientSocket.Send(bSendLen);
            } catch (Exception e) {
                throw e;
            }
            // DEBUG
            //Console.WriteLine("BigEndian bSendLen :  " + MyUtil.ByteArrayToString(bSendLen));
            
            // Default UTF8
            byte[] bSendMsg = Encoding.GetEncoding(myEncoding).GetBytes(message);
            
            // Console.WriteLine(">bSendMsg" + myEncoding + " :  " + bSendMsg.Count() );
            clientSocket.Send(bSendMsg);
            
            // DEBUG
            Console.WriteLine("Client Send :" + MyUtil.ByteArrayToString(bSendMsg));
        }

        public string ReadMessage()
        {
            byte[] bReadLen = new byte[4];
            try {
                clientSocket.Receive(bReadLen);
            } catch (Exception e) {
                throw e;
            }
            // DEBUG
            Console.WriteLine("BigEndian bReadLen : " + MyUtil.ByteArrayToString(bReadLen));

            // littleEndian to BigEndian
            if (BitConverter.IsLittleEndian) Array.Reverse(bReadLen);
            int iReadLen = BitConverter.ToInt32(bReadLen, 0);

            // DEBUG
            Console.WriteLine("<iReadLen : "+ iReadLen );
            Console.WriteLine("LittleEndian bReadLen : " + MyUtil.ByteArrayToString(bReadLen) );

            byte[] bReadMsg = new byte[iReadLen];
            try {
                clientSocket.Receive(bReadMsg);
            } catch (Exception e) {
                throw e;
            }
            string readString = Encoding.GetEncoding(myEncoding).GetString(bReadMsg);
            
            //string readString = Encoding.Default.GetString(bReadMsg);

            // UTF8
            //byte[] cReadMsg = Encoding.Convert(Encoding.Default, Encoding.UTF8, bReadMsg);
            //string readString = Encoding.UTF8.GetString(cReadMsg);

            Console.WriteLine("Dafault/ASCII:"
                    + Encoding.Unicode.GetString( Encoding.Convert(Encoding.Default, Encoding.ASCII, bReadMsg) ) );

            Console.WriteLine("Dafault/UTF8:"
                    + Encoding.Unicode.GetString( Encoding.Convert(Encoding.Default, Encoding.UTF8, bReadMsg) ) );

            // unicode
            //byte[] cReadMsg = Encoding.Convert(Encoding.Default, Encoding.UTF8, bReadMsg);
            //string readString = Encoding.UTF8.GetString(cReadMsg);

            // DEBUG
            Console.WriteLine( "Client Read:"+ readString );
            return readString;
        }

        public bool CloseConnect()
        {

            this.clientSocket.Close();
            return true;
        }
    }
}
