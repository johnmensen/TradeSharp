#pragma once

#define ORDER_REQUEST_MAXLENGTH 512

//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
struct SLTPOrderRequest
{
	int			orderId;	
	double		price;
	int			isTP; /* FALSE - SL, TRUE - TP */	

	void		Copy(SLTPOrderRequest *request)
	{
		orderId = request->orderId;		
		price = request->price;
		isTP = request->isTP;
	}	
};
//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
class OrderRequestQueue
{
private:	
	CRITICAL_SECTION	locker;
	SLTPOrderRequest	buffer[ORDER_REQUEST_MAXLENGTH];
	volatile int		length;
private:
	void				Free(int count);	
public:
	OrderRequestQueue();
	~OrderRequestQueue();

	void				Enqueue(SLTPOrderRequest *req);			
	int					Dequeue(int orderId, SLTPOrderRequest *request);		
};

