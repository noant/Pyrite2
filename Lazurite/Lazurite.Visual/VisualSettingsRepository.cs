﻿using Lazurite.Data;
using Lazurite.IOC;
using Lazurite.MainDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lazurite.Visual
{
    public class VisualSettingsRepository
    {
        private ISavior _savior = Singleton.Resolve<ISavior>();
        private UsersRepositoryBase _usersRepository = Singleton.Resolve<UsersRepositoryBase>();
        private ScenariosRepositoryBase _scenariosRepository = Singleton.Resolve<ScenariosRepositoryBase>();
        private List<UserVisualSettings> _allSettings = new List<UserVisualSettings>();
        private readonly string _key = "visualLayouts";

        public VisualSettingsRepository()
        {
            if (_savior.Has(_key))
                _allSettings = _savior.Get<List<UserVisualSettings>>(_key);
            _usersRepository.OnUserRemoved = (user) => 
            {
                _allSettings.RemoveAll(x => x.UserId.Equals(user.Id));
                Save();
            };
            _scenariosRepository.OnScenarioRemoved += (scenario) =>
            {
                _allSettings.RemoveAll(x => x.ScenarioId.Equals(scenario.Id));
                Save();
            };
        }

        private void Save()
        {
            _savior.Set(_key, _allSettings);
        }

        public UserVisualSettings[] VisualSettings
        {
            get
            {
                return _allSettings.ToArray();
            }
        }

        public void Add(UserVisualSettings settings)
        {
            _allSettings.RemoveAll(x => x.SameAs(settings));
            _allSettings.Add(settings);
            Save();
        }

        public void Remove(UserVisualSettings settings)
        {
            _allSettings.RemoveAll(x => x.SameAs(settings));
            Save();
        }

        public void Update(UserVisualSettings settings)
        {
            Add(settings); //operation equals
        }
    }
}