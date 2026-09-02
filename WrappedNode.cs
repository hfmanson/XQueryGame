using System.Diagnostics;
using System.Xml.Linq;

namespace Xslt2Game
{
    public class WrappedNode
    {
        private XObject node;

        public WrappedNode(XObject node) { this.node = node; }

        public static WrappedNode wrap(XObject node)
        {
            return node == null ? null : new WrappedNode(node);
        }

        public XObject unwrap() { return node; }

        public string lookupNamespaceURI(string prefix)
        {
            Debug.WriteLine($"[WrappedNode] lookupNamespaceURI: node={node}, prefix={prefix}");
            //return (prefix == null || prefix.Length == 0) ? "" : node.GetNamespaceOfPrefix(prefix);
            return "";
        }

        public void setAttributeNS(WrappedNode node, string ns, string name, string value)
        {
            var element = (XElement)node.unwrap();
            element.SetAttributeValue(XName.Get(name, ns), value);
        }

        public string value
        {
            get => ((XAttribute)node).Value;
            set => ((XAttribute)node).Value = value;
        }

        private XName getXName(XObject xObject)
        {
            if (xObject is XAttribute attr)
            {
                return attr.Name;
            } else if (xObject is XElement element)
            {
                return element.Name;
            }
            return null;
        }
        public int nodeType  => (int)node.NodeType;

        public string nodeName => getXName(node).LocalName;

        public string localName => getXName(node).LocalName;

        public string namespaceURI => getXName(node).NamespaceName;
    }
}
