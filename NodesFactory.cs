using System;
using System.Xml.Linq;

/*
 	createAttributeNS(namespaceURI: string, name: string): Attr;

	createCDATASection(contents: string): CDATASection;

	createComment(contents: string): Comment;

	createElementNS(namespaceURI: string, name: string): Element;

	createProcessingInstruction(target: string, data: string): ProcessingInstruction;

	createTextNode(contents: string): Text;
 */

namespace Xslt2Game
{
    public class NodesFactory
    {
        public WrappedNode createAttributeNS(string namespaceURI, String name)
        {
            return WrappedNode.wrap(new XAttribute(XName.Get(name, namespaceURI), ""));
        }

        public WrappedNode createElementNS(string namespaceURI, String name)
        {
            return WrappedNode.wrap(new XElement(XName.Get(name, namespaceURI)));
        }

        public WrappedNode createTextNode(String text)
        {
            return WrappedNode.wrap(new XText(text));           
        }
    }
}
