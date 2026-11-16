using Microsoft.EntityFrameworkCore;

namespace Web_API_for_Contacts_2._0.Data.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ContactsDbContext _context;
        public UnitOfWork(ContactsDbContext context) => _context = context;
        public Task<int> SaveChangesAsync(CancellationToken ct = default) => _context.SaveChangesAsync(ct);
    }
}
