﻿using System.Web;
using System.Web.Mvc;

namespace CP.Azure.Samples.MultiComponentRole.Frontend
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}