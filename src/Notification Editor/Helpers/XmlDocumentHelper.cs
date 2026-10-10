using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;

namespace FireBlade.NotificationEditor.Helpers
{
    public static class XmlDocumentHelper
    {
        // note: XmlDocument implements IXmlNode
        public static XmlElement CreateOrSelectSingleElement(this IXmlNode node, string xpath)
        {
            if (node.SelectSingleNode(xpath) is XmlElement el)
                return el;

            var split = xpath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            XmlElement last = node as XmlElement;

            if (last == null)
            {
                last = node.OwnerDocument.DocumentElement;
                split = split.Skip(1).ToArray();
            }

            foreach (var name in split)
            {
                var found = last.ChildNodes.FirstOrDefault(x => x.NodeName.Equals(name, StringComparison.OrdinalIgnoreCase)) as XmlElement;

                if (found == null)
                {
                    var element = node.OwnerDocument.CreateElement(name);
                    last.AppendChild(element);

                    last = element;
                }
                else
                    last = found;
            }

            return last;
        }

        public static void InsertAt(this IXmlNode node, IXmlNode newNode, int index)
        {
            var elements = node.ChildNodes.OfType<XmlElement>().ToList();

            if (index >= elements.Count)
                node.AppendChild(newNode);
            else
                node.InsertBefore(newNode, elements.ElementAt(index));
        }
    }
}
