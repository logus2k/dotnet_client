# Alpine with dotnet SDK
FROM alpine:3.15

RUN apk add bash icu-libs krb5-libs libgcc libintl libssl1.1 libstdc++ zlib
RUN apk add libgdiplus --repository https://dl-3.alpinelinux.org/alpine/edge/testing/

WORKDIR /

# Local file was downloaded from:
# https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-6.0.201-linux-x64-alpine-binaries
COPY dotnet-sdk-6.0.201-linux-musl-x64.tar.gz dotnet.tar.gz

RUN mkdir -p /usr/share/dotnet && tar zxf dotnet.tar.gz -C /usr/share/dotnet && rm dotnet.tar.gz
ENV PATH=$PATH:/usr/share/dotnet

ENV DOTNET_CLI_HOME=/tmp
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1

RUN dotnet tool install --global dotnet-sonarscanner --version 5.5.0
ENV PATH=$PATH:/tmp/.dotnet/tools

RUN chmod -R 777 /tmp
RUN mkdir /.local
RUN chmod -R 777 /.local
RUN mkdir /Microsoft
RUN chmod -R 777 /Microsoft

RUN apk add openjdk11-jre
