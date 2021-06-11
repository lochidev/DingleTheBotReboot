#! /bin/bash

BOT_PROCESS=$(getbotprocessfn)
kill -9 $BOT_PROCESS
printf "Killed bot with pId $BOT_PROCESS\nEnter bot token\n"
read BOT_TOKEN
export BOT_TOKEN=$BOT_TOKEN
cd mystuff/dinglerebooted/publish
echo $(nohup dotnet DingleTheBotReboot.dll >/dev/null 2>&1 &)