using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using XamlAnimatedGif;

namespace SkinText
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string filepath ;
        private bool fileChanged=false;
        public string gifMethod = "CPU";
        public string path;
        public string imagepath ;
        public int borderSz;
        private WindowConfig config;
        public FontConfig font;
        public MainWindow()
        { 
            InitializeComponent();
        }
        ///////////////////////////////////////////////////
        private void New_Click(object sender, RoutedEventArgs e)
        {
            if (fileChanged)
            {
                switch (MessageBox.Show("There are unsaved Changes to: "+ filepath + "\r\nDo you want to save?", "Save Changes?", MessageBoxButton.YesNoCancel, MessageBoxImage.Exclamation, MessageBoxResult.Cancel))
                {
                    case (MessageBoxResult.Yes):
                        {
                            Save(false);
                            New_File();
                            break;
                        }
                    case MessageBoxResult.No:
                        {
                            New_File();
                            break;
                        }
                    case MessageBoxResult.Cancel:
                        {
                            //do nothing
                            break;
                        }
                    default: break;
                }
            }
            else
            {
                New_File();
            }
        }
        /// <summary>
        /// <para>filepath = "";</para>
        /// <para>rtb.Document = new FlowDocument();</para>
        /// <para>fileChanged = false;</para>
        /// </summary>
        private void New_File()
        {
            filepath = "";
            rtb.Document = new FlowDocument();
            fileChanged = false;
        }
        private void Open_Click(object sender, RoutedEventArgs e)
        {
            if (fileChanged)
            {
                switch (MessageBox.Show("There are unsaved Changes to: " + filepath + "\r\nDo you want to save?", "Save Changes?", MessageBoxButton.YesNoCancel, MessageBoxImage.Exclamation, MessageBoxResult.Cancel))
                {
                    case (MessageBoxResult.Yes):
                        {
                            Save(false);
                            Open_File();
                            break;
                        }
                    case MessageBoxResult.No:
                        {
                            Open_File();
                            break;
                        }
                    case MessageBoxResult.Cancel:
                        {
                            //do nothing
                            break;
                        }
                    default: break;
                }
            }
            else
            {
                Open_File();
            }
        }
        private void Open_File()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Multiselect = false,
                CheckFileExists = true,
                CheckPathExists = true,
                ValidateNames = true,
                RestoreDirectory = true,
                Filter = "Text files (*.txt, *.rtf) | *.txt; *.rtf"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                filepath = openFileDialog.FileName;
                try
                {
                    TextRange range;
                    FileStream fStream;
                    range = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
                    fStream = new FileStream(filepath, FileMode.OpenOrCreate);
                   if (filepath.Substring(filepath.Length-3).ToLower().Equals("rtf"))
                    {
                        range.Load(fStream, DataFormats.Rtf);
                    }
                    else
                    {
                        range.Load(fStream, DataFormats.Text);
                    }
                    /*
                    using (MemoryStream rtfMemoryStream = new MemoryStream())
                    {
                        rtfMemoryStream.Seek(0, SeekOrigin.Begin);
                        fStream.Seek(0, SeekOrigin.Begin);
                        fStream.CopyTo(rtfMemoryStream);
                        range.Load(rtfMemoryStream, DataFormats.Rtf);
                    }*/
                    fStream.Close();
                    fileChanged = false;
                    Save_config();
                }
                catch (Exception)
                {
                    filepath = "";
                    MessageBox.Show("Failed to Open File:\r\n" + filepath, "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.ServiceNotification);
                }
            }
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Save(false);
        }
        private void Save_as_Click(object sender, RoutedEventArgs e)
        {
            Save(true);
        }
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            if (fileChanged)
            {
                switch (MessageBox.Show("There are unsaved Changes to: " + filepath + "\r\nDo you want to save?", "Save Changes?", MessageBoxButton.YesNoCancel, MessageBoxImage.Exclamation, MessageBoxResult.Cancel))
                {
                    case (MessageBoxResult.Yes):
                        {
                            Save(false);
                            Exit_program();
                            break;
                        }
                    case MessageBoxResult.No:
                        {
                            Exit_program();
                            break;
                        }
                    case MessageBoxResult.Cancel:
                        {
                            //do nothing
                            break;
                        }
                    default: break;
                }
            }
            else
            {
                Exit_program();
            }
        }
        private void Exit_program()
        {
            font.Close();
            config.Close();
            Save_config();
            this.Close();
        }
        private void Config_Click(object sender, RoutedEventArgs e)
        {
            config.Show();
            FixResizeTextbox(); 
        }
        private void Rtb_MouseMove(object sender, MouseEventArgs e)
        {
            Point m = e.GetPosition(rtb);
            if (m.Y <= 10)
            {
                menu.Visibility= Visibility.Visible;
            }
            else if (m.Y > 20)
            {
                menu.Visibility = Visibility.Collapsed;
            }
            if (true)
            {
                if (m.X >= panel.ActualWidth - 20)
                {
                    rtb.VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto;
                }
                if ((rtb.Document.PageWidth>= panel.ActualWidth - 20) && (m.Y >= panel.ActualHeight - 20))
                {
                    rtb.HorizontalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto;
                }
                if ((m.X < panel.ActualWidth - 20) && (m.Y < panel.ActualHeight - 20))
                {
                    rtb.VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Hidden;
                    rtb.HorizontalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Hidden;
                }
            }
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
        public void Rtb_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (panel.ActualWidth>21)
            {
                rtb.Document.PageWidth = panel.Width-20;
            }
            else
            {
                rtb.Document.PageWidth = 1;
            }
            double row0=grid.RowDefinitions[0].ActualHeight;
            double row2 = grid.RowDefinitions[2].ActualHeight;
            double column0 = grid.ColumnDefinitions[0].ActualWidth;
            double column2 = grid.ColumnDefinitions[2].ActualWidth;
            corner1.Margin = new Thickness(column0 - borderSz   , row0 - borderSz   , 0         , 0                 );
            corner2.Margin = new Thickness(column0 - borderSz   , -borderSz         , 0         , row2    );
            corner3.Margin = new Thickness(-borderSz            , row0 - borderSz   , column2   , 0                 );
            corner4.Margin = new Thickness(-borderSz            , -borderSz         , column2   , row2  );
        }
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);
        private void Menu_MouseDown(object sender, MouseButtonEventArgs e)
        {
            WindowInteropHelper helper = new WindowInteropHelper(window);
            SendMessage(helper.Handle, 161, 2, 0);
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            config = new WindowConfig(this);
            font = new FontConfig(this);
            Load_default();
            //getPath();
            path = ((App)Application.Current).GetPath();
            #region get FileName test
            /*
            string fileNameTest1 = window.GetType().Assembly.Lo‌​cation;
            string fileNameTest2 = AppDomain.CurrentDomain.FriendlyName;
            string fileNameTest3 = Environment.GetCommandLineArgs()[0];
            string fileNameTest4 = Assembly.GetEntryAssembly().Location;
            string fileNameTest5 = Assembly.GetEntryAssembly().CodeBase;
            string fileNameTest6 = Assembly.GetExecutingAssembly().ManifestModule.Name;
            //string fileNameTest7 = Assembly.GetExecutingAssembly().GetName().Name;
            string fileNameTest8 = Assembly.GetExecutingAssembly().GetName().CodeBase;
            //string fileNameTest9 = Path.GetFileName(fileNameTest8);
            //string fileNameTest10 = Path.GetFileNameWithoutExtension(fileNameTest8);
            string fileNameTest11 = Process.GetCurrentProcess().ProcessName;
            //string fileNameTest12 = Process.GetCurrentProcess().MainModule.FileName.Replace(".vshost", "");
            //If you need the Program name to set up a firewall rule, use:
            string fileNameTest13 = Process.GetCurrentProcess().MainModule.FileName;
            //All works when changing exe name except 7
            /* MessageBox.Show("fileNameTest1: \t" + fileNameTest1 + "\r\n" +
                            "fileNameTest2: \t" + fileNameTest2 + "\r\n" +
                            "fileNameTest3: \t" + fileNameTest3 + "\r\n" +
                            "fileNameTest4: \t" + fileNameTest4 + "\r\n" +
                            "fileNameTest5: \t" + fileNameTest5 + "\r\n" +
                            "fileNameTest6: \t" + fileNameTest6 + "\r\n" +
                           // "fileNameTest7: \t" + fileNameTest7 + "\r\n" +
                            "fileNameTest8: \t" + fileNameTest8 + "\r\n" +
                           // "fileNameTest9: \t" + fileNameTest9 + "\r\n" +
                           // "fileNameTest10: \t" + fileNameTest10 + "\r\n" +
                            "fileNameTest11: \t" + fileNameTest11 + "\r\n" +
                           // "fileNameTest12: \t" + fileNameTest12 + "\r\n" +
                            "fileNameTest13: \t" + fileNameTest13 + "\r\n" +
                            "");
          */
            #endregion
            ReadConfig();
            grid.UpdateLayout();
            FixResizeTextbox();

            //set to false after the initial text_change that occur on first load
            fileChanged = false;

            /*
                textBlock.TextWrapping = TextWrapping.Wrap;
                textBlock.LineHeight = lineHeight;
            */
        }
        private void FixResizeTextbox()
        {
            GridLength ad = new GridLength(grid.ColumnDefinitions[0].ActualWidth, GridUnitType.Star);
            grid.ColumnDefinitions[0].Width = ad;
            ad = new GridLength(grid.ColumnDefinitions[1].ActualWidth, GridUnitType.Star);
            grid.ColumnDefinitions[1].Width = ad;
            ad = new GridLength(grid.ColumnDefinitions[2].ActualWidth, GridUnitType.Star);
            grid.ColumnDefinitions[2].Width = ad;
            ad = new GridLength(grid.RowDefinitions[0].ActualHeight, GridUnitType.Star);
            grid.RowDefinitions[0].Height = ad;
            ad = new GridLength(grid.RowDefinitions[1].ActualHeight, GridUnitType.Star);
            grid.RowDefinitions[1].Height = ad;
            ad = new GridLength(grid.RowDefinitions[2].ActualHeight, GridUnitType.Star);
            grid.RowDefinitions[2].Height = ad;
        }
        /// <summary>
        /// <para>Reads the skintext.ini and loads its config</para>
        /// <para>   </para>
        /// <para>test</para>
        /// </summary>
        private void ReadConfig()
        {
            try
            {
                StreamReader reader = new StreamReader(path+@"\skintext.ini",System.Text.Encoding.UTF8);
                string currentLine;
                string[] line;
                config.bordersize.Value = borderSz = 5;//default value due to text size dependency
                while ((currentLine = reader.ReadLine()) != null)
                {
                    line = currentLine.Split('=');
                    line[0] = line[0].Trim();
                    line[0] = line[0].ToLower();
                    if (line[0] != "")
                    {
                        line[1] = line[1].Trim();
                    }
                    switch (line[0])
                    {
                        case "window_position":
                            {
                                #region position
                                try
                                {
                                    string[] pos = line[1].Split(',');
                                    if (double.TryParse(pos[0], out double top) && double.TryParse(pos[1], out double left))
                                    {
                                        window.Top = top;
                                        window.Left = left;
                                    }
                                }
                                catch (System.Exception)
                                {
                                    double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
                                    double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
                                    double windowWidth = this.Width;
                                    double windowHeight = this.Height;
                                    this.Left = (screenWidth / 2) - (windowWidth / 2);
                                    this.Top = (screenHeight / 2) - (windowHeight / 2);
                                    //throw;
                                }
                                break;
                                #endregion
                            }
                        case "window_size":
                            {
                                #region size
                                try
                                {
                                    string[] wsize = line[1].Split(',');
                                    if (double.TryParse(wsize[0], out double wwidth) && double.TryParse(wsize[1], out double wheight))
                                    {
                                        if (wwidth > window.MinWidth && wwidth < window.MaxWidth)
                                        {
                                            window.Width = wwidth;
                                        }
                                        if (wheight > window.MinHeight && wheight < window.MaxHeight)
                                        {
                                            window.Height = wheight;
                                        }
                                    }
                                }
                                catch (System.Exception)
                                {
                                    window.Width = 525;
                                    window.Height = 350;
                                    //throw;
                                }
                                break;
                                #endregion
                            }
                        case "border_size":
                            {
                                #region border size
                                try
                                {
                                    if (int.TryParse(line[1], out int bsize))
                                    {
                                        if (bsize <= config.bordersize.Maximum && bsize > 0)
                                        {
                                            config.bordersize.Value = borderSz = bsize;
                                        }
                                    }
                                }
                                catch (System.Exception)
                                {
                                    config.bordersize.Value = borderSz = 5;
                                    //throw;
                                }
                                break;
                                #endregion
                            }
                        case "text_size":
                            {
                                #region text_size
                                try
                                {
                                    string[] tsize = line[1].Split(',');
                                    if (double.TryParse(tsize[0], out double twidth0) && double.TryParse(tsize[1], out double twidth1) && double.TryParse(tsize[2], out double twidth2) && double.TryParse(tsize[3], out double theight0) && double.TryParse(tsize[4], out double theight1) && double.TryParse(tsize[5], out double theight2))
                                    {
                                        if (twidth0 >= 0)
                                        {
                                            GridLength gl = new GridLength(twidth0, GridUnitType.Star);
                                            grid.ColumnDefinitions[0].Width = gl;
                                        }
                                        if (twidth2 >= 0)
                                        {
                                            GridLength gl = new GridLength(twidth2, GridUnitType.Star);
                                            grid.ColumnDefinitions[2].Width = gl;
                                        }
                                        if (theight0 >= 0)
                                        {
                                            GridLength gl = new GridLength(theight0, GridUnitType.Star);
                                            grid.RowDefinitions[0].Height = gl;
                                        }
                                        if (theight2 >= 0)
                                        {
                                            GridLength gl = new GridLength(theight2, GridUnitType.Star);
                                            grid.RowDefinitions[2].Height = gl;
                                        }


                                        if ((twidth1 < window.Width - (borderSz * 2 + 1)) && (twidth1 >= grid.ColumnDefinitions[1].MinWidth))
                                        {
                                            GridLength tw = new GridLength(twidth1);
                                            grid.ColumnDefinitions[1].Width = tw;
                                        }
                                        else
                                        {
                                            GridLength tw = new GridLength(window.Width - (borderSz * 2 + 1));
                                            grid.ColumnDefinitions[1].Width = tw;
                                        }
                                        if ((theight1 < window.Height - (borderSz * 2 + 1)) && (theight1 >= grid.RowDefinitions[1].MinHeight))
                                        {
                                            GridLength th = new GridLength(theight1);
                                            grid.RowDefinitions[1].Height = th;
                                        }
                                        else
                                        {
                                            GridLength th = new GridLength(window.Height - (borderSz * 2 + 1));
                                            grid.RowDefinitions[1].Height = th;
                                        }
                                    }
                                }
                                catch (System.Exception)
                                {
                                    grid.ColumnDefinitions[1].Width = new GridLength(window.Width - (borderSz * 2 + 1));
                                    grid.RowDefinitions[1].Height = new GridLength(window.Height - (borderSz * 2 + 1));
                                    //throw;
                                }
                                break;
                                #endregion
                            }
                        case "file":
                            {
                                #region file
                                try
                                {
                                    string fileName = line[1].Trim();
                                    TextRange range;
                                    FileStream fStream;
                                    if (!fileName.Contains("\\") && fileName != "")
                                    {
                                        fileName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + fileName;
                                    }
                                    if (File.Exists(fileName))
                                    {
                                        range = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
                                        fStream = new FileStream(fileName, FileMode.OpenOrCreate);
                                        if (fileName.Substring(fileName.Length - 3).ToLower().Equals("rtf"))
                                        {
                                            range.Load(fStream, DataFormats.Rtf);
                                        }
                                        else
                                        {
                                            range.Load(fStream, DataFormats.Text);
                                        }
                                        /*
                                        using (MemoryStream rtfMemoryStream = new MemoryStream())
                                        {
                                            rtfMemoryStream.Seek(0, SeekOrigin.Begin);
                                            fStream.Seek(0, SeekOrigin.Begin);
                                            fStream.CopyTo(rtfMemoryStream);
                                            range.Load(rtfMemoryStream, DataFormats.Rtf);

                                        }*/


                                        fStream.Close();
                                        filepath = fileName;
                                    }
                                    else
                                    {
                                        if (fileName != "")
                                        {
                                            MessageBox.Show("Failed to Open File:\r\n " + fileName, "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.ServiceNotification);
                                        }
                                    }                                        
                                    
                                }
                                catch (System.Exception)
                                {
                                    filepath = "";
                                    //empty file
                                    //throw;
                                }
                                break;
                                #endregion
                            }
                        case "resize_enabled":
                            {
                                #region border show
                                try
                                {
                                    if (bool.TryParse(line[1], out bool rcheck))
                                    {
                                        config.resizecheck.IsChecked = rcheck;
                                    }
                                }
                                catch (System.Exception)
                                {
                                    config.resizecheck.IsChecked = true;
                                    //throw;
                                }
                                break;
                                #endregion
                            }
                        case "border_color":
                            {
                                #region border color
                                try
                                {
                                    Color bcolor = (Color)ColorConverter.ConvertFromString(line[1]);
                                    config.ClrPcker_Background.SelectedColor = bcolor;
                                }
                                catch (System.Exception)
                                {
                                    //#997E7E7E by default in xaml
                                    //and copies to config.ClrPcker_Background.SelectedColor on its Window_Loaded()
                                    Color color = (Color)ColorConverter.ConvertFromString("#997E7E7E");
                                    config.ClrPcker_Background.SelectedColor = color;
                                    //throw;
                                }
                                break;
                                #endregion
                            }
                        case "window_color":
                            {
                                #region window color
                                try
                                {
                                    Color wcolor = (Color)ColorConverter.ConvertFromString(line[1]);
                                    config.ClrPcker_Background2.SelectedColor = wcolor;
                                }
                                catch (System.Exception)
                                {
                                    //transparent by default in xaml
                                    //and copies to config.ClrPcker_Background2.SelectedColor on its Window_Loaded()
                                    config.ClrPcker_Background2.SelectedColor = Colors.Transparent;
                                    //throw;
                                }
                                break;
                                #endregion
                            }
                        case "text_bg_color":
                            {
                                #region text bg color
                                try
                                {
                                    Color tcolor = (Color)ColorConverter.ConvertFromString(line[1]);
                                    config.ClrPcker_Background3.SelectedColor = tcolor;
                                }
                                catch (System.Exception)
                                {
                                    //transparent by default in xaml
                                    //and copies to config.ClrPcker_Background3.SelectedColor on its Window_Loaded()
                                    config.ClrPcker_Background3.SelectedColor = Colors.Transparent;
                                    //throw;
                                }
                                break;
                                #endregion
                            }
                        case "rotation":
                            {
                                #region rotation
                                try
                                {
                                    if (double.TryParse(line[1], out double angle))
                                    {
                                        config.slValue.Value = angle;
                                    }
                                }
                                catch (System.Exception)
                                {
                                    config.slValue.Value = 0;
                                    //throw;
                                }
                                break;
                                #endregion
                            }
                        case "gifmethod":
                            {
                                #region gifMethod
                                try
                                {
                                    string method = line[1].Trim().ToUpper();
                                    if (method != "RAM")
                                    {
                                        method = "CPU";
                                        config.GifMethodCPU.IsChecked = true;
                                    }
                                    else
                                    {
                                        config.GifMethodRAM.IsChecked = true;
                                    }
                                    gifMethod = method;
                                }
                                catch (System.Exception)
                                {
                                    gifMethod = "CPU";
                                    config.GifMethodCPU.IsChecked = true;
                                    //throw;
                                }
                                break;
                                #endregion
                            }
                        case "bg_image":
                            {
                                #region Load Background Image
                                try
                                {
                                    imagepath = line[1].Trim();
                                    if (!imagepath.Contains("\\") && imagepath != "")
                                    {
                                        imagepath =Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + imagepath;
                                    }
                                    if (File.Exists(imagepath))
                                    {
                                        config.LoadImage(imagepath);
                                    }
                                    else
                                    {
                                        if (imagepath != "")
                                        {
                                            MessageBox.Show("Failed to Load Image:\r\n " + imagepath, "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.ServiceNotification);
                                            if (window.Background.ToString() == Colors.Transparent.ToString())
                                            {
                                                config.ClrPcker_Background2.SelectedColor = (Color)ColorConverter.ConvertFromString("#85949494");
                                            }
                                        }
                                    }
                                }
                                catch (Exception)
                                {
                                    if (window.Background.ToString() == Colors.Transparent.ToString())
                                    {
                                        config.ClrPcker_Background2.SelectedColor = (Color)ColorConverter.ConvertFromString("#85949494");
                                        imagepath = "";
                                    }
                                    //throw;
                                }
                                break;
                                #endregion
                            }
                        case "image_opacity":
                            {
                                #region image opcatity
                                try
                                {
                                    if (double.TryParse(line[1], out double ival))
                                    {
                                        config.imageopacityslider.Value = ival;
                                    }
                                }
                                catch (System.Exception)
                                {
                                    config.imageopacityslider.Value = 100;
                                    //throw;
                                }
                                break;
                                #endregion
                            }
                        case "text_opacity":
                            {
                                #region text opcatity
                                try
                                {
                                    if (double.TryParse(line[1], out double tval))
                                    {
                                        config.textopacityslider.Value = tval;
                                    }
                                }
                                catch (System.Exception)
                                {
                                    //default=100;
                                    //throw;
                                }
                                break;
                                #endregion
                            }
                        case "window_opacity":
                            {
                                #region window opcatity
                                try
                                {
                                    if (double.TryParse(line[1], out double wval))
                                    {
                                        config.windowopacityslider.Value = wval;
                                    }
                                }
                                catch (System.Exception)
                                {
                                    //default=100;
                                    //throw;
                                }
                                break;
                                #endregion
                            }
                        case "read_only":
                            {
                                #region read only
                                try
                                {
                                    if (bool.TryParse(line[1], out bool rocheck))
                                    {
                                        config.@readonly.IsChecked = rocheck;
                                    }
                                }
                                catch (System.Exception)
                                {
                                    config.@readonly.IsChecked = false;
                                    //throw;
                                }
                                break;
                                #endregion
                            }
                        case "spell_check":
                            {
                                #region spellcheck
                                try
                                {
                                    if (bool.TryParse(line[1], out bool scheck))
                                    {
                                        config.spellcheck.IsChecked = scheck;
                                    }
                                }
                                catch (System.Exception)
                                {
                                    config.spellcheck.IsChecked = false;
                                    //throw;
                                }
                                break;
                                #endregion
                            }
                        case "always_on_top":
                            {
                                #region  always on top
                                try
                                {
                                    if (bool.TryParse(line[1], out bool acheck))
                                    {
                                        config.allwaysontop.IsChecked = acheck;
                                    }
                                }
                                catch (System.Exception)
                                {
                                    config.allwaysontop.IsChecked = false;
                                    //throw;
                                }
                                break;
                                #endregion
                            }
                        case "taskbar_icon":
                            {
                                #region  taskbar icon
                                try
                                {
                                    if (bool.TryParse(line[1], out bool tcheck))
                                    {
                                        config.taskbarvisible.IsChecked = tcheck;
                                    }
                                }
                                catch (System.Exception)
                                {
                                    config.taskbarvisible.IsChecked = true;
                                    //throw;
                                }
                                break;
                                #endregion
                            }
                        case "notification_icon":
                            {
                                #region  notification icon
                                try
                                {
                                    if (bool.TryParse(line[1], out bool tcheck))
                                    {
                                        config.NotificationVisible.IsChecked = tcheck;
                                    }
                                }
                                catch (System.Exception)
                                {
                                    config.NotificationVisible.IsChecked = true;
                                    //throw;
                                }
                                break;
                                #endregion
                            }
                        case "resize_visible":
                            {
                                #region  resize visible
                                try
                                {
                                    if (bool.TryParse(line[1], out bool tcheck))
                                    {
                                        config.ResizeVisible.IsChecked = tcheck;
                                    }
                                }
                                catch (System.Exception)
                                {
                                    config.ResizeVisible.IsChecked = true;
                                    //throw;
                                }
                                break;
                                #endregion
                            }
                        default: break;
                    }
                    /*
                        spellcheck dictionary
                    */
                }
            }
            catch (System.Exception)
            {
                // The appdata folders dont exist
                //throw;
            }
        }
        public void Save_config()
        {
            //had to do this: https://msdn.microsoft.com/library/ms182334.aspx
            FileStream fs = null;
            try
            {
                fs = new FileStream(path + @"\skintext.ini", FileMode.Create, FileAccess.Write);
                using (TextWriter writer = new StreamWriter(fs, System.Text.Encoding.UTF8))
                {
                    fs = null;
                    string data;
                    //window_position
                    data = "window_position = " + window.Top.ToString() + " , " + window.Left.ToString();
                    writer.WriteLine(data);
                    //window_size
                    data = "window_size = " + window.Width.ToString() + " , " + window.Height.ToString();
                    writer.WriteLine(data);
                    //border_size
                    data = "border_size = " + borderSz;
                    writer.WriteLine(data);
                    //text_size
                    data = "text_size = " + grid.ColumnDefinitions[0].ActualWidth.ToString() + " , " + grid.ColumnDefinitions[1].ActualWidth.ToString() + " , " + grid.ColumnDefinitions[2].ActualWidth.ToString() + " , " + grid.RowDefinitions[0].ActualHeight.ToString() + " , " + grid.RowDefinitions[1].ActualHeight.ToString() + " , " + grid.RowDefinitions[2].ActualHeight.ToString();
                    writer.WriteLine(data);
                    //filetry 
                    try
                    {
                        /*int index = filepath.LastIndexOf("\\");
                        string tempdir;
                        if (index>0)
                        {
                            tempdir = filepath.Substring(0, index).ToLower();
                        }
                        else
                        {
                            tempdir = filepath.ToLower();
                        }
                        string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location).ToLower();
                        if (object.Equals(tempdir, dir))
                        {
                            filepath = filepath.Substring(index + 1);
                        }*/
                        data = "file = " + filepath;
                        writer.WriteLine(data);
                    }
                    catch (Exception) { }
                    //resize_enabled
                    data = "resize_enabled = " + config.resizecheck.IsChecked.Value.ToString();
                    writer.WriteLine(data);
                    //border_color
                    data = "border_color = " + config.ClrPcker_Background.SelectedColor.ToString();
                    writer.WriteLine(data);
                    //border_color
                    data = "window_color = " + config.ClrPcker_Background2.SelectedColor.ToString();
                    writer.WriteLine(data);
                    //border_color
                    data = "text_bg_color = " + config.ClrPcker_Background3.SelectedColor.ToString();
                    writer.WriteLine(data);
                    //rotation
                    data = "rotation = " + config.slValue.Value.ToString();
                    writer.WriteLine(data);
                    //GIF Method
                    data = "gifMethod = " + gifMethod;
                    writer.WriteLine(data);
                    //bg_image
                    try
                    {
                        /*int index = imagepath.LastIndexOf("\\");
                        string tempdir;
                        if (index > 0)
                        {
                            tempdir = imagepath.Substring(0, index).ToLower();
                        }
                        else
                        {
                            tempdir = imagepath.ToLower();
                        }
                        string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location).ToLower();
                        if (object.Equals(tempdir, dir))
                        {
                            imagepath = imagepath.Substring(index + 1);
                        }*/
                        data = "bg_image = " + imagepath;
                        writer.WriteLine(data);
                    }
                    catch (Exception) { }
                    //image_opacity
                    data = "image_opacity = " + config.imageopacityslider.Value;
                    writer.WriteLine(data);
                    //text_opacity
                    data = "text_opacity = " + config.textopacityslider.Value;
                    writer.WriteLine(data);
                    //window_opacity
                    data = "window_opacity = " + config.windowopacityslider.Value;
                    writer.WriteLine(data);
                    //read_only
                    data = "read_only = " + config.@readonly.IsChecked.Value.ToString();
                    writer.WriteLine(data);
                    //spell_check
                    data = "spell_check = " + config.spellcheck.IsChecked.Value.ToString();
                    writer.WriteLine(data);
                    //always_on_top
                    data = "always_on_top = " + config.allwaysontop.IsChecked.Value.ToString();
                    writer.WriteLine(data);
                    //taskbar_icon
                    data = "taskbar_icon = " + config.taskbarvisible.IsChecked.Value.ToString();
                    writer.WriteLine(data);
                    //notification_icon
                    data = "notification_icon = " + config.NotificationVisible.IsChecked.Value.ToString();
                    writer.WriteLine(data);
                    //ResizeVisible
                    data = "resize_visible = " + config.ResizeVisible.IsChecked.Value.ToString();
                    writer.WriteLine(data);
                }
                
                //FileInfo info;
                //info = new FileInfo("skintext.ini");
                //info.Attributes = FileAttributes.Hidden;

                /*TODO: skinfile
                 * Skin install config
                 * Same file?, other file?
                 * probably other is best
                 * 
                    SkinName = Circuitous
                    SkinAuthor = Col-Darby
                    SkinVersion = 1.0
                    SkinTextVer = 1.3.0.560
                 */
            }
            catch (Exception)
            {
            }
            finally
            {
                if (fs != null)
                {
                    fs.Dispose();
                }
            }
        }
        private void Load_default() 
        {
            //window size
            window.Width = 525;
            window.Height = 350;
            
            //Window position
            double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
            double windowWidth = this.Width;
            double windowHeight = this.Height;
            this.Left = (screenWidth / 2) - (windowWidth / 2);
            this.Top = (screenHeight / 2) - (windowHeight / 2);
            
            //border size
            config.bordersize.Value = borderSz = 5;

            //text area size
            grid.ColumnDefinitions[1].Width = new GridLength(window.Width - (borderSz * 2 + 1)); ;
            grid.RowDefinitions[1].Height = new GridLength(window.Height - (borderSz * 2 + 1));

            //border color
            //#997E7E7E by default in xaml
            //and copies to config.ClrPcker_Background.SelectedColor on its Window_Loaded()
            Color bcolor = (Color)ColorConverter.ConvertFromString("#997E7E7E");
            config.ClrPcker_Background.SelectedColor = bcolor;
            
            //window color
            //transparent by default in xaml
            //and copies to config.ClrPcker_Background2.SelectedColor on its Window_Loaded()
            config.ClrPcker_Background2.SelectedColor = Colors.Transparent;
            
            //text bg color
            //transparent by default in xaml
            //and copies to config.ClrPcker_Background3.SelectedColor on its Window_Loaded()
            config.ClrPcker_Background3.SelectedColor = Colors.Transparent;

            //rotation angle
            config.slValue.Value = 0;


            //no file
            filepath = "";

            //GIF Method
            config.GifMethodCPU.IsChecked = true;

            //change bg to something visible (overwrites default window color)
            config.ImageClear();
            config.ClrPcker_Background2.SelectedColor = (Color)ColorConverter.ConvertFromString("#85949494");
            imagepath = "";

            //opacity
            config.imageopacityslider.Value = 100;
            config.windowopacityslider.Value = 100;
            config.textopacityslider.Value = 100;
            
            //checkboxes
            config.resizecheck.IsChecked = true;
            config.@readonly.IsChecked = false;
            config.spellcheck.IsChecked = false;
            config.allwaysontop.IsChecked = false;
            config.taskbarvisible.IsChecked = true;
            config.NotificationVisible.IsChecked = true;
            config.ResizeVisible.IsChecked = true;

        }
        private void Save(bool saveas)
        {
            if (filepath.Length < 4)
            {
                saveas = true;
            }
            if (saveas)
            {
                try
                {
                    SaveFileDialog savedialog = new SaveFileDialog()
                    {
                        CreatePrompt = true,
                        OverwritePrompt = true,
                        CheckPathExists = true,
                        ValidateNames = true,
                        RestoreDirectory = true,
                        FileName = "Notes",
                        Filter = "RTF|*.rtf",
                        DefaultExt = ".rtf"
                    };
                    if (savedialog.ShowDialog() == true)
                    {
                        filepath = savedialog.FileName;

                        TextRange t = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
                        FileStream file = new FileStream(filepath, FileMode.Create);
                        t.Save(file, System.Windows.DataFormats.Rtf);
                        /*using (MemoryStream rtfMemoryStream = new MemoryStream())
                        {
                            using (StreamWriter rtfStreamWriter = new StreamWriter(rtfMemoryStream))
                            {
                                t.Save(rtfMemoryStream, DataFormats.Rtf);
                                rtfMemoryStream.Flush();
                                rtfMemoryStream.Position = 0;
                                StreamReader sr = new StreamReader(rtfMemoryStream);
                                //MessageBox.Show(sr.ReadToEnd());
                                rtfMemoryStream.WriteTo(file);
                            }
                        }*/
                        file.Close();
                        fileChanged = false;
                        Save_config();                        
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Failed to Write File", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.ServiceNotification);
                }
            }
            else
            {
                try
                {
                    TextRange t = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
                    FileStream file = new FileStream(filepath, FileMode.Create);
                    t.Save(file, System.Windows.DataFormats.Rtf);
                    /*using (MemoryStream rtfMemoryStream = new MemoryStream())
                    {
                        using (StreamWriter rtfStreamWriter = new StreamWriter(rtfMemoryStream))
                        {
                            t.Save(rtfMemoryStream, DataFormats.Rtf);
                            rtfMemoryStream.Flush();
                            rtfMemoryStream.Position = 0;
                            StreamReader sr = new StreamReader(rtfMemoryStream);
                            //MessageBox.Show(sr.ReadToEnd());
                            rtfMemoryStream.WriteTo(file);
                        }
                    }*/
                    file.Close();
                    fileChanged = false;
                    Save_config();
                }
                catch (Exception)
                {
                    MessageBox.Show("Failed to Write File", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.ServiceNotification);
                }
            }
        }
        public void TextFormat(Brush foreColor,Brush backgroundColor,FontFamily fontFamily,double fontSize,TextDecorationCollection decor,FontStyle fontStyle,FontWeight fontWeight, TextAlignment textalign, FlowDirection flow)
        {
            if (backgroundColor.Equals(Brushes.Transparent))
            {
                backgroundColor = null;
            }

            // Make sure we have a selection. Should have one even if there is no text selected.
            if (rtb.Selection != null)
            {
                // Check whether there is text selected or just sitting at cursor
                if (rtb.Selection.IsEmpty)
                {
                    // Check to see if we are at the start of the textbox and nothing has been added yet
                    if (rtb.Selection.Start.Paragraph == null)
                    {
                        // Add a new paragraph object to the richtextbox with the font properties
                        /* Paragraph p = new Paragraph();
                         p.Foreground = foreColor;
                         p.Background = backgroundColor;
                         p.FontFamily = fontFamily;
                         p.FontSize = fontSize;
                         p.TextDecorations = null;
                         p.TextDecorations = decor.Clone();
                         p.FontStyle = fontStyle;
                         p.FontWeight = fontWeight;
                         p.TextAlignment = textalign;
                         p.FlowDirection = flow;
                         //p.LineHeight = lineHeight;
                         rtb.Document.Blocks.Add(p);*/

                        //Apparently if we add properties like textdecorations to a paragraph it will override
                        //the run properties and be stuck forever so testing with adding a run instead of a paragraph

                        Paragraph p = new Paragraph();
                        // Create a new run object with the font properties, and add it to the current block
                        Run newRun = new Run()
                        {
                            Foreground = foreColor,
                            Background = backgroundColor,
                            FontFamily = fontFamily,
                            FontSize = fontSize,
                            TextDecorations = null
                        };
                        newRun.TextDecorations = decor.Clone();
                        newRun.FontStyle = fontStyle;
                        newRun.FontWeight = fontWeight;
                        newRun.FlowDirection = flow;
                        rtb.Document.Blocks.Add(p);// worked!
                        p.Inlines.Add(newRun);
                        //curParagraph.LineHeight = lineHeight;
                        p.TextAlignment = textalign;
                        // Reset the cursor into the new block. 
                        // If we don't do this, the font properties will default again when you start typing.
                        rtb.CaretPosition = newRun.ElementStart;

                    }
                    else
                    {
                        // Get current position of cursor
                        TextPointer curCaret = rtb.CaretPosition;
                        // Get the current block object that the cursor is in
                        Block curBlock = rtb.Document.Blocks.Where
                            (x => x.ContentStart.CompareTo(curCaret) == -1 && x.ContentEnd.CompareTo(curCaret) == 1).FirstOrDefault();
                        if (curBlock != null)
                        {
                            Paragraph curParagraph = curBlock as Paragraph;
                            // Create a new run object with the font properties, and add it to the current block
                            Run newRun = new Run()
                            {
                                Foreground = foreColor,
                                Background = backgroundColor,
                                FontFamily = fontFamily,
                                FontSize = fontSize,
                                TextDecorations = null
                            };
                            newRun.TextDecorations = decor.Clone();
                            newRun.FontStyle = fontStyle;
                            newRun.FontWeight = fontWeight;
                            newRun.FlowDirection = flow;
                            curParagraph.Inlines.Add(newRun);
                            //curParagraph.LineHeight = lineHeight;
                            curParagraph.TextAlignment = textalign;
                            // Reset the cursor into the new block. 
                            // If we don't do this, the font properties will default again when you start typing.
                            rtb.CaretPosition = newRun.ElementStart;
                        }
                    }
                }
                else // There is selected text, so change the font properties of the selection
                {
                    TextRange selectionTextRange = new TextRange(rtb.Selection.Start, rtb.Selection.End);
                    selectionTextRange.ApplyPropertyValue(TextElement.ForegroundProperty, foreColor);
                    selectionTextRange.ApplyPropertyValue(TextElement.BackgroundProperty, backgroundColor);
                    selectionTextRange.ApplyPropertyValue(TextElement.FontFamilyProperty, fontFamily);
                    selectionTextRange.ApplyPropertyValue(TextElement.FontSizeProperty, fontSize);
                    selectionTextRange.ApplyPropertyValue(TextElement.FontStyleProperty, fontStyle);
                    selectionTextRange.ApplyPropertyValue(TextElement.FontWeightProperty, fontWeight);
                    selectionTextRange.ApplyPropertyValue(Inline.TextDecorationsProperty, null);
                    selectionTextRange.ApplyPropertyValue(Inline.TextDecorationsProperty, decor.Clone());
                    //selectionTextRange.ApplyPropertyValue(Paragraph.LineHeightProperty, lineHeight);
                    selectionTextRange.ApplyPropertyValue(Paragraph.TextAlignmentProperty, textalign);
                    selectionTextRange.ApplyPropertyValue(Paragraph.FlowDirectionProperty, flow);
                }
            }
            //rtb.Document.LineHeight = lineHeight;
            // Reset the focus onto the richtextbox after selecting the font in a toolbar etc
            rtb.Focus();
            fileChanged = true;
        }
        private void Font_Click(object sender, RoutedEventArgs e)
        {
            font.Show();
        }
        private void Panel_MouseLeave(object sender, MouseEventArgs e)
        {
            rtb.VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Hidden;
            rtb.HorizontalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Hidden;
            menu.Visibility = Visibility.Collapsed;
        }
        private void Rtb_SelectionChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                if (rtb.Selection != null && rtb.Selection.Start.Paragraph != null)
                {
                    TextRange selectionTextRange = new TextRange(rtb.Selection.Start, rtb.Selection.End);
                    SolidColorBrush newBrush = (SolidColorBrush)selectionTextRange.GetPropertyValue(TextElement.ForegroundProperty);
                    font.ClrPcker_Font.SelectedColor = newBrush.Color;
                    newBrush = (SolidColorBrush)selectionTextRange.GetPropertyValue(TextElement.BackgroundProperty);
                    if (null == newBrush)
                    {
                        font.ClrPcker_Bg.SelectedColor = Colors.Transparent;
                    }
                    else
                    {
                        font.ClrPcker_Bg.SelectedColor = newBrush.Color;
                    }

                    font.fontSizeSlider.Value = (double)selectionTextRange.GetPropertyValue(TextElement.FontSizeProperty);

                    object fontfamily= selectionTextRange.GetPropertyValue(TextElement.FontFamilyProperty);
                    font.lstFamily.SelectedItem = fontfamily;

                    FontStyle fontsyle = (FontStyle)selectionTextRange.GetPropertyValue(TextElement.FontStyleProperty);
                    FontWeight fontweight = (FontWeight)selectionTextRange.GetPropertyValue(TextElement.FontWeightProperty);
                    FontStretch fonttretch = (FontStretch)selectionTextRange.GetPropertyValue(TextElement.FontStretchProperty);

                    FamilyTypeface fonttype = new FamilyTypeface()
                    {
                        Style = fontsyle,
                        Weight = fontweight,
                        Stretch = FontStretch
                    };
                    font.lstTypefaces.SelectedItem = fonttype;

                    font.txtSampleText.Selection.Start.Paragraph.TextAlignment = (TextAlignment)selectionTextRange.GetPropertyValue(Paragraph.TextAlignmentProperty);

                    //flow direction is working, see examples: https://stackoverflow.com/questions/7045676/wpf-how-does-flowdirection-righttoleft-change-a-string

                    font.txtSampleText.FlowDirection = (FlowDirection)selectionTextRange.GetPropertyValue(Paragraph.FlowDirectionProperty);

                    TextDecorationCollection temp = (TextDecorationCollection)selectionTextRange.GetPropertyValue(Inline.TextDecorationsProperty);
                    font.textrun.TextDecorations = null;
                    font.textrun.TextDecorations = temp.Clone();//this works, but is overriden on textrun.TextDecorations.Clear(); of UpdateStrikethrough in fontConfig 



                    font.Baseline.IsChecked = false;
                    font.OverLine.IsChecked = false;
                    font.Strikethrough.IsChecked = false;
                    font.Underline.IsChecked = false;
                    
                    font.UpdateStrikethrough();
                    
                    /*not working*/
                    //TODO: Fix decorators
                    foreach (TextDecoration decor in temp)
                    {
                        if (decor.Location.Equals(System.Windows.TextDecorationLocation.Baseline))
                        {
                            font.Baseline.IsChecked = true;
                        }
                        if (decor.Location.Equals(System.Windows.TextDecorationLocation.OverLine))
                        {
                            font.OverLine.IsChecked = true;
                        }
                        if (decor.Location.Equals(System.Windows.TextDecorationLocation.Strikethrough))
                        {
                            font.Strikethrough.IsChecked = true;
                        }
                        if (decor.Location.Equals(System.Windows.TextDecorationLocation.Underline))
                        {
                            font.Underline.IsChecked = true;
                        }
                    }
                }

            }
            catch (System.Exception)
            {
            }
        }
        private void Resettodefaults_Click(object sender, RoutedEventArgs e)
        {
            if (fileChanged)
            {
                switch (MessageBox.Show("There are unsaved Changes to: " + filepath + "\r\nDo you want to save?", "Save Changes?", MessageBoxButton.YesNoCancel, MessageBoxImage.Exclamation, MessageBoxResult.Cancel))
                {
                    case (MessageBoxResult.Yes):
                        {
                            Save(false);
                            Reset_Defaults();
                            break;
                        }
                    case MessageBoxResult.No:
                        {
                            Reset_Defaults();
                            break;
                        }
                    case MessageBoxResult.Cancel:
                        {
                            //do nothing
                            break;
                        }
                    default: break;
                }
            }
            else
            {
                Reset_Defaults();
            }
        }
        private void Reset_Defaults()
        {
            Load_default();
            grid.UpdateLayout();
            FixResizeTextbox();
        }
        private void Donate_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://google.com");
        }
        private void About_Click(object sender, RoutedEventArgs e)
        {
            string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            MessageBox.Show("Version: " + version);
        }
        private void Rtb_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.SystemKey.Equals(Key.LeftAlt) || e.Key.Equals(Key.LeftAlt) || e.SystemKey.Equals(Key.RightAlt) || e.Key.Equals(Key.RightAlt))
            {
                //TODO: fix
                menu.Visibility = Visibility.Visible;
            }
        }
        private void CharMap_Click(object sender, RoutedEventArgs e)
        {
            Process process = new Process();
            process.StartInfo.FileName = "charmap";
            //process.StartInfo.Arguments = arguments;
            //process.StartInfo.ErrorDialog = true;
            //process.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
            process.Start(); 
            //process.WaitForExit(1000 * 60 * 5);    // Wait up to five minutes.
        }
        private void Rtb_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            //creates a spam on first load, fixed on window_onLoad()
            fileChanged = true; 
        }
    }
    public class ShowMessageCommand : ICommand
    {
        public void Execute(object parameter)
        {
            //MessageBox.Show(parameter.ToString());
            foreach (Window item in Application.Current.Windows)
            {
                if (item.Title.Equals("SkinText"))
                {
                    if (item.Visibility == Visibility.Hidden)
                    {
                        item.Show();
                        item.Activate();
                    }
                    else
                    {
                        item.Hide();
                    }
                }
                
            }
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;
    }
}
