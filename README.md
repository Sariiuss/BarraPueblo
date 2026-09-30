# Barra del pueblo

TPV sencillo para móvil, tablet y ordenador. Backend real en C# / ASP.NET Core 10, pantalla HTML/CSS/JavaScript servida por la misma aplicación. Sin bingo.

## Ejecutar

Instala el SDK .NET 10, abre esta carpeta y ejecuta:

```sh
dotnet run --urls http://0.0.0.0:5080
```

Abre http://localhost:5080. Para un móvil conectado al mismo wifi, abre `http://IP-DEL-ORDENADOR:5080` y permite el puerto 5080 en el firewall. El ordenador tiene que permanecer encendido. No hace falta Internet si los dispositivos están en la misma red.

En Visual Studio abre `BarraPueblo.csproj`.

## Uso

1. Añade o quita bebidas con + / −.
2. Introduce el dinero recibido, con coma o punto, o pulsa 10 / 20 / 50 €.
3. Lee el cambio. Si falta dinero, no permite cobrar.
4. Pulsa **Cobrar y guardar venta**. Guarda la venta, deja visible el cambio del último cobro y vacía el pedido.
5. **Ventas** muestra pedidos, unidades y recaudación. **Nuevo pedido** vacía sin registrar una venta.

Las ventas se guardan en `data/sales.json` (o `BAR_DATA_DIR`). Se comparten entre dispositivos que usan el mismo servidor. Se conservan tras reiniciar. Usa una sola instancia del servidor. Haz copia de esa carpeta antes y después del evento. Las solicitudes de cobro tienen un identificador para evitar duplicar la venta al reintentar una petición perdida.

El resumen es acumulado: para empezar otro evento, detén la aplicación y mueve `sales.json` a una carpeta de copias. No incluye fondo de caja, pagos con tarjeta ni retiradas de dinero. Sin impresora ni facturación.

## Docker / alojamiento

```sh
docker build -t barra-pueblo .
docker run -p 5080:8080 -v barra-datos:/data barra-pueblo
```

Para un enlace de Internet necesitas un alojamiento que ejecute ASP.NET Core o Docker y almacenamiento persistente en `/data`. GitHub almacena el código; GitHub Pages no ejecuta este backend C#.

Define `BAR_ACCESS_CODE` en el alojamiento para proteger toda la aplicación con una clave compartida. El usuario es `barra`; la clave se configura únicamente como secreto del servidor y no debe guardarse en GitHub. Sin esa variable, el acceso queda abierto para facilitar el uso en una red local de confianza. El endpoint `/healthz` permanece público para las comprobaciones del alojamiento.

## Precios

Agua 1 €; refresco 2 €; cachi Kalimotxo/cerveza 5 €; cachi cubata 11 €; pinta cerveza/Kali 3 €; cubata 6 €; caña/botellín 1,50 €; Radler/0,0 1,50 €; chupito 1,50 €; Jäger 2 €; vino 1,50 €; vermut 2 €; mosto 1,50 €; zumo/batido 2 €.

Edita el catálogo en `Program.cs`; los precios se expresan en céntimos. El servidor comprueba las cantidades y calcula el total sin confiar en precios enviados por el navegador.
