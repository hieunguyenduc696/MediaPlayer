using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.IO;
using System.Timers;
using System.Windows.Threading;
using System.Reflection;
using Path = System.IO.Path;
using MediaPlayer.Classes;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.ConstrainedExecution;
using System.Windows.Controls.Primitives;

namespace MediaPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _currentPlaying = string.Empty;

        private string curPlayListName;
        private Dictionary<string,Playlist> playlists = new Dictionary<string, Playlist>();

        private ObservableCollection<string> _listOfPlaylist;
        private ObservableCollection<MediaItem> _workingMediaItems;

        private bool _playing = false;

        private bool _dragStarted = false;

        private double _volumeOld = 1;

        private double _volume = 1;

        private bool _muted = false;

        private string _shortName
        {
            get
            {
                var info = new FileInfo(_currentPlaying);
                var name = info.Name;

                return name;
            }
        }

        public static RoutedCommand PLayCommand = new RoutedCommand();

        public static RoutedCommand MuteCommand = new RoutedCommand();

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;

            this.CommandBindings.Add(new CommandBinding(PLayCommand));
            this.CommandBindings.Add(new CommandBinding(MuteCommand));
        }

        public void configBindingResource()
        {
            _listOfPlaylist = new ObservableCollection<string>();
            _workingMediaItems = new ObservableCollection<MediaItem>();

            playlists = FileService.loadPlaylists();

            playlists = FileService.loadPlaylists();
            _listOfPlaylist = PlaylistService.getListOfPlaylists(playlists);

            _workingMediaItems = new ObservableCollection<MediaItem>(playlists["Recent"].Items);
            curPlayListName = "Recent";

            ListPlaylist.ItemsSource = _listOfPlaylist;
            ListPlaylist.SelectedIndex = 0;

            ListMediaItem.ItemsSource = _workingMediaItems;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string path = Path.GetFullPath(@"Icons");
            var bitmapPlay = new BitmapImage(new Uri(path + "\\play.png", UriKind.Absolute));
            playButtonImage.Source = bitmapPlay;
            var bitmapPrev = new BitmapImage(new Uri(path + "\\prev.png", UriKind.Absolute));
            prevButtonImage.Source = bitmapPrev;
            var bitmapNext = new BitmapImage(new Uri(path + "\\next.png", UriKind.Absolute));
            nextButtonImage.Source = bitmapNext;
            var bitmapVolume = new BitmapImage(new Uri(path + "\\volume.png", UriKind.Absolute));
            volumeButtonImage.Source = bitmapVolume;
            var bitmapShuffle = new BitmapImage(new Uri(path + "\\shuffle.png", UriKind.Absolute));
            shuffleButtonImage.Source = bitmapShuffle;

            volumeSlider.Value = _volume * 100;
            player.Volume = _volume;

            configBindingResource();
        }

        DispatcherTimer _timer;

        private void browse_Click(object sender, RoutedEventArgs e)
        {
            var screen = new OpenFileDialog();
            if (screen.ShowDialog() == true)
            {
                _currentPlaying = screen.FileName;

                //this.Title = $"Opened: {_shortName}";
                player.Source = new Uri(_currentPlaying, UriKind.Absolute);

                _timer = new DispatcherTimer();
                _timer.Interval = new TimeSpan(0, 0, 0, 1, 0); ;
                _timer.Tick += _timer_Tick;
            }
        }

        private void _timer_Tick(object? sender, EventArgs e)
        {
            if (!_dragStarted)
            {
                string hours = player.Position.Hours.ToString();
                string minutes = player.Position.Minutes.ToString();
                string seconds = player.Position.Seconds.ToString();
                if (hours.Length == 1) hours = "0" + hours;
                if (minutes.Length == 1) minutes = "0" + minutes;
                if (seconds.Length == 1) seconds = "0" + seconds;
                currentPosition.Text = $"{hours}:{minutes}:{seconds}";
            }
        }

        private void playMedia()
        {
            if (!string.IsNullOrEmpty(_currentPlaying))
            {
                if (_playing)
                {
                    string path = Path.GetFullPath(@"Icons");
                    var bitmap = new BitmapImage(new Uri(path + "\\play.png", UriKind.Absolute));
                    playButtonImage.Source = bitmap;
                    playButton.ToolTip = "Play (k)";
                    player.Pause();
                    _playing = false;
                    //Title = $"Stop playing: {_shortName}";
                    _timer.Stop();
                }
                else
                {
                    string path = Path.GetFullPath(@"Icons");
                    var bitmap = new BitmapImage(new Uri(path + "\\pause.png", UriKind.Absolute));
                    playButtonImage.Source = bitmap;
                    playButton.ToolTip = "Pause (k)";
                    _playing = true;
                    player.Play();
                    //Title = $"Playing: {_shortName}";
                    _timer.Start();
                }
            }
        }

        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            playMedia();
        }

        private void player_MediaOpened(object sender, RoutedEventArgs e)
        {
            double total = player.NaturalDuration.TimeSpan.TotalSeconds;
            string hours = ((int)Math.Floor(total / 3600)).ToString();
            string minutes = ((int)Math.Floor(total / 60)).ToString();
            string seconds = ((int)Math.Floor(total) % 60).ToString();
            if (hours.Length == 1) hours = "0" + hours;
            if (minutes.Length == 1) minutes = "0" + minutes;
            if (seconds.Length == 1) seconds = "0" + seconds;

            totalPosition.Text = $"{hours}:{minutes}:{seconds}";

            progressSlider.Maximum = player.NaturalDuration.TimeSpan.TotalSeconds;
        }

        private void player_MediaEnded(object sender, RoutedEventArgs e)
        {

        }

        private void progressSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_dragStarted)
            {
                double value = progressSlider.Value;
                TimeSpan newPosition = TimeSpan.FromSeconds(value);
                player.Position = newPosition;
            }

            double total = progressSlider.Value;
            string hours = ((int)Math.Floor(total / 3600)).ToString();
            string minutes = ((int)Math.Floor(total / 60)).ToString();
            string seconds = ((int)Math.Floor(total) % 60).ToString();
            if (hours.Length == 1) hours = "0" + hours;
            if (minutes.Length == 1) minutes = "0" + minutes;
            if (seconds.Length == 1) seconds = "0" + seconds;

            currentPosition.Text = $"{hours}:{minutes}:{seconds}";

        }

        private void progressSlider_DragStarted(object sender, DragStartedEventArgs e)
        {
            this._dragStarted = true;
        }

        private void progressSlider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            double value = progressSlider.Value;
            TimeSpan newPosition = TimeSpan.FromSeconds(value);
            player.Position = newPosition;

            this._dragStarted = false;
        }

        private void muteMedia()
        {
            string path = Path.GetFullPath(@"Icons");
            if (_muted)
            {
                var bitmap = new BitmapImage(new Uri(path + "\\volume.png", UriKind.Absolute));
                volumeButtonImage.Source = bitmap;
                volumeButton.ToolTip = "Mute (m)";
                volumeSlider.Value = _volumeOld;
                _muted = false;
            }
            else
            {
                _volumeOld = volumeSlider.Value;
                var bitmap = new BitmapImage(new Uri(path + "\\muted.png", UriKind.Absolute));
                volumeButtonImage.Source = bitmap;
                volumeButton.ToolTip = "Unmute (m)";
                volumeSlider.Value = 0;
                _muted = true;
            }
            player.Volume = volumeSlider.Value / 100;
        }

        private void volumeButton_Click(object sender, RoutedEventArgs e)
        {
            muteMedia();
        }

        private void volumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            string path = Path.GetFullPath(@"Icons");
            if (volumeSlider.Value == 0)
            {
                var bitmap = new BitmapImage(new Uri(path + "\\muted.png", UriKind.Absolute));
                volumeButtonImage.Source = bitmap;
                _muted = true;
            }
            else
            {
                if (_muted)
                {
                    var bitmap = new BitmapImage(new Uri(path + "\\volume.png", UriKind.Absolute));
                    volumeButtonImage.Source = bitmap;
                    _muted = false;
                }
                _volume = volumeSlider.Value;
            }
            player.Volume = volumeSlider.Value / 100;
        }

        private void playCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void playCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            playMedia();
        }

        private void muteCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void muteCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            muteMedia();

        }

        private void removePlaylistItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddItem_Click(object sender, RoutedEventArgs e)
        {
            var screen = new OpenFileDialog();
            screen.Multiselect = true;
            screen.Filter = "All Media Files|*.wav;*.aac;*.wma;*.wmv;*.avi;*.mpg;*.mpeg;*.m1v;*.mp2;*.mp3;*.mpa;*.mpe;*.m3u;*.mp4;*.mov;*.3g2;*.3gp2;*.3gp;*.3gpp;*.m4a;*.cda;*.aif;*.aifc;*.aiff;*.mid;*.midi;*.rmi;*.mkv;*.WAV;*.AAC;*.WMA;*.WMV;*.AVI;*.MPG;*.MPEG;*.M1V;*.MP2;*.MP3;*.MPA;*.MPE;*.M3U;*.MP4;*.MOV;*.3G2;*.3GP2;*.3GP;*.3GPP;*.M4A;*.CDA;*.AIF;*.AIFC;*.AIFF;*.MID;*.MIDI;*.RMI;*.MKV";
            
            if (screen.ShowDialog() == true)
            {
                foreach (var file in screen.FileNames)
                {
                    PlaylistService.addMediaFile(playlists[curPlayListName], file);
                    ViewUtils.updateListMediaView(ListMediaItem, _workingMediaItems, playlists[curPlayListName].Items);
                }
            }
        }

        private void NewPlayList_Click(object sender, RoutedEventArgs e)
        {
            var screen = new NewPlaylist();
            screen.playLists = playlists;
            screen.Owner = this;
            if (screen.ShowDialog() == true)
            {
                string newPlayListName = screen.NameInput;

                Playlist newPlaylist = new Playlist() { Name = newPlayListName, Items = new List<MediaItem>() };
                playlists.Add(newPlayListName, newPlaylist);

                _listOfPlaylist.Add(newPlayListName);
                ListPlaylist.SelectedIndex = ListPlaylist.Items.Count - 1;

                FileService.saveAplayList(newPlaylist);
                ViewUtils.updateListMediaView(ListMediaItem, _workingMediaItems, newPlaylist.Items);

            }
        }

        private void ListPlayList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            string cur = (string)ListPlaylist.SelectedItem;
          
            if (cur != null)
            {
                curPlayListName = cur;
                ViewUtils.updateListMediaView(ListMediaItem, _workingMediaItems, playlists[cur].Items);
                
            }
            else
            {
                ViewUtils.updateListMediaView(ListMediaItem, _workingMediaItems, playlists["Recent"].Items);
                curPlayListName = "Recent";
            }

            if (curPlayListName == "Recent")
            {
                AddNewMediaBtn.IsEnabled = false;
                DeletePlaylistBtn.IsEnabled = false;
            }
            else
            {
                AddNewMediaBtn.IsEnabled = true;
                DeletePlaylistBtn.IsEnabled = true;
            }

        }

        private void MediaItem_Changed(object sender, SelectionChangedEventArgs e)
        {
            MediaItem cur = (MediaItem)ListMediaItem.SelectedItem;
            if (cur != null)
            {
                _currentPlaying = cur.Path;

                //this.Title = $"Opened: {_shortName}";
                player.Source = new Uri(_currentPlaying, UriKind.Absolute);

                _timer = new DispatcherTimer();
                _timer.Interval = new TimeSpan(0, 0, 0, 1, 0); ;
                _timer.Tick += _timer_Tick;

                progressSlider.Value = 0;
                currentPosition.Text = "00:00:00";
                if (_playing)
                {
                    player.Play();
                    _timer.Start();
                }
            }

        }

        private void removeMediaItem_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button btn = e.Source as System.Windows.Controls.Button;
            MediaItem item = btn.DataContext as MediaItem;

            playlists[curPlayListName].Items.Remove(item);
            FileService.saveAplayList(playlists[curPlayListName]);
            ViewUtils.updateListMediaView(ListMediaItem, _workingMediaItems, playlists[curPlayListName].Items);
        }

        private void DeletePlayList_Click(object sender, RoutedEventArgs e)
        {
            string location = $@"Playlists\{curPlayListName}.txt";

            playlists.Remove(curPlayListName);
            _listOfPlaylist.Remove(curPlayListName);
            ListPlaylist.SelectedIndex = 0;

            if (System.IO.File.Exists(location))
            {
                System.IO.File.Delete(location);
            }

           
        }

        private void shuffleButton_Click(object sender, RoutedEventArgs e)
        {
            string cur = (string)ListPlaylist.SelectedItem;
            Random random = new Random();

            if (cur != null)
            {
                curPlayListName = cur;
                ViewUtils.updateListMediaView(ListMediaItem, _workingMediaItems, playlists[cur].Items.OrderBy(x => random.Next()).ToList());

            }
            else
            {
                ViewUtils.updateListMediaView(ListMediaItem, _workingMediaItems, playlists["Recent"].Items.OrderBy(x => random.Next()).ToList());
                curPlayListName = "Recent";
            }
        }

        private void prevButton_Click(object sender, RoutedEventArgs e)
        {
            int length = ListMediaItem.Items.Count;
            int index = ListMediaItem.SelectedIndex - 1;
            if (index < 0) index = length - 1;
            ListMediaItem.SelectedIndex = index;
            Title = $"{index}";
        }

        private void nextButton_Click(object sender, RoutedEventArgs e)
        {
            int length = ListMediaItem.Items.Count;
            int index = ListMediaItem.SelectedIndex + 1;
            if (index == length) index = 0;
            ListMediaItem.SelectedIndex = index;
            Title = $"{index}";
        }

        private void player_Loaded(object sender, RoutedEventArgs e)
        {
            player.Play();
            player.Pause();
        }
    }
}
