using MediaPlayer.Classes;
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
using System.Windows.Shapes;

namespace MediaPlayer
{
    /// <summary>
    /// Interaction logic for NewPlaylist.xaml
    /// </summary>
    public partial class NewPlaylist : Window
    {
        public string NameInput { get; set; }
        public Dictionary<string, Playlist> playLists = new Dictionary<string, Playlist>();
        public NewPlaylist()
        {
            InitializeComponent();
        }

        private void OkBtn_Click(object sender, RoutedEventArgs e)
        {
            string input = NameInput_TextBox.Text;
            Status isValidName = PlaylistService.isValidName(playLists, input);

            if (isValidName.TrueValue) {
                NameInput = NameInput_TextBox.Text;
                DialogResult = true;
            }
            else
            {
                MessageBox.Show(isValidName.Message, "Warning!", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
