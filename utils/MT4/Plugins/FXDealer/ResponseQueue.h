#pragma once
#include "UDP\UdpListener.h"

#define QUOTES_MAXLEN 256

//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
struct Response
{
	int			requestId;			// id запроса
	int			status;				// см. ниже
	double		price;				// цена исполнения
	double		sl;
	double		tp;
	char		comment[32];		// id позиции, сохраняемый в коментарии
	char		isPendingActivate;	// ответ на запрос активации отложенного ордера
	int			pendingOrderId;		// id активируемого отложенного ордера
	char		isStopoutReply;		// признак стопаута

	void		Copy(Response *resp)
	{
		requestId = resp->requestId; 
		status = resp->status; 
		price = resp->price;
		sl = resp->sl; 
		tp = resp->tp;
		isPendingActivate = resp->isPendingActivate;
		pendingOrderId = resp->pendingOrderId;
		strcpy(comment, resp->comment);
		isStopoutReply = resp->isStopoutReply;
	}
};
// результат исполнения запроса
enum 
{ 
	RequestCompleted = 10, 
	RequestFailed = 20, 
	RequestWrongParams = 30, 
	RequestWrongPrice = 40,
	RequestToClose = 50,
	OrderSLExecuted = 60,
	OrderTPExecuted = 70
};

// resp - указатель на добавляемый response
// TRUE - response обработан, в очередь не добавляется
typedef int (*ON_RESPONSE)(Response *resp);
//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
class ResponseQueue
{
private:	
	UdpListener			*listener;
	CRITICAL_SECTION	locker;
	Response			buffer[QUOTES_MAXLEN];
	volatile int		length;
	static ON_RESPONSE	OnResponse;
private:
	void				Free(int count);
	void				Enqueue(Response *resp);	
	static void			OnReceive(char *buf, int len);
public:
	ResponseQueue(ON_RESPONSE _onResp);
	~ResponseQueue();

	void				Init(char *host, int port);	
	int					FindAndDequeue(int requestId, Response* resp);
};

extern ResponseQueue	*extQueue;