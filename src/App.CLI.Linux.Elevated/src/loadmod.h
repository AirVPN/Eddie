/*
 * loadmod.h
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

#ifndef LOAD_KERNEL_MODULE_H
#define LOAD_KERNEL_MODULE_H

#define MODULE_LOAD_SUCCESS         1
#define MODULE_LOAD_ERROR           -1
#define MODULE_NOT_FOUND            -2
#define MODULE_ALREADY_LOADED       -3
#define MODULE_LZMA_MEM_ERROR       -4
#define MODULE_LZMA_OPTIONS_ERROR   -5
#define MODULE_LZMA_ERROR           -6
#define MODULE_OUT_OF_MEMORY        -7

/*
 *
 * Function: load_kernel_module
 * ----------------------------
 *  Load a Linux kernel module
 *
 *  interface: int load_kernel_module(const char *module, const char *params)
 *
 *  module: name of module or part of it. In case of partial name, the first
 *          match from modules.dep will be used.
 *  params: optional module parameters
 *
 *  returns: MODULE_LOAD_SUCCESS on success, other MODULE_* in case of error
 *
 */

int load_kernel_module(const char *module, const char *params);

#endif
