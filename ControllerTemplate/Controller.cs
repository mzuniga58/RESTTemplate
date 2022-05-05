﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Text.Json;
using $entitynamespace$;
using $resourcenamespace$;
using Serilog.Context;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using $validationnamespace$;
using $orchestrationnamespace$;
$if$ ($policy$ == using)using Microsoft.AspNetCore.Authorization;
$endif$using $examplesnamespace$;
namespace $rootnamespace$
{
$model$}
