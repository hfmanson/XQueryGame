using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Linq;

public class BoxViewModel : INotifyPropertyChanged
{
    public Dictionary<string, string> Attributes { get; } = new Dictionary<string, string>();

    public BoxViewModel(XElement xml)
    {
        // Load all XML attributes dynamically
        foreach (XAttribute attr in xml.Attributes())
        {
            Attributes[attr.Name.LocalName] = attr.Value;
        }
    }

    public void UpdateAttribute(string name, string value)
    {
        Attributes[name] = value;
        OnPropertyChanged(nameof(Attributes));
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string name)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}