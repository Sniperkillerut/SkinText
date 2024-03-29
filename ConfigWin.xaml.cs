﻿using System;
using System.Diagnostics;
using System.IO.Compression;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;

namespace SkinText {

    /// <summary>
    /// Lógica de interacción para ConfigWin.xaml
    /// </summary>
    public partial class ConfigWin : Window {

        public ConfigWin() {
            InitializeComponent();
        }

        #region General

        private void CloseButt_Click(object sender, RoutedEventArgs e) {
            resizecheck.IsChecked = false;
            SkinsListbox.SelectedIndex = 0;
            this.Hide();
            CustomMethods.SaveConfig();
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

        #endregion General

        #region Colors

        private void ClrPcker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) {
            Xceed.Wpf.Toolkit.ColorPicker picker = (Xceed.Wpf.Toolkit.ColorPicker)sender;
            string resource = picker.Name.Substring(picker.Name.IndexOf("_", StringComparison.Ordinal) + 1);
            SolidColorBrush color = new SolidColorBrush(picker.SelectedColor.Value);
            color.Freeze();
            CustomMethods.ChangeBrushResource(color, resource);
            color = null;
        }

        #endregion Colors

        #region Colors Legacy

        /*
        private void ClrPcker_BackgroundColorBrush_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) {
            CustomMethods.ChangeBrushResource(new SolidColorBrush(ClrPcker_BackgroundColorBrush.SelectedColor.Value), "BackgroundColorBrush");
        }
        private void ClrPcker_ButtonBackgroundColorBrush_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) {
            CustomMethods.ChangeBrushResource(new SolidColorBrush(ClrPcker_ButtonBackgroundColorBrush.SelectedColor.Value), "ButtonBackgroundColorBrush");
        }
        private void ClrPcker_ButtonBackgroundMouseOverColorBrush_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) {
            CustomMethods.ChangeBrushResource(new SolidColorBrush(ClrPcker_ButtonBackgroundMouseOverColorBrush.SelectedColor.Value), "ButtonBackgroundMouseOverColorBrush");
        }
        private void ClrPcker_ButtonBackgroundCheckedColorBrush_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) {
            CustomMethods.ChangeBrushResource(new SolidColorBrush(ClrPcker_ButtonBackgroundCheckedColorBrush.SelectedColor.Value), "ButtonBackgroundCheckedColorBrush");
        }

        private void ClrPcker_TextColorBrush_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) {
            CustomMethods.ChangeBrushResource(new SolidColorBrush(ClrPcker_TextColorBrush.SelectedColor.Value), "TextColorBrush");
        }

        private void ClrPcker_FontPickBackgroundColorBrush_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) {
            CustomMethods.ChangeBrushResource(new SolidColorBrush(ClrPcker_FontPickBackgroundColorBrush.SelectedColor.Value), "FontPickBackgroundColorBrush");
        }

        private void ClrPcker_FontPickMouseOverBackgroundColorBrush_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) {
            CustomMethods.ChangeBrushResource(new SolidColorBrush(ClrPcker_FontPickMouseOverBackgroundColorBrush.SelectedColor.Value), "FontPickMouseOverBackgroundColorBrush");
        }

        private void ClrPcker_FontPickMouseOverBorderColorBrush_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) {
            CustomMethods.ChangeBrushResource(new SolidColorBrush(ClrPcker_FontPickMouseOverBorderColorBrush.SelectedColor.Value), "FontPickMouseOverBorderColorBrush");
        }

        private void ClrPcker_FontPickSelectedBackgroundColorBrush_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) {
            CustomMethods.ChangeBrushResource(new SolidColorBrush(ClrPcker_FontPickSelectedBackgroundColorBrush.SelectedColor.Value), "FontPickSelectedBackgroundColorBrush");
        }

        private void ClrPcker_FontPickSelectedBorderColorBrush_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) {
            CustomMethods.ChangeBrushResource(new SolidColorBrush(ClrPcker_FontPickSelectedBorderColorBrush.SelectedColor.Value), "FontPickSelectedBorderColorBrush");
        }

        private void ClrPcker_MenuBackgroundColorBrush_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) {
            CustomMethods.ChangeBrushResource(new SolidColorBrush(ClrPcker_MenuBackgroundColorBrush.SelectedColor.Value), "MenuBackgroundColorBrush");
        }

        private void ClrPcker_MenuItem1BorderColorBrush_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) {
            CustomMethods.ChangeBrushResource(new SolidColorBrush(ClrPcker_MenuItem1BorderColorBrush.SelectedColor.Value), "MenuItem1BorderColorBrush");
        }

        private void ClrPcker_MenuItem2HighlightTextColorBrush_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) {
            CustomMethods.ChangeBrushResource(new SolidColorBrush(ClrPcker_MenuItem2HighlightTextColorBrush.SelectedColor.Value), "MenuItem2HighlightTextColorBrush");
        }

        private void ClrPcker_MenuItem2HighlightBorderColorBrush_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) {
            CustomMethods.ChangeBrushResource(new SolidColorBrush(ClrPcker_MenuItem2HighlightBorderColorBrush.SelectedColor.Value), "MenuItem2HighlightBorderColorBrush");
        }

        private void ClrPcker_MenuItem2DisabledColorBrush_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) {
            CustomMethods.ChangeBrushResource(new SolidColorBrush(ClrPcker_MenuItem2DisabledColorBrush.SelectedColor.Value), "MenuItem2DisabledColorBrush");
        }

        private void ClrPcker_FontPickTextColorBrush_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) {
            CustomMethods.ChangeBrushResource(new SolidColorBrush(ClrPcker_FontPickTextColorBrush.SelectedColor.Value), "FontPickTextColorBrush");
        }

        private void ClrPcker_ButtonFrontColorBrush_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) {
            CustomMethods.ChangeBrushResource(new SolidColorBrush(ClrPcker_ButtonFrontColorBrush.SelectedColor.Value), "ButtonFrontColorBrush");
        }

        private void ClrPcker_ButtonBorderMouseOverColorBrush_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) {
            CustomMethods.ChangeBrushResource(new SolidColorBrush(ClrPcker_ButtonBorderMouseOverColorBrush.SelectedColor.Value), "ButtonBorderMouseOverColorBrush");
        }

        private void ClrPcker_ButtonBorderCheckedColorBrush_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) {
            CustomMethods.ChangeBrushResource(new SolidColorBrush(ClrPcker_ButtonBorderCheckedColorBrush.SelectedColor.Value), "ButtonBorderCheckedColorBrush");
        }

        private void ClrPcker_BorderColorBrush_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) {
            CustomMethods.ChangeBrushResource(new SolidColorBrush(ClrPcker_BorderColorBrush.SelectedColor.Value), "BorderColorBrush");
        }
        private void ClrPcker_RTBBackgroundColorBrush_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) {
            CustomMethods.ChangeBrushResource(new SolidColorBrush(ClrPcker_RTBBackgroundColorBrush.SelectedColor.Value), "RTBBackgroundColorBrush");
        }

        private void ClrPcker_MainWindowBackgroundColorBrush_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) {
            CustomMethods.ChangeBrushResource(new SolidColorBrush(ClrPcker_MainWindowBackgroundColorBrush.SelectedColor.Value),"MainWindowBackgroundColorBrush");
        }
        */

        #endregion Colors Legacy

        #region Text

        private void Bordersize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            CustomMethods.RtbBorderSize(bordersize.Value);
        }

        private void FlipXButton_Checked(object sender, RoutedEventArgs e) {
            CustomMethods.RtbFlip(FlipXButton.IsChecked.Value, FlipYButton.IsChecked.Value);
        }

        private void ReadOnlyCheck_Checked(object sender, RoutedEventArgs e) {
            CustomMethods.RtbReadOnly(readOnlyCheck.IsChecked.Value);
        }

        private void ResizeCheckBoxChanged(object sender, RoutedEventArgs e) {
            CustomMethods.ResizeRtb(resizecheck.IsChecked.Value);
        }

        private void SlValue_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            CustomMethods.RtbRotate(slValue.Value);
        }

        private void Spellcheck_Checked(object sender, RoutedEventArgs e) {
            CustomMethods.RtbSpellCheck(spellcheck.IsChecked.Value);
        }

        private void TextOpacityslider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            CustomMethods.RtbOpacity(textopacityslider.Value / 100);
        }

        private void CornerRadius1Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            CustomMethods.MainW.stackborder.CornerRadius = new CornerRadius(CornerRadius1Slider.Value, CornerRadius2Slider.Value, CornerRadius3Slider.Value, CornerRadius4Slider.Value);
        }

        private void LockSlidersCheckbox_Checked(object sender, RoutedEventArgs e) {
            if (lockSlidersCheckbox.IsChecked.Value) {
                System.Windows.Data.Binding myBinding = new System.Windows.Data.Binding {
                    Source = CornerRadius1Slider,
                    Path = new PropertyPath("Value"),
                    Mode = System.Windows.Data.BindingMode.TwoWay,
                    UpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged
                };
                System.Windows.Data.BindingOperations.SetBinding(CornerRadius2Slider, System.Windows.Controls.Primitives.RangeBase.ValueProperty, myBinding);
                System.Windows.Data.BindingOperations.SetBinding(CornerRadius3Slider, System.Windows.Controls.Primitives.RangeBase.ValueProperty, myBinding);
                System.Windows.Data.BindingOperations.SetBinding(CornerRadius4Slider, System.Windows.Controls.Primitives.RangeBase.ValueProperty, myBinding);
            }
            else {
                System.Windows.Data.BindingOperations.ClearBinding(CornerRadius2Slider, System.Windows.Controls.Primitives.RangeBase.ValueProperty);
                System.Windows.Data.BindingOperations.ClearBinding(CornerRadius3Slider, System.Windows.Controls.Primitives.RangeBase.ValueProperty);
                System.Windows.Data.BindingOperations.ClearBinding(CornerRadius4Slider, System.Windows.Controls.Primitives.RangeBase.ValueProperty);
            }
        }

        private void TextWrap_Checked(object sender, RoutedEventArgs e) {
            CustomMethods.LineWrap(TextWrap.IsChecked.Value);
        }

        private void ToolBarEnabled_Checked(object sender, RoutedEventArgs e) {
            CustomMethods.ToolBarVisible(ToolBarEnabled.IsChecked.Value);
        }

        private void Autosavetimer_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (autosavetimersider.Value > 0) {
                CustomMethods.AutoSaveEnabled = true;
                CustomMethods.AutoSaveTimer = (int)autosavetimersider.Value * 60 * 1000;
            }
            else {
                CustomMethods.AutoSaveEnabled = false;
            }
        }

        #endregion Text

        #region BG Image

        private void Imagedir_Click(object sender, RoutedEventArgs e) {
            CustomMethods.OpenImage();
        }

        private void ClearImage_Click(object sender, RoutedEventArgs e) {
            CustomMethods.ImageClear();
        }

        private void Imageopacityslider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            CustomMethods.WindowImageOpacity(imageopacityslider.Value / 100);
        }

        private void ImageBlurSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            CustomMethods.BlurBGImage(imageBlurSlider.Value, ImageBlurGauss.IsChecked.Value);
        }

        private void ImageBlurBox_Checked(object sender, RoutedEventArgs e) {
            CustomMethods.BlurBGImage(imageBlurSlider.Value, ImageBlurGauss.IsChecked.Value);
        }

        #endregion BG Image

        #region Window Config

        private void NotificationVisible_Unchecked(object sender, RoutedEventArgs e) {
            CustomMethods.WindowVisibleNotification(NotificationVisible.IsChecked.Value);
        }

        private void Alwaysontop_Unchecked(object sender, RoutedEventArgs e) {
            CustomMethods.WindowAlwaysOnTop(alwaysontop.IsChecked.Value);
        }

        private void ResizeVisible_Unchecked(object sender, RoutedEventArgs e) {
            CustomMethods.WindowVisibleResize(ResizeVisible.IsChecked.Value);
        }

        private void Taskbarvisible_Unchecked(object sender, RoutedEventArgs e) {
            CustomMethods.WindowVisibleTaskbar(taskbarvisible.IsChecked.Value);
        }

        private void WindowOpacityslider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            CustomMethods.WindowOpacity(windowopacityslider.Value / 100);
        }

        private void BgBlur_Checked(object sender, RoutedEventArgs e) {
            CustomMethods.BlurBG(BgBlur.IsChecked.Value);
        }

        private void Toolsalwaysontop_Checked(object sender, RoutedEventArgs e) {
            CustomMethods.ToolsAlwaysOnTop(toolsalwaysontop.IsChecked.Value);
        }

        #endregion Window Config

        #region Skins

        private void SkinsListbox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
            if (SkinsListbox.SelectedItem!=null) {
                CustomMethods.SkinList_Information(((System.Windows.Controls.ListBoxItem)(SkinsListbox.SelectedItem)).Content.ToString());
            }
        }

        private void LoadSkin_Click(object sender, RoutedEventArgs e) {
            //LoadDefault calls ClearImage that deletes the bgimg so first clear the path
            CustomMethods.Imagepath = "";
            CustomMethods.LoadDefault();
            string skin = ((System.Windows.Controls.ListBoxItem)(SkinsListbox.SelectedItem)).Content.ToString();
            CustomMethods.CurrentSkin = @"\"+skin;
            CustomMethods.ReadConfig();
            CustomMethods.SaveCurrentState();
        }

        private void CreateSkin_Click(object sender, RoutedEventArgs e) {
            string skin = ((System.Windows.Controls.ListBoxItem)(SkinsListbox.SelectedItem)).Content.ToString();

            if (skin == "Create New Skin") {
                CustomMethods.CreateModifySkin(@"\" + SkinName_TextBox.Text, SkinName_TextBox.Text, SkinAuthor_TextBox.Text, SkinVersion_TextBox.Text, CustomMethods.ScreenShotPath, SkinNotes_TextBox.Text);

                CustomMethods.CurrentSkin = @"\" + SkinName_TextBox.Text;
                CustomMethods.SaveCurrentState();
                //copy img, load img, then save
                string temp = CustomMethods.Imagepath;
                CustomMethods.Imagepath = "";
                CustomMethods.LoadImage(temp);

                CustomMethods.SaveConfig();
                CustomMethods.GetSkinList();


            }
            else {
                if ((@"\" + skin) == CustomMethods.CurrentSkin) {
                    CustomMethods.SaveConfig();
                }
                CustomMethods.CreateModifySkin(skin, SkinName_TextBox.Text, SkinAuthor_TextBox.Text, SkinVersion_TextBox.Text, CustomMethods.ScreenShotPath, SkinNotes_TextBox.Text);
            }
        }

        private void SkinScreenshot_Button_Click(object sender, RoutedEventArgs e) {
            string skin = ((System.Windows.Controls.ListBoxItem)(SkinsListbox.SelectedItem)).Content.ToString();
            if (!string.IsNullOrWhiteSpace(skin) && skin == "Create New Skin") {
                skin = SkinName_TextBox.Text;
            }
            CustomMethods.SelectScreenshot(skin);
        }

        private void ExportSkin_Click(object sender, RoutedEventArgs e) {
            string skinName = SkinName_TextBox.Text;
            //set the name to the user specified name
            string skinFolder = ((System.Windows.Controls.ListBoxItem)(SkinsListbox.SelectedItem)).Content.ToString();
            //TODO: check if skin is null on all function calls
            if (!string.IsNullOrWhiteSpace(skinFolder) && skinFolder != "Create New Skin") {
                CustomMethods.ExportSkin(skinFolder,skinName);
            }
        }
        private void ImportSkin_Click(object sender, RoutedEventArgs e) {
            CustomMethods.OpenImportSkin();
        }

        private void SkinsListbox_KeyUp(object sender, KeyEventArgs e) {
            if (e.SystemKey.Equals(Key.Delete) || e.Key.Equals(Key.Delete)) {
                string skin = ((System.Windows.Controls.ListBoxItem)(SkinsListbox.SelectedItem)).Content.ToString();
                if (!string.IsNullOrWhiteSpace(skin) && skin != "Create New Skin") {
                    CustomMethods.DeleteSkin(skin);
                    CustomMethods.GetSkinList();
                }
            }
        }
        #endregion Skins

        private void GoToReddit_Click(object sender, RoutedEventArgs e) {
            Process.Start("https://www.reddit.com/r/SkinText/");
        }

        private void GoToWebPage_Click(object sender, RoutedEventArgs e) {
            //TODO: WebPage link
            //Process.Start("http://google.com");
        }

        private void GoToDeviantArt_Click(object sender, RoutedEventArgs e) {
            Process.Start("https://skintext.deviantart.com/");
        }

        private void GoToLicense_Click(object sender, RoutedEventArgs e) {
            Process.Start("https://creativecommons.org/licenses/by-nc-sa/4.0/");
        }

        private void GoToEdefex_Click(object sender, RoutedEventArgs e) {
            //TODO: EdeFex link
            Process.Start("https://www.twitch.tv/edefexarts");
        }

        private void GoToMacku_Click(object sender, RoutedEventArgs e) {
            //TODO: Macku link
            //Process.Start("http://google.com");
        }

        private void GoToPoisonbleed_Click(object sender, RoutedEventArgs e) {
            //TODO: PoisonBleed link
            //Process.Start("http://google.com");
        }

        private void GradientPicker1_ColorChanged(object sender, ColorBox.ColorChangedEventArgs e) {
            //change color changed to other less spammy event and that actually changes the brush, not the selected color
            //CustomMethods.ChangeBrushResource(GradientPicker1.Brush.Clone(), "BackgroundColorBrush");
        }

        private void StartWithWindows_Checked(object sender, RoutedEventArgs e)
        {
            CustomMethods.StartWithWindows(StartWithWindows.IsChecked.Value);
        }

        private void ExploreSkinsFolder_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(CustomMethods.AppDataPath);
        }

        private void RegisterFileTypes_Checked(object sender, RoutedEventArgs e)
        {
            CustomMethods.RegisterFileTypes(RegisterFileTypes.IsChecked.Value);
        }

        private void HelpButt_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://skintext-documentation.readthedocs.io/en/latest/index.html");
        }
    }
}