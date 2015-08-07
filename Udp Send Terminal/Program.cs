using LumiSoft.Net.STUN.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Udp_Send_Terminal
{
    class Program
    {
        static int listeningPort = 1111;
        static int opponentPort = 1111;

        static string serverIP = "127.0.0.1";
        static void Main(string[] args)
        {
            Console.WriteLine("Open and listening port: ");
            listeningPort = int.Parse(Console.ReadLine());
            Console.WriteLine("Opponent port: ");
            opponentPort = int.Parse(Console.ReadLine());
            Console.WriteLine("Raise server or client (s/c):");
            string server = Console.ReadLine();
            if (server.Equals("s"))
            {
                Console.WriteLine("Run as server ");
                ListenServer();
            }
            else
            {
                Console.WriteLine("Server IP: ");
                string servIPbuf = Console.ReadLine();
                if (!servIPbuf.Equals("")) {
                    serverIP = servIPbuf;
                }
                Console.WriteLine("Run as client");
                ListenClient();
            }
        }
        static void ListenClient()
        {

            int recv; //храним размер полученных данных

            byte[] data = new byte[1024]; //данные, которые будут передаваться или приниматься

     

            //создаем новый сокет

            //параметр AddressFamily задает схему адресации. В нашем случае это адреса IPv4

            //параметр SocketType указывает, какой тип сокета применяется.

            //В данном случае SocketType.Dgram это ненадежные сообщения, но для примера хватит.

            //Тем более, что Dgram поддерживает протокол UDP

            //последний параметр, ProtocolType, задает тип протокола

            Socket mysocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

     

            //создаем конечную точку по адресу сокета.
            //Т.е. будем "слушать" порт 9051 и контролировать все сетевые интерфейсы

            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, listeningPort);//9051);

            mysocket.Bind(ipep); //привязываем точку к нашему сокету

 

            //создаем еще одну точку

            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);

     

            //определяем сетевой адрес

            EndPoint Remote = (EndPoint)(sender);

     

            //отправляем первое сообщение нашему серверу

            string text = "Hello"; //текст сообщения

            data = Encoding.ASCII.GetBytes(text); //переводим строку в байты

     

            //отправляем на указанны адрес

            //первый параметр это сами данные в виде массива байт

            //второй параметр, какая длиная сообщения должна быть передана.

            //Если указать меньше, то передаст только то число символов, сколько укажете

            //третий параметр указывает поведение сокета при приеме и получении данных. В нашем случае ничего...

            //четвертый парамерт задает адрес и порт сервера, которому нужно отправить сообщение

            //что делает функция _getHost, смотрите чуть ниже...

            //в данном примере используется бродкастовый адрес подсети 192.168.15.255,

            //если мы не знаем по какому адресу находится сервер. 
            //Порт подставляем любой (1111), он все равно будет перезаписан в функции _getHost

            //Использование функции SendTo позволяет заранее не соединяться с сервером

            mysocket.SendTo(data, data.Length, SocketFlags.None, _getHost(serverIP.ToString() + ":1111"));//("192.168.88.244:1111"));

     

            //запускаем бесконечный цикл, который будет принимать и отправлять данные

            while (true)

            {

                data = new byte[1024];

                //принимаем данные от сервера. recv содержир размер, т.е. количество принятых символов

                recv = mysocket.ReceiveFrom(data, ref Remote);

                //в данном примере сервер шлет сообщение, которое разделено ":"

                //разбираем его в массив

                //функция Encoding.ASCII.GetString переводит массив байт в строку

                string[] args = Encoding.ASCII.GetString(data, 0, recv).ToLower().Split(':');  

                foreach(string item in args)

                {

                    Console.WriteLine("(server): " + item);
                }

                //отправим ответ что мы получили. Например, первый элемент массива

                data = Encoding.ASCII.GetBytes(args[0]);

         

                //Remote содержит адрес, с которого пришло сообщение. Ему его назад и отправляем

                mysocket.SendTo(data, data.Length, SocketFlags.None, _getHost(Remote.ToString()));

                //Console.ReadLine();
            }

        }

 

        //функция _getHost

        static EndPoint _getHost(string text)

        {

            //вырезаем из строки только IP адрес формата IPv4

            string host = text.Remove(text.IndexOf(":"), text.Length - text.IndexOf(":"));

     

            //создаем объект адреса. Переменная host уже имеет вид [0-255].[0-255].[0-255].[0-255]

            IPAddress hostIPAddress = IPAddress.Parse(host);

     

            //создаем конечную точку. В нашем случае это адрес сервера, который слушает порт 9050

            IPEndPoint hostIPEndPoint = new IPEndPoint(hostIPAddress, opponentPort);//9050);

            EndPoint To = (EndPoint)(hostIPEndPoint);

            return To;

	    }
    

        static void ListenServer()
        {

            int recv; //храним размер полученных данных

            byte[] data = new byte[1024]; //данные, которые будут передаваться или приниматься



            Socket mysocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);


            //STUN = STUN_Client.Query(StunServer, StunServerPort, mysocket);

            //Console.WriteLine("STUN.PublicEndPoint: " + STUN.PublicEndPoint);

            //В отличие от клиента "слушаем" порт 9050

            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, listeningPort);// 9050);

            mysocket.Bind(ipep); //привязываем точку к нашему сокету

            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);

            EndPoint Remote = (EndPoint)(sender);



            //запускаем бесконечный цикл, который будет принимать и отправлять данные

            while (true)
            {

                data = new byte[1024];

                //принимаем данные от клиента. recv содержир размер, т.е. количество принятых символов

                recv = mysocket.ReceiveFrom(data, ref Remote);

                string message = Encoding.ASCII.GetString(data, 0, recv);

                switch (message)
                {

                    case "Hello": //на ответ от клиента Hello, шлем ответ ОК

                        data = Encoding.ASCII.GetBytes("OK:Server finded!");

                        break;
                    default:

                        data = Encoding.ASCII.GetBytes("Hello! I'm a server.");

                        break;
                }

                //Remote содержит адрес, с которого пришло сообщение. Ему его назад и отправляем

                mysocket.SendTo(data, data.Length, SocketFlags.None, _getHost(Remote.ToString()));

            }

        }



        //функция _getHost для сервера отличается только портом. Вместо 9050, отправлять всегда будем на 9051

        //static EndPoint _getHostS(string text)
        //{

        //    //вырезаем из строки только IP адрес формата IPv4

        //    string host = text.Remove(text.IndexOf(":"), text.Length - text.IndexOf(":"));



        //    //создаем объект адреса. Переменная host уже имеет вид [0-255].[0-255].[0-255].[0-255]

        //    IPAddress hostIPAddress = IPAddress.Parse(host);



        //    //создаем конечную точку. В нашем случае это адрес сервера, который слушает порт 9051

        //    IPEndPoint hostIPEndPoint = new IPEndPoint(hostIPAddress, opponentPort);// 9051);

        //    EndPoint To = (EndPoint)(hostIPEndPoint);

        //    return To;

        //}



        #region STUN

        public const string StunServer = "144.76.120.214";
        public const int StunServerPort = 3478;
        public static STUN_Result STUN;
        private static Socket socket;
        private static void StartReceiveCycle()
        {
            while (true)
            {
                if (socket.Available > 0)
                {
                    var buffer = new byte[100];
                    var length = socket.Receive(buffer);
                    if (length > 0)
                        Console.WriteLine("Received " + length + " bytes");
                }
                Thread.Sleep(100);
            }
        }
        private static void StartSendCycle()
        {
            var data = Encoding.ASCII.GetBytes("Hello!!");
            var sender = new UdpClient(AddressFamily.InterNetwork);
            sender.AllowNatTraversal(true);
            while (true)
            {
                sender.Send(data, data.Length, STUN.PublicEndPoint);
                Console.WriteLine("Sent " + data.Length + " bytes");
                Thread.Sleep(500);
            }
        }

        public static void InitSTUN()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Bind(new IPEndPoint(IPAddress.Any, 0));
            STUN = STUN_Client.Query(StunServer, StunServerPort, socket);
        }

        #endregion
    }
}
