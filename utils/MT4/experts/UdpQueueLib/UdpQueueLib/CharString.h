#pragma once

#include <string.h>
#include <stdlib.h>
#include <stdarg.h>

class CharString
{
private:
	TCHAR *data;
	int length;

public:
	// конструкторы - деструктор, установка длины
	CharString()
	{
		data = 0;
		length = 0;
	}
	CharString(const TCHAR *str);
	CharString(CharString *str);
	CharString(const TCHAR *str, int len);
	CharString(int val, int radix);
	CharString(double val, int numDigits);
	~CharString();
	inline int GetLength() { return length; }
	void SetLength(int len);	
	inline TCHAR* ToArray() { return data; }	
	TCHAR *StrCpy(TCHAR *destBuffer, int bufLen);
	CharString *SplitByChar(TCHAR c, int *itemsCount);
	int ToTime(struct tm *tm, TCHAR *format);
	int ToInt(int defaultValue);
	double ToDouble(double defValue);
	void CopyToBuffer(TCHAR *buffer, int bufLen);
	int FindPos(CharString *substr);
	int FindPos(const TCHAR *substr);

	// перегрузка операторов
	CharString& operator=(const CharString &str);
	CharString& operator=(TCHAR *str);
	int operator ==(const CharString &other) const;	
	int operator ==(const TCHAR *other) const;	
	int operator !=(const CharString &other) const;	
	CharString& operator+=(const CharString &str);
	CharString& operator+=(const TCHAR *str);
	const CharString operator+(const CharString &other) const;
	operator TCHAR*() { return data; }	

	static bool AnsiToUnicode16(const CHAR *in_Src, WCHAR *out_Dst, int in_MaxLen);
	static CHAR *Unicode16ToAnsi(const WCHAR *src);
};


class Path
{
public:
	static CharString *ChangeFileExt(CharString *str, TCHAR *newExt);
};