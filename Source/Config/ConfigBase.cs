#region Copyright

//
// Nini Configuration Project.
// Copyright (C) 2006 Brent R. Matzelle.  All rights reserved.
//
// This software is published under the terms of the MIT X11 license, a copy of 
// which has been included with this distribution in the LICENSE.txt file.
// 

#endregion

using System;
using System.Collections;
using System.Globalization;

using Nini.Util;

namespace Nini.Config
{
    #region ConfigKeyEventArgs class

    public delegate void ConfigKeyEventHandler(object sender, ConfigKeyEventArgs e);

    public class ConfigKeyEventArgs : EventArgs
    {
        public ConfigKeyEventArgs(string keyName, string keyValue)
        {
            this.KeyName  = keyName;
            this.KeyValue = keyValue;
        }

        public string KeyName { get; } = null;

        public string KeyValue { get; } = null;
    }

    #endregion

    public class ConfigBase : IConfig
    {
        #region Private variables

        private          string          _configName = null;
        private readonly IFormatProvider _format     = NumberFormatInfo.CurrentInfo;

        #endregion

        #region Protected variables

        #endregion

        #region Constructors

        public ConfigBase(string name, IConfigSource source)
        {
            _configName   = name;
            ConfigSource = source;
        }

        #endregion

        #region Public properties

        public string Name
        {
            get => _configName;
            set
            {
                if (_configName != value)
                {
                    Rename(value);
                }
            }
        }

        public IConfigSource ConfigSource { get; } = null;

        public OrderedList Keys { get; set; } = new OrderedList();

        #endregion

        #region Public methods

        public bool Contains(string key)
        {
            return (Get(key) != null);
        }

        public virtual string Get(string key)
        {
            string result = null;

            if (Keys.Contains(key))
            {
                result = Keys[key].ToString();
            }

            return result;
        }

        public string Get(string key, string defaultValue)
        {
            string result = Get(key);

            return result ?? defaultValue;
        }

        public string GetExpanded(string key)
        {
            return this.ConfigSource.GetExpanded(this, key);
        }

        public string GetString(string key)
        {
            return Get(key);
        }

        public string GetString(string key, string defaultValue)
        {
            return Get(key, defaultValue);
        }

        public int GetInt(string key)
        {
            string text = Get(key);

            if (text == null)
            {
                throw new ArgumentException("Value not found: " + key);
            }

            return Convert.ToInt32(text, _format);
        }

        public int GetInt(string key, int defaultValue)
        {
            string result = Get(key);

            return (result == null)
                       ? defaultValue
                       : Convert.ToInt32(result, _format);
        }

        public uint GetUint(string key)
        {
            string text = Get(key);

            if (text == null)
            {
                throw new ArgumentException("Value not found: " + key);
            }

            return Convert.ToUInt32(text, _format);
        }

        public uint GetUint(string key, uint defaultValue)
        {
            string result = Get(key);

            return (result == null)
                ? defaultValue
                : Convert.ToUInt32(result, _format);
        }

        public long GetLong(string key)
        {
            string text = Get(key);

            if (text == null)
            {
                throw new ArgumentException("Value not found: " + key);
            }

            return Convert.ToInt64(text, _format);
        }

        public long GetLong(string key, long defaultValue)
        {
            string result = Get(key);

            return (result == null)
                       ? defaultValue
                       : Convert.ToInt64(result, _format);
        }

        public bool GetBoolean(string key)
        {
            string text = Get(key);

            if (text == null)
            {
                throw new ArgumentException("Value not found: " + key);
            }

            return Convert.ToBoolean(text);
        }

        public bool GetBoolean(string key, bool defaultValue)
        {
            string text = Get(key);

            return (text == null) ? defaultValue : Convert.ToBoolean(text);
        }

        public float GetFloat(string key)
        {
            string text = Get(key);

            if (text == null)
            {
                throw new ArgumentException("Value not found: " + key);
            }

            return Convert.ToSingle(text, _format);
        }

        public float GetFloat(string key, float defaultValue)
        {
            string result = Get(key);

            return (result == null)
                       ? defaultValue
                       : Convert.ToSingle(result, _format);
        }

        public double GetDouble(string key)
        {
            string text = Get(key);

            if (text == null)
            {
                throw new ArgumentException("Value not found: " + key);
            }

            return Convert.ToDouble(text, _format);
        }

        public double GetDouble(string key, double defaultValue)
        {
            string result = Get(key);

            return (result == null)
                       ? defaultValue
                       : Convert.ToDouble(result, _format);
        }

        public string[] GetKeys()
        {
            string[] result = new string[Keys.Keys.Count];

            Keys.Keys.CopyTo(result, 0);

            return result;
        }

        public string[] GetValues()
        {
            string[] result = new string[Keys.Values.Count];

            Keys.Values.CopyTo(result, 0);

            return result;
        }

        public void Add(string key, string value)
        {
            Keys.Add(key, value);
        }

        public virtual void Set(string key, object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("Value cannot be null");
            }

            if (Get(key) == null)
            {
                this.Add(key, value.ToString());
            }
            else
            {
                Keys[key] = value.ToString();
            }

            if (ConfigSource.AutoSave)
            {
                ConfigSource.Save();
            }

            OnKeySet(new ConfigKeyEventArgs(key, value.ToString()));
        }

        public virtual void Remove(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("Key cannot be null");
            }

            if (Get(key) != null)
            {
                string keyValue = null;

                if (KeySet != null)
                {
                    keyValue = Get(key);
                }

                Keys.Remove(key);

                OnKeyRemoved(new ConfigKeyEventArgs(key, keyValue));
            }
        }

        #endregion

        #region Public events

        public event ConfigKeyEventHandler KeySet;

        public event ConfigKeyEventHandler KeyRemoved;

        #endregion

        #region Protected methods

        protected void OnKeySet(ConfigKeyEventArgs e)
        {
            KeySet?.Invoke(this, e);
        }

        protected void OnKeyRemoved(ConfigKeyEventArgs e)
        {
            KeyRemoved?.Invoke(this, e);
        }

        #endregion

        #region Private methods

        private void Rename(string name)
        {
            this.ConfigSource.Configs.Remove(this);
            _configName = name;
            this.ConfigSource.Configs.Add(this);
        }

        #endregion
    }
}