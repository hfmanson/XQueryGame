using Jint;
using Jint.Native;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Xslt2Game.ViewModels;

namespace Xslt2Game
{
    public static class GameAssets
    {
        private static readonly Dictionary<string, (PointCollection points, Brush color)> TypesRegistry =
            new Dictionary<string, (PointCollection, Brush)>();

        private static string BigBoxPoints = "0.1,0.1 0.1,0.9 0.9,0.9 0.9,0.1";
        private static string SmallBoxPoints = "0.225,0.225 0.225,0.8 0.8,0.8 0.8,0.225";
        //private static string MoverPoints = "0.375,0.375 0.625,0.375 0.625,0.625 0.375,0.625";

        static GameAssets()
        {
            // Register your "box-types" with native objects
            TypesRegistry["normal"] = (
                (PointCollection)XamlBindingHelper.ConvertValue(typeof(PointCollection), BigBoxPoints),
                new SolidColorBrush(Colors.Black)
            );
            TypesRegistry["destination"] = (
                (PointCollection)XamlBindingHelper.ConvertValue(typeof(PointCollection), BigBoxPoints),
                new SolidColorBrush(Colors.Blue)
            );
            TypesRegistry["destination"] = (
                (PointCollection)XamlBindingHelper.ConvertValue(typeof(PointCollection), SmallBoxPoints),
                new SolidColorBrush(Colors.Red)
            );
        }

        public static (PointCollection Points, Brush Foreground) GetAssetsForType(string typeName)
        {
            if (TypesRegistry.TryGetValue(typeName ?? "", out var assets)) return assets;
            return TypesRegistry["normal"]; // Fallback
        }
    }
    public sealed partial class Boxup : Page
    {
        private XDocument _document;
        private readonly Dictionary<XAttribute, BoxViewModel> _attributeModels;
        private readonly Dictionary<string, DependencyProperty> _bindings = new Dictionary<string, DependencyProperty>
        {
            ["x"] = Canvas.LeftProperty,
            ["y"] = Canvas.TopProperty,
            ["rotation"] = ExtendedContentControl.RotationProperty,
            ["color"] = Control.ForegroundProperty
        };
        private readonly Dictionary<string, string> _boxtypes = new Dictionary<string, string>
        {
            ["normal"] = "bigbox",
            ["destination"] = "bigbox",
            ["source"] = "smallbox"
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

        // Example input: <box row="3" column="5" dx="1" dy="0" box-type="normal"/>
        public static BoxNodeViewModel CreateFromXml(XElement element)
        {
            var vm = new BoxNodeViewModel();

            // 1. Position Setup (Converts Row/Column to Left/Top)
            int row = int.Parse(element.Attribute("row")?.Value ?? "1");
            vm.UpdateRow(row);
            int col = int.Parse(element.Attribute("column")?.Value ?? "1");
            vm.UpdateColum(col);

            double dx = double.Parse(element.Attribute("dx")?.Value ?? "0");
            double dy = double.Parse(element.Attribute("dy")?.Value ?? "0");
            vm.UpdateRotation(dx, dy);

            string boxType = element.Attribute("box-type")?.Value;
            var assets = GameAssets.GetAssetsForType(boxType);
            vm.Points = assets.Points;
            vm.Foreground = assets.Foreground;

            return vm;
        }

        public void LoadXML()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "Assets", "boxup.xml");
            _document = XDocument.Load(path);

            //_document.NodeChanged += new XmlNodeChangedEventHandler(this.MyNodeChangedEvent);
            _document.Changed += _document_Changed;
            foreach (XElement box in _document.Root.Elements())
            {
                XAttribute boxtypeAttr = box.Attribute(XName.Get("box-type", "http://mansoft.nl/boxup"));
                string boxtype;
                if (boxtypeAttr == null)
                {
                    boxtype = "mover";
                }
                else
                {
                    boxtype = _boxtypes[boxtypeAttr.Value];
                }
                ControlTemplate template = (ControlTemplate)game.Resources[boxtype];
                BoxViewModel model = new BoxViewModel(box);

                ExtendedContentControl control = new ExtendedContentControl
                {
                    Template = template,
                    DataContext = model
                };

                // Map each attribute to this viewmodel
                foreach (XAttribute attr in box.Attributes())
                {
                    if (attr.Name.NamespaceName == "")
                    {
                        _attributeModels[attr] = model;
                        control.SetBinding(_bindings[attr.Name.LocalName], GetBinding(attr.Name.LocalName));
                    }
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
            _engine.SetValue("nodesFactory", new NodesFactory());
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
            _document = new XDocument();
            _attributeModels = new Dictionary<XAttribute, BoxViewModel>();
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
        private void _document_Changed(object sender, XObjectChangeEventArgs e)
        {
            if (sender is XAttribute attr)
            {
                if (_attributeModels.TryGetValue(attr, out var model))
                {
                    // Re-parse the attribute and push into VM
                    model.UpdateAttribute(attr.Name.LocalName, attr.Value);
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
