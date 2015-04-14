//+------------------------------------------------------------------+
//|                                                        UniFeeder |
//|                 Copyright © 2001-2008, MetaQuotes Software Corp. |
//|                                         http://www.metaquotes.ru |
//+------------------------------------------------------------------+
#include "stdafx.h"
#include "Source.h"
//+------------------------------------------------------------------+
//| Description-you can edit it                                      |
//+------------------------------------------------------------------+
#define ProgramVersion 931

static FeedDescription ExtDescription=
  {
   ProgramVersion,                                 // feeder version
   "FX UniFeeder (from orig)",                     // feeder name
   "Copyright © 2001-2008, MetaQuotes Software Corp.",  // copyright string
   "http://www.metaquotes.net",                    // web information
   "info@metaquotes.net",                          // e-mail
   "localhost:2222",                               // communicating server
   "localhost",                                    // default login
   "localhost",                                    // default password
   modeQuotesAndNews,                              // mode (see FeederModes enum)
   //---- feeder short description
   "The feeder connects either to Universal DDE Connector for quotations\n"
   "or to eSignal News Server for news.\n\n"

   "For using the feeder, it is necessary to install and set up the proper software.\n"
   "In case of receiving quotations, the names of delivered instruments depend on the  \nDDE server set up.   \n\n"

   "Requirements:\n"
   "\tServer\t :  yes\n"
   "\tLogin\t :  yes\n"
   "\tPassword\t :  yes\n",
   0
  };
//+------------------------------------------------------------------+
//|      Standard wrapper for reenterable datafeed library           |
//|                                                                  |
//|          DO NOT EDIT next lines until end of file                |
//+------------------------------------------------------------------+
#define DATASOURCE_API __declspec(dllexport)
//+------------------------------------------------------------------+
//| Global variables used for feeders list                           |
//+------------------------------------------------------------------+
CSync                ExtSync;
CSourceInterface    *ExtFeeders=NULL;
//+------------------------------------------------------------------+
//| Entry point                                                      |
//+------------------------------------------------------------------+
BOOL APIENTRY DllMain(HANDLE hModule,DWORD ul_reason_for_call,LPVOID /*lpReserved*/)
  {
   char *cp;
//----
   switch(ul_reason_for_call)
     {
      case DLL_PROCESS_ATTACH:
        //---- parse current folder
        GetModuleFileName((HMODULE)hModule,ExtProgramPath,sizeof(ExtProgramPath)-1);
        if((cp=strrchr(ExtProgramPath,'.'))!=NULL) *cp=0;
        //---- initialization message
        ExtLogger.Cut();
        ExtLogger.Out("");
        ExtLogger.Out("%s %d.%02d initialized",ExtDescription.name,ProgramVersion/100,ProgramVersion%100);
        break;
      case DLL_THREAD_ATTACH:  break;
      case DLL_THREAD_DETACH:  break;
      case DLL_PROCESS_DETACH:
        //---- delete all datafeeds
        ExtSync.Lock();
        while(ExtFeeders!=NULL)
          {
           //---- datafeed not closed properly!
           ExtLogger.Out("Unload: datafeed %0X not freed",ExtFeeders);
           CSourceInterface *next=ExtFeeders->Next();
           delete ExtFeeders;
           ExtFeeders=next;
          }
        ExtSync.Unlock();
        break;
     }
//----
   return(TRUE);
  }
//+------------------------------------------------------------------+
//| Create a new datafeed                                            |
//+------------------------------------------------------------------+
DATASOURCE_API CFeedInterface* DsCreate()
  {
   CSourceInterface  *feed;
//---- lock access
   ExtSync.Lock();
//---- create an interface
   if((feed=new CSourceInterface)!=NULL)
     {
      //---- insert to list (at first position)
      feed->Next(ExtFeeders);
      ExtFeeders=feed;
     }
//---- unlock
   ExtSync.Unlock();
   return((CFeedInterface*)feed);  // return virtual interface
  }
//+------------------------------------------------------------------+
//| Delete datafeed                                                  |
//+------------------------------------------------------------------+
DATASOURCE_API void DsDestroy(CFeedInterface *feed)
  {
   ExtSync.Lock();
//---- find the datafeed
   CSourceInterface *next=ExtFeeders,*last=NULL;
   while(next!=NULL)
     {
      if(next==feed)  // found
        {
         if(last==NULL) ExtFeeders=next->Next();
         else           last->Next(next->Next());
         delete next;
         ExtSync.Unlock();
         return;
        }
      last=next; next=next->Next();
     }
//----
   ExtSync.Unlock();
   ExtLogger.Out("Destroy: unknown datafeed %0X",feed);
  }
//+------------------------------------------------------------------+
//| Request description                                              |
//+------------------------------------------------------------------+
DATASOURCE_API FeedDescription *DsVersion() { return(&ExtDescription); }
//+------------------------------------------------------------------+
