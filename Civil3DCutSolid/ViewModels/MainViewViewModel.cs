using Autodesk.AutoCAD.ApplicationServices;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;

namespace Civil3DCutSolid.ViewModels
{
    public class MainViewViewModel : BindableBase
    {
        private readonly IEventAggregator _eventAggregator;

        public MainViewViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;

            DocumentCollection documentCollection = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager;
            documentCollection.DocumentActivated += DocumentCollectionDocumentActivated;
        }

        private void DocumentCollectionDocumentActivated(object sender, DocumentCollectionEventArgs e)
        {
            RaiseCloseRequest();
        }

        protected void RaiseCloseRequest()
        {
            OnRequestClose?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler OnRequestClose;
    }
}
