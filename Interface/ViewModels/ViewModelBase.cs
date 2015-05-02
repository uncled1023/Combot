using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interface.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propname)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propname));
            }
        }

        public class DelegateCommand : System.Windows.Input.ICommand
        {
            private readonly Action _mExecute;
            private readonly Func<bool> _mCanExecute;
            public event EventHandler CanExecuteChanged;

            public DelegateCommand(Action execute)
                : this(execute, () => true) { /* empty */ }

            public DelegateCommand(Action execute, Func<bool> canexecute)
            {
                if (execute == null)
                    throw new ArgumentNullException("execute");
                _mExecute = execute;
                _mCanExecute = canexecute;
            }

            public bool CanExecute(object p)
            {
                return _mCanExecute == null || _mCanExecute();
            }

            public void Execute(object p)
            {
                if (CanExecute(null))
                    _mExecute();
            }

            public void RaiseCanExecuteChanged()
            {
                if (CanExecuteChanged != null)
                    CanExecuteChanged(this, EventArgs.Empty);
            }
        }
    }
}
