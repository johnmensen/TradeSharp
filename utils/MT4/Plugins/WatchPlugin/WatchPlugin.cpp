//+------------------------------------------------------------------+
//|                                                  Trade Collector |
//|                 Copyright © 2005-2008, MetaQuotes Software Corp. |
//|                                        http://www.metaquotes.net |
//+------------------------------------------------------------------+
#include "stdafx.h"
#include "Logger.h"
#include "misc\common.h"
#include "Configuration.h"
#include "Reporter.h"

//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
PluginInfo          ExtPluginInfo = { PROGRAM_TITLE, 101, "Forexinvest LTD", {0} };
CServerInterface   *ExtServer = NULL;
//+------------------------------------------------------------------+
//| DLL entry point                                                  |
//+------------------------------------------------------------------+
BOOL APIENTRY DllMain(HANDLE hModule,DWORD  ul_reason_for_call,LPVOID lpReserved)
{
	char *cp;
	switch(ul_reason_for_call)
    {
      case DLL_PROCESS_ATTACH:
        //---- current folder
        GetModuleFileName((HMODULE)hModule, ExtProgramPath, sizeof(ExtProgramPath)-1);
        cp = &ExtProgramPath[strlen(ExtProgramPath)-2];
        while(cp>ExtProgramPath && *cp!='\\') cp--; *cp=0;		
        
        ExtLogger.Out(CmdOK, PROGRAM_TITLE, "Watch plugin is initialized");
        break;

      case DLL_THREAD_ATTACH:
      case DLL_THREAD_DETACH:
        break;

      case DLL_PROCESS_DETACH:       

        break;
    }

	return(TRUE);
}
//+------------------------------------------------------------------+
//| About, must be present always!                                   |
//+------------------------------------------------------------------+
void APIENTRY MtSrvAbout(PluginInfo *info)
{
   if (info != NULL) memcpy(info, &ExtPluginInfo, sizeof(PluginInfo));
}
//+------------------------------------------------------------------+
//| Set server interface point                                       |
//+------------------------------------------------------------------+
int APIENTRY MtSrvStartup(CServerInterface *server)
{
	if(server == NULL) return(FALSE);	
	
	//---- save server interface link
	ExtServer = server;
	ExtServer->LogsOut(CmdOK, PROGRAM_TITLE, "Watch: plugin initialized");

	extReporter = new Reporter();

	//---- all is ok
	return (TRUE);
}
//+------------------------------------------------------------------+
//| MUST BE ALWAYS! CLEAR ALL OBJECTS HERE                           |
//+------------------------------------------------------------------+
void APIENTRY MtSrvCleanup(void)
{
	delete extReporter;
}
//+------------------------------------------------------------------+
//| Plugin config access                                             |
//+------------------------------------------------------------------+
int APIENTRY MtSrvPluginCfgAdd(const PluginCfg *cfg)            { return ExtConfig.Add(cfg);             }
int APIENTRY MtSrvPluginCfgSet(const PluginCfg *values,const int total)
                                                                { return ExtConfig.Set(values,total);    }
int APIENTRY MtSrvPluginCfgGet(LPCSTR name,PluginCfg *cfg)      { return ExtConfig.Get(name,cfg);        }
int APIENTRY MtSrvPluginCfgNext(const int index,PluginCfg *cfg) { return ExtConfig.Next(index,cfg);      }
int APIENTRY MtSrvPluginCfgDelete(LPCSTR name)                  { return ExtConfig.Delete(name);         }
int APIENTRY MtSrvPluginCfgTotal()                              { return ExtConfig.Total();              }
int APIENTRY MtSrvPluginCfgLog(LPSTR log,const int max_len)     { return ExtLogger.Journal(log,max_len); }
//+------------------------------------------------------------------+
