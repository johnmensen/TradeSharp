using System;
using System.ServiceModel;

namespace TradeSharp.Contract.Proxy
{
    public class ServiceProxyHelper<TProxy, TChannel> : IDisposable
        where TProxy : ClientBase<TChannel>, new()
        where TChannel : class
    {
        /// <summary>
        /// Private instance of the WCF service proxy.
        /// </summary>
        private TProxy proxy;

        /// <summary>
        /// Gets the WCF service proxy wrapped by this instance.
        /// </summary>
        protected TProxy Proxy
        {
            get
            {
                if (proxy != null) return proxy;
                throw new ObjectDisposedException("ServiceProxyHelper");                
            }
        }

        /// <summary>
        /// Constructs an instance.
        /// </summary>
        protected ServiceProxyHelper()
        {
            proxy = new TProxy();
        }

        /// <summary>
        /// Disposes of this instance.
        /// </summary>
        public void Dispose()
        {
            try
            {
                if (proxy != null)
                {
                    if (proxy.State != CommunicationState.Faulted)
                    {
                        proxy.Close();
                    }
                    else
                    {
                        proxy.Abort();
                    }
                }
            }
            catch (CommunicationException)
            {
                proxy.Abort();
            }
            catch (TimeoutException)
            {
                proxy.Abort();
            }
            catch (Exception)
            {
                proxy.Abort();
                throw;
            }
            finally
            {
                proxy = null;
            }
        }
    }
}
