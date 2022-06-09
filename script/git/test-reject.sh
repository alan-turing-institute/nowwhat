#!/usr/bin/env bash
# set -xe

saveifs=$IFS
IFS=$(echo -en "\n\b")

cd $(git rev-parse --show-toplevel)
files=$(git ls-files -o --exclude-standard)

# Specific to certain file extensions for now.
for f in $files
do
   if [[ $f == *.new.txt ]]
   then
      rm $f
   fi
done

IFS=$saveifs
