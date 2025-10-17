#!/usr/bin/env bash

# finds associated commits for tablet name
# does NOT (yet) find parser changes

set -e

### CONFIG

FTS_GIT_IGNORE_REVS_FILE="${FTS_GIT_IGNORE_REVS:-".git-blame-ignore-revs_tablet-configurations"}"

### hardcoded env

if [ "$#" -eq 0 ]; then
  echo "$0 needs as least 1 argument: a tablet name"
  exit 2
fi

input_string="$@"

root_dir="$(dirname "${BASH_SOURCE[0]}")/.."

configs_path="OpenTabletDriver.Configurations/Configurations/"

### funcs

git_prettyprint_ref() {
  local text="$(git log --format="%h {{DESCRIBEHERE}} - %aN - %as: %s" -1 "$1")"
  local describe="$(git describe --contains "${1}")"
  sed "s/{{DESCRIBEHERE}}/${describe}/" <<< $text
}

get_excluded_commits() {
  while IFS='' read -r line; do
    [[ "${line}" == \#* ]] && continue # skip comment lines
    printf "%s\n" "$line"
  done < "${root_dir}/${FTS_GIT_IGNORE_REVS_FILE}"
}

# removes refs that are known minor
# $1: tablet config path
# $2: (optional) minimum commit
get_modified_refs(){
  local tablet_config="$1"
  if [ -n "$2" ]; then
    local commit_range="${2}..HEAD"
  fi
  mapfile -t modified_refs < <(git log --follow --pretty=format:%H --diff-filter=M ${gitlog_settings} ${commit_range} -- "${tablet_config}")
  mapfile -t skippable_refs < <(get_excluded_commits)

  # this can probably be done smarter, but naive lookup:
  for modified_ref in "${modified_refs[@]}"; do
    local found_ref="n"
    for skippable_ref in "${skippable_refs[@]}"; do
      [ "${modified_ref}" == "${skippable_ref}" ] && found_ref="y"
    done
    if [ "$found_ref" == "n" ]; then
      printf "%s\n" "$modified_ref"
    fi
  done
}

### program start

cd "${root_dir}"/"${configs_path}"
mapfile -t file_matches < <(grep --include=\*.json -rw \
  -Ei -e"\"Name\":.*${input_string}" \
  | awk -F: '{ print $1 }')

if [ "${#file_matches[@]}" -gt 1 ]; then
  iterator=1

  echo "More than 1 file matched. Please select the appropriate file:"
  for file_match in "${file_matches[@]}"; do
    local_tablet_name="$(jq -r .Name "${file_match}")"
    printf '%d) %s (%s)\n' "$iterator" "$file_match" "$local_tablet_name"
    ((iterator++))
  done

  # not using bash's select since we want to control listing format
  input_solved="n"
  while [ "$input_solved" != "y" ] ; do
    read -p "Enter index: " file_match_index
    # validate input
    ! [[ $file_match_index =~ ^[0-9]+$ ]] && continue

    let file_match_index-- || true
    if [ "$file_match_index" -ge 0 ] && [ "$file_match_index" -lt "${#file_matches[@]}" ]; then
      input_solved="y"
      tablet_config="${file_matches[$file_match_index]}"
    else
      echo "Invalid index"
    fi
  done
elif [ "${#file_matches[@]}" -eq 1 ]; then
  tablet_config="${file_matches[0]}"
else
  echo "No tablets found matching name '${input_string}'"
  exit 1
fi

cd "$OLDPWD" 2>/dev/null

if [ -z "${tablet_config}" ]; then
  echo "Tablet config missing, wrong selection?"
  exit 2
fi

echo "Tablet config: ${tablet_config}"

tablet_config="${root_dir}"/"${configs_path}"/"${tablet_config}"

commit_added="$(git log --follow --pretty=format:%H --diff-filter=AC -1 -- "${tablet_config}")"

echo -e "\nAdded in:"
git_prettyprint_ref "${commit_added}"

mapfile -t modified_commits < <(get_modified_refs "${tablet_config}" "${commit_added}")

if [ "${#modified_commits[@]}" -gt 0 ]; then
  echo -e "\nModified in:"
  for modified_commit in "${modified_commits[@]}"; do
    git_prettyprint_ref "${modified_commit}"
  done
else
  echo -e "\nNo changes found"
fi
