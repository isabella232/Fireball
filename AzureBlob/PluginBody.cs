using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzureBlob;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Fireball.Plugin
{
    public class PluginBody : IPlugin
    {
        private AzureSettings settings;

        public PluginBody()
        {
            try
            {
                settings = SettingsManager.Load();
            }
            catch
            {
                settings = new AzureSettings();
                SettingsManager.Save(settings);
            }
        }
        public bool HasSettings
        {
            get { return false; }
        }

        public void ShowSettings()
        {
            throw new NotImplementedException();
        }

        public string Name
        {
            get { return "Azure"; }
        }

        public Single Version
        {
            get { return 1.0f; }
        }
        public string Upload(byte[] image, string filename, bool isFile)
        {
            string fileName = isFile ? Path.GetFileName(filename) : String.Format("{0}.png", DateTime.Now.ToString("dd.MM.yyyy-HH.mm.ss"));
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}",
                settings.AccountName, settings.AccountKey));
            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference(settings.ContainerName);

            // Retrieve reference to a blob named "myblob".
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileName);

            // Create or overwrite the "myblob" blob with contents from a local file.
            blockBlob.UploadFromByteArray(image,0,image.Length);
            return String.Format("{0}/{1}", settings.Url, fileName); 
        }
    }
}
