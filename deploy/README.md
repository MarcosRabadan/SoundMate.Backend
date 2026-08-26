# deploy/ — Infraestructura local de desarrollo

Contiene la infra que SoundMate necesita para desarrollo y pruebas en local, y la imagen de la
API. **No es infraestructura de producción.**

## Contenido

- `docker-compose.yml` — levanta los servicios en contenedores Docker.
- `.env.example` — plantilla de las variables de entorno. Cópiala a `.env` (ignorado por git).
- El `Dockerfile` de la API vive en [`../SoundMate.API/Dockerfile`](../SoundMate.API/Dockerfile),
  junto al proyecto que empaqueta.

## Requisitos

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) instalado y en marcha.

## Puertos

Están **corridos respecto a los de Agendia** a propósito: los dos microservicios se desarrollan a
la vez y no pueden pisarse.

| Servicio   | SoundMate                                        | Agendia (referencia) |
|------------|--------------------------------------------------|----------------------|
| PostgreSQL | `localhost:5434`                                 | 5433                 |
| Seq        | http://localhost:5342                            | 5341                 |
| RabbitMQ   | http://localhost:15673 (AMQP 5673)               | 15672 / 5672         |
| API        | http://localhost:8080/scalar/v1 *(en contenedor)*| —                    |

El **5432** lo ocupa el PostgreSQL nativo de la máquina, y el **5046** es el de `dotnet run`, por
eso la API en contenedor sale por el 8080.

Credenciales de todo (dev, por eso están escritas en el compose): usuario `soundmate`,
clave `soundmate`, base de datos `soundmate`.

## Uso

Desde esta carpeta (`deploy/`):

```bash
docker compose up -d              # solo la infra (la 1a vez descarga las imágenes)
docker compose ps                 # ver estado
docker compose logs -f postgres   # ver logs en vivo de un servicio
docker compose down               # parar (los datos se conservan en los volúmenes)
```

### Levantar también la API en contenedor

El servicio `api` está detrás de un **perfil**, así que `docker compose up -d` a secas **no** lo
levanta. El día a día es API con `dotnet run` (hot reload) contra la infra en contenedores; el
contenedor de la API se pide aparte, cuando quieres probar la imagen de verdad:

```bash
docker compose --profile app up -d --build
```

Antes hace falta el secreto de Agendia, que en el host vive en user-secrets y el contenedor no ve:

```bash
cp .env.example .env
```

y rellenar `AGENDIA_CLIENT_SECRET` con el valor de
`dotnet user-secrets list --project ../SoundMate.API`.

Para parar solo la API sin tumbar la infra:

```bash
docker compose --profile app stop api
```

## Primera vez: mover la base de datos de dev al contenedor

El connection string de dev apuntaba al PostgreSQL **nativo** del 5432. Para usar el contenedor,
apúntalo al 5434 y aplica las migraciones (la base de datos del contenedor arranca vacía):

```bash
dotnet user-secrets set "ConnectionStrings:SoundMate" "Host=localhost;Port=5434;Database=soundmate;Username=soundmate;Password=soundmate" --project ../SoundMate.API
```

```bash
cd .. && $env:ASPNETCORE_ENVIRONMENT="Development"; dotnet ef database update --project SoundMate.Infrastructure --startup-project SoundMate.API
```

El contenedor de la API **no aplica migraciones al arrancar**: las sigues aplicando tú desde el
host, contra el 5434. Si arrancas la API sin haberlas aplicado, arranca igual y falla en la
primera consulta.

## Notas

- **El proyecto de compose se llama `soundmate`** (línea `name:` arriba del archivo). Docker lo
  sacaría del nombre de la carpeta, y en el repo de Agendia la carpeta también se llama `deploy`:
  sin esa línea los dos compose compartirían proyecto y un `docker compose down` aquí intentaría
  tumbar los contenedores de Agendia.
- **Agendia se ve desde el contenedor como `host.docker.internal:7097`**, no como `localhost` —
  dentro del contenedor `localhost` es el contenedor mismo. Y **Agendia tiene que estar arrancada
  en el host** para que `GET /api/agendia/connection` responda.
- **Ese salto lleva un `Agendia__DangerousAcceptAnyServerCertificate: "true"`** en el compose, y
  conviene entender por qué antes de copiarlo a ningún sitio. Apuntar al puerto HTTP de Agendia
  (5255) no sirve: contesta `307` hacia `https://host.docker.internal:7097`, así que se acaba en
  HTTPS igual. Y el certificado de desarrollo de ASP.NET falla por dos motivos a la vez —
  lo emitieron para `localhost`, no para `host.docker.internal`, y su CA no está en el almacén de
  confianza del contenedor. El problema es **el certificado local, no Agendia**. La opción está
  apagada por defecto y **este archivo es el único sitio que la enciende**: en producción
  aceptaría cualquier certificado de cualquiera y le entregaría nuestro token de servicio.
- Las versiones de las imágenes están **fijadas** (no `latest`) para que la infra sea
  reproducible. Actualiza los tags conscientemente.
- Los datos persisten en volúmenes Docker (`postgres-data`, `seq-data`, `rabbitmq-data`), así que
  parar/arrancar no pierde nada. `docker compose down -v` **sí** los borra.
- **Seq y RabbitMQ están levantados pero aún no conectados a SoundMate**: no hay Serilog ni
  transporte de eventos todavía. Están aquí para no tener que tocar la infra cuando toque
  cablearlos (Agendia tiene RabbitMQ en la misma situación).
