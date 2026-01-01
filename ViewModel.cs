using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RdfsBeautyDoc
{
    // Дополнительные модели для навигации
    public class BreadcrumbItem
    {
        public string Name { get; set; }
        public string Url { get; set; }
    }

    public class NavigationViewModel
    {
        public List<BreadcrumbItem> Breadcrumbs { get; set; } = new();
        public List<Class> RecentClasses { get; set; } = new();
    }

    public class LayoutViewModel
    {
        public string Title { get; set; } = "RDFS Documentation";
        public DateTime Generated { get; set; } = DateTime.Now;
        public List<BreadcrumbItem> Breadcrumbs { get; set; } = new();
        public string CurrentPage { get; set; } = "home";
        public int ClassCount { get; set; }
        public int PropertyCount { get; set; }
        public int EnumCount { get; set; }
        public int PrimitiveCount { get; set; }
        public int DataTypeCount { get; set; }
        public List<Class> AllClasses { get; set; } = new();
        public List<Property> AllProperties { get; set; } = new();
        public List<Class> AllEnums { get; set; } = new();
        public List<Class> AllPrimitives { get; set; } = new();
        public List<Class> AllDataTypes { get; set; } = new();
    }

    public class IndexViewModel : LayoutViewModel
    {
        public List<Class> RecentClasses { get; set; } = new();
        public List<Property> RecentProperties { get; set; } = new();
    }

    public class ClassViewModel : LayoutViewModel
    {
        public Class Class { get; set; }
        public List<Property> Properties { get; set; } = new();
        public List<Class> ParentClasses { get; set; } = new();
        public List<Class> ChildClasses { get; set; } = new();
        public List<Class> UsedInClasses { get; set; } = new();
    }

    public class PropertyViewModel : LayoutViewModel
    {
        public Property Property { get; set; }
        public Class DomainClass { get; set; }
        public Class RangeClass { get; set; }
    }

    // Для списков
    public class ClassListViewModel : LayoutViewModel
    {
        public List<Class> Classes { get; set; } = new();
    }

    public class PropertyListViewModel : LayoutViewModel
    {
        public List<Property> Properties { get; set; } = new();
    }

    public class EnumListViewModel : LayoutViewModel
    {
        public List<Class> Enums { get; set; } = new();
    }

    public class PrimitiveListViewModel : LayoutViewModel
    {
        public List<Class> Primitives { get; set; } = new();
    }

    public class DataTypeListViewModel : LayoutViewModel
    {
        public List<Class> DataTypes { get; set; } = new();
    }
}
