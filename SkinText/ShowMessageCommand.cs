using System;
using System.Windows;
using System.Windows.Input;

namespace SkinText
{
    //for TaskbarIcon

    public class ShowMessageCommand : ICommand
    {
        public void Execute(object parameter)
        {
            //MessageBox.Show(parameter.ToString());
            foreach (Window item in Application.Current.Windows)
            {
                if (item.Title.Equals("SkinText"))
                {
                    if (item.Visibility == Visibility.Hidden)
                    {
                        item.Show();
                        item.Activate();
                    }
                    else
                    {
                        item.Hide();
                    }
                }
            }
        }
        public bool CanExecute(object parameter)
        {
            return true;
        }
        public event EventHandler CanExecuteChanged
        {
            add { throw new NotSupportedException(); }
            remove { }
        }
    }
}
