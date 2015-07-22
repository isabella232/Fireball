using System;
using System.Drawing;
using System.Windows.Forms;

namespace Fireball.Managers
{
    static class ScreenManager
    {
        public static Image GetScreenshot()
        {
            var result = GetScreenSize();
            Image bmp = new Bitmap(result[0], result[1]);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(SystemInformation.VirtualScreen.Left, SystemInformation.VirtualScreen.Top, 0, 0, bmp.Size);
            }
            return bmp;
        }

        public static int[] GetScreenSize()
        {
            int minx, miny, maxx, maxy;
            minx = miny = int.MaxValue;
            maxx = maxy = int.MinValue;

            foreach (Screen screen in Screen.AllScreens)
            {
                var bounds = screen.Bounds;
                minx = Math.Min(minx, bounds.X);
                miny = Math.Min(miny, bounds.Y);
                maxx = Math.Max(maxx, bounds.Right);
                maxy = Math.Max(maxy, bounds.Bottom);
            }
            return new[] {maxx - minx, maxy - miny};//width,height

            Console.WriteLine("(width, height) = ({0}, {1})", maxx - minx, maxy - miny);
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
