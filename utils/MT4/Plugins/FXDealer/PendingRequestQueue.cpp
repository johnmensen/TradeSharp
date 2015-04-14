#include "stdafx.h"
#include "PendingRequestQueue.h"
#include "CharString.h"

PendingRequestQueue extPendingRequestQueue;

PendingRequestQueue::PendingRequestQueue()
{
	length = 0;
	InitializeCriticalSection(&locker);
}

PendingRequestQueue::~PendingRequestQueue()
{
	DeleteCriticalSection(&locker);
}

void PendingRequestQueue::AddRequest(int reqId, int login, int cmd, char *symbol, 
							int lots, double price)
{
	PendingRequest r = { 0 };
	r.requestId = reqId;
	r.login = login;
	r.cmd = cmd;
	strcpy(r.symbol, symbol);
	r.price = price;
	r.lots = lots;
	r.time = ::time(NULL);

	if (length == PENDING_REQUEST_MAXLEN) Free(30);
	EnterCriticalSection(&locker);
	
	buffer[length].Copy(&r);
	length++;

	LeaveCriticalSection(&locker);

	/* KICKME */
	CharString str;
	str.Printf(FALSE, "Add Pending Request: id=%d,login=%d,cmd=%d,symbol=%s,lots=%d,price=%g", 
		reqId, login, cmd, symbol, lots, price);
	ExtServer->LogsOut(CmdOK, PROGRAM_TITLE, str.ToArray());
}

int PendingRequestQueue::FindRequest(int login, int cmd, char *symbol, 
							int lots, double price, PendingRequest *req)
{
	if (!length) return FALSE;
	EnterCriticalSection(&locker);

	for (int i = 0; i < length; i++)
	{
		if (buffer[i].login == login &&
			buffer[i].cmd == cmd && 
			strcmp(buffer[i].symbol, symbol) == 0 &&
			buffer[i].lots == lots/* &&	buffer[i].price == price*/)
		{			
			req->Copy(&(buffer[i]));
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

void PendingRequestQueue::Free(int count)
{
	EnterCriticalSection(&locker);
	
	if (count > PENDING_REQUEST_MAXLEN) count = PENDING_REQUEST_MAXLEN;
	int room = PENDING_REQUEST_MAXLEN - length;
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
	length = PENDING_REQUEST_MAXLEN - room;
		
	LeaveCriticalSection(&locker);
}