#include "stdafx.h"
#include "OrderRequestQueue.h"


#include "stdafx.h"
#include "ResponseQueue.h"
#include "CharString.h"

OrderRequestQueue::OrderRequestQueue()
{
	InitializeCriticalSection(&locker);
	length = 0;
}

OrderRequestQueue::~OrderRequestQueue()
{
	DeleteCriticalSection(&locker);
}

/* освободить в очереди count мест */
void OrderRequestQueue::Free(int count)
{
	EnterCriticalSection(&locker);
	
	if (count > ORDER_REQUEST_MAXLENGTH) count = ORDER_REQUEST_MAXLENGTH;
	int room = ORDER_REQUEST_MAXLENGTH - length;
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
	length = ORDER_REQUEST_MAXLENGTH - room;
		
	LeaveCriticalSection(&locker);
}

void OrderRequestQueue::Enqueue(SLTPOrderRequest *resp)
{	
	if (length == ORDER_REQUEST_MAXLENGTH) Free(20);
	EnterCriticalSection(&locker);
	
	buffer[length].Copy(resp);
	length++;

	LeaveCriticalSection(&locker);
}

int	OrderRequestQueue::Dequeue(int orderId, SLTPOrderRequest *request)
{	
	if (!length) return FALSE;
	EnterCriticalSection(&locker);

	for (int i = 0; i < length; i++)
	{
		if (buffer[i].orderId == orderId)
		{
			request->Copy(&(buffer[i]));
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