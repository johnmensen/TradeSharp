#include <winsock2.h>
#include "stdafx.h"
#include "UdpListener.h"
#include "Logger.h"
#include "CharString.h"

#define MAXDATASIZE 1024

//#define DEBUG_MESSAGE_MODE

void UdpListener::SetupWSA()
{
	// init win sock library
    WSADATA wsa_data;
	WSAStartup(MAKEWORD(1, 1), &wsa_data);    
}

void UdpListener::TeardownWSA()
{
	WSACleanup();
}

UdpListener::UdpListener(char *ip, int port, ON_RECV onRecv)
{
	strcpy(m_ip, ip);
	m_port = port;
	m_onRecv = onRecv;
	m_hShutdownCompletedEvent = CreateEvent(0, 0, 0, "eventShutdownCompleted");
	m_hSocket = 0;	
}

UdpListener::~UdpListener()
{	
	CloseHandle(m_hShutdownCompletedEvent);	
}

void UdpListener::Listen()
{	
	ExtLogger.Out("UdpListener::Listen()");
	DWORD threadId;
	m_hThread = CreateThread(0, 0, 
		(LPTHREAD_START_ROUTINE)ThreadProc, this, 0, &threadId);		
}

DWORD WINAPI UdpListener::ThreadProc(LPVOID lpParameter)
{	
	UdpListener* thisObj = (UdpListener*) lpParameter;
	    
    char                buffer[MAXDATASIZE + 1];        
    
    // create socket
    if ((thisObj->m_hSocket = socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP)) == INVALID_SOCKET)
    {
        // Failed creating socket
		SetEvent(thisObj->m_hShutdownCompletedEvent);
		ExtLogger.Out("socket was not created: %d", WSAGetLastError());
		return -200;
    }	
	
	sockaddr_in local_addr;
    local_addr.sin_family=AF_INET;
    local_addr.sin_addr.s_addr=INADDR_ANY;
	local_addr.sin_port=htons(thisObj->m_port);

    if (bind(thisObj->m_hSocket,(sockaddr *) &local_addr,
        sizeof(local_addr)))
    {		
		closesocket(thisObj->m_hSocket);
		WSACleanup();
		SetEvent(thisObj->m_hShutdownCompletedEvent);
		ExtLogger.Out("socket was not bound: %d", WSAGetLastError());
		return -300;
    }	
    
	while (1)		
	{
#ifdef DEBUG_MESSAGE_MODE
		CharString str;
		str.Printf("Listening on port %d", thisObj->m_port);
		Logger::LogMessage(str.ToArray());
#endif

		int retVal = recv(thisObj->m_hSocket, buffer, MAXDATASIZE, 0);
		if (retVal == 0) break;
		if (retVal < 0)
		{
			int erCode = WSAGetLastError();			
			if (erCode == WSAEINTR) break;
			else
			{
				CharString errStr;
				errStr.Printf("Error code is %d", erCode);
				ExtLogger.Out(CharString(errStr).ToArray());
			}
		}
		if (retVal > 0)
		{
			buffer[retVal] = 0;
			(*thisObj->m_onRecv)(buffer, retVal);
		}
	}

    // close socket
    if (thisObj->m_hSocket > 0)
		closesocket(thisObj->m_hSocket);        
    SetEvent(thisObj->m_hShutdownCompletedEvent);
	return 0;
}

void UdpListener::Stop()
{
	if (m_hSocket == 0) return;
	closesocket(m_hSocket);
	WaitForSingleObject(m_hShutdownCompletedEvent, 6000);
	closesocket(m_hSocket);
}