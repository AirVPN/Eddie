// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2017 AirVPN (support@airvpn.org) / https://airvpn.org
//
// Eddie is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Eddie is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Eddie. If not, see <http://www.gnu.org/licenses/>.
// </eddie_source_header>

#include "stdafx.h"
#include "api.h"

#include <linux/fs.h>
#include <signal.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <sys/ioctl.h>
#include <sys/stat.h>

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifdef __cplusplus
extern "C" {
#endif

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

e_uint32 eddie_linux_get_3()
{
	return 3;
}

int eddie_file_get_mode(const char *filename)
{
    struct stat s;
    EDDIE_ZEROMEMORY(&s, sizeof(struct stat));

    if(stat(filename, &s) == -1)
        return -1;

    return (int) s.st_mode;
}

int eddie_file_set_mode(const char *filename, int mode)
{
    return chmod(filename, (mode_t) mode);
}

int eddie_file_set_mode_str(const char *filename, const char *mode)
{
    return eddie_file_set_mode(filename, (int) strtol(mode, NULL, 8));
}

int eddie_file_get_immutable(const char *filename)
{
    FILE *fp;
    if((fp = fopen(filename, "r")) == NULL)
        return -1;

    int result = -1;

    int attr = 0;
    if(ioctl(fileno(fp), FS_IOC_GETFLAGS, &attr) != -1)
        result = (attr & FS_IMMUTABLE_FL) == FS_IMMUTABLE_FL;

    fclose(fp);

    return result;
}

int eddie_file_set_immutable(const char *filename, int flag)
{
    FILE *fp;
    if((fp = fopen(filename, "r")) == NULL)
        return -1;

    int fd = fileno(fp);

    int result = -1;

    int attr = 0;
    if(ioctl(fd, FS_IOC_GETFLAGS, &attr) != -1)
    {
        attr = flag ? (attr | FS_IMMUTABLE_FL) : (attr & ~FS_IMMUTABLE_FL);

        if(ioctl(fd, FS_IOC_SETFLAGS, &attr) != -1)
            result = 0;
    }

    fclose(fp);

    return result;
}

void eddie_signal(int signum, eddie_sighandler_t handler)
{
    signal(signum, handler);
}

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifdef __cplusplus
}
#endif

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
