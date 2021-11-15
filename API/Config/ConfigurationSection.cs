///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Bukkit'
// https://github.com/Bukkit/Bukkit
// which is released under GNU General Public License v3.0.
// https://www.gnu.org/licenses/gpl-3.0.en.html
// Copyright 2021 Bukkit Team
///////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using API.Helper;

namespace API.Config
{
    /// <summary>
    /// Represents a section of a <see cref="Configuration"/>
    /// </summary>
    public class ConfigurationSection
    {
        private readonly Dictionary<string, object> _map = new Dictionary<string, object>();

        private readonly Configuration _root;

        private readonly ConfigurationSection _parent;

        private readonly string _path;

        private readonly string _fullPath;

        /// <summary>
        /// Creates an empty ConfigurationSection for use as a root <see cref="Configuration"/> section.
        /// </summary>
        protected ConfigurationSection()
        {
            if (!(this is Configuration))
            {
                throw new Exception("Cannot construct a root ConfigurationSection when not a Configuration");
            }

            _path = "";
            _fullPath = "";
            _parent = null;
            _root = (Configuration)this;
        }

        /// <summary>
        /// Creates an empty ConfigurationSection with the specified parent and path.
        /// </summary>
        /// <param name="parent">Parent section that contains this own section.</param>
        /// <param name="path">Path that you may access this section from via the root</param>
        protected ConfigurationSection(ConfigurationSection parent, string path)
        {
            Validate.NotNull(parent, "Parent cannot be null");
            Validate.NotNull(path, "Path cannot be null");

            _path = path;
            _parent = parent;
            _root = _parent.GetRoot();

            Validate.NotNull(_root, "Path cannot be orphaned");

            _fullPath = CreatePath(_parent, _path);
        }

        /// <summary>
        /// Gets a set containing all keys in this section.
        /// </summary>
        /// <param name="deep"> Whether or not to get a deep list, as opposed to a shallow list.</param>
        /// <returns>Set of keys contained within this ConfigurationSection.</returns>
        public List<string> GetKeys(bool deep)
        {
            var result = new List<string>();
            var root = GetRoot();

            if (root != null && _root.Options().CopyDefaults())
            {
                var defaults = GetDefaultSection();

                if (defaults != null)
                {
                    result.AddRange(defaults.GetKeys(deep));
                }
            }

            MapChildrenKeys(ref result, this, deep);

            return result;
        }

        /// <summary>
        /// Gets a Dictionary containing all keys and their values for this section.
        /// </summary>
        /// <param name="deep">Whether or not to get a deep list, as opposed to a shallow list.</param>
        /// <returns>Dictionary of keys and values of this section.</returns>
        public Dictionary<string, object> GetValues(bool deep)
        {
            var result = new Dictionary<string, object>();
            var root = GetRoot();

            if (root != null && root.Options().CopyDefaults())
            {
                var defaults = GetDefaultSection();

                if (defaults != null)
                {
                    foreach (var (key, value) in defaults.GetValues(deep))
                        result.Add(key, value);
                }
            }

            MapChildrenValues(ref result, this, deep);

            return result;
        }

        /// <summary>
        /// Checks if this <see cref="ConfigurationSection"/> contains the given path.
        /// </summary>
        /// <param name="path">Path to check for existence.</param>
        /// <returns>True if this section contains the requested path, either via default or being set.</returns>
        public bool Contains(string path)
        {
            return Get(path) != null;
        }

        /// <summary>
        /// Checks if this <see cref="ConfigurationSection"/> has a value set for the given path.
        /// </summary>
        /// <param name="path">Path to check for existence.</param>
        /// <returns>True if this section contains the requested path, regardless of having a default.</returns>
        public bool IsSet(string path)
        {
            var root = GetRoot();

            if (root == null)
            {
                return false;
            }

            if (root.Options().CopyDefaults())
            {
                return Contains(path);
            }

            return Get(path, null) != null;
        }

        /// <summary>
        /// Gets the path of this <see cref="ConfigurationSection"/> from its root <see cref="Configuration"/>
        /// </summary>
        /// <returns>Path of this section relative to its root</returns>
        public string GetCurrentPath()
        {
            return _fullPath;
        }

        /// <summary>
        /// Gets the name of this individual <see cref="ConfigurationSection"/>, in the path.
        /// </summary>
        /// <returns>Name of this section</returns>
        public string GetName()
        {
            return _path;
        }

        /// <summary>
        /// Gets the root <see cref="Configuration"/> that contains this <see cref="ConfigurationSection"/>
        /// </summary>
        /// <returns>Root configuration containing this section.</returns>
        public Configuration GetRoot()
        {
            return _root;
        }

        /// <summary>
        /// Gets the parent <see cref="ConfigurationSection"/> that directly contains this <see cref="ConfigurationSection"/>.
        /// </summary>
        /// <returns>Parent section containing this section.</returns>
        public ConfigurationSection GetParent()
        {
            return _parent;
        }

        /// <summary>
        ///  Gets the requested Object by path.
        /// </summary>
        /// <param name="path">Path of the Object to get.</param>
        /// <returns>Requested Object.</returns>
        public object Get(string path)
        {
            return Get(path, GetDefault(path));
        }

        /// <summary>
        ///  Gets the requested Object by path, returning a default value if not found.
        /// </summary>
        /// <param name="path">Path of the Object to get.</param>
        /// <param name="def">The default value to return if the path is not found.</param>
        /// <returns>Requested Object.</returns>
        public object Get(string path, object def)
        {
            Validate.NotNull(path, "Path cannot be null");

            if (path.Length == 0)
            {
                return this;
            }

            var root = GetRoot();

            if (root == null)
            {
                throw new Exception("Cannot access section without a root");
            }

            var separator = root.Options().PathSeparator();
            // i1 is the leading (higher) index
            // i2 is the trailing (lower) index
            int i1 = -1, i2;

            var section = this;

            while ((i1 = path.IndexOf(separator, i2 = i1 + 1)) != -1)
            {
                section = section.GetConfigurationSection(path.Substring(i2, i1));
                if (section == null)
                {
                    return def;
                }
            }

            var key = path[i2..];

            if (section != this) return section.Get(key, def);

            return _map.ContainsKey(key) ? _map[key] : def;
        }

        /// <summary>
        /// Sets the specified path to the given value. <br/>
        /// If value is null, the entry will be removed. Any existing entry will be replaced, regardless of what the new value is.
        /// </summary>
        /// <param name="path">Path of the object to set.</param>
        /// <param name="value">New value to set the path to.</param>
        public void Set(string path, object value)
        {
            Validate.NotEmpty(path, "Cannot set to an empty path");

            var root = GetRoot();

            if (root == null)
            {
                throw new Exception("Cannot use section without a root");
            }

            var separator = root.Options().PathSeparator();

            // i1 is the leading (higher) index
            // i2 is the trailing (lower) index
            int i1 = -1, i2;
            var section = this;
            while ((i1 = path.IndexOf(separator, i2 = i1 + 1)) != -1)
            {
                var node = path.Substring(i2, i1);
                var subSection = section.GetConfigurationSection(node);

                section = subSection ?? section.CreateSection(node);
            }

            var key = path[i2..];

            if (section == this)
            {
                if (value == null)
                {
                    _map.Remove(key);
                }
                else
                {
                    _map.Add(key, value);
                }
            }
            else
            {
                section.Set(key, value);
            }
        }

        /// <summary>
        /// Creates an empty <see cref="ConfigurationSection"/> at the specified path.
        /// </summary>
        /// <param name="path">Path of the object to set.</param>
        /// <returns>Newly created section</returns>
        public ConfigurationSection CreateSection(string path)
        {
            Validate.NotEmpty(path, "Cannot create section at empty path");
            var root = GetRoot();
            if (root == null)
            {
                throw new Exception("Cannot create section without a root");
            }

            var separator = root.Options().PathSeparator();
            // i1 is the leading (higher) index
            // i2 is the trailing (lower) index
            int i1 = -1, i2;
            var section = this;
            while ((i1 = path.IndexOf(separator, i2 = i1 + 1)) != -1)
            {
                var node = path.Substring(i2, i1);
                var subSection = section.GetConfigurationSection(node);

                section = subSection ?? section.CreateSection(node);
            }

            var key = path[i2..];
            if (section != this) return section.CreateSection(key);

            var result = new ConfigurationSection(this, key);
            _map.Add(key, result);
            return result;
        }

        /// <summary>
        /// Creates a <see cref="ConfigurationSection"/> at the specified path, with specified values.
        /// </summary>
        /// <param name="path">Path to create the section at.</param>
        /// <param name="map">The values to used.</param>
        /// <returns>Newly created section</returns>
        public ConfigurationSection CreateSection(string path, Dictionary<string, object> map)
        {
            var section = CreateSection(path);

            foreach (var (key, value) in map)
            {
                if (value is IDictionary)
                {
                    section.CreateSection(key, (Dictionary<string, object>)value);
                }
                else
                {
                    section.Set(key, value);
                }
            }

            return section;
        }

        /// <summary>
        /// Gets the requested String by path.
        /// </summary>
        /// <param name="path">Path of the String to get.</param>
        /// <returns>Requested String.</returns>
        public string GetString(string path)
        {
            var def = GetDefault(path);
            return GetString(path, def?.ToString());
        }

        /// <summary>
        /// Gets the requested String by path, returning a default value if not found.
        /// </summary>
        /// <param name="path">Path of the String to get.</param>
        /// <param name="def">The default value to return if the path is not found or is not a String.</param>
        /// <returns>Requested String.</returns>
        public string GetString(string path, string def)
        {
            var val = Get(path, def);
            return val != null ? val.ToString() : def;
        }

        /// <summary>
        /// Checks if the specified path is a String.
        /// </summary>
        /// <param name="path">Path of the String to check.</param>
        /// <returns>Whether or not the specified path is a String.</returns>
        public bool IsString(string path)
        {
            var val = Get(path);
            return val is string;
        }

        /// <summary>
        /// Gets the requested int by path.
        /// </summary>
        /// <param name="path">Path of the int to get.</param>
        /// <returns>Requested int.</returns>
        public int GetInt(string path)
        {
            var def = GetDefault(path);
            return GetInt(path, int.TryParse(def.ToString(), out var ret) ? ret : 0);
        }

        /// <summary>
        /// Gets the requested int by path, returning a default value if not found.
        /// </summary>
        /// <param name="path">Path of the int to get.</param>
        /// <param name="def"> The default value to return if the path is not found or is not an int.</param>
        /// <returns>Requested int.</returns>
        public int GetInt(string path, int def)
        {
            var val = Get(path, def);
            return int.TryParse(val.ToString(), out var ret) ? ret : def;
        }

        /// <summary>
        /// Checks if the specified path is an int.
        /// </summary>
        /// <param name="path">Path of the int to check.</param>
        /// <returns>Whether or not the specified path is an int.</returns>
        public bool IsInt(string path)
        {
            var val = Get(path);
            return val is int;
        }

        /// <summary>
        /// Gets the requested boolean by path.
        /// </summary>
        /// <param name="path">Path of the boolean to get.</param>
        /// <returns>Requested boolean.</returns>
        public bool GetBool(string path)
        {
            var def = GetDefault(path);
            return GetBool(path, def is bool b && b);
        }

        /// <summary>
        /// Gets the requested boolean by path, returning a default value if not found.
        /// </summary>
        /// <param name="path">Path of the boolean to get.</param>
        /// <param name="def">The default value to return if the path is not found or is not a boolean.</param>
        /// <returns>Requested boolean.</returns>
        public bool GetBool(string path, bool def)
        {
            var val = Get(path, def);
            return val is bool b ? b : def;
        }

        /// <summary>
        /// Checks if the specified path is a boolean.
        /// </summary>
        /// <param name="path">Path of the boolean to check.</param>
        /// <returns>Whether or not the specified path is a boolean.</returns>
        public bool IsBool(string path)
        {
            var val = Get(path);
            return val is bool;
        }

        /// <summary>
        /// Gets the requested double by path.
        /// </summary>
        /// <param name="path">Path of the double to get.</param>
        /// <returns>Requested double.</returns>
        public double GetDouble(string path)
        {
            var def = GetDefault(path);
            return GetDouble(path, def is double ? Convert.ToDouble(def) : 0);
        }

        /// <summary>
        /// Gets the requested double by path, returning a default value if not found.
        /// </summary>
        /// <param name="path">Path of the double to get.</param>
        /// <param name="def">The default value to return if the path is not found or is not a double.</param>
        /// <returns>Requested double.</returns>
        public double GetDouble(string path, double def)
        {
            var val = Get(path, def);
            return val is double ? Convert.ToDouble(val) : def;
        }

        /// <summary>
        /// Checks if the specified path is a double.
        /// </summary>
        /// <param name="path">Path of the double to check.</param>
        /// <returns>Whether or not the specified path is a double.</returns>
        public bool IsDouble(string path)
        {
            var val = Get(path);
            return val is double;
        }

        /// <summary>
        /// Gets the requested long by path.
        /// </summary>
        /// <param name="path">Path of the long to get.</param>
        /// <returns>Requested long.</returns>
        public long GetLong(string path)
        {
            var def = GetDefault(path);
            return GetLong(path, def is long ? Convert.ToInt64(def) : 0);
        }

        /// <summary>
        /// Gets the requested long by path, returning a default value if not found.
        /// </summary>
        /// <param name="path">Path of the long to get.</param>
        /// <param name="def">The default value to return if the path is not found or is not a long.</param>
        /// <returns>Requested long.</returns>
        public long GetLong(string path, long def)
        {
            var val = Get(path, def);
            return val is long ? Convert.ToInt64(val) : def;
        }

        /// <summary>
        /// Checks if the specified path is a long.
        /// </summary>
        /// <param name="path">Path of the long to check.</param>
        /// <returns>Whether or not the specified path is a long.</returns>
        public bool IsLong(string path)
        {
            var val = Get(path);
            return val is long;
        }

        /// <summary>
        /// Gets the requested List by path.
        /// </summary>
        /// <param name="path">Path of the List to get.</param>
        /// <returns>Requested List.</returns>
        public IList GetList(string path)
        {
            var def = GetDefault(path);
            return GetList(path, def is IList list ? list : null);
        }

        /// <summary>
        /// Gets the requested List by path, returning a default value if not found.
        /// </summary>
        /// <param name="path">Path of the List to get.</param>
        /// <param name="def">The default value to return if the path is not found or is not a List.</param>
        /// <returns>Requested List.</returns>
        public IList GetList(string path, IList def)
        {
            var val = Get(path, def);
            return (IList)(val is IList ? val : def);
        }

        /// <summary>
        /// Checks if the specified path is a List.
        /// </summary>
        /// <param name="path">Path of the List to check.</param>
        /// <returns>Whether or not the specified path is a List.</returns>
        public bool IsList(string path)
        {
            var val = Get(path);
            return val is IList;
        }

        /// <summary>
        /// Gets the requested List of String by path.
        /// </summary>
        /// <param name="path">Path of the List to get.</param>
        /// <returns>List of String.</returns>
        public List<string> GetStringList(string path)
        {
            var list = GetList(path);

            if (list == null)
            {
                return new List<string>(0);
            }

            var result = new List<string>();

            foreach (var obj in list)
            {
                if (obj is string || IsPrimitiveWrapper(obj))
                {
                    result.Add(obj.ToString());
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the requested List of Integer by path.
        /// </summary>
        /// <param name="path">Path of the List to get.</param>
        /// <returns>List of Integer.</returns>
        public List<int> GetIntList(string path)
        {
            var list = GetList(path);

            if (list == null)
            {
                return new List<int>(0);
            }

            var result = new List<int>();

            foreach (var obj in list)
            {
                switch (obj)
                {
                    case int i:
                        result.Add(i);
                        break;

                    case string s:
                        try
                        {
                            result.Add(int.Parse(s));
                        }
                        catch (Exception)
                        {
                            // ignored
                        }

                        break;

                    case char c:
                        try
                        {
                            result.Add(c);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }

                        break;

                    default:

                        if (IsPrimitiveWrapper(obj))
                        {
                            result.Add(Convert.ToInt32(obj));
                        }

                        break;
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the requested List of Boolean by path.
        /// </summary>
        /// <param name="path">Path of the List to get.</param>
        /// <returns>List of Boolean.</returns>
        public List<bool> GetBooleanList(string path)
        {
            var list = GetList(path);

            if (list == null)
            {
                return new List<bool>(0);
            }

            var result = new List<bool>();

            foreach (var obj in list)
            {
                switch (obj)
                {
                    case bool b:
                        result.Add(b);
                        break;

                    case string _ when bool.TrueString.Equals(obj):
                        result.Add(true);
                        break;

                    case string _:

                        if (bool.FalseString.Equals(obj))
                        {
                            result.Add(false);
                        }

                        break;
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the requested List of Double by path.
        /// </summary>
        /// <param name="path">Path of the List to get.</param>
        /// <returns>List of Double.</returns>
        public List<double> GetDoubleList(string path)
        {
            var list = GetList(path);

            if (list == null)
            {
                return new List<double>(0);
            }

            var result = new List<double>();

            foreach (var obj in list)
            {
                switch (obj)
                {
                    case double d:
                        result.Add(d);
                        break;

                    case string s:
                        try
                        {
                            result.Add(double.Parse(s));
                        }
                        catch (Exception)
                        {
                            // ignored
                        }

                        break;

                    case char c:
                        try
                        {
                            result.Add(c);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }

                        break;

                    default:
                        if (IsPrimitiveWrapper(obj))
                        {
                            result.Add(Convert.ToDouble(obj));
                        }

                        break;
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the requested List of Float by path.
        /// </summary>
        /// <param name="path">Path of the List to get.</param>
        /// <returns>Requested List of Float.</returns>
        public List<float> GetFloatList(string path)
        {
            var list = GetList(path);

            if (list == null)
            {
                return new List<float>(0);
            }

            var result = new List<float>();

            foreach (var obj in list)
            {
                switch (obj)
                {
                    case float f:
                        result.Add(f);
                        break;

                    case string s:
                        try
                        {
                            result.Add(float.Parse(s));
                        }
                        catch (Exception)
                        {
                            // ignored
                        }

                        break;

                    case char c:
                        try
                        {
                            result.Add(c);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }

                        break;

                    default:

                        if (IsPrimitiveWrapper(obj))
                        {
                            result.Add(Convert.ToSingle(obj));
                        }

                        break;

                }
            }

            return result;
        }

        /// <summary>
        /// Gets the requested List of Long by path.
        /// </summary>
        /// <param name="path">Path of the List to get.</param>
        /// <returns>Requested List of Long.</returns>
        public List<long> GetLongList(string path)
        {
            var list = GetList(path);

            if (list == null)
            {
                return new List<long>(0);
            }

            var result = new List<long>();

            foreach (var obj in list)
            {
                switch (obj)
                {
                    case long l:
                        result.Add(l);
                        break;

                    case string s:
                        try
                        {
                            result.Add(long.Parse(s));
                        }
                        catch (Exception)
                        {
                            // ignored
                        }

                        break;

                    case char c:
                        try
                        {
                            result.Add(c);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }

                        break;

                    default:
                        if (IsPrimitiveWrapper(obj))
                        {
                            result.Add(Convert.ToInt64(obj));
                        }

                        break;

                }
            }

            return result;
        }

        /// <summary>
        /// Gets the requested List of Byte by path.
        /// </summary>
        /// <param name="path">Path of the List to get.</param>
        /// <returns>Requested List of Byte.</returns>
        public List<byte> GetByteList(string path)
        {
            var list = GetList(path);

            if (list == null)
            {
                return new List<byte>(0);
            }

            var result = new List<byte>();

            foreach (var obj in list)
            {
                switch (obj)
                {
                    case byte b:
                        result.Add(b);
                        break;
                    case string s:
                        try
                        {
                            result.Add(byte.Parse(s));
                        }
                        catch (Exception)
                        {
                            // ignored
                        }

                        break;
                    case char c:
                        try
                        {
                            result.Add((byte)c);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }

                        break;
                    default:
                        if (IsPrimitiveWrapper(obj))
                        {
                            result.Add(Convert.ToByte(obj));
                        }

                        break;
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the requested List of Character by path.
        /// </summary>
        /// <param name="path">Path of the List to get.</param>
        /// <returns>List of Character.</returns>
        public List<char> GetCharList(string path)
        {
            var list = GetList(path);

            if (list == null)
            {
                return new List<char>(0);
            }

            var result = new List<char>();

            foreach (var obj in list)
            {
                switch (obj)
                {
                    case char c:
                        result.Add(c);
                        break;
                    case string s:
                        var str = s;

                        if (str.Length == 1)
                        {
                            result.Add(str[0]);
                        }

                        break;

                    default:
                        if (IsPrimitiveWrapper(obj))
                        {
                            result.Add(Convert.ToChar(obj));
                        }

                        break;
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the requested List of Short by path.
        /// </summary>
        /// <param name="path">Path of the List to get.</param>
        /// <returns>Requested List of Short.</returns>
        public List<short> GetShortList(string path)
        {
            var list = GetList(path);

            if (list == null)
            {
                return new List<short>(0);
            }

            var result = new List<short>();

            foreach (var obj in list)
            {
                switch (obj)
                {
                    case short s:
                        result.Add(s);
                        break;
                    case string s:
                        try
                        {
                            result.Add(short.Parse(s));
                        }
                        catch (Exception)
                        {
                            // ignored
                        }

                        break;
                    case char c:
                        try
                        {
                            result.Add((short)c);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }

                        break;
                    default:
                        {
                            if (IsPrimitiveWrapper(obj))
                            {
                                result.Add(Convert.ToInt16(obj));
                            }

                            break;
                        }
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the requested List of Dictionarys by path.
        /// </summary>
        /// <param name="path">Path of the List to get.</param>
        /// <returns>Requested List of Dictionarys.</returns>
        public List<Dictionary<string, object>> GetMapList(string path)
        {
            var list = GetList(path);
            var result = new List<Dictionary<string, object>>();

            if (list == null)
            {
                return result;
            }

            foreach (var obj in list)
            {
                if (obj is IDictionary)
                {
                    result.Add((Dictionary<string, object>)obj);
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the requested ConfigurationSection by path.
        /// </summary>
        /// <param name="path">Path of the ConfigurationSection to get.</param>
        /// <returns>Requested ConfigurationSection.</returns>
        public ConfigurationSection GetConfigurationSection(string path)
        {
            var val = Get(path, null);
            if (val != null)
            {
                return val is ConfigurationSection section ? section : null;
            }

            val = Get(path, GetDefault(path));
            return val is ConfigurationSection ? CreateSection(path) : null;
        }

        /// <summary>
        /// Checks if the specified path is a ConfigurationSection.
        /// </summary>
        /// <param name="path">Path of the ConfigurationSection to check.</param>
        /// <returns>Whether or not the specified path is a ConfigurationSection.</returns>
        public bool IsConfigurationSection(string path)
        {
            var val = Get(path);
            return val is ConfigurationSection;
        }

        /// <summary>
        /// Gets the equivalent <see cref="ConfigurationSection"/> from the default <see cref="Configuration"/> defined in <see cref="GetRoot()"/>.
        /// </summary>
        /// <returns>Equivalent section in root configuration</returns>
        public ConfigurationSection GetDefaultSection()
        {
            var root = GetRoot();

            var defaults = root?.GetDefaults();

            if (defaults == null) return null;

            return defaults.IsConfigurationSection(GetCurrentPath()) ? defaults.GetConfigurationSection(GetCurrentPath()) : null;
        }

        /// <summary>
        /// Sets the default value in the root at the given path as provided.
        /// </summary>
        /// <param name="path">Path of the value to set.</param>
        /// <param name="value">Value to set the default to.</param>
        public void AddDefault(string path, object value)
        {
            Validate.NotNull(path, "Path cannot be null");

            var root = GetRoot();

            if (root == null)
            {
                throw new Exception("Cannot add default without root");
            }

            if (root == this)
            {
                throw new Exception("Unsupported addDefault(String, Object) implementation");
            }

            root.AddDefault(CreatePath(this, path), value);
        }

        /// <summary>
        /// Checks if the input from one of the eight primitive data types
        /// </summary>
        /// <param name="input">object to check</param>
        /// <returns>true if primitive type</returns>
        protected bool IsPrimitiveWrapper(object input)
        {
            return input is int || input is bool ||
                input is char || input is byte ||
                input is short || input is double ||
                input is long || input is float;
        }

        /// <summary>
        /// Gets the default value for a given path
        /// </summary>
        /// <param name="path">Path of the default value</param>
        /// <returns>Default value</returns>
        protected object GetDefault(string path)
        {
            Validate.NotNull(path, "Path cannot be null");

            var root = GetRoot();
            var defaults = root?.GetDefaults();
            return defaults?.Get(CreatePath(this, path));
        }

        /// <summary>
        /// Flat dictionary output of section keys
        /// </summary>
        protected void MapChildrenKeys(ref List<string> output, ConfigurationSection section, bool deep)
        {
            var sec = section;

            foreach (var (key, value) in sec._map)
            {
                output.Add(CreatePath(section, key, this));

                if (!deep || !(value is ConfigurationSection configurationSection)) continue;

                var subsection = configurationSection;
                MapChildrenKeys(ref output, subsection, deep);
            }
        }

        /// <summary>
        /// Flat dictionary output of section values
        /// </summary>
        protected void MapChildrenValues(ref Dictionary<string, object> output, ConfigurationSection section, bool deep)
        {
            var sec = section;

            foreach (var (key, value) in sec._map)
            {
                output.Add(CreatePath(section, key, this), value);

                if ((value is ConfigurationSection configurationSection))
                {
                    if (deep)
                    {
                        MapChildrenValues(ref output, configurationSection, deep);
                    }
                }
            }
        }

        /// <summary>
        /// Creates a full path to the given <see cref="ConfigurationSection"/> from its root <see cref="Configuration"/>.
        /// </summary>
        /// <param name="section">Section to create a path for.</param>
        /// <param name="key">Name of the specified section.</param>
        /// <returns>Full path of the section from its root.</returns>
        public static string CreatePath(ConfigurationSection section, string key)
        {
            return CreatePath(section, key, section?.GetRoot());
        }

        /// <summary>
        /// Creates a relative path to the given {@link ConfigurationSection} from the given relative section.
        /// </summary>
        /// <param name="section">Section to create a path for.</param>
        /// <param name="key">Name of the specified section.</param>
        /// <param name="relativeTo">Section to create the path relative to.</param>
        /// <returns>Full path of the section from its root.</returns>
        public static string CreatePath(ConfigurationSection section, string key, ConfigurationSection relativeTo)
        {
            Validate.NotNull(section, "Cannot create path without a section");
            var root = section.GetRoot();

            if (root == null)
            {
                throw new Exception("Cannot create path without a root");
            }

            var separator = root.Options().PathSeparator();

            var builder = new StringBuilder();

            if (section != null)
            {
                for (var parent = section; parent != null && parent != relativeTo; parent = parent.GetParent())
                {
                    if (builder.Length > 0)
                    {
                        builder.Insert(0, separator);
                    }

                    builder.Insert(0, parent.GetName());
                }
            }

            if (string.IsNullOrEmpty(key))
                return builder.ToString();

            if (builder.Length > 0)
            {
                builder.Append(separator);
            }

            builder.Append(key);

            return builder.ToString();
        }

        /// <inheritdoc cref="object.ToString"/>
        public override string ToString()
        {
            var root = GetRoot();

            return new StringBuilder()
                .Append(GetType().Name)
                .Append("[path='")
                .Append(GetCurrentPath())
                .Append("', root='")
                .Append(root?.GetType().Name)
                .Append("']")
                .ToString();
        }
    }
}