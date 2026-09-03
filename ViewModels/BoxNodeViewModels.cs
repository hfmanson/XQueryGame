using System.Xml.Linq;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Xslt2Game.Views;

namespace Xslt2Game.ViewModels
{
    public class BoxNodeViewModels : NodeViewModelsBase
    {
        override public XDocument LoadGame(Canvas game)
        {
            XDocument document = LoadXML("boxup.xml");
            foreach (XElement element in document.Root.Elements())
            {
                Control control;
                XAttribute boxtype = element.Attribute(XName.Get("box-type"));
                BoxNodeViewModel model = new BoxNodeViewModel(element);
                AddAttributeModel(model, element);
                if (boxtype != null)
                {
                    if (boxtype.Value == "source")
                    {
                        SmallBoxControl smallBoxControl = new SmallBoxControl();
                        smallBoxControl.ViewModel = model;
                        control = smallBoxControl;
                    }
                    else
                    {
                        BigBoxControl bigBoxControl = new BigBoxControl();
                        model.Stroke = new SolidColorBrush(boxtype.Value == "destination" ? Colors.Blue : Colors.Black);
                        bigBoxControl.ViewModel = model;
                        control = bigBoxControl;
                    }
                }
                else
                {
                    MoverControl moverControl = new MoverControl();
                    moverControl.ViewModel = model;
                    control = moverControl;
                }
                game.Children.Add(control);
            }
            return document;
        }
    }
}
