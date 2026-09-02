using System.Collections.Generic;
using System.Xml.Linq;

namespace Xslt2Game
{
    public class DomFacade
    {

        public WrappedNode GetParentNode(WrappedNode wrappedNode, object bucket)
        {
            return WrappedNode.wrap(wrappedNode.unwrap().Parent);
        }

        public WrappedNode GetFirstChild(WrappedNode wrappedNode, object bucket)
        {
            return WrappedNode.wrap(((XContainer)wrappedNode.unwrap()).FirstNode);
        }

        public WrappedNode GetNextSibling(WrappedNode wrappedNode, object bucket)
        {
            return WrappedNode.wrap(((XContainer)wrappedNode.unwrap()).NextNode);
        }

        public object GetAllAttributes(WrappedNode wrappedNode, object bucket)
        {
            var element = (XElement)wrappedNode.unwrap();
            var list = new List<object>();

            foreach (XAttribute attr in element.Attributes())
            {
                list.Add(new WrappedNode(attr));
            }

            return list.ToArray(); // JS array
        }

        public string GetAttribute(WrappedNode wrappedNode, string attrName)
        {
            var element = (XElement)wrappedNode.unwrap();
            return element.Attribute(XName.Get(attrName, "")).Value;
        }

        public string GetData(WrappedNode wrappedNode)
        {
            var node = wrappedNode.unwrap();
            return ((XText)node).Value;// node.Value;
        }

        public WrappedNode[] GetChildNodes(WrappedNode wrappedNode, object bucket)
        {
            var container = (XContainer)wrappedNode.unwrap();
            var list = new List<WrappedNode>();

            //foreach (XmlNode childnode in node.ChildNodes)
            //{
            //    list.Add(new WrappedNode(childnode));
            //}
            return list.ToArray(); // JS array
        }        
    }
}
