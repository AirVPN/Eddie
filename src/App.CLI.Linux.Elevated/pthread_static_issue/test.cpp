#include <iostream> 
#include <string>
#include <map>
#include <unistd.h>
#include <thread>
#include <sys/types.h>

#include "test.h"

void MyTest::Log(std::string msg)
{
	std::cout << msg << "\n";
}

void MyTest::Main()
{
	Log("Main start");

	Log("Main create thread");
	std::thread t = std::thread(ThreadCommand, this);
	
	Log("Main detach thread");
	t.detach();

    usleep(3000000);

	Log("Main end");
}


void ThreadCommand(MyTest* pTest)
{
	pTest->Log("From thread");
}



int main(int argc, char* argv[])
{
	MyTest t;
	t.Main();
}

