//+------------------------------------------------------------------+
//|                                        MetaTrader Virtual Dealer |
//|                 Copyright © 2001-2008, MetaQuotes Software Corp. |
//|                                         http://www.metaquotes.ru |
//+------------------------------------------------------------------+
#include "stdafx.h"
#include "Processor.h"
#include "UDP\UdpSender.h"
#include "PendingRequestQueue.h"

//---- ссылка на интрефейс сервера
extern CServerInterface *ExtServer;
//---- наш родной процессор
CProcessor               ExtProcessor;

#define FXI_OP_BUY	1
#define FXI_OP_SELL	-1

int startPendingRequestId = 40000; // ID запроса на отложенный ордер, уникален в пределах сессии
int startStopoutRequestId = 90000; // ID запроса на закрытие по стопауту, уникален в пределах сессии

void FormatRequestType(int reqType, char *outStr)
{
	switch (reqType)
	{
		case (TT_PRICES_GET) : strcpy(outStr, "TT_PRICES_GET"); break;

		case (TT_PRICES_REQUOTE) : strcpy(outStr, "TT_PRICES_REQUOTE"); break;
		case (TT_ORDER_IE_OPEN) : strcpy(outStr, "TT_ORDER_IE_OPEN"); break;
		case (TT_ORDER_REQ_OPEN) : strcpy(outStr, "TT_ORDER_REQ_OPEN"); break;
		case (TT_ORDER_MK_OPEN) : strcpy(outStr, "TT_ORDER_MK_OPEN"); break;
		case (TT_ORDER_PENDING_OPEN) : strcpy(outStr, "TT_ORDER_PENDING_OPEN"); break;
		case (TT_ORDER_IE_CLOSE) : strcpy(outStr, "TT_ORDER_IE_CLOSE"); break;

		case (TT_ORDER_REQ_CLOSE) : strcpy(outStr, "TT_ORDER_REQ_CLOSE"); break;
		case (TT_ORDER_MK_CLOSE) : strcpy(outStr, "TT_ORDER_MK_CLOSE"); break;
		case (TT_ORDER_MODIFY) : strcpy(outStr, "TT_ORDER_MODIFY"); break;
		case (TT_ORDER_DELETE) : strcpy(outStr, "TT_ORDER_DELETE"); break;
		case (TT_ORDER_CLOSE_BY) : strcpy(outStr, "TT_ORDER_CLOSE_BY"); break;
		case (TT_ORDER_CLOSE_ALL) : strcpy(outStr, "TT_ORDER_CLOSE_ALL"); break;

		case (TT_BR_ORDER_OPEN) : strcpy(outStr, "TT_BR_ORDER_OPEN"); break;
		case (TT_BR_ORDER_CLOSE) : strcpy(outStr, "TT_BR_ORDER_CLOSE"); break;
		case (TT_BR_ORDER_DELETE) : strcpy(outStr, "TT_BR_ORDER_DELETE"); break;
		case (TT_BR_ORDER_CLOSE_BY) : strcpy(outStr, "TT_BR_ORDER_CLOSE_BY"); break;
		case (TT_BR_ORDER_CLOSE_ALL) : strcpy(outStr, "TT_BR_ORDER_CLOSE_ALL"); break;
		case (TT_BR_ORDER_MODIFY) : strcpy(outStr, "TT_BR_ORDER_MODIFY"); break;

		case (TT_BR_ORDER_ACTIVATE) : strcpy(outStr, "TT_BR_ORDER_ACTIVATE"); break;
		case (TT_BR_ORDER_COMMENT) : strcpy(outStr, "TT_BR_ORDER_COMMENT"); break;
		case (TT_BR_BALANCE) : strcpy(outStr, "TT_BR_BALANCE"); break;
		
		default:	
			ltoa(reqType, outStr, 10);
	}
	return;
}

void FormatCommand(int cmd, char *outStr)
{	
	switch (cmd)
	{
		case (OP_BUY) : strcpy(outStr, "OP_BUY"); break;
		case (OP_SELL) : strcpy(outStr, "OP_SELL"); break;
		case (OP_BUY_LIMIT) : strcpy(outStr, "OP_BUY_LIMIT"); break;
		case (OP_SELL_LIMIT) : strcpy(outStr, "OP_SELL_LIMIT"); break;
		case (OP_BUY_STOP) : strcpy(outStr, "OP_BUY_STOP"); break;
		case (OP_SELL_STOP) : strcpy(outStr, "OP_SELL_STOP"); break;
		case (OP_BALANCE) : strcpy(outStr, "OP_BALANCE"); break;
		case (OP_CREDIT) : strcpy(outStr, "OP_CREDIT"); break;
		default:	
			ltoa(cmd, outStr, 10);
	}
	return;
}

void GetUserInfoByLogin(int login, UserInfo *inf)
{	
	UserRecord rec = { 0 };
	ExtServer->ClientsUserInfo(login, &rec);		
		
	inf->login = rec.login;
	inf->enable = rec.enable;
	inf->enable_change_password = rec.enable_change_password;
	inf->enable_read_only = rec.enable_read_only;
	//inf->flags = ?
	inf->leverage = rec.leverage;
	inf->agent_account = rec.agent_account;
	inf->balance = rec.balance;
	inf->credit = rec.credit;
	inf->prevbalance = rec.prevbalance;

	memcpy(inf->group, rec.group, 16);
	memcpy(inf->password, rec.password, 16);
	memcpy(inf->name, rec.name, 16);

	ExtServer->GroupsGet(rec.group, &(inf->grp));	
}

//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
CProcessor::CProcessor() : m_delay(0),m_loss_confirm(FALSE),m_manager(0),
                           m_thread(NULL),m_stop_flag(FALSE),
                           m_requests(NULL),m_requests_total(0),m_requests_max(0)
{
	m_groups[0] = 0;
	m_sender = new UdpSender();	
	strcpy(m_sendHost, "localhost");
	m_sendPort = 18001;
	m_receivePort = 18002;
	m_waitForFXIReply = 3000;
	m_SLTPTimeout = 3000;
	m_nextStopRequestId = INT_MAX - 30000;	
	m_lastLogWriteTime = time(NULL);
}
//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
CProcessor::~CProcessor()
{	
	m_stop_flag = TRUE;	
	if(m_thread != NULL) 
	{
		WaitForSingleObject(m_thread, 2000);
		CloseHandle(m_thread);
	}
	
	if(m_requests != NULL) { delete m_requests; m_requests = NULL; }
	delete m_sender; m_sender = NULL;
}
//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
void CProcessor::Initialize(void)
{
	// менеджер
	ExtConfig.GetInteger("Manager", &m_manager, "666");
	// получим группы
	ExtConfig.GetString("Groups", m_groups, sizeof(m_groups) - 1, ",*,");
	// получим задержку исполнения
	ExtConfig.GetInteger("Execution Delay", &m_delay, "2");
	// получим флаг процессинга с убытком клиенту
	ExtConfig.GetInteger("Order Loss Confirm", &m_loss_confirm, "0");
	// получим хост-порт FXI
	ExtConfig.GetString("Send host", m_sendHost, 127, "localhost");	 
	ExtConfig.GetInteger("Send port", &m_sendPort, "18001");
	// порт сообщений от FXI
	ExtConfig.GetInteger("Receive port", &m_receivePort, "18001");
	// ожидание ответа от FXI на ордер
	ExtConfig.GetInteger("Wait for FXI timeout", &m_waitForFXIReply, "3000");	
	// ожидание ответа от FXI на SL/TP ордер
	ExtConfig.GetInteger("Wait for FXI SL TP timeout", &m_SLTPTimeout, "3000");	

	// проверим на корректность
	if(m_delay < 0) m_delay = 0;
	if(m_delay > 5) m_delay = 5;
	
	m_loss_confirm = (m_loss_confirm == 0 ? FALSE : TRUE);	

	extQueue->Init("localhost", m_receivePort); 
}
//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
int CProcessor::IsGroupEnabled(const char *group)
{
	return CheckGroup(m_groups, group);
}
//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
int CProcessor::Add(RequestInfo *request, int isStopout)
{
	Request *temp;
	UINT     id;
	Request  req = {0};
	static int uid = 1;	
	
	if (request == NULL) return(FALSE);
	// проверки - кто управляет счетом
	if (m_delay == 0 || CheckGroup(m_groups, request->group) == FALSE) 
		return (TRUE);	

	// лог
	char reqTypeStr[64];
	char reqCmdStr[64];
	FormatCommand(request->trade.cmd, reqCmdStr);
	FormatRequestType(request->trade.type, reqTypeStr);
	CharString str;
	str.Printf(FALSE, "CProcessor::Add(тип [%s], cmd [%s])", reqTypeStr, reqCmdStr);
	ExtServer->LogsOut(CmdOK, PROGRAM_TITLE, str.ToArray());

	m_sync.Lock();
	//---- если место для запросов нет выделим
	if(m_requests == NULL)
	{
		if((m_requests = new Request[256]) == NULL) 
		{ 
			m_sync.Unlock(); 
			return (FALSE); 
		}
		m_requests_max = 256;
		m_requests_total = 0;
	}
	//---- посмотрим может надо перевыделить
	if (m_requests_total >= m_requests_max)
	{
		if((temp = new Request[m_requests_max + 256]) == NULL) 
		{ 
			m_sync.Unlock(); 
			return(FALSE); 
		}
		//---- скопируем старое
		memcpy(temp, m_requests, sizeof(Request) * m_requests_total);
		//---- удалим уже не нужное
		delete m_requests;
		
		m_requests = temp;
		m_requests_max += 256;
	}
	//---- вставляем запрос
	m_requests[m_requests_total].isStopout = isStopout;
	m_requests[m_requests_total].isPending = FALSE;
	m_requests[m_requests_total].id = request->id;
	m_requests[m_requests_total].time = GetTickCount();
	m_requests[m_requests_total].sent = FALSE;
	m_requests[m_requests_total].login = request->login;
	memcpy(&m_requests[m_requests_total].trans, &request->trade, sizeof(TradeTransInfo));
	COPY_STR(m_requests[m_requests_total].group, request->group);
	m_requests_total++;
	//---- запускаем поток
	if (m_thread == NULL)
	if ((m_thread = (HANDLE)_beginthreadex(NULL, 256000, ThreadFunction, this, 0, &id))==NULL)
	{
		m_sync.Unlock();
		return(FALSE);
	}
	
	m_sync.Unlock();
	return(TRUE);
}

int CProcessor::Add(const UserInfo *user, const ConGroup *group, const ConSymbol *symbol,
									 const TradeRecord *pending, TradeRecord *trade)
{	
	Request *temp;
	UINT     id;	
	static int uid = 1;	
	CharString str;
	
	if (pending == NULL || trade == NULL) return(FALSE);
	// проверки - кто управляет счетом
	if (m_delay == 0 || CheckGroup(m_groups, group->group) == FALSE) 
		return (TRUE);	

	double    prices[2];
	// получим текущие цены для группы и установки символа
	if (ExtServer->HistoryPricesGroup(symbol->symbol, group, prices) != RET_OK)			
	{
		str.Printf(FALSE, "CProcessor::Add Pending (symbol [%s], group [%s]) failed", symbol->symbol, group->group);
		ExtServer->LogsOut(CmdOK, PROGRAM_TITLE, str.ToArray());
		return FALSE;
	}
	
	m_sync.Lock();

	// проверить - нет ли такого запроса в списке
	Request  *reqTmp;
	if (m_requests != NULL)
	{
		int i = 0;
		for (reqTmp = m_requests; i < m_requests_total; i++, reqTmp++)
		{
			if (reqTmp->isPending)
				if (reqTmp->pendingId == pending->order)
				{
					ExtServer->LogsOut(CmdOK, PROGRAM_TITLE, "Отложенный ордер уже в списке");
					m_sync.Unlock();
					return (TRUE);
				}
		}
	}

	// лог	
	/*char reqCmdStr[64];
	FormatCommand(pending->cmd, reqCmdStr);		
	str.Printf(FALSE, "CProcessor::Add Pending(order [%d], cmd [%s], req Id [%d])", 
		pending->order, reqCmdStr, startPendingRequestId);
	ExtServer->LogsOut(CmdOK, PROGRAM_TITLE, str.ToArray());*/


	//---- если место для запросов нет выделим
	if(m_requests == NULL)
	{
		if((m_requests = new Request[256]) == NULL) 
		{ 
			m_sync.Unlock(); 
			return (FALSE); 
		}
		m_requests_max = 256;
		m_requests_total = 0;
	}
	//---- посмотрим может надо перевыделить
	if (m_requests_total >= m_requests_max)
	{
		if((temp = new Request[m_requests_max + 256]) == NULL) 
		{ 
			m_sync.Unlock(); 
			return(FALSE); 
		}
		//---- скопируем старое
		memcpy(temp, m_requests, sizeof(Request) * m_requests_total);
		//---- удалим уже не нужное
		delete m_requests;
		
		m_requests = temp;
		m_requests_max += 256;
	}
	//---- вставляем запрос
	m_requests[m_requests_total].pendingId = pending->order;
	m_requests[m_requests_total].isPending = TRUE;
	m_requests[m_requests_total].id = startPendingRequestId++;
	m_requests[m_requests_total].time = GetTickCount();
	m_requests[m_requests_total].sent = FALSE;
	m_requests[m_requests_total].login = user->login;	
	memcpy(&m_requests[m_requests_total].trans, pending, sizeof(TradeTransInfo));
	m_requests[m_requests_total].trans.volume = trade->volume;
	Out(CmdOK, "Pending order %d added of volume %d", 
		m_requests[m_requests_total].id, trade->volume);
	COPY_STR(m_requests[m_requests_total].group, group->group);
	// поправить команду
	char origTradeCmd[32], newTradeCmd[32];
	FormatCommand(m_requests[m_requests_total].trans.cmd, origTradeCmd);

	int pendCmd = trade->cmd; //m_requests[m_requests_total].trans.cmd;	
	pendCmd = (pendCmd == OP_BUY_LIMIT || pendCmd == OP_BUY_STOP) ? OP_BUY 
		: (pendCmd == OP_SELL_LIMIT || pendCmd == OP_SELL_STOP) ? OP_SELL 
		: pendCmd;
	m_requests[m_requests_total].trans.cmd = pendCmd;
	FormatCommand(m_requests[m_requests_total].trans.cmd, newTradeCmd);

	m_requests[m_requests_total].trans.type = TT_ORDER_MK_OPEN;
	m_requests[m_requests_total].trans.price = pendCmd == OP_BUY ? prices[1] : prices[0];
	strcpy(m_requests[m_requests_total].trans.symbol, symbol->symbol);
	m_requests[m_requests_total].trans.order = pending->order;
		
	str.Printf(FALSE, "Отложенный ордер %d добавлен, всего %d (%s, теперь %s)", 
		pending->order, m_requests_total, origTradeCmd, newTradeCmd);
	ExtServer->LogsOut(CmdOK, PROGRAM_TITLE, str.ToArray());
	
	m_requests_total++;
	//---- запускаем поток
	if (m_thread == NULL)
	if ((m_thread = (HANDLE)_beginthreadex(NULL, 256000, ThreadFunction, this, 0, &id))==NULL)
	{
		m_sync.Unlock();
		return(FALSE);
	}
	
	m_sync.Unlock();
	return(TRUE);
}

//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
UINT __stdcall CProcessor::ThreadFunction(LPVOID param)
{
	//---- запускаем функцию мембер
	if(param != NULL) ((CProcessor *)param)->Process();
	_endthreadex(0);	
	return(0);
}
//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
void CProcessor::Process(void)   
{
	int       i;
	UserInfo  user = {0};
	ConGroup  group = {0};
	double    prices[2];
	Request   *req;	
	user.login = m_manager;
	COPY_STR(user.name, PROGRAM_TITLE);
	COPY_STR(user.ip,   PROGRAM_TITLE);
	

	while(m_stop_flag == FALSE)
    {
		// ждем пока что-нибудь появится
		if (m_requests_total == 0) 
		{ 
			Sleep(100); 
			continue; 
		}
		// обязательно в локе
		m_sync.Lock();
		// идем по реквестам
		for (i = 0, req = m_requests; i < m_requests_total; i++, req++)
        {
			if (!ProcessSingleRequest(req))
			{
				// удаляем нафиг
				if (i < m_requests_total - 1) 
					memmove(req, req + 1, sizeof(Request) * (m_requests_total - i - 1));
				// коррекция индексов
				m_requests_total--; 
				i--; 
				req--;
			}
		}
		// разлочим
		m_sync.Unlock();
		// поспим
		Sleep(100);
	}
}

int CProcessor::ConfirmPendingRequest(RequestInfo *request)
{
	UserInfo  user = {0};
	ConGroup  group = {0};
	double    prices[2];

	user.login = m_manager;
	COPY_STR(user.name, "Virtual Dealer");
	COPY_STR(user.ip,   "VirtualDealer");

	ExtServer->GroupsGet(request->group, &group);
	// получим текущие цены для группы и установки символа
	if (ExtServer->HistoryPricesGroup(request->trade.symbol, &group, prices) != RET_OK)			
	{
		ExtServer->RequestsReset(request->id, &user, DC_RESETED);
		return FALSE;
	}
	
	ExtServer->RequestsConfirm(request->id, &user, prices);
	ExtServer->LogsOut(CmdOK, PROGRAM_TITLE, "отложенный ордер подтвержден");
}

/* 
returns: FALSE - request should be deleted, TRUE - keep request
*/
int CProcessor::ProcessSingleRequest(Request *req)
{
	UserInfo   user = {0};
	ConGroup   group = {0};
	double     prices[2];
	CharString str, strMsg;	
	char       strCmd[64], strType[64];

	user.login = m_manager;
	COPY_STR(user.name, "Virtual Dealer");
	COPY_STR(user.ip,   "VirtualDealer");

	ExtServer->GroupsGet(req->group, &group);
	// получим текущие цены для группы и установки символа
	if (ExtServer->HistoryPricesGroup(req->trans.symbol, &group, prices) != RET_OK)			
	{
		ExtServer->RequestsReset(req->id, &user, DC_RESETED);
		str.Printf(FALSE, "ProcessSingleRequest: HistoryPricesGroup (symbol [%s],  group [%s]) failed", req->trans.symbol, req->group);
		ExtServer->LogsOut(CmdOK, PROGRAM_TITLE, str.ToArray());
		return FALSE;
	}

	// если это запрос на открытие позы...
	if (req->trans.type == TT_ORDER_IE_OPEN || req->trans.type == TT_ORDER_IE_CLOSE
		|| req->trans.type == TT_ORDER_MK_OPEN || req->trans.type == TT_ORDER_MK_CLOSE)
	{		
		// запрос был отправлен, ответа нет - таймаут
		if (req->sent && (GetTickCount() > req->time + m_waitForFXIReply))
		{
			str.Printf(FALSE, "Таймаут для запроса %d (нет ответа от сервера FXI)", req->id);
			ExtServer->LogsOut(CmdOK, PROGRAM_TITLE, str.ToArray());
			// !! добавить открытие сделки (отложенное)
			ExtServer->RequestsRequote(req->id, &user, prices, FALSE);									
			return FALSE;
		}

		// запрос к FXI еще не был отправлен - отправить
		if (!req->sent)
		{
			ExtServer->LogsOut(CmdOK, PROGRAM_TITLE, "Отправка UDP");
			CharString reqStr;
			reqStr.Printf(FALSE, "requestId=%d;group=%s;time=%d;login=%d;", req->id, req->group, req->time, req->login);
			
			reqStr.Printf(TRUE, "type=%s;side=%d;order=%d;orderby=%d;price=%g;symbol=%s;volume=%d;tp=%g;sl=%g;slippage=%d;pending=%d;stopout=%d",
				req->trans.type == TT_ORDER_IE_OPEN || req->trans.type == TT_ORDER_MK_OPEN 
				? "OPEN" : "CLOSE", 
				req->trans.cmd == OP_BUY || req->trans.cmd == OP_BUY_LIMIT || req->trans.cmd == OP_BUY_STOP
				? FXI_OP_BUY : FXI_OP_SELL, 
				req->trans.order, req->trans.orderby, 
				req->trans.price, req->trans.symbol, req->trans.volume,
				req->trans.tp, req->trans.sl, req->trans.ie_deviation, req->isPending, req->isStopout);
			
			FormatCommand(req->trans.cmd, strCmd);
			FormatRequestType(req->trans.type, strType);
			str.Printf(FALSE, "Отправка запроса FXI [%s] типа [%s] по цене [%g], ордер [%d]", 
				strCmd, strType, req->trans.price, req->trans.order);
			ExtServer->LogsOut(CmdOK, PROGRAM_TITLE, str.ToArray());

			m_sender->SendTo(&reqStr, m_sendHost, m_sendPort);
			req->sent = TRUE;
			ExtServer->LogsOut(CmdOK, PROGRAM_TITLE, "Отправка UDP::OK");
			return TRUE;
		}				

		Response resp = { 0 };
		int requestId = req->trans.type == TT_ORDER_MK_CLOSE || req->trans.type == TT_ORDER_IE_CLOSE ?
			req->trans.order : req->id;
		int respFound = extQueue->FindAndDequeue(requestId, &resp);
		
		if (!respFound) return TRUE;
		
		// получен ответ от FXI
		ExtServer->LogsOut(CmdOK, PROGRAM_TITLE, "Обработка ответа FXI");						
		
		// обработан успешно
		if (resp.status == RequestCompleted)
		{			
			strMsg.Printf(FALSE, 
				"Запрос (%d) подтвержден по цене (%g), сейчас цена (%g)", 
				req->id, 
				req->trans.cmd == OP_BUY ? prices[1] : prices[0],
				resp.price);
			ExtServer->LogsOut(CmdOK, PROGRAM_TITLE, strMsg.ToArray());
			double    newPrices[2] = { resp.price, resp.price };
			req->trans.price = resp.price;

			// отложенный ордер
			if (resp.isPendingActivate)
			{
				ActivatePendingOrder(&resp);
				return FALSE;
			}

			// обработка ответа на закрытие (стопаут)
			if (resp.isStopoutReply)
			{
				ClosePositionStopout(&resp, req);
				return FALSE;
			}
			
			// обычный ордер
			// подтвердить запрос
			if (ExtServer->RequestsConfirm(req->id, &user, newPrices) == RET_OK)
			{
				// если запрос на открытие...
				if (req->trans.type == TT_ORDER_IE_OPEN || req->trans.type == TT_ORDER_MK_OPEN)
					// сохранить подтвержденный запрос (потом ассоциировать его с открывшейся
					// позицией)
					extPendingRequestQueue.AddRequest(resp.requestId, req->login,
						req->trans.cmd, req->trans.symbol, req->trans.volume, resp.price);
			}
			else
			{// сделка открыта у брокера, но не открылась в МТ4
				
			}
			return FALSE;
		}
		// обработан с ошибкой
		if (resp.status == RequestFailed)
		{
			ExtServer->LogsOut(CmdOK, PROGRAM_TITLE, "Запрос не подтвержден (отрицательная квитанция)");
			ExtServer->RequestsReset(req->id, &user, DC_RESETED);							
			return FALSE;
		}
		// прочие возвраты
		strMsg.Printf(FALSE, "Запрос: код ответа [%d]", resp.status);
		ExtServer->LogsOut(CmdOK, PROGRAM_TITLE, strMsg.ToArray());

		return FALSE;
	} // if (req->trans.type == TT_ORDER_IE_OPEN ...

	// запрос цены, не более
	if (req->trans.type == TT_PRICES_GET)
	{
		ExtServer->RequestsPrices(req->id, &user, prices, FALSE);
		return FALSE;
	}			

	if (req->trans.type == TT_ORDER_DELETE)
	{
		ExtServer->LogsOut(CmdOK, PROGRAM_TITLE, "Удаление отложенного ордера");
		ExtServer->RequestsConfirm(req->id, &user, prices);
		ExtServer->LogsOut(CmdOK, PROGRAM_TITLE, "Отложенный ордер удален");
		return FALSE;
	}

	// остальные запросы подтверждаем
	ExtServer->RequestsConfirm(req->id, &user, prices);

	return FALSE;
}
//+------------------------------------------------------------------+
//| Активировать отложенный ордер (пришла положительная квитанция)   |
//+------------------------------------------------------------------+
int CProcessor::ActivatePendingOrder(Response *resp)
{
	// получим ордер
	TradeRecord trade = { 0 };	
	TradeRecord new_trade = { 0 };

	if (ExtServer->OrdersGet(resp->pendingOrderId, &trade) == FALSE)
	{
		ExtServer->LogsOut(CmdOK, PROGRAM_TITLE, "ActivatePendingOrder - OrdersGet = FALSE");
		// проверим может это уже не отложенный ордер
		if (trade.cmd <= OP_SELL || trade.cmd >= OP_BALANCE) 
		{
			ExtServer->LogsOut(CmdOK, PROGRAM_TITLE, "ActivatePendingOrder - OrdersGet = FALSE (not pending)");
			return 0;
		}
		return 101; // ошибка - ордер не найден
	}

	// получим символ
	ConSymbol symbol = { 0 };
	ExtServer->SymbolsGet(trade.symbol, &symbol);
	// и пользователя
	UserInfo userInfo = { 0 };
	GetUserInfoByLogin(trade.login, &userInfo);	
	double prices[2] = { symbol.ask_tickvalue, symbol.bid_tickvalue };

	// проверим, возможно открывать новых позиций нельзя
	if (symbol.trade == TRADE_CLOSE)
	{
		// готовим описание		
		memcpy(&new_trade, &trade, sizeof(TradeRecord));
		new_trade.close_time = ExtServer->TradeTime();
		new_trade.close_price = (trade.cmd == OP_BUY_LIMIT || trade.cmd == OP_BUY_STOP) 
			? symbol.ask_tickvalue : symbol.bid_tickvalue;
		new_trade.profit = 0;
		new_trade.storage = 0;
		new_trade.expiration = 0;
		new_trade.taxes = 0;
		COPY_STR(new_trade.comment, "deleted [close only]");
		// удаляем
		if (ExtServer->OrdersUpdate(&new_trade, &userInfo, UPDATE_CLOSE) == FALSE)		
		{
			ExtServer->LogsOut(CmdOK, PROGRAM_TITLE, "ActivatePendingOrder - TRADE_CLOSE error");
			return 102;
		}
		ExtServer->LogsOut(CmdOK, PROGRAM_TITLE, "ActivatePendingOrder - TRADE_CLOSE");
		return 0;
	}

	// проверка маржевых требований
	TradeTransInfo trans = {0};
	// готовим описание новой транзакции для проверки маржи клиента
	COPY_STR(trans.symbol, trade.symbol);
	trans.cmd = (trade.cmd == OP_BUY_LIMIT || trade.cmd == OP_BUY_STOP) ? OP_BUY : OP_SELL;
	trans.volume = trade.volume;
	trans.price = trade.open_price;
	trans.sl = trade.sl;
	trans.tp = trade.tp;
	// получим состояние маржи
	double profit, freemargin, prevmargin, margin;	
	ConGroup groupInfo = { 0 };
	ExtServer->GroupsGet(userInfo.group, &groupInfo);

	margin = ExtServer->TradesMarginCheck(&userInfo, &trans, &profit, &freemargin, &prevmargin);
	
	// проверим ее
	if ((freemargin + groupInfo.credit) < 0 && (symbol.margin_hedged_strong != FALSE || prevmargin <= margin))
	{
		// готовим описание
		memcpy(&new_trade, &trade, sizeof(TradeRecord));
		new_trade.close_time = ExtServer->TradeTime();
		new_trade.close_price = (trade.cmd == OP_BUY_LIMIT || trade.cmd == OP_BUY_STOP) ? prices[1] : prices[0];
		new_trade.profit = 0;
		new_trade.storage = 0;
		new_trade.expiration = 0;
		new_trade.taxes = 0;
		COPY_STR(new_trade.comment, "deleted [no money]");
		// удаляем
		if (ExtServer->OrdersUpdate(&new_trade, &userInfo, UPDATE_CLOSE) == FALSE)		
		{
			ExtServer->LogsOut(CmdOK, PROGRAM_TITLE, "ActivatePendingOrder - margin close error");
			return 103;
		}
		ExtServer->LogsOut(CmdOK, PROGRAM_TITLE, "ActivatePendingOrder - margin close");
		return 0;
	}

	// активировать
	// готовим новый ордер
	memcpy(&new_trade, &trade, sizeof(TradeRecord));
	new_trade.cmd = (trade.cmd == OP_BUY_LIMIT || trade.cmd == OP_BUY_STOP) ? OP_BUY : OP_SELL;
	new_trade.open_time = ExtServer->TradeTime();
	new_trade.close_price = (new_trade.cmd == OP_BUY) ? prices[0] : prices[1];
	new_trade.profit = 0;
	new_trade.storage = 0;
	new_trade.expiration = 0;
	new_trade.taxes = 0;
	new_trade.open_price = resp->price;
	// начисляем комиссию
	ExtServer->TradesCommission(&new_trade, groupInfo.group, &symbol);
	// подсчитаем профиты
	ExtServer->TradesCalcProfit(groupInfo.group, &new_trade);
	// вычисляем курсы конвертации
	new_trade.conv_rates[0] = ExtServer->TradesCalcConvertation(groupInfo.group, FALSE, new_trade.open_price, &symbol);
	new_trade.margin_rate = ExtServer->TradesCalcConvertation(groupInfo.group, TRUE, new_trade.open_price, &symbol);
	// активируем новый ордер
	if (ExtServer->OrdersUpdate(&new_trade, &userInfo, UPDATE_ACTIVATE) == FALSE)
	{
		ExtServer->LogsOut(CmdOK, PROGRAM_TITLE, "ActivatePendingOrder - activate error");
		return 104;
	}
	ExtServer->LogsOut(CmdOK, PROGRAM_TITLE, "ActivatePendingOrder - OK");
	return 0;
}

//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
int CProcessor::ProcessStopApply(TradeRecord *trade, 
								 const ConGroup *group, const int isTP)
{
	char *groupStr = new char[50];
	strcpy(groupStr, group->group);
	if (CheckGroup(m_groups, groupStr) == FALSE) 
	{
		delete []groupStr;
		return FALSE;
	}
	delete []groupStr;

	double price = 0;	
	// получим текущие цены для группы и установки символа		
	double    prices[2] = { 0, 0 };
	int orderSide = trade->cmd == OP_BUY ? 1 : 0;
	if (ExtServer->HistoryPricesGroup(trade->symbol, group, prices) == RET_OK)			
	{
		int orderSide = trade->cmd == OP_BUY ? 0 : 1; // меняется от знака сделки
		price = prices[orderSide];
	}		
	
	CharString strMsg;
	strMsg.Printf(FALSE, "%s (%s at %g) is applying to order %d", 
		isTP ? "TP" : "SL", 
		orderSide == 1 ? "BUY" : "SELL",
		price,
		trade->order);
	ExtServer->LogsOut(CmdOK, PROGRAM_TITLE, strMsg.ToArray());
	
	SLTPOrderRequest req = { trade->order, price };
	m_orderRequests.Enqueue(&req);

	// отправить запрос на сервер
	int requestId = m_nextStopRequestId++;
	strMsg.Printf(FALSE, "requestId=%d;type=%s;price=%g;login=%d;symbol=%s;volume=%d;", 
		trade->order,
		isTP ? "TP" : "SL", 
		price,
		trade->login,
		trade->symbol,
		trade->volume);
	m_sender->SendTo(&strMsg, m_sendHost, m_sendPort);

	return TRUE;
}
//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
int CProcessor::OnFXIResponse(Response *resp)
{
	SLTPOrderRequest req = { 0 };	
	if (resp->status == OrderSLExecuted || resp->status == OrderTPExecuted)
	if (ExtProcessor.m_orderRequests.Dequeue(resp->requestId, &req))
	{
		CharString str;
		str.Printf(FALSE, "FXI response on SL-TP order %d, price=%g", req.orderId, resp->price);
		ExtServer->LogsOut(CmdOK, PROGRAM_TITLE, str.ToArray());

		// модифицировать ордер - активировать его	
		TradeRecord order = { 0 };
		if (ExtServer->OrdersGet(req.orderId, &order))
		{
			UserInfo  user = {0};
			user.login = ExtProcessor.m_manager;
			COPY_STR(user.name, "Virtual Dealer");
			COPY_STR(user.ip,   "VirtualDealer");
			order.close_price = resp->price;			
			str.Printf(FALSE, "FXI response order detail: login=%d, volume=%d", order.login, order.volume);
			ExtServer->LogsOut(CmdOK, PROGRAM_TITLE, str.ToArray());
			

			UserInfo us;
			if (GetUserInfo(order.login, &us))
			{
				#pragma pack(push,1)
				TradeTransInfo tti;
				memset(&tti, 0, sizeof(tti));
				tti.cmd = order.cmd;
				strcpy(tti.symbol, order.symbol);
				tti.volume = order.volume;
				tti.price = resp->price;
				tti.type = TT_ORDER_MK_CLOSE;
				tti.order = order.order;				
				if (ExtServer->OrdersClose(&tti, &us) == FALSE)
					ExtServer->LogsOut(CmdOK, PROGRAM_TITLE, "OrdersClose failed");
				else
					ExtServer->LogsOut(CmdOK, PROGRAM_TITLE, "OrdersClose OK");				
				#pragma pack(pop)
			}
			else 
				ExtServer->LogsOut(CmdOK, PROGRAM_TITLE, "GetUserInfo failed");			
		}
		return TRUE;
	}
	return FALSE;
}
//+------------------------------------------------------------------+
//| Позиция открылась (закрылась) - уведомить FXI                    |
//+------------------------------------------------------------------+
void CProcessor::OnTradeHistoryRecord(TradeRecord *trade)
{
	CharString str;
	PendingRequest req = { 0 };
	if (extPendingRequestQueue.FindRequest(trade->login, trade->cmd, trade->symbol,
		trade->volume, trade->open_price, &req))
	{		
		str.Printf(FALSE, "type=NOTIFY;requestId=%d;order=%d;", req.requestId, trade->order);
		m_sender->SendTo(&str, m_sendHost, m_sendPort);
		return;
	}
	str.Printf(FALSE, "Request for order=%d not found", trade->order);
	ExtServer->LogsOut(CmdOK, PROGRAM_TITLE, str.ToArray());
	str.Printf(FALSE, "Detail: login=%d, cmd=%d, symbol=%s, volume=%d, op_price=%g, state=%d", 
		trade->login, trade->cmd, trade->symbol,
		trade->volume, trade->open_price, trade->state);
	ExtServer->LogsOut(CmdOK, PROGRAM_TITLE, str.ToArray());
}
//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
void CProcessor::PrepareTradeRecord(TradeRecord *req, /*TradeRecord *orig,*/ double price,
						int isTP)
{
	// цена-время закрытия
	req->close_price = price;
	req->close_time = ::time(NULL);

	UserRecord user = { 0 };
	ConGroup group = { 0 };
	if (ExtServer->ClientsUserInfo(req->login, &user) == FALSE)
	{
		ExtServer->LogsOut(CmdOK, PROGRAM_TITLE, "PrepareTradeRecord: ClientsUserInfo failed");
		return;
	}
	if (ExtServer->GroupsGet(user.group, &group) == FALSE)
	{
		ExtServer->LogsOut(CmdOK, PROGRAM_TITLE, "PrepareTradeRecord: GroupsGet failed");
		return;
	}
	if (ExtServer->TradesCalcProfit(group.group, req) == FALSE)
	{
		ExtServer->LogsOut(CmdOK, PROGRAM_TITLE, "PrepareTradeRecord: TradesCalcProfit failed");
		return;
	}	       
}


//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
int CProcessor::GetUserInfo(int user_id, UserInfo *us)
{
	if (!us || !ExtServer) return 0;
	UserRecord User;
	memset(us, 0, sizeof(UserInfo));
	if(ExtServer->ClientsUserInfo(user_id, &User))
	{
		strcpy(us->name, User.name);
		us->agent_account = User.agent_account;
		us->balance = User.balance;
		us->credit = User.credit;
		us->enable = User.enable;
		us->login = User.login;
		strcpy(us->group, User.group);
		if (!ExtServer->GroupsGet(User.group, &us->grp))
			return 0;
		us->enable_change_password = User.enable_change_password;
		us->enable_read_only = User.enable_read_only;
		strcpy(us->password, User.password);
		us->prevbalance = User.prevbalance;  
		us->leverage = User.leverage;
		return 1;
	}
	return 0;
}

//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
int CProcessor::StopoutsFilter(const ConGroup *group)
{	
	if (group == NULL) return RET_OK_NONE;
	
	m_sync.Lock();	
	if (CheckGroup(m_groups, group->group) == FALSE)
	{
		OutNonFlood(CmdOK, 180, "Stopout: not in this group [%s], should be [%s]", 
			group->group, m_groups);
		m_sync.Unlock();
		return RET_OK_NONE;
	}	
	m_sync.Unlock();
	return RET_OK;
}
//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
int	CProcessor::StopoutsApply(const UserInfo *user, const ConGroup *group, 
								  const char *comment)
{
	Out(CmdOK, "CProcessor::StopoutsApply - Applying stopout for user [%d]", user->login);

	TradeRecord    *trades,*trade;
	int            total, i;
	ConSymbol      symbol = {0};
	TradeTransInfo trans  = {0};
	time_t         currtime;
	char           stopout[64] = "[stopout]";
	const char	   *cp;
	
	// check
	if (user == NULL || group == NULL || comment == NULL) 
	{
		Out(CmdOK, "CProcessor::StopoutsApply - check failed");
		return RET_OK;
	}
	// lock
	m_sync.Lock();
	
	// check user group
	if (CheckGroup(m_groups, group->group) == FALSE)
	{
		Out(CmdOK, "CProcessor::StopoutsApply - group check failed");
		m_sync.Unlock();
		return(RET_OK);
	}
	
	// receive all opened trades for user
	if ((trades = ExtServer->OrdersGetOpen(user, &total)) == NULL) 
	{
		Out(CmdOK, "CProcessor::StopoutsApply - no open trade");
		return RET_OK;
	}
	// get current server time
	currtime = ExtServer->TradeTime();
	// prepare comment about stopout
	if ((cp = strrchr(comment, '[')) != NULL) COPY_STR(stopout, cp);
	// output to server log
	Out(CmdOK, "'%d': close all orders due stop out %s", user->login, stopout);
	// go trough trades
	for (i = 0, trade = trades; i < total; i++, trade++)
	{
		// it is opened trade
		if (trade->cmd > OP_SELL) continue;
		// check symbol - зачем?
		// if (CheckGroup(m_symbols, trade->symbol) == FALSE) continue;
		// check volume - зачем?
		// if (m_max_volume != 0 && trade->volume > m_max_volume) continue;
		// receive symbol information
		if (ExtServer->SymbolsGet(trade->symbol, &symbol) == FALSE)
		{
			Out(CmdAtt, "stopout: receiving information for %s symbol failed", trade->symbol);
			continue;
		}
		
		// check trade session for symbol
		if (ExtServer->TradesCheckSessions(&symbol, currtime) == FALSE) continue;
		// prepare transaction
		trans.order  = trade->order;
		trans.price  = trade->close_price;
		trans.volume = trade->volume;
		trans.type   = TT_ORDER_MK_CLOSE; // !
		// prepare comment
		if (trade->comment[0] != 0)
		{
			COPY_STR(trans.comment, trade->comment);
			if (strstr(trans.comment, stopout) == NULL)
				_snprintf(trans.comment, sizeof(trans.comment) - 1, "%s %s", trans.comment, "[stopout FXI]");
		}
		else 
			COPY_STR(trans.comment, stopout);
		
		// отправить запрос на закрытие позиции на сервер
		// создать запрос
		RequestInfo inf = { 0 };
		memcpy(&(inf.trade), &trans, sizeof(TradeTransInfo));			
		inf.login = user->login;
		inf.id = trade->order; // startStopoutRequestId++; // !!
		memcpy(&inf.group, &group->group, sizeof(inf.group));
		memcpy(&inf.trade.symbol, &trade->symbol, sizeof(trade->symbol));
		// добавить в очередь
		Add(&inf, TRUE);
	}	
	m_sync.Unlock();
	// free memory
	HEAP_FREE(trades);	
	// сервер больше не процессит стопаут
	return RET_OK_NONE;
}
//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
int CProcessor::ClosePositionStopout(Response *resp, Request *req)
{
	Out(CmdOK, "Ответ на запрос [%d] закрытия ордера [%d], цена [%4.4f]", 
		req->id, req->trans.order, resp->price);
	// получить пользователя
	UserInfo user = { 0 };
	GetUserInfoByLogin(req->login, &user);

	// закрыть позицию
	if (ExtServer->OrdersClose(&req->trans, &user) == FALSE)
	{
		Out(CmdAtt, "Стопаут ордера [%d] не сработал", req->trans.order);
		return FALSE;
	}

	return TRUE;	
}
//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
void CProcessor::Out(const int code, LPCSTR msg, ...) const
{
	char buffer[1024];
	// check
	if (msg == NULL) return;
	// formatting string
	va_list ptr;
	va_start(ptr,msg);
	_vsnprintf(buffer, sizeof(buffer)-1, msg, ptr);
	va_end(ptr);
	// output to server log
	ExtServer->LogsOut(code, PROGRAM_TITLE, buffer);
}
//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
void CProcessor::OutNonFlood(const int code, int floodInterval, LPCSTR msg,...)
{
	time_t nowT = time(NULL);
	time_t deltaSec = nowT - m_lastLogWriteTime;
	
	if (deltaSec > floodInterval && msg != NULL)
	{
		char buffer[1024];				
		// formatting string
		va_list ptr;
		va_start(ptr,msg);
		_vsnprintf(buffer, sizeof(buffer)-1, msg, ptr);
		va_end(ptr);
		// output to server log
		ExtServer->LogsOut(code, PROGRAM_TITLE, buffer);
		m_lastLogWriteTime = nowT;
	}
}

//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
/*
int CProcessor::OrdersAdd(int login, int side, const char* smb, 
	double price, int volume)
{
	ExtServer->LogsOut(CmdOK, PROGRAM_TITLE, "Adding order");
		
	// получить пользователя
	UserInfo us = { 0 };
	if (!GetUserInfo(login, &us))
	{
		ExtServer->LogsOut(CmdOK, PROGRAM_TITLE, "Adding order - couldn't get user info");
		return FALSE;
	}
	char msg[256], strtmp[64];
	strcpy(msg, "User info: login=");
	ltoa(login, strtmp, 10);
	strcat(msg, strtmp);
	strcat(msg, ", leverage=");
	ltoa(us.leverage, strtmp, 10);
	strcat(msg, strtmp);
	ExtServer->LogsOut(CmdOK, PROGRAM_TITLE, msg);
	
	
	// заполнить структуру торговой записи	
	TradeTransInfo  tr = { 0 };	
	strcpy(tr.symbol, smb);
	tr.cmd = side > 0 ? OP_BUY : OP_SELL;
	tr.type = TT_ORDER_MK_OPEN;
	tr.volume = volume;	
	tr.price = price;
	strcpy(tr.comment, "[FXI]");
	// добавить позицию
	return ExtServer->OrdersOpen(&tr, &us);	
}*/
