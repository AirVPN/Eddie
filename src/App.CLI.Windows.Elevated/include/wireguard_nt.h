/* SPDX-License-Identifier: GPL-2.0 OR MIT
 *
 * Copyright (C) 2018-2026 WireGuard LLC. All Rights Reserved.
 *
 * WireGuardNT public API, verbatim from upstream
 * https://git.zx2c4.com/wireguard-nt/plain/api/wireguard.h
 * Paired with the deployed wireguard.dll (driver loader, signed by WireGuard LLC).
 */

#pragma once

#include <winsock2.h>
#include <windows.h>
#include <ipexport.h>
#include <ifdef.h>
#include <ws2ipdef.h>

#ifdef __cplusplus
extern "C" {
#endif

#ifndef ALIGNED
#    if defined(_MSC_VER)
#        define ALIGNED(n) __declspec(align(n))
#    elif defined(__GNUC__)
#        define ALIGNED(n) __attribute__((aligned(n)))
#    else
#        error "Unable to define ALIGNED"
#    endif
#endif

/* MinGW is missing this one, unfortunately. */
#ifndef _Post_maybenull_
#    define _Post_maybenull_
#endif

#pragma warning(push)
#pragma warning(disable : 4324) /* structure was padded due to alignment specifier */

/**
 * A handle representing WireGuard adapter
 */
typedef struct _WIREGUARD_ADAPTER *WIREGUARD_ADAPTER_HANDLE;

/**
 * Creates a new WireGuard adapter.
 */
typedef _Must_inspect_result_
_Return_type_success_(return != NULL)
_Post_maybenull_
WIREGUARD_ADAPTER_HANDLE(WINAPI WIREGUARD_CREATE_ADAPTER_FUNC)
(_In_z_ LPCWSTR Name, _In_z_ LPCWSTR TunnelType, _In_opt_ const GUID *RequestedGUID);

/**
 * Opens an existing WireGuard adapter.
 */
typedef _Must_inspect_result_
_Return_type_success_(return != NULL)
_Post_maybenull_
WIREGUARD_ADAPTER_HANDLE(WINAPI WIREGUARD_OPEN_ADAPTER_FUNC)(_In_z_ LPCWSTR Name);

/**
 * Releases WireGuard adapter resources and, if adapter was created with WireGuardCreateAdapter, removes adapter.
 */
typedef VOID(WINAPI WIREGUARD_CLOSE_ADAPTER_FUNC)(_In_opt_ WIREGUARD_ADAPTER_HANDLE Adapter);

/**
 * Deletes the WireGuard driver if there are no more adapters in use.
 */
typedef _Return_type_success_(return != FALSE)
BOOL(WINAPI WIREGUARD_DELETE_DRIVER_FUNC)(VOID);

/**
 * Returns the LUID of the adapter.
 */
typedef VOID(WINAPI WIREGUARD_GET_ADAPTER_LUID_FUNC)(_In_ WIREGUARD_ADAPTER_HANDLE Adapter, _Out_ NET_LUID *Luid);

/**
 * Determines the version of the WireGuard driver currently loaded.
 */
typedef _Return_type_success_(return != 0)
DWORD(WINAPI WIREGUARD_GET_RUNNING_DRIVER_VERSION_FUNC)(VOID);

/**
 * Determines the level of logging, passed to WIREGUARD_LOGGER_CALLBACK.
 */
typedef enum
{
    WIREGUARD_LOG_INFO, /**< Informational */
    WIREGUARD_LOG_WARN, /**< Warning */
    WIREGUARD_LOG_ERR   /**< Error */
} WIREGUARD_LOGGER_LEVEL;

/**
 * Called by internal logger to report diagnostic messages
 */
typedef VOID(CALLBACK *WIREGUARD_LOGGER_CALLBACK)(
    _In_ WIREGUARD_LOGGER_LEVEL Level,
    _In_ DWORD64 Timestamp,
    _In_z_ LPCWSTR Message);

/**
 * Sets logger callback function.
 */
typedef VOID(WINAPI WIREGUARD_SET_LOGGER_FUNC)(_In_ WIREGUARD_LOGGER_CALLBACK NewLogger);

/**
 * Whether and how logs from the driver are collected for the callback function.
 */
typedef enum
{
    WIREGUARD_ADAPTER_LOG_OFF,            /**< No logs are generated from the driver. */
    WIREGUARD_ADAPTER_LOG_ON,             /**< Logs are generated from the driver. */
    WIREGUARD_ADAPTER_LOG_ON_WITH_PREFIX  /**< Logs are generated from the driver, index-prefixed. */
} WIREGUARD_ADAPTER_LOG_STATE;

/**
 * Sets whether and how the adapter logs to the logger previously set up with WireGuardSetLogger.
 */
typedef _Return_type_success_(return != FALSE)
BOOL(WINAPI WIREGUARD_SET_ADAPTER_LOGGING_FUNC)
(_In_ WIREGUARD_ADAPTER_HANDLE Adapter, _In_ WIREGUARD_ADAPTER_LOG_STATE LogState);

/**
 * Determines the state of the adapter.
 */
typedef enum
{
    WIREGUARD_ADAPTER_STATE_DOWN, /**< Down */
    WIREGUARD_ADAPTER_STATE_UP,   /**< Up */
} WIREGUARD_ADAPTER_STATE;

/**
 * Sets the adapter state of the WireGuard adapter. Note: sockets are owned by the process that sets the state to up.
 */
typedef _Return_type_success_(return != FALSE)
BOOL(WINAPI WIREGUARD_SET_ADAPTER_STATE_FUNC)
(_In_ WIREGUARD_ADAPTER_HANDLE Adapter, _In_ WIREGUARD_ADAPTER_STATE State);

/**
 * Gets the adapter state of the WireGuard adapter.
 */
typedef _Must_inspect_result_
_Return_type_success_(return != FALSE)
BOOL(WINAPI WIREGUARD_GET_ADAPTER_STATE_FUNC)
(_In_ WIREGUARD_ADAPTER_HANDLE Adapter, _Out_ WIREGUARD_ADAPTER_STATE *State);

#define WIREGUARD_KEY_LENGTH 32

typedef enum
{
    WIREGUARD_ALLOWED_IP_REMOVE = 1 << 0 /**< Remove the specified allowed IP instead of adding it to the peer. */
} WIREGUARD_ALLOWED_IP_FLAG;
DEFINE_ENUM_FLAG_OPERATORS(WIREGUARD_ALLOWED_IP_FLAG)

typedef struct _WIREGUARD_ALLOWED_IP WIREGUARD_ALLOWED_IP;
struct ALIGNED(8) _WIREGUARD_ALLOWED_IP
{
    union
    {
        IN_ADDR V4;
        IN6_ADDR V6;
    } Address;                      /**< IP address */
    ADDRESS_FAMILY AddressFamily;   /**< Address family, either AF_INET or AF_INET6 */
    BYTE Cidr;                      /**< CIDR of allowed IPs */
    WIREGUARD_ALLOWED_IP_FLAG Flags; /**< Bitwise combination of flags */
};

typedef enum
{
    WIREGUARD_PEER_HAS_PUBLIC_KEY = 1 << 0,          /**< The PublicKey field is set */
    WIREGUARD_PEER_HAS_PRESHARED_KEY = 1 << 1,       /**< The PresharedKey field is set */
    WIREGUARD_PEER_HAS_PERSISTENT_KEEPALIVE = 1 << 2,/**< The PersistentKeepAlive field is set */
    WIREGUARD_PEER_HAS_ENDPOINT = 1 << 3,            /**< The Endpoint field is set */
    WIREGUARD_PEER_REPLACE_ALLOWED_IPS = 1 << 5,     /**< Remove all allowed IPs before adding new ones */
    WIREGUARD_PEER_REMOVE = 1 << 6,                  /**< Remove specified peer */
    WIREGUARD_PEER_UPDATE_ONLY = 1 << 7              /**< Do not add a new peer */
} WIREGUARD_PEER_FLAG;
DEFINE_ENUM_FLAG_OPERATORS(WIREGUARD_PEER_FLAG)

typedef struct _WIREGUARD_PEER WIREGUARD_PEER;
struct ALIGNED(8) _WIREGUARD_PEER
{
    WIREGUARD_PEER_FLAG Flags;                /**< Bitwise combination of flags */
    DWORD Reserved;                           /**< Reserved; must be zero */
    BYTE PublicKey[WIREGUARD_KEY_LENGTH];     /**< Public key, the peer's primary identifier */
    BYTE PresharedKey[WIREGUARD_KEY_LENGTH];  /**< Preshared key for additional layer of post-quantum resistance */
    WORD PersistentKeepalive;                 /**< Seconds interval, or 0 to disable */
    SOCKADDR_INET Endpoint;                   /**< Endpoint, with IP address and UDP port number */
    DWORD64 TxBytes;                          /**< Number of bytes transmitted */
    DWORD64 RxBytes;                          /**< Number of bytes received */
    DWORD64 LastHandshake;                    /**< Time of the last handshake, in 100ns intervals since 1601-01-01 UTC */
    DWORD AllowedIPsCount;                    /**< Number of allowed IP structs following this struct */
};

typedef enum
{
    WIREGUARD_INTERFACE_HAS_PUBLIC_KEY = 1 << 0,  /**< The PublicKey field is set */
    WIREGUARD_INTERFACE_HAS_PRIVATE_KEY = 1 << 1, /**< The PrivateKey field is set */
    WIREGUARD_INTERFACE_HAS_LISTEN_PORT = 1 << 2, /**< The ListenPort field is set */
    WIREGUARD_INTERFACE_REPLACE_PEERS = 1 << 3    /**< Remove all peers before adding new ones */
} WIREGUARD_INTERFACE_FLAG;
DEFINE_ENUM_FLAG_OPERATORS(WIREGUARD_INTERFACE_FLAG)

typedef struct _WIREGUARD_INTERFACE WIREGUARD_INTERFACE;
struct ALIGNED(8) _WIREGUARD_INTERFACE
{
    WIREGUARD_INTERFACE_FLAG Flags;        /**< Bitwise combination of flags */
    WORD ListenPort;                       /**< Port for UDP listen socket, or 0 to choose randomly */
    BYTE PrivateKey[WIREGUARD_KEY_LENGTH]; /**< Private key of interface */
    BYTE PublicKey[WIREGUARD_KEY_LENGTH];  /**< Corresponding public key of private key */
    DWORD PeersCount;                      /**< Number of peer structs following this struct */
};

/**
 * Sets the configuration of the WireGuard adapter.
 */
typedef _Return_type_success_(return != FALSE)
BOOL(WINAPI WIREGUARD_SET_CONFIGURATION_FUNC)
(_In_ WIREGUARD_ADAPTER_HANDLE Adapter, _In_reads_bytes_(Bytes) const WIREGUARD_INTERFACE *Config, _In_ DWORD Bytes);

/**
 * Gets the configuration of the WireGuard adapter.
 */
typedef _Must_inspect_result_
_Return_type_success_(return != FALSE)
BOOL(WINAPI WIREGUARD_GET_CONFIGURATION_FUNC)
(_In_ WIREGUARD_ADAPTER_HANDLE Adapter,
 _Out_writes_bytes_all_(*Bytes) WIREGUARD_INTERFACE *Config,
 _Inout_ DWORD *Bytes);

#pragma warning(pop)

#ifdef __cplusplus
}
#endif
