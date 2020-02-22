FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS dotnet-build
WORKDIR /build

COPY . .
WORKDIR /build/Server
RUN dotnet publish -c Release -o out

FROM node:lts-alpine AS node-build
WORKDIR /build
RUN apk add yarn

COPY . .
WORKDIR /build/Client
RUN yarn install \
 && yarn build --dest out


FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS runtime
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
