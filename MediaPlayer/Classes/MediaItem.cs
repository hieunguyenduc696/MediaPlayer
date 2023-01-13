using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaPlayer.Classes
{
    public class MediaItem : ICloneable, INotifyPropertyChanged
    {
        public string Name { get; set; }
        public string Path { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public static string extractNameFromPath(string Path)
        {
            string result = "";
            string[] splitStr = Path.Split("\\");
            result = splitStr[splitStr.Length - 1];
            return result;
        }

        public object Clone()
        {
            return new MediaItem()
            {
                Name = this.Name,
                Path = this.Path,
            };
        }
    }
}
