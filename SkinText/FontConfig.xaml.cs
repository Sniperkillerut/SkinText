using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace SkinText
{
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
            catch (System.NullReferenceException)
            {}
            catch (Exception)
            {
                throw;
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
            catch (System.NullReferenceException)
            {}
            catch (Exception)
            {
               throw;
            }
        }
        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            if (textrun.TextDecorations == null)
            {
                textrun.TextDecorations = new TextDecorationCollection();
            }
            par.TextFormat(textrun.Foreground, textrun.Background, textrun.FontFamily, textrun.FontSize, textrun.TextDecorations, textrun.FontStyle, textrun.FontWeight, txtSampleText.Selection.Start.Paragraph.TextAlignment, txtSampleText.FlowDirection, textrun.BaselineAlignment, textpar.LineHeight);
        }
        private void FlowDir_Checked(object sender, RoutedEventArgs e)
        {
            if (FlowDir.IsChecked.Value)
            {
                txtSampleText.FlowDirection = FlowDirection.RightToLeft;
            }
            else
            {
                txtSampleText.FlowDirection = FlowDirection.LeftToRight;
            }
        }
        private void Superscript_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Windows.Controls.RadioButton btn = (System.Windows.Controls.RadioButton)sender;
                if (btn != null && btn.IsChecked.Value)
                {
                    switch (btn.Name)
                    {
                        case "topScript":
                            {
                                textrun.BaselineAlignment = BaselineAlignment.Top;
                                break;
                            }
                        case "superscript":
                            {
                                textrun.BaselineAlignment = BaselineAlignment.Subscript;
                                break;
                            }
                        case "texttopScript":
                            {
                                textrun.BaselineAlignment = BaselineAlignment.TextTop;
                                break;
                            }
                        case "centerScript":
                            {
                                textrun.BaselineAlignment = BaselineAlignment.Center;
                                break;
                            }
                        case "subscript":
                            {
                                textrun.BaselineAlignment = BaselineAlignment.Subscript;
                                break;
                            }
                        case "textbottomScript":
                            {
                                textrun.BaselineAlignment = BaselineAlignment.TextBottom;
                                break;
                            }
                        case "bottomScript":
                            {
                                textrun.BaselineAlignment = BaselineAlignment.Bottom;
                                break;
                            }
                        case "baseScript":
                            {
                                textrun.BaselineAlignment = BaselineAlignment.Baseline;
                                break;
                            }
                    }
                }
            }
            catch (System.NullReferenceException)
            {}
            catch (Exception)
            {
                throw;
            }
        }
        private void Align_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Windows.Controls.RadioButton btn = (System.Windows.Controls.RadioButton) sender ;
                if (btn != null && btn.IsChecked.Value)
                {
                    switch (btn.Name)
                    {
                        case "leftAlign":
                            {
                                txtSampleText.Selection.Start.Paragraph.TextAlignment = TextAlignment.Left;
                                break;
                            }
                        case "centerAlign":
                            {
                                txtSampleText.Selection.Start.Paragraph.TextAlignment = TextAlignment.Center;
                                break;
                            }
                        case "rightAlign":
                            {
                                txtSampleText.Selection.Start.Paragraph.TextAlignment = TextAlignment.Right;
                                break;
                            }
                        case "justifyAlign":
                            {
                                txtSampleText.Selection.Start.Paragraph.TextAlignment = TextAlignment.Justify;
                                break;
                            }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void FontSzLineSz_Click(object sender, RoutedEventArgs e)
        {
            lineHeightSlider.Value = fontSizeSlider.Value;
        }
    }
}