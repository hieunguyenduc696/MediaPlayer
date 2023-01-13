using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MediaPlayer.Classes
{
    public class ViewUtils
    {
        public static void updateListMediaView(ListView target, ObservableCollection<MediaItem> items, List<MediaItem> itemsData)
        {
            items = new ObservableCollection<MediaItem>(itemsData);
            target.ItemsSource = items;
        }

    }
}
