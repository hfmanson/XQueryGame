using Jint;
using Jint.Native;
using System;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;

namespace Xslt2Game.EngineJS
{
    public class FontoXPath
    {
        private static Engine _engine;
        public FontoXPath()
        {
            _engine = new Engine(options => {
                options.AllowClr();
                //options.AddExtensionMethods(typeof(Extension));
            });
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

        public void SetDocument(XDocument document)
        {
            _engine.SetValue("xmlDocument", new WrappedNode(document));
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
                    foreach (object el in array)
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

        public object registerXQueryModule(string module)
        {
            return testJS($"fontoxpath.registerXQueryModule(`{module}`);");
        }

        public object testXQuery(string xquery)
        {
            return testJS($"fontoxpath.evaluateXPath(`{xquery}`, xmlDocument, myDomFacade, null, null);");
        }

        public object testUpdateXQuery(string xquf)
        {
            return testJS($"fontoxpath.executePendingUpdateList(fontoxpath.evaluateUpdatingExpressionSync(`{xquf}`, xmlDocument, myDomFacade, null, {{ nodesFactory: nodesFactory }}).pendingUpdateList);");
        }

        public object testUpdateXQuery(string xquf, string prefix, string namespaceURI)
        {
            return testJS($"fontoxpath.executePendingUpdateList(fontoxpath.evaluateUpdatingExpressionSync(`{xquf}`, xmlDocument, myDomFacade, null, {{ nodesFactory: nodesFactory, moduleImports: {{ {prefix}: '{namespaceURI}' }} }}).pendingUpdateList);");
        }
    }
}
