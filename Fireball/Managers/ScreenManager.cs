using System.Drawing;
using System.Windows.Forms;

namespace Fireball.Managers
{
    static class ScreenManager
    {
        public static Image GetScreenshot()
        {
            Image bmp = new Bitmap(SystemInformation.VirtualScreen.Width, SystemInformation.VirtualScreen.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(SystemInformation.VirtualScreen.Left, SystemInformation.VirtualScreen.Top, 0, 0, bmp.Size);
            }
            return bmp;
        }

        public static Image CropImage(Image srcImage, Rectangle cropArea)
        {
            Image rtnImage = new Bitmap(cropArea.Width, cropArea.Height);

            /*using (Graphics gfx = Graphics.FromImage(rtnImage))
            {
                gfx.DrawImage(srcImage, -cropArea.X, -cropArea.Y);
            }*/

            using (Graphics gfx = Graphics.FromImage(rtnImage))
            {
                gfx.DrawImage(srcImage, 0, 0, cropArea, GraphicsUnit.Pixel);
            }

            return rtnImage;
        }
    }
}
