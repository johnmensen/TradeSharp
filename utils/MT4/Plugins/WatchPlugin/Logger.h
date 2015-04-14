//+------------------------------------------------------------------+
//|                                                   MT4 Connector  |
//|                 Copyright © 2005-2008, MetaQuotes Software Corp. |
//|                                        http://www.metaquotes.net |
//+------------------------------------------------------------------+
#pragma once
//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
class CLogger
  {
private:
   CSync             m_sync;
   FILE             *m_file;
   char             *m_prebuf;
   char              m_logname[64];

public:
                     CLogger(void);
                    ~CLogger(void);

   void              Initialize(const char *logname) { m_sync.Lock(); if(logname!=NULL) COPY_STR(m_logname,logname); m_sync.Unlock(); }
   void              Out(const int code,LPCSTR ip,LPCSTR msg,...);
   int               Journal(LPSTR value,const int max_len);
  };

extern CLogger ExtLogger;
extern char    ExtProgramPath[200];
extern char    ExtProgramFile[200];
//+------------------------------------------------------------------+
