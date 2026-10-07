using Agro.Application.Common.Interfaces;

namespace Agro.Infrastructure.Persistence;

/// <summary>
/// Hace que AgroDbContext cumpla el contrato IAgroDbContext.
/// Está en un archivo aparte (clase partial) para que, si se vuelve a generar
/// el scaffold con --force, este código no se borre.
/// </summary>
public partial class AgroDbContext : IAgroDbContext
{
    public Task<bool> PuedeConectarAsync(CancellationToken cancellationToken = default)
        => Database.CanConnectAsync(cancellationToken);
}
