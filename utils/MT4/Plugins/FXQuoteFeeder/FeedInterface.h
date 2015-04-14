#pragma once

//+------------------------------------------------------------------+
//| Feeder work mode                                                 |
//+------------------------------------------------------------------+
enum FeederModes
{
   modeOnlyQuotes    =0,                         // feeder can take quotes only
   modeOnlyNews      =1,                         // feeder can take news only
   modeQuotesAndNews =2,                         // feeder can take quotes and news as well
   modeQuotesOrNews  =3                          // feeder can take quotes or news only
};
//+------------------------------------------------------------------+
//| Feeder description                                               |
//+------------------------------------------------------------------+
struct FeedDescription
{
   int               version;                    // feeder version
   char              name[128];                  // feeder name
   char              copyright[128];             // copyright string
   char              web[128];                   // web information
   char              email[128];                 // e-mail
   char              server[128];                // communicating server
   char              username[32];               // default login
   char              userpass[32];               // default password
   int               modes;                      // mode (see FeederModes enum)
   char              description[512];           // feeder short description
   int               reserved[62];               // reserverd/unused
};
//+------------------------------------------------------------------+
//| Virtual interface                                                |
//+------------------------------------------------------------------+
class CFeedInterface
{
public:
   //---- virtual methods
   virtual int       Connect(LPCSTR server,LPCSTR login,LPCSTR password)=0;
   virtual void      Close(void)                    =0;
   virtual void      SetSymbols(LPCSTR symbols)     =0;
   virtual int       Read(FeedData *data)           =0;
   virtual int       Journal(char *buffer)          =0;
   //----
};
//+------------------------------------------------------------------+