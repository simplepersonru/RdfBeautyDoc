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
        public string Name { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
	}

    public class LayoutViewModel
    {
        public string Title { get; set; } = "RDFS Documentation";
        public List<BreadcrumbItem> Breadcrumbs { get; set; } = new();
        public string CurrentPage { get; set; } = "home";
        public int PropertyCount { get; set; }
        public int EnitityCount { get; set; }
    }

    public class IndexViewModel : LayoutViewModel
    {
        public List<Class> ExampleClasses { get; set; } = new();
        public string Description { get; set; } = string.Empty;
        public List<Property> ExampleProperties { get; set; } = new();
		public int ClassCount { get; set; }
		public int EnumCount { get; set; }
		public int PrimitiveCount { get; set; }
		public int DataTypeCount { get; set; }
	}

    public class ClassViewModel : LayoutViewModel
    {
        public Class Class { get; set; } = new();
        public List<Property> Properties { get; set; } = new();
        public List<Class> ChildClasses { get; set; } = new();
		public List<Property> AllProperties { get; set; } = new();
	}

    public class PropertyViewModel : LayoutViewModel
    {
        public Property Property { get; set; }
    }

    // Для списков
    public class ClassListViewModel : LayoutViewModel
    {
        public List<Class> Classes { get; set; } = new();
        public Stereotype Stereotype { get; set; }
    }

    public class PropertyListViewModel : LayoutViewModel
    {
        public List<Property> Properties { get; set; } = new();
    }

}
