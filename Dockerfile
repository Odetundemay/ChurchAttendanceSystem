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
ENTRYPOINT ["dotnet", "ChurchAttendanceSystem.dll"]