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
using System.Text;
using System.Collections;

namespace Nini.Config
{
    public abstract class ConfigSourceBase : IConfigSource
    {
        #region Private variables

        ArrayList        sourceList = new ArrayList();

        #endregion

        #region Constructors

        public ConfigSourceBase()
        {
            Configs = new ConfigCollection(this);
        }

        #endregion

        #region Public properties

        public ConfigCollection Configs { get; } = null;

        public bool AutoSave { get; set; } = false;

        #endregion

        #region Public methods

        public void Merge(IConfigSource source)
        {
            if (!sourceList.Contains(source))
            {
                sourceList.Add(source);
            }

            foreach (IConfig config in source.Configs)
            {
                this.Configs.Add(config);
            }
        }

        public virtual IConfig AddConfig(string name)
        {
            return Configs.Add(name);
        }

        public string GetExpanded(IConfig config, string key)
        {
            return Expand(config, key, false);
        }

        public virtual void Save()
        {
            OnSaved(new EventArgs());
        }

        public virtual void Reload()
        {
            OnReloaded(new EventArgs());
        }

        public void ExpandKeyValues()
        {
            string[] keys = null;

            foreach (IConfig config in Configs)
            {
                keys = config.GetKeys();

                for (int i = 0; i < keys.Length; i++)
                {
                    Expand(config, keys[i], true);
                }
            }
        }

        public void ReplaceKeyValues()
        {
            ExpandKeyValues();
        }

        #endregion

        #region Public events

        public event EventHandler Reloaded;

        public event EventHandler Saved;

        #endregion

        #region Protected methods

        protected void OnReloaded(EventArgs e)
        {
            Reloaded?.Invoke(this, e);
        }

        protected void OnSaved(EventArgs e)
        {
            Saved?.Invoke(this, e);
        }

        #endregion

        #region Private methods	

        /// <summary>
        /// Expands key values from the given IConfig.
        /// </summary>
        private string Expand(IConfig config, string key, bool setValue)
        {
            string result = config.Get(key);

            if (result == null)
            {
                throw new ArgumentException($"[{key}] not found in [{config.Name}]");
            }

            while (true)
            {
                int startIndex = result.IndexOf("${", 0);

                if (startIndex == -1)
                {
                    break;
                }

                int endIndex = result.IndexOf("}", startIndex + 2);

                if (endIndex == -1)
                {
                    break;
                }

                string search = result.Substring(startIndex + 2,
                                                 endIndex   - (startIndex + 2));

                if (search == key)
                {
                    // Prevent infinite recursion
                    throw new ArgumentException
                        ("Key cannot have a expand value of itself: " + key);
                }

                string replace = ExpandValue(config, search);

                result = result.Replace("${" + search + "}", replace);
            }

            if (setValue)
            {
                config.Set(key, result);
            }

            return result;
        }

        private string ExpandValue(IConfig config, string search)
        {
            string result = null;

            string[] replaces = search.Split('|');

            if (replaces.Length > 1)
            {
                IConfig newConfig = this.Configs[replaces[0]];

                if (newConfig == null)
                {
                    throw new ArgumentException("Expand config not found: "
                                              + replaces[0]);
                }

                result = newConfig.Get(replaces[1]);

                if (result == null)
                {
                    throw new ArgumentException("Expand key not found: "
                                              + replaces[1]);
                }
            }
            else
            {
                result = config.Get(search);

                if (result == null)
                {
                    throw new ArgumentException("Key not found: " + search);
                }
            }

            return result;
        }

        #endregion
    }
}