#include "stdafx.h"
#include "Reporter.h"
#include "Configuration.h"

Reporter *extReporter;

Reporter::Reporter()
{
	m_nPort = 18500;
	m_nInterval = 10000;
	strcpy(m_Code, "MT4");
	strcpy(m_address, "bmw");
	m_serviceStopping = FALSE;
	
	// запустить поток
	UINT id;	
	if ((m_hThread = (HANDLE)_beginthreadex(NULL, 256000, ThreadFunction, this, 0, &id))==NULL)
	{		
		return;
	}
}

Reporter::~Reporter()
{
	m_serviceStopping = TRUE;
}

// прочитать настройки
void Reporter::Initialize()
{
	ExtConfig.GetInteger("Port", &m_nPort, "18500");
	ExtConfig.GetInteger("Interval ms", &m_nInterval, "10000");	
	ExtConfig.GetString("Code", m_Code, sizeof(m_Code) - 1, "MT4");
	ExtConfig.GetString("Address", m_address, sizeof(m_address) - 1, "worker");
}

UINT __stdcall Reporter::ThreadFunction(LPVOID param)
{
	int interval = 300, counter = 0;
	char message[128] = { 0 };
	strcpy(message, "code=");
	strcat(message, extReporter->m_Code);
	strcat(message, ";status=OK;");
	int msgLen = strlen(message);

	while (!extReporter->m_serviceStopping)
	{
		counter += interval;
		if (counter > extReporter->m_nInterval)
		{
			counter = 0;
			// отправить сообщение
			extReporter->m_sender.SendTo(message, msgLen, extReporter->m_address, extReporter->m_nPort);
		}

		::Sleep(interval);
	}
	return 0;
}