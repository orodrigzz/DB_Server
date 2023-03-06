using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;


namespace Database_Server
{
    public class Database_Manager
    {
        public static Database_Manager _DATABASE_MANAGER;
        //Connection parameters
        const string connectionString = "Server=db4free.net;" + "Port=3306;" + "database=duckgame;" + "Uid=orodrigzz;" + "password=12345678;" + "SSL Mode=None;" + "connect timeout=3600;" + "default command timeout=3600;";
        MySqlConnection conn;
        MySqlDataReader reader;
        MySqlCommand cmd;

        //Database lists
        List<UsersData> users;

        //Class Constructor
        public Database_Manager()
        {
            conn = new MySqlConnection(connectionString);
        }

        private List<UsersData> findNicknames(string nickname)
        {
            string query = "SELECT * FROM users WHERE nickname = \"";
            query += nickname;
            query += "\"";

            cmd = conn.CreateCommand();
            cmd.CommandText = query;
            reader = cmd.ExecuteReader();

            List<UsersData> res = new List<UsersData>();
            while (reader.Read())
            {
                res.Add(new UsersData(reader));
            }
            reader.Close();
            return res;
        }

        private List<UsersData> findUsers(string usrnm, string pswrd)
        {
            string query = "SELECT * FROM users WHERE nickname = \"";
            query += usrnm;
            query += "\" AND password = \"";
            query += pswrd;
            query += "\";";

            cmd = conn.CreateCommand();
            cmd.CommandText = query;
            reader = cmd.ExecuteReader();

            List<UsersData> res = new List<UsersData>();
            while (reader.Read())
            {
                UsersData user = new UsersData(reader);
                res.Add(user);
            }
            reader.Close();
            return res;
        }

        private void insertUser(string usrnm, string psswrd, int id_race)
        {
            string query = "INSERT INTO users(nickname, password, id_race) VALUES(\"";
            query += usrnm;
            query += "\", \"";
            query += psswrd;
            query += "\",";
            query += id_race;
            query += ");";

            cmd = conn.CreateCommand();
            cmd.CommandText = query;
            reader = cmd.ExecuteReader();

            reader.Close();
        }

        public List<Race> getRaces()
        {
            string query = "SELECT * FROM races;";

            cmd = conn.CreateCommand();
            cmd.CommandText = query;
            reader = cmd.ExecuteReader();

            List<Race> res = new List<Race>();
            while (reader.Read())
            {
                res.Add(new Race(reader));
            }
            reader.Close();
            return res;
        }

        public void OpenConnection()
        {
            try 
            { 
                conn.Open();
                if (_DATABASE_MANAGER == null) { _DATABASE_MANAGER = this; }
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }

        public void CloseConnection()
        {
            conn.Close();
        }

        public List<UsersData> FindUsernames(string username) { return findNicknames(username); }

        public List<UsersData> FindUsers(string user, string pass) { return findUsers(user, pass); }

        public void InsertUser(string nickname, string password, int id_race) { insertUser(nickname, password, id_race); }

        public List<Race> GetRaces() { return getRaces(); }
    }
}