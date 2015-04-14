#include "stdafx.h"
#include "CharString.h"

CharString::CharString(const TCHAR *str)
{
	if (str == 0)
	{
		data = 0;
		length = 0;
		return;
	}

	length = _tcslen(str);
	data = new TCHAR[length + 1];
	_tcscpy(data, str);
}

CharString::CharString(const TCHAR *str, int len)
{
	if (str == 0)
	{
		data = 0;
		length = 0;
		return;
	}

	length = len;
	data = new TCHAR[length + 1];
	for (int i = 0; i < length; i++)
		data[i] = str[i];
	data[length] = 0;
}

CharString::CharString(CharString *str)
{
	if (str == 0)
	{
		data = 0;
		length = 0;
		return;
	}
	if (str->data == 0)
	{
		data = 0;
		length = 0;
		return;
	}

	length = _tcslen(str->data);
	data = new TCHAR[length + 1];
	_tcscpy(data, str->data);
}

CharString::CharString(int val, int radix)
{
	TCHAR *buf = new TCHAR[30];
	_ltot(val, buf, radix);	
	length = _tcslen(buf);
	data = new TCHAR[length + 1];
	_tcscpy(data, buf);
	delete[] buf;
}

CharString::CharString(double val, int numDigits)
{
	int decimal, sign;
	char *ansiBuf = _ecvt(val, numDigits, &decimal, &sign);
#ifdef UNICODE
	WCHAR buf[_CVTBUFSIZE + 1];
	if(!AnsiToUnicode16(ansiBuf, buf, _CVTBUFSIZE))
		return;
#else
	char *buf = ansiBuf;
#endif
	length = _tcslen(buf);
	data = new TCHAR[length + 1];
	_tcscpy(data, buf);
}

CharString::~CharString()
{
	if (data != 0) delete[] data;
	data = 0;
}

void CharString::SetLength(int len)
{ 
	if (len == length) return;
	length = len; 
	if (data != 0)
		delete[] data;
	data = 0;
	if (length == 0)
		return;
	data = new TCHAR[length + 1];
	data[0] = 0;
}

CharString& CharString::operator=(const CharString &str)
{
	if (this == &str) return *this;
	SetLength(0);
	if (str.data == 0)
		return *this;
	length = _tcslen(str.data);
	data = new TCHAR[length + 1];
	_tcscpy(data, str.data);
	return *this;
}

CharString& CharString::operator=(TCHAR *str)
{
	SetLength(0);
	if (str == 0)
		return *this;
	length = _tcslen(str);
	data = new TCHAR[length + 1];
	_tcscpy(data, str);
	return *this;
}

int CharString::operator==(const CharString &other) const 
{
	if (length != other.length) return 0;
	if (length == 0 && other.length == 0) return 1;
	return _tcscmp(data, other.data) == 0;
}

int CharString::operator==(const TCHAR *other) const 
{
	if (other == 0 && length == 0) return 1;
	if (other == 0) return 0;
	int len = _tcslen(other);

	if (length != len) return 0;	
	return _tcscmp(data, other) == 0;
}

int CharString::FindPos(CharString *substr)
{
	if (length == 0 || substr == 0) return -1;
	if (substr->length == 0) return -1;
	TCHAR *pos = _tcsstr(data, substr->data);
	return pos == 0 ? -1 : (pos - data) / sizeof(char);
}

int CharString::FindPos(const TCHAR *substr)
{	
	if (length == 0 || substr == 0) return -1;	
	TCHAR *pos = _tcsstr(data, substr);
	return pos == 0 ? -1 : (pos - data) / sizeof(char);
}

int CharString::operator!=(const CharString &other) const 
{
	if (length != other.length) return 1;
	if (length == 0 && other.length == 0) return 0;
	return _tcscmp(data, other.data) != 0;
}

CharString& CharString::operator+=(const CharString &str) 
{
	if (str.length == 0) return *this;
	length += str.length;
	TCHAR *newData = new TCHAR[length + 1];
	if (data != 0)	
	{
		_tcscpy(newData, data);
		delete[] data;
	}
	_tcscat(newData, str.data);
	data = newData;
	return *this;
}

CharString& CharString::operator+=(const TCHAR *str)
{
	if (str == 0) return *this;
	int len = _tcslen(str);
	if (len == 0) return *this;

	length += len;
	TCHAR *newData = new TCHAR[length + 1];
	if (data != 0)	
	{
		_tcscpy(newData, data);
		delete[] data;
	}
	_tcscat(newData, str);
	data = newData;
	return *this;
}

const CharString CharString::operator+(const CharString &other) const 
{
    CharString result = *this;
    result += other;
    return result;
}

void CharString::CopyToBuffer(TCHAR *buffer, int bufLen)
{
	if (length == 0 && bufLen > 0) 
	{
		buffer[0] = 0;
		return;
	}

	if (bufLen == 0) return;
	
	int len = length;
	if (bufLen <= len) len = bufLen - 1;	

	for (int i = 0; i < len; i++)
		buffer[i] = data[i];

	buffer[len] = 0;
}

TCHAR *CharString::StrCpy(TCHAR *destBuffer, int bufLen)
{	
	if (bufLen == 0) return destBuffer;
	int len = length < bufLen ? length : bufLen - 1;
	for (int i = 0; i < len; i++)
	{
		destBuffer[i] = data[i];
	}
	destBuffer[len] = 0;
	return destBuffer;
}

CharString *CharString::SplitByChar(TCHAR c, int *itemsCount)
{
	*itemsCount = 0;
	if (length == 0) return 0;
	int count = 0;
	int prevIndex = 0;
	for (int i = 0; i < length - 1; i++)
	{
		if (data[i] == c)
		{
			if (i - prevIndex > 0) count++;
			prevIndex = i;
		}
	}
	if (data[length - 1] != c && prevIndex < (length - 2)) 
		count++;
	if (count == 0) return 0;

	CharString *items = new CharString[count];
	TCHAR buffer[2048 * 10];
	int index = 0, bufferIndex = 0;
	
	for (int i = 0; i < length; i++)
	{
		if (data[i] == c)
		{
			if (bufferIndex > 0)
			{
				buffer[bufferIndex] = 0;
				items[index++] = buffer;
				bufferIndex = 0;
			}
			continue;
		}
		buffer[bufferIndex++] = data[i];
	}
	if (bufferIndex > 0)
	{
		buffer[bufferIndex] = 0;
		items[index++] = buffer;
		bufferIndex = 0;
	}
	*itemsCount = index;
	return items;
}

// format: %Y - год %M - месяц %D - день %h - час %m - минута %s - секунда
// пример: CharString("2001/11/12 18:31:01").ToTime(&tm, "%Y/%M/%D %h:%m:%s");
int CharString::ToTime(struct tm *tm, TCHAR *format)
{		
	// получить из строки формата набор форматных символов и разделителей
	// пример: %Y/%M/%D %h:%m:%s
	// symbols = { Y, M, D, h, m, s }
	// delimiters = { /, /, ' ', :, :, 0x0 }
	
	int startIndex = _tcschr(format, '%') - format;
	if (startIndex < 0) return -1;
	int count;
	CharString *items = CharString(format).SplitByChar('%', &count);
	if (count == 0) return -1;

	char buffer[25];	
	for (int i = 0; i < count; i++)
	{
		// копировать с текущего индекса вплоть до делимитера в буфер
		int delimiterIndex = items[i].length == 1 
			? length 
			: _tcsstr(data + startIndex, items[i].data + 1) - data;
		int bufferIndex = 0;
		for (; startIndex < delimiterIndex; startIndex++)
		{
			buffer[bufferIndex++] = data[startIndex];
		}
		startIndex = delimiterIndex + items[i].length - 1;
		buffer[bufferIndex] = 0;
		int partValue = atoi(buffer);
		
		if (items[i].data[0] == 'Y') tm->tm_year = partValue;
		if (items[i].data[0] == 'M') tm->tm_mon = partValue;
		if (items[i].data[0] == 'D') tm->tm_mday = partValue;
		if (items[i].data[0] == 'h') tm->tm_hour = partValue;
		if (items[i].data[0] == 'm') tm->tm_min = partValue;
		if (items[i].data[0] == 's') tm->tm_sec = partValue;
		
		if (startIndex >= length) break;
	}		

	return 0;
}

int CharString::ToInt(int defaultValue)
{
	if (length == 0) return defaultValue;
	return _tstol(data);
}

double CharString::ToDouble(double defValue)
{
	if (length == 0) return defValue;
	return _tstof(data);
}

// copied from MSDN
// static
bool CharString::AnsiToUnicode16(const CHAR *in_Src, WCHAR *out_Dst, int in_MaxLen)
{
    int lv_Len;
	if (in_MaxLen <= 0)
		return false;
	// let windows find out the meaning of ansi
	// - the SrcLen=-1 triggers MBTWC to add a eos to Dst and fails if MaxLen is too small.
	// - if SrcLen is specified then no eos is added
	// - if (SrcLen+1) is specified then the eos IS added
	lv_Len = MultiByteToWideChar(CP_ACP, 0, in_Src, -1, out_Dst, in_MaxLen);
	// validate
	if (lv_Len < 0)
		lv_Len = 0;
	// ensure eos, watch out for a full buffersize
	// - if the buffer is full without an eos then clear the output like MBTWC does
	//   in case of too small outputbuffer
	// - unfortunately there is no way to let MBTWC return shortened strings,
	//   if the outputbuffer is too small then it fails completely
	if (lv_Len < in_MaxLen)
		out_Dst[lv_Len] = 0;
	else if (out_Dst[in_MaxLen-1])
		out_Dst[0] = 0;
	return true;
}

// static
CHAR *CharString::Unicode16ToAnsi(const WCHAR *src)
{
	int len = WideCharToMultiByte(CP_ACP, 0, src, -1, NULL, 0, NULL, NULL);
	char *result = new char[len + 1];
	WideCharToMultiByte(CP_ACP, 0, src, -1, result, len, NULL, NULL);
	return result;
}

// static
CharString *Path::ChangeFileExt(CharString *str, TCHAR *newExt)
{
	int len = str->GetLength();
	if (len == 0) return new CharString();
	
	TCHAR *data = str->ToArray();	
	for (int i = len - 1; i >= 0; i--)
	{
		if (data[i] == '.')
		{
			len = i + _tcslen(newExt) + 1;
			TCHAR *newData = new TCHAR[len + 1];
			for (int j = 0; j <= i; j++)
				newData[j] = data[j];
			newData[i + 1] = 0;
			_tcscat(newData, newExt);
			CharString *strNew = new CharString(newData);
			delete[] newData;
			return strNew;
		}
	}
	TCHAR *newData = new TCHAR[len + _tcslen(newExt) + 2];
	_tcscpy(newData, data);
	_tcscat(newData, _T("."));
	_tcscat(newData, newExt);
	CharString *strNew = new CharString(newData);
	delete[] newData;
	return strNew;
}