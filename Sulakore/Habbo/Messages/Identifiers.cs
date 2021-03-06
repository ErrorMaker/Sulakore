﻿using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

using Sulakore.Habbo.Web;

namespace Sulakore.Habbo.Messages
{
    public abstract class Identifiers : IEnumerable<ushort>
    {
        private readonly string _section;
        private readonly Dictionary<ushort, string> _namesById;
        private readonly Dictionary<ushort, string> _hashesById;
        private readonly Dictionary<string, string> _namesByHash;
        private readonly SortedDictionary<string, ushort> _idsByName;

        public Identifiers()
        {
            _section = GetType().Name;
            _namesById = new Dictionary<ushort, string>();
            _hashesById = new Dictionary<ushort, string>();
            _namesByHash = new Dictionary<string, string>();
            _idsByName = new SortedDictionary<string, ushort>();
        }
        public Identifiers(HGame game, string identifiersPath)
            : this()
        {
            Load(game, identifiersPath);
        }

        public ushort this[string name]
        {
            get => _idsByName[name];
            set => _idsByName[name] = value;
        }

        public ushort GetId(string name)
        {
            if (!_idsByName.TryGetValue(name, out ushort id))
            {
                return ushort.MaxValue;
            }
            return id;
        }
        public bool TryGetId(string name, out ushort id)
        {
            return _idsByName.TryGetValue(name, out id);
        }

        public string GetHash(ushort id)
        {
            _hashesById.TryGetValue(id, out string hash);
            return hash;
        }
        public string GetName(ushort id)
        {
            _namesById.TryGetValue(id, out string name);
            return name;
        }
        public string GetName(string hash)
        {
            _namesByHash.TryGetValue(hash, out string name);
            return name;
        }

        public void Save(string path)
        {
            using (var output = new StreamWriter(path))
            {
                Save(output);
            }
        }
        public void Save(StreamWriter output)
        {
            output.WriteLine($"[{_section}]");
            foreach (string name in _idsByName.Keys)
            {
                output.WriteLine($"{name}={_idsByName[name]}");
            }
        }

        public void Load(HGame game, string identifiersPath)
        {
            using (var identifiersStream = new StreamReader(identifiersPath))
            {
                Load(game, identifiersStream);
            }
        }
        public void Load(HGame game, Stream identifiersStream)
        {
            using (var wrappedIdentifiersStream = new StreamReader(identifiersStream))
            {
                Load(game, wrappedIdentifiersStream);
            }
        }
        public void Load(HGame game, StreamReader identifiersStream)
        {
            _namesById.Clear();
            _idsByName.Clear();
            _hashesById.Clear();
            _namesByHash.Clear();
            bool isInSection = false;
            while (!identifiersStream.EndOfStream)
            {
                string line = identifiersStream.ReadLine();
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    isInSection = (line == ("[" + _section + "]"));
                }
                else if (isInSection)
                {
                    string[] values = line.Split('=');
                    string name = values[0].Trim();
                    string hash = values[1].Trim();

                    var id = ushort.MaxValue;
                    if (game.Messages.TryGetValue(hash, out List<MessageItem> messages) && messages.Count == 1)
                    {
                        id = messages[0].Id;
                        if (!_namesByHash.ContainsKey(hash))
                        {
                            _namesByHash.Add(hash, name);
                        }
                    }

                    if (id != ushort.MaxValue)
                    {
                        _namesById[id] = name;
                        _hashesById[id] = hash;
                    }
                    _idsByName[name] = id;
                    GetType().GetProperty(name)?.SetValue(this, id);
                }
            }
        }

        public override string ToString()
        {
            return ToString(false);
        }
        public string ToString(bool isFormatting)
        {
            if (!isFormatting)
            {
                return base.ToString();
            }
            else
            {
                var builder = new StringBuilder();
                builder.AppendLine($"[{_section}]");
                foreach (string name in _idsByName.Keys)
                {
                    builder.AppendLine($"{name}={_idsByName[name]}");
                }
                return builder.ToString().Trim();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public IEnumerator<ushort> GetEnumerator()
        {
            return _idsByName.Values.GetEnumerator();
        }
    }
}