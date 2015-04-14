//+------------------------------------------------------------------+
//|                                                  Trade Collector |
//|                 Copyright © 2005-2008, MetaQuotes Software Corp. |
//|                                        http://www.metaquotes.net |
//+------------------------------------------------------------------+
#pragma once
//---- exclude rarely-used stuff from Windows headers
#define WIN32_LEAN_AND_MEAN
//----
#include <windows.h>
#include <winsock2.h>
#include <time.h>
#include <io.h>
#include <stdio.h>
#include <stdlib.h>
#include <math.h>
#include <process.h>
//---- MT Server API
#include "..\include\mt4serverapi.h"
//----
#include "sync.h"
//---- macros
#define TERMINATE_STR(str) str[sizeof(str)-1]=0;
#define COPY_STR(dst,src) { strncpy(dst,src,sizeof(dst)-1); dst[sizeof(dst)-1]=0; }

#define PROGRAM_TITLE "Watch Plugin"
//+------------------------------------------------------------------+
