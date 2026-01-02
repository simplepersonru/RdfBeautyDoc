using RazorLight;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Collections.Specialized.BitVector32;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RdfsBeautyDoc
{
    internal class SiteGenerator
    {
        private readonly RazorLightEngine _engine;
        private readonly Dictionary<string, Class> _data;
        private readonly List<Class> _classes;
        private readonly List<Class> _enums;
        private readonly List<Class> _primitives;
        private readonly List<Class> _dataTypes;
        private readonly List<Property> _properties;
        private readonly string _templatePath;
        private readonly string _outputPath;

        public SiteGenerator(Dictionary<string, Class> data, string outputPath)
        {
            _data = data;
            _outputPath = outputPath;

            // Настраиваем RazorLight для работы с файлами
            _engine = new RazorLightEngineBuilder()
                .UseFileSystemProject(Path.Combine(Directory.GetCurrentDirectory(), "templates"))
                .UseMemoryCachingProvider()
                .Build();

            // Разделяем данные по стереотипам
            _classes = data.Values
                .Where(x => x.Stereotype == Stereotype.Class)
                .ToList();

            _enums = data.Values
                .Where(x => x.Stereotype == Stereotype.Enum)
                .ToList();

            _primitives = data.Values
                .Where(x => x.Stereotype == Stereotype.Primitive)
                .ToList();

            _dataTypes = data.Values
                .Where(x => x.Stereotype == Stereotype.DataType)
                .ToList();

            // Собираем все свойства
            _properties = data.Values
                .Where(x => x.Stereotype == Stereotype.Class)
                .SelectMany(x => x.Properties.Values)
                .ToList();

            // Создаем структуру папок
            CreateOutputDirectories();

        }

        private void CreateOutputDirectories()
        {
            Directory.CreateDirectory(_outputPath);
            Directory.CreateDirectory(Path.Combine(_outputPath, "classes"));
            Directory.CreateDirectory(Path.Combine(_outputPath, "properties"));
            Directory.CreateDirectory(Path.Combine(_outputPath, "enums"));
            Directory.CreateDirectory(Path.Combine(_outputPath, "primitives"));
            Directory.CreateDirectory(Path.Combine(_outputPath, "datatypes"));
            Directory.CreateDirectory(Path.Combine(_outputPath, "assets"));
            Directory.CreateDirectory(Path.Combine(_outputPath, "assets", "js"));
            Directory.CreateDirectory(Path.Combine(_outputPath, "assets", "css"));
        }

        public async Task GenerateAsync()
        {
            // Генерируем данные для поиска
            await GenerateSearchIndexAsync();

            // Главная страница
            await GenerateIndexAsync();

            // Страница со списками
            await GeneratePropertyListAsync();
            await GenerateClassListAsync(Stereotype.Class);
            await GenerateClassListAsync(Stereotype.Enum);
            await GenerateClassListAsync(Stereotype.Primitive);
            await GenerateClassListAsync(Stereotype.DataType);
            await GenerateClassListAsync(Stereotype.All);

            // Страницы объектов
            foreach (var cls in _data.Values)
            {
                await GenerateClassPageAsync(cls);
            }
            // Страницы свойств
            foreach (var prop in _properties)
            {
                await GeneratePropertyPageAsync(prop);
            }
        }

        private async Task GeneratePropertyPageAsync(Property prop)
        {
            var rangeClass = _data.TryGetValue(prop.Range, out var rc) ? rc : null;

            var model = new PropertyViewModel
            {
                Title = prop.FieldName,
                Property = prop,
                DomainClass = prop.Domain,
                RangeClass = rangeClass,
                CurrentPage = "properties",
                ClassCount = _classes.Count,
                PropertyCount = _properties.Count,
                EnitityCount = _data.Count,
                EnumCount = _enums.Count,
                PrimitiveCount = _primitives.Count,
                DataTypeCount = _dataTypes.Count,
                Breadcrumbs = new List<BreadcrumbItem>
                {
                    new() { Name = "Home", Url = "/index.html" },
                    new() { Name = "Properties", Url = "/properties/_index.html" },
                    new() { Name = prop.Id, Url = $"/properties/{prop.Id}.html" }
                }
            };

            string html = await _engine.CompileRenderAsync("Property.cshtml", model);
            await WriteOutputAsync($"properties/{prop.Id}.html", html);
        }

        private string stereotype(Stereotype val)
        {
            return val switch
            {
                Stereotype.DataType => "datatypes",
                Stereotype.Enum => "enums",
                Stereotype.Class => "classes",
                Stereotype.Primitive => "primitives",
                Stereotype.All=> "entities",
            };
        }

        private string nameList(Stereotype val) => val switch
        {
            Stereotype.Class => "Classes",
            Stereotype.Enum=> "Enums",
            Stereotype.Primitive => "Primitives",
            Stereotype.DataType => "DataTypes",
            Stereotype.All => "Entities",
        };

        private async Task GenerateClassPageAsync(Class cls)
        {
            var properties = _properties.Where(p => p.Domain.Id == cls.Id).ToList();
            var usedInClasses = _properties
                .Where(p => p.Range == cls.Id)
                .Select(p => p.Domain)
                .DistinctBy(c => c.Id)
                .ToList();

            var parentClasses = cls.SubClass != null && _data.TryGetValue(cls.SubClass.Id, out var parent)
                ? new List<Class> { parent }
                : new List<Class>();

            var childClasses = _classes.Where(c => c.SubClass?.Id == cls.Id).ToList();


            var model = new ClassViewModel
            {
                Title = cls.Id,
                Class = cls,
                Properties = properties,
                ChildClasses = childClasses,
                UsedInClasses = usedInClasses,
                CurrentPage = cls.StereoPath,
                ClassCount = _classes.Count,
                EnitityCount = _data.Count,
                PropertyCount = _properties.Count,
                EnumCount = _enums.Count,
                PrimitiveCount = _primitives.Count,
                DataTypeCount = _dataTypes.Count,
                AllClasses = _classes,
                AllProperties = _properties,
                AllEnums = _enums,
                AllPrimitives = _primitives,
                AllDataTypes = _dataTypes,
                Breadcrumbs = new List<BreadcrumbItem>
                {
                    new() { Name = "Home", Url = "/index.html" },
                    new() { Name = nameList(cls.Stereotype), Url = $"/{cls.StereoPath}/_index.html" },
                    new() { Name = cls.Id, Url = $"/{cls.StereoPath}/{cls.Id}.html" }
                }
            };

            string html = await _engine.CompileRenderAsync("Class.cshtml", model);


            await WriteOutputAsync($"{cls.StereoPath}/{cls.Id}.html", html);
        }

        private async Task GenerateSearchIndexAsync()
        {
            // 1. Создаем JSON индекс для поиска
            var searchData = new
            {
                Classes = _data.Values.Select(c => new
                {
                    id = c.Id,
                    name = c.Label,
                    url = $"/{c.StereoPath}/{c.Id}.html",
                    type = c.Stereotype.ToString(),
                    description = c.Comment,
                    stereotype = c.Stereotype.ToString()
                }),
                Properties = _properties.Select(p => new
                {
                    id = p.Id,
                    name = p.Label,
                    url = $"/properties/{p.Id}.html",
                    type = "Property",
                    description = $"{p.Domain.Id} → {p.Range}",
                    domain = p.Domain.Id,
                    range = p.Range
                }),
            };

            string json = JsonSerializer.Serialize(searchData, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await WriteOutputAsync("assets/search-index.json", json);

            Directory.CreateDirectory(Path.Combine(_outputPath, "assets", "js"));
            File.Copy(Path.Combine("assets", "js", "search.js"),
                      Path.Combine(_outputPath, "assets", "js", "search.js"),
                      overwrite: true);

            Directory.CreateDirectory(Path.Combine(_outputPath, "assets", "css"));
            File.Copy(Path.Combine("assets", "css", "site.css"),
                      Path.Combine(_outputPath, "assets", "css", "site.css"),
                      overwrite: true);
        }

        private async Task GenerateIndexAsync()
        {
            var model = new IndexViewModel
            {
                Title = "CIM ontology",
                Description = "Онтология стандартного CIM по IEC-61970 с примесью ГОСТ РФ (приказ 1340)",
                ExampleClasses = _classes.Take(5).ToList(),
                ExampleProperties = _properties.Take(5).ToList(),
                CurrentPage = "home",
                ClassCount = _classes.Count,
                PropertyCount = _properties.Count,
                EnumCount = _enums.Count,
                PrimitiveCount = _primitives.Count,
                EnitityCount = _data.Count,
                DataTypeCount = _dataTypes.Count,
                AllClasses = _classes,
                AllProperties = _properties,
                AllEnums = _enums,
                AllPrimitives = _primitives,
                AllDataTypes = _dataTypes
            };

            string html = await _engine.CompileRenderAsync("Index.cshtml", model);
            await WriteOutputAsync("index.html", html);
        }

        private async Task GenerateClassListAsync(Stereotype type)
        {

            List<Class> classes;
            if (type == Stereotype.All)
                classes = _data.Values
                            .OrderBy(c => c.Id)
                            .ToList();
            else
                classes = _data.Values
                            .Where(c => c.Stereotype == type)
                            .OrderBy(c => c.Id)
                            .ToList();

            string url;
            if (type == Stereotype.All)
                url = "entities.html";
            else
                url = $"{stereotype(type)}/_index.html";


            var model = new ClassListViewModel
            {
                Title = nameList(type),
                Classes = classes,
                Stereotype = type,
                CurrentPage = stereotype(type),
                ClassCount = _classes.Count,
                PropertyCount = _properties.Count,
                EnumCount = _enums.Count,
                PrimitiveCount = _primitives.Count,
                EnitityCount = _data.Count,
                DataTypeCount = _dataTypes.Count,
                Breadcrumbs = new List<BreadcrumbItem>
                {
                    new() { Name = "Home", Url = "/index.html" },
                    new() { Name = nameList(type), Url = url }
                }
            };

            string html = await _engine.CompileRenderAsync("ClassList.cshtml", model);
            await WriteOutputAsync(url, html);
        }

        private async Task GeneratePropertyListAsync()
        {
            var model = new PropertyListViewModel
            {
                Title = "All Properties",
                Properties = _properties.OrderBy(p => p.Id).ToList(),
                CurrentPage = "properties",
                ClassCount = _classes.Count,
                PropertyCount = _properties.Count,
                EnumCount = _enums.Count,
                EnitityCount = _data.Count,
                PrimitiveCount = _primitives.Count,
                DataTypeCount = _dataTypes.Count,
                Breadcrumbs = new List<BreadcrumbItem>
                {
                    new() { Name = "Home", Url = "/index.html" },
                    new() { Name = "Properties", Url = "/properties/_index.html" }
                }
            };

            string html = await _engine.CompileRenderAsync("PropertyList.cshtml", model);
            await WriteOutputAsync("properties/_index.html", html);
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
