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

namespace Nini.Config
{
    #region ConfigEventHandler class

    public delegate void ConfigEventHandler(object sender, ConfigEventArgs e);

    public class ConfigEventArgs : EventArgs
    {
        IConfig config = null;

        public ConfigEventArgs(IConfig config)
        {
            this.config = config;
        }

        public IConfig Config => config;
    }

    #endregion

    public class ConfigCollection : ICollection, IEnumerable, IList
    {
        #region Private variables

        ArrayList        configList = new ArrayList();
        ConfigSourceBase owner      = null;

        #endregion

        #region Constructors

        public ConfigCollection(ConfigSourceBase owner)
        {
            this.owner = owner;
        }

        #endregion

        #region Public properties

        public int Count => configList.Count;

        public bool IsSynchronized => false;

        public object SyncRoot => this;

        public IConfig this[int index] => (IConfig) configList[index];

        object IList.this[int index]
        {
            get => configList[index];
            set { }
        }

        public IConfig this[string configName]
        {
            get
            {
                IConfig result = null;

                foreach (IConfig config in configList)
                {
                    if (config.Name == configName)
                    {
                        result = config;

                        break;
                    }
                }

                return result;
            }
        }

        public bool IsFixedSize => false;

        public bool IsReadOnly => false;

        #endregion

        #region Public methods

        public void Add(IConfig config)
        {
            if (configList.Contains(config))
            {
                throw new ArgumentException("IConfig already exists");
            }

            IConfig existingConfig = this[config.Name];

            if (existingConfig != null)
            {
                // Set all new keys
                string[] keys = config.GetKeys();

                for (int i = 0; i < keys.Length; i++)
                {
                    existingConfig.Set(keys[i], config.Get(keys[i]));
                }
            }
            else
            {
                configList.Add(config);
                OnConfigAdded(new ConfigEventArgs(config));
            }
        }

        int IList.Add(object config)
        {
            IConfig newConfig = config as IConfig;

            if (newConfig == null)
            {
                throw new Exception("Must be an IConfig");
            }
            else
            {
                this.Add(newConfig);

                return IndexOf(newConfig);
            }
        }

        public IConfig Add(string name)
        {
            ConfigBase result = null;

            if (this[name] == null)
            {
                result = new ConfigBase(name, owner);
                configList.Add(result);
                OnConfigAdded(new ConfigEventArgs(result));
            }
            else
            {
                throw new ArgumentException("An IConfig of that name already exists");
            }

            return result;
        }

        public void Remove(IConfig config)
        {
            configList.Remove(config);
            OnConfigRemoved(new ConfigEventArgs(config));
        }

        public void Remove(object config)
        {
            configList.Remove(config);
            OnConfigRemoved(new ConfigEventArgs((IConfig) config));
        }

        public void RemoveAt(int index)
        {
            IConfig config = (IConfig) configList[index];
            configList.RemoveAt(index);
            OnConfigRemoved(new ConfigEventArgs(config));
        }

        public void Clear()
        {
            configList.Clear();
        }

        public IEnumerator GetEnumerator()
        {
            return configList.GetEnumerator();
        }

        public void CopyTo(Array array, int index)
        {
            configList.CopyTo(array, index);
        }

        public void CopyTo(IConfig[] array, int index)
        {
            ((ICollection) configList).CopyTo(array, index);
        }

        public bool Contains(object config)
        {
            return configList.Contains(config);
        }

        public int IndexOf(object config)
        {
            return configList.IndexOf(config);
        }

        public void Insert(int index, object config)
        {
            configList.Insert(index, config);
        }

        #endregion

        #region Public events

        public event ConfigEventHandler ConfigAdded;

        public event ConfigEventHandler ConfigRemoved;

        #endregion

        #region Protected methods

        protected void OnConfigAdded(ConfigEventArgs e)
        {
            ConfigAdded?.Invoke(this, e);
        }

        protected void OnConfigRemoved(ConfigEventArgs e)
        {
            ConfigRemoved?.Invoke(this, e);
        }

        #endregion

        #region Private methods

        #endregion
    }
}