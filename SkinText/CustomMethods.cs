﻿using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace SkinText {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public static class CustomMethods {
        private static string appDataPath;
        private static int autoSaveTimer = 5 * 60 * 1000;
        private static string currentSkin;
        private static bool fileChanged;
        private static string filepath;
        private static string imagepath;
        private static MainWindow mainW;
        public static string AppDataPath { get => appDataPath; set => appDataPath = value; }
        public static int AutoSaveTimer { get => autoSaveTimer; set => autoSaveTimer = value; }
        public static string CurrentSkin { get => currentSkin; set => currentSkin = value; }
        public static bool FileChanged { get => fileChanged; set => fileChanged = value; }
        public static string Filepath { get => filepath; set => filepath = value; }

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
        /// <para>Closes <see cref="MainWindow.FontConf"/> and <see cref="WindowConfig"/>, then calls <see cref="SaveConfig"/></para>
        /// Called from <see cref="MainWindow.Window_Closing"/>
        /// </summary>
        public static void ExitProgram() {
            MainW.FontConf.Close();
            MainW.WinConfig.Close();
            MainW.AdvConf.Close();
            SaveConfig();
        }

        /// <summary>
        /// After changing the size of the RichTextBox by code (FirstLoad,readConfig,LoadDefaults)
        /// <para>it's necessary to make it <see cref="GridUnitType.Star"/> again with the ActualWidth/ActualHeight
        /// otherwise it uses static values</para>
        /// </summary>
        public static void FixResizeTextBox() {
            MainW.grid.ColumnDefinitions[0].Width = new GridLength(MainW.grid.ColumnDefinitions[0].ActualWidth, GridUnitType.Star);
            MainW.grid.ColumnDefinitions[1].Width = new GridLength(MainW.grid.ColumnDefinitions[1].ActualWidth, GridUnitType.Star);
            MainW.grid.ColumnDefinitions[2].Width = new GridLength(MainW.grid.ColumnDefinitions[2].ActualWidth, GridUnitType.Star);
            MainW.grid.RowDefinitions[0].Height = new GridLength(MainW.grid.RowDefinitions[0].ActualHeight, GridUnitType.Star);
            MainW.grid.RowDefinitions[1].Height = new GridLength(MainW.grid.RowDefinitions[1].ActualHeight, GridUnitType.Star);
            MainW.grid.RowDefinitions[2].Height = new GridLength(MainW.grid.RowDefinitions[2].ActualHeight, GridUnitType.Star);
        }

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
                MessageBox.Show(ex.ToString());
                //throw;
                #endif
            }
        }

        /// <summary>
        /// Clears, Disposes, Makes null, Unload, Frees from memory and HDD the loaded image
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.GC.Collect")]
        public static void ImageClear() {
            Imagepath = "";
            MainW.backgroundimg.Source = null;
            WpfAnimatedGif.ImageBehavior.SetAnimatedSource(MainW.backgroundimg, null);
            XamlAnimatedGif.AnimationBehavior.SetSourceUri(MainW.backgroundimg, null);
            XamlAnimatedGif.AnimationBehavior.SetSourceStream(MainW.backgroundimg, null);
            if (MainW.window.Background.ToString() == Colors.Transparent.ToString()) {
                MainW.WinConfig.ClrPcker_WindowBackground.SelectedColor = (Color)ColorConverter.ConvertFromString("#85949494");
            }
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
        }

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

            //border size
                //default value due to RTB size dependency
                MainW.WinConfig.bordersize.Value = 5;

            //RTB size
                double borderSize = MainW.WinConfig.bordersize.Value;
                MainW.grid.ColumnDefinitions[1].Width = new GridLength(MainW.window.Width - (borderSize * 2 + 1));
                MainW.grid.RowDefinitions[1].Height = new GridLength(MainW.window.Height - (borderSize * 2 + 1));
                MainW.grid.ColumnDefinitions[0].Width = new GridLength(borderSize, GridUnitType.Star);
                MainW.grid.ColumnDefinitions[2].Width = new GridLength(borderSize, GridUnitType.Star);
                MainW.grid.RowDefinitions[0].Height = new GridLength(borderSize, GridUnitType.Star);
                MainW.grid.RowDefinitions[2].Height = new GridLength(borderSize, GridUnitType.Star);

            //Colors
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



            //text bg color

            //Colors:
            MainW.WinConfig.ClrPcker_BorderBackground.SelectedColor = ((SolidColorBrush)MainW.TryFindResource("BorderColorBrush")).Color;
                MainW.WinConfig.ClrPcker_WindowBackground.SelectedColor = ((SolidColorBrush)MainW.TryFindResource("MainWindowBackgroundColorBrush")).Color;
                MainW.WinConfig.ClrPcker_RTB_Background.SelectedColor = ((SolidColorBrush)MainW.TryFindResource("RTBBackgroundColorBrush")).Color;
            //Advanced Colors:
                MainW.AdvConf.ClrPcker_BackgroundColorBrush.SelectedColor = ((SolidColorBrush)MainW.TryFindResource("BackgroundColorBrush")).Color;
                MainW.AdvConf.ClrPcker_ButtonBackgroundCheckedColorBrush.SelectedColor = ((SolidColorBrush)MainW.TryFindResource("ButtonBackgroundCheckedColorBrush")).Color;
                MainW.AdvConf.ClrPcker_ButtonBackgroundColorBrush.SelectedColor = ((SolidColorBrush)MainW.TryFindResource("ButtonBackgroundColorBrush")).Color;
                MainW.AdvConf.ClrPcker_ButtonBackgroundMouseOverColorBrush.SelectedColor = ((SolidColorBrush)MainW.TryFindResource("ButtonBackgroundMouseOverColorBrush")).Color;
                MainW.AdvConf.ClrPcker_FontPickBackgroundColorBrush.SelectedColor = ((SolidColorBrush)MainW.TryFindResource("FontPickBackgroundColorBrush")).Color;
                MainW.AdvConf.ClrPcker_FontPickMouseOverBackgroundColorBrush.SelectedColor = ((SolidColorBrush)MainW.TryFindResource("FontPickMouseOverBackgroundColorBrush")).Color;
                MainW.AdvConf.ClrPcker_FontPickMouseOverBorderColorBrush.SelectedColor = ((SolidColorBrush)MainW.TryFindResource("FontPickMouseOverBorderColorBrush")).Color;
                MainW.AdvConf.ClrPcker_FontPickSelectedBackgroundColorBrush.SelectedColor = ((SolidColorBrush)MainW.TryFindResource("FontPickSelectedBackgroundColorBrush")).Color;
                MainW.AdvConf.ClrPcker_FontPickSelectedBorderColorBrush.SelectedColor = ((SolidColorBrush)MainW.TryFindResource("FontPickSelectedBorderColorBrush")).Color;
                MainW.AdvConf.ClrPcker_MenuBackgroundColorBrush.SelectedColor = ((SolidColorBrush)MainW.TryFindResource("MenuBackgroundColorBrush")).Color;
                MainW.AdvConf.ClrPcker_MenuItem1BorderColorBrush.SelectedColor = ((SolidColorBrush)MainW.TryFindResource("MenuItem1BorderColorBrush")).Color;
                MainW.AdvConf.ClrPcker_MenuItem2DisabledColorBrush.SelectedColor = ((SolidColorBrush)MainW.TryFindResource("MenuItem2DisabledColorBrush")).Color;
                MainW.AdvConf.ClrPcker_MenuItem2HighlightBorderColorBrush.SelectedColor = ((SolidColorBrush)MainW.TryFindResource("MenuItem2HighlightBorderColorBrush")).Color;
                MainW.AdvConf.ClrPcker_MenuItem2HighlightTextColorBrush.SelectedColor = ((SolidColorBrush)MainW.TryFindResource("MenuItem2HighlightTextColorBrush")).Color;
                MainW.AdvConf.ClrPcker_TextColorBrush.SelectedColor = ((SolidColorBrush)MainW.TryFindResource("TextColorBrush")).Color;
                MainW.AdvConf.ClrPcker_FontPickTextColorBrush.SelectedColor = ((SolidColorBrush)MainW.TryFindResource("FontPickTextColorBrush")).Color;
                MainW.AdvConf.ClrPcker_ButtonFrontColorBrush.SelectedColor = ((SolidColorBrush)MainW.TryFindResource("ButtonFrontColorBrush")).Color;
                MainW.AdvConf.ClrPcker_ButtonBorderMouseOverColorBrush.SelectedColor = ((SolidColorBrush)MainW.TryFindResource("ButtonBorderMouseOverColorBrush")).Color;
                MainW.AdvConf.ClrPcker_ButtonBorderCheckedColorBrush.SelectedColor = ((SolidColorBrush)MainW.TryFindResource("ButtonBorderCheckedColorBrush")).Color;

            //rotation angle
            MainW.WinConfig.slValue.Value = 0;

            //no file
                NewFile();

            //Auto Save Timer
                //TODO: add to load/save config and to windowConf to modify
                AutoSaveTimer = 5 * 60 * 1000;

            //gif_uses_ram
                //Default = false (use CPU)
                MainW.WinConfig.GifMethodCPU.IsChecked = true;

            //BG image clear
                //this makes Imagepath = "";
                ImageClear();

            //opacity
                MainW.WinConfig.imageopacityslider.Value = 100;
                MainW.WinConfig.windowopacityslider.Value = 100;
                MainW.WinConfig.textopacityslider.Value = 100;

            //checkboxes
                MainW.WinConfig.resizecheck.IsChecked = true;
                MainW.WinConfig.readOnlyCheck.IsChecked = false;
                MainW.WinConfig.spellcheck.IsChecked = false;
                MainW.WinConfig.alwaysontop.IsChecked = false;
                MainW.WinConfig.taskbarvisible.IsChecked = true;
                MainW.WinConfig.NotificationVisible.IsChecked = true;
                MainW.WinConfig.ResizeVisible.IsChecked = true;
                MainW.WinConfig.FlipXButton.IsChecked = false;
                MainW.WinConfig.FlipYButton.IsChecked = false;
                MainW.LineWrapMenuItem.IsChecked = true;
        }

        /// <summary>
        /// Will try to load <paramref name="imagepath"/> into <see cref="MainWindow.backgroundimg"/> and set <see cref="Imagepath"/> to either <paramref name="imagepath"/> or "" (<see cref="string.Empty"/>)
        /// </summary>
        /// <param name="imagepath"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static void LoadImage(string imagepath) {
            try {
                if (File.Exists(imagepath)) {
                    Uri uri = new Uri(imagepath);
                    if (Path.GetExtension(imagepath).ToUpperInvariant() == ".GIF") {
                        if (MainW.WinConfig.GifMethodCPU.IsChecked.Value) {//CPU Method
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
                    if (MainW.WinConfig.ClrPcker_WindowBackground.SelectedColor.Value.A == 255) {
                        MainW.WinConfig.ClrPcker_WindowBackground.SelectedColor = Color.Subtract(MainW.WinConfig.ClrPcker_WindowBackground.SelectedColor.Value, Color.FromArgb(155, 0, 0, 0));
                    }
                    //MainW.WinConfig.ClrPcker_WindowBackground.SelectedColor = Colors.Transparent;
                    uri = null;
                    Imagepath = imagepath; //set Global Imagepath
                }
                else {
                    throw new FileNotFoundException("Error: File not found", imagepath);
                }
            }
            catch (Exception ex) {
                if (!string.IsNullOrWhiteSpace(imagepath)) {
                    MessageBox.Show("Failed to Load Image:\r\n " + imagepath, "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.ServiceNotification);
                }
                ImageClear();
#if DEBUG
                MessageBox.Show(ex.ToString());
                //throw;
#endif
            }
        }

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
                try {
                    string currentImagepath = openFileDialog.FileName;
                    string newImagePath = AppDataPath + CurrentSkin + @"\bgImg" + Path.GetExtension(currentImagepath);//+ Path.GetFileName(imagepath);
                    ImageClear();
                    if (File.Exists(Imagepath)) {
                        File.Delete(Imagepath);
                    }
                    File.Copy(currentImagepath, newImagePath, true);

                    LoadImage(newImagePath);
                }
                catch (Exception ex) {
#if DEBUG
                    MessageBox.Show(ex.ToString());
                    //throw;
#endif
                }
            }
            else {
                ImageClear();
                //imagedir.Content = par.imagepath.Substring(par.imagepath.LastIndexOf("\\") + 1); // "" when imagepath is empty
                //imagedir.ToolTip = par.imagepath;
            }
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
                            if (!string.IsNullOrEmpty(line[0]) && !string.IsNullOrEmpty(line[1])) {
                                line[1] = line[1].Trim();
                                if (!string.IsNullOrWhiteSpace(line[1])) {
                                    ReadConfigLine(line);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex) {
                // The appdata folders dont exist
                //can be first open, let default values
#if DEBUG
                MessageBox.Show(ex.ToString());
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
                                    if (int1 <= MainW.WinConfig.bordersize.Maximum && int1 >= MainW.WinConfig.bordersize.Minimum) {
                                        MainW.WinConfig.bordersize.Value = int1;
                                    }
                                }
                                break;
                            }
                        case "TEXT_SIZE": {
                                array = line[1].Split(',');
                                if (double.TryParse(array[0], out double1) &&  //column 0 width
                                    double.TryParse(array[1], out double2) &&  //column 1 width
                                    double.TryParse(array[2], out double double3) &&  //column 2 width
                                    double.TryParse(array[3], out double double4) &&  //row 0 height
                                    double.TryParse(array[4], out double double5) &&  //row 1 height
                                    double.TryParse(array[5], out double double6))    //row 2 height
                                {
                                    if (double1 >= 0) {//column 0 width
                                        MainW.grid.ColumnDefinitions[0].Width = new GridLength(double1, GridUnitType.Star);
                                    }
                                    if (double3 >= 0) {//column 2 width
                                        MainW.grid.ColumnDefinitions[2].Width = new GridLength(double3, GridUnitType.Star);
                                    }
                                    if (double4 >= 0) {//row 0 height
                                        MainW.grid.RowDefinitions[0].Height = new GridLength(double4, GridUnitType.Star);
                                    }
                                    if (double6 >= 0) {//row 2 height
                                        MainW.grid.RowDefinitions[2].Height = new GridLength(double6, GridUnitType.Star);
                                    }
                                    double borderSize = MainW.WinConfig.bordersize.Value;
                                    if ((double2 < MainW.window.Width - (borderSize * 2 + 1)) && (double2 >= MainW.grid.ColumnDefinitions[1].MinWidth)) {//column 1 width
                                        MainW.grid.ColumnDefinitions[1].Width = new GridLength(double2);
                                    }
                                    else {//column 1 width
                                        MainW.grid.ColumnDefinitions[1].Width = new GridLength(MainW.window.Width - (borderSize * 2 + 1));
                                    }
                                    if ((double5 < MainW.window.Height - (borderSize * 2 + 1)) && (double5 >= MainW.grid.RowDefinitions[1].MinHeight)) {//row 1 height
                                        MainW.grid.RowDefinitions[1].Height = new GridLength(double5);
                                    }
                                    else {//row 1 height
                                        MainW.grid.RowDefinitions[1].Height = new GridLength(MainW.window.Height - (borderSize * 2 + 1));
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
                                    MainW.WinConfig.resizecheck.IsChecked = bool1;
                                }
                                break;
                            }
                        case "BORDER_COLOR": {
                                MainW.WinConfig.ClrPcker_BorderBackground.SelectedColor = (Color)ColorConverter.ConvertFromString(line[1]);
                                break;
                            }
                        case "WINDOW_COLOR": {
                                MainW.WinConfig.ClrPcker_WindowBackground.SelectedColor = (Color)ColorConverter.ConvertFromString(line[1]);
                                break;
                            }
                        case "TEXT_BG_COLOR": {
                                MainW.WinConfig.ClrPcker_RTB_Background.SelectedColor = (Color)ColorConverter.ConvertFromString(line[1]);
                                break;
                            }
                        case "ROTATION": {
                                if (double.TryParse(line[1], out double1)) //angle
                                {
                                    if (double1 <= MainW.WinConfig.slValue.Maximum && double1 >= MainW.WinConfig.slValue.Minimum) {
                                        MainW.WinConfig.slValue.Value = double1;
                                    }
                                }
                                break;
                            }
                        case "GIF_USES_RAM": {//GIF rendering method
                                if (bool.TryParse(line[1], out bool1)) {// true = RAM //DEFAULT = false (Use CPU)
                                    MainW.WinConfig.GifMethodRAM.IsChecked = bool1;
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
                                    if (double1 <= MainW.WinConfig.imageopacityslider.Maximum && double1 >= MainW.WinConfig.imageopacityslider.Minimum) {
                                        MainW.WinConfig.imageopacityslider.Value = double1;
                                    }
                                }
                                break;
                            }
                        case "TEXT_OPACITY": {
                                if (double.TryParse(line[1], out double1)) {
                                    if (double1 <= MainW.WinConfig.textopacityslider.Maximum && double1 >= MainW.WinConfig.textopacityslider.Minimum) {
                                        MainW.WinConfig.textopacityslider.Value = double1;
                                    }
                                }
                                break;
                            }
                        case "WINDOW_OPACITY": {
                                if (double.TryParse(line[1], out double1)) {
                                    if (double1 <= MainW.WinConfig.windowopacityslider.Maximum && double1 >= MainW.WinConfig.windowopacityslider.Minimum) {
                                        MainW.WinConfig.windowopacityslider.Value = double1;
                                    }
                                }
                                break;
                            }
                        case "READ_ONLY": {
                                if (bool.TryParse(line[1], out bool1)) {
                                    MainW.WinConfig.readOnlyCheck.IsChecked = bool1;
                                }
                                break;
                            }
                        case "SPELL_CHECK": {
                                if (bool.TryParse(line[1], out bool1)) {
                                    MainW.WinConfig.spellcheck.IsChecked = bool1;
                                }
                                break;
                            }
                        case "ALWAYS_ON_TOP": {
                                if (bool.TryParse(line[1], out bool1)) {
                                    MainW.WinConfig.alwaysontop.IsChecked = bool1;
                                }
                                break;
                            }
                        case "TASKBAR_ICON": {
                                if (bool.TryParse(line[1], out bool1)) {
                                    MainW.WinConfig.taskbarvisible.IsChecked = bool1;
                                }
                                break;
                            }
                        case "NOTIFICATION_ICON": {
                                if (bool.TryParse(line[1], out bool1)) {
                                    MainW.WinConfig.NotificationVisible.IsChecked = bool1;
                                }
                                break;
                            }
                        case "RESIZE_VISIBLE": {
                                if (bool.TryParse(line[1], out bool1)) {
                                    MainW.WinConfig.ResizeVisible.IsChecked = bool1;
                                }
                                break;
                            }
                        case "FLIP_RTB": {//flip rich text box (rendertransform)
                                array = line[1].Split(',');
                                if (bool.TryParse(array[0], out bool1) &&    // X
                                    bool.TryParse(array[1], out bool bool2)) // Y
                                {
                                    MainW.WinConfig.FlipXButton.IsChecked = bool1;
                                    MainW.WinConfig.FlipYButton.IsChecked = bool2;
                                }
                                break;
                            }
                        case "LINE_WRAP": {//Line Wrapping (Default on)
                                if (bool.TryParse(line[1], out bool1)) {
                                    MainW.LineWrapMenuItem.IsChecked = bool1;
                                }
                                break;
                            }
                            ////////////////////////////////////
                        case "BACKGROUNDCOLORBRUSH": {
                                MainW.AdvConf.ClrPcker_BackgroundColorBrush.SelectedColor = (Color)ColorConverter.ConvertFromString(line[1]);
                                break;
                            }
                        case "BUTTONBACKGROUNDCHECKEDCOLORBRUSH": {
                                MainW.AdvConf.ClrPcker_ButtonBackgroundCheckedColorBrush.SelectedColor = (Color)ColorConverter.ConvertFromString(line[1]);
                                break;
                            }
                        case "BUTTONBORDERCHECKEDCOLORBRUSH": {
                                MainW.AdvConf.ClrPcker_ButtonBorderCheckedColorBrush.SelectedColor = (Color)ColorConverter.ConvertFromString(line[1]);
                                break;
                            }
                        case "BUTTONBACKGROUNDCOLORBRUSH": {
                                MainW.AdvConf.ClrPcker_ButtonBackgroundColorBrush.SelectedColor = (Color)ColorConverter.ConvertFromString(line[1]);
                                break;
                            }
                        case "ButtonFrontColorBrush": {
                                MainW.AdvConf.ClrPcker_ButtonFrontColorBrush.SelectedColor = (Color)ColorConverter.ConvertFromString(line[1]);
                                break;
                            }
                        case "BUTTONBACKGROUNDMOUSEOVERCOLORBRUSH": {
                                MainW.AdvConf.ClrPcker_ButtonBackgroundMouseOverColorBrush.SelectedColor = (Color)ColorConverter.ConvertFromString(line[1]);
                                break;
                            }
                        case "BUTTONBORDERMOUSEOVERCOLORBRUSH": {
                                MainW.AdvConf.ClrPcker_ButtonBorderMouseOverColorBrush.SelectedColor = (Color)ColorConverter.ConvertFromString(line[1]);
                                break;
                            }
                        case "FONTPICKBACKGROUNDCOLORBRUSH": {
                                MainW.AdvConf.ClrPcker_FontPickBackgroundColorBrush.SelectedColor = (Color)ColorConverter.ConvertFromString(line[1]);
                                break;
                            }
                        case "FONTPICKMOUSEOVERBACKGROUNDCOLORBRUSH": {
                                MainW.AdvConf.ClrPcker_FontPickMouseOverBackgroundColorBrush.SelectedColor = (Color)ColorConverter.ConvertFromString(line[1]);
                                break;
                            }
                        case "FONTPICKMOUSEOVERBORDERCOLORBRUSH": {
                                MainW.AdvConf.ClrPcker_FontPickMouseOverBorderColorBrush.SelectedColor = (Color)ColorConverter.ConvertFromString(line[1]);
                                break;
                            }
                        case "FONTPICKSELECTEDBACKGROUNDCOLORBRUSH": {
                                MainW.AdvConf.ClrPcker_FontPickSelectedBackgroundColorBrush.SelectedColor = (Color)ColorConverter.ConvertFromString(line[1]);
                                break;
                            }
                        case "FONTPICKSELECTEDBORDERCOLORBRUSH": {
                                MainW.AdvConf.ClrPcker_FontPickSelectedBorderColorBrush.SelectedColor = (Color)ColorConverter.ConvertFromString(line[1]);
                                break;
                            }
                        case "MENUBACKGROUNDCOLORBRUSH": {
                                MainW.AdvConf.ClrPcker_MenuBackgroundColorBrush.SelectedColor = (Color)ColorConverter.ConvertFromString(line[1]);
                                break;
                            }
                        case "MENUITEM1BORDERCOLORBRUSH": {
                                MainW.AdvConf.ClrPcker_MenuItem1BorderColorBrush.SelectedColor = (Color)ColorConverter.ConvertFromString(line[1]);
                                break;
                            }
                        case "MENUITEM2DISABLEDCOLORBRUSH": {
                                MainW.AdvConf.ClrPcker_MenuItem2DisabledColorBrush.SelectedColor = (Color)ColorConverter.ConvertFromString(line[1]);
                                break;
                            }
                        case "MENUITEM2HIGHLIGHTBORDERCOLORBRUSH": {
                                MainW.AdvConf.ClrPcker_MenuItem2HighlightBorderColorBrush.SelectedColor = (Color)ColorConverter.ConvertFromString(line[1]);
                                break;
                            }
                        case "MENUITEM2HIGHLIGHTTEXTCOLORBRUSH": {
                                MainW.AdvConf.ClrPcker_MenuItem2HighlightTextColorBrush.SelectedColor = (Color)ColorConverter.ConvertFromString(line[1]);
                                break;
                            }
                        case "TEXTCOLORBRUSH": {
                                MainW.AdvConf.ClrPcker_TextColorBrush.SelectedColor = (Color)ColorConverter.ConvertFromString(line[1]);
                                break;
                            }
                        case "FONTPICKTEXTCOLORBRUSH": {
                                MainW.AdvConf.ClrPcker_FontPickTextColorBrush.SelectedColor = (Color)ColorConverter.ConvertFromString(line[1]);
                                break;
                            }

                    }
                }
                catch (Exception ex) {
                    //System.FormatException catched from BORDER_COLOR, WINDOW_COLOR, TEXT_BG_COLOR
#if DEBUG
                    MessageBox.Show(ex.ToString());
                    //throw;
#endif
                }
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
                            default: {//TODO: if no format open as txt, or should throw error?
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
                MessageBox.Show(ex.ToString());
                //throw;
                #endif
            }
        }

        /// <summary>
        /// Just a wrapper to call <see cref="LoadDefault"/> grid.UpdateLayout and <see cref="FixResizeTextBox"/>
        /// <para>Called from <see cref="MainWindow.Resettodefaults_Click"/> using <see cref="SaveChanges"/></para>
        /// </summary>
        public static void ResetDefaults() {
            LoadDefault();
            MainW.grid.UpdateLayout();
            FixResizeTextBox();
        }

        /// <summary>
        /// Sets the <see cref="MainWindow.rtb"/> background color to <paramref name="brush"/>
        /// </summary>
        /// <param name="brush"></param>
        public static void RtbBackgroundColor(Brush brush) {
            if (brush != null) {
                brush.Freeze();
                Application.Current.Resources["RTBBackgroundColorBrush"] = brush;
                //MainW.rtb.Background = brush;
                //MainW.FontConf.txtSampleText.Background = brush;
                brush = null;
            }
        }

        /// <summary>
        /// Sets the <see cref="MainWindow.rtb"/> resize borders color to <paramref name="brush"/>
        /// </summary>
        /// <param name="brush"></param>
        public static void RtbBorderColor(Brush brush) {
            if (brush != null) {
                brush.Freeze();
                Application.Current.Resources["BorderColorBrush"] = brush;
                //MainW.MyNotifyIcon. Border.BorderBrush= brush; //Fix: notify icon border color not updating
                // Resources["BorderColorBrush"]
                brush = null;
            }
        }

        /// <summary>
        /// Sets the <see cref="MainWindow.rtb"/> grid corners and splitters width and height according to the new <paramref name="borderSize"/>
        /// </summary>
        /// <param name="borderSize"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static void RtbBorderSize(double borderSize) {
            try {
                MainW.corner1.Width = MainW.corner1.Height = borderSize * 2;
                MainW.corner2.Width = MainW.corner2.Height = borderSize * 2;
                MainW.corner3.Width = MainW.corner3.Height = borderSize * 2;
                MainW.corner4.Width = MainW.corner4.Height = borderSize * 2;
                MainW.splitter1.Width = borderSize;
                MainW.splitter2.Height = borderSize;
                MainW.splitter3.Width = borderSize;
                MainW.splitter4.Height = borderSize;
                //MainW.RtbSizeChanged(null, null);
                RtbSizeChanged();
            }
            catch (Exception ex) {
#if DEBUG
                MessageBox.Show(ex.ToString());
                //throw;
#endif
            }
        }

        /// <summary>
        /// Flips the <see cref="MainWindow.rtb"/>
        /// <para>
        /// <see cref="ScaleTransform"/> takes doubles and scale the RTB to those values, with -1 fliping it and 1 being default
        /// </para>
        /// </summary>
        /// <param name="flipX"></param>
        /// <param name="flipY"></param>
        public static void RtbFlip(bool flipX, bool flipY) {
            MainW.FontConf.txtSampleText.RenderTransform = MainW.rtb.RenderTransform = new ScaleTransform(flipX ? -1 : 1, flipY ? -1 : 1);
        }

        /// <summary>
        /// Hides/shows the <see cref          ="MainWindow.rtb"/> resize borders
        /// </summary>
        /// <param name="hide"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static void RtbHideBorder(bool hide) {
            try {
                /* is also posible to:
                * MainW.corner1.Visibility = hide? Visibility.Visible : Visibility.Collapsed;
                * but I dont know if checking for hide? every line would hit perfomance so leaving as is
                */
                if (hide) {
                    MainW.corner1.Visibility = Visibility.Visible;
                    MainW.corner2.Visibility = Visibility.Visible;
                    MainW.corner3.Visibility = Visibility.Visible;
                    MainW.corner4.Visibility = Visibility.Visible;
                    MainW.splitter1.Visibility = Visibility.Visible;
                    MainW.splitter2.Visibility = Visibility.Visible;
                    MainW.splitter3.Visibility = Visibility.Visible;
                    MainW.splitter4.Visibility = Visibility.Visible;
                }
                else {
                    MainW.corner1.Visibility = Visibility.Collapsed;
                    MainW.corner2.Visibility = Visibility.Collapsed;
                    MainW.corner3.Visibility = Visibility.Collapsed;
                    MainW.corner4.Visibility = Visibility.Collapsed;
                    MainW.splitter1.Visibility = Visibility.Collapsed;
                    MainW.splitter2.Visibility = Visibility.Collapsed;
                    MainW.splitter3.Visibility = Visibility.Collapsed;
                    MainW.splitter4.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex) {
#if DEBUG
                MessageBox.Show(ex.ToString());
                //throw;
#endif
            }
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
            RotateTransform rotateTransform = new RotateTransform(angle);
            MainW.grid.RenderTransformOrigin = new Point(0.5, 0.5);
            MainW.grid.RenderTransform = rotateTransform;
            rotateTransform = new RotateTransform(-angle);
            MainW.backgroundimg.RenderTransform = rotateTransform;
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
                    MessageBox.Show(ex.ToString());
                    //throw;
                    #endif
                }
            }
        }

        /// <summary>
        /// Whenever the <see cref="MainWindow.rtb"/> size is changed it is necessary to re-calulate the curved corners margins
        /// it is called on creation and on update due to <see cref="ReadConfig"/>
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static void RtbSizeChanged() {
            try {
                if (MainW?.WinConfig != null) {
                    double borderSize = MainW.WinConfig.bordersize.Value;
                    double row0 = MainW.grid.RowDefinitions[0].ActualHeight;
                    double row2 = MainW.grid.RowDefinitions[2].ActualHeight;
                    double column0 = MainW.grid.ColumnDefinitions[0].ActualWidth;
                    double column2 = MainW.grid.ColumnDefinitions[2].ActualWidth;
                    MainW.corner1.Margin = new Thickness(column0 - borderSize, row0 - borderSize, 0, 0);
                    MainW.corner2.Margin = new Thickness(column0 - borderSize, -borderSize, 0, row2);
                    MainW.corner3.Margin = new Thickness(-borderSize, row0 - borderSize, column2, 0);
                    MainW.corner4.Margin = new Thickness(-borderSize, -borderSize, column2, row2);
                }
            }
            catch (Exception ex) {
#if DEBUG
                MessageBox.Show(ex.ToString());
                //throw;
#endif
            }
        }

        /// <summary>
        /// Sets the <see cref="MainWindow.rtb"/> SpellCheck.IsEnabled state
        /// </summary>
        /// <param name="enabled"></param>
        public static void RtbSpellCheck(bool enabled) {
            MainW.rtb.SpellCheck.IsEnabled = enabled;
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
                    Filter = "XAML Package (*.xamlp)|*.xamlp|Rich Text File (*.rtf)|*.rtf|XAML File (*.xaml)|*.xaml|Text file (*.txt)|*.txt",
                    DefaultExt = ".xamlp"
                };
                if (Filepath.Length < 4) {//checks for complete filenames instead of just .rtf or .xaml //should be "" becouse it is validate in other places too but just in case
                    savedialog.FileName = "Notes";
                }
                else {
                    savedialog.FileName = Path.GetFileName(Filepath);
                }

                if (savedialog.ShowDialog() == true) {
                    Filepath = savedialog.FileName;
                    MainW.MenuFileName.Header = Path.GetFileName(Filepath);
                }
            }

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

                        case (".XAMLP"): {//TODO: change extension to skintext and register it to open with defaults
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
                MessageBox.Show(ex.ToString());
                //throw;
                #endif
            }
        }

        private static bool _isSaving;
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
                    data = "window_position = " + MainW.window.Top.ToString(System.Globalization.CultureInfo.InvariantCulture) + " , " + MainW.window.Left.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    writer.WriteLine(data);
                    //window_size
                    data = "window_size = " + MainW.window.Width.ToString(System.Globalization.CultureInfo.InvariantCulture) + " , " + MainW.window.Height.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    writer.WriteLine(data);
                    //border_size
                    data = "border_size = " + Math.Floor(MainW.WinConfig.bordersize.Value).ToString(System.Globalization.CultureInfo.InvariantCulture);
                    writer.WriteLine(data);
                    //text_size
                    data = "text_size = " +
                        MainW.grid.ColumnDefinitions[0].ActualWidth.ToString(System.Globalization.CultureInfo.InvariantCulture) + " , " +
                        MainW.grid.ColumnDefinitions[1].ActualWidth.ToString(System.Globalization.CultureInfo.InvariantCulture) + " , " +
                        MainW.grid.ColumnDefinitions[2].ActualWidth.ToString(System.Globalization.CultureInfo.InvariantCulture) + " , " +
                        MainW.grid.RowDefinitions[0].ActualHeight.ToString(System.Globalization.CultureInfo.InvariantCulture) + " , " +
                        MainW.grid.RowDefinitions[1].ActualHeight.ToString(System.Globalization.CultureInfo.InvariantCulture) + " , " +
                        MainW.grid.RowDefinitions[2].ActualHeight.ToString(System.Globalization.CultureInfo.InvariantCulture);
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
                    data = "resize_enabled = " + MainW.WinConfig.resizecheck.IsChecked.Value.ToString();
                    writer.WriteLine(data);
                    //border_color
                    data = "border_color = " + MainW.WinConfig.ClrPcker_BorderBackground.SelectedColor.ToString();
                    writer.WriteLine(data);
                    //border_color
                    data = "window_color = " + MainW.WinConfig.ClrPcker_WindowBackground.SelectedColor.ToString();
                    writer.WriteLine(data);
                    //border_color
                    data = "text_bg_color = " + MainW.WinConfig.ClrPcker_RTB_Background.SelectedColor.ToString();
                    writer.WriteLine(data);
                    //rotation
                    data = "rotation = " + MainW.WinConfig.slValue.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    writer.WriteLine(data);
                    //GIF Method
                    data = "gif_uses_ram = " + MainW.WinConfig.GifMethodRAM.IsChecked.Value.ToString();
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
                    data = "image_opacity = " + MainW.WinConfig.imageopacityslider.Value;
                    writer.WriteLine(data);
                    //text_opacity
                    data = "text_opacity = " + MainW.WinConfig.textopacityslider.Value;
                    writer.WriteLine(data);
                    //window_opacity
                    data = "window_opacity = " + MainW.WinConfig.windowopacityslider.Value;
                    writer.WriteLine(data);
                    //read_only
                    data = "read_only = " + MainW.WinConfig.readOnlyCheck.IsChecked.Value.ToString();
                    writer.WriteLine(data);
                    //spell_check
                    data = "spell_check = " + MainW.WinConfig.spellcheck.IsChecked.Value.ToString();
                    writer.WriteLine(data);
                    //always_on_top
                    data = "always_on_top = " + MainW.WinConfig.alwaysontop.IsChecked.Value.ToString();
                    writer.WriteLine(data);
                    //taskbar_icon
                    data = "taskbar_icon = " + MainW.WinConfig.taskbarvisible.IsChecked.Value.ToString();
                    writer.WriteLine(data);
                    //notification_icon
                    data = "notification_icon = " + MainW.WinConfig.NotificationVisible.IsChecked.Value.ToString();
                    writer.WriteLine(data);
                    //ResizeVisible
                    data = "resize_visible = " + MainW.WinConfig.ResizeVisible.IsChecked.Value.ToString();
                    writer.WriteLine(data);
                    //Render transform flip
                    string x = MainW.WinConfig.FlipXButton.IsChecked.Value.ToString();
                    string y = MainW.WinConfig.FlipYButton.IsChecked.Value.ToString();
                    //two methods for getting the variables:
                    //method 1 string y = rtb.RenderTransform.Value.M22.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    //method 2 string y =((System.Windows.Media.ScaleTransform)rtb.RenderTransform).ScaleY.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    data = "flip_rtb = " + x + " , " + y;
                    writer.WriteLine(data);
                    //Line Wrap
                    data = "line_wrap = " + MainW.LineWrapMenuItem.IsChecked.ToString();
                    writer.WriteLine(data);



                    //////////////////////////////////////
                    //ADVANCED
                    data = "BackgroundColorBrush = " + MainW.AdvConf.ClrPcker_BackgroundColorBrush.SelectedColor.ToString();
                    writer.WriteLine(data);
                    data = "ButtonBackgroundCheckedColorBrush = " + MainW.AdvConf.ClrPcker_ButtonBackgroundCheckedColorBrush.SelectedColor.ToString();
                    writer.WriteLine(data);
                    data = "ButtonBorderCheckedColorBrush = " + MainW.AdvConf.ClrPcker_ButtonBorderCheckedColorBrush.SelectedColor.ToString();
                    writer.WriteLine(data);
                    data = "ButtonBackgroundColorBrush = " + MainW.AdvConf.ClrPcker_ButtonBackgroundColorBrush.SelectedColor.ToString();
                    writer.WriteLine(data);
                    data = "ButtonFrontColorBrush = " + MainW.AdvConf.ClrPcker_ButtonFrontColorBrush.SelectedColor.ToString();
                    writer.WriteLine(data);
                    data = "ButtonBorderMouseOverColorBrush = " + MainW.AdvConf.ClrPcker_ButtonBorderMouseOverColorBrush.SelectedColor.ToString();
                    writer.WriteLine(data);
                    data = "ButtonBackgroundMouseOverColorBrush = " + MainW.AdvConf.ClrPcker_ButtonBackgroundMouseOverColorBrush.SelectedColor.ToString();
                    writer.WriteLine(data);
                    data = "FontPickBackgroundColorBrush = " + MainW.AdvConf.ClrPcker_FontPickBackgroundColorBrush.SelectedColor.ToString();
                    writer.WriteLine(data);
                    data = "FontPickMouseOverBackgroundColorBrush = " + MainW.AdvConf.ClrPcker_FontPickMouseOverBackgroundColorBrush.SelectedColor.ToString();
                    writer.WriteLine(data);
                    data = "FontPickMouseOverBorderColorBrush = " + MainW.AdvConf.ClrPcker_FontPickMouseOverBorderColorBrush.SelectedColor.ToString();
                    writer.WriteLine(data);
                    data = "FontPickSelectedBackgroundColorBrush = " + MainW.AdvConf.ClrPcker_FontPickSelectedBackgroundColorBrush.SelectedColor.ToString();
                    writer.WriteLine(data);
                    data = "FontPickSelectedBorderColorBrush = " + MainW.AdvConf.ClrPcker_FontPickSelectedBorderColorBrush.SelectedColor.ToString();
                    writer.WriteLine(data);
                    data = "MenuBackgroundColorBrush = " + MainW.AdvConf.ClrPcker_MenuBackgroundColorBrush.SelectedColor.ToString();
                    writer.WriteLine(data);
                    data = "MenuItem1BorderColorBrush = " + MainW.AdvConf.ClrPcker_MenuItem1BorderColorBrush.SelectedColor.ToString();
                    writer.WriteLine(data);
                    data = "ClrPcker_MenuItem2DisabledColorBrush = " + MainW.AdvConf.ClrPcker_MenuItem2DisabledColorBrush.SelectedColor.ToString();
                    writer.WriteLine(data);
                    data = "MenuItem2HighlightBorderColorBrush = " + MainW.AdvConf.ClrPcker_MenuItem2HighlightBorderColorBrush.SelectedColor.ToString();
                    writer.WriteLine(data);
                    data = "MenuItem2HighlightTextColorBrush = " + MainW.AdvConf.ClrPcker_MenuItem2HighlightTextColorBrush.SelectedColor.ToString();
                    writer.WriteLine(data);
                    data = "TextColorBrush = " + MainW.AdvConf.ClrPcker_TextColorBrush.SelectedColor.ToString();
                    writer.WriteLine(data);
                    data = "FontPickTextColorBrush = " + MainW.AdvConf.ClrPcker_FontPickTextColorBrush.SelectedColor.ToString();
                    writer.WriteLine(data);

                }

                //to hide file: but exception from FileMode.Create
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
            catch (Exception ex) {
#if DEBUG
                MessageBox.Show(ex.ToString());
                //throw;
#endif
            }
            finally {
                fs?.Dispose();
            }
        }

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
            if ((backgroundColor != null) && (backgroundColor.Equals(Brushes.Transparent))) {
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
                            /* TODO: hyperlinks
                            Hyperlink textlink = new Hyperlink(new Run("LINK"))
                            {
                                NavigateUri = new Uri("https://ar.ikariam.gameforge.com/main/gametour_extended")
                            };
                            curParagraph.Inlines.Add(textlink);
                            //TODO: add checkboxes:
                            System.Windows.Controls.CheckBox checkbox = new System.Windows.Controls.CheckBox();
                            curParagraph.Inlines.Add(checkbox);

                            //TODO: add expander NOT ADDING EXPANDERS, NOT WORTH IT, NOT PRETTY, CANT ADD INLINES, MUST ADD TEXTBOX OR SIMILAR:
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
                        MessageBox.Show(ex.ToString());
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
        /// Sets the <see cref="MainWindow.window"/> Topmost state
        /// </summary>
        /// <param name="visible"></param>
        ///
        public static void WindowAlwaysOnTop(bool visible) {
            MainW.window.Topmost = visible;
        }

        /// <summary>
        /// Sets the <see cref="MainWindow.window"/> background color to <paramref name="brush"/>
        /// </summary>
        /// <param name="brush"></param>
        public static void WindowBackgroundColor(Brush brush) {
            if (brush != null) {
                brush.Freeze();
                Application.Current.Resources["MainWindowBackgroundColorBrush"] = brush;
                //MainW.window.Background = brush;
                brush = null;
            }
        }

        /// <summary>
        /// Sets the <see cref="MainWindow.backgroundimg"/> opacity to <paramref name="opacity"/>
        /// </summary>
        /// <param name="opacity"></param>
        public static void WindowImageOpacity(double opacity) {
            MainW.backgroundimg.Opacity = opacity;
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
                    //MessageBox.Show(ex.ToString());
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
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


        public static void BlurBgImage(double blurVal) {
            var blur = new System.Windows.Media.Effects.BlurEffect();
            blur.KernelType = System.Windows.Media.Effects.KernelType.Gaussian;
            blur.Radius = blurVal;

            mainW.backgroundimg.Effect = blur;
        }
    }
}
