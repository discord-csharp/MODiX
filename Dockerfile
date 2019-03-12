FROM microsoft/dotnet:2.2-sdk-stretch as dotnet-test
WORKDIR /src
COPY . .
RUN dotnet test Modix.Data.Test

FROM microsoft/dotnet:2.2-sdk-stretch as dotnet-build
WORKDIR /src
COPY . .

RUN apt-get update && apt install curl -y
RUN curl -sL https://deb.nodesource.com/setup_10.x | bash -
RUN apt-get install nodejs -y

RUN dotnet publish Modix.sln -c Release -r linux-x64 -o /app

FROM microsoft/dotnet:2.2-aspnetcore-runtime
WORKDIR /app
COPY --from=dotnet-build /app .

HEALTHCHECK CMD ./healthcheck.sh healthcheck.txt 120
ENTRYPOINT ["dotnet", "Modix.dll"]
