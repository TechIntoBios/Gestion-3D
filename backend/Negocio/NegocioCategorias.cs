using Datos;
using DTOs;
using Entidades;
using Npgsql;

namespace Negocio;

public class NegocioCategorias
{
    private const string MensajeDuplicada = "Ya existe una categoría con ese nombre.";

    private readonly DaoCategorias daoCategorias;

    public NegocioCategorias(DaoCategorias dao) => daoCategorias = dao;

    public async Task AgregarCategorias(CategoriaRequest request)
    {
        var categoria = new Categorias
        {
            NOMBRE = NegocioValidaciones.textoRequerido(request.nombre, "El nombre de la categoría", 100)
        };

        if (await daoCategorias.ExisteCategoria(categoria))
        {
            throw new NegocioConflictoException(MensajeDuplicada);
        }

        try
        {
            await daoCategorias.AgregarCategorias(categoria);
        }
        catch (PostgresException e) when (e.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new NegocioConflictoException(MensajeDuplicada);
        }
    }

    public async Task<bool> actualizar(int idCategoria, CategoriaRequest request)
    {
        var categoria = new Categorias
        {
            IDCATEGORIA = idCategoria,
            NOMBRE = NegocioValidaciones.textoRequerido(request.nombre, "El nombre de la categoría", 100),
            ACTIVO = request.activo
        };

        return await daoCategorias.actualizar(categoria) switch
        {
            "OK" => true,
            "NO_EXISTE" => false,
            "DUPLICADO" => throw new NegocioConflictoException(MensajeDuplicada),
            var otro => throw new InvalidOperationException($"fn_ActualizarCategoria devolvió un resultado inesperado: {otro}.")
        };
    }

    public async Task<List<Categorias>> ListarCategorias(bool todos = false)
    {
        return await daoCategorias.ListarTodasLasCategorias(soloActivas: !todos);
    }
}
