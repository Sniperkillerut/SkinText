using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace SkinText
{
    public partial class FontConfig : Window
    {
        public FontConfig()
        {
            InitializeComponent();
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) {
                this.DragMove();
            }
        }
        private void CloseButt_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            CustomMethods.SaveConfig();
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void ClrPcker_Font_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            try
            {
                if (textrun!= null) {
                    textrun.Foreground = new SolidColorBrush(ClrPcker_Font.SelectedColor.Value);
                }
            }
            catch (Exception ex)
            {
                #if DEBUG
                    MessageBox.Show(ex.ToString());
                    //throw;
                #endif
            }
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void ClrPcker_Bg_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            try
            {
                if (textrun != null) {
                    SolidColorBrush brush = new SolidColorBrush(ClrPcker_Bg.SelectedColor.Value);
                    if (brush.Color.A<255)
                    {
                        brush =  Brushes.Transparent;
                    }
                    textrun.Background = brush;
                    ClrPcker_Bg.SelectedColor = brush.Color;
                }
            }
            catch (Exception ex) {
                #if DEBUG
                    MessageBox.Show(ex.ToString());
                    //throw;
                #endif
            }
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void Superscript_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (textrun != null) {
                    System.Windows.Controls.RadioButton btn = (System.Windows.Controls.RadioButton)sender;
                    if (btn != null && btn.IsChecked.Value)
                    {
                        switch (btn.Name)
                        {
                            case nameof(topScript):
                                {
                                    textrun.BaselineAlignment = BaselineAlignment.Top;
                                    break;
                                }
                            case nameof(superscript):
                                {
                                    textrun.BaselineAlignment = BaselineAlignment.Subscript;
                                    break;
                                }
                            case nameof(texttopScript):
                                {
                                    textrun.BaselineAlignment = BaselineAlignment.TextTop;
                                    break;
                                }
                            case nameof(centerScript):
                                {
                                    textrun.BaselineAlignment = BaselineAlignment.Center;
                                    break;
                                }
                            case nameof(subscript):
                                {
                                    textrun.BaselineAlignment = BaselineAlignment.Subscript;
                                    break;
                                }
                            case nameof(textbottomScript):
                                {
                                    textrun.BaselineAlignment = BaselineAlignment.TextBottom;
                                    break;
                                }
                            case nameof(bottomScript):
                                {
                                    textrun.BaselineAlignment = BaselineAlignment.Bottom;
                                    break;
                                }
                            case nameof(baseScript):
                                {
                                    textrun.BaselineAlignment = BaselineAlignment.Baseline;
                                    break;
                                }
                        }
                    }
                }
            }
            catch (Exception ex) {
                #if DEBUG
                MessageBox.Show(ex.ToString());
                //throw;
                #endif
            }
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void Align_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Windows.Controls.RadioButton btn = (System.Windows.Controls.RadioButton) sender ;
                if (btn != null && btn.IsChecked.Value)
                {
                    switch (btn.Name)
                    {
                        case nameof(leftAlign):
                            {
                                txtSampleText.Selection.Start.Paragraph.TextAlignment = TextAlignment.Left;
                                break;
                            }
                        case nameof(centerAlign):
                            {
                                txtSampleText.Selection.Start.Paragraph.TextAlignment = TextAlignment.Center;
                                break;
                            }
                        case nameof(rightAlign):
                            {
                                txtSampleText.Selection.Start.Paragraph.TextAlignment = TextAlignment.Right;
                                break;
                            }
                        case nameof(justifyAlign):
                            {
                                txtSampleText.Selection.Start.Paragraph.TextAlignment = TextAlignment.Justify;
                                break;
                            }
                    }
                }
            }
            catch (Exception ex) {
                #if DEBUG
                MessageBox.Show(ex.ToString());
                //throw;
                #endif
            }
        }
        private void FontSzLineSz_Click(object sender, RoutedEventArgs e)
        {
            lineHeightSlider.Value = fontSizeSlider.Value;
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            if (textrun.TextDecorations == null)
            {
                textrun.TextDecorations = new TextDecorationCollection();
            }
            CustomMethods.TextFormat(textrun.Foreground, textrun.Background, textrun.FontFamily, textrun.FontSize, textrun.TextDecorations, textrun.FontStyle, textrun.FontWeight, txtSampleText.Selection.Start.Paragraph.TextAlignment, txtSampleText.FlowDirection, textrun.BaselineAlignment, textpar.LineHeight);
        }
    }
}