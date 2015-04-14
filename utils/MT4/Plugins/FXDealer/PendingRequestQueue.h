#pragma once

#define PENDING_REQUEST_MAXLEN 500

struct PendingRequest
{
	int		requestId;
	
	int		login;
	int		cmd;
	char	symbol[16];
	int		lots;
	double	price;
	time_t	time;

	void	Copy(PendingRequest *r)
	{
		requestId	= r->requestId;
		login		= r->login;
		cmd			= r->cmd;
		strcpy(symbol, r->symbol);
		lots		= r->lots;
		price		= r->price;
		time		= r->time;
	}
};

class PendingRequestQueue
{
private:
	CRITICAL_SECTION	locker;
	PendingRequest		buffer[PENDING_REQUEST_MAXLEN];
	volatile int		length;
	
	void		Free(int count);

public:
	PendingRequestQueue();	
	~PendingRequestQueue();
	void		AddRequest(int reqId, int login, int cmd, char *symbol, 
							int lots, double price);
	int			FindRequest(int logon, int cmd, char *symbol, 
							int lots, double price, PendingRequest *req);
};

extern PendingRequestQueue extPendingRequestQueue;