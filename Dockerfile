FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 10000

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["ChurchAttendanceSystem/ChurchAttendanceSystem.csproj", "ChurchAttendanceSystem/"]
RUN dotnet restore "ChurchAttendanceSystem/ChurchAttendanceSystem.csproj"
COPY . .
WORKDIR "/src/ChurchAttendanceSystem"
RUN dotnet build "ChurchAttendanceSystem.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ChurchAttendanceSystem.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN mkdir -p /app/data && chmod 777 /app/data
ENV ASPNETCORE_URLS=http://0.0.0.0:10000
ENV ConnectionStrings__Default="Data Source=/app/data/church.db"
ENV ASPNETCORE_ENVIRONMENT=Production
ENV Logging__LogLevel__Default=Information
ENTRYPOINT ["dotnet", "ChurchAttendanceSystem.dll"]