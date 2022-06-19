using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Security.Permissions;
using MySql.Data.MySqlClient;
using TShockAPI;
using TShockAPI.DB;

namespace POBC.TaskSystem
{
    public static class Db
    {
        //连接mysql数据库
        public static void Connect()
        {
            SqlTableCreator sqlcreator = new SqlTableCreator(TShock.DB, TShock.DB.GetSqlType() == SqlType.Sqlite ? (IQueryBuilder)new SqliteQueryCreator() : new MysqlQueryCreator());

            sqlcreator.EnsureTableStructure(new SqlTable("POBCTask",
                new SqlColumn("ID", MySqlDbType.Int32) { Primary = true, Unique = true, Length = 7, AutoIncrement = true },
                new SqlColumn("UserName", MySqlDbType.Text) { Length = 500 },
                new SqlColumn("MianTaskUser", MySqlDbType.Text) { Length = 500 },
                new SqlColumn("MianTaskData", MySqlDbType.Text) { Length = 500 },
                new SqlColumn("MianTaskCompleted", MySqlDbType.Int32) { Length = 255 },
                new SqlColumn("RegionalTaskUser", MySqlDbType.Text) { Length = 500 },
                new SqlColumn("RegionalTaskData", MySqlDbType.Text) { Length = 500 },
                new SqlColumn("RegionalCompleted", MySqlDbType.Int32) { Length = 255 }));      
        }
        public static bool Queryuser(string user)
        {
            bool u;
            //  string query = "SELECT * FROM POBC WHERE UserName = @user";
            using (QueryResult reader = TShock.DB.QueryReader("SELECT * FROM POBCTask WHERE UserName = @0", user))
            {
                if (reader.Read())
                {
                    u = true;
                }
                else
                {
                    u = false;
                }
                return u;
            }
        }
        public static DBData QueryData(string user)
        {
            DBData dBData = new DBData();
            //string query = $"SELECT Currency FROM POBC WHERE UserName = '{user}'";
            using (QueryResult reader = TShock.DB.QueryReader("SELECT UserName,MianTaskUser,MianTaskData,MianTaskCompleted,RegionalTaskUser,RegionalTaskData,RegionalCompleted FROM POBCTask WHERE UserName = @0", user))
            {
                if (reader.Read())
                {
                    //dBData.ID = reader.Get<int>("ID");
                    dBData.UserName = reader.Get<string>("UserName");
                    dBData.MianTaskUser = reader.Get<string>("MianTaskUser");
                    dBData.MianTaskData = reader.Get<string>("MianTaskData");
                    dBData.MianTaskCompleted = reader.Get<int>("MianTaskCompleted");
                    dBData.RegionalTaskUser = reader.Get<string>("RegionalTaskUser");
                    dBData.RegionalTaskData = reader.Get<string>("RegionalTaskData");
                    dBData.RegionalCompleted = reader.Get<int>("RegionalCompleted");
                }
                else
                {
                    dBData = null;
                }
            }
            return dBData;
        }

        public static void Adduser(DBData dBData)
        {
            TShock.DB.Query("INSERT INTO POBC (UserName,MianTaskUser,MianTaskData,MianTaskCompleted,RegionalTaskUser,RegionalTaskData,RegionalCompleted) VALUES (@0,@1,@2,@3,@4,@5,@6)", dBData.UserName, dBData.MianTaskUser, dBData.MianTaskData, dBData.MianTaskCompleted, dBData.RegionalTaskUser, dBData.RegionalTaskData, dBData.RegionalCompleted);          
        }
    }


}
    

