using Rocket.Core.Plugins;
using Rocket.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace AnomolyAvery.UnturnedPluginLoader
{
    public class UnturnedPluginLoader: RocketPlugin<UnturnedPluginLoaderConfiguration>
    {
        public static UnturnedPluginLoader Instance { get; private set; }

        private List<GameObject> _loaderObjs;

        internal LoaderApi _api;
        protected override void Load()
        {
            base.Load();
            Instance = this;
            _loaderObjs = new List<GameObject>();
            _api = new LoaderApi(Configuration.Instance.Host, Configuration.Instance.PluginIds.ToArray());

            _api.onPluginsDownloaded += _api_onPluginsDownloaded;

            _api.Download();
        }

        private void _api_onPluginsDownloaded(List<System.Reflection.Assembly> assemblies)
        {

            List<Type> pluginImplemenations = RocketHelper.GetTypesFromInterface(assemblies, "IRocketPlugin");
            foreach (Type pluginType in pluginImplemenations)
            {
                GameObject plugin = new GameObject(pluginType.Name, pluginType);
                DontDestroyOnLoad(plugin);
                _loaderObjs.Add(plugin);
            }
        }

        protected override void Unload()
        {
            base.Unload();
            Instance = null;
        }


    }
}
