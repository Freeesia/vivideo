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

FROM mcr.microsoft.com/dotnet/runtime:${DOTNET_VERSION} AS runtime

ARG InformationalVersion
ARG BuildDate
ARG Sha

ENV DEBIAN_FRONTEND noninteractive
WORKDIR /app
RUN apt update \
 && apt install -y --no-install-recommends ffmpeg \
 && apt clean \
 && rm -rf  /var/lib/apt/lists/*
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
