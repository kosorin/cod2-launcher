using System.Linq;

namespace CoD2_Launcher.Models
{
    public class ServerInfo
    {
        public static int DefaultPort => 28960;


        public string Name { get; set; }

        public string Host { get; set; }

        public int Port { get; set; } = DefaultPort;


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

        public override string ToString()
        {
            string format;
            if (!string.IsNullOrWhiteSpace(Name))
            {
                format = "{0}";
            }
            else
            {
                format = "{1}:{2}";
            }
            return string.Format(format, Name, Host, Port);
        }

        public override bool Equals(object obj)
        {
            if (obj is ServerInfo)
            {
                ServerInfo o = obj as ServerInfo;
                if (o != null)
                {
                    return Host == o.Host && Port == o.Port;
                }
                return false;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Host.GetHashCode() * 17 + Port.GetHashCode();
        }
    }
}
