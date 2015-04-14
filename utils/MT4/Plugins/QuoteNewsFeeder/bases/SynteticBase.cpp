//+------------------------------------------------------------------+
//|                                        UniFeeder Syntetic Quotes |
//|                 Copyright © 2001-2005, MetaQuotes Software Corp. |
//|                                         http://www.metaquotes.ru |
//+------------------------------------------------------------------+
#include "stdafx.h"

//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
CSynteticBase::CSynteticBase():m_syntetics(NULL),m_syntetics_total(0),m_syntetics_max(0),
                               m_ticks_total(0),m_ticks_current(0)
  {
//---- зачистим буфер котировок
   memset(m_ticks,0,sizeof(m_ticks));
//----
  }
//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
CSynteticBase::~CSynteticBase()
  {
//---- освободим буферы
   if(m_syntetics!=NULL) { free(m_syntetics); m_syntetics=NULL; }
   m_syntetics_total=m_syntetics_max=0;
   m_ticks_total=m_ticks_current=0;
//----
  }
//+------------------------------------------------------------------+
//| Загрузка и парсинг синтетических инструментов                    |
//+------------------------------------------------------------------+
void CSynteticBase::Load()
  {
   FILE             *fp;
   SynteticSymbol    sym;
   char              tmp[2050],*cp,*cp2;
   int               i;
//---- удалим старье
   if(m_syntetics!=NULL) { free(m_syntetics); m_syntetics=NULL; }
   m_syntetics_total=m_syntetics_max=0;
   m_ticks_total=m_ticks_current=0;
//---- откроем файл описаний
   _snprintf(tmp,sizeof(tmp)-1,"%s.dat",ExtProgramPath);
   if((fp=fopen(tmp,"rt"))==NULL) return;
//---- читаем построчно
   while(fgets(tmp,sizeof(tmp)-1,fp)!=0)
     {
      //---- пропуск комментарий и зачистка строки
      if(tmp[0]==0 || tmp[0]==';')   continue;
      if((cp=strstr(tmp,";"))!=NULL)  *cp=0; // комментарий сзади строки
      if((cp=strstr(tmp,"\n"))!=NULL) *cp=0; // конец строки
      //---- зачистим инфу
      memset(&sym,0,sizeof(sym));
      //---- выделим символ
      if((cp=strstr(tmp,"="))==NULL) continue;
      *cp=0;
      COPY_STR(sym.name,tmp);
      //---- выделим левый операнд
      cp++;
      cp2=sym.left.symbol;
      sym.left.type=OPERAND_VALUE; // по-умолчанию считаем что это числовой операнд
      //---- безопасно анализируем его содержимое
      i=0;
      while(*cp!=0 && strchr("+-*/",*cp)==NULL && i<sizeof(sym.left.symbol))
        {
         //---- по ходу дела определяем тип операнда
         if(strchr("01234567890.",*cp)==NULL) sym.left.type=OPERAND_SYMBOL;
         //---- просто копируем символ
         *cp2++=*cp++;
         i++;
        }
      *cp2=0;
      //---- если операнд оказался числовым пропишем ему значение в value
      if(sym.left.type==OPERAND_VALUE)
        {
         sym.left.value=atof(sym.left.symbol);
         sym.left.symbol[0]=0;
        }
      //---- проверим операцию
      if(*cp==0 || strchr("+-*/",*cp)==NULL)   continue;
      sym.operation=*cp;
      //---- выделим правый операнд
      cp++;
      cp2=sym.right.symbol;
      sym.right.type=OPERAND_VALUE; // по-умолчанию считаем что это числовой операнд
      //---- безопасно анализируем его содержимое
      i=0;
      while(*cp!=0 && strchr("+-*/",*cp)==NULL && i<sizeof(sym.right.symbol))
        {
         //---- по ходу дела определяем тип операнда
         if(strchr("01234567890.",*cp)==NULL) sym.right.type=OPERAND_SYMBOL;
         //---- просто копируем символ
         *cp2++=*cp++;
         i++;
        }
      *cp2=0;
      //---- если операнд оказался числовым пропишем ему значение в value
      if(sym.right.type==OPERAND_VALUE)
        {
         sym.right.value=atof(sym.right.symbol);
         sym.right.symbol[0]=0;
        }
      //---- если операция деление и правый операнд нулевой
      //---- то выкидываем эту запись сразу
      if(sym.operation=='/' && sym.right.type==OPERAND_VALUE && sym.right.value==0) continue;
      //---- добавим инфу в таблицу
      AddSynteticSymbol(&sym);
     }
//---- все считали, закроем файл
   fclose(fp);
  }
//+------------------------------------------------------------------+
//| Добавление синтетического символа                                |
//| Если такой уже есть, считаем последний верным (психология)       |
//+------------------------------------------------------------------+
void CSynteticBase::AddSynteticSymbol(const SynteticSymbol *sym)
  {
   SynteticSymbol   *buf;
   int               i;
//---- проверка
   if(sym==NULL) return;
//---- прежде определим: а есть ли этот символ в базе?
   if(m_syntetics!=NULL && m_syntetics_total>0)
     {
      for(i=0;i<m_syntetics_total;i++)
        if(strcmp(sym->name,m_syntetics[i].name)==0) break;
      //---- если нашли, то просто перезапишем инфу и выйдем
      if(i<m_syntetics_total)
        {
         memcpy(&m_syntetics[i],sym,sizeof(m_syntetics[i]));
         return;
        }
     }
//---- нужно перевыделение памяти?
   if(m_syntetics_total+1>m_syntetics_max)
     {
      //---- выделим память под новый буфер
      if((buf=(SynteticSymbol*)malloc((m_syntetics_total+32)*sizeof(SynteticSymbol)))==NULL) return;
      //---- если есть что копировать сделаем это
      //---- также освободим старый буфер
      if(m_syntetics!=NULL && m_syntetics_total>0)
        {
         memcpy(buf,m_syntetics,m_syntetics_total*sizeof(SynteticSymbol));
         free(m_syntetics);
        }
      //---- назначим новый буфер и размерность
      m_syntetics=buf;
      m_syntetics_max=m_syntetics_total+32;
     }
//---- добавим новую запись
   memcpy(&m_syntetics[m_syntetics_total++],sym,sizeof(SynteticSymbol));
  }
//+------------------------------------------------------------------+
//| Добавление свежей реальной котировки                             |
//+------------------------------------------------------------------+
void CSynteticBase::AddQuotes(const FeedData *data)
  {
   int               i,changed;
   SynteticSymbol   *cs;
//---- проверки
   if(data==NULL || m_syntetics==NULL) return;
//---- бежим по списку принятых котировок
   for(int j=0;j<data->ticks_count;j++)
     {
      FeedTick *quote=(FeedTick*)&data->ticks[j];
      //---- пробежимся по базе синтетиков с целью поиска операндов
      //---- указывающих на текущий символ
      for(i=0;i<m_syntetics_total;i++)
        {
         cs=&m_syntetics[i];
         changed=FALSE;
         //---- проверка левого операнда
         if(cs->left.type==OPERAND_SYMBOL && strcmp(cs->left.symbol,quote->symbol)==0)
           {
            //---- выставим новые бид/аск
            cs->left.ask=quote->ask;
            cs->left.bid=quote->bid;
            //---- выставим флаг изменения
            changed=TRUE;
           }
         //---- проверка правого операнда
         if(cs->right.type==OPERAND_SYMBOL && strcmp(cs->right.symbol,quote->symbol)==0)
           {
            //---- выставим новые бид/аск
            cs->right.ask=quote->ask;
            cs->right.bid=quote->bid;
            //---- выставим флаг изменения
            changed=TRUE;
           }
         //---- изменилось что-то?
         if(changed!=FALSE) RecalculateSymbol(cs);
        }
     }
//----
  }
//+------------------------------------------------------------------+
//| Перерасчет значения синтетического символа и добавление (по необ-|
//| ходимости) котировки в буфер ожидающих котировок                 |
//+------------------------------------------------------------------+
void CSynteticBase::RecalculateSymbol(const SynteticSymbol *cs)
  {
   double   ask,bid,lvalue,rvalue;
//---- проверка
   if(cs==NULL) return;
//---- определим ask
   if(cs->left.type==OPERAND_SYMBOL)
         lvalue=cs->left.ask;
   else  lvalue=cs->left.value;
   if(cs->right.type==OPERAND_SYMBOL)
         rvalue=cs->operation=='/'?cs->right.bid:cs->right.ask;
   else  rvalue=cs->right.value;
//---- вычислим
   if(lvalue==0 || rvalue==0) return;
   ask=CalculateOpeation(cs->operation,lvalue,rvalue);
//---- определим bid
   if(cs->left.type==OPERAND_SYMBOL)
         lvalue=cs->left.bid;
   else  lvalue=cs->left.value;
   if(cs->right.type==OPERAND_SYMBOL)
         rvalue=cs->operation=='/'?cs->right.ask:cs->right.bid;
   else  rvalue=cs->right.value;
//---- вычислим
   if(lvalue==0 || rvalue==0) return;
   bid=CalculateOpeation(cs->operation,lvalue,rvalue);
//---- проверим новую синтетическую котировку
   if(bid==0 || ask==0 || bid>ask) return;
//---- добавим котировку
   if(m_ticks_total<MAX_TICKS)
     {
      COPY_STR(m_ticks[m_ticks_total].security,cs->name);
      m_ticks[m_ticks_total].ask=ask;
      m_ticks[m_ticks_total].bid=bid;
      //---- увеличим счетчик котировок
      m_ticks_total++;
      //---- рекурсия для более сложных формул
      FeedData data={0};
      COPY_STR(data.ticks->symbol,cs->name);
      data.ticks->ask=ask;
      data.ticks->bid=bid;
      data.ticks_count=1;
      AddQuotes(&data);
     }
//----
  }
//+------------------------------------------------------------------+
//| Получение следующей котировки из буфера, если нет FALSE          |
//+------------------------------------------------------------------+
int CSynteticBase::GetTicks(FeedData *data)
  {
//---- проверка
   if(data==NULL) return(FALSE);
   data->ticks_count=0;
//---- проверим, есть ли в буфере котировки?
   if(m_ticks_total>0)
     {
      if(m_ticks_current<m_ticks_total)
        {
         for(data->ticks_count=0;data->ticks_count<32;)
           {
            TickInfo   *ti=&m_ticks[m_ticks_current];
            FeedTick   *ft=&data->ticks[data->ticks_count];
            //---- копируем информацию
            COPY_STR(ft->symbol,ti->security);
            ft->bid=ti->bid;
            ft->ask=ti->ask;
            ft->ctm=ti->time;
            //---- наращиваем и проверяем счетчик
            data->ticks_count++;
            m_ticks_current++;
            if(m_ticks_current>=m_ticks_total) break;
           }
         return(TRUE);
        }
      else m_ticks_total=0;
     }
//---- нет ничего
   m_ticks_current=0; m_ticks_total=0;
   return(FALSE);
  }
//+------------------------------------------------------------------+
//| Выполнение операции с операндами и получение рузультата          |
//+------------------------------------------------------------------+
double CSynteticBase::CalculateOpeation(char op,double left,double right) const
  {
   double res=0;
//---- какая операция?
   switch(op)
     {
      case '+': res=left+right;              break;
      case '-': res=left-right;              break;
      case '*': res=left*right;              break;
      case '/': if(right!=0) res=left/right; break;
     }
//---- вернем результат
   return(res);
  }
//+------------------------------------------------------------------+
