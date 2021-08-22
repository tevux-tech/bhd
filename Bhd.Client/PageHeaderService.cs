using System.ComponentModel;

namespace Bhd.Client {
    public class PageHeaderService : INotifyPropertyChanged {
        private string _currentPageTitle;
        public string CurrentPageTitle {
            get => _currentPageTitle;
            set {
                if (_currentPageTitle == value) {
                    return;
                }

                _currentPageTitle = value;
                NotifyPropertyChanged(nameof(CurrentPageTitle));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void NotifyPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
