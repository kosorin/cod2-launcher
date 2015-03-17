﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoD2_Launcher
{
    public class ServerInfo
    {
        public ServerInfo()
        {
            Port = DefaultPort;
        }

        public string Name { get; set; }

        public string Host { get; set; }

        public int Port { get; set; }

        public static int DefaultPort { get { return 28960; } }

        public static ServerInfo Parse(string server)
        {
            if (server == null)
            {
                return null;
            }

            ServerInfo info = new ServerInfo();

            if (server.Contains('='))
            {
                int nameEndPosition = server.IndexOf('=');

                info.Name = server.Substring(0, nameEndPosition).Trim();
                server = server.Substring(nameEndPosition + 1);
            }

            if (server.Contains(':'))
            {
                string[] parts = server.Split(':');

                // Host
                info.Host = parts[0].Trim();

                // Port
                int port;
                if (int.TryParse(parts[1], out port))
                {
                    info.Port = port;
                }
            }
            else
            {
                info.Host = server.Trim();
            }

            return info;
        }
    }
}