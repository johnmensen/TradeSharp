//+------------------------------------------------------------------+
//|                                        MetaTrader Virtual Dealer |
//|                 Copyright © 2001-2008, MetaQuotes Software Corp. |
//|                                        http://www.metaquotes.net |
//+------------------------------------------------------------------+
#include "stdafx.h"
#include "Configuration.h"
#include "stringfile.h"

CConfiguration ExtConfig;
//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
CConfiguration::CConfiguration() : m_cfg(NULL),m_cfg_total(0),m_cfg_max(0)
  {
   m_filename[0]=0;
  }
//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
CConfiguration::~CConfiguration()
  {
   m_sync.Lock();
//----
   if(m_cfg!=NULL) { delete[] m_cfg; m_cfg=NULL; }
   m_cfg_total=m_cfg_max=0;
//----
   m_sync.Unlock();
  }
//+------------------------------------------------------------------+
//| Мы только читаем конфигурацию, но ничего не меняем в ней         |
//+------------------------------------------------------------------+
void CConfiguration::Load(LPCSTR filename)
  {
   char        tmp[MAX_PATH],*cp,*start;
   CStringFile in;
   PluginCfg   cfg,*buf;
//---- проверка
   if(filename==NULL) return;
//---- сохраним имя конфигурационного файла
   m_sync.Lock();
   COPY_STR(m_filename,filename);
//---- откроем файл
   if(in.Open(m_filename,GENERIC_READ,OPEN_EXISTING))
     {
      while(in.GetNextLine(tmp,sizeof(tmp)-1)>0)
        {
         if(tmp[0]==';') continue;
         //---- пропустим пробелы
         start=tmp; while(*start==' ') start++;
         if((cp=strchr(start,'='))==NULL) continue;
         *cp=0;
         //---- занулим, берем имя параметра
         ZeroMemory(&cfg,sizeof(cfg));
         COPY_STR(cfg.name,start);
         //---- опять пропустим пробелы
         cp++; while(*cp==' ') cp++;
         COPY_STR(cfg.value,cp);
         //---- проверяем
         if(cfg.name[0]==0 || cfg.value[0]==0) continue;
         //---- добавляем
         if(m_cfg==NULL || m_cfg_total>=m_cfg_max) // место есть?
           {
            //---- перевыделим новый буфер
            if((buf=new PluginCfg[m_cfg_total+64])==NULL) break;
            //---- скопируем остатки из старого
            if(m_cfg!=NULL)
              {
               if(m_cfg_total>0) memcpy(buf,m_cfg,sizeof(PluginCfg)*m_cfg_total);
               delete[] m_cfg;
              }
            //---- подменим буфер
            m_cfg    =buf;
            m_cfg_max=m_cfg_total+64;
           }
         //---- копируем
         memcpy(&m_cfg[m_cfg_total++],&cfg,sizeof(PluginCfg));
        }
      //---- закрываемся
      in.Close();
     }
//---- теперь возьмём и отсортируем по имени (чтобы искать быстро)
   if(m_cfg!=NULL && m_cfg_total>0) qsort(m_cfg,m_cfg_total,sizeof(PluginCfg),SortByName);
   m_sync.Unlock();
  }
//+------------------------------------------------------------------+
//| Сброс конфигов на диск                                           |
//+------------------------------------------------------------------+
void CConfiguration::Save(void)
  {
   CStringFile out;
   char        tmp[512];
//---- запишем все на диск
   m_sync.Lock();
   if(m_filename[0]!=0)
     if(out.Open(m_filename,GENERIC_WRITE,CREATE_ALWAYS))
       {
        if(m_cfg!=NULL)
          for(int i=0;i<m_cfg_total;i++)
            {
             _snprintf(tmp,sizeof(tmp)-1,"%s=%s\n",m_cfg[i].name,m_cfg[i].value);
             if(out.Write(tmp,strlen(tmp))<1) break;
            }
        //---- закроем файл
        out.Close();
       }
   m_sync.Unlock();
//----
  }
//+------------------------------------------------------------------+
//| Неблокируемый поиск по имени                                     |
//+------------------------------------------------------------------+
PluginCfg* CConfiguration::Search(LPCSTR name)
  {
   PluginCfg *config=NULL;
//----
   if(m_cfg!=NULL && m_cfg_total>0)
     config=(PluginCfg*)bsearch(name,m_cfg,m_cfg_total,sizeof(PluginCfg),SearchByName);
//----
   return(config);
  }
//+------------------------------------------------------------------+
//| Добавление/модификация плагина                                   |
//+------------------------------------------------------------------+
int CConfiguration::Add(const PluginCfg *cfg)
  {
   PluginCfg *config,*buf;
//---- проверки
   if(cfg==NULL || cfg->name[0]==0) return(FALSE);
//----
   m_sync.Lock();
   if((config=Search(cfg->name))!=NULL) memcpy(config,cfg,sizeof(PluginCfg));
   else
     {
      //---- место есть?
      if(m_cfg==NULL || m_cfg_total>=m_cfg_max)
        {
         //---- выделим место
         if((buf=new PluginCfg[m_cfg_total+64])==NULL) { m_sync.Unlock(); return(FALSE); }
         //---- скопируем остатки из старого буфера
         if(m_cfg!=NULL)
           {
            if(m_cfg_total>0) memcpy(buf,m_cfg,sizeof(PluginCfg)*m_cfg_total);
            delete[] m_cfg;
           }
         //---- заменим буфер
         m_cfg    =buf;
         m_cfg_max=m_cfg_total+64;
        }
      //---- добавляем в конец
      memcpy(&m_cfg[m_cfg_total++],cfg,sizeof(PluginCfg));
      //---- отсортируемся
      qsort(m_cfg,m_cfg_total,sizeof(PluginCfg),SortByName);
     }
   m_sync.Unlock();
//---- сохранимся, перезачитаемся
   Save();
//---- выходим
   return(TRUE);
  }
//+------------------------------------------------------------------+
//| Выставляем набор настроек                                        |
//+------------------------------------------------------------------+
int CConfiguration::Set(const PluginCfg *values,const int total)
  {
//---- проверки
   if(total<0) return(FALSE);
//----
   m_sync.Lock();
   if(values!=NULL && total>0)
     {
      //---- место есть?
      if(m_cfg==NULL || total>=m_cfg_max)
        {
         //---- удалим старый и выделим новый буфер
         if(m_cfg!=NULL) delete[] m_cfg;
         if((m_cfg=new PluginCfg[total+64])==NULL)
           {
            m_cfg_max=m_cfg_total=0;
            m_sync.Unlock();
            return(FALSE);
           }
         //---- выставим новый предел
         m_cfg_max=total+64;
        }
      //---- скопируем всем скопом
      memcpy(m_cfg,values,sizeof(PluginCfg)*total);
     }
//---- выставим общее количество и отсортируемся
   m_cfg_total=total;
   if(m_cfg!=NULL && m_cfg_total>0) qsort(m_cfg,m_cfg_total,sizeof(PluginCfg),SortByName);
   m_sync.Unlock();
//---- сохранимся
   Save();
   return(TRUE);
  }
//+------------------------------------------------------------------+
//| Ищем конфиг по имени                                             |
//+------------------------------------------------------------------+
int CConfiguration::Get(LPCSTR name,PluginCfg *cfg)
  {
   PluginCfg *config=NULL;
//---- проверки
   if(name!=NULL && cfg!=NULL)
     {
      m_sync.Lock();
      if((config=Search(name))!=NULL) memcpy(cfg,config,sizeof(PluginCfg));
      m_sync.Unlock();
     }
//---- вернем результат
   return(config!=NULL);
  }
//+------------------------------------------------------------------+
//| Ищем конфиг                                                      |
//+------------------------------------------------------------------+
int CConfiguration::Next(const int index,PluginCfg *cfg)
  {
//---- проверки
   if(cfg!=NULL && index>=0)
     {
      m_sync.Lock();
      if(m_cfg!=NULL && index<m_cfg_total)
        {
         memcpy(cfg,&m_cfg[index],sizeof(PluginCfg));
         m_sync.Unlock();
         return(TRUE);
        }
      m_sync.Unlock();
     }
//---- неудача
   return(FALSE);
  }
//+------------------------------------------------------------------+
//| Удаляем конфиг                                                   |
//+------------------------------------------------------------------+
int CConfiguration::Delete(LPCSTR name)
  {
   PluginCfg *config=NULL;
//---- проверки
   if(name!=NULL)
     {
      m_sync.Lock();
      if((config=Search(name))!=NULL)
        {
         int index=config-m_cfg;
         if((index+1)<m_cfg_total) memmove(config,config+1,sizeof(PluginCfg)*(m_cfg_total-index-1));
         m_cfg_total--;
        }
      //---- отсортируемся
      if(m_cfg!=NULL && m_cfg_total>0) qsort(m_cfg,m_cfg_total,sizeof(PluginCfg),SortByName);
      m_sync.Unlock();
     }
//---- вернем результат
   return(config!=NULL);
  }
//+------------------------------------------------------------------+
//| Сортировка по имени                                              |
//+------------------------------------------------------------------+
int CConfiguration::SortByName(const void *left,const void *right)
  {
   return strcmp( ((PluginCfg*)left)->name,((PluginCfg*)right)->name );
  }
//+------------------------------------------------------------------+
//| Поиск по имени                                                   |
//+------------------------------------------------------------------+
int CConfiguration::SearchByName(const void *left,const void *right)
  {
   return strcmp( (char*)left, ((PluginCfg*)right)->name);
  }
//+------------------------------------------------------------------+
//| Ищем конфиг по имени                                             |
//+------------------------------------------------------------------+
int CConfiguration::GetInteger(LPCSTR name,int *value,LPCSTR defvalue)
  {
   PluginCfg *config=NULL;
//---- проверки
   if(name!=NULL && value!=NULL)
     {
      m_sync.Lock();
      if((config=Search(name))!=NULL) *value=atoi(config->value);
      else
         if(defvalue!=NULL)
           {
            m_sync.Unlock();
            //---- создадим новую запись
            PluginCfg cfg={0};
            COPY_STR(cfg.name,name);
            COPY_STR(cfg.value,defvalue);
            Add(&cfg);
            //---- выставим значение по умолчанию и вернемся
            *value=atoi(cfg.value);
            return(TRUE);
           }
      m_sync.Unlock();
     }
//---- вернем результат
   return(config!=NULL);
  }
//+------------------------------------------------------------------+
//| Ищем конфиг по имени                                             |
//+------------------------------------------------------------------+
int CConfiguration::GetString(LPCSTR name,LPTSTR value,const int maxlen,LPCSTR defvalue)
  {
   PluginCfg *config=NULL;
//---- проверки
   if(name!=NULL && value!=NULL)
     {
      m_sync.Lock();
      if((config=Search(name))!=NULL)
        {
         strncpy(value,config->value,maxlen);
         value[maxlen]=0;
        }
      else
         if(defvalue!=NULL)
           {
            m_sync.Unlock();
            //---- создадим новую запись
            PluginCfg cfg={0};
            COPY_STR(cfg.name,name);
            COPY_STR(cfg.value,defvalue);
            Add(&cfg);
            //---- выставим значение по умолчанию и вернемся
            strncpy(value,cfg.value,maxlen);
            value[maxlen]=0;
            return(TRUE);
           }
      m_sync.Unlock();
     }
//---- вернем результат
   return(config!=NULL);
  }
//+------------------------------------------------------------------+
