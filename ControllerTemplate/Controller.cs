using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Text.Json;
using $entitynamespace$;
using $resourcenamespace$;
using $extensionsnamespace$;
using Serilog.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Swashbuckle.AspNetCore.Annotations;
using Rql;
using Rql.Models;
using Rql.Extensions;
using $orchestrationnamespace$;
$if$ ($policy$ != none)using Microsoft.AspNetCore.Authorization;
$endif$

namespace $rootnamespace$
{
$model$}
