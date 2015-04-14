#pragma once
#include "UdpListener.h"
#include "CharString.h"

#define QUOTES_MAXLEN 256

// resp - указатель на добавляемый response
// TRUE - response обработан, в очередь не добавляется
typedef int (*ON_RESPONSE)(CharString *resp);
//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
class MessageQueue
{
private:	
	UdpListener			*listener;
	CRITICAL_SECTION	locker;
	CharString			buffer[QUOTES_MAXLEN];
	volatile int		length;
	static ON_RESPONSE	OnResponse;
private:
	void				Free(int count);
	void				Enqueue(CharString *resp);	
	static void			OnReceive(const TCHAR *buf, int len);
public:
	MessageQueue(ON_RESPONSE _onResp);
	~MessageQueue();

	void				Init(const char *host, int port);	
	bool				Dequeue(CharString* resp);
};

extern MessageQueue	*extQueue;