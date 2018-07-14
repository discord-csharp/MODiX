FROM microsoft/dotnet:2.1-aspnetcore-runtime
ARG source
WORKDIR /app
COPY ${source:-obj/Docker/publish} .
EXPOSE 8080/tcp
ENTRYPOINT ["dotnet", "Modix.dll"]
