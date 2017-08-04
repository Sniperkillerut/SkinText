using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;

namespace SkinText {

    public partial class MainWindow : Window {
        private WindowConfig config;
        private FontConfig fontConf;

        public MainWindow() {
            InitializeComponent();
        }

        public FontConfig FontConf { get => fontConf; set => fontConf = value; }
        public WindowConfig WinConfig { get => config; set => config = value; }
        private void About_Click(object sender, RoutedEventArgs e) {
            string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            MessageBox.Show("Version: " + version);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void CharMap_Click(object sender, RoutedEventArgs e) {
            Process process = new Process();
            try {
                process.StartInfo.FileName = "charmap";
                //process.StartInfo.Arguments = arguments;
                //process.StartInfo.ErrorDialog = true;
                //process.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
                process.Start();
            }
            catch (Exception ex) {
                #if DEBUG
                MessageBox.Show(ex.ToString());
                //throw;
                #endif
            }
            finally {
                process.Dispose();
            }
            //process.WaitForExit(1000 * 60 * 5);    // Wait up to five minutes.
        }

        private void Config_Click(object sender, RoutedEventArgs e) {
            WinConfig.Show();
        }

        private void Donate_Click(object sender, RoutedEventArgs e) {
            System.Diagnostics.Process.Start("http://google.com");
        }

        private void Exit_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void Font_Click(object sender, RoutedEventArgs e) {
            FontConf.Show();
        }

        private void Hyperlink_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            Hyperlink hyperlink = (Hyperlink)sender;
            if (hyperlink.NavigateUri != null) {
                Process.Start(hyperlink.NavigateUri.ToString());
            }
        }

        private void LineWrapMenuItem_Click(object sender, RoutedEventArgs e) {
            if (!LineWrapMenuItem.IsChecked) {
                rtb.Document.PageWidth = 1000;
            }
            else {
                rtb.Document.PageWidth = double.NaN;
            }
        }

        private void Menu_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            WindowInteropHelper helper = new WindowInteropHelper(window);
            IntPtr wparam = new IntPtr(2);
            IntPtr lparam = new IntPtr(0);
            NativeMethods.SendMessage(helper.Handle, 161, wparam, lparam);
        }

        private void Panel_MouseLeave(object sender, MouseEventArgs e) {
            rtb.VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Hidden;
            rtb.HorizontalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Hidden;
            menu.Visibility = Visibility.Collapsed;
        }

        private void Resettodefaults_Click(object sender, RoutedEventArgs e) {
            CustomMethods.SaveChanges(CustomMethods.ResetDefaults, new System.ComponentModel.CancelEventArgs(), "Reset to Defaults");
        }

        private void Rtb_KeyUp(object sender, KeyEventArgs e) {
            if (e.SystemKey.Equals(Key.LeftAlt) || e.Key.Equals(Key.LeftAlt) || e.SystemKey.Equals(Key.RightAlt) || e.Key.Equals(Key.RightAlt)) {
                //FIXME: fix weird menu visibility issues when pressing alt
                menu.Visibility = Visibility.Visible;
            }
        }

        private void Rtb_MouseMove(object sender, MouseEventArgs e) {
            Point m = e.GetPosition(rtb);
            if (m.Y <= 10) {
                menu.Visibility = Visibility.Visible;
            }
            else if (m.Y > 20) {
                menu.Visibility = Visibility.Collapsed;
            }
            if (true) {
                if (m.X >= panel.ActualWidth - 20) {
                    rtb.VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto;
                }
                if ((rtb.Document.PageWidth >= panel.ActualWidth - 20) && (m.Y >= panel.ActualHeight - 20)) {
                    rtb.HorizontalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto;
                }
                if ((m.X < panel.ActualWidth - 20) && (m.Y < panel.ActualHeight - 20)) {
                    rtb.VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Hidden;
                    rtb.HorizontalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Hidden;
                }
            }
        }

        private void Rtb_SelectionChanged(object sender, RoutedEventArgs e) {
            CustomMethods.RtbSelectionChanged();
        }

        private void Rtb_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) {
            //creates a spam on first load, fixed on window_onLoad()
            if (!CustomMethods.FileChanged) {
                CustomMethods.FileChanged = true;
            }
        }

        private void RtbSizeChanged(object sender, SizeChangedEventArgs e) {
            //called upon creation of the rtb
            //and on update when reading a file
            //and on mouseover (as the scrollbars or menu will change the size)
            CustomMethods.RtbSizeChanged();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            CustomMethods.SaveChanges(CustomMethods.ExitProgram, e, "Exit");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            CustomMethods.MainW = this;
            WinConfig = new WindowConfig();
            FontConf  = new FontConfig();
            //AppDataPath = ((App)Application.Current).GAppPath;
            //read FrostHive/SkinText/default.ini
            //check what skin to use and load FrostHive/SkinText/SKIN01/skintext.ini
            //skin information saved in FrostHive/SkinText/SKIN01/skin.ini
            CustomMethods.AppDataPath = CustomMethods.GAppPath;

            CustomMethods.GetSkin();

            //CustomMethods.AppDataPath = App.GAppPath+@"\skinfolder";
            //Todo: Check if loadDefault onload works
            //CustomMethods.LoadDefault();

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

            #endregion get FileName test

            CustomMethods.ReadConfig();
            grid.UpdateLayout();
            CustomMethods.FixResizeTextBox();
            //set to false after the initial text_change that occur on first load
            CustomMethods.FileChanged = false;
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void Window_MouseDown(object sender, MouseButtonEventArgs e) {
            if (Mouse.LeftButton == MouseButtonState.Pressed) {
                try {
                    this.DragMove();
                }
                catch (Exception ex) {
                    #if DEBUG
                        MessageBox.Show(ex.ToString());
                        //throw;
                    #endif
                    //System.InvalidOperationException
                    //dragdrop with only leftclick
                    //dragdrop must be with pressed click
                }
            }
        }
        #region Custom Commands

        // Check Custom Commands class
        private void ExitCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = true;
        }

        private void ExitCommand_Executed(object sender, ExecutedRoutedEventArgs e) {
            //Application.Current.Shutdown();
            this.Close();
        }

        private void NewCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = true;
        }

        private void NewCommand_Executed(object sender, ExecutedRoutedEventArgs e) {
            CustomMethods.SaveChanges(CustomMethods.NewFile, new System.ComponentModel.CancelEventArgs(), "New File");
        }

        private void OpenCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = true;
        }

        private void OpenCommand_Executed(object sender, ExecutedRoutedEventArgs e) {
            CustomMethods.SaveChanges(CustomMethods.OpenFile, new System.ComponentModel.CancelEventArgs(), "Open File");
        }

        private void SaveAsCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = true;
        }

        private void SaveAsCommand_Executed(object sender, ExecutedRoutedEventArgs e) {
            CustomMethods.Save(true);
        }

        private void SaveCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = true;
        }

        private void SaveCommand_Executed(object sender, ExecutedRoutedEventArgs e) {
            CustomMethods.Save(false);
        }
        #endregion Custom Commands
    }
}