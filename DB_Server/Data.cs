using System.Data;
using System.Data.Common;

namespace Database_Server
{
    public struct UsersData
    {
        public int id_user;
        public string nickname;
        public string password;
        public int id_race;

        public UsersData(IDataReader dataReader)
        {
            id_user = dataReader.GetInt32(0);
            nickname = dataReader.GetString(1);
            password = dataReader.GetString(2);
            id_race = dataReader.GetInt32(3);
        }
    }

    public struct Race
    {
        public int id_race;
        public int health;
        public int damage;
        public int speed;
        public int jumping;
        public int cadency;
        public string name;

        public Race(IDataReader dataReader)
        {
            id_race = dataReader.GetInt32(0);
            health = dataReader.GetInt32(1);
            damage = dataReader.GetInt32(2);
            speed = dataReader.GetInt32(3);
            jumping = dataReader.GetInt32(4);
            cadency = dataReader.GetInt32(5);
            name = dataReader.GetString(6);
        }
    }
}
