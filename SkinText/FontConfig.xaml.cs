using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace SkinText
{
    /// <summary>
    /// Lógica de interacción para FontConfig.xaml
    /// </summary>
    public partial class FontConfig : Window
    {
        private MainWindow par;
        public FontConfig(MainWindow own)
        {
            InitializeComponent();
            par = own;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtSampleText.Background = par.panel.Background;
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
        private void CloseButt_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            par.SaveConfig();
        }
        public void UpdateStrikethrough()
        {
            if (textrun.TextDecorations != null)
            {
               textrun.TextDecorations.Clear();
            }
            else
            {
                textrun.TextDecorations = new TextDecorationCollection();
            }
            if (OverLine.IsChecked.Value)
            {
                textrun.TextDecorations.Add(TextDecorations.OverLine);
            }
            if (Strikethrough.IsChecked.Value)
            {
                textrun.TextDecorations.Add(TextDecorations.Strikethrough);
            }
            if (Baseline.IsChecked.Value)
            {
                textrun.TextDecorations.Add(TextDecorations.Baseline);
            }
            if (Underline.IsChecked.Value)
            {
                textrun.TextDecorations.Add(TextDecorations.Underline);
            }
        }
        private void Strikethrough_Checked(object sender, RoutedEventArgs e)
        {
            UpdateStrikethrough();
        }
        private void ClrPcker_Font_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            try
            {
                SolidColorBrush brush = new SolidColorBrush(ClrPcker_Font.SelectedColor.Value);
                textrun.Foreground = brush;
            }
            catch (Exception)
            {
            }
        }
        private void ClrPcker_Bg_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            try
            {
                SolidColorBrush brush = new SolidColorBrush(ClrPcker_Bg.SelectedColor.Value);
                if (brush.Color.A<255)
                {
                    brush =  Brushes.Transparent;
                }
                textrun.Background = brush;
            }
            catch (Exception)
            {
            }
        }
        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            if (textrun.TextDecorations == null)
            {
                textrun.TextDecorations = new TextDecorationCollection();
            }
            par.TextFormat(textrun.Foreground, textrun.Background, textrun.FontFamily, textrun.FontSize, textrun.TextDecorations, textrun.FontStyle, textrun.FontWeight, txtSampleText.Selection.Start.Paragraph.TextAlignment, txtSampleText.FlowDirection, textrun.BaselineAlignment);
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (left.Equals(sender))
            {
                txtSampleText.Selection.Start.Paragraph.TextAlignment = TextAlignment.Left;
            }
            if (center.Equals(sender))
            {
                txtSampleText.Selection.Start.Paragraph.TextAlignment = TextAlignment.Center;
            }
            if (right.Equals(sender))
            {
                txtSampleText.Selection.Start.Paragraph.TextAlignment = TextAlignment.Right;
            }
            if (justify.Equals(sender))
            {
                txtSampleText.Selection.Start.Paragraph.TextAlignment = TextAlignment.Justify;
            }
        }
        private void Flowdir_Click(object sender, RoutedEventArgs e)
        {
            if(txtSampleText.FlowDirection == FlowDirection.LeftToRight)
            {
                txtSampleText.FlowDirection = FlowDirection.RightToLeft;
            }
            else
            {
                txtSampleText.FlowDirection = FlowDirection.LeftToRight;
            }
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            if (subscript.Equals(sender))
            {
                textrun.BaselineAlignment = BaselineAlignment.Subscript;
            }
            if (superscript.Equals(sender))
            {
                textrun.BaselineAlignment = BaselineAlignment.Superscript;
            }
            if (right2.Equals(sender))
            {
                textrun.BaselineAlignment = BaselineAlignment.TextTop;
            }
            if (justify2.Equals(sender))
            {
                textrun.BaselineAlignment = BaselineAlignment.Top;
            }
            if (flowdir2.Equals(sender))
            {
                textrun.BaselineAlignment = BaselineAlignment.TextBottom;
            }
        }

        private void fontSizeText_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.SystemKey.Equals(Key.Return) || e.Key.Equals(Key.Return) || e.SystemKey.Equals(Key.Enter) || e.Key.Equals(Key.Enter))
            {
                fontSizeSlider.Focus();
            }
        }
    }
}