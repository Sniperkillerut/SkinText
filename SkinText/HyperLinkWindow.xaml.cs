using System;
using System.Windows;
using System.Windows.Input;

namespace SkinText {

    /// <summary>
    /// Lógica de interacción para HyperLinkWindow.xaml
    /// </summary>
    public partial class HyperLinkWindow : Window {

        public HyperLinkWindow() {
            InitializeComponent();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void Window_MouseDown(object sender, MouseButtonEventArgs e) {
            if (Mouse.LeftButton == MouseButtonState.Pressed) {
                try {
                    this.DragMove();
                }
                catch (Exception ex) {
#if DEBUG
                    MessageBox.Show("DEBUG: "+ex.ToString());
                    //throw;
#endif
                    //System.InvalidOperationException
                    //dragdrop with only leftclick
                    //dragdrop must be with pressed click
                }
            }
        }

        private void Window_ContentRendered(object sender, EventArgs e) {
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = true;
        }

        public string HyperNameResult => HyperName.Text;

        public string HyperLinkResult => HyperLink.Text;
    }
}