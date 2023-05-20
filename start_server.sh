cd BetterServer
dotnet build
cd bin/Debug/net7.0/
nohup ./BetterServer /root/Config.json > /root/server.log 2>&1 &