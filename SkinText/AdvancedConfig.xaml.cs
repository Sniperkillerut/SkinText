using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace SkinText {
    /// <summary>
    /// Lógica de interacción para AdvancedConfig.xaml
    /// </summary>
    public partial class AdvancedConfig : Window
    {
        public AdvancedConfig()
        {
            InitializeComponent();
        }

        private void CloseButt_Click(object sender, RoutedEventArgs e) {
            this.Hide();
            CustomMethods.SaveConfig();
        }

        private void ClrPcker_BackgroundColorBrush_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) {
            SolidColorBrush brush = new SolidColorBrush(ClrPcker_BackgroundColorBrush.SelectedColor.Value);
            if (brush != null) {
                brush.Freeze();
                Application.Current.Resources["BackgroundColorBrush"] = brush;
                brush = null;
            }
        }

        private void ClrPcker_ButtonBackgroundColorBrush_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) {
            SolidColorBrush brush = new SolidColorBrush(ClrPcker_ButtonBackgroundColorBrush.SelectedColor.Value);
            if (brush != null) {
                brush.Freeze();
                Application.Current.Resources["ButtonBackgroundColorBrush"] = brush;
                brush = null;
            }
        }

        private void ClrPcker_ButtonBackgroundMouseOverColorBrush_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) {
            SolidColorBrush brush = new SolidColorBrush(ClrPcker_ButtonBackgroundMouseOverColorBrush.SelectedColor.Value);
            if (brush != null) {
                brush.Freeze();
                Application.Current.Resources["ButtonBackgroundMouseOverColorBrush"] = brush;
                brush = null;
            }
        }

        private void ClrPcker_ButtonBackgroundCheckedColorBrush_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) {
            SolidColorBrush brush = new SolidColorBrush(ClrPcker_ButtonBackgroundCheckedColorBrush.SelectedColor.Value);
            if (brush != null) {
                brush.Freeze();
                Application.Current.Resources["ButtonBackgroundCheckedColorBrush"] = brush;
                brush = null;
            }
        }

        private void ClrPcker_TextColorBrush_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) {
            SolidColorBrush brush = new SolidColorBrush(ClrPcker_TextColorBrush.SelectedColor.Value);
            if (brush != null) {
                brush.Freeze();
                Application.Current.Resources["TextColorBrush"] = brush;
                brush = null;
            }
        }

        private void ClrPcker_FontPickBackgroundColorBrush_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) {
            SolidColorBrush brush = new SolidColorBrush(ClrPcker_FontPickBackgroundColorBrush.SelectedColor.Value);
            if (brush != null) {
                brush.Freeze();
                Application.Current.Resources["FontPickBackgroundColorBrush"] = brush;
                brush = null;
            }
        }

        private void ClrPcker_FontPickMouseOverBackgroundColorBrush_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) {
            SolidColorBrush brush = new SolidColorBrush(ClrPcker_FontPickMouseOverBackgroundColorBrush.SelectedColor.Value);
            if (brush != null) {
                brush.Freeze();
                Application.Current.Resources["FontPickMouseOverBackgroundColorBrush"] = brush;
                brush = null;
            }
        }

        private void ClrPcker_FontPickMouseOverBorderColorBrush_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) {
            SolidColorBrush brush = new SolidColorBrush(ClrPcker_FontPickMouseOverBorderColorBrush.SelectedColor.Value);
            if (brush != null) {
                brush.Freeze();
                Application.Current.Resources["FontPickMouseOverBorderColorBrush"] = brush;
                brush = null;
            }
        }

        private void ClrPcker_FontPickSelectedBackgroundColorBrush_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) {
            SolidColorBrush brush = new SolidColorBrush(ClrPcker_FontPickSelectedBackgroundColorBrush.SelectedColor.Value);
            if (brush != null) {
                brush.Freeze();
                Application.Current.Resources["FontPickSelectedBackgroundColorBrush"] = brush;
                brush = null;
            }
        }

        private void ClrPcker_FontPickSelectedBorderColorBrush_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) {
            SolidColorBrush brush = new SolidColorBrush(ClrPcker_FontPickSelectedBorderColorBrush.SelectedColor.Value);
            if (brush != null) {
                brush.Freeze();
                Application.Current.Resources["FontPickSelectedBorderColorBrush"] = brush;
                brush = null;
            }
        }

        private void ClrPcker_MenuBackgroundColorBrush_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) {
            SolidColorBrush brush = new SolidColorBrush(ClrPcker_MenuBackgroundColorBrush.SelectedColor.Value);
            if (brush != null) {
                brush.Freeze();
                Application.Current.Resources["MenuBackgroundColorBrush"] = brush;
                brush = null;
            }
        }

        private void ClrPcker_MenuItem1BorderColorBrush_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) {
            SolidColorBrush brush = new SolidColorBrush(ClrPcker_MenuItem1BorderColorBrush.SelectedColor.Value);
            if (brush != null) {
                brush.Freeze();
                Application.Current.Resources["MenuItem1BorderColorBrush"] = brush;
                brush = null;
            }
        }

        private void ClrPcker_MenuItem2HighlightTextColorBrush_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) {
            SolidColorBrush brush = new SolidColorBrush(ClrPcker_MenuItem2HighlightTextColorBrush.SelectedColor.Value);
            if (brush != null) {
                brush.Freeze();
                Application.Current.Resources["MenuItem2HighlightTextColorBrush"] = brush;
                brush = null;
            }
        }

        private void ClrPcker_MenuItem2HighlightBorderColorBrush_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) {
            SolidColorBrush brush = new SolidColorBrush(ClrPcker_MenuItem2HighlightBorderColorBrush.SelectedColor.Value);
            if (brush != null) {
                brush.Freeze();
                Application.Current.Resources["MenuItem2HighlightBorderColorBrush"] = brush;
                brush = null;
            }
        }

        private void ClrPcker_MenuItem2DisabledColorBrush_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) {
            SolidColorBrush brush = new SolidColorBrush(ClrPcker_MenuItem2DisabledColorBrush.SelectedColor.Value);
            if (brush != null) {
                brush.Freeze();
                Application.Current.Resources["MenuItem2DisabledColorBrush"] = brush;
                brush = null;
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e) {
            if (e.ChangedButton == MouseButton.Left) {
                this.DragMove();
            }
        }

        private void ClrPcker_FontPickTextColorBrush_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) {
            SolidColorBrush brush = new SolidColorBrush(ClrPcker_FontPickTextColorBrush.SelectedColor.Value);
            if (brush != null) {
                brush.Freeze();
                Application.Current.Resources["FontPickTextColorBrush"] = brush;
                brush = null;
            }
        }

        private void ClrPcker_ButtonFrontColorBrush_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) {
            SolidColorBrush brush = new SolidColorBrush(ClrPcker_ButtonFrontColorBrush.SelectedColor.Value);
            if (brush != null) {
                brush.Freeze();
                Application.Current.Resources["ButtonFrontColorBrush"] = brush;
                brush = null;
            }
        }

        private void ClrPcker_ButtonBorderMouseOverColorBrush_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) {
            SolidColorBrush brush = new SolidColorBrush(ClrPcker_ButtonBorderMouseOverColorBrush.SelectedColor.Value);
            if (brush != null) {
                brush.Freeze();
                Application.Current.Resources["ButtonBorderMouseOverColorBrush"] = brush;
                brush = null;
            }
        }

        private void ClrPcker_ButtonBorderCheckedColorBrush_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) {
            SolidColorBrush brush = new SolidColorBrush(ClrPcker_ButtonBorderCheckedColorBrush.SelectedColor.Value);
            if (brush != null) {
                brush.Freeze();
                Application.Current.Resources["ButtonBorderCheckedColorBrush"] = brush;
                brush = null;
            }
        }
    }
}
