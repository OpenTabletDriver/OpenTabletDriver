#!/usr/bin/env bash

# Packs or removes a local package from the local nuget feed.
# This allows one to test a project as a nuget package without publishing it.

# Usage: pack_local.sh <options> [pack|remove] [project path/csproj file]
# Options: -h|--help
#          -f|--feed <feed path> (default: ./.local_feed)

PROJECT_DIR=""
DEFAULT_FEED="./.local_feed"

feed="$DEFAULT_FEED"

get_csproj() {
    local path="${PROJECT_DIR}$1"

    if [ -d "$path" ]; then
        local csproj=$(find "$path" -name "*.csproj" | head -n 1)
        if [ -z "$csproj" ]; then
            echo "No csproj file found in $path" >&2
            exit 1
        fi
        echo "$csproj"
    elif [ -f "$path" ]; then
        echo "$path"
    else
        echo "Invalid path: $path" >&2
        exit 1
    fi
}

main() {
    while [ $# -gt 0 ]; do
        local key="$1"
        case $key in
            -h|--help)
                echo "Usage: pack_local.sh <options> [pack|remove] [project path/csproj file]"
                echo "Options: -h|--help"
                echo "         -f|--feed <feed path> (default: $DEFAULT_FEED)"
                exit 0
                ;;
            -f|--feed)
                feed="$2"
                shift
                ;;
            *)
                break
                ;;
        esac
        shift
    done

    if [ $# -lt 2 ]; then
        echo "Usage: pack_local.sh <options> [pack|remove] [project path/csproj file]"
        exit 1
    fi

    if [ ! -d "$feed" ]; then
        mkdir "$feed"
    fi

    local command="$1"
    shift

    if [ "$command" == "pack" ]; then
        while [ $# -gt 0 ]; do
            local project_name="$1"
            remove "${project_name}"
            pack "${project_name}"
            shift
        done
    elif [ "$command" == "remove" ]; then
        while [ $# -gt 0 ]; do
            remove "$1"
            shift
        done
    else
        echo "Invalid command: $command" >&2
        exit 1
    fi
}

pack() {
    local csproj=$(get_csproj "$1")
    local package=$(dotnet pack "$csproj" -o "$feed" | grep -oP "(?<=Successfully created package ').*?(?=')")

    if [ -z "$package" ]; then
        echo "Failed to pack $csproj" >&2
        exit 1
    fi

    echo "Packed: $package"
}

remove() {
    local project_name="$1"
    local package=$(readlink -f "$(find "$feed" -name "$project_name.*.nupkg" | head -n 1)")

    if [ -f "$package" ]; then
        rm "$package"
        echo "Removed: $package"
    fi
}

main "$@"
