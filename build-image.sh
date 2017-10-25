#!/usr/bin/env bash

set -e

pushd ${BASH_SOURCE%/*}/

# TARANTOOL_VERSION: 1.6.9-11-gf4619d0, 1.7.5-0-g24b70de10, 1.8.1-415-ge3d2485c7
# TARANTOOL_VERSION=1.7.5-0-g24b70de10
REPOSITORY=progaudi/tarantool

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

for dir in $major* ;
do
    pushd $dir


    tail=${dir#$major}
    docker build -t $REPOSITORY:$major$tail -t $REPOSITORY:$minor$tail -t $REPOSITORY:$TARANTOOL_VERSION$tail --build-arg TARANTOOL_VERSION=$TARANTOOL_VERSION .
    docker push $REPOSITORY:$major$tail
    docker push $REPOSITORY:$minor$tail
    docker push $REPOSITORY:$TARANTOOL_VERSION$tail

    popd
done

popd
