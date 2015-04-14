//+------------------------------------------------------------------+
//|                                            MetaTrader Server API |
//|                 Copyright © 2001-2008, MetaQuotes Software Corp. |
//|                                         http://www.metaquotes.ru |
//+------------------------------------------------------------------+
#pragma once

#define WIN32_LEAN_AND_MEAN      // Exclude rarely-used stuff from Windows headers

#include <windows.h>
#include <time.h>
#include <stdio.h>
#include <stdlib.h>
#include <math.h>
//----
#include "..\include\MT4ServerAPI.h"
//----
//---- macros
#define TERMINATE_STR(str) str[sizeof(str)-1]=0;
#define COPY_STR(dst,src) { strncpy(dst,src,sizeof(dst)-1); dst[sizeof(dst)-1]=0; }
//+------------------------------------------------------------------+
