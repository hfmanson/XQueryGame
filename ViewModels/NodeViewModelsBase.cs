using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Xslt2Game.ViewModels
{
    public abstract class NodeViewModelsBase
    {
        private readonly Dictionary<XAttribute, NodeViewModelBase> _attributeModels = new Dictionary<XAttribute, NodeViewModelBase>();

        public abstract XDocument LoadGame(Canvas game, int level);

        private void Document_Changed(object sender, XObjectChangeEventArgs e)
        {
            if (sender is XAttribute attr && _attributeModels.TryGetValue(attr, out NodeViewModelBase model))
            {
                model.UpdateField(attr.Name.LocalName, attr.Value);
            }
        }

        protected XDocument LoadXML(string XMLFile)
        {
            var path = Path.Combine(AppContext.BaseDirectory, "Assets", XMLFile);
            XDocument document = XDocument.Load(path);
            document.Changed += Document_Changed;
            return document;
        }

        protected void AddAttributeModel(NodeViewModelBase model, XElement element)
        {
            foreach (XAttribute attr in element.Attributes())
            {
                _attributeModels[attr] = model;
            }
        }
    }
}
