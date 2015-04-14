//+------------------------------------------------------------------+
//|                                            Logger for Data Feeds |
//|                 Copyright © 2001-2008, MetaQuotes Software Corp. |
//|                                         http://www.metaquotes.ru |
//+------------------------------------------------------------------+
#include "stdafx.h"
#include "SourceInterface.h"
//#include <io.h>
//----
#define PREBUF_SIZE    16384
#define COLBUF_SIZE    65536

CLogger ExtLogger;
char    ExtProgramPath[200]="";

long GetFileLength(FILE *stream)
{
	fseek( stream, 0L, SEEK_END );
    long endPos = ftell( stream );
	fseek( stream, 0L, SEEK_SET );
	return endPos;
}

//+------------------------------------------------------------------+
//| Constructor                                                      |
//+------------------------------------------------------------------+
CLogger::CLogger(void):m_file(NULL),m_collen(0)
  {
//---- buffers
   m_prebuf=(char*)malloc(PREBUF_SIZE);
   m_colbuf=(char*)malloc(COLBUF_SIZE);
//----
  }
//+------------------------------------------------------------------+
//| Destructor                                                       |
//+------------------------------------------------------------------+
CLogger::~CLogger(void) { Shutdown(); }
//+------------------------------------------------------------------+
//| Закрываем логгер полностью                                       |
//+------------------------------------------------------------------+
void CLogger::Shutdown(void)
  {
   m_sync.Lock();
//----
   FinalizeDay();
//----
   if(m_prebuf!=NULL)  { free(m_prebuf); m_prebuf=NULL;  }
   if(m_colbuf!=NULL)  { free(m_colbuf); m_colbuf=NULL;  }
//----
   m_sync.Unlock();
  }
//+------------------------------------------------------------------+
//| Output logs into the file                                        |
//+------------------------------------------------------------------+
void CLogger::Out(LPCSTR msg,...)
  {
   SYSTEMTIME st;
   va_list    arg_ptr;
   char       tmp[256];
//---- checks and time parsing
   if(msg==NULL || m_prebuf==NULL || m_colbuf==NULL) return;
//---- lets add time
   GetSystemTime(&st);
   va_start(arg_ptr, msg);
   _snprintf(tmp,sizeof(tmp)-1,"%04d/%02d/%02d %02d:%02d:%02d ",st.wYear,st.wDay,st.wMonth,st.wHour,st.wMinute,st.wSecond);
//----
   m_sync.Lock();
   Add(tmp);
//---- parse message
   vsprintf(m_prebuf,msg,arg_ptr);
   va_end(arg_ptr);
   strcat(m_prebuf,"\r\n");
   Add(m_prebuf);
//----
   m_sync.Unlock();
  }
//+------------------------------------------------------------------+
//| Добавляем строку в буфер                                         |
//+------------------------------------------------------------------+
void CLogger::Add(LPCSTR msg)
  {
   char tmp[256];
   int  len;
//---- проверки на размер сообщений
   if(m_colbuf==NULL || msg==NULL) return;
   len=strlen(msg);
   if(len<1 || len>COLBUF_SIZE)    return;
//---- места не хватает?
   if((len+m_collen)>=COLBUF_SIZE)
     {
      if(m_collen>0)  // есть что записывать?
        {
         //---- откроем файл если надо
         if(m_file==NULL)
           {
            _snprintf(tmp,sizeof(tmp)-1,"%s.log",ExtProgramPath);
            if((m_file=fopen(tmp,"ab"))==NULL) return;
           }
         //---- пробуем записаться
         fwrite(m_colbuf,m_collen,1,m_file);
         m_collen=0;
        }
     }
//---- ну а теперь пробуем добавиться
   memcpy(m_colbuf+m_collen,msg,len);
   m_collen+=len;
//----
  }
//+------------------------------------------------------------------+
//| Сброс и закрытие логов за предыдущий день                        |
//+------------------------------------------------------------------+
void CLogger::FinalizeDay(void)
  {
   char tmp[256];
//---- нужно ли вообще что-то сбрасывать?
   if(m_collen<1)
     {
      if(m_file!=NULL) { fclose(m_file); m_file=NULL; }
      return;
     }
//---- а файл за предыдущий день открыт?
   if(m_file==NULL)
     {
      _snprintf(tmp,sizeof(tmp)-1,"%s.log",ExtProgramPath);
      m_file=fopen(tmp,"ab");
     }
//---- попробуем сбросить буфер на диск
   if(m_file!=NULL)
     {
      //---- сбросим предыдущий буфер
      if(m_colbuf!=NULL && m_collen>0) fwrite(m_colbuf,m_collen,1,m_file);
      //---- закроем файл
      fclose(m_file);
      m_file=NULL;
     }
//---- в любом случае сбросим счетчик данных в буфере
   m_collen=0;
  }

//+------------------------------------------------------------------+
//| Cut the file                                                     |
//+------------------------------------------------------------------+
void CLogger::Cut(void)
  {
   FILE *in,*out;
   char *buf,*cp,tmp[256];
   int   len;
//---- закроем файл на всякий случай
   m_sync.Lock();
   FinalizeDay();
//---- откроем файл заново, проверим размер
   _snprintf(tmp,sizeof(tmp)-1,"%s.log",ExtProgramPath);
   if((in=fopen(tmp,"rb"))==NULL)        {             m_sync.Unlock(); return; }
   len=GetFileLength(in);
   if(len<300000)                        { fclose(in); m_sync.Unlock(); return; }
//---- allocate 100 Kb for last messages and read them
   if((buf=(char*)malloc(102401))==NULL) { fclose(in); m_sync.Unlock(); return; }
   fseek(in,len-102400,SEEK_SET);
   fread(buf,102400,1,in); buf[102400]=0;
   fclose(in);
//---- reopen log-file and write last messages
   if((out=fopen(tmp,"wb"))!=NULL)
     {
      if((cp=strstr(buf,"\n"))!=NULL) fwrite(cp+1,strlen(cp+1),1,out);
      fclose(out);
     }
   free(buf);
//----
   m_sync.Unlock();
  }
//+------------------------------------------------------------------+
//| Flushing file & get last 8 Kb                                    |
//+------------------------------------------------------------------+
int CLogger::Journal(char *buffer)
  {
   char *cp,tmp[256];
   int   len,clen;
//---- check
   if(buffer==NULL) return(0);
   m_sync.Lock();
//---- flush file
   FinalizeDay();
//---- reopen file
   _snprintf(tmp,sizeof(tmp)-1,"%s.log",ExtProgramPath);
   if((m_file=fopen(tmp,"rb"))==NULL)       { m_sync.Unlock(); return(0); }
//---- check the size of file
   if((len=GetFileLength(m_file))<1) { fclose(m_file); m_file=NULL; m_sync.Unlock(); return(0); }
//---- read last 8 Kb messages
   fseek(m_file,len-1024*8,SEEK_SET);
   len=(len<1024*8)?len:1024*8; // correct length
   if(fread(buffer,len,1,m_file)!=1)        { fclose(m_file); m_file=NULL; m_sync.Unlock(); return(0); }
   fclose(m_file);
   m_file=NULL; 
   buffer[len]=0; // null-terminator
//---- find \n from start of buffer
   if((cp=strstr(buffer,"\n"))!=NULL)
     {
      clen=strlen(cp+1);
      memmove(buffer,cp+1,clen); len=clen;
      *(buffer+len)=0;
     }
//---- find \n from end of buffer
   cp=buffer+len-1;
   while(cp>buffer && *cp!=0x0D) { cp--; len--; }
   *cp=0; len--;// terminate last string
//---- return buffer length
   m_sync.Unlock();
   return(len);
  }
//+------------------------------------------------------------------+
