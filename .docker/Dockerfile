FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS dotnet-build
WORKDIR /build

ARG Version
ARG AssemblyVersion
ARG FileVersion
ARG InformationalVersion

COPY . .
WORKDIR /build/Server
RUN dotnet publish -c Release -o out \
    -p:Version=$Version \
    -p:AssemblyVersion=$AssemblyVersion \
    -p:FileVersion=$FileVersion \
    -p:InformationalVersion=$InformationalVersion

FROM node:lts-alpine AS node-build
WORKDIR /build
RUN apk add yarn

COPY . .
WORKDIR /build/Client
RUN yarn install \
 && yarn build --dest out


FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS runtime

ARG InformationalVersion
ARG BuildDate
ARG Sha

ENV DEBIAN_FRONTEND noninteractive
WORKDIR /app
EXPOSE 80
RUN apt update \
 && apt install -y --no-install-recommends ffmpeg \
 && apt clean \
 && rm -rf  /var/lib/apt/lists/*
COPY --from=dotnet-build /build/Server/out ./
COPY --from=node-build /build/Client/out ./Client/
ENTRYPOINT ["dotnet", "Server.dll"]

LABEL org.label-schema.schema-version="1.0" \
      org.label-schema.build-date=$BuildDate \
      org.label-schema.name="Vivideo" \
      org.label-schema.description="Vivideo is video streming server" \
      org.label-schema.vcs-url="https://github.com/Freeesia/vivideo" \
      org.label-schema.vcs-ref=$Sha \
      org.label-schema.version=$InformationalVersion
