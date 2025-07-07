FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ParaglidingFlightLogWeb/ParaglidingFlightLogWeb/ParaglidingFlightLogWeb.csproj", "ParaglidingFlightLogWeb/ParaglidingFlightLogWeb/"]
COPY ["ParagliderFlightLog/ParagliderFlightLog.csproj", "ParagliderFlightLog/"]
COPY ["ParaglidingFlightLogWeb/ParaglidingFlightLogWeb.Client/ParaglidingFlightLogWeb.Client.csproj", "ParaglidingFlightLogWeb/ParaglidingFlightLogWeb.Client/"]
RUN dotnet restore "ParaglidingFlightLogWeb/ParaglidingFlightLogWeb/ParaglidingFlightLogWeb.csproj"
COPY . .
WORKDIR "/src/ParaglidingFlightLogWeb/ParaglidingFlightLogWeb"
RUN dotnet build "./ParaglidingFlightLogWeb.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ParaglidingFlightLogWeb.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN mkdir -p ./data/score
COPY ["Data/UserDbParagliderFlightLog.db", "./data/UserDbParagliderFlightLog.db"]
COPY ["Data/6f897e17-1b06-4651-8015-755e857b4e86/", "./data/6f897e17-1b06-4651-8015-755e857b4e86/"]
ENTRYPOINT ["dotnet", "ParaglidingFlightLogWeb.dll"]