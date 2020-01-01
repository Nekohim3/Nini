using System.IO;

using Nini.Config;

namespace Nini
{
    public class NiniConfig
    {
        private static IniConfigSource _config;

        public NiniConfig(string name)
        {
            if (!File.Exists(name))
            {
                _config = new IniConfigSource { AutoSave = true };
                _config.Save(name);
            }
            else
                _config = new IniConfigSource(name) { AutoSave = true };
        }

        public void AddConfig(string name, bool useDefault)
        {
            if (ExistConfig(name)) return;
            _config.AddConfig(name);
            if (useDefault)
                _config.AddConfig($"{name}.default");
        }

        public void RemoveConfig(string name)
        {
            IConfig cfg = null;
            IConfig cfgd = null;

            foreach (IConfig q in _config.Configs)
            {
                if (q.Name == name)
                    cfg = q;

                if (q.Name == $"{name}.default")
                    cfgd = q;
            }

            if (cfg != null)
                _config.Configs.Remove(cfg);

            if (cfgd != null)
                _config.Configs.Remove(cfgd);
        }

        public void Set(string config, string key, object value, object defaultValue = null)
        {
            _config.Configs[config].Set(key, value);

            if (defaultValue != null && ExistConfig($"{config}.default"))
                _config.Configs[$"{config}.default"].Set(key, defaultValue);

            if (!ExistKey($"{config}.default", key) && ExistConfig($"{config}.default"))
                _config.Configs[$"{config}.default"].Set(key, value);
        }

        public void SetNew(string config, string key, object value, object defaultValue = null)
        {
            if (!ExistKey(config, key))
                Set(config, key, value, defaultValue);
        }

        public void SetDefault(string config, string key, object defaultValue)
        {
            if (ExistConfig($"{config}.default"))
            {
                _config.Configs[$"{config}.default"].Set(key, defaultValue);
                if (ExistConfig(config) && _config.Configs[config].Get(key) == null)
                    _config.Configs[config].Set(key, defaultValue);
            }
        }

        public void Remove(string config, string key)
        {
            _config.Configs[config].Remove(key);
            if (ExistConfig($"{config}.default"))
                _config.Configs[$"{config}.default"].Remove(key);
        }

        public void RemoveAll(string config)
        {
            foreach (var q in GetKeys(config))
            {
                _config.Configs[config].Remove(q);
            }

            foreach (var q in GetKeys($"{config}.default"))
            {
                _config.Configs[$"{config}.default"].Remove(q);
            }
        }

        public string GetString(string config, string key, bool defaultValue = false) => defaultValue ? _config.Configs[$"{config}.default"].GetString(key) : _config.Configs[config].GetString(key, _config.Configs[$"{config}.default"].GetString(key));
        public int GetInt(string config, string key, bool defaultValue = false) => defaultValue ? _config.Configs[$"{config}.default"].GetInt(key) : _config.Configs[config].GetInt(key, _config.Configs[$"{config}.default"].GetInt(key));
        public uint GetUInt(string config, string key, bool defaultValue = false) => defaultValue ? _config.Configs[$"{config}.default"].GetUint(key) : _config.Configs[config].GetUint(key, _config.Configs[$"{config}.default"].GetUint(key));
        public long GetLong(string config, string key, bool defaultValue = false) => defaultValue ? _config.Configs[$"{config}.default"].GetLong(key) : _config.Configs[config].GetLong(key, _config.Configs[$"{config}.default"].GetLong(key));
        public bool GetBoolean(string config, string key, bool defaultValue = false) => defaultValue ? _config.Configs[$"{config}.default"].GetBoolean(key) : _config.Configs[config].GetBoolean(key, _config.Configs[$"{config}.default"].GetBoolean(key));
        public float GetFloat(string config, string key, bool defaultValue = false) => defaultValue ? _config.Configs[$"{config}.default"].GetFloat(key) : _config.Configs[config].GetFloat(key, _config.Configs[$"{config}.default"].GetFloat(key));
        public double GetDouble(string config, string key, bool defaultValue = false) => defaultValue ? _config.Configs[$"{config}.default"].GetDouble(key) : _config.Configs[config].GetDouble(key, _config.Configs[$"{config}.default"].GetDouble(key));

        public string[] GetKeys(string config) => _config.Configs[config].GetKeys();

        public bool ExistConfig(string name)
        {
            foreach (IConfig q in _config.Configs)
                if (q.Name == name)
                    return true;
            return false;
        }

        public bool ExistKey(string config, string name)
        {
            foreach (var q in GetKeys(config))
                if (q == name)
                    return true;
            return false;
        }
    }
}