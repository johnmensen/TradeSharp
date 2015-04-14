//+------------------------------------------------------------------+
//|                                        MetaTrader Virtual Dealer |
//|                 Copyright © 2001-2008, MetaQuotes Software Corp. |
//|                                         http://www.metaquotes.ru |
//+------------------------------------------------------------------+
#pragma once

#include "config/Configuration.h"
#include "CharString.h"
#include "UDP\UdpSender.h"
#include "ResponseQueue.h"
#include "OrderRequestQueue.h"

//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
struct Request 
{
	int            id;
	char           group[16];
	DWORD          time;
	TradeTransInfo trans;
	int			   sent;		// отправлен FXI
	int			   login;
	int			   pendingId;	// OrderID - заранее известен для отложенных ордеров
	int			   isPending;	// флаг отложенного ордера
	int			   isStopout;	// флаг стопаута 
};
//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
class CProcessor  
{
private:
	CSync             m_sync;
	int               m_delay;
	int               m_loss_confirm;
	int               m_manager;
	char              m_groups[64];
	HANDLE            m_thread;
	int               m_stop_flag;
	Request          *m_requests;
	int               m_requests_total;
	int               m_requests_max;
	int				  m_waitForFXIReply;
	int				  m_SLTPTimeout;
	volatile int	  m_nextStopRequestId;
	time_t			  m_lastLogWriteTime;
public:
	CProcessor();
	~CProcessor();
	//----
	void              Initialize(void);
	// обертка для вывода в лог
	void              Out(const int code, LPCSTR msg,...) const;
	void              OutNonFlood(const int code,  int floodInterval, LPCSTR msg,...);
	// постановка запросов в очередь	
	int               Add(RequestInfo *request, int isStopout = FALSE);
	int				  Add(const UserInfo *user, const ConGroup *group, const ConSymbol *symbol,
									 const TradeRecord *pending, TradeRecord *trade);
	//int				OrdersAdd(int login, int side, const char* smb, 
	//	double price, int volume)
	// подтверждение запроса
	int 			  ConfirmPendingRequest(RequestInfo *request);
	//
	int 			  ProcessStopApply(TradeRecord *trade,
						const ConGroup *group, const int isTP);
	void			  OnTradeHistoryRecord(TradeRecord *trade);
	static int		  GetUserInfo(int user_id, UserInfo *us);
	int		  IsGroupEnabled(const char *group);
private:
	static UINT __stdcall ThreadFunction(LPVOID param);
	// обработчик запросов
	void              Process(void);
	int 			  ProcessSingleRequest(Request *req);	
	static void		  PrepareTradeRecord(/*TradeRecord *req, */TradeRecord *orig, double price,
						int isTP);	
	int				  ActivatePendingOrder(Response *resp);
	int				  ClosePositionStopout(Response *resp, Request *req);
public:
	static int		  OnFXIResponse(Response *resp);
private:
	// FXI
	UdpSender			*m_sender;
	OrderRequestQueue	m_orderRequests;
	char				m_sendHost[128];
	int					m_sendPort;	
	int					m_receivePort;
public:
	// Обработка стопаута
	int					StopoutsFilter(const ConGroup *group);
	int					StopoutsApply(const UserInfo *user, const ConGroup *group, const char *comment);
};
//+------------------------------------------------------------------+
extern CProcessor ExtProcessor;

extern void FormatRequestType(int reqType, char *outStr);
extern void FormatCommand(int cmd, char *outStr);
