using System.Text.RegularExpressions;

namespace CoD2_Launcher.Models
{
    public class Player
    {
        public string Name => Regex.Replace(FullName, @"\^[0-9]", "");

        public string FullName { get; set; }

        public int Score { get; set; }

        public int Ping { get; set; }

        public override string ToString()
        {
            return string.Format("{0}; {1}; {2}", Name, Score, Ping);
        }
    }
}
