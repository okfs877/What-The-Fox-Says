using Microsoft.EntityFrameworkCore;
 
namespace multilib.Models
{
    public class LibContext : DbContext
    {
        // base() calls the parent class' constructor passing the "options" parameter along
        public LibContext(DbContextOptions<LibContext> options) : base(options) { }
        public DbSet<ModelUser> Users {get;set;}
        public DbSet<ModelLib> Libs {get;set;}
    }
}