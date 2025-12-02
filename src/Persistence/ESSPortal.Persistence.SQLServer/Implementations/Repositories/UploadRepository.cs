using ESSPortal.Domain.Entities;
using ESSPortal.Domain.IRepositories;
using ESSPortal.Persistence.SQLServer.DataContext;

namespace ESSPortal.Persistence.SQLServer.Implementations.Repositories;

internal sealed class UploadRepository : BaseRepository<Upload>, IUploadRepository
{

    public UploadRepository(DBContext context) : base(context)
    {

    }

    
}

