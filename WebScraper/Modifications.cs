using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebScraper
{
    public class Modifications
    {
        public void InsertCategories(List<Category> categories)
        {
            using (var db = new AudioContext())
            {
                db.Database.OpenConnection();
                try
                {
                    db.Categories.ExecuteDelete();
                    db.Products.ExecuteDelete();
                    db.Companies.ExecuteDelete();

                    db.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Categories ON"); //allow insert primary key to db
                    db.Categories.AddRange(categories);
                    db.SaveChanges(true);
                    db.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Categories OFF");
                }
                finally
                {
                    db.Database.CloseConnection();
                }
            }
        
        }
    }
}
