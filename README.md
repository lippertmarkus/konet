# `konet`: Easy .NET Containers

[![Nuget](https://img.shields.io/nuget/v/konet)](https://www.nuget.org/packages/konet)
[![Build](https://github.com/lippertmarkus/konet/actions/workflows/ci.yml/badge.svg)](https://github.com/lippertmarkus/konet/actions/workflows/ci.yml)

![Alt text](https://raw.githubusercontent.com/lippertmarkus/konet/main/images/logo.svg)

`konet` is an easy to use and fast container image builder for .NET applications.

It creates binaries for different architectures by running `dotnet build` and pushes only those binaries as new layers to a container image registry.

`konet` is fast as it builds binaries locally and doesn't require `docker` to be installed or base images to be pulled. It's easy to use and doesn't require writing a `Dockerfile`.

`konet` is heavily inspired by [`ko`](https://github.com/google/ko).

## Setup

### Installation

`konet` is distributed as a [.NET tool](https://aka.ms/global-tools). With .NET [set up](https://dotnet.microsoft.com/en-us/download) you can easily install and update it: 

```
dotnet tool install --global konet
```

### Update

```
dotnet tool update --global konet
```

### Authenticate

`konet` can use existing authentication data, e.g. from `~/.docker/config.json`. As `konet` doesn't require `docker`, you can also login to a container image registry via `konet login`.


## Build an Image

`konet build` automatically compiles the current .NET project for all available architectures and pushes images including the binaries - all without downloading any base image and without a container runtime.

Example:

```bash
dotnet new webapi -n mywebapi
cd mywebapi/
konet build -t lippertmarkus/test-webapi:1.0
# ...
# Successfully pushed to lippertmarkus/test-webapi:1.0
```

The result is a manifest list at the tag specified in `-t`, referencing images for different architectures with the compiled binary as the entrypoint.

### Configuration

#### Target Platforms

Per default `konet` creates images for all platforms .NET supports and for which there is an official base image available. Those include `windows/amd64:1809, windows/amd64:1903, windows/amd64:1909, windows/amd64:2004, windows/amd64:20H2, windows/amd64:ltsc2022, linux/amd64, linux/arm/v7, linux/arm64/v8`.

You can limit the platforms by adding `-p windows/amd64:ltsc2022,linux/amd64` to `konet build`.

## Acknowledgements

This work is heavily inspired by [`ko`](https://github.com/google/ko) and uses [`crane`](https://github.com/google/go-containerregistry/tree/main/cmd/crane) under the hood.
