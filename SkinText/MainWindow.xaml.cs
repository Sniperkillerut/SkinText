using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;

namespace SkinText {

    public partial class MainWindow : Window
    {
        private FontConfig fontConf;
        private ConfigWin conf;

        public MainWindow()
        {
            InitializeComponent();
        }

        public FontConfig FontConf { get => fontConf; set => fontConf = value; }
        public ConfigWin Conf { get => conf; set => conf = value; }

        public double[] GetFontSizes() => new double[] {
            3.0, 4.0, 5.0, 6.0, 6.5, 7.0, 7.5, 8.0, 8.5, 9.0, 9.5,
            10.0, 10.5, 11.0, 11.5, 12.0, 12.5, 13.0, 13.5, 14.0, 15.0,
            16.0, 17.0, 18.0, 19.0, 20.0, 22.0, 24.0, 26.0, 28.0, 30.0,
            32.0, 34.0, 36.0, 38.0, 40.0, 44.0, 48.0, 52.0, 56.0, 60.0, 64.0, 68.0, 72.0, 76.0,
            80.0, 88.0, 96.0, 104.0, 112.0, 120.0, 128.0, 136.0, 144.0
            };

        #region General

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CustomMethods.SaveChanges(CustomMethods.ExitProgram, e, "Exit");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CustomMethods.MainW = this;
            FontConf = new FontConfig();
            Conf = new ConfigWin();
            //FontConf.Show(); FontConf.Hide();
            //Conf.Show(); Conf.Hide();

            //check what skin to use and load FrostHive/SkinText/SKIN01/skintext.ini
            //skin information saved in FrostHive/SkinText/SKIN01/skin.ini
            CustomMethods.AppDataPath = CustomMethods.GAppPath;

            CustomMethods.GetSkin();

            CustomMethods.LoadDefault();

            CustomMethods.ReadConfig();

            //if there is an associated file it will read it instead of the config one
            CustomMethods.ReadAssociatedFile();

            //set to false after the initial text_change that occur on first load
            CustomMethods.FileChanged = false;

            //Editing things
            _fontFamily.ItemsSource = System.Windows.Media.Fonts.SystemFontFamilies;
            _fontSize.ItemsSource = GetFontSizes();
            _fontFamily.SelectedIndex = 0;
            _fontSize.SelectedIndex = 23;
            DropDownFontColor.IsEnabled = false;
            DropDownFontBackColor.IsEnabled = false;
            rtb.Focus();

            CustomMethods.GetSkinList();
            string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Conf.TxtBloqVersion.Text = version;
        }

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
                    MessageBox.Show("DEBUG: " + ex.ToString());
                    //throw;
#endif
                    //System.InvalidOperationException
                    //dragdrop with only leftclick
                    //dragdrop must be with pressed click
                }
            }
        }

        #endregion General

        #region DragDrop
        //http://xcalibursystems.com/2011/12/wpf-drag-and-drop-textbox-for-windows-explorer-files/
        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                string ext = System.IO.Path.GetExtension(files[0]).ToUpperInvariant();

                if (Array.Exists(new string[] { ".JPG", ".JPEG", ".JPE", ".JFIF", ".PNG", ".GIF" }, element => element == ext))
                {
                    CustomMethods.LoadImage(files[0]);
                }
                else
                {
                    CustomMethods.ReadFile(files[0]);
                }
                //to stop calling multiple times on drop
                e.Handled = true;
            }
        }

        private void Rtb_PreviewDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.All;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void Rtb_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }

        #endregion DragDrop

        #region menu

        private void About_Click(object sender, RoutedEventArgs e)
        {
            Conf.Show();
            CustomMethods.CheckBlurBG();
            Conf.AboutItem.IsSelected = true;
            Conf.Focus();
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
                MessageBox.Show("DEBUG: " + ex.ToString());
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
            Conf.Show();
            CustomMethods.CheckBlurBG();
            Conf.Focus();
        }

        private void Donate_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://google.com");
        }

        private void Font_Click(object sender, RoutedEventArgs e)
        {
            FontConf.Show();
            CustomMethods.CheckBlurBG();
            FontConf.Focus();
        }

        private void LineWrapMenuItem_Click(object sender, RoutedEventArgs e)
        {
            CustomMethods.LineWrap(LineWrapMenuItem.IsChecked);
        }

        private void ToolBarMenuItem_Checked(object sender, RoutedEventArgs e)
        {
            CustomMethods.ToolBarVisible(ToolBarMenuItem.IsChecked);
        }

        private void ResizeMenuItem_Checked(object sender, RoutedEventArgs e)
        {
            Conf.resizecheck.IsChecked = ResizeMenuItem.IsChecked;
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
            rtb.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            rtb.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            menu.Visibility = Visibility.Collapsed;
        }

        private void Resettodefaults_Click(object sender, RoutedEventArgs e)
        {
            if (sender.Equals(resetToDefaultsTray))
            {
                MessageBox.Show("Ignore this");
                //when called from tray icon the first msgbox is cancelled automaticly
                //still dont know why, seems to be a library issue, same error reported on the forum
            }
            CustomMethods.SaveChanges(CustomMethods.ResetDefaults, new System.ComponentModel.CancelEventArgs(), "Reset to Defaults");
        }

        private void MouseMove_Menu(object sender, MouseEventArgs e)
        {
            Point m = e.GetPosition(panel);
            if (!e.Source.GetType().Equals(typeof(Menu)) && !e.Source.GetType().Equals(typeof(MenuItem)))
            {
                if (m.Y <= 10)
                {
                    menu.Visibility = Visibility.Visible;
                }
                else if (m.Y > 20)
                {
                    menu.Visibility = Visibility.Collapsed;
                }
            }
        }

        #endregion menu

        #region RTB functions

        private void Rtb_MouseMove(object sender, MouseEventArgs e)
        {
            if (sender != ToolBarTray)
            {
                Point m = e.GetPosition(rtb);
                if (m.X >= panel.ActualWidth - 20)
                {
                    rtb.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                }
                if ((rtb.Document.PageWidth >= panel.ActualWidth - 20) && (m.Y >= panel.ActualHeight - 20))
                {
                    rtb.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                }
                if ((m.X < panel.ActualWidth - 20) && (m.Y < panel.ActualHeight - 20))
                {
                    rtb.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                    rtb.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
                }
            }
        }

        private void Hyperlink_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                Hyperlink hyperlink = (Hyperlink)sender;
                if (hyperlink.NavigateUri != null)
                {
                    Process.Start(hyperlink.NavigateUri.ToString());
                }
            }
        }

        private void Rtb_SelectionChanged(object sender, RoutedEventArgs e)
        {
            //FontConf
            CustomMethods.RtbSelectionChanged();
            //ToolBar
            CustomMethods.UpdateToggleButtonState();
            CustomMethods.UpdateDecorators();
            CustomMethods.UpdateSelectionListType();
            CustomMethods.UpdateSelectedFontSize();
            CustomMethods.UpdateSelectedFontFamily();

            if (!DependencyProperty.UnsetValue.Equals(rtb.Selection.GetPropertyValue(TextElement.ForegroundProperty)))
            {
                System.Windows.Media.SolidColorBrush newBrush = (System.Windows.Media.SolidColorBrush)rtb.Selection.GetPropertyValue(TextElement.ForegroundProperty);
                newBrush.Freeze();
                ClrPcker_Font.SelectedColor = newBrush.Color;
            }
            if (!DependencyProperty.UnsetValue.Equals(rtb.Selection.GetPropertyValue(TextElement.BackgroundProperty)))
            {
                System.Windows.Media.SolidColorBrush newBrush = (System.Windows.Media.SolidColorBrush)rtb.Selection.GetPropertyValue(TextElement.BackgroundProperty);
                if (newBrush != null)
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

        private void Rtb_TextChanged(object sender, TextChangedEventArgs e)
        {
            //creates a spam on first load, fixed on window_onLoad()
            CustomMethods.FileChanged = true;
            if (CustomMethods.Filepath != null)
            {
                if (CustomMethods.Filepath.Length > 4)
                {
                    if (CustomMethods.FileChanged)
                    {
                        if (CustomMethods.AutoSaveEnabled)
                        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                            CustomMethods.DelayedSaveAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        }
                    }
                }
            }
        }

        private void Rtb_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.SystemKey.Equals(Key.Enter) || e.Key.Equals(Key.Enter) || e.SystemKey.Equals(Key.Space) || e.Key.Equals(Key.Space))
            {
                try
                {
                    //Check all paragraphs for hyperlinks
                    /*for (int i = 0; i < rtb.Document.Blocks.Count; i++) {
                        var arr = new Block[rtb.Document.Blocks.Count];
                        rtb.Document.Blocks.CopyTo(arr,0);
                        if (typeof(Paragraph).Equals(arr[i].GetType())) {
                            CustomMethods.DetectURLs((System.Windows.Documents.Paragraph)arr[i]);
                        }
                    }*/
                    CustomMethods.DetectURLs(rtb.CaretPosition.Paragraph);
                }
                catch (Exception ex)
                {
#if DEBUG
                    MessageBox.Show("DEBUG: " + ex.ToString());
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                    //throw;
#endif
                }
            }
            e.Handled = false;
        }

        private void Rtb_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.SystemKey.Equals(Key.LeftAlt) || e.Key.Equals(Key.LeftAlt) || e.SystemKey.Equals(Key.RightAlt) || e.Key.Equals(Key.RightAlt))
            {
                //FIXME: fix weird menu visibility issues when pressing alt
                menu.Visibility = Visibility.Visible;
            }
        }
        #endregion RTB functions

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

        #region Toolbar Buttons

        private void _btnLine_Clicked(object sender, RoutedEventArgs e)
        {
            CustomMethods.ApplyTextDecorators();
        }

        private void _btnScript_Clicked(object sender, RoutedEventArgs e)
        {
            CustomMethods.ApplyTextScript(sender);
        }

        private void _btnFlowDir_Click(object sender, RoutedEventArgs e)
        {
            CustomMethods.ApplyFlowDir();
        }

        private void _fontSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CustomMethods.ApplyFontSize(e);
        }

        private void _fontFamily_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CustomMethods.ApplyFontFamily(e);
        }

        private void _btn_importimg_Click(object sender, RoutedEventArgs e)
        {
            CustomMethods.SelectImg();
        }

        private void ClrPcker_Font_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e)
        {
            CustomMethods.ApplyFontColor();
        }

        private void ClrPcker_FontBack_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e)
        {
            CustomMethods.ApplyFontBackColor();
        }

        private void _btnListType_Click(object sender, RoutedEventArgs e)
        {
            CustomMethods.ApplyListType(sender);
        }

        private void Cmb_KeyUp(object sender, KeyEventArgs e)
        {
            CustomMethods.FilterFontFamilyComboBox();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:No pasar cadenas literal como parámetros localizados", MessageId = "System.Windows.MessageBox.Show(System.String,System.String,System.Windows.MessageBoxButton,System.Windows.MessageBoxImage,System.Windows.MessageBoxResult,System.Windows.MessageBoxOptions)")]
        private void _btn_addhyperlink_Click(object sender, RoutedEventArgs e)
        {
            HyperLinkWindow link = new HyperLinkWindow();
            if (link.ShowDialog() == true)
            {
                if (!string.IsNullOrWhiteSpace(link.HyperNameResult) && !string.IsNullOrWhiteSpace(link.HyperLinkResult))
                {
                    try
                    {
#pragma warning disable RECS0026 // Possible unassigned object created by 'new'
                        new Hyperlink(new Run(link.HyperNameResult), rtb.CaretPosition.GetInsertionPosition(LogicalDirection.Forward))
                        {
                            NavigateUri = new Uri(link.HyperLinkResult),
                            Cursor = Cursors.Hand
                        };
#pragma warning restore RECS0026 // Possible unassigned object created by 'new'
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Information is invalid", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.ServiceNotification);
#if DEBUG
                        MessageBox.Show("DEBUG: " + ex.ToString());
                        //throw;
#endif
                    }
                }
                else
                {
                    MessageBox.Show("Information is invalid", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.ServiceNotification);
                }
            }
        }

        #endregion Toolbar Buttons

        /////////////////////////////////////////////////////

        #region Easy Custom Opacity Mask

        /*
            <DockPanel.OpacityMask>
                <VisualBrush>
                    <VisualBrush.Visual>
                        <Border x:Name="BorderOpacityMask"
                    Background="Black"
                    BorderBrush="Black"
                    SnapsToDevicePixels="True"
                    BorderThickness="0"
                    CornerRadius="{Binding CornerRadius, RelativeSource={RelativeSource FindAncestor, AncestorType=Border}}"
                    Width="{Binding ActualWidth, RelativeSource={RelativeSource FindAncestor, AncestorType=DockPanel}}"
                    Height="{Binding ActualHeight, RelativeSource={RelativeSource FindAncestor, AncestorType=DockPanel}}"
                    />
                    </VisualBrush.Visual>
                </VisualBrush>
            </DockPanel.OpacityMask>
        */

        #endregion Easy Custom Opacity Mask

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

        ///////////////////////////////////////////////////////////////////////////////

        #region Resize with Adorners

        // Handler for drag stopping on leaving the window
        public void Window1MouseLeave(object sender, RoutedEventArgs e)
        {
            StopDragging();
            e.Handled = true;
        }

        // Handler for drag stopping on user choise
        public void DragFinishedMouseHandler(object sender, RoutedEventArgs e)
        {
            StopDragging();
            e.Handled = true;
        }

        // Method for stopping dragging
        public void StopDragging()
        {
            if (_isDown)
            {
                _isDown = false;
                _isDragging = false;
            }
        }

        // Hanler for providing drag operation with selected element
        public void Window1MouseMove(object sender, MouseEventArgs e)
        {
            if (e != null)
            {
                if (_isDown)
                {
                    if ((!_isDragging) &&
                        ((Math.Abs(e.GetPosition(canvas).X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance) ||
                        (Math.Abs(e.GetPosition(canvas).Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)))
                    {
                        _isDragging = true;
                    }

                    if (_isDragging)
                    {
                        Point position = Mouse.GetPosition(canvas);
                        if ((position.Y - (_startPoint.Y - OriginalTop) >= 0) && ((position.Y - (_startPoint.Y - OriginalTop) + TitleBorder.ActualHeight) <= canvas.ActualHeight))
                        {
                            Canvas.SetTop(selectedElement, position.Y - (_startPoint.Y - OriginalTop));
                        }
                        if ((position.X - (_startPoint.X - OriginalLeft) >= 0) && ((position.X - (_startPoint.X - OriginalLeft) + TitleBorder.ActualWidth) <= canvas.ActualWidth))
                        {
                            Canvas.SetLeft(selectedElement, position.X - (_startPoint.X - OriginalLeft));
                        }
                    }
                }
            }
        }

        // Handler for clearing element selection, adorner removal
        public void Window1MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (selected)
            {
                selected = false;
                if (selectedElement != null)
                {
                    aLayer.Remove(aLayer.GetAdorners(selectedElement)[0]);
                    selectedElement = null;
                    ResizeMenuItem.IsChecked = false;
                }
            }
        }

        // Handler for element selection on the canvas providing resizing adorner
        public void MyCanvasPreviewMouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            Deselect();

            // If any element except canvas is clicked,
            // assign the selected element and add the adorner
            //if (e.Source != canvas)
            if (e != null)
            {
                if (e.Source == TitleBorder)
                {
                    _isDown = true;
                    _startPoint = e.GetPosition(canvas);

                    selectedElement = e.Source as UIElement;

                    OriginalLeft = Canvas.GetLeft(selectedElement);
                    OriginalTop = Canvas.GetTop(selectedElement);

                    aLayer = AdornerLayer.GetAdornerLayer(selectedElement);
                    aLayer.Add(new ResizingAdorner(selectedElement));
                    selected = true;
                    e.Handled = true;
                }
                else
                {
                    e.Handled = true;
                }
            }
        }

        public void Deselect()
        {
            try
            {
                // Remove selection on clicking anywhere the window
                if (selected)
                {
                    selected = false;
                    if (selectedElement != null)
                    {
                        // Remove the adorner from the selected element
                        aLayer.Remove(aLayer.GetAdorners(selectedElement)[0]);
                        selectedElement = null;
                    }
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                MessageBox.Show("DEBUG: " + ex.ToString());
                //throw;
#endif
            }
        }

        ////////////////////////////////////////////

        public AdornerLayer aLayer;

        private bool _isDown;
        private bool _isDragging;
        public bool selected;
        public UIElement selectedElement;

        private Point _startPoint;
        private double _originalLeft;
        private double _originalTop;

        public double OriginalLeft { get => _originalLeft; set => _originalLeft = value; }
        public double OriginalTop { get => _originalTop; set => _originalTop = value; }


        #endregion Resize with Adorners

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double BLeft = Canvas.GetLeft(TitleBorder);
            double BTop = Canvas.GetTop(TitleBorder);
            double BWidth = TitleBorder.ActualWidth;
            double BHeight = TitleBorder.ActualHeight;
            double WWidth = window.ActualWidth;
            double WHeight = window.ActualHeight;
            //width
            if (BWidth >= WWidth - BLeft)
            {
                if (WWidth - BWidth > 0)
                {
                    Canvas.SetLeft(TitleBorder, WWidth - BWidth);
                }
                else
                {
                    TitleBorder.Width = WWidth;
                }
            }
            //height
            if (BHeight >= WHeight - BTop)
            {
                if (WHeight - BHeight > 0)
                {
                    Canvas.SetTop(TitleBorder, WHeight - BHeight);
                }
                else
                {
                    TitleBorder.Height = WHeight;
                }
            }
        }
    }
        
}