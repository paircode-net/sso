using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using SSO.Core.Domain.Identity._Context.Interfaces.Services;
using SSO.Core.Domain.Identity.AuthAuditEvents.Entity;
using SSO.Core.Domain.Identity.Users.Entity;
using SSO.Core.Domain.Interfaces.Infrastructures.Services;
using SSO.Infrastructures.Data.Identity;
using SSO.Middleware.Identity.Authorization;
using SSO.Shared.Identity;

namespace SSO.Web.Api.Resources.IdentityDb
{
	[ApiController]
	[Route("api/identity/account")]
	[IgnoreAntiforgeryToken]
	public sealed class AccountController : ControllerBase
	{
		private readonly UserManager<User> _userManager;
		private readonly IMailService _mailService;
		private readonly IAuthAuditService _auditService;
		private readonly IUserSessionService _sessionService;
		private readonly IdentityDbContext _dbContext;

		public AccountController(
			UserManager<User> userManager,
			IMailService mailService,
			IAuthAuditService auditService,
			IUserSessionService sessionService,
			IdentityDbContext dbContext)
		{
			_userManager = userManager;
			_mailService = mailService;
			_auditService = auditService;
			_sessionService = sessionService;
			_dbContext = dbContext;
		}

		public sealed class EmailRequest
		{
			[Required]
			[EmailAddress]
			public string Email { get; set; } = string.Empty;
		}

		[HttpPost("request-email-confirmation")]
		[AllowAnonymous]
		public async Task<IActionResult> RequestEmailConfirmation(
			[FromBody] EmailRequest request,
			CancellationToken cancellationToken)
		{
			var user = await _userManager.FindByEmailAsync(request.Email);
			if (user is null || user.IsDeleted)
			{
				return Ok(new { sent = true });
			}

			if (await _userManager.IsEmailConfirmedAsync(user))
			{
				return Ok(new { sent = true, alreadyConfirmed = true });
			}

			var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
			var encoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
			var link = $"{Request.Scheme}://{Request.Host}/Account/ConfirmEmail?userId={user.Id:D}&code={encoded}";

			await _mailService.SendAsync(
				user.Email!,
				"Confirm your email",
				$"ConfirmToken={encoded};UserId={user.Id:D};Link={link}",
				cancellationToken);

			await _auditService.WriteAsync(
				AuthAuditEvent.Create(
					AuthAuditEventTypes.EmailConfirmRequested,
					AuthAuditOutcomes.Success,
					userId: user.Id,
					email: user.Email),
				cancellationToken);

			return Ok(new { sent = true });
		}

		[HttpGet("~/api/identity/auth-audit-events")]
		[RequiresPermission(SsoAdminPermissions.AuditRead)]
		public async Task<IActionResult> GetAuditEvents(
			[FromQuery] Guid? userId,
			[FromQuery] string? eventType,
			[FromQuery] int take = 50,
			CancellationToken cancellationToken = default)
		{
			take = Math.Clamp(take, 1, 200);

			var query = _dbContext.AuthAuditEvents.AsNoTracking().AsQueryable();
			if (userId is Guid uid)
			{
				query = query.Where(x => x.UserId == uid);
			}

			if (!string.IsNullOrWhiteSpace(eventType))
			{
				query = query.Where(x => x.EventType == eventType);
			}

			var items = await query
				.OrderByDescending(x => x.CreatedAt)
				.Take(take)
				.Select(x => new
				{
					x.Id,
					x.CreatedAt,
					x.EventType,
					x.Outcome,
					x.UserId,
					x.Email,
					x.ClientId,
					x.IpAddress,
					x.Detail
				})
				.ToListAsync(cancellationToken);

			return Ok(items);
		}

		[HttpPost("sessions/{userId:guid}/revoke")]
		[RequiresPermission(SsoAdminPermissions.SessionsRevoke)]
		public async Task<IActionResult> RevokeSessions(Guid userId, CancellationToken cancellationToken)
		{
			var count = await _sessionService.RevokeAllForUserAsync(userId, "api.bulk_revoke", cancellationToken);
			return Ok(new { revoked = count });
		}
	}
}
