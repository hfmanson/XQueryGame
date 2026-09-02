using System;
using System.Xml;

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
		private XmlDocument _document;
		
		public NodesFactory(XmlDocument xmlDocument) { _document = xmlDocument; }

        public WrappedNode createAttributeNS(string namespaceURI, String name)
        {
            return WrappedNode.wrap(_document.CreateAttribute(name));
        }

        public WrappedNode createElementNS(string namespaceURI, String name)
        {
            return WrappedNode.wrap(_document.CreateElement(name));
        }

        public WrappedNode createTextNode(String text)
        {
            return WrappedNode.wrap(_document.CreateTextNode(text));
           
        }
    }
}
