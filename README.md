# Why another tarantool docker image?

This image is built on every commit to major branches of [Tarantool](https://github.com/tarantool/tarantool/). Sometimes we're updating [packages on board](https://github.com/tarantool/docker/#whats-on-board) to latest versions of them. Goal is simple: we want to provide docker image for every build tarantool package version with latest packages.

# Version strategy

Every build of Tarantool from stable branches is tagged (e.g. branch is 1.7): 1.7.5-123-g234dsa, which can be break into parts:
- 1.7 is major version
- 5 is minor version
- 123-g234dsa is build version

When we build such an image, we'll push tags:
- major version (1.7)
- minor version (1.7.5)
- package version (1.7.5-123-g234dsa)

# Image information

Detailed information about image is available in [official repo](https://github.com/tarantool/docker/)
