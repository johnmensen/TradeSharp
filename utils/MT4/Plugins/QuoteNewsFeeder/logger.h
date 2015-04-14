//+------------------------------------------------------------------+
//|                                            Logger for Data Feeds |
//|                 Copyright © 2001-2008, MetaQuotes Software Corp. |
//|                                         http://www.metaquotes.ru |
//+------------------------------------------------------------------+
#pragma once
//+------------------------------------------------------------------+
//| Логгер-рекомендуется использовать для фидеров                    |
//|                                                                  |
//| Класс создан для эффективной работы с частым выводов логов.      |
//| Преимущества:                                                    |
//|-не делаем лишних перевыделений памяти                            |
//|-собирает короткие строки в отдельный буфер(m_colbuf) и выводит   |
//| их блоками, что серьезно уменьшает фрагментацию файла            |
//|-автоматически подрезает слишком длинные файлы, сохраняя          |
//| последние 100 Кб                                                 |
//+------------------------------------------------------------------+
class CLogger
  {
private:
   CSync             m_sync;         // synchronization
   FILE             *m_file;         // file
   char             *m_prebuf;       // buffer for parsing
   char             *m_colbuf;       // buffer for collecting
   int               m_collen;       // size of collected data

public:
                     CLogger(void);
                    ~CLogger(void);
   void              Shutdown(void);
   //---- logging helpers
   void              Out(LPCSTR msg,...);
   int               Journal(char *buffer);
   void              Cut(void);

private:
   void              FinalizeDay(void);
   void              Add(LPCSTR msg);
  };
//---- global object
extern CLogger ExtLogger;
extern char    ExtProgramPath[200];
//+------------------------------------------------------------------+
