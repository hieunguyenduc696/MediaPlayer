using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaPlayer.Classes
{
    class FileService
    {
        public static Playlist loadRecents()
        {
            Playlist recent = new Playlist();
            string recentPath = $@"Playlists\Recent.txt";
            if (!System.IO.File.Exists(recentPath))
            {
                using (StreamWriter sw = System.IO.File.CreateText(recentPath))
                {
                    sw.Write("Recent");
                }
            }
            else
            {
                List<string> lines = System.IO.File.ReadLines(recentPath).ToList();
                recent = loadAPlayList(recentPath);
            }

            return recent;
        }

        public static Dictionary<string, Playlist> loadPlaylists()
        {
            Dictionary<string, Playlist> result = new Dictionary<string, Playlist>();
            string directory = @"Playlists";
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);

                using (StreamWriter sw = System.IO.File.CreateText($@"Playlists\CustomPlaylist.txt"))
                {
                    sw.Write("CustomPlaylist");
                }
            }

            result.Add("Recent", loadRecents());
            foreach (string file in Directory.EnumerateFiles(@"Playlists", "*.txt"))
            {
                string[] split = file.Split("\\");

                if (split[split.Length - 1] != "Recent.txt")
                {
                    result.Add(split[split.Length - 1].Substring(0, split[split.Length - 1].Length - 4), loadAPlayList(file));
                }

            }
            return result;
        }

        public static Playlist loadAPlayList(string path)
        {
            Playlist result = new Playlist();
            List<string> lines = System.IO.File.ReadLines(path).ToList();

            for (int i = 0; i < lines.Count; i++)
            {
                if (i == 0)
                {
                    result.Name = lines[i];
                }
                else
                {
                    string[] split = lines[i].Split("?");
                    result.Items.Add(new MediaItem() { Name = split[0], Path = split[1] });
                }
            }
            return result;
        }

        public static void saveAplayList(Playlist newPlayList) 
        {
            using (StreamWriter sw = System.IO.File.CreateText($@"Playlists\{newPlayList.Name}.txt"))
            {
                sw.Write(newPlayList.Name);
                foreach(MediaItem item in newPlayList.Items)
                {
                    sw.Write("\n");
                    sw.Write(item.Name + "?");
                    sw.Write(item.Path);
                }
            }
        }
    }
}
