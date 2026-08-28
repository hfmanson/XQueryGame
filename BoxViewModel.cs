using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Xml;

public class BoxViewModel : INotifyPropertyChanged
{
    public Dictionary<string, object> Attributes { get; } = new Dictionary<string, object>();

    public BoxViewModel(XmlElement xml)
    {
        // Load all XML attributes dynamically
        foreach (XmlAttribute attr in xml.Attributes)
        {
            Attributes[attr.Name] = attr.Value;
        }
    }

    public void UpdateAttribute(string name, object value)
    {
        Attributes[name] = value;
        OnPropertyChanged(nameof(Attributes));
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string name)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}