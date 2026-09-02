using System.Diagnostics;
using System.Xml;

namespace Xslt2Game
{
    public class WrappedNode
    {
        private XmlNode node;

        public WrappedNode(XmlNode node) { this.node = node; }

        public static WrappedNode wrap(XmlNode node)
        {
            return node == null ? null : new WrappedNode(node);
        }

        public XmlNode unwrap() { return node; }

        public string lookupNamespaceURI(string prefix)
        {
            Debug.WriteLine($"[WrappedNode] lookupNamespaceURI: node={node}, prefix={prefix}");
            return (prefix == null || prefix.Length == 0) ? "" : node.GetNamespaceOfPrefix(prefix);
        }

        public void setAttributeNS(WrappedNode node, string ns, string name, string value)
        {
            var element = (XmlElement)node.unwrap();
            element.SetAttribute(name, value);
        }

        public string value
        {
            get => node.Value;
            set => node.Value = value;
        }

        public int nodeType  => (int)node.NodeType;
        
        public string nodeName => node.Name;

        public string localName => node.LocalName;

        public string namespaceURI => node.NamespaceURI;
    }
}
