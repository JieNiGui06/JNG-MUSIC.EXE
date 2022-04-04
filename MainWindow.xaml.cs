using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
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

using Newtonsoft.Json;
using System.Windows.Threading;
using MaterialDesignThemes.Wpf;

using Microsoft.WindowsAPICodePack.Taskbar;
using System.Windows.Interop;

namespace JNG音乐
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {


        public NAudio.Wave.AudioFileReader myplayfile;
        public NAudio.Wave.WaveOut myplayer = new NAudio.Wave.WaveOut();
        public DispatcherTimer timer = new DispatcherTimer();
        public List<ListBoxItem> songlists;
        public List<ListBoxItem> playedsongs = new List<ListBoxItem>();

        public bool reprocessing = false;
        public MainWindow()
        {
            InitializeComponent();

            this.MaxHeight = SystemParameters.PrimaryScreenHeight;
            timer.Tick += Timer_Tick;
            timer.Interval = new TimeSpan(0, 0, 0, 0, 20);
            timer.IsEnabled = true;
            //System.Drawing.Icon icon = new System.Drawing.Icon("F:\\QQ个人空间\\JNG音乐\\Icon\\a_next1.png");
            //ThumbnailToolBarButton btnPlayPause = new ThumbnailToolBarButton(icon, "Play");
            //btnPlayPause.Enabled = true;

            //下一首按钮
            //ThumbnailToolBarButton btnNext = new ThumbnailToolBarButton(icon, "Next");
            //btnNext.Enabled = true;

            //上一首按钮
            //ThumbnailToolBarButton btnPre = new ThumbnailToolBarButton(icon, "Previous");
            //btnNext.Enabled = true;

            //添加按钮
            //TaskbarManager.Instance.ThumbnailToolBars.AddButtons((new WindowInteropHelper(this)).Handle, btnPre, btnPlayPause, btnNext);

            //裁剪略缩图，后面提到
            //TaskbarManager.Instance.TabbedThumbnail.SetThumbnailClip((new WindowInteropHelper(this)).Handle, new System.Drawing.Rectangle((int)abimage.Margin.Left, (int)abimage.Margin.Top, 20,70));
            //TaskbarManager.Instance.TabbedThumbnail.SetThumbnailClip((new WindowInteropHelper(this)).Handle, new System.Drawing.Rectangle((int)abimage.Margin.Left, (int)abimage.Margin.Top, (int)abimage.RenderSize.Width, (int)abimage .RenderSize.Height));
            taskBar.ThumbnailClipMargin = new Thickness(12, Height - 109, Width - 111, 10);

            string rdata = readfromconf();
            if (rdata == "")
            {

            }
            else
            {
                string[] splits = rdata.Split(new string[] { ".<split>" }, StringSplitOptions.RemoveEmptyEntries);
                if (splits.Length != 2)
                {

                }
                else
                {
                    string[] abs = splits[0].Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                    if (abs.Length != 0) {
                        foreach (string str in abs)
                        {
                            ListBoxItem ab = new ListBoxItem();
                            ab.Content = str;
                            ab.VerticalContentAlignment = VerticalAlignment.Center;
                            ab.HorizontalContentAlignment = HorizontalAlignment.Center;
                            ab.PreviewMouseDown += wdbd_click;
                            ab.PreviewMouseUp += checksel;
                            wdBD.Items.Add(ab);
                            string[] absongs = splits[1].Split(new string[] { ".<" + str + ">" }, StringSplitOptions.None);
                            if (absongs.Length != 3)
                            {

                            }
                            else
                            {
                                absongs = absongs[1].Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                                if (absongs.Length == 0)
                                {

                                }
                                else
                                {
                                    foreach (string s in absongs)
                                    {
                                        string[] infos = s.Split(new string[] { ".<,>." }, StringSplitOptions.None);
                                        addtoab(findabbycontent(str), crnewsong(infos[0], infos[1], infos[2], infos[3], infos[4]));

                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public ListBoxItem findabbycontent(string content)
        {
            foreach (ListBoxItem i in wdBD.Items)
            {
                if (i.Content.ToString() == content)
                {
                    return i;
                }
            }
            return new ListBoxItem();
        }

        public ListBoxItem crnewsong(string songname,string singer,string abname,string songurl,string picurl)
        {
            string[] songq = new string[2];

            string sq128 = songurl;

            WebClient wc = new WebClient();
            //MessageBox.Show(song.SongId.ToString());
            string sq320 = songurl;
            //songq[1] = tmpq;
            songq[0] = sq128;
            string abpicurl = picurl;
            //DataTable dataTable = JsonConvert.DeserializeObject<DataTable>(abpicurl);
            songq[1] = sq320;
            Grid songfather = new Grid();
            Label songnamelab = new Label(), songabname = new Label(), songartist = new Label(), songplayurl = new Label();

            songnamelab.Content = songname;
            songnamelab.HorizontalAlignment = HorizontalAlignment.Left;
            songnamelab.Margin = new Thickness(0, 0, 0, 0);
            songnamelab.Width = 100;

            songartist.Content = singer;
            songartist.HorizontalAlignment = HorizontalAlignment.Left;
            songartist.Margin = new Thickness(110, 0, 0, 0);
            songartist.Width = 100;
            songabname.Content = abname;
            songabname.HorizontalAlignment = HorizontalAlignment.Left;
            songabname.Margin = new Thickness(220, 0, 0, 0);
            songabname.Width = 100;
            songplayurl.Content = songq[0];
            songplayurl.HorizontalAlignment = HorizontalAlignment.Left;
            songplayurl.Margin = new Thickness(330, 0, 0, 0);
            //songplayurl.Width = ;

            songfather.Width = searchresultlist.Width;
            songfather.Children.Add(songnamelab);
            songfather.Children.Add(songartist);
            songfather.Children.Add(songabname);
            songfather.Children.Add(songplayurl);
            ListBoxItem lfather = new ListBoxItem();
            lfather.Content = songfather;
            //abimage.Source = new BitmapImage(new Uri(song.SongImageUrl));
            lfather.Tag = new object[] { songname, singer, abname, songq, abpicurl };
            ContextMenu menu = new ContextMenu();
            MenuItem mi = new MenuItem();
            mi.Header = "收藏";
            mi.MouseEnter += putsonginwdbd_MouseEnter;
            menu.Items.Add(mi);
            menu.Tag = lfather;
            lfather.ContextMenu = menu;
            lfather.MouseDoubleClick += playsong;
            return lfather;
        }

        double abpicang = 0;
        double lastpoint = 0;
        private void Timer_Tick(object sender, EventArgs e)
        {
            playpicbox.RenderTransform = new RotateTransform(abpicang);
            abpicang+=0.2;
            if ((playpicbox.RenderTransform as RotateTransform).Angle == 360)
            {
                abpicang = 0;
            }

            if (myplayfile != null)
            {
                if (!reprocessing)
                {
                    if (processofmusic.Value != lastpoint)
                    {

                    }
                    else
                    {
                        processofmusic.Value = 100 * (myplayfile.CurrentTime.TotalMilliseconds / myplayfile.TotalTime.TotalMilliseconds);
                        lastpoint = processofmusic.Value;
                        if (processofmusic.Value == 100)
                        {
                            reprocessing = false;
                            processofmusic.Value = 0;
                            lastpoint = 0;
                            nextsong();
                            return;
                        }
                    }
                    thistime.Content = myplayfile.CurrentTime.Minutes.ToString().PadLeft(2, '0') + ":" + myplayfile.CurrentTime.Seconds.ToString().PadLeft(2, '0');
                }
                //
            }
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            
        }

        private void windowclosebtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void windowmaxbtn_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState != WindowState.Maximized)
                WindowState = WindowState.Maximized;
            else
                WindowState = WindowState.Normal;
        }

        private void windowminbtn_Click(object sender, RoutedEventArgs e)
        {
            WindowState=WindowState.Minimized;
        }
        double lastwd;
        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (WindowState == WindowState.Maximized)
                {
                    Point lastp = e.GetPosition(this);
                    lastwd = e.GetPosition(this).X;
                    WindowState = WindowState.Normal;
                    Left = e.GetPosition(this).X - Width * lastwd / SystemParameters.PrimaryScreenWidth;
                    Top = 0;
                }
                DragMove();
            }
        }

        private void searchbtn_Click(object sender, RoutedEventArgs e)
        {
            searchresultlist.Items.Clear();
            runningsign.Visibility = Visibility.Visible;
            SearchSong(searchbox.Text);
        }

        public async void SearchSong(string songstr)
        {
            int agtimes = 0;
            onerr:
            try
            {
                Music.SDK.Provider.KuWoMusic kuWoMusic = new Music.SDK.Provider.KuWoMusic();
                List<Music.SDK.Models.SongItem> songs = await Task.Run(() => kuWoMusic.SearchSong(songstr));
                List<ListBoxItem> tmplist = new List<ListBoxItem>();
                foreach (Music.SDK.Models.SongItem song in songs)
                {
                    //string tmpq = "";
                    string[] songq = new string[2];
                    
                    string sq128 = await Task.Run(() => kuWoMusic.GetSongPlayUrl(song).Result);
                    
                    WebClient wc = new WebClient();
                    //MessageBox.Show(song.SongId.ToString());
                    string sq320 = await Task.Run(() => wc.DownloadString("http://antiserver.kuwo.cn/anti.s?type=convert_url&rid=" + song.SongId.ToString() + "&format=mp3&response=url"));
                    //songq[1] = tmpq;
                    songq[0] = sq128; 
                    string abpicurl = await Task.Run(() => wc.DownloadString("http://m.kuwo.cn/newh5/singles/songinfoandlrc?musicId=" + song.SongId.ToString() + "&httpsStatus=1&reqId=fcd6bc60-3e06-11ec-8722-67bb659a8433").Split(new string[] { "\"songinfo\"" }, StringSplitOptions.None)[1].Split(new string[] { "\"pic\"" }, StringSplitOptions.None)[1].Split('\"')[1]);
                    //DataTable dataTable = JsonConvert.DeserializeObject<DataTable>(abpicurl);
                    songq[1] = sq320;
                    Grid songfather = new Grid();
                    Label songname = new Label(), songabname = new Label(), songartist = new Label(), songplayurl = new Label();

                    songname.Content = song.SongName;
                    songname.HorizontalAlignment = HorizontalAlignment.Left;
                    songname.Margin = new Thickness(0, 0, 0, 0);
                    songname.Width = 100;

                    songartist.Content = string.Join("&", song.SongArtistName);
                    songartist.HorizontalAlignment = HorizontalAlignment.Left;
                    songartist.Margin = new Thickness(110, 0, 0, 0);
                    songartist.Width = 100;
                    songabname.Content = song.SongAlbumName.Replace(" ", "") == "" ? "未知专辑" : song.SongAlbumName;
                    songabname.HorizontalAlignment = HorizontalAlignment.Left;
                    songabname.Margin = new Thickness(220, 0, 0, 0);
                    songabname.Width = 100;
                    songplayurl.Content = songq[0];
                    songplayurl.HorizontalAlignment = HorizontalAlignment.Left;
                    songplayurl.Margin = new Thickness(330, 0, 0, 0);
                    //songplayurl.Width = ;

                    songfather.Width = searchresultlist.Width;
                    songfather.Children.Add(songname);
                    songfather.Children.Add(songartist);
                    songfather.Children.Add(songabname);
                    songfather.Children.Add(songplayurl);
                    ListBoxItem lfather = new ListBoxItem();
                    lfather.Content = songfather;
                    //abimage.Source = new BitmapImage(new Uri(song.SongImageUrl));
                    lfather.Tag = new object[] { song.SongName, song.SongArtistName, song.SongAlbumName, songq, abpicurl };
                    ContextMenu menu = new ContextMenu();
                    MenuItem mi = new MenuItem();
                    mi.Header = "收藏";
                    mi.MouseEnter += putsonginwdbd_MouseEnter;
                    menu.Items.Add(mi);
                    menu.Tag = lfather;
                    lfather.ContextMenu = menu;
                    lfather.MouseDoubleClick += playsong;
                    tmplist.Add(lfather);
                }
                runningsign.Visibility = Visibility.Hidden;
                foreach (ListBoxItem item in tmplist)
                {
                    searchresultlist.Items.Add(item);
                }
            }catch (Exception ex)
            {
                agtimes++;
                if (agtimes == 10)
                {
                    runningsign.Visibility = Visibility.Hidden;
                    return;
                }
                await Task.Run(() => Task.Delay(200));
                goto onerr;
            }
        }
        //http://m.kuwo.cn/newh5/singles/songinfoandlrc?musicId=324244&httpsStatus=1&reqId=fcd6bc60-3e06-11ec-8722-67bb659a8433
        private async void playsong(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ListBox senderlistbox = (ListBox)((ListBoxItem)sender).Parent;
                if (senderlistbox != songlist)
                {
                    songlist.Items.Clear();
                    foreach (ListBoxItem l in senderlistbox.Items)
                    {
                        ListBoxItem i = new ListBoxItem();
                        Grid g = l.Content as Grid;
                        Grid thisg = new Grid();
                        int thisleft = 0;
                        foreach (UIElement uie in g.Children)
                        {
                            if (uie is Label && thisleft != 340)
                            {
                                Label thisl = new Label();
                                thisl.FontSize = 12;
                                thisl.Content = (uie as Label).Content;
                                thisl.VerticalAlignment = VerticalAlignment.Top;
                                thisl.HorizontalAlignment = HorizontalAlignment.Left;
                                thisl.Width = 80;
                                thisl.Height = 30;
                                thisl.Margin = new Thickness(thisleft, 0, 0, 0);
                                thisleft += 85;
                                thisl.Padding = new Thickness(0);
                                thisg.Children.Add(thisl);
                            }
                        }
                        i.Content = thisg;
                        i.Tag = l.Tag;
                        i.Height = 30;
                        i.MouseDoubleClick += playsong;
                        songlist.Items.Add(i);
                    }
                    songlist.SelectedIndex = senderlistbox.SelectedIndex;
                }
                object[] myinfos = ((ListBoxItem)sender).Tag as object[];

                
                string[] selsq= myinfos[3] as string[];
                string lasts_ = "";
                if (songqs.Content.ToString() == "128")
                {
                    lasts_ = selsq[0].Replace("\\", "/");
                }
                else if (songqs.Content.ToString() == "320")
                {
                    lasts_ = selsq[1].Replace("\\", "/");
                }
                MessageBox.Show(string.Join("\n", selsq));
                string[] lasts__ = lasts_.Split('/');
                string last_ = lasts__[lasts__.Length - 1];
                if (last_.ToString().Split('.').Length != 2)
                {
                    MessageBox.Show("失败。");
                    return;
                }
                abimage.Source = new BitmapImage(new Uri(myinfos[4].ToString()));
                playpicbox.Fill = new ImageBrush(abimage.Source);
                string filename = myinfos[0].ToString() + "." + last_.Split('.')[1];
                reprocessing = true;

                myplayfile?.Close();
                await Task.Run(() => downloadmusic(lasts_.ToString(), filename));
                if (downloadstate != 1)
                {
                    return;
                }
                myplayfile = new NAudio.Wave.AudioFileReader(filename);
                if (File.Exists(filename) != true)
                    return;

                //myplayer = new NAudio.Wave.WaveOut();
                myplayfile.Volume = 1;
                myplayer.Init(myplayfile);
                processofmusic.Value = 0;
                lastpoint = 0;
                myplayer.Play();
                totaltime.Content = myplayfile.TotalTime.Minutes.ToString().PadLeft(2, '0') + ":" + myplayfile.TotalTime.Seconds.ToString().PadLeft(2, '0');
                (playbtn.Content as PackIcon).Kind = PackIconKind.Pause;
                reprocessing = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        int downloadstate = -1;
        public async void downloadmusic(string url,string name)
        {
            int i = 0;downloadstate = -1;
            err:
            try
            {
                WebClient webClient = new WebClient();
                webClient.DownloadFile(new Uri(url), name);
            }
            catch
            {
                if (i == 6)
                {
                    downloadstate = 0;
                    return;
                }
                await Task.Delay(200);
                i++;
                goto err;
            }
            downloadstate = 1;
        }

        private void playbtn_Click(object sender, RoutedEventArgs e)
        {
            if (myplayer.PlaybackState == NAudio.Wave.PlaybackState.Playing)
            {
                myplayer.Pause();
                (playbtn.Content as PackIcon).Kind = PackIconKind.Play;
            }
            else if (myplayer.PlaybackState == NAudio.Wave.PlaybackState.Paused)
            {
                myplayer.Play();
                (playbtn.Content as PackIcon).Kind = PackIconKind.Pause;
            }
        }

        private void playstylebtn_Click(object sender, RoutedEventArgs e)
        {
            if ((playstylebtn.Content as PackIcon).Kind == PackIconKind.RepeatVariant)
            {
                (playstylebtn.Content as PackIcon).Kind = PackIconKind.ShuffleVariant;
            }
            else if ((playstylebtn.Content as PackIcon).Kind == PackIconKind.ShuffleVariant)
            {
                (playstylebtn.Content as PackIcon).Kind = PackIconKind.RepeatOnce;
            }
            else
            {
                (playstylebtn.Content as PackIcon).Kind = PackIconKind.RepeatVariant;
            }
        }


        private void processofmusic_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (myplayfile != null)
            {
                myplayfile.CurrentTime = TimeSpan.FromMilliseconds(myplayfile.TotalTime.TotalMilliseconds * (processofmusic.Value / 100));
                lastpoint = processofmusic.Value;
                reprocessing = false;
            }
        }

        private void processofmusic_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            reprocessing = true;
        }

        private void songlistviewbtn_Click(object sender, RoutedEventArgs e)
        {
            if (songlist.Visibility == Visibility.Hidden)
                songlist.Visibility = Visibility.Visible;
            else
                songlist.Visibility = Visibility.Hidden;
        }

        private void processofmusic_ValueChanged()
        {
            if (processofmusic.Value == 100)
            {
                reprocessing = false;
                processofmusic.Value = 0;
                lastpoint = 0;
                nextsong();
            }
        }

        public void nextsong()
        {
            if (songlist != null && songlist.Items.Count != 0)
            {
                playedsongs.Add(songlist.SelectedItem as ListBoxItem);
                if ((playstylebtn.Content as PackIcon).Kind == PackIconKind.RepeatVariant)
                {
                    if (songlist.SelectedIndex == songlist.Items.Count - 1)
                    {
                        songlist.SelectedIndex = 0;
                    }
                    else
                    {
                        songlist.SelectedIndex += 1;
                    }
                }
                else if ((playstylebtn.Content as PackIcon).Kind == PackIconKind.ShuffleVariant)
                {
                    Random rnd = new Random();
                    songlist.SelectedIndex = rnd.Next(0, songlist.Items.Count);
                }
                else
                {

                }
                playsong(songlist.SelectedItem, null);
            }
        }


        //本地歌单
        private void NewFile_Click(object sender, RoutedEventArgs e)
        {
            ListBoxItem l = new ListBoxItem();
            dialoghost.Content = "add_bdgd";
            OpenDialog(dialoghost);
        }

        private void skipnext_Click(object sender, RoutedEventArgs e)
        {
            nextsong();
        }

        private void skiplast_Click(object sender, RoutedEventArgs e)
        {
            if (playedsongs.Count != 0)
            {
                playsong(playedsongs[playedsongs.Count - 1], null);
                playedsongs.RemoveAt(playedsongs.Count - 1);
            }
        }

        private void processofvulome_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            myplayer.Volume = (float)processofvolume.Value / 100;
        }

        public void writetoconf(string str)
        {
            using (StreamWriter sw = new StreamWriter("conf.txt"))
            {
                sw.Write(str);
            }
        }

        public string readfromconf()
        {
            if (!File.Exists("conf.txt"))
            {
                writetoconf("");
            }
            using (StreamReader sr = new StreamReader("conf.txt"))
            {
                return sr.ReadToEnd();
            }
        }

        private void dialogacceptbtn_Click(object sender, RoutedEventArgs e)
        {
            if (dialogtext.Text.Replace(" ", "") != "")
            {
                if (dialoghost.Content.ToString() == "add_bdgd")
                {
                    ListBoxItem l = new ListBoxItem();
                    l.VerticalContentAlignment = VerticalAlignment.Center;
                    l.HorizontalContentAlignment = HorizontalAlignment.Center;
                    l.Content = dialogtext.Text;
                    l.PreviewMouseDown += wdbd_click;
                    l.PreviewMouseUp += checksel;
                    writeabtoconf(l.Content.ToString());
                    wdBD.Items.Add(l);
                }
            }
            CloseDialog(dialoghost);
        }

        public void writeabtoconf(string abname)
        {
            string rall = readfromconf();
            if (rall.Trim() == "")
            {
                writetoconf(abname + "\n" + ".<split>\n" + ".<" + abname + ">\n" + ".<" + abname + ">\n");
            }
            else
            {
                string[] abs = rall.Split(new string[] { ".<split>" }, StringSplitOptions.None);
                abs[0] += "\n" + abname;
                writetoconf(string.Join(".<split>", abs));
            }
        }

        public void writesongtoconf(string abname,string[] infos)
        {
            string rall = readfromconf();
            string[] ab = rall.Split(new string[] { ".<" + abname + ">" }, StringSplitOptions.None);
            ab[1] += "\n" + string.Join(".<,>.", infos);
            writetoconf(string.Join(".<" + abname + ">", ab));
        }

        private void checksel(object sender, MouseButtonEventArgs e)
        {
            ListBoxItem l = sender as ListBoxItem;
            if (l.Tag != null && selwdbditem == l)
            {
                selwdbditem = null;
                List<ListBoxItem> list = l.Tag as List<ListBoxItem>;
                searchresultlist.Items.Clear();
                foreach (ListBoxItem item in list)
                {
                    searchresultlist.Items.Add(item);
                }
            }
        }
        ListBoxItem selwdbditem = null;
        private void wdbd_click(object sender, RoutedEventArgs e)
        {
            selwdbditem = sender as ListBoxItem;
        }

        public void OpenDialog(DialogHost hostdialog)
        {
            dialogtext.Text = "";
            (hostdialog.Parent as Grid).Visibility = Visibility.Visible;
            hostdialog.IsOpen = true;
        }
        public void CloseDialog(DialogHost hostdialog)
        {
            (hostdialog.Parent as Grid).Visibility = Visibility.Hidden;
            hostdialog.IsOpen = false;
        }

        private void dialogcancelbtn_Click(object sender, RoutedEventArgs e)
        {
            CloseDialog(dialoghost);
        }

        private void putsonginwdbd_MouseEnter(object sender, MouseEventArgs e)
        {
            MenuItem thism = sender as MenuItem;
            thism.Items.Clear();
            foreach (ListBoxItem l in wdBD.Items)
            {
                MenuItem m = new MenuItem();
                m.Header = l.Content.ToString();
                m.Tag = l;
                m.Click += addtowdbd_click;
                thism.Items.Add(m);
            }
        }

        
        private void addtowdbd_click(object sender, RoutedEventArgs e)
        {
            
            MenuItem m = sender as MenuItem;
            ListBoxItem l = m.Tag as ListBoxItem;
            ListBoxItem addtowdbdsong = (((m.Parent as MenuItem).Parent as ContextMenu).Tag as ListBoxItem);
            writesongtoconf(l.Content.ToString(), getsonginfos(addtowdbdsong));
            addtoab(l, addtowdbdsong);
        }

        public string[] getsonginfos(ListBoxItem song)
        {
            Grid g = song.Content as Grid;
            Grid thisg = new Grid();
            List<string> infos = new List<string>();
            object[] objs = song.Tag as object[];
            infos.Add(objs[0].ToString());
            infos.Add(string.Join("&", (objs[1] as List<string>)).ToString());
            infos.Add(objs[2].ToString());
            infos.Add((objs[3] as string[])[0]);
            infos.Add(objs[4].ToString());
            return infos.ToArray();
        }

        public void addtoab(ListBoxItem ab, ListBoxItem songobj)
        {
            if (ab.Tag == null)
            {
                ab.Tag = new List<ListBoxItem>();
            }
            List<ListBoxItem> list = ab.Tag as List<ListBoxItem>;
            //if (hasl)
            ListBoxItem newl = new ListBoxItem();
            Grid g = songobj.Content as Grid;
            int thisleft = 0;
            Grid thisg = new Grid();
            foreach (UIElement ui in g.Children)
            {
                if (ui is Label)
                {
                    Label thisl = new Label();
                    thisl.Content = (ui as Label).Content;
                    thisl.VerticalAlignment = VerticalAlignment.Top;
                    thisl.HorizontalAlignment = HorizontalAlignment.Left;
                    thisl.Width = 100;
                    thisl.Margin = new Thickness(thisleft, 0, 0, 0);
                    thisleft += 110;
                    thisl.Padding = new Thickness(0);
                    thisg.Children.Add(thisl);

                }
            }
            newl.Content = thisg;
            newl.Tag = songobj.Tag;
            newl.MouseDoubleClick += playsong;
            list.Add(newl);
            ab.Tag = list;
        }

        private void songqs_Click(object sender, RoutedEventArgs e)
        {
            if (songqs.Content.ToString () == "128")
            {
                songqs.Content = "320";
            }
            else if (songqs.Content.ToString() == "320")
            {
                songqs.Content = "128";
            }
        }

        private void skipnext_Copy1_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void likesong_Click(object sender, RoutedEventArgs e)
        {
            if (likesong.ToolTip.ToString() == "Like")
            {
                likesong.ToolTip = "Left";
                (likesong.Content as Image).Source = new BitmapImage(new Uri("Icon/b_like_1.png", UriKind.Relative));
            }
            else
            {
                likesong.ToolTip = "Like";
                (likesong.Content as Image).Source = new BitmapImage(new Uri("Icon/b_like_01.png", UriKind.Relative));
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
        }
    }
}
