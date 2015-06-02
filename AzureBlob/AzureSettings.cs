using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureBlob
{
    public class AzureSettings
    {
        public String ContainerName { get; set; }
        public String Url { get; set; }
        public String AccountName { get; set; }
        public String AccountKey { get; set; }

        public AzureSettings()
        {
            ContainerName = "Name";
            Url = "http://";
            AccountName = "user";
            AccountKey = "key";
        }
    }
}
