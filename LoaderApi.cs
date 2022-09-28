using AnomolyAvery.UnturnedPluginLoader.models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rocket.Core.Logging;
using Rocket.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AnomolyAvery.UnturnedPluginLoader
{
    internal class LoaderApi
    {
        internal delegate void PluginsDownloaded(List<Assembly> assemblies);

        internal event PluginsDownloaded onPluginsDownloaded;

        private int[] _pluginIds;
        private string _host;

        private Dictionary<int, byte[]> _rawPlugins;
        private Dictionary<string, byte[]> _rawLibs;

        public LoaderApi(string host, int[] pluginsIds)
        {
            _pluginIds = pluginsIds;
            _host = host;
            _rawPlugins = new Dictionary<int, byte[]>();
            _rawLibs = new Dictionary<string, byte[]>();
        }

        public LoaderPlugin GetPlugin(int pluginId)
        {
            LoaderPlugin plugin = null;

            using(var client = new WebClient())
            {
                try
                {
                    Logger.Log($"Fetching info ({_host}/api/plugins/{pluginId}): #{pluginId}");
                    var data = client.DownloadString($"{_host}/api/plugins/{pluginId}");

                    plugin = JsonConvert.DeserializeObject<LoaderPlugin>(data);
                }
                catch (WebException ex)
                {
                    if (ex.Data.Contains("message"))
                    {
                        var message = ex.Data["message"];
                        Logger.LogError($"Plugin info error: {message}");
                    }
                    Logger.Log($"Failed to get info {pluginId}: {ex.Message}");
                }
            }

            return plugin;
        }

        private LoaderStream GetLoaderStream(string data)
        {
            LoaderStream result = null;
            try
            {
                result = JsonConvert.DeserializeObject<LoaderStream>(data);
            }
            catch(Exception ex)
            {
                Logger.LogException(ex, "Failed to deserialize loader stream");
            }

            return result;
        }

        private void DownloadPlugin(int pluginId, LoaderPlugin plugin)
        {
            using(var client = new WebClient())
            {
                try
                {

                    var uri = new Uri($"{_host}/api/plugins/{pluginId}/stream");
                    Logger.Log($"Download plugin ({_host}/api/plugins/{pluginId}/stream): #{pluginId}");

                    var data = client.DownloadString(uri);

                    var stream = GetLoaderStream(data);

                    if (stream == null)
                    {
                        Logger.LogError("Failed to get stream for plugin");
                        return;
                    }

                    var rawPlugin = Convert.FromBase64String(stream.Base64);

                    _rawPlugins.Add(pluginId, rawPlugin);
                }
                catch (WebException ex)
                {
                    if (ex.Data.Contains("message"))
                    {
                        var message = ex.Data["message"];
                        Logger.LogError($"Plugin download error: {message}");
                    }
                    Logger.Log($"Failed to download {plugin.Name}: {ex.Message}");
                }
            }
        }

        public byte[] DownloadLibrary(string id)
        {
            using (var client = new WebClient())
            {
                try
                {

                    var uri = new Uri($"{_host}/api/libraries/{id}/stream");
                    Logger.Log($"Downloading library ({_host}/api/libraries/{id}/stream): #{id}");

                    var data = client.DownloadString(uri);

                    var stream = GetLoaderStream(data);

                    if (stream == null)
                    {
                        Logger.LogError("Failed to get stream for library");
                        return null;
                    }

                    var rawPlugin = Convert.FromBase64String(stream.Base64);

                    return rawPlugin;
                }
                catch (WebException ex)
                {
                    if (ex.Data.Contains("message"))
                    {
                        var message = ex.Data["message"];
                        Logger.LogError($"Library download error: {message}");
                    }
                    Logger.Log($"Failed to download library {id}: {ex.Message}");
                }
            }

            return null;
        }

        public void Download()
        {
            foreach(var pluginId in _pluginIds)
            {
                var plugin = GetPlugin(pluginId);

                if (plugin == null)
                    continue;

                Logger.Log($"Downloading: {plugin.Name}");

                if (plugin.Libraries.Length > 0)
                {

                   foreach(var library in plugin.Libraries)
                    {
                        var rawLibrary = DownloadLibrary(library.Id);

                        Assembly.Load(rawLibrary);
                    }
                }

                DownloadPlugin(pluginId, plugin);
            }

            var asms = LoadPluginAssemblies();

            Logger.Log("Assembly load count: " + asms.Length);

            onPluginsDownloaded?.Invoke(asms.ToList());
        }

        private Assembly[] LoadPluginAssemblies()
        {
            List<Assembly> _result = new List<Assembly>();
            foreach (var plugin in _rawPlugins)
            {
                Assembly pluginAsm = Assembly.Load(plugin.Value);
                _result.Add(pluginAsm);
            }

            return _result.ToArray();
        }
    }
}
