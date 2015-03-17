using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CoD2_Launcher
{
    public class Player
    {
        public string Name
        {
            get
            {
                return Regex.Replace(FullName, @"\^[0-9]", "");
            }
        }

        public string FullName { get; set; }

        public int Score { get; set; }

        public int Ping { get; set; }

        public override string ToString()
        {
            return string.Format("{0}; {1}; {2}", Name, Score, Ping);
        }
    }
}
