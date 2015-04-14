// stdafx.h : include file for standard system include files,
// or project specific include files that are used frequently, but
// are changed infrequently
//

#pragma once

#define WIN32_LEAN_AND_MEAN      // Exclude rarely-used stuff from Windows headers

#include <windows.h>
#include <time.h>
#include <stdio.h>
#include <stdlib.h>
#include <process.h>
#include <math.h>
#include <winsock2.h>
#include <tchar.h>

#define _USE_32BIT_TIME_T
//---- macros
#define TERMINATE_STR(str) str[sizeof(str)-1]=0;
#define COPY_STR(dst,src) { strncpy(dst,src,sizeof(dst)-1); dst[sizeof(dst)-1]=0; }

//#define DISABLE_UDP
//+------------------------------------------------------------------+