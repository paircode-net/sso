using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SSO.Core.Domain.Interfaces.Infrastructures.Services;

namespace SSO.Infrastructures.Services
{
	public sealed class MailMessage
	{
		public string To { get; init; } = string.Empty;
		public string Subject { get; init; } = string.Empty;
		public string Body { get; init; } = string.Empty;
	}

	/// <summary>Production/dev logger-backed mail (no SMTP in MVP).</summary>
	public sealed class MailService : IMailService
	{
		private readonly ILogger<MailService> _logger;

		public MailService(ILogger<MailService> logger)
		{
			_logger = logger;
		}

		public Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
		{
			_logger.LogInformation("Mail to {To} | {Subject} | {Body}", to, subject, body);
			return Task.CompletedTask;
		}
	}

	/// <summary>In-memory capture for integration tests.</summary>
	public sealed class CapturingMailService : IMailService
	{
		private readonly ConcurrentQueue<MailMessage> _messages = new();

		public IReadOnlyList<MailMessage> Messages => _messages.ToArray();

		public void Clear()
		{
			while (_messages.TryDequeue(out _))
			{
			}
		}

		public Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
		{
			_messages.Enqueue(new MailMessage { To = to, Subject = subject, Body = body });
			return Task.CompletedTask;
		}
	}
}
