# Imagen de ZUNOX 3D para Render: la API y la web (carpeta frontend) en un solo servicio.
# El contexto de build es la RAÍZ del repo (en Render: Root Directory vacío y Dockerfile Path "backend/Dockerfile"),
# porque la imagen necesita las dos carpetas: backend/ y frontend/.

# --- 1. Compilar y publicar la API ---
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Primero solo el .csproj: mientras no cambien los paquetes, Docker reutiliza esta capa.
COPY backend/backend.csproj backend/
RUN dotnet restore backend/backend.csproj

COPY backend/ backend/
RUN dotnet publish backend/backend.csproj -c Release -o /app --no-restore

# --- 2. Imagen final, solo con lo necesario para correr ---
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app .

# La web que sirve la API (Program.cs la busca en /app/frontend).
COPY frontend/ ./frontend/

ENV ASPNETCORE_ENVIRONMENT=Production
# Render indica el puerto con la variable PORT (la app la lee en Program.cs); este es el valor por defecto.
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

# No corre como root.
USER $APP_UID

ENTRYPOINT ["dotnet", "backend.dll"]
