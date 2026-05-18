using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using projectPartiuDestino.Autenticacao;

namespace projectPartiuDestino.Filters
{
    public class SessionAuthorizeAttribute : ActionFilterAttribute
    {
        public string? RoleAnyOf { get; set; } // CSV ex: &quot;Adm,Bibliotecario&quot;

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var http = context.HttpContext;
            var role = http.Session.GetString(SessionKeys.UserRole);
            var userId = http.Session.GetInt32(SessionKeys.UserId);
            if (userId == null)
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);

                return;
            }

            if (!string.IsNullOrWhiteSpace(RoleAnyOf))
            {
                var allowed = RoleAnyOf.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
              
              
            if (!allowed.Contains(role))
                {
                    context.Result = new ForbidResult(); // ou RedirectToAction(&quot;AcessoNegado&quot;, &quot;Auth&quot;)
                    return;
                }
            }
            base.OnActionExecuting(context);
        }
    }
}
