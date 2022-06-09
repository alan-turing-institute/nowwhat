#!/usr/bin/env bash
# set -x
set -e

saveifs=$IFS
IFS=$(echo -en "\n\b")

cd $(git rev-parse --show-toplevel)
files=$(git ls-files -o --exclude-standard)

# Specific to certain file extensions for now.
# No git add here (handled by the git aliases).
for f in $files
do
   if [[ $f == *.new.txt ]]
   then
      file=${f%%.*}
      ext=${f##*.}
      mv $f $file.$ext
   fi
done

IFS=$saveifs
