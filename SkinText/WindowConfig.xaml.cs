using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace SkinText {

    public partial class WindowConfig : Window {

        public WindowConfig() {
            InitializeComponent();
        }

        private void CloseButt_Click(object sender, RoutedEventArgs e) {
            this.Hide();
            CustomMethods.SaveConfig();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e) {
            if (e.ChangedButton == MouseButton.Left) {
                this.DragMove();
            }
        }

        private void Bordersize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            CustomMethods.RtbBorderSize(bordersize.Value);
        }

        private void SlValue_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            CustomMethods.RtbRotate(slValue.Value);
        }

        private void ResizeCheckBoxChanged(object sender, RoutedEventArgs e) {
            CustomMethods.RtbHideBorder(resizecheck.IsChecked.Value);
        }

        private void ClrPcker_BorderBackground_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) {
            CustomMethods.RtbBorderColor(new SolidColorBrush(ClrPcker_BorderBackground.SelectedColor.Value));
        }

        private void Imagedir_Click(object sender, RoutedEventArgs e) {
            CustomMethods.OpenImage();
        }

        private void ClrPcker_WindowBackground_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) {
            CustomMethods.WindowBackgroundColor(new SolidColorBrush(ClrPcker_WindowBackground.SelectedColor.Value));
        }

        private void ClrPcker_RTB_Background_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) {
            CustomMethods.RtbBackgroundColor(new SolidColorBrush(ClrPcker_RTB_Background.SelectedColor.Value));
        }

        private void Imageopacityslider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            CustomMethods.WindowImageOpacity(imageopacityslider.Value / 100);
        }

        private void TextOpacityslider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            CustomMethods.RtbOpacity(textopacityslider.Value / 100);
        }

        private void WindowOpacityslider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            CustomMethods.WindowOpacity(windowopacityslider.Value / 100);
        }

        private void ReadOnlyCheck_Checked(object sender, RoutedEventArgs e) {
            CustomMethods.RtbReadOnly(readOnlyCheck.IsChecked.Value);
        }

        private void Spellcheck_Checked(object sender, RoutedEventArgs e) {
            CustomMethods.RtbSpellCheck(spellcheck.IsChecked.Value);
        }

        private void Alwaysontop_Unchecked(object sender, RoutedEventArgs e) {
            CustomMethods.WindowAlwaysOnTop(alwaysontop.IsChecked.Value);
        }

        private void Taskbarvisible_Unchecked(object sender, RoutedEventArgs e) {
            CustomMethods.WindowVisibleTaskbar(taskbarvisible.IsChecked.Value);
        }

        private void NotificationVisible_Unchecked(object sender, RoutedEventArgs e) {
            CustomMethods.WindowVisibleNotification(NotificationVisible.IsChecked.Value);
        }

        private void ResizeVisible_Unchecked(object sender, RoutedEventArgs e) {
            CustomMethods.WindowVisibleResize(ResizeVisible.IsChecked.Value);
        }

        private void FlipXButton_Checked(object sender, RoutedEventArgs e) {
            CustomMethods.RtbFlip(FlipXButton.IsChecked.Value, FlipYButton.IsChecked.Value);
        }
    }
}