#  .NET CORE 8.0 Angular 17 SAML 2.0

Plantilla que muestra como federar con SAML 2.0 en NAM una aplicación desarrollada en NET 8.0 y Angular 17. Incluye funcionalidad de Single Logout (SLO) y registro de Logs en archivos con Serilog. Esta configurada en AMFS DEVL y Okta.

# Introducción

Al desarrollar una aplicación, dos de los puntos más importantes a considerar dentro de la arquitectura son: la **autenticación** y la **autorización**.

La **autenticación** es: ¿quién está ingresando al sistema? Y la **autorización** es: ¿qué acciones puede realizar dentro del sistema?

La autorización es responsabilidad de la aplicación, por lo que dentro del diseño y desarrollo hay que considerar la definición y creación de roles y permisos para ejecutar ciertas acciones en el sistema.

Para la autenticación existen muchos protocolos de identidad ya definidos, por ejemplo: WS-Federation, SAML, OAuth, OpenID Connect, etc.

En el Tec de Monterrey se utiliza el protocolo SAML 2.0 y básicamente implica que existe un servidor llamado proveedor de identidad (IdP) **(NAM)** que se encarga de recibir un usuario y contraseña y validar si el usuario se encuentra dentro del directorio activo del Tec y si su contraseña es correcta, si es así, regresa un token de autenticación en formato XML con unos atributos previamente configurados **(claims)**, tales como: la nómina, el correo, el nombre, etc., que permite a la aplicación identificar quién está ingresando y posterior a esto, realizar el proceso de autorización correspondiente.

## Flujo de Autenticación SAML 2.0.

<img width="800" alt="Flujo SAML" src="https://user-images.githubusercontent.com/75271834/157577517-24a53f6f-f19b-48da-9bb6-834f8a288c49.png">

## Librerías

Existen librerías para distintos lenguajes y frameworks que encapsulan la lógica del flujo SAML permitiendo con pocas líneas de código y configuraciones implementar la autenticación en las aplicaciones web.

En este ejemplo, se muestra una aplicación web con .NET 8.0 + Angular 17 y se hace uso de la librería [**ITFoxtec**](https://www.itfoxtec.com/IdentitySaml2).

# Instalación de Librerías y Configuración

1. Instalar los siguientes paquetes de NuGet para la federación:

   * ITfoxtec.Identity.Saml2
   * ITfoxtec.Identity.Saml2.MvcCore

2. Instalar los siguientes paquetes de NuGet para el log.

   * Serilog.AspNetCore
   * Serilog.Sinks.Console
   * Serilog.Sinks.File
 
3. Copiar de este repo la carpeta App_Data y su contenido. Aquí residen los archivos de certificado que se utilizan en la configuración de la federación.
4. Copiar de este repo la carpeta Common y su contenido. Se agregan clases auxiliares para leer los claims de los usuarios, así como leer el archivo del certificado de la ruta especificada en el punto anterior.
5. En la carpeta Controllers incluir el: AuthController.cs.
6. En cada controllador de la solución (excepto el AuthController) hay que incluir el decorador [Authorize] en el encabezado del controlador. Este atributo es el que ejecuta el proceso de autorización (la federación).
7. Incluir dentro de la carpeta Models la clase: UserClaims.cs. En esta clase se encapsularan los claims solicitados al equipo de identidad. Pueden variar, pero los que se configuraron en este repo son los más comunes.
8. En el archivo appsettings.json incluir las secciones necesarias para la configuración de la federación (sección: Saml2) y el log (sección: Serilog).
9. En el archivo Startup.cs incluir la configuración del log y la federación *(se indica con comentarios)*.
10. En el controllador inicial incluir el método GetUserClaims, es el encargado de leer los claims configurados para el usuario.

# Single Logout - SLO

Para configurar un Single Logout (SLO) se requieren 2 cosas:

1. Generar un certificado autofirmado.
2. Generar el metadata con la configuración de la federación SAML 2.0 con NAM.

## Generar Certificado

Para generar un certificado autofirmado se hace uso de la consola [**GitBash**](https://gitforwindows.org/) que ya viene con la herramienta [**OpenSSL**](https://www.openssl.org/) instalada.

1. Hay que posicionarse en la carpeta en donde se almacenaran los certificados, dar clic derecho sobre la superficie vacía y seleccionar del menú contextual la opción *Git Bash Here*.

<img src="https://user-images.githubusercontent.com/75271834/157761683-10331e1d-3986-47e3-8688-d0efdb87f94d.png" alt="Abrir Git Bash" style="width: 400px;">

2. Ingresar el siguiente comando para generar el certificado público:

```winpty openssl req -x509 -nodes -days 18250 -newkey rsa:2048 -keyout private.key -out certificate.crt```

*El parámetro **-days** indica el número de días que estará vigente el certificado que vamos a generar.*

Después de presionar *Enter*, la consola requerira cierta información para la creación del certificado. Se ingresa la información solicitada y se presiona *Enter*. Si hay algún dato que no tenemos, se escribe un **punto** y se presiona *Enter*, para dejarlo vacío.

<img width="600" alt="Genera Certificado.crt" src="https://user-images.githubusercontent.com/75271834/157764343-806e1085-1d0a-455a-a9de-c95e4d4d0aca.png">

Al terminar de ingresar toda la información solicitada, en la carpeta veremos que se generaron 2 archivos:

  * **certificate.crt** - Este es el certificado público que se le comparte al equipo de [**Identidad**](mailto:dsi.identidad@itesm.mx).
  * **private.key** - Esta es la llave privada que se requiere para generar el certificado privado **.pfx**.

3. Ingresar el siguiente comando para generar el certificado privado:

```winpty openssl pkcs12 -export -out certificate.pfx -CSP "Microsoft Platform Crypto Provider" -inkey private.key -in certificate.crt```

Al presionar *Enter* solicitara ingresemos una contraseña. Es muy importante anotarla, ya que es necesaria para poder leer dicho certificado.  
Al escribirla no se ve reflejada en pantalla, se presiona *Enter* y solicita confirmar.

<img width="600" alt="Genera Certificado.pfx" src="https://user-images.githubusercontent.com/75271834/157916378-7a14ab2b-8e65-4b40-a137-85f41af9118d.png">

En la carpeta ahora veremos un archivo adicional:

  * **certificate.pfx** - Este es el certificado privado que se lee dentro del código para firmar los SAML Requests.

Al equipo de Identidad se le comparte el archivo: **certificate.crt** y en el appsettings.json se hace referencia al archivo: **certificate.pfx** y a su contraseña correspondiente.  

**NOTA: Cualquier cambio en los certificados, implica volver a configurar el metadata y cargarlo nuevamente en NAM.**

## Generar Metadata

El metadata es un archivo XML que contiene la configuración de la federación de la aplicación web *(Service Provider o SP)*. Este archivo se comparte al equipo de Identidad y ellos lo cargan dentro de NAM.

Para generar uno, se puede tomar como base este ejemplo: [Metadata](https://github.com/ti-tecnologico-de-monterrey-oficial/template-net-core-angular-saml2/blob/develop/Radar7D/App_Data/Metadata/Metadata_DEV_Radar7D.xml) y realizar los siguientes ajustes:

  * En el atributo **entityID** del elemento **<md:EntityDescriptor>** se debe colocar la URL del servicio.
  * Dentro del elemento **<ds:X509Certificate>** se debe colocar el **certificate.crt** que se genero previamente. Este archivo se puede abrir con un editor de texto como Notepad.
*(En este página web se puede formatear el certicado: [Saml Developer Tools](https://www.samltool.com/format_x509cert.php))*
  * En los atributos **Location** y **ResponseLocation** del elemento **<md:SingleLogoutService>** se debe cambiar la URL del servicio, dejando las rutas: **/Auth/SingleLogout** y **/Auth/LoggedOut**.
  * En el atributo **Location** del elemento **<md:AssertionConsumerService>** se debe cambiar la URL del servicio, dejando la ruta **/Auth/AssertionConsumerService**.

**NOTA: Es importante no cambiar el orden de los elementos del XML. Cualquier cambio de orden ocasiona errores al cargar el archivo en el *(Identity Provider o IdP).***

## Escenarios del Single Logout - SLO

Para considerar que un Single Logout esta completo se deben de validar los siguientes escenarios:

En un explorador *(Chrome, Firefox, Safari, etc.)* tener una sesión iniciada en mi aplicación *(que nombraremos como aplicación A)* y cualquier otra aplicación que también tenga implementado SLO *(que nombraremos como aplicación B)*, cada una de ellas abiertas en diferentes pestañas de la misma ventana del explorador.

 **1. Cerrar sesión desde la aplicación A.**
 
  **Resultado esperado:**
 
   * **En la aplicación A.** *(La que disparó el SLO).*
    
     * La página se refresca y muestra el Login, indicando que la sesión se cerró exitosamente.
    
   * **En la aplicación B.** *(La que se encontraba en otra pestaña del mismo explorador donde se disparó el SLO).*
    
     * Permanece tal cual se encontraba, pero si se presiona cualquier opción de la aplicación, debe redirigir al login, indicado que también se cerró sesión en esta aplicación aun cuando el SLO se disparó desde de la aplicación A.

 **2. Cerrar sesión desde la aplicación B.**

 **Resultado esperado:**
 
   * **En la aplicación A.** *(La que se encontraba en otra pestaña del mismo explorador donde se disparó el SLO).*
     
     * Permanece tal cual se encontraba, pero si se presiona cualquier opción de la aplicación, debe redirigir al login, indicado que también se cerró sesión en esta aplicación aun cuando el SLO se disparó desde de la aplicación B.
    
   * **En la aplicación B.** *(La que disparó el SLO).*
   
     * La página se refresca y muestra el Login, indicando que la sesión se cerró exitosamente.
  
  **NOTA: Si alguno de estos escenarios no cumple con el resultado esperado, es un indicativo de que la aplicación A o B, no tienen implementado el SLO correctamente.**
  



# 🧪 Template Angular + ASP.NET Core (SAML)

Proyecto que combina Angular y ASP.NET Core (.NET 8) con autenticación SAML, usando Docker para desarrollo.

## 🚀 Instrucciones para desarrollo local con Docker

```bash
# 1. Levanta el contenedor en modo desarrollo
docker-compose -f docker-compose.dev.yml up --build

# 2. Abre Terminal 1 y entra al contenedor
docker exec -it <container_id> bash

# 3. Dentro del contenedor, genera el certificado y ejecuta .NET con hot reload
dotnet dev-certs https
dotnet watch run

# 4. Abre Terminal 2 y entra nuevamente al contenedor
docker exec -it <container_id> bash

# 5. Instala dependencias de Angular y levanta el frontend
npm install --prefix ClientApp
npm start --prefix ClientApp

```

## 🚀 Instrucciones para desarrollo sin contendor
 ### ir al archivo Startup.cs y dejarlo asi 
 ```bash
 if (env.IsDevelopment())
                    spa.UseAngularCliServer(npmScript: "start");
                    //spa.UseProxyToSpaDevelopmentServer("http://localhost:4200");