using System;
using TradeSharp.Util;

namespace TradeSharp.Contract.Entity
{
    [Serializable]
    public class ServiceStateInfo
    {
        public ServiceProcessState State { get; private set; }

        public string LastError { get; private set; }

        public DateTime LastErrorOccured { get; private set; }

        private const int LockTimeout = 1000;

        /// <summary>
        /// коды ошибок
        /// ошибки с одним кодом не накапливаются        
        /// </summary>
        [NonSerialized]
        private readonly ThreadSafeList<ServiceProcessErrorWithCode> errorCodes = new ThreadSafeList<ServiceProcessErrorWithCode>();

        public ServiceStateInfo()
        {
            State = ServiceProcessState.Offline;
        }
        public ServiceStateInfo(ServiceProcessState _state)
        {
            State = _state;
        }
        public ServiceStateInfo(ServiceProcessState _state, string _lastError, DateTime _lastErrorOccured)
        {
            State = _state;
            LastError = _lastError;
            LastErrorOccured = _lastErrorOccured;
        }
        public bool IsOfSameSeverity(ServiceStateInfo ssi)
        {
            return ssi.State == State;
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj is ServiceStateInfo == false) return false;
            var ssi = (ServiceStateInfo)obj;
            return State == ssi.State &&
                LastError == ssi.LastError &&
                LastErrorOccured == ssi.LastErrorOccured;
        }
        public override int GetHashCode()
        {
            return (int)State *
                (string.IsNullOrEmpty(LastError) ? 1 : LastError.Length + 1) *
                LastErrorOccured.Second;
        }

        /// <summary>
        /// установить ошибку, добавить ее в список ошибок для возможности снять
        /// </summary>        
        public void AddError(ServiceProcessState _state, string _lastError, DateTime _lastErrorOccured, string errorCode)
        {
            State = _state;
            var errorWithCode = new ServiceProcessErrorWithCode { code = errorCode, error = _state };

            if (_state == ServiceProcessState.HasCriticalErrors ||
                _state == ServiceProcessState.HasErrors ||
                _state == ServiceProcessState.HasWarnings)
            {
                LastError = _lastError;
                LastErrorOccured = _lastErrorOccured;
                errorCodes.TryRemove(errorWithCode, LockTimeout);
            }
        }

        public void AddError(ServiceProcessState _state, string _lastError, DateTime _lastErrorOccured)
        {
            AddError(_state, _lastError, _lastErrorOccured, "");
        }

        public void SetState(ServiceProcessState _state)
        {
            State = _state;
        }

        /// <summary>
        /// снять ошибку. состояние устанавливается в пред. состояние
        /// (ОК если других ошибок в списке нет)
        /// пример: получение котировки после приостанова потока котировок
        /// </summary>        
        public void RemoveError(ServiceProcessState _state, string errorCode)
        {
            var errorWithCode = new ServiceProcessErrorWithCode { code = errorCode, error = _state };
            errorCodes.TryRemove(errorWithCode, LockTimeout);
            var count = errorCodes.Count ?? 0;
            State = count == 0 ? ServiceProcessState.OK : errorCodes[count - 1].error;
        }
    }

    [Flags]
    public enum ServiceProcessState
    {
        OK = 0,
        Offline = 1,
        Starting = 2,
        ShuttingDown = 4,
        HasWarnings = 8,
        HasErrors = 16,
        HasCriticalErrors = 32
    }

    /// <summary>
    /// ошибка сохраняется с кодом (строка, м.б. пустой)
    /// если потребуется снять ошибку, нужно указать ее код
    /// </summary>
    struct ServiceProcessErrorWithCode
    {
        public ServiceProcessState error;
        public string code;

        public override bool Equals(object obj)
        {
            return obj is ServiceProcessErrorWithCode
                       ? ((ServiceProcessErrorWithCode)obj).error == error && ((ServiceProcessErrorWithCode)obj).code == code
                       : false;
        }

        public override int GetHashCode()
        {
            return string.IsNullOrEmpty(code) ? (int)error : (int)error + 1000 * code.Length;
        }

        public static bool operator ==(ServiceProcessErrorWithCode x, ServiceProcessErrorWithCode y)
        {
            return x.code == y.code && x.error == y.error;
        }

        public static bool operator !=(ServiceProcessErrorWithCode x, ServiceProcessErrorWithCode y)
        {
            return !(x == y);
        }
    }
}
