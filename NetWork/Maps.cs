using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GeoServer.NetWork
{
    public class MapInfo
    {
        public string MapName;
        public int Width;
        public int Height;

        public string MapInf;
        public int MapCode;
        public int MapRank;
        public bool Changed;
        public bool Deleted;

        // struct is deep copy

        public MapInfo(MapInfo temp)
        {
            MapName = temp.MapName;
            Width = temp.Width;
            Height = temp.Height;

            MapInf = temp.MapInf;
            MapCode = temp.MapCode;
            MapRank = temp.MapRank;

        }

        public MapInfo(string MapName, int Width, int Height,  string MapInf, int MapCode)
        {
            this.MapName = MapName;
            this.Width = Width;
            this.Height = Height;

            this.MapInf = MapInf;
            this.MapCode = MapCode;
            this.MapRank = 0;
        }

    }

    internal class Maps
    {

        public static List<MapInfo> RequestMapLists()
        {
            // 查询地图列表
            string sql = "SELECT * FROM Maps";
            List<MapInfo> MapList = new List<MapInfo>();

            using (MySqlConnection connection = Program.pool.GetConnection())
            {
                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // 从查询结果中提取数据
                            string mapName = reader["MapName"].ToString();
                            string mapInf = reader["MapInf"].ToString();
                            int width = reader.GetInt32("Width");
                            int height = reader.GetInt32("Height");
                            int mapCode = reader.GetInt32("MapCode");

                            // 将地图信息添加到列表中
                            MapList.Add(new MapInfo(mapName, width, height, mapInf, mapCode));
                        }
                    }
                }
            }
            // MapList 转为string
            return MapList;
        }
    }


}
