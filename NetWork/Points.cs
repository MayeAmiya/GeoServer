using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xamarin.Forms.PlatformConfiguration;

namespace GeoServer.NetWork
{
    public class Property
    {
        public string Name { get; set; }
        public int Index { get; set; }
        // Type当有约束 String ; Int Double Enum Bool ; List
        public string Type { get; set; }
        public object Object { get; set; }
        public string date { get; set; }

        // 通过检查updated和deleted来判断是否需要更新 事后只需根据索引号上传信息即可
        public bool deleted = false;
        public bool updated = false;

        public Property(string name, int index, string type, object obj)
        {
            Name = name;
            Index = index;
            Type = type;
            Object = obj;
            date = DateTime.Now.ToString();
        }

        public Property(Property property)
        {
            Name = property.Name;
            Index = property.Index;
            Type = property.Type;
            Object = property.Object;
            date = DateTime.Now.ToString();
        }
    }

    internal class Points
    {
        public class PointData
        {
            public double X { get; set; }
            public double Y { get; set; }
            public int PointInfCode { get; set; }
        }

        public static List<PointData> RequestPoints(int mapcode)
        {
            List<PointData> points = new List<PointData>();
            string sql = "SELECT x, y, PointInfCode FROM Points Where MapCode = @mapcode"; // 替换为你的表名

            using (MySqlConnection connection = Program.pool.GetConnection())
            {
                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    connection.Open();

                    command.Parameters.AddWithValue("@mapcode", mapcode);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // 从查询结果中提取数据
                            var x = reader.GetDouble("x");
                            var y = reader.GetDouble("y");
                            var pointInfCode = reader.GetInt32("PointInfCode");

                            var pointData = new PointData
                            {
                                X = x,
                                Y = y,
                                PointInfCode = pointInfCode
                            };

                            points.Add(pointData);
                        }
                    }
                    return points;
                }
            }
        }

        public static Dictionary<int, Property> IndexRequest(int mapcode)
        {
            string sql = "SELECT Name, PIndex, Type, Object FROM Properties Where MapCode = @mapcode"; // 替换为你的表名

            Dictionary<int, Property> basicInfo = new Dictionary<int, Property>();

            using (MySqlConnection connection = Program.pool.GetConnection())
            {
                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    connection.Open();
                    command.Parameters.AddWithValue("@mapcode", mapcode);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string name = reader.IsDBNull(reader.GetOrdinal("Name")) ? null : reader.GetString("Name");
                            var index = reader.GetInt32("PIndex");
                            var type = reader.GetString("Type");
                            string obj_content = reader.IsDBNull(reader.GetOrdinal("Object")) ? null : reader.GetString("Object");

                            object obj;

                            // 根据 Type 进行处理
                            if (type is "Enum" or "Page" or "EnumList")
                            {
                                var list = JsonConvert.DeserializeObject<List<int>>(obj_content as string);
                                basicInfo.Add(index, new Property(name, index, type, list));
                                // 处理 list
                            }
                            else if (type is "String" or "EnumItem")
                            {
                                string strValue = obj_content as string;
                                basicInfo.Add(index, new Property(name, index, type, strValue));
                            }
                        }
                        return basicInfo;
                    }

                }
            }
        }

        public static string Search(string root)
        {
            List<int> temp = new List<int>();

            using (MySqlConnection connection = Program.pool.GetConnection())
            {
                string query = "SELECT * FROM Properties WHERE Object LIKE @target AND MapCode = @mapcode;";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    connection.Open();

                    dynamic jsonObject = JsonConvert.DeserializeObject(root);
                    string target = jsonObject.target;
                    string mapcode = jsonObject.mapcode;

                    command.Parameters.AddWithValue("@target", "%" + target + "%");
                    command.Parameters.AddWithValue("@MapCode", mapcode);

                    
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // 在这里访问你的数据
                            temp.Add(reader.GetInt32("PIndex"));
                        }
                        
                    }
                }
            }

            using (MySqlConnection connection = Program.pool.GetConnection())
            {
                string query = "SELECT * FROM Properties WHERE Name LIKE @target AND MapCode = @mapcode;";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    connection.Open();

                    dynamic jsonObject = JsonConvert.DeserializeObject(root);
                    string target = jsonObject.target;
                    string mapcode = jsonObject.mapcode;

                    command.Parameters.AddWithValue("@target", "%" + target + "%");
                    command.Parameters.AddWithValue("@MapCode", mapcode);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // 在这里访问你的数据
                            temp.Add(reader.GetInt32("PIndex"));
                        }

                    }


                }
            }

            HashSet<int> uniqueItems = new HashSet<int>(temp);

            // 将 HashSet 转换回 List
            List<int> result = new List<int>(uniqueItems);

            var send = new
            {
                found = result
            };

            return JsonConvert.SerializeObject(send);
        }
    }
}
