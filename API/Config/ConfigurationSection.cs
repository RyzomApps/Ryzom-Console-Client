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
    public class ConfigurationSection
    {
        private readonly Dictionary<string, object> _map = new Dictionary<string, object>();

        private readonly Configuration _root;

        private readonly ConfigurationSection _parent;

        private readonly string _path;

        private readonly string _fullPath;

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

        protected ConfigurationSection(ConfigurationSection parent, string path)
        {
            Validate.NotNull(_parent, "Parent cannot be null");
            Validate.NotNull(_path, "Path cannot be null");
            _path = path;
            _parent = parent;
            _root = _parent.GetRoot();
            Validate.NotNull(_root, "Path cannot be orphaned");
            _fullPath = CreatePath(_parent, _path);
        }

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

            MapChildrenKeys(result, this, deep);
            return result;
        }

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

            MapChildrenValues(result, this, deep);

            return result;
        }

        public bool Contains(string path)
        {
            return Get(path) != null;
        }

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

        public string GetCurrentPath()
        {
            return _fullPath;
        }

        public string GetName()
        {
            return _path;
        }

        public Configuration GetRoot()
        {
            return _root;
        }

        public ConfigurationSection GetParent()
        {
            return _parent;
        }

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

        public ConfigurationSection GetDefaultSection()
        {
            var root = GetRoot();

            var defaults = root?.GetDefaults();

            if (defaults == null) return null;

            return defaults.IsConfigurationSection(GetCurrentPath()) ? defaults.GetConfigurationSection(GetCurrentPath()) : null;
        }

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

        public object Get(string path)
        {
            return Get(path, GetDefault(path));
        }

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

        public string GetString(string path)
        {
            var def = GetDefault(path);
            return GetString(path, def?.ToString());
        }

        public string GetString(string path, string def)
        {
            var val = Get(path, def);
            return val != null ? val.ToString() : def;
        }

        public bool IsString(string path)
        {
            var val = Get(path);
            return val is string;
        }

        public int GetInt(string path)
        {
            var def = GetDefault(path);
            return GetInt(path, def is int ? Convert.ToInt32(def) : 0);
        }

        public int GetInt(string path, int def)
        {
            var val = Get(path, def);
            return val is int ? Convert.ToInt32(val) : def;
        }

        public bool IsInt(string path)
        {
            var val = Get(path);
            return val is int;
        }

        public bool GetBool(string path)
        {
            var def = GetDefault(path);
            return GetBool(path, def is bool b && b);
        }

        public bool GetBool(string path, bool def)
        {
            var val = Get(path, def);
            return val is bool b ? b : def;
        }

        public bool IsBool(string path)
        {
            var val = Get(path);
            return val is bool;
        }

        public double GetDouble(string path)
        {
            var def = GetDefault(path);
            return GetDouble(path, def is double ? Convert.ToDouble(def) : 0);
        }

        public double GetDouble(string path, double def)
        {
            var val = Get(path, def);
            return val is double ? Convert.ToDouble(val) : def;
        }

        public bool IsDouble(string path)
        {
            var val = Get(path);
            return val is double;
        }

        public long GetLong(string path)
        {
            var def = GetDefault(path);
            return GetLong(path, def is long ? Convert.ToInt64(def) : 0);
        }

        public long GetLong(string path, long def)
        {
            var val = Get(path, def);
            return val is long ? Convert.ToInt64(val) : def;
        }

        public bool IsLong(string path)
        {
            var val = Get(path);
            return val is long;
        }

        public IList GetList(string path)
        {
            var def = GetDefault(path);
            return GetList(path, def is IList list ? list : null);
        }

        public IList GetList(string path, IList def)
        {
            var val = Get(path, def);
            return (IList)(val is IList ? val : def);
        }

        public bool IsList(string path)
        {
            var val = Get(path);
            return val is IList;
        }

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
                if (obj is string || (IsPrimitiveWrapper(obj)))
                {
                    result.Add(obj.ToString());
                }
            }

            return result;
        }

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

        public bool IsConfigurationSection(string path)
        {
            var val = Get(path);
            return val is ConfigurationSection;
        }

        protected bool IsPrimitiveWrapper(object input)
        {
            return input is int || input is bool ||
                input is char || input is byte ||
                input is short || input is double ||
                input is long || input is float;
        }

        protected object GetDefault(string path)
        {
            Validate.NotNull(path, "Path cannot be null");

            var root = GetRoot();
            var defaults = root?.GetDefaults();
            return defaults?.Get(CreatePath(this, path));
        }

        protected void MapChildrenKeys(List<string> output, ConfigurationSection section, bool deep)
        {
            var sec = section;

            foreach (var (key, value) in sec._map)
            {
                output.Add(CreatePath(section, key, this));

                if (!deep || !(value is ConfigurationSection configurationSection)) continue;

                var subsection = configurationSection;
                MapChildrenKeys(output, subsection, deep);
            }
        }

        protected void MapChildrenValues(Dictionary<string, object> output, ConfigurationSection section, bool deep)
        {
            var sec = section;

            foreach (var (key, value) in sec._map)
            {
                output.Add(CreatePath(section, key, this), value);

                if ((value is ConfigurationSection configurationSection))
                {
                    if (deep)
                    {
                        MapChildrenValues(output, configurationSection, deep);
                    }
                }
            }
        }

        public static string CreatePath(ConfigurationSection section, string key)
        {
            return CreatePath(section, key, section?.GetRoot());
        }

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