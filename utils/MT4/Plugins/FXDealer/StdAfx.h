//+------------------------------------------------------------------+
//|                                        MetaTrader Virtual Dealer |
//|                 Copyright © 2001-2008, MetaQuotes Software Corp. |
//|                                         http://www.metaquotes.ru |
//+------------------------------------------------------------------+
#pragma once

#define WIN32_LEAN_AND_MEAN      // Exclude rarely-used stuff from Windows headers

#define _USE_32BIT_TIME_T
#include <windows.h>
#include <time.h>
#include <stdio.h>
#include <stdlib.h>
#include <process.h>
#include <math.h>
#include <winsock2.h>
//----
#include "..\include\MT4ServerAPI.h"
//----
#include "common/common.h"
#include "config/stringfile.h"
#include "config/configuration.h"

//---- macros
#define TERMINATE_STR(str) str[sizeof(str)-1]=0;
#define COPY_STR(dst,src) { strncpy(dst,src,sizeof(dst)-1); dst[sizeof(dst)-1]=0; }

#define PROGRAM_TITLE "FX Virtual Dealer R"

//#define DISABLE_UDP
//+------------------------------------------------------------------+
