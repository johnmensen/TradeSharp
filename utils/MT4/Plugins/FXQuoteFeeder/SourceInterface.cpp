#include "stdafx.h"
#include "SourceInterface.h"
#include "CharString.h"

#define BUFFER_SIZE 128*1024
#define COUNTOF(arr) (sizeof(arr)/sizeof(arr[0]))

QuoteSyncQueue	*CSourceInterface::queue;
NewsSyncQueue	*CSourceInterface::news;
//+------------------------------------------------------------------+
//| Constructor                                                      |
//+------------------------------------------------------------------+
CSourceInterface::CSourceInterface(void) : m_symbols(NULL)
{
	listener = 0;
	queue = new QuoteSyncQueue();
	news = new NewsSyncQueue();
}
//+------------------------------------------------------------------+
//| Destructor                                                       |
//+------------------------------------------------------------------+
CSourceInterface::~CSourceInterface(void)
{
	if (m_symbols != NULL) { free(m_symbols); m_symbols = NULL; }
	delete queue;
	delete news;
	if (listener)
	{
		listener->Stop();
		delete listener;
		listener = 0;
	}

	m_next = NULL;
}
//+------------------------------------------------------------------+
//| Connecting to server                                             |
//+------------------------------------------------------------------+
int CSourceInterface::Connect(LPCSTR server,LPCSTR login,LPCSTR password)
{
	int port = 19000;	
	ExtLogger.Out("CSourceInterface::Connect()...");
	CharString str(server);
	int count;
	CharString* parts = str.SplitByChar(':', &count);
	if (count == 2)
		port = atoi(parts[1].ToArray());

	listener = new UdpListener("127.0.0.1", port, OnReceive);
	listener->Listen();

	ExtLogger.Out("CSourceInterface::Connect() - OK");

	return(TRUE);
}
//+------------------------------------------------------------------+
//| Closing connection                                               |
//+------------------------------------------------------------------+
void CSourceInterface::Close(void)
{
	if (listener)
	{
		listener->Stop();
		delete listener;
		listener = 0;
	}
}
//+------------------------------------------------------------------+
//| Set prefered instruments                                         |
//+------------------------------------------------------------------+
void CSourceInterface::SetSymbols(LPCSTR symbols)
{
	if(m_symbols != NULL) { free(m_symbols); m_symbols=NULL; }
	
	if(symbols != NULL)
	{
		m_symbols = (char*)malloc(strlen(symbols) + 10);
		if (m_symbols != NULL) strcpy(m_symbols, symbols);
		CharString str;
		str.Printf("Symbols: %s", symbols);
		ExtLogger.Out(str.ToArray());
	}
}
//+------------------------------------------------------------------+
//| Main data reading function                                       |
//+------------------------------------------------------------------+
int CSourceInterface::Read(FeedData *inf)
{
	if (inf == NULL) return (FALSE);	
	
	try
	{
		inf->ticks_count = 0;
		
		QuoteData *quotes;
		int count = 32;
		quotes = queue->Dequeue(&count);
		if (count == 0) return (TRUE);

		int realCount = COUNTOF(inf->ticks);
		ExtLogger.Out("There are %d ticks. Read %d quotes", realCount, count);		

		for (int i = 0; i < count; i++)
		{
			inf->ticks[i].ask		= quotes[i].ask;
			inf->ticks[i].bid		= quotes[i].bid;
			strcpy(inf->ticks[i].symbol, quotes[i].symbol);
			inf->ticks[i].ctm		= quotes[i].time;		
			ExtLogger.Out("Quote: %s ask=%g bid=%g", quotes[i].symbol, quotes[i].ask, quotes[i].bid);
		}		
		inf->ticks_count = count;
		free(quotes);		
	}
	catch (...)
	{
		ExtLogger.Out("Exception in CSourceInterface::Read()");
	}	

	return(TRUE);
}
//+------------------------------------------------------------------+
//| Получить котировку, сложить ее в массив                          |
//+------------------------------------------------------------------+
// формат:
// symbol,bid,ask,date;... (date is %Y/%m/%d %H:%M:%S)
// пример:
// EURUSD,1.5213,1.5215,2001/11/12 18:31:01;GBPUSD,1.7103,1.7105,2001 11 12 18:31:04;
void CSourceInterface::OnReceive(char *buf, int len)
{
	if (len <= 0) return;	
	ExtLogger.Out("CSourceInterface::OnReceive ...");
	CharString str(buf, len);	

	ExtLogger.Out("CSourceInterface::OnReceive(%s, %d)", str, len);
	
	if (str.FindPos("< News") >= 0)
	{// разобрать новостную строку
		NewsQueueItem item;
		if (NewsQueueItem::FromString(buf + sizeof("< News\r\n"),  
			len - sizeof("< News\r\n"), &item))
			news->Enqueue(&item, 1);
		return;
	}
	
	// разобрать строку с котировками
	try
	{		
		int count;
		CharString *quoteStrs = str.SplitByChar(';', &count);
		if (count == 0) return;
		QuoteData *quotes = new QuoteData[count];

		for (int i = 0; i < count; i++)
		{
			quotes[i].FromString(&(quoteStrs[i]));
			ExtLogger.Out("Got quote: %s,%g,%g", 
				quotes[i].symbol, quotes[i].ask, quotes[i].bid);
		}
		
		queue->Enqueue(quotes, count);

		if (quoteStrs) delete quoteStrs;
	}
	catch (...)
	{
		ExtLogger.Out("Exception in CSourceInterface::OnReceive()");
	}
}