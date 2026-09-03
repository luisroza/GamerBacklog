using GamerBacklog.Services;
using Microsoft.AspNetCore.Mvc;

namespace GamerBacklog.Controllers;

/// <summary>
/// Gera capas e avatares SVG determinísticos localmente (initials + cor por hash).
/// Garante que nenhuma imagem de capa quebre, mesmo sem internet.
/// </summary>
public class CoversController : Controller
{
    [HttpGet("covers/{**slug}")]
    public IActionResult Cover(string? slug)
    {
        Response.Headers["Cache-Control"] = "public,max-age=86400";
        return Content(CoverSvg.Game(slug ?? ""), "image/svg+xml");
    }

    [HttpGet("avatar/{username}")]
    public IActionResult Avatar(string? username)
    {
        Response.Headers["Cache-Control"] = "public,max-age=86400";
        return Content(CoverSvg.Avatar(username ?? "?"), "image/svg+xml");
    }
}
