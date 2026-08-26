# Imagen de la API de SoundMate.
#
# Build en dos etapas: se compila con el SDK (que pesa ~800 MB y lleva compilador, NuGet y
# MSBuild) pero lo publicado se copia sobre la imagen de runtime, que no lleva nada de eso.
# La imagen final es mucho mas pequena y tiene mucha menos superficie de ataque.
#
# El contexto de build es la RAIZ del repo, no esta carpeta: hacen falta los cuatro .csproj
# y Directory.Build.props. Desde deploy/ ya viene dado (context: ..). A mano, desde la raiz:
#   docker build -f SoundMate.API/Dockerfile -t soundmate-api .

# ---------------------------------------------------------------- etapa 1: compilar
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Los .csproj primero y solos. Mientras no cambien las referencias de paquetes, Docker
# reutiliza la capa del restore y no vuelve a bajar NuGet en cada build. Si copiaramos todo
# de golpe, cualquier cambio en un .cs invalidaria el restore entero.
COPY Directory.Build.props ./
COPY SoundMate.API/SoundMate.Api.csproj SoundMate.API/
COPY SoundMate.Application/SoundMate.Application.csproj SoundMate.Application/
COPY SoundMate.Domain/SoundMate.Domain.csproj SoundMate.Domain/
COPY SoundMate.Infrastructure/SoundMate.Infrastructure.csproj SoundMate.Infrastructure/
RUN dotnet restore SoundMate.API/SoundMate.Api.csproj

# Ahora el codigo. OJO con las mayusculas: el contenedor es Linux y distingue, la carpeta es
# "SoundMate.API" pero el proyecto dentro se llama "SoundMate.Api.csproj". En Windows da
# igual; aqui un fallo de caja rompe el build.
COPY SoundMate.API/ SoundMate.API/
COPY SoundMate.Application/ SoundMate.Application/
COPY SoundMate.Domain/ SoundMate.Domain/
COPY SoundMate.Infrastructure/ SoundMate.Infrastructure/

# UseAppHost=false: el ejecutable nativo no sirve de nada aqui, arrancamos con `dotnet x.dll`.
RUN dotnet publish SoundMate.API/SoundMate.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

# ---------------------------------------------------------------- etapa 2: runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Solo HTTP dentro del contenedor: el certificado de desarrollo de ASP.NET vive en el perfil
# de Windows y aqui no existe, y en produccion el TLS lo termina el proxy de delante.
# Program.cs se salta UseHttpsRedirection cuando detecta DOTNET_RUNNING_IN_CONTAINER (lo pone
# esta imagen base), asi que nadie acaba redirigido a un puerto donde no escucha nadie.
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .

# Usuario sin privilegios que ya trae la imagen (APP_UID = 1654). Si algun dia la app
# necesita escribir en disco, habra que darle permisos explicitos a esa ruta.
USER $APP_UID

ENTRYPOINT ["dotnet", "SoundMate.Api.dll"]
