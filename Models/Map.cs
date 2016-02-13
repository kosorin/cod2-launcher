using System.Windows.Media;

namespace CoD2_Launcher.Models
{
    public class Map
    {
        public string Name { get; set; }

        public string Type { get; set; }

        public string ShortType { get; set; }

        public Color TypeColor
        {
            get
            {
                switch (ShortType.ToUpper())
                {
                case "DM": return Colors.DarkRed;
                case "SD": return Colors.Blue;
                case "UTD": return Colors.Purple;
                case "TDM": return Colors.Green;
                case "CTF": return Colors.Orange;
                case "HQ": return Colors.DeepPink;
                default: return Colors.Black;
                }
            }
        }
    }
}
