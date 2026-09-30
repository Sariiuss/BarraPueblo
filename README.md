# Barra de Valdorros

Calculadora de precios para móvil, tablet y ordenador. Está servida por ASP.NET Core 10 y funciona como una aplicación web instalable (PWA). No registra ventas, no tiene base de datos y no envía pedidos al servidor.

## Ejecutar

Instala el SDK .NET 10, abre esta carpeta y ejecuta:

```sh
dotnet run --urls http://0.0.0.0:5080
```

Abre http://localhost:5080. Para un móvil conectado al mismo wifi, abre `http://IP-DEL-ORDENADOR:5080` y permite el puerto 5080 en el firewall.

En Visual Studio abre `BarraPueblo.csproj`.

## Uso

1. Añade o quita bebidas con + / −.
2. Introduce el dinero recibido, con coma o punto, o pulsa 10 / 20 / 50 €.
3. Lee las vueltas o el importe que falta.
4. Pulsa **Nuevo pedido / borrar** para empezar de cero.

Todo el cálculo se realiza en el dispositivo. Al recargar o cerrar la aplicación se vacía el pedido actual.

## Instalar y usar sin Internet

La primera apertura necesita conexión para descargar la aplicación. Después:

- Android (Chrome): menú ⋮ → **Añadir a pantalla de inicio** o **Instalar aplicación**.
- iPhone (Safari): botón Compartir → **Añadir a pantalla de inicio**.

Ábrela una vez desde el icono mientras todavía tienes conexión. A partir de entonces la calculadora, los precios y el escudo quedan guardados en el móvil y funcionan sin Internet.

## Docker / alojamiento

```sh
docker build -t barra-pueblo .
docker run -p 5080:8080 barra-pueblo
```

GitHub almacena el código. Para instalar la PWA en móviles, el sitio publicado debe servirse por HTTPS.

## Precios

Agua 1 €; refresco 2 €; cachi Kalimotxo/cerveza 5 €; cachi cubata 11 €; pinta cerveza/Kali 3 €; cubata 6 €; caña/botellín 1,50 €; Radler/0,0 1,50 €; chupito 1,50 €; Jäger 2 €; vino 1,50 €; vermut 2 €; mosto 1,50 €; zumo/batido 2 €.

Edita el catálogo en `wwwroot/index.html`; los precios se expresan en céntimos.

## Escudo

La imagen `wwwroot/escudo-valdorros.png` es una miniatura sin modificaciones del [Escudo de Valdorros](https://commons.wikimedia.org/wiki/File:Escudo_de_Valdorros.svg), obra de Asqueladd, utilizada bajo licencia [CC BY-SA 3.0](https://creativecommons.org/licenses/by-sa/3.0/). El diseño heráldico municipal fue aprobado oficialmente el 16 de marzo de 1998.
