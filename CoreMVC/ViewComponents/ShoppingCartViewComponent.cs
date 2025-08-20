using CoreMVC.DataAccess.Repository.IRepository;
using CoreMVC.Utility;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CoreMVC.ViewComponents
{
    public class ShoppingCartViewComponent : ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;
        public ShoppingCartViewComponent(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Task<IViewComponentResult> InvokeAsync()
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var claim = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier);

            if (claim != null)
            {
                if (HttpContext.Session.GetInt32(SD.SessionCart) == null)
                {
                    var count = _unitOfWork.ShoppingCart
                        .GetAll(u => u.ApplicationUserId == claim.Value)
                        .Count();

                    HttpContext.Session.SetInt32(SD.SessionCart, count);
                }

                return Task.FromResult<IViewComponentResult>(View(HttpContext.Session.GetInt32(SD.SessionCart)));
            }

            HttpContext.Session.Remove(SD.SessionCart);
            return Task.FromResult<IViewComponentResult>(View(0));
        }
    }
}
