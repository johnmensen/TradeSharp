//+------------------------------------------------------------------+
//|                                        MetaTrader Virtual Dealer |
//|                 Copyright © 2001-2008, MetaQuotes Software Corp. |
//|                                         http://www.metaquotes.ru |
//+------------------------------------------------------------------+
#include "stdafx.h"
#include "Processor.h"
#include "Config\Configuration.h"
#include "UDP\UdpListener.h"
//----
PluginInfo        ExtPluginInfo={ "FX Virtual Dealer", 100, "ForexInvest LTD",{0} };
CServerInterface *ExtServer=NULL;
//+------------------------------------------------------------------+
//| DLL entry point                                                  |
//+------------------------------------------------------------------+
BOOL APIENTRY DllMain(HANDLE hModule,DWORD  ul_reason_for_call,LPVOID /*lpReserved*/)
  {
   char tmp[256],*cp;
//----
   switch(ul_reason_for_call)
     {
      case DLL_PROCESS_ATTACH:
        //---- create configuration filename
        GetModuleFileName((HMODULE)hModule,tmp,sizeof(tmp)-5);
        if((cp=strrchr(tmp,'.'))!=NULL) { *cp=0; strcat(tmp,".ini"); }
        //---- load configuration
        ExtConfig.Load(tmp);
		extQueue = new ResponseQueue(CProcessor::OnFXIResponse);

		UdpListener::SetupWSA();

        break;

      case DLL_THREAD_ATTACH:
      case DLL_THREAD_DETACH:
        break;

      case DLL_PROCESS_DETACH:
		  delete extQueue;		  

		  UdpListener::TeardownWSA();

        //ExtProcessor.ShowStatus();
        break;
     }
//----
   return(TRUE);
  }
//+------------------------------------------------------------------+
//| About, must be present always!                                   |
//+------------------------------------------------------------------+
void APIENTRY MtSrvAbout(PluginInfo *info)
{
	if(info!=NULL) memcpy(info,&ExtPluginInfo,sizeof(PluginInfo));
}
//+------------------------------------------------------------------+
//| Set server interface point                                       |
//+------------------------------------------------------------------+
int APIENTRY MtSrvStartup(CServerInterface *server)
{
	// check version
	if(server==NULL)                        return(FALSE);
	if(server->Version()!=ServerApiVersion) return(FALSE);
	// save server interface link
	ExtServer=server;
	// initialize dealer helper
	ExtProcessor.Initialize();
	return(TRUE);
}
//+------------------------------------------------------------------+
//| Standard configuration functions                                 |
//+------------------------------------------------------------------+
int APIENTRY MtSrvPluginCfgAdd(const PluginCfg *cfg)
{
	int res = ExtConfig.Add(cfg);
	ExtProcessor.Initialize();
	return(res);
}
int APIENTRY MtSrvPluginCfgSet(const PluginCfg *values,const int total)
{
	int res=ExtConfig.Set(values,total);
	ExtProcessor.Initialize();
	return(res);
}
int APIENTRY MtSrvPluginCfgDelete(LPCSTR name)
{
	int res=ExtConfig.Delete(name);
	ExtProcessor.Initialize();
	return(res);
}
int APIENTRY MtSrvPluginCfgGet(LPCSTR name,PluginCfg *cfg)              { return ExtConfig.Get(name,cfg);        }
int APIENTRY MtSrvPluginCfgNext(const int index,PluginCfg *cfg)         { return ExtConfig.Next(index,cfg);      }
int APIENTRY MtSrvPluginCfgTotal()                                      { return ExtConfig.Total();              }
//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
void APIENTRY MtSrvTradeRequestApply(RequestInfo *request,const int isdemo)
{
	// запрос на отложенный ордер
	if (request->trade.type == TT_ORDER_PENDING_OPEN)
	{
		ExtServer->LogsOut(CmdOK, PROGRAM_TITLE, "Добавление отложенного ордера");		
		ExtProcessor.ConfirmPendingRequest(request);
		return;
	}
	
	// прочие ордера
	ExtProcessor.Add(request);
}
//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
int APIENTRY MtSrvTradePendingsFilter(const ConGroup *group,const ConSymbol *symbol,const TradeRecord *trade)
{
   return (RET_OK);
}
//+------------------------------------------------------------------+
//| срабатывание отложенного ордера                                  |
//+------------------------------------------------------------------+
int APIENTRY MtSrvTradePendingsApply(const UserInfo *user, const ConGroup *group, const ConSymbol *symbol,
									 const TradeRecord *pending, TradeRecord *trade)
{
	if (ExtProcessor.IsGroupEnabled(group->group) == FALSE)
		return RET_OK; //_NONE;
		
	// добавить запрос (будет отправлен в FXI)
	ExtProcessor.Out(CmdOK, "MtSrvTradePendingsApply (trade of %d, pending of %d, trade cmd=%d, pending cmd=%d)", 
			trade->volume, pending->volume, trade->cmd, pending->cmd);
	ExtProcessor.Add(user, group, symbol, pending, trade);
	
	// открытие обрабатываю сам
	return RET_TRADE_ACCEPTED;
}
//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
int APIENTRY MtSrvTradeStopsFilter(const ConGroup *group,const ConSymbol *symbol,const TradeRecord *trade)
{
	return(RET_OK);
}
//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
int  APIENTRY MtSrvTradeStopsApply(const UserInfo *user, const ConGroup *group,
								   const ConSymbol *symbol, TradeRecord *trade,
								   const int isTP)
{
	if (trade)
	{				
		// запрос на сервер		
		if (!ExtProcessor.ProcessStopApply(trade, group, isTP))
			return RET_OK;
	}
	return RET_OK_NONE; // ордер обработает процессор
}
//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
void APIENTRY MtSrvTradesAdd(TradeRecord *trade, const UserInfo *user,
							 const ConSymbol *symbol)
{	
	ExtProcessor.OnTradeHistoryRecord(trade);
}
//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
void APIENTRY MtSrvTradesUpdate(TradeRecord *trade, UserInfo *user, const int mode)
{
	CharString str;
	str.Printf(FALSE, 
		"MtSrvTradesUpdate::%s (order=%d, profit=%g, margin_rate=%g, cvr0=%g, cvr1=%g, storage(swap)=%g, commis=%g, com_agent=%g, taxes=%g). User [%s]",
		mode == UPDATE_NORMAL ? "NORMAL" :
		mode == UPDATE_ACTIVATE ? "ACTIVATE" :
		mode == UPDATE_CLOSE ? "CLOSE" :
		mode == UPDATE_DELETE ? "DELETE" : "UNDEFINED",
		trade->order,
		trade->profit, trade->margin_rate, trade->conv_rates[0], trade->conv_rates[1], 
		trade->storage, trade->commission, trade->commission_agent, trade->taxes, user->name);
	ExtServer->LogsOut(CmdOK, PROGRAM_TITLE, str.ToArray());	
}
//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
void APIENTRY MtSrvTradesAddExt(TradeRecord *trade, const UserInfo *user,
								const ConSymbol *symbol, const int mode)
{
	if (mode == OPEN_ROLLOVER)
	{
		CharString str;
		str.Printf(FALSE, 
			"MtSrvTradesAddExt::ROLLOVER (order=%d, profit=%g, margin_rate=%g, cvr0=%g, cvr1=%g, storage(swap)=%g)",
			trade->order,
			trade->profit, trade->margin_rate, trade->conv_rates[0], trade->conv_rates[1], 
			trade->storage);
		ExtServer->LogsOut(CmdOK, PROGRAM_TITLE, str.ToArray());	
	}
}
//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
int  APIENTRY MtSrvTradeRollover(TradeRecord *trade, const double value,
								 const OverNightData *data)
{
	CharString str;
		str.Printf(FALSE, 
			"MtSrvTradeRollover::(order=%d, point=%g, value=%g)",
			trade->order, data->point, value);
		ExtServer->LogsOut(CmdOK, PROGRAM_TITLE, str.ToArray());	

	return FALSE;
}
//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
int APIENTRY MtSrvTradeStopoutsFilter(const ConGroup *group, const ConSymbol *symbol,
									  const int login, const double equity, const double margin)
{
   return ExtProcessor.StopoutsFilter(group);
}
//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
int APIENTRY MtSrvTradeStopoutsApply(const UserInfo *user, const ConGroup *group,
									 const ConSymbol *symbol, TradeRecord *stopout)
{
	return ExtProcessor.StopoutsApply(user, group,stopout->comment);
}
//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
