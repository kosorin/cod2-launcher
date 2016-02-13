using CoD2_Launcher.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace CoD2_Launcher.Models
{
    public class Game
    {
        public Dictionary<string, string> Settings { get; set; } = new Dictionary<string, string>();

        public List<Player> Players { get; set; } = new List<Player>();

        public string PlayersCountInfo
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

        public Map Map { get; set; } = new Map();


        public static Game Download(ServerInfo server)
        {
            if (server == null)
            {
                return null;
            }

            Logger.Log($"Získávám informace ze serveru {server} ... ", Logger.MessageType.WithoutNewLine);

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
                    Logger.Log("FAIL: chyba odeslání", Logger.MessageType.Continue);
                    return null;
                }

                // Příjem odpovědi
                bytes = client.Receive(ref ep);
                client.Close();
                if (bytes == null)
                {
                    Logger.Log("FAIL: chyba přijímání", Logger.MessageType.Continue);
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

                // Map
                string map = game.Settings.ContainsKey("mapname") ? game.Settings["mapname"] : "<Unknown>";
                switch (map)
                {
                case "mp_downtown": game.Map.Name = "Moscow, Russia"; break;
                case "mp_toujane": game.Map.Name = "Toujane, Tunisia"; break;
                case "mp_burgundy": game.Map.Name = "Burgundy, France"; break;
                case "mp_carentan": game.Map.Name = "Carentan, France"; break;
                case "mp_trainstation": game.Map.Name = "Caen, France"; break;
                case "mp_dawnville": game.Map.Name = "St. Mere Eglise, France"; break;
                case "mp_railyard": game.Map.Name = "Stalingrad, Russia"; break;
                case "mp_farmhouse": game.Map.Name = "Beltot, France"; break;
                case "mp_harbor": game.Map.Name = "Rostov, Russia"; break;
                case "mp_matmata": game.Map.Name = "Matmata, Tunisia"; break;
                case "mp_leningrad": game.Map.Name = "Leningrad, Russia"; break;
                case "mp_rhine": game.Map.Name = "Wallendar, Germany"; break;
                case "mp_decoy": game.Map.Name = "El Alamein, Egypt"; break;
                case "mp_breakout": game.Map.Name = "Villers-Bocage, France"; break;
                case "mp_brecourt": game.Map.Name = "Brecourt, France"; break;
                case "mp_eindhoven": game.Map.Name = "Eindhoven, Holland"; break;
                case "mp_tripoli": game.Map.Name = "Tripoli, Libya"; break;
                default: game.Map.Name = map; break;
                }

                // Type
                string type = game.Settings.ContainsKey("g_gametype") ? game.Settings["g_gametype"] : "<Unknown>";
                switch (type)
                {
                case "dm": game.Map.Type = "Deathmatch"; break;
                case "sd": game.Map.Type = "Search and Destroy"; break;
                case "utd": game.Map.Type = "UT Domination"; break;
                case "tdm": game.Map.Type = "Team Deathmatch"; break;
                case "ctf": game.Map.Type = "Capture The Flag"; break;
                case "hq": game.Map.Type = "Headquarters"; break;
                default: game.Map.Type = type; break;
                }
                game.Map.ShortType = type.ToUpper();

                sw.Stop();
                Logger.Log($"OK ({ sw.ElapsedMilliseconds} ms)", Logger.MessageType.Continue);
                return game;
            }
            catch (Exception e)
            {
                Logger.Log($"FAIL: {e.Message}", Logger.MessageType.Continue);
                return null;
            }
        }
    }
}
