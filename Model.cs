namespace RdfsBeautyDoc
{
	public abstract class Base
	{
		public string Label { get; set; } = string.Empty;
		public string Namespace { get; set; } = string.Empty;
		public string Name { get; set; } = string.Empty;
		public virtual string Id => $"{Namespace}:{Name}";
		
	}
	public class Property : Base
	{
		/// <summary>
		/// Класс, которому принадлежит атрибут
		/// </summary>
		required public Class Domain { get; set; }
		public string Multiplicity { get; set; } = string.Empty;
		public string InverseRoleName { get; set; } = string.Empty;
		/// <summary>
		/// Тип атрибута (может быть примитивным типа Float, может быть именем класса, если ссылка на класс)
		/// </summary>
		public Class Range { get; set; }

		public override string Id => $"{Namespace}:{Domain.Name}.{Name}";
		public string PropertyId => $"{Namespace}:{Name}";
	}

	public class Description : Base
	{
		required public Class Domain { get; set; }

		public override string Id => $"{Namespace}:{Domain.Name}.{Name}";
	}


	public enum Stereotype
	{
		Class,
		Enum, 
		DataType,
		Primitive,
		All
	}

	public class Class : Base
	{
		public string Comment { get; set; } = string.Empty;
		public string SvgDiagram { get; set; } = string.Empty;
		public Class? SubClass { get; set; }

		public string StereoPath => Stereotype switch
		{
			Stereotype.DataType => "datatypes",
			Stereotype.Enum => "enums",
			Stereotype.Class => "classes",
			Stereotype.Primitive => "primitives",
			Stereotype.All => "entities",
		};

		public static string BadgeClassStatic(Stereotype stereotype) => stereotype switch
		{
            Stereotype.Class => "badge-class",
            Stereotype.Enum => "badge-enum",
            Stereotype.Primitive => "badge-primitive",
            Stereotype.DataType => "badge-datatype",
            _ => "badge-secondary"
        };

		public string BadgeClass => BadgeClassStatic(Stereotype);


		public Stereotype Stereotype { get; set; } = Stereotype.Class;

		public Dictionary<string, Property> Properties { get; set; } = new Dictionary<string, Property>();
		/// <summary>
		/// Описания элементов перечисления (для Stereotype.Enum)
		/// </summary>
		public Dictionary<string, Description> Descriptions { get; set; } = new Dictionary<string, Description>();
	}
}
