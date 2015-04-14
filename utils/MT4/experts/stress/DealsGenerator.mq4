//+------------------------------------------------------------------+
//|                                               DealsGenerator.mq4 |
//|                                                  ForexInvest LTD |
//|                                            http://forexinvest.ru |
//+------------------------------------------------------------------+
#property copyright "ForexInvest LTD"
#property link      "http://forexinvest.ru"

#include <stderror.mqh>

extern string p0 = "Глобальные настройки советника:";
extern int Magic_num = 124458;
extern int Slippage = 30;
extern int TradeAttempts = 5;
extern int BarsShiftForClose = 2;

datetime timeCurrBar;
//+------------------------------------------------------------------+
//| expert initialization function                                   |
//+------------------------------------------------------------------+
int init()
  {
//----
   
//----
   return(0);
  }
//+------------------------------------------------------------------+
//| expert deinitialization function                                 |
//+------------------------------------------------------------------+
int deinit()
  {
//----
   
//----
   return(0);
  }
//+------------------------------------------------------------------+
//| expert start function                                            |
//+------------------------------------------------------------------+
int start()
  {
//----
      if (timeCurrBar != iTime(NULL, 0, 0))
      {
         // начался новый бар, открываем еще позицию
         int op = 0;
         if (iClose(NULL, 0, 0) > iClose(NULL, 0, 1))
            op = OP_SELL;
         else   
            op = OP_BUY;
         OpenPosition(Symbol(), op, AmountToLots(AccountBalance(), Symbol()), 0, 0, "DealsGenerator");
         timeCurrBar = iTime(NULL, 0, 0);
      }
      ActivePosManager();
//----
   return(0);
  }
//+------------------------------------------------------------------+

void ActivePosManager()
{
    
   for (int i = 1; i <= OrdersTotal(); i++)          
   {
      if (OrderSelect(i - 1, SELECT_BY_POS) == true)
      {
         if (OrderMagicNumber() == Magic_num)
         {
            if (iBarShift(NULL, 0, OrderOpenTime()) >= BarsShiftForClose)
            {
               ClosePosition(OrderTicket());  
               i = i - 1;
            }
         }
      }
   }
}

void OpenPosition(string currency, int deal_type, double lots, double sl, double tp, string comment)
{

	
	// код ошибки
	int err = 0;
	// открываем позицию
	int ticket = -1;
	// даем несколько попыток
	int attempt = 1;
	
	double slipp = Slippage;
	
	while (ticket < 0 && attempt <= TradeAttempts)
	{
	  // обновляем рыночную информацию
	  RefreshRates();
	  Comment(attempt + " попытка открыть позицию...");
	  GetTradeContext();
	  if (deal_type == OP_BUY)
	    ticket = OrderSend(currency, deal_type, lots, MarketInfo(currency, MODE_ASK), slipp, 0, 0, comment, Magic_num, 0, Red);
	  else
	    ticket = OrderSend(currency, deal_type, lots, MarketInfo(currency, MODE_BID), slipp, 0, 0, comment, Magic_num, 0, Red);
	  attempt = attempt + 1;
	  if (ticket < 0)
     {
         err = GetLastError();
         switch(err)
         {
            case ERR_INVALID_TRADE_PARAMETERS:  
            case ERR_ACCOUNT_DISABLED:
            case ERR_INVALID_ACCOUNT:
            //case ERR_INVALID_PRICE:
            case ERR_INVALID_STOPS:
            case ERR_INVALID_TRADE_VOLUME:
            case ERR_MARKET_CLOSED:
            case ERR_TRADE_DISABLED:
            case ERR_NOT_ENOUGH_MONEY:
            case ERR_TRADE_EXPIRATION_DENIED:
            case ERR_TRADE_TOO_MANY_ORDERS:
            Print("Ошибка открытия позиции: " + ErrorDescription(err));
            return;
            break;
         }
      }
  	}
  	if (OrderSelect(ticket, SELECT_BY_TICKET) == true)
  	{
  	   double OrderPrice = OrderOpenPrice();
  	
  	   // теперь выставляем тейк и стоп если они есть
  	   if (tp != 0 || sl != 0)
  	   {
  	      double stoploss = NormalizeDouble(sl, MarketInfo(currency, MODE_DIGITS));
	      double takeprofit = NormalizeDouble(tp, MarketInfo(currency, MODE_DIGITS));
  	      
   	   Sleep(400);
  	      bool res = false;
  	   
	     // даем несколько попыток
	     attempt = 1;
	     while (res == false && attempt <= TradeAttempts)
	     {
	       // обновляем рыночную информацию
	       RefreshRates();
	       Comment(attempt + " attempt to change Stop Loss & Take Profit for opened position ...");
	       GetTradeContext(); 
	       res = OrderModify(ticket, OrderOpenPrice(), stoploss, takeprofit, OrderExpiration());
	  
	       attempt = attempt + 1;
	       if (res == false)
          {
              err = GetLastError();
              switch(err)
              {
                 case ERR_INVALID_TRADE_PARAMETERS:  
                 case ERR_ACCOUNT_DISABLED:
                 case ERR_INVALID_ACCOUNT:
                 //case ERR_INVALID_PRICE:
                 case ERR_INVALID_STOPS:
                 case ERR_INVALID_TRADE_VOLUME:
                 case ERR_MARKET_CLOSED:
                 case ERR_TRADE_DISABLED:
                 case ERR_NOT_ENOUGH_MONEY:
                 case ERR_TRADE_EXPIRATION_DENIED:
                 case ERR_TRADE_TOO_MANY_ORDERS:
                 Print("Ошибка установки стопов позиции: " + ErrorDescription(err)); 
                 return;
                 break;
              } 
          }
  	     }
      }
   }
   else
      Print("Не смог выбрать ордер №" + ticket);
}

void ClosePosition(int ticket)
{
   bool res = false;
   int err;
      if (OrderSelect(ticket, SELECT_BY_TICKET) == true) 
      {  
         if (OrderMagicNumber() == Magic_num && (OrderType() == OP_BUY || OrderType() == OP_SELL))
         {  
            double price = 0;
            res = false;
            // даем несколько попыток
            int attempt = 1;
            while (res == false && attempt <= TradeAttempts)
            {

              Comment(attempt + " попытка закрыть позицию...");
              RefreshRates();
              if (OrderType() == OP_BUY)
                 price = MarketInfo(OrderSymbol(), MODE_BID);
              else
                 price = MarketInfo(OrderSymbol(), MODE_ASK);
              GetTradeContext();
              res = OrderClose(OrderTicket(), OrderLots(), price, Slippage, Violet);
              Sleep(200);
              attempt = attempt + 1;
              if (res == false)
              {
                  err = GetLastError();
                  Print("Не закрылась позиция: " + ErrorDescription(err));
                  switch(err)
                  {
                     case ERR_INVALID_TRADE_PARAMETERS:  
                     case ERR_ACCOUNT_DISABLED:
                     case ERR_INVALID_ACCOUNT:
                     //case ERR_INVALID_PRICE:
                     case ERR_INVALID_STOPS:
                     case ERR_INVALID_TRADE_VOLUME:
                     case ERR_MARKET_CLOSED:
                     case ERR_TRADE_DISABLED:
                     case ERR_NOT_ENOUGH_MONEY:
                     case ERR_TRADE_EXPIRATION_DENIED:
                     case ERR_TRADE_TOO_MANY_ORDERS:
                 
                     
                     break;
                  }
               }
            }
            
  	          
         }
       }
   
}

// функция конвертации абсолютного объема в количество лотов
double AmountToLots(double amount, string pair)
{
   double FI_Minlot = MarketInfo(pair, MODE_MINLOT); 
   double FI_Lotsize = MarketInfo(pair, MODE_LOTSIZE);
   double FI_Lotstep = MarketInfo(pair, MODE_LOTSTEP);
   Print("Получен amount = ", amount);
   Print("MinLot = " + FI_Minlot);
   Print("LotSize = " + FI_Lotsize);
   Print("LotStep = " + FI_Lotstep);
   
   double lots = MathRound(amount/FI_Lotsize/FI_Lotstep) * FI_Lotstep;
   Print("Вычислен лот lots = ", lots);
   //Comment("lots="+ lots); Print("lots="+ lots);
   if (lots >= FI_Minlot)
      return (lots);
   else
      return (FI_Minlot);
}

bool GetTradeContext()
{
   while(true)
   {
      if (IsTradeAllowed( ))
        return (true);
      Sleep(200);
   }
   return (false);
}

string ErrorDescription(int err)
 {
   switch (err)
   {
       
         case ERR_NO_ERROR:                     return ("Нет ошибки");
         case ERR_NO_RESULT:                    return ("Нет ошибки, но результат неизвестен"); 
         case ERR_COMMON_ERROR:                 return ("Общая ошибка"); 
         case ERR_INVALID_TRADE_PARAMETERS:     return ("Неправильные параметры"); 
         case ERR_SERVER_BUSY:                  return ("Торговый сервер занят"); 
         case ERR_OLD_VERSION:                  return ("Старая версия клиентского терминала"); 
         case ERR_NO_CONNECTION:                return ("Нет связи с торговым сервером"); 
         case ERR_NOT_ENOUGH_RIGHTS:            return ("Недостаточно прав"); 
         case ERR_TOO_FREQUENT_REQUESTS:        return ("Слишком частые запросы"); 
         case ERR_MALFUNCTIONAL_TRADE:          return ("Недопустимая операция нарушающая функционирование сервера"); 
         case ERR_ACCOUNT_DISABLED:             return ("Счет заблокирован"); 
         case ERR_INVALID_ACCOUNT:              return ("Неправильный номер счета"); 
         case ERR_TRADE_TIMEOUT:                return ("Истек срок ожидания совершения сделки"); 
         case ERR_INVALID_PRICE:                return ("Неправильная цена"); 
         case ERR_INVALID_STOPS:                return ("Неправильные стопы"); 
         case ERR_INVALID_TRADE_VOLUME:         return ("Неправильный объем"); 
         case ERR_MARKET_CLOSED:                return ("Рынок закрыт"); 
         case ERR_TRADE_DISABLED:               return ("Торговля запрещена"); 
         case ERR_NOT_ENOUGH_MONEY:             return ("Недостаточно денег для совершения операции"); 
         case ERR_PRICE_CHANGED:                return ("Цена изменилась"); 
         case ERR_OFF_QUOTES:                   return ("Нет цен"); 
         case ERR_BROKER_BUSY:                  return ("Брокер занят"); 
         case ERR_REQUOTE:                      return ("Новые цены"); 
         case ERR_ORDER_LOCKED:                 return ("Ордер заблокирован и уже обрабатывается"); 
         case ERR_LONG_POSITIONS_ONLY_ALLOWED:  return ("Разрешена только покупка"); 
         case ERR_TOO_MANY_REQUESTS:            return ("Слишком много запросов"); 
         case ERR_TRADE_MODIFY_DENIED:          return ("Модификация запрещена, так как ордер слишком близок к рынку"); 
         case ERR_TRADE_CONTEXT_BUSY:           return ("Подсистема торговли занята"); 
         case ERR_TRADE_EXPIRATION_DENIED:      return ("Использование даты истечения ордера запрещено брокером"); 
         case ERR_TRADE_TOO_MANY_ORDERS:        return ("Количество открытых и отложенных ордеров достигло предела, установленного брокером");
         
         case ERR_NO_MQLERROR:                  return ("Нет ошибки"); 
         case ERR_WRONG_FUNCTION_POINTER:       return ("Неправильный указатель функции");
         case ERR_ARRAY_INDEX_OUT_OF_RANGE:     return ("Индекс массива - вне диапазона");
         case ERR_NO_MEMORY_FOR_CALL_STACK:     return ("Нет памяти для стека функций");
         case ERR_RECURSIVE_STACK_OVERFLOW:     return ("Переполнение стека после рекурсивного вызова");
         case ERR_NOT_ENOUGH_STACK_FOR_PARAM:   return ("На стеке нет памяти для передачи параметров");
         case ERR_NO_MEMORY_FOR_PARAM_STRING:   return ("Нет памяти для строкового параметра");
         case ERR_NO_MEMORY_FOR_TEMP_STRING:    return ("Нет памяти для временной строки");
         case ERR_NOT_INITIALIZED_STRING:       return ("Неинициализированная строка");
         case ERR_NOT_INITIALIZED_ARRAYSTRING:  return ("Неинициализированная строка в массиве");
         case ERR_NO_MEMORY_FOR_ARRAYSTRING:    return ("Нет памяти для строкового массива");
         case ERR_TOO_LONG_STRING:              return ("Слишком длинная строка");
         case ERR_REMAINDER_FROM_ZERO_DIVIDE:   return ("Остаток от деления на ноль");
         case ERR_ZERO_DIVIDE:                  return ("Деление на ноль");
         case ERR_UNKNOWN_COMMAND:              return ("Неизвестная команда");
         case ERR_WRONG_JUMP:                   return ("Неправильный переход");
         case ERR_NOT_INITIALIZED_ARRAY:        return ("Неинициализированный массив");
         case ERR_DLL_CALLS_NOT_ALLOWED:        return ("Вызовы DLL не разрешены");
         case ERR_CANNOT_LOAD_LIBRARY:          return ("Невозможно загрузить библиотеку");
         case ERR_CANNOT_CALL_FUNCTION:         return ("Невозможно вызвать функцию");
         case ERR_EXTERNAL_CALLS_NOT_ALLOWED:   return ("Вызовы внешних библиотечных функций не разрешены");
         case ERR_NO_MEMORY_FOR_RETURNED_STR:   return ("Недостаточно памяти для строки, возвращаемой из функции");
         case ERR_SYSTEM_BUSY:                  return ("Система занята");
         case ERR_INVALID_FUNCTION_PARAMSCNT:   return ("Неправильное количество параметров функции");
         case ERR_INVALID_FUNCTION_PARAMVALUE:  return ("Недопустимое значение параметра функции");
         case ERR_STRING_FUNCTION_INTERNAL:     return ("Внутренняя ошибка строковой функции");
         case ERR_SOME_ARRAY_ERROR:             return ("Ошибка массива");
         case ERR_INCORRECT_SERIESARRAY_USING:  return ("Неправильное использование массива-таймсерии");
         case ERR_CUSTOM_INDICATOR_ERROR:       return ("Ошибка пользовательского индикатора");
         case ERR_INCOMPATIBLE_ARRAYS:          return ("Массивы несовместимы");
         case ERR_GLOBAL_VARIABLES_PROCESSING:  return ("Ошибка обработки глобальныех переменных");
         case ERR_GLOBAL_VARIABLE_NOT_FOUND:    return ("Глобальная переменная не обнаружена");
         case ERR_FUNC_NOT_ALLOWED_IN_TESTING:  return ("Функция не разрешена в тестовом режиме");
         case ERR_FUNCTION_NOT_CONFIRMED:       return ("Функция не разрешена");
         case ERR_SEND_MAIL_ERROR:              return ("Ошибка отправки почты");
         case ERR_STRING_PARAMETER_EXPECTED:    return ("Ожидается параметр типа string");
         case ERR_INTEGER_PARAMETER_EXPECTED:   return ("Ожидается параметр типа integer");
         case ERR_DOUBLE_PARAMETER_EXPECTED:    return ("Ожидается параметр типа double");
         case ERR_ARRAY_AS_PARAMETER_EXPECTED:  return ("В качестве параметра ожидается массив");
         case ERR_HISTORY_WILL_UPDATED:         return ("Запрошенные исторические данные в состоянии обновления");
         case ERR_TRADE_ERROR:                  return ("Ошибка при выполнении торговой операции");
         case ERR_END_OF_FILE:                  return ("Конец файла");
         case ERR_SOME_FILE_ERROR:              return ("Ошибка при работе с файлом");
         case ERR_WRONG_FILE_NAME:              return ("Неправильное имя файла");
         case ERR_TOO_MANY_OPENED_FILES:        return ("Слишком много открытых файлов");
         case ERR_CANNOT_OPEN_FILE:             return ("Невозможно открыть файл");
         case ERR_INCOMPATIBLE_FILEACCESS:      return ("Несовместимый режим доступа к файлу");
         case ERR_NO_ORDER_SELECTED:            return ("Ни один ордер не выбран");
         case ERR_UNKNOWN_SYMBOL:               return ("Неизвестный символ");
         case ERR_INVALID_PRICE_PARAM:          return ("Неправильный параметр цены для торговой функции");
         case ERR_INVALID_TICKET:               return ("Неверный номер тикета");
         case ERR_TRADE_NOT_ALLOWED:            return ("Торговля не разрешена. Необходимо включить опцию \"Разрешить советнику торговать\" в свойствах эксперта.");
         case ERR_LONGS_NOT_ALLOWED:            return ("Длинные позиции не разрешены. Необходимо проверить свойства эксперта.");
         case ERR_SHORTS_NOT_ALLOWED:           return ("Короткие позиции не разрешены. Необходимо проверить свойства эксперта.");
         case ERR_OBJECT_ALREADY_EXISTS:        return ("Объект уже существует");
         case ERR_UNKNOWN_OBJECT_PROPERTY:      return ("Запрошено неизвестное свойство объекта");
         case ERR_OBJECT_DOES_NOT_EXIST:        return ("Объект не существует");
         case ERR_UNKNOWN_OBJECT_TYPE:          return ("Неизвестный тип объекта");
         case ERR_NO_OBJECT_NAME:               return ("Нет имени объекта");
         case ERR_OBJECT_COORDINATES_ERROR:     return ("Ошибка координат объекта");
         case ERR_NO_SPECIFIED_SUBWINDOW:       return ("Не найдено указанное подокно");
         case ERR_SOME_OBJECT_ERROR:            return ("Ошибка при работе с объектом");
     
         default: 
         return ("Код ошибки: " + err);
   }
}

