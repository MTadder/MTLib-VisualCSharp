using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTLib
{
    public class Authentication
    {
        public enum Level
        {
            None,
            RoomAdmin,
            TrailHunter
        }

        public Level CurrentLevel { get; private set; }

        public Authentication()
        {

        }
    }
}
