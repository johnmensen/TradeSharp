#include "stdafx.h"
#include "UdpSender.h"
#include <winsock2.h>

int UdpSender::SendTo(char *str, int length, char *adr, int port)
{
	if (m_hSocket == 0)
	{
		if ((m_hSocket = socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP)) == INVALID_SOCKET)
		{			
			m_hSocket = 0;
			return -100;
		}
	}
	
	sockaddr_in local_addr;
    local_addr.sin_family = AF_INET;
    local_addr.sin_addr.s_addr = inet_addr(adr);
	local_addr.sin_port = htons(port);	

	PHOSTENT phe = gethostbyname (adr);	
	memcpy((char FAR *)&(local_addr.sin_addr ), phe->h_addr, phe->h_length);

	int rst = sendto(m_hSocket, 
		str, 
		length, 
		0, 
		(SOCKADDR *) &local_addr, 
		sizeof(local_addr));
	if (rst == SOCKET_ERROR) 
	{		
		closesocket(m_hSocket);
		m_hSocket = 0;
		return -200;
	}

    return TRUE;
}

UdpSender::UdpSender()
{
	m_hSocket = 0;
}

UdpSender::~UdpSender()
{
	if (m_hSocket != 0)
		closesocket(m_hSocket);
}