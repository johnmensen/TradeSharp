#pragma once
#include <time.h>
#include <windows.h>
#include <stdio.h>
#include "CharString.h"

#define QUOTE_ARRAY_LENGTH 100

struct QuoteData
{
public:
	char symbol[16];
	double ask, bid;
	time_t time;

	void Copy(QuoteData *quote);
	int FromString(CharString *str);
};

class QuoteSyncQueue
{
private:
	CRITICAL_SECTION cs;
	QuoteData *quotes;
	volatile int length;

public:
	QuoteSyncQueue();
	~QuoteSyncQueue();
	// положить в стек count котировок, thread-safe
	// _quotes не очищается
	void Enqueue(QuoteData *_quotes, int count);
	// заполнить массив _quotes котировками (не более count)
	QuoteData *Dequeue(int *count);
};