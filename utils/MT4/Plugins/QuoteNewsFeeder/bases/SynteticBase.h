//+------------------------------------------------------------------+
//|                                        UniFeeder Syntetic Quotes |
//|                 Copyright © 2001-2005, MetaQuotes Software Corp. |
//|                                         http://www.metaquotes.ru |
//+------------------------------------------------------------------+
#pragma once

#define MAX_TICKS 128
//---- Tick structure
struct TickInfo
  {
   time_t            time;                // tick time
   char              security[16];        // symbol name
   double            bid,ask;             // prices
  };
//---- типы операндов
enum { OPERAND_SYMBOL, OPERAND_VALUE };
//---- описание операнда
struct SynteticOperand
  {
   int               type;                // тип операнда
   char              symbol[16];          // символ операнда
   double            value;               // значение операнда
   double            bid,ask;             // текущие значения по символу
  };
//---- описание синтетического инструмента
struct SynteticSymbol
  {
   char              name[16];            // имя синтетического инструмента
   //---- способ и параметры вычисления
   SynteticOperand   left;                // левый операнд
   SynteticOperand   right;               // правый операнд
   char              operation;           // операция (+,-,*,/)
  };
//+------------------------------------------------------------------+
//| База хранения и расчета синтетических инструментов               |
//+------------------------------------------------------------------+
class CSynteticBase
  {
private:
   SynteticSymbol   *m_syntetics;         // список синтетических инструментов
   int               m_syntetics_total;   // кол-во синт. инструментов
   int               m_syntetics_max;     // максимум буфера
   TickInfo          m_ticks[MAX_TICKS];  // буфер котировок
   int               m_ticks_total;       // всего котировок
   int               m_ticks_current;     // текущая позиция в котировках

public:
                     CSynteticBase();
                    ~CSynteticBase();

   void              Load(void);
   void              AddQuotes(const FeedData *data);
   int               GetTicks(FeedData *data);

private:
   void              AddSynteticSymbol(const SynteticSymbol *sym);
   void              RecalculateSymbol(const SynteticSymbol *cs);
   inline double     CalculateOpeation(char op,double left,double right) const;
  };
//+------------------------------------------------------------------+
