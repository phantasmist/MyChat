using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyChat
{
    public partial class frmChat : Form
    {
        public frmChat()
        {
            InitializeComponent();
        }
        //global var.
        Socket sock = null;
        TcpClient tcp = null;
        TcpListener listen = null;
        Thread threadServer = null; //Connect request thread
        Thread threadRead = null; //input string parse thread
        Thread threadClientRead = null;
        private void btnStartServer_Click(object sender, EventArgs e)
        {
            if(listen != null)
            {
                if (MessageBox.Show("Restart Server?\r\n", "", MessageBoxButtons.OKCancel) == DialogResult.OK)
                {
                    listen.Stop();
                    if(threadServer != null) threadServer.Abort();
                    if (threadRead != null) threadRead.Abort();
                }
            }
            listen = new TcpListener(int.Parse(tbServerPort.Text));
            listen.Start();
            AddText($"Server Started [Port:{tbServerPort.Text}]\r\n");

            threadServer = new Thread(ServerProcess);
            threadServer.Start();
            //threadRead = new Thread(ReadProcess); 1대다 통신을 위해서 옮겼다
            
        }

        void ServerProcess() //Connection Request Thread Process
        {
            while (true)
            {
                if (listen.Pending()) //1대1 통신용 서버
                {
                    if (tcp != null) //기존 세션 존재하면 종료
                    {                
                        tcp.Close(); 
                        threadRead.Abort();
                    }
                    tcp = listen.AcceptTcpClient(); //(다시) 세션 수립
                    AddText($"Connection from Client [{tcp.Client.RemoteEndPoint.ToString()}]\r\n");
                    statusBar.Items.Add(tcp.Client.RemoteEndPoint.ToString());
                    threadRead = new Thread(ReadProcess); //1대다 세션 위해 이동
                    threadRead.Start();
                }
                Thread.Sleep(100); //while 문의 부담을 줄이기 위함
            }
        }

        void ReadProcess() //thread process
        {
            NetworkStream ns = tcp.GetStream();
            byte[] buf = new byte[512];
            while (true)
            {
                int n = ns.Read(buf, 0, 512);
                AddText(Encoding.Default.GetString(buf, 0, n));
                Thread.Sleep(100);
            }
        }

        void ClientReadProcess()
        {
            byte[] bArr = new byte[512];
            while (true)
            {
                if (sock.Available > 0)
                {
                    int n = sock.Receive(bArr);
                    AddText($"{Encoding.Default.GetString(bArr, 0, n)}", 1);
                }
                Thread.Sleep(100);
            }
        }

        // delegate func
        delegate void cbAddText(string str, int i = 0);
        void AddText(string str, int i = 0)
        {
            if(tbServer.InvokeRequired || tbClient.InvokeRequired)
            {
                cbAddText cb = new cbAddText(AddText);
                object[] obj = { str, i };
                Invoke(cb, obj);
            }
            else
            {
                if (i == 0)
                {
                    tbServer.Text += str;
                }
                else
                {
                    tbClient.Text += str;
                }
            }
            
        }

        private void frmChat_FormClosing(object sender, FormClosingEventArgs e)
        {   //close threads when program ends
            //does not seem to work w/ multiple connections
            if (threadServer != null) threadServer.Abort();
            if (threadRead != null) threadRead.Abort();
            if (threadClientRead != null) threadClientRead.Abort();
            
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {                
                if (sock != null) // 소켓 연결 재설정 필요
                {                 // 소켓 닫고, 스레드 재설정
                    if (MessageBox.Show("Reconnect?\r\n", "", MessageBoxButtons.OKCancel) == DialogResult.OK)
                    {
                        sock.Close();
                        threadClientRead.Abort();
                    }
                    //위 내용 실행되고 아래에서 재연결
                }
                //AddressFamily.InterNetwork: IPv4
                sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                sock.Connect(tbConnectIP.Text, int.Parse(tbConnectPort.Text)); // Connection Request
                AddText($"Server [{tbConnectIP.Text}:{tbConnectPort.Text}] Connection OK\r\n", 1);
                //스레드가 여러번 생성되면 안됨..
                threadClientRead = new Thread(ClientReadProcess);
                threadClientRead.Start();

            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message);
            }
        }

        private void pmnSendClientText_Click(object sender, EventArgs e)
        {
            string str = (tbClient.SelectedText == "") ? tbClient.Text : tbClient.SelectedText;
            byte[] bArr = Encoding.Default.GetBytes(str);
            sock.Send(bArr);
        }

        
    }
}
