ARG DOTNET_VERSION
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS dotnet-build
WORKDIR /build

ARG Version
ARG AssemblyVersion
ARG FileVersion
ARG InformationalVersion

COPY . .
WORKDIR /build/Worker
RUN dotnet publish -c Release -o out \
    -p:Version=$Version \
    -p:AssemblyVersion=$AssemblyVersion \
    -p:FileVersion=$FileVersion \
    -p:InformationalVersion=$InformationalVersion

FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS runtime

ARG InformationalVersion
ARG BuildDate
ARG Sha

ENV DEBIAN_FRONTEND noninteractive
WORKDIR /app
RUN echo 'APT::Default-Release "stable";\n' > /etc/apt/apt.conf.d/99target &&\
    echo 'deb http://ftp.jp.debian.org/debian/ unstable main non-free\n' >> /etc/apt/sources.list &&\
    apt-get update &&\
    apt-get install --no-install-recommends -y -t unstable ffmpeg &&\
    apt-get clean &&\
    rm -rf  /var/lib/apt/lists/*
COPY --from=dotnet-build /build/Worker/out ./
ENTRYPOINT ["dotnet", "Worker.dll"]

LABEL org.label-schema.schema-version="1.0" \
    org.label-schema.build-date=$BuildDate \
    org.label-schema.name="Vivideo" \
    org.label-schema.description="Vivideo is video streming server" \
    org.label-schema.vcs-url="https://github.com/Freeesia/vivideo" \
    org.label-schema.vcs-ref=$Sha \
    org.label-schema.version=$InformationalVersion \
    org.opencontainers.image.source="https://github.com/Freeesia/vivideo"
