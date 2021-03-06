﻿using Lazurite.ActionsDomain.ValueTypes;
using System;

namespace Lazurite.Shared
{
    /// <summary>
    /// Слепок сценария для использования в плагинах
    /// </summary>
    public class ScenarioCast
    {
        private readonly Action<string> _set;
        private readonly Func<string> _get;

        public ScenarioCast(Action<string> set, Func<string> get, ValueTypeBase valueType, string name, string id, bool enabled, bool canSet, bool canView)
        {
            _set = set;
            _get = get;
            ValueType = valueType;
            Name = name;
            ID = id;
            Enabled = enabled;
            CanView = canView;
            CanSet = canSet;
        }

        public string Value
        {
            get => _get();
            set => _set(value);
        }

        public string Name { get; private set; }
        public ValueTypeBase ValueType { get; private set; }
        public string ID { get; private set; }
        public bool Enabled { get; private set; }
        public bool CanView { get; private set; }
        public bool CanSet { get; private set; }
    }
}