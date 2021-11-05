#! /bin/bash
mkdir -p mystuff/dinglerebooted/publish
cd mystuff/dinglerebooted/publish
BOT_PROCESS=$(bash $HOME/get_process.sh [.]/DingleTheBotReboot)
kill -9 $BOT_PROCESS
printf "Killed bot with pId $BOT_PROCESS\nIf you do not want to use azure keyvault, press 1, else any key\n"
read KEY
if [ $KEY == "1" ]
then
	printf "Enter bot token\n"
	read BOT_TOKEN
	export BOT_TOKEN=$BOT_TOKEN
	printf "Enter Redis Connection String\n"
	read RCS
	export Redis=$RCS
	printf "Enter Postgres Connection String\n"
	read PSQL
	export Postgres=$PSQL
else
	printf "Enter AZURE_CLIENT_ID\n"
	read AZURE_CLIENT_ID
	export AZURE_CLIENT_ID=$AZURE_CLIENT_ID
	printf "Enter AZURE_CLIENT_SECRET\n"
	read AZURE_CLIENT_SECRET
	export AZURE_CLIENT_SECRET=$AZURE_CLIENT_SECRET
	printf "Enter AZURE_TENANT_ID\n"
	read AZURE_TENANT_ID
	export AZURE_TENANT_ID=$AZURE_TENANT_ID	
	printf "Enter Keyvault Name\n"
	read KVN
	export KeyVaultName=$KVN
fi
chmod +x DingleTheBotReboot
nohup ./DingleTheBotReboot >/dev/null 2>&1 &
ps aux | grep Dingle