FROM microsoft/dotnet:2.1-sdk as dotnet-build
WORKDIR /src
COPY . .
RUN dotnet publish Modix.sln -r linux-x64 -f netcoreapp2.1 -o /app

FROM node:9 as node-build
WORKDIR /src
COPY Modix.Frontend .
RUN npm install && npm run build

FROM microsoft/dotnet:2.1-aspnetcore-runtime
WORKDIR /app
COPY --from=dotnet-build /app .
COPY --from=node-build /src/dist ./wwwroot
ENTRYPOINT ["dotnet", "Modix.dll"]
