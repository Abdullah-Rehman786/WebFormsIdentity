using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using WebFormsIdentity.Models;

namespace WebFormsIdentity.Roles
{
    public class RoleActions
    {
        internal void AddUserAndRole()
        {
            // Access the application context and create result variables.
            Models.ApplicationDbContext context = new ApplicationDbContext();
            IdentityResult IdRoleResult;
            IdentityResult IdUserResult;

            // Create a RoleStore object by using the ApplicationDbContext object. 
            // The RoleStore is only allowed to contain IdentityRole objects.
            var roleStore = new RoleStore<IdentityRole>(context);

            // Create a RoleManager object that is only allowed to contain IdentityRole objects.
            // When creating the RoleManager object, you pass in (as a parameter) a new RoleStore object. 
            var roleMgr = new RoleManager<IdentityRole>(roleStore);

            // Then, you create the "masterUser" role if it doesn't already exist.
            if (!roleMgr.RoleExists("masterUser"))
            {
                IdRoleResult = roleMgr.Create(new IdentityRole { Name = "masterUser" });
            }

            // Create a UserManager object based on the UserStore object and the ApplicationDbContext  
            // object. Note that you can create new objects and use them as parameters in
            // a single line of code, rather than using multiple lines of code, as you did
            // for the RoleManager object.
            var userMgr = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            var appUser = new ApplicationUser
            {
                UserName = "masterUser@izazsolutions.com",
                Email = "masterUser@izazsolutions.com"
            };
            if (userMgr.FindByEmail("masterUser@izazsolutions.com") == null)
            {
                IdUserResult = userMgr.Create(appUser, ServiceUtils.getSettingsFromFile("masterPassword"));
            }

            // If the new "masterUser" user was successfully created, 
            // add the "masterUser" user to the "masterUser" role. 
            if (!userMgr.IsInRole(userMgr.FindByEmail("masterUser@izazsolutions.com").Id, "masterUser"))
            {
                IdUserResult = userMgr.AddToRole(userMgr.FindByEmail("masterUser@izazsolutions.com").Id, "masterUser");
            }
        }
    }
}