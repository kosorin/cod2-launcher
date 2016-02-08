using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoD2_Launcher
{
    public static class Logger
    {
        public enum MessageType
        {
            NewLine,
            WithoutNewLine,
            Continue,
        }

        public static void Log(string message, MessageType type = MessageType.NewLine)
        {
            if (type != MessageType.Continue)
            {
                Console.Write(Prefix);
            }
            Console.Write(message);
            if (type != MessageType.WithoutNewLine)
            {
                Console.WriteLine();
            }
        }

        private static string Prefix => DateTime.Now.ToString("HH':'mm':'ss'> '");
    }
}
