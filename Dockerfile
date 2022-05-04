#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS publish
WORKDIR /src
COPY ["DlibInstanceSegmentaion.csproj", ""]
RUN dotnet restore "./DlibInstanceSegmentaion.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "DlibInstanceSegmentaion.csproj" -c Release -o /app/build
RUN dotnet publish "DlibInstanceSegmentaion.csproj" -c Release -o /app/publish

FROM  mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim  AS final
EXPOSE 2100
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DlibInstanceSegmentaion.dll"]