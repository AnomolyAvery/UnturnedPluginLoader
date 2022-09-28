using Rocket.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AnomolyAvery.UnturnedPluginLoader
{
    public class UnturnedPluginLoaderConfiguration : IRocketPluginConfiguration
    {
        public string Host { get;  set; }
        
        [XmlArray("Plugins")]
        [XmlArrayItem("PluginId")]
        public List<int> PluginIds { get; set; }

        public void LoadDefaults()
        {
            Host = "http://localhost:4000";
            PluginIds = new List<int>()
            {
                1
            };
        }
    }
}
