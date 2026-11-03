using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebApplication10.Infrastructure
{
    /// <summary>Yêu cầu đăng nhập (Session) và tuỳ chọn một vai trò, ví dụ [SessionAuthorize(Role = "Admin")].</summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class SessionAuthorizeAttribute : ActionFilterAttribute
    {
        public string? Role { get; set; }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var http = context.HttpContext;

            if (http.Session.GetInt32("UserId") == null)
            {
                // Chỉ nhớ returnUrl với yêu cầu GET (POST không thể quay lại bằng redirect)
                var req = http.Request;
                string? returnUrl = HttpMethods.IsGet(req.Method) ? req.Path + req.QueryString : null;
                context.Result = new RedirectToActionResult("Login", "Account", new { returnUrl });
                return;
            }

            if (Role != null && http.Session.GetString("UserRole") != Role)
                context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
        }
    }
}
