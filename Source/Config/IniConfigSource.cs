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
using System.IO;
using System.Collections;

using Nini.Ini;

namespace Nini.Config
{
    public class IniConfigSource : ConfigSourceBase
    {
        #region Private variables

        IniDocument _iniDocument   = null;
        string      _savePath      = null;

        #endregion

        #region Public properties

        #endregion

        #region Constructors

        public IniConfigSource()
        {
            _iniDocument = new IniDocument();
        }

        public IniConfigSource(string filePath)
        {
            Load(filePath);
        }

        public IniConfigSource(TextReader reader)
        {
            Load(reader);
        }

        public IniConfigSource(IniDocument document)
        {
            Load(document);
        }

        public IniConfigSource(Stream stream)
        {
            Load(stream);
        }

        #endregion

        #region Public properties

        public bool CaseSensitive { get; set; } = true;

        public string SavePath => _savePath;

        #endregion

        #region Public methods

        public void Load(string filePath)
        {
            Load(new StreamReader(filePath));
            this._savePath = filePath;
        }

        public void Load(TextReader reader)
        {
            Load(new IniDocument(reader));
        }

        public void Load(IniDocument document)
        {
            this.Configs.Clear();

            this.Merge(this); // required for SaveAll
            _iniDocument = document;
            Load();
        }

        public void Load(Stream stream)
        {
            Load(new StreamReader(stream));
        }

        public override void Save()
        {
            if (!IsSavable())
            {
                throw new ArgumentException("Source cannot be saved in this state");
            }

            MergeConfigsIntoDocument();

            _iniDocument.Save(this._savePath);
            base.Save();
        }

        public void Save(string path)
        {
            this._savePath = path;
            this.Save();
        }

        public void Save(TextWriter writer)
        {
            MergeConfigsIntoDocument();
            _iniDocument.Save(writer);
            _savePath = null;
            OnSaved(new EventArgs());
        }

        public void Save(Stream stream)
        {
            MergeConfigsIntoDocument();
            _iniDocument.Save(stream);
            _savePath = null;
            OnSaved(new EventArgs());
        }

        public override void Reload()
        {
            if (_savePath == null)
            {
                throw new ArgumentException("Error reloading: You must have "
                                          + "the loaded the source from a file");
            }

            _iniDocument = new IniDocument(_savePath);
            MergeDocumentIntoConfigs();
            base.Reload();
        }

        public override string ToString()
        {
            MergeConfigsIntoDocument();
            StringWriter writer = new StringWriter();
            _iniDocument.Save(writer);

            return writer.ToString();
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Merges all of the configs from the config collection into the 
        /// IniDocument before it is saved.  
        /// </summary>
        private void MergeConfigsIntoDocument()
        {
            RemoveSections();

            foreach (IConfig config in this.Configs)
            {
                string[] keys = config.GetKeys();

                // Create a new section if one doesn't exist
                if (_iniDocument.Sections[config.Name] == null)
                {
                    IniSection section = new IniSection(config.Name);
                    _iniDocument.Sections.Add(section);
                }

                RemoveKeys(config.Name);

                for (int i = 0; i < keys.Length; i++)
                {
                    _iniDocument.Sections[config.Name].Set(keys[i], config.Get(keys[i]));
                }
            }
        }

        /// <summary>
        /// Removes all INI sections that were removed as configs.
        /// </summary>
        private void RemoveSections()
        {
            for (int i = 0; i < _iniDocument.Sections.Count; i++)
            {
                var section = _iniDocument.Sections[i];

                if (this.Configs[section.Name] == null)
                {
                    _iniDocument.Sections.Remove(section.Name);
                }
            }
        }

        /// <summary>
        /// Removes all INI keys that were removed as config keys.
        /// </summary>
        private void RemoveKeys(string sectionName)
        {
            var section = _iniDocument.Sections[sectionName];

            if (section != null)
            {
                foreach (string key in section.GetKeys())
                {
                    if (this.Configs[sectionName].Get(key) == null)
                    {
                        section.Remove(key);
                    }
                }
            }
        }

        /// <summary>
        /// Loads the configuration file.
        /// </summary>
        private void Load()
        {
            for (int j = 0; j < _iniDocument.Sections.Count; j++)
            {
                var section = _iniDocument.Sections[j];
                var config = new IniConfig(section.Name, this);

                for (int i = 0; i < section.ItemCount; i++)
                {
                    var item = section.GetItem(i);

                    if (item.Type == IniType.Key)
                    {
                        config.Add(item.Name, item.Value);
                    }
                }

                this.Configs.Add(config);
            }
        }

        /// <summary>
        /// Merges the IniDocument into the Configs when the document is 
        /// reloaded.  
        /// </summary>
        private void MergeDocumentIntoConfigs()
        {
            // Remove all missing configs first
            RemoveConfigs();

            for (int i = 0; i < _iniDocument.Sections.Count; i++)
            {
                var section = _iniDocument.Sections[i];

                IConfig config = this.Configs[section.Name];

                if (config == null)
                {
                    // The section is new so add it
                    config = new ConfigBase(section.Name, this);
                    this.Configs.Add(config);
                }

                RemoveConfigKeys(config);
            }
        }

        /// <summary>
        /// Removes all configs that are not in the newly loaded INI doc.  
        /// </summary>
        private void RemoveConfigs()
        {
            for (int i = this.Configs.Count - 1; i > -1; i--)
            {
                var config = this.Configs[i];

                // If the section is not present in the INI doc
                if (_iniDocument.Sections[config.Name] == null)
                {
                    this.Configs.Remove(config);
                }
            }
        }

        /// <summary>
        /// Removes all INI keys that were removed as config keys.
        /// </summary>
        private void RemoveConfigKeys(IConfig config)
        {
            IniSection section = _iniDocument.Sections[config.Name];

            // Remove old keys
            string[] configKeys = config.GetKeys();

            foreach (string configKey in configKeys)
            {
                if (!section.Contains(configKey))
                {
                    // Key doesn't exist, remove
                    config.Remove(configKey);
                }
            }

            // Add or set all new keys
            string[] keys = section.GetKeys();

            for (int i = 0; i < keys.Length; i++)
            {
                string key = keys[i];
                config.Set(key, section.GetItem(i).Value);
            }
        }

        /// <summary>
        /// Returns true if this instance is savable.
        /// </summary>
        private bool IsSavable()
        {
            return (this._savePath != null);
        }

        #endregion
    }
}