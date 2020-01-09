FROM mcr.microsoft.com/dotnet/core/sdk:3.1 as dotnet-test
WORKDIR /src
COPY . .
RUN sh build/test.sh

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 as dotnet-build
WORKDIR /src
COPY . .

RUN sh build/build.sh

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY --from=dotnet-build /app .
COPY --from=dotnet-build /src/healthcheck.sh .

ENTRYPOINT ["dotnet", "Modix.dll"]
