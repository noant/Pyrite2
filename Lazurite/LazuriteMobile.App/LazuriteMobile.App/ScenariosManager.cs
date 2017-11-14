﻿using LazuriteMobile.MainDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lazurite.MainDomain;
using Lazurite.IOC;
using Lazurite.MainDomain.MessageSecurity;
using System.Threading;
using Xamarin.Forms;
using Lazurite.Data;
using System.ServiceModel;
using System.Net;
using System.Runtime.Serialization;
using Lazurite.Utils;

namespace LazuriteMobile.App
{
    public class ScenariosManager : IScenariosManager
    {
        private class OperationResult<T>
        {
            public T Value
            {
                get;
                private set;
            }

            public bool Success
            {
                get;
                private set;
            }

            public DateTime? ServerTime
            {
                get;
                private set;
            }

            public OperationResult(T result, bool success, DateTime? serverTime = null)
            {
                Value = result;
                Success = success;
                ServerTime = serverTime;
            }
        }

        private readonly int _listenInterval = 12000;
        private readonly int _fullRefreshInterval = 20;
        private readonly string _cachedScenariosKey = "scensCache";
        private readonly string _clientSettingsKey = "clientSettings";
        private CancellationTokenSource _listenersCancellationTokenSource;
        private CancellationTokenSource _operationCancellationTokenSource;
        private IServiceClientManager _clientManager = Singleton.Resolve<IServiceClientManager>();
        private SaviorBase _savior = Singleton.Resolve<SaviorBase>();
        private IServiceClient _serviceClient;
        private ClientSettings _clientSettings;
        private DateTime _lastUpdateTime;

        public ScenarioInfo[] Scenarios { get; private set; }
        public bool Connected { get; private set; } = false;

        public event Action<ScenarioInfo[]> ScenariosChanged;
        public event Action ConnectionLost;
        public event Action ConnectionRestored;
        public event Action NeedRefresh;
        public event Action NeedClientSettings;
        public event Action LoginOrPasswordInvalid;
        public event Action SecretCodeInvalid;
        public event Action CredentialsLoaded;

        public ScenariosManager()
        {
            this.SecretCodeInvalid += () => StopListenChanges();
            this.LoginOrPasswordInvalid += () => StopListenChanges();
        }

        private bool HandleExceptions(Action action)
        {
            var cancellationToken = _operationCancellationTokenSource.Token;
            bool success = false;
            try
            {
                action();
                success = true;
                if (!Connected)
                {
                    Connected = true;
                    ConnectionRestored?.Invoke();
                }
            }
            catch (System.Exception e)
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    //if login or password wrong; error 403
                    if (e is WebException && 
                        ((HttpWebResponse)((WebException)e).Response)?.StatusCode == HttpStatusCode.Forbidden)
                    {
                        LoginOrPasswordInvalid?.Invoke();
                    }
                    //if data is wrong or secretKey.Length is wrong
                    else if (e is SerializationException || e.Message == "Key length not 128/192/256 bits.")
                    {
                        SecretCodeInvalid?.Invoke();
                    }
                    else if (Connected)
                        ConnectionLost?.Invoke();
                    Connected = false;
                    success = false;
                }
            }
            if (cancellationToken.IsCancellationRequested)
                return false;
            return success;
        }

        private OperationResult<T> Handle<T>(Func<Encrypted<T>> func) {
            T result = default(T);
            DateTime? serverTime = null;
            var success = HandleExceptions(() =>
            {
                var encryptedResult = func();
                result = encryptedResult.Decrypt(_clientSettings.SecretKey);
                serverTime = encryptedResult.ServerTime;
            });
            return new OperationResult<T>(result, success, serverTime);
        }

        private OperationResult<List<T>> Handle<T>(Func<EncryptedList<T>> func)
        {
            List<T> result = null;
            DateTime? serverTime = null;
            var success = HandleExceptions(() =>
            {
                var encryptedResult = func();
                result = encryptedResult.Decrypt(_clientSettings.SecretKey);
                serverTime = encryptedResult.ServerTime;
            });
            return new OperationResult<List<T>>(result, success, serverTime);
        }
        
        public void Initialize(Action<bool> callback)
        {
            TryLoadClientSettings();
            if (_clientSettings != null)
                InitializeInternal(callback);
            else 
            {
                callback?.Invoke(false);
                NeedClientSettings?.Invoke();
            }
        }

        private void InitializeInternal(Action<bool> callback)
        {
            TryLoadCachedScenarios();
            //cancel all operations
            if (_operationCancellationTokenSource != null)
                _operationCancellationTokenSource.Cancel();
            _operationCancellationTokenSource = new CancellationTokenSource();
            StopListenChanges();
            RecreateConnection();
            Refresh(callback);
            StartListenChanges();
        }

        private void RecreateConnection()
        {
            if (_serviceClient != null)
                _serviceClient.Close();
            _serviceClient = _clientManager.Create(_clientSettings.Host, _clientSettings.Port, _clientSettings.ServiceName, _clientSettings.SecretKey, _clientSettings.Login, _clientSettings.Password);
        }
        
        public void StartListenChanges()
        {
            if (_listenersCancellationTokenSource != null)
                _listenersCancellationTokenSource.Cancel();

            _listenersCancellationTokenSource = new CancellationTokenSource();

            int fullRefreshIncrement = 0;

            TaskUtils.StartLongRunning(() => { 
                while (!_listenersCancellationTokenSource.Token.IsCancellationRequested)
                {
                    if (fullRefreshIncrement == _fullRefreshInterval)
                    {
                        fullRefreshIncrement = 0;
                        Refresh(success =>
                        {
                            if (!success)
                                RecreateConnection();
                        });
                    }
                    else
                    {
                        Update(success =>
                        {
                            if (!success)
                                RecreateConnection();
                        });
                        fullRefreshIncrement++;
                    }
                    Task.Delay(_listenInterval).Wait();
                }
            });
        }

        public void StopListenChanges()
        {
            _listenersCancellationTokenSource?.Cancel();
            _serviceClient?.Close();
            Connected = false;
        }

        public void ExecuteScenario(ExecuteScenarioArgs args)
        {
            _serviceClient.BeginAsyncExecuteScenario(new Encrypted<string>(args.Id, _clientSettings.SecretKey), new Encrypted<string>(args.Value, _clientSettings.SecretKey), 
                (result) => {
                    HandleExceptions(() => _serviceClient.EndAsyncExecuteScenario(result));
                },
            null);
        }

        public void Refresh(Action<bool> callback)
        {
            _serviceClient.BeginGetScenariosInfo((x) => {
                var result = Handle(() => _serviceClient.EndGetScenariosInfo(x));
                if (result.Success)
                {
                    _lastUpdateTime = result.ServerTime ?? _lastUpdateTime;
                    Scenarios = result.Value.ToArray();
                    CacheScenarios();
                    NeedRefresh?.Invoke();
                }
                callback?.Invoke(result.Success);
            }, null);
        }

        public void Refresh()
        {
            Refresh(success => {
                if (!success)
                    RecreateConnection();
            });
        }

        public void Update(Action<bool> callback)
        {
            _serviceClient.BeginGetChangedScenarios(_lastUpdateTime,
            (o) =>
            {
                var result = Handle(() => _serviceClient.EndGetChangedScenarios(o));
                if (result.Success && result.Value != null && result.Value.Any())
                {
                    var changedScenariosLW = result.Value;
                    var changedScenarios = Scenarios.Where(x => changedScenariosLW.Any(z => z.ScenarioId == x.ScenarioId)).ToArray();
                    foreach (var changedScenario in changedScenariosLW)
                    {
                        var existingScenario = changedScenarios.FirstOrDefault(x => x.ScenarioId.Equals(changedScenario.ScenarioId));
                        if (existingScenario != null)
                            existingScenario.CurrentValue = changedScenario.CurrentValue;
                    }
                    _lastUpdateTime = result.ServerTime ?? _lastUpdateTime;
                    ScenariosChanged?.Invoke(changedScenarios);
                }
                callback?.Invoke(result.Success);
            }, null);
        }

        private void CacheScenarios()
        {
            _savior.Set(_cachedScenariosKey, Scenarios);
        }

        private void TryLoadCachedScenarios()
        {
            if (_savior.Has(_cachedScenariosKey))
            {
                try
                {
                    Scenarios = _savior.Get<ScenarioInfo[]>(_cachedScenariosKey);
                    NeedRefresh?.Invoke();
                }
                catch
                {
                    _savior.Clear(_cachedScenariosKey);
                }
            }
        }

        private void TryLoadClientSettings()
        {
            if (_savior.Has(_clientSettingsKey))
            {
                try
                {
                    _clientSettings = _savior.Get<ClientSettings>(_clientSettingsKey);
                    CredentialsLoaded?.Invoke();
                }
                catch
                {
                    _savior.Clear(_clientSettingsKey);
                }
            }
        }

        private void SaveClientSettings()
        {
            _savior.Set(_clientSettingsKey, _clientSettings);
        }

        public ClientSettings GetClientSettings()
        {
            return _clientSettings ?? new ClientSettings();
        }

        public void SetClientSettings(ClientSettings settings)
        {
            _clientSettings = settings;
            SaveClientSettings();
            InitializeInternal(null);
        }

        public void GetClientSettings(Action<ClientSettings> callback)
        {
            callback(GetClientSettings());
        }

        public void IsConnected(Action<bool> callback)
        {
            callback(Connected);
        }

        public void GetScenarios(Action<ScenarioInfo[]> callback)
        {
            callback(Scenarios);
        }

        public void Close()
        {
            StopListenChanges();
        }
    }
}
