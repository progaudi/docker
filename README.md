# Why another tarantool docker image?

This image is built on every commit to major branches of [Tarantool](https://github.com/tarantool/tarantool/). Sometimes we're updating [packages on board](https://github.com/tarantool/docker/#whats-on-board) to latest versions of them. Goal is simple: we want to provide docker image for every build tarantool package version with latest packages.

# Version strategy

Every build of Tarantool from stable branches is tagged (e.g. branch is 1.7): 1.7.5-123-g234dsa, which can be broken into parts:
- 1.7 is major version
- 5 is minor version
- 123-g234dsa is build version

If we would build an image from that commit, we'll push tags:
- major version (1.7)
- minor version (1.7.5)
- package version (1.7.5-123-g234dsa)

In this way we achieve a couple of interesting things. 1.7 is the latest 1.7 build, so if you care only about major version, use this tag. If you care about specific minor version, then stick to minor version tag (1.7.5). But my recommendation is to stick to most detailed tag (package version).

# Image information

Source is available at [our github repo](https://github.com/progaudi/tarantool-docker)
Detailed information about image is available in [official repo](https://github.com/tarantool/docker/).
