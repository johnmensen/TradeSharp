#include "stdafx.h"
#include "QuoteSyncQueue.h"
#include <time.h> 

// EURUSD,1.5213,1.5215,2001/11/12 18:31:01
int QuoteData::FromString(CharString *str)
{
	int count;
	CharString *parts = str->SplitByChar(',', &count);
	if (count != 4) 
	{
		if (count != 0) delete parts;
		return (FALSE);
	}
	parts[0].StrCpy(symbol, 16);
	bid = atof(parts[1].ToArray());
	ask = atof(parts[2].ToArray());
	struct tm tm;
	int correct = parts[3].ToTime(&tm, "%Y/%M/%D %h:%m:%s");
	if (correct < 0) return (FALSE);
	time = mktime(&tm);
	return TRUE;
}

void QuoteData::Copy(QuoteData *quote)
{
	strcpy(symbol, quote->symbol);
	ask = quote->ask;
	bid = quote->bid;
	time = quote->time;
}

QuoteSyncQueue::QuoteSyncQueue()
{
	InitializeCriticalSection(&cs);
	quotes = (QuoteData*) malloc (sizeof(QuoteData) * QUOTE_ARRAY_LENGTH);
	length = 0;
}

QuoteSyncQueue::~QuoteSyncQueue()
{
	DeleteCriticalSection(&cs);
	free(quotes);
}

void QuoteSyncQueue::Enqueue(QuoteData *_quotes, int count)
{	
	EnterCriticalSection(&cs);
	if ((length + count) > QUOTE_ARRAY_LENGTH) 
		count = QUOTE_ARRAY_LENGTH - length;
	if (count <= 0) 
	{
		LeaveCriticalSection(&cs);
		return;
	}
	
	for (int i = 0; i < count; i++)
	{
		quotes[i + length].Copy(&(_quotes[i]));
	}
	length += count;
	LeaveCriticalSection(&cs);
}

QuoteData *QuoteSyncQueue::Dequeue(int *count)
{
	EnterCriticalSection(&cs);
	QuoteData *_quotes;
	if (length == 0 || *count == 0) 
	{
		LeaveCriticalSection(&cs);
		*count = 0;
		return 0;
	}
	if (*count > length) *count = length;
	int startIndex = length - (*count);
	_quotes = (QuoteData*) malloc(sizeof(QuoteData) * (*count));

	for (int i = 0; i < (*count); i++)
	{
		_quotes[i].Copy(&(quotes[startIndex + i]));
	}
	length -= (*count);
	LeaveCriticalSection(&cs);
	return _quotes;
}

