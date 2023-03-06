using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database_Server
{
    class Server
    {
        static void Main(string[] args)
        {
            bool bServerOn = true;

            //Instancio los servicios 
            Network_Manager Network_Service = new Network_Manager();
            Database_Manager Database_Manager = new Database_Manager();

            //Inicio los servicios
            StartServices();

            //Mientras sea true el servidor se mantiene encendido
            while (bServerOn)
            {
                Network_Service.CheckConnection();
                Network_Service.CheckMessage();
                Network_Service.DisconnectClients();
            }

            //Función para iniciar los servicios
            void StartServices()
            {
                Network_Service.Start_Network_Service();
                Database_Manager.OpenConnection();
            }
        }
    }
}
