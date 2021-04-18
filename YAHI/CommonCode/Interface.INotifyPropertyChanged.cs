namespace DevBot9.Protocols.Homie {
    public delegate void PropertyChangedEventHandler(object sender, PropertyChangedEventArgs e);
    public delegate void ActionString(string parameter);
    public interface INotifyPropertyChanged {
        event PropertyChangedEventHandler PropertyChanged;
    }
}
