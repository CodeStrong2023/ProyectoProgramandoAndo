using Microsoft.AspNetCore.Mvc;

using AutoMapper;
using SistemaVenta.AplicacionWeb.Models.ViewModels;
using SistemaVenta.BLL.Interfaces;

namespace SistemaVenta.AplicacionWeb.Controllers
{
    public class PlantillaController : Controller
    {
        private readonly IMapper _mapper;
        private readonly INegocioService _negocioServicio;
        private readonly IVentaService _ventaServicio;



        public PlantillaController(IMapper mapper,
            INegocioService negocioServicio,
            IVentaService ventaServicio)
        {
            _mapper = mapper;
            _negocioServicio = negocioServicio;
            _ventaServicio = ventaServicio;
        }
        public IActionResult EnviarClave(string correo, string clave)
        {
            ViewData["Correo"] = correo;
            ViewData["Clave"] = clave;
            ViewData["Url"] = $"{this.Request.Scheme}://{this.Request.Host}";

            return View();
        }



        public IActionResult RestablecerClave(string clave)
        {
            ViewData["Clave"] = clave;
            return View();
        }
    }
}
