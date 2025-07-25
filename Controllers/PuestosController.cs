using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using promerica_frontend.Models;
using System.Text;

namespace promerica_frontend.Controllers
{
    public class PuestosController : Controller
    {
        private readonly HttpClient _http;

        public PuestosController(IHttpClientFactory factory)
        {
            _http = factory.CreateClient();
            _http.BaseAddress = new Uri("http://localhost:5153/api/"); // tu backend
        }

        // GET: /Puestos
        public async Task<IActionResult> Index()
        {
            var response = await _http.GetAsync("puestos");
            if (!response.IsSuccessStatusCode)
                return View(new List<PuestoViewModel>());

            var json = await response.Content.ReadAsStringAsync();
            var puestos = JsonConvert.DeserializeObject<List<PuestoViewModel>>(json, new JsonSerializerSettings
            {
                ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver()
            });

            return View(puestos ?? new List<PuestoViewModel>());
        }

        // GET: /Puestos/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var response = await _http.GetAsync("puestos");
            if (!response.IsSuccessStatusCode)
            {
                ViewBag.PuestosDisponibles = new List<SelectListItem>();
            }
            else
            {
                var json = await response.Content.ReadAsStringAsync();
                var lista = JsonConvert.DeserializeObject<List<PuestoViewModel>>(json);

                // Aplanar la jerarquía
                var flatList = FlattenPuestos(lista);

                ViewBag.PuestosDisponibles = flatList
                    .Select(p => new SelectListItem
                    {
                        Value = p.Codigo.ToString(),
                        Text = $"{p.Codigo} - {p.Nombre} - {p.Puesto}"
                    }).ToList();
            }

            return View(new PuestoViewModel());
        }


        private List<PuestoViewModel> FlattenPuestos(List<PuestoViewModel> puestos)
        {
            var flatList = new List<PuestoViewModel>();

            foreach (var puesto in puestos)
            {
                flatList.Add(puesto);
                if (puesto.Subordinados != null && puesto.Subordinados.Any())
                {
                    flatList.AddRange(FlattenPuestos(puesto.Subordinados));
                }
            }

            return flatList;
        }


        // POST: /Puestos/Create
        [HttpPost]
        public async Task<IActionResult> Create(PuestoViewModel model)
        {
            var json = JsonConvert.SerializeObject(model);
            var response = await _http.PostAsync("puestos", new StringContent(json, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Error al guardar el puesto.";
                return View(model);
            }

            return RedirectToAction("Index");
        }

        // GET: /Puestos/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            // 1. Obtener el puesto actual
            var getPuesto = await _http.GetAsync($"puestos/{id}");
            if (!getPuesto.IsSuccessStatusCode) return NotFound();

            var jsonPuesto = await getPuesto.Content.ReadAsStringAsync();
            var model = JsonConvert.DeserializeObject<PuestoViewModel>(jsonPuesto);

            // 2. Obtener todos los puestos jerárquicos
            var response = await _http.GetAsync("puestos");
            if (!response.IsSuccessStatusCode)
            {
                ViewBag.PuestosDisponibles = new List<SelectListItem>();
            }
            else
            {
                var json = await response.Content.ReadAsStringAsync();
                var lista = JsonConvert.DeserializeObject<List<PuestoViewModel>>(json);

                // 3. Aplanar todos los puestos
                var flatList = FlattenPuestos(lista);

                // 4. Evitar que el puesto sea su propio jefe
                ViewBag.PuestosDisponibles = flatList
                    .Where(p => p.Codigo != model.Codigo)
                    .Select(p => new SelectListItem
                    {
                        Value = p.Codigo.ToString(),
                        Text = $"{p.Codigo} - {p.Nombre} - {p.Puesto}"
                    }).ToList();
            }

            return View(model);
        }


        // POST: /Puestos/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(PuestoViewModel model)
        {
            var json = JsonConvert.SerializeObject(model);
            var response = await _http.PutAsync($"puestos/{model.Id}", new StringContent(json, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Error al actualizar.";
                return View(model);
            }

            return RedirectToAction("Index");
        }

        // GET: /Puestos/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _http.DeleteAsync($"puestos/{id}");
            return RedirectToAction("Index");
        }
    }
}
