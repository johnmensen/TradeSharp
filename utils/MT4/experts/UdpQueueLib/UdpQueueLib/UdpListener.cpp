#include "stdafx.h"
#include <winsock2.h>

#include "UdpListener.h"
#include "CharString.h"

#define MAXDATASIZE 2048

//#define DEBUG_MESSAGE_MODE

int UdpListener::lastErrorCode = -1;

void UdpListener::SetupWSA()
{
	// init win sock library
    WSADATA wsa_data;
	lastErrorCode = WSAStartup(MAKEWORD(1, 1), &wsa_data);
}

void UdpListener::TeardownWSA()
{
	WSACleanup();
}

UdpListener::UdpListener(const char *ip, int port, ON_RECV onRecv)
{
	strcpy(m_ip, ip);
	m_port = port;
	m_onRecv = onRecv;
	m_hShutdownCompletedEvent = CreateEvent(0, 0, 0, _T("eventShutdownCompleted"));
	m_hSocket = 0;	
}

UdpListener::~UdpListener()
{	
	CloseHandle(m_hShutdownCompletedEvent);	
}

void UdpListener::Listen()
{	
	DWORD threadId;
	m_hThread = CreateThread(0, 0, 
		(LPTHREAD_START_ROUTINE)ThreadProc, this, 0, &threadId);		
}

DWORD WINAPI UdpListener::ThreadProc(LPVOID lpParameter)
{	
	UdpListener* thisObj = (UdpListener*) lpParameter;
	CharString str;
	    
    char buffer[MAXDATASIZE];
    
    // create socket
    if ((thisObj->m_hSocket = socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP)) == INVALID_SOCKET)
    {
        // Failed creating socket
		SetEvent(thisObj->m_hShutdownCompletedEvent);
		lastErrorCode = -200;
		return lastErrorCode;
    }
	
	sockaddr_in local_addr;
    local_addr.sin_family=AF_INET;
	local_addr.sin_addr.s_addr=inet_addr(thisObj->m_ip);
	local_addr.sin_port=htons(thisObj->m_port);

    if (bind(thisObj->m_hSocket,(sockaddr *) &local_addr,
        sizeof(local_addr)))
    {		
		closesocket(thisObj->m_hSocket);
		WSACleanup();
		SetEvent(thisObj->m_hShutdownCompletedEvent);		
		lastErrorCode = -300;
		return lastErrorCode;
    }	
    
	while (1)
	{
		int retVal = recv(thisObj->m_hSocket, buffer, MAXDATASIZE, 0);
		if (retVal == 0) break;
		if (retVal < 0)
		{
			int erCode = WSAGetLastError();			
			if (erCode == WSAEINTR) break;
			else
			{
				lastErrorCode = erCode;
			}
		}
		if (retVal > 0)
		{
			//buffer[retVal] = 0;
			(*thisObj->m_onRecv)((TCHAR *)buffer, retVal / sizeof(TCHAR));
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