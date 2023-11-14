using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Forms;
using System.IO;
using System.Timers;
using NAudio.Wave;
using TagLib;
using System.Runtime.CompilerServices;

namespace MusicPlayerWPF
{
    public partial class MainWindow : Window
    {
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

            public static void SetState(ref StatesOfPlayer s, ref MediaElement el, ref System.Timers.Timer t)
            {
                MainWindow window = new MainWindow();
                switch (s)
                {
                    case StatesOfPlayer.Play:
                        el.Play();
                        t.Start();
                        
                        break;
                    case StatesOfPlayer.Close: el.Close(); break;
                    case StatesOfPlayer.Pause: el.Pause(); break;
                    case StatesOfPlayer.Stop:
                        el.Stop();
                        t.Stop();
                        break;
                }
            }
        }

        private States.StatesOfPlayer state;
        private System.Timers.Timer timer;
        private string[] musics;
        private int count;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //media.Source = new Uri(@"C:\Users\Grumm\Desktop\links.mp3", UriKind.Relative);

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
                media.Source = new Uri(musics[count]);
            }
        }

        private void buttonPlayPause_Click(object sender, RoutedEventArgs e)
        {
            if (state == States.StatesOfPlayer.Play)
            {
                state = States.StatesOfPlayer.Pause;
                States.SetState(ref state, ref media, ref timer);
            }
            else
            {
                state = States.StatesOfPlayer.Play;
                States.SetState(ref state, ref media, ref timer);
            }

            SetTitleAndArtist();
        }

        private void SetTitleAndArtist()
        {
            string filePath = musics[count];
            using (var audioFile = new AudioFileReader(filePath))
            {
                var tagFile = TagLib.File.Create(filePath);
                string artist = tagFile.Tag.FirstPerformer;
                string title = tagFile.Tag.Title;

                if (!string.IsNullOrEmpty(title) & !string.IsNullOrEmpty(artist))
                {
                    labelTitle.Content = new StringBuilder($"{artist} -- {title}");
                }
                else
                {
                    labelTitle.Content = System.IO.Path.GetFileName(title);
                }
            }
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                progres.Value+=500;
            });
        }

        private void buttonRight_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenMedia(sender);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void buttonLeft_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenMedia(sender);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        void OpenMedia(object sender)
        {
            System.Windows.Controls.Button bt = sender as System.Windows.Controls.Button;
            if (bt.Name == buttonRight.Name)
            {
                if (!(count + 1 >= musics.Length))
                    media.Source = new Uri(musics[++count]);
            }
            if (bt.Name == buttonLeft.Name)
            {
                if (!(count - 1 < 0))
                    media.Source = new Uri(musics[--count]);
            }
        }
            

        private void media_MediaOpened_1(object sender, RoutedEventArgs e)
        {
            progres.Value = progres.Minimum;
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
        //проверка громкости и вызов метода ChoiseImage, с передачей ему в параметры громкости
        private void ChangeVolumeImage(double volume)
        {
            if (volume == 0.0)
            {
                ChoiseImage("VolumeMin");
            }
            else if (volume > 0 & volume <= 0.33)
            {
                ChoiseImage("VolumeOne");
            }
            else if(volume >0.33 & volume <= 0.66)
            {
                ChoiseImage("VolumeTwo");
            }
            else
            {
                ChoiseImage("VolumeMax");
            }
        }
        //установка картинки громкости
        private void ChoiseImage(string filename)
        {
            Image img = btImageVolume;
            
            string imagePath = $@"/Images/{filename}.png";
            BitmapImage bitmapImage = new BitmapImage(new Uri(imagePath, UriKind.Relative));
            img.Source = bitmapImage;
        }

        private void Media_MediaEnded(object sender, RoutedEventArgs e)
        {
            state = States.StatesOfPlayer.Stop;
            States.SetState(ref state, ref media, ref timer);
        }

        private void progres_ValueChanged(object sender, MouseButtonEventArgs e)
        {
            textPath.Text = progres.Value.ToString();

            if (progres.IsMouseCaptureWithin)
            {
                media.Position = TimeSpan.FromMilliseconds(progres.Value);
            }
        }
        
        private void progres_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //Dispatcher.Invoke(() =>
            //{
            //    Point clickPosition = e.GetPosition(progres);
            //    double newValue = (clickPosition.X / progres.ActualWidth) * (progres.Maximum - progres.Minimum) + progres.Minimum;
            //    progres.SetValue(Slider.ValueProperty, newValue);
            //    media.Position = TimeSpan.FromMilliseconds(newValue);
            //});

            //C# ЭТО ИЗИ НАХУЙ СУКА БЛЯТЬ БИМБИМ БАМБАМ
            media.Position = TimeSpan.FromMilliseconds(progres.Value);
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
    
}
