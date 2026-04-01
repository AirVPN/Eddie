#!/bin/bash
# Detached OpenPGP signature for release artifacts. Creates ${path}.asc.
# No-op if EDDIESIGNINGDIR/eddie_gpg_2026.passphrase is not set (e.g. public CI).

set -euo pipefail

if [ "${1-}" = "" ] || [ ! -f "${1}" ]; then
	echo "sign-openpgp.sh: missing or non-existent file" >&2
	exit 1
fi

if ! test -f "${EDDIESIGNINGDIR:-/none}/eddie_gpg_2026.passphrase"; then
	exit 0
fi

PASSPHRASE=$(cat "${EDDIESIGNINGDIR}/eddie_gpg_2026.passphrase")
export GPG_TTY=$(tty 2>/dev/null || true)
gpg --batch --yes --pinentry-mode loopback --passphrase "${PASSPHRASE}" \
	--detach-sign --armor -u "apt@eddie.website" -o "${1}.asc" "${1}"
gpg --verify "${1}.asc" "${1}"
