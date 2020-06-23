using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Net;
using System.Text;
using System.Windows.Forms;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Media;
using System.Threading;
using TransferInfor.TungHuynh.Client;

namespace FormRPC
{
    public partial class Form1 : Form, IMsg
    {
        #region Các biến toàn cục
        InterfaceXulyChoi ti;
        Type type;
        string name = "Player1";
        static int kichthuoc = 10;//kích thước chiến trường (số ô)
        int port = 2301;
        string ipServer = "localhost";
        int kickthuocO = 0;//Kich thước của từng cell đã kẻ trên picturebox
        public static int batdauMienTa = 0;//vị trí bắt đầu khu vực của quân Ta 
                            //(quân ta từ pictureBox.Width-pictureBox.Height, địch từ 0)
        bool dangchon = false;


        #endregion 

        public Form1()
        {
            InitializeComponent();
            kickthuocO = pictureBox1.Height / kichthuoc;
            batdauMienTa = pictureBox1.Width - pictureBox1.Height;

            #region Kết nối đến server
            TcpChannel channel = new TcpChannel();
            ChannelServices.RegisterChannel(channel);
            type = typeof(InterfaceXulyChoi);
            ti = (InterfaceXulyChoi)Activator.GetObject(type, "tcp://" + ipServer + ":2304/" + name + "");
            #endregion
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            pictureBox1.BackgroundImageLayout = ImageLayout.Stretch;
            comboBox1.SelectedIndex = 0;//Player
            chophepChoi(0);
        }

        //Nhận tin từ class khác (qua interface IMsg) đưa lên listview (các phản hồi từ server)
        public void Msg(string msg)
        {
            CheckForIllegalCrossThreadCalls = false; //Bỏ qua lỗi Cross-Thread
            listView1.Items.Add(msg);
        }

        //Báo thua cuộc
        public void BaoThuaCuoc()
        {
            listView1.Items.Add("Bạn đã thua!. Kết thúc");
            chophepChoi(2);
        }

        #region Bắn
        //Vị trí đạn rơi
        public void Ban(int x, int y, int vungTaDich, bool Trung)
        {//int vungTaDich: Quân ta bắn -> trên màn hình hiện điểm bắn từ x=0
            //trên màn hình quân địch hiển thị điểm bắn từ x=batdauMienTa
            if (Trung) draw(x * kickthuocO + vungTaDich, y * kickthuocO, kickthuocO/2, Brushes.Red);
            else draw(x * kickthuocO + vungTaDich, y * kickthuocO, kickthuocO/2, Brushes.Green);
        }
        //Hiệu ứng bắn
        private void draw(int x, int y, int r, Brush defaultBrush)
        {
            //SoundPlayer sp = new SoundPlayer();//System.Threading;
            //sp.SoundLocation = @"sounds/bomno.wav";
            //sp.Play();
            //Thread.Sleep(3000);

            Graphics g = pictureBox1.CreateGraphics();
            Pen blackPen = new Pen(defaultBrush);
            Rectangle rect = new Rectangle(x, y, r, r);
            g.DrawEllipse(blackPen, rect);
            g.FillEllipse(defaultBrush, rect);
        }
        #endregion

        //thiết lập quyền chơi
        public void chophepChoi(byte chedo)
        {
            //chế độ = 0: chưa tham gia
            //-------- 1: đã tham gia, được quyền bắn
            //-------- 2: đã tham gia, chờ bên kia bắn

            bool chophep = false;
            if (chedo == 0) chophep = false;
            else if (chedo == 1) chophep = true;

            pictureBox1.Enabled = chophep;
            button2.Enabled = chophep;//bỏ cuộc
            button3.Enabled = !chophep;//tham gia
            comboBox1.Enabled = !chophep;

            if (chedo == 2)
            {
                pictureBox1.Enabled = false;
                button2.Enabled = true;//bỏ cuộc
                button3.Enabled = false;//tham gia
                comboBox1.Enabled = false;
            }
        }

        #region Xử lý trên PictureBox
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            drawGrid(0, pictureBox1.Height, e);
            drawGrid(batdauMienTa, pictureBox1.Width, e);
        }
        //Vẽ đường kẻ ô vuông
        public void drawGrid(int start, int end, PaintEventArgs e)
        {
            //Dọc
            for (int i = 0; i <= kichthuoc; i++)
                e.Graphics.DrawLine(Pens.DarkGreen, kickthuocO * i + start, 0, kickthuocO * i + start, pictureBox1.Height);

            //Ngang
            for (int i = 0; i <= kichthuoc; i++)
                e.Graphics.DrawLine(Pens.DarkGreen, start, kickthuocO * i, end, kickthuocO * i);
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (dangchon)//đặt máy bay tại vị trí nhấn chuột
                dangchon = false;
            else//chọn điểm bắn theo vị trí nhấn chuột
            {
                int x = e.X / (pictureBox1.Height / kichthuoc);
                int y = e.Y / (pictureBox1.Height / kichthuoc);
                if (x < kichthuoc && y < kichthuoc)
                {
                    textBox1.Text = x.ToString();
                    textBox2.Text = y.ToString();
                    byte ktra = ti.Ban(x, y, name);
                    if (ktra == 0)
                        Ban(x, y, 0, false);
                    else if (ktra == 1)
                        Ban(x, y, 0, true);
                }
            }
        }


        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {

        }
        #endregion

        #region Chọn người chơi và tham gia
        //Chọn người chơi
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == 0)
                name = "Player1";
            else name = "Player2";
            //comboBox2.SelectedIndex = comboBox1.SelectedIndex;
        }

        //Chọn cổng kết nối (dùng khi chạy trên 1 máy, IP trùng nhau)
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            //comboBox1.SelectedIndex = comboBox2.SelectedIndex;
            try
            {
                port = Convert.ToInt32(comboBox2.Text);
            }
            catch (Exception ex)
            {
                port = 0;
            }
            #region Nhận các phản hồi từ server
            TcpChannel channel2 = new TcpChannel(port);
            //ChannelServices.RegisterChannel(channel2);
            Type type2 = Type.GetType("FormRPC.testClient");
            RemotingConfiguration.RegisterWellKnownServiceType(type2, "Client", WellKnownObjectMode.SingleCall);

            MsgContent.AddObject(this);
            #endregion
        }
        //Nhập IP của server
        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            ipServer = textBox3.Text;
        }
        //tham gia
        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                IPHostEntry ip = Dns.GetHostByName(Dns.GetHostName());//Lấy IP hiện tại của máy
                byte ktrThamgia = ti.DangKyChoi(comboBox1.Text + " is online", name, ip.AddressList[0].ToString());

                if (ktrThamgia != 0)
                {
                    listView1.Items.Add(ktrThamgia.ToString() + ": Tham gia thành công!");
                    chophepChoi(1);
                }
                else
                {
                    listView1.Items.Add("Không thành công!. " + comboBox1.Text + " đã được sử dụng.");
                    chophepChoi(ktrThamgia);
                }
            }
            catch
            {
                listView1.Items.Add("Server chưa khởi động!");
            }
        }
        #endregion

        #region Bỏ cuộc
        //Bỏ cuộc
        private void button2_Click(object sender, EventArgs e)
        {
            if (ti.XinThua(name)) listView1.Items.Add("Bạn đã đầu hàng! Kết thúc trò chơi");
            chophepChoi(0);
        }
        //Nếu đóng client thì tự động bỏ cuộc
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                ti.XinThua(name);
            }
            catch { }
        }
        #endregion

        private void button1_Click(object sender, EventArgs e)
        {
            About ab = new About();
            ab.ShowDialog();
        }
    }


    //Interface gọi hàm trên server
    public interface InterfaceXulyChoi
    {
        byte Ban(int x, int y, string player);
        byte DangKyChoi(string str, string player, string ipAdress);// Đăng ký tham gia chơi
        bool XinThua(string player);// Xin thua cuộc, ko chơi nữa.
    }

    #region Nhận lệnh từ server
    //Interface nhận lệnh từ server
    public interface Client
    {
        void QuanDichBan(int x, int y, bool Trung);//Hiệu ứng boom nổ khi quân địch bắn, ktra trúng ko
    }
    //Class kế thừa interface xử lý lệnh từ server
    public class testClient : MarshalByRefObject, Client
    {

        public void QuanDichBan(int x, int y, bool Trung)
        {
            MsgContent.GetObject().SetMessage = "Dich ban: (" + x.ToString() + ", " + y.ToString() + ")";
            MsgContent.Ban(x, y, Form1.batdauMienTa ,Trung);
        }
    }
    #endregion
}
