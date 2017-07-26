using System.Windows.Input;

namespace SkinText
{
    public static class CustomCommands
    {
        public static readonly RoutedUICommand Exit = new RoutedUICommand
                (
                        "Exit",
                        "Exit",
                        typeof(CustomCommands),
                        new InputGestureCollection()
                        {
                            new KeyGesture(Key.F4, ModifierKeys.Alt)
                        }
                );
        public static readonly RoutedUICommand Save = new RoutedUICommand
                (
                        "Save",
                        "Save",
                        typeof(CustomCommands),
                        new InputGestureCollection()
                        {
                            new KeyGesture(Key.S, ModifierKeys.Control)
                        }
                );
        public static readonly RoutedUICommand SaveAs = new RoutedUICommand
                (
                        "SaveAs",
                        "SaveAs",
                        typeof(CustomCommands),
                        new InputGestureCollection()
                        {
                            new KeyGesture(Key.S, ModifierKeys.Control | ModifierKeys.Shift)
                        }
                );
        public static readonly RoutedUICommand Open = new RoutedUICommand
                (
                        "Open",
                        "Open",
                        typeof(CustomCommands),
                        new InputGestureCollection()
                        {
                            new KeyGesture(Key.O, ModifierKeys.Control)
                        }
                );
        public static readonly RoutedUICommand New = new RoutedUICommand
                (
                        "New",
                        "New",
                        typeof(CustomCommands),
                        new InputGestureCollection()
                        {
                            new KeyGesture(Key.N, ModifierKeys.Control)
                        }
                );
        //Define more commands here, just like the one above
    }
}
