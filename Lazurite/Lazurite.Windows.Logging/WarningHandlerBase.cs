﻿using Lazurite.Data;
using Lazurite.IOC;
using Lazurite.Logging;
using Lazurite.Shared;
using System;

namespace Lazurite.Windows.Logging
{
    public abstract class WarningHandlerBase : ILogger
    {
        private WarnType? _maxWritingWarnType = null;

        public WarnType MaxWritingWarnType
        {
            get
            {
                if (_maxWritingWarnType == null)
                {
                    var dataManager = Singleton.Resolve<DataManagerBase>();
                    if (dataManager.Has(nameof(_maxWritingWarnType)))
                    {
                        _maxWritingWarnType = dataManager.Get<WarnType>(nameof(_maxWritingWarnType));
                    }
                    else
                    {
                        _maxWritingWarnType = WarnType.Info;
                    }
                }
                return _maxWritingWarnType.Value;
            }
            set
            {
                _maxWritingWarnType = value;
                var dataManager = Singleton.Resolve<DataManagerBase>();
                dataManager.Set(nameof(_maxWritingWarnType), value);
            }
        }

        public abstract void InternalWrite(WarnType type, string message = null, Exception exception = null);

        public void Write(WarnType type, string message = null, Exception exception = null)
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine(message);
            if (exception != null)
            {
                System.Diagnostics.Debug.WriteLine(exception.Message);
                System.Diagnostics.Debug.WriteLine(exception.StackTrace);
            }
#endif
            if (type <= MaxWritingWarnType)
            {
                InternalWrite(type, message, exception);
            }

            RaiseOnWrite(type, message, exception);
        }

        public void Debug(string message = null, Exception exception = null)
        {
            Write(WarnType.Debug, message, exception);
        }

        public void Info(string message = null, Exception exception = null)
        {
            Write(WarnType.Info, message, exception);
        }

        public void InfoFormat(string message, params object[] infoParams)
        {
            Write(WarnType.Info, string.Format(message, infoParams), null);
        }

        public void Error(string message = null, Exception exception = null)
        {
            Write(WarnType.Error, message, exception);
        }

        public void Fatal(string message = null, Exception exception = null)
        {
            Write(WarnType.Fatal, message, exception);
        }

        public void Warn(string message = null, Exception exception = null)
        {
            Write(WarnType.Warn, message, exception);
        }

        public void WarnFormat(string message, params object[] warnParams)
        {
            Write(WarnType.Warn, string.Format(message, warnParams), null);
        }

        public void DebugFormat(string message, params object[] @params)
        {
            Write(WarnType.Debug, string.Format(message, @params), null);
        }

        public void ErrorFormat(Exception exception, string message, params object[] @params)
        {
            Write(WarnType.Error, string.Format(message, @params), exception);
        }

        public void InfoFormat(Exception exception, string message, params object[] infoParams)
        {
            Write(WarnType.Info, string.Format(message, infoParams), exception);
        }

        public void FatalFormat(Exception exception, string message = null, params object[] fatalParams)
        {
            Write(WarnType.Fatal, string.Format(message, fatalParams), exception);
        }

        public void WarnFormat(Exception exception, string message, params object[] warnParams)
        {
            Write(WarnType.Warn, string.Format(message, warnParams), exception);
        }

        private void RaiseOnWrite(WarnType type, string message, Exception exception)
        {
            OnWrite?.Invoke(this, new WarningEventArgs(type, message, exception));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
        public event EventsHandler<WarnType> OnWrite;
    }
}