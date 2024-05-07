#!/usr/bin/env bash

# Get some sleepy time c:
SLEEP_TIME=0
DOTNET_PATH=dotnet
while [[ $# -gt 0 ]]; do
  case $1 in
    -s|--sleep)
      SLEEP_TIME=$2
      shift
      shift
      ;;
	-dp|--dotnet-path)
      DOTNET_PATH=$2
      shift
      shift
      ;;
    -*|--*)
      echo "Unknown option $1"
      exit
      ;;
    *)
      shift
      ;;
  esac
done
sleep $SLEEP_TIME

# Wait for connection
while ! ping -c 4 google.com > /dev/null; do 
  echo "The network is not up yet"
  sleep 1
done

# Get to the right place
SCRIPT_DIR=$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )
cd $SCRIPT_DIR/MEE7-Discord-Bot

echo "Work until the end of time..."
while true; do
	echo "Pull me baby one more time"
	git pull
	echo "Restore me baby one more time"
	$DOTNET_PATH restore
	echo "Run me baby one more time"
	$DOTNET_PATH run -c Release
done