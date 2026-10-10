using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NotificationsVisualizerLibrary.Parsers;

namespace FireBlade.NotificationEditor.DataModel
{
    public class ParseWarning
    {
        public ParseError Error { get; set; }

        public string Glyph => Error.Type == ParseErrorType.Warning ? "\uE7BA" : "\uEA39";
        public string LineNumber => "Line " + Error.Position.LineNumber.ToString();
        public string Source { get; set; }
    }
}
