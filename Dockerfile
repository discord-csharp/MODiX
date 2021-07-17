FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS dotnet-build-base
WORKDIR /src
RUN apt-get update && apt-get install curl -y \
  && curl -sL https://deb.nodesource.com/setup_14.x | bash -\
  && apt-get install nodejs -y
COPY Modix.sln .
COPY Directory.* .
COPY **/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p ${file%.*}/ && mv $file ${file%.*}/; done
RUN dotnet restore Modix.sln
COPY . . 

FROM dotnet-build-base AS dotnet-build
RUN dotnet build -c Release --no-restore Modix.sln

FROM dotnet-build as dotnet-test
RUN dotnet test -c Release --no-build --no-restore Modix.sln

FROM dotnet-build AS publish
RUN dotnet publish -c Release --no-build --no-restore -o /app  Modix/Modix.csproj

FROM base AS final
COPY --from=publish /app .
COPY --from=publish /src/healthcheck.sh .
ENTRYPOINT ["dotnet", "Modix.dll"]
