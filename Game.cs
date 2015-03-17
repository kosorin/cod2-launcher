using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CoD2_Launcher
{
    public class Game
    {
        private Game()
        {
            Players = new List<Player>();
            Settings = new Dictionary<string, string>();
        }

        public Dictionary<string, string> Settings { get; set; }

        public List<Player> Players { get; set; }

        public string PlayersCountString
        {
            get
            {
                string format = "";
                if (MaxPrivatePlayers > 0)
                {
                    format = "{0}/{1}+{2}";
                }
                else
                {
                    format = "{0}/{1}";
                }
                return string.Format(format, Players.Count, MaxPlayers, MaxPrivatePlayers);
            }
        }

        public int MaxPlayers { get; set; }

        public int MaxPrivatePlayers { get; set; }

        public string Type { get; set; }

        public string Map { get; set; }

        public static Game GetStatus(ServerInfo server)
        {
            if (server == null)
            {
                return null;
            }

            Console.Write("Získávám informace o serveru... ");

            try
            {
                Stopwatch sw = Stopwatch.StartNew();

                // Připojení
                UdpClient client = new UdpClient();
                IPAddress ip;
                try
                {
                    ip = IPAddress.Parse(server.Host);
                }
                catch (FormatException)
                {
                    ip = Dns.GetHostEntry(server.Host).AddressList[0];
                }
                IPEndPoint ep = new IPEndPoint(ip, server.Port);
                client.Connect(ep);

                // Odeslání požadavku
                byte[] bytes = Encoding.ASCII.GetBytes("XXXXgetstatus");
                for (int i = 0; i < 4; i++)
                {
                    bytes[i] = 0xFF;
                }
                if (client.Send(bytes, bytes.Length) != bytes.Length)
                {
                    Console.WriteLine("FAIL: chyba odeslání");
                    return null;
                }

                // Příjem odpovědi
                bytes = client.Receive(ref ep);
                client.Close();
                if (bytes == null)
                {
                    Console.WriteLine("FAIL: chyba přijímání");
                    return null;
                }
                string[] data = Encoding.ASCII.GetString(bytes).Split('\n');

                // Zpracování dat
                Game game = new Game();
                if (data.Length > 1 && data[1].Length > 0)
                {
                    // Nastavení
                    string settingsString = data[1].Substring(1);
                    bool isKey = true;
                    string key = "";
                    foreach (string value in settingsString.Split('\\'))
                    {
                        if (isKey)
                        {
                            key = value;
                        }
                        else
                        {
                            game.Settings.Add(key, value);
                        }
                        isKey = !isKey;
                    }

                    // Hráči
                    for (int i = 2; i < data.Length - 1; i++)
                    {
                        string[] playerString = data[i].Split(new char[] { ' ' }, 3);
                        if (playerString.Length == 3)
                        {
                            try
                            {
                                Player player = new Player
                                {
                                    FullName = playerString[2].Trim('"'),
                                    Score = int.Parse(playerString[0]),
                                    Ping = int.Parse(playerString[1])
                                };
                                game.Players.Add(player);
                            }
                            catch (FormatException) { }
                        }
                    }
                    game.Players.Sort((p, q) => q.Score.CompareTo(p.Score));
                }

                // MaxPrivatePlayers
                int maxPlayers;
                if (int.TryParse(game.Settings.ContainsKey("sv_privateClients") ? game.Settings["sv_privateClients"] : "0", out maxPlayers))
                {
                    game.MaxPrivatePlayers = maxPlayers;
                }
                else
                {
                    game.MaxPrivatePlayers = 0;
                }

                // MaxPlayers
                if (int.TryParse(game.Settings.ContainsKey("sv_maxclients") ? game.Settings["sv_maxclients"] : "0", out maxPlayers))
                {
                    game.MaxPlayers = maxPlayers - game.MaxPrivatePlayers;
                }
                else
                {
                    game.MaxPlayers = 0;
                }

                // Type
                string type = game.Settings.ContainsKey("g_gametype") ? game.Settings["g_gametype"] : "<Unknown>";
                switch (type)
                {
                case "dm": game.Type = "Deathmatch"; break;
                case "sd": game.Type = "Search and Destroy"; break;
                case "utd": game.Type = "UT Domination"; break;
                case "tdm": game.Type = "Team Deathmatch"; break;
                case "ctf": game.Type = "Capture The Flag"; break;
                case "hq": game.Type = "Headquarters"; break;
                default: game.Type = type; break;
                }

                // Map
                string map = game.Settings.ContainsKey("mapname") ? game.Settings["mapname"] : "<Unknown>";
                switch (map)
                {
                case "mp_downtown": game.Map = "Moscow, Russia"; break;
                case "mp_toujane": game.Map = "Toujane, Tunisia"; break;
                case "mp_burgundy": game.Map = "Burgundy, France"; break;
                case "mp_carentan": game.Map = "Carentan, France"; break;
                case "mp_trainstation": game.Map = "Caen, France"; break;
                case "mp_dawnville": game.Map = "St. Mere Eglise, France"; break;
                case "mp_railyard": game.Map = "Stalingrad, Russia"; break;
                case "mp_farmhouse": game.Map = "Beltot, France"; break;
                case "mp_harbor": game.Map = "Rostov, Russia"; break;
                case "mp_matmata": game.Map = "Matmata, Tunisia"; break;
                case "mp_leningrad": game.Map = "Leningrad, Russia"; break;
                case "mp_rhine": game.Map = "Wallendar, Germany"; break;
                case "mp_decoy": game.Map = "El Alamein, Egypt"; break;
                case "mp_breakout": game.Map = "Villers-Bocage, France"; break;
                case "mp_brecourt": game.Map = "Brecourt, France"; break;
                case "mp_eindhoven": game.Map = "Eindhoven, Holland"; break;
                case "mp_tripoli": game.Map = "Tripoli, Libya"; break;
                default: game.Map = map; break;
                }

                sw.Stop();
                Console.WriteLine("OK ({0} ms)", sw.ElapsedMilliseconds);
                return game;
            }
            catch (Exception e)
            {
                Console.WriteLine("FAIL: {0}", e.Message);
                return null;
            }
        }
    }
}
