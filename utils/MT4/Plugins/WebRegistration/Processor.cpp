//+------------------------------------------------------------------+
//|                                       MetaTrader WebRegistration |
//|                 Copyright © 2001-2008, MetaQuotes Software Corp. |
//|                                        http://www.metaquotes.net |
//+------------------------------------------------------------------+
#include "stdafx.h"
#include "Processor.h"
//---- Link to our server interface
extern CServerInterface *ExtServer;
//---- Our Telnet processor
CProcessor               ExtProcessor;

#define		DELETE_FLOODER_SECONDS 30

#define		CMD_MODIFY			"MODIFYACCOUNT"
#define		CMD_MODIFY_LEN		13
#define		CMD_NEW				"NEWACCOUNT"
#define		CMD_NEW_LEN			10

#define		CMD_OPEN			"ORDEROPEN"
#define		CMD_OPEN_LEN		9
#define		CMD_CLOSE			"ORDERCLOSE"
#define		CMD_CLOSE_LEN		10

#define		CMD_OPEN_CLOSE		"ORDEROPCLOSE"
#define		CMD_OPEN_CLOSE_LEN	12

#define		CMD_ADDHISTORY		"ORDERHISTORY"
#define		CMD_ADDHISTORY_LEN	12

#define		CMD_GETHISTORY		"GETHISTORY"
#define		CMD_GETHISTORY_LEN	10


//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
void GetUserInfoByLogin(int login, UserInfo *inf)
{	
	UserRecord rec = { 0 };
	ExtServer->ClientsUserInfo(login, &rec);		
		
	inf->login = rec.login;
	inf->enable = rec.enable;
	inf->enable_change_password = rec.enable_change_password;
	inf->enable_read_only = rec.enable_read_only;
	
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
CProcessor::CProcessor() : m_groups(NULL), m_groups_total(0),
                           m_flooders(NULL), m_flooders_total(0), m_flooders_max(0)
{
	m_ip[0] = 0; m_ip[1] = 0; m_ip[2] = 0;
	m_antifloodPeriod = 0;
	m_password[0]=0;
}
//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
CProcessor::~CProcessor()
  {
//---- lock
   m_sync.Lock();
//---- delete all groups and flooders
   if(m_groups  !=NULL) { delete[] m_groups;   m_groups  =NULL; }
   if(m_flooders!=NULL) { delete[] m_flooders; m_flooders=NULL; }
//---- set all to zero
   m_groups_total=m_flooders_total=m_flooders_max=0;
//---- unlock
   m_sync.Unlock();
  }
//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
void CProcessor::Initialize(void)
  {
	char        buffer[1024];
	Group      *temp;
	int         i,groups_max;
	PluginCfg   cfg;
	
   
	//---- lock
	m_sync.Lock();	
	//---- get master password
	ExtConfig.GetString(1,"Master Password",m_password,sizeof(m_password)-1,"password");
	//---- get allowed master IP and conver IP to number format
	ExtConfig.GetString(2, "Master IP",      buffer,    sizeof(buffer)-1,    "127.0.0.1");	
	m_ip[0] = inet_addr(buffer);
	//---- additional IPs
	ExtConfig.GetString(2, "Master IP 2",      buffer,    sizeof(buffer)-1,    "127.0.0.1");	
	m_ip[1] = inet_addr(buffer);
	ExtConfig.GetString(2, "Master IP 3",      buffer,    sizeof(buffer)-1,    "127.0.0.1");	
	m_ip[2] = inet_addr(buffer);



	// antiflood period
	ExtConfig.GetInteger(3, "Antifllod seconds", &m_antifloodPeriod, "10");
	//---- get groups map
	groups_max=m_groups_total;
	//---- parse groups
   for(i=0,m_groups_total=0;;i++)
     {
      //---- prepare query
      _snprintf(buffer,sizeof(buffer)-1,"Group %d",i+1);
      //---- try to receive group
      if(ExtConfig.Get(buffer,&cfg)==FALSE) break;
      //---- check
      if(cfg.value[0]==0) continue;
      //---- check space
      if(m_groups==NULL || m_groups_total>=groups_max)
        {
         //---- allocate new buffer
         if((temp=new Group[m_groups_total+1024])==NULL)
           {
            m_sync.Unlock();
            Out(CmdAtt, PROGRAM_TITLE, "not enough memory for groups [%d]", m_groups_total + 1024);
            return;
           }
         //---- copy all from old buffer to new and delete old
         if(m_groups!=NULL)
           {
            memcpy(temp,m_groups,sizeof(Group)*m_groups_total);
            delete[] m_groups;
           }
         //---- set new buffer
         m_groups  =temp;
         groups_max=m_groups_total+1024;
        }
      //---- add group
      m_groups[m_groups_total].id=i+1;
      COPY_STR(m_groups[m_groups_total].group,cfg.value);
      //---- increment groups total
      m_groups_total++;
     }
//---- if groups not exists then create samples
   if(m_groups_total==0)
     {
      //---- add first example
      COPY_STR(cfg.name, "Group 1");
      COPY_STR(cfg.value,"demoforex");
      ExtConfig.Add(0,&cfg);
      //---- add second example
      COPY_STR(cfg.name, "Group 2");
      COPY_STR(cfg.value,"");
      ExtConfig.Add(0,&cfg);
     }
//---- unlock
   m_sync.Unlock();
  }
//+------------------------------------------------------------------+
//| Parser Telnet requests                                           |
//+------------------------------------------------------------------+
char* CProcessor::Process(const ULONG ip, char *buffer, int *resultLength)
{
	ExtServer->LogsOut(CmdOK, PROGRAM_TITLE, "CProcessor::Process");
	UserRecord user  = {0};
	ConGroup   group = {0};
	char       group_name[32] = {0};	
	int        i;
	double     deposit=0;
	char       temp[1024];
	const int  tempSize = 1024;
	
	// lock
	m_sync.Lock();
	
	// checks
	if (buffer == NULL) 
	{
		*resultLength = 0;
		ExtServer->LogsOut(CmdOK, PROGRAM_TITLE, "CProcessor::Buffer is 0");
		m_sync.Unlock();
		return 0;
	}

	if (ip != m_ip[0] && ip != m_ip[1] && ip != m_ip[2])
	{
		*resultLength = 0;
		ExtServer->LogsOut(CmdOK, PROGRAM_TITLE, "CProcessor::ip != m_ip");
		m_sync.Unlock();
		return 0;
	}
	
	if (m_groups == NULL) 
	{
		*resultLength = 0;
		ExtServer->LogsOut(CmdOK, PROGRAM_TITLE, "CProcessor::m_groups == 0");
		m_sync.Unlock();
		return 0;
	}

	// check command
	ExtServer->LogsOut(CmdOK, PROGRAM_TITLE, "CProcessor::Checking");
	if (memcmp(buffer, CMD_NEW, CMD_NEW_LEN)!=0 && 
		memcmp(buffer, CMD_MODIFY, CMD_MODIFY_LEN) != 0 &&
		memcmp(buffer, CMD_OPEN, CMD_OPEN_LEN) != 0 &&
		memcmp(buffer, CMD_CLOSE, CMD_CLOSE_LEN) != 0 &&
		memcmp(buffer, CMD_ADDHISTORY, CMD_ADDHISTORY_LEN) != 0 &&
		memcmp(buffer, CMD_GETHISTORY, CMD_GETHISTORY_LEN) != 0)
	{ 
		*resultLength = 0;
		m_sync.Unlock(); 
		return 0; 
	}
	ExtServer->LogsOut(CmdOK, PROGRAM_TITLE, "CProcessor::Check OK");
	
	// receive master password and check it
	if (GetStrParam(buffer, "MASTER=", temp, sizeof(temp) - 1) == FALSE || strcmp(temp, m_password) != 0)
	{
		m_sync.Unlock();		
		*resultLength = _snprintf(temp, tempSize - 1, "ERROR\r\ninvalid data(MASTER)\r\nend\r\n");
		char *dest = (char*) HEAP_ALLOC(*resultLength + 1);
		strcpy(dest, temp);
		return dest;
	}
	// unlock
	m_sync.Unlock();
	
	// receive IP
	if (GetStrParam(buffer, "IP=", temp, sizeof(temp) - 1) == FALSE) 
	{
		*resultLength = _snprintf(buffer, tempSize - 1, "ERROR\r\ninvalid data(IP)\r\nend\r\n");
		char *dest = (char*) HEAP_ALLOC(*resultLength + 1);
		strcpy(dest, temp);
		return dest;
	}
	
	// receive group id and map it
	if (GetIntParam(buffer, "GROUP=", &i) == FALSE || (MapGroup(i, group_name)) == FALSE)
	{
		*resultLength = _snprintf(buffer, tempSize - 1, "ERROR\r\ninvalid data(GROUP)\r\nend\r\n");
		char *dest = (char*) HEAP_ALLOC(*resultLength + 1);
		strcpy(dest, temp);
		return dest;
	}
	
	// receive group overview
	if (ExtServer->GroupsGet(group_name, &group) == FALSE)
	{
		*resultLength = _snprintf(buffer, tempSize - 1, "ERROR\r\ninvalid data(GROUP DATA)\r\nend\r\n");
		char *dest = (char*) HEAP_ALLOC(*resultLength + 1);
		strcpy(dest, temp);
		return dest;
	}
   
	// если - изменить аккаунт
	if (memcmp(buffer, CMD_MODIFY, CMD_MODIFY_LEN) == 0)
	{
		ExtServer->LogsOut(CmdOK, PROGRAM_TITLE, "modifyaccount >");		
		ProcessModifyRequest(ip, buffer, tempSize, temp);   		
		strcat(temp, "< on modifyaccount");
		
		*resultLength = strlen(temp);
		char *dest = (char*) HEAP_ALLOC(*resultLength + 1);
		strcpy(dest, temp);
		ExtServer->LogsOut(CmdOK, PROGRAM_TITLE, "ProcessModifyRequest::OK");
		return dest;
	}

	// если - открыть позу
	if (memcmp(buffer, CMD_OPEN, CMD_OPEN_LEN) == 0 || 
		memcmp(buffer, CMD_OPEN_CLOSE, CMD_OPEN_CLOSE_LEN) == 0)
	{
		ExtServer->LogsOut(CmdOK, PROGRAM_TITLE, "open position >");
		int login, side, volm;
		char smb[16];
		double price;
		GetIntParam(buffer, "LOGIN=", &login);
		GetIntParam(buffer, "SIDE=", &side);
		GetStrParam(buffer, "SYMBOL=", smb, 16);
		GetFltParam(buffer, "PRICE=", &price);
		GetIntParam(buffer, "VOLUME=", &volm);
		// сразу же закрыть?
		int needClose = 0;
		if (memcmp(buffer, CMD_OPEN_CLOSE, CMD_OPEN_CLOSE_LEN) == 0) 
			needClose = 1;
		int orderId = OrdersAdd(login, side, smb, price, volm, needClose);
		
		ltoa(orderId, temp, 10);
		*resultLength = strlen(temp);
		char *dest = (char*) HEAP_ALLOC(*resultLength + 1);
		strcpy(dest, temp);
		ExtServer->LogsOut(CmdOK, PROGRAM_TITLE, "open position: OK");

		return dest;
	}

	// добавить инвесторскую сделку
	if (memcmp(buffer, CMD_ADDHISTORY, CMD_ADDHISTORY_LEN) == 0)
	{		
		ProcessHistoryRequest(ip, buffer, tempSize, temp);
		strcat(temp, "< on addhistory");
		
		*resultLength = strlen(temp);
		char *dest = (char*) HEAP_ALLOC(*resultLength + 1);
		strcpy(dest, temp);		
		
		return dest;
	}
	

	// закрыть позу
	if (memcmp(buffer, CMD_CLOSE, CMD_CLOSE_LEN) == 0)
	{
		ExtServer->LogsOut(CmdOK, PROGRAM_TITLE, "close position >");
		int login, order;		
		double price;
		GetIntParam(buffer, "LOGIN=", &login);
		GetIntParam(buffer, "ORDER=", &order);		
		GetFltParam(buffer, "PRICE=", &price);		
		int closeOK = OrdersClose(login, order, price);
		if (closeOK) strcpy(temp, "1");
		else         strcpy(temp, "0");
		
		*resultLength = strlen(temp);
		char *dest = (char*) HEAP_ALLOC(*resultLength + 1);
		strcpy(dest, temp);

		ExtServer->LogsOut(CmdOK, PROGRAM_TITLE, "close position: OK");
		return dest;
	}

	// получить историю ордеров
	if (memcmp(buffer, CMD_GETHISTORY, CMD_GETHISTORY_LEN) == 0)
	{
		ExtServer->LogsOut(CmdOK, PROGRAM_TITLE, "get history >");
		int logins[200];
		GetStrParam(buffer, "LOGINS=", temp, sizeof(temp)-1);
		int loginsCount = IntArrayFromString(temp, ',', logins);		
		time_t start, end;
		GetTimeParam(buffer, "START=", &start);
		GetTimeParam(buffer, "END=", &end);
		int openOnly = 0;
		GetIntParam(buffer, "OPEN=", &openOnly);
		
		char *dest = GetPositionsInfo(logins, loginsCount, start, end, openOnly, 
			resultLength);

		ExtServer->LogsOut(CmdOK, PROGRAM_TITLE, "< get history");
		return dest;
	}

	// prepare user record
	user.enable                =TRUE;
	user.enable_change_password=TRUE;
	user.leverage              =group.default_leverage;
	user.user_color            =0xff000000;
	int userLogin;
	COPY_STR(user.group,group_name);
	GetStrParam(buffer,"NAME=",          user.name,             sizeof(user.name)-1);
	GetStrParam(buffer,"PASSWORD=",      user.password,         sizeof(user.password)-1);
	GetStrParam(buffer,"INVESTOR=",      user.password_investor,sizeof(user.password_investor)-1);
	GetStrParam(buffer,"EMAIL=",         user.email,            sizeof(user.email)-1);
	GetStrParam(buffer,"COUNTRY=",       user.country,          sizeof(user.country)-1);
	GetStrParam(buffer,"STATE=",         user.state,            sizeof(user.state)-1);
	GetStrParam(buffer,"CITY=",          user.city,             sizeof(user.city)-1);
	GetStrParam(buffer,"ADDRESS=",       user.address,          sizeof(user.address)-1);
	GetStrParam(buffer,"COMMENT=",       user.comment,          sizeof(user.comment)-1);
	GetStrParam(buffer,"PHONE=",         user.phone,            sizeof(user.phone)-1);
	GetStrParam(buffer,"PHONE_PASSWORD=",user.password_phone,   sizeof(user.password_phone)-1);
	GetStrParam(buffer,"STATUS=",        user.status,           sizeof(user.status)-1);
	GetStrParam(buffer,"ZIPCODE=",       user.zipcode,          sizeof(user.zipcode)-1);
	GetStrParam(buffer,"ID=",            user.id,               sizeof(user.id)-1);
	GetIntParam(buffer,"LEVERAGE=",     &user.leverage);
	GetIntParam(buffer,"AGENT=",        &user.agent_account);
	GetIntParam(buffer,"SEND_REPORTS=", &user.send_reports);
	GetFltParam(buffer,"DEPOSIT=",      &deposit);
	if (GetIntParam(buffer, "LOGIN=", &userLogin)) user.login = userLogin;
	// checks
	if (user.leverage     <1)         user.leverage     =0;
	if (user.agent_account<1)         user.agent_account=0;
	if (user.send_reports!=0)         user.send_reports =TRUE;
	if (deposit           <0)         deposit           =0;
	if (deposit           >100000000) deposit           =100000000;
	// check default deposit
	if (group.default_deposit > 0) deposit = group.default_deposit;
	// check complexity of password
	if (CheckPassword(user.password) == FALSE)
	{
		*resultLength = _snprintf(buffer, tempSize - 1, "ERROR\r\nsimple password\r\nend\r\n");
		char *dest = (char*) HEAP_ALLOC(*resultLength + 1);
		strcpy(dest, temp);
		return dest;						
	}
	// check user record
	if (user.name[0] == 0 || user.leverage < 1 || user.agent_account < 0)
	{
		*resultLength = _snprintf(buffer, tempSize - 1, "ERROR\r\ninvalid data\r\nend\r\n");
		char *dest = (char*) HEAP_ALLOC(*resultLength + 1);
		strcpy(dest, temp);
		return dest;				
	}
	// check IP by antiflood
	/*if(CheckFlooder(temp)==FALSE)
		return _snprintf(buffer,size-1,
			"ERROR\r\nIP is blocked. Please wait %d secs and try again.\r\nend\r\n",
			m_antifloodPeriod);*/
	// creating new account
	if (ExtServer->ClientsAddUser(&user) == FALSE)
	{
		*resultLength = _snprintf(buffer, tempSize - 1, "ERROR\r\naccount create failed\r\nend\r\n");
		char *dest = (char*) HEAP_ALLOC(*resultLength + 1);
		strcpy(dest, temp);
		return dest;		
	}
	// checking is demo
	if (strncmp(user.group, "demo", 4) == 0 && deposit > 0)
	{//---- trying process deposit operation
		ExtServer->ClientsChangeBalance(user.login, &group, deposit, "");
    }
	// write answer
	*resultLength = _snprintf(buffer, tempSize - 1, "OK\r\nLOGIN=%d\r\nend\r\n", user.login);
	char *dest = (char*) HEAP_ALLOC(*resultLength + 1);
	strcpy(dest, temp);
	return dest;	
}

//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
int CProcessor::ProcessModifyRequest(const ULONG ip, char *buffer, const int size, char *destBuffer)
{
	ExtServer->LogsOut(CmdOK, PROGRAM_TITLE, "ProcessModifyRequest");
	int login;
	if (!GetIntParam(buffer, "LOGIN=", &login)) return 0;
	// получить счет
	UserRecord user = { 0 };
	if (ExtServer->ClientsUserInfo(login, &user) == FALSE) return 0;
	// получить группу счета
	ConGroup group = { 0 };
	if (ExtServer->GroupsGet(user.group, &group) == FALSE) return 0;

	// установить параметры
	char buf[512];
	double dVal = 0;

	if (GetStrParam(buffer, "NAME=", buf, 512))
		strcpy(user.name, buf);
	if (GetStrParam(buffer, "ADDRESS=", buf, 512))
		strcpy(user.address, buf);
	if (GetStrParam(buffer, "PASSWORD=", buf, 512))
		strcpy(user.password, buf);
	if (GetStrParam(buffer, "COUNTRY=", buf, 512))
		strcpy(user.country, buf);
	if (GetStrParam(buffer, "EMAIL=", buf, 512))
		strcpy(user.email, buf);
	if (GetStrParam(buffer, "CITY=", buf, 512))
		strcpy(user.city, buf);
	if (GetStrParam(buffer, "COMMENT=", buf, 512))
		strcpy(user.comment, buf);
	if (GetStrParam(buffer, "INVESTOR=", buf, 512))
		strcpy(user.password_investor, buf);
	if (GetStrParam(buffer, "STATE=", buf, 512))
		strcpy(user.state, buf);
	if (GetStrParam(buffer, "PHONE=", buf, 512))
		strcpy(user.phone, buf);
	if (GetStrParam(buffer, "ZIPCODE=", buf, 512))
		strcpy(user.zipcode, buf);

	int result = 0, ordNum = 0;
	if (GetFltParam(buffer, "DEPOSIT=", &dVal))
	{
		// обновить депозит
		result = ExtServer->ClientsChangeBalance(user.login, &group, dVal, "Modified by plugin FXWebRegistration");
		if (result > 0)
		{
			ordNum = result;
			result = 1;
		}
	}
	
	
	// принять изменения
	int updateResult = (ExtServer->ClientsUserUpdate(&user)); // TRUE - FALSE
	if (updateResult == TRUE) result |= 2;
	switch (result)
	{
		case 0: _snprintf(buf, 512, "UPDATE=ERROR;DEPOSIT=ERROR;LOGIN=%d", user.login); break;
		case 1: _snprintf(buf, 512, "UPDATE=ERROR;DEPOSIT=OK;LOGIN=%d", user.login); break;
		case 2: _snprintf(buf, 512, "UPDATE=OK;DEPOSIT=ERROR;LOGIN=%d", user.login); break;
		case 3: _snprintf(buf, 512, "UPDATE=OK;DEPOSIT=OK;LOGIN=%d", user.login); break;
		default: 
			_snprintf(buf, 512, "RESULT=%d;LOGIN=%d", result, user.login); 
			break;
	}

	strcpy(destBuffer, buf);
	return 1;	
}
//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
int CProcessor::ProcessHistoryRequest(const ULONG ip, char *buffer, 
									  const int size, char *destBuffer)
{
	// пример ордера
	// ORDERHISTORY MASTER=4000001|IP=127.0.0.1|GROUP=fxi_hedged|LOGIN=40000009|PRICEOP=1.21320
	// |TIMEOP=30185679|SIDE=-1|VOLUME=10|SYMBOL=EURUSD|TIMECLOS=30189120|PRICECLOS=1.19846
	// установить параметры
	char symbol[20] = { 0 }, group[32] = { 0 };
	double priceOpen = 0, priceClose = 0;
	int side = 0, volume = 0;
	long timeOpen = 0, timeClose = 0;
	int login = 0, master = 0;

	GetIntParam(buffer, "MASTER=", &master);
	GetIntParam(buffer, "LOGIN=", &login);
	GetFltParam(buffer, "PRICEOP=", &priceOpen);
	GetFltParam(buffer, "PRICECLOS=", &priceClose);	
	GetStrParam(buffer, "SYMBOL=", symbol, sizeof(symbol));
	GetLongParam(buffer, "TIMEOP=", &timeOpen);
	GetLongParam(buffer, "TIMECLOS=", &timeClose);
	GetIntParam(buffer, "SIDE=", &side);
	GetIntParam(buffer, "VOLUME=", &volume);
	GetStrParam(buffer, "GROUP=", group, sizeof(group));		

	int rst = AddHistoryDeal(login, priceOpen, (time_t)timeOpen, symbol, side, 
		volume, priceClose, timeClose);
	sprintf(destBuffer, "ProcessHistoryRequest(m=%d, log=%d, smb=%s, open=%5.4f, side=%d, volume=%d, close=%5.4f) : [%d]",
		master, login, symbol, priceOpen, side, volume, priceClose, rst);

	return rst;
}
//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
char *CProcessor::GetPositionsInfo(int *logins, int count, time_t start, time_t end, int openOnly,
								   int *resultSize)
{
	Out(CmdOK, PROGRAM_TITLE, "GetPositionsInfo::from (%s)", ctime(&start));
	Out(CmdOK, PROGRAM_TITLE, "%d logins passed", count);
	
	int total;
	TradeRecord* recs = 0;
	
	if (!openOnly)
	{
		recs = ExtServer->OrdersGet(start, end, logins, count, &total);
	}
	else
	{
		UserInfo inf = { 0 };
		GetUserInfoByLogin(logins[0], &inf);
		Out(CmdOK, PROGRAM_TITLE, "GetUserInfoByLogin::OK"); // debug
		recs = ExtServer->OrdersGetOpen(&inf, &total);
		Out(CmdOK, PROGRAM_TITLE, "OrdersGetOpen::%d orders", total); // debug
	}

	char *destBuffer = new char[total * 165];
	destBuffer[0] = 0;
	int length = 0;

	if (recs != 0)
	for (int i = 0; i < total; i++)
	{
		if (recs[i].cmd != 0 && recs[i].cmd != 1) continue;		

		length += sprintf(destBuffer + length,
			"LOG=%d;ORD=%d;OP=%7.5f;CLOS=%7.5f;VOL=%d;TMOP=%u;TMCLOS=%u;SMB=%s;CMT=%s;SIDE=%s;SL=%7.5f;TP=%7.5f;#;",
			recs[i].login,
			recs[i].order,
			recs[i].open_price,
			recs[i].close_price,
			recs[i].volume,
			recs[i].open_time,
			recs[i].close_time,
			recs[i].symbol,
			recs[i].comment,
			recs[i].cmd == OP_BUY ? "1" : "-1",
			recs[i].sl,
			recs[i].tp);		
	}

	if (recs != 0)	HEAP_FREE(recs);

	char *buff = (char*)HEAP_ALLOC(length + 1);
	strcpy(buff, destBuffer);
	*resultSize = length;
	delete []destBuffer;

	return buff;
}

//

//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
void CProcessor::Out(const int code, LPCSTR ip, LPCSTR msg, ...) const
  {
   char buffer[1024];
//---- check
   if(ExtServer==NULL || msg==NULL) return;
//---- format string
   va_list arg_ptr;
   va_start(arg_ptr,msg);
   _vsnprintf(buffer,sizeof(buffer)-1,msg,arg_ptr);
   va_end(arg_ptr);
//---- out to server log
   ExtServer->LogsOut(code,ip,buffer);
  }
//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
int CProcessor::MapGroup(const int index,char *group)
  {
//---- check
   if(index<1 || group==NULL) return(FALSE);
//---- lock
   m_sync.Lock();
//---- check
   if(index>m_groups_total || m_groups==NULL) 
     {
      m_sync.Unlock();
      return(FALSE);
     }
//---- return group name by index
   strcpy(group,m_groups[index-1].group);
//---- ok
   m_sync.Unlock();
   return(TRUE);
  }

//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
int CProcessor::CheckFlooder(LPCSTR ip)
  {
   Flooder *temp,flooder;
   time_t   currtime;
   int      i;
//---- check
   if(ExtServer==NULL || ip==NULL) return(FALSE);
//---- get current time
   currtime=ExtServer->TradeTime();
//---- lock
   m_sync.Lock();
//---- refresh flooders list
   if(m_flooders!=NULL && m_flooders_total>0)
     {
      //---- check: may be somebody can delete
      for(i=0,temp=m_flooders;i<m_flooders_total;i++,temp++)
        {
         //---- we should delete the oldest flooders
         if(currtime-temp->lasttime > DELETE_FLOODER_SECONDS * 5)
           {
            //---- delete from buffer
            memmove(temp,temp+1,sizeof(Flooder)*(m_flooders_total-i-1));
            //---- correct counters
            m_flooders_total--; i--; temp--;
           }
        }
     }
//---- try find IP
   if((temp=(Flooder *)bsearch(ip,m_flooders,m_flooders_total,sizeof(Flooder),FloodersSorts)) == NULL)
     {
      //---- check space
      if(m_flooders==NULL || m_flooders_total>=m_flooders_max)
        {
         //---- allocate new buffer
         if((temp=new Flooder[m_flooders_total+1024])==NULL)
           {
            m_sync.Unlock();
            Out(CmdAtt, PROGRAM_TITLE, "not enough memory for flooders [%d]", m_flooders_total + 1024);
            return(FALSE);
           }
         //---- copy all from old buffer to new and delete old buffer
         if(m_flooders!=NULL)
           {
            memcpy(temp,m_flooders,sizeof(Flooder)*m_flooders_total);
            delete[] m_flooders;
           }
         //---- set new buffer
         m_flooders    =temp;
         m_flooders_max=m_flooders_total+1024;
        }
      //---- prepare flooder
      flooder.lasttime=currtime;
      COPY_STR(flooder.ip,ip);
      //---- insert flooder in the list
      if(insert(m_flooders,&flooder,m_flooders_total,sizeof(Flooder),FloodersSorts)!=NULL)
         m_flooders_total++;
      //---- all right
      m_sync.Unlock();
      return(TRUE);
     }
//---- check: may be IP is blocked?
   if((currtime-temp->lasttime)<m_antifloodPeriod)
     { 
      m_sync.Unlock(); 
      return(FALSE); 
     }
//---- set last time
   temp->lasttime=currtime;
//---- ok
   m_sync.Unlock();
   return(TRUE);
  }
//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
int CProcessor::FloodersSorts(const void *param1,const void *param2)
  {
   return strcmp(((Flooder *)param1)->ip,((Flooder *)param2)->ip);
  }
//+------------------------------------------------------------------+
//| возвращает номер открытой позиции или 0                          |
//+------------------------------------------------------------------+
int CProcessor::OrdersAdd(int login, int side, const char* smb, 
	double price, int volume, int needClose)
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
	int rst = ExtServer->OrdersOpen(&tr, &us);			
	if (!needClose) return rst;
	if (rst == 0) return rst;

	// тут же закрыть	
	return ExtServer->OrdersClose(&tr, &us);	
}

//+------------------------------------------------------------------+
//| Закрыть сделку у инвестора                                       |
//+------------------------------------------------------------------+
int CProcessor::OrdersClose(int login, int order, double price)
{
	ExtServer->LogsOut(CmdOK, PROGRAM_TITLE, "Closing order");
	// получить ордер
	TradeRecord record = { 0 };
	if (!ExtServer->OrdersGet(order, &record)) return FALSE;
	// получить пользователя
	UserInfo us = { 0 };
	GetUserInfo(login, &us);
	// заполнить запись
	TradeTransInfo  tr = { 0 };	
	tr.type   = TT_ORDER_MK_CLOSE;
	tr.order  = order;
	tr.price  = price;
	tr.cmd    = record.cmd;
	tr.volume = record.volume;
	// закрыть позу
	return ExtServer->OrdersClose(&tr, &us);
}

//+------------------------------------------------------------------+
//| Получить по логину инф. по пользователю (сокращенную)            |
//+------------------------------------------------------------------+
int CProcessor::GetUserInfo(int user_id, UserInfo *us)
{
	if(!us || !ExtServer) return FALSE;
	UserRecord ur = { 0 };
	memset(us, 0, sizeof(UserInfo));
	if (ExtServer->ClientsUserInfo(user_id, &ur))
	{
		strcpy(us->name, ur.name);
		us->agent_account = ur.agent_account;
		us->balance = ur.balance;
		us->credit = ur.credit;
		us->enable = ur.enable;
		us->login = ur.login;
		strcpy(us->group, ur.group);
		if (!ExtServer->GroupsGet(ur.group, &us->grp)) 
			return FALSE;
		us->enable_change_password = ur.enable_change_password;
		us->enable_read_only = ur.enable_read_only;
		strcpy(us->password, ur.password);
		us->prevbalance = ur.prevbalance;  
		us->leverage = ur.leverage;
		return TRUE;
	}
	return FALSE;
}


int CProcessor::AddHistoryDeal(int userId, double openPrice, time_t timeOpen, 
				   const char* smb, int side, int volume, double closePrice, time_t timeClose)
{
	// получить символ
	ConSymbol symb = { 0 };
	if (!ExtServer->SymbolsGet(smb, &symb))
	{
		ExtServer->LogsOut(CmdOK, PROGRAM_TITLE, "Adding history record - couldn't get symbol info");
		return FALSE;
	}
	
	// получить пользователя
	UserInfo us = { 0 };
	if (!GetUserInfo(userId, &us))
	{
		ExtServer->LogsOut(CmdOK, PROGRAM_TITLE, "Adding history record - couldn't get user info");
		return FALSE;
	}	
	
	// новая запись
	TradeRecord order = { 0 };
	order.open_price = openPrice;
	order.open_time = timeOpen;
	order.cmd = side > 0 ? OP_BUY : OP_SELL;
	order.volume = volume;
	order.value_date = timeOpen;
	strcpy(order.comment, "[history deal]");
	order.login = userId;
	strcpy(order.symbol, smb);	
	
	int rst = ExtServer->OrdersAdd(&order, &us, &symb);	

	char msg[256];
	_snprintf(msg, 255,
		"AddHistoryDeal: %d, user=%d, side=%d, smb=%s",
		rst,		
		userId,
		side,
		smb);
	Out(CmdOK, PROGRAM_TITLE, msg);
	// ордер создан - попытка закрыть, при необходимости
	if (rst && closePrice != 0)
	{
		order.order = rst;
		order.close_price = closePrice;
		order.close_time = timeClose;		

		order.conv_rates[1] = order.close_price; // for X/USD		

		ExtServer->TradesCalcProfit(us.group, &order);
		ConSymbol   symcfg  ={0};
		// convertation rates from profit currency to group deposit currency
        // (first element-for open time, second element-for close time)
		order.conv_rates[1] = ExtServer->TradesCalcConvertation(us.group, FALSE, order.close_price, &symcfg);
		ExtServer->TradesCommissionAgent(&order, &symcfg, &us);

		int rstUpdate = ExtServer->OrdersUpdate(&order, &us, UPDATE_CLOSE);
		Out(CmdOK, PROGRAM_TITLE, "AddHistoryDeal:order [%d] update: %d", rst, rstUpdate);
	}

	return rst;
}