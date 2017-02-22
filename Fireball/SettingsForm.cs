using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Fireball.Core;
using Fireball.Editor;
using Fireball.Managers;
using Fireball.Plugin;
using Fireball.UI;

namespace Fireball
{
    public partial class SettingsForm : Form
    {
        private readonly Settings settings;
        private readonly string imageFilter;

        private Boolean isUploading;
        private Boolean isVisible;
        private IPlugin activePlugin;

        #region :: Ctor ::
        public SettingsForm()
        {
            InitializeComponent();

            Icon = tray.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

            var builder = new StringBuilder();
            {
                builder.Append("Image Files (*.png;*.gif;*.jpg;*.jpeg;*.bmp)|*.png;*.gif;*.jpg;*.jpeg;*.bmp|");
                builder.Append("PNG|*.png|");
                builder.Append("GIF|*.gif|");
                builder.Append("JPG|*.jpg|");
                builder.Append("JPEG|*.jpeg|");
                builder.Append("BMP|*.bmp");
            }

            imageFilter = builder.ToString();

            PopulateCombos();
            Settings.Instance = SettingsManager.Load();
            settings = Settings.Instance;
            PopulateSettings();

            PluginManager.Load();

            foreach (IPlugin plugin in PluginManager.Plugins)
            {
                PluginItem item = new PluginItem(plugin);
                cPlugins.Items.Add(item);

                if (settings.ActivePlugin.Equals(plugin.Name))
                    cPlugins.SelectedItem = item;
            }

            if (cPlugins.SelectedItem == null && cPlugins.Items.Count > 0)
                cPlugins.SelectedIndex = 0;

            #region :: Register Hotkeys ::
            /*StringBuilder hotkeyRegisterErrorBuilder = new StringBuilder();

            if (settings.CaptureScreenHotey.GetCanRegister(this))
            {
                settings.CaptureScreenHotey.Register(this);
                settings.CaptureScreenHotey.Pressed += CaptureScreenHotkeyPressed;
            }
            else
            {
                if (settings.CaptureScreenHotey.KeyCode != Keys.None)
                    hotkeyRegisterErrorBuilder.AppendFormat(" - Can't register capture screen hotkey ({0})\n", settings.CaptureScreenHotey);
            }

            if (settings.CaptureAreaHotkey.GetCanRegister(this))
            {
                settings.CaptureAreaHotkey.Register(this);
                settings.CaptureAreaHotkey.Pressed += CaptureAreaHotkeyPressed;
            }
            else
            {
                if (settings.CaptureScreenHotey.KeyCode != Keys.None)
                    hotkeyRegisterErrorBuilder.AppendFormat(" - Can't register capture area hotkey ({0})\n", settings.CaptureAreaHotkey);
            }

            if (hotkeyRegisterErrorBuilder.Length > 0)
            {
                Helper.InfoBoxShow(String.Format("Failed to register hotkeys!\n{0}", hotkeyRegisterErrorBuilder));
            }*/
            #endregion

            SaveSettings();

            Application.ApplicationExit += (s, e) => SettingsManager.Save();
        }
        #endregion

        #region :: Overrides ::
        protected override void SetVisibleCore(bool value)
        {
            if (!isVisible)
                value = false;

            base.SetVisibleCore(value);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason != CloseReason.UserClosing)
                return;

            if (isVisible)
            {
                Hide();
                isVisible = false;
                e.Cancel = true;
            }

            base.OnFormClosing(e);
        }
        #endregion

        private void PopulateCombos()
        {
            cLanguage.Items.Clear();
            cLanguage.Items.Add(new LanguageItem("Eng", new CultureInfo("en-US")));
            cLanguage.Items.Add(new LanguageItem("Rus", new CultureInfo("ru-RU")));

            cNotification.Items.Clear();
            foreach (object type in Enum.GetValues(typeof(NotificationType)))
            {
                cNotification.Items.Add(type);
            }
        }

        private void PopulateHotkeyControl(HotkeySelectControl khControl, Hotkey hk)
        {
            khControl.Hotkey = hk.KeyCode;
            khControl.Win = hk.Win;
            khControl.Ctrl = hk.Ctrl;
            khControl.Shift = hk.Shift;
            khControl.Alt = hk.Alt;
        }

        private void PopulateSettings()
        {
            foreach (object item in cLanguage.Items)
            {
                if (item.ToString().Equals(settings.Language))
                {
                    cLanguage.SelectedItem = item;
                    break;
                }
            }

            if (cLanguage.SelectedIndex == -1)
                cLanguage.SelectedIndex = 0;

            PopulateHotkeyControl(hkScreen, settings.CaptureScreenHotey);
            PopulateHotkeyControl(hkArea, settings.CaptureAreaHotkey);
            PopulateHotkeyControl(hkClipboard, settings.UploadFromClipboardHotkey);
            PopulateHotkeyControl(hkFile, settings.UploadFromFileHotkey);
            PopulateHotkeyControl(hkUrl, settings.UploadFromUrlHotkey);
            PopulateHotkeyControl(bfUp, settings.CaptureScreenWithoutUpload);

            if (cNotification.Items.Contains(settings.Notification))
                cNotification.SelectedItem = settings.Notification;

            cAutoStart.Checked = settings.StartWithComputer;
            cWithoutEditor.Checked = settings.WithoutEditor;
        }

        private void UpdateHotkey(HotkeySelectControl hkControl, Hotkey hotkey, HandledEventHandler hkEvent)
        {
            if (hkControl.Hotkey == Keys.None && hotkey.Registered)
            {
                hotkey.Pressed -= hkEvent;
                hotkey.Unregister();
                hotkey.KeyCode = Keys.None;
            }
            else
            {
                hotkey.KeyCode = hkControl.Hotkey;
                hotkey.Win = hkControl.Win;
                hotkey.Ctrl = hkControl.Ctrl;
                hotkey.Shift = hkControl.Shift;
                hotkey.Alt = hkControl.Alt;

                if (!hotkey.Registered && hotkey.KeyCode != Keys.None)
                {
                    hotkey.Register(this);
                    hotkey.Pressed += hkEvent;
                }
            }
        }

        private Boolean SaveSettings()
        {
            LanguageItem languageItem = cLanguage.SelectedItem as LanguageItem;

            if (languageItem != null)
                settings.Language = languageItem.Name;

            try
            {
                UpdateHotkey(hkScreen, settings.CaptureScreenHotey, CaptureScreenHotkeyPressed);
            }
            catch (Exception)
            {
                Helper.InfoBoxShow("Failed to register capture screen hotkey!");
                return false;
            }

            try
            {
                UpdateHotkey(hkArea, settings.CaptureAreaHotkey, CaptureAreaHotkeyPressed);
            }
            catch (Exception)
            {
                Helper.InfoBoxShow("Failed to register capture area hotkey!");
                return false;
            }

            try
            {
                UpdateHotkey(hkClipboard, settings.UploadFromClipboardHotkey, UploadFromClipboardHotkeyPressed);
            }
            catch (Exception)
            {
                Helper.InfoBoxShow("Failed to register upload from clipboard hotkey!");
                return false;
            }

            try
            {
                UpdateHotkey(hkFile, settings.UploadFromFileHotkey, UploadFromFileHotkeyPressed);
            }
            catch (Exception)
            {
                Helper.InfoBoxShow("Failed to register upload from file hotkey!");
                return false;
            }
            try
            {
                UpdateHotkey(hkUrl, settings.UploadFromUrlHotkey, UploadFromUrlHotkeyPressed);
            }
            catch (Exception)
            {
                Helper.InfoBoxShow("Failed to register upload from file hotkey!");
                return false;
            }

            try
            {
                UpdateHotkey(bfUp, settings.CaptureScreenWithoutUpload, UploadToBufferHotkeyPressed);
            }
            catch (Exception)
            {
                Helper.InfoBoxShow("Failed to register upload from file hotkey!");
                return false;
            }

            var selectedPlugin = cPlugins.SelectedItem as PluginItem;

            if (selectedPlugin != null)
                settings.ActivePlugin = selectedPlugin.Plugin.Name;

            var notification = (NotificationType)cNotification.SelectedItem;
            settings.Notification = notification;

            settings.StartWithComputer = cAutoStart.Checked;
            settings.WithoutEditor = cWithoutEditor.Checked;

            Helper.SetStartup(cAutoStart.Checked);
            SettingsManager.Save();
            return true;
        }
        public Image ByteArrayToImage(byte[] byteArrayIn)
        {
            MemoryStream ms = new MemoryStream(byteArrayIn);
            Image returnImage = Image.FromStream(ms);
            return returnImage;
        }
        public byte[] ImageToByteArray(Image imageIn)
        {
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            return ms.ToArray();
        }
        private void ForwardImageToPlugin(byte[] data, string path = "", bool isFile = false)
        {
            if (data == null)
                return;
            if (!isFile)
                if (!Settings.Instance.WithoutEditor)
                {
                    var image = ByteArrayToImage(data);
                    using (EditorForm editor = new EditorForm(image, Thread.CurrentThread.CurrentUICulture))
                    {
                        if (editor.ShowDialog() == DialogResult.OK)
                        {
                            data = ImageToByteArray(editor.GetImage());
                            SettingsManager.Save();
                        }
                        else
                        {
                            image.Dispose();
                            SettingsManager.Save();
                            return;
                        }
                    }
                    image.Dispose();
                }

            NotificationForm notificationForm = null;

            if (settings.Notification == NotificationType.Tooltip)
            {
                tray.BalloonTipIcon = ToolTipIcon.Info;
                tray.BalloonTipTitle = String.Format("Fireball: {0}", activePlugin.Name);
                tray.BalloonTipText = "Uploading...";
                tray.ShowBalloonTip(1000);
            }
            else if (settings.Notification == NotificationType.Window)
            {
                notificationForm = new NotificationForm(String.Format("Fireball: {0}", activePlugin.Name));
                notificationForm.Show();
            }

            string url = string.Empty;

            var uploadTask = new Task(() =>
            {
                isUploading = true;

                try
                {
                    url = activePlugin.Upload(data, path, isFile);
                    data = null;
                }
                catch { }

                isUploading = false;

                if (url.StartsWith("http://"))
                    Settings.Instance.MRUList.Enqueue(url);
            });

            uploadTask.ContinueWith(arg =>
            {
                try
                {
                    if (settings.Notification == NotificationType.Tooltip)
                    {
                        // Скопировать в буфер и показать тултип
                        Clipboard.SetDataObject(url, true, 5, 500);

                        // ======= твик =======
                        // прячем предыдущий тултип, если он сам не скрылся
                        tray.Visible = false;
                        tray.Visible = true;
                        // ====================

                        tray.BalloonTipIcon = ToolTipIcon.Info;
                        tray.BalloonTipTitle = String.Format("Fireball: {0}", activePlugin.Name);
                        tray.BalloonTipText = String.IsNullOrEmpty(url) ? "empty" : url;
                        tray.ShowBalloonTip(1000);
                    }
                    else if (settings.Notification == NotificationType.Window)
                    {
                        // Вывести ссылку в форму уведомления
                        if (notificationForm != null)
                            notificationForm.SetUrl(url);
                    }
                    else
                    {
                        // Тихо копировать в буфер обмена
                        Clipboard.SetDataObject(url, true, 5, 500);
                    }
                }
                catch { }
            }, TaskScheduler.FromCurrentSynchronizationContext());
            uploadTask.Start();
        }

        private bool PreuploadCheck()
        {
            if (activePlugin == null)
            {
                Helper.InfoBoxShow("Plugin not loaded, can't upload!");
                return false;
            }

            if (isUploading)
            {
                tray.ShowBalloonTip(1000, "Fireball", "Wait until the upload is complete!", ToolTipIcon.Warning);
                return false;
            }

            return true;
        }

        private Image CaptureAreaInternal()
        {
            var screenImage = ScreenManager.GetScreenshot();
            trayMenu.Hide();

            bool createdNew;
            using (new Mutex(true, "Fireball TakeForm", out createdNew))
            {
                if (!createdNew)
                    return null;

                using (var takeForm = new TakeForm(screenImage))
                {
                    if (takeForm.ShowDialog() == DialogResult.OK)
                    {
                        return takeForm.GetSelection();
                    }
                }
            }

            return null;
        }
        private void CaptureArea()
        {
            if (!PreuploadCheck())
                return;

            ForwardImageToPlugin(ImageToByteArray(CaptureAreaInternal()), "");
        }

        private void CaptureScreen()
        {
            if (!PreuploadCheck())
                return;

            var screenImage = ScreenManager.GetScreenshot();
            trayMenu.Hide();

            ForwardImageToPlugin(ImageToByteArray(screenImage));
        }

        private void UploadFromClipboard()
        {
            if (!PreuploadCheck())
                return;
            byte[] data = null;
            string path1 = "";
            try
            {
                if (Clipboard.ContainsFileDropList())
                {
                    StringCollection col = Clipboard.GetFileDropList();

                    if (col.Count > 0)
                    {
                        string path = col[0];
                        path1 = path;
                        if (File.Exists(path))
                            data = File.ReadAllBytes(path);
                    }
                }
            }
            catch { return; }

            if (data == null)
            {
                tray.ShowBalloonTip(1000, "Fireball", "Clipboard is empty!", ToolTipIcon.Warning);
                return;
            }

            ForwardImageToPlugin(data, path1, true);
        }

        private void UploadToBuffer()
        {
            if (!PreuploadCheck())
                return;

            var image = CaptureAreaInternal();

            if (Settings.Instance.WithoutEditor)
                return;

            using (var editor = new EditorForm(image, Thread.CurrentThread.CurrentUICulture))
            {
                if (editor.ShowDialog() == DialogResult.OK)
                {
                    Clipboard.SetImage(editor.GetImage());

                    tray.ShowBalloonTip(1000, "Fireball", "Image added to clipboard", ToolTipIcon.Info);
                }
                else
                    image.Dispose();
            }
        }
        private void UploadFromFile(string filename = "")
        {
            if (!PreuploadCheck())
                return;

            if (!string.IsNullOrEmpty(filename))
            {
                ForwardImageToPlugin(File.ReadAllBytes(filename), filename, true);

                File.Delete(filename);

                return;
            }
            using (var op = new OpenFileDialog { FileName = string.Empty })
            {
                if (op.ShowDialog() != DialogResult.OK)
                    return;

                try
                {
                    var image = Image.FromFile(op.FileName);
                    ForwardImageToPlugin(ImageToByteArray(image));
                }
                catch
                {
                    var data = File.ReadAllBytes(op.FileName);
                    ForwardImageToPlugin(data, op.FileName, true);
                }

            }
        }

        private void SetLanguage(Form form, CultureInfo lang)
        {
            Thread.CurrentThread.CurrentUICulture = lang;
            var resources = new ComponentResourceManager(form.GetType());

            Localizer.ApplyResourceToControl(resources, trayMenu, lang);
            Localizer.ApplyResourceToControl(resources, form, lang);

            form.Text = resources.GetString("$this.Text", lang);
            lVersion.Text = $"Version: {Application.ProductVersion}";
        }

        #region :: Form Controlls Events ::
        private void BApplyClick(object sender, EventArgs e)
        {
            if (SaveSettings())
                Close();
        }

        private void BCancelClick(object sender, EventArgs e)
        {
            Close();
        }

        private void CPluginsSelectedIndexChanged(object sender, EventArgs e)
        {
            PluginItem item = cPlugins.SelectedItem as PluginItem;

            if (item == null)
                return;

            activePlugin = item.Plugin;
            bPluginSettings.Enabled = activePlugin.HasSettings;
        }

        private void CLanguageSelectedIndexChanged(object sender, EventArgs e)
        {
            LanguageItem item = cLanguage.SelectedItem as LanguageItem;

            if (item != null)
                SetLanguage(this, item.Culture);
        }

        private void BPluginSettingsClick(object sender, EventArgs e)
        {
            PluginItem item = cPlugins.SelectedItem as PluginItem;

            if (item == null)
                return;

            if (item.Plugin.HasSettings)
                item.Plugin.ShowSettings();
        }
        #endregion

        #region :: Hotkeys Events ::
        private void CaptureAreaHotkeyPressed(object sender, HandledEventArgs e)
        {
            CaptureArea();
        }

        private void CaptureScreenHotkeyPressed(object sender, HandledEventArgs e)
        {
            CaptureScreen();
        }

        private void UploadFromClipboardHotkeyPressed(object semder, HandledEventArgs e)
        {
            UploadFromClipboard();
        }
        private void UploadFromFileHotkeyPressed(object semder, HandledEventArgs e)
        {
            UploadFromFile();
        }

        private void UploadToBufferHotkeyPressed(object sender, HandledEventArgs e)
        {
            UploadToBuffer();
        }
        private void UploadFromUrlHotkeyPressed(object semder, HandledEventArgs e)
        {
            using (var client = new WebClient())
            {
                Uri result;
                if (!Uri.TryCreate(Clipboard.GetText(), UriKind.Absolute, out result))
                {
                    Helper.InfoBoxShow("Invalid url");
                    return;
                }
                var filename = Path.GetFileName(Clipboard.GetText());
                client.DownloadFile(Clipboard.GetText(), filename);
                UploadFromFile(filename);
            }
        }
        #endregion

        #region :: Tray Events ::
        private void TrayDoubleClick(object sender, EventArgs e)
        {
            TraySubSettingsClick(this, new EventArgs());
        }

        private void TraySubCaptureAreaClick(object sender, EventArgs e)
        {
            CaptureArea();
        }

        private void TraySubCaptureScreenClick(object sender, EventArgs e)
        {
            CaptureScreen();
        }

        private void TraySubUploadFromClipboardClick(object sender, EventArgs e)
        {
            UploadFromClipboard();
        }

        private void UploadFromFileClick(object sender, EventArgs e)
        {
            UploadFromFile();
        }

        private void TraySubSettingsClick(object sender, EventArgs e)
        {
            if (!isVisible)
            {
                isVisible = true;
                PopulateSettings();
                Show();
            }

            mainTabControl.SelectedTab = generalTab;
        }

        private void TraySubAboutClick(object sender, EventArgs e)
        {
            if (!isVisible)
            {
                isVisible = true;
                PopulateSettings();
                Show();
            }

            mainTabControl.SelectedTab = aboutTab;
        }

        private void TraySubCheckForUpdatesClick(object sender, EventArgs e)
        {
            TraySubSettingsClick(this, new EventArgs());
        }

        private void TraySubExitClick(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void TrayBalloonTipClicked(object sender, EventArgs e)
        {
            if (tray.BalloonTipText.StartsWith("http://"))
                System.Diagnostics.Process.Start(tray.BalloonTipText);
        }

        private void TrayMenuOpening(object sender, CancelEventArgs e)
        {
            recentToolStripMenuItem.DropDown.Items.Clear();
            Settings.Instance.MRUList.Items.ForEach(item => recentToolStripMenuItem.DropDown.Items.Add(item, Properties.Resources.image, (s1, e1) => System.Diagnostics.Process.Start(item)));
        }
        #endregion

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            UploadFromFile();
        }

        private void hkClipboard_Load(object sender, EventArgs e)
        {

        }

        private void hkFile_Load(object sender, EventArgs e)
        {

        }
    }
}