cd BetterServer
dotnet build --configuration Release
cd bin/Release/net7.0/
nohup ./BetterServer /root/Config.json > /root/server.log 2>&1 &