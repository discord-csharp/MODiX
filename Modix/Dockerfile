FROM microsoft/aspnetcore:2.0
ARG source
WORKDIR /app
COPY ${source:-obj/Docker/publish} .
SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop';"]
RUN set-itemproperty -path 'HKLM:\SYSTEM\CurrentControlSet\Services\Dnscache\Parameters' -Name ServerPriorityTimeLimit -Value 0 -Type DWord
EXPOSE 8080/tcp
ENTRYPOINT ["dotnet", "Modix.dll"]
