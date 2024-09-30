using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

using static GeoServer.NetWork.Update;
using static Org.BouncyCastle.Asn1.Cmp.Challenge;
using static System.Net.Mime.MediaTypeNames;

namespace GeoServer.NetWork
{
    class userinf
    {
        public string permission;
        public string userInfo;
        public int userRank;
        public string token;
        public userinf(string permission, string userInfo, int userRank, string token)
        {
            this.permission = permission;
            this.userInfo = userInfo;
            this.userRank = userRank;
            this.token = token;
        }
    }

    internal class Users
    {
        public string username;

        public string permission;

        public string userInfo;

        public int userRank;

        public string token;

        public Users(string username, string permission, string userInfo, int userRank, string token = null)
        {
            this.username = username;
            this.permission = permission;
            this.userInfo = userInfo;
            this.userRank = userRank;
            this.token = token;
        }

        public static List<Users> userlist = new List<Users>();

        public static object newlogin(string username, string password)
        {
            string checkQuery = "SELECT COUNT(*) FROM MapUsers WHERE Username = @Username";
            using (MySqlConnection connection = Program.pool.GetConnection())
            {
                using (MySqlCommand commandc = new MySqlCommand(checkQuery, connection))
                {
                    commandc.Parameters.AddWithValue("@Username", username);
                    connection.Open();
                    var result = commandc.ExecuteScalar();

                    if ((int)(long)result == 0)
                    {
                        var user = new
                        {
                            permission = "",
                            userInfo = "",
                            userRank = 0,
                            token = "usernotexist"
                        };

                        return user;
                     
                    }

                }
            }
            string query = "SELECT password,userinfo,userrank,permission FROM MapUsers WHERE Username = @Username";
            // 登录验证 从数据库查找密码
            using (MySqlConnection connection = Program.pool.GetConnection())
            {

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    connection.Open();

                    username = username.Trim().Trim('\'');
                    command.Parameters.AddWithValue("@Username", username);

                    using (var reader = command.ExecuteReader())
                    {
                        string Password = null;
                        string userInfo = null;
                        string Permission = null;
                        int userRank = 0;
                        string token = null;

                        if (reader.Read())
                        {
                            Password = reader.GetString("password");
                            Permission = reader.GetString("permission");
                            userInfo = reader.GetString("userinfo");
                            userRank = reader.GetInt32("userrank");
                            token = Guid.NewGuid().ToString();
                        }

                        Console.WriteLine(password+" "+Password);

                        if (password == Password)
                        {
                            var user = new
                            {
                                permission = Permission,
                                userInfo = userInfo,
                                userRank = userRank,
                                token = token
                            };
                            // 查找用户
                            userlists(token);

                            Users usertemp = userlist.FirstOrDefault(u => u.username == username);

                            if (usertemp == null)
                            {
                                usertemp.token = token; // 更新用户的 token
                            }
                            else
                            {
                                token = "alreadylogin";
                            }
                            return user;
                        }
                        else
                        {
                            var user = new
                            {
                                permission = Permission,
                                userInfo = userInfo,
                                userRank = userRank,
                                token = "passworderror"
                            };
                            return user;
                        }
                    }

                }
            }
        }

        public static string newregister(string username, string password)
        {
            // 注册新用户

            // 插入新用户的 SQL 查询
            //
            string checkQuery = "SELECT COUNT(*) FROM MapUsers WHERE Username = @Username";
            using (MySqlConnection connection = Program.pool.GetConnection())
            {
                using (MySqlCommand commandc = new MySqlCommand(checkQuery, connection))
                {
                    commandc.Parameters.AddWithValue("@Username", username);
                    connection.Open();
                    var result = commandc.ExecuteScalar();
                    
                    if ((int)(long)result >0)
                    {
                        return "existed";
                    }

                }
            }


            using (MySqlConnection connection = Program.pool.GetConnection())
            {
                using (MySqlCommand command = new MySqlCommand("InsertMapUser", connection))
                {
                    // 设置参数
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@p_Username", username);
                    command.Parameters.AddWithValue("@p_Password", password);
                    command.Parameters.AddWithValue("@p_Permission", "Write");
                    command.Parameters.AddWithValue("@p_UserRank", 6);
                    command.Parameters.AddWithValue("@p_UserInfo", "null");

                    connection.Open();

                    try
                    {
                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            Console.WriteLine(password);
                            return "success";
                        }
                        else
                        {
                            return "failed";
                        }
                    }
                    catch (MySqlException ex)
                    {
                        // 处理数据库异常
                        return $"注册失败: {ex.Message}";
                    }
                }
            }
        }

        public static string newpassword(string username, string password)
        {
            // 修改密码
            string query = "UPDATE MapUsers SET Password = @Password WHERE Username = @Username";

            using (MySqlConnection connection = Program.pool.GetConnection())
            {
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    // 设置参数
                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@Password", password); // 注意：实际应用中，密码应该被哈希处理

                    connection.Open();

                    try
                    {
                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return "ReSet success";
                        }
                        else
                        {
                            return "Failed";
                        }
                    }
                    catch (MySqlException ex)
                    {
                        // 处理数据库异常
                        return $"修改失败: {ex.Message}";
                    }
                }
            }
        }

        public static void userlists(string token)
        {
            userlist.Clear();
            string query = "SELECT Username, Permission, UserInfo, UserRank FROM MapUsers"; // 根据你的表结构调整

            using (MySqlConnection connection = Program.pool.GetConnection())
            {
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    connection.Open();
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string username = reader.GetString("Username");
                            string permission = reader.GetString("Permission");
                            string userInfo = reader.GetString("UserInfo");
                            int userRank = reader.GetInt32("UserRank");

                            Users user = new Users(username, permission, userInfo, userRank);
                            userlist.Add(user);
                        }
                    }
                }
            }
        }

        public static List<Users> RequestUserLists()
        {
            return userlist;
        }

        public static void exit(string token)
        {
            if(userlist.Any(user => user.token == token))
            {
                Users user = userlist.FirstOrDefault(u => u.token == token);
                user.token = null;
            }
        }
        public class CaptchaGenerator
        {
            public static Bitmap GenerateCaptcha(string text, int width, int height)
            {
                Bitmap bitmap = new Bitmap(width, height);
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    // 背景颜色
                    g.Clear(System.Drawing.Color.White);

                    Random rand = new Random();
                    // 画文本
                    {
                        // 随机颜色
                        
                        Color[] colors = { Color.Black, Color.Red, Color.Blue, Color.Green, Color.Purple };

                        for (int i = 0; i < text.Length; i++)
                        {
                            System.Drawing.Font font = new System.Drawing.Font("Arial", rand.Next(60, 100), FontStyle.Bold);
                            // 随机位置
                            float x = 10 + (i * 50) + rand.Next(-20, 20);
                            float y = 10 + rand.Next(-30, 30);

                            // 随机选择颜色
                            Brush brush = new SolidBrush(colors[rand.Next(colors.Length)]);
                            // 画文本
                            g.DrawString(text[i].ToString(), font, brush, new PointF(x, y));
                        }
                    }

                    // 添加干扰线
                    for (int i = 0; i < 10; i++)
                    {
                        g.DrawLine(Pens.Gray, rand.Next(0, width), rand.Next(0, height), rand.Next(0, width), rand.Next(0, height));
                    }

                    // 添加噪点
                    for (int i = 0; i < 100; i++)
                    {
                        g.FillEllipse(Brushes.LightGray, rand.Next(0, width), rand.Next(0, height), 2, 2);
                    }
                }
                return bitmap;
            }

            public static void SaveCaptchaImage(Bitmap captchaImage, string filePath)
            {
                string directoryPath = "./cache";

                // 检查目录是否存在，如果不存在则创建它
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                captchaImage.Save(filePath, ImageFormat.Png);
            }

            public static string returnCaptcha()
            {
                // 生成一个六位随机字符串
                string captcha = new Random().Next(100000, 999999).ToString();

                if (!Directory.Exists("./cache/captcha"))
                {
                    Directory.CreateDirectory("./cache/captcha");
                }

                SaveCaptchaImage(GenerateCaptcha(captcha, 400, 150), $"./cache/captcha/{captcha}.png");
                return captcha;
            }
        }

        // 使用示例
    }
}
