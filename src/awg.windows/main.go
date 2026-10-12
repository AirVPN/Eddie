package main

import (
	"bufio"
	"encoding/base64"
	"fmt"
	"net"
	"os"
	"strconv"
	"strings"
	"time"

	"github.com/amnezia-vpn/amneziawg-go/conn"
	"github.com/amnezia-vpn/amneziawg-go/device"
	"github.com/amnezia-vpn/amneziawg-go/tun"
	"github.com/amnezia-vpn/amneziawg-windows/conf"
	"github.com/amnezia-vpn/amneziawg-windows/tunnel/winipcfg"
	"golang.org/x/sys/windows"
)

func bindroute(family winipcfg.AddressFamily, binder conn.BindSocketToInterface, own winipcfg.LUID) error {
	routes, err := winipcfg.GetIPForwardTable2(family)
	if err != nil {
		return err
	}
	metric, index := ^uint32(0), uint32(0)
	for _, route := range routes {
		if route.DestinationPrefix.PrefixLength != 0 || route.InterfaceLUID == own {
			continue
		}
		row, err := route.InterfaceLUID.Interface()
		if err != nil || row.OperStatus != winipcfg.IfOperStatusUp {
			continue
		}
		iface, err := route.InterfaceLUID.IPInterface(family)
		if err == nil && route.Metric+iface.Metric < metric {
			metric = route.Metric + iface.Metric
			index = route.InterfaceIndex
		}
	}
	if family == windows.AF_INET {
		return binder.BindSocketToInterface4(index, index == 0)
	}
	return binder.BindSocketToInterface6(index, index == 0)
}

func parseprofile(text, name string) (*conf.Config, error) {
	profile, err := conf.FromWgQuick(text, name)
	if err != nil {
		return nil, fmt.Errorf("invalid awg configuration: %w", err)
	}
	if profile.Interface.PreUp != "" || profile.Interface.PostUp != "" || profile.Interface.PreDown != "" || profile.Interface.PostDown != "" {
		return nil, fmt.Errorf("script directives are not supported")
	}
	if !profile.Interface.TableOff {
		return nil, fmt.Errorf("table must be off: eddie manages routing")
	}
	if len(profile.Peers) == 0 || len(profile.Interface.Addresses) == 0 {
		return nil, fmt.Errorf("missing peer or address")
	}
	return profile, nil
}

func run() error {
	if len(os.Args) != 4 {
		return fmt.Errorf("expected interface and handshake timeouts")
	}
	first, err := strconv.Atoi(os.Args[2])
	if err != nil || first < 1 {
		return fmt.Errorf("invalid first handshake timeout")
	}
	connected, err := strconv.Atoi(os.Args[3])
	if err != nil || connected < 1 {
		return fmt.Errorf("invalid connected handshake timeout")
	}
	input := bufio.NewScanner(os.Stdin)
	input.Buffer(make([]byte, 4096), 1048576)
	if !input.Scan() {
		return fmt.Errorf("missing configuration on stdin")
	}
	decoded, err := base64.StdEncoding.DecodeString(input.Text())
	if err != nil {
		return fmt.Errorf("invalid configuration encoding")
	}
	profile, err := parseprofile(string(decoded), os.Args[1])
	if err != nil {
		return err
	}
	uapi, err := profile.ToUAPI()
	if err != nil {
		return fmt.Errorf("endpoint resolution: %w", err)
	}
	mtu := int(profile.Interface.MTU)
	if mtu == 0 {
		mtu = 1320
	}
	adapter, err := tun.CreateTUN(profile.Name, mtu)
	if err != nil {
		return fmt.Errorf("create wintun: %w", err)
	}
	binding := conn.NewDefaultBind()
	engine := device.NewDevice(adapter, binding, &device.Logger{
		Verbosef: func(format string, args ...any) {},
		Errorf: func(format string, args ...any) {
			fmt.Printf("error:engine: %s\n", strings.ToLower(fmt.Sprintf(format, args...)))
		},
	})
	luid := winipcfg.LUID(adapter.(*tun.NativeTun).LUID())
	defer func() {
		for _, family := range []winipcfg.AddressFamily{windows.AF_INET, windows.AF_INET6} {
			luid.FlushRoutes(family)
			luid.FlushIPAddresses(family)
			luid.FlushDNS(family)
		}
		engine.Close()
		fmt.Println("stop-interface")
	}()
	if err = engine.IpcSet(uapi); err != nil {
		return fmt.Errorf("set awg parameters: %w", err)
	}
	addresses := make([]net.IPNet, len(profile.Interface.Addresses))
	for i, address := range profile.Interface.Addresses {
		addresses[i] = address.IPNet()
	}
	if err = luid.SetIPAddresses(addresses); err != nil {
		return fmt.Errorf("set interface addresses: %w", err)
	}
	for _, family := range []winipcfg.AddressFamily{windows.AF_INET, windows.AF_INET6} {
		row, err := luid.IPInterface(family)
		if err != nil {
			return fmt.Errorf("get interface: %w", err)
		}
		row.NLMTU = uint32(mtu)
		row.SitePrefixLength = 0
		if err = row.Set(); err != nil {
			return fmt.Errorf("set mtu: %w", err)
		}
		if len(profile.Interface.DNS) > 0 || len(profile.Interface.DNSSearch) > 0 {
			if err = luid.SetDNS(family, profile.Interface.DNS, profile.Interface.DNSSearch); err != nil {
				return fmt.Errorf("set dns: %w", err)
			}
		}
	}
	if err = engine.Up(); err != nil {
		return fmt.Errorf("bring awg up: %w", err)
	}
	binder := binding.(conn.BindSocketToInterface)
	for _, family := range []winipcfg.AddressFamily{windows.AF_INET, windows.AF_INET6} {
		if err = bindroute(family, binder, luid); err != nil {
			return fmt.Errorf("bind transport: %w", err)
		}
	}
	fmt.Println("setup-complete")
	fmt.Println("setup-interface")
	stop := make(chan struct{})
	go func() { input.Scan(); close(stop) }()
	ticker := time.NewTicker(time.Second)
	defer ticker.Stop()
	started := time.Now()
	var last int64
	for {
		select {
		case <-stop:
			fmt.Println("stop-requested")
			return nil
		case <-engine.Wait():
			return fmt.Errorf("awg engine stopped unexpectedly")
		case <-ticker.C:
			live, err := engine.IpcGet()
			if err != nil {
				return fmt.Errorf("poll handshake: %w", err)
			}
			latest := int64(0)
			for _, line := range strings.Split(live, "\n") {
				if strings.HasPrefix(line, "last_handshake_time_sec=") {
					stamp, err := strconv.ParseInt(strings.TrimPrefix(line, "last_handshake_time_sec="), 10, 64)
					if err == nil && stamp > latest {
						latest = stamp
					}
				}
			}
			if latest > last {
				if last == 0 {
					fmt.Println("handshake-first")
				}
				last = latest
			}
			if (last == 0 && time.Since(started) > time.Duration(first)*time.Second) || (last != 0 && time.Now().Unix()-last > int64(connected)) {
				fmt.Println("handshake-out")
				return fmt.Errorf("awg handshake timeout")
			}
			for _, family := range []winipcfg.AddressFamily{windows.AF_INET, windows.AF_INET6} {
				if err = bindroute(family, binder, luid); err != nil {
					return fmt.Errorf("update transport binding: %w", err)
				}
			}
		}
	}
}

func main() {
	if len(os.Args) == 2 && os.Args[1] == "--public-key" {
		input := bufio.NewScanner(os.Stdin)
		if !input.Scan() {
			fmt.Fprintln(os.Stderr, "missing private key")
			os.Exit(1)
		}
		key, err := conf.NewPrivateKeyFromString(strings.TrimSpace(input.Text()))
		if err != nil {
			fmt.Fprintln(os.Stderr, "invalid private key")
			os.Exit(1)
		}
		fmt.Println(key.Public().String())
		return
	}
	if err := run(); err != nil {
		fmt.Println("error:" + strings.ToLower(err.Error()))
		os.Exit(1)
	}
}
