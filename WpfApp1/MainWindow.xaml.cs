using Microsoft.Win32;
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

using System.Runtime.InteropServices;

using MathWorks.MATLAB.NET.Arrays;
using MathWorks.MATLAB.NET.Utility;
using imread_turbopixel;
using imread_ncut;
using imncut_sp;

using Window = System.Windows.Window;

using System.Threading;
using System.Diagnostics;
using System.Windows.Threading;
using System.IO;
using System.Net.Cache;

namespace WpfApp1
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        [DllImport("Dll1.dll", EntryPoint = "SLICmain", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SLICmain(int superPixelNum = 200, int compactFactor = 10, bool saveLabels = false, bool drawBoundaries = true, bool speedMode = true);

        [DllImport("Dll1.dll", EntryPoint = "SLICinit", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SLICinit(string inputFile, string outputDir, int superPixelNum = -1, int compactFactor = -1);

        [DllImport(@"FH.dll", EntryPoint = "FH", ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        public static extern int FH(string inpath, string outpath, float sigma, float k, int min_size);

        public int method { set; get; }
        public string FileURL { set; get; }
        public int para1 { set; get; }
        public int para2 { set; get; }
        public int runningTime { set; get; }

        public MainWindow()
        {
            InitializeComponent();
        }
        public void ChangeInputPicture(string FileName)
        {
            GC.Collect();
            t1.Text = FileName;
            hint0.Content = "";

            var images = new BitmapImage();
            images.BeginInit();
            images.CacheOption = BitmapCacheOption.OnLoad;
            images.UriSource = new Uri(FileName);
            images.EndInit();
            i1.Source = images;

        }
        public void ChangeOutputPicture(string FileName)
        {
            GC.Collect();
            hint1.Content = "";

            var images = new BitmapImage();
            images.BeginInit();
            images.CacheOption = BitmapCacheOption.OnLoad;
            images.UriCachePolicy = new RequestCachePolicy(RequestCacheLevel.BypassCache);
            images.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            images.UriSource = new Uri(FileName);
            images.EndInit();

            o1.Source = images;
            double tT = (double)(System.Environment.TickCount - runningTime) / 1000;
            Console.WriteLine(tT.ToString());
            t1_Copy.Text ="花费时间："+ tT.ToString() + "s";
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(comb.Text);
            OpenFileDialog inImage = new OpenFileDialog();
            inImage.Filter = ".jpg Image Files|*.jpg";
            if ((bool)inImage.ShowDialog())
            {
                ChangeInputPicture(inImage.FileName);
            }
        }

        private void G1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.Link;
            else e.Effects = DragDropEffects.None;
        }

        private void G1_Drop(object sender, DragEventArgs e)
        {
            string fileName = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            var inFormat = fileName.Substring(fileName.Length - 4).ToLower();
            if (inFormat == ".jpg" || inFormat == ".png" || inFormat == ".jepg")
            {
                ChangeInputPicture(fileName);
            }
            else
            {
                MessageBox.Show("图片格式错误！");
            }
        }

        public void GetResult()
        {
            runningTime = System.Environment.TickCount;
            switch (method)
            {
                case 0://SLIC
                    SLICinit(FileURL, "", para1, para2);
                    SLICmain(para1, para2);
                    break;
                case 1://FH
                    FH(FileURL, System.Environment.CurrentDirectory + @"\out2.jpg", (float)0.5, (float)para1, para2);
                    break;
                case 2://NC
                    imread_ncut.Class1 h = new imread_ncut.Class1();
                    h.imread_ncut(FileURL, (int)i1.ActualHeight, (int)i1.ActualWidth);
                    break;
                case 3://turbopixel
                    imread_turbopixel.Class1 tb = new imread_turbopixel.Class1();
                    tb.imread_turbopixel(FileURL, para1);
                    break;
                case 4://Graphcut                     
                    imncut_sp.Class1 b2 = new imncut_sp.Class1();
                    b2.sp_demo(FileURL);
                    break;
                case 5://MEAN
                    Process p = Process.Start(System.Environment.CurrentDirectory + @"\meanshift.exe");
                    p.WaitForExit();
                    break;
                default://?                   
                    break;
            }


            this.Dispatcher.BeginInvoke((Action)delegate ()
            {
                switch (method)
                {
                    case 0://SLIC   
                        ChangeOutputPicture(System.Environment.CurrentDirectory + @"\out.jpg");
                        break;
                    case 1://FH
                        ChangeOutputPicture(System.Environment.CurrentDirectory + @"\out2.jpg");
                        break;
                    case 2://NC                    
                        ChangeOutputPicture(System.Environment.CurrentDirectory + @"\2.png");
                        break;
                    case 3://turbopixel
                        ChangeOutputPicture(System.Environment.CurrentDirectory + @"\out4.png");
                        break;
                    case 4://Graphcut    
                        ChangeOutputPicture(System.Environment.CurrentDirectory + @"\out5.png");
                        break;
                    default:
                        break;
                }
                Running.Visibility = System.Windows.Visibility.Hidden;
            });
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (t1.Text != "")
            {
                Running.Visibility = System.Windows.Visibility.Visible;

                method = comb.SelectedIndex;
                FileURL = t1.Text;
                para1 = (int)PixelNum.Value;
                para2 = (int)CompactFactor.Value;

                Thread newThread = new Thread(GetResult);
                newThread.Start();
            }
        }

        private void Comb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                switch (comb.SelectedIndex)
                {
                    case 0://SLIC                    
                        hint3.Visibility = System.Windows.Visibility.Hidden;
                        pl1.Text = "pixel number";
                        p1.Visibility = System.Windows.Visibility.Visible;
                        pl2.Text = "compact factor";
                        p2.Visibility = System.Windows.Visibility.Visible;
                        break;
                    case 1://FH
                           //FH(FileURL, System.Environment.CurrentDirectory + @"\out2.jpg", (float)0.5, (float)500, 50);
                           //sigma为对原图像进行高斯滤波去噪，k为控制合并后的区域数量，min_size的作用是分割后产生若干小区域
                        hint3.Visibility = System.Windows.Visibility.Hidden;
                        pl1.Text = "区域数量";
                        p1.Visibility = System.Windows.Visibility.Visible;
                        pl2.Text = "min size";
                        p2.Visibility = System.Windows.Visibility.Visible;
                        break;
                    case 2://NC
                        hint3.Visibility = System.Windows.Visibility.Visible;
                        p1.Visibility = System.Windows.Visibility.Hidden;
                        p2.Visibility = System.Windows.Visibility.Hidden;
                        break;
                    case 3://turbopixel
                        hint3.Visibility = System.Windows.Visibility.Hidden;
                        pl1.Text = "pixel number";
                        p1.Visibility = System.Windows.Visibility.Visible;
                        p2.Visibility = System.Windows.Visibility.Hidden;
                        //tb.imread_turbopixel(FileURL, 600);
                        break;
                    case 4://Graphcut                     
                        hint3.Visibility = System.Windows.Visibility.Visible;
                        p1.Visibility = System.Windows.Visibility.Hidden;
                        p2.Visibility = System.Windows.Visibility.Hidden;
                        break;
                    case 5://MEAN
                        hint3.Visibility = System.Windows.Visibility.Visible;
                        p1.Visibility = System.Windows.Visibility.Hidden;
                        p2.Visibility = System.Windows.Visibility.Hidden;
                        break;
                    default://?                   
                        break;

                }
            }

        }
    }
}
