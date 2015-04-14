#include "stdafx.h"
#include "ResponseQueue.h"
#include "CharString.h"

ResponseQueue	*extQueue;

ON_RESPONSE	ResponseQueue::OnResponse;

ResponseQueue::ResponseQueue(ON_RESPONSE _onResp)
{
	InitializeCriticalSection(&locker);
	listener = 0;
	length = 0;
	OnResponse = _onResp;
}

ResponseQueue::~ResponseQueue()
{
	DeleteCriticalSection(&locker);
	if (listener)
	{
		listener->Stop();
		delete listener;
	}
}

/* освободить в очереди count мест */
void ResponseQueue::Free(int count)
{
	EnterCriticalSection(&locker);
	
	if (count > QUOTES_MAXLEN) count = QUOTES_MAXLEN;
	int room = QUOTES_MAXLEN - length;
	if (room >= count) 
	{
		LeaveCriticalSection(&locker);
		return;
	}

	int nMove = count - room;
	for (int i = 0; i < nMove; i++)
	{
		buffer[i].Copy(&(buffer[i + 1]));
	}
	length = QUOTES_MAXLEN - room;
	
	
	LeaveCriticalSection(&locker);
}

void ResponseQueue::Enqueue(Response *resp)
{
	CharString str;
	str.Printf(FALSE, "ResponseQueue::Enqueue(id=%d, status=%d, price=%g)", resp->requestId, resp->status, resp->price);
	ExtServer->LogsOut(CmdOK, PROGRAM_TITLE, str.ToArray());
	if (length == QUOTES_MAXLEN) Free(20);
	EnterCriticalSection(&locker);
	
	buffer[length].Copy(resp);
	length++;

	LeaveCriticalSection(&locker);
}

int ResponseQueue::FindAndDequeue(int requestId, Response* resp)
{	
	if (!length) return FALSE;
	EnterCriticalSection(&locker);

	for (int i = 0; i < length; i++)
	{
		if (buffer[i].requestId == requestId)
		{			
			resp->Copy(&(buffer[i]));
			// удалить запрос !! (memcpy)
			for (int j = i; j < (length - 1); j++)
			{ buffer[j].Copy(&(buffer[j + 1])); }
			length--;

			LeaveCriticalSection(&locker);
			return TRUE;
		}
	}

	LeaveCriticalSection(&locker);
	return FALSE;
}

void ResponseQueue::Init(char *host, int port)
{
	if (listener)
	{
		listener->Stop();
		delete listener;
	}
	listener = new UdpListener(host, port, OnReceive);
	listener->Listen();
}

void ResponseQueue::OnReceive(char *buf, int len)
{
	if (len == 0) return;

	CharString msg;
	msg.Printf(FALSE, "OnReceive:\"%s\"", buf);
	ExtServer->LogsOut(CmdOK, PROGRAM_TITLE, msg);
	
	Response resp = { 0 };
	resp.requestId = 0;

	// requestId=1;price=1.455;status=10;sl=0;tp=0;
	CharString str(buf);
	int itemsCount;
	CharString *items = 0;
	itemsCount = str.SplitByChar(';', &items);
	for (int i = 0; i < itemsCount; i++)
	{
		int subCount;
		CharString *subItems;
		subCount = items[i].SplitByChar('=', &subItems);
		if (subCount == 2)
		{
			if (subItems[0] == "requestId") 
			{ resp.requestId = subItems[1].ToInt(0); delete[] subItems; continue; }
			if (subItems[0] == "price") 
			{ resp.price = subItems[1].ToDouble(0); delete[] subItems; continue; }
			if (subItems[0] == "status") 
			{ resp.status = subItems[1].ToInt(0); delete[] subItems; continue; }
			if (subItems[0] == "sl") 
			{ resp.sl = subItems[1].ToDouble(0); delete[] subItems; continue; }
			if (subItems[0] == "tp") 
			{ resp.tp = subItems[1].ToDouble(0); delete[] subItems; continue; }
			if (subItems[0] == "comment") 
			{ strcpy(resp.comment, subItems[1]); delete[] subItems; continue; }
			if (subItems[0] == "pending") 
			{ resp.isPendingActivate = subItems[1].ToInt(0); delete[] subItems; continue; }
			if (subItems[0] == "pendingOrderId") 
			{ resp.pendingOrderId = subItems[1].ToInt(0); delete[] subItems; continue; }
			if (subItems[0] == "stopout") 
			{ resp.isStopoutReply = subItems[1].ToInt(0); delete[] subItems; continue; }
		}
		if (subCount > 0) delete[] subItems;
	}
	if (items) delete[] items;
	if (resp.requestId != 0)	
	{
		if (!OnResponse(&resp))
			extQueue->Enqueue(&resp);
	}
}