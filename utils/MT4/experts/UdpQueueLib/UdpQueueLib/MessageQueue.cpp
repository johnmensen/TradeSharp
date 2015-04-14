#include "stdafx.h"
#include "MessageQueue.h"
#include "CharString.h"

MessageQueue *extQueue;

ON_RESPONSE	MessageQueue::OnResponse;

MessageQueue::MessageQueue(ON_RESPONSE _onResp)
{
	InitializeCriticalSection(&locker);
	listener = 0;
	length = 0;
	OnResponse = _onResp;
}

MessageQueue::~MessageQueue()
{
	DeleteCriticalSection(&locker);
	if (listener)
	{
		listener->Stop();
		delete listener;
	}
}

/* освободить в очереди count мест */
void MessageQueue::Free(int count)
{
	if (count > QUOTES_MAXLEN)
		count = QUOTES_MAXLEN;
	int room = QUOTES_MAXLEN - length;
	if (room >= count) 
		return;
	int nMove = count - room;
	for (int i = 0; i < nMove; i++)
		buffer[i] = buffer[i + 1];
	length -= count;
}

void MessageQueue::Enqueue(CharString *resp)
{	
	EnterCriticalSection(&locker);

	if (length == QUOTES_MAXLEN)
		Free(20);
	buffer[length] = (*resp);
	length++;

	LeaveCriticalSection(&locker);
}

bool MessageQueue::Dequeue(CharString* resp)
{	
	if (length == 0)
		return false;

	EnterCriticalSection(&locker);
	
	*resp = buffer[length - 1];	
	length--;

	LeaveCriticalSection(&locker);
	return true;			
}

void MessageQueue::Init(const char *host, int port)
{
	if (listener)
	{
		listener->Stop();
		delete listener;
	}
	listener = new UdpListener(host, port, OnReceive);
	listener->Listen();
}

void MessageQueue::OnReceive(const TCHAR *buf, int len)
{
	if (len == 0)
		return;
	CharString str(buf, len);
	if (OnResponse != 0)
		if (OnResponse(&str)) return;
	extQueue->Enqueue(&str);	
}