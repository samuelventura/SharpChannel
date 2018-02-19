rem netsh advfirewall firewall show rule name="ChannelManager.Service"
netsh advfirewall firewall add rule name="ChannelManager.Service" dir=in action=allow program="%~dp0SharpChannel.Manager.Service.exe" enable=yes
rem netsh advfirewall firewall show rule name="ChannelManager.Instance"
netsh advfirewall firewall add rule name="ChannelManager.Instance" dir=in action=allow program="%~dp0SharpChannel.Manager.Instance.exe" enable=yes
rem netsh advfirewall firewall show rule name="ChannelManager.HTTP"
netsh advfirewall firewall add rule name="ChannelManager.HTTP" dir=in action=allow protocol=TCP localport=2017
SharpChannel.Manager.Service --install
net start ChannelManager
