//+------------------------------------------------------------------+
//|                                                  Trade Collector |
//|                 Copyright © 2005-2008, MetaQuotes Software Corp. |
//|                                        http://www.metaquotes.net |
//+------------------------------------------------------------------+
#pragma once
//+------------------------------------------------------------------+
//| Functions                                                        |
//+------------------------------------------------------------------+
int               GetIntParam(LPCSTR string,LPCSTR param,int *data);
int               GetStrParam(LPCSTR string,LPCSTR param,char *buf,int len);
void              ClearLF(char *line);
double            CalcToDouble(const int price,int floats);
double __fastcall NormalizeDouble(const double val,int digits);
LPCSTR            GetCmd(const int cmd);
double            GetDecimalPow(const int digits);
char*             insert(void *base,const void *elem,size_t num,const size_t width,int(__cdecl *compare)( const void *elem1,const void *elem2 ));
//+------------------------------------------------------------------+
