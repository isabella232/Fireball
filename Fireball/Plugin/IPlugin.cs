using System;
using System.Drawing;

namespace Fireball.Plugin
{
    public interface IPlugin
    {
        String Name { get; }
        Single Version { get; }
        bool HasSettings { get; }
        void ShowSettings();
        string Upload(byte[] image, string filename,bool isFile);
    }
}
