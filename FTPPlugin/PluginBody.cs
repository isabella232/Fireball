using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.FtpClient;
using System.Windows.Forms;
using Fireball.Plugin;
using FTPPlugin;

namespace Fireball.Plugin
{
    public class PluginBody : IPlugin
    {
        private FTPSettings settings;

        public PluginBody()
        {
            try
            {
                settings = SettingsManager.Load();
            }
            catch
            {
                settings = new FTPSettings();
            }
        }

        public string Name
        {
            get { return  "ftp server"; }
        }

        public Single Version
        {
            get { return 1.0f; }
        }

        public bool HasSettings
        {
            get { return true; }
        }

        public void ShowSettings()
        {
            using (FTPSettingsForm fs = new FTPSettingsForm(settings))
            {
                if (fs.ShowDialog() == DialogResult.OK)
                {
                    settings = fs.GetSettings();

                    try
                    {
                        SettingsManager.Save(settings);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error while saving ftp settings!\r\n\r\n" + ex.Message, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }

        public string Upload(byte[] imageData, string filename,bool isFile)
        {
            if (imageData == null)
                return string.Empty;

            try
            {
                string fileName = isFile ? Path.GetFileName(filename) : String.Format("{0}.png", DateTime.Now.ToString("dd.MM.yyyy-HH.mm.ss"));
                FtpWebRequest request = (FtpWebRequest)FtpWebRequest.Create(String.Format("{0}/{1}/{2}", settings.Server, settings.Directory, fileName));
                {
                    request.Method = WebRequestMethods.Ftp.UploadFile;
                    request.Credentials = new NetworkCredential(settings.Username, settings.Password);
                    request.UsePassive = true;
                    request.UseBinary = true;
                    request.KeepAlive = true;
                    request.Timeout = -1;
                    request.ReadWriteTimeout = -1;
                }

                Stream reqStream = request.GetRequestStream();
                reqStream.Write(imageData, 0, imageData.Length);
                reqStream.Close();
                return String.Format("{0}/{1}", settings.Url, fileName);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

    }
}