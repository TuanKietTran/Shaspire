using Shaspire.ServiceDefaults.Models;
using Shaspire.ServiceDefaults.Repositories;

namespace Shaspire.ServiceDefaults.I18n;
// ReSharper disable once InconsistentNaming
internal class I18nRepository(ApplicationDbContext dbContext)
: Repository<ApplicationDbContext, EntityTranslation>(dbContext), II18nRepository;

internal class CultureRepository(ApplicationDbContext dbContext)
: Repository<ApplicationDbContext, Culture>(dbContext), ICultureRepository;


// ReSharper disable once InconsistentNaming
public interface II18nRepository : IRepository<EntityTranslation> { }

public interface ICultureRepository : IRepository<Culture> { }
