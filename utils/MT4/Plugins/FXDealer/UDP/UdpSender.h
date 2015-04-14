#pragma once
#include "..\charstring.h"

class UdpSender
{
private:
	UINT_PTR m_hSocket;

public:
	UdpSender();
	~UdpSender();
	int SendTo(CharString *str, char *adr, int port);
};