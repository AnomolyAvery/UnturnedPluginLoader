using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnomolyAvery.UnturnedPluginLoader.models
{
    public class LoaderPlugin
    { 

        public string Name { get; set; }

        public LoaderLibrary[] Libraries { get; set; }
    }
}
