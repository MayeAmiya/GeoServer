using MySqlX.XDevAPI;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace GeoServer.NetWork
{
    internal class Files
    {
        private const string FileDirectory = "./cache";

        public static string Download(string root)
        {
            string directoryPath = "./cache";

            // 检查目录是否存在，如果不存在则创建它
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            dynamic jsonObject = JsonConvert.DeserializeObject(root);
            string filename = jsonObject.filename;
            string type = jsonObject.type;

            string fullPath,path;
            switch (type)
            {
                case "map":
                    path = Path.Combine("./cache", "map");
                    fullPath = Path.Combine(path, filename + ".png");
                    break;
                case "captcha":
                    path = Path.Combine("./cache", "captcha");
                    fullPath = Path.Combine(path, filename + ".png");
                    break;
                case "user":
                    path = Path.Combine("./cache", "user");
                    fullPath = Path.Combine(path, filename + ".png");
                    break;
                default:
                    fullPath = Path.Combine("./cache", filename + ".png"); ;
                    break;
            }

            if (!File.Exists(fullPath))
            {
                var request = new
                {
                    filename = "404notfound",
                    data = ""
                };

                string jsonRequest = JsonConvert.SerializeObject(request);

                return jsonRequest;
            }
            else
            {
                byte[] imageBytes = File.ReadAllBytes(fullPath);
                string base64String = Convert.ToBase64String(imageBytes);

                var request = new
                {
                    filename = filename,
                    data = base64String
                };

                string jsonRequest = JsonConvert.SerializeObject(request);

                return jsonRequest;
            }
        }


        public static string Upload(string root)
        {
            string directoryPath = "./cache";

            // 检查目录是否存在，如果不存在则创建它
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            Console.WriteLine("Files Requested");

            dynamic jsonObject = JsonConvert.DeserializeObject(root);

            string base64Data = jsonObject.data;
            string filename = jsonObject.filename;

            byte[] imageBytes = Convert.FromBase64String(base64Data);

            string type = jsonObject.type;

            string fullPath, path;
            switch (type)
            {
                case "map":
                    path = Path.Combine("./cache", "map");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    fullPath = Path.Combine(path, filename + ".png");
                    break;
                case "captcha":
                    path = Path.Combine("./cache", "captcha");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    fullPath = Path.Combine(path, filename + ".png");
                    break;
                case "user":
                    path = Path.Combine("./cache", "user");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    fullPath = Path.Combine(path, filename + ".png");
                    break;
                default:
                    fullPath = Path.Combine("./cache", filename + ".png"); ;
                    break;
            }

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }


            File.WriteAllBytes(fullPath, imageBytes);

            var response = new
            {
                response = "success"
            };

            string jsonRequest = JsonConvert.SerializeObject(response);

            return jsonRequest;
        }
    }
}
