netsh advfirewall firewall delete rule name="ChannelManager.Service"
netsh advfirewall firewall delete rule name="ChannelManager.Instance"
netsh advfirewall firewall delete rule name="ChannelManager.HTTP"
net stop ChannelManager
SharpChannel.Manager.Service --uninstall

