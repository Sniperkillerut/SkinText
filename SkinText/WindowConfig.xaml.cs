using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfAnimatedGif;
using XamlAnimatedGif;

namespace SkinText
{
    /// <summary>
    /// Lógica de interacción para WindowConfig.xaml
    /// </summary>
    public partial class WindowConfig : Window
    {
        private MainWindow par;
        public WindowConfig(MainWindow own)
        { 
            par = own;
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SolidColorBrush newBrush = (SolidColorBrush)par.splitter1.Background;
            ClrPcker_Background.SelectedColor = newBrush.Color;

            newBrush = (SolidColorBrush)par.window.Background;
            ClrPcker_Background2.SelectedColor = newBrush.Color;

            newBrush = (SolidColorBrush)par.rtb.Background;
            ClrPcker_Background3.SelectedColor = newBrush.Color;

            newBrush = null;
            if (par.imagepath.Length<4)
            {
                par.imagepath = "";
            }
            imagedir.Content = "Change Background";
            //imagedir.Content = par.imagepath.Substring(par.imagepath.LastIndexOf("\\") + 1);
            //imagedir.ToolTip = par.imagepath;


            imageopacityslider.Value = par.backgroundimg.Opacity * 100;
            textopacityslider.Value = par.rtb.Opacity * 100;
            windowopacityslider.Value = par.Opacity * 100;
        }
        private void CloseButt_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            par.Save_config();            
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
        private void bordersize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                par.borderSz = int.Parse(bordersize.Value.ToString());
                par.corner1.Width = par.corner1.Height = par.borderSz * 2;
                par.corner2.Width = par.corner2.Height = par.borderSz * 2;
                par.corner3.Width = par.corner3.Height = par.borderSz * 2;
                par.corner4.Width = par.corner4.Height = par.borderSz * 2;
                par.splitter1.Width = par.borderSz;
                par.splitter2.Height = par.borderSz;
                par.splitter3.Width = par.borderSz;
                par.splitter4.Height = par.borderSz;
                par.Rtb_SizeChanged(null, null);
            }
            catch (System.Exception)
            {
            }
        }
        private void slValue_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            RotateTransform rotateTransform = new RotateTransform(slValue.Value);
            par.grid.RenderTransformOrigin = new Point(0.5, 0.5);
            par.grid.RenderTransform = rotateTransform;
            rotateTransform = new RotateTransform(-slValue.Value);
            par.backgroundimg.RenderTransform = rotateTransform;

            //bugs graphics when resize enabled
            // not seeing the bug ¿?
        }
        private void ResizeCheckBoxChanged(object sender, RoutedEventArgs e)
        {
            if (resizecheck.IsChecked.Value)
            {
                par.corner1.Visibility = Visibility.Visible;
                par.corner2.Visibility = Visibility.Visible;
                par.corner3.Visibility = Visibility.Visible;
                par.corner4.Visibility = Visibility.Visible;
                par.splitter1.Visibility = Visibility.Visible;
                par.splitter2.Visibility = Visibility.Visible;
                par.splitter3.Visibility = Visibility.Visible;
                par.splitter4.Visibility = Visibility.Visible;
            }
            else
            {
                par.corner1.Visibility = Visibility.Collapsed;
                par.corner2.Visibility = Visibility.Collapsed;
                par.corner3.Visibility = Visibility.Collapsed;
                par.corner4.Visibility = Visibility.Collapsed;
                par.splitter1.Visibility = Visibility.Collapsed;
                par.splitter2.Visibility = Visibility.Collapsed;
                par.splitter3.Visibility = Visibility.Collapsed;
                par.splitter4.Visibility = Visibility.Collapsed;
            }
        }
        private void ClrPcker_Background_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            Color color = ClrPcker_Background.SelectedColor.Value;
            Brush brush = new SolidColorBrush(color);
            par.corner1.Fill = brush;
            par.corner2.Fill = brush;
            par.corner3.Fill = brush;
            par.corner4.Fill = brush;
            par.splitter1.Background = brush;
            par.splitter2.Background = brush;
            par.splitter3.Background = brush;
            par.splitter4.Background = brush;
        }

        public ImageSource BitmapFromUri(Uri source)
        {
            //TODO: move to library class
            //No longer in use, but usefull for future uses
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = source;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            bitmap.EndInit();
            return bitmap;
        }

        public void ImageClear()
        {
            par.backgroundimg.Source = null;
            WpfAnimatedGif.ImageBehavior.SetAnimatedSource(par.backgroundimg, null);
            XamlAnimatedGif.AnimationBehavior.SetSourceUri(par.backgroundimg, null);
            XamlAnimatedGif.AnimationBehavior.SetSourceStream(par.backgroundimg, null);
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
        }

        public void LoadImage(string imagepath)
        {
            if (File.Exists(imagepath))
            {
                var uri = new System.Uri(imagepath);
                if (Path.GetExtension(imagepath).ToLower() == ".gif")
                {
                    if (par.gifMethod == "RAM")
                    {
                        ImageSource bitmap = BitmapFromUri(uri);
                        ImageBehavior.SetAnimatedSource(par.backgroundimg, bitmap);
                        bitmap = null;
                    }
                    else
                    {//CpuMethod
                        AnimationBehavior.SetSourceUri(par.backgroundimg, uri);
                    }
                }
                else
                {//default (no gif animation)
                    ImageSource bitmap = BitmapFromUri(uri);
                    par.backgroundimg.Source = bitmap;
                    bitmap = null;
                }
                uri = null;
            }
        }

        private void imagedir_Click(object sender, RoutedEventArgs e)
        {
            //openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Multiselect = false,
                CheckFileExists = true,
                CheckPathExists = true,
                ValidateNames = true,
                RestoreDirectory = true,
                Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png, *.gif) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png; *.gif"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string imagepath = openFileDialog.FileName;
                    string newImagePath = par.path + "\\bgImg"+ Path.GetExtension(imagepath);//+ Path.GetFileName(imagepath);
                    ImageClear();
                    if (File.Exists(par.imagepath))
                    {
                        File.Delete(par.imagepath);
                    }
                    File.Copy(imagepath, newImagePath,true);

                    LoadImage(newImagePath);

                    //imagedir.Content = newImagePath.Substring(newImagePath.LastIndexOf("\\")+1);
                    //imagedir.ToolTip = newImagePath;
                    par.imagepath = newImagePath;
                    ClrPcker_Background2.SelectedColor = Colors.Transparent;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    ImageClear();
                    ClrPcker_Background2.SelectedColor = (Color)ColorConverter.ConvertFromString("#85949494");
                    //MessageBox.Show("Background image failed to load, try another image or try again later", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.ServiceNotification);
                }
            }
            else
            {
                ImageClear();
                par.imagepath = "";
                //imagedir.Content = par.imagepath.Substring(par.imagepath.LastIndexOf("\\") + 1);
                //imagedir.ToolTip = par.imagepath;
                ClrPcker_Background2.SelectedColor = (Color)ColorConverter.ConvertFromString("#85949494");
            }
        }
        private void ClrPcker_Background2_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            Color color = ClrPcker_Background2.SelectedColor.Value;
            Brush brush = new SolidColorBrush(color);
            par.window.Background = brush;
        }
        private void ClrPcker_Background3_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            Color color = ClrPcker_Background3.SelectedColor.Value;
            Brush brush = new SolidColorBrush(color);
            par.rtb.Background = brush;
            par.font.txtSampleText.Background = brush;
        }
        private void imageopacityslider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            par.backgroundimg.Opacity = imageopacityslider.Value/100;
        }
        private void textopacityslider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            par.rtb.Opacity = textopacityslider.Value/100;
        }
        private void windowopacityslider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            par.window.Opacity = windowopacityslider.Value/100;
        }
        private void readonly_Checked(object sender, RoutedEventArgs e)
        {
            if (@readonly.IsChecked.Value)
            {
                par.rtb.IsReadOnly = true;
                par.rtb.IsReadOnlyCaretVisible = false;
            }
            else
            {
                par.rtb.IsReadOnly = false;
                par.rtb.IsReadOnlyCaretVisible = true;
            }
        }
        private void spellcheck_Checked(object sender, RoutedEventArgs e)
        {
            if (spellcheck.IsChecked.Value)
            {
                par.rtb.SpellCheck.IsEnabled = true;
                int asd=par.rtb.SpellCheck.CustomDictionaries.Count;
                asd++;
            }
            else
            {
                par.rtb.SpellCheck.IsEnabled = false;
            }            
        }
        private void allwaysontop_Unchecked(object sender, RoutedEventArgs e)
        {
            if (allwaysontop.IsChecked.Value)
            {
                par.window.Topmost = true;
            }
            else
            {
                par.window.Topmost = false;
            }
        }
        private void taskbarvisible_Unchecked(object sender, RoutedEventArgs e)
        {
            if (taskbarvisible.IsChecked.Value)
            {
                par.window.ShowInTaskbar = true;
            }
            else
            {
                par.window.ShowInTaskbar = false;
                
            }
        }
        private void NotificationVisible_Unchecked(object sender, RoutedEventArgs e)
        {
            if (NotificationVisible.IsChecked.Value)
            {
                par.window.MyNotifyIcon.Visibility = Visibility.Visible;
            }
            else
            {
                par.window.MyNotifyIcon.Visibility = Visibility.Collapsed;
            }
        }

        private void GifMethodRAM_Checked(object sender, RoutedEventArgs e)
        {
            par.gifMethod = "RAM";
        }

        private void GifMethodCPU_Checked(object sender, RoutedEventArgs e)
        {
            par.gifMethod = "CPU";
        }
        
    }
}
