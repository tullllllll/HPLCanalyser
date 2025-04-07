using System;
using System.Linq;
using HPLC.Data;
using Microsoft.EntityFrameworkCore;

namespace HPLC.Services;

public class SimpleKeyCRUDService<T> (HPLCDbContext context)
    where T : class
{
    public IQueryable<T> Get()
    {
        return context.Set<T>();
    }

    public T? Get(int id)
    {
        return context.Set<T>().Find(id);
    }
    
    public IQueryable<T> GetWithChildren()
    {
        IQueryable<T> query = context.Set<T>();

        var navigations = context.Model.FindEntityType(typeof(T))?
            .GetNavigations();

        if (navigations != null)
        {
            foreach (var navigation in navigations)
            {
                query = query.Include(navigation.Name);
            }
        }

        return query;
    }
    
    public T? GetWithChildren(int id)
    {
        return GetWithChildren().FirstOrDefault(e =>
            EF.Property<int>(e, "ID") == id);
    }
    
    public void Add(T entity)
    {
        context.Add(entity);
        context.SaveChanges();
    }

    public void Delete(T entity)
    {
        context.Remove(entity);
        context.SaveChanges();
    }

    public void Delete(int id)
    {
        var entity = context.Set<T>().Find(id);
        if (entity == null) return;
        
        context.Remove(entity);
        context.SaveChanges();
    }
}