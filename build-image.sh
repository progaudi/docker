#!/usr/bin/env bash

set -e

pushd ${BASH_SOURCE%/*}/

REPOSITORY=progaudi/tarantool

# $TARANTOOL_VERSION: 1.6.9-11-gf4619d0, 1.7.5-0-g24b70de10, 1.8.1-415-ge3d2485c7

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

for dir in $major*/ ;
do
    pushd $dir

    docker build -t $REPOSITORY:$major -t $REPOSITORY:$minor -t $REPOSITORY:$TARANTOOL_VERSION --build-arg TARANTOOL_VERSION=$TARANTOOL_VERSION .
    docker push $REPOSITORY:$major
    docker push $REPOSITORY:$minor
    docker push $REPOSITORY:$TARANTOOL_VERSION

    popd
done

popd
