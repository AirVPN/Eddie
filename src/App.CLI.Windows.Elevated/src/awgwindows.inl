#include <memory>

void IWindows::startawg(const std::string& commandid, std::map<std::string, std::string>& params)
{
	std::string id = params["id"];
	std::string stopkey = "amneziawg_stop_" + id;
	if (params["action"] == "stop")
	{
		std::lock_guard<std::mutex> lock(awgmutex);
		awgstoprequests[stopkey] = true;
		return;
	}
	if (params["action"] != "start") ThrowException("invalid amneziawg action");
	std::string name = params["interface"];
	if (name.empty() || name.size() > 32 || StringEnsureAsciiName(name) != name) ThrowException("invalid amneziawg interface name");
	std::string config = params["config"];
	if (config.size() > 65536) ThrowException("amneziawg configuration is too large");
	std::string error = CheckValidWireGuardConfig(config);
	if (!error.empty()) ThrowException("unsupported amneziawg configuration: " + error);
	std::string executable = FsLocateExecutable("amneziawg-eddie.exe", true, true);
	unsigned long first = StringToULong(params["handshake_timeout_first"]);
	unsigned long connected = StringToULong(params["handshake_timeout_connected"]);
	if (first == 0 || connected == 0) ThrowException("invalid amneziawg handshake timeout");
	HANDLE inputread = NULL, inputwrite = NULL, outputread = NULL, outputwrite = NULL, job = NULL;
	PROCESS_INFORMATION process = {};
	STARTUPINFOEXW startup = {};
	std::vector<BYTE> attributes;
	bool initialized = false;
	std::shared_ptr<void> cleanup(NULL, [&](void*)
	{
		if (inputwrite) CloseHandle(inputwrite);
		if (process.hProcess && WaitForSingleObject(process.hProcess, 10000) != WAIT_OBJECT_0) TerminateProcess(process.hProcess, 1);
		if (process.dwProcessId) PidRemove(process.dwProcessId);
		if (process.hThread) CloseHandle(process.hThread);
		if (process.hProcess) CloseHandle(process.hProcess);
		if (job) CloseHandle(job);
		if (inputread) CloseHandle(inputread);
		if (outputread) CloseHandle(outputread);
		if (outputwrite) CloseHandle(outputwrite);
		if (initialized) DeleteProcThreadAttributeList(startup.lpAttributeList);
		std::lock_guard<std::mutex> lock(awgmutex);
		awgstoprequests.erase(stopkey);
	});
	try
	{
		ReplyCommand(commandid, "log:setup-start");
		SECURITY_ATTRIBUTES security = { sizeof(SECURITY_ATTRIBUTES), NULL, TRUE };
		if (!CreatePipe(&inputread, &inputwrite, &security, 0) || !CreatePipe(&outputread, &outputwrite, &security, 0)) ThrowException("cannot create amneziawg pipes");
		if (!SetHandleInformation(inputwrite, HANDLE_FLAG_INHERIT, 0) || !SetHandleInformation(outputread, HANDLE_FLAG_INHERIT, 0)) ThrowException("cannot secure amneziawg pipe handles");
		SIZE_T size = 0;
		InitializeProcThreadAttributeList(NULL, 1, 0, &size);
		attributes.resize(size);
		startup.lpAttributeList = reinterpret_cast<LPPROC_THREAD_ATTRIBUTE_LIST>(attributes.data());
		if (!InitializeProcThreadAttributeList(startup.lpAttributeList, 1, 0, &size)) ThrowException("cannot initialize amneziawg process attributes");
		initialized = true;
		HANDLE inherited[] = { inputread, outputwrite };
		if (!UpdateProcThreadAttribute(startup.lpAttributeList, 0, PROC_THREAD_ATTRIBUTE_HANDLE_LIST, inherited, sizeof(inherited), NULL, NULL)) ThrowException("cannot restrict amneziawg inherited handles");
		startup.StartupInfo.cb = sizeof(startup);
		startup.StartupInfo.dwFlags = STARTF_USESTDHANDLES;
		startup.StartupInfo.hStdInput = inputread;
		startup.StartupInfo.hStdOutput = outputwrite;
		startup.StartupInfo.hStdError = outputwrite;
		job = CreateJobObjectW(NULL, NULL);
		JOBOBJECT_EXTENDED_LIMIT_INFORMATION limits = {};
		limits.BasicLimitInformation.LimitFlags = JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE;
		if (!job || !SetInformationJobObject(job, JobObjectExtendedLimitInformation, &limits, sizeof(limits))) ThrowException("cannot create amneziawg lifetime job");
		std::wstring command = L"\"" + StringUTF8ToWString(executable) + L"\" " + StringUTF8ToWString(name) + L" " + std::to_wstring(first) + L" " + std::to_wstring(connected);
		std::wstring directory = StringUTF8ToWString(FsFileGetDirectory(executable));
		if (!CreateProcessW(StringUTF8ToWString(executable).c_str(), &command[0], NULL, NULL, TRUE, CREATE_NO_WINDOW | CREATE_SUSPENDED | EXTENDED_STARTUPINFO_PRESENT, NULL, directory.c_str(), &startup.StartupInfo, &process)) ThrowException("cannot start amneziawg engine: " + GetLastErrorAsString());
		if (!AssignProcessToJobObject(job, process.hProcess)) ThrowException("cannot assign amneziawg engine lifetime");
		if (ResumeThread(process.hThread) == (DWORD)-1) ThrowException("cannot resume amneziawg engine");
		CloseHandle(inputread); inputread = NULL;
		CloseHandle(outputwrite); outputwrite = NULL;
		PidAdd(process.dwProcessId);
		std::string body = StringBase64Encode(config) + "\n";
		DWORD written = 0;
		if (!WriteFile(inputwrite, body.data(), (DWORD)body.size(), &written, NULL) || written != body.size()) ThrowException("cannot send amneziawg configuration");
		std::string pending;
		bool stopping = false;
		ULONGLONG stopstarted = 0;
		for (;;)
		{
			DWORD available = 0;
			if (PeekNamedPipe(outputread, NULL, 0, NULL, &available, NULL) && available > 0)
			{
				char buffer[4096];
				DWORD received = 0;
				if (ReadFile(outputread, buffer, (DWORD)std::min<size_t>(sizeof(buffer), available), &received, NULL)) pending.append(buffer, received);
				if (pending.size() > 1048576) ThrowException("amneziawg engine output exceeded limit");
				size_t newline;
				while ((newline = pending.find('\n')) != std::string::npos)
				{
					std::string line = StringTrim(pending.substr(0, newline));
					pending.erase(0, newline + 1);
					if (StringStartsWith(line, "error:")) ReplyCommand(commandid, "err:" + line.substr(6));
					else if (!line.empty()) ReplyCommand(commandid, "log:" + line);
				}
				continue;
			}
			if (WaitForSingleObject(process.hProcess, 0) == WAIT_OBJECT_0)
			{
				DWORD exitcode = 0;
				GetExitCodeProcess(process.hProcess, &exitcode);
				if (!stopping && exitcode != 0) ReplyCommand(commandid, "err:amneziawg engine exited with code " + std::to_string(exitcode));
				break;
			}
			bool requested = false;
			{
				std::lock_guard<std::mutex> lock(awgmutex);
				requested = awgstoprequests.find(stopkey) != awgstoprequests.end();
			}
			if (!stopping && (requested || !clientconnected() || IsStopRequested()))
			{
				stopping = true;
				stopstarted = GetTickCount64();
				CloseHandle(inputwrite); inputwrite = NULL;
			}
			if (stopping && GetTickCount64() - stopstarted > 10000)
			{
				ReplyCommand(commandid, "err:amneziawg shutdown timed out");
				TerminateJobObject(job, 1);
				break;
			}
			Sleep(100);
		}
	}
	catch (const std::exception& exception)
	{
		ReplyCommand(commandid, "err:" + StringTrim(exception.what()));
	}
	cleanup.reset();
	ReplyCommand(commandid, "log:stop");
}
