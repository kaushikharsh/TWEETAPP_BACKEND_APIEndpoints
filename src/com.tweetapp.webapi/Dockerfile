#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["src/com.tweetapp.webapi/com.tweetapp.webapi.csproj", "src/com.tweetapp.webapi/"]
COPY ["src/com.tweetapp.service/com.tweetapp.service.csproj", "src/com.tweetapp.service/"]
COPY ["src/com.tweetapp.DAO/com.tweetapp.DAO.csproj", "src/com.tweetapp.DAO/"]
RUN dotnet restore "src/com.tweetapp.webapi/com.tweetapp.webapi.csproj"
COPY . .
WORKDIR "/src/src/com.tweetapp.webapi"
RUN dotnet build "com.tweetapp.webapi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "com.tweetapp.webapi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "com.tweetapp.webapi.dll"]