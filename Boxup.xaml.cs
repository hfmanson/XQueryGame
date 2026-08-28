using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using System.Xml.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;

namespace Xslt2Game
{
    public sealed partial class Boxup : Page
    {
        private XmlDocument _document;
        private readonly Dictionary<XmlAttribute, BoxViewModel> _attributeModels;
        private AttributeConverter _converter;

        private Binding GetBinding(string attrName)
        {
            return new Binding
            {
                Path = new PropertyPath("Attributes"),
                Converter = _converter,
                ConverterParameter = attrName
            };
        }

        public Boxup()
        {
            this.InitializeComponent();
            _document = new XmlDocument();
            _attributeModels = new Dictionary<XmlAttribute, BoxViewModel>();
            _converter = new AttributeConverter();
            _document.Load("boxup.xml");
            _document.NodeChanged += new XmlNodeChangedEventHandler(this.MyNodeChangedEvent);
            XmlNodeList boxes = _document.DocumentElement.ChildNodes;
            foreach (XmlElement box in boxes)
            {
                string type = box.LocalName; // "bigbox" or "smallbox"
                ControlTemplate template = (ControlTemplate)game.Resources[type];
                BoxViewModel model = new BoxViewModel(box);
                
                // Map each attribute to this viewmodel
                foreach (XmlAttribute attr in box.Attributes)
                {
                    _attributeModels[attr] = model;
                }

                ExtendedContentControl control = new ExtendedContentControl
                {
                    Template = template,
                    DataContext = model
                };
                control.SetBinding(Canvas.LeftProperty, GetBinding("left"));
                control.SetBinding(Canvas.TopProperty, GetBinding("top"));
                control.SetBinding(ExtendedContentControl.RotationProperty, GetBinding("rotation"));
                control.SetBinding(ForegroundProperty, GetBinding("color"));
                game.Children.Add(control);
            }
        }

        void DumpTree(DependencyObject obj, int indent = 0)
        {
            var spaces = new string(' ', indent);
            Debug.WriteLine($"{spaces}{obj.GetType().Name}");

            int count = VisualTreeHelper.GetChildrenCount(obj);
            for (int i = 0; i < count; i++)
            {
                DumpTree(VisualTreeHelper.GetChild(obj, i), indent + 2);
            }
        }

        // Handle the NodeChanged event.
        private void MyNodeChangedEvent(object source, XmlNodeChangedEventArgs args)
        {
            if (args.Node.NodeType == XmlNodeType.Text &&
                args.Node.ParentNode is XmlAttribute attr)
            {
                if (_attributeModels.TryGetValue(attr, out var model))
                {
                    // Re-parse the attribute and push into VM
                    model.UpdateAttribute(attr.Name, attr.Value);
                }
            }
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            XmlElement el = (XmlElement)_document.DocumentElement.ChildNodes[0];
            el.SetAttribute("color", "orange");
            //DumpTree(Viewbox);
        }
    }
}
