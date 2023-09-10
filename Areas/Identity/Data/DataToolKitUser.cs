using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace DataToolKit.Areas.Identity.Data;

// Add profile data for application users by adding properties to the DataToolKitUser class
public class DataToolKitUser : IdentityUser
{
    public string firstName { set; get; }
    public string lastName { set; get; }

    public string customerName { set; get; }
}

