using System;
using TradeSharp.Util;
using QuickFix;

namespace TradeSharp.ProviderProxy.BL
{
    class FixProxyServer
    {
        private QuoteRequest quoteRequest;
        private static FixProxyServer instance;

        public static FixProxyServer Instance
        {
            get
            {
                if (instance == null)
                    instance = new FixProxyServer();
                return instance;
            }
        }

        private readonly SocketInitiator initiator;
        private readonly FixApplication app;
        public SessionSettings sessionSettings;

        private FixProxyServer()
        {
            quoteRequest = new QuoteRequest();
            FileStoreFactory mesStoreFact;
            DefaultMessageFactory mesFact;

            SessionSettings sets;
            try
            {
                sets = new SessionSettings(string.Format("{0}\\{1}",
                                                         ExecutablePath.ExecPath,
                                                         SessionSettingsParser.SessionSettingsFileName));
                SessionSettingsParser.Init(string.Format("{0}\\{1}",
                                                         ExecutablePath.ExecPath,
                                                         SessionSettingsParser.SessionSettingsFileName));
                app = new FixApplication();
                app.processMessageFromBroker += MessageDispatcher.ProcessMessageFromBroker;
                mesStoreFact = new FileStoreFactory(string.Format("{0}\\log",
                                                         ExecutablePath.ExecPath));
                mesFact = new DefaultMessageFactory();

                app.OnSessionLogon += AppOnSessionLogon;
            }
            catch (Exception ex)
            {
                Logger.Error("SocketInitiator pre-create error", ex);
                throw;
            }

            try
            {
                initiator = new SocketInitiator(app, mesStoreFact, sets, mesFact);
            }
            catch (Exception ex)
            {
                Logger.Error("SocketInitiator create error", ex);
                throw;
            }
        }

        private void AppOnSessionLogon(SessionID sessionId)
        {
            Logger.InfoFormat("FixProxyServer.AppOnSessionLogon: {0}", sessionId);
            quoteRequest.RequestMarketData(sessionId);
        }

        public ProcessMessageFromBrokerDel ProcessMessageFromBroker
        {
            get
            {
                return app.processMessageFromBroker;
            }
            set
            {
                app.processMessageFromBroker = value;
            }
        }

        public void Start()
        {
            try
            {
                initiator.start();
            }
            catch (Exception ex)
            {
                Logger.Error("SocketInitiator start error", ex);
                throw;
            }
        }

        public void Stop()
        {
            if (initiator.isLoggedOn())
            {
                try
                {
                    initiator.stop();
                }
                catch (Exception ex)
                {
                    Logger.Error("SocketInitiator stop error", ex);
                    throw;
                }
            }
        }

        public bool IsLoggedOn
        {
            get
            {
                return initiator.isLoggedOn();
            }
        }
    }
}
