//+------------------------------------------------------------------+
//|                                       MetaTrader WebRegistration |
//|                 Copyright © 2001-2008, MetaQuotes Software Corp. |
//|                                        http://www.metaquotes.net |
//+------------------------------------------------------------------+
#pragma once
//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
int		 GetLongParam(LPCSTR string, LPCSTR param, long *data);
int      GetIntParam(LPCSTR string,LPCSTR param,int *data);
int		 GetTimeParam(LPCSTR string, LPCSTR param, time_t *data);
int      GetFltParam(LPCSTR string,LPCSTR param,double *data);
int      GetStrParam(LPCSTR string,LPCSTR param,char *buf,const int maxlen);
int      CheckPassword(LPCSTR password);
char *   insert(void *base,const void *elem,size_t num,const size_t width,
                  int(__cdecl *compare)( const void *elem1,const void *elem2));
int		 IntArrayFromString(char *str, char separator, int *outArray);

//+------------------------------------------------------------------+
