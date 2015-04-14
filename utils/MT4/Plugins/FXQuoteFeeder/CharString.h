#pragma once

#include <string.h>
#include <stdlib.h>
#include <stdarg.h>

class CharString
{
private:
	char *data;
	int length;
public:
	// конструкторы - деструктор, установка длины
	CharString()
	{
		data = 0;
		length = 0;
	}
	CharString(const char *str);
	CharString(CharString *str);
	CharString(const char *str, int len);
	CharString(int val, int radix);
	CharString(double val, int numDigits);
	~CharString()
	{
		if (data != 0) delete data;
		data = 0;		
	}
	inline int GetLength() { return length; }
	void SetLength(int len);	
	inline char* ToArray() { return data; }	
	char* StrCpy(char *destBuffer, int bufLen);
	void Printf(char *format, ... );	
	CharString *SplitByChar(char c, int *itemsCount);
	int ToTime(struct tm *tm, char *format);
	int FindPos(CharString *substr);
	int FindPos(const char *substr);

	// перегрузка операторов
	CharString& operator=(const CharString &str);
	CharString& operator=(char *str);
	int operator ==(const CharString &other) const;	
	int operator !=(const CharString &other) const;	
	CharString& operator+=(const CharString &str);
	CharString& operator+=(const char *str);
	const CharString operator+(const CharString &other) const;
	operator char*() { return data; }	
};
