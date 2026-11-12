using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using DTOs;
using Entidades;

namespace Negocio;

public class NegocioMail
{
    private const string UrlResend = "https://api.resend.com/emails";
    private static readonly CultureInfo Pesos = CultureInfo.GetCultureInfo("es-AR");

    private const string Fondo = "#080B10";
    private const string Tarjeta = "#10161E";
    private const string Borde = "#25333F";
    private const string Texto = "#F5F7F8";
    private const string TextoSuave = "#9CAAB7";
    private const string Cian = "#00E5FF";

    private static readonly Dictionary<string, (string color, string fondo, string borde)> ColoresEstado = new()
    {
        ["PENDIENTE"] = ("#F6C453", "#1A160B", "#65521F"),
        ["REALIZADO"] = ("#22C55E", "#0B1A11", "#1E5634"),
        ["CANCELADO"] = ("#FF4D4D", "#1E0E0E", "#5C2323")
    };

    private readonly HttpClient http;
    private readonly ILogger<NegocioMail> logger;
    private readonly string? apiKey;
    private readonly string? emisor;
    private readonly string? responderA;
    private readonly string? urlBase;
    private readonly string? whatsapp;

    public NegocioMail(HttpClient http, IConfiguration configuracion, ILogger<NegocioMail> logger)
    {
        this.http = http;
        this.logger = logger;
        apiKey = configuracion["Resend:ApiKey"];
        emisor = configuracion["Resend:EmailEmisor"];
        responderA = configuracion["Resend:ResponderA"];
        urlBase = configuracion["Frontend:UrlBase"]?.TrimEnd('/');
        whatsapp = configuracion["Contacto:WhatsApp"];
    }

    private bool EstaConfigurado => !string.IsNullOrWhiteSpace(apiKey) && !string.IsNullOrWhiteSpace(emisor);

    public async Task<bool> enviarEstadoPedido(Clientes cliente, PedidoResponse pedido, NegocioAvisoPedido aviso)
    {
        if (string.IsNullOrWhiteSpace(cliente.Correo))
        {
            return false;
        }

        var (titulo, asunto, introduccion) = aviso == NegocioAvisoPedido.Nuevo
            ? ("¡Tu pedido está registrado!",
               $"Tu pedido #{pedido.numero} está registrado | ZUNOX 3D",
               "Recibimos tu solicitud en ZUNOX 3D. Tu pedido quedó registrado y ya está en proceso.")
            : ("Tu pedido cambió de estado",
               $"Tu pedido #{pedido.numero} está {pedido.estado?.ToLowerInvariant()} | ZUNOX 3D",
               "Actualizamos el estado de tu pedido en ZUNOX 3D.");

        return await enviar(cliente.Correo, asunto, Plantilla(asunto, Cuerpo(cliente, pedido, titulo, introduccion)));
    }

    private string Cuerpo(Clientes cliente, PedidoResponse pedido, string titulo, string introduccion)
    {
        var (color, fondo, borde) = ColoresEstado.GetValueOrDefault(pedido.codigoestado ?? "", ColoresEstado["PENDIENTE"]);

        var productos = new StringBuilder();
        foreach (var item in pedido.detalle)
        {
            productos.Append($"""
                <tr>
                  <td style="padding:10px 0;border-top:1px solid {Borde};font-size:15px;color:{Texto}">{Html(item.nombre)}<br><span style="font-size:13px;color:{TextoSuave}">{item.cantidad} x {item.precio.ToString("C", Pesos)}</span></td>
                  <td align="right" style="padding:10px 0;border-top:1px solid {Borde};font-size:15px;color:{Texto};white-space:nowrap">{item.importe.ToString("C", Pesos)}</td>
                </tr>
                """);
        }

        var observacion = string.IsNullOrWhiteSpace(pedido.seguimiento)
            ? ""
            : $"""
                <tr>
                  <td colspan="2" style="padding:0 16px 16px">
                    <p style="margin:0 0 4px;font-size:13px;color:{TextoSuave}">Observación</p>
                    <p style="margin:0;font-size:15px;line-height:1.5;color:{Texto};overflow-wrap:anywhere;word-break:break-word">{HtmlConLinks(pedido.seguimiento)}</p>
                  </td>
                </tr>
                """;

        var boton = string.IsNullOrWhiteSpace(urlBase)
            ? ""
            : $"""
                <p style="margin:24px 0 12px;font-size:15px;line-height:1.5;color:{Texto}">Podés ver el detalle y el estado de tu pedido cuando quieras.</p>
                <a href="{Html($"{urlBase}/seguimiento?id={pedido.idpedido}")}" style="display:block;background:{Cian};color:{Fondo};text-decoration:none;text-align:center;font-weight:bold;font-size:16px;padding:16px;border-radius:12px">Seguir mi pedido</a>
                """;

        return $"""
            <h1 style="margin:0 0 20px;font-size:28px;line-height:1.25;font-weight:bold;color:{Texto}">{Html(titulo)}</h1>

            <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="background:{fondo};border:1px solid {borde};border-radius:14px">
              <tr>
                <td style="padding:16px">
                  <p style="margin:0 0 2px;font-size:13px;color:{TextoSuave}">Estado actual</p>
                  <p style="margin:0;font-size:22px;font-weight:bold;color:{color}">{Html(pedido.estado)}</p>
                </td>
                <td align="right" valign="top" style="padding:16px">
                  <p style="margin:0 0 2px;font-size:13px;color:{TextoSuave}">Pedido</p>
                  <p style="margin:0;font-size:22px;font-weight:bold;color:{Cian}">#{pedido.numero}</p>
                </td>
              </tr>
              {observacion}
            </table>

            <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="margin-top:16px;background:{Tarjeta};border:1px solid {Borde};border-radius:14px">
              <tr>
                <td style="padding:20px 16px">
                  <p style="margin:0 0 6px;font-size:16px;font-weight:bold;color:{Texto}">¡Hola {Html(cliente.Nombre)}!</p>
                  <p style="margin:0 0 16px;font-size:15px;line-height:1.5;color:{Texto}">{Html(introduccion)}</p>
                  <table role="presentation" width="100%" cellpadding="0" cellspacing="0">
                    {productos}
                    <tr>
                      <td style="padding:12px 0 0;border-top:1px solid {Borde};font-size:15px;color:{TextoSuave}">Total estimado</td>
                      <td align="right" style="padding:12px 0 0;border-top:1px solid {Borde};font-size:20px;font-weight:bold;color:{Cian};white-space:nowrap">{pedido.total.ToString("C", Pesos)}</td>
                    </tr>
                  </table>
                  {boton}
                </td>
              </tr>
            </table>
            """;
    }

    private async Task<bool> enviar(String para, String asunto, String html)
    {
        if (!EstaConfigurado)
        {
            logger.LogInformation("No se envió el mail \"{Asunto}\": falta configurar Resend:ApiKey y Resend:EmailEmisor.", asunto);
            return false;
        }

        try
        {
            object datos = string.IsNullOrWhiteSpace(responderA)
                ? new { from = Remitente(), to = new[] { para }, subject = asunto, html }
                : new { from = Remitente(), to = new[] { para }, subject = asunto, html, reply_to = responderA };

            using var mensaje = new HttpRequestMessage(HttpMethod.Post, UrlResend) { Content = JsonContent.Create(datos) };
            mensaje.Headers.Authorization = new("Bearer", apiKey);

            using var respuesta = await http.SendAsync(mensaje);
            if (!respuesta.IsSuccessStatusCode)
            {
                logger.LogWarning("Resend no envió el mail \"{Asunto}\" ({Estado}): {Detalle}", asunto, (int)respuesta.StatusCode, await respuesta.Content.ReadAsStringAsync());
                return false;
            }

            return true;
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "No se pudo conectar con Resend para enviar el mail \"{Asunto}\".", asunto);
            return false;
        }
    }

    private string Remitente() => emisor!.Contains('<') ? emisor : $"ZUNOX 3D <{emisor}>";

    private static string Html(string? texto) => WebUtility.HtmlEncode(texto ?? "");

    private static string HtmlConLinks(string texto) =>
        System.Text.RegularExpressions.Regex.Replace(Html(texto), @"https?://[^\s<]+",
            enlace => $"""<a href="{enlace.Value}" style="color:{Cian}">{enlace.Value}</a>""");
    private string Plantilla(string vistaPrevia, string cuerpo)
    {
        var contacto = string.IsNullOrWhiteSpace(whatsapp)
            ? ""
            : $"""<br><a href="https://wa.me/{Html(whatsapp)}" style="color:{Cian};text-decoration:none">Escribinos por WhatsApp</a>""";

        return $"""
            <!DOCTYPE html>
            <html lang="es">
            <head><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1"><meta name="color-scheme" content="dark"></head>
            <body style="margin:0;padding:0;background:{Fondo}">
              <div style="display:none;max-height:0;overflow:hidden">{Html(vistaPrevia)}</div>
              <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="background:{Fondo}">
                <tr>
                  <td align="center" style="padding:24px 12px">
                    <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="max-width:480px;font-family:Arial,Helvetica,sans-serif">
                      <tr>
                        <td style="background:{Tarjeta};border:1px solid {Borde};border-radius:14px;padding:16px">
                          <table role="presentation" width="100%" cellpadding="0" cellspacing="0">
                            <tr>
                              <td style="font-size:18px;font-weight:bold;color:{Cian}">ZUNOX 3D</td>
                              <td align="right" style="font-size:13px;color:{TextoSuave}">Pedidos</td>
                            </tr>
                          </table>
                        </td>
                      </tr>
                      <tr>
                        <td style="padding:24px 0">
                          {cuerpo}
                        </td>
                      </tr>
                      <tr>
                        <td style="padding:0 4px 24px;font-size:13px;line-height:1.6;color:{TextoSuave}">
                          ¿Tenés dudas? Respondé este mail o escribinos.{contacto}
                        </td>
                      </tr>
                    </table>
                  </td>
                </tr>
              </table>
            </body>
            </html>
            """;
    }
}
