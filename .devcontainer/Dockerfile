FROM mcr.microsoft.com/vscode/devcontainers/dotnet:0-7.0

RUN echo 'APT::Default-Release "stable";\n' > /etc/apt/apt.conf.d/99target &&\
    echo 'deb http://ftp.jp.debian.org/debian/ unstable main non-free\n' >> /etc/apt/sources.list &&\
    apt-get update &&\
    apt-get install --no-install-recommends -y -t unstable ffmpeg
