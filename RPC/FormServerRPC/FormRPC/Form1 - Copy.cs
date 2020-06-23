using System;
using System.Windows.Forms;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Net;

namespace FormRPC
{
    public partial class Form1 : Form, IMsg
    {
        public static Client ti1;//client
        public static Client ti2;//client
        Type type2;//client
        string ipClient1 = "localhost";
        string ipClient2 = "localhost";
        public Form1()
        {
            InitializeComponent();

            #region Nhận yêu cầu từ client
            listView1.Items.Add("Service start now....");
            TcpChannel channel = new TcpChannel(2304);
            ChannelServices.RegisterChannel(channel);
            Type type = Type.GetType("FormRPC.Test");
            RemotingConfiguration.RegisterWellKnownServiceType(type, "Player1", WellKnownObjectMode.SingleCall);
            RemotingConfiguration.RegisterWellKnownServiceType(type, "Player2", WellKnownObjectMode.SingleCall);

            MsgContent.AddObject(this);
            #endregion

            #region Phản hồi lại client
            TcpChannel channel2 = new TcpChannel();
            //ChannelServices.RegisterChannel(channel2);
            type2 = typeof(Client);
            ti1 = (Client)Activator.GetObject(type2, "tcp://" + ipClient1 + ":2301/Client");
            ti2 = (Client)Activator.GetObject(type2, "tcp://" + ipClient2 + ":2302/Client");
            #endregion

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        //Cài đặt phương thức Msg() kế thừa từ giao diện IMsg
        public void Msg(string msg)
        {
            CheckForIllegalCrossThreadCalls = false; //Bỏ qua lỗi Cross-Thread
            listView1.Items.Add(msg);
        }

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

        private void button2_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            textBox1.Text = "";
            textBox2.Text = "";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ti1.testSend();
            ti2.testSend();
        }
    }
    public interface TestInterface
    {
        string TestProcedure(string str, string player);
        bool ban(int x, int y, string player);
        byte Reg(string str, string player, string ipAdress);// Đăng ký tham gia chơi
        int[] AddElement(int k);
        bool xinthua(string player);// Xin thua cuộc, ko chơi nữa.
    }

    public interface Client
    {
        void testSend();
        void QuanTaBan(int x, int y);//Hiệu ứng boom nổ khi quân ta bắn 
        void QuanDichBan(int x, int y);//Hiệu ứng boom nổ khi quân địch bắn
    }

    public class Test : MarshalByRefObject, TestInterface
    {
        static bool[] client = new bool[2] { true, true };//2 client chơi với nhau
        static int[] a = new int[10];
        static int i = 0;
        Client cl;
        
        public string TestProcedure(string str, string player)
        {
            string thongbao = "Kết nối thành công! \nChào ";
            if (player == "Player1")
            {
                if (client[0])
                {
                    client[0] = false;
                    MsgContent.GetObject().SetMessage = str;
                }
                else
                {
                    thongbao = "Kết nối không thành công! \nChọn người chơi khác.";
                }
            }
            else if (player == "Player2")
            {
                if (client[1])
                {
                    client[1] = false;
                    MsgContent.GetObject().SetMessage = str;
                }
                else
                {
                    thongbao = "Kết nối không thành công! \nChọn người chơi khác.";
                }
            }
            return thongbao;
        }

        public bool xinthua(string player)
        {
            MsgContent.GetObject().SetMessage = player + " đã xin thua. Kết thúc trò chơi";
            if (player == "Player1") client[0] = true;
            else client[1] = true;
            return true;
        }

        public byte Reg(string str, string player, string ipAdress)
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
            return ktrThamgia;
        }

        public bool ban(int x, int y, string player)
        {
            MsgContent.GetObject().SetMessage = player + " bắn vào (" + x.ToString() + ", " + y.ToString() + ")";
            //MessageBox.Show(player + "bắn vào (" + x.ToString() + ", " + y.ToString() + ")");
           
            return false;
        }

        public int[] AddElement(int k)
        {
            a[i] = k;
            i++;
            return a;
        }
    }

    #region Hỗ trợ hiển thị thông tin nhận từ Client vào Listview trên Server
    public interface IMsg
    {
        void Msg(string msg);
        void IP1(string ipAdress);
        void IP2(string ipAdress);
    }

    public class MsgContent
    {
        private static MsgContent msgContent = null;
        private static IMsg msg = null;


        public string SetMessage
        {
            set { msg.Msg(value); }
        }

        public string setIPAddress1
        {
            set { msg.IP1(value); }
        }

        public string setIPAddress2
        {
            set { msg.IP2(value); }
        }

        public static void AddObject(IMsg test)
        {
            msg = test;
        }
        public static MsgContent GetObject()
        {
            if (msgContent == null)
                msgContent = new MsgContent();
            return msgContent;
        }
    }
    #endregion

}
