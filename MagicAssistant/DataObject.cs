using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp.Serialization.Json;

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
            JsonSerializer serializer = new JsonSerializer();
            string jsonString = serializer.Serialize(this);
            return jsonString;
        }
    }
}
