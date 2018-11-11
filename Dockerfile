FROM microsoft/dotnet:2.1-sdk-alpine as dotnet-build
WORKDIR /src
COPY . .
RUN apk add --update nodejs nodejs-npm
RUN dotnet publish Modix.sln -c Release -r linux-x64 -f netcoreapp2.1 -o /app

FROM microsoft/dotnet:2.1-aspnetcore-runtime
WORKDIR /app
COPY --from=dotnet-build /app .
ENTRYPOINT ["dotnet", "Modix.Bot.dll"]
