using System.Collections.Generic;
using System.Xml;

namespace Xslt2Game
{
    public class DomFacade
    {

        public WrappedNode GetParentNode(WrappedNode wrappedNode, object bucket)
        {
            return WrappedNode.wrap(wrappedNode.unwrap().ParentNode);
        }

        public WrappedNode GetFirstChild(WrappedNode wrappedNode, object bucket)
        {
            return WrappedNode.wrap(wrappedNode.unwrap().FirstChild);
        }

        public WrappedNode GetNextSibling(WrappedNode wrappedNode, object bucket)
        {
            return WrappedNode.wrap(wrappedNode.unwrap().NextSibling);
        }

        public object GetAllAttributes(WrappedNode wrappedNode, object bucket)
        {
            var node = wrappedNode.unwrap();
            var list = new List<object>();

            if (node.Attributes != null)
            {
                foreach (XmlAttribute attr in node.Attributes)
                {
                    list.Add(new WrappedNode(attr));
                }
            }

            return list.ToArray(); // JS array
        }

        public string GetAttribute(WrappedNode wrappedNode, string attrName)
        {
            var element = (XmlElement)wrappedNode.unwrap();
            return element.GetAttribute(attrName);
        }

        public string GetData(WrappedNode wrappedNode)
        {
            var node = wrappedNode.unwrap();
            return node.Value;
        }

        public WrappedNode[] GetChildNodes(WrappedNode wrappedNode, object bucket)
        {
            var node = wrappedNode.unwrap();
            var list = new List<WrappedNode>();

            foreach (XmlNode childnode in node.ChildNodes)
            {
                list.Add(new WrappedNode(childnode));
            }
            return list.ToArray(); // JS array
        }        
    }
}
