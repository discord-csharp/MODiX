#!/bin/sh
if [ ! -f $1 ]; then
  echo "$1 doesn't exist"
  exit 1
fi

lastUpdate=$(date -r $1 +%s)
now=$(date +%s)
file_age=$(($now - $lastUpdate))
if [ $file_age -gt $2 ]; then 
  echo "$1 is older than $2 seconds"
  exit 1
else
  echo "$1 is current"
  exit 0
fi