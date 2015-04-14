//+------------------------------------------------------------------+
//|                                       MetaTrader WebRegistration |
//|                 Copyright © 2001-2008, MetaQuotes Software Corp. |
//|                                        http://www.metaquotes.net |
//+------------------------------------------------------------------+
#include "..\stdafx.h"

//+------------------------------------------------------------------+
//| Reading number parameter                                         |
//+------------------------------------------------------------------+
int GetLongParam(LPCSTR string, LPCSTR param, long *data)
{
   if (string==NULL || param==NULL || data==NULL) return(FALSE);
   if ((string=strstr(string,param))==NULL)       return(FALSE);
   *data = atol(&string[strlen(param)]);
   return TRUE;
}
//+------------------------------------------------------------------+
//| Reading number parameter                                         |
//+------------------------------------------------------------------+
int GetIntParam(LPCSTR string,LPCSTR param,int *data)
{
   if (string==NULL || param==NULL || data==NULL) return(FALSE);
   if ((string=strstr(string,param))==NULL)       return(FALSE);
   *data = atol(&string[strlen(param)]);
   return TRUE;
}

//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
int GetTimeParam(LPCSTR string, LPCSTR param, time_t *data)
{
   if (string == NULL || param == NULL || data == NULL) return FALSE;
   if ((string = strstr(string, param)) == NULL)        return FALSE;
   *data = strtoul(&string[strlen(param)], 0, 10);
   return TRUE;
}
//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
int GetFltParam(LPCSTR string,LPCSTR param,double *data)
  {
//---- проверки
   if(string==NULL || param==NULL || data==NULL) return(FALSE);
//---- найдем вхождение такого параметра
   if((string=strstr(string,param))==NULL)       return(FALSE);
//---- все хорошо парсим
   *data=atof(&string[strlen(param)]);
   return(TRUE);
  }
//+------------------------------------------------------------------+
//| Reading string parameter                                         |
//+------------------------------------------------------------------+
int GetStrParam(LPCSTR string,LPCSTR param,char *buf,const int maxlen)
  {
   int i=0;
//---- checks
   if(string==NULL || param==NULL || buf==NULL)  return(FALSE);
//---- find
   if((string=strstr(string,param))==NULL)       return(FALSE);
//---- receive result
   string+=strlen(param);
   while(*string!=0 && *string!='|' && i<maxlen) { *buf++=*string++; i++; }
   *buf=0;
//----
   return(TRUE);
  }
//+------------------------------------------------------------------+
//| Check complexity of password                                     |
//+------------------------------------------------------------------+
int CheckPassword(LPCSTR password)
  {
   char   tmp[256];
   int    len,num=0,upper=0,lower=0;
   USHORT type[256];
//----
   if(password==NULL) return(FALSE);
//---- check len
   if((len=strlen(password))<5) return(FALSE);
//---- must Upper case,lower case and digits
   strcpy(tmp,password);
   if(GetStringTypeA(LOCALE_SYSTEM_DEFAULT,CT_CTYPE1,tmp,len,(USHORT*)type))
     {
      for(int i=0;i<len;i++)
        {
         if(type[i]&C1_DIGIT)  { num=1;   continue; }
         if(type[i]&C1_UPPER)  { upper=1; continue; }
         if(type[i]&C1_LOWER)  { lower=1; continue; }
         if(!(type[i] & (C1_ALPHA | C1_DIGIT) )) { num=2; break; }
        }
     }
//---- compute complexity
   return((num+upper+lower)>=2);
  }
//+------------------------------------------------------------------+
//| Функия вставки записи в упорядоченный массив с учётом сортировки |
//| в случае наличия записи вставляемой записи возвращается 0        |
//| в массиве должно быть (num+1)*width свободного места             |
//+------------------------------------------------------------------+
char* insert(void *base,const void *elem,size_t num,const size_t width,int(__cdecl *compare)( const void *elem1,const void *elem2 ))
  {
//---- проверки
   if(base==NULL || elem==NULL || compare==NULL) return(NULL);
//---- первый элемент?
   if(num<1) { memcpy(base,elem,width); return(char*)(base); }
//---- а теперь инициализируем переменные! (не в начале!)
   register char *lo=(char *)base;
   register char *hi=(char *)base+(num-1) * width, *end=hi;
   register char *mid;
   unsigned int   half;
   int            result;
//----
   while(num>0)
     {
      half=num/2;
      mid=lo+half*width;
      //---- сравниваем
      if((result=compare(elem,mid))>0) // data[mid]<elem
        {
         lo  =mid+width;
         num =num-half-1;
        }
      else if(result<0)                // data[mid]>elem
        {
         num=half;
        }
      else                             // data[mid]==elem
        return(NULL);
     }
//---- вставляем
   memmove(lo+width,lo,end-lo+width);
   memcpy(lo,elem,width);
//----
   return(lo);
  }
//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
int	IntArrayFromString(char *str, char separator, int *outArray)
{
	if (strlen(str) == 0) return 0;
	
	int count = 0;
	char curNumber[20];
	curNumber[0] = 0;

	for (int i = 0; i < strlen(str); i++)
	{
		if (str[i] == separator)
		{
			if (strlen(curNumber) > 0)
			{
				outArray[count] = atol(curNumber);
				count++;
				curNumber[0] = 0;
				continue;
			}
		}

		if ((str[i] >= '0' && str[i] <= '9') || str[i] == '-')
		{
			int len = strlen(curNumber);
			curNumber[len] = str[i];
			curNumber[len + 1] = 0;
		}
	}
	if (strlen(curNumber) > 0)
	{
		outArray[count] = atol(curNumber);
		count++;
	}
	return count;
}
//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+