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
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Timers;
using Microsoft.Win32;
using System.Windows.Media.Animation;

namespace MusicPlayerWPF
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        States.StatesOfPlayer state;
        private System.Timers.Timer timer;
        string[] musics;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            media.Source = new Uri(@"C:\Users\Grumm\Desktop\links.mp3", UriKind.Relative);

            timer = new System.Timers.Timer(500);
            timer.Elapsed += Timer_Elapsed;
            media.MediaEnded += Media_MediaEnded;
        }

        

        private void buttonFolder_Click(object sender, RoutedEventArgs e)
        {
            using (FolderBrowserDialog folder = new FolderBrowserDialog())
            {
                if (folder.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    folder.RootFolder = Environment.SpecialFolder.Desktop;
                    textPath.Text = folder.SelectedPath;

                    musics = Directory.GetFiles(folder.SelectedPath, "*.mp3", SearchOption.TopDirectoryOnly);
                }
            }
        }

        private void buttonPlayPause_Click(object sender, RoutedEventArgs e)
        {
            if (state == States.StatesOfPlayer.Play)
            {
                timer.Stop();
                state = States.StatesOfPlayer.Pause;
                States.SetState(ref state, ref media, ref progres);
            }
            else
            {
                state = States.StatesOfPlayer.Play;
                States.SetState(ref state, ref media, ref progres);
                timer.Start();
            }
        }

        //позициЯ из media.Position

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                progres.Value+=500;
            });
        }

        private void buttonRight_Click(object sender, RoutedEventArgs e)
        {
        }

        private void media_MediaOpened_1(object sender, RoutedEventArgs e)
        {
            progres.Maximum = Convert.ToDouble(media.NaturalDuration.TimeSpan.TotalMilliseconds);
        }

        private void mediaVolume_ValueChanged_1(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            media.Volume = (double)e.NewValue;
            //System.Windows.Forms.MessageBox.Show(e.NewValue.ToString());
            if (btImageVolume!=null)
            {
                ChangeVolumeImage(media.Volume);
            }
        }

        private void ChangeVolumeImage(double volume)
        {
            if (volume == 0.0)
            {
                ChoiseImage("VolumeMin.png");
            }
            else if (volume > 0 & volume <= 0.33)
            {
                ChoiseImage("VolumeOne.png");
            }
            else if(volume >0.33 & volume <= 0.66)
            {
                ChoiseImage("VolumeTwo.png");
            }
            else
            {
                ChoiseImage("VolumeMax.png");
            }
        }
        private void ChoiseImage(string filename)
        {
            Image img = btImageVolume;
            
            string imagePath = $@"/Images/{filename}";
            BitmapImage bitmapImage = new BitmapImage(new Uri(imagePath, UriKind.Relative));
            img.Source = bitmapImage;
        }

        private void Media_MediaEnded(object sender, RoutedEventArgs e)
        {
            state = States.StatesOfPlayer.Stop;
            States.SetState(ref state, ref media, ref progres);
            timer.Stop();
        }

        private void progres_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            textPath.Text = progres.Value.ToString();
        }
        
        private void progres_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                Point clickPosition = e.GetPosition(progres);
                double newValue = (clickPosition.X / progres.ActualWidth) * (progres.Maximum - progres.Minimum) + progres.Minimum;
                progres.SetValue(Slider.ValueProperty, newValue);
                media.Position = TimeSpan.FromMilliseconds(newValue);
            });
        }

        private void bt_VolumeImage_Click(object sender, RoutedEventArgs e)
        {
            //System.Windows.Forms.MessageBox.Show("TestTestTestTestTestTestTestTestTestTestTestTestTestTest");
            System.Windows.Controls.Button bt = (System.Windows.Controls.Button)sender;
            Image img = (Image)bt.Content;

            if (media.Volume != 0)
            {
                string imagePath = @"/Images/VolumeMin.png";

                BitmapImage bitmapImage = new BitmapImage(new Uri(imagePath, UriKind.Relative));
                img.Source = bitmapImage;
                media.Volume = 0;
            }
            else
            {
                string imagePath = @"/Images/VolumeMax.png";

                BitmapImage bitmapImage = new BitmapImage(new Uri(imagePath, UriKind.Relative));
                img.Source = bitmapImage;
                media.Volume = mediaVolume.Value;
            }
        }
    }
    struct States
    {
        public enum StatesOfPlayer
        {
            Manual,
            Play,
            Close,
            Pause,
            Stop
        }

        public static void SetState(ref StatesOfPlayer s, ref MediaElement el, ref Slider progres)
        {
            switch (s)
            {
                case StatesOfPlayer.Play: 
                    el.Play();
                    break;
                case StatesOfPlayer.Close: el.Close(); break;
                case StatesOfPlayer.Pause: el.Pause(); break;
                case StatesOfPlayer.Stop: el.Stop(); break;
            }
        }
    }
}
