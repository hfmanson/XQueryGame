using Jint;
using Jint.Native;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Xslt2Game
{
    public sealed partial class Boxup : Page
    {
        private XmlDocument _document;
        private readonly Dictionary<XmlAttribute, BoxViewModel> _attributeModels;
        private readonly Dictionary<string, DependencyProperty> _bindings = new Dictionary<string, DependencyProperty>
        {
            ["x"] = Canvas.LeftProperty,
            ["y"] = Canvas.TopProperty,
            ["rotation"] = ExtendedContentControl.RotationProperty,
            ["color"] = Control.ForegroundProperty
        };
        private AttributeConverter _converter;
        private Engine _engine;

        private Binding GetBinding(string attrName)
        {
            return new Binding
            {
                Path = new PropertyPath("Attributes"),
                Mode = BindingMode.OneWay,
                Converter = _converter,
                ConverterParameter = attrName
            };
        }

        public void LoadXML()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "Assets", "boxup.xml");
            _document.Load(path);

            _document.NodeChanged += new XmlNodeChangedEventHandler(this.MyNodeChangedEvent);
            XmlNodeList boxes = _document.DocumentElement.ChildNodes;
            foreach (XmlElement box in boxes)
            {
                string type = box.LocalName; // "bigbox" or "smallbox"
                ControlTemplate template = (ControlTemplate)game.Resources[type];
                BoxViewModel model = new BoxViewModel(box);

                ExtendedContentControl control = new ExtendedContentControl
                {
                    Template = template,
                    DataContext = model
                };

                // Map each attribute to this viewmodel
                foreach (XmlAttribute attr in box.Attributes)
                {
                    _attributeModels[attr] = model;
                    control.SetBinding(_bindings[attr.Name], GetBinding(attr.Name));
                }
                game.Children.Add(control);
            }
        }

        public object testJS(string javascript)
        {
            try
            {
                Debug.WriteLine(javascript);
                object result = _engine.Evaluate(javascript);
                if (result.GetType().IsAssignableFrom(typeof(JsArray)))
                {
                    JsArray array = (JsArray)result;
                    foreach(object el in array)
                    {
                        Debug.WriteLine(el);
                    }
                }
                else
                {
                    Debug.WriteLine(result);
                }
                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                System.Diagnostics.Debugger.Break();   // forces VS to stop here
                return null;
            }
        }

        public object testXQuery(string xquery)
        {
            return testJS($"fontoxpath.evaluateXPath('{xquery}', xmlDocument, myDomFacade, null, null);");
        }


        public object testUpdateXQuery(string xquf)
        {
            return testJS($"fontoxpath.executePendingUpdateList(fontoxpath.evaluateUpdatingExpressionSync(`{xquf}`, xmlDocument, myDomFacade, null, {{ nodesFactory: nodesFactory }}).pendingUpdateList);");
        }


        private void SetupEngine()
        {
            _engine = new Engine(options => {
                options.AllowClr();
                //options.AddExtensionMethods(typeof(Extension));
            });
            _engine.SetValue("xmlDocument", new WrappedNode(_document));
            _engine.SetValue("myDomFacade", new DomFacade());
            _engine.SetValue("nodesFactory", new NodesFactory(_document));
            //_engine.SetValue("documentWriter", new DocumentWriter());
            _engine.SetValue("console", new
            {
                log = new Action<object>(x => Debug.WriteLine($"[JS] {x}")),
                error = new Action<object>(x => Debug.WriteLine($"[JS ERROR] {x}")),
                warn = new Action<object>(x => Debug.WriteLine($"[JS WARN] {x}"))
            });

            var basePath = AppContext.BaseDirectory;
            _engine.Execute(File.ReadAllText(Path.Combine(basePath, "EngineJS", "fontoxpath.js")));
        }

        public Boxup()
        {
            InitializeComponent();
            _document = new XmlDocument();
            _attributeModels = new Dictionary<XmlAttribute, BoxViewModel>();
            _converter = new AttributeConverter();
            LoadXML();
            SetupEngine();
            //testJS("xmlDocument.nodeType");
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
            string xquf = @"
declare %updating function local:move-mover($model-mover, $x, $y) {
  replace value of node $model-mover/@x with $x,
  replace value of node $model-mover/@y with $y  
};

let $model-element := /boxes
    , $model-mover := $model-element/mover

return local:move-mover($model-mover, 2, 2)
";
            //testXQuery(text.Text);
            //testUpdateXQuery(text.Text);
            testUpdateXQuery(xquf);
            //XmlElement? el = (XmlElement)_document.DocumentElement.ChildNodes[0];
            //el?.SetAttribute("color", "orange");
            //DumpTree(Viewbox);
        }
    }
}
