using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;

namespace SkinText {

    public partial class MainWindow : Window
    {
        private WindowConfig config;
        private FontConfig fontConf;
        private AdvancedConfig advConf;

        public MainWindow()
        {
            InitializeComponent();
        }

        public FontConfig FontConf { get => fontConf; set => fontConf = value; }
        public WindowConfig WinConfig { get => config; set => config = value; }
        public AdvancedConfig AdvConf { get => advConf; set => advConf = value; }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            MessageBox.Show("Version: " + version);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void CharMap_Click(object sender, RoutedEventArgs e)
        {
            Process process = new Process();
            try
            {
                process.StartInfo.FileName = "charmap";
                //process.StartInfo.Arguments = arguments;
                //process.StartInfo.ErrorDialog = true;
                //process.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
                process.Start();
            }
            catch (Exception ex)
            {
#if DEBUG
                MessageBox.Show(ex.ToString());
                //throw;
#endif
            }
            finally
            {
                process.Dispose();
            }
            //process.WaitForExit(1000 * 60 * 5);    // Wait up to five minutes.
        }

        private void Config_Click(object sender, RoutedEventArgs e)
        {
            WinConfig.Show();
        }

        private void AdvancedConfig_Click(object sender, RoutedEventArgs e)
        {
            AdvConf.Show();
        }

        private void Donate_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://google.com");
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Font_Click(object sender, RoutedEventArgs e)
        {
            FontConf.Show();
        }

        private void Hyperlink_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Hyperlink hyperlink = (Hyperlink)sender;
            if (hyperlink.NavigateUri != null)
            {
                Process.Start(hyperlink.NavigateUri.ToString());
            }
        }

        private void LineWrapMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (!LineWrapMenuItem.IsChecked)
            {
                rtb.Document.PageWidth = 1000;
            }
            else
            {
                rtb.Document.PageWidth = double.NaN;
            }
        }

        private void Menu_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            WindowInteropHelper helper = new WindowInteropHelper(window);
            IntPtr wparam = new IntPtr(2);
            IntPtr lparam = new IntPtr(0);
            NativeMethods.SendMessage(helper.Handle, 161, wparam, lparam);
        }

        private void Panel_MouseLeave(object sender, MouseEventArgs e)
        {
            rtb.VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Hidden;
            rtb.HorizontalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Hidden;
            menu.Visibility = Visibility.Collapsed;
        }

        private void Resettodefaults_Click(object sender, RoutedEventArgs e)
        {
            CustomMethods.SaveChanges(CustomMethods.ResetDefaults, new System.ComponentModel.CancelEventArgs(), "Reset to Defaults");
        }

        private void Rtb_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.SystemKey.Equals(Key.LeftAlt) || e.Key.Equals(Key.LeftAlt) || e.SystemKey.Equals(Key.RightAlt) || e.Key.Equals(Key.RightAlt))
            {
                //FIXME: fix weird menu visibility issues when pressing alt
                menu.Visibility = Visibility.Visible;
            }
        }

        private void Rtb_MouseMove(object sender, MouseEventArgs e)
        {
            Point m = e.GetPosition(rtb);
            if (m.Y <= 10)
            {
                menu.Visibility = Visibility.Visible;
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
                if ((rtb.Document.PageWidth >= panel.ActualWidth - 20) && (m.Y >= panel.ActualHeight - 20))
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

        private void Rtb_SelectionChanged(object sender, RoutedEventArgs e)
        {
            CustomMethods.RtbSelectionChanged();
            UpdateToggleButtonState();
            UpdateDecorators();
            UpdateSelectionListType();
            UpdateSelectedFontSize();
            UpdateSelectedFontFamily();

            if (!DependencyProperty.UnsetValue.Equals(rtb.Selection.GetPropertyValue(TextElement.ForegroundProperty)))
            {
                System.Windows.Media.SolidColorBrush newBrush = (System.Windows.Media.SolidColorBrush)rtb.Selection.GetPropertyValue(TextElement.ForegroundProperty);
                newBrush.Freeze();
                ClrPcker_Font.SelectedColor = newBrush.Color;
            }
            if (!DependencyProperty.UnsetValue.Equals(rtb.Selection.GetPropertyValue(TextElement.BackgroundProperty)))
            {
                System.Windows.Media.SolidColorBrush newBrush = (System.Windows.Media.SolidColorBrush)rtb.Selection.GetPropertyValue(TextElement.BackgroundProperty);
                if (newBrush!=null)
                {
                    newBrush.Freeze();
                    ClrPcker_FontBack.SelectedColor = newBrush.Color;
                }
                else
                {
                    ClrPcker_FontBack.SelectedColor = System.Windows.Media.Colors.Transparent;
                }
            }

            DropDownFontColor.IsEnabled = !rtb.Selection.IsEmpty;
            DropDownFontBackColor.IsEnabled = !rtb.Selection.IsEmpty;
        }

        private void Rtb_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            //creates a spam on first load, fixed on window_onLoad()
            if (CustomMethods.Filepath != null)
            {
                if (CustomMethods.Filepath.Length > 4)
                {
                    if (CustomMethods.FileChanged)
                    {
                        #pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        CustomMethods.DelayedSaveAsync();
                        #pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    }
                }
            }
        }
        private void RtbSizeChanged(object sender, SizeChangedEventArgs e)
        {
            //called upon creation of the rtb
            //and on update when reading a file
            //and on mouseover (as the scrollbars or menu will change the size)
            CustomMethods.RtbSizeChanged();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CustomMethods.SaveChanges(CustomMethods.ExitProgram, e, "Exit");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CustomMethods.MainW = this;
            WinConfig = new WindowConfig();
            FontConf = new FontConfig();
            AdvConf = new AdvancedConfig();
            //AppDataPath = ((App)Application.Current).GAppPath;
            //read FrostHive/SkinText/default.ini
            //check what skin to use and load FrostHive/SkinText/SKIN01/skintext.ini
            //skin information saved in FrostHive/SkinText/SKIN01/skin.ini
            CustomMethods.AppDataPath = CustomMethods.GAppPath;

            CustomMethods.GetSkin();

            //CustomMethods.AppDataPath = App.GAppPath+@"\skinfolder";
            CustomMethods.LoadDefault();


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


            //Editing things
            _fontFamily.ItemsSource = System.Windows.Media.Fonts.SystemFontFamilies;
            _fontSize.ItemsSource = FontSizes;
            _fontFamily.SelectedIndex = 0;
            _fontSize.SelectedIndex = 23;
            DropDownFontColor.IsEnabled = false;
            DropDownFontBackColor.IsEnabled = false;
        }

        public double[] FontSizes => new double[] {
            3.0, 4.0, 5.0, 6.0, 6.5, 7.0, 7.5, 8.0, 8.5, 9.0, 9.5,
            10.0, 10.5, 11.0, 11.5, 12.0, 12.5, 13.0, 13.5, 14.0, 15.0,
            16.0, 17.0, 18.0, 19.0, 20.0, 22.0, 24.0, 26.0, 28.0, 30.0,
            32.0, 34.0, 36.0, 38.0, 40.0, 44.0, 48.0, 52.0, 56.0, 60.0, 64.0, 68.0, 72.0, 76.0,
            80.0, 88.0, 96.0, 104.0, 112.0, 120.0, 128.0, 136.0, 144.0
            };

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                try
                {
                    this.DragMove();
                }
                catch (Exception ex)
                {
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




        void UpdateItemCheckedState(System.Windows.Controls.Primitives.ToggleButton button, DependencyProperty formattingProperty, object expectedValue)
        {
            object currentValue = rtb.Selection.GetPropertyValue(formattingProperty);
            button.IsChecked = (currentValue != DependencyProperty.UnsetValue) && currentValue != null && currentValue.Equals(expectedValue);
        }

        private void UpdateToggleButtonState()
        {
            UpdateItemCheckedState(_btnBold, TextElement.FontWeightProperty, FontWeights.Bold);
            UpdateItemCheckedState(_btnItalic, TextElement.FontStyleProperty, FontStyles.Italic);

            UpdateItemCheckedState(_btnTopscript, Inline.BaselineAlignmentProperty, BaselineAlignment.Top);
            UpdateItemCheckedState(_btnTextTopscript, Inline.BaselineAlignmentProperty, BaselineAlignment.TextTop);
            UpdateItemCheckedState(_btnSuperscript, Inline.BaselineAlignmentProperty, BaselineAlignment.Superscript);
            UpdateItemCheckedState(_btnCentercript, Inline.BaselineAlignmentProperty, BaselineAlignment.Center);
            UpdateItemCheckedState(_btnSubscript, Inline.BaselineAlignmentProperty, BaselineAlignment.Subscript);
            UpdateItemCheckedState(_btnTextBottomscript, Inline.BaselineAlignmentProperty, BaselineAlignment.TextBottom);
            UpdateItemCheckedState(_btnBottomscript, Inline.BaselineAlignmentProperty, BaselineAlignment.Bottom);
            UpdateItemCheckedState(_btnBasescript, Inline.BaselineAlignmentProperty, BaselineAlignment.Baseline);

            UpdateItemCheckedState(_btnAlignLeft, Block.TextAlignmentProperty, TextAlignment.Left);
            UpdateItemCheckedState(_btnAlignCenter, Block.TextAlignmentProperty, TextAlignment.Center);
            UpdateItemCheckedState(_btnAlignRight, Block.TextAlignmentProperty, TextAlignment.Right);
            UpdateItemCheckedState(_btnAlignJustify, Block.TextAlignmentProperty, TextAlignment.Justify);

            UpdateItemCheckedState(_btnFlowDirLTR, Block.FlowDirectionProperty, FlowDirection.LeftToRight);
            UpdateItemCheckedState(_btnFlowDirRTL, Block.FlowDirectionProperty, FlowDirection.RightToLeft);

            /*
            UpdateItemCheckedState(_btnOverLine        , Inline.TextDecorationsProperty, TextDecorations.OverLine);
            UpdateItemCheckedState(_btnStrikethrough   , Inline.TextDecorationsProperty, TextDecorations.Strikethrough);
            UpdateItemCheckedState(_btnBaseline        , Inline.TextDecorationsProperty, TextDecorations.Baseline);
            UpdateItemCheckedState(_btnUnderline       , Inline.TextDecorationsProperty, TextDecorations.Underline);
            */
        }

        private void UpdateDecorators()
        {
            if (rtb.Selection != null)
            {
                _btnOverLine.IsChecked = false;
                _btnStrikethrough.IsChecked = false;
                _btnBaseline.IsChecked = false;
                _btnUnderline.IsChecked = false;
                if (!DependencyProperty.UnsetValue.Equals(rtb.Selection.GetPropertyValue(Inline.TextDecorationsProperty)))
                {
                    TextDecorationCollection temp = (TextDecorationCollection)rtb.Selection.GetPropertyValue(Inline.TextDecorationsProperty);
                    foreach (TextDecoration decor in temp)
                    {
                        switch (decor.Location)
                        {
                            case (TextDecorationLocation.Baseline):
                                {
                                    _btnBaseline.IsChecked = true;
                                    break;
                                }
                            case (TextDecorationLocation.OverLine):
                                {
                                    _btnOverLine.IsChecked = true;
                                    break;
                                }
                            case (TextDecorationLocation.Strikethrough):
                                {
                                    _btnStrikethrough.IsChecked = true;
                                    break;
                                }
                            case (TextDecorationLocation.Underline):
                                {
                                    _btnUnderline.IsChecked = true;
                                    break;
                                }
                        }
                    }
                }
            }
        }

        #region Custom Commands

        // Check Custom Commands class
        private void ExitCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void ExitCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //Application.Current.Shutdown();
            this.Close();
        }

        private void NewCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void NewCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            CustomMethods.SaveChanges(CustomMethods.NewFile, new System.ComponentModel.CancelEventArgs(), "New File");
        }

        private void OpenCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void OpenCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            CustomMethods.SaveChanges(CustomMethods.OpenFile, new System.ComponentModel.CancelEventArgs(), "Open File");
        }

        private void SaveAsCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void SaveAsCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            CustomMethods.Save(true);
        }

        private void SaveCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void SaveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            CustomMethods.Save(false);
        }
        #endregion Custom Commands

        private void _btnLine_Clicked(object sender, RoutedEventArgs e)
        {
            if (rtb.Selection != null)
            {
                TextDecorationCollection decor = new TextDecorationCollection();

                if (_btnOverLine.IsChecked.Value)
                {
                    decor.Add(TextDecorations.OverLine);
                }
                if (_btnStrikethrough.IsChecked.Value)
                {
                    decor.Add(TextDecorations.Strikethrough);
                }
                if (_btnBaseline.IsChecked.Value)
                {
                    decor.Add(TextDecorations.Baseline);
                }
                if (_btnUnderline.IsChecked.Value)
                {
                    decor.Add(TextDecorations.Underline);
                }
                decor.Freeze();
                if (decor != null)
                {
                    //rtb.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, null);
                    rtb.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, decor);
                }
            }
        }

        private void _btnScript_Clicked(object sender, RoutedEventArgs e)
        {
            if (rtb.Selection != null)
            {
                System.Windows.Controls.Primitives.ToggleButton btn = (System.Windows.Controls.Primitives.ToggleButton)sender;
                if (btn != null && btn.IsChecked.Value)
                {
                    _btnTopscript.IsChecked = false;
                    _btnSuperscript.IsChecked = false;
                    _btnTextTopscript.IsChecked = false;
                    _btnCentercript.IsChecked = false;
                    _btnSubscript.IsChecked = false;
                    _btnTextBottomscript.IsChecked = false;
                    _btnBottomscript.IsChecked = false;
                    _btnBasescript.IsChecked = false;
                    switch (btn.Name)
                    {
                        case nameof(_btnTopscript):
                            {
                                rtb.Selection.ApplyPropertyValue(Inline.BaselineAlignmentProperty, BaselineAlignment.Top);
                                _btnTopscript.IsChecked = true;
                                break;
                            }
                        case nameof(_btnSuperscript):
                            {
                                rtb.Selection.ApplyPropertyValue(Inline.BaselineAlignmentProperty, BaselineAlignment.Superscript);
                                _btnSuperscript.IsChecked = true;
                                break;
                            }
                        case nameof(_btnTextTopscript):
                            {
                                rtb.Selection.ApplyPropertyValue(Inline.BaselineAlignmentProperty, BaselineAlignment.TextTop);
                                _btnTextTopscript.IsChecked = true;
                                break;
                            }
                        case nameof(_btnCentercript):
                            {
                                rtb.Selection.ApplyPropertyValue(Inline.BaselineAlignmentProperty, BaselineAlignment.Center);
                                _btnCentercript.IsChecked = true;
                                break;
                            }
                        case nameof(_btnSubscript):
                            {
                                rtb.Selection.ApplyPropertyValue(Inline.BaselineAlignmentProperty, BaselineAlignment.Subscript);
                                _btnSubscript.IsChecked = true;
                                break;
                            }
                        case nameof(_btnTextBottomscript):
                            {
                                rtb.Selection.ApplyPropertyValue(Inline.BaselineAlignmentProperty, BaselineAlignment.TextBottom);
                                _btnTextBottomscript.IsChecked = true;
                                break;
                            }
                        case nameof(_btnBottomscript):
                            {
                                rtb.Selection.ApplyPropertyValue(Inline.BaselineAlignmentProperty, BaselineAlignment.Bottom);
                                _btnBottomscript.IsChecked = true;
                                break;
                            }
                        case nameof(_btnBasescript):
                            {
                                rtb.Selection.ApplyPropertyValue(Inline.BaselineAlignmentProperty, BaselineAlignment.Baseline);
                                _btnBasescript.IsChecked = true;
                                break;
                            }
                    }
                }
                else
                {
                    _btnTopscript.IsChecked = false;
                    _btnSuperscript.IsChecked = false;
                    _btnTextTopscript.IsChecked = false;
                    _btnCentercript.IsChecked = false;
                    _btnSubscript.IsChecked = false;
                    _btnTextBottomscript.IsChecked = false;
                    _btnBottomscript.IsChecked = false;
                    rtb.Selection.ApplyPropertyValue(Inline.BaselineAlignmentProperty, BaselineAlignment.Baseline);
                    _btnBasescript.IsChecked = true;
                }
            }
        }

        private void _btnFlowDir_Click(object sender, RoutedEventArgs e)
        {
            if (rtb.Selection != null)
            {
                if (_btnFlowDirLTR.IsChecked.Value)
                {
                    rtb.Selection.ApplyPropertyValue(Block.FlowDirectionProperty, FlowDirection.LeftToRight);
                }
                else
                {
                    rtb.Selection.ApplyPropertyValue(Block.FlowDirectionProperty, FlowDirection.RightToLeft);
                }
            }
        }

        private void UpdateSelectionListType()
        {
            Paragraph startParagraph = rtb.Selection.Start.Paragraph;
            Paragraph endParagraph = rtb.Selection.End.Paragraph;
            if (startParagraph != null && endParagraph != null && (startParagraph.Parent is ListItem) && (endParagraph.Parent is ListItem) && object.ReferenceEquals(((ListItem)startParagraph.Parent).List, ((ListItem)endParagraph.Parent).List))
            {
                _btnToggleBox.IsChecked = false;
                _btnToggleCircle.IsChecked = false;
                _btnToggleNumbering.IsChecked = false;
                _btnToggleBullets.IsChecked = false;
                _btnToggleLowerLatin.IsChecked = false;
                _btnToggleLowerRoman.IsChecked = false;
                _btnToggleSquare.IsChecked = false;
                _btnToggleUpperLatin.IsChecked = false;
                _btnToggleUpperRoman.IsChecked = false;

                TextMarkerStyle markerStyle = ((ListItem)startParagraph.Parent).List.MarkerStyle;
                switch (markerStyle)
                {
                    case (TextMarkerStyle.Box): {
                            _btnToggleBox.IsChecked = true;
                            break;
                        }
                    case (TextMarkerStyle.Circle): {
                            _btnToggleCircle.IsChecked = true;
                            break;
                        }
                    case (TextMarkerStyle.Decimal):
                        {
                            _btnToggleNumbering.IsChecked = true;
                            break;
                        }
                    case (TextMarkerStyle.Disc):
                        {
                            _btnToggleBullets.IsChecked = true;
                            break;
                        }
                    case (TextMarkerStyle.LowerLatin):
                        {
                            _btnToggleLowerLatin.IsChecked = true;
                            break;
                        }
                    case (TextMarkerStyle.LowerRoman):
                        {
                            _btnToggleLowerRoman.IsChecked = true;
                            break;
                        }
                    case (TextMarkerStyle.Square):
                        {
                            _btnToggleSquare.IsChecked = true;
                            break;
                        }
                    case (TextMarkerStyle.UpperLatin):
                        {
                            _btnToggleUpperLatin.IsChecked = true;
                            break;
                        }
                    case (TextMarkerStyle.UpperRoman):
                        {
                            _btnToggleUpperRoman.IsChecked = true;
                            break;
                        }

                }
            }
            else
            {
                _btnToggleBox.IsChecked = false;
                _btnToggleCircle.IsChecked = false;
                _btnToggleNumbering.IsChecked = false;
                _btnToggleBullets.IsChecked = false;
                _btnToggleLowerLatin.IsChecked = false;
                _btnToggleLowerRoman.IsChecked = false;
                _btnToggleSquare.IsChecked = false;
                _btnToggleUpperLatin.IsChecked = false;
                _btnToggleUpperRoman.IsChecked = false;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void _fontSize_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
            try
            {
                ApplyPropertyValueToSelectedText(TextElement.FontSizeProperty, e.AddedItems[0]);
            }
            catch (Exception ex)
            {
                #if DEBUG
                MessageBox.Show(ex.ToString());
                //throw;
                #endif
            }
        }

        private void _fontFamily_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
            System.Windows.Media.FontFamily editValue = (System.Windows.Media.FontFamily)e.AddedItems[0];
            ApplyPropertyValueToSelectedText(TextElement.FontFamilyProperty, editValue);
        }
        void ApplyPropertyValueToSelectedText(DependencyProperty formattingProperty, object value) {
            if (value == null){
                return;
            }
            rtb.Selection.ApplyPropertyValue(formattingProperty, value);
        }

        private void UpdateSelectedFontFamily() {
            object value = rtb.Selection.GetPropertyValue(TextElement.FontFamilyProperty);
            System.Windows.Media.FontFamily currentFontFamily = (System.Windows.Media.FontFamily)((value == DependencyProperty.UnsetValue) ? null : value);
            if (currentFontFamily != null)
            {
                _fontFamily.SelectedItem = currentFontFamily;
            }
        }

        private void UpdateSelectedFontSize() {
            object value = rtb.Selection.GetPropertyValue(TextElement.FontSizeProperty);
            _fontSize.SelectedValue = (value == DependencyProperty.UnsetValue) ? null : value;
        }

        public void SelectImg() {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog {
                Filter = "Image files (*.jpg, *.jpeg,*.gif, *.png) | *.jpg; *.jpeg; *.gif; *.png"
            };
            bool? result = dlg.ShowDialog();
            if (result.Value)
            {
                Uri uri = new Uri(dlg.FileName, UriKind.Relative);
                System.Windows.Media.Imaging.BitmapImage bitmapImg = new System.Windows.Media.Imaging.BitmapImage(uri);
                System.Windows.Controls.Image image = new System.Windows.Controls.Image {
                    Stretch = System.Windows.Media.Stretch.Fill,
                    Width = 250,
                    Height = 200,
                    Source = bitmapImg
                };
                var tp = rtb.CaretPosition.GetInsertionPosition(LogicalDirection.Forward);
                #pragma warning disable RECS0026 // Possible unassigned object created by 'new'
                new InlineUIContainer(image, tp);
                #pragma warning restore RECS0026 // Possible unassigned object created by 'new'
            }
        }
        private void _btn_importimg_Click(object sender, RoutedEventArgs e) {
            SelectImg();
        }



        private void ClrPcker_Font_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e) {
            if (rtb!=null && rtb.Selection!=null && !rtb.Selection.IsEmpty)
            {
                System.Windows.Media.SolidColorBrush newBrush = new System.Windows.Media.SolidColorBrush(ClrPcker_Font.SelectedColor.Value);
                newBrush.Freeze();
                rtb.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, newBrush);
            }
        }
        private void ClrPcker_FontBack_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e) {
            if (rtb != null && rtb.Selection != null && !rtb.Selection.IsEmpty)
            {
                System.Windows.Media.SolidColorBrush newBrush = new System.Windows.Media.SolidColorBrush(ClrPcker_FontBack.SelectedColor.Value);
                if (newBrush.Color.A < 255)
                {
                    newBrush = System.Windows.Media.Brushes.Transparent;
                }
                if ((newBrush != null) && (newBrush.Equals(System.Windows.Media.Brushes.Transparent)))
                {
                    newBrush = null;
                }
                rtb.Selection.ApplyPropertyValue(TextElement.BackgroundProperty, newBrush);
            }
        }

        private void _btnListType_Click(object sender, RoutedEventArgs e){

            if (rtb.Selection != null)
            {
                System.Windows.Controls.Primitives.ToggleButton btn = (System.Windows.Controls.Primitives.ToggleButton)sender;
                if (btn != null)
                {
                    Paragraph startParagraph = rtb.Selection.Start.Paragraph;
                    Paragraph endParagraph = rtb.Selection.End.Paragraph;
                    if (startParagraph != null && endParagraph != null && (startParagraph.Parent is ListItem) && (endParagraph.Parent is ListItem) && object.ReferenceEquals(((ListItem)startParagraph.Parent).List, ((ListItem)endParagraph.Parent).List))
                    {//if there is a list only change TextMarkerStyle
                    }
                    else
                    {
                        EditingCommands.ToggleBullets.Execute(null, rtb);
                    }
                    if (btn.IsChecked.Value)
                    {
                        _btnToggleBox.IsChecked = false;
                        _btnToggleCircle.IsChecked = false;
                        _btnToggleNumbering.IsChecked = false;
                        _btnToggleBullets.IsChecked = false;
                        _btnToggleLowerLatin.IsChecked = false;
                        _btnToggleLowerRoman.IsChecked = false;
                        _btnToggleSquare.IsChecked = false;
                        _btnToggleUpperLatin.IsChecked = false;
                        _btnToggleUpperRoman.IsChecked = false;
                        #pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        switch (btn.Name)
                        {
                            case nameof(_btnToggleBox):
                                {
                                    _btnToggleBox.IsChecked = true;
                                    ListBulletTypeAsync(rtb.Selection.Start.Paragraph, rtb.Selection.End.Paragraph, TextMarkerStyle.Box);
                                    break;
                                }
                            case nameof(_btnToggleCircle):
                                {
                                    _btnToggleCircle.IsChecked = true;
                                    ListBulletTypeAsync(rtb.Selection.Start.Paragraph, rtb.Selection.End.Paragraph, TextMarkerStyle.Circle);
                                    break;
                                }
                            case nameof(_btnToggleNumbering):
                                {
                                    _btnToggleNumbering.IsChecked = true;
                                    ListBulletTypeAsync(rtb.Selection.Start.Paragraph, rtb.Selection.End.Paragraph, TextMarkerStyle.Decimal);
                                    break;
                                }
                            case nameof(_btnToggleBullets):
                                {
                                    _btnToggleBullets.IsChecked = true;
                                    ListBulletTypeAsync(rtb.Selection.Start.Paragraph, rtb.Selection.End.Paragraph, TextMarkerStyle.Disc);
                                    break;
                                }
                            case nameof(_btnToggleLowerLatin):
                                {
                                    _btnToggleLowerLatin.IsChecked = true;
                                    ListBulletTypeAsync(rtb.Selection.Start.Paragraph, rtb.Selection.End.Paragraph, TextMarkerStyle.LowerLatin);
                                    break;
                                }
                            case nameof(_btnToggleLowerRoman):
                                {
                                    _btnToggleLowerRoman.IsChecked = true;
                                    ListBulletTypeAsync(rtb.Selection.Start.Paragraph, rtb.Selection.End.Paragraph, TextMarkerStyle.LowerRoman);
                                    break;
                                }
                            case nameof(_btnToggleSquare):
                                {
                                    _btnToggleSquare.IsChecked = true;
                                    ListBulletTypeAsync(rtb.Selection.Start.Paragraph, rtb.Selection.End.Paragraph, TextMarkerStyle.Square);
                                    break;
                                }
                            case nameof(_btnToggleUpperLatin):
                                {
                                    _btnToggleUpperLatin.IsChecked = true;
                                    ListBulletTypeAsync(rtb.Selection.Start.Paragraph, rtb.Selection.End.Paragraph, TextMarkerStyle.UpperLatin);
                                    break;
                                }
                            case nameof(_btnToggleUpperRoman):
                                {
                                    _btnToggleUpperRoman.IsChecked = true;
                                    ListBulletTypeAsync(rtb.Selection.Start.Paragraph, rtb.Selection.End.Paragraph, TextMarkerStyle.UpperRoman);
                                    break;
                                }
                        }
                        #pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    }
                    else
                    {//if unchecking
                        TextMarkerStyle markerStyle = ((ListItem)startParagraph.Parent).List.MarkerStyle;
                        if (markerStyle != TextMarkerStyle.Disc)
                        {
                            EditingCommands.ToggleBullets.Execute(null, rtb);
                        }
                        EditingCommands.ToggleBullets.Execute(null, rtb);
                    }
                }
            }
        }

        public async static System.Threading.Tasks.Task ListBulletTypeAsync(Paragraph startParagraph, Paragraph endParagraph, TextMarkerStyle textMarker) {
            await System.Threading.Tasks.Task.Delay(100);
            if (startParagraph != null && endParagraph != null && (startParagraph.Parent is ListItem) && (endParagraph.Parent is ListItem) && object.ReferenceEquals(((ListItem)startParagraph.Parent).List, ((ListItem)endParagraph.Parent).List))
            {
                ((ListItem)startParagraph.Parent).List.MarkerStyle = textMarker;
            }
        }

    }
}