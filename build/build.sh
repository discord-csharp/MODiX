#!/bin/sh
apt-get update && apt install curl -y
curl -sL https://deb.nodesource.com/setup_10.x | bash -
apt-get install nodejs -y

dotnet publish Modix.sln -c Release -r linux-x64 --self-contained -o /app