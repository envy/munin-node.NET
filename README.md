munin-node.NET
==============

Port of munin-node to .NET for use on Windows

Troubleshooting
===============

If you get an error like `Cannot load Counter Name data because an invalid index 'X' was read from the registry` then your performance counte rregistry is corrupt.
You can fix this by running `lodctr.exe /r` from an elevated command prompt.