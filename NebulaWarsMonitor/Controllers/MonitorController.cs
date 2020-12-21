using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NebulaWarsMonitor.Models;
using NebulaWarsMonitor.Models.EF;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NebulaWarsMonitor.Controllers
{
    public class MonitorController : Controller
    {
        //private readonly ILogger<MonitorController> _logger;
        private readonly ApplicationContext _applicationContext;

        public MonitorController(/*ILogger<MonitorController> logger, */ApplicationContext appContext)
        {
            //_logger = logger;
            _applicationContext = appContext;
        }

        [HttpGet, AllowAnonymous]
        public IActionResult LogIn()
        {
            return User.Identity.IsAuthenticated ? (IActionResult) RedirectToAction("Home") : View();
        }

        [HttpGet, Authorize]
        public IActionResult LogOut()
        {
            HttpContext.SignOutAsync().Wait();
            return RedirectToAction("Index");
        }

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<ActionResult> LogIn(string password)
        {
            if (PasswordManager.PasswordIsCorrect(password))
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimsIdentity.DefaultRoleClaimType, "Admin")
                };
                var id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
            }
            else
            {
                ViewData["ErrorMessage"] = "Неправильний пароль.";
                return View();
            }

            return RedirectToAction("Home");
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            return RedirectToAction(User.Identity.IsAuthenticated ? "Home" : "LogIn");
        }

        [HttpGet, AllowAnonymous]
        public ActionResult<string> GetRandomServerAddress(string secret)
        {
            if (PasswordManager.SecretIsCorrect(secret))
            {
                var count = _applicationContext.Servers.Count();
                var random = new Random();
                var randomIndex = random.Next(count);
                var server = _applicationContext.Servers.Skip(randomIndex).First();
                return new ActionResult<string>(server.Address);
            }
            else
            {
                return StatusCode(403, "Wrong secret.");
            }
        }

        private const int PageSize = 20;
        private static readonly Type DefaultHomeType = typeof(Server);
        private static readonly string EfCoreModelsNamespace = DefaultHomeType.Namespace;

        [HttpGet, Authorize]
        public IActionResult Home(string type = nameof(Server), int page = 0)
        {
            var entityType = Type.GetType($"{EfCoreModelsNamespace}.{type}", false) ?? DefaultHomeType;
            using var adminContext = _applicationContext;
            var set = (IQueryable<object>) adminContext.GetType().GetMethod("Set", new Type[0])
                .MakeGenericMethod(entityType).Invoke(adminContext, null);

            var properties = adminContext.Model.FindEntityType(entityType).GetProperties();
            var keysProps = adminContext.Model.FindEntityType(entityType).FindPrimaryKey().Properties;
            var firstPrimaryKey = keysProps.First();
            var entities = set.OrderBy(firstPrimaryKey.Name, entityType).Skip(page * PageSize).Take(PageSize).ToList();
            var foreignKeysInfo = adminContext.Model.FindEntityType(entityType).GetForeignKeys()
                .Where(fk => fk.Properties.Count == 1).Select(fk =>
                    new
                    {
                        ForeignName = fk.Properties.First().Name,
                        EntityType = fk.PrincipalEntityType.ClrType,
                        PrimaryName = fk.PrincipalKey.Properties.First().Name
                    });

            var model = new PropertiesDataModel
            {
                EntitiesNames = adminContext.Model.GetEntityTypes().Select(t => t.ClrType.Name),
                Data = properties.Select(p => (p.Name, q: entities.Select(e => entityType.GetProperty(p.Name).GetValue(e, null)).AsEnumerable().ToList()))
                    .ToDictionary<(string n, List<object> q), string, IEnumerable<object>>(p => p.n, p => p.q),
                TableName = adminContext.Model.FindEntityType(entityType)?.GetTableName(),
                PrimaryKeys = entities.Select(e => keysProps.Select(p => new { p.Name, q = entityType.GetProperty(p.Name).GetValue(e, null) })
                    .ToDictionary(o => o.Name, o => o.q)).AsEnumerable(),
                ForeignKeys = foreignKeysInfo.Select(fk => new
                {
                    Name = fk.ForeignName,
                    Value = ((IQueryable<object>)adminContext.GetType().GetMethod("Set", new Type[0])
                        .MakeGenericMethod(fk.EntityType).Invoke(adminContext, null)).ToDictionary(e =>
                            fk.EntityType.GetProperty(fk.PrimaryName).GetValue(e, null).ToString(),
                        e => e.ToString())
                }).ToDictionary(inf => inf.Name, inf => inf.Value),
                EntityName = type,
                PrevPage = page == 0 ? page : page - 1,
                CurrentPage = page,
                NextPage = page + 1,
                LastPage = (set.Count() - 1) / PageSize,
                IsReadOnly = Attribute.IsDefined(entityType, typeof(ReadOnlyTableAttribute))
            };

            return View(model);
        }

        [HttpPost, Authorize]
        public IActionResult SaveChanges(string rows)
        {
            try
            {
                var url = Request.Headers["Referer"].ToString();
                const string typeUrlPrefix = "type=";
                var index = url.IndexOf(typeUrlPrefix, StringComparison.Ordinal);
                var typeName = index > 0 ? url.Substring(index + typeUrlPrefix.Length) : DefaultHomeType.Name;
                var entityType = Type.GetType($"{EfCoreModelsNamespace}.{typeName}") ?? DefaultHomeType;

                if (Attribute.IsDefined(entityType, typeof(ReadOnlyTableAttribute)))
                {
                    return StatusCode(400, $"Editing table of type '{typeName}' is prohibited!");
                }

                var jsonSerializer = new JsonSerializer
                {
                    NullValueHandling = NullValueHandling.Ignore
                };

                var rowsContainer = JsonConvert.DeserializeObject<JObject>(rows);
                var created = rowsContainer["created"].ToObject<JArray>();
                var changed = rowsContainer["changed"].ToObject<JArray>();
                var removed = rowsContainer["removed"].ToObject<JArray>();

                using var adminContext = _applicationContext;

                foreach (var newRow in created)
                {
                    var newEntity = newRow["newVal"].ToObject(entityType, jsonSerializer);
                    adminContext.Add(newEntity);
                }

                var keyProps = adminContext.Model.FindEntityType(entityType).FindPrimaryKey().Properties
                    .Select(p => (Name: p.Name.ToLowerInvariant(), Type: p.ClrType)).ToList();

                foreach (var changedRow in changed)
                {
                    var oldPK = changedRow["oldPK"];
                    var keyValues = keyProps.Select(p => oldPK[p.Name].ToObject(p.Type, jsonSerializer)).ToArray();
                    var changedEntity = changedRow["newVal"].ToObject(entityType, jsonSerializer);
                    var oldEntity = adminContext.Find(entityType, keyValues);
                    adminContext.Entry(oldEntity).CurrentValues.SetValues(changedEntity);
                }

                foreach (var removedRow in removed)
                {
                    var oldPK = removedRow["oldPK"];
                    var keyValues = keyProps.Select(p => oldPK[p.Name].ToObject(p.Type, jsonSerializer)).ToArray();
                    var oldEntity = adminContext.Find(entityType, keyValues);
                    adminContext.Entry(oldEntity).State = EntityState.Deleted;
                }

                adminContext.SaveChanges();
            }
            catch (Exception e)
            {
                return StatusCode(400, GetFullMessage(e));

                static string GetFullMessage(Exception ex)
                {
                    return ex.InnerException == null
                        ? ex.Message
                        : ex.Message + " --> " + GetFullMessage(ex.InnerException);
                }
            }

            return Ok();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous, ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
