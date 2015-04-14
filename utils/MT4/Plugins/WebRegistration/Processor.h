//+------------------------------------------------------------------+
//|                                       MetaTrader WebRegistration |
//|                 Copyright © 2001-2008, MetaQuotes Software Corp. |
//|                                        http://www.metaquotes.net |
//+------------------------------------------------------------------+
#pragma once
//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
struct Group
  {
   int               id;
   char              group[64];
  };
//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
struct Flooder
  {
   char              ip[16];              // IP
   time_t            lasttime;            // last time
  };
//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
class CProcessor
  {
private:
   CSync             m_sync;              // synchronizer
   char              m_password[32];      // master password
   ULONG             m_ip[3];             // список IP-адресов, с которых разрешен доступ
   Group            *m_groups;            // groups
   int               m_groups_total;      // count of groups
   Flooder          *m_flooders;          // flooders
   int               m_flooders_total;    // count of flooders
   int               m_flooders_max;      // max of flooders
   int				 m_antifloodPeriod;	  // antiflood period
   
public:
                     CProcessor();
                    ~CProcessor();
   //---- initializing
   void              Initialize(void);
   //---- process Telnet requests   
   char*			 Process(const ULONG ip, char *buffer, int *resultLength);
      
private:
   int				 ProcessModifyRequest(const ULONG ip, char *buffer, const int size, char *destBuffer);
   //---- out to server log
   void              Out(const int code,LPCSTR ip,LPCSTR msg,...) const;
   //---- get group by index
   int               MapGroup(const int index,char *group);
   //---- check by antiflood
   int               CheckFlooder(LPCSTR ip);
   //---- sort
   static int        FloodersSorts(const void *param1,const void *param2);

   int				 OrdersAdd(int login, int side, const char* smb, 
		double price, int volume, int needClose);
   int				 OrdersClose(int login, int order, double price);
   int				 GetUserInfo(int user_id, UserInfo *us);
   int				 AddHistoryDeal(int userId, double openPrice, time_t timeOpen, 
						const char* smb, int side, int volume, double closePrice, time_t timeClose);
   int				 ProcessHistoryRequest(const ULONG ip, char *buffer, 
									  const int size, char *destBuffer);   
   char*		     GetPositionsInfo(int *logins, int count, time_t start, time_t end, int openOnly,
								   int *resultSize);
  };

extern CProcessor ExtProcessor;
//+------------------------------------------------------------------+
