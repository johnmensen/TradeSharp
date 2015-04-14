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
	~CharString();
	inline int GetLength() { return length; }
	void SetLength(int len);	
	inline char* ToArray() { return data; }	
	char* StrCpy(char *destBuffer, int bufLen);
	void Printf(int shouldAppend, char *format, ... );	
	int SplitByChar(char c, CharString **parts);
	int ToTime(struct tm *tm, char *format);
	int ToInt(int defaultValue);
	double ToDouble(double defValue);

	// перегрузка операторов
	CharString& operator=(const CharString &str);
	CharString& operator=(char *str);
	int operator ==(const CharString &other) const;	
	int operator ==(const char *other) const;	
	int operator !=(const CharString &other) const;	
	CharString& operator+=(const CharString &str);
	CharString& operator+=(const char *str);
	const CharString operator+(const CharString &other) const;
	operator char*() { return data; }	
};


class Path
{
public:
	static CharString *ChangeFileExt(CharString *str, char *newExt);
};