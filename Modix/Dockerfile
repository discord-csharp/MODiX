FROM microsoft/aspnetcore:2.0
ARG source
WORKDIR /app
COPY ${source:-obj/Docker/publish} .
SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop';"]
RUN set-itemproperty -path 'HKLM:\SYSTEM\CurrentControlSet\Services\Dnscache\Parameters' -Name ServerPriorityTimeLimit -Value 0 -Type DWord
ENTRYPOINT ["dotnet", "Modix.dll"]
