using System;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Gui.CompletionWindow;

namespace Kaxaml.CodeCompletion
{
    /// <summary>
    /// Holds the text for  namespace, child element or attribute 
    /// autocomplete (intellisense).
    /// </summary>
    public class XmlCompletionData : ICompletionData, IComparable
    {
        string text;
        DataType dataType = DataType.XmlElement;
        string description = String.Empty;

        /// <summary>
        /// The type of text held in this object.
        /// </summary>
        public enum DataType
        {
            XmlElement = 1,
            XmlAttribute = 2,
            NamespaceUri = 3,
            XmlAttributeValue = 4,
            Snippet = 5,
            Comment = 6,
            Other = 7,
        }

        public XmlCompletionData(string text)
            : this(text, String.Empty, DataType.XmlElement)
        {
        }

        public XmlCompletionData(string text, string description)
            : this(text, description, DataType.XmlElement)
        {
        }

        public XmlCompletionData(string text, DataType dataType)
            : this(text, String.Empty, dataType)
        {
        }

        public XmlCompletionData(string text, string description, DataType dataType)
        {
            this.text = text;
            this.description = description;
            this.dataType = dataType;
        }

        public int ImageIndex
        {
            get
            {
                return 0;
            }
        }

        public string Text
        {
            get
            {
                return text;
            }
            set
            {
                text = value;
            }
        }

        /// <summary>
        /// Returns the xml item's documentation as retrieved from
        /// the xs:annotation/xs:documentation element.
        /// </summary>
        public string Description
        {
            get
            {
                return description;
            }
        }

        public double Priority
        {
            get
            {
                return 0;
            }
        }

        public DataType CompletionDataType
        {
            get
            {
                return dataType;
            }
        }

        public bool InsertAction(TextArea textArea, char ch)
        {
            if ((dataType == DataType.XmlElement) || (dataType == DataType.XmlAttributeValue))
            {
                textArea.InsertString(text);
            }
            else if (dataType == DataType.NamespaceUri)
            {
                textArea.InsertString(String.Concat("\"", text, "\""));
            }
            else
            {
                // Insert an attribute.
                Caret caret = textArea.Caret;
                textArea.InsertString(String.Concat(text, "=\"\""));

                // Move caret into the middle of the attribute quotes.
                caret.Position = textArea.Document.OffsetToPosition(caret.Offset - 1);
            }
            return false;
        }

        public int CompareTo(object obj)
        {
            if ((obj == null) || !(obj is XmlCompletionData))
            {
                return -1;
            }
            return text.CompareTo(((XmlCompletionData)obj).text);
        }
    }
}
