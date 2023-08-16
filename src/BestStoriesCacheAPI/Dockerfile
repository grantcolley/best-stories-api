#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/BestStoriesCacheAPI/BestStoriesCacheAPI.csproj", "src/BestStoriesCacheAPI/"]
COPY ["src/BestStories.Core/BestStories.Core.csproj", "src/BestStories.Core/"]
RUN dotnet restore "src/BestStoriesCacheAPI/BestStoriesCacheAPI.csproj"
COPY . .
WORKDIR "/src/src/BestStoriesCacheAPI"
RUN dotnet build "BestStoriesCacheAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BestStoriesCacheAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BestStoriesCacheAPI.dll"]