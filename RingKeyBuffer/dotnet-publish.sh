#!/bin/bash

parse_version_csproj() {
    CSPROJ=${1}
	VERSION=$(grep VersionPrefix "${CSPROJ}" | grep -oP '([0-9]\.?)+')
	# VERSION_SUFFIX=$(grep VersionSuffix "${CSPROJ}" | grep -oP '(?<=>).+(?=<)') || true
	# echo "${VERSION}-${VERSION_SUFFIX}"
    echo "${VERSION}"
}

PACKAGE_VERSION=$(parse_version_csproj RingKeyBuffer.csproj)

APIKEY=$(cat ../nuget-apikey.txt)

FRAMEWORK=net7.0

dotnet publish -c Release -f "${FRAMEWORK}"

dotnet pack -c Release &&
  dotnet nuget push -s 'https://api.nuget.org/v3/index.json' -k "${APIKEY}" "bin/Release/RingKeyBuffer.${PACKAGE_VERSION}.nupkg"
