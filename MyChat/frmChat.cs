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
        private void btnStartServer_Click(object sender, EventArgs e)
        {
            listen = new TcpListener(int.Parse(tbServerPort.Text));
            listen.Start();

            threadServer = new Thread(ServerProcess);
            threadServer.Start();

            threadRead = new Thread(ReadProcess);
            
              
        }

        void ServerProcess() //Connect request thread
        {
            while (true)
            {
                if (listen.Pending())
                {
                    tcp = listen.AcceptTcpClient(); //세션 수립
                    AddText("Connection from Client []\r\n");
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
    }
}
