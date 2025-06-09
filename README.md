# Sistema Radar7D - .NET 8 + Angular 17 + SAML 2.0

Aplicación web desarrollada con ASP.NET Core 8.0 y Angular 17. Implementa autenticación federada con SAML 2.0 (NAM y Okta) y funcionalidad de Single Logout (SLO). Utiliza **Serilog** para el registro de eventos en archivos locales y está contenida en Docker para facilitar su ejecución y despliegue.

---

## 🏗️ Arquitectura

- **Backend**: ASP.NET Core 8.0
- **Frontend**: Angular 17 (ubicado en `/ClientApp`)
- **Autenticación**: Protocolo SAML 2.0 con certificados autofirmados
- **Logging**: Serilog (archivos en `/Logs`)
- **Contenedores**: Docker + Docker Compose para desarrollo

---

## 🚀 Ejecución en entorno local (Docker)

### 1. Clonar el repositorio y navegar al proyecto:

```bash
git clone https://tu-repo.git
cd powerbi-embedded-radar7d
```

### 2. Configurar variables de entorno

Crear el archivo `.env` con las variables necesarias. Puedes usar `.env.example` como base.

### 3. Levantar el entorno:

```bash
docker-compose -f docker-compose.dev.yml up --build
```

Este comando construye y levanta el contenedor con .NET + Angular en modo desarrollo, expone los puertos `5000`, `5001`, y `4200`.

---

## 🗂️ Estructura del proyecto

```bash
powerbi-embedded-radar7d/
│
├── App_Data/                # Certificados SAML
├── ClientApp/              # Aplicación Angular
├── Common/                 # Lógica auxiliar (claims, certificados)
├── Controllers/            # Controladores API (incluye AuthController)
├── Models/                 # Modelos de usuario y claims
├── Pages/                  # Razor Pages, Error.cshtml
├── Services/               # Servicios auxiliares (Power BI, TokenHelper)
├── Logs/                   # Logs generados por Serilog
├── Startup.cs              # Configuración de servicios
├── appsettings.*.json      # Configuraciones por entorno
├── docker-compose.dev.yml  # Configuración de entorno de desarrollo
└── Dockerfile.dev          # Dockerfile multistage (desarrollo)
```

---

## 🔐 Autenticación SAML 2.0

### Federación con NAM / Okta

El sistema está federado contra NAM / Okta utilizando la biblioteca `ITfoxtec.Identity.Saml2`.

### Certificados ()

- Certificado público: `crtbase64.txt` <=  `se le entrega al idp dentro del xml`
- Certificado público: `pfxbase64.txt` en  `.env` como `Saml2__SigningCertificate`
- Contraseña configurada en `.env` como `Saml2__SigningCertificatePassword`

---

## 🔄 Single Logout (SLO)

Implementado en las rutas:

- `/Auth/SingleLogout`
- `/Auth/LoggedOut`

Si un usuario cierra sesión en esta app o en otra federada, se termina la sesión globalmente en el navegador.

---

## 📝 Logs

Serilog guarda los logs en:

```bash
/Logs/radar7-yyyyMMdd.txt
```

Configuración en `appsettings.Development.json` → sección `"Serilog"`.

---

## 📊 Power BI

El sistema se conecta a reportes de Power BI Embedded. Las configuraciones están en las variables de entorno (.env):
- `ASPNETCORE_ENVIRONMENT`

- `Saml2__SigningCertificate= use App_Data/CertificadosDev/pfxbase64.txt`
- `Saml2__SigningCertificatePassword=Template2022`

- `ConnectionPowerBi__TenantID`
- `ConnectionPowerBi__ClientID`
- `ConnectionPowerBi__ClientSecret`
- `ConnectionPowerBi__WorkspaceId`
- `ConnectionPowerBi__ReportCurrentId`
- `ConnectionPowerBi__ReportArchivedId`

---

## 🧪 Desarrollo sin contenedor

Editar en `Startup.cs`:

```csharp
if (env.IsDevelopment())
    spa.UseAngularCliServer(npmScript: "start");
// spa.UseProxyToSpaDevelopmentServer("http://localhost:4200");
```

Y luego:

```bash
# Ejecutar backend
dotnet run

# En otra terminal, ejecutar Angular
cd ClientApp
npm install
npm start
```

---

## 📁 Recursos adicionales

- [Librería ITfoxtec SAML2](https://www.itfoxtec.com/IdentitySaml2)
- [Documentación SAML2 - GitHub](https://github.com/ti-tecnologico-de-monterrey-oficial/template-net-core-angular-saml2)
- [OpenSSL para certificados](https://www.openssl.org/)
- [SAML Tool para formatear X509](https://www.samltool.com/format_x509cert.php)
