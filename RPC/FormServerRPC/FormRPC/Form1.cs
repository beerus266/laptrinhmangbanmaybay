using System;
using System.Windows.Forms;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Net;
using TransferInfor.TungHuynh.Server;

namespace FormRPC
{
    public partial class Form1 : Form, IMsg
    {
        public static string ipClient1 = "localhost";
        public static string ipClient2 = "localhost";

        public Form1()
        {
            InitializeComponent();

            #region Nhận yêu cầu từ client
            listView1.Items.Add("Service start now....");
            TcpChannel channel = new TcpChannel(2304);
            ChannelServices.RegisterChannel(channel);
            Type type = Type.GetType("FormRPC.XulyChoi");
            RemotingConfiguration.RegisterWellKnownServiceType(type, "Player1", WellKnownObjectMode.SingleCall);
            RemotingConfiguration.RegisterWellKnownServiceType(type, "Player2", WellKnownObjectMode.SingleCall);

            MsgContent.AddObject(this);
            #endregion
        }

        #region Cài đặt phương thức của Interface IMsg
        //Cài đặt phương thức lấy IP từ Client vừa đăng ký từ interface IMsg
        public void IP1(string ipAdress)
        {
            CheckForIllegalCrossThreadCalls = false;
            ipClient1 = ipAdress;
            listView1.Items.Add("1: " + ipClient1 + " | " + ipClient2);
        }
        public void IP2(string ipAdress)
        {
            CheckForIllegalCrossThreadCalls = false;
            ipClient2 = ipAdress;
            listView1.Items.Add("2: " + ipClient1 + " | " + ipClient2);
        }

        //Cài đặt phương thức Msg() kế thừa từ giao diện IMsg
        public void Msg(string msg)
        {
            CheckForIllegalCrossThreadCalls = false; //Bỏ qua lỗi Cross-Thread
            listView1.Items.Add(msg);
        }
        #endregion

        //[Test] Lấy danh sách IP các máy trong mạng Lan
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                //IPAddress ipAddr = (IPAddress)requestHeaders[CommonTransportKeys.IPAddress];
                IPHostEntry iphost = Dns.GetHostByName(textBox1.Text);
                foreach (IPAddress ip in iphost.AddressList)
                {
                    string ipaddress = ip.AddressFamily.ToString();
                    listView1.Items.Add(ipaddress + ": " + ip.ToString());
                }
                textBox2.Text = iphost.HostName;
                //listView1.Items.Add(ipAddr.ToString());
            }
            catch { }
        }
        
        //Clear listview và textbox
        private void button2_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            textBox1.Text = "";
            textBox2.Text = "";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            About ab = new About();
            ab.ShowDialog();
        }
    }

    #region Class, Interface
    //Interface gửi lệnh đến client (lúc này client trở thành server và server trở thành client)
    public interface Client
    {
        void QuanDichBan(int x, int y, bool Trung);//Hiệu ứng boom nổ khi quân địch bắn, kiểm tra trúng ko
    }

    //Interface nhận yêu cầu từ client
    public interface InterfaceXulyChoi
    {
        byte Ban(int x, int y, string player);
        byte DangKyChoi(string str, string player, string ipAdress);// Đăng ký tham gia chơi
        bool XinThua(string player);// Xin thua cuộc, ko chơi nữa.
    }

    //Class xử lý nhận yêu cầu từ client
    public class XulyChoi : MarshalByRefObject, InterfaceXulyChoi
    {
        static bool[] client = new bool[2] { true, true };//2 client chơi với nhau
        static Client ti1;//client
        static Client ti2;//client
        Type type2;//client


        public bool XinThua(string player)
        {
            MsgContent.GetObject().SetMessage = player + " đã xin thua. Kết thúc trò chơi";
            if (player == "Player1") client[0] = true;
            else client[1] = true;
            return true;
        }

        public byte DangKyChoi(string str, string player, string ipAdress)
        {
            byte ktrThamgia = 0;
            if (player == "Player1")
            {
                if (client[0])
                {
                    client[0] = false;
                    MsgContent.GetObject().SetMessage = str;
                    if (client[1]) ktrThamgia = 1;//Player2 chưa tham gia, thì được quyền bắn
                    else ktrThamgia = 2;//Player2 đã tham gia trước thì nhường quyền bắn cho Player2
                    MsgContent.GetObject().setIPAddress1 = ipAdress;

                }
                else
                {
                    ktrThamgia = 0;
                }
            }
            else if (player == "Player2")
            {
                if (client[1])
                {
                    client[1] = false;
                    MsgContent.GetObject().SetMessage = str;
                    if (client[0]) ktrThamgia = 1;//Player1 chưa tham gia, thì được quyền bắn
                    else ktrThamgia = 2;//Player1 đã tham gia trước thì nhường quyền bắn cho Player1
                    MsgContent.GetObject().setIPAddress2 = ipAdress;

                }
                else
                {
                    ktrThamgia = 0;
                }
            }
            TcpChannel channel2 = new TcpChannel();
            type2 = typeof(Client);
            ti1 = (Client)Activator.GetObject(type2, "tcp://" + Form1.ipClient1 + ":2301/Client");

            ti2 = (Client)Activator.GetObject(type2, "tcp://" + Form1.ipClient2 + ":2302/Client");

            return ktrThamgia;

        }

        public byte Ban(int x, int y, string player)
        {
            //return 0: Bắn trượt
            //return 1: Bắn trúng

            MsgContent.GetObject().SetMessage = player + " bắn vào (" + x.ToString() + ", " + y.ToString() + ")";
            if (player == "Player1")
            {
                ti2.QuanDichBan(x, y, true);
                return 1;
            }
            else if (player == "Player2")
            {
                ti1.QuanDichBan(x, y, true);
                return 1;
            }
            return 0;
        }
    }
    #endregion
}
