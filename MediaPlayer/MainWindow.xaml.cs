﻿using System;
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

        private bool autoPLay = true;

        private bool repeat = false;

        private bool shuffle = false;

        private Random random = new Random();

        private List<int> historyIndex = new List<int>();

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
        public static RoutedCommand PrevCommand = new RoutedCommand();
        public static RoutedCommand NextCommand = new RoutedCommand();
        public static RoutedCommand MuteCommand = new RoutedCommand();

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;

            this.CommandBindings.Add(new CommandBinding(PLayCommand));
            this.CommandBindings.Add(new CommandBinding(PrevCommand));
            this.CommandBindings.Add(new CommandBinding(NextCommand));
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
            var bitmapAutoplay = new BitmapImage(new Uri(path + "\\autoplay.png", UriKind.Absolute));
            autoplayButtonImage.Source = bitmapAutoplay;
            var bitmapShuffle = new BitmapImage(new Uri(path + "\\shuffle.png", UriKind.Absolute));
            shuffleButtonImage.Source = bitmapShuffle;
            var bitmapRepeat = new BitmapImage(new Uri(path + "\\repeat.png", UriKind.Absolute));
            repeatButtonImage.Source = bitmapRepeat;
            var bitmapAddPlaylist = new BitmapImage(new Uri(path + "\\add_playlist.png", UriKind.Absolute));
            addPlaylistButtonImage.Source = bitmapAddPlaylist;
            var bitmapAddMedia = new BitmapImage(new Uri(path + "\\add_media.png", UriKind.Absolute));
            addMediaButtonImage.Source = bitmapAddMedia;
            var bitmapTrashBin = new BitmapImage(new Uri(path + "\\trash_bin.png", UriKind.Absolute));
            deletePlaylistButtonImage.Source = bitmapTrashBin;

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
                progressSlider.Value = player.Position.TotalSeconds;
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
            if (repeat == true)
            {
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
            else
            {
                if (autoPLay == true)
                {
                    if (shuffle)
                    {
                        if (historyIndex.Count == ListMediaItem.Items.Count)
                        {
                            historyIndex = new List<int>();
                        }
                        int index = random.Next(0, ListMediaItem.Items.Count);
                        while (historyIndex.Contains(index))
                        {
                            index = random.Next(0, ListMediaItem.Items.Count);
                        }
                        ListMediaItem.SelectedIndex = index;
                        historyIndex.Add(index);
                    }
                    else
                    {
                        if (ListMediaItem.SelectedIndex != ListMediaItem.Items.Count - 1)
                        {
                            ListMediaItem.SelectedIndex += 1;
                        }
                    }
                }
            }
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

            player.Pause();
            _timer = new DispatcherTimer();
            currentPosition.Text = "00:00:00";
            totalPosition.Text = "00:00:00";
            historyIndex = new List<int>();
        }

        private void MediaItem_Changed(object sender, SelectionChangedEventArgs e)
        {
            MediaItem cur = (MediaItem)ListMediaItem.SelectedItem;
            if (cur != null)
            {
                _currentPlaying = cur.Path;

                player.Source = new Uri(_currentPlaying, UriKind.Absolute);
                if (curPlayListName != "Recent")
                {
                    PlaylistService.updateRecentPlayList(playlists["Recent"], cur);
                }
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
            string path = Path.GetFullPath(@"Icons");
            if (shuffle)
            {
                var bitmap = new BitmapImage(new Uri(path + "\\shuffle.png", UriKind.Absolute));
                shuffleButtonImage.Source = bitmap;
                historyIndex = new List<int>();
            }
            else
            {
                var bitmap = new BitmapImage(new Uri(path + "\\shuffle_on.png", UriKind.Absolute));
                shuffleButtonImage.Source = bitmap;
                if (!_playing)
                {
                    int index = random.Next(0, ListMediaItem.Items.Count);
                    ListMediaItem.SelectedIndex = index;
                    historyIndex.Add(index);
                }
            }
            shuffle = !shuffle;
            //string cur = (string)ListPlaylist.SelectedItem;
            //if (cur != "Recent")
            //{
            //    Random random = new Random();

            //    if (cur != null)
            //    {
            //        ViewUtils.updateListMediaView(ListMediaItem, _workingMediaItems, playlists[cur].Items.OrderBy(x => random.Next()).ToList());
            //        ListMediaItem.SelectedIndex = 0;
            //    }
            //    else
            //    {
            //        ViewUtils.updateListMediaView(ListMediaItem, _workingMediaItems, playlists["Recent"].Items.OrderBy(x => random.Next()).ToList());
            //        ListMediaItem.SelectedIndex = 0;
            //    }
            //}
        }

        private void prevMedia()
        {
            if (shuffle)
            {
                int index = historyIndex.IndexOf(ListMediaItem.SelectedIndex) - 1;
                if (index > -1)
                {
                    ListMediaItem.SelectedIndex = historyIndex[index];
                    Title = $"{ListMediaItem.SelectedIndex}";
                }
            }
            else
            {
                int index = ListMediaItem.SelectedIndex - 1;
                if (index < 0) return;
                ListMediaItem.SelectedIndex = index;
            }
        }

        private void nextMedia()
        {
            if (shuffle)
            {
                int hIndex = historyIndex.IndexOf(ListMediaItem.SelectedIndex) + 1;
                if (hIndex < historyIndex.Count)
                {
                    ListMediaItem.SelectedIndex = historyIndex[hIndex];
                }
                else
                {
                    if (historyIndex.Count == ListMediaItem.Items.Count)
                    {
                        historyIndex = new List<int>();
                    }
                    int index = random.Next(0, ListMediaItem.Items.Count);
                    while (historyIndex.Contains(index))
                    {
                        index = random.Next(0, ListMediaItem.Items.Count);
                    }
                    ListMediaItem.SelectedIndex = index;
                    historyIndex.Add(index);
                }
                Title = $"{ListMediaItem.SelectedIndex}";
            }
            else
            {
                int length = ListMediaItem.Items.Count;
                int index = ListMediaItem.SelectedIndex + 1;
                if (index == length) return;
                ListMediaItem.SelectedIndex = index;
            }
        }

        private void prevButton_Click(object sender, RoutedEventArgs e)
        {
            prevMedia();
        }

        private void nextButton_Click(object sender, RoutedEventArgs e)
        {
            nextMedia();
        }

        private void player_Loaded(object sender, RoutedEventArgs e)
        {
            player.Play();
            player.Pause();
        }

        private void prevCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void prevCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            prevMedia();
        }

        private void nextCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void nextCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            nextMedia();
        }

        private void AutoPlay_Click(object sender, RoutedEventArgs e)
        {
            string path = Path.GetFullPath(@"Icons");
            if (autoPLay)
            {
                var bitmap = new BitmapImage(new Uri(path + "\\autoplay_off.png", UriKind.Absolute));
                autoplayButtonImage.Source = bitmap;
            }
            else
            {
                var bitmap = new BitmapImage(new Uri(path + "\\autoplay.png", UriKind.Absolute));
                autoplayButtonImage.Source = bitmap;
            }
            autoPLay = !autoPLay;
        }

        private void Repeat_Click(object sender, RoutedEventArgs e)
        {
            string path = Path.GetFullPath(@"Icons");
            if (repeat)
            {
                var bitmap = new BitmapImage(new Uri(path + "\\repeat.png", UriKind.Absolute));
                repeatButtonImage.Source = bitmap;
            }
            else
            {
                var bitmap = new BitmapImage(new Uri(path + "\\repeat_on.png", UriKind.Absolute));
                repeatButtonImage.Source = bitmap;
            }
            repeat = !repeat;
        }
    }
}
