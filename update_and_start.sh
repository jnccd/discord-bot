#!/usr/bin/env bash

# Get some sleepy time c:
SLEEP_TIME=0
while [[ $# -gt 0 ]]; do
  case $1 in
    -s|--sleep)
      SLEEP_TIME=$2
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

# Get to the right place
SCRIPT_DIR=$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )
cd $SCRIPT_DIR/MEE7-Discord-Bot

# Work
git pull
dotnet restore
dotnet run -c Release