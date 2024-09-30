// See https://aka.ms/new-console-template for more information

// 这里启动服务端呢
using GeoServer;
using GeoServer.NetWork;
using MySqlX.XDevAPI;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using System.Text.Json.Nodes;
using static GeoServer.NetWork.Points;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text.Json;
using System.Linq;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Reflection.Emit;


internal partial class Program
{
    public static DB_Connection_Pool? pool;

    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        // 建立一个TCP连接
        TcpListener listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 10721);
        listener.Start();

        string connectionString = "server=127.0.0.1;user=kei;password=213Zzdx@sb;database=GeoGraph;Pooling=true;";
        pool = new DB_Connection_Pool(connectionString);

        // 建立数据库连接池

        // 不同的输入启动不同的处理程序 不同处理程序调用DB的connection


        Console.WriteLine("服务器已启动，等待连接...");

        while (true)
        {
            TcpClient client = listener.AcceptTcpClient();
            ThreadPool.QueueUserWorkItem(HandleClient, client);
        }
    }

    static async void HandleClient(object clientObj)
    {
        NetworkStream stream = ((TcpClient)clientObj).GetStream();

        using (StreamReader reader = new StreamReader(stream))
        using (StreamWriter writer = new StreamWriter(stream) { AutoFlush = true })
        {
                // 处理客户端连接
                Console.WriteLine("客户端已连接");

            // 在这里可以添加更多的处理逻辑，比如读取数据等
            
            {
                // 读取客户端发送的数据
                Console.WriteLine("Recv");
                string jsonRequest;
                try 
                { 
                    jsonRequest = await reader.ReadLineAsync();
                }
                catch
                {
                    return;
                }

                Console.WriteLine("RecvED");
                Console.WriteLine(jsonRequest);

                var jsonDocument = JsonDocument.Parse(jsonRequest);
                var root = jsonDocument.RootElement;
                string command = root.GetProperty("command").GetString();
                string token = root.GetProperty("token").GetString();

                Console.WriteLine(command);
                string response = null;
                switch (command)
                {
                    case "Captcha":
                        {
                            string cap = Users.CaptchaGenerator.returnCaptcha();
                            var Data = new
                            {
                                response = "captcha",
                                captcha = cap
                            };
                            // 发送响应
                            response = JsonConvert.SerializeObject(Data);
                        }
                        break;
                    case "login":
                        // 登录处理
                        {
                            string username = root.GetProperty("user").GetString();
                            string password = root.GetProperty("hash").GetString();
                            Console.WriteLine(username);
                            var ret = Users.newlogin(username,password);
                            var Data = new
                            {
                                response = ret
                            };

                            response = JsonConvert.SerializeObject(Data);
                        }
                        break;
                    case "register":
                        {// 注册处理
                            
                            string username = root.GetProperty("user").GetString();
                            string password = root.GetProperty("hash").GetString();
                            string ret = Users.newregister(username, password);
                            Console.WriteLine(ret);
                            var Data = new
                            {
                                response = ret
                            };
                            response = JsonConvert.SerializeObject(Data);
                        }
                        break;
                    case "reset":
                        {// 重设密码
                            string username = root.GetProperty("user").GetString();
                            string password = root.GetProperty("hash").GetString();
                            string ret = Users.newpassword(username, password);
                            var Data = new
                            {
                                response = ret
                            };
                            response = JsonConvert.SerializeObject(Data);
                        }
                        break;

                    case "updateProperties":
                        {
                            JsonElement propertiesArray = root.GetProperty("properties");
                            int map = root.GetProperty("map").GetInt32();
                            await Update.UpdateProperties(propertiesArray, map);
                            var Data = new
                            {
                                response = "updatecomplete"
                            };
                            response = JsonConvert.SerializeObject(Data);
                        }
                        break;
                    case "updatePoints":
                        {
                            JsonElement pointsArray = root.GetProperty("points");
                            int map = root.GetProperty("map").GetInt32();
                            await Update.UpdatePoints(pointsArray,map);
                            var Data = new
                            {
                                response = "updatecomplete"
                            };
                            response = JsonConvert.SerializeObject(Data);
                        }
                        break;
                    case "updateUsers":
                        {
                            JsonElement usersArray = root.GetProperty("users");
                            await Update.UpdateUsers(usersArray);
                            var Data = new
                            {
                                response = "updatecomplete"
                            };
                            response = JsonConvert.SerializeObject(Data);
                        }
                        break;
                    case "updateMaps":
                        {
                            JsonElement mapsArray = root.GetProperty("maps");
                            await Update.UpdateMaps(mapsArray);
                            var Data = new
                            {
                                response = "updatecomplete"
                            };
                            response = JsonConvert.SerializeObject(Data);
                        }
                        break;

                    case "Users":
                        {
                            List<Users> userList = Users.RequestUserLists();

                            // 将用户列表序列化为 JSON
                            var Data = new
                            {
                                response = "Users",
                                content = userList
                            };

                            response = JsonConvert.SerializeObject(Data);
                        }
                        break;
                    case "Maps":
                        {
                            List<MapInfo> MapList = Maps.RequestMapLists();
                            var Data = new
                            {
                                response = "Maps",
                                content = MapList
                            };
                            response = JsonConvert.SerializeObject(Data);
                        }
                        break;
                    case "Points":
                        {
                            int map = root.GetProperty("map").GetInt32();
                            List<PointData> ret = Points.RequestPoints(map);
                            var Data = new
                            {
                                response = "Points",
                                content = ret
                            };
                            response = JsonConvert.SerializeObject(Data);
                        }
                        break;
                    case "Properties":
                        {
                            int map = root.GetProperty("map").GetInt32();
                            Dictionary<int, Property> ret = Points.IndexRequest(map);
                            var Data = new
                            {
                                response = "Properties",
                                content = ret
                            };
                            response = JsonConvert.SerializeObject(Data);
                        }
                        break;
                    case "download":
                        {
                           response = Files.Download(jsonRequest);
                        }
                        break;
                    case "upload":
                        {
                            response = Files.Upload(jsonRequest);
                        }
                        break;
                    case "search":
                        {
                            response = Points.Search(jsonRequest);
                        }
                        break;
                    case "exit":
                        Users.exit(token);
                        break;
                    default:
                        {
                            var Data = new
                            {
                                response = "IDK",
                            };
                            response = JsonConvert.SerializeObject(Data);
                        }
                        break;
                }
                await writer.WriteLineAsync(response);
                Console.WriteLine("Responsed" + response);
            }
        }
    }
}