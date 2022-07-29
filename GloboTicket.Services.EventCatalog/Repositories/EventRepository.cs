using GloboTicket.Services.EventCatalog.DbContexts;
using GloboTicket.Services.EventCatalog.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GloboTicket.Services.EventCatalog.Repositories
{
    public class EventRepository: IEventRepository
    {
        private readonly EventCatalogCosmosDbContext _eventCatalogDbContext;

        public EventRepository(EventCatalogCosmosDbContext eventCatalogDbContext)
        {
            _eventCatalogDbContext = eventCatalogDbContext;
        }

        public async Task<IEnumerable<Event>> GetEvents(Guid categoryId)
        {
            return await _eventCatalogDbContext.Events
                //.Include(x => x.Category)
                .Where(x => (Guid.Parse(x.CategoryId) == categoryId || categoryId == Guid.Empty)).ToListAsync();
        }

        public async Task<Event> GetEventById(Guid eventId)
        {
            return await _eventCatalogDbContext.Events.Where(x => x.EventId == eventId).FirstOrDefaultAsync();
        }

        public async Task<bool> SaveChanges()
        {
            return (await _eventCatalogDbContext.SaveChangesAsync() > 0);
        }
    }
}
