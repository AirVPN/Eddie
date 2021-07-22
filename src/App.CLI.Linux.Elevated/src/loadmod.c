/*
 * loadmod.c
 *
 * This file is part of AirVPN's Linux/MacOS OpenVPN Client software.
 * Copyright (C) 2019 AirVPN (support@airvpn.org) / https://airvpn.org
 *
 * Developed by ProMIND
 *
 * This is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with Eddie. If not, see <http://www.gnu.org/licenses/>.
 *
 * [ProMIND] [20191105] Version 1.0
 *
 */

#include <fcntl.h>
#include <stdio.h>
#include <sys/stat.h>
#include <sys/syscall.h>
#include <sys/types.h>
#include <unistd.h>
#include <stdlib.h>
#include <string.h>
#include <errno.h>
#include <lzma.h> // Under debian, liblzma.dev
#include <ctype.h>
#include <sys/utsname.h>
#include "loadmod.h"

#define init_module(module_module_image, len, param_values) syscall(__NR_init_module, module_module_image, len, param_values)

#define MAX_MODULE_DEP  20

int process_module(const char* module, const char* params);

int load_kernel_module(const char* module, const char* params)
{
	struct utsname uts_uname;
	char lib_module_path[64], module_path[128], module_dep_path[128];
	char line[1024], depmod[MAX_MODULE_DEP][128], * cpos;
	int i, ndep = 0, done = 0, retval;
	FILE* fmodule;

	strcpy(lib_module_path, "/lib/modules/");

	if (uname(&uts_uname) != 0)
		return MODULE_LOAD_ERROR;

	strcat(lib_module_path, uts_uname.release);

	strcpy(module_dep_path, lib_module_path);
	strcat(module_dep_path, "/");
	strcat(module_dep_path, "modules.dep");

	fmodule = fopen(module_dep_path, "r");

	while (fgets(line, sizeof(line), fmodule) && done == 0)
	{
		line[strlen(line) - 1] = '\0';

		cpos = strtok(line, ":");
		ndep = 0;

		while (cpos != NULL && ndep < MAX_MODULE_DEP)
		{
			strcpy(depmod[ndep++], cpos);

			cpos = strtok(NULL, " ");
		}

		if (strstr(depmod[0], module) != NULL)
			done = 1;
	}

	fclose(fmodule);

	if (ndep >= MAX_MODULE_DEP)
		return MODULE_LOAD_ERROR;

	if (done == 0)
		return MODULE_NOT_FOUND;

	for (i = ndep - 1; i >= 0; i--)
	{
		strcpy(module_path, lib_module_path);
		strcat(module_path, "/");
		strcat(module_path, depmod[i]);

		if (access(module_path, F_OK) != 0)
			return MODULE_NOT_FOUND;

		retval = process_module(module_path, params);

		if (retval != MODULE_LOAD_SUCCESS && retval != MODULE_ALREADY_LOADED)
			return retval;
	}

	return MODULE_LOAD_SUCCESS;
}

int process_module(const char* module, const char* params)
{
	struct stat st;
	int fd, module_is_compressed = 0, retval;
	size_t module_image_size, file_module_image_size, uncompressed_module_image_size;
	unsigned char* module_image = NULL, * file_module_image = NULL, * uncompressed_module_image = NULL;
	lzma_stream lz_stream = LZMA_STREAM_INIT, * ptr_lz_stream;
	lzma_action action = LZMA_RUN;
	lzma_ret ret;

	if (strcmp((char*)(module + strlen(module) - 2), "xz") == 0)
		module_is_compressed = 1;
	else
		module_is_compressed = 0;

	fd = open(module, O_RDONLY);

	fstat(fd, &st);

	if (st.st_size == 0)
	{
		close(fd);

		return MODULE_LOAD_ERROR;
	}

	file_module_image_size = st.st_size;
	file_module_image = (unsigned char*)malloc(file_module_image_size);

	if (read(fd, file_module_image, file_module_image_size) == -1)
	{
		free(file_module_image);

		close(fd);

		return MODULE_LOAD_ERROR;
	}

	close(fd);

	if (module_is_compressed == 1)
	{
		uncompressed_module_image_size = file_module_image_size * 10;

		uncompressed_module_image = (unsigned char*)malloc(uncompressed_module_image_size);

		ptr_lz_stream = &lz_stream;

		ret = lzma_stream_decoder(ptr_lz_stream, UINT64_MAX, LZMA_CONCATENATED);

		if (ret != LZMA_OK)
		{
			switch (ret)
			{
			case LZMA_MEM_ERROR:
			{
				retval = MODULE_LZMA_MEM_ERROR;
			}
			break;

			case LZMA_OPTIONS_ERROR:
			{
				retval = MODULE_LZMA_OPTIONS_ERROR;
			}
			break;

			default:
			{
				retval = MODULE_LZMA_ERROR;
			}
			break;
			}

			free(file_module_image);

			free(uncompressed_module_image);

			return retval;
		}

		action = LZMA_RUN;

		ptr_lz_stream->next_out = (uint8_t*)uncompressed_module_image;
		ptr_lz_stream->avail_out = uncompressed_module_image_size;

		ptr_lz_stream->next_in = (uint8_t*)file_module_image;
		ptr_lz_stream->avail_in = file_module_image_size;

		close(fd);

		ret = lzma_code(ptr_lz_stream, action);

		module_image_size = uncompressed_module_image_size - ptr_lz_stream->avail_out;

		if (ptr_lz_stream->avail_out == 0 && ret != LZMA_STREAM_END)
		{
			free(file_module_image);

			free(uncompressed_module_image);

			return MODULE_OUT_OF_MEMORY;
		}

		action = LZMA_FINISH;

		ret = lzma_code(ptr_lz_stream, action);

		if (ret != LZMA_STREAM_END)
		{
			switch (ret)
			{
			case LZMA_MEM_ERROR:
			{
				retval = MODULE_OUT_OF_MEMORY;
			}
			break;

			case LZMA_FORMAT_ERROR:
			case LZMA_OPTIONS_ERROR:
			case LZMA_DATA_ERROR:
			case LZMA_BUF_ERROR:
			{
				retval = MODULE_LZMA_ERROR;
			}
			break;

			default:
			{
				retval = MODULE_LZMA_ERROR;
			}
			break;
			}

			free(file_module_image);

			free(uncompressed_module_image);

			return retval;
		}

		module_image = uncompressed_module_image;
	}
	else
	{
		module_image = file_module_image;

		module_image_size = file_module_image_size;
	}

	if (init_module(module_image, module_image_size, params) != 0)
	{
		switch (errno)
		{
		case EBADMSG:
		case EBUSY:
		case EFAULT:
		case ENOKEY:
		case EPERM:
		case EINVAL:
		case ENOEXEC:
		{
			retval = MODULE_LOAD_ERROR;
		}
		break;

		case ENOMEM:
		{
			retval = MODULE_OUT_OF_MEMORY;
		}
		break;

		case EEXIST:
		{
			retval = MODULE_ALREADY_LOADED;
		}
		break;

		default:
		{
			retval = MODULE_LOAD_ERROR;
		}
		break;
		}
	}
	else
		retval = MODULE_LOAD_SUCCESS;

	lzma_end(ptr_lz_stream);

	free(file_module_image);

	if (module_is_compressed == 1)
		free(uncompressed_module_image);

	return retval;
}
