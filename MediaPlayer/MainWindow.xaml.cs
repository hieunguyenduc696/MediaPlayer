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

namespace MediaPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _currentPlaying = string.Empty;

        private bool _playing = false;

        private string _shortName
        {
            get
            {
                var info = new FileInfo(_currentPlaying);
                var name = info.Name;

                return name;
            }
        }
        public MainWindow()
        {
            InitializeComponent();
        }

        DispatcherTimer _timer;

        private void browse_Click(object sender, RoutedEventArgs e)
        {
            var screen = new OpenFileDialog();
            if (screen.ShowDialog() == true)
            {
                _currentPlaying = screen.FileName;

                this.Title = $"Opened: {_shortName}";
                player.Source = new Uri(_currentPlaying, UriKind.Absolute);

                _timer = new DispatcherTimer();
                _timer.Interval = new TimeSpan(0, 0, 0, 1, 0); ;
                _timer.Tick += _timer_Tick;
            }
        }
        private void _timer_Tick(object? sender, EventArgs e)
        {
            int hours = player.Position.Hours;
            int minutes = player.Position.Minutes;
            int seconds = player.Position.Seconds;
            currentPosition.Text = $"{hours}:{minutes}:{seconds}";
            Title = $"{hours}:{minutes}:{seconds}";
        }

        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            if (_playing)
            {
                player.Pause();
                _playing = false;
                playButton.Content = "Play";
                Title = $"Stop playing: {_shortName}";
                _timer.Stop();
            }
            else
            {
                _playing = true;
                player.Play();
                playButton.Content = "Pause";
                Title = $"Playing: {_shortName}";
                _timer.Start();
            }
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            player.Stop();
            Title = $"Stop playing: {_shortName}";
            _playing = false;
        }

        private void player_MediaOpened(object sender, RoutedEventArgs e)
        {
            int hours = player.Position.Hours;
            int minutes = player.Position.Minutes;
            int seconds = player.Position.Seconds;
            totalPosition.Text = $"{hours}:{minutes}:{seconds}";

            progressSlider.Maximum = player.NaturalDuration.TimeSpan.TotalSeconds;
        }

        private void progressSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double value = progressSlider.Value;
            TimeSpan newPosition = TimeSpan.FromSeconds(value);
            player.Position = newPosition;
        }

        private void player_MediaEnded(object sender, RoutedEventArgs e)
        {

        }
    }
}
