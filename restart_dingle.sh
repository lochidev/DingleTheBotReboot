#! /bin/bash
mkdir -p mystuff/dinglerebooted/publish
cd mystuff/dinglerebooted/publish
BOT_PROCESS=$(bash $HOME/get_process.sh [.]/DingleTheBotReboot)
kill -9 $BOT_PROCESS
printf "Killed bot with pId $BOT_PROCESS\nEnter bot token\n"
read BOT_TOKEN
export BOT_TOKEN=$BOT_TOKEN
printf "Enter Cosmos_AccountEndpoint\n"
read CAE
export Cosmos_AccountEndpoint=$CAE
printf "Enter Cosmos_AccountKey\n"
read CAK
export Cosmos_AccountKey=$CAK
printf "Enter Cosmos_Database\n"
read CDB
export Cosmos_Database=$CDB
chmod +x DingleTheBotReboot
nohup ./DingleTheBotReboot >/dev/null 2>&1 &
ps aux | grep Dingle