FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/DictionaryService.Web/DictionaryService.Web.csproj", "src/DictionaryService.Web/"]
COPY ["src/DictionaryService.Application/DictionaryService.Application.csproj", "src/DictionaryService.Application/"]
COPY ["src/DictionaryService.Contracts/DictionaryService.Contracts.csproj", "src/DictionaryService.Contracts/"]
COPY ["src/DictionaryService.Domain/DictionaryService.Domain.csproj", "src/DictionaryService.Domain/"]
COPY ["src/DictionaryService.Infrastructure/DictionaryService.Infrastructure.csproj", "src/DictionaryService.Infrastructure/"]
COPY ["src/DictionaryService.Presenters/DictionaryService.Presenters.csproj", "src/DictionaryService.Presenters/"]

COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]

RUN dotnet restore "src/DictionaryService.Web/DictionaryService.Web.csproj"
COPY . .
WORKDIR "/src/src/DictionaryService.Web"
RUN dotnet build "./DictionaryService.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DictionaryService.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DictionaryService.Web.dll"]
