using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Microsoft.Win32;

namespace SkinText {

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public static class CustomMethods {
        private static string appDataPath;
        private static int autoSaveTimer = 5 * 60 * 1000;
        private static string currentSkin;
        private static bool fileChanged;
        private static string filepath;
        private static string filepathAsociated;
        private static string imagepath;
        private static string screenShotPath;
        private static string oldScreenShotPath;
        private static bool screenshotUpload;
        private static MainWindow mainW;
        public static string AppDataPath { get => appDataPath; set => appDataPath = value; }
        public static int AutoSaveTimer { get => autoSaveTimer; set => autoSaveTimer = value; }
        public static string CurrentSkin { get => currentSkin; set => currentSkin = value; }
        public static bool FileChanged { get => fileChanged; set => fileChanged = value; }
        public static string Filepath { get => filepath; set => filepath = value; }
        public static string FilepathAssociated { get => filepathAsociated; set => filepathAsociated = value; }

        public static string GAppPath {
            get {
                Assembly assm;
                Type at;
                Type at2;
                object[] r;
                object[] r2;
                // Get the .EXE assembly
                assm = Assembly.GetEntryAssembly();
                // Get a 'Type' of the AssemblyCompanyAttribute
                at = typeof(AssemblyCompanyAttribute);
                at2 = typeof(AssemblyTitleAttribute);
                // Get a collection of custom attributes from the .EXE assembly
                r = assm.GetCustomAttributes(at, false);
                r2 = assm.GetCustomAttributes(at2, false);
                // Get the Company Attribute
                AssemblyCompanyAttribute ct = ((AssemblyCompanyAttribute)(r[0]));
                AssemblyTitleAttribute ct2 = ((AssemblyTitleAttribute)(r2[0]));
                // Build the User App Data Path
                string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                //path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                path += @"\" + ct.Company;
                path += @"\" + ct2.Title;
                //path += @"\Default";
                //path += @"\" + assm.GetName().Name.ToString();
                //path += @"\" + assm.GetName().Version.ToString();
                return path;
            }
        }

        public static string Imagepath { get => imagepath; set => imagepath = value; }
        public static MainWindow MainW { get => mainW; set => mainW = value; }
        public static string ScreenShotPath { get => screenShotPath; set => screenShotPath = value; }
        public static string OldScreenShotPath { get => oldScreenShotPath; set => oldScreenShotPath = value; }
        public static bool ScreenshotUpload { get => screenshotUpload; set => screenshotUpload = value; }

        #region Bg Image

        /// <summary>
        /// Correct way to load an image and release it from the Hdd, also allows to unload it from memory effectively
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static ImageSource BitmapFromUri(Uri source) {
            //throws System.NotSupportedException but must be catched in LoadImage
            System.Windows.Media.Imaging.BitmapImage bitmap = new System.Windows.Media.Imaging.BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = source;
            bitmap.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
            bitmap.CreateOptions = System.Windows.Media.Imaging.BitmapCreateOptions.IgnoreImageCache;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }

        /// <summary>
        /// Clears, Disposes, Makes null, Unload, Frees from memory and HDD the loaded image
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.GC.Collect")]
        public static void ImageClear() {
            string oldpath = Imagepath;
            Imagepath = "";
            MainW.backgroundimg.Source = null;
            WpfAnimatedGif.ImageBehavior.SetAnimatedSource(MainW.backgroundimg, null);
            XamlAnimatedGif.AnimationBehavior.SetSourceUri(MainW.backgroundimg, null);
            XamlAnimatedGif.AnimationBehavior.SetSourceStream(MainW.backgroundimg, null);
            if (((SolidColorBrush)MainW.window.Background).Color.A == 0) {
                MainW.Conf.ClrPcker_MainWindowBackgroundColorBrush.SelectedColor = (Color)ColorConverter.ConvertFromString("#85949494");
            }
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
            try {
                if (File.Exists(oldpath)) {
                    File.Delete(oldpath);
                }
            }
            catch (Exception ex) {
#if DEBUG
                MessageBox.Show("DEBUG: "+ex.ToString());
                //throw;
#endif
            }
        }

        /// <summary>
        /// Will try to load <paramref name="imagepath"/> into <see cref="MainWindow.backgroundimg"/> and set <see cref="Imagepath"/> to either <paramref name="imagepath"/> or "" (<see cref="string.Empty"/>)
        /// </summary>
        /// <param name="imagepath"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static void LoadImage(string imagepath) {
            string newImagePath = AppDataPath + CurrentSkin + @"\bgImg" + Path.GetExtension(imagepath);//+ Path.GetFileName(imagepath);
            try {
                if (imagepath != newImagePath) {
                    ImageClear();
                    File.Copy(imagepath, newImagePath, true);
                    //imagepath = newImagePath;
                    //first copy the img to appdata, then load it
                }

                if (File.Exists(newImagePath)) {
                    Uri uri = new Uri(newImagePath);
                    if (Path.GetExtension(newImagePath).ToUpperInvariant() == ".GIF") {
                        if (MainW.Conf.GifMethodCPU.IsChecked.Value) {//CPU Method
                            XamlAnimatedGif.AnimationBehavior.SetSourceUri(MainW.backgroundimg, uri);
                        }
                        else {//RAM Method
                            ImageSource bitmap = BitmapFromUri(uri);
                            bitmap.Freeze();
                            WpfAnimatedGif.ImageBehavior.SetAnimatedSource(MainW.backgroundimg, bitmap);
                            bitmap = null;
                        }
                    }
                    else {//default (no gif animation)
                        ImageSource bitmap = BitmapFromUri(uri);
                        bitmap.Freeze();
                        MainW.backgroundimg.Source = bitmap;
                        bitmap = null;
                    }
                    //imagedir.Content = newImagePath.Substring(newImagePath.LastIndexOf("\\")+1);
                    //imagedir.ToolTip = newImagePath;
                    if (MainW.Conf.ClrPcker_MainWindowBackgroundColorBrush.SelectedColor.Value.A == 255) {
                        MainW.Conf.ClrPcker_MainWindowBackgroundColorBrush.SelectedColor = Color.Subtract(MainW.Conf.ClrPcker_MainWindowBackgroundColorBrush.SelectedColor.Value, Color.FromArgb(155, 0, 0, 0));
                    }
                    //MainW.WinConfig.ClrPcker_WindowBackground.SelectedColor = Colors.Transparent;
                    uri = null;
                    Imagepath = newImagePath; //set Global Imagepath
                }
                else {
                    throw new FileNotFoundException("Error: File not found", newImagePath);
                }
            }
            catch (Exception ex) {
                if (!string.IsNullOrWhiteSpace(newImagePath)) {
                    MessageBox.Show("Failed to Load Image:\r\n " + newImagePath, "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.ServiceNotification);
                }
                ImageClear();
#if DEBUG
                MessageBox.Show("DEBUG: "+ex.ToString());
                //throw;
#endif
            }
        }

        /// <summary>
        /// Calls a <see cref="OpenFileDialog"/> to select the image to load
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static void OpenImage() {
            OpenFileDialog openFileDialog = new OpenFileDialog() {
                Multiselect = false,
                CheckFileExists = true,
                CheckPathExists = true,
                ValidateNames = true,
                RestoreDirectory = true,
                Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png, *.gif) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png; *.gif"
            };
            //openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (openFileDialog.ShowDialog() == true) {
                LoadImage(openFileDialog.FileName);
            }
            else {
                ImageClear();
                //imagedir.Content = par.imagepath.Substring(par.imagepath.LastIndexOf("\\") + 1); // "" when imagepath is empty
                //imagedir.ToolTip = par.imagepath;
            }
        }

        /// <summary>
        /// Sets the <see cref="MainWindow.backgroundimg"/> opacity to <paramref name="opacity"/>
        /// </summary>
        /// <param name="opacity"></param>
        public static void WindowImageOpacity(double opacity) {
            MainW.backgroundimg.Opacity = opacity;
        }

        public static void BlurBGImage(double blurVal, bool gauss) {
            System.Windows.Media.Effects.BlurEffect blur = new System.Windows.Media.Effects.BlurEffect {
                Radius = blurVal
            };
            if (gauss) {
                blur.KernelType = System.Windows.Media.Effects.KernelType.Gaussian;
            }
            else {
                blur.KernelType = System.Windows.Media.Effects.KernelType.Box;
            }
            mainW.backgroundimg.Effect = blur;
        }

        #endregion Bg Image

        #region Config File

        /// <summary>
        /// Loads Default Values
        /// <para>Called from <see cref="MainWindow.Window_Loaded"/> and <see cref="ResetDefaults"/></para>
        /// </summary>
        public static void LoadDefault() {
            //window size
            MainW.window.Width = 525;
            MainW.window.Height = 350;

            //Window position
            MainW.Left = (SystemParameters.PrimaryScreenWidth / 2) - (MainW.Width / 2);
            MainW.Top = (SystemParameters.PrimaryScreenHeight / 2) - (MainW.Height / 2);

            //Text position
            System.Windows.Controls.Canvas.SetLeft(MainW.TitleBorder, 25);
            System.Windows.Controls.Canvas.SetTop(MainW.TitleBorder, 25);

            //Text Size
            MainW.TitleBorder.Width = 475;
            MainW.TitleBorder.Height = 300;

            //Text border corner radius
            MainW.Conf.CornerRadius1Slider.Value = 20;
            MainW.Conf.CornerRadius2Slider.Value = 20;
            MainW.Conf.CornerRadius3Slider.Value = 20;
            MainW.Conf.CornerRadius4Slider.Value = 20;

            //Text border max corner radius
            MainW.Conf.CornerMax.Value = 500;

            //Text border corner radius linked
            MainW.Conf.lockSlidersCheckbox.IsChecked = true;

            //Text border size
            MainW.Conf.bordersize.Value = 5;

            #region Legacy styles

            /*Application.Current.Resources["BorderColorBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#997E7E7E"));
            Application.Current.Resources["MainWindowBackgroundColorBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#85949494"));
            Application.Current.Resources["RTBBackgroundColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Transparent"));

            Application.Current.Resources["BackgroundColorBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C03A3A3A"));
            Application.Current.Resources["ButtonBackgroundColorBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#602C2C2C"));
            Application.Current.Resources["ButtonFrontColorBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("##FFE6E6E6"));
            Application.Current.Resources["ButtonBackgroundMouseOverColorBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#85949494"));
            Application.Current.Resources["ButtonBorderMouseOverColorBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFE6E6E6"));
            Application.Current.Resources["ButtonBackgroundCheckedColorBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#607E7E7E"));
            Application.Current.Resources["ButtonBorderCheckedColorBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF808080"));
            Application.Current.Resources["TextColorBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFE6E6E6"));

            Application.Current.Resources["FontPickBackgroundColorBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#AA606060"));
            Application.Current.Resources["FontPickTextColorBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFE6E6E6"));
            Application.Current.Resources["FontPickMouseOverBackgroundColorBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#20FFFFFF"));
            Application.Current.Resources["FontPickMouseOverBorderColorBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#90FFFFFF"));
            Application.Current.Resources["FontPickSelectedBackgroundColorBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#40FFFFFF"));
            Application.Current.Resources["FontPickSelectedBorderColorBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFFFF"));

            Application.Current.Resources["MenuBackgroundColorBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#aa535353"));
            Application.Current.Resources["MenuItem1BorderColorBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Transparent"));
            Application.Current.Resources["MenuItem2HighlightTextColorBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Black"));
            Application.Current.Resources["MenuItem2HighlightBorderColorBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#602C2C2C"));
            Application.Current.Resources["MenuItem2DisabledColorBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF2C2C2C"));
            */

            #endregion Legacy styles

            #region Legacy color reset method

            /*
            Application.Current.Resources["BorderColorBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF424242"));
            Application.Current.Resources["MainWindowBackgroundColorBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#55FFFFFF"));
            Application.Current.Resources["RTBBackgroundColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7DFFFFFF"));
            Application.Current.Resources["BackgroundColorBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF1C1C1C"));
            Application.Current.Resources["ButtonBackgroundColorBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF262626"));
            Application.Current.Resources["ButtonFrontColorBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFE6E6E6"));
            Application.Current.Resources["ButtonBackgroundMouseOverColorBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF00B5FF"));
            Application.Current.Resources["ButtonBorderMouseOverColorBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFE6E6E6"));
            Application.Current.Resources["ButtonBackgroundCheckedColorBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CD000000"));
            Application.Current.Resources["ButtonBorderCheckedColorBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF808080"));
            Application.Current.Resources["TextColorBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF808080"));

            Application.Current.Resources["FontPickBackgroundColorBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#23696969"));
            Application.Current.Resources["FontPickTextColorBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFE6E6E6"));
            Application.Current.Resources["FontPickMouseOverBackgroundColorBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF000000"));
            Application.Current.Resources["FontPickMouseOverBorderColorBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF00B5FF"));
            Application.Current.Resources["FontPickSelectedBackgroundColorBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8C000000"));
            Application.Current.Resources["FontPickSelectedBorderColorBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFDB6929"));

            Application.Current.Resources["MenuBackgroundColorBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C9000000"));
            Application.Current.Resources["MenuItem1BorderColorBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00FFFFFF"));
            Application.Current.Resources["MenuItem2HighlightTextColorBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF00B5FF"));
            Application.Current.Resources["MenuItem2HighlightBorderColorBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFDB6929"));
            Application.Current.Resources["MenuItem2DisabledColorBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#73696969"));

            //text bg color

            //Colors:
                MainW.Conf.ClrPcker_BorderColorBrush.SelectedColor = ((SolidColorBrush)MainW.TryFindResource("BorderColorBrush")).Color;
                MainW.Conf.ClrPcker_MainWindowBackgroundColorBrush.SelectedColor = ((SolidColorBrush)MainW.TryFindResource("MainWindowBackgroundColorBrush")).Color;
                MainW.Conf.ClrPcker_RTBBackgroundColorBrush.SelectedColor = ((SolidColorBrush)MainW.TryFindResource("RTBBackgroundColorBrush")).Color;
            //Advanced Colors:
                MainW.Conf.ClrPcker_BackgroundColorBrush.SelectedColor = ((SolidColorBrush)MainW.TryFindResource("BackgroundColorBrush")).Color;
                MainW.Conf.ClrPcker_ButtonBackgroundCheckedColorBrush.SelectedColor = ((SolidColorBrush)MainW.TryFindResource("ButtonBackgroundCheckedColorBrush")).Color;
                MainW.Conf.ClrPcker_ButtonBackgroundColorBrush.SelectedColor = ((SolidColorBrush)MainW.TryFindResource("ButtonBackgroundColorBrush")).Color;
                MainW.Conf.ClrPcker_ButtonBackgroundMouseOverColorBrush.SelectedColor = ((SolidColorBrush)MainW.TryFindResource("ButtonBackgroundMouseOverColorBrush")).Color;

                MainW.Conf.ClrPcker_ButtonBorderMouseOverColorBrush.SelectedColor = ((SolidColorBrush)MainW.TryFindResource("ButtonBorderMouseOverColorBrush")).Color;
                MainW.Conf.ClrPcker_ButtonBorderCheckedColorBrush.SelectedColor = ((SolidColorBrush)MainW.TryFindResource("ButtonBorderCheckedColorBrush")).Color;
                MainW.Conf.ClrPcker_ButtonFrontColorBrush.SelectedColor = ((SolidColorBrush)MainW.TryFindResource("ButtonFrontColorBrush")).Color;
                MainW.Conf.ClrPcker_FontPickBackgroundColorBrush.SelectedColor = ((SolidColorBrush)MainW.TryFindResource("FontPickBackgroundColorBrush")).Color;
                MainW.Conf.ClrPcker_FontPickMouseOverBackgroundColorBrush.SelectedColor = ((SolidColorBrush)MainW.TryFindResource("FontPickMouseOverBackgroundColorBrush")).Color;
                MainW.Conf.ClrPcker_FontPickMouseOverBorderColorBrush.SelectedColor = ((SolidColorBrush)MainW.TryFindResource("FontPickMouseOverBorderColorBrush")).Color;
                MainW.Conf.ClrPcker_FontPickSelectedBackgroundColorBrush.SelectedColor = ((SolidColorBrush)MainW.TryFindResource("FontPickSelectedBackgroundColorBrush")).Color;
                MainW.Conf.ClrPcker_FontPickSelectedBorderColorBrush.SelectedColor = ((SolidColorBrush)MainW.TryFindResource("FontPickSelectedBorderColorBrush")).Color;
                MainW.Conf.ClrPcker_MenuBackgroundColorBrush.SelectedColor = ((SolidColorBrush)MainW.TryFindResource("MenuBackgroundColorBrush")).Color;
                MainW.Conf.ClrPcker_MenuItem1BorderColorBrush.SelectedColor = ((SolidColorBrush)MainW.TryFindResource("MenuItem1BorderColorBrush")).Color;
                MainW.Conf.ClrPcker_MenuItem2DisabledColorBrush.SelectedColor = ((SolidColorBrush)MainW.TryFindResource("MenuItem2DisabledColorBrush")).Color;
                MainW.Conf.ClrPcker_MenuItem2HighlightBorderColorBrush.SelectedColor = ((SolidColorBrush)MainW.TryFindResource("MenuItem2HighlightBorderColorBrush")).Color;
                MainW.Conf.ClrPcker_MenuItem2HighlightTextColorBrush.SelectedColor = ((SolidColorBrush)MainW.TryFindResource("MenuItem2HighlightTextColorBrush")).Color;
                MainW.Conf.ClrPcker_TextColorBrush.SelectedColor = ((SolidColorBrush)MainW.TryFindResource("TextColorBrush")).Color;
                MainW.Conf.ClrPcker_FontPickTextColorBrush.SelectedColor = ((SolidColorBrush)MainW.TryFindResource("FontPickTextColorBrush")).Color;
            */

            #endregion Legacy color reset method

            MainW.Conf.ClrPcker_BorderColorBrush.SelectedColor = (Color)ColorConverter.ConvertFromString("#FF424242");
            MainW.Conf.ClrPcker_MainWindowBackgroundColorBrush.SelectedColor = (Color)ColorConverter.ConvertFromString("#55FFFFFF");
            MainW.Conf.ClrPcker_RTBBackgroundColorBrush.SelectedColor = (Color)ColorConverter.ConvertFromString("#7DFFFFFF");

            MainW.Conf.ClrPcker_BackgroundColorBrush.SelectedColor = (Color)ColorConverter.ConvertFromString("#CA1C1C1C");
            MainW.Conf.ClrPcker_ButtonBackgroundColorBrush.SelectedColor = (Color)ColorConverter.ConvertFromString("#FF262626");
            MainW.Conf.ClrPcker_ButtonFrontColorBrush.SelectedColor = (Color)ColorConverter.ConvertFromString("#FFE6E6E6");
            MainW.Conf.ClrPcker_ButtonBackgroundMouseOverColorBrush.SelectedColor = (Color)ColorConverter.ConvertFromString("#FF00B5FF");
            MainW.Conf.ClrPcker_ButtonBorderMouseOverColorBrush.SelectedColor = (Color)ColorConverter.ConvertFromString("#FFE6E6E6");
            MainW.Conf.ClrPcker_ButtonBackgroundCheckedColorBrush.SelectedColor = (Color)ColorConverter.ConvertFromString("#CD000000");
            MainW.Conf.ClrPcker_ButtonBorderCheckedColorBrush.SelectedColor = (Color)ColorConverter.ConvertFromString("#FF808080");
            MainW.Conf.ClrPcker_TextColorBrush.SelectedColor = (Color)ColorConverter.ConvertFromString("#FF808080");

            MainW.Conf.ClrPcker_FontPickBackgroundColorBrush.SelectedColor = (Color)ColorConverter.ConvertFromString("#23696969");
            MainW.Conf.ClrPcker_FontPickTextColorBrush.SelectedColor = (Color)ColorConverter.ConvertFromString("#FFE6E6E6");
            MainW.Conf.ClrPcker_FontPickMouseOverBackgroundColorBrush.SelectedColor = (Color)ColorConverter.ConvertFromString("#FF000000");
            MainW.Conf.ClrPcker_FontPickMouseOverBorderColorBrush.SelectedColor = (Color)ColorConverter.ConvertFromString("#FF00B5FF");
            MainW.Conf.ClrPcker_FontPickSelectedBackgroundColorBrush.SelectedColor = (Color)ColorConverter.ConvertFromString("#8C000000");
            MainW.Conf.ClrPcker_FontPickSelectedBorderColorBrush.SelectedColor = (Color)ColorConverter.ConvertFromString("#FFDB6929");

            MainW.Conf.ClrPcker_MenuBackgroundColorBrush.SelectedColor = (Color)ColorConverter.ConvertFromString("#C9000000");
            MainW.Conf.ClrPcker_MenuItem1BorderColorBrush.SelectedColor = (Color)ColorConverter.ConvertFromString("#00FFFFFF");
            MainW.Conf.ClrPcker_MenuItem2HighlightTextColorBrush.SelectedColor = (Color)ColorConverter.ConvertFromString("#FF00B5FF");
            MainW.Conf.ClrPcker_MenuItem2HighlightBorderColorBrush.SelectedColor = (Color)ColorConverter.ConvertFromString("#FFDB6929");
            MainW.Conf.ClrPcker_MenuItem2DisabledColorBrush.SelectedColor = (Color)ColorConverter.ConvertFromString("#73696969");

            //rotation angle
            MainW.Conf.slValue.Value = 0;

            //no file
            NewFile();

            //BG image clear
            //this makes Imagepath = "";
            ImageClear();

            //BgImage Blur
            MainW.Conf.imageBlurSlider.Value = 0;

            //opacity
            MainW.Conf.imageopacityslider.Value = 100;
            MainW.Conf.windowopacityslider.Value = 100;
            MainW.Conf.textopacityslider.Value = 100;

            //Auto Save Timer
            MainW.Conf.autosavetimersider.Value = 5;

            //window Checkboxes
            MainW.Conf.alwaysontop.IsChecked = false;
            MainW.Conf.taskbarvisible.IsChecked = true;
            MainW.Conf.NotificationVisible.IsChecked = true;
            MainW.Conf.ResizeVisible.IsChecked = true;
            MainW.Conf.BgBlur.IsChecked = true;
            MainW.Conf.toolsalwaysontop.IsChecked = true;

            //Image Checkboxes
            MainW.Conf.GifMethodCPU.IsChecked = true;
            MainW.Conf.ImageBlurGauss.IsChecked = true;

            //menu checkboxes
            MainW.LineWrapMenuItem.IsChecked = true;
            MainW.ToolBarMenuItem.IsChecked = false;
            MainW.Conf.ToolBarEnabled.IsChecked = false;

            //text checkboxes
            MainW.Conf.TextWrap.IsChecked = true;
            MainW.Conf.readOnlyCheck.IsChecked = false;
            MainW.Conf.spellcheck.IsChecked = false;
            MainW.Conf.resizecheck.IsChecked = false;
            MainW.Conf.FlipXButton.IsChecked = false;
            MainW.Conf.FlipYButton.IsChecked = false;
        }

        /// <summary>
        /// <para>Reads the skintext.ini and calls <see cref="ReadConfigLine"/> line by line to load the config</para>
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static void ReadConfig() {
            try {
                string configFile = AppDataPath + CurrentSkin + @"\skintext.ini";
                if (File.Exists(configFile)) {
                    using (StreamReader reader = new StreamReader(configFile, System.Text.Encoding.UTF8)) {
                        string currentLine;
                        string[] line;
                        while ((currentLine = reader.ReadLine()) != null) {
                            line = currentLine.Split('=');
                            line[0] = line[0].Trim();
                            line[0] = line[0].ToUpperInvariant();
                            if (!string.IsNullOrEmpty(line[0]) && line.Length > 1 && !string.IsNullOrWhiteSpace(line[1])) {
                                line[1] = line[1].Trim();
                                ReadConfigLine(line);
                            }
                        }
                    }
                }
            }
            catch (Exception ex) {
                // The appdata folders dont exist
                //can be first open, let default values
#if DEBUG
                MessageBox.Show("DEBUG: "+ex.ToString());
                //throw;
#endif
            }
        }

        /// <summary>
        /// Takes a line of the SkinText.ini config file and interpret it to load it's config values
        /// </summary>
        /// <param name="line">Line of the SkinText.ini config file</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public static void ReadConfigLine(string[] line) {
            if (line != null) {
                try {
                    double double1;
                    double double2;
                    bool bool1;
                    string string1;
                    string[] array;
                    switch (line[0]) {//Changed var names to use less variables CA1809
                        case "WINDOW_POSITION": {
                                array = line[1].Split(',');
                                if (double.TryParse(array[0], out double1) && //top
                                    double.TryParse(array[1], out double2))   //left
                                {
                                    MainW.window.Top = double1;
                                    MainW.window.Left = double2;
                                }
                                break;
                            }
                        case "WINDOW_SIZE": {
                                array = line[1].Split(',');
                                if (double.TryParse(array[0], out double1) && //window width
                                    double.TryParse(array[1], out double2))   //window height
                                {
                                    if (double1 > MainW.window.MinWidth && double1 < MainW.window.MaxWidth) {
                                        MainW.window.Width = double1;
                                    }
                                    if (double2 > MainW.window.MinHeight && double2 < MainW.window.MaxHeight) {
                                        MainW.window.Height = double2;
                                    }
                                }
                                break;
                            }
                        case "BORDER_SIZE": {
                                if (int.TryParse(line[1], out int int1)) //border size
                                {
                                    if (int1 <= MainW.Conf.bordersize.Maximum && int1 >= MainW.Conf.bordersize.Minimum) {
                                        MainW.Conf.bordersize.Value = int1;
                                    }
                                }
                                break;
                            }
                        case "TEXT_POSITION": {
                                array = line[1].Split(',');
                                if (double.TryParse(array[0], out double1) &&  //left
                                    double.TryParse(array[1], out double2))    //top
                                {
                                    System.Windows.Controls.Canvas.SetLeft(MainW.TitleBorder, double1);
                                    System.Windows.Controls.Canvas.SetTop(MainW.TitleBorder, double2);
                                    /*
                                    double borderSize = MainW.Conf.bordersize.Value;
                                     *if ((double1 < MainW.window.Width - (borderSize * 2 + 1)) && (double1 >= 0)){//left
                                         System.Windows.Controls.Canvas.SetLeft(MainW.TitleBorder, double1);
                                     }
                                     if ((double2 < MainW.window.Height - (borderSize * 2 + 1)) && (double2 >= 0)){//top
                                         System.Windows.Controls.Canvas.SetTop(MainW.TitleBorder, double2);
                                     }*/
                                }
                                break;
                            }
                        case "TEXT_SIZE": {
                                array = line[1].Split(',');
                                if (double.TryParse(array[0], out double1) &&  //width
                                    double.TryParse(array[1], out double2))    //height
                                {
                                    MainW.TitleBorder.Width = double1;
                                    MainW.TitleBorder.Height = double2;
                                    /*
                                    double borderSize = MainW.Conf.bordersize.Value;
                                     * if ((double1 < MainW.window.Width - (borderSize * 2 + 1)) && (double1 > 0)) {//width
                                        MainW.TitleBorder.Width = double1;
                                    }
                                    if ((double2 < MainW.window.Height - (borderSize * 2 + 1)) && (double2 > 0)) {//height
                                        MainW.TitleBorder.Height =double2;
                                    }*/
                                }
                                break;
                            }
                        case "TEXT_MAX_CORNER_RADIUS": {
                                if (double.TryParse(line[1], out double1)) {
                                    if (double1 >= 0 && double1 < 1000000) {
                                        MainW.Conf.CornerMax.Value = Convert.ToDecimal(double1);
                                    }
                                }
                                break;
                            }
                        case "TEXT_CORNER_RADIUS_LOCKED": {
                                if (bool.TryParse(line[1], out bool1)) {
                                    MainW.Conf.lockSlidersCheckbox.IsChecked = bool1;
                                }
                                break;
                            }
                        case "TEXT_CORNER_RADIUS": {
                                array = line[1].Split(',');
                                if (double.TryParse(array[0], out double1) &&  //left-top
                                    double.TryParse(array[1], out double2) &&  //right-top
                                    double.TryParse(array[2], out double double3) &&  //right-bottom
                                    double.TryParse(array[3], out double double4))    //left-bottom
                                {
                                    if (double1 >= 0 && double1 <= Convert.ToDouble(MainW.Conf.CornerMax.Value, System.Globalization.CultureInfo.InvariantCulture)) {
                                        MainW.Conf.CornerRadius1Slider.Value = double1;
                                    }
                                    if (double2 >= 0 && double2 <= Convert.ToDouble(MainW.Conf.CornerMax.Value, System.Globalization.CultureInfo.InvariantCulture)) {
                                        MainW.Conf.CornerRadius2Slider.Value = double2;
                                    }
                                    if (double3 >= 0 && double3 <= Convert.ToDouble(MainW.Conf.CornerMax.Value, System.Globalization.CultureInfo.InvariantCulture)) {
                                        MainW.Conf.CornerRadius3Slider.Value = double3;
                                    }
                                    if (double4 >= 0 && double4 <= Convert.ToDouble(MainW.Conf.CornerMax.Value, System.Globalization.CultureInfo.InvariantCulture)) {
                                        MainW.Conf.CornerRadius4Slider.Value = double4;
                                    }
                                }
                                break;
                            }
                        case "FILE": {
                                string1 = line[1].Trim(); //fileName
                                if (!string1.Contains("\\") && !string.IsNullOrEmpty(string1)) {//if is a relative path, use current .exe path to find it //not necesary as it will never be relative, but for historical pruposes
                                    string1 = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + string1;
                                }
                                ReadFile(string1); //this sets Global Filepath
                                break;
                            }
                        case "RESIZE_ENABLED": {
                                if (bool.TryParse(line[1], out bool1)) //resize checked
                                {
                                    MainW.Conf.resizecheck.IsChecked = bool1;
                                }
                                break;
                            }
                        case "BORDER_COLOR": {
                                MainW.Conf.ClrPcker_BorderColorBrush.SelectedColor = (Color)ColorConverter.ConvertFromString(line[1]);
                                break;
                            }
                        case "WINDOW_COLOR": {
                                MainW.Conf.ClrPcker_MainWindowBackgroundColorBrush.SelectedColor = (Color)ColorConverter.ConvertFromString(line[1]);
                                break;
                            }
                        case "TEXT_BG_COLOR": {
                                MainW.Conf.ClrPcker_RTBBackgroundColorBrush.SelectedColor = (Color)ColorConverter.ConvertFromString(line[1]);
                                break;
                            }
                        case "ROTATION": {
                                if (double.TryParse(line[1], out double1)) //angle
                                {
                                    if (double1 <= MainW.Conf.slValue.Maximum && double1 >= MainW.Conf.slValue.Minimum) {
                                        MainW.Conf.slValue.Value = double1;
                                    }
                                }
                                break;
                            }
                        case "GIF_USES_RAM": {//GIF rendering method
                                if (bool.TryParse(line[1], out bool1)) {// true = RAM //DEFAULT = false (Use CPU)
                                    MainW.Conf.GifMethodRAM.IsChecked = bool1;
                                }
                                break;
                            }
                        case "BG_IMAGE": {//(always after GIF method)
                                string1 = line[1].Trim();
                                if (!string1.Contains("\\") && !string.IsNullOrEmpty(string1)) {//if is a relative path, use current .exe path to find it //not necesary as it will never be relative, but for historical pruposes
                                    string1 = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + string1;
                                }
                                LoadImage(string1);//this sets Global Imagepath
                                break;
                            }
                        case "IMAGE_OPACITY": {
                                if (double.TryParse(line[1], out double1)) {
                                    if (double1 <= MainW.Conf.imageopacityslider.Maximum && double1 >= MainW.Conf.imageopacityslider.Minimum) {
                                        MainW.Conf.imageopacityslider.Value = double1;
                                    }
                                }
                                break;
                            }
                        case "TEXT_OPACITY": {
                                if (double.TryParse(line[1], out double1)) {
                                    if (double1 <= MainW.Conf.textopacityslider.Maximum && double1 >= MainW.Conf.textopacityslider.Minimum) {
                                        MainW.Conf.textopacityslider.Value = double1;
                                    }
                                }
                                break;
                            }
                        case "WINDOW_OPACITY": {
                                if (double.TryParse(line[1], out double1)) {
                                    if (double1 <= MainW.Conf.windowopacityslider.Maximum && double1 >= MainW.Conf.windowopacityslider.Minimum) {
                                        MainW.Conf.windowopacityslider.Value = double1;
                                    }
                                }
                                break;
                            }
                        case "READ_ONLY": {
                                if (bool.TryParse(line[1], out bool1)) {
                                    MainW.Conf.readOnlyCheck.IsChecked = bool1;
                                }
                                break;
                            }
                        case "SPELL_CHECK": {
                                if (bool.TryParse(line[1], out bool1)) {
                                    MainW.Conf.spellcheck.IsChecked = bool1;
                                }
                                break;
                            }
                        case "ALWAYS_ON_TOP": {
                                if (bool.TryParse(line[1], out bool1)) {
                                    MainW.Conf.alwaysontop.IsChecked = bool1;
                                }
                                break;
                            }
                        case "TASKBAR_ICON": {
                                if (bool.TryParse(line[1], out bool1)) {
                                    MainW.Conf.taskbarvisible.IsChecked = bool1;
                                }
                                break;
                            }
                        case "NOTIFICATION_ICON": {
                                if (bool.TryParse(line[1], out bool1)) {
                                    MainW.Conf.NotificationVisible.IsChecked = bool1;
                                }
                                break;
                            }
                        case "RESIZE_VISIBLE": {
                                if (bool.TryParse(line[1], out bool1)) {
                                    MainW.Conf.ResizeVisible.IsChecked = bool1;
                                }
                                break;
                            }
                        case "FLIP_RTB": {//flip rich text box (rendertransform)
                                array = line[1].Split(',');
                                if (bool.TryParse(array[0], out bool1) &&    // X
                                    bool.TryParse(array[1], out bool bool2)) // Y
                                {
                                    MainW.Conf.FlipXButton.IsChecked = bool1;
                                    MainW.Conf.FlipYButton.IsChecked = bool2;
                                }
                                break;
                            }
                        case "LINE_WRAP": {//Line Wrapping (Default on)
                                if (bool.TryParse(line[1], out bool1)) {
                                    MainW.LineWrapMenuItem.IsChecked = bool1;
                                    MainW.Conf.TextWrap.IsChecked = bool1;
                                }
                                break;
                            }
                        ////////////////////////////////////
                        case "BACKGROUNDCOLORBRUSH": {
                                MainW.Conf.ClrPcker_BackgroundColorBrush.SelectedColor = (Color)ColorConverter.ConvertFromString(line[1]);
                                break;
                            }
                        case "BUTTONBACKGROUNDCHECKEDCOLORBRUSH": {
                                MainW.Conf.ClrPcker_ButtonBackgroundCheckedColorBrush.SelectedColor = (Color)ColorConverter.ConvertFromString(line[1]);
                                break;
                            }
                        case "BUTTONBORDERCHECKEDCOLORBRUSH": {
                                MainW.Conf.ClrPcker_ButtonBorderCheckedColorBrush.SelectedColor = (Color)ColorConverter.ConvertFromString(line[1]);
                                break;
                            }
                        case "BUTTONBACKGROUNDCOLORBRUSH": {
                                MainW.Conf.ClrPcker_ButtonBackgroundColorBrush.SelectedColor = (Color)ColorConverter.ConvertFromString(line[1]);
                                break;
                            }
                        case "BUTTONFRONTCOLORBRUSH": {
                                MainW.Conf.ClrPcker_ButtonFrontColorBrush.SelectedColor = (Color)ColorConverter.ConvertFromString(line[1]);
                                break;
                            }
                        case "BUTTONBACKGROUNDMOUSEOVERCOLORBRUSH": {
                                MainW.Conf.ClrPcker_ButtonBackgroundMouseOverColorBrush.SelectedColor = (Color)ColorConverter.ConvertFromString(line[1]);
                                break;
                            }
                        case "BUTTONBORDERMOUSEOVERCOLORBRUSH": {
                                MainW.Conf.ClrPcker_ButtonBorderMouseOverColorBrush.SelectedColor = (Color)ColorConverter.ConvertFromString(line[1]);
                                break;
                            }
                        case "FONTPICKBACKGROUNDCOLORBRUSH": {
                                MainW.Conf.ClrPcker_FontPickBackgroundColorBrush.SelectedColor = (Color)ColorConverter.ConvertFromString(line[1]);
                                break;
                            }
                        case "FONTPICKMOUSEOVERBACKGROUNDCOLORBRUSH": {
                                MainW.Conf.ClrPcker_FontPickMouseOverBackgroundColorBrush.SelectedColor = (Color)ColorConverter.ConvertFromString(line[1]);
                                break;
                            }
                        case "FONTPICKMOUSEOVERBORDERCOLORBRUSH": {
                                MainW.Conf.ClrPcker_FontPickMouseOverBorderColorBrush.SelectedColor = (Color)ColorConverter.ConvertFromString(line[1]);
                                break;
                            }
                        case "FONTPICKSELECTEDBACKGROUNDCOLORBRUSH": {
                                MainW.Conf.ClrPcker_FontPickSelectedBackgroundColorBrush.SelectedColor = (Color)ColorConverter.ConvertFromString(line[1]);
                                break;
                            }
                        case "FONTPICKSELECTEDBORDERCOLORBRUSH": {
                                MainW.Conf.ClrPcker_FontPickSelectedBorderColorBrush.SelectedColor = (Color)ColorConverter.ConvertFromString(line[1]);
                                break;
                            }
                        case "MENUBACKGROUNDCOLORBRUSH": {
                                MainW.Conf.ClrPcker_MenuBackgroundColorBrush.SelectedColor = (Color)ColorConverter.ConvertFromString(line[1]);
                                break;
                            }
                        case "MENUITEM1BORDERCOLORBRUSH": {
                                MainW.Conf.ClrPcker_MenuItem1BorderColorBrush.SelectedColor = (Color)ColorConverter.ConvertFromString(line[1]);
                                break;
                            }
                        case "MENUITEM2DISABLEDCOLORBRUSH": {
                                MainW.Conf.ClrPcker_MenuItem2DisabledColorBrush.SelectedColor = (Color)ColorConverter.ConvertFromString(line[1]);
                                break;
                            }
                        case "MENUITEM2HIGHLIGHTBORDERCOLORBRUSH": {
                                MainW.Conf.ClrPcker_MenuItem2HighlightBorderColorBrush.SelectedColor = (Color)ColorConverter.ConvertFromString(line[1]);
                                break;
                            }
                        case "MENUITEM2HIGHLIGHTTEXTCOLORBRUSH": {
                                MainW.Conf.ClrPcker_MenuItem2HighlightTextColorBrush.SelectedColor = (Color)ColorConverter.ConvertFromString(line[1]);
                                break;
                            }
                        case "TEXTCOLORBRUSH": {
                                MainW.Conf.ClrPcker_TextColorBrush.SelectedColor = (Color)ColorConverter.ConvertFromString(line[1]);
                                break;
                            }
                        case "FONTPICKTEXTCOLORBRUSH": {
                                MainW.Conf.ClrPcker_FontPickTextColorBrush.SelectedColor = (Color)ColorConverter.ConvertFromString(line[1]);
                                break;
                            }
                        case "IMG_BLUR_VAL": {
                                if (double.TryParse(line[1], out double1)) //angle
                                {
                                    if (double1 <= MainW.Conf.imageBlurSlider.Maximum && double1 >= MainW.Conf.imageBlurSlider.Minimum) {
                                        MainW.Conf.imageBlurSlider.Value = double1;
                                    }
                                }
                                break;
                            }
                        case "IMG_BLUR_BOX": {
                                if (bool.TryParse(line[1], out bool1)) {// true = Gauss (DEFAULT) false = Box
                                    MainW.Conf.ImageBlurBox.IsChecked = bool1;
                                }
                                break;
                            }
                        case "WINDOW_BLUR": {
                                if (bool.TryParse(line[1], out bool1)) {
                                    MainW.Conf.BgBlur.IsChecked = bool1;
                                }
                                break;
                            }
                        case "TOOLBAR_ENABLED": {
                                if (bool.TryParse(line[1], out bool1)) {
                                    MainW.ToolBarMenuItem.IsChecked = bool1;
                                    MainW.Conf.ToolBarEnabled.IsChecked = bool1;
                                }
                                break;
                            }
                        case "TOOLS_TOP": {
                                if (bool.TryParse(line[1], out bool1)) {
                                    MainW.Conf.toolsalwaysontop.IsChecked = bool1;
                                }
                                break;
                            }
                        case "AUTOSAVE_ENABLED": {
                                if (bool.TryParse(line[1], out bool1)) {
                                    AutoSaveEnabled = bool1;
                                }
                                break;
                            }
                        case "AUTOSAVE_TIMER": {
                                if (double.TryParse(line[1], out double1)) {
                                    if (double1 <= MainW.Conf.autosavetimersider.Maximum && double1 >= MainW.Conf.autosavetimersider.Minimum) {
                                        MainW.Conf.autosavetimersider.Value = double1;
                                    }
                                }
                                break;
                            }
                        default: {
#if DEBUG
                                MessageBox.Show("Not recognized:  \"" + line[0] + "\" in SkinConfig file");
#endif
                                break;
                            }
                    }
                }
                catch (Exception ex) {
                    //System.FormatException catched from COLOR converters
#if DEBUG
                    MessageBox.Show("DEBUG: "+ex.ToString());
                    //throw;
#endif
                }
            }
        }

        /// <summary>
        /// Saves SkinText config into SkinText.ini in /appdata
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static void SaveConfig() {// had to do this: https://msdn.microsoft.com/library/ms182334.aspx
            FileStream fs = null;
            try {
                string configFile = AppDataPath + CurrentSkin + @"\skintext.ini";
                Directory.CreateDirectory(AppDataPath + CurrentSkin);//!Exists ? create it : ignore
                fs = new FileStream(configFile, FileMode.Create, FileAccess.Write);
                using (TextWriter writer = new StreamWriter(fs, System.Text.Encoding.UTF8)) {
                    fs = null; //is no longer needed
                    string data;

                    //window_position
                    data = "window_position = " +
                        Math.Truncate(MainW.window.Top).ToString(System.Globalization.CultureInfo.InvariantCulture) + " , " +
                         Math.Truncate(MainW.window.Left).ToString(System.Globalization.CultureInfo.InvariantCulture);
                    writer.WriteLine(data);

                    //window_size
                    data = "window_size = " +
                         Math.Truncate(MainW.window.Width).ToString(System.Globalization.CultureInfo.InvariantCulture) + " , " +
                         Math.Truncate(MainW.window.Height).ToString(System.Globalization.CultureInfo.InvariantCulture);
                    writer.WriteLine(data);

                    //border_size
                    data = "border_size = " +
                        Math.Truncate(MainW.Conf.bordersize.Value).ToString(System.Globalization.CultureInfo.InvariantCulture);
                    writer.WriteLine(data);

                    //text_size
                    data = "text_size = " +
                        Math.Truncate(MainW.TitleBorder.Width).ToString(System.Globalization.CultureInfo.InvariantCulture) + " , " +
                        Math.Truncate(MainW.TitleBorder.Height).ToString(System.Globalization.CultureInfo.InvariantCulture);
                    writer.WriteLine(data);

                    //text_position
                    data = "text_position = " +
                        Math.Truncate(System.Windows.Controls.Canvas.GetLeft(MainW.TitleBorder)).ToString(System.Globalization.CultureInfo.InvariantCulture) + " , " +
                        Math.Truncate(System.Windows.Controls.Canvas.GetTop(MainW.TitleBorder)).ToString(System.Globalization.CultureInfo.InvariantCulture);
                    writer.WriteLine(data);

                    //text_corner_radius_locked
                    data = "text_corner_radius_locked = " +
                        MainW.Conf.lockSlidersCheckbox.IsChecked.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    writer.WriteLine(data);

                    //text_max_corner_radius
                    data = "text_max_corner_radius = " +
                        MainW.Conf.CornerMax.Value.ToString();
                    writer.WriteLine(data);

                    //text_corner_radius
                    data = "text_corner_radius = " +
                    MainW.Conf.CornerRadius1Slider.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) + " , " +
                    MainW.Conf.CornerRadius2Slider.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) + " , " +
                    MainW.Conf.CornerRadius3Slider.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) + " , " +
                    MainW.Conf.CornerRadius4Slider.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    writer.WriteLine(data);

                    //filetry

                    #region relative Path (Disabled)

                    /*
                        //To store as relative Path
                        int index = filepath.LastIndexOf("\\");
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
                        }
                    */

                    #endregion relative Path (Disabled)

                    data = "file = " + Filepath;
                    writer.WriteLine(data);

                    //resize_enabled
                    data = "resize_enabled = " + MainW.Conf.resizecheck.IsChecked.Value.ToString();
                    writer.WriteLine(data);

                    //border_color
                    data = "border_color = " + MainW.Conf.ClrPcker_BorderColorBrush.SelectedColor.ToString();
                    writer.WriteLine(data);

                    //border_color
                    data = "window_color = " + MainW.Conf.ClrPcker_MainWindowBackgroundColorBrush.SelectedColor.ToString();
                    writer.WriteLine(data);

                    //border_color
                    data = "text_bg_color = " + MainW.Conf.ClrPcker_RTBBackgroundColorBrush.SelectedColor.ToString();
                    writer.WriteLine(data);

                    //rotation
                    data = "rotation = " + MainW.Conf.slValue.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    writer.WriteLine(data);

                    //GIF Method
                    data = "gif_uses_ram = " + MainW.Conf.GifMethodRAM.IsChecked.Value.ToString();
                    writer.WriteLine(data);

                    //bg_image (always after GIF method)

                    #region relative Path (Disabled)

                    /*
                        //To store as relative Path
                        int index = imagepath.LastIndexOf("\\");
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
                        }
                    */

                    #endregion relative Path (Disabled)

                    data = "bg_image = " + Imagepath;
                    writer.WriteLine(data);

                    //image_opacity
                    data = "image_opacity = " + MainW.Conf.imageopacityslider.Value;
                    writer.WriteLine(data);

                    //text_opacity
                    data = "text_opacity = " + MainW.Conf.textopacityslider.Value;
                    writer.WriteLine(data);

                    //window_opacity
                    data = "window_opacity = " + MainW.Conf.windowopacityslider.Value;
                    writer.WriteLine(data);

                    //read_only
                    data = "read_only = " + MainW.Conf.readOnlyCheck.IsChecked.Value.ToString();
                    writer.WriteLine(data);

                    //spell_check
                    data = "spell_check = " + MainW.Conf.spellcheck.IsChecked.Value.ToString();
                    writer.WriteLine(data);

                    //always_on_top
                    data = "always_on_top = " + MainW.Conf.alwaysontop.IsChecked.Value.ToString();
                    writer.WriteLine(data);

                    //taskbar_icon
                    data = "taskbar_icon = " + MainW.Conf.taskbarvisible.IsChecked.Value.ToString();
                    writer.WriteLine(data);

                    //notification_icon
                    data = "notification_icon = " + MainW.Conf.NotificationVisible.IsChecked.Value.ToString();
                    writer.WriteLine(data);

                    //ResizeVisible
                    data = "resize_visible = " + MainW.Conf.ResizeVisible.IsChecked.Value.ToString();
                    writer.WriteLine(data);

                    //Render transform flip
                    string x = MainW.Conf.FlipXButton.IsChecked.Value.ToString();
                    string y = MainW.Conf.FlipYButton.IsChecked.Value.ToString();
                    //two methods for getting the variables:
                    //method 1 string y = rtb.RenderTransform.Value.M22.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    //method 2 string y =((System.Windows.Media.ScaleTransform)rtb.RenderTransform).ScaleY.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    data = "flip_rtb = " + x + " , " + y;
                    writer.WriteLine(data);

                    //Line Wrap
                    data = "line_wrap = " + MainW.LineWrapMenuItem.IsChecked.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    writer.WriteLine(data);

                    //Image Blur Value
                    data = "img_blur_val = " + MainW.Conf.imageBlurSlider.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    writer.WriteLine(data);

                    //Image Blur Mehod (Gauss, Box)
                    data = "img_blur_Box = " + MainW.Conf.ImageBlurBox.IsChecked.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    writer.WriteLine(data);

                    //Window Blur Enabled
                    data = "window_blur = " + MainW.Conf.BgBlur.IsChecked.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    writer.WriteLine(data);

                    //ToolBar Enabled
                    data = "toolbar_enabled = " + MainW.ToolBarMenuItem.IsChecked.ToString();
                    writer.WriteLine(data);

                    //////////////////////////////////////
                    //ADVANCED
                    data = "BackgroundColorBrush = " + MainW.Conf.ClrPcker_BackgroundColorBrush.SelectedColor.ToString();
                    writer.WriteLine(data);
                    data = "ButtonBackgroundCheckedColorBrush = " + MainW.Conf.ClrPcker_ButtonBackgroundCheckedColorBrush.SelectedColor.ToString();
                    writer.WriteLine(data);
                    data = "ButtonBorderCheckedColorBrush = " + MainW.Conf.ClrPcker_ButtonBorderCheckedColorBrush.SelectedColor.ToString();
                    writer.WriteLine(data);
                    data = "ButtonBackgroundColorBrush = " + MainW.Conf.ClrPcker_ButtonBackgroundColorBrush.SelectedColor.ToString();
                    writer.WriteLine(data);
                    data = "ButtonFrontColorBrush = " + MainW.Conf.ClrPcker_ButtonFrontColorBrush.SelectedColor.ToString();
                    writer.WriteLine(data);
                    data = "ButtonBorderMouseOverColorBrush = " + MainW.Conf.ClrPcker_ButtonBorderMouseOverColorBrush.SelectedColor.ToString();
                    writer.WriteLine(data);
                    data = "ButtonBackgroundMouseOverColorBrush = " + MainW.Conf.ClrPcker_ButtonBackgroundMouseOverColorBrush.SelectedColor.ToString();
                    writer.WriteLine(data);
                    data = "FontPickBackgroundColorBrush = " + MainW.Conf.ClrPcker_FontPickBackgroundColorBrush.SelectedColor.ToString();
                    writer.WriteLine(data);
                    data = "FontPickMouseOverBackgroundColorBrush = " + MainW.Conf.ClrPcker_FontPickMouseOverBackgroundColorBrush.SelectedColor.ToString();
                    writer.WriteLine(data);
                    data = "FontPickMouseOverBorderColorBrush = " + MainW.Conf.ClrPcker_FontPickMouseOverBorderColorBrush.SelectedColor.ToString();
                    writer.WriteLine(data);
                    data = "FontPickSelectedBackgroundColorBrush = " + MainW.Conf.ClrPcker_FontPickSelectedBackgroundColorBrush.SelectedColor.ToString();
                    writer.WriteLine(data);
                    data = "FontPickSelectedBorderColorBrush = " + MainW.Conf.ClrPcker_FontPickSelectedBorderColorBrush.SelectedColor.ToString();
                    writer.WriteLine(data);
                    data = "MenuBackgroundColorBrush = " + MainW.Conf.ClrPcker_MenuBackgroundColorBrush.SelectedColor.ToString();
                    writer.WriteLine(data);
                    data = "MenuItem1BorderColorBrush = " + MainW.Conf.ClrPcker_MenuItem1BorderColorBrush.SelectedColor.ToString();
                    writer.WriteLine(data);
                    data = "MenuItem2DisabledColorBrush = " + MainW.Conf.ClrPcker_MenuItem2DisabledColorBrush.SelectedColor.ToString();
                    writer.WriteLine(data);
                    data = "MenuItem2HighlightBorderColorBrush = " + MainW.Conf.ClrPcker_MenuItem2HighlightBorderColorBrush.SelectedColor.ToString();
                    writer.WriteLine(data);
                    data = "MenuItem2HighlightTextColorBrush = " + MainW.Conf.ClrPcker_MenuItem2HighlightTextColorBrush.SelectedColor.ToString();
                    writer.WriteLine(data);
                    data = "TextColorBrush = " + MainW.Conf.ClrPcker_TextColorBrush.SelectedColor.ToString();
                    writer.WriteLine(data);
                    data = "FontPickTextColorBrush = " + MainW.Conf.ClrPcker_FontPickTextColorBrush.SelectedColor.ToString();
                    writer.WriteLine(data);

                    data = "tools_top = " + MainW.Conf.toolsalwaysontop.IsChecked.Value.ToString();
                    writer.WriteLine(data);

                    data = "autosave_enabled = " + AutoSaveEnabled.ToString();
                    writer.WriteLine(data);
                    data = "autosave_timer = " + MainW.Conf.autosavetimersider.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    writer.WriteLine(data);
                }

                //to hide file: but exception from FileMode.Create
                //FileInfo info;
                //info = new FileInfo("skintext.ini");
                //info.Attributes = FileAttributes.Hidden;

            }
            catch (Exception ex) {
#if DEBUG
                MessageBox.Show("DEBUG: "+ex.ToString());
                //throw;
#endif
            }
            finally {
                fs?.Dispose();
            }
        }

        #endregion Config File

        #region File

        /// <summary>
        /// <para>Sets internal vars ready for empty file usage</para>
        /// Called from <see cref="MainWindow.NewCommand_Executed"/>
        /// </summary>
        public static void NewFile() {
            Filepath = "";
            MainW.MenuFileName.Header = "";
            MainW.rtb.Document = new FlowDocument() {
                LineStackingStrategy = LineStackingStrategy.BlockLineHeight
            };
            FileChanged = false;
        }

        /// <summary>
        /// <para>Calls an <see cref="OpenFileDialog"/> then calls <see cref="ReadFile"/></para>
        /// Called from <see cref="MainWindow.OpenCommand_Executed"/>
        /// </summary>
        public static void OpenFile() {
            OpenFileDialog openFileDialog = new OpenFileDialog() {
                Multiselect = false,
                CheckFileExists = true,
                CheckPathExists = true,
                ValidateNames = true,
                RestoreDirectory = true,
                Filter = "Text files (*.txt, *.rtf, *.xaml, *.xamlp) | *.txt; *.rtf; *.xaml; *.xamlp"
            };
            if (openFileDialog.ShowDialog() == true) {
                ReadFile(openFileDialog.FileName);
            }
        }

        /// <summary>
        /// <para>
        /// Checks if <paramref name="pathToFile"/> is valid and then loads it to <see cref="MainWindow.rtb"/>
        /// </para>
        /// Called from <see cref="OpenFile"/> and <see cref="ReadConfig"/>
        /// </summary>
        /// <param name="pathToFile"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static void ReadFile(string pathToFile) {
            try {
                if (File.Exists(pathToFile)) {
                    TextRange range = new TextRange(MainW.rtb.Document.ContentStart, MainW.rtb.Document.ContentEnd);
                    using (FileStream fStream = new FileStream(pathToFile, FileMode.OpenOrCreate)) {
                        switch (Path.GetExtension(pathToFile).ToUpperInvariant()) {
                            case (".RTF"): {
                                    range.Load(fStream, DataFormats.Rtf);
                                    break;
                                }
                            case (".TXT"): {
                                    range.Load(fStream, DataFormats.Text);
                                    break;
                                }
                            case (".XAML"): {
                                    range.Load(fStream, DataFormats.Xaml);
                                    break;
                                }
                            case (".XAMLP"): {
                                    range.Load(fStream, DataFormats.XamlPackage);
                                    break;
                                }
                            default: {//NOTE: if no format open as txt, or should throw error?
                                    range.Load(fStream, DataFormats.Text);
                                    break;
                                }
                        }
                        //set global vars
                        Filepath = pathToFile;
                        MainW.MenuFileName.Header = Path.GetFileName(pathToFile);
                        FileChanged = false;
                        MainW.rtb.Document.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;
                    }
                }
                else {
                    throw new FileNotFoundException("Error: File not found", pathToFile);
                }
            }
            catch (Exception ex) {
                if (!string.IsNullOrWhiteSpace(pathToFile)) {
                    MessageBox.Show("Failed to Open File:\r\n " + pathToFile, "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.ServiceNotification);
                }
                NewFile();
#if DEBUG
                MessageBox.Show("DEBUG: "+ex.ToString());
                //throw;
#endif
            }
        }

        public static void ReadAssociatedFile() {
            if (!string.IsNullOrWhiteSpace(FilepathAssociated)) {
                if (Path.GetExtension(FilepathAssociated).ToUpperInvariant() == ".SKTSKIN") {
                    ImportSkin(FilepathAssociated);
                    MainW.Conf.Show();
                    MainW.Conf.skinmgmt.IsSelected = true;
                    CheckBlurBG();
                    //GetSkinList(); //is run on mainWindow.load()
                }
                else {
                    ReadFile(FilepathAssociated);
                }
                FilepathAssociated = null;
            }
        }

        /// <summary>
        /// Saves the Open File, default extension: .xalmp
        /// <para>
        /// <paramref name="saveas"/>: True = Save As, False = save as <see cref="Filepath"/> name
        /// </para>
        /// </summary>
        /// <param name="saveas"> True = Save As, False = save as <see cref="Filepath"/> name</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:No pasar cadenas literal como parámetros localizados", MessageId = "System.Windows.MessageBox.Show(System.String,System.String,System.Windows.MessageBoxButton,System.Windows.MessageBoxImage,System.Windows.MessageBoxResult,System.Windows.MessageBoxOptions)")]
        public static void Save(bool saveas) {
            bool skip = false;
            if (!saveas && Filepath.Length < 4) {//checks for complete filenames instead of just .rtf or .xaml //should be "" because it is validated in other places too but just in case
                saveas = true;
            }
            if (saveas) {
                SaveFileDialog savedialog = new SaveFileDialog() {
                    CreatePrompt = true,
                    OverwritePrompt = true,
                    CheckPathExists = true,
                    ValidateNames = true,
                    RestoreDirectory = true,
                    Filter = "XAML Package (*.xamlp)|*.xamlp|Rich Text File (*.rtf)|*.rtf|XAML File (*.xaml)|*.xaml|Text file (*.txt)|*.txt"
                };
                if (Filepath.Length < 4) {//checks for complete filenames instead of just .rtf or .xaml //should be "" because it is validate in other places too but just in case
                    savedialog.FileName = "Notes";
                }
                else {
                    savedialog.FileName = Path.GetFileName(Filepath);
                }

                if (savedialog.ShowDialog() == true) {
                    Filepath = savedialog.FileName;
                    MainW.MenuFileName.Header = Path.GetFileName(Filepath);
                }
                else {
                    skip = true;
                }
            }
            if (!skip) {
                try {
                    TextRange t = new TextRange(MainW.rtb.Document.ContentStart, MainW.rtb.Document.ContentEnd);
                    using (FileStream file = new FileStream(Filepath, FileMode.Create)) {
                        switch (Path.GetExtension(Filepath).ToUpperInvariant()) {
                            case (".RTF"): {
                                    t.Save(file, DataFormats.Rtf);
                                    break;
                                }
                            case (".TXT"): {
                                    t.Save(file, DataFormats.Text);
                                    break;
                                }
                            case (".XAML"): {
                                    t.Save(file, DataFormats.Xaml);
                                    break;
                                }
                            case (".XAMLP"): {//NOTE: change extension to skintext and register it to open with defaults
                                    t.Save(file, DataFormats.XamlPackage);
                                    break;
                                }
                            default: {//ths should never hapen
                                    throw new FileFormatException();
                                }
                        }
                        FileChanged = false;
                        SaveConfig();
                    }
                }
                catch (Exception ex) {
                    MessageBox.Show("Failed to Write File", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.ServiceNotification);
#if DEBUG
                    MessageBox.Show("DEBUG: "+ex.ToString());
                    //throw;
#endif
                }
            }
        }

        private static bool _isSaving;
        public static bool AutoSaveEnabled = true;

        public async static System.Threading.Tasks.Task DelayedSaveAsync() {
            // if already waiting, get out
            if (_isSaving) {
                return;
            }
            _isSaving = true;
            await System.Threading.Tasks.Task.Delay(AutoSaveTimer);
            Save(false);
            _isSaving = false;
        }

        #endregion File

        #region General

        /// <summary>
        /// <para>Closes <see cref="MainWindow.FontConf"/> and <see cref="ConfigWin"/>, then calls <see cref="SaveConfig"/></para>
        /// Called from <see cref="MainWindow.Window_Closing"/>
        /// </summary>
        public static void ExitProgram() {
            MainW.FontConf.Close();
            MainW.Conf.Close();
            SaveConfig();
            SaveCurrentSkin();

        }

        /// <summary>
        /// <para>Called from <see cref="MainWindow.Resettodefaults_Click"/> using <see cref="SaveChanges"/></para>
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:No pasar cadenas literal como parámetros localizados", MessageId = "System.Windows.MessageBox.Show(System.String,System.String,System.Windows.MessageBoxButton,System.Windows.MessageBoxImage,System.Windows.MessageBoxResult)")]
        public static void ResetDefaults() {
            switch (MessageBox.Show("This will reset ALL configuration\r\n Are you sure?", "Reset To Defaults", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No)) {
                case (MessageBoxResult.Yes): {
                        LoadDefault();
                        //MainW.grid.UpdateLayout();
                        break;
                    }
                case MessageBoxResult.No: {
                        break;
                    }
            }
        }

        /// <summary>
        /// Checks if there are unsaved changes and asks the user what to do, then calls the <paramref name="fileFunc"/> sent (must be <see cref="void"/>)
        /// <para>
        /// <paramref name="txtBoxTitle"/> is the <see cref="MessageBox"/> Caption <see cref="string"/>
        /// </para>
        /// </summary>
        /// <param name="fileFunc"> <see cref="Action"/> Method to be called must be <see cref="void"/></param>
        /// <param name="e"> <see cref="System.ComponentModel.CancelEventArgs"/> to cancel <see cref="Window.Close"/></param>
        /// <param name="txtBoxTitle"> <see cref="MessageBox"/> Caption <see cref="string"/></param>
        public static void SaveChanges(Action fileFunc, System.ComponentModel.CancelEventArgs e, string txtBoxTitle) {
            if (fileFunc != null) {
                if (FileChanged) {
                    switch (MessageBox.Show("There are unsaved Changes to: \r\n" + Filepath + "\r\nDo you want to save?", txtBoxTitle, MessageBoxButton.YesNoCancel, MessageBoxImage.Exclamation, MessageBoxResult.Cancel)) {
                        case (MessageBoxResult.Yes): {
                                Save(false);
                                fileFunc();
                                break;
                            }
                        case MessageBoxResult.No: {
                                fileFunc();
                                break;
                            }
                        case MessageBoxResult.Cancel: {
                                //do nothing
                                if (e != null) {
                                    e.Cancel = true;
                                }
                                break;
                            }
                    }
                }
                else {
                    fileFunc();
                }
            }
        }

        #endregion General

        #region Window Config

        /// <summary>
        /// Flips the <see cref="MainWindow.rtb"/>
        /// <para>
        /// <see cref="ScaleTransform"/> takes doubles and scale the RTB to those values, with -1 fliping it and 1 being default
        /// </para>
        /// </summary>
        /// <param name="flipX"></param>
        /// <param name="flipY"></param>
        public static void RtbFlip(bool flipX, bool flipY) {
            MainW.FontConf.txtSampleText.RenderTransform = MainW.stackborder.RenderTransform = new ScaleTransform(flipX ? -1 : 1, flipY ? -1 : 1);
        }

        /// <summary>
        /// Sets the <see cref="MainWindow.rtb"/> opacity to <paramref name="opacity"/>
        /// </summary>
        /// <param name="opacity"></param>
        public static void RtbOpacity(double opacity) {
            MainW.rtb.Opacity = opacity;
        }

        /// <summary>
        /// Sets the <see cref="MainWindow.rtb"/> IsReadOnly state
        /// </summary>
        /// <param name="readOnly"></param>
        public static void RtbReadOnly(bool readOnly) {
            MainW.rtb.IsReadOnly = readOnly;
            MainW.rtb.IsReadOnlyCaretVisible = !readOnly;
            MainW.rtb.IsInactiveSelectionHighlightEnabled = !readOnly;
        }

        /// <summary>
        ///Rotates the <see cref="MainWindow.grid"/> to <paramref name="angle"/> and <see cref="MainWindow.backgroundimg"/> to the oposite direction to mantain it straight
        ///<para>On some cases bugs <see cref="MainWindow.backgroundimg"/> when resize borders are enabled</para>
        /// </summary>
        /// <param name="angle"></param>
        public static void RtbRotate(double angle) {
            MainW.TitleBorder.RenderTransform = new RotateTransform(angle);
            //rotateTransform = new RotateTransform(-angle);
            //MainW.backgroundimg.RenderTransform = rotateTransform;
        }

        /// <summary>
        /// Sets the <see cref="MainWindow.rtb"/> SpellCheck.IsEnabled state
        /// </summary>
        /// <param name="enabled"></param>
        public static void RtbSpellCheck(bool enabled) {
            MainW.rtb.SpellCheck.IsEnabled = enabled;
        }

        /// <summary>
        /// Sets the <see cref="MainWindow.window"/> Topmost state
        /// </summary>
        /// <param name="visible"></param>
        ///
        public static void WindowAlwaysOnTop(bool visible) {
            MainW.window.Topmost = visible;
        }

        /// <summary>
        /// Sets the <see cref="MainWindow.window"/> opacity to <paramref name="opacity"/>
        /// </summary>
        /// <param name="opacity"></param>
        public static void WindowOpacity(double opacity) {
            MainW.window.Opacity = opacity;
        }

        /// <summary>
        /// Sets the <see cref="MainWindow.MyNotifyIcon"/> Visibility state
        /// </summary>
        /// <param name="visible"></param>
        public static void WindowVisibleNotification(bool visible) {
            MainW.MyNotifyIcon.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Sets the <see cref="MainWindow.MyNotifyIcon"/> ResizeMode state
        /// </summary>
        /// <param name="visible"></param>
        public static void WindowVisibleResize(bool visible) {
            MainW.window.ResizeMode = visible ? ResizeMode.CanResizeWithGrip : ResizeMode.NoResize;
        }

        /// <summary>
        /// Sets the <see cref="MainWindow.window"/> ShowInTaskbar state
        /// </summary>
        /// <param name="visible"></param>
        public static void WindowVisibleTaskbar(bool visible) {
            MainW.window.ShowInTaskbar = visible;
            MainW.Conf.ShowInTaskbar = visible;
            MainW.FontConf.ShowInTaskbar = visible;
        }

        public static void CheckBlurBG() {
            CustomMethods.BlurBG(MainW.Conf.BgBlur.IsChecked.Value);
        }

        public static void BlurBG(bool enableBlur) {
            if (MainW?.FontConf != null && MainW?.Conf != null) {
                if (enableBlur) {
                    BgBlur.EnableBlur(AccentState.ACCENT_ENABLE_BLURBEHIND, MainW);
                    BgBlur.EnableBlur(AccentState.ACCENT_ENABLE_BLURBEHIND, MainW.FontConf);
                    BgBlur.EnableBlur(AccentState.ACCENT_ENABLE_BLURBEHIND, MainW.Conf);
                }
                else {
                    BgBlur.EnableBlur(AccentState.ACCENT_INVALID_STATE, MainW);
                    BgBlur.EnableBlur(AccentState.ACCENT_INVALID_STATE, MainW.FontConf);
                    BgBlur.EnableBlur(AccentState.ACCENT_INVALID_STATE, MainW.Conf);
                }
            }
        }

        public static void RtbBorderSize(double val) {
            MainW.stackborder.BorderThickness = new Thickness(val);
        }

        public static void ResizeRtb(bool resizeEnabled) {
            if (resizeEnabled) {
                MainW.panel.IsEnabled = false;

                MainW.MouseLeftButtonDown += MainW.Window1MouseLeftButtonDown;
                MainW.MouseLeftButtonUp += MainW.DragFinishedMouseHandler;
                MainW.MouseMove += MainW.Window1MouseMove;
                MainW.MouseLeave += MainW.Window1MouseLeave;

                MainW.canvas.PreviewMouseLeftButtonDown += MainW.MyCanvasPreviewMouseLeftButtonDown;
                MainW.canvas.PreviewMouseLeftButtonUp += MainW.DragFinishedMouseHandler;

                MainW.selectedElement = MainW.TitleBorder as UIElement;
                MainW.OriginalLeft = System.Windows.Controls.Canvas.GetLeft(MainW.selectedElement);
                MainW.OriginalTop = System.Windows.Controls.Canvas.GetTop(MainW.selectedElement);
                MainW.aLayer = AdornerLayer.GetAdornerLayer(MainW.selectedElement);
                MainW.aLayer.Add(new ResizingAdorner(MainW.selectedElement));
                MainW.selected = true;
            }
            else {
                MainW.Deselect();
                MainW.MouseLeftButtonDown -= MainW.Window1MouseLeftButtonDown;
                MainW.MouseLeftButtonUp -= MainW.DragFinishedMouseHandler;
                MainW.MouseMove -= MainW.Window1MouseMove;
                MainW.MouseLeave -= MainW.Window1MouseLeave;

                MainW.canvas.PreviewMouseLeftButtonDown -= MainW.MyCanvasPreviewMouseLeftButtonDown;
                MainW.canvas.PreviewMouseLeftButtonUp -= MainW.DragFinishedMouseHandler;

                MainW.panel.IsEnabled = true;
            }
        }

        public static void LineWrap(bool wrapEnabled) {
            if (wrapEnabled) {
                MainW.rtb.Document.PageWidth = double.NaN;
            }
            else {
                MainW.rtb.Document.PageWidth = 1000;
            }
            MainW.LineWrapMenuItem.IsChecked = wrapEnabled;
            if (MainW.Conf != null) {
                MainW.Conf.TextWrap.IsChecked = wrapEnabled;
            }
        }

        public static void ToolBarVisible(bool toolBarEnabled) {
            if (toolBarEnabled) {
                MainW.ToolBarTray.Visibility = Visibility.Visible;
            }
            else {
                MainW.ToolBarTray.Visibility = Visibility.Collapsed;
            }
            MainW.ToolBarMenuItem.IsChecked = toolBarEnabled;
            if (MainW.Conf != null) {
                MainW.Conf.ToolBarEnabled.IsChecked = toolBarEnabled;
            }
        }

        public static void ToolsAlwaysOnTop(bool onTop) {
            if (MainW?.Conf != null && MainW?.FontConf != null) {
                MainW.Conf.Topmost = onTop;
                MainW.FontConf.Topmost = onTop;
            }
        }

        public static void ChangeBrushResource(Brush brush, string resource) {
            if (brush != null) {
                brush.Freeze();
                Application.Current.Resources[resource] = brush;
                brush = null;
            }
        }

        #endregion Window Config

        #region Skins

        /// <summary>
        /// Reads Config.ini and sets wich skin to use
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static void GetSkin() {
            CurrentSkin = @"\Default";
            try {
                if (File.Exists(AppDataPath + @"\config.ini")) {
                    using (StreamReader reader = new StreamReader(AppDataPath + @"\config.ini", System.Text.Encoding.UTF8)) {
                        string currentLine;
                        string[] line;
                        if ((currentLine = reader.ReadLine()) != null) {
                            line = currentLine.Split('=');
                            line[0] = line[0].Trim();
                            line[0] = line[0].ToUpperInvariant();
                            if (!string.IsNullOrEmpty(line[0]) && !string.IsNullOrEmpty(line[1])) {
                                if (line[0] == "SKIN") {
                                    line[1] = line[1].Trim();
                                    if (!string.IsNullOrWhiteSpace(line[1])) {
                                        CurrentSkin = @"\" + line[1];//A Folder
                                        if (!File.Exists(AppDataPath + CurrentSkin + @"\skintext.ini")) {
                                            CurrentSkin = @"\Default";
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex) {
                // The appdata folders dont exist
                //can be first open, let default values
                CurrentSkin = @"\Default";
#if DEBUG
                MessageBox.Show("DEBUG: "+ex.ToString());
                //throw;
#endif
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static void SaveCurrentSkin() {
            FileStream fs = null;
            try {
                string configFile = AppDataPath + @"\config.ini";
                fs = new FileStream(configFile, FileMode.Create, FileAccess.Write);
                using (TextWriter writer = new StreamWriter(fs, System.Text.Encoding.UTF8)) {
                    fs = null; //is no longer needed
                    string data = "Skin = " + CurrentSkin.Replace(@"\", "");
                    writer.WriteLine(data);
                }
            }
            catch (Exception ex2) {
#if DEBUG
                MessageBox.Show("DEBUG: "+ex2.ToString());
                //throw;
#endif
            }
            finally {
                fs?.Dispose();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:No pasar cadenas literal como parámetros localizados", MessageId = "System.Windows.MessageBox.Show(System.String,System.String,System.Windows.MessageBoxButton,System.Windows.MessageBoxImage)")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static void CreateModifySkin(string skin, string skinName, string skinAuthor, string skinVersion, string screenshotPath, string notes) {
            FileStream fs = null;
            try {
                Directory.CreateDirectory(AppDataPath + @"\" + skin);
                string configFile = AppDataPath + @"\" + skin + @"\skininfo.ini";
                fs = new FileStream(configFile, FileMode.Create, FileAccess.Write);
                using (TextWriter writer = new StreamWriter(fs, System.Text.Encoding.UTF8)) {
                    fs = null; //is no longer needed
                    string data = "Skin Name = " + skinName;
                    writer.WriteLine(data);
                    data = "Skin Author = " + skinAuthor;
                    writer.WriteLine(data);
                    data = "Skin Version = " + skinVersion;
                    writer.WriteLine(data);
                    data = "SkinText Version = " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                    writer.WriteLine(data);

                    if (string.IsNullOrWhiteSpace(screenshotPath) || screenshotPath == "pack://application:,,,/SkinText;component/Resources/icon_01.ico") {
                        screenshotPath = "DefaultIcon";
                        if (File.Exists(OldScreenShotPath)) {
                            DeleteOldScreenshot(OldScreenShotPath);
                        }
                    }
                    else {
                        if (Path.GetFileNameWithoutExtension(screenshotPath).Contains("_temp")){
                            try {
                                ScreenShotClear();
                                if (File.Exists(OldScreenShotPath)) {
                                    DeleteOldScreenshot(OldScreenShotPath);
                                }
                                File.Move(screenshotPath, screenshotPath.Replace("_temp", ""));
                                screenshotPath = screenshotPath.Replace("_temp", "");
                                LoadScreenshot(screenshotPath);
                                OldScreenShotPath = screenshotPath;
                            }
                            catch (Exception ex) {
                                MessageBox.Show("Error creating Screenshot, Please try again", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                #if DEBUG
                                MessageBox.Show("DEBUG: "+ex.ToString());
                                //throw;
                                #endif
                            }
                        }
                        screenshotPath = Path.GetFileName(screenshotPath);
                    }
                    data = "ScreenShot = " + screenshotPath;
                    writer.WriteLine(data);

                    data = "Notes = " + notes?.Replace("\r\n", @"   ");
                    writer.WriteLine(data);
                }
                ScreenshotUpload = false;
            }
            catch (Exception ex) {
                #if DEBUG
                MessageBox.Show("DEBUG: "+ex.ToString());
                //throw;
                #endif
            }
            finally {
                fs?.Dispose();
            }
        }

        public static void GetSkinList() {
            MainW.Conf.SkinsListbox.Items.Clear();
            ListBoxItem itm2 = new ListBoxItem {
                Content = "Create New Skin"
            };
            MainW.Conf.SkinsListbox.Items.Add(itm2);
            System.Collections.Generic.IEnumerable<string> skinList = Directory.EnumerateDirectories(appDataPath);
            foreach (string item in skinList) {
                ListBoxItem itm = new ListBoxItem {
                    Content = Path.GetFileName(item)
                };
                MainW.Conf.SkinsListbox.Items.Add(itm);
                if (itm.Content.ToString() == CurrentSkin.Replace("\\", "")) {
                    MainW.Conf.SkinsListbox.SelectedItem = itm;
                }
            }
            if (MainW.Conf.SkinsListbox.SelectedItem == null) {
                MainW.Conf.SkinsListbox.SelectedIndex = 0;
            }

            //MainW.Conf.SkinsListbox.SelectedItem=CurrentSkin.Replace("\\","");
        }

        public static void SkinList_Information(string SelectedSkin) {
            MainW.Conf.SkinName_TextBox.Text = "";
            MainW.Conf.SkinAuthor_TextBox.Text = "";
            MainW.Conf.SkinVersion_TextBox.Text = "";
            //MainW.Conf.SkinTextVersion_TextBox.Text = "";
            ScreenShotClear();
            DeleteOldScreenshot(ScreenShotPath);
            //ScreenshotUpload = false;
            ScreenShotPath = "";
            MainW.Conf.SkinTextVersion_TextBox.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            MainW.Conf.SkinScreenshot_IMG.Source = (System.Windows.Media.Imaging.BitmapImage)Application.Current.Resources["ImageSource1icon"];
            MainW.Conf.SkinNotes_TextBox.Text = "";

            if (SelectedSkin != "Create New Skin") {
                MainW.Conf.LoadSkin.Visibility = Visibility.Visible;
                MainW.Conf.CreateSkin.Content = "Modify Skin";

                string infoPath = AppDataPath + @"\" + SelectedSkin;
                if (File.Exists(infoPath + @"\skininfo.ini")) {
                    try {
                        using (StreamReader reader = new StreamReader(infoPath + @"\SkinInfo.ini", System.Text.Encoding.UTF8)) {
                            string currentLine;
                            string[] line;
                            while ((currentLine = reader.ReadLine()) != null) {
                                line = currentLine.Split('=');
                                line[0] = line[0].Trim();
                                line[0] = line[0].ToUpperInvariant();
                                if (!string.IsNullOrEmpty(line[0]) && line.Length > 1 && !string.IsNullOrWhiteSpace(line[1])) {
                                    line[1] = line[1].Trim();
                                    switch (line[0]) {
                                        case ("SKIN NAME"): {
                                                MainW.Conf.SkinName_TextBox.Text = line[1];
                                                break;
                                            }
                                        case ("SKIN AUTHOR"): {
                                                MainW.Conf.SkinAuthor_TextBox.Text = line[1];
                                                break;
                                            }
                                        case ("SKIN VERSION"): {
                                                MainW.Conf.SkinVersion_TextBox.Text = line[1];
                                                break;
                                            }
                                        case ("SKINTEXT VERSION"): {
                                                MainW.Conf.SkinTextVersion_TextBox.Text = line[1];
                                                break;
                                            }
                                        case ("SCREENSHOT"): {
                                                LoadScreenshot(infoPath + @"\" + line[1]);
                                                OldScreenShotPath = ScreenShotPath;
                                                break;
                                            }
                                        case ("NOTES"): {
                                                MainW.Conf.SkinNotes_TextBox.Text = line[1].Replace(@"   ", "\r\n");
                                                break;
                                            }
                                        default: {
                                                #if DEBUG
                                                MessageBox.Show("Not recognized:  \"" + line[0] + "\" in SkinInfo file");
                                                #endif
                                                break;
                                            }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex) {
                        #if DEBUG
                        MessageBox.Show("DEBUG: "+ex.ToString());
                        //throw;
                        #endif
                    }
                }
                else {
                    MessageBox.Show("Can not find SkinInfo.ini of this Skin", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else {
                MainW.Conf.LoadSkin.Visibility = Visibility.Hidden;
                MainW.Conf.CreateSkin.Content = "Create Skin";
            }
        }

        public static void LoadScreenshot(string screenshotPath) {
            if (Path.GetFileNameWithoutExtension(screenshotPath) == "DefaultIcon") {
                //MainW.Conf.SkinScreenshot_IMG.Source = (System.Windows.Media.Imaging.BitmapImage)Application.Current.Resources["ImageSource1icon"];
                //do nothing, is already loaded the default
            }
            else {
                try {
                    if (File.Exists(screenshotPath)) {
                        ScreenShotClear();
                        Uri uri = new Uri(screenshotPath);
                        if (Path.GetExtension(screenshotPath).ToUpperInvariant() == ".GIF") {
                            if (MainW.Conf.GifMethodCPU.IsChecked.Value) {//CPU Method
                                XamlAnimatedGif.AnimationBehavior.SetSourceUri(MainW.Conf.SkinScreenshot_IMG, uri);
                            }
                            else {//RAM Method
                                ImageSource bitmap = BitmapFromUri(uri);
                                bitmap.Freeze();
                                WpfAnimatedGif.ImageBehavior.SetAnimatedSource(MainW.Conf.SkinScreenshot_IMG, bitmap);
                                bitmap = null;
                            }
                        }
                        else {//default (no gif animation)
                            ImageSource bitmap = BitmapFromUri(uri);
                            bitmap.Freeze();
                            MainW.Conf.SkinScreenshot_IMG.Source = bitmap;
                            bitmap = null;
                        }
                        uri = null;
                        ScreenShotPath = screenshotPath;
                    }
                    else {
                        throw new FileNotFoundException("Error: File not found", screenshotPath);
                    }
                }
                catch (Exception ex) {
                    if (!string.IsNullOrWhiteSpace(screenshotPath)) {
                        MessageBox.Show("Failed to Load ScreenShot:\r\n " + screenshotPath, "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.ServiceNotification);
                    }
                    ScreenShotClear();
                    MainW.Conf.SkinScreenshot_IMG.Source = (System.Windows.Media.Imaging.BitmapImage)Application.Current.Resources["ImageSource1icon"];
                    ScreenShotPath = "";
#if DEBUG
                    MessageBox.Show("DEBUG: "+ex.ToString());
                    //throw;
#endif
                }
            }
        }

        public static void SaveScreenshot(string skinName, string screenShotFile) {
            try {
                string newImagePath = AppDataPath + @"\" + skinName + @"\screenshot_temp" + Path.GetExtension(screenShotFile);
                if (screenShotFile != newImagePath) {
                    ScreenShotClear();
                    DeleteOldScreenshot(ScreenShotPath);
                    ScreenShotPath = "";
                    Directory.CreateDirectory(AppDataPath + @"\" + skinName);
                    File.Copy(screenShotFile, newImagePath, true);
                }
                //first copy the screenshot to appdata/skin, then load it
                LoadScreenshot(newImagePath);
            }
            catch (Exception ex) {
                #if DEBUG
                MessageBox.Show("DEBUG: "+ex.ToString());
                //throw;
                #endif
            }
        }

        public static void SelectScreenshot(string skin) {
            OpenFileDialog openFileDialog = new OpenFileDialog() {
                Multiselect = false,
                CheckFileExists = true,
                CheckPathExists = true,
                ValidateNames = true,
                RestoreDirectory = true,
                Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png, *.gif) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png; *.gif"
            };
            //openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (openFileDialog.ShowDialog() == true) {

                SaveScreenshot(skin, openFileDialog.FileName);
            }
            else {
                ScreenShotClear();
                MainW.Conf.SkinScreenshot_IMG.Source = (System.Windows.Media.Imaging.BitmapImage)Application.Current.Resources["ImageSource1icon"];
                DeleteOldScreenshot(ScreenShotPath);
                ScreenShotPath = "";
            }
            ScreenshotUpload = true;
        }

        private static void DeleteOldScreenshot(string deletepath) {
            try {
                if (File.Exists(deletepath) && ScreenshotUpload) {
                    File.Delete(deletepath);
                    ScreenshotUpload = false;
                }
            }
            catch (Exception ex) {
                //MessageBox.Show("Error deleting temporal file, please try again", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                #if DEBUG
                MessageBox.Show("DEBUG: "+ex.ToString());
                //throw;
                #endif
            }
        }

        public static void ScreenShotClear() {
            MainW.Conf.SkinScreenshot_IMG.Source = null;
            WpfAnimatedGif.ImageBehavior.SetAnimatedSource(MainW.Conf.SkinScreenshot_IMG, null);
            XamlAnimatedGif.AnimationBehavior.SetSourceUri(MainW.Conf.SkinScreenshot_IMG, null);
            XamlAnimatedGif.AnimationBehavior.SetSourceStream(MainW.Conf.SkinScreenshot_IMG, null);
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
        }

        public static void ExportSkin(string skin) {
            SaveFileDialog savedialog = new SaveFileDialog {
                CreatePrompt = true,
                OverwritePrompt = true,
                CheckPathExists = true,
                ValidateNames = true,
                RestoreDirectory = true,
                Filter = "SkinText Skin (*.sktskin)|*.sktskin",
                FileName = Path.GetFileName(skin)
            };

            if (savedialog.ShowDialog() == true) {
                try {
                    ZipFile.CreateFromDirectory(AppDataPath+@"\"+skin, savedialog.FileName, CompressionLevel.Optimal,false);
                    MessageBox.Show("Export of skin: " + skin + " complete!", "Success!", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex) {
                    MessageBox.Show("Failed to Export skin", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.ServiceNotification);
                    #if DEBUG
                    MessageBox.Show("DEBUG: "+ex.ToString());
                    //throw;
                    #endif
                }
            }
        }

        public static void OpenImportSkin() {
            OpenFileDialog openFileDialog = new OpenFileDialog() {
                Multiselect = false,
                CheckFileExists = true,
                CheckPathExists = true,
                ValidateNames = true,
                RestoreDirectory = true,
                Filter = "SkinText Skin (*.sktskin)|*.sktskin",
            };
            if (openFileDialog.ShowDialog() == true) {
                ImportSkin(openFileDialog.FileName);
            }
        }

        public static void ImportSkin(string skin) {
            try {
                string newSkinPath =  Path.GetFileNameWithoutExtension(skin);
                Directory.CreateDirectory(AppDataPath + @"\" + newSkinPath);
                ZipFile.ExtractToDirectory(skin, AppDataPath + @"\" + newSkinPath);
                MessageBox.Show("Import of skin: " + newSkinPath + " complete!", "Success!", MessageBoxButton.OK, MessageBoxImage.Information);
                GetSkinList();
            }
            catch (Exception ex) {
                MessageBox.Show("Failed to Import skin", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.ServiceNotification);
                #if DEBUG
                MessageBox.Show("DEBUG: "+ex.ToString());
                //throw;
                #endif
            }
        }

        public static void DeleteSkin(string skin) {
            if (@"\" + skin == CurrentSkin) {
                MessageBox.Show("Cant Delete Skin Currently in use","Error",MessageBoxButton.OK,MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.ServiceNotification);
            }
            else {
                try {
                    Directory.Delete(AppDataPath + @"\" + skin, true);
                    MessageBox.Show("Skin: " + skin + " Deleted Sucsefully!", "Success!", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex) {
                    MessageBox.Show("Failed to Delete skin", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.ServiceNotification);
                    #if DEBUG
                    MessageBox.Show("DEBUG: "+ex.ToString());
                    //throw;
                    #endif
                }
            }
        }

        #endregion Skins

        #region Text Functions

        /// <summary>
        /// Sets the format to the selected text
        /// </summary>
        /// <param name="foreColor">Font Fore Color</param>
        /// <param name="backgroundColor">Font Background Color</param>
        /// <param name="fontFamily">Font Family</param>
        /// <param name="fontSize">Font Size</param>
        /// <param name="decor"> TextDecorationCollection </param>
        /// <param name="fontStyle">Font Style</param>
        /// <param name="fontWeight">Font Weight</param>
        /// <param name="textalign">Text Alignment</param>
        /// <param name="flow">Text Flow Direction</param>
        /// <param name="basealign">Text Baseline Alignment</param>
        /// <param name="lineHeight">Line Height</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static void TextFormat(Brush foreColor, Brush backgroundColor, FontFamily fontFamily, double fontSize, TextDecorationCollection decor, FontStyle fontStyle, FontWeight fontWeight, TextAlignment textalign, FlowDirection flow, BaselineAlignment basealign, double lineHeight) {
            if ((backgroundColor != null) && (((SolidColorBrush)backgroundColor).Color.A == 0)) {
                backgroundColor = null;
            }
            // Make sure we have a selection. Should have one even if there is no text selected.
            if (MainW.rtb.Selection != null) {
                // Check whether there is text selected or just sitting at cursor
                if (MainW.rtb.Selection.IsEmpty) {
                    // Check to see if we are at the start of the textbox and nothing has been added yet
                    if (MainW.rtb.Selection.Start.Paragraph == null) {
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
                        Run newRun = new Run() {
                            Foreground = foreColor,
                            Background = backgroundColor,
                            FontFamily = fontFamily,
                            FontSize = fontSize,
                            FontStyle = fontStyle,
                            FontWeight = fontWeight,
                            FlowDirection = flow,
                            BaselineAlignment = basealign,
                            TextDecorations = null
                        };
                        if (decor != null) {
                            newRun.TextDecorations = decor.Clone();
                        }
                        MainW.rtb.Document.Blocks.Add(p);// worked!
                        p.Inlines.Add(newRun);
                        p.TextAlignment = textalign;
                        p.LineHeight = lineHeight;
                        // Reset the cursor into the new block.
                        // If we don't do this, the font properties will default again when you start typing.
                        MainW.rtb.CaretPosition = newRun.ElementStart;
                    }
                    else {   //not an empty textbox
                        // Get current position of cursor
                        TextPointer curCaret = MainW.rtb.CaretPosition;
                        // Get the current block object that the cursor is in
                        Block curBlock = MainW.rtb.Document.Blocks.FirstOrDefault(x => x.ContentStart.CompareTo(curCaret) == -1 &&
                                                                                       x.ContentEnd.CompareTo(curCaret) == 1);
                        if (curBlock != null) {
                            Paragraph curParagraph = curBlock as Paragraph;
                            // Create a new run object with the font properties, and add it to the current block
                            Run newRun = new Run() {
                                Foreground = foreColor,
                                Background = backgroundColor,
                                FontFamily = fontFamily,
                                FontSize = fontSize,
                                FontStyle = fontStyle,
                                FontWeight = fontWeight,
                                FlowDirection = flow,
                                BaselineAlignment = basealign,
                                TextDecorations = null
                            };
                            if (decor != null) {
                                newRun.TextDecorations = decor.Clone();
                            }
                            /*
                            //NOTE: add checkboxes. NOT POSSIBLE, ON SAVE THEY ARE DELETED
                            System.Windows.Controls.CheckBox checkbox = new System.Windows.Controls.CheckBox();
                            curParagraph.Inlines.Add(checkbox);

                            //NOTE: NOT ADDING EXPANDERS, NOT WORTH IT, NOT PRETTY, CANT ADD INLINES, MUST ADD TEXTBOX OR SIMILAR:
                            System.Windows.Controls.TextBox txtbox = new System.Windows.Controls.TextBox();
                            System.Windows.Controls.Expander expander = new System.Windows.Controls.Expander();
                            MainW.rtb.Selection.Start.Paragraph.Inlines.Add(expander);
                            txtbox.Text = selectionTextRange.Text;
                            expander.Content = txtbox;
                            //expander.Content = (Run)MainW.rtb.Selection.Start.Parent;
                            MainW.rtb.Selection.Text = "";

                            System.Windows.Controls.Expander expander = new System.Windows.Controls.Expander();
                            System.Windows.Controls.StackPanel stackpanel = new System.Windows.Controls.StackPanel();
                            stackpanel.Children.Add(checkbox);
                            expander.Content = stackpanel;
                            curParagraph.Inlines.Add(expander);

                            EditingCommands.AlignCenter.Execute(null, MainW.rtb);
                            */

                            curParagraph.Inlines.Add(newRun);
                            curParagraph.LineHeight = lineHeight;
                            curParagraph.TextAlignment = textalign;
                            curParagraph.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;
                            // Reset the cursor into the new block.
                            // If we don't do this, the font properties will default again when you start typing.
                            MainW.rtb.CaretPosition = newRun.ElementStart;
                        }
                    }
                }
                else // There is selected text, so change the font properties of the selection
                {
                    try {

                        //fix for sticky bg color
                        //if this is above the ApplyPropertyValue block, it works on the first apply, but will remove whole paragraph always
                        //but when it is below, it is neccesary to click apply twice, but it will not remove whole line color, just part of it
                        SolidColorBrush newBrush2 = null;
                        newBrush2 = (SolidColorBrush)MainW.rtb.Selection.Start.Paragraph.Background;
                        if (newBrush2 != null) {
                            MainW.rtb.Selection.Start.Paragraph.Background = null;
                        }
                        try {
                            SolidColorBrush newBrush3 = null;
                            DependencyObject spanOrParagraph = ((Run)MainW.rtb.Selection.Start.Parent).Parent;
                            if (!(spanOrParagraph is Paragraph)) {
                                newBrush3 = (SolidColorBrush)((Span)spanOrParagraph).Background;
                            }

                            if (newBrush3 != null) {
                                ((Span)spanOrParagraph).Background = null;
                            }
                        }
                        catch (Exception ex2) {
                            #if DEBUG
                            MessageBox.Show("DEBUG: " + ex2.ToString());
                            //throw;
                            #endif
                        }
                        //end fix

                        TextRange selectionTextRange = new TextRange(MainW.rtb.Selection.Start, MainW.rtb.Selection.End);
                        selectionTextRange.ApplyPropertyValue(TextElement.ForegroundProperty, foreColor);
                        selectionTextRange.ApplyPropertyValue(TextElement.BackgroundProperty, backgroundColor);
                        selectionTextRange.ApplyPropertyValue(TextElement.FontFamilyProperty, fontFamily);
                        selectionTextRange.ApplyPropertyValue(TextElement.FontSizeProperty, fontSize);
                        selectionTextRange.ApplyPropertyValue(TextElement.FontStyleProperty, fontStyle);
                        selectionTextRange.ApplyPropertyValue(TextElement.FontWeightProperty, fontWeight);
                        selectionTextRange.ApplyPropertyValue(Inline.TextDecorationsProperty, null);
                        if (decor != null) {
                            selectionTextRange.ApplyPropertyValue(Inline.TextDecorationsProperty, decor.Clone());
                        }
                        selectionTextRange.ApplyPropertyValue(Block.TextAlignmentProperty, textalign);
                        selectionTextRange.ApplyPropertyValue(Block.FlowDirectionProperty, flow);
                        selectionTextRange.ApplyPropertyValue(Block.LineHeightProperty, lineHeight);
                        selectionTextRange.ApplyPropertyValue(Inline.BaselineAlignmentProperty, basealign);

                    }
                    catch (Exception ex) {
                        //crashes when changing hyperLink properties
                        #if DEBUG
                        MessageBox.Show("DEBUG: "+ex.ToString());
                        //throw;
                        #endif
                    }
                }
            }
            // Reset the focus onto the richtextbox after selecting the font in a toolbar etc
            MainW.rtb.Focus();
            FileChanged = true;
        }

        /// <summary>
        /// Whenever the <see cref="MainWindow.rtb"/> selection is changed, it is neccesary to read current text selection information and display it on <see cref="FontConfig"/> window
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static void RtbSelectionChanged() {
            if (MainW.FontConf.Visibility == Visibility.Visible) {
                try {
                    if (MainW.rtb.Selection != null && MainW.rtb.Selection.Start.Paragraph != null) {
                        TextRange selectionTextRange = new TextRange(MainW.rtb.Selection.Start, MainW.rtb.Selection.End);

                        RtbSelectionChangedBGColor(selectionTextRange);
                        RtbSelectionChangedFontColor(selectionTextRange);
                        RtbSelectionChangedFontSize(selectionTextRange);
                        RtbSelectionChangedLineHeight(selectionTextRange);
                        RtbSelectionChangedFontFamily(selectionTextRange);
                        RtbSelectionChangedFontType(selectionTextRange);
                        RtbSelectionChangedTextAlignment(selectionTextRange);
                        RtbSelectionChangedFlowDirection(selectionTextRange);
                        RtbSelectionChangedBaselineAlignment(selectionTextRange);
                        RtbSelectionChangedTextDecorations(selectionTextRange);
                    }
                }
                catch (Exception ex) {
#if DEBUG
                    MessageBox.Show("DEBUG: "+ex.ToString());
                    //throw;
#endif
                }
            }
        }

        private static void RtbSelectionChangedBaselineAlignment(TextRange selectionTextRange) {
            if (!DependencyProperty.UnsetValue.Equals(selectionTextRange.GetPropertyValue(Inline.BaselineAlignmentProperty))) {
                switch (selectionTextRange.GetPropertyValue(Inline.BaselineAlignmentProperty)) {
                    case (BaselineAlignment.Top): {
                            MainW.FontConf.topScript.IsChecked = true;
                            break;
                        }
                    case (BaselineAlignment.Superscript): {
                            MainW.FontConf.superscript.IsChecked = true;
                            break;
                        }
                    case (BaselineAlignment.TextTop): {
                            MainW.FontConf.texttopScript.IsChecked = true;
                            break;
                        }
                    case (BaselineAlignment.Center): {
                            MainW.FontConf.centerScript.IsChecked = true;
                            break;
                        }
                    case (BaselineAlignment.Subscript): {
                            MainW.FontConf.subscript.IsChecked = true;
                            break;
                        }
                    case (BaselineAlignment.TextBottom): {
                            MainW.FontConf.textbottomScript.IsChecked = true;
                            break;
                        }
                    case (BaselineAlignment.Bottom): {
                            MainW.FontConf.bottomScript.IsChecked = true;
                            break;
                        }
                    case (BaselineAlignment.Baseline): {
                            MainW.FontConf.baseScript.IsChecked = true;
                            break;
                        }
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private static void RtbSelectionChangedBGColor(TextRange selectionTextRange) {
            if (!DependencyProperty.UnsetValue.Equals(selectionTextRange.GetPropertyValue(TextElement.BackgroundProperty))) {
                SolidColorBrush newBrush = null;
                newBrush = (SolidColorBrush)selectionTextRange.GetPropertyValue(TextElement.BackgroundProperty);
                SolidColorBrush newBrush2 = null;
                newBrush2 = (SolidColorBrush)MainW.rtb.Selection.Start.Paragraph.Background;
                SolidColorBrush newBrush3 = null;
                try {
                    newBrush3 = (SolidColorBrush)((Span)((Run)MainW.rtb.Selection.Start.Parent).Parent).Background;
                }
                catch (Exception ex) {
#if DEBUG
                    //MessageBox.Show("DEBUG: "+ex.ToString());
                    System.Diagnostics.Debug.WriteLine("DEBUG: "+ex.ToString());
                    //throw;
#endif
                }

                if (newBrush == null) {
                    if (newBrush2 == null) {
                        if (newBrush3 == null) {
                            MainW.FontConf.ClrPcker_Bg.SelectedColor = Colors.Transparent;
                        }
                        else {
                            MainW.FontConf.ClrPcker_Bg.SelectedColor = newBrush3.Color;
                        }
                    }
                    else {
                        MainW.FontConf.ClrPcker_Bg.SelectedColor = newBrush2.Color;
                    }
                }
                else {
                    MainW.FontConf.ClrPcker_Bg.SelectedColor = newBrush.Color;
                }
            }
        }

        private static void RtbSelectionChangedFlowDirection(TextRange selectionTextRange) {
            if (!DependencyProperty.UnsetValue.Equals(selectionTextRange.GetPropertyValue(Block.FlowDirectionProperty))) {
                //flow direction is working, see examples: https://stackoverflow.com/questions/7045676/wpf-how-does-flowdirection-righttoleft-change-a-string
                if (((FlowDirection)selectionTextRange.GetPropertyValue(Block.FlowDirectionProperty)).Equals(FlowDirection.RightToLeft)) {
                    MainW.FontConf.FlowDirRTL.IsChecked = true;
                }
                else {
                    MainW.FontConf.FlowDirRTL.IsChecked = false;
                }
            }
        }

        private static void RtbSelectionChangedFontColor(TextRange selectionTextRange) {
            if (!DependencyProperty.UnsetValue.Equals(selectionTextRange.GetPropertyValue(TextElement.ForegroundProperty))) {
                SolidColorBrush newBrush = null;
                newBrush = (SolidColorBrush)selectionTextRange.GetPropertyValue(TextElement.ForegroundProperty);
                MainW.FontConf.ClrPcker_Font.SelectedColor = newBrush.Color;
            }
        }

        private static void RtbSelectionChangedFontFamily(TextRange selectionTextRange) {
            if (!DependencyProperty.UnsetValue.Equals(selectionTextRange.GetPropertyValue(TextElement.FontFamilyProperty))) {
                object fontfamily = selectionTextRange.GetPropertyValue(TextElement.FontFamilyProperty);
                MainW.FontConf.lstFamily.SelectedItem = fontfamily;
            }
        }

        private static void RtbSelectionChangedFontSize(TextRange selectionTextRange) {
            if (!DependencyProperty.UnsetValue.Equals(selectionTextRange.GetPropertyValue(TextElement.FontSizeProperty))) {
                MainW.FontConf.fontSizeSlider.Value = (double)selectionTextRange.GetPropertyValue(TextElement.FontSizeProperty);
            }
        }

        private static void RtbSelectionChangedFontType(TextRange selectionTextRange) {
            FamilyTypeface fonttype = new FamilyTypeface();
            if (!DependencyProperty.UnsetValue.Equals(selectionTextRange.GetPropertyValue(TextElement.FontStyleProperty))) {
                FontStyle fontsyle = (FontStyle)selectionTextRange.GetPropertyValue(TextElement.FontStyleProperty);
                fonttype.Style = fontsyle;
            }
            if (!DependencyProperty.UnsetValue.Equals(selectionTextRange.GetPropertyValue(TextElement.FontWeightProperty))) {
                FontWeight fontweight = (FontWeight)selectionTextRange.GetPropertyValue(TextElement.FontWeightProperty);
                fonttype.Weight = fontweight;
            }
            if (!DependencyProperty.UnsetValue.Equals(selectionTextRange.GetPropertyValue(TextElement.FontStretchProperty))) {
                FontStretch fontStretch = (FontStretch)selectionTextRange.GetPropertyValue(TextElement.FontStretchProperty);
                fonttype.Stretch = fontStretch;
            }
            MainW.FontConf.lstTypefaces.SelectedItem = fonttype;
        }

        private static void RtbSelectionChangedLineHeight(TextRange selectionTextRange) {
            if ((!DependencyProperty.UnsetValue.Equals(selectionTextRange.GetPropertyValue(Block.LineHeightProperty))) &&
                                        (!double.IsNaN((double)selectionTextRange.GetPropertyValue(Block.LineHeightProperty)))) {
                MainW.FontConf.lineHeightSlider.Value = (double)selectionTextRange.GetPropertyValue(Block.LineHeightProperty);
            }
            else {
                MainW.FontConf.lineHeightSlider.Value = MainW.FontConf.fontSizeSlider.Value;
            }
        }

        private static void RtbSelectionChangedTextAlignment(TextRange selectionTextRange) {
            if (!DependencyProperty.UnsetValue.Equals(selectionTextRange.GetPropertyValue(Block.TextAlignmentProperty))) {
                switch (selectionTextRange.GetPropertyValue(Block.TextAlignmentProperty)) {
                    case (TextAlignment.Left): {
                            MainW.FontConf.leftAlign.IsChecked = true;
                            break;
                        }
                    case (TextAlignment.Center): {
                            MainW.FontConf.centerAlign.IsChecked = true;
                            break;
                        }
                    case (TextAlignment.Right): {
                            MainW.FontConf.rightAlign.IsChecked = true;
                            break;
                        }
                    case (TextAlignment.Justify): {
                            MainW.FontConf.justifyAlign.IsChecked = true;
                            break;
                        }
                }
            }
        }

        private static void RtbSelectionChangedTextDecorations(TextRange selectionTextRange) {
            if (!DependencyProperty.UnsetValue.Equals(selectionTextRange.GetPropertyValue(Inline.TextDecorationsProperty))) {
                TextDecorationCollection temp = (TextDecorationCollection)selectionTextRange.GetPropertyValue(Inline.TextDecorationsProperty);
                //FontConf.textrun.TextDecorations = null;
                //FontConf.textrun.TextDecorations = temp.Clone();//this works, but is overriden on textrun.TextDecorations.Clear(); of UpdateStrikethrough in fontConfig
                MainW.FontConf.Baseline.IsChecked = false;
                MainW.FontConf.OverLine.IsChecked = false;
                MainW.FontConf.Strikethrough.IsChecked = false;
                MainW.FontConf.Underline.IsChecked = false;
                //FontConf.UpdateStrikethrough();
                foreach (TextDecoration decor in temp) {
                    switch (decor.Location) {
                        case (TextDecorationLocation.Baseline): {
                                MainW.FontConf.Baseline.IsChecked = true;
                                break;
                            }
                        case (TextDecorationLocation.OverLine): {
                                MainW.FontConf.OverLine.IsChecked = true;
                                break;
                            }
                        case (TextDecorationLocation.Strikethrough): {
                                MainW.FontConf.Strikethrough.IsChecked = true;
                                break;
                            }
                        case (TextDecorationLocation.Underline): {
                                MainW.FontConf.Underline.IsChecked = true;
                                break;
                            }
                    }
                }
            }
        }

        #endregion Text Functions

        #region ToolBar functions

        public static void UpdateItemCheckedState(System.Windows.Controls.Primitives.ToggleButton button, DependencyProperty formattingProperty, object expectedValue) {
            if (button != null) {
                object currentValue = MainW.rtb.Selection.GetPropertyValue(formattingProperty);
                button.IsChecked = (currentValue != DependencyProperty.UnsetValue) && currentValue != null && currentValue.Equals(expectedValue);
            }
        }

        public static void UpdateToggleButtonState() {
            UpdateItemCheckedState(MainW._btnBold, TextElement.FontWeightProperty, FontWeights.Bold);
            UpdateItemCheckedState(MainW._btnItalic, TextElement.FontStyleProperty, FontStyles.Italic);

            UpdateItemCheckedState(MainW._btnTopscript, Inline.BaselineAlignmentProperty, BaselineAlignment.Top);
            UpdateItemCheckedState(MainW._btnTextTopscript, Inline.BaselineAlignmentProperty, BaselineAlignment.TextTop);
            UpdateItemCheckedState(MainW._btnSuperscript, Inline.BaselineAlignmentProperty, BaselineAlignment.Superscript);
            UpdateItemCheckedState(MainW._btnCentercript, Inline.BaselineAlignmentProperty, BaselineAlignment.Center);
            UpdateItemCheckedState(MainW._btnSubscript, Inline.BaselineAlignmentProperty, BaselineAlignment.Subscript);
            UpdateItemCheckedState(MainW._btnTextBottomscript, Inline.BaselineAlignmentProperty, BaselineAlignment.TextBottom);
            UpdateItemCheckedState(MainW._btnBottomscript, Inline.BaselineAlignmentProperty, BaselineAlignment.Bottom);
            UpdateItemCheckedState(MainW._btnBasescript, Inline.BaselineAlignmentProperty, BaselineAlignment.Baseline);

            UpdateItemCheckedState(MainW._btnAlignLeft, Block.TextAlignmentProperty, TextAlignment.Left);
            UpdateItemCheckedState(MainW._btnAlignCenter, Block.TextAlignmentProperty, TextAlignment.Center);
            UpdateItemCheckedState(MainW._btnAlignRight, Block.TextAlignmentProperty, TextAlignment.Right);
            UpdateItemCheckedState(MainW._btnAlignJustify, Block.TextAlignmentProperty, TextAlignment.Justify);

            UpdateItemCheckedState(MainW._btnFlowDirLTR, Block.FlowDirectionProperty, FlowDirection.LeftToRight);
            UpdateItemCheckedState(MainW._btnFlowDirRTL, Block.FlowDirectionProperty, FlowDirection.RightToLeft);

            /*
            UpdateItemCheckedState(_btnOverLine        , Inline.TextDecorationsProperty, TextDecorations.OverLine);
            UpdateItemCheckedState(_btnStrikethrough   , Inline.TextDecorationsProperty, TextDecorations.Strikethrough);
            UpdateItemCheckedState(_btnBaseline        , Inline.TextDecorationsProperty, TextDecorations.Baseline);
            UpdateItemCheckedState(_btnUnderline       , Inline.TextDecorationsProperty, TextDecorations.Underline);
            */
        }

        public static void UpdateDecorators() {
            if (MainW.rtb.Selection != null) {
                MainW._btnOverLine.IsChecked = false;
                MainW._btnStrikethrough.IsChecked = false;
                MainW._btnBaseline.IsChecked = false;
                MainW._btnUnderline.IsChecked = false;
                if (!DependencyProperty.UnsetValue.Equals(MainW.rtb.Selection.GetPropertyValue(Inline.TextDecorationsProperty))) {
                    TextDecorationCollection temp = (TextDecorationCollection)MainW.rtb.Selection.GetPropertyValue(Inline.TextDecorationsProperty);
                    foreach (TextDecoration decor in temp) {
                        switch (decor.Location) {
                            case (TextDecorationLocation.Baseline): {
                                    MainW._btnBaseline.IsChecked = true;
                                    break;
                                }
                            case (TextDecorationLocation.OverLine): {
                                    MainW._btnOverLine.IsChecked = true;
                                    break;
                                }
                            case (TextDecorationLocation.Strikethrough): {
                                    MainW._btnStrikethrough.IsChecked = true;
                                    break;
                                }
                            case (TextDecorationLocation.Underline): {
                                    MainW._btnUnderline.IsChecked = true;
                                    break;
                                }
                        }
                    }
                }
            }
        }

        public static void UpdateSelectionListType() {
            Paragraph startParagraph = MainW.rtb.Selection.Start.Paragraph;
            Paragraph endParagraph = MainW.rtb.Selection.End.Paragraph;
            if (startParagraph != null && endParagraph != null && (startParagraph.Parent is ListItem) && (endParagraph.Parent is ListItem) && ReferenceEquals(((ListItem)startParagraph.Parent).List, ((ListItem)endParagraph.Parent).List)) {
                MainW._btnToggleBox.IsChecked = false;
                MainW._btnToggleCircle.IsChecked = false;
                MainW._btnToggleNumbering.IsChecked = false;
                MainW._btnToggleBullets.IsChecked = false;
                MainW._btnToggleLowerLatin.IsChecked = false;
                MainW._btnToggleLowerRoman.IsChecked = false;
                MainW._btnToggleSquare.IsChecked = false;
                MainW._btnToggleUpperLatin.IsChecked = false;
                MainW._btnToggleUpperRoman.IsChecked = false;

                TextMarkerStyle markerStyle = ((ListItem)startParagraph.Parent).List.MarkerStyle;
                switch (markerStyle) {
                    case (TextMarkerStyle.Box): {
                            MainW._btnToggleBox.IsChecked = true;
                            break;
                        }
                    case (TextMarkerStyle.Circle): {
                            MainW._btnToggleCircle.IsChecked = true;
                            break;
                        }
                    case (TextMarkerStyle.Decimal): {
                            MainW._btnToggleNumbering.IsChecked = true;
                            break;
                        }
                    case (TextMarkerStyle.Disc): {
                            MainW._btnToggleBullets.IsChecked = true;
                            break;
                        }
                    case (TextMarkerStyle.LowerLatin): {
                            MainW._btnToggleLowerLatin.IsChecked = true;
                            break;
                        }
                    case (TextMarkerStyle.LowerRoman): {
                            MainW._btnToggleLowerRoman.IsChecked = true;
                            break;
                        }
                    case (TextMarkerStyle.Square): {
                            MainW._btnToggleSquare.IsChecked = true;
                            break;
                        }
                    case (TextMarkerStyle.UpperLatin): {
                            MainW._btnToggleUpperLatin.IsChecked = true;
                            break;
                        }
                    case (TextMarkerStyle.UpperRoman): {
                            MainW._btnToggleUpperRoman.IsChecked = true;
                            break;
                        }
                }
            }
            else {
                MainW._btnToggleBox.IsChecked = false;
                MainW._btnToggleCircle.IsChecked = false;
                MainW._btnToggleNumbering.IsChecked = false;
                MainW._btnToggleBullets.IsChecked = false;
                MainW._btnToggleLowerLatin.IsChecked = false;
                MainW._btnToggleLowerRoman.IsChecked = false;
                MainW._btnToggleSquare.IsChecked = false;
                MainW._btnToggleUpperLatin.IsChecked = false;
                MainW._btnToggleUpperRoman.IsChecked = false;
            }
        }

        public static void UpdateSelectedFontFamily() {
            object value = MainW.rtb.Selection.GetPropertyValue(TextElement.FontFamilyProperty);
            FontFamily currentFontFamily = (FontFamily)((value == DependencyProperty.UnsetValue) ? null : value);
            if (currentFontFamily != null) {
                MainW._fontFamily.SelectedItem = currentFontFamily;
            }
        }

        public static void UpdateSelectedFontSize() {
            object value = MainW.rtb.Selection.GetPropertyValue(TextElement.FontSizeProperty);
            MainW._fontSize.SelectedValue = (value == DependencyProperty.UnsetValue) ? null : value;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "System.Windows.Documents.InlineUIContainer")]
        public static void SelectImg() {
            OpenFileDialog dlg = new OpenFileDialog {
                Filter = "Image files (*.jpg, *.jpeg,*.gif, *.png) | *.jpg; *.jpeg; *.gif; *.png"
            };
            bool? result = dlg.ShowDialog();
            if (result.Value) {
                Uri uri = new Uri(dlg.FileName, UriKind.Relative);
                System.Windows.Media.Imaging.BitmapImage bitmapImg = (System.Windows.Media.Imaging.BitmapImage)BitmapFromUri(uri);
                System.Windows.Controls.Image image = new System.Windows.Controls.Image {
                    Stretch = Stretch.Fill,
                    Width = 250,
                    Height = 200,
                    Source = bitmapImg
                };
                TextPointer tp = MainW.rtb.CaretPosition.GetInsertionPosition(LogicalDirection.Forward);
#pragma warning disable RECS0026 // Possible unassigned object created by 'new'
                new InlineUIContainer(image, tp);
#pragma warning restore RECS0026 // Possible unassigned object created by 'new'
            }
        }

        public static void ApplyTextDecorators() {
            if (MainW.rtb.Selection != null) {
                TextDecorationCollection decor = new TextDecorationCollection();

                if (MainW._btnOverLine.IsChecked.Value) {
                    decor.Add(TextDecorations.OverLine);
                }
                if (MainW._btnStrikethrough.IsChecked.Value) {
                    decor.Add(TextDecorations.Strikethrough);
                }
                if (MainW._btnBaseline.IsChecked.Value) {
                    decor.Add(TextDecorations.Baseline);
                }
                if (MainW._btnUnderline.IsChecked.Value) {
                    decor.Add(TextDecorations.Underline);
                }
                decor.Freeze();
                if (decor != null) {
                    //rtb.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, null);
                    MainW.rtb.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, decor);
                }
            }
        }

        public static void ApplyTextScript(object sender) {
            if (MainW.rtb.Selection != null) {
                System.Windows.Controls.Primitives.ToggleButton btn = (System.Windows.Controls.Primitives.ToggleButton)sender;
                if (btn != null && btn.IsChecked.Value) {
                    MainW._btnTopscript.IsChecked = false;
                    MainW._btnSuperscript.IsChecked = false;
                    MainW._btnTextTopscript.IsChecked = false;
                    MainW._btnCentercript.IsChecked = false;
                    MainW._btnSubscript.IsChecked = false;
                    MainW._btnTextBottomscript.IsChecked = false;
                    MainW._btnBottomscript.IsChecked = false;
                    MainW._btnBasescript.IsChecked = false;
                    switch (btn.Name) {
                        case nameof(MainW._btnTopscript): {
                                MainW.rtb.Selection.ApplyPropertyValue(Inline.BaselineAlignmentProperty, BaselineAlignment.Top);
                                MainW._btnTopscript.IsChecked = true;
                                break;
                            }
                        case nameof(MainW._btnSuperscript): {
                                MainW.rtb.Selection.ApplyPropertyValue(Inline.BaselineAlignmentProperty, BaselineAlignment.Superscript);
                                MainW._btnSuperscript.IsChecked = true;
                                break;
                            }
                        case nameof(MainW._btnTextTopscript): {
                                MainW.rtb.Selection.ApplyPropertyValue(Inline.BaselineAlignmentProperty, BaselineAlignment.TextTop);
                                MainW._btnTextTopscript.IsChecked = true;
                                break;
                            }
                        case nameof(MainW._btnCentercript): {
                                MainW.rtb.Selection.ApplyPropertyValue(Inline.BaselineAlignmentProperty, BaselineAlignment.Center);
                                MainW._btnCentercript.IsChecked = true;
                                break;
                            }
                        case nameof(MainW._btnSubscript): {
                                MainW.rtb.Selection.ApplyPropertyValue(Inline.BaselineAlignmentProperty, BaselineAlignment.Subscript);
                                MainW._btnSubscript.IsChecked = true;
                                break;
                            }
                        case nameof(MainW._btnTextBottomscript): {
                                MainW.rtb.Selection.ApplyPropertyValue(Inline.BaselineAlignmentProperty, BaselineAlignment.TextBottom);
                                MainW._btnTextBottomscript.IsChecked = true;
                                break;
                            }
                        case nameof(MainW._btnBottomscript): {
                                MainW.rtb.Selection.ApplyPropertyValue(Inline.BaselineAlignmentProperty, BaselineAlignment.Bottom);
                                MainW._btnBottomscript.IsChecked = true;
                                break;
                            }
                        case nameof(MainW._btnBasescript): {
                                MainW.rtb.Selection.ApplyPropertyValue(Inline.BaselineAlignmentProperty, BaselineAlignment.Baseline);
                                MainW._btnBasescript.IsChecked = true;
                                break;
                            }
                    }
                }
                else {
                    MainW._btnTopscript.IsChecked = false;
                    MainW._btnSuperscript.IsChecked = false;
                    MainW._btnTextTopscript.IsChecked = false;
                    MainW._btnCentercript.IsChecked = false;
                    MainW._btnSubscript.IsChecked = false;
                    MainW._btnTextBottomscript.IsChecked = false;
                    MainW._btnBottomscript.IsChecked = false;
                    MainW.rtb.Selection.ApplyPropertyValue(Inline.BaselineAlignmentProperty, BaselineAlignment.Baseline);
                    MainW._btnBasescript.IsChecked = true;
                }
            }
        }

        public static void ApplyFlowDir() {
            if (MainW.rtb.Selection != null) {
                if (MainW._btnFlowDirLTR.IsChecked.Value) {
                    MainW.rtb.Selection.ApplyPropertyValue(Block.FlowDirectionProperty, FlowDirection.LeftToRight);
                }
                else {
                    MainW.rtb.Selection.ApplyPropertyValue(Block.FlowDirectionProperty, FlowDirection.RightToLeft);
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static void ApplyFontSize(System.Windows.Controls.SelectionChangedEventArgs e) {
            try {
                if (e != null) {
                    if (e.AddedItems.Count > 0) {
                        ApplyPropertyValueToSelectedText(TextElement.FontSizeProperty, e.AddedItems[0]);
                    }
                }
            }
            catch (Exception ex) {
#if DEBUG
                MessageBox.Show("DEBUG: "+ex.ToString());
                //throw;
#endif
            }
        }

        public static void ApplyPropertyValueToSelectedText(DependencyProperty formattingProperty, object value) {
            if (MainW.rtb?.Selection != null) {
                if (value == null) {
                    return;
                }
                MainW.rtb.Selection.ApplyPropertyValue(formattingProperty, value);
            }
        }

        public static void ApplyFontFamily(System.Windows.Controls.SelectionChangedEventArgs e) {
            if (e != null) {
                if (e.AddedItems.Count > 0) {
                    FontFamily editValue = (FontFamily)e.AddedItems[0];
                    ApplyPropertyValueToSelectedText(TextElement.FontFamilyProperty, editValue);
                }
            }
        }

        public static void ApplyFontColor() {
            if (MainW.rtb != null && MainW.rtb.Selection != null && !MainW.rtb.Selection.IsEmpty) {
                SolidColorBrush newBrush = new SolidColorBrush(MainW.ClrPcker_Font.SelectedColor.Value);
                newBrush.Freeze();
                MainW.rtb.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, newBrush);
            }
        }

        public static void ApplyFontBackColor() {
            if (MainW.rtb != null && MainW.rtb.Selection != null && !MainW.rtb.Selection.IsEmpty) {
                SolidColorBrush newBrush = new SolidColorBrush(MainW.ClrPcker_FontBack.SelectedColor.Value);
                if ((newBrush != null) && (newBrush.Color.A == 0)) {
                    newBrush = null;
                }
                MainW.rtb.Selection.ApplyPropertyValue(TextElement.BackgroundProperty, newBrush);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public static void ApplyListType(object sender) {
            if (MainW.rtb.Selection != null) {
                System.Windows.Controls.Primitives.ToggleButton btn = (System.Windows.Controls.Primitives.ToggleButton)sender;
                if (btn != null) {
                    Paragraph startParagraph = MainW.rtb.Selection.Start.Paragraph;
                    Paragraph endParagraph = MainW.rtb.Selection.End.Paragraph;
                    if (startParagraph != null && endParagraph != null && (startParagraph.Parent is ListItem) && (endParagraph.Parent is ListItem) && ReferenceEquals(((ListItem)startParagraph.Parent).List, ((ListItem)endParagraph.Parent).List)) {
                        //if there is a list only change TextMarkerStyle
                    }
                    else {
                        EditingCommands.ToggleBullets.Execute(null, MainW.rtb);
                    }
                    if (btn.IsChecked.Value) {
                        MainW._btnToggleBox.IsChecked = false;
                        MainW._btnToggleCircle.IsChecked = false;
                        MainW._btnToggleNumbering.IsChecked = false;
                        MainW._btnToggleBullets.IsChecked = false;
                        MainW._btnToggleLowerLatin.IsChecked = false;
                        MainW._btnToggleLowerRoman.IsChecked = false;
                        MainW._btnToggleSquare.IsChecked = false;
                        MainW._btnToggleUpperLatin.IsChecked = false;
                        MainW._btnToggleUpperRoman.IsChecked = false;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        switch (btn.Name) {
                            case nameof(MainW._btnToggleBox): {
                                    MainW._btnToggleBox.IsChecked = true;
                                    ListBulletTypeAsync(MainW.rtb.Selection.Start.Paragraph, MainW.rtb.Selection.End.Paragraph, TextMarkerStyle.Box);
                                    break;
                                }
                            case nameof(MainW._btnToggleCircle): {
                                    MainW._btnToggleCircle.IsChecked = true;
                                    ListBulletTypeAsync(MainW.rtb.Selection.Start.Paragraph, MainW.rtb.Selection.End.Paragraph, TextMarkerStyle.Circle);
                                    break;
                                }
                            case nameof(MainW._btnToggleNumbering): {
                                    MainW._btnToggleNumbering.IsChecked = true;
                                    ListBulletTypeAsync(MainW.rtb.Selection.Start.Paragraph, MainW.rtb.Selection.End.Paragraph, TextMarkerStyle.Decimal);
                                    break;
                                }
                            case nameof(MainW._btnToggleBullets): {
                                    MainW._btnToggleBullets.IsChecked = true;
                                    ListBulletTypeAsync(MainW.rtb.Selection.Start.Paragraph, MainW.rtb.Selection.End.Paragraph, TextMarkerStyle.Disc);
                                    break;
                                }
                            case nameof(MainW._btnToggleLowerLatin): {
                                    MainW._btnToggleLowerLatin.IsChecked = true;
                                    ListBulletTypeAsync(MainW.rtb.Selection.Start.Paragraph, MainW.rtb.Selection.End.Paragraph, TextMarkerStyle.LowerLatin);
                                    break;
                                }
                            case nameof(MainW._btnToggleLowerRoman): {
                                    MainW._btnToggleLowerRoman.IsChecked = true;
                                    ListBulletTypeAsync(MainW.rtb.Selection.Start.Paragraph, MainW.rtb.Selection.End.Paragraph, TextMarkerStyle.LowerRoman);
                                    break;
                                }
                            case nameof(MainW._btnToggleSquare): {
                                    MainW._btnToggleSquare.IsChecked = true;
                                    ListBulletTypeAsync(MainW.rtb.Selection.Start.Paragraph, MainW.rtb.Selection.End.Paragraph, TextMarkerStyle.Square);
                                    break;
                                }
                            case nameof(MainW._btnToggleUpperLatin): {
                                    MainW._btnToggleUpperLatin.IsChecked = true;
                                    ListBulletTypeAsync(MainW.rtb.Selection.Start.Paragraph, MainW.rtb.Selection.End.Paragraph, TextMarkerStyle.UpperLatin);
                                    break;
                                }
                            case nameof(MainW._btnToggleUpperRoman): {
                                    MainW._btnToggleUpperRoman.IsChecked = true;
                                    ListBulletTypeAsync(MainW.rtb.Selection.Start.Paragraph, MainW.rtb.Selection.End.Paragraph, TextMarkerStyle.UpperRoman);
                                    break;
                                }
                        }
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    }
                    else {//if unchecking
                        TextMarkerStyle markerStyle = ((ListItem)startParagraph.Parent).List.MarkerStyle;
                        if (markerStyle != TextMarkerStyle.Disc && markerStyle != TextMarkerStyle.Box && markerStyle != TextMarkerStyle.Circle && markerStyle != TextMarkerStyle.Square) {
                            EditingCommands.ToggleBullets.Execute(null, MainW.rtb);
                        }
                        EditingCommands.ToggleBullets.Execute(null, MainW.rtb);
                    }
                }
            }
        }

        private async static System.Threading.Tasks.Task ListBulletTypeAsync(Paragraph startParagraph, Paragraph endParagraph, TextMarkerStyle textMarker) {
            await System.Threading.Tasks.Task.Delay(100);
            if (startParagraph != null && endParagraph != null && (startParagraph.Parent is ListItem) && (endParagraph.Parent is ListItem) && ReferenceEquals(((ListItem)startParagraph.Parent).List, ((ListItem)endParagraph.Parent).List)) {
                ((ListItem)startParagraph.Parent).List.MarkerStyle = textMarker;
            }
        }

        public static void FilterFontFamilyComboBox() {
            if (!MainW._fontFamily.IsDropDownOpen) {
                MainW._fontFamily.IsDropDownOpen = true;
            }

            System.Windows.Data.CollectionView itemsViewOriginal = (System.Windows.Data.CollectionView)System.Windows.Data.CollectionViewSource.GetDefaultView(MainW._fontFamily.ItemsSource);

            itemsViewOriginal.Filter = ((o) => {
                /*if (String.IsNullOrEmpty(_fontFamily.Text)) return true;
                else
                {
                    if (((string)o.ToString()).Contains(_fontFamily.Text)) return true;
                    else return false;
                }*/
                //1 line:
                return string.IsNullOrEmpty(MainW._fontFamily.Text) || o.ToString().Contains(MainW._fontFamily.Text);
            });

            //itemsViewOriginal.Refresh();

            // if datasource is a DataView, then apply RowFilter as below and replace above logic with below one
            /*
             DataView view = (DataView) _fontFamily.ItemsSource;
             view.RowFilter = ("Name like '*" + _fontFamily.Text + "*'");
            */
        }

        #endregion ToolBar functions
    }
}