using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AzureBlob
{
    static class SettingsManager
    {
        private static string fileName = "azuresettings.xml";

        public static void Save(AzureSettings settings)
        {
            using (FileStream stream = File.Open(fileName, FileMode.Create))
            {
                XmlSerializer s = new XmlSerializer(typeof(AzureSettings));
                s.Serialize(stream, settings);
            }
        }

        public static AzureSettings Load()
        {
            using (FileStream stream = File.Open(fileName, FileMode.Open))
            {
                XmlSerializer s = new XmlSerializer(typeof(AzureSettings));
                return (AzureSettings)s.Deserialize(stream);
            }
        }
    }
}
