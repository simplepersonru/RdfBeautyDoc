namespace RdfsBeautyDoc
{
	public class Identified
	{
		public string Id { get; set; } = string.Empty;
	}
	public class Property : Identified
	{
		public string FieldName { get; set; } = string.Empty;
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
		public Class Range { get; set; }
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
		Primitive,
		All
	}

	public class Class : Identified
	{

		public string Label { get; set; } = string.Empty;
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
