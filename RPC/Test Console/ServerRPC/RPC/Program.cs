using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace RPC//Client và Server bắt buộc phải trùng namespace thì mới truyền được
{
    //Test goi ham tu xa
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Service start now....");
            TcpChannel channel = new TcpChannel(1008);
            ChannelServices.RegisterChannel(channel);
            Type type = Type.GetType("RPC.Test");
            RemotingConfiguration.RegisterWellKnownServiceType(type, "Player1", WellKnownObjectMode.SingleCall);
            RemotingConfiguration.RegisterWellKnownServiceType(type, "Player2", WellKnownObjectMode.SingleCall);
            //Console.WriteLine("Da chay toi day....");
            Console.ReadLine();//Dung man hinh de chuong trinh khong ket thuc.
        }
    }

    public interface TestInterface
    {
        string TestProcedure(string str);
        bool ban(int x, int y, string player);
    }

    public class Test : MarshalByRefObject, TestInterface
    {
        //int[,] maybay = new int[8,8];
        public string TestProcedure(string str)
        {
            Console.WriteLine("Client sent: " + str);
            return "Server says: Hello Client";
        }
        public bool ban(int x, int y, string player)
        {
            Console.WriteLine("May " + player + " da ban tai toa do ({0}, {1})", x, y);
            return false;
        }
    }

}