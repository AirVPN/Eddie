#include <iostream> 
#include <string>
#include <map>
#include <unistd.h>
#include <sys/types.h>


class MyTest
{
	public:
		void Log(std::string msg);

		void Main();
};

void ThreadCommand(MyTest* pTest);
