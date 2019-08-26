using System;
using System.Windows;
using System.Windows.Input;

namespace SkinText {
    //for TaskbarIcon

    public class ShowMessageCommand : ICommand {

        public void Execute(object parameter) {
            //MessageBox.Show(parameter.ToString());
            foreach (Window item in Application.Current.Windows) {
                if (item.Title.Equals(nameof(SkinText))) {
                    if (item.Visibility == Visibility.Hidden) {
                        item.Show();
                        item.Activate();
                    }
                    else {
                        item.Show();
                        item.Hide();
                    }
                }
            }
        }

        public bool CanExecute(object parameter) => true;

        public event EventHandler CanExecuteChanged {
            add { throw new NotSupportedException(); }
#pragma warning disable RECS0029 // Warns about property or indexer setters and event adders or removers that do not use the value parameter
            remove { }
#pragma warning restore RECS0029 // Warns about property or indexer setters and event adders or removers that do not use the value parameter
        }
    }
}