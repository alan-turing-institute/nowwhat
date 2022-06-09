#!/usr/bin/env bash
# set -xe

saveifs=$IFS
IFS=$(echo -en "\n\b")

pushd $(git rev-parse --show-toplevel) > /dev/null
files=$(git ls-files -o --exclude-standard)

# Specific to certain file extensions for now.
for f in $files
do
   if [[ $f == *.new.txt ]]
   then
      file=${f%%.*}
      ext=${f##*.}

      if [[ -f "$file.$ext" ]]; then
         git diff --no-index $file.$ext $f
      else
         echo "$file.$ext is new."
      fi
   fi
done

popd > /dev/null

IFS=$saveifs
