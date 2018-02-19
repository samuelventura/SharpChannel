rem netsh advfirewall firewall show rule name="ChannelManager.WebUI.Debug"
netsh advfirewall firewall delete rule name="ChannelManager.WebUI.Debug"
rem netsh advfirewall firewall show rule name="ChannelManager.Instance.Debug"
netsh advfirewall firewall delete rule name="ChannelManager.Instance.Debug"
rem netsh advfirewall firewall show rule name="ChannelManager.HTTP.Debug"
netsh advfirewall firewall delete rule name="ChannelManager.HTTP.Debug"
rem netsh http show urlacl
netsh http delete urlacl http://+:2018/
