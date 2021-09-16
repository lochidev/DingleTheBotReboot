#! /bin/bash

BOT_PROCESS=$(bash getbotprocess.sh DingleTheBotReboot)
kill -9 $BOT_PROCESS
printf "Killed bot with pId $BOT_PROCESS\nEnter bot token\n"
read BOT_TOKEN
export BOT_TOKEN=$BOT_TOKEN
printf "Enter Cosmos:AccountEndpoint\n"
read CAE
export Cosmos:AccountEndpoint=$CAE
printf "Enter Cosmos:AccountKey\n"
read CAK
export Cosmos:AccountKey=$CAK
printf "Enter Cosmos:Database\n"
read CDB
export Cosmos:AccountEndpoint=$CDB
cd mystuff/dinglerebooted/publish
chmod +x DingleTheBotReboot
nohup ./DingleTheBotReboot >/dev/null 2>&1 &
ps aux | grep Dingle