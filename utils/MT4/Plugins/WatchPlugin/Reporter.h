#pragma once
#include "UdpSender.h"

class Reporter
{
private:
	// порт отправки
	int				m_nPort;
	// адрес доставки
	char			m_address[64];
	// интервал между отправками, милисекунд
	int				m_nInterval;
	// код (в сообщении)
	char			m_Code[64];
	UdpSender		m_sender;
	// поток отправки
	HANDLE			m_hThread;	
	// признак остнова
	int				m_serviceStopping;

public:
	Reporter();
	~Reporter();
	// прочитать настройки
	void			Initialize();
	// потоковый опрос состояния и оповещение
	static UINT __stdcall ThreadFunction(LPVOID param);
};

extern Reporter *extReporter;