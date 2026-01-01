using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RdfsBeautyDoc
{
	public class Identified
	{
		public string Id { get; set; } = string.Empty;
	}
	public class Property : Identified
	{
		public string Label { get; set; } = string.Empty;
		/// <summary>
		/// Класс, которому принадлежит атрибут
		/// </summary>
		required public Class Domain { get; set; }
		public string Multiplicity { get; set; } = string.Empty;
		public string InverseRoleName { get; set; } = string.Empty;
		/// <summary>
		/// Тип атрибута (может быть примитивным типа Float, может быть именем класса, если ссылка на класс)
		/// </summary>
		public string Range { get; set; } = string.Empty;
		public Class? RangeClass { get; set; }
	}

	public class Description : Identified
	{
		public string Label { get; set; } = string.Empty;
	}


	public enum Stereotype
	{
		Class,
		Enum, 
		DataType,
		Primitive
	}

	public class Class : Identified
	{

		public string Label { get; set; } = string.Empty;
		public string Comment { get; set; } = string.Empty;
		public string SvgDiagram { get; set; } = string.Empty;
		public Class? SubClass { get; set; }

		public Stereotype Stereotype { get; set; } = Stereotype.Class;

		public Dictionary<string, Property> Properties { get; set; } = new Dictionary<string, Property>();
		/// <summary>
		/// Описания элементов перечисления (для Stereotype.Enum)
		/// </summary>
		public Dictionary<string, Description> Descriptions { get; set; } = new Dictionary<string, Description>();
	}

    // Models/ViewModels.cs
    public class LayoutViewModel
    {
        public string Title { get; set; } = "RDFS Documentation";
        public DateTime Generated { get; set; } = DateTime.Now;
        public List<BreadcrumbItem> Breadcrumbs { get; set; } = new();
        public string CurrentPage { get; set; } = "home";
        public int ClassCount { get; set; }
        public int PropertyCount { get; set; }
    }

    public class IndexViewModel : LayoutViewModel
	{
		public List<ClassViewModel> Classes { get; set; } = new();
		public List<Property> Properties { get; set; } = new();
	}

    public class ClassViewModel : LayoutViewModel
    {
        public Class Class { get; set; }
        public List<Property> Properties { get; set; } = new();
        public List<Class> AllClasses { get; set; } = new();
        public List<Class> ParentClasses { get; set; } = new();
        public List<Class> ChildClasses { get; set; } = new();
        public List<Class> UsedInClasses { get; set; } = new(); // Где используется этот класс как range
    }

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
}
