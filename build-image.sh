#!/usr/bin/env bash

function build_and_push()
{
    local repo=$1
    local branch=$2
    local version=$3
    local major=$4
    local minor=$5
    local tail=$6
    echo "Will build $version$tail from branch: $branch"

    docker build \
        -t "$repo":"$major""$tail" \
        -t "$repo":"$minor""$tail" \
        -t "$repo":"$version""$tail" \
        --build-arg TARANTOOL_VERSION="$version" \
        --build-arg TARANTOOL_BRANCH="$branch" \
        .

    # docker run --rm --entrypoint=/usr/local/bin/tarantool $repo:$version$tail \
    #     | grep "Tarantool $version"

    docker push "$repo":"$major""$tail"
    docker push "$repo":"$minor""$tail"
    docker push "$repo":"$version""$tail"
}

REPOSITORY=progaudi/tarantool

set -e

pushd "${BASH_SOURCE%/*}/"

if [ ! -d .tarantool/.git ]
then
    rm -rf .tarantool
    git clone git://github.com/tarantool/tarantool.git .tarantool
fi

pushd .tarantool

git checkout "$TARANTOOL_BRANCH"
git pull
git tag | grep -v "$TARANTOOL_BRANCH" | xargs -I {} git tag -d {}
TARANTOOL_VERSION=$(git describe --long || echo "no version")

popd

if [ "$TARANTOOL_VERSION" == "no version" ]
then
    echo "No version to build, possible a feature branch. Skipping it."
    exit 0
fi

# TARANTOOL_VERSION: 1.6.9-11-gf4619d0, 1.7.5-0-g24b70de10, 1.8.1-415-ge3d2485c7
# TARANTOOL_VERSION=1.7.5-0-g24b70de10

# split by .
IFS=. read -ra version_slugs <<< "$TARANTOOL_VERSION"
if [ ${#version_slugs[@]} != 3 ]; then
    echo "Can't parse tarantool version. Set TARANTOOL_VERSION environment variable "
    exit 1
fi

major=${version_slugs[0]}.${version_slugs[1]}

# split by -
IFS=- read -ra minor_slugs <<< "${version_slugs[2]}"
if [ ${#minor_slugs[@]} != 3 ]; then
    echo "Can't parse tarantool version. Set TARANTOOL_VERSION environment variable "
    exit 2
fi

minor=$major.${minor_slugs[0]}

echo "$DOCKER_PASSWORD" | docker login -u "$DOCKER_USERNAME" --password-stdin

set -x

for dir in "$TARANTOOL_DIRECTORY"* ;
do
    pushd "$dir"
    tail=${dir#$TARANTOOL_DIRECTORY}

    if [ ! -z "$FORCE_BUILD" ]
    then
        build_and_push $REPOSITORY "$TARANTOOL_BRANCH" "$TARANTOOL_VERSION" "$major" "$minor" "$tail"
    elif docker pull $REPOSITORY:"$TARANTOOL_VERSION""$tail"
    then
        echo "$TARANTOOL_VERSION$tail is already built. Rebuild it manually."
    else
        build_and_push $REPOSITORY "$TARANTOOL_BRANCH" "$TARANTOOL_VERSION" "$major" "$minor" "$tail"
    fi

    popd
done

popd
