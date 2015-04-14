#include "stdafx.h"
#include "CharString.h"

CharString::CharString(const char *str)
{
	if (str == 0)
	{
		data = 0;
		length = 0;
		return;
	}
	length = strlen(str);
	data = new char[length + 1];
	strcpy(data, str);
}

CharString::CharString(const char *str, int len)
{
	if (str == 0)
	{
		data = 0;
		length = 0;
		return;
	}

	length = len;
	data = new char[length + 1];
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

	length = strlen(str->data);
	data = new char[length + 1];
	strcpy(data, str->data);
}

CharString::CharString(int val, int radix)
{
	char *buf = new char[30];
	ltoa(val, buf, radix);	
	length = strlen(buf);
	data = new char[length + 1];
	strcpy(data, buf);
	delete buf;	
}

CharString::CharString(double val, int numDigits)
{
	char *buf = new char[40];
	gcvt(val, numDigits, buf);		
	length = strlen(buf);
	data = new char[length + 1];
	strcpy(data, buf);
	delete buf;	
}

void CharString::SetLength(int len) 
{ 
	if (len == length) return;
	length = len; 
	if (data != 0) delete data;
	if (length == 0) return;
	data = new char[length + 1];
	data[0] = 0;
}

CharString& CharString::operator=(const CharString &str)
{
	if (this == &str) return *this;
	if (str.data == 0)
	{
		data = 0;
		length = 0;
		return *this;
	}

	length = strlen(str.data);
	data = new char[length + 1];
	strcpy(data, str.data);
	return *this;
}

CharString& CharString::operator=(char *str)
{
	if (str == 0)
	{
		data = 0;
		length = 0;
		return *this;
	}

	length = strlen(str);
	data = new char[length + 1];
	strcpy(data, str);
	return *this;
}

int CharString::operator==(const CharString &other) const 
{
	if (length != other.length) return 0;
	if (length == 0 && other.length == 0) return 1;
	return strcmp(data, other.data) == 0;
}

int CharString::FindPos(CharString *substr)
{
	if (length == 0 || substr == 0) return -1;
	if (substr->length == 0) return -1;
	char *pos = strstr(data, substr->data);
	return pos == 0 ? -1 : (pos - data) / sizeof(char);
}

int CharString::FindPos(const char *substr)
{	
	if (length == 0 || substr == 0) return -1;	
	char *pos = strstr(data, substr);
	return pos == 0 ? -1 : (pos - data) / sizeof(char);
}

int CharString::operator!=(const CharString &other) const 
{
	if (length != other.length) return 1;
	if (length == 0 && other.length == 0) return 0;
	return strcmp(data, other.data) != 0;
}

CharString& CharString::operator+=(const CharString &str) 
{
	if (str.length == 0) return *this;
	length += str.length;
	char *newData = new char[length + 1];
	if (data != 0)	
	{
		strcpy(newData, data);
		delete data;
	}
	strcat(newData, str.data);
	data = newData;
	return *this;
}

CharString& CharString::operator+=(const char *str)
{
	if (str == 0) return *this;
	int len = strlen(str);
	if (len == 0) return *this;

	length += len;
	char *newData = new char[length + 1];
	if (data != 0)	
	{
		strcpy(newData, data);
		delete data;
	}
	strcat(newData, str);
	data = newData;
	return *this;
}

const CharString CharString::operator+(const CharString &other) const 
{
    CharString result = *this;
    result += other;
    return result;
}

// str.Printf("The string is %s and the int is %d", "a string", 105);
void CharString::Printf(char *format, ... )
{
	char buf[512];
	
	va_list vl;
	int i, bufIndex = 0;
	va_start(vl, format);
	
	for (i = 0; format[i] != '\0'; ++i)
	{
		if (format[i] != '%')
		{
			buf[bufIndex++] = format[i];
			continue;
		}
		i++;
		buf[bufIndex] = 0;
		if (format[i] == 's')
		{
			char* sPtr = va_arg(vl, char *);
			strcat(buf, sPtr);
			bufIndex += strlen(sPtr);
			continue;
		}
		if (format[i] == 'd')
		{
			int nPtr = va_arg(vl, int);
			char strInt[30];
			ltoa(nPtr, strInt, 10);
			strcat(buf, strInt);
			bufIndex += strlen(strInt);
			continue;
		}
    }
    
	va_end( vl );

	buf[bufIndex] = 0;
	length = strlen(buf);
	if (data != 0) delete data;
	data = new char[length + 1];
	strcpy(data, buf);		
}

char* CharString::StrCpy(char *destBuffer, int bufLen)
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

CharString *CharString::SplitByChar(char c, int *itemsCount)
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
	char buffer[512];
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
int CharString::ToTime(struct tm *tm, char *format)
{		
	// получить из строки формата набор форматных символов и разделителей
	// пример: %Y/%M/%D %h:%m:%s
	// symbols = { Y, M, D, h, m, s }
	// delimiters = { /, /, ' ', :, :, 0x0 }
	
	int startIndex = strchr(format, '%') - format;
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
			: strstr(data + startIndex, items[i].data + 1) - data;
		int bufferIndex = 0;
		for (; startIndex < delimiterIndex; startIndex++)
		{
			buffer[bufferIndex++] = data[startIndex];
		}
		startIndex = delimiterIndex + items[i].length - 1;
		buffer[bufferIndex] = 0;
		int partValue = atoi(buffer);
		
		if (items[i].data[0] == 'Y') tm->tm_year = partValue;
		if (items[i].data[0] == 'M') tm->tm_mon = partValue - 1;
		if (items[i].data[0] == 'D') tm->tm_mday = partValue;
		if (items[i].data[0] == 'h') tm->tm_hour = partValue;
		if (items[i].data[0] == 'm') tm->tm_min = partValue;
		if (items[i].data[0] == 's') tm->tm_sec = partValue;
		
		if (startIndex >= length) break;
	}		

	return 0;
}