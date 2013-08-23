munin-node.NET
==============

munin-node.NET is a port of [munin-node](https://github.com/munin-monitoring/munin) for use on Windows. It's written in C#. It can be installed as a Windows Service.

Install
=======
Binary is located in build-rel, either install munin-node Service.exe as a service or start munin-node Standalone.exe.

Troubleshooting
===============

If you get an error like `Cannot load Counter Name data because an invalid index 'X' was read from the registry` then your performance counter registry is corrupt.
You can fix this by running `lodctr.exe /r` from an elevated command prompt.