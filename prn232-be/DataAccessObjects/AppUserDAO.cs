using System.Collections.Generic;
using System.Linq;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects
{
    public class AppUserDAO
    {
        private static AppUserDAO instance = null;
        private static readonly object instanceLock = new object();

        private AppUserDAO() { }

        public static AppUserDAO Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        instance = new AppUserDAO();
                    }
                    return instance;
                }
            }
        }

        public AppUser GetUserByEmail(string email)
        {
            using var context = new CarShowroomContext();
            return context.AppUsers.Include(u => u.Role).SingleOrDefault(u => u.Email == email);
        }

        public AppUser GetUserById(int id)
        {
            using var context = new CarShowroomContext();
            return context.AppUsers.Include(u => u.Role).SingleOrDefault(u => u.UserId == id);
        }

        public void AddUser(AppUser user)
        {
            using var context = new CarShowroomContext();
            context.AppUsers.Add(user);
            context.SaveChanges();
        }

        public void UpdateUser(AppUser user)
        {
            using var context = new CarShowroomContext();
            context.Entry(user).State = EntityState.Modified;
            context.SaveChanges();
        }
    }
}
