// dllmain.cpp : Defines the entry point for the DLL application.
#include "stdafx.h"
#include "UdpListener.h"
#include "MessageQueue.h"

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
					 )
{
	switch (ul_reason_for_call)
    {
		case DLL_PROCESS_ATTACH:
			DisableThreadLibraryCalls(hModule);
			extQueue = 0;
			UdpListener::SetupWSA();
			break;

		case DLL_PROCESS_DETACH:
			if (extQueue != NULL)
				delete extQueue;
			//UdpListener::TeardownWSA();		
			break;
	}
	return(TRUE);
}

BOOL APIENTRY StartListen(int port)
{
	if (extQueue != 0) return FALSE;
	extQueue = new MessageQueue(0);
	extQueue->Init("127.0.0.1", port);
	return TRUE;
}

BOOL APIENTRY StartListenAddr(const TCHAR *addr, int port)
{
	if (extQueue != 0) return FALSE;
	extQueue = new MessageQueue(0);
	char *addrAnsi;
#ifdef UNICODE
	addrAnsi = CharString::Unicode16ToAnsi(addr);
#else
	addrAnsi = (char *)addr;
#endif
	extQueue->Init(addrAnsi, port);
#ifdef UNICODE
	delete[] addrAnsi;
#endif
	return TRUE;
}

BOOL APIENTRY StopListen()
{
	if (extQueue != 0) return FALSE;
	delete extQueue;
	extQueue = 0;
	return TRUE;
}

TCHAR *APIENTRY PickMessage()
{
	TCHAR *empty = new TCHAR[2];
	empty[0] = 0;

	if (extQueue == 0) return empty;
	CharString str;
	if (!extQueue->Dequeue(&str)) return empty;
	
	int len = str.GetLength();
	if (len == 0) return empty;

	delete[] empty;
	empty = new TCHAR[len + 1]; 

	str.CopyToBuffer(empty, len + 1);
	return empty;
}

int APIENTRY GetPickFlag()
{
	return UdpListener::lastErrorCode;
}

int APIENTRY SendMessageUDP(const TCHAR *str, const TCHAR *addr, int port)
{
	SOCKET SendSocket;
	sockaddr_in RecvAddr;    

	SendSocket = socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP);

	char *addrAnsi;
#ifdef UNICODE
	addrAnsi = CharString::Unicode16ToAnsi(addr);
#else
	addrAnsi = (char *)addr;
#endif
	RecvAddr.sin_family = AF_INET;
	RecvAddr.sin_addr.s_addr = inet_addr(addrAnsi);
	RecvAddr.sin_port = htons(port);

	int rst = sendto(SendSocket,
		(char *)str,
		(_tcslen(str) + 1) * sizeof(TCHAR),
		0,
		(SOCKADDR *) &RecvAddr,
		sizeof(RecvAddr));

#ifdef UNICODE
	delete[] addrAnsi;
#endif

	if (rst == SOCKET_ERROR)
	{
		closesocket(SendSocket);
		return WSAGetLastError();
	}

	closesocket(SendSocket);
	
	return 0;
}
