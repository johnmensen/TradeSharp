//+------------------------------------------------------------------+
//|                                                        UniFeeder |
//|                 Copyright © 2001-2008, MetaQuotes Software Corp. |
//|                                         http://www.metaquotes.ru |
//+------------------------------------------------------------------+
#pragma once
#include "FeedInterface.h"
#include "Logger.h"
#include "QuoteSyncQueue.h"
#include "NewsSyncQueue.h"
#include "UdpListener.h"

//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
#define READ_BUFFER_MAX  (1024*1024)
#define READ_BUFFER_STEP (16*1024)
//+------------------------------------------------------------------+
//| Datafeed interface implementation                                |
//+------------------------------------------------------------------+
class CSourceInterface : public CFeedInterface
{
protected:
	char						*m_symbols;// instruments list      
	CSourceInterface			*m_next;   // next interface pointer (do not delete!)
	static QuoteSyncQueue		*queue;	   // очередь котировок, поступающих по UDP
	static NewsSyncQueue		*news;	   // очередь новостей, поступающих по UDP
	UdpListener					*listener;
public:
                     CSourceInterface(void);
	virtual          ~CSourceInterface(void);
	//---- search helpers (do not delete)
	inline CSourceInterface* Next(void) const      { return(m_next); }
	inline void       Next(CSourceInterface *next) { m_next=next;    }
	//---- implementations (do not delete)
	virtual int       Connect(LPCSTR server,LPCSTR login,LPCSTR password);
	virtual void      Close(void);
	virtual void      SetSymbols(LPCSTR symbols);
	virtual int       Read(FeedData *data);
	virtual int       Journal(char *buffer) { return ExtLogger.Journal(buffer); }
public:
	static void		  OnReceive(char *buf, int len);
};

//+------------------------------------------------------------------+
//| News header structure                                            |
//+------------------------------------------------------------------+
struct NewsTopic
{
   char              time[20];        // time in "yyyy/mm/dd hh:mm:ss" format
   char              topic[256];      // subject (topic)
   char              keywords[256];   // keywords
   int               len;             // message length
   char              unused[12];      // unused data (left for compatibility)
};