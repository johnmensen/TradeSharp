//+------------------------------------------------------------------+
//|                                            MetaTrader Server API |
//|                 Copyright © 2001-2008, MetaQuotes Software Corp. |
//|                                         http://www.metaquotes.ru |
//+------------------------------------------------------------------+
#include "stdafx.h"
#include "Logger.h"
#include "FeedInterface.h"
#include "SourceInterface.h"

#define DATASOURCE_API __declspec(dllexport)
#define ProgramVersion 101

static FeedDescription ExtDescription =
{
   ProgramVersion,                                 // feeder version
   "FX Feeder",                                    // feeder name
   "Copyright © 2001-2009, FOREXINVEST LTD.",	   // copyright string
   "http://www.forexinvest.ru",                    // web information
   "asitaev@forexinvest.ru",                       // e-mail
   "localhost:1900",                               // communicating server
   "localhost",                                    // default login
   "localhost",                                    // default password
   modeQuotesAndNews,                              // mode (see FeederModes enum)
   //---- feeder short description
   "Слушает указанный в настройках (19000 по-умолчанию) порт на предмет котировок формата EURUSD,1.5213,1.5215,2001/11/12 18:31:01;GBPUSD,1.7103,1.7105,2001 11 12 18:31:04",
   0
};

CSourceInterface    *ExtFeeders = NULL;
CRITICAL_SECTION	locker;
CSync               ExtSync;


BOOL APIENTRY DllMain(HANDLE hModule,DWORD  ul_reason_for_call,LPVOID /*lpReserved*/)
{
	char *cp;

	switch(ul_reason_for_call)
	{
		case DLL_PROCESS_ATTACH:			
			
			InitializeCriticalSection(&locker);
			//---- parse current folder
			GetModuleFileName((HMODULE)hModule, ExtProgramPath, sizeof(ExtProgramPath)-1);
			if((cp = strrchr(ExtProgramPath, '.')) != NULL) *cp = 0;
			//---- initialization message
			ExtLogger.Cut();
			ExtLogger.Out("");
			ExtLogger.Out("%s %d.%02d initialized",ExtDescription.name,ProgramVersion/100,ProgramVersion%100);

			try
			{
				UdpListener::SetupWSA();
			}
			catch (...)
			{
				ExtLogger.Out("Couldn't setup WSA");
			}
		break;

		case DLL_THREAD_ATTACH: break;
		case DLL_THREAD_DETACH:	break;

		case DLL_PROCESS_DETACH:			

			ExtSync.Lock();
			while(ExtFeeders!=NULL)
			{
				//---- datafeed not closed properly!
				ExtLogger.Out("Unload: datafeed %0X not freed", ExtFeeders);
				CSourceInterface *next = ExtFeeders->Next();
				delete ExtFeeders;
				ExtFeeders = next;
			}
			ExtSync.Unlock();

			DeleteCriticalSection(&locker);
			UdpListener::TeardownWSA();

			break;
	}

	return(TRUE);
}


//+------------------------------------------------------------------+
//| Create a new datafeed                                            |
//+------------------------------------------------------------------+
CFeedInterface* DATASOURCE_API  DsCreate()
{
	CSourceInterface  *feed;

	EnterCriticalSection(&locker);

	ExtLogger.Out("Feeder creating (DsCreate())");
	if ((feed = new CSourceInterface) != NULL)
	{
		//---- insert to list (at first position)
		feed->Next(ExtFeeders);
		ExtFeeders = feed;
	}
	ExtLogger.Out("Feeder created (DsCreate())");

	LeaveCriticalSection(&locker);

	return ((CFeedInterface*)feed);  // return virtual interface
}
//+------------------------------------------------------------------+
//| Delete datafeed                                                  |
//+------------------------------------------------------------------+
void DATASOURCE_API DsDestroy(CFeedInterface *feed)
{
	EnterCriticalSection(&locker);

	CSourceInterface *next = ExtFeeders, *last = NULL;
	while(next != NULL)
	{
		if(next == feed)  // found
		{
			if(last == NULL) ExtFeeders = next->Next();
			else             last->Next(next->Next());
			delete next;
			LeaveCriticalSection(&locker);
			return;
		}
		last = next; 
		next = next->Next();
	}

	LeaveCriticalSection(&locker);
}
//+------------------------------------------------------------------+
//| Request description                                              |
//+------------------------------------------------------------------+
FeedDescription* DATASOURCE_API DsVersion() { return(&ExtDescription); }
//+------------------------------------------------------------------+



//void APIENTRY MtSrvAbout(PluginInfo *info)
//{
//   if (info != NULL) 
//	   memcpy(info, &ExtPluginInfo, sizeof(PluginInfo));
//}
//
//int APIENTRY MtSrvStartup(CServerInterface *server)
//{
//	if(server == NULL)                        return(FALSE);
//	if(server->Version() != ServerApiVersion) return(FALSE);	
//	ExtServer = server;	
//
//	Logger::SetServerInstance(ExtServer);	
//
//	curStorage = new CurrencyStorage(ExtServer);
//	curStorage->Init();	
//
//	Logger::LogMessage("Ready");
//
//	return(TRUE);
//}
//
//void APIENTRY MtSrvCleanup()
//{	
//	delete curStorage;
//}
//
//int APIENTRY MtSrvPluginCfgAdd(const PluginCfg *cfg)
//{
//   int res = ExtConfig.Add(cfg);   
//   curStorage->Init();
//   return(res);
//}
//
//int APIENTRY MtSrvPluginCfgSet(const PluginCfg *values,const int total)
//{
//   int res = 0;
//   res = ExtConfig.Set(values,total);   
//   curStorage->Init();        
//   /*char state[256];
//   curStorage.GetStateString(state, 256);
//   ExtServer->LogsOut(CmdOK, "FX Feeder", state);
//   delete state; */   
//   return(res);
//}
//
//int APIENTRY MtSrvPluginCfgDelete(LPCSTR name)
//{
//   int res = ExtConfig.Delete(name);   
//   curStorage->Init();   	     
//   return(res);
//}
//
//int APIENTRY MtSrvPluginCfgGet(LPCSTR name,PluginCfg *cfg)              { return ExtConfig.Get(name,cfg);        }
//int APIENTRY MtSrvPluginCfgNext(const int index,PluginCfg *cfg)         { return ExtConfig.Next(index,cfg);      }
//int APIENTRY MtSrvPluginCfgTotal()                                      { return ExtConfig.Total();              }
//
