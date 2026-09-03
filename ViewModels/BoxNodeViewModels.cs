using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.UI.Xaml.Controls;

namespace Xslt2Game.ViewModels
{
    public class BoxNodeViewModels : NodeViewModelsBase
    {
        override public XDocument LoadGame(Canvas game)
        {
            XDocument document = LoadXML("boxup.xml");
            foreach (XElement element in document.Root.Elements())
            {
                BoxNodeViewModel model = new BoxNodeViewModel(element);
                AddAttributeModel(model, element);
                BoxControl control = new BoxControl();
                control.ViewModel = model;
                game.Children.Add(control);
            }
            return document;
        }
    }
}
