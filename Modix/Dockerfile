FROM microsoft/dotnet:1.1.2-runtime-nanoserver
ARG source
WORKDIR /app
COPY ${source:-obj/Docker/publish} .
SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop';"]
RUN set-itemproperty -path 'HKLM:\SYSTEM\CurrentControlSet\Services\Dnscache\Parameters' -Name ServerPriorityTimeLimit -Value 0 -Type DWord
ENTRYPOINT ["dotnet", "Modix.dll"]
