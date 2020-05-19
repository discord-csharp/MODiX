FROM mcr.microsoft.com/dotnet/sdk:5.0 as dotnet-test
WORKDIR /src
COPY . .
RUN sh build/test.sh

FROM mcr.microsoft.com/dotnet/sdk:5.0 as dotnet-build
WORKDIR /src
COPY . .

RUN sh build/build.sh

FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /app
COPY --from=dotnet-build /app .
COPY --from=dotnet-build /src/healthcheck.sh .

RUN apt-get update && apt-get install -y ca-certificates
ENTRYPOINT ["dotnet", "Modix.dll"]