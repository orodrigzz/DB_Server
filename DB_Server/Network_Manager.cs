using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Database_Server
{
    public class Network_Manager
    {
        private TcpListener serverListener;
        private List<Client> clients;
        private Mutex clientListMutex;

        private int lastTimePing;
        private List<Client> disconnectClients;

        public Network_Manager()
        {

            int port = 6543;

            //Lista para almacenar los usuarios que se van conectando
            this.clients = new List<Client>();

            //Instancio clase para aceptar conexiones de cualquier IP por el puerto 6543
            this.serverListener = new TcpListener(IPAddress.Any, port);

            //Instancio el mutex
            this.clientListMutex = new Mutex();

            //Variable control de tiempo
            this.lastTimePing = Environment.TickCount;

            //Lista para almacenar usuarios a desconectar
            this.disconnectClients = new List<Client>();
        }

        //Función para iniciar el listener
        public void Start_Network_Service()
        {
            try
            {
                //Inicio el listener y empiezo a escuchar
                this.serverListener.Start();
                StartListening();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        //Función para acepatar conexiones TCP
        private void StartListening()
        {
            Console.WriteLine("Esperando nueva conexión");
            this.serverListener.BeginAcceptTcpClient(AcceptConnection, this.serverListener);
        }

        private void AcceptConnection(IAsyncResult ar)
        {
            Console.WriteLine("Recibo una conexion");

            //Serializo la conexion recibida a TcpListener que me permite leer esos datos
            TcpListener listener = (TcpListener)ar.AsyncState;

            //Bloqueo mutex
            this.clientListMutex.WaitOne();

            //Genera un nuevo usuario una vez hayas finalizado de ejecutar la conexión
            this.clients.Add(new Client(listener.EndAcceptTcpClient(ar)));

            //Libero mutex
            this.clientListMutex.ReleaseMutex();

            //Vuelvo a escuchar
            StartListening();
        }

        public void CheckMessage()
        {
            //Bloqueo mutex
            this.clientListMutex.WaitOne();

            //Recorro los clientes conectados
            foreach (Client client in this.clients)
            {
                //Accedo a su streaming de datos
                NetworkStream netStream = client.GetTcpClient().GetStream();

                //Compruebo si la información esta lista para ser leida
                if (netStream.DataAvailable)
                {
                    //Almaceno la información
                    StreamReader reader = new StreamReader(netStream, true);
                    string data = reader.ReadLine();

                    //Si hay algo de información "valiosa"
                    if (data != null)
                    {
                        //Gestiono los valores
                        ManageData(client, data);
                    }
                }
            }
            //Libero mutex
            this.clientListMutex.ReleaseMutex();
        }

        private void ManageData(Client client, string data)
        {
            //YO me autodefino que la información ira separada por el caracter /
            string[] parameters = data.Split('/');

            //YO defino que el primer parametro es la acción
            switch (parameters[0])
            {
                case "0":
                    Login(parameters[1], parameters[2], client);
                    break;
                case "1":
                    ReceivePing(client);
                    break;
                case "2":
                    int id_race = Int32.Parse(parameters[3]);
                    Register(parameters[1], parameters[2], id_race, client);
                    break;
                case "3":
                    GetRaces(client);
                    break;
            }
        }

        private void Login(string nick, string password, Client client)
        {
            List<UsersData> users = Database_Manager._DATABASE_MANAGER.FindUsers(nick, password);

            if (users.Count != 1){
                StreamWriter writer = new StreamWriter(client.GetTcpClient().GetStream());

                writer.WriteLine("IncorrectLogin");
                writer.Flush();
            }
            else {
                StreamWriter writer = new StreamWriter(client.GetTcpClient().GetStream());

                writer.WriteLine("CorrectLogin/" + users[0].id_user + "/" + users[0].nickname + "/" + users[0].password + "/" + users[0].id_race);
                writer.Flush();
            }
        }

        private void Register(string nickname, string password, int id_race, Client client) 
        {
            Console.WriteLine("Registro de: " + nickname + " usando la passwd: " + password + " con el id_race: " + id_race);

            try
            {
                StreamWriter writer = new StreamWriter(client.GetTcpClient().GetStream());

                Database_Manager._DATABASE_MANAGER.InsertUser(nickname, password, id_race);

                writer.WriteLine("CorrectRegister");
                writer.Flush();
            }
            catch (Exception ex) 
            {
                Console.WriteLine("Registro error: " + ex.Message + " con el cliente: " + client.GetNick());
                client.GetNick();
            }
        }

        private void GetRaces(Client client)
        {
            List<Race> races = Database_Manager._DATABASE_MANAGER.getRaces();

            foreach (Race race in races)
            {
                StreamWriter writer = new StreamWriter(client.GetTcpClient().GetStream());

                writer.WriteLine("CorrectRaces/" + race.id_race + "/" + race.health + "/" + race.damage + "/" + race.speed + "/" + race.jumping + "/" + race.cadency + "/" + race.name + "/" + races.Count());
                writer.Flush();
            }            
        }

        private void SendPing(Client client)
        {
            try
            {
                StreamWriter writer = new StreamWriter(client.GetTcpClient().GetStream());

                writer.WriteLine("Ping");
                writer.Flush();

                Console.WriteLine("Ping enviado");

                client.SetWaitingPing(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message + " con el cliente: " + client.GetNick());
            }
        }

        public void CheckConnection()
        {
            //Compruebo si ha pasado suficiente tiempo para enviar otros pings
            if (Environment.TickCount - this.lastTimePing > 5000)
            {
                //Bloqueo mutex
                clientListMutex.WaitOne();

                //Accedo a los clientes conectados
                foreach (Client client in this.clients)
                {
                    //Ya habia enviado un ping antes
                    if (client.GetWaitingPing() == true)
                    {
                        disconnectClients.Add(client);
                    }
                    else
                    {
                        //Envio ping si no te lo habia enviado antes
                        SendPing(client);
                    }
                }
                this.lastTimePing = Environment.TickCount;
                clientListMutex.ReleaseMutex();
            }
        }

        private void ReceivePing(Client client)
        {
            client.SetWaitingPing(false);
        }

        public void DisconnectClients()
        {
            //Bloqueo mutex
            clientListMutex.WaitOne();

            //Recorro la lista de usuarios
            foreach (Client client in this.disconnectClients)
            {
                Console.WriteLine("Desconectando usuarios");

                //Cierro conexion
                client.GetTcpClient().Close();

                //Elimino el usuario de la lista de clientes conectados
                this.clients.Remove(client);
            }

            //Limpio la lista de usuarios a desconectar (ya los elimine)
            this.disconnectClients.Clear();

            //Libero mutex
            clientListMutex.ReleaseMutex();
        }
    }
}
