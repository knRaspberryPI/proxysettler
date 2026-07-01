# Proxy Settler

A small Windows desktop app to turn your system proxy on/off and check your public IP.

## What it does

- **Proxy Configuration**: enter a proxy address and port, then hit the single
  **Connect / Disconnect** toggle to apply or remove it as your Windows system proxy
  (per-user, no admin rights needed). A status line shows what's currently active.
  The address and port you last used are remembered and pre-filled the next time you
  open the app.
- **IP Check**: click **Check My IP** to query `https://api.myip.com` and show your
  current public IP (bold) and country on one line, below a divider. Since it uses the
  system proxy, this doubles as a way to confirm the proxy is actually being used.

## UI

White background, `#0E0E10` Segoe UI text, flat 16px-rounded buttons.

## Getting the .exe

Every push to this repo builds a self-contained Windows executable via GitHub Actions
and publishes it to the **[latest-build release](../../releases/tag/latest-build)** —
download `ProxySettler.exe` from there. No .NET install required on the target machine;
it's a single self-contained file.

You can also grab it from the **Actions** tab: open the latest successful
"Build Windows EXE" run and download the `ProxySettler-win-x64` artifact.

## Building it yourself

Requires the [.NET 8 SDK](https://dotnet.microsoft.com/download) on Windows:

```
dotnet publish ProxySettler/ProxySettler.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish
```

The resulting `publish/ProxySettler.exe` is standalone.

## Notes

- The proxy is written to `HKEY_CURRENT_USER\...\Internet Settings`, the same setting
  shown in Windows Settings > Network > Proxy. It affects Edge and most apps that use
  the system/WinINet proxy. Apps with their own independent proxy settings (e.g.
  Firefox) are not affected.
- No credentials/auth or bypass list support — this targets a plain `host:port` HTTP
  proxy, kept intentionally simple.

## License

MIT — see [LICENSE](LICENSE).
