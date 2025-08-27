using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Civil3DDataExport.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void RaiseCloseRequest()
        {
            OnRequestClose?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler OnRequestClose;

        protected void RaiseHideRequest()
        {
            OnRequestHide?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler OnRequestHide;
    }
}
