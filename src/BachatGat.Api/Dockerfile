# Stage 1 — restore (layer-cached on .csproj changes only)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS restore
WORKDIR /repo

COPY BachatGat.slnx ./
COPY src/BachatGat.Api/BachatGat.Api.csproj                         ./src/BachatGat.Api/
COPY src/BachatGat.Application/BachatGat.Application.csproj         ./src/BachatGat.Application/
COPY src/BachatGat.Core/BachatGat.Core.csproj                       ./src/BachatGat.Core/
COPY src/BachatGat.Infrastructure/BachatGat.Infrastructure.csproj   ./src/BachatGat.Infrastructure/

RUN dotnet restore BachatGat.slnx

# Stage 2 — build & publish
FROM restore AS publish
COPY src/ ./src/
RUN dotnet publish src/BachatGat.Api/BachatGat.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

# Stage 3 — runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
RUN mkdir -p logs
COPY --from=publish /app/publish .

RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser
RUN chown -R appuser:appgroup /app
USER appuser

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "BachatGat.Api.dll"]
