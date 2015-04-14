#pragma once

class UdpSender
{
private:
	UINT_PTR m_hSocket;

public:
	UdpSender();
	~UdpSender();
	int SendTo(char *str, int length, char *adr, int port);
};