using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Asn1;

class DB_MANAGER
{
    static void Main(string[] args)
    {
        const string connectionString = "Server=db4free.net;"+"Port=3306;"+"database=duckgame;" + "Uid=orodrigzz;"+"password=12345678;"+"SSL Mode=None;"+"connect timeout=3600;"+"default command timeout=3600;";
        
        MySqlConnection mySqlConnection = new MySqlConnection(connectionString); 
        
        try
        {
            mySqlConnection.Open(); 

        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.Message);  
        }

        //select(mySqlConnection);
        //insertOrDelete(mySqlConnection, "INSERT INTO users values(1,'Jotauveele', '123', 1);");

        void select(MySqlConnection mySqlConnection)
        {
            MySqlDataReader reader;

            MySqlCommand command = mySqlConnection.CreateCommand();

            command.CommandText = "Select * from users;";

            try
            {
                reader = command.ExecuteReader();

                while(reader.Read())
                {
                    Console.WriteLine(reader["Nick"].ToString());
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        void insertOrDelete(MySqlConnection mySqlConnection, string query)
        {
            MySqlCommand command = mySqlConnection.CreateCommand();

            command.CommandText = query;
            try
            {
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        mySqlConnection.Close();        
    }
}
