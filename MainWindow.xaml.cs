using System;

using System.Collections.Generic;

using System.Linq;

using System.Text;

using System.ComponentModel;

using System.Windows;

//using System.Windows.Forms;

using System.Windows.Controls;

using System.Windows.Data;

using System.Data.SqlClient;

using System.Windows.Documents;

using System.Windows.Input;

using System.Windows.Media;

using System.Windows.Media.Animation;

using System.Windows.Media.Imaging;

using System.Windows.Navigation;

using System.Windows.Shapes;

using System.Drawing;

using System.Windows.Threading;

using System.Threading.Tasks;

using System.Data;

using System.Data.Common;

using System.Timers;

using Microsoft.VisualBasic;

using System.IO;

using System.Diagnostics;





namespace 大作业视频播放器

{

    /// <summary>

    /// MainWindow.xaml 的交互逻辑

    /// </summary>

    public partial class MainWindow : Window

    {



        //进度时间设置

        private TimeSpan duration;//timeSpan 时间间隔  duration 持续时间

        private DispatcherTimer timer = new DispatcherTimer();//计数器

        private DoubleAnimation c_daListAnimation;

        public bool c_bState = true;//记录菜单栏状态 false隐藏 true显示

        public string pathformark;

        public MainWindow()

        {

            InitializeComponent();

            Loaded += new RoutedEventHandler(Window_Loaded);

            //连接数据库，并将path导入dataTable

            DB_To_Lb();

            mediaElement.MediaOpened += new RoutedEventHandler(mediaElement_MediaOpened);

            mediaElement.MediaEnded += new RoutedEventHandler(mediaElement_MediaEnded);


        }

        //启动视频
        private void mediaElement_MediaOpened(object sender, RoutedEventArgs e)

        {
            //timelineSlider时间轴滑块的名字  TotalMilliseconds返回时间段总毫秒数 NaturalDuration 获得媒体持续时间
            timelineSlider.Maximum = mediaElement.NaturalDuration.TimeSpan.TotalMilliseconds;
            //？ 前面不为NULL时，执行后面语句，：三目运算符 >1左边 <1右边
            duration = mediaElement.NaturalDuration.HasTimeSpan ? mediaElement.NaturalDuration.TimeSpan : TimeSpan.FromMilliseconds(0);
            //显示获取的视频时间
            totalTime.Text += string.Format("{0}{1:00}:{2:00}:{3:00}", "总时长：", duration.Hours, duration.Minutes, duration.Seconds);

        }


        //停止视频

        private void mediaElement_MediaEnded(object sender, RoutedEventArgs e)

        {

            mediaElement.Stop();

            timelineSlider.Value = 0;

        }

        //窗口加载

        private void Window_Loaded(object sender, RoutedEventArgs e)

        {

            // 每 500 毫秒调用一次指定的方法

            timer.Interval = TimeSpan.FromMilliseconds(500);

            timer.Tick += new EventHandler(timer_Tick);

            timer.Start();



        }

        //使播放进度条显示时间；显示播放进度

        private void timer_Tick(object sender, EventArgs e)

        {



            timelineSlider.ToolTip = mediaElement.Position.ToString().Substring(0, 8);

            txtTime.Text = string.Format("{0}{1:00}:{2:00}:{3:00}", "播放进度：", mediaElement.Position.Hours,

                               mediaElement.Position.Minutes, mediaElement.Position.Seconds);

        }

        // 设置播放，暂停，停止，前进，后退按钮是否可用

        private void SetPlayer(bool bVal)

        {

            playBtn.IsEnabled = bVal;

            pauseBtn.IsEnabled = bVal;

            stopBtn.IsEnabled = bVal;

            backBtn.IsEnabled = bVal;

            forwardBtn.IsEnabled = bVal;

        }

        //选择视频文件对话框

        private void OpenBtn_Click(object sender, RoutedEventArgs e)

        {

            var openFileDialog = new Microsoft.Win32.OpenFileDialog()

            {

                //Filter = @"视频文件(*.avi格式)|*.avi|视频文件(*.wav格式)|*.wav|视频文件(*.wmv格式)|*.wmv|视频文件(*.mp4格式)|*.mp4|视频文件(*.mov格式)|*.mov|视频文件(*.mkv格式)|*.mkv"
                Filter = @"视频文件(*.avi;*.wav;*.wmv;*.mp4;*.mov;*.mkv)|*.avi;*.wav;*.wmv;*.mp4;*.mov;*.mkv"

            };

            var result = openFileDialog.ShowDialog();

            if (result == true)

            {
                string path;

                //获取视频的路径和名字
                path = openFileDialog.FileName;

                pathformark = openFileDialog.FileName;

                //添加进数据库
                string ConStr = "server=LAPTOP-IGQAGJFV;database=medialist;Trusted_Connection=SSPI";

                SqlConnection conn = new SqlConnection(ConStr.ToString());
                try
                {
                    if (conn.State == ConnectionState.Closed)
                        conn.Open();

                    string sqlstr_1 = "insert into mediaTable(path) values('" + path + "')";

                    string sqlstr_2 = "insert into mediaMark(path) values('" + path + "')";

                    SqlCommand sqlcmd_1 = new SqlCommand(sqlstr_1, conn);

                    SqlCommand sqlcmd_2 = new SqlCommand(sqlstr_2, conn);

                    sqlcmd_1.ExecuteNonQuery();

                    sqlcmd_2.ExecuteNonQuery();

                    DB_To_Lb();

                    MessageBox.Show("载入成功");

                }
                catch (Exception ex)
                {
                    MessageBox.Show("出现问题：" + ex.ToString());
                }
                finally
                {
                    conn.Close();
                }
                //UriKind.Relative 定义了相对的Uri类型
                mediaElement.Source = new Uri(openFileDialog.FileName, UriKind.Relative);
                SetPlayer(true);

                timelineSlider.Value = 0;

                mediaElement.Play();
            }

        }



        //播放视频

        private void playBtn_Click(object sender, RoutedEventArgs e)

        {
            SetPlayer(true);

            timelineSlider.Value = mediaElement.Position.TotalMilliseconds;

            mediaElement.Play();


        }

        //暂停播放视频

        private void pauseBtn_Click(object sender, RoutedEventArgs e)

        {

            mediaElement.Pause();

            string a = mediaElement.Position.ToString();
            //获取当前视频的时间
            string b = a.Substring(0, 8);
            //使用split中的char分割字符串
            string[] videotime = b.Split(':');

            int totime = int.Parse(videotime[0]) * 3600 + int.Parse(videotime[1]) * 60 + int.Parse(videotime[2]);

            textBox1.Text = Convert.ToString(totime);

            pauseBtn.IsEnabled = false;



        }



        //停止播放

        private void stopBtn_Click(object sender, RoutedEventArgs e)

        {

            mediaElement.Stop();

            SetPlayer(false);

            playBtn.IsEnabled = true;


        }

        //后退播放

        private void backBtn_Click(object sender, RoutedEventArgs e)

        {

            mediaElement.Position -= TimeSpan.FromSeconds(10);

            timelineSlider.Value = mediaElement.Position.TotalMilliseconds;

        }

        //快进播放

        private void forwardBtn_Click(object sender, RoutedEventArgs e)

        {

            mediaElement.Position += TimeSpan.FromSeconds(10);

            timelineSlider.Value = mediaElement.Position.TotalMilliseconds;

        }

        //使播放进度条跟随播放时间移动

        private void MediaTimeChanged(object sender, EventArgs e)

        {

            timelineSlider.Value = mediaElement.Position.TotalMilliseconds;

        }

        //跳转播放

        private void LocationBtn_Click(object sender, RoutedEventArgs e)

        {

            string Current_Position = textBox1.Text;

            if (Current_Position != "")

            {

                int current_time = int.Parse(Current_Position);

                int hour = current_time / 3600;

                int minutes = (current_time - (3600 * hour)) / 60;

                int second = current_time - (3600 * hour) - (minutes * 60);

                DateTime nows = DateTime.Now;

                int year = nows.Year;

                int month = nows.Month;

                int day = nows.Day;

                DateTime dt = new DateTime(year, month, day, hour, minutes, second);

                DateTime dt2 = new DateTime(year, month, day, 0, 0, 0);

                TimeSpan times = new TimeSpan((dt - dt2).Ticks);

                mediaElement.Position = times;

                timelineSlider.Value = mediaElement.Position.TotalMilliseconds;

                mediaElement.Play();

            }

            pauseBtn.IsEnabled = true;

        }

        //是否静音

        private void IsMutedBtn_Click(object sender, RoutedEventArgs e)

        {

            if (mediaElement.IsMuted == true)

            {

                IsMutedBtn.Content = "静音";

                mediaElement.IsMuted = false;

            }

            else

            {

                IsMutedBtn.Content = "有声";

                mediaElement.IsMuted = true;

            }

        }

        private void SreemShotBtn_Click(object sender, RoutedEventArgs e)
        {

            System.Windows.Size dpi = new System.Windows.Size((int)mediaElement.ActualWidth, (int)mediaElement.ActualHeight);
            RenderTargetBitmap bmp = new RenderTargetBitmap(5150, 3000,dpi.Width, dpi.Height + 250, PixelFormats.Pbgra32);
            bmp.Render(mediaElement);

            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bmp));

            string filename = Guid.NewGuid().ToString() + ".jpg";
            FileStream fs = new FileStream(filename, FileMode.Create);
            encoder.Save(fs);
            fs.Close();

            Process.Start(filename);

        }

        // 播放进度，跳转到播放的哪个地方

        void timelineSlider_ValueChanged_MouseUP(object sender, MouseEventArgs e)
        {

            int SliderValue = (int)timelineSlider.Value;

            TimeSpan ts = new TimeSpan(0, 0, 0, 0, SliderValue);

            mediaElement.Position = ts;

            timelineSlider.Value = mediaElement.Position.TotalMilliseconds;

            timelineSlider.ToolTip = mediaElement.Position.ToString().Substring(0, 8);

            mediaElement.Play();



        }

        #region 全屏相关
        //全屏

        private void mediaElement_MouseClick(object sender, RoutedEventArgs e)
        {
            if (FullScreenHelper.IsFullscreen(this))
            {
                FullScreenHelper.ExitFullscreen(this);

                txtTime.Visibility = Visibility.Visible;

                totalTime.Visibility = Visibility.Visible;

                aStackPanel.Visibility = Visibility.Visible;

                bStackPanel.Visibility = Visibility.Visible;

            }
            else
            {
                FullScreenHelper.GoFullscreen(this);

                txtTime.Visibility = Visibility.Collapsed;

                totalTime.Visibility = Visibility.Collapsed;

                aStackPanel.Visibility = Visibility.Collapsed;

                bStackPanel.Visibility = Visibility.Collapsed;

            }


        }
        //ESC 出全屏
        private void MediaElement_KeyDown_ESC(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (FullScreenHelper.IsFullscreen(this))
                {
                    FullScreenHelper.ExitFullscreen(this);

                    txtTime.Visibility = Visibility.Visible;

                    totalTime.Visibility = Visibility.Visible;

                    aStackPanel.Visibility = Visibility.Visible;

                    bStackPanel.Visibility = Visibility.Visible;
                }
            }
        }


        #endregion

        #region 播放列表

        //media双击全屏
        private void mediaElement_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var element = (FrameworkElement)sender;
            if (e.ClickCount == 1)
            {
                Timer timer = new Timer(500)
                {
                    AutoReset = false
                };
                timer.Elapsed += new ElapsedEventHandler((o, ex) => element.Dispatcher.Invoke(new Action(() =>
                                 {
                                     var timer2 = (System.Timers.Timer)element.Tag;
                                     timer2.Stop();
                                     timer2.Dispose();

                                 })));
                timer.Start();
                element.Tag = timer;
            }
            if (e.ClickCount > 1)
            {
                var timer = element.Tag as System.Timers.Timer;
                if (timer != null)
                {
                    timer.Stop();
                    timer.Dispose();
                    if (FullScreenHelper.IsFullscreen(this))
                    {
                        FullScreenHelper.ExitFullscreen(this);

                        txtTime.Visibility = Visibility.Visible;

                        totalTime.Visibility = Visibility.Visible;

                        aStackPanel.Visibility = Visibility.Visible;

                        bStackPanel.Visibility = Visibility.Visible;

                    }
                    else
                    {
                        FullScreenHelper.GoFullscreen(this);

                        txtTime.Visibility = Visibility.Collapsed;

                        totalTime.Visibility = Visibility.Collapsed;

                        aStackPanel.Visibility = Visibility.Collapsed;

                        bStackPanel.Visibility = Visibility.Collapsed;

                    }


                }
            }
        }

        //展开播放列表
        private void No_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            c_daListAnimation = new DoubleAnimation();

            if (c_bState)
            {

                c_bState = false;
                c_daListAnimation.From = 0;
                c_daListAnimation.To = -154;
                cd.Width = new GridLength(0);
            }
            else
            {
                c_bState = true;
                c_daListAnimation.From = -154;
                c_daListAnimation.To = 0;
                cd.Width = new GridLength(154);
            }

        }
        //列表item双击播放
        void listBox_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            // get index Clicked & DoubleClick BlankSpace return NoMatches
            int index = this.mediaList.SelectedIndex;

            pathformark = (string)((ListBoxItem)mediaList.SelectedItem).Content;

            if (index != System.Windows.Forms.ListBox.NoMatches)
            {

                string Filename = (string)((ListBoxItem)mediaList.SelectedItem).Content;

                mediaElement.Source = new Uri(Filename, UriKind.Relative);
                SetPlayer(true);
                timelineSlider.Value = 0;
                mediaElement.Play();


            }

            clear_Bm();

        }
        //列表右击
        private void mediaList_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            //object item = GetElementFromPoint((ItemsControl)sender, e.GetPosition((ItemsControl)sender));

            //if (item == null)
            //{
            //    mediaList.SelectedIndex = -1;
            //}
            int index = this.mediaList.SelectedIndex;
            if (index != System.Windows.Forms.ListBox.NoMatches)
            {
                Menu1.IsEnabled = true;
                Menu2.IsEnabled = true;
                Menu5.IsEnabled = true;
                Menu3.IsEnabled = false;
                Menu4.IsEnabled = false;
            }
            else
            {
                Menu1.IsEnabled = false;
                Menu2.IsEnabled = false;
                Menu5.IsEnabled = false;
                Menu3.IsEnabled = true;
                Menu4.IsEnabled = true;
            }

            clear_Bm();

            DB_TO_Bm();
        }
        //封装了数据库路径导出到listbox的方法
        private void DB_To_Lb()
        {

            DataTable Dt = new DataTable("Dt");

            Dt.Columns.Add(new DataColumn("path", typeof(string)));

            string ConStr = "server=LAPTOP-IGQAGJFV;database=medialist;Trusted_Connection=SSPI";

            SqlConnection conn = new SqlConnection(ConStr.ToString());
            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                string sqlstr = "SELECT path FROM mediaTable";

                SqlCommand sqlcmd = new SqlCommand(sqlstr, conn);

                SqlDataReader dataReader = null;

                dataReader = sqlcmd.ExecuteReader();

                while (dataReader.Read())
                {
                    DataRow dr = Dt.NewRow();
                    dr["path"] = dataReader["path"].ToString();
                    Dt.Rows.Add(dr);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("出现问题：" + ex.ToString());
            }
            finally
            {
                conn.Close();
            }

            //将datatable导入listbox

            mediaList.Items.Clear();
            foreach (DataRow dr in Dt.Rows)
            {
                ListBoxItem li = new ListBoxItem();
                li.Content = dr["path"].ToString();
                li.ToolTip = li.Content;
                mediaList.Items.Add(li);
            }

            Dt.Clear();

        }





        //右键播放
        private void Menu1_Click(object sender, RoutedEventArgs e)
        {
            int index = this.mediaList.SelectedIndex;

            pathformark = (string)((ListBoxItem)mediaList.SelectedItem).Content;

            if (index != System.Windows.Forms.ListBox.NoMatches)
            {

                string Filename = (string)((ListBoxItem)mediaList.SelectedItem).Content;

                mediaElement.Source = new Uri(Filename, UriKind.Relative);
                SetPlayer(true);
                mediaElement.Play();
            }
        }
        //右键删除
        private void Menu2_Click(object sender, RoutedEventArgs e)
        {
            string listcontent = (string)((ListBoxItem)mediaList.SelectedItem).Content;

            string ConStr = "server=LAPTOP-IGQAGJFV;database=medialist;Trusted_Connection=SSPI";

            SqlConnection conn = new SqlConnection(ConStr.ToString());
            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                string sqlstr_1 = "DELETE FROM mediaTable WHERE path='" + listcontent + "'";

                string sqlstr_2 = "DELETE FROM mediaMark WHERE path='" + listcontent + "'";

                SqlCommand sqlcmd_1 = new SqlCommand(sqlstr_1, conn);

                SqlCommand sqlcmd_2 = new SqlCommand(sqlstr_2, conn);

                sqlcmd_1.ExecuteNonQuery();

                sqlcmd_2.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                MessageBox.Show("出现问题：" + ex.ToString());
            }
            finally
            {
                conn.Close();
            }
            DB_To_Lb();

            MessageBox.Show("删除成功");
        }
        //右键打开
        private void Menu3_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog()

            {

                Filter = @"All Files|*.*|视频文件(*.avi格式)|*.avi|视频文件(*.wav格式)|*.wav|视频文件(*.wmv格式)|*.wmv|视频文件(*.mp4格式)|*.mp4|视频文件(*.mov格式)|*.mov"

            };

            var result = openFileDialog.ShowDialog();

            if (result == true)

            {
                string path;

                //获取视频的路径和名字
                path = openFileDialog.FileName;

                pathformark = openFileDialog.FileName;

                //添加进数据库
                string ConStr = "server=LAPTOP-IGQAGJFV;database=medialist;Trusted_Connection=SSPI";

                SqlConnection conn = new SqlConnection(ConStr.ToString());
                try
                {
                    if (conn.State == ConnectionState.Closed)
                        conn.Open();

                    string sqlstr = "insert into mediaTable(path) values('" + path + "')";

                    SqlCommand sqlcmd = new SqlCommand(sqlstr, conn);

                    sqlcmd.ExecuteNonQuery();

                    DB_To_Lb();

                    MessageBox.Show("载入成功");

                }
                catch (Exception ex)
                {
                    MessageBox.Show("出现问题：" + ex.ToString());
                }
                finally
                {
                    conn.Close();
                }
                //UriKind.Relative 定义了相对的Uri类型
                mediaElement.Source = new Uri(openFileDialog.FileName, UriKind.Relative);
                SetPlayer(true);
                timelineSlider.Value = 0;
                mediaElement.Play();
            }
        }
        //并不需要的右键排序
        private void Menu4_Click(object sender, RoutedEventArgs e)
        {

            string ConStr = "server=LAPTOP-IGQAGJFV;database=medialist;Trusted_Connection=SSPI";

            SqlConnection conn = new SqlConnection(ConStr.ToString());
            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                string sqlstr = "SELECT path FROM mediaTable ORDER BY path DESC";

                SqlCommand sqlcmd = new SqlCommand(sqlstr, conn);

                sqlcmd.ExecuteNonQuery();

                MessageBox.Show("排序完成");
            }
            catch (Exception ex)
            {
                MessageBox.Show("出现问题：" + ex.ToString());
            }
            finally
            {
                conn.Close();
            }
            DB_To_Lb();

        }
        //播放上一个
        private void lastscreen_Click(object sender, RoutedEventArgs e)
        {
            if (mediaList.SelectedIndex == -1)
            {
                return;
            }

            int index = this.mediaList.SelectedIndex - 1;
            if (index != System.Windows.Forms.ListBox.NoMatches)
            {
                mediaList.SelectedIndex--;

                string Filename = (string)((ListBoxItem)mediaList.SelectedItem).Content;

                pathformark = (string)((ListBoxItem)mediaList.SelectedItem).Content;

                mediaElement.Source = new Uri(Filename, UriKind.Relative);
                SetPlayer(true);
                mediaElement.Play();


            }
            else
            {
                mediaList.SelectedIndex = -1;
            }

        }
        //播放下一个
        private void nextscreen_Click(object sender, RoutedEventArgs e)
        {
            if (mediaList.SelectedIndex != mediaList.Items.Count -1)
            {
                mediaList.SelectedIndex++;
                string Filename = (string)((ListBoxItem)mediaList.SelectedItem).Content;

                pathformark = (string)((ListBoxItem)mediaList.SelectedItem).Content;

                mediaElement.Source = new Uri(Filename, UriKind.Relative);
                SetPlayer(true);
                mediaElement.Play();


            }
            else
            {
                mediaList.SelectedIndex = -1;
            }
        }

        //数据库 书签信息导出

        #endregion

        #region 书签
        private void clear_Bm()
        {
            string ConStr = "server=LAPTOP-IGQAGJFV;database=medialist;Trusted_Connection=SSPI";

            string pathname = pathformark;

            SqlConnection conn = new SqlConnection(ConStr.ToString());

            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                string sqlstr = "SELECT * FROM mediaMark WHERE path = '" + pathname + "'";


                SqlCommand sqlcmd = new SqlCommand(sqlstr, conn);

                sqlcmd.ExecuteNonQuery();

                SqlDataReader dataReader = null;


                dataReader = sqlcmd.ExecuteReader();


                while (dataReader.Read())
                {

                    MenuMark_1.ToolTip = null;

                    MenuMark_2.ToolTip = null;

                    MenuMark_3.ToolTip = null;

                    MenuMark_4.ToolTip = null;

                    MenuMark_5.ToolTip = null;

                    MenuMark_6.ToolTip = null;

                    MenuMark_7.ToolTip = null;

                    MenuMark_8.ToolTip = null;

                    MenuMark_9.ToolTip = null;

                    MenuMark_10.ToolTip = null;

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("出现问题：" + ex.ToString());
            }
            finally
            {
                conn.Close();
            }
        }
        private void DB_TO_Bm()
        {
            string ConStr = "server=LAPTOP-IGQAGJFV;database=medialist;Trusted_Connection=SSPI";

            string pathname = pathformark;

            SqlConnection conn = new SqlConnection(ConStr.ToString());

            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                string sqlstr = "SELECT * FROM mediaTable WHERE path = '" + pathname + "'";


                SqlCommand sqlcmd = new SqlCommand(sqlstr, conn);

                sqlcmd.ExecuteNonQuery();


                SqlDataReader dataReader = null;



                dataReader = sqlcmd.ExecuteReader();



                while (dataReader.Read())
                {

                    MenuMark_1.IsEnabled = false;

                    MenuMark_2.IsEnabled = false;

                    MenuMark_3.IsEnabled = false;

                    MenuMark_4.IsEnabled = false;

                    MenuMark_5.IsEnabled = false;

                    MenuMark_6.IsEnabled = false;

                    MenuMark_7.IsEnabled = false;

                    MenuMark_8.IsEnabled = false;

                    MenuMark_9.IsEnabled = false;

                    MenuMark_10.IsEnabled = false;

                    //MenuMark_1.Visibility = Visibility.Collapsed;

                    //MenuMark_2.Visibility = Visibility.Collapsed;

                    //MenuMark_3.Visibility = Visibility.Collapsed;

                    //MenuMark_4.Visibility = Visibility.Collapsed;

                    //MenuMark_5.Visibility = Visibility.Collapsed;

                    //MenuMark_6.Visibility = Visibility.Collapsed;

                    //MenuMark_7.Visibility = Visibility.Collapsed;

                    //MenuMark_8.Visibility = Visibility.Collapsed;

                    //MenuMark_9.Visibility = Visibility.Collapsed;

                    //MenuMark_10.Visibility = Visibility.Collapsed;


                    if (dataReader["bookmark_1"].ToString() != "")
                    {
                        MenuMark_1.IsEnabled = true;

                        //MenuMark_1.Visibility = Visibility.Visible;
                    }
                    if (dataReader["bookmark_2"].ToString() != "")
                    {
                        MenuMark_2.IsEnabled = true;

                        //MenuMark_2.Visibility = Visibility.Visible;
                    }
                    if (dataReader["bookmark_3"].ToString() != "")
                    {
                        MenuMark_3.IsEnabled = true;

                        //MenuMark_3.Visibility = Visibility.Visible;
                    }
                    if (dataReader["bookmark_4"].ToString() != "")
                    {
                        MenuMark_4.IsEnabled = true;

                        //MenuMark_4.Visibility = Visibility.Visible;
                    }
                    if (dataReader["bookmark_5"].ToString() != "")
                    {
                        MenuMark_5.IsEnabled = true;

                        //MenuMark_5.Visibility = Visibility.Visible;
                    }
                    if (dataReader["bookmark_6"].ToString() != "")
                    {
                        MenuMark_6.IsEnabled = true;

                        //MenuMark_6.Visibility = Visibility.Visible;
                    }
                    if (dataReader["bookmark_7"].ToString() != "")
                    {
                        MenuMark_7.IsEnabled = true;

                        //MenuMark_7.Visibility = Visibility.Visible;
                    }
                    if (dataReader["bookmark_8"].ToString() != "")
                    {
                        MenuMark_8.IsEnabled = true;

                        //MenuMark_8.Visibility = Visibility.Visible;
                    }
                    if (dataReader["bookmark_9"].ToString() != "")
                    {
                        MenuMark_9.IsEnabled = true;

                        //MenuMark_9.Visibility = Visibility.Visible;
                    }
                    if (dataReader["bookmark_10"].ToString() != "")
                    {
                        MenuMark_10.IsEnabled = true;

                        //MenuMark_10.Visibility = Visibility.Visible;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("出现问题：" + ex.ToString());
            }
            finally
            {
                conn.Close();
            }

            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                string sqlstr = "SELECT * FROM mediaMark WHERE path = '" + pathname + "'";


                SqlCommand sqlcmd = new SqlCommand(sqlstr, conn);

                sqlcmd.ExecuteNonQuery();


                SqlDataReader dataReader = null;



                dataReader = sqlcmd.ExecuteReader();



                while (dataReader.Read())
                {
                    if (dataReader["Mark_1"].ToString() != "")
                    {
                        MenuMark_1.ToolTip = dataReader["Mark_1"].ToString();
                    }

                    if (dataReader["Mark_2"].ToString() != "")
                    {
                        MenuMark_2.ToolTip = dataReader["Mark_2"].ToString();
                    }
                    if (dataReader["Mark_3"].ToString() != "")
                    {
                        MenuMark_3.ToolTip = dataReader["Mark_3"].ToString();
                    }
                    if (dataReader["Mark_4"].ToString() != "")
                    {
                        MenuMark_4.ToolTip = dataReader["Mark_4"].ToString();
                    }
                    if (dataReader["Mark_5"].ToString() != "")
                    {
                        MenuMark_5.ToolTip = dataReader["Mark_5"].ToString();
                    }
                    if (dataReader["Mark_6"].ToString() != "")
                    {
                        MenuMark_6.ToolTip = dataReader["Mark_6"].ToString();
                    }
                    if (dataReader["Mark_7"].ToString() != "")
                    {
                        MenuMark_7.ToolTip = dataReader["Mark_7"].ToString();
                    }
                    if (dataReader["Mark_8"].ToString() != "")
                    {
                        MenuMark_8.ToolTip = dataReader["Mark_8"].ToString();
                    }
                    if (dataReader["Mark_9"].ToString() != "")
                    {
                        MenuMark_9.ToolTip = dataReader["Mark_9"].ToString();
                    }
                    if (dataReader["Mark_10"].ToString() != "")
                    {
                        MenuMark_10.ToolTip = dataReader["Mark_10"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("出现问题：" + ex.ToString());
            }
            finally
            {
                conn.Close();
            }



        }

        //添加书签
        private void bookmarkBtn_Click(object sender, RoutedEventArgs e)
        {
            int num = 0;

            string time = mediaElement.Position.ToString();

            string ConStr = "server=LAPTOP-IGQAGJFV;database=medialist;Trusted_Connection=SSPI";

            SqlConnection conn = new SqlConnection(ConStr.ToString());
            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                //string pathname = (string)((ListBoxItem)mediaList.SelectedItem).Content;

                string pathname = pathformark;

                string sqlstr = "SELECT *  FROM mediaTable WHERE path = '" + pathname + " '";

                SqlCommand sqlcmd = new SqlCommand(sqlstr, conn);

                sqlcmd.ExecuteNonQuery();

                SqlDataReader dataReader = null;

                dataReader = sqlcmd.ExecuteReader();

                while (dataReader.Read())
                {
                    if (dataReader["bookmark_1"].ToString() == "")
                    {
                        num = 1;
                        break;
                    }
                    else if (dataReader["bookmark_2"].ToString() == "")
                    {
                        num = 2;
                        break;
                    }
                    else if (dataReader["bookmark_3"].ToString() == "")
                    {
                        num = 3;
                        break;
                    }
                    else if (dataReader["bookmark_4"].ToString() == "")
                    {
                        num = 4;
                        break;
                    }
                    else if (dataReader["bookmark_5"].ToString() == "")
                    {
                        num = 5;
                        break;
                    }
                    else if (dataReader["bookmark_6"].ToString() == "")
                    {
                        num = 6;
                        break;
                    }
                    else if (dataReader["bookmark_7"].ToString() == "")
                    {
                        num = 7;
                        break;
                    }
                    else if (dataReader["bookmark_8"].ToString() == "")
                    {
                        num = 8;
                        break;
                    }
                    else if (dataReader["bookmark_9"].ToString() == "")
                    {
                        num = 9;
                        break;
                    }
                    else if (dataReader["bookmark_10"].ToString() == "")
                    {
                        num = 10;
                        break;
                    }
                    else
                    {
                        MessageBox.Show("书签已满");
                    }
                    MessageBox.Show("添加失败");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("出现问题：" + ex.ToString());
            }
            finally
            {
                conn.Close();
            }


            string ConStr_2 = "server=LAPTOP-IGQAGJFV;database=medialist;Trusted_Connection=SSPI";

            SqlConnection conn_2 = new SqlConnection(ConStr_2.ToString());
            try
            {
                if (conn_2.State == ConnectionState.Closed)
                {
                    conn_2.Open();
                }

                //string pathname = (string)((ListBoxItem)mediaList.SelectedItem).Content;

                string pathname = pathformark;

                if (num == 1)
                {
                    string sqlstr = "UPDATE mediaTable SET bookmark_1 = '" + time + "' WHERE path = '" + pathname + " '";

                    SqlCommand sqlCommand = new SqlCommand(sqlstr, conn_2);

                    sqlCommand.ExecuteNonQuery();

                    MessageBox.Show("添加成功");
                }
                else if (num == 2)
                {
                    string sqlstr = "UPDATE mediaTable SET bookmark_2 = '" + time + "' WHERE path = '" + pathname + " '";

                    SqlCommand sqlCommand = new SqlCommand(sqlstr, conn_2);

                    sqlCommand.ExecuteNonQuery();

                    MessageBox.Show("添加成功");
                }
                else if (num == 3)
                {
                    string sqlstr = "UPDATE mediaTable SET bookmark_3 = '" + time + "' WHERE path = '" + pathname + " '";

                    SqlCommand sqlCommand = new SqlCommand(sqlstr, conn_2);

                    sqlCommand.ExecuteNonQuery();

                    MessageBox.Show("添加成功");
                }
                else if (num == 4)
                {
                    string sqlstr = "UPDATE mediaTable SET bookmark_4 = '" + time + "' WHERE path = '" + pathname + " '";

                    SqlCommand sqlCommand = new SqlCommand(sqlstr, conn_2);

                    sqlCommand.ExecuteNonQuery();

                    MessageBox.Show("添加成功");
                }
                else if (num == 5)
                {
                    string sqlstr = "UPDATE mediaTable SET bookmark_5 = '" + time + "' WHERE path = '" + pathname + " '";

                    SqlCommand sqlCommand = new SqlCommand(sqlstr, conn_2);

                    sqlCommand.ExecuteNonQuery();

                    MessageBox.Show("添加成功");
                }
                else if (num == 6)
                {
                    string sqlstr = "UPDATE mediaTable SET bookmark_6 = '" + time + "' WHERE path = '" + pathname + " '";

                    SqlCommand sqlCommand = new SqlCommand(sqlstr, conn_2);

                    sqlCommand.ExecuteNonQuery();

                    MessageBox.Show("添加成功");
                }
                else if (num == 7)
                {
                    string sqlstr = "UPDATE mediaTable SET bookmark_7 = '" + time + "' WHERE path = '" + pathname + " '";

                    SqlCommand sqlCommand = new SqlCommand(sqlstr, conn_2);

                    sqlCommand.ExecuteNonQuery();

                    MessageBox.Show("添加成功");
                }
                else if (num == 8)
                {
                    string sqlstr = "UPDATE mediaTable SET bookmark_8 = '" + time + "' WHERE path = '" + pathname + " '";

                    SqlCommand sqlCommand = new SqlCommand(sqlstr, conn_2);

                    sqlCommand.ExecuteNonQuery();

                    MessageBox.Show("添加成功");
                }
                else if (num == 9)
                {
                    string sqlstr = "UPDATE mediaTable SET bookmark_9 = '" + time + "' WHERE path = '" + pathname + " '";

                    SqlCommand sqlCommand = new SqlCommand(sqlstr, conn_2);

                    sqlCommand.ExecuteNonQuery();

                    MessageBox.Show("添加成功");
                }
                else if (num == 10)
                {
                    string sqlstr = "UPDATE mediaTable SET bookmark_10 = '" + time + "' WHERE path = '" + pathname + " '";

                    SqlCommand sqlCommand = new SqlCommand(sqlstr, conn_2);

                    sqlCommand.ExecuteNonQuery();

                    MessageBox.Show("添加成功");
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("出现问题：" + ex.ToString());
            }
            finally
            {
                conn_2.Close();
            }

        }

        #region 10个书签点击使用
        private void bookmark_Use_Click_1(object sender, RoutedEventArgs e)
        {

            string ConStr = "server=LAPTOP-IGQAGJFV;database=medialist;Trusted_Connection=SSPI";

            string pathname = pathformark;

            SqlConnection conn = new SqlConnection(ConStr.ToString());
            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                string sqlstr = "SELECT bookmark_1 FROM mediaTable WHERE path = '" + pathname + "'";

                SqlCommand sqlcmd = new SqlCommand(sqlstr, conn);

                SqlDataReader dataReader = null;

                dataReader = sqlcmd.ExecuteReader();

                while (dataReader.Read())
                {
                    string[] videotime = dataReader["bookmark_1"].ToString().Substring(0, 8).Split(':');

                    int totime = (int.Parse(videotime[0]) * 3600) + (int.Parse(videotime[1]) * 60) + int.Parse(videotime[2]);

                    TimeSpan ts = new TimeSpan(0, 0, 0, totime);

                    mediaElement.Position = ts;

                    mediaElement.Play();

                    timelineSlider.Value = mediaElement.Position.TotalMilliseconds;

                    timelineSlider.ToolTip = mediaElement.Position.ToString().Substring(0, 8);


                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("出现问题：" + ex.ToString());
            }
            finally
            {
                conn.Close();
            }
        }

        private void bookmark_Use_Click_2(object sender, RoutedEventArgs e)
        {

            string ConStr = "server=LAPTOP-IGQAGJFV;database=medialist;Trusted_Connection=SSPI";

            string pathname = pathformark;

            SqlConnection conn = new SqlConnection(ConStr.ToString());
            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                string sqlstr = "SELECT bookmark_2 FROM mediaTable WHERE path = '" + pathname + "'";

                SqlCommand sqlcmd = new SqlCommand(sqlstr, conn);

                SqlDataReader dataReader = null;

                dataReader = sqlcmd.ExecuteReader();

                while (dataReader.Read())
                {
                    string[] videotime = dataReader["bookmark_2"].ToString().Substring(0, 8).Split(':');

                    int totime = (int.Parse(videotime[0]) * 3600) + (int.Parse(videotime[1]) * 60) + int.Parse(videotime[2]);

                    TimeSpan ts = new TimeSpan(0, 0, 0, totime);

                    mediaElement.Position = ts;

                    mediaElement.Play();

                    timelineSlider.Value = mediaElement.Position.TotalMilliseconds;

                    timelineSlider.ToolTip = mediaElement.Position.ToString().Substring(0, 8);


                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("出现问题：" + ex.ToString());
            }
            finally
            {
                conn.Close();
            }
        }

        private void bookmark_Use_Click_3(object sender, RoutedEventArgs e)
        {

            string ConStr = "server=LAPTOP-IGQAGJFV;database=medialist;Trusted_Connection=SSPI";

            string pathname = pathformark;

            SqlConnection conn = new SqlConnection(ConStr.ToString());
            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                string sqlstr = "SELECT bookmark_3 FROM mediaTable WHERE path = '" + pathname + "'";

                SqlCommand sqlcmd = new SqlCommand(sqlstr, conn);

                SqlDataReader dataReader = null;

                dataReader = sqlcmd.ExecuteReader();

                while (dataReader.Read())
                {
                    string[] videotime = dataReader["bookmark_3"].ToString().Substring(0, 8).Split(':');

                    int totime = (int.Parse(videotime[0]) * 3600) + (int.Parse(videotime[1]) * 60) + int.Parse(videotime[2]);

                    TimeSpan ts = new TimeSpan(0, 0, 0, totime);

                    mediaElement.Position = ts;

                    mediaElement.Play();

                    timelineSlider.Value = mediaElement.Position.TotalMilliseconds;

                    timelineSlider.ToolTip = mediaElement.Position.ToString().Substring(0, 8);


                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("出现问题：" + ex.ToString());
            }
            finally
            {
                conn.Close();
            }
        }

        private void bookmark_Use_Click_4(object sender, RoutedEventArgs e)
        {

            string ConStr = "server=LAPTOP-IGQAGJFV;database=medialist;Trusted_Connection=SSPI";

            string pathname = pathformark;

            SqlConnection conn = new SqlConnection(ConStr.ToString());
            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                string sqlstr = "SELECT bookmark_4 FROM mediaTable WHERE path = '" + pathname + "'";

                SqlCommand sqlcmd = new SqlCommand(sqlstr, conn);

                SqlDataReader dataReader = null;

                dataReader = sqlcmd.ExecuteReader();

                while (dataReader.Read())
                {
                    string[] videotime = dataReader["bookmark_4"].ToString().Substring(0, 8).Split(':');

                    int totime = (int.Parse(videotime[0]) * 3600) + (int.Parse(videotime[1]) * 60) + int.Parse(videotime[2]);

                    TimeSpan ts = new TimeSpan(0, 0, 0, totime);

                    mediaElement.Position = ts;

                    mediaElement.Play();

                    timelineSlider.Value = mediaElement.Position.TotalMilliseconds;

                    timelineSlider.ToolTip = mediaElement.Position.ToString().Substring(0, 8);


                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("出现问题：" + ex.ToString());
            }
            finally
            {
                conn.Close();
            }
        }

        private void bookmark_Use_Click_5(object sender, RoutedEventArgs e)
        {

            string ConStr = "server=LAPTOP-IGQAGJFV;database=medialist;Trusted_Connection=SSPI";

            string pathname = pathformark;

            SqlConnection conn = new SqlConnection(ConStr.ToString());
            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                string sqlstr = "SELECT bookmark_5 FROM mediaTable WHERE path = '" + pathname + "'";

                SqlCommand sqlcmd = new SqlCommand(sqlstr, conn);

                SqlDataReader dataReader = null;

                dataReader = sqlcmd.ExecuteReader();

                while (dataReader.Read())
                {
                    string[] videotime = dataReader["bookmark_5"].ToString().Substring(0, 8).Split(':');

                    int totime = (int.Parse(videotime[0]) * 3600) + (int.Parse(videotime[1]) * 60) + int.Parse(videotime[2]);

                    TimeSpan ts = new TimeSpan(0, 0, 0, totime);

                    mediaElement.Position = ts;

                    mediaElement.Play();

                    timelineSlider.Value = mediaElement.Position.TotalMilliseconds;

                    timelineSlider.ToolTip = mediaElement.Position.ToString().Substring(0, 8);


                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("出现问题：" + ex.ToString());
            }
            finally
            {
                conn.Close();
            }
        }

        private void bookmark_Use_Click_6(object sender, RoutedEventArgs e)
        {

            string ConStr = "server=LAPTOP-IGQAGJFV;database=medialist;Trusted_Connection=SSPI";

            string pathname = pathformark;

            SqlConnection conn = new SqlConnection(ConStr.ToString());
            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                string sqlstr = "SELECT bookmark_6 FROM mediaTable WHERE path = '" + pathname + "'";

                SqlCommand sqlcmd = new SqlCommand(sqlstr, conn);

                SqlDataReader dataReader = null;

                dataReader = sqlcmd.ExecuteReader();

                while (dataReader.Read())
                {
                    string[] videotime = dataReader["bookmark_6"].ToString().Substring(0, 8).Split(':');

                    int totime = (int.Parse(videotime[0]) * 3600) + (int.Parse(videotime[1]) * 60) + int.Parse(videotime[2]);

                    TimeSpan ts = new TimeSpan(0, 0, 0, totime);

                    mediaElement.Position = ts;

                    mediaElement.Play();

                    timelineSlider.Value = mediaElement.Position.TotalMilliseconds;

                    timelineSlider.ToolTip = mediaElement.Position.ToString().Substring(0, 8);


                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("出现问题：" + ex.ToString());
            }
            finally
            {
                conn.Close();
            }
        }

        private void bookmark_Use_Click_7(object sender, RoutedEventArgs e)
        {

            string ConStr = "server=LAPTOP-IGQAGJFV;database=medialist;Trusted_Connection=SSPI";

            string pathname = pathformark;

            SqlConnection conn = new SqlConnection(ConStr.ToString());
            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                string sqlstr = "SELECT bookmark_7 FROM mediaTable WHERE path = '" + pathname + "'";

                SqlCommand sqlcmd = new SqlCommand(sqlstr, conn);

                SqlDataReader dataReader = null;

                dataReader = sqlcmd.ExecuteReader();

                while (dataReader.Read())
                {
                    string[] videotime = dataReader["bookmark_7"].ToString().Substring(0, 8).Split(':');

                    int totime = (int.Parse(videotime[0]) * 3600) + (int.Parse(videotime[1]) * 60) + int.Parse(videotime[2]);

                    TimeSpan ts = new TimeSpan(0, 0, 0, totime);

                    mediaElement.Position = ts;

                    mediaElement.Play();

                    timelineSlider.Value = mediaElement.Position.TotalMilliseconds;

                    timelineSlider.ToolTip = mediaElement.Position.ToString().Substring(0, 8);


                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("出现问题：" + ex.ToString());
            }
            finally
            {
                conn.Close();
            }
        }

        private void bookmark_Use_Click_8(object sender, RoutedEventArgs e)
        {

            string ConStr = "server=LAPTOP-IGQAGJFV;database=medialist;Trusted_Connection=SSPI";

            string pathname = pathformark;

            SqlConnection conn = new SqlConnection(ConStr.ToString());
            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                string sqlstr = "SELECT bookmark_8 FROM mediaTable WHERE path = '" + pathname + "'";

                SqlCommand sqlcmd = new SqlCommand(sqlstr, conn);

                SqlDataReader dataReader = null;

                dataReader = sqlcmd.ExecuteReader();

                while (dataReader.Read())
                {
                    string[] videotime = dataReader["bookmark_8"].ToString().Substring(0, 8).Split(':');

                    int totime = (int.Parse(videotime[0]) * 3600) + (int.Parse(videotime[1]) * 60) + int.Parse(videotime[2]);

                    TimeSpan ts = new TimeSpan(0, 0, 0, totime);

                    mediaElement.Position = ts;

                    mediaElement.Play();

                    timelineSlider.Value = mediaElement.Position.TotalMilliseconds;

                    timelineSlider.ToolTip = mediaElement.Position.ToString().Substring(0, 8);


                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("出现问题：" + ex.ToString());
            }
            finally
            {
                conn.Close();
            }
        }

        private void bookmark_Use_Click_9(object sender, RoutedEventArgs e)
        {

            string ConStr = "server=LAPTOP-IGQAGJFV;database=medialist;Trusted_Connection=SSPI";

            string pathname = pathformark;

            SqlConnection conn = new SqlConnection(ConStr.ToString());
            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                string sqlstr = "SELECT bookmark_9 FROM mediaTable WHERE path = '" + pathname + "'";

                SqlCommand sqlcmd = new SqlCommand(sqlstr, conn);

                SqlDataReader dataReader = null;

                dataReader = sqlcmd.ExecuteReader();

                while (dataReader.Read())
                {
                    string[] videotime = dataReader["bookmark_9"].ToString().Substring(0, 8).Split(':');

                    int totime = (int.Parse(videotime[0]) * 3600) + (int.Parse(videotime[1]) * 60) + int.Parse(videotime[2]);

                    TimeSpan ts = new TimeSpan(0, 0, 0, totime);

                    mediaElement.Position = ts;

                    mediaElement.Play();

                    timelineSlider.Value = mediaElement.Position.TotalMilliseconds;

                    timelineSlider.ToolTip = mediaElement.Position.ToString().Substring(0, 8);


                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("出现问题：" + ex.ToString());
            }
            finally
            {
                conn.Close();
            }
        }

        private void bookmark_Use_Click_10(object sender, RoutedEventArgs e)
        {

            string ConStr = "server=LAPTOP-IGQAGJFV;database=medialist;Trusted_Connection=SSPI";

            string pathname = pathformark;

            SqlConnection conn = new SqlConnection(ConStr.ToString());
            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                string sqlstr = "SELECT bookmark_10 FROM mediaTable WHERE path = '" + pathname + "'";

                SqlCommand sqlcmd = new SqlCommand(sqlstr, conn);

                SqlDataReader dataReader = null;

                dataReader = sqlcmd.ExecuteReader();

                while (dataReader.Read())
                {
                    string[] videotime = dataReader["bookmark_10"].ToString().Substring(0, 8).Split(':');

                    int totime = (int.Parse(videotime[0]) * 3600) + (int.Parse(videotime[1]) * 60) + int.Parse(videotime[2]);

                    TimeSpan ts = new TimeSpan(0, 0, 0, totime);

                    mediaElement.Position = ts;

                    mediaElement.Play();

                    timelineSlider.Value = mediaElement.Position.TotalMilliseconds;

                    timelineSlider.ToolTip = mediaElement.Position.ToString().Substring(0, 8);


                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("出现问题：" + ex.ToString());
            }
            finally
            {
                conn.Close();
            }
        }
        #endregion

        #region 10个书签删除

        private void bookmark_Delete_Click_1(object sender, RoutedEventArgs e)
        {
            string ConStr = "server=LAPTOP-IGQAGJFV;database=medialist;Trusted_Connection=SSPI";

            string pathname = pathformark;

            SqlConnection conn = new SqlConnection(ConStr.ToString());
            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                string sqlstr_1 = "UPDATE mediaTable SET bookmark_1 = '' WHERE path = '" + pathname + "'";

                string sqlstr_2 = "UPDATE mediaMark SET mark_1 = '' WHERE path = '" + pathname + "'";

                SqlCommand sqlcmd_1 = new SqlCommand(sqlstr_1, conn);

                SqlCommand sqlcmd_2 = new SqlCommand(sqlstr_2, conn);

                sqlcmd_1.ExecuteNonQuery();

                sqlcmd_2.ExecuteNonQuery();

                MessageBox.Show("清除成功");


            }
            catch (Exception ex)
            {
                MessageBox.Show("出现问题：" + ex.ToString());
            }
            finally
            {
                conn.Close();
            }
        }

        private void bookmark_Delete_Click_2(object sender, RoutedEventArgs e)
        {
            string ConStr = "server=LAPTOP-IGQAGJFV;database=medialist;Trusted_Connection=SSPI";

            string pathname = pathformark;

            SqlConnection conn = new SqlConnection(ConStr.ToString());
            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                string sqlstr_1 = "UPDATE mediaTable SET bookmark_2 = '' WHERE path = '" + pathname + "'";

                string sqlstr_2 = "UPDATE mediaMark SET mark_2 = '' WHERE path = '" + pathname + "'";

                SqlCommand sqlcmd_1 = new SqlCommand(sqlstr_1, conn);

                SqlCommand sqlcmd_2 = new SqlCommand(sqlstr_2, conn);

                sqlcmd_1.ExecuteNonQuery();

                sqlcmd_2.ExecuteNonQuery();

                MessageBox.Show("清除成功");


            }
            catch (Exception ex)
            {
                MessageBox.Show("出现问题：" + ex.ToString());
            }
            finally
            {
                conn.Close();
            }
        }

        private void bookmark_Delete_Click_3(object sender, RoutedEventArgs e)
        {
            string ConStr = "server=LAPTOP-IGQAGJFV;database=medialist;Trusted_Connection=SSPI";

            string pathname = pathformark;

            SqlConnection conn = new SqlConnection(ConStr.ToString());
            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                string sqlstr_1 = "UPDATE mediaTable SET bookmark_3 = '' WHERE path = '" + pathname + "'";

                string sqlstr_2 = "UPDATE mediaMark SET mark_3 = '' WHERE path = '" + pathname + "'";

                SqlCommand sqlcmd_1 = new SqlCommand(sqlstr_1, conn);

                SqlCommand sqlcmd_2 = new SqlCommand(sqlstr_2, conn);

                sqlcmd_1.ExecuteNonQuery();

                sqlcmd_2.ExecuteNonQuery();

                MessageBox.Show("清除成功");


            }
            catch (Exception ex)
            {
                MessageBox.Show("出现问题：" + ex.ToString());
            }
            finally
            {
                conn.Close();
            }
        }

        private void bookmark_Delete_Click_4(object sender, RoutedEventArgs e)
        {
            string ConStr = "server=LAPTOP-IGQAGJFV;database=medialist;Trusted_Connection=SSPI";

            string pathname = pathformark;

            SqlConnection conn = new SqlConnection(ConStr.ToString());
            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                string sqlstr_1 = "UPDATE mediaTable SET bookmark_4 = '' WHERE path = '" + pathname + "'";

                string sqlstr_2 = "UPDATE mediaMark SET mark_4 = '' WHERE path = '" + pathname + "'";

                SqlCommand sqlcmd_1 = new SqlCommand(sqlstr_1, conn);

                SqlCommand sqlcmd_2 = new SqlCommand(sqlstr_2, conn);

                sqlcmd_1.ExecuteNonQuery();

                sqlcmd_2.ExecuteNonQuery();

                MessageBox.Show("清除成功");


            }
            catch (Exception ex)
            {
                MessageBox.Show("出现问题：" + ex.ToString());
            }
            finally
            {
                conn.Close();
            }
        }

        private void bookmark_Delete_Click_5(object sender, RoutedEventArgs e)
        {
            string ConStr = "server=LAPTOP-IGQAGJFV;database=medialist;Trusted_Connection=SSPI";

            string pathname = pathformark;

            SqlConnection conn = new SqlConnection(ConStr.ToString());
            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                string sqlstr_1 = "UPDATE mediaTable SET bookmark_5 = '' WHERE path = '" + pathname + "'";

                string sqlstr_2 = "UPDATE mediaMark SET mark_5 = '' WHERE path = '" + pathname + "'";

                SqlCommand sqlcmd_1 = new SqlCommand(sqlstr_1, conn);

                SqlCommand sqlcmd_2 = new SqlCommand(sqlstr_2, conn);

                sqlcmd_1.ExecuteNonQuery();

                sqlcmd_2.ExecuteNonQuery();

                MessageBox.Show("清除成功");


            }
            catch (Exception ex)
            {
                MessageBox.Show("出现问题：" + ex.ToString());
            }
            finally
            {
                conn.Close();
            }
        }

        private void bookmark_Delete_Click_6(object sender, RoutedEventArgs e)
        {
            string ConStr = "server=LAPTOP-IGQAGJFV;database=medialist;Trusted_Connection=SSPI";

            string pathname = pathformark;

            SqlConnection conn = new SqlConnection(ConStr.ToString());
            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                string sqlstr_1 = "UPDATE mediaTable SET bookmark_6 = '' WHERE path = '" + pathname + "'";

                string sqlstr_2 = "UPDATE mediaMark SET mark_6 = '' WHERE path = '" + pathname + "'";

                SqlCommand sqlcmd_1 = new SqlCommand(sqlstr_1, conn);

                SqlCommand sqlcmd_2 = new SqlCommand(sqlstr_2, conn);

                sqlcmd_1.ExecuteNonQuery();

                sqlcmd_2.ExecuteNonQuery();

                MessageBox.Show("清除成功");


            }
            catch (Exception ex)
            {
                MessageBox.Show("出现问题：" + ex.ToString());
            }
            finally
            {
                conn.Close();
            }
        }

        private void bookmark_Delete_Click_7(object sender, RoutedEventArgs e)
        {
            string ConStr = "server=LAPTOP-IGQAGJFV;database=medialist;Trusted_Connection=SSPI";

            string pathname = pathformark;

            SqlConnection conn = new SqlConnection(ConStr.ToString());
            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                string sqlstr_1 = "UPDATE mediaTable SET bookmark_7 = '' WHERE path = '" + pathname + "'";

                string sqlstr_2 = "UPDATE mediaMark SET mark_7 = '' WHERE path = '" + pathname + "'";

                SqlCommand sqlcmd_1 = new SqlCommand(sqlstr_1, conn);

                SqlCommand sqlcmd_2 = new SqlCommand(sqlstr_2, conn);

                sqlcmd_1.ExecuteNonQuery();

                sqlcmd_2.ExecuteNonQuery();

                MessageBox.Show("清除成功");


            }
            catch (Exception ex)
            {
                MessageBox.Show("出现问题：" + ex.ToString());
            }
            finally
            {
                conn.Close();
            }
        }

        private void bookmark_Delete_Click_8(object sender, RoutedEventArgs e)
        {
            string ConStr = "server=LAPTOP-IGQAGJFV;database=medialist;Trusted_Connection=SSPI";

            string pathname = pathformark;

            SqlConnection conn = new SqlConnection(ConStr.ToString());
            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                string sqlstr_1 = "UPDATE mediaTable SET bookmark_8 = '' WHERE path = '" + pathname + "'";

                string sqlstr_2 = "UPDATE mediaMark SET mark_8 = '' WHERE path = '" + pathname + "'";

                SqlCommand sqlcmd_1 = new SqlCommand(sqlstr_1, conn);

                SqlCommand sqlcmd_2 = new SqlCommand(sqlstr_2, conn);

                sqlcmd_1.ExecuteNonQuery();

                sqlcmd_2.ExecuteNonQuery();

                MessageBox.Show("清除成功");


            }
            catch (Exception ex)
            {
                MessageBox.Show("出现问题：" + ex.ToString());
            }
            finally
            {
                conn.Close();
            }
        }

        private void bookmark_Delete_Click_9(object sender, RoutedEventArgs e)
        {
            string ConStr = "server=LAPTOP-IGQAGJFV;database=medialist;Trusted_Connection=SSPI";

            string pathname = pathformark;

            SqlConnection conn = new SqlConnection(ConStr.ToString());
            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                string sqlstr_1 = "UPDATE mediaTable SET bookmark_9 = '' WHERE path = '" + pathname + "'";

                string sqlstr_2 = "UPDATE mediaMark SET mark_9 = '' WHERE path = '" + pathname + "'";

                SqlCommand sqlcmd_1 = new SqlCommand(sqlstr_1, conn);

                SqlCommand sqlcmd_2 = new SqlCommand(sqlstr_2, conn);

                sqlcmd_1.ExecuteNonQuery();

                sqlcmd_2.ExecuteNonQuery();

                MessageBox.Show("清除成功");


            }
            catch (Exception ex)
            {
                MessageBox.Show("出现问题：" + ex.ToString());
            }
            finally
            {
                conn.Close();
            }
        }

        private void bookmark_Delete_Click_10(object sender, RoutedEventArgs e)
        {
            string ConStr = "server=LAPTOP-IGQAGJFV;database=medialist;Trusted_Connection=SSPI";

            string pathname = pathformark;

            SqlConnection conn = new SqlConnection(ConStr.ToString());
            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                string sqlstr_1 = "UPDATE mediaTable SET bookmark_10 = '' WHERE path = '" + pathname + "'";

                string sqlstr_2 = "UPDATE mediaMark SET mark_10 = '' WHERE path = '" + pathname + "'";

                SqlCommand sqlcmd_1 = new SqlCommand(sqlstr_1, conn);

                SqlCommand sqlcmd_2 = new SqlCommand(sqlstr_2, conn);

                sqlcmd_1.ExecuteNonQuery();

                sqlcmd_2.ExecuteNonQuery();

                MessageBox.Show("清除成功");


            }
            catch (Exception ex)
            {
                MessageBox.Show("出现问题：" + ex.ToString());
            }
            finally
            {
                conn.Close();
            }
        }

        #endregion

        #region 书签添加备注

        private void bookmark_Anno_Click_1(object sender, RoutedEventArgs e)
        {
            string str = Interaction.InputBox("请输入注释", "书签1 注释", "", -1, -1);

            string markcontent = str;


            string ConStr = "server=LAPTOP-IGQAGJFV;database=medialist;Trusted_Connection=SSPI";

            string pathname = pathformark;

            SqlConnection conn = new SqlConnection(ConStr.ToString());

            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                string sqlstr = "UPDATE mediaMark set mark_1 = '" + markcontent + "' WHERE path = '" + pathname + "' ";

                SqlCommand sqlcmd = new SqlCommand(sqlstr, conn);

                sqlcmd.ExecuteNonQuery();


                MessageBox.Show("添加注释成功");


            }
            catch (Exception ex)
            {
                MessageBox.Show("出现问题：" + ex.ToString());
            }
            finally
            {
                conn.Close();
            }

            clear_Bm();

            DB_TO_Bm();


        }

        private void bookmark_Anno_Click_2(object sender, RoutedEventArgs e)
        {
            string str = Interaction.InputBox("请输入注释", "书签2 注释", "", -1, -1);

            string markcontent = str;

            string ConStr = "server=LAPTOP-IGQAGJFV;database=medialist;Trusted_Connection=SSPI";

            string pathname = pathformark;

            SqlConnection conn = new SqlConnection(ConStr.ToString());

            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                string sqlstr = "UPDATE mediaMark set mark_2 = '" + markcontent + "' WHERE path = '" + pathname + "' ";

                SqlCommand sqlcmd = new SqlCommand(sqlstr, conn);

                sqlcmd.ExecuteNonQuery();


                MessageBox.Show("添加注释成功");


            }
            catch (Exception ex)
            {
                MessageBox.Show("出现问题：" + ex.ToString());
            }
            finally
            {
                conn.Close();
            }

            clear_Bm();

            DB_TO_Bm();


        }

        private void bookmark_Anno_Click_3(object sender, RoutedEventArgs e)
        {
            string str = Interaction.InputBox("请输入注释", "书签3 注释", "", -1, -1);

            string markcontent = str;

            string ConStr = "server=LAPTOP-IGQAGJFV;database=medialist;Trusted_Connection=SSPI";

            string pathname = pathformark;

            SqlConnection conn = new SqlConnection(ConStr.ToString());

            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                string sqlstr = "UPDATE mediaMark set mark_3 = '" + markcontent + "' WHERE path = '" + pathname + "' ";

                SqlCommand sqlcmd = new SqlCommand(sqlstr, conn);

                sqlcmd.ExecuteNonQuery();


                MessageBox.Show("添加注释成功");


            }
            catch (Exception ex)
            {
                MessageBox.Show("出现问题：" + ex.ToString());
            }
            finally
            {
                conn.Close();
            }

            clear_Bm();

            DB_TO_Bm();


        }

        private void bookmark_Anno_Click_4(object sender, RoutedEventArgs e)
        {
            string str = Interaction.InputBox("请输入注释", "书签4 注释", "", -1, -1);

            string markcontent = str;

            string ConStr = "server=LAPTOP-IGQAGJFV;database=medialist;Trusted_Connection=SSPI";

            string pathname = pathformark;

            SqlConnection conn = new SqlConnection(ConStr.ToString());

            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                string sqlstr = "UPDATE mediaMark set mark_4 = '" + markcontent + "' WHERE path = '" + pathname + "' ";

                SqlCommand sqlcmd = new SqlCommand(sqlstr, conn);

                sqlcmd.ExecuteNonQuery();


                MessageBox.Show("添加注释成功");


            }
            catch (Exception ex)
            {
                MessageBox.Show("出现问题：" + ex.ToString());
            }
            finally
            {
                conn.Close();
            }

            clear_Bm();

            DB_TO_Bm();


        }

        private void bookmark_Anno_Click_5(object sender, RoutedEventArgs e)
        {
            string str = Interaction.InputBox("请输入注释", "书签5 注释", "", -1, -1);

            string markcontent = str;

            string ConStr = "server=LAPTOP-IGQAGJFV;database=medialist;Trusted_Connection=SSPI";

            string pathname = pathformark;

            SqlConnection conn = new SqlConnection(ConStr.ToString());

            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                string sqlstr = "UPDATE mediaMark set mark_5 = '" + markcontent + "' WHERE path = '" + pathname + "' ";

                SqlCommand sqlcmd = new SqlCommand(sqlstr, conn);

                sqlcmd.ExecuteNonQuery();


                MessageBox.Show("添加注释成功");


            }
            catch (Exception ex)
            {
                MessageBox.Show("出现问题：" + ex.ToString());
            }
            finally
            {
                conn.Close();
            }

            clear_Bm();

            DB_TO_Bm();


        }

        private void bookmark_Anno_Click_6(object sender, RoutedEventArgs e)
        {
            string str = Interaction.InputBox("请输入注释", "书签6 注释", "", -1, -1);

            string markcontent = str;

            string ConStr = "server=LAPTOP-IGQAGJFV;database=medialist;Trusted_Connection=SSPI";

            string pathname = pathformark;

            SqlConnection conn = new SqlConnection(ConStr.ToString());

            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                string sqlstr = "UPDATE mediaMark set mark_6 = '" + markcontent + "' WHERE path = '" + pathname + "' ";

                SqlCommand sqlcmd = new SqlCommand(sqlstr, conn);

                sqlcmd.ExecuteNonQuery();


                MessageBox.Show("添加注释成功");


            }
            catch (Exception ex)
            {
                MessageBox.Show("出现问题：" + ex.ToString());
            }
            finally
            {
                conn.Close();
            }

            clear_Bm();

            DB_TO_Bm();


        }

        private void bookmark_Anno_Click_7(object sender, RoutedEventArgs e)
        {
            string str = Interaction.InputBox("请输入注释", "书签7 注释", "", -1, -1);

            string markcontent = str;

            string ConStr = "server=LAPTOP-IGQAGJFV;database=medialist;Trusted_Connection=SSPI";

            string pathname = pathformark;

            SqlConnection conn = new SqlConnection(ConStr.ToString());

            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                string sqlstr = "UPDATE mediaMark set mark_7 = '" + markcontent + "' WHERE path = '" + pathname + "' ";

                SqlCommand sqlcmd = new SqlCommand(sqlstr, conn);

                sqlcmd.ExecuteNonQuery();


                MessageBox.Show("添加注释成功");


            }
            catch (Exception ex)
            {
                MessageBox.Show("出现问题：" + ex.ToString());
            }
            finally
            {
                conn.Close();
            }

            clear_Bm();

            DB_TO_Bm();


        }

        private void bookmark_Anno_Click_8(object sender, RoutedEventArgs e)
        {
            string str = Interaction.InputBox("请输入注释", "书签8 注释", "", -1, -1);

            string markcontent = str;

            string ConStr = "server=LAPTOP-IGQAGJFV;database=medialist;Trusted_Connection=SSPI";

            string pathname = pathformark;

            SqlConnection conn = new SqlConnection(ConStr.ToString());

            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                string sqlstr = "UPDATE mediaMark set mark_8 = '" + markcontent + "' WHERE path = '" + pathname + "' ";

                SqlCommand sqlcmd = new SqlCommand(sqlstr, conn);

                sqlcmd.ExecuteNonQuery();


                MessageBox.Show("添加注释成功");


            }
            catch (Exception ex)
            {
                MessageBox.Show("出现问题：" + ex.ToString());
            }
            finally
            {
                conn.Close();
            }

            clear_Bm();

            DB_TO_Bm();


        }

        private void bookmark_Anno_Click_9(object sender, RoutedEventArgs e)
        {
            string str = Interaction.InputBox("请输入注释", "书签9 注释", "", -1, -1);

            string markcontent = str;

            string ConStr = "server=LAPTOP-IGQAGJFV;database=medialist;Trusted_Connection=SSPI";

            string pathname = pathformark;

            SqlConnection conn = new SqlConnection(ConStr.ToString());

            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                string sqlstr = "UPDATE mediaMark set mark_9 = '" + markcontent + "' WHERE path = '" + pathname + "' ";

                SqlCommand sqlcmd = new SqlCommand(sqlstr, conn);

                sqlcmd.ExecuteNonQuery();


                MessageBox.Show("添加注释成功");


            }
            catch (Exception ex)
            {
                MessageBox.Show("出现问题：" + ex.ToString());
            }
            finally
            {
                conn.Close();
            }

            clear_Bm();

            DB_TO_Bm();


        }

        private void bookmark_Anno_Click_10(object sender, RoutedEventArgs e)
        {
            string str = Interaction.InputBox("请输入注释", "书签10 注释", "", -1, -1);

            string markcontent = str;

            string ConStr = "server=LAPTOP-IGQAGJFV;database=medialist;Trusted_Connection=SSPI";

            string pathname = pathformark;

            SqlConnection conn = new SqlConnection(ConStr.ToString());

            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                string sqlstr = "UPDATE mediaMark set mark_10 = '" + markcontent + "' WHERE path = '" + pathname + "' ";

                SqlCommand sqlcmd = new SqlCommand(sqlstr, conn);

                sqlcmd.ExecuteNonQuery();


                MessageBox.Show("添加注释成功");


            }
            catch (Exception ex)
            {
                MessageBox.Show("出现问题：" + ex.ToString());
            }
            finally
            {
                conn.Close();
            }

            clear_Bm();

            DB_TO_Bm();


        }

        #endregion

        #endregion


        private void Mark_Clear__Click_1(object sender, RoutedEventArgs e)
        {
            string pathname = pathformark;

            string listcontent = (string)((ListBoxItem)mediaList.SelectedItem).Content;

            string ConStr = "server=LAPTOP-IGQAGJFV;database=medialist;Trusted_Connection=SSPI";

            SqlConnection conn = new SqlConnection(ConStr.ToString());
            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                string sqlstr_1 = "UPDATE mediaTable set bookmark_1 = ''  WHERE path = '" + pathname + " ' ";

                string sqlstr_2 = "UPDATE mediaMark set mark_1 = ''  WHERE path = '" + pathname + " ' ";


                SqlCommand sqlcmd_1 = new SqlCommand(sqlstr_1, conn);

                SqlCommand sqlcmd_2 = new SqlCommand(sqlstr_2, conn);


                sqlcmd_1.ExecuteNonQuery();

                sqlcmd_2.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                MessageBox.Show("出现问题：" + ex.ToString());
            }
            finally
            {
                conn.Close();
            }
            DB_To_Lb();

            MessageBox.Show("删除成功");
        }


        private void Mark_Clear__Click_2(object sender, RoutedEventArgs e)
        {
            string pathname = pathformark;

            string listcontent = (string)((ListBoxItem)mediaList.SelectedItem).Content;

            string ConStr = "server=LAPTOP-IGQAGJFV;database=medialist;Trusted_Connection=SSPI";

            SqlConnection conn = new SqlConnection(ConStr.ToString());
            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }
                string sqlstr_1 = "UPDATE mediaTable set bookmark_2 = ''  WHERE path = '" + pathname + " ' ";

                string sqlstr_2 = "UPDATE mediaMark set mark_2 = ''  WHERE path = '" + pathname + " ' ";


                SqlCommand sqlcmd_1 = new SqlCommand(sqlstr_1, conn);

                SqlCommand sqlcmd_2 = new SqlCommand(sqlstr_2, conn);


                sqlcmd_1.ExecuteNonQuery();

                sqlcmd_2.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                MessageBox.Show("出现问题：" + ex.ToString());
            }
            finally
            {
                conn.Close();
            }
            DB_To_Lb();

            MessageBox.Show("删除成功");
        }

        private void Mark_Clear__Click_3(object sender, RoutedEventArgs e)
        {
            string pathname = pathformark;

            string listcontent = (string)((ListBoxItem)mediaList.SelectedItem).Content;

            string ConStr = "server=LAPTOP-IGQAGJFV;database=medialist;Trusted_Connection=SSPI";

            SqlConnection conn = new SqlConnection(ConStr.ToString());
            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                string sqlstr_1 = "UPDATE mediaTable set bookmark_3 = ''  WHERE path = '" + pathname + " ' ";

                string sqlstr_2 = "UPDATE mediaMark set mark_3 = ''  WHERE path = '" + pathname + " ' ";


                SqlCommand sqlcmd_1 = new SqlCommand(sqlstr_1, conn);

                SqlCommand sqlcmd_2 = new SqlCommand(sqlstr_2, conn);


                sqlcmd_1.ExecuteNonQuery();

                sqlcmd_2.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show("出现问题：" + ex.ToString());
            }
            finally
            {
                conn.Close();
            }
            DB_To_Lb();

            MessageBox.Show("删除成功");
        }

        private void Mark_Clear__Click_4(object sender, RoutedEventArgs e)
        {
            string pathname = pathformark;

            string listcontent = (string)((ListBoxItem)mediaList.SelectedItem).Content;

            string ConStr = "server=LAPTOP-IGQAGJFV;database=medialist;Trusted_Connection=SSPI";

            SqlConnection conn = new SqlConnection(ConStr.ToString());
            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                string sqlstr_1 = "UPDATE mediaTable set bookmark_4 = ''  WHERE path = '" + pathname + " ' ";

                string sqlstr_2 = "UPDATE mediaMark set mark_4 = ''  WHERE path = '" + pathname + " ' ";


                SqlCommand sqlcmd_1 = new SqlCommand(sqlstr_1, conn);

                SqlCommand sqlcmd_2 = new SqlCommand(sqlstr_2, conn);


                sqlcmd_1.ExecuteNonQuery();

                sqlcmd_2.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show("出现问题：" + ex.ToString());
            }
            finally
            {
                conn.Close();
            }
            DB_To_Lb();

            MessageBox.Show("删除成功");
        }

        private void Mark_Clear__Click_5(object sender, RoutedEventArgs e)
        {
            string pathname = pathformark;

            string listcontent = (string)((ListBoxItem)mediaList.SelectedItem).Content;

            string ConStr = "server=LAPTOP-IGQAGJFV;database=medialist;Trusted_Connection=SSPI";

            SqlConnection conn = new SqlConnection(ConStr.ToString());
            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                string sqlstr_1 = "UPDATE mediaTable set bookmark_5 = ''  WHERE path = '" + pathname + " ' ";

                string sqlstr_2 = "UPDATE mediaMark set mark_5 = ''  WHERE path = '" + pathname + " ' ";


                SqlCommand sqlcmd_1 = new SqlCommand(sqlstr_1, conn);

                SqlCommand sqlcmd_2 = new SqlCommand(sqlstr_2, conn);


                sqlcmd_1.ExecuteNonQuery();

                sqlcmd_2.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                MessageBox.Show("出现问题：" + ex.ToString());
            }
            finally
            {
                conn.Close();
            }
            DB_To_Lb();

            MessageBox.Show("删除成功");
        }

        private void Mark_Clear__Click_6(object sender, RoutedEventArgs e)
        {
            string pathname = pathformark;

            string listcontent = (string)((ListBoxItem)mediaList.SelectedItem).Content;

            string ConStr = "server=LAPTOP-IGQAGJFV;database=medialist;Trusted_Connection=SSPI";

            SqlConnection conn = new SqlConnection(ConStr.ToString());
            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                string sqlstr_1 = "UPDATE mediaTable set bookmark_6 = ''  WHERE path = '" + pathname + " ' ";

                string sqlstr_2 = "UPDATE mediaMark set mark_6 = ''  WHERE path = '" + pathname + " ' ";


                SqlCommand sqlcmd_1 = new SqlCommand(sqlstr_1, conn);

                SqlCommand sqlcmd_2 = new SqlCommand(sqlstr_2, conn);


                sqlcmd_1.ExecuteNonQuery();

                sqlcmd_2.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                MessageBox.Show("出现问题：" + ex.ToString());
            }
            finally
            {
                conn.Close();
            }
            DB_To_Lb();

            MessageBox.Show("删除成功");
        }

        private void Mark_Clear__Click_7(object sender, RoutedEventArgs e)
        {
            string pathname = pathformark;

            string listcontent = (string)((ListBoxItem)mediaList.SelectedItem).Content;

            string ConStr = "server=LAPTOP-IGQAGJFV;database=medialist;Trusted_Connection=SSPI";

            SqlConnection conn = new SqlConnection(ConStr.ToString());
            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                string sqlstr_1 = "UPDATE mediaTable set bookmark_7 = ''  WHERE path = '" + pathname + " ' ";

                string sqlstr_2 = "UPDATE mediaMark set mark_7 = ''  WHERE path = '" + pathname + " ' ";


                SqlCommand sqlcmd_1 = new SqlCommand(sqlstr_1, conn);

                SqlCommand sqlcmd_2 = new SqlCommand(sqlstr_2, conn);


                sqlcmd_1.ExecuteNonQuery();

                sqlcmd_2.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                MessageBox.Show("出现问题：" + ex.ToString());
            }
            finally
            {
                conn.Close();
            }
            DB_To_Lb();

            MessageBox.Show("删除成功");
        }

        private void Mark_Clear__Click_8(object sender, RoutedEventArgs e)
        {
            string pathname = pathformark;

            string listcontent = (string)((ListBoxItem)mediaList.SelectedItem).Content;

            string ConStr = "server=LAPTOP-IGQAGJFV;database=medialist;Trusted_Connection=SSPI";

            SqlConnection conn = new SqlConnection(ConStr.ToString());
            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                string sqlstr_1 = "UPDATE mediaTable set bookmark_8 = ''  WHERE path = '" + pathname + " ' ";

                string sqlstr_2 = "UPDATE mediaMark set mark_8 = ''  WHERE path = '" + pathname + " ' ";


                SqlCommand sqlcmd_1 = new SqlCommand(sqlstr_1, conn);

                SqlCommand sqlcmd_2 = new SqlCommand(sqlstr_2, conn);


                sqlcmd_1.ExecuteNonQuery();

                sqlcmd_2.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                MessageBox.Show("出现问题：" + ex.ToString());
            }
            finally
            {
                conn.Close();
            }
            DB_To_Lb();

            MessageBox.Show("删除成功");
        }

        private void Mark_Clear__Click_9(object sender, RoutedEventArgs e)
        {
            string pathname = pathformark;

            string listcontent = (string)((ListBoxItem)mediaList.SelectedItem).Content;

            string ConStr = "server=LAPTOP-IGQAGJFV;database=medialist;Trusted_Connection=SSPI";

            SqlConnection conn = new SqlConnection(ConStr.ToString());
            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                string sqlstr_1 = "UPDATE mediaTable set bookmark_9 = ''  WHERE path = '" + pathname + " ' ";

                string sqlstr_2 = "UPDATE mediaMark set mark_9 = ''  WHERE path = '" + pathname + " ' ";


                SqlCommand sqlcmd_1 = new SqlCommand(sqlstr_1, conn);

                SqlCommand sqlcmd_2 = new SqlCommand(sqlstr_2, conn);


                sqlcmd_1.ExecuteNonQuery();

                sqlcmd_2.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                MessageBox.Show("出现问题：" + ex.ToString());
            }
            finally
            {
                conn.Close();
            }
            DB_To_Lb();

            MessageBox.Show("删除成功");
        }

        private void Mark_Clear__Click_10(object sender, RoutedEventArgs e)
        {
            string pathname = pathformark;

            string listcontent = (string)((ListBoxItem)mediaList.SelectedItem).Content;

            string ConStr = "server=LAPTOP-IGQAGJFV;database=medialist;Trusted_Connection=SSPI";

            SqlConnection conn = new SqlConnection(ConStr.ToString());
            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                string sqlstr_1 = "UPDATE mediaTable set bookmark_10 = ''  WHERE path = '" + pathname + " ' ";

                string sqlstr_2 = "UPDATE mediaMark set mark_10 = ''  WHERE path = '" + pathname + " ' ";


                SqlCommand sqlcmd_1 = new SqlCommand(sqlstr_1, conn);

                SqlCommand sqlcmd_2 = new SqlCommand(sqlstr_2, conn);


                sqlcmd_1.ExecuteNonQuery();

                sqlcmd_2.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                MessageBox.Show("出现问题：" + ex.ToString());
            }
            finally
            {
                conn.Close();
            }
            DB_To_Lb();

            MessageBox.Show("删除成功");
        }



    }



}
