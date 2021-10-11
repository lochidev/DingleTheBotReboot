#! /bin/bash
mkdir -p mystuff/dinglerebooted/publish
cd mystuff/dinglerebooted/publish
BOT_PROCESS=$(bash $HOME/get_process.sh [.]/DingleTheBotReboot)
kill -9 $BOT_PROCESS
printf "Killed bot with pId $BOT_PROCESS\nEnter bot token\n"
read BOT_TOKEN
export BOT_TOKEN=$BOT_TOKEN
printf "Enter Redis Connection String\n"
read RCS
export Redis=$RCS
printf "Enter Postgres Connection String\n"
read PSQL
export Postgres=$PSQL
chmod +x DingleTheBotReboot
nohup ./DingleTheBotReboot >/dev/null 2>&1 &
ps aux | grep Dingle