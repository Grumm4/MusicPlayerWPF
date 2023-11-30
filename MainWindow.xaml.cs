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
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Linq;

namespace MusicPlayerWPF
{
    
    public partial class MainWindow : Window
    {
        private States.StatesOfPlayer state;
        private System.Timers.Timer timer;
        private string[] music; // массив путей до файлов
        private int count; //счётчик, какая по счёту открыта песня
        private Song song;
        internal static ResourceDictionary resDictionary = (ResourceDictionary)XamlReader.Parse(System.IO.File.ReadAllText("Symbols.xaml"));
        bool isRepeat;
        bool isShuffle;
        private string[] shuffleMusic;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            timer = new System.Timers.Timer(1000);
            timer.Elapsed += Timer_Elapsed;
            media.MediaEnded += Media_MediaEnded;
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

                    //music = Directory.GetFiles(folder.SelectedPath, "*.waw", SearchOption.TopDirectoryOnly);
                    music = Directory.EnumerateFiles(folder.SelectedPath, "*.*", SearchOption.TopDirectoryOnly).Where(s => s.EndsWith(".mp3") || s.EndsWith(".wav")).ToArray();
                }
                media.Source = new Uri(music[count]);

                FillDataGrid(music);
            }
        }

        private void ButtonPlayPause_Click(object sender, RoutedEventArgs e) //Кнопка воспроизведения
        {
            if (media.Source == null)
            {
                System.Windows.Forms.MessageBox.Show("Не указана папка с аудиозаписями");
                return;
            }

            if (state == States.StatesOfPlayer.Play)
            {
                state = States.StatesOfPlayer.Pause;
                States.SetState(ref state, ref media, ref timer, ref ppImage);
            }
            else
            {
                state = States.StatesOfPlayer.Play;
                States.SetState(ref state, ref media, ref timer, ref ppImage);
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
            if (isShuffle)
            {
                CheckCount(shuffleMusic, 'N');
                return;
            }
            else
            {
                CheckCount(music, 'N');
                return;
            }
        }

        private void MediaPrevious()
        {
            if (isShuffle)
            {
                CheckCount(shuffleMusic, 'P');
                return;
            }
            else
            {
                CheckCount(music, 'P');
                return;
            }
        }

        private void CheckCount(in string[] arr, char previousNext)
        {
            if (previousNext == 'N')
            {
                if (!(count + 1 >= arr.Length))
                    media.Source = new Uri(arr[++count]);
                else
                {
                    progres.Value = progres.Minimum;
                    state = States.StatesOfPlayer.Stop;
                    States.SetState(ref state, ref media, ref timer, ref ppImage);

                }
            }
            else if (previousNext == 'P')
            {
                if (!(count - 1 < 0))
                    media.Source = new Uri(arr[--count]);
                else
                {
                    media.Source = null;
                    media.Source = new Uri(arr[count]);
                    //progres.Value = progres.Minimum;
                    //state = States.StatesOfPlayer.Stop;
                    //States.SetState(ref state, ref media, ref timer, ref buttonPlayPause);

                }
            }
        }

        private void Media_MediaOpened(object sender, RoutedEventArgs e)
        {
            //songsDataGrid.UnselectAll();
            songsDataGrid.SelectedIndex = count;
            song = new Song((sender as MediaElement).Source.LocalPath);
            progres.Value = progres.Minimum;
            progres.Maximum = Convert.ToDouble(media.NaturalDuration.TimeSpan.TotalSeconds);
            songTitle.Text = song.GetSongName();
            totalTime.Text = song.GetMinSec();
            
        }

        private void Media_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (isRepeat)
            {
                if (isShuffle)
                {
                    media.Source = null;
                    media.Source = new Uri(shuffleMusic[count]);
                }
                else
                {
                    media.Source = null;
                    media.Source = new Uri(music[count]);
                }
                
            }
            else
            {
                MediaNext();
            }
            
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

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                progres.Value += 1;
            });
        }

        private void Progres_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            currTime.Text = $"{media.Position.Minutes}:{media.Position.Seconds}";

            textPath.Text = progres.Value.ToString();
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {

                Track track = progres.Template.FindName("PART_Track", progres) as Track;
                if (track.IsMouseOver)
                {
                    //System.Windows.Forms.MessageBox.Show("ValueChanged");
                    media.Position = TimeSpan.FromSeconds(progres.Value);
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

        private void BtRepeating_Click(object sender, RoutedEventArgs e)
        {
            
            isRepeat = !isRepeat;
            if (isRepeat)
                btRepeating.Style = (Style)FindResource("RandAndShuffle");
            else
            {
                btRepeating.Style = (Style)FindResource("ButtonStyle");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.MessageBox.Show($"{Width}:{Height}");
        }

        private void songsDataGrid_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            int index = songsDataGrid.SelectedIndex;
            count = index;
            if (isShuffle)
            {
                media.Source = new Uri(shuffleMusic[index]);
            }
            else
            {
                media.Source = new Uri(music[index]);
            }
            state = States.StatesOfPlayer.Play;
            States.SetState(ref state, ref media, ref timer, ref ppImage);
        }

        private void buttonRand_Click(object sender, RoutedEventArgs e)
        {
            isShuffle = !isShuffle;
            if (isShuffle)
            {
                buttonRand.Style = (Style)FindResource("RandAndShuffle");
                Shuffle();
            }
            else
            {
                buttonRand.Style = (Style)FindResource("ButtonStyle");
                UnShuffle();
            }
        }

        void FillDataGrid(in string[] arr)
        {
            List<SongModel> list = new List<SongModel>();
            for (int i = 0; i < arr.Length; i++)
            {
                Song song = new Song(arr[i]);
                //System.Windows.Forms.MessageBox.Show(el.NaturalDuration.TimeSpan.TotalSeconds.ToString());
                list.Add(new SongModel()
                {
                    Title = song.GetSongName(),
                    Album = song.album,
                    Artist = song.artist,
                    Duration = song.GetMinSec()
                });
            }
            songsDataGrid.ItemsSource = list;
        }

        void Shuffle()
        {
            Random random = new Random();
            count = 0;
            shuffleMusic = music.OrderBy(x => random.Next()).ToArray();
            media.Source = new Uri(shuffleMusic[0]);
            FillDataGrid(shuffleMusic);
        }

        void UnShuffle()
        {
            count = 0;
            media.Source = new Uri(music[0]);
            FillDataGrid(music);
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

    public static void SetState(ref StatesOfPlayer s, ref MediaElement el, ref System.Timers.Timer t, ref Image PlayPause)
    {
        
        //MainWindow window = new MainWindow();
        switch (s)
        {
            case StatesOfPlayer.Play:
                el.Play();
                t.Start();
                PlayPause.Source = (BitmapImage)MainWindow.resDictionary["Pause"];
                break;
            case StatesOfPlayer.Close: el.Close(); break;
            case StatesOfPlayer.Pause: 
                el.Pause();
                t.Stop();
                PlayPause.Source = (BitmapImage)MainWindow.resDictionary["Play"];
                break;
            case StatesOfPlayer.Stop:
                el.Stop();
                t.Stop();
                PlayPause.Source = (BitmapImage)MainWindow.resDictionary["Play"];
                break;
        }
    }
}
