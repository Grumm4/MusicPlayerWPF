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
using MusicPlayerWPF;
using System.Windows.Controls.Primitives;
using System.Diagnostics;
using System.Windows.Markup;
using System.Windows.Media;

namespace MusicPlayerWPF
{
    public partial class MainWindow : Window
    {
        private States.StatesOfPlayer state;
        private System.Timers.Timer timer;
        private string[] musics; // массив путей до файлов
        private int[] musicsPositions;
        private int count; //счётчик, какая по счёту открыта песня
        private Song song;
        internal static ResourceDictionary resDictionary = (ResourceDictionary)XamlReader.Parse(System.IO.File.ReadAllText("Symbols.xaml"));

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
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                progres.Value += 500;
            });
        }

        //Кнопки
        private void ButtonFolder_Click(object sender, RoutedEventArgs e) //Открытие директории
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

        private void ButtonPlayPause_Click(object sender, RoutedEventArgs e) //Кнопка воспроизведения
        {
            if (media.Source == null)
                return;
            if (state == States.StatesOfPlayer.Play)
            {
                state = States.StatesOfPlayer.Pause;
                States.SetState(ref state, ref media, ref timer, ref buttonPlayPause);
            }
            else
            {
                state = States.StatesOfPlayer.Play;
                States.SetState(ref state, ref media, ref timer, ref buttonPlayPause);
            }
        }

        private void ButtonRight_Click(object sender, RoutedEventArgs e) //Вправо
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

        private void ButtonLeft_Click(object sender, RoutedEventArgs e) //Влево
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

        private void Bt_VolumeImage_Click(object sender, RoutedEventArgs e) //Кнопка громкости (на которой картинка с величиной текущей громкости)
        {
            System.Windows.Controls.Button bt = (System.Windows.Controls.Button)sender;

            if (media.Volume != 0)
            {
                bt.Content = (string)resDictionary["VolumeMin"];
                media.Volume = 0;
            }
            else
            {
                media.Volume = mediaVolume.Value;
                ChangeVolumeImage(media.Volume);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
        }


        //Открытие медиа, перелистывание влево вправо
        private void OpenMedia(object sender)
        {
            System.Windows.Controls.Button bt = sender as System.Windows.Controls.Button;
            
            if (bt.Name == buttonRight.Name)
                MediaNext();
            else if (bt.Name == buttonLeft.Name)
                MediaPrevious();
        }

        private void MediaNext()
        {
            if (!(count + 1 >= musics.Length))
                media.Source = new Uri(musics[++count]);
            else
            {
                progres.Value = progres.Minimum;
                state = States.StatesOfPlayer.Stop;
                States.SetState(ref state, ref media, ref timer, ref buttonPlayPause);

            }
        }

        private void MediaPrevious()
        {
            if (!(count - 1 < 0))
                media.Source = new Uri(musics[--count]);
        }
        //
        

        private void Media_MediaOpened(object sender, RoutedEventArgs e)
        {
            song = new Song((sender as MediaElement).Source.LocalPath);
            progres.Value = progres.Minimum;
            progres.Maximum = Convert.ToDouble(media.NaturalDuration.TimeSpan.TotalMilliseconds);
            labelTitle.Content = song.GetTitleAndArtist();
            //System.Windows.Forms.MessageBox.Show(song.GetTitleAndArtist());
        }

        private void Media_MediaEnded(object sender, RoutedEventArgs e)
        {
            MediaNext();
        }


        private void MediaVolume_ValueChanged_1(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            media.Volume = (double)e.NewValue;
            //System.Windows.Forms.MessageBox.Show(e.NewValue.ToString());
            ChangeVolumeImage(media.Volume);
        }
        
        //проверка громкости и установка соответствующей картинки
        private void ChangeVolumeImage(double volume)
        {
            if(btImageVolume != null)
            {
                if (volume == 0.0)
                {
                    btImageVolume.Content = (string)resDictionary["VolumeMin"];
                }
                else if (volume > 0 & volume <= 0.33)
                {
                    btImageVolume.Content = (string)resDictionary["VolumeOne"];
                }
                else if (volume > 0.33 & volume <= 0.66)
                {
                    btImageVolume.Content = (string)resDictionary["VolumeTwo"];
                }
                else
                {
                    btImageVolume.Content = (string)resDictionary["VolumeMax"];

                }
            }
        }
        

        private void Progres_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            textPath.Text = progres.Value.ToString();
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                Track track = progres.Template.FindName("PART_Track", progres) as Track;
                if (track.IsMouseOver)
                {
                    //System.Windows.Forms.MessageBox.Show("ValueChanged");
                    media.Position = TimeSpan.FromMilliseconds(progres.Value);
                }
            }
        }

        private void progres_DragStarted(object sender, DragStartedEventArgs e)
        {
            media.Volume = 0;
        }

        private void Progres_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            media.Volume = mediaVolume.Value;
            //media.Position = TimeSpan.FromMilliseconds(progres.Value);
        }

        private void Slider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            //System.Windows.Forms.MessageBox.Show("DragCompleted");
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

    public static void SetState(ref StatesOfPlayer s, ref MediaElement el, ref System.Timers.Timer t, ref System.Windows.Controls.Button PlayPause)
    {
        
        //MainWindow window = new MainWindow();
        switch (s)
        {
            case StatesOfPlayer.Play:
                el.Play();
                t.Start();
                PlayPause.Content = (string)MainWindow.resDictionary["Pause"];
                break;
            case StatesOfPlayer.Close: el.Close(); break;
            case StatesOfPlayer.Pause: 
                el.Pause();
                t.Stop();
                PlayPause.Content = (string)MainWindow.resDictionary["Play"];
                break;
            case StatesOfPlayer.Stop:
                el.Stop();
                t.Stop();
                PlayPause.Content = (string)MainWindow.resDictionary["Play"];
                break;
        }
    }
}
