using RazorLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RdfsBeautyDoc
{
	internal class SiteGenerator
	{
		private readonly RazorLightEngine _engine;
		private readonly Dictionary<string, Class> _data;
		private readonly List<Class> _classes = new();
		private readonly List<Property> _properties = new();
		private readonly string _templatePath;
		private readonly string _outputPath;

		public SiteGenerator(Dictionary<string, Class> data, string templatePath, string outputPath)
		{
			_data = data;
			_templatePath = templatePath;
			_outputPath = outputPath;

			// Настраиваем RazorLight для работы с файлами
			_engine = new RazorLightEngineBuilder()
				.UseFileSystemProject(Path.Combine(Directory.GetCurrentDirectory(), "templates")) 
				.UseMemoryCachingProvider()
				.Build();

			_classes = _data.Values
				.Where(x => x.Stereotype == Stereotype.Class)
				.ToList();

			foreach (var cls in _classes)
				foreach (var prop in cls.Properties)
					_properties.Add(prop.Value);

			// Создаем структуру папок
			Directory.CreateDirectory(outputPath);
			Directory.CreateDirectory(Path.Combine(outputPath, "classes"));
			Directory.CreateDirectory(Path.Combine(outputPath, "properties"));
			Directory.CreateDirectory(Path.Combine(outputPath, "enums"));
			Directory.CreateDirectory(Path.Combine(outputPath, "assets"));
			Directory.CreateDirectory(Path.Combine(outputPath, "assets", "css"));
			Directory.CreateDirectory(Path.Combine(outputPath, "assets", "js"));
		}

		public async Task GenerateAsync()
		{
			// Копируем статические файлы
			await CopyAssetsAsync();

			// Генерируем данные для поиска
			await GenerateSearchIndexAsync();

			// Главная страница
			await GenerateIndexAsync();

			// Страница со списком всех классов
			//await GenerateClassListAsync();

			// Страницы отдельных классов
			foreach (var cls in _classes)
			{
				await GenerateClassPageAsync(cls);
			}

			// Страницы свойств
			foreach (var prop in _properties)
			{
				await GeneratePropertyPageAsync(prop);
			}
		}

		private async Task CopyAssetsAsync()
		{
			// Создаем минимальный CSS если нет
			string defaultCss = @"/* site.css */
                body { font-family: -apple-system, BlinkMacSystemFont, sans-serif; }
                .class-card { transition: all 0.2s; }
                .class-card:hover { box-shadow: 0 4px 8px rgba(0,0,0,0.1); }
                .diagram-container { background: #f8f9fa; padding: 1rem; }
                .breadcrumb { background: transparent; padding: 0; }";
			await File.WriteAllTextAsync(
				Path.Combine(_outputPath, "assets", "css", "site.css"),
				defaultCss);
		}

		private async Task GenerateSearchIndexAsync()
		{
			var searchData = new
			{
				Classes = _classes.Select(c => new
				{
					id = c.Id,
					name = c.Label,
					url = $"/classes/{c.Id}.html",
					description = c.Comment
				}),
				Properties = _properties.Select(p => new
				{
					id = p.Id,
					name = p.Label,
					url = $"/properties/{p.Id}.html",
					description = ""
				})
			};

			string json = JsonSerializer.Serialize(searchData, new JsonSerializerOptions
			{
				WriteIndented = true,
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			});

			await WriteOutputAsync("assets/search-index.json", json);
		}

		private async Task GenerateIndexAsync()
		{
			var model = new IndexViewModel
			{
				Title = "RDFS Documentation",
				Classes = _classes
					.OrderBy(c => c.Id)
					.Select(c => new ClassViewModel { Class = c })
					.Take(20)
					.ToList(),
				Properties = _properties
					.OrderBy(p => p.Id)
					.Take(20)
					.ToList()
			};

			string html = await _engine.CompileRenderAsync("Index.cshtml", model);
			await WriteOutputAsync("index.html", html);
		}

		private async Task GenerateClassListAsync()
		{
			var model = new
			{
				Title = "All Classes",
				Classes = _classes
					.OrderBy(c => c.Id)
					.ToList(),
				Breadcrumbs = new[]
				{
				new { Name = "Home", Url = "/index.html" },
				new { Name = "Classes", Url = "/classes/index.html" }
			}
			};

			string html = await _engine.CompileRenderAsync("ClassList.cshtml", model);
			await WriteOutputAsync("classes/index.html", html);
		}

		private async Task GenerateClassPageAsync(Class cls)
		{
			var model = new ClassViewModel
			{
				Title = cls.Label,
				Class = cls,
				Properties = _properties,

				UsedInClasses = _properties
					.Where(p => p.Range == cls.Id)
					.Select(p => p.Domain)
					.Where(c => c != null)
					.DistinctBy(c => c.Id)
					.ToList(),

                ClassCount = _classes.Count,
                PropertyCount = _properties.Count,
				AllClasses = _classes,
				ChildClasses = _classes
					.Where(c => c.SubClass?.Id == cls.Id)
					.ToList(),
                CurrentPage = "classes",


                //UsedIn = usedIn,
                //ParentClass = cls.SubClass,
                //ChildClasses = _classes
                //	.Where(c => c.SubClass != null 
                //				&& c.SubClass.Id == cls.Id)
                //	.ToList(),
                //Breadcrumbs = new[]
                //{
                //new { Name = "Home", Url = "/index.html" },
                //new { Name = "Classes", Url = "/classes/index.html" },
                //new { Name = cls.Label, Url = $"/classes/{cls.Id}.html" }
            };

			string html = await _engine.CompileRenderAsync("Class.cshtml", model);
			await WriteOutputAsync($"classes/{cls.Id}.html", html);
		}

		private async Task GeneratePropertyPageAsync(Property prop)
		{
			var rangeClass = _data.TryGetValue(prop.Range, out var rc) ? rc : null;
			if (rangeClass == null)
				return;

			var model = new
			{
				Title = prop.Label,
				Property = prop,
				DomainClass = prop.Domain,
				RangeClass = rangeClass,
				Breadcrumbs = new[]
				{
				new { Name = "Home", Url = "/index.html" },
				new { Name = "Properties", Url = "/properties/index.html" },
				new { Name = prop.Label, Url = $"/properties/{prop.Id}.html" }
			}
			};

			string html = await _engine.CompileRenderAsync("Property.cshtml", model);
			await WriteOutputAsync($"properties/{prop.Id}.html", html);
		}

		private async Task WriteOutputAsync(string relativePath, string content)
		{
			string fullPath = Path.Combine(_outputPath, relativePath);
			string? directory = Path.GetDirectoryName(fullPath);

			if (!string.IsNullOrEmpty(directory))
				Directory.CreateDirectory(directory);

			await File.WriteAllTextAsync(fullPath, content, Encoding.UTF8);
		}
	}
}
