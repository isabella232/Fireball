using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayAAPlugin
{
    public class PlayAASettings
    {
        public String UserName { get; set; }
        public String Url { get; set; }

        public PlayAASettings()
        {
            UserName = "Name";
            Url = "http://";
        }
    }
}
