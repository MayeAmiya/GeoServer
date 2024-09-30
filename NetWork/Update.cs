using MySql.Data.MySqlClient;
using Mysqlx.Crud;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GeoServer.NetWork
{
    internal class Update
    {
        public class MapInfo
        {
            public string MapName { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public string MapInf { get; set; }
            public int MapCode { get; set; }
            public string ImagePath { get; set; }

            public bool Deleted { get; set; }
        }

        public class RequestDataMap
        {
            public string Command { get; set; }
            public List<MapInfo> Maps { get; set; }
        }

        public static async Task UpdateMaps(JsonElement mapsArray)
        {
            string jsonString = mapsArray.GetRawText();
            var requestData = JsonConvert.DeserializeObject<List<MapInfo>>(jsonString);
            using (MySqlConnection connection = Program.pool.GetConnection())
            {
                connection.Open();

                foreach (var map in requestData)
                {
                    // 检查地图是否已存在
                    int t;
                    using (MySqlConnection connectionC = Program.pool.GetConnection())
                    {
                        string selectQuery = "SELECT COUNT(*) FROM Maps WHERE MapCode = @MapCode";
                        connectionC.Open();
                        using (var commandC = new MySqlCommand(selectQuery, connectionC))
                        {
                            commandC.Parameters.AddWithValue("@MapCode", map.MapCode);

                            t = (int)(long)await commandC.ExecuteScalarAsync();
                        }
                    }

                    if (t != 0)
                    {
                        if (map.Deleted)
                        {
                            // 删除原有属性
                            string deleteQuery = "DELETE FROM Maps WHERE MapCode = @MapCode";
                            using (var command = new MySqlCommand(deleteQuery, connection))
                            {
                                command.Parameters.AddWithValue("@MapCode", map.MapCode);
                                await command.ExecuteNonQueryAsync();
                            }
                            return;
                        }

                        // 更新已存在的地图
                        string updateQuery = @"
                            UPDATE Maps 
                            SET MapName = @MapName, 
                                Width = @Width, 
                                Height = @Height, 
                                MapInf = @MapInf 
                            WHERE MapCode = @MapCode";

                        using (var updateCommand = new MySqlCommand(updateQuery, connection))
                        {
                            updateCommand.Parameters.AddWithValue("@MapName", map.MapName);
                            updateCommand.Parameters.AddWithValue("@Width", map.Width);
                            updateCommand.Parameters.AddWithValue("@Height", map.Height);
                            updateCommand.Parameters.AddWithValue("@MapInf", map.MapInf);
                            updateCommand.Parameters.AddWithValue("@MapCode", map.MapCode);
                            await updateCommand.ExecuteNonQueryAsync();
                        }
                    }
                    else
                    {
                        // 插入新地图
                        string insertQuery = @"
                            INSERT INTO Maps (MapName, Width, Height, MapInf, MapCode) 
                            VALUES (@MapName, @Width, @Height, @MapInf, @MapCode)";

                        using (var insertCommand = new MySqlCommand(insertQuery, connection))
                        {
                            insertCommand.Parameters.AddWithValue("@MapName", map.MapName);
                            insertCommand.Parameters.AddWithValue("@Width", map.Width);
                            insertCommand.Parameters.AddWithValue("@Height", map.Height);
                            insertCommand.Parameters.AddWithValue("@MapInf", map.MapInf);
                            insertCommand.Parameters.AddWithValue("@MapCode", map.MapCode);
                            await insertCommand.ExecuteNonQueryAsync();
                        }
                    }
                }
            }
        }
    


        public class RequestDataU
        {
            public string Command { get; set; }
            public List<User> Users { get; set; }
        }

        // User 类定义
        public class User
        {
            public string Username { get; set; }
            public string Password { get; set; }
            public string Permission { get; set; }
            public string UserRank { get; set; }
        }

        // 插入MapUsers
        public static async Task UpdateUsers(JsonElement usersArray)
        {
            string jsonString = usersArray.GetRawText();
            var requestData = JsonConvert.DeserializeObject<List<User>>(jsonString);

            using (MySqlConnection connection = Program.pool.GetConnection())
            {
                await connection.OpenAsync();

                foreach (var user in requestData)
                {
                    // 检查用户是否已存在
                    string selectQuery = "SELECT COUNT(*) FROM MapUsers WHERE Username = @Username";
                    using (var command = new MySqlCommand(selectQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Username", user.Username);
                        int count = Convert.ToInt32(await command.ExecuteScalarAsync());

                        if (count > 0)
                        {
                            // 更新已存在的用户
                            string updateQuery = @"
                            UPDATE MapUsers 
                            SET Permission = @Permission, 
                                UserRank = @UserRank 
                            WHERE Username = @Username";

                            using (var updateCommand = new MySqlCommand(updateQuery, connection))
                            {
                                updateCommand.Parameters.AddWithValue("@Permission", user.Permission);
                                updateCommand.Parameters.AddWithValue("@UserRank", user.UserRank);
                                updateCommand.Parameters.AddWithValue("@Username", user.Username);
                                await updateCommand.ExecuteNonQueryAsync();
                            }
                        }
                    }
                }
            }
        }

        public class RequestDataM
        {
            public string Command { get; set; }
            public List<PointData> Points { get; set; }
        }

        public class PointData
        {
            public double x { get; set; }
            public double y { get; set; }
            public bool Deleted { get; set; }
            public int PointInfCode { get; set; }
        }

        public static async Task UpdatePoints(JsonElement pointsArray, int mapcode)
        {
            string jsonString = pointsArray.GetRawText();
            int MapCode = mapcode;
            var requestData = JsonConvert.DeserializeObject<List<PointData>>(jsonString);

            using (MySqlConnection connection = Program.pool.GetConnection())
            {
                await connection.OpenAsync();

                foreach (var point in requestData)
                {
                    double x = point.x;
                    double y = point.y;
                    int PointInfCode = point.PointInfCode;
                    if (point.Deleted)
                    {
                        // 删除原有点
                        string deleteQuery = "DELETE FROM Points WHERE X = @X AND Y = @Y";
                        using (var command = new MySqlCommand(deleteQuery, connection))
                        {
                            command.Parameters.AddWithValue("@X", x);
                            command.Parameters.AddWithValue("@Y", y);
                            await command.ExecuteNonQueryAsync();
                        }
                    }
                    else
                    {
                        // 查找原有点
                        string selectQuery = "SELECT COUNT(*) FROM Points WHERE X = @X AND Y = @Y";
                        using (var command = new MySqlCommand(selectQuery, connection))
                        {
                            command.Parameters.AddWithValue("@X", x);
                            command.Parameters.AddWithValue("@Y", y);
                            int count = Convert.ToInt32(await command.ExecuteScalarAsync());

                            if (count > 0)
                            {
                                // 如果存在，则更新
                                string updateQuery = "UPDATE Points SET PointInfCode = @PointInfCode WHERE X = @X AND Y = @Y";
                                using (var updateCommand = new MySqlCommand(updateQuery, connection))
                                {
                                    updateCommand.Parameters.AddWithValue("@X", x);
                                    updateCommand.Parameters.AddWithValue("@Y", y);
                                    updateCommand.Parameters.AddWithValue("@PointInfCode", PointInfCode);
                                    await updateCommand.ExecuteNonQueryAsync();
                                }
                            }
                            else
                            {
                                // 如果不存在，则插入新点
                                string insertQuery = "INSERT INTO Points (X, Y, PointInfCode, MapCode) VALUES (@X, @Y, @PointInfCode, @MapCode)";
                                using (var insertCommand = new MySqlCommand(insertQuery, connection))
                                {
                                    insertCommand.Parameters.AddWithValue("@X", x);
                                    insertCommand.Parameters.AddWithValue("@Y", y);
                                    insertCommand.Parameters.AddWithValue("@PointInfCode", PointInfCode.ToString());
                                    insertCommand.Parameters.AddWithValue("@MapCode", MapCode.ToString());
                                    await insertCommand.ExecuteNonQueryAsync();
                                }
                            }
                        }
                    }
                }
            }
        }



        public class PropertyData
        {
            public string Name { get; set; }
            public int Index { get; set; }
            public string Type { get; set; }
            public object Object { get; set; }
            public bool Updated { get; set; }
            public bool Deleted { get; set; }
        }

        public static async Task UpdateProperties(JsonElement propertiesArray, int mapcode)
        {
            // 解析 JSON 数据
            int MapCode = mapcode;
            string jsonString = propertiesArray.GetRawText();
            var requestData = JsonConvert.DeserializeObject<List<PropertyData>>(jsonString);

            using (MySqlConnection connection = Program.pool.GetConnection())
            {
                await connection.OpenAsync();

                foreach (var property in requestData)
                {
                    if (property.Deleted)
                    {
                        // 删除原有属性
                        string deleteQuery = "DELETE FROM Properties WHERE PIndex = @PIndex";
                        using (var command = new MySqlCommand(deleteQuery, connection))
                        {
                            command.Parameters.AddWithValue("@PIndex", property.Index);
                            await command.ExecuteNonQueryAsync();
                        }
                    }
                    else
                    {
                        // 查找原有属性
                        string selectQuery = "SELECT COUNT(*) FROM Properties WHERE PIndex = @PIndex";
                        using (var command = new MySqlCommand(selectQuery, connection))
                        {
                            command.Parameters.AddWithValue("@PIndex", property.Index);
                            int count = Convert.ToInt32(await command.ExecuteScalarAsync());

                            if (count > 0)
                            {
                                // 如果存在，则更新
                                string updateQuery = "UPDATE Properties SET Type = @Type, PIndex = @PIndex, Object = @Object WHERE Name = @Name";
                                using (var updateCommand = new MySqlCommand(updateQuery, connection))
                                {
                                    updateCommand.Parameters.AddWithValue("@Type", property.Type);
                                    updateCommand.Parameters.AddWithValue("@PIndex", property.Index);

                                    if (property.Type is "Enum" or "Page" or "EnumList")
                                    {
                                        string objectJson = JsonConvert.SerializeObject(property.Object);
                                        updateCommand.Parameters.AddWithValue("@Object", objectJson);
                                    }
                                    else if (property.Type is "String" or "EnumItem")
                                    {
                                        updateCommand.Parameters.AddWithValue("@Object", property.Object);
                                    }
                                    else
                                    {
                                        // 处理其他类型（如 null 或其他类型），根据需要决定如何存储
                                        updateCommand.Parameters.AddWithValue("@Object", DBNull.Value);
                                    }
                                    updateCommand.Parameters.AddWithValue("@Name", property.Name);
                                    await updateCommand.ExecuteNonQueryAsync();
                                }
                            }
                            else
                            {
                                // 如果不存在，则插入新属性
                                string insertQuery = "INSERT INTO Properties (Name, Type, PIndex, Object, MapCode) VALUES (@Name, @Type, @PIndex, @Object, @MapCode)";
                                using (var insertCommand = new MySqlCommand(insertQuery, connection))
                                {
                                    insertCommand.Parameters.AddWithValue("@Name", property.Name);
                                    insertCommand.Parameters.AddWithValue("@Type", property.Type);
                                    insertCommand.Parameters.AddWithValue("@PIndex", property.Index);
                                    insertCommand.Parameters.AddWithValue("@Object", property.Object);
                                    insertCommand.Parameters.AddWithValue("@MapCode", MapCode);
                                    await insertCommand.ExecuteNonQueryAsync();
                                }
                            }
                        }
                    }
                }
            }
        }
    }

}
