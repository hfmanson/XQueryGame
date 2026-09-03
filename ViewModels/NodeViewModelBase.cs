using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Xslt2Game.ViewModels
{
    // A simple base class implementing INotifyPropertyChanged for data binding
    public abstract class NodeViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public abstract void UpdateField(string AttrName, string AttrValue);
    }
}
