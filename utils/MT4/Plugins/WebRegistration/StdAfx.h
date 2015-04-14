//+------------------------------------------------------------------+
//|                                       MetaTrader WebRegistration |
//|                 Copyright © 2001-2008, MetaQuotes Software Corp. |
//|                                        http://www.metaquotes.net |
//+------------------------------------------------------------------+
#pragma once
//----
#define WIN32_LEAN_AND_MEAN
#define _USE_32BIT_TIME_T
//---- Standart headers
#include <windows.h>
#include <time.h>
#include <stdio.h>
#include <stdlib.h>
#include <process.h>
#include <math.h>
#include <io.h>
#include <sys/stat.h>
#include <winsock2.h>
//---- Server API
#include "..\include\MT4ServerAPI.h"
//---- Common headers
#include "common\common.h"
#include "config\stringfile.h"
#include "config\configuration.h"

//---- Macros for strings
#define TERMINATE_STR(str)  str[sizeof(str)-1]=0;
#define COPY_STR(dst,src) { strncpy(dst,src,sizeof(dst)-1); dst[sizeof(dst)-1]=0; }
//+------------------------------------------------------------------+
#define PROGRAM_TITLE "FX Web Registration"
