using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MagicAssistant
{
    [Serializable]
    public class DataObject
    {
        public int ID;
        public string Name;
        public MASettings Settings = new MASettings();
        public MAMatch Match = new MAMatch();

        public string SerializeObject()
        {
            XmlSerializer xmlSerializer = new XmlSerializer(this.GetType());

            using (StringWriter textWriter = new StringWriter())
            {
                xmlSerializer.Serialize(textWriter, this);
                return textWriter.ToString();
            }
        }
    }
}
