///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Bukkit'
// https://github.com/Bukkit/Bukkit
// which is released under GNU General Public License v3.0.
// https://www.gnu.org/licenses/gpl-3.0.en.html
// Copyright 2021 Bukkit Team
///////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using API.Commands;
using API.Config;
using API.Helper.Tasks;
using API.Logger;
using API.Plugins.Interfaces;

namespace API.Plugins
{
    /// <summary>
    /// Represents a plugin
    /// </summary>
    public abstract class Plugin : IPlugin
    {
        private bool _isEnabled;
        private IPluginLoader _loader;
        private IClient _client;
        private FileInfo _file;
        private PluginDescriptionFile _description;
        private DirectoryInfo _dataFolder;
        private PluginClassLoader _classLoader;
        private YamlConfiguration _newConfig;
        private FileInfo _configFile;
        private PluginLoggerWrapper _logger;

        private readonly List<TaskWithDelay> _delayedTasks = new List<TaskWithDelay>();

        private readonly object _delayTasksLock = new object();

        // TODO: unregister commands
        private readonly List<string> _registeredCommands = new List<string>();

        /// <summary>
        /// Parameterless constructor
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        protected Plugin() { }

        /// <summary>
        /// Constructor with parameters
        /// </summary>
        /// <param name="classLoader">class loader</param>
        /// <param name="loader">plugin loader</param>
        /// <param name="description">container for the information in the plugin.yml</param>
        /// <param name="dataFolder">path to the data folder</param>
        /// <param name="file">path to the plugin</param>
        // ReSharper disable once UnusedMember.Global
        protected Plugin(PluginClassLoader classLoader, IPluginLoader loader, PluginDescriptionFile description, DirectoryInfo dataFolder, FileInfo file)
        {
            Init(loader, loader.Handler, description, dataFolder, file, classLoader);
        }

        /// <inheritdoc />
        public DirectoryInfo GetDataFolder()
        {
            return _dataFolder;
        }

        /// <inheritdoc />
        public IPluginLoader GetPluginLoader()
        {
            return _loader;
        }

        /// <inheritdoc />
        public IClient GetClient()
        {
            return _client;
        }

        /// <inheritdoc />
        public bool IsEnabled()
        {
            return _isEnabled;
        }

        /// <summary>
        /// Returns the file which contains this plugin
        /// </summary>
        /// <returns>File containing this plugin</returns>
        protected FileInfo GetFile()
        {
            return _file;
        }

        /// <inheritdoc />
        public PluginDescriptionFile GetDescription()
        {
            return _description;
        }

        /// <inheritdoc />
        public YamlConfiguration GetConfig()
        {
            if (_newConfig == null)
            {
                ReloadConfig();
            }

            return _newConfig;
        }

        /// <inheritdoc />
        public void ReloadConfig()
        {
            _newConfig = YamlConfiguration.LoadConfiguration(_configFile, _client);

            var defConfigStream = GetResource("config.yml");

            if (defConfigStream == null)
                return;

            using var reader = new StreamReader(defConfigStream);

            var defConfig = new YamlConfiguration();

            try
            {
                defConfig.LoadFromString(reader.ReadToEnd());
            }
            catch (Exception e)
            {
                _client.GetLogger().Error("Cannot load configuration from jar\r\n" + e);
            }

            _newConfig.SetDefaults(defConfig);
        }

        /// <inheritdoc />
        public void SaveConfig()
        {
            try
            {
                GetConfig().Save(_configFile);
            }
            catch (IOException ex)
            {
                _logger.Error($"Could not save config to {_configFile}\r\n{ex}");
            }
        }

        /// <inheritdoc />
        public void SaveDefaultConfig()
        {
            if (!_configFile.Exists)
            {
                SaveResource("config.yml", false);
            }
        }

        /// <inheritdoc />
        public void SaveResource(string fileName, bool replace)
        {
            if (fileName == null || fileName.Equals(""))
            {
                throw new ArgumentException("ResourcePath cannot be null or empty");
            }

            var inputStream = GetResource(fileName);

            if (inputStream == null)
            {
                throw new ArgumentException($"The embedded resource '{fileName}' cannot be found in {_file}");
            }

            var outFile = new FileInfo($"{_dataFolder}\\{fileName}");

            if (!_dataFolder.Exists)
            {
                Directory.CreateDirectory(_dataFolder.FullName);
            }

            try
            {
                if (!outFile.Exists || replace)
                {
                    using var outputStream = File.Create(outFile.FullName);

                    inputStream.Seek(0, SeekOrigin.Begin);
                    inputStream.CopyTo(outputStream);
                }
                else
                {
                    _client.GetLogger().Error($"Could not save {outFile.Name} to {outFile.DirectoryName} because it already exists.");
                }
            }
            catch (IOException ex)
            {
                _client.GetLogger().Error($"Could not save {outFile.Name} to {outFile}", ex);
            }
        }

        /// <inheritdoc />
        public Stream GetResource(string filename)
        {
            var assembly = Assembly.GetAssembly(GetType());

            if (assembly == null)
                return null;

            var nameSpace = GetType().Namespace;
            var resourceName = $"{nameSpace}.{filename}";

            return assembly.GetManifestResourceStream(resourceName);
        }

        /// <summary>
        /// Returns the ClassLoader which holds this plugin
        /// </summary>
        /// <returns>ClassLoader holding this plugin</returns>
        public PluginClassLoader GetClassLoader()
        {
            return _classLoader;
        }

        /// <summary>
        /// Sets the enabled state of this plugin
        /// </summary>
        /// <param name="enabled">true if enabled, otherwise false</param>  
        public void SetEnabled(bool enabled)
        {
            if (_isEnabled == enabled) return;

            _isEnabled = enabled;

            if (_isEnabled)
            {
                OnEnable();
            }
            else
            {
                OnDisable();
            }
        }

        public void Init(IPluginLoader loader, IClient server, PluginDescriptionFile description, DirectoryInfo dataFolder, FileInfo file, PluginClassLoader classLoader)
        {
            _loader = loader;
            _client = server;
            _file = file;
            _description = description;
            _dataFolder = dataFolder;
            _classLoader = classLoader;
            _configFile = new FileInfo($@"{dataFolder}\config.yml");
            _logger = new PluginLoggerWrapper(this, server.GetLogger());
        }

        /// <inheritdoc />
        public virtual bool OnCommand(object sender, ICommand command, string label, string[] args)
        {
            return false;
        }

        /// <inheritdoc />
        public virtual void OnLoad() { }

        /// <inheritdoc />
        public virtual void OnDisable() { }

        /// <inheritdoc />
        public virtual void OnEnable() { }

        /// <inheritdoc />
        public virtual ILogger GetLogger()
        {
            return _logger;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return GetName().GetHashCode();
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }
            if (obj == null)
            {
                return false;
            }
            return obj is Plugin plugin && GetName().Equals(plugin.GetName());
        }

        /// <inheritdoc />
        public string GetName()
        {
            return GetDescription().GetName();
        }

        /// <inheritdoc />
        public bool PerformInternalCommand(string command, Dictionary<string, object> localVars = null)
        {
            var temp = "";
            return _client.PerformInternalCommand(command, ref temp, localVars);
        }

        /// <inheritdoc />
        public bool PerformInternalCommand(string command, ref string responseMsg, Dictionary<string, object> localVars = null)
        {
            return _client.PerformInternalCommand(command, ref responseMsg, localVars);
        }

        /// <inheritdoc />
        public bool RegisterCommand(string cmdName, string cmdDesc, string cmdUsage, IClient.CommandRunner callback)
        {
            var result = _client.RegisterCommand(cmdName, cmdDesc, cmdUsage, callback);
            if (result)
                _registeredCommands.Add(cmdName.ToLower());
        
            return result;
        }

        /// <summary>
        /// Will be called every ~100ms.
        /// </summary>
        /// <remarks>
        /// <see cref="ListenerBase.OnUpdate"/> method can be overridden by child class so need an extra update method
        /// </remarks>
        public void UpdateInternal()
        {
            lock (_delayTasksLock)
            {
                if (_delayedTasks.Count <= 0) return;

                var tasksToRemove = new List<int>();

                for (var i = 0; i < _delayedTasks.Count; i++)
                {
                    if (!_delayedTasks[i].Tick()) continue;

                    _delayedTasks[i].Task();
                    tasksToRemove.Add(i);
                }

                if (tasksToRemove.Count <= 0) return;

                tasksToRemove.Sort((a, b) => b.CompareTo(a)); // descending sort

                foreach (var index in tasksToRemove)
                {
                    _delayedTasks.RemoveAt(index);
                }
            }
        }

        /// <summary>
        /// Schedule a task to run on the main thread, and do not wait for completion
        /// </summary>
        /// <param name="task">Task to run</param>
        /// <param name="delayTicks">Run the task after X ticks (1 tick delay = ~100ms). 0 for no delay</param>
        /// <example>
        /// <example>InvokeOnMainThread(methodThatReturnsNothing, 10);</example>
        /// <example>InvokeOnMainThread(() => methodThatReturnsNothing(argument), 10);</example>
        /// <example>InvokeOnMainThread(() => { yourCode(); }, 10);</example>
        /// </example>
        protected void ScheduleOnMainThread(Action task, int delayTicks = 0)
        {
            lock (_delayTasksLock)
            {
                _delayedTasks.Add(new TaskWithDelay(task, delayTicks));
            }
        }

        /// <summary>
        /// Schedule a task to run on the main thread, and do not wait for completion
        /// </summary>
        /// <param name="task">Task to run</param>
        /// <param name="delay">Run the task after the specified delay</param>
        protected void ScheduleOnMainThread(Action task, TimeSpan delay)
        {
            lock (_delayTasksLock)
            {
                _delayedTasks.Add(new TaskWithDelay(task, delay));
            }
        }

        /// <summary>
        /// Invoke a task on the main thread, wait for completion and retrieve return value.
        /// </summary>
        /// <param name="task">Task to run with any type or return value</param>
        /// <returns>Any result returned from task, result type is inferred from the task</returns>
        /// <example>bool result = InvokeOnMainThread(methodThatReturnsAbool);</example>
        /// <example>bool result = InvokeOnMainThread(() => methodThatReturnsAbool(argument));</example>
        /// <example>int result = InvokeOnMainThread(() => { yourCode(); return 42; });</example>
        /// <typeparam name="T">Type of the return value</typeparam>
        protected T InvokeOnMainThread<T>(Func<T> task)
        {
            return _client.InvokeOnMainThread(task);
        }

        /// <summary>
        /// Invoke a task on the main thread and wait for completion
        /// </summary>
        /// <param name="task">Task to run without return value</param>
        /// <example>InvokeOnMainThread(methodThatReturnsNothing);</example>
        /// <example>InvokeOnMainThread(() => methodThatReturnsNothing(argument));</example>
        /// <example>InvokeOnMainThread(() => { yourCode(); });</example>
        protected void InvokeOnMainThread(Action task)
        {
            _client.InvokeOnMainThread(task);
        }
    }
}
