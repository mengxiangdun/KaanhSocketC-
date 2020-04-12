using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace KaanhSocketExample
{
    class KaanhSocket
    {
        public int port;
        public IPAddress ip;
        static Socket listener;
        private static byte[] result = new byte[1024];
        static Socket botClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        Message ms = new Message();
        public KaanhSocket(string _ip, string _port)
        {

            try
            {
                ip = IPAddress.Parse(_ip);
                port = int.Parse(_port);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }


        }

        public void Connect()
        {
            try
            {
                botClient.Connect(ip, port);
                Console.WriteLine("Robot server connected!!");
            }
            catch (Exception e)
            {
                Console.WriteLine("Connect to robot server failed!" + e.ToString());
                throw;
            }
        }

        public void Send(string cmd)
        {
            botClient.Send(OperateBotCmd(cmd));
            Console.WriteLine("Send Cmd: " + cmd);
            ReceiveMess();
        }
        void ReceiveMess()
        {
            byte[] messByte = new byte[1024];
            int messLength = botClient.Receive(messByte);
            byte[] mess = new byte[messLength];
            Array.Copy(messByte, 0, mess, 0, messLength);
            ms.byteFromBotServer.Enqueue(mess);
            if (ms.byteFromBotServer.Count > 0)
            {
                string thisMess = ms.ProcessMessQue();
                Console.WriteLine("Receieve from bot sever:" + thisMess);

           
            }
        }

        private static byte[] OperateBotCmd(string str)
        {
            string[] strArray = str.Split(new char[] { '&' });
            if (strArray.Length <= 1)
            {
                try
                {
                    int cmd_id = 0;
                    long cmd_option = 0;
                    int cmd_code = 0;
                    int reserved_0 = 0;
                    long reserved_1 = 0;
                    long reserved_2 = 0;
                    byte[] cmdByte = Encoding.Default.GetBytes(strArray[0]);

                    List<byte> packData = new List<byte>();

                    byte[] byte_cmd_length = BitConverter.GetBytes(cmdByte.Length);
                    byte[] byte_cmd_id = BitConverter.GetBytes(cmd_id);
                    byte[] byte_cmd_option = BitConverter.GetBytes(cmd_option);
                    byte[] byte_cmd_code = BitConverter.GetBytes(cmd_code);
                    byte[] byte_cmd_res0 = BitConverter.GetBytes(reserved_0);
                    byte[] byte_cmd_res1 = BitConverter.GetBytes(reserved_1);
                    byte[] byte_cmd_res2 = BitConverter.GetBytes(reserved_2);
                    packData.AddRange(byte_cmd_length);
                    packData.AddRange(byte_cmd_id);
                    packData.AddRange(byte_cmd_option);
                    packData.AddRange(byte_cmd_code);
                    packData.AddRange(byte_cmd_res0);
                    packData.AddRange(byte_cmd_res1);
                    packData.AddRange(byte_cmd_res2);
                    packData.AddRange(cmdByte);

                    return packData.ToArray();
                }
                catch (Exception e)
                {
                    Console.WriteLine("解析客户端命令出错：" + e.ToString());

                    return null;
                }
            }
            if (strArray.Length > 2)
            {
                try
                {
                    int cmd_id = int.Parse(strArray[0]);
                    long cmd_option = long.Parse(strArray[1]);
                    long reserved_1 = long.Parse(strArray[2]);
                    long reserved_2 = long.Parse(strArray[3]);
                    long reserved_3 = long.Parse(strArray[4]);
                    byte[] cmdByte = Encoding.Default.GetBytes(strArray[5]);

                    List<byte> packData = new List<byte>();

                    byte[] byte_cmd_length = BitConverter.GetBytes(cmdByte.Length);
                    byte[] byte_cmd_id = BitConverter.GetBytes(cmd_id);
                    byte[] byte_cmd_option = BitConverter.GetBytes(cmd_option);
                    byte[] byte_cmd_res1 = BitConverter.GetBytes(reserved_1);
                    byte[] byte_cmd_res2 = BitConverter.GetBytes(reserved_2);
                    byte[] byte_cmd_res3 = BitConverter.GetBytes(reserved_3);
                    packData.AddRange(byte_cmd_length);
                    packData.AddRange(byte_cmd_id);
                    packData.AddRange(byte_cmd_option);
                    packData.AddRange(byte_cmd_res1);
                    packData.AddRange(byte_cmd_res2);
                    packData.AddRange(byte_cmd_res3);
                    packData.AddRange(cmdByte);

                    return packData.ToArray();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());

                    return null;
                }

            }
            else
            {
                return null;
            }

        }

    }

    class Message
    {
        public Queue<byte[]> byteFromBotServer = new Queue<byte[]>();
        int bufferLength = 0;
        byte[] bufferBetween = new byte[1024];
        List<byte[]> buffer = new List<byte[]>();

        public string ProcessMessQue()//处理bot server传过来的数据
        {

            byte[] tempByte = byteFromBotServer.Dequeue();//提取处理队列
            byte[] processByte = new byte[bufferLength + tempByte.Length];
            if (bufferLength > 0)
            {
                Array.Copy(bufferBetween, 0, processByte, 0, bufferLength);//将未处理的数据拷入
            }

            Array.Copy(tempByte, 0, processByte, bufferLength, tempByte.Length);//将新数据拷入
            if (processByte.Length <= 40)//如果长度小于40，直接存入buffer
            {
                bufferLength = processByte.Length;
                Array.Copy(processByte, 0, bufferBetween, 0, processByte.Length);
                return null;
            }
            else
            {
                int length = BitConverter.ToInt32(processByte, 0);
                if (processByte.Length < length + 40)//长度不够，数据不完整，存入buffer
                {
                    bufferLength = processByte.Length;
                    Array.Copy(processByte, 0, bufferBetween, 0, processByte.Length);
                    return null;
                }
                else //长度足够，处理数据
                {
                    string output = Encoding.Default.GetString(processByte, 40, length);
                   // int cmd_code = Encoding.Default.
                    if (processByte.Length == length + 40)//长度刚好，buffer 置零
                    {
                        bufferLength = 0;
                    }
                    else
                    {
                        bufferLength = processByte.Length - (length + 40);
                        Array.Copy(processByte, (length + 40), bufferBetween, 0, bufferLength);
                    }
                    return output;
                }

            }



        }
        
    }
}
