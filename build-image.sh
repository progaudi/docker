#!/usr/bin/env bash

REPOSITORY=progaudi/tarantool

set -e

pushd ${BASH_SOURCE%/*}/

if [ ! -d .tarantool/.git ]
then
    rm -rf .tarantool
    git clone git://github.com/tarantool/tarantool.git .tarantool
fi

pushd .tarantool

git checkout $TARANTOOL_BRANCH
git pull
if [ -z "$TARANTOOL_TAG_PREFIX" ]; then
    git tag | grep -v $TARANTOOL_BRANCH | xargs -I {} git tag -d {}
else
    git tag | grep -v $TARANTOOL_BRANCH | grep -v $TARANTOOL_TAG_PREFIX | xargs -I {} git tag -d {}
fi
TARANTOOL_VERSION=$(git describe --long)

popd

if docker pull $REPOSITORY:$TARANTOOL_VERSION
then
    echo "$TARANTOOL_VERSION is already built. Rebuild it manually."
    exit 0
fi

echo "Will build $TARANTOOL_VERSION from branch: $TARANTOOL_BRANCH"

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

docker login -u="$DOCKER_USERNAME" -p="$DOCKER_PASSWORD";

set -x

for dir in $major* ;
do
    pushd $dir

    tail=${dir#$major}
    docker build -t $REPOSITORY:$major$tail -t $REPOSITORY:$minor$tail -t $REPOSITORY:$TARANTOOL_VERSION$tail --build-arg TARANTOOL_VERSION=$TARANTOOL_VERSION --build-arg TARANTOOL_BRANCH=$TARANTOOL_BRANCH .
    docker push $REPOSITORY:$major$tail
    docker push $REPOSITORY:$minor$tail
    docker push $REPOSITORY:$TARANTOOL_VERSION$tail

    popd
done

popd
