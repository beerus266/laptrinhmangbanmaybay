using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace RPC//Client và Server bắt buộc phải trùng namespace thì mới truyền được
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Enter name (Player1/Player2): ");
            string name = Console.ReadLine();
            TcpChannel channel = new TcpChannel();
            ChannelServices.RegisterChannel(channel);
            Type type = typeof(TestInterface);
            TestInterface ti = (TestInterface)Activator.GetObject(type,
                                                        "tcp://localhost:1008/" + name + "");
            if (name == "Player1")
                Console.WriteLine(ti.TestProcedure("Player 1 is online") + name);
            else
                Console.WriteLine(ti.TestProcedure("Player 2 is online") + name);
            bool kt = ti.ban(10, 23, name);
            if (!kt)
                Console.WriteLine("Cho ben kia ban.");
            Console.WriteLine("Xong");
            Console.ReadLine();
        }
    }

    public interface TestInterface
    {
        string TestProcedure(string str);
        bool ban(int x, int y, string player);
    }
}
