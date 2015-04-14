//+------------------------------------------------------------------+
//|                                                        UniFeeder |
//|                 Copyright © 2001-2008, MetaQuotes Software Corp. |
//|                                         http://www.metaquotes.ru |
//+------------------------------------------------------------------+
#pragma once
//----
#define WIN32_LEAN_AND_MEAN      // Exclude rarely-used stuff from Windows headers
#define _USE_32BIT_TIME_T
//----
#include <stdlib.h>
#include <stdio.h>
#include <io.h>
#include <winsock2.h>
#include <time.h>

#include "sync.h"
#include "logger.h"
#include "FeedInterface.h"
#include "bases/SynteticBase.h"

#define COPY_STR(dest,src) strncpy(dest,src,sizeof(dest)-1); dest[sizeof(dest)-1]=0;
#define COUNTOF(arr) (sizeof(arr)/sizeof(arr[0]))
//+------------------------------------------------------------------+
