FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/AppSecDotnetDemo/AppSecDotnetDemo.csproj src/AppSecDotnetDemo/
COPY plugins/LegitimatePlugin/LegitimatePlugin.csproj plugins/LegitimatePlugin/
COPY plugins/HardcodedAttackPlugin/HardcodedAttackPlugin.csproj plugins/HardcodedAttackPlugin/
COPY plugins/TaintedInputPlugin/TaintedInputPlugin.csproj plugins/TaintedInputPlugin/
RUN dotnet restore src/AppSecDotnetDemo/AppSecDotnetDemo.csproj
RUN dotnet restore plugins/LegitimatePlugin/LegitimatePlugin.csproj
RUN dotnet restore plugins/HardcodedAttackPlugin/HardcodedAttackPlugin.csproj
RUN dotnet restore plugins/TaintedInputPlugin/TaintedInputPlugin.csproj

COPY src/AppSecDotnetDemo/ src/AppSecDotnetDemo/
COPY plugins/ plugins/
RUN dotnet build plugins/LegitimatePlugin -c Release
RUN dotnet build plugins/HardcodedAttackPlugin -c Release
RUN dotnet build plugins/TaintedInputPlugin -c Release
RUN dotnet publish src/AppSecDotnetDemo/AppSecDotnetDemo.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish ./
RUN mkdir -p /app/assemblies
COPY --from=build /src/plugins/LegitimatePlugin/bin/Release/net8.0/LegitimatePlugin.dll ./assemblies/
COPY --from=build /src/plugins/HardcodedAttackPlugin/bin/Release/net8.0/HardcodedAttackPlugin.dll ./assemblies/
COPY --from=build /src/plugins/TaintedInputPlugin/bin/Release/net8.0/TaintedInputPlugin.dll ./assemblies/

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "AppSecDotnetDemo.dll"]
