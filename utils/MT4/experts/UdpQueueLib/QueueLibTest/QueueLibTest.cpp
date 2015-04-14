// QueueLibTest.cpp : Defines the entry point for the console application.
//
#include "stdafx.h"
#include <Windows.h>
#include <string.h>
#include <conio.h>

BOOL (__stdcall *startListen)(int port);
BOOL (__stdcall *startListenAddr)(const TCHAR *addr, int port);
BOOL (__stdcall *stopListen)();
TCHAR *(__stdcall *pickMessage)();
int (__stdcall *sendMessageUDP)(const TCHAR *str, const TCHAR *addr, int port);

bool AnsiToUnicode16(const CHAR *in_Src, WCHAR *out_Dst, int in_MaxLen)
{
    int lv_Len;
	if (in_MaxLen <= 0)
		return false;
	// let windows find out the meaning of ansi
	// - the SrcLen=-1 triggers MBTWC to add a eos to Dst and fails if MaxLen is too small.
	// - if SrcLen is specified then no eos is added
	// - if (SrcLen+1) is specified then the eos IS added
	lv_Len = MultiByteToWideChar(CP_ACP, 0, in_Src, -1, out_Dst, in_MaxLen);
	// validate
	if (lv_Len < 0)
		lv_Len = 0;
	// ensure eos, watch out for a full buffersize
	// - if the buffer is full without an eos then clear the output like MBTWC does
	//   in case of too small outputbuffer
	// - unfortunately there is no way to let MBTWC return shortened strings,
	//   if the outputbuffer is too small then it fails completely
	if (lv_Len < in_MaxLen)
		out_Dst[lv_Len] = 0;
	else if (out_Dst[in_MaxLen-1])
		out_Dst[0] = 0;
	return true;
}

CHAR *Unicode16ToAnsi(const WCHAR *src)
{
	int len = WideCharToMultiByte(CP_ACP, 0, src, -1, NULL, 0, NULL, NULL);
	char *result = new char[len + 1];
	WideCharToMultiByte(CP_ACP, 0, src, -1, result, len, NULL, NULL);
	return result;
}

HMODULE LoadLib()
{
	HMODULE dllp = LoadLibrary(_T("UdpQueueLib.dll"));
	if (dllp == NULL) 
	{
		printf("error loading lib\n");
		return NULL;
	}
	startListen = (BOOL(__stdcall *) (int))	GetProcAddress(dllp, "StartListen");
	startListenAddr = (BOOL(__stdcall *) (const TCHAR *, int)) GetProcAddress(dllp, "StartListenAddr");
	stopListen = (BOOL(__stdcall *) ())	GetProcAddress(dllp, "StopListen");
	pickMessage = (TCHAR *(__stdcall *) ())	GetProcAddress(dllp, "PickMessage");
	sendMessageUDP = (int (__stdcall *) (const TCHAR *, const TCHAR *, int)) GetProcAddress(dllp, "SendMessageUDP");
	if (startListen == NULL || startListenAddr == NULL || stopListen == NULL || pickMessage == NULL || sendMessageUDP == NULL)
	{
		printf("error importing functs from lib\n");
		return NULL;
	}
	return dllp;
}

void TestRecv(const TCHAR *addr, int port)
{
	HMODULE dllp = LoadLib();
	if (dllp == NULL)
		return;

	_tprintf(_T("listening %s:%d\n"), addr, port);
	startListenAddr(addr, port);
	_tprintf(_T("listen port - OK\n"));
	
	while (1)
	{
		TCHAR *data = pickMessage();
		if (data != 0)
	    {
			if(data[0] != 0)
				_tprintf(_T("%s\n"), data);
			delete[] data;
		}
	}

	_tprintf(_T("stopping listening\n"));
	stopListen();
	_tprintf(_T("stopping listening - OK\n"));

	FreeLibrary(dllp);
}

void TestSend(const TCHAR *addr, int port)
{
	int iResult;
	WSADATA wsaData;
	iResult = WSAStartup(MAKEWORD(1,1), &wsaData);
	if (iResult != NO_ERROR)
	{
	    _tprintf(_T("WSAStartup error: %d\n"), WSAGetLastError());
		return;
	}
	struct sockaddr_in clientService;
	clientService.sin_family = AF_INET;
	char *addrAnsi;
#ifdef UNICODE
	addrAnsi = Unicode16ToAnsi(addr);
#else
	addrAnsi = (char *)addr;
#endif
	clientService.sin_addr.s_addr = inet_addr(addrAnsi);
	clientService.sin_port = htons(port);
	SOCKET sendSocket = socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP);
#ifdef UNICODE
	delete[] addrAnsi;
#endif
	if (sendSocket == INVALID_SOCKET)
	{
		_tprintf(_T("create socket error: %d\n"), WSAGetLastError());
		return;
	}
	while(1)
	{
		TCHAR *sendbuf = _T("Client: sending data test");
		int len = (int) (_tcslen(sendbuf) + 1) * sizeof(TCHAR);
		iResult = sendto(sendSocket, (char *)sendbuf, len, 0, (SOCKADDR *) &clientService, sizeof(clientService));
		if (iResult == SOCKET_ERROR)
		{
			_tprintf(_T("send failed with error: %d\n"), WSAGetLastError());
			return;
		}
		else if (iResult != len)
		{
			_tprintf(_T("send error: bytes send: %d\n"), iResult);
		}
	}
	closesocket(sendSocket);
	WSACleanup();
}

void TestSend2(const TCHAR *addr, int port)
{
	HMODULE dllp = LoadLib();
	if (dllp == NULL)
		return;
	while(1)
	{
		int iResult = sendMessageUDP(_T("Client: sending data test"), addr, port);
		if (iResult != 0)
		{
			_tprintf(_T("send failed with error %d\n"), iResult);
			return;
		}
	}
	FreeLibrary(dllp);
}

int _tmain(int argc, _TCHAR* argv[])
{
	char addr[128], cmd[128];
	int port;

	scanf_s("%s", cmd, _countof(cmd));
	if (strcmp(cmd, "q") == 0)
		return 0;
	if(strcmp(cmd, "r") != 0 && strcmp(cmd, "s") != 0 && strcmp(cmd, "s2") != 0)
	{
		printf("unrecognized command");
		scanf_s("%s", cmd, _countof(cmd));
		return 0;
	}
	printf("address & port: ");
	scanf_s("%s %d", addr, _countof(addr), &port);
	TCHAR *addrTChar;
#ifdef UNICODE
	WCHAR addrWChar[128];
	AnsiToUnicode16(addr, addrWChar, 128);
	addrTChar = addrWChar;
#else
	addrTChar = addr;
#endif
	if (strcmp(cmd, "r") == 0)
		TestRecv(addrTChar, port);
	else if (strcmp(cmd, "s") == 0)
		TestSend(addrTChar, port);
	else if (strcmp(cmd, "s2") == 0)
		TestSend2(addrTChar, port);
	_getch();
	return 0;
}
