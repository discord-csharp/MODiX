FROM microsoft/dotnet:2.1-sdk-stretch as dotnet-build
WORKDIR /src
COPY . .

RUN apt-get update && apt install curl -y
RUN curl -sL https://deb.nodesource.com/setup_9.x | bash -
RUN apt-get install nodejs -y

RUN dotnet publish Modix.sln -c Release -r linux-x64 -o /app

FROM microsoft/dotnet:2.1-aspnetcore-runtime
WORKDIR /app
COPY --from=dotnet-build /app .
ENTRYPOINT ["dotnet", "Modix.dll"]
