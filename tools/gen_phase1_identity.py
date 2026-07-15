# -*- coding: utf-8 -*-
"""Generate Phase 1 Identity aggregates mirroring Sample patterns."""
from pathlib import Path

ROOT = Path(r"f:\DEV\cursor\PairCode\sso\src")

RESX_HEADER = """<?xml version="1.0" encoding="utf-8"?>
<root>
  <resheader name="resmimetype"><value>text/microsoft-resx</value></resheader>
  <resheader name="version"><value>2.0</value></resheader>
  <resheader name="reader"><value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value></resheader>
  <resheader name="writer"><value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value></resheader>
"""

def write(path: Path, content: str):
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(content.replace("\n", "\r\n"), encoding="utf-8")
    print("wrote", path.relative_to(ROOT.parent))


def designer(ns: str, class_name: str, props: list[str]) -> str:
    props_code = "\n".join(
        f"""        public static string {p} {{
            get {{ return ResourceManager.GetString("{p}", resourceCulture); }}
        }}"""
        for p in props
    )
    return f"""namespace {ns} {{
    using System;
    public sealed class {class_name} {{
        private static global::System.Resources.ResourceManager resourceMan;
        private static global::System.Globalization.CultureInfo resourceCulture;
        internal {class_name}() {{ }}
        public static global::System.Resources.ResourceManager ResourceManager {{
            get {{
                if (object.ReferenceEquals(resourceMan, null)) {{
                    resourceMan = new global::System.Resources.ResourceManager("{ns}.{class_name}", typeof({class_name}).Assembly);
                }}
                return resourceMan;
            }}
        }}
        public static global::System.Globalization.CultureInfo Culture {{
            get {{ return resourceCulture; }}
            set {{ resourceCulture = value; }}
        }}
{props_code}
    }}
}}
"""


def resx(props: list[str]) -> str:
    data = "\n".join(
        f'  <data name="{p}" xml:space="preserve"><value>{p}</value></data>' for p in props
    )
    return RESX_HEADER + data + "\n</root>\n"


def gen_simple_crud(entity: str, plural: str, props: list[tuple[str, str, int]], unique_prop: str | None):
    """props: (Name, csharp_type, max_length) — string only for validators."""
    d_ent = ROOT / f"SSO.Core.Domain/Identity/{plural}"
    d_app = ROOT / f"SSO.Core.Application/Identity/{plural}"
    d_map = ROOT / "SSO.Infrastructures.Data/Identity/EntityMappings"
    d_api = ROOT / "SSO.Web.Api/Resources/IdentityDb"

    entity_ns = f"SSO.Core.Domain.Identity.{plural}.Entity"
    prop_decls = "\n".join(f"\t\tpublic {t} {n} {{ get; set; }}" for n, t, _ in props)

    write(
        d_ent / f"Entity/{entity}.cs",
        f"""using BAYSOFT.Abstractions.Core.Domain.Entities;
using BAYSOFT.Abstractions.Crosscutting.InheritStringLocalization;
using SSO.Core.Domain.Identity._Shared;
using SSO.Core.Domain.Identity.{plural}.Resources;
using SSO.Core.Domain.Resources;
using System;

namespace {entity_ns}
{{
\t[InheritStringLocalizer(typeof(Messages), Priority = 1)]
\t[InheritStringLocalizer(typeof(Entity{entity}), Priority = 0)]
\tpublic sealed class {entity} : IdentityAuditableEntity
\t{{
{prop_decls}

\t\tpublic {entity}()
\t\t{{
\t\t}}
\t}}
}}
""",
    )

    prop_names = [n for n, _, _ in props]
    write(d_ent / f"Resources/Entity{entity}.resx", resx(prop_names))
    write(
        d_ent / f"Resources/Entity{entity}.Designer.cs",
        designer(f"SSO.Core.Domain.Identity.{plural}.Resources", f"Entity{entity}", prop_names),
    )

    # Validator
    rules = []
    for n, t, maxlen in props:
        if t == "string":
            rules.append(f'\t\t\tRuleFor(x => x.{n}).NotNull().WithMessage("\'{{PropertyName}}\' cannot be null!");')
            rules.append(f'\t\t\tRuleFor(x => x.{n}).NotEmpty().WithMessage("\'{{PropertyName}}\' cannot be empty!");')
            if maxlen:
                rules.append(
                    f'\t\t\tRuleFor(x => x.{n}).MaximumLength({maxlen}).WithMessage("\'{{PropertyName}}\' must have a maximum of \'{{MaxLength}}\' caracters!");'
                )
        elif t == "Guid":
            rules.append(f'\t\t\tRuleFor(x => x.{n}).NotEmpty().WithMessage("\'{{PropertyName}}\' is required!");')

    write(
        d_ent / f"Validations/EntityValidations/{entity}Validator.cs",
        f"""using BAYSOFT.Abstractions.Core.Domain.Entities.Validations;
using FluentValidation;
using {entity_ns};

namespace SSO.Core.Domain.Identity.{plural}.Validations.EntityValidations
{{
\tpublic sealed class {entity}Validator : EntityValidator<{entity}>
\t{{
\t\tpublic {entity}Validator()
\t\t{{
{chr(10).join(rules)}
\t\t}}
\t}}
}}
""",
    )

    # Spec + domain validators
    if unique_prop:
        write(
            d_ent / f"Specifications/{entity}{unique_prop}AlreadyExistsSpecification.cs",
            f"""using BAYSOFT.Abstractions.Core.Domain.Entities.Specifications;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using {entity_ns};
using System;
using System.Linq;
using System.Linq.Expressions;

namespace SSO.Core.Domain.Identity.{plural}.Specifications
{{
\tpublic class {entity}{unique_prop}AlreadyExistsSpecification : DomainSpecification<{entity}>
\t{{
\t\tprivate IIdentityDbContextReader Reader {{ get; set; }}
\t\tpublic {entity}{unique_prop}AlreadyExistsSpecification(IIdentityDbContextReader reader)
\t\t{{
\t\t\tReader = reader;
\t\t\tSpecificationMessage = "A record with this {unique_prop.lower()} already exists!";
\t\t}}

\t\toverride public Expression<Func<{entity}, bool>> ToExpression()
\t\t\t=> entity => CheckRule(entity);

\t\tprivate bool CheckRule({entity} entity)
\t\t{{
\t\t\treturn Reader.Query<{entity}>().Any(x => !x.IsDeleted && x.{unique_prop} == entity.{unique_prop} && x.Id != entity.Id);
\t\t}}
\t}}
}}
""",
        )
        create_validator_ctor = f"{entity}{unique_prop}AlreadyExistsSpecification spec"
        create_validator_body = f"""\t\t\tAdd(nameof(spec), new DomainRule<{entity}>(spec.Not(), spec.ToString()));"""
        update_same = True
    else:
        create_validator_ctor = ""
        create_validator_body = ""
        update_same = False

    for op in ("Create", "Update", "Delete"):
        ctor = create_validator_ctor if op != "Delete" and unique_prop else ""
        body = create_validator_body if op != "Delete" and unique_prop else ""
        usings = (
            f"using SSO.Core.Domain.Identity.{plural}.Specifications;\n"
            if unique_prop and op != "Delete"
            else ""
        )
        write(
            d_ent / f"Validations/DomainValidations/{op}{entity}SpecificationsValidator.cs",
            f"""using BAYSOFT.Abstractions.Core.Domain.Entities.Validations;
{usings}using {entity_ns};

namespace SSO.Core.Domain.Identity.{plural}.Validations.DomainValidations
{{
\tpublic sealed class {op}{entity}SpecificationsValidator : DomainValidator<{entity}>
\t{{
\t\tpublic {op}{entity}SpecificationsValidator({ctor})
\t\t{{
{body}
\t\t}}
\t}}
}}
""",
        )

    # Services
    for op, action in (
        ("Create", "await Writer.AddAsync(request.Payload);"),
        ("Update", "request.Payload.TouchUpdated();\n\t\t\t// tracked entity"),
        (
            "Delete",
            "request.Payload.MarkDeleted();\n\t\t\t// soft-delete; entity remains tracked",
        ),
    ):
        write(
            d_ent / f"Services/{op}{entity}Service.cs",
            f"""using BAYSOFT.Abstractions.Core.Domain.Entities.Services;
using Microsoft.Extensions.Localization;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using {entity_ns};
using SSO.Core.Domain.Identity.{plural}.Validations.DomainValidations;
using SSO.Core.Domain.Identity.{plural}.Validations.EntityValidations;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Domain.Identity.{plural}.Services
{{
\tpublic sealed class {op}{entity}ServiceRequest : DomainServiceRequest<{entity}>
\t{{
\t\tpublic {op}{entity}ServiceRequest({entity} payload) : base(payload) {{ }}
\t}}

\tpublic sealed class {op}{entity}ServiceRequestHandler
\t\t: DomainServiceRequestHandler<{entity}, {op}{entity}ServiceRequest>
\t{{
\t\tprivate IIdentityDbContextWriter Writer {{ get; set; }}
\t\tpublic {op}{entity}ServiceRequestHandler(
\t\t\tIIdentityDbContextWriter writer,
\t\t\tIStringLocalizer<{entity}> localizer,
\t\t\t{entity}Validator entityValidator,
\t\t\t{op}{entity}SpecificationsValidator domainValidator)
\t\t\t: base(localizer, entityValidator, domainValidator)
\t\t{{
\t\t\tWriter = writer;
\t\t}}

\t\toverride public async Task<{entity}> Handle({op}{entity}ServiceRequest request, CancellationToken cancellationToken)
\t\t{{
\t\t\tValidateEntity(request.Payload);
\t\t\tValidateDomain(request.Payload);
\t\t\t{action}
\t\t\treturn request.Payload;
\t\t}}
\t}}
}}
""",
        )

    # Map
    prop_maps = []
    for n, t, maxlen in props:
        if t == "string":
            prop_maps.append(
                f"""\t\t\tbuilder.Property(e => e.{n})
\t\t\t\t.HasColumnType("NVARCHAR({maxlen})")
\t\t\t\t.HasColumnName("{n}")
\t\t\t\t.IsRequired(true);"""
            )
        elif t == "Guid":
            prop_maps.append(
                f"""\t\t\tbuilder.Property(e => e.{n})
\t\t\t\t.HasColumnType("UNIQUEIDENTIFIER")
\t\t\t\t.HasColumnName("{n}")
\t\t\t\t.IsRequired(true);"""
            )

    unique_index = ""
    if unique_prop:
        unique_index = f"""
\t\t\tbuilder.HasIndex(e => e.{unique_prop})
\t\t\t\t.IsUnique()
\t\t\t\t.HasFilter(\"[IsDeleted] = 0\");"""

    write(
        d_map / f"{entity}Map.cs",
        f"""using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using {entity_ns};
using System;

namespace SSO.Infrastructures.Data.Identity.EntityMappings
{{
\tpublic sealed class {entity}Map : IEntityTypeConfiguration<{entity}>
\t{{
\t\tpublic void Configure(EntityTypeBuilder<{entity}> builder)
\t\t{{
\t\t\tbuilder.ToTable("{plural}");

\t\t\tbuilder.Property(p => p.Id)
\t\t\t\t.HasColumnName("Id")
\t\t\t\t.HasColumnType("UNIQUEIDENTIFIER")
\t\t\t\t.ValueGeneratedOnAdd()
\t\t\t\t.IsRequired(true);
\t\t\tbuilder.HasKey(e => e.Id);

{chr(10).join(prop_maps)}

\t\t\tbuilder.Property(e => e.CreatedAt).HasColumnType("datetimeoffset").IsRequired(true);
\t\t\tbuilder.Property(e => e.UpdatedAt).HasColumnType("datetimeoffset").IsRequired(false);
\t\t\tbuilder.Property(e => e.DeletedAt).HasColumnType("datetimeoffset").IsRequired(false);
\t\t\tbuilder.Property(e => e.IsDeleted).HasColumnType("bit").IsRequired(true);
{unique_index}
\t\t}}
\t}}
}}
""",
    )

    # Notifications helper
    def notification(verb: str, past: str):
        write(
            d_app / f"Notifications/{verb}{entity}Notification.cs",
            f"""using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using {entity_ns};
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.{plural}.Notifications
{{
\tpublic sealed class {verb}{entity}Notification : INotification
\t{{
\t\tpublic {entity} Payload {{ get; set; }}
\t\tpublic DateTime CreatedAt {{ get; set; }}
\t\tpublic {verb}{entity}Notification({entity} payload)
\t\t{{
\t\t\tPayload = payload;
\t\t\tCreatedAt = DateTime.UtcNow;
\t\t}}
\t}}

\tpublic sealed class {verb}{entity}NotificationHandler : INotificationHandler<{verb}{entity}Notification>
\t{{
\t\tprivate ILoggerFactory Logger {{ get; set; }}
\t\tpublic {verb}{entity}NotificationHandler(ILoggerFactory logger) {{ Logger = logger; }}
\t\tpublic Task Handle({verb}{entity}Notification notification, CancellationToken cancellationToken)
\t\t{{
\t\t\tLogger.CreateLogger<{verb}{entity}NotificationHandler>()
\t\t\t\t.Log(LogLevel.Information, "{entity} {past}! Payload: {{Payload}}", JsonConvert.SerializeObject(notification.Payload));
\t\t\treturn Task.CompletedTask;
\t\t}}
\t}}
}}
""",
        )

    for verb, past in (("Post", "posted"), ("Put", "putted"), ("Delete", "deleted")):
        notification(verb, past)

    # Post command
    write(
        d_app / f"Commands/Post{entity}Command.cs",
        f"""using BAYSOFT.Abstractions.Core.Application;
using BAYSOFT.Abstractions.Crosscutting.Helpers;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using ModelWrapper;
using ModelWrapper.Extensions.Post;
using SSO.Core.Application.Identity.{plural}.Notifications;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.{plural}.Services;
using {entity_ns};
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.{plural}.Commands
{{
\tpublic sealed class Post{entity}Command : ApplicationRequest<{entity}, Post{entity}CommandResponse>
\t{{
\t\tpublic Post{entity}Command()
\t\t{{
\t\t\tConfigKeys(x => x.Id);
\t\t\tConfigSuppressedProperties(x => x.Id);
\t\t}}
\t}}

\tpublic sealed class Post{entity}CommandResponse : ApplicationResponse<{entity}>
\t{{
\t\tpublic Post{entity}CommandResponse(Tuple<int, int, WrapRequest<{entity}>, Dictionary<string, object>, Dictionary<string, object>, string, long?> tuple) : base(tuple) {{ }}
\t\tpublic Post{entity}CommandResponse(int statusCode, WrapRequest<{entity}> request, object data, string message = "Successful operation!", long? resultCount = null) : base(statusCode, request, data, message, resultCount) {{ }}
\t}}

\tpublic sealed class Post{entity}CommandHandler : ApplicationRequestHandler<{entity}, Post{entity}Command, Post{entity}CommandResponse>
\t{{
\t\tprivate ILoggerFactory Logger {{ get; set; }}
\t\tprivate IMediator Mediator {{ get; set; }}
\t\tprivate IStringLocalizer Localizer {{ get; set; }}
\t\tprivate IIdentityDbContextWriter Writer {{ get; set; }}

\t\tpublic Post{entity}CommandHandler(ILoggerFactory logger, IMediator mediator, IStringLocalizer<{entity}> localizer, IIdentityDbContextWriter writer)
\t\t{{
\t\t\tLogger = logger; Mediator = mediator; Localizer = localizer; Writer = writer;
\t\t}}

\t\toverride public async Task<Post{entity}CommandResponse> Handle(Post{entity}Command request, CancellationToken cancellationToken)
\t\t{{
\t\t\ttry
\t\t\t{{
\t\t\t\trequest.IsValid(Localizer, true);
\t\t\t\tvar data = request.Post();
\t\t\t\tdata.MarkCreated();
\t\t\t\tawait Mediator.Send(new Create{entity}ServiceRequest(data));
\t\t\t\tawait Writer.CommitAsync(cancellationToken);
\t\t\t\tawait Mediator.Publish(new Post{entity}Notification(data));
\t\t\t\treturn new Post{entity}CommandResponse((int)HttpStatusCode.Created, request, data, Localizer["Successful operation!"], 1);
\t\t\t}}
\t\t\tcatch (Exception exception)
\t\t\t{{
\t\t\t\tLogger.CreateLogger<Post{entity}CommandHandler>().Log(LogLevel.Error, exception, exception.Message);
\t\t\t\treturn new Post{entity}CommandResponse(ExceptionResponseHelper.CreateTuple(Localizer, request, exception));
\t\t\t}}
\t\t}}
\t}}
}}
""",
    )

    # Put
    write(
        d_app / f"Commands/Put{entity}Command.cs",
        f"""using BAYSOFT.Abstractions.Core.Application;
using BAYSOFT.Abstractions.Core.Domain.Exceptions;
using BAYSOFT.Abstractions.Crosscutting.Helpers;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using ModelWrapper;
using ModelWrapper.Extensions.Put;
using SSO.Core.Application.Identity.{plural}.Notifications;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.{plural}.Services;
using {entity_ns};
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.{plural}.Commands
{{
\tpublic sealed class Put{entity}Command : ApplicationRequest<{entity}, Put{entity}CommandResponse>
\t{{
\t\tpublic Put{entity}Command()
\t\t{{
\t\t\tConfigKeys(x => x.Id);
\t\t\tConfigSuppressedProperties(x => x.Id);
\t\t\tValidator.RuleFor(x => x.Id).NotEmpty().WithMessage("{{0}} is required!");
\t\t}}
\t}}

\tpublic sealed class Put{entity}CommandResponse : ApplicationResponse<{entity}>
\t{{
\t\tpublic Put{entity}CommandResponse(Tuple<int, int, WrapRequest<{entity}>, Dictionary<string, object>, Dictionary<string, object>, string, long?> tuple) : base(tuple) {{ }}
\t\tpublic Put{entity}CommandResponse(int statusCode, WrapRequest<{entity}> request, object data, string message = "Successful operation!", long? resultCount = null) : base(statusCode, request, data, message, resultCount) {{ }}
\t}}

\tpublic sealed class Put{entity}CommandHandler : ApplicationRequestHandler<{entity}, Put{entity}Command, Put{entity}CommandResponse>
\t{{
\t\tprivate ILoggerFactory Logger {{ get; set; }}
\t\tprivate IMediator Mediator {{ get; set; }}
\t\tprivate IStringLocalizer Localizer {{ get; set; }}
\t\tprivate IIdentityDbContextWriter Writer {{ get; set; }}

\t\tpublic Put{entity}CommandHandler(ILoggerFactory logger, IMediator mediator, IStringLocalizer<{entity}> localizer, IIdentityDbContextWriter writer)
\t\t{{
\t\t\tLogger = logger; Mediator = mediator; Localizer = localizer; Writer = writer;
\t\t}}

\t\toverride public async Task<Put{entity}CommandResponse> Handle(Put{entity}Command request, CancellationToken cancellationToken)
\t\t{{
\t\t\ttry
\t\t\t{{
\t\t\t\trequest.IsValid(Localizer, true);
\t\t\t\tvar id = request.Project(x => x.Id);
\t\t\t\tvar data = await Writer.Query<{entity}>().SingleOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
\t\t\t\tif (data == null) throw new EntityNotFoundException<{entity}>(Localizer);
\t\t\t\trequest.Put(data);
\t\t\t\tawait Mediator.Send(new Update{entity}ServiceRequest(data));
\t\t\t\tawait Writer.CommitAsync(cancellationToken);
\t\t\t\tawait Mediator.Publish(new Put{entity}Notification(data));
\t\t\t\treturn new Put{entity}CommandResponse((int)HttpStatusCode.OK, request, data, Localizer["Successful operation!"], 1);
\t\t\t}}
\t\t\tcatch (Exception exception)
\t\t\t{{
\t\t\t\tLogger.CreateLogger<Put{entity}CommandHandler>().Log(LogLevel.Error, exception, exception.Message);
\t\t\t\treturn new Put{entity}CommandResponse(ExceptionResponseHelper.CreateTuple(Localizer, request, exception));
\t\t\t}}
\t\t}}
\t}}
}}
""",
    )

    # Delete
    write(
        d_app / f"Commands/Delete{entity}Command.cs",
        f"""using BAYSOFT.Abstractions.Core.Application;
using BAYSOFT.Abstractions.Core.Domain.Exceptions;
using BAYSOFT.Abstractions.Crosscutting.Helpers;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using ModelWrapper;
using SSO.Core.Application.Identity.{plural}.Notifications;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.{plural}.Services;
using {entity_ns};
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.{plural}.Commands
{{
\tpublic sealed class Delete{entity}Command : ApplicationRequest<{entity}, Delete{entity}CommandResponse>
\t{{
\t\tpublic Delete{entity}Command()
\t\t{{
\t\t\tConfigKeys(x => x.Id);
\t\t\tConfigSuppressedProperties(x => x.Id);
\t\t\tValidator.RuleFor(x => x.Id).NotEmpty().WithMessage("{{0}} is required!");
\t\t}}
\t}}

\tpublic sealed class Delete{entity}CommandResponse : ApplicationResponse<{entity}>
\t{{
\t\tpublic Delete{entity}CommandResponse(Tuple<int, int, WrapRequest<{entity}>, Dictionary<string, object>, Dictionary<string, object>, string, long?> tuple) : base(tuple) {{ }}
\t\tpublic Delete{entity}CommandResponse(int statusCode, WrapRequest<{entity}> request, object data, string message = "Successful operation!", long? resultCount = null) : base(statusCode, request, data, message, resultCount) {{ }}
\t}}

\tpublic sealed class Delete{entity}CommandHandler : ApplicationRequestHandler<{entity}, Delete{entity}Command, Delete{entity}CommandResponse>
\t{{
\t\tprivate ILoggerFactory Logger {{ get; set; }}
\t\tprivate IMediator Mediator {{ get; set; }}
\t\tprivate IStringLocalizer Localizer {{ get; set; }}
\t\tprivate IIdentityDbContextWriter Writer {{ get; set; }}

\t\tpublic Delete{entity}CommandHandler(ILoggerFactory logger, IMediator mediator, IStringLocalizer<{entity}> localizer, IIdentityDbContextWriter writer)
\t\t{{
\t\t\tLogger = logger; Mediator = mediator; Localizer = localizer; Writer = writer;
\t\t}}

\t\toverride public async Task<Delete{entity}CommandResponse> Handle(Delete{entity}Command request, CancellationToken cancellationToken)
\t\t{{
\t\t\ttry
\t\t\t{{
\t\t\t\trequest.IsValid(Localizer, true);
\t\t\t\tvar id = request.Project(x => x.Id);
\t\t\t\tvar data = await Writer.Query<{entity}>().SingleOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
\t\t\t\tif (data == null) throw new EntityNotFoundException<{entity}>(Localizer);
\t\t\t\tawait Mediator.Send(new Delete{entity}ServiceRequest(data));
\t\t\t\tawait Writer.CommitAsync(cancellationToken);
\t\t\t\tawait Mediator.Publish(new Delete{entity}Notification(data));
\t\t\t\treturn new Delete{entity}CommandResponse((int)HttpStatusCode.OK, request, data, Localizer["Successful operation!"], 1);
\t\t\t}}
\t\t\tcatch (Exception exception)
\t\t\t{{
\t\t\t\tLogger.CreateLogger<Delete{entity}CommandHandler>().Log(LogLevel.Error, exception, exception.Message);
\t\t\t\treturn new Delete{entity}CommandResponse(ExceptionResponseHelper.CreateTuple(Localizer, request, exception));
\t\t\t}}
\t\t}}
\t}}
}}
""",
    )

    # Queries
    write(
        d_app / f"Queries/Get{entity}ByIdQuery.cs",
        f"""using BAYSOFT.Abstractions.Core.Application;
using BAYSOFT.Abstractions.Core.Domain.Exceptions;
using BAYSOFT.Abstractions.Crosscutting.Helpers;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using ModelWrapper;
using ModelWrapper.Extensions.Select;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using {entity_ns};
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.{plural}.Queries
{{
\tpublic sealed class Get{entity}ByIdQuery : ApplicationRequest<{entity}, Get{entity}ByIdQueryResponse>
\t{{
\t\tpublic Get{entity}ByIdQuery()
\t\t{{
\t\t\tConfigKeys(x => x.Id);
\t\t\tConfigSuppressedProperties(x => x.Id);
\t\t\tValidator.RuleFor(x => x.Id).NotEmpty().WithMessage("{{0}} is required!");
\t\t}}
\t}}

\tpublic sealed class Get{entity}ByIdQueryResponse : ApplicationResponse<{entity}>
\t{{
\t\tpublic Get{entity}ByIdQueryResponse(Tuple<int, int, WrapRequest<{entity}>, Dictionary<string, object>, Dictionary<string, object>, string, long?> tuple) : base(tuple) {{ }}
\t\tpublic Get{entity}ByIdQueryResponse(int statusCode, WrapRequest<{entity}> request, object data, string message = "Successful operation!", long? resultCount = null) : base(statusCode, request, data, message, resultCount) {{ }}
\t}}

\tpublic sealed class Get{entity}ByIdQueryHandler : ApplicationRequestHandler<{entity}, Get{entity}ByIdQuery, Get{entity}ByIdQueryResponse>
\t{{
\t\tprivate ILoggerFactory Logger {{ get; set; }}
\t\tprivate IStringLocalizer Localizer {{ get; set; }}
\t\tprivate IIdentityDbContextReader Reader {{ get; set; }}

\t\tpublic Get{entity}ByIdQueryHandler(ILoggerFactory logger, IMediator mediator, IStringLocalizer<{entity}> localizer, IIdentityDbContextReader reader)
\t\t{{
\t\t\tLogger = logger; Localizer = localizer; Reader = reader;
\t\t}}

\t\toverride public async Task<Get{entity}ByIdQueryResponse> Handle(Get{entity}ByIdQuery request, CancellationToken cancellationToken)
\t\t{{
\t\t\ttry
\t\t\t{{
\t\t\t\tvar id = request.Project(x => x.Id);
\t\t\t\tvar data = await Reader.Query<{entity}>().Where(x => x.Id == id && !x.IsDeleted).Select(request).SingleOrDefaultAsync();
\t\t\t\tif (data == null) throw new EntityNotFoundException<{entity}>(Localizer);
\t\t\t\treturn new Get{entity}ByIdQueryResponse((int)HttpStatusCode.OK, request, data, Localizer["Successful operation!"], 1);
\t\t\t}}
\t\t\tcatch (Exception exception)
\t\t\t{{
\t\t\t\tLogger.CreateLogger<Get{entity}ByIdQueryHandler>().Log(LogLevel.Error, exception, exception.Message);
\t\t\t\treturn new Get{entity}ByIdQueryResponse(ExceptionResponseHelper.CreateTuple(Localizer, request, exception));
\t\t\t}}
\t\t}}
\t}}
}}
""",
    )

    write(
        d_app / f"Queries/Get{plural}ByFilterQuery.cs",
        f"""using BAYSOFT.Abstractions.Core.Application;
using BAYSOFT.Abstractions.Crosscutting.Helpers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using ModelWrapper;
using ModelWrapper.Extensions.FullSearch;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using {entity_ns};
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.{plural}.Queries
{{
\tpublic sealed class Get{plural}ByFilterQuery : ApplicationRequest<{entity}, Get{plural}ByFilterQueryResponse>
\t{{
\t\tpublic Get{plural}ByFilterQuery()
\t\t{{
\t\t\tConfigKeys(x => x.Id);
\t\t\tConfigSuppressedProperties(x => x.Id);
\t\t}}
\t}}

\tpublic sealed class Get{plural}ByFilterQueryResponse : ApplicationResponse<{entity}>
\t{{
\t\tpublic Get{plural}ByFilterQueryResponse(Tuple<int, int, WrapRequest<{entity}>, Dictionary<string, object>, Dictionary<string, object>, string, long?> tuple) : base(tuple) {{ }}
\t\tpublic Get{plural}ByFilterQueryResponse(int statusCode, WrapRequest<{entity}> request, object data, string message = "Successful operation!", long? resultCount = null) : base(statusCode, request, data, message, resultCount) {{ }}
\t}}

\tpublic sealed class Get{plural}ByFilterQueryHandler : ApplicationRequestHandler<{entity}, Get{plural}ByFilterQuery, Get{plural}ByFilterQueryResponse>
\t{{
\t\tprivate ILoggerFactory Logger {{ get; set; }}
\t\tprivate IStringLocalizer Localizer {{ get; set; }}
\t\tprivate IIdentityDbContextReader Reader {{ get; set; }}

\t\tpublic Get{plural}ByFilterQueryHandler(ILoggerFactory logger, IMediator mediator, IStringLocalizer<{entity}> localizer, IIdentityDbContextReader reader)
\t\t{{
\t\t\tLogger = logger; Localizer = localizer; Reader = reader;
\t\t}}

\t\toverride public async Task<Get{plural}ByFilterQueryResponse> Handle(Get{plural}ByFilterQuery request, CancellationToken cancellationToken)
\t\t{{
\t\t\ttry
\t\t\t{{
\t\t\t\tlong resultCount = 1;
\t\t\t\tvar data = await Reader.Query<{entity}>().AsNoTracking().Where(x => !x.IsDeleted).FullSearch(request, out resultCount).ToListAsync(cancellationToken);
\t\t\t\treturn new Get{plural}ByFilterQueryResponse((int)HttpStatusCode.OK, request, data, Localizer["Successful operation!"], resultCount);
\t\t\t}}
\t\t\tcatch (Exception exception)
\t\t\t{{
\t\t\t\tLogger.CreateLogger<Get{plural}ByFilterQueryHandler>().Log(LogLevel.Error, exception, exception.Message);
\t\t\t\treturn new Get{plural}ByFilterQueryResponse(ExceptionResponseHelper.CreateTuple(Localizer, request, exception));
\t\t\t}}
\t\t}}
\t}}
}}
""",
    )

    route = plural.lower()
    write(
        d_api / f"{plural}Controller.cs",
        f"""using Microsoft.AspNetCore.Mvc;
using SSO.Core.Application.Identity.{plural}.Commands;
using SSO.Core.Application.Identity.{plural}.Queries;
using SSO.Web.Api.Abstractions.Controllers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Web.Api.Identity
{{
\t[Produces("application/json")]
\t[Route("api/identity/{route}")]
\tpublic sealed class {plural}Controller : ResourceController
\t{{
\t\t[HttpGet]
\t\tpublic async Task<ActionResult<Get{plural}ByFilterQueryResponse>> Get(Get{plural}ByFilterQuery request, CancellationToken cancellationToken = default)
\t\t\t=> await Send(request, cancellationToken);

\t\t[HttpGet("{{id:Guid}}")]
\t\tpublic async Task<ActionResult<Get{entity}ByIdQueryResponse>> Get(Get{entity}ByIdQuery request, CancellationToken cancellationToken = default)
\t\t\t=> await Send(request, cancellationToken);

\t\t[HttpPost]
\t\tpublic async Task<ActionResult<Post{entity}CommandResponse>> Post(Post{entity}Command request, CancellationToken cancellationToken = default)
\t\t\t=> await Send(request, cancellationToken);

\t\t[HttpPut("{{id:Guid}}")]
\t\tpublic async Task<ActionResult<Put{entity}CommandResponse>> Put(Put{entity}Command request, CancellationToken cancellationToken = default)
\t\t\t=> await Send(request, cancellationToken);

\t\t[HttpDelete("{{id:Guid}}")]
\t\tpublic async Task<ActionResult<Delete{entity}CommandResponse>> Delete(Delete{entity}Command request, CancellationToken cancellationToken = default)
\t\t\t=> await Send(request, cancellationToken);
\t}}
}}
""",
    )


# --- generate ---
write(
    ROOT / "SSO.Core.Domain/Identity/_Shared/IdentityAuditableEntity.cs",
    """using BAYSOFT.Abstractions.Core.Domain.Entities;
using System;

namespace SSO.Core.Domain.Identity._Shared
{
	public abstract class IdentityAuditableEntity : DomainEntity<Guid>
	{
		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset? UpdatedAt { get; set; }
		public DateTimeOffset? DeletedAt { get; set; }
		public bool IsDeleted { get; set; }

		public void MarkCreated()
		{
			CreatedAt = DateTimeOffset.UtcNow;
			IsDeleted = false;
			DeletedAt = null;
		}

		public void TouchUpdated()
		{
			UpdatedAt = DateTimeOffset.UtcNow;
		}

		public void MarkDeleted()
		{
			IsDeleted = true;
			DeletedAt = DateTimeOffset.UtcNow;
			UpdatedAt = DeletedAt;
		}
	}
}
""",
)

gen_simple_crud("Organization", "Organizations", [("Name", "string", 128), ("Code", "string", 64)], "Code")
gen_simple_crud("Product", "Products", [("Name", "string", 128), ("Code", "string", 64)], "Code")
gen_simple_crud(
    "Membership",
    "Memberships",
    [("UserId", "Guid", 0), ("OrganizationId", "Guid", 0)],
    None,
)

print("done")
