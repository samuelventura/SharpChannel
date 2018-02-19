rem netsh advfirewall firewall show rule name="ChannelManager.WebUI.Debug"
rem netsh advfirewall firewall delete rule name="ChannelManager.WebUI.Debug"
netsh advfirewall firewall add rule name="ChannelManager.WebUI.Debug" dir=in action=allow program="%~dp0SharpChannel.Manager.WebUI.exe" enable=yes
rem netsh advfirewall firewall show rule name="ChannelManager.Instance.Debug"
rem netsh advfirewall firewall delete rule name="ChannelManager.Instance.Debug"
netsh advfirewall firewall add rule name="ChannelManager.Instance.Debug" dir=in action=allow program="%~dp0SharpChannel.Manager.Instance.exe" enable=yes
rem netsh advfirewall firewall show rule name="ChannelManager.HTTP.Debug"
rem netsh advfirewall firewall delete rule name="ChannelManager.HTTP.Debug"
netsh advfirewall firewall add rule name="ChannelManager.HTTP.Debug" dir=in action=allow protocol=TCP localport=2018
rem Only need for userspace apps
rem Services do not need this
rem netsh http show urlacl
rem netsh http delete urlacl http://+:2018/
rem required to avoid AutomaticUrlReservationCreationFailureException
netsh http add urlacl url=http://+:2018/ user="Everyone"
rem netsh http show iplisten
rem netsh http delete iplisten ipaddress=0.0.0.0:2017
rem netsh http add iplisten ipaddress=0.0.0.0:2017
