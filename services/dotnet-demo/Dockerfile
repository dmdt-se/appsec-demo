FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/AppSecDotnetDemo/AppSecDotnetDemo.csproj src/AppSecDotnetDemo/
RUN dotnet restore src/AppSecDotnetDemo/AppSecDotnetDemo.csproj

COPY src/AppSecDotnetDemo/ src/AppSecDotnetDemo/
RUN dotnet publish src/AppSecDotnetDemo/AppSecDotnetDemo.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish ./
COPY assemblies ./assemblies

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "AppSecDotnetDemo.dll"]
