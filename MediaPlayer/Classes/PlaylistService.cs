using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaPlayer.Classes
{
    public class PlaylistService
    {
        public static void addMediaFile(Playlist selected, string path)
        {

            MediaItem item = new MediaItem();

            string fileName = System.IO.Path.GetFileName(path);
            string[] fileSplit = fileName.Split(".");
            item.Name = string.Join(".", fileSplit.Where((val, idx) => idx != fileSplit.Length - 1).ToArray());
            item.Path = path;

            selected.Items.Add(item);

            if (selected.Name == "Recent" && selected.Items.Count > 15)
            {
                Playlist resetRecent = new Playlist();
                resetRecent.Name = "Recent";
                resetRecent.Items.Add(item);
                FileService.saveAplayList(selected);
            }
            FileService.saveAplayList(selected);
        }

        public static void removeMediaFile(Playlist selected, MediaItem target)
        {

            selected.Items.Remove(target);
            FileService.saveAplayList(selected);
        }

        public static ObservableCollection<string> getListOfPlaylists(Dictionary<string, Playlist> input)
        {
            ObservableCollection<string> result = new ObservableCollection<string>();
            foreach(string key in input.Keys)
            {
                result.Add(key);
            }

            return result;
        }

        public static Status isValidName(Dictionary<string, Playlist> myPlaylists, string name)
        {
            if (name == "Recent")
            {
                return new Status(false, "Name cant be Recent");
            }

            if (myPlaylists.ContainsKey(name))
            {
                return new Status(false, "Duplicate name");
            }

            return new Status(true, "");
        }

        public static void updateRecentPlayList(Playlist recent, MediaItem newItem)
        {
            if (recent.Items.Count == 15)
            {
                recent.Items.RemoveAt(0);
                recent.Items.Add(newItem);
            }
            else
            {
                recent.Items.Add(newItem);
            }
            FileService.saveAplayList(recent);
        }
    }
}
