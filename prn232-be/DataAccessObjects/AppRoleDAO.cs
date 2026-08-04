using System.Collections.Generic;
using System.Linq;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects
{
    public class AppRoleDAO
    {
        private static AppRoleDAO instance = null;
        private static readonly object instanceLock = new object();

        private AppRoleDAO() { }

        public static AppRoleDAO Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        instance = new AppRoleDAO();
                    }
                    return instance;
                }
            }
        }

        public AppRole GetRoleById(int id)
        {
            using var context = new CarShowroomContext();
            return context.AppRoles.SingleOrDefault(r => r.RoleId == id);
        }

        public AppRole GetRoleByName(string name)
        {
            using var context = new CarShowroomContext();
            return context.AppRoles.SingleOrDefault(r => r.RoleName == name);
        }
    }
}
